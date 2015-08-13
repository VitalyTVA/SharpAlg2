﻿//using System;
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
                multArgs.Select(x => GetParamOrPowerArgs(x).Select(y => y.Value))
                    .IsOrdered(new DelegateComparer<IEnumerable<ParamPowerInfo>>(CompareMult));
        }
        static int CompareMult(IEnumerable<ParamPowerInfo> x, IEnumerable<ParamPowerInfo> y) {
            var powerComparison = Comparer<BigInteger>.Default.Compare(GetTotalPower(y), GetTotalPower(x));
            if(powerComparison != 0)
                return powerComparison;
            return x.Zip(y, (a, b) => {
                var paramComparison = Comparer<string>.Default.Compare(a.Param, b.Param);
                if(paramComparison != 0)
                    return paramComparison;
                return Comparer<BigInteger>.Default.Compare(b.Power, a.Power);
            }).FirstOrDefault(a => a != 0);
        }

        static BigInteger GetTotalPower(IEnumerable<ParamPowerInfo> paramPowerInfo) {
            return paramPowerInfo.Aggregate<ParamPowerInfo, BigInteger>(0, (sum, a) => sum + a.Power);
        }

        static bool IsNormalProduct(ExprList args) {
            if(args.Last().AsSqrt().Return(x => !x.IsNormalNoDiv(), () => false))
                return false;
            var noSqrtArgs = args.Last().IsSqrt() ? args.Take(args.Length - 1) : args;
            var paramOrPowerArgs = GetParamOrPowerArgs(noSqrtArgs);
            return paramOrPowerArgs.All(x => x != null) &&
                paramOrPowerArgs
                    .Select(x => x.Value)
                    .IsOrdered(new DelegateComparer<ParamPowerInfo>((x, y) => Comparer<string>.Default.Compare(x.Param, y.Param)));
        }

        static IEnumerable<ParamPowerInfo?> GetParamOrPowerArgs(IEnumerable<Expr> args) {
            var noConstArgs = args.First().IsConst() ? args.Tail() : args;
            return noConstArgs.Select(x => x.ParamOrParamPowerAsPowerInfo());
        }
    }
}