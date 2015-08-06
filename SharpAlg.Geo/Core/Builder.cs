using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace SharpAlg.Geo.Core {
    public interface IBuilder {
        Expr Add(params Expr[] args);
        void Check(IEnumerable<Expr> args);
        Expr Divide(Expr a, Expr b);
        Expr Multiply(params Expr[] args);
        Expr Power(Expr value, BigInteger power);
        Expr Sqrt(Expr value);
    }
    public class CachingBuilder : IBuilder {
        public Expr Add(params Expr[] args) {
            return new AddExpr(this, ImmutableArray.Create(args));
        }
        public Expr Multiply(params Expr[] args) {
            return new MultExpr(this, ImmutableArray.Create(args));
        }
        public Expr Divide(Expr a, Expr b) {
            return new DivExpr(this, a, b);
        }
        public Expr Power(Expr value, BigInteger power) {
            return new PowerExpr(this, value, power);
        }
        public Expr Sqrt(Expr value) {
            return new SqrtExpr(this, value);
        }
        public void Check(IEnumerable<Expr> args) {
            if(args.OfType<ComplexExpr>().Any(x => x.Builder != this))
                throw new CannotMixExpressionsFromDifferentBuildersException();
        }
    }
    public sealed class SimpleBuilder : IBuilder {
        public static readonly IBuilder Instance = new SimpleBuilder();
        SimpleBuilder() { }
        Expr IBuilder.Add(params Expr[] args) {
            return new AddExpr(this, ImmutableArray.Create(args));
        }
        Expr IBuilder.Multiply(params Expr[] args) {
            return new MultExpr(this, ImmutableArray.Create(args));
        }
        Expr IBuilder.Divide(Expr a, Expr b) {
            return new DivExpr(this, a, b);
        }
        Expr IBuilder.Power(Expr value, BigInteger power) {
            return new PowerExpr(this, value, power);
        }
        Expr IBuilder.Sqrt(Expr value) {
            return new SqrtExpr(this, value);
        }
        void IBuilder.Check(IEnumerable<Expr> args) {
        }
    }
}
