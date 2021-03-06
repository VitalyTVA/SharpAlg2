﻿using System;
using System.Numerics;
using Numerics;
using System.Linq;
using System.Diagnostics;
using ExprList = SharpAlg.Geo.Core.ImmutableListWrapper<SharpAlg.Geo.Core.Expr>;
using System.Collections.Generic;

namespace SharpAlg.Geo.Core {
    public abstract class Expr {
        public static readonly Expr One = 1;
        public static readonly Expr Zero = 0;
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

    public static class ExprExtensions {
        public static Expr sqrt(Expr value) {
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
        public static ExprList? AsAdd(this Expr expr)
            => Builder.AsAdd(expr);
        public static bool IsAdd(this Expr expr)
            => expr.AsAdd() != null;

        public static ExprList ToMult(this Expr expr)
            => Builder.ToMult(expr);
        public static ExprList? AsMult(this Expr expr)
            => Builder.AsMult(expr);
        public static bool IsMult(this Expr expr)
            => expr.AsMult() != null;

        public static Expr ToSqrt(this Expr expr)
            => Builder.ToSqrt(expr);
        public static Expr AsSqrt(this Expr expr)
            => Builder.AsSqrt(expr);
        public static bool IsSqrt(this Expr expr)
            => expr.AsSqrt() != null;

        public static DivInfo ToDiv(this Expr expr)
            => Builder.ToDiv(expr);
        public static DivInfo? AsDiv(this Expr expr)
            => Builder.AsDiv(expr);

        public static PowerInfo ToPower(this Expr expr)
            => Builder.ToPower(expr);
        public static PowerInfo? AsPower(this Expr expr)
            => Builder.AsPower(expr);

        public static string ToParam(this Expr expr)
            => Builder.ToParam(expr);
        public static string AsParam(this Expr expr)
            => Builder.AsParam(expr);
        public static bool IsParam(this Expr expr)
            => expr.AsParam() != null;

        public static BigRational ToConst(this Expr expr)
            => Builder.ToConst(expr);
        public static BigRational? AsConst(this Expr expr)
            => Builder.AsConst(expr);
        public static bool IsConst(this Expr expr)
            => expr.AsConst() != null;

        public static ParamPowerInfo? ParamOrParamPowerAsPowerInfo(this Expr expr) {
            return expr.AsPower().If(x => x.Value.IsParam()).With(x => (ParamPowerInfo?)new ParamPowerInfo(x.Value.ToParam(), x.Power))
                ?? expr.AsParam().With(x => (ParamPowerInfo?)new ParamPowerInfo(x, 1));
        }
        public static ExprList ExprOrMultToMult(this Expr expr) {
            return expr.AsMult() ?? MakeExprList(expr);
        }
        public static KoeffMultInfo  ExprOrMultToKoeffMultInfo(this Expr expr, Builder.CoreBuilder b) {
            var args = expr.ExprOrMultToMult();
            var @const = args.First().AsConst() ?? BigRational.One;
            return new KoeffMultInfo(@const, (args.First().IsConst() ? args.Tail() : args).ToExprList());
        }
        public static ExprList ExprOrAddToAdd(this Expr expr) {
            return expr.AsAdd() ?? MakeExprList(expr);
        }
        public static PowerInfo ExprOrPowerToPower(this Expr expr) {
            return expr.AsPower() ?? new PowerInfo(expr, BigInteger.One);
        }
        public static DivInfo ExprOrDivToDiv(this Expr expr) {
            return expr.AsDiv() ?? new DivInfo(expr, 1);
        }
        public static ExprList ToExprList(this IEnumerable<Expr> source) {
            if((source is Expr[]))
                throw new InvalidOperationException();
            var list = (source as IList<Expr>) ?? source.ToList();
            return new ExprList(list);
        }
        public static ExprList MakeExprList(Expr e1) {
            return new ExprList(new[] { e1 });
        }
        public static ExprList MakeExprList(Expr e1, Expr e2) {
            return new ExprList(new[] { e1, e2 });
        }
        public static ExprList MakeExprList(Expr e1, Expr e2, Expr e3) {
            return new ExprList(new[] { e1, e2, e3 });
        }
        public static readonly ExprList EmptyExprList = new ExprList(new Expr[0]);
        //public static bool IsParamOrPower(this Expr expr) {
        //    return expr.ParamOrPowerAsPowerInfo() != null;
        //}
    }
    public class CannotImplicitlyCreateExpressionException : Exception { }
    public class PowerShouldBePositiveException : Exception { }
    public class InvalidExpressionException : Exception { }
    public class CannotMixExpressionsFromDifferentBuildersException : Exception { }
}