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
            return noConstArgs.All(x => x.IsParam()) && 
                noConstArgs.Select(x => x.ToParam()).IsOrdered(Comparer<string>.Default);
        }
    }
}