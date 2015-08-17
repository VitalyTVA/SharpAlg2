using Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static SharpAlg.Geo.Core.Utility;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;

namespace SharpAlg.Geo.Core {
    public sealed class Builder {
        #region classes
        sealed class ParamExpr : Expr {
            public readonly string Name;
            public ParamExpr(string name)
                : base(HashCodeProvider.ParamHash(name)) {
                if(string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");
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
        sealed class ConstExpr : Expr {
            public readonly BigRational Value;
            public ConstExpr(BigRational value)
                : base(HashCodeProvider.ConstHash(value)) {
                Value = value;
            }
            public override bool Equals(object obj) {
                var other = obj as ConstExpr;
                return other != null && other.Value == Value;
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
        #endregion

        #region access methods
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static T MatchDefault<T>(Expr expr,
            Func<Expr, T> @default,
            Func<ExprList, T> add = null,
            Func<ExprList, T> mult = null,
            Func<Expr, Expr, T> div = null,
            Func<Expr, BigInteger, T> power = null,
            Func<Expr, T> sqrt = null,
            Func<string, T> param = null,
            Func<BigRational, T> @const = null) 
        {
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
        internal static ExprList? AsAdd(Expr expr)
            => (expr as AddExpr)?.Args;

        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static ExprList ToMult(Expr expr)
            => ((MultExpr)expr).Args;
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static ExprList? AsMult(Expr expr)
            => (expr as MultExpr)?.Args;


        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static Expr ToSqrt(Expr expr)
            => ((SqrtExpr)expr).Value;
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static Expr AsSqrt(Expr expr)
            => (expr as SqrtExpr)?.Value;

        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static DivInfo ToDiv(Expr expr) {
            var divExpr = (DivExpr)expr;
            return new DivInfo(divExpr.Numerator, divExpr.Denominator);
        }
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static DivInfo? AsDiv(Expr expr) {
            return (expr as DivExpr).With(x => (DivInfo?)new DivInfo(x.Numerator, x.Denominator));
        }

        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static PowerInfo ToPower(Expr expr) {
            var powerExpr = (PowerExpr)expr;
            return new PowerInfo(powerExpr.Value, powerExpr.Power);
        }
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static PowerInfo? AsPower(Expr expr) {
            return (expr as PowerExpr).With(x => (PowerInfo?)new PowerInfo(x.Value, x.Power));
        }

        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static string ToParam(Expr expr)
            => ((ParamExpr)expr).Name;
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static string AsParam(Expr expr)
            => (expr as ParamExpr)?.Name;

        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static Expr Param(string name)
            => new ParamExpr(name);
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static Expr Const(BigRational value)
            => new ConstExpr(value);
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static BigRational ToConst(Expr expr)
            => ((ConstExpr)expr).Value;
        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
        internal static BigRational? AsConst(Expr expr)
            => (expr as ConstExpr)?.Value;
        #endregion

        #region CoreBuilder
        public class CoreBuilder {
            static Func<T, T> Memoize<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) {
                return Func((T x) => x).Memoize(new DelegateEqualityComparer<T>(equals, x => x.GetHashCode()));
            }
            public static Func<Builder, CoreBuilder> CachingFactory(Func<Expr, int> getHashCode) {
                return b => new CoreBuilder(b,
                    add: Memoize<AddExpr>((x, y) => Enumerable.SequenceEqual(x.Args, y.Args), getHashCode),
                    mult: Memoize<MultExpr>((x, y) => Enumerable.SequenceEqual(x.Args, y.Args), getHashCode),
                    sqrt: Memoize<SqrtExpr>((x, y) => Equals(x.Value, y.Value), getHashCode),
                    power: Memoize<PowerExpr>((x, y) => Equals(x.Value, y.Value) && Equals(x.Power, y.Power), getHashCode),
                    div: Memoize<DivExpr>((x, y) => Equals(x.Numerator, y.Numerator) && Equals(x.Denominator, y.Denominator), getHashCode),
                    check: (builder, args) => {
                        if(args.OfType<ComplexExpr>().Any(x => x.Builder != builder))
                            throw new CannotMixExpressionsFromDifferentBuildersException();
                    });
            }
            public static CoreBuilder CreateSimple(Builder builder)
                => new CoreBuilder(builder, x => x, x => x, x => x, x => x, x => x, (b, args) => { });

            readonly Builder owner;
            readonly Func<AddExpr, AddExpr> add;
            readonly Func<MultExpr, MultExpr> mult;
            readonly Func<SqrtExpr, SqrtExpr> sqrt;
            readonly Func<PowerExpr, PowerExpr> power;
            readonly Func<DivExpr, DivExpr> div;
            public readonly Action<Builder, IEnumerable<Expr>> Check;
            CoreBuilder(Builder owner, Func<AddExpr, AddExpr> add, Func<MultExpr, MultExpr> mult, Func<SqrtExpr, SqrtExpr> sqrt, Func<PowerExpr, PowerExpr> power, Func<DivExpr, DivExpr> div, Action<Builder, IEnumerable<Expr>> check) {
                this.owner = owner;
                this.add = add;
                this.mult = mult;
                this.sqrt = sqrt;
                this.power = power;
                this.div = div;
                Check = check;
            }
            public Expr Add(ExprList args)
                => add(new AddExpr(owner, args));
            public Expr Multiply(ExprList args)
                => mult(new MultExpr(owner, args));
            public Expr Divide(Expr a, Expr b)
                => div(new DivExpr(owner, a, b));
            public Expr Power(Expr value, BigInteger pow)
                => power(new PowerExpr(owner, value, pow));
            public Expr Sqrt(Expr value)
                => sqrt(new SqrtExpr(owner, value));
        }
        #endregion

        public static readonly Builder Simple = new Builder(CoreBuilder.CreateSimple, DefaultTransformer.GetFactory());
        public static Builder CreateSimple() {
            return CreateCaching(x => 0, null);
        }
        public static Builder CreateRealLife(bool openBraces = true) {
            return CreateCaching(x => x.GetHashCode(), SingleDivTransformer.GetFactory(openBraces));
        }
        static Builder CreateCaching(Func<Expr, int> getHashCode, Func<CoreBuilder, Transformer> createTransfomer) {
            return new Builder(CoreBuilder.CachingFactory(getHashCode), createTransfomer ?? DefaultTransformer.GetFactory());
        }

        readonly CoreBuilder builder;
        readonly Transformer transformer;

        Builder(Func<Builder, CoreBuilder> createBuilder, Func<CoreBuilder, Transformer> createTransfomer) {
            builder = createBuilder(this);
            transformer = createTransfomer(builder);
        }

        public Expr Add(params Expr[] args) {
            return transformer.Add(builder, args);
        }
        public Expr Multiply(params Expr[] args) {
            return transformer.Mult(builder, args);
        }
        public Expr Divide(Expr a, Expr b) {
            return transformer.Div(builder, a, b);
        }
        public Expr Power(Expr value, BigInteger pow) {
            return transformer.Power(builder, value , pow);
        }
        public Expr Sqrt(Expr value) {
            return transformer.Sqrt(builder, value);
        }
        public void Check(IEnumerable<Expr> args) {
            builder.Check(this, args);
        }
    }
}
