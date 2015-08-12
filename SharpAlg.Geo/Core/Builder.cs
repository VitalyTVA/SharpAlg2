using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using static SharpAlg.Geo.Core.Utility;

namespace SharpAlg.Geo.Core {
    public interface IBuilder {
        Expr Add(params Expr[] args);
        void Check(IEnumerable<Expr> args);
        Expr Divide(Expr a, Expr b);
        Expr Multiply(params Expr[] args);
        Expr Power(Expr value, BigInteger power);
        Expr Sqrt(Expr value);
    }
    public sealed class CachingBuilder : IBuilder {
        readonly Func<AddExpr, AddExpr> add;
        readonly Func<MultExpr, MultExpr> mult;
        readonly Func<SqrtExpr, SqrtExpr> sqrt;
        readonly Func<PowerExpr, PowerExpr> power;
        readonly Func<DivExpr, DivExpr> div;

        static Func<T, T> Memoize<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) {
            return Func((T x) => x).Memoize(new DelegateEqualityComparer<T>(equals, x => x.GetHashCode()));
        }

        public static IBuilder CreateSimple() {
            return new CachingBuilder(x => 0);
        }
        public static IBuilder CreateRealLife() {
            return new CachingBuilder(x => x.GetHashCode());
        }

        CachingBuilder(Func<Expr, int> getHashCode) {
            add = Memoize<AddExpr>((x, y) => Enumerable.SequenceEqual(x.Args, y.Args), getHashCode);
            mult = Memoize<MultExpr>((x, y) => Enumerable.SequenceEqual(x.Args, y.Args), getHashCode);
            sqrt = Memoize<SqrtExpr>((x, y) => Equals(x.Value, y.Value), getHashCode);
            power = Memoize<PowerExpr>((x, y) => Equals(x.Value, y.Value) && Equals(x.Power, y.Power), getHashCode);
            div = Memoize<DivExpr>((x, y) => Equals(x.Numerator, y.Numerator) && Equals(x.Denominator, y.Denominator), getHashCode);
        }
        Expr IBuilder.Add(params Expr[] args) {
            return add(new AddExpr(this, ImmutableArray.Create(args)));
        }

        Expr IBuilder.Multiply(params Expr[] args) {
            return mult(new MultExpr(this, ImmutableArray.Create(args)));
        }
        Expr IBuilder.Divide(Expr a, Expr b) {
            return div(new DivExpr(this, a, b));
        }
        Expr IBuilder.Power(Expr value, BigInteger pow) {
            return power(new PowerExpr(this, value, pow));
        }
        Expr IBuilder.Sqrt(Expr value) {
            return sqrt(new SqrtExpr(this, value));
        }
        void IBuilder.Check(IEnumerable<Expr> args) {
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
