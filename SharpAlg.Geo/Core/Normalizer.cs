using System;
using System.Collections.Generic;
using System.Linq;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;

namespace SharpAlg.Geo.Core {
    public static class Normalizer {
        public static bool IsNormal(this Expr expr) {
            return expr.MatchDefault(
                x => { throw new NotImplementedException(); },
                mult: IsNormalProduct,
                param: x => true,
                @const: x => true
            );
        }
        static bool IsNormalProduct(ExprList args) {
            var noConstArgs = args[0].IsConst() ? args.Tail() : args;
            var paramOrPowerArgs = noConstArgs.Select(x => x.AsParamPowerInfo());
            return paramOrPowerArgs.All(x => x != null) &&
                paramOrPowerArgs
                    .Select(x => x.Value)
                    .IsOrdered(new DelegateComparer<ParamPowerInfo>((x, y) => Comparer<string>.Default.Compare(x.Param, y.Param)));
        }
    }
}