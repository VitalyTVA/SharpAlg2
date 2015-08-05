using System;
using System.Collections.Immutable;
using System.Numerics;
using Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;

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
        public readonly ExprList Args;
        public AddExpr(ExprList args) 
            : base(HashCodeProvider.AddHash(args)) {
            Args = args;
        }
    }

    public class MultExpr : Expr {
        public readonly ExprList Args;
        public MultExpr(ExprList args) 
            : base(HashCodeProvider.MultHash(args)) {
            Args = args;
        }
    }

    public class DivExpr : Expr {
        public readonly Expr Numerator, Denominator;
        public DivExpr(Expr numerator, Expr denominator)
            : base(HashCodeProvider.DivHash(numerator, denominator)) {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    public class PowerExpr : Expr {
        public readonly Expr Value;
        public readonly BigInteger Power;
        public PowerExpr(Expr value, BigInteger power)
            : base(HashCodeProvider.PowerHash(value, power)) {
            if(power < 1)
                throw new PowerShouldBePositiveException();
            Value = value;
            Power = power;
        }
    }

    public class SqrtExpr : Expr {
        public readonly Expr Value;
        public SqrtExpr(Expr value)
            : base(HashCodeProvider.SqrtHash(value)) {
            Value = value;
        }
    }

    public class ParamExpr : Expr {
        public static implicit operator ParamExpr(string name) {
            return new ParamExpr(name);
        }
        public readonly string Name;
        public ParamExpr(string name)
            : base(HashCodeProvider.ParamHash(name)) {
            Name = name;
        }
    }

    public class ConstExpr : Expr {
        public readonly BigRational Value;
        public ConstExpr(BigRational value)
            : base(HashCodeProvider.ConstHash(value)) {
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

        public static int ParamHash(string name) 
            => SingleHash(ParamSalt, name);
        public static int ConstHash(BigRational value) 
            => SingleHash(ConstSalt, value);
        public static int PowerHash(Expr value, BigInteger power) 
            => PairHash(PowerSalt, value, power);
        public static int DivHash(Expr numerator, Expr denominator) 
            => PairHash(DivSalt, numerator, denominator);
        public static int AddHash(IEnumerable<Expr> args) 
            => SequenceHash(AddSalt, args);
        public static int MultHash(IEnumerable<Expr> args) 
            => SequenceHash(MultSalt, args);
        public static int SqrtHash(Expr value) 
            => SingleHash(SqrtSalt, value);

        static int PairHash<T1, T2>(int salt, T1 value1, T2 value2) => salt ^ value1.GetHashCode() ^ value2.GetHashCode();
        static int SingleHash<T>(int salt, T value) => salt ^ value.GetHashCode();
        static int SequenceHash<T>(int salt, IEnumerable<T> args) => args.Aggregate(salt, (hash, x) => hash ^ x.GetHashCode());
    }

    public static class ExprExtensions {
        public static SqrtExpr Sqrt(Expr value) {
            throw new CannotImplicitlyCreateExpressionException();
            //return new SqrtExpr(value);
        }
        [DebuggerStepThrough]
        public static T MatchStrict<T>(this Expr expr, 
            Func<ExprList, T> add, 
            Func<ExprList, T> mult, 
            Func<DivExpr, T> div, 
            Func<PowerExpr, T> power, 
            Func<SqrtExpr, T> sqrt, 
            Func<ParamExpr, T> param, 
            Func<ConstExpr, T> @const) 
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
            Func<DivExpr, T> div = null, 
            Func<PowerExpr, T> power = null, 
            Func<SqrtExpr, T> sqrt = null, 
            Func<ParamExpr, T> param = null, Func<ConstExpr, T> @const = null) 
        {
            var addExpr = expr as AddExpr;
            if(addExpr != null)
                return add != null ? add(addExpr.Args) : @default(expr);
            var multExpr = expr as MultExpr;
            if(multExpr != null)
                return mult != null ? mult(multExpr.Args) : @default(expr);
            var divExpr = expr as DivExpr;
            if(divExpr != null)
                return div != null ? div(divExpr) : @default(expr);
            var powerExpr = expr as PowerExpr;
            if(powerExpr != null)
                return power != null ? power(powerExpr) : @default(expr);
            var sqrtExpr = expr as SqrtExpr;
            if(sqrtExpr != null)
                return sqrt != null ? sqrt(sqrtExpr) : @default(expr);
            var paramExpr = expr as ParamExpr;
            if(paramExpr != null)
                return param != null ? param(paramExpr) : @default(expr);
            var constExpr = expr as ConstExpr;
            if(constExpr != null)
                return @const != null ? @const(constExpr) : @default(expr);
            throw new InvalidOperationException();
        }
        static T Evaluate<T>(this Expr expr, Func<T, T, T> add, Func<T, T, T> mult, Func<T, T, T> div, Func<T, T, T> power, Func<T, T> sqrt, Func<string, T> param, Func<BigRational, T> @const) {
            //Func<Expr, T> eval = x => x.Evaluate(add, mult, div, power, sqrt, param, @const);
            Func<Expr, T> doEval = null;
            doEval = e => e.MatchStrict(
                add: x => x.Select(doEval).Aggregate(add),
                mult: x => x.Select(doEval).Aggregate(mult),
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

        public static Expr Add(params Expr[] args) {
            return new AddExpr(ImmutableArray.Create(args));
        }
        public static Expr Multiply(params Expr[] args) {
            return new MultExpr(ImmutableArray.Create(args));
        }
        public static Expr Minus(Expr a) {
            return new MultExpr(ImmutableArray.Create(-1, a));
        }
        public static Expr Subtract(Expr a, Expr b) {
            return Add(a, Minus(b));
        }
        public static Expr Divide(Expr a, Expr b) {
            return new DivExpr(a, b);
        }
        public static Expr Power(Expr value, BigInteger power) {
            return new PowerExpr(value, power);
        }
        public static Expr Const(BigRational value) {
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