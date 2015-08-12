using System;
using System.Numerics;
using Numerics;
using System.Linq;
using System.Diagnostics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;

namespace SharpAlg.Geo.Core {
    public abstract class Expr {
        public static implicit operator Expr(int val) {
            return Builder.Const(val);
        }

        public static Expr operator +(Expr a, Expr b) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static Expr operator *(Expr a, Expr b) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static Expr operator -(Expr a) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static Expr operator -(Expr a, Expr b) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static Expr operator /(Expr a, Expr b) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static Expr operator ^(Expr value,  BigInteger power) {
            throw new CannotImplicitlyCreateExpressionException();
            //return new PowerExpr(value, power);
        }

        readonly int hashCode;
        protected Expr(int hashCode) {
            this.hashCode = hashCode;
        }
        public override string ToString() => this.Print();
        public override int GetHashCode() => hashCode;
    }

    public sealed class ConstExpr : Expr {
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

    public static class ExprExtensions {
        public static Expr Sqrt(Expr value) {
            throw new CannotImplicitlyCreateExpressionException();
            //return new SqrtExpr(value);
        }
        [DebuggerStepThrough]
        public static T MatchStrict<T>(this Expr expr, 
            Func<ExprList, T> add, 
            Func<ExprList, T> mult, 
            Func<Expr, Expr, T> div, 
            Func<Expr, BigInteger, T> power, 
            Func<Expr, T> sqrt, 
            Func<string, T> param, 
            Func<BigRational, T> @const) 
        {
            return expr.MatchDefault(
                x => { throw new InvalidOperationException(); },
                add,
                mult,
                div,
                power,
                sqrt,
                param,
                @const
            );

        }
        [DebuggerStepThrough]
        public static T MatchDefault<T>(this Expr expr,
            Func<Expr, T> @default,
            Func<ExprList, T> add = null,
            Func<ExprList, T> mult = null,
            Func<Expr, Expr, T> div = null,
            Func<Expr, BigInteger, T> power = null,
            Func<Expr, T> sqrt = null,
            Func<string, T> param = null,
            Func<BigRational, T> @const = null)
            => Builder.MatchDefault(expr, @default, add, mult, div, power, sqrt, param, @const);

        static T Evaluate<T>(this Expr expr, Func<T, T, T> add, Func<T, T, T> mult, Func<T, T, T> div, Func<T, T, T> power, Func<T, T> sqrt, Func<string, T> param, Func<BigRational, T> @const) {
            Func<Expr, T> doEval = null;
            doEval = e => e.MatchStrict(
                add: x => x.Select(doEval).Aggregate(add),
                mult: x => x.Select(doEval).Aggregate(mult),
                div: (x, y) => div(doEval(x), doEval(y)),
                power: (x, y) => power(doEval(x), @const(y)),
                sqrt: x => sqrt(doEval(x)),
                param: x => param(x),
                @const: x => @const(x)
            );
            doEval = doEval.Memoize();
            return doEval(expr);
        }
        public static double ToReal(this Expr expr, Func<string, double> param) {
            return expr.Evaluate(
                add: (x, y) => x + y,
                mult: (x, y) => x * y,
                div: (x, y) => x / y,
                power: (x, y) => Math.Pow(x, y),
                sqrt: x => Math.Sqrt(x),
                param: param,
                @const: x => (double)x
            );
        }
        public static double ToReal(this Expr expr, ImmutableContext context) => expr.ToReal(context.GetValue);

        public static Expr Const(BigRational value)
            => Builder.Const(value);
        public static Expr Param(string name) 
            => Builder.Param(name);

        public static bool IsFraction(this BigRational value) 
            => value.Denominator != BigInteger.One;

        public static ExprList ToAdd(this Expr expr) 
            => Builder.ToAdd(expr);
        public static ExprList ToMult(this Expr expr)
            => Builder.ToMult(expr);
        public static Expr ToSqrt(this Expr expr)
            => Builder.ToSqrt(expr);
        public static DivInfo ToDiv(this Expr expr)
            => Builder.ToDiv(expr);
        public static PowerInfo ToPower(this Expr expr)
            => Builder.ToPower(expr);
        public static string ToParam(this Expr expr)
            => Builder.ToParam(expr);
    }
    public class CannotImplicitlyCreateExpressionException : Exception { }
    public class PowerShouldBePositiveException : Exception { }
    public class InvalidExpressionException : Exception { }
    public class CannotMixExpressionsFromDifferentBuildersException : Exception { }
}