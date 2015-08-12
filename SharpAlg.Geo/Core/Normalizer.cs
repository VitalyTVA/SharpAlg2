using System;

namespace SharpAlg.Geo.Core {
    public static class Normalizer {
        public static bool IsNormal(this Expr expr) {
            return expr.MatchDefault(
                x => { throw new NotImplementedException(); },
                param: x => true,
                @const: x => true
            );
        }
    }
}