using System.Collections.Immutable;
using System.Numerics;

namespace SharpAlg.Geo.Core {
    public class Builder {
        public Expr Add(params Expr[] args) {
            return new AddExpr(this, ImmutableArray.Create(args));
        }
        public Expr Subtract(Expr a, Expr b) {
            return Add(a, Minus(b));
        }
        public Expr Multiply(params Expr[] args) {
            return new MultExpr(this, ImmutableArray.Create(args));
        }
        public Expr Minus(Expr a) {
            return Multiply(-1, a);
        }
        public Expr Divide(Expr a, Expr b) {
            return new DivExpr(this, a, b);
        }
        public Expr Power(Expr value, BigInteger power) {
            return new PowerExpr(this, value, power);
        }
    }
}
