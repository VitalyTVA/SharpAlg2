using System;
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
            return expr.MatchDefault(
                x => { throw new NotImplementedException(); },
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
                multArgs.Select(x => GetParamOrPowerArgs(x).Select(y => y.Value))
                    .IsOrdered(new DelegateComparer<IEnumerable<ParamPowerInfo>>(CompareMult));
        }
        static int CompareMult(IEnumerable<ParamPowerInfo> x, IEnumerable<ParamPowerInfo> y) {
            var xTotalPower = GetTotalPower(x);
            var yTotalPower = GetTotalPower(y);
            if(xTotalPower == 0 && yTotalPower == 0)
                return 0;
            var powerComparison = Comparer<BigInteger>.Default.Compare(yTotalPower, xTotalPower);
            if(powerComparison != 0)
                return powerComparison;
            //if(!x.Any() || !y.Any())
            //return Comparer<int>.Default.Compare(y.Count(), x.Count());
            return Comparer<string>.Default.Compare(x.First().Param, y.First().Param);
        }

        private static BigInteger GetTotalPower(IEnumerable<ParamPowerInfo> paramPowerInfo) {
            return paramPowerInfo.Aggregate<ParamPowerInfo, BigInteger>(0, (sum, a) => sum + a.Power);
        }

        static bool IsNormalProduct(ExprList args) {
            var paramOrPowerArgs = GetParamOrPowerArgs(args);
            return paramOrPowerArgs.All(x => x != null) &&
                paramOrPowerArgs
                    .Select(x => x.Value)
                    .IsOrdered(new DelegateComparer<ParamPowerInfo>((x, y) => Comparer<string>.Default.Compare(x.Param, y.Param)));
        }

        private static IEnumerable<ParamPowerInfo?> GetParamOrPowerArgs(ExprList args) {
            var noConstArgs = args[0].IsConst() ? args.Tail() : args;
            var paramOrPowerArgs = noConstArgs.Select(x => x.ParamOrParamPowerAsPowerInfo());
            return paramOrPowerArgs;
        }
    }
}