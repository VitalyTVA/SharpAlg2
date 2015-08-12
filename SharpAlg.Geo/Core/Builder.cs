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
        static Func<T, T> Memoize<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) {
            return Func((T x) => x).Memoize(new DelegateEqualityComparer<T>(equals, x => x.GetHashCode()));
        }
        public static readonly IBuilder Simple = new CachingBuilder(Id<AddExpr>(), Id<MultExpr>(), Id<SqrtExpr>(), Id<PowerExpr>(), Id<DivExpr>(), (builder, args) => { });
        public static IBuilder CreateSimple() {
            return CreateCaching(x => 0);
        }
        public static IBuilder CreateRealLife() {
            return CreateCaching(x => x.GetHashCode());
        }

        static IBuilder CreateCaching(Func<Expr, int> getHashCode) {
            return new CachingBuilder(
                add: Memoize<AddExpr>((x, y) => Enumerable.SequenceEqual(x.Args, y.Args), getHashCode),
                mult: Memoize<MultExpr>((x, y) => Enumerable.SequenceEqual(x.Args, y.Args), getHashCode),
                sqrt: Memoize<SqrtExpr>((x, y) => Equals(x.Value, y.Value), getHashCode),
                power: Memoize<PowerExpr>((x, y) => Equals(x.Value, y.Value) && Equals(x.Power, y.Power), getHashCode),
                div: Memoize<DivExpr>((x, y) => Equals(x.Numerator, y.Numerator) && Equals(x.Denominator, y.Denominator), getHashCode),
                check: (builder, args) => {
                    if(args.OfType<ComplexExpr>().Any(x => x.Builder != builder))
                        throw new CannotMixExpressionsFromDifferentBuildersException();
                }
            );
        }

        readonly Func<AddExpr, AddExpr> add;
        readonly Func<MultExpr, MultExpr> mult;
        readonly Func<SqrtExpr, SqrtExpr> sqrt;
        readonly Func<PowerExpr, PowerExpr> power;
        readonly Func<DivExpr, DivExpr> div;
        readonly Action<IBuilder, IEnumerable<Expr>> check;

        CachingBuilder(Func<AddExpr, AddExpr> add, Func<MultExpr, MultExpr> mult, Func<SqrtExpr, SqrtExpr> sqrt, Func<PowerExpr, PowerExpr> power, Func<DivExpr, DivExpr> div, Action<IBuilder, IEnumerable<Expr>> check) {
            this.add = add;
            this.mult = mult;
            this.sqrt = sqrt;
            this.power = power;
            this.div = div;
            this.check = check;
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
            check(this, args);
        }
    }
}
