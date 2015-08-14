//using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;

namespace SharpAlg.Geo.Core {
    public static class Normalizer {
        public static bool IsNormal(this Expr expr) {
            return expr.MatchDefault(
                x => x.IsNormalNoDiv(),
                div: (n, d) => n.IsNormalNoDiv() && d.IsNormalNoDiv()
            );
        }
        static bool IsNormalNoDiv(this Expr expr) {
            return expr.MatchStrict(
                sqrt: IsNormalNoDiv,
                div: (n, d) => false,
                add: IsNormalSum,
                mult: IsNormalProduct,
                param: x => true,
                @const: x => true,
                power: (val, pow) => val.IsParam()
            );
        }
        static bool IsNormalSum(ExprList args) {
            var multArgs = args.Select(x => x.ExprOrMultToMult());
            return 
                multArgs.All(x => IsNormalProduct(x)) &&
                multArgs.Select(x => GetParamOrPowerArgsWithSqrt(x))
                    .IsOrdered(new DelegateComparer<ParamPowerInfoListWithSqrt>(CompareMult));
        }
        static int CompareMult(ParamPowerInfoListWithSqrt xList, ParamPowerInfoListWithSqrt yList) {
            var x = xList.ParamPowerInfoList.Select(a => a.Value);
            var y = yList.ParamPowerInfoList.Select(a => a.Value);
            var powerComparison = Comparer<BigInteger>.Default.Compare(GetTotalPower(y), GetTotalPower(x));
            if(powerComparison != 0)
                return powerComparison;
            var itemsComparison = x.Zip(y, (a, b) => {
                var paramComparison = Comparer<string>.Default.Compare(a.Param, b.Param);
                if(paramComparison != 0)
                    return paramComparison;
                return Comparer<BigInteger>.Default.Compare(b.Power, a.Power);
            }).FirstOrDefault(a => a != 0);
            if(itemsComparison != 0)
                return itemsComparison;
            var nullComparison = LinqExtensions.CompareObjects(xList.Sqrt, yList.Sqrt);
            if(nullComparison != 0)
                return nullComparison;
            //if(xList.Sqrt != null && yList.Sqrt != null)
                return Comparer<int>.Default.Compare(yList.Sqrt.ExprOrAddToAdd().Length, xList.Sqrt.ExprOrAddToAdd().Length);
            //return 0;
        }
        //static int CompareAdd(IEnumerable<ParamPowerInfoListWithSqrt> xList, IEnumerable<ParamPowerInfoListWithSqrt> yList) {
        //}

        static BigInteger GetTotalPower(IEnumerable<ParamPowerInfo> paramPowerInfo) {
            return paramPowerInfo.Aggregate<ParamPowerInfo, BigInteger>(0, (sum, a) => sum + a.Power);
        }

        static bool IsNormalProduct(ExprList args) {
            var paramPowerInfoListWithSqrt = GetParamOrPowerArgsWithSqrt(args);
            if(paramPowerInfoListWithSqrt.Sqrt.Return(x => !x.IsNormalNoDiv(), () => false))
                return false;
            return paramPowerInfoListWithSqrt.ParamPowerInfoList.All(x => x != null) &&
                paramPowerInfoListWithSqrt.ParamPowerInfoList
                    .Select(x => x.Value)
                    .IsOrdered(new DelegateComparer<ParamPowerInfo>((x, y) => Comparer<string>.Default.Compare(x.Param, y.Param)));
        }
        static ParamPowerInfoListWithSqrt GetParamOrPowerArgsWithSqrt(ExprList args) {
            var noSqrtArgs = args.Last().IsSqrt() ? args.Take(args.Length - 1) : args;
            return new ParamPowerInfoListWithSqrt(GetParamOrPowerArgs(noSqrtArgs), args.Last().AsSqrt());
        }

        static IEnumerable<ParamPowerInfo?> GetParamOrPowerArgs(IEnumerable<Expr> args) {
            var noConstArgs = (args.Any() && args.First().IsConst()) ? args.Tail() : args;
            return noConstArgs.Select(x => x.ParamOrParamPowerAsPowerInfo());
        }
    }
}