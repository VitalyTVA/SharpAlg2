using System;
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
    public sealed class CachingBuilder : IBuilder {
        class ExprEqualityComparer : IEqualityComparer<Expr> {
            readonly Func<Expr, int> getHashCode;
            public ExprEqualityComparer(Func<Expr, int> getHashCode) {
                this.getHashCode = getHashCode;
            }

            bool IEqualityComparer<Expr>.Equals(Expr x, Expr y) {
                if(x.GetType() != y.GetType())
                    return false;
                return x.MatchDefault(
                    e => { throw new InvalidOperationException(); },
                    add: args => Enumerable.SequenceEqual(args, ((AddExpr)y).Args),
                    mult: args => Enumerable.SequenceEqual(args, ((MultExpr)y).Args),
                    sqrt: val => Equals(val, ((SqrtExpr)y).Value),
                    power: (val, power) => Equals(val, ((PowerExpr)y).Value) && Equals(power, ((PowerExpr)y).Power),
                    div: (num, den) => Equals(num, ((DivExpr)y).Numerator) && Equals(den, ((DivExpr)y).Denominator)
                );
            }

            int IEqualityComparer<Expr>.GetHashCode(Expr obj) {
                return getHashCode(obj);
            }
        }

        readonly IDictionary<Expr, Expr> cache;
        public CachingBuilder() {
            cache = new Dictionary<Expr, Expr>(new ExprEqualityComparer(x => x.GetHashCode()));

        }
        Expr IBuilder.Add(params Expr[] args) {
            return GetCachedExpr(new AddExpr(this, ImmutableArray.Create(args)));
        }

        Expr IBuilder.Multiply(params Expr[] args) {
            return GetCachedExpr(new MultExpr(this, ImmutableArray.Create(args)));
        }
        Expr IBuilder.Divide(Expr a, Expr b) {
            return GetCachedExpr(new DivExpr(this, a, b));
        }
        Expr IBuilder.Power(Expr value, BigInteger power) {
            return GetCachedExpr(new PowerExpr(this, value, power));
        }
        Expr IBuilder.Sqrt(Expr value) {
            return GetCachedExpr(new SqrtExpr(this, value));
        }
        void IBuilder.Check(IEnumerable<Expr> args) {
            if(args.OfType<ComplexExpr>().Any(x => x.Builder != this))
                throw new CannotMixExpressionsFromDifferentBuildersException();
        }

        Expr GetCachedExpr(Expr e) {
            return cache.GetOrAdd(e, e);
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
