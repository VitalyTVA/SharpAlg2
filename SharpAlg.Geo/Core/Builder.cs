using Numerics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static SharpAlg.Geo.Core.Utility;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;

namespace SharpAlg.Geo.Core {
    public sealed class Builder {
        sealed class ParamExpr : Expr {
            public readonly string Name;
            public ParamExpr(string name)
                : base(HashCodeProvider.ParamHash(name)) {
                Name = name;
            }
            public override bool Equals(object obj) {
                var other = obj as ParamExpr;
                return other != null && string.Equals(other.Name, Name, StringComparison.Ordinal);
            }
            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }
        abstract class ComplexExpr : Expr {
            public readonly Builder Builder;
            protected ComplexExpr(Builder builder, int hashCode)
                : base(hashCode) {
                Builder = builder;
            }
        }
        sealed class AddExpr : ComplexExpr {
            public readonly ExprList Args;
            public AddExpr(Builder builder, ExprList args)
                : base(builder, HashCodeProvider.AddHash(args)) {
                builder.Check(args);
                Args = args;
            }
        }
        sealed class MultExpr : ComplexExpr {
            public readonly ExprList Args;
            public MultExpr(Builder builder, ExprList args)
                : base(builder, HashCodeProvider.MultHash(args)) {
                builder.Check(args);
                Args = args;
            }
        }
        sealed class SqrtExpr : ComplexExpr {
            public readonly Expr Value;
            public SqrtExpr(Builder builder, Expr value)
                : base(builder, HashCodeProvider.SqrtHash(value)) {
                builder.Check(value.Yield());
                Value = value;
            }
        }
        sealed class DivExpr : ComplexExpr {
            public readonly Expr Numerator, Denominator;
            public DivExpr(Builder builder, Expr numerator, Expr denominator)
                : base(builder, HashCodeProvider.DivHash(numerator, denominator)) {
                builder.Check(new[] { numerator, denominator });
                Numerator = numerator;
                Denominator = denominator;
            }
        }
        sealed class PowerExpr : ComplexExpr {
            public readonly Expr Value;
            public readonly BigInteger Power;
            public PowerExpr(Builder builder, Expr value, BigInteger power)
                : base(builder, HashCodeProvider.PowerHash(value, power)) {
                builder.Check(value.Yield());
                if(power < 1)
                    throw new PowerShouldBePositiveException();
                Value = value;
                Power = power;
            }
        }

        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static T MatchDefault<T>(Expr expr,
            Func<Expr, T> @default,
            Func<ExprList, T> add = null,
            Func<ExprList, T> mult = null,
            Func<Expr, Expr, T> div = null,
            Func<Expr, BigInteger, T> power = null,
            Func<Expr, T> sqrt = null,
            Func<string, T> param = null,
            Func<BigRational, T> @const = null) {
            var addExpr = expr as AddExpr;
            if(addExpr != null)
                return add != null ? add(addExpr.Args) : @default(expr);
            var multExpr = expr as MultExpr;
            if(multExpr != null)
                return mult != null ? mult(multExpr.Args) : @default(expr);
            var divExpr = expr as DivExpr;
            if(divExpr != null)
                return div != null ? div(divExpr.Numerator, divExpr.Denominator) : @default(expr);
            var powerExpr = expr as PowerExpr;
            if(powerExpr != null)
                return power != null ? power(powerExpr.Value, powerExpr.Power) : @default(expr);
            var sqrtExpr = expr as SqrtExpr;
            if(sqrtExpr != null)
                return sqrt != null ? sqrt(sqrtExpr.Value) : @default(expr);
            var paramExpr = expr as ParamExpr;
            if(paramExpr != null)
                return param != null ? param(paramExpr.Name) : @default(expr);
            var constExpr = expr as ConstExpr;
            if(constExpr != null)
                return @const != null ? @const(constExpr.Value) : @default(expr);
            throw new InvalidOperationException();
        }

        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static ExprList ToAdd(Expr expr) 
            => ((AddExpr)expr).Args;
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static ExprList ToMult(Expr expr)
            => ((MultExpr)expr).Args;
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static Expr ToSqrt(Expr expr)
            => ((SqrtExpr)expr).Value;
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static DivInfo ToDiv(Expr expr) {
            var divExpr = (DivExpr)expr;
            return new DivInfo(divExpr.Numerator, divExpr.Denominator);
        }
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static PowerInfo ToPower(Expr expr) {
            var powerExpr = (PowerExpr)expr;
            return new PowerInfo(powerExpr.Value, powerExpr.Power);
        }
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static string ToParam(Expr expr)
            => ((ParamExpr)expr).Name;
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static Expr Param(string name)
            => new ParamExpr(name);
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static Expr Const(BigRational value)
            => new ConstExpr(value);

        public static readonly Builder Simple = new Builder(x => x, x => x, x => x, x => x, x => x, (builder, args) => { });
        public static Builder CreateSimple() {
            return CreateCaching(x => 0);
        }
        public static Builder CreateRealLife() {
            return CreateCaching(x => x.GetHashCode());
        }
        static Builder CreateCaching(Func<Expr, int> getHashCode) {
            return new Builder(
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
        static Func<T, T> Memoize<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) {
            return Func((T x) => x).Memoize(new DelegateEqualityComparer<T>(equals, x => x.GetHashCode()));
        }

        readonly Func<AddExpr, AddExpr> add;
        readonly Func<MultExpr, MultExpr> mult;
        readonly Func<SqrtExpr, SqrtExpr> sqrt;
        readonly Func<PowerExpr, PowerExpr> power;
        readonly Func<DivExpr, DivExpr> div;
        readonly Action<Builder, IEnumerable<Expr>> check;

        Builder(Func<AddExpr, AddExpr> add, Func<MultExpr, MultExpr> mult, Func<SqrtExpr, SqrtExpr> sqrt, Func<PowerExpr, PowerExpr> power, Func<DivExpr, DivExpr> div, Action<Builder, IEnumerable<Expr>> check) {
            this.add = add;
            this.mult = mult;
            this.sqrt = sqrt;
            this.power = power;
            this.div = div;
            this.check = check;
        }

        public Expr Add(params Expr[] args) {
            return add(new AddExpr(this, ImmutableArray.Create(args)));
        }

        public Expr Multiply(params Expr[] args) {
            return mult(new MultExpr(this, ImmutableArray.Create(args)));
        }
        public Expr Divide(Expr a, Expr b) {
            return div(new DivExpr(this, a, b));
        }
        public Expr Power(Expr value, BigInteger pow) {
            return power(new PowerExpr(this, value, pow));
        }
        public Expr Sqrt(Expr value) {
            return sqrt(new SqrtExpr(this, value));
        }
        public void Check(IEnumerable<Expr> args) {
            check(this, args);
        }
    }
}
