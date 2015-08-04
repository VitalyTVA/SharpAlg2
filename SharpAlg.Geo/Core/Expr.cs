using System;
using System.Collections.Immutable;
using System.Numerics;
using Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace SharpAlg.Geo.Core {
    public abstract class Expr {
        public static implicit operator Expr(int val) {
            return new ConstExpr(val);
        }

        public static AddExpr operator +(Expr a, Expr b) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static MultExpr operator *(Expr a, Expr b) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static MultExpr operator -(Expr a) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static AddExpr operator -(Expr a, Expr b) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static DivExpr operator /(Expr a, Expr b) {
            throw new CannotImplicitlyCreateExpressionException();
        }

        public static PowerExpr operator ^(Expr value,  BigInteger power) {
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

    public class AddExpr : Expr {
        public readonly ImmutableArray<Expr> Args;
        public AddExpr(ImmutableArray<Expr> args) 
            : base(HashCodeProvider.Add(args)) {
            Args = args;
        }
    }

    public class MultExpr : Expr {
        public readonly ImmutableArray<Expr> Args;
        public MultExpr(ImmutableArray<Expr> args) 
            : base(HashCodeProvider.Mult(args)) {
            Args = args;
        }
    }

    public class DivExpr : Expr {
        public readonly Expr Numerator, Denominator;
        public DivExpr(Expr numerator, Expr denominator)
            : base(HashCodeProvider.Div(numerator, denominator)) {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    public class PowerExpr : Expr {
        public readonly Expr Value;
        public readonly BigInteger Power;
        public PowerExpr(Expr value, BigInteger power)
            : base(HashCodeProvider.Power(value, power)) {
            if(power < 1)
                throw new PowerShouldBePositiveException();
            Value = value;
            Power = power;
        }
    }

    public class SqrtExpr : Expr {
        public readonly Expr Value;
        public SqrtExpr(Expr value)
            : base(HashCodeProvider.Sqrt(value)) {
            Value = value;
        }
    }

    public class ParamExpr : Expr {
        public static implicit operator ParamExpr(string name) {
            return new ParamExpr(name);
        }
        public readonly string Name;
        public ParamExpr(string name)
            : base(HashCodeProvider.Param(name)) {
            Name = name;
        }
    }

    public class ConstExpr : Expr {
        public readonly BigRational Value;
        public ConstExpr(BigRational value)
            : base(HashCodeProvider.Const(value)) {
            Value = value;
        }
    }

    public static class HashCodeProvider {
        readonly static int ParamSalt, ConstSalt, PowerSalt, DivSalt, SqrtSalt, AddSalt, MultSalt;
        static HashCodeProvider() {
            var rnd = new Random(0);
            Func<int> getSalt = () => rnd.Next(int.MinValue, int.MaxValue);
            ParamSalt = getSalt();
            ConstSalt = getSalt();
            PowerSalt = getSalt();
            DivSalt = getSalt();
            SqrtSalt = getSalt();
            AddSalt = getSalt();
            MultSalt = getSalt();
        }

        public static int Param(string name) 
            => Single(ParamSalt, name);
        public static int Const(BigRational value) 
            => Single(ConstSalt, value);
        public static int Power(Expr value, BigInteger power) 
            => Pair(PowerSalt, value, power);
        public static int Div(Expr numerator, Expr denominator) 
            => Pair(DivSalt, numerator, denominator);
        public static int Add(IEnumerable<Expr> args) 
            => Sequence(AddSalt, args);
        public static int Mult(IEnumerable<Expr> args) 
            => Sequence(MultSalt, args);
        public static int Sqrt(Expr value) 
            => Single(SqrtSalt, value);

        static int Pair<T1, T2>(int salt, T1 value1, T2 value2) => salt ^ value1.GetHashCode() ^ value2.GetHashCode();
        static int Single<T>(int salt, T value) => salt ^ value.GetHashCode();
        static int Sequence<T>(int salt, IEnumerable<T> args) => args.Aggregate(salt, (hash, x) => hash ^ x.GetHashCode());
    }

    public static class ExprExtensions {
        public static SqrtExpr Sqrt(Expr value) {
            throw new CannotImplicitlyCreateExpressionException();
            //return new SqrtExpr(value);
        }
        [DebuggerStepThrough]
        public static T MatchStrict<T>(this Expr expr, Func<AddExpr, T> add, Func<MultExpr, T> mult, Func<DivExpr, T> div, Func<PowerExpr, T> power, Func<SqrtExpr, T> sqrt, Func<ParamExpr, T> param, Func<ConstExpr, T> @const) {
            var addExpr = expr as AddExpr;
            if(addExpr != null)
                return add(addExpr);
            var multExpr = expr as MultExpr;
            if(multExpr != null)
                return mult(multExpr);
            var divExpr = expr as DivExpr;
            if(divExpr != null)
                return div(divExpr);
            var powerExpr = expr as PowerExpr;
            if(powerExpr != null)
                return power(powerExpr);
            var sqrtExpr = expr as SqrtExpr;
            if(sqrtExpr != null)
                return sqrt(sqrtExpr);
            var paramExpr = expr as ParamExpr;
            if(paramExpr != null)
                return param(paramExpr);
            var constExpr = expr as ConstExpr;
            if(constExpr != null)
                return @const(constExpr);
            throw new InvalidOperationException();
        }
        [DebuggerStepThrough]
        public static T MatchDefault<T>(this Expr expr, Func<Expr, T> @default, Func<AddExpr, T> add = null, Func<MultExpr, T> mult = null, Func<DivExpr, T> div = null, Func<PowerExpr, T> power = null, Func<SqrtExpr, T> sqrt = null, Func<ParamExpr, T> param = null, Func<ConstExpr, T> @const = null) {
            return expr.MatchStrict(
                add ?? @default,
                mult ?? @default,
                div ?? @default,
                power ?? @default,
                sqrt ?? @default,
                param ?? @default,
                @const ?? @default
            );
        }
        static T Evaluate<T>(this Expr expr, Func<T, T, T> add, Func<T, T, T> mult, Func<T, T, T> div, Func<T, T, T> power, Func<T, T> sqrt, Func<string, T> param, Func<BigRational, T> @const) {
            //Func<Expr, T> eval = x => x.Evaluate(add, mult, div, power, sqrt, param, @const);
            Func<Expr, T> doEval = null;
            doEval = e => e.MatchStrict(
                add: x => x.Args.Select(doEval).Aggregate(add),
                mult: x => x.Args.Select(doEval).Aggregate(mult),
                div: x => div(doEval(x.Numerator), doEval(x.Denominator)),
                power: x => power(doEval(x.Value), @const(x.Power)),
                sqrt: x => sqrt(doEval(x.Value)),
                param: x => param(x.Name),
                @const: x => @const(x.Value)
            );
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

        public static AddExpr Add(params Expr[] args) {
            return new AddExpr(ImmutableArray.Create(args));
        }
        public static MultExpr Multiply(params Expr[] args) {
            return new MultExpr(ImmutableArray.Create(args));
        }
        public static MultExpr Minus(Expr a) {
            return new MultExpr(ImmutableArray.Create(-1, a));
        }
        public static AddExpr Subtract(Expr a, Expr b) {
            return Add(a, Minus(b));
        }
        public static DivExpr Divide(Expr a, Expr b) {
            return new DivExpr(a, b);
        }
        public static PowerExpr Power(Expr value, BigInteger power) {
            return new PowerExpr(value, power);
        }
        public static ConstExpr Const(BigRational value) {
            return new ConstExpr(value);
        }
        public static ParamExpr Param(string name) {
            return new ParamExpr(name);
        }
        public static Expr Tail(this MultExpr multi) {
            return Multiply(multi.Args.Tail().ToArray());
        }
        public static bool IsFraction(this BigRational value) {
            return value.Denominator != BigInteger.One;
        }
    }
    public class CannotImplicitlyCreateExpressionException : Exception { }
    public class PowerShouldBePositiveException : Exception { }
    public class InvalidExpressionException : Exception { }
}