using System;
using System.Collections.Immutable;
using System.Numerics;
using Numerics;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace SharpAlg.Geo.Core {
    public abstract class Expr {
        public static implicit operator Expr(int val) {
            return new ConstExpr(val);
        }

        public static AddExpr operator +(Expr a, Expr b) {
            throw new CantImplicitlyCreateExpressionException();
        }

        public static MultExpr operator *(Expr a, Expr b) {
            throw new CantImplicitlyCreateExpressionException();
        }

        public static MultExpr operator -(Expr a) {
            throw new CantImplicitlyCreateExpressionException();
        }

        public static AddExpr operator -(Expr a, Expr b) {
            throw new CantImplicitlyCreateExpressionException();
        }

        public static DivExpr operator /(Expr a, Expr b) {
            throw new CantImplicitlyCreateExpressionException();
        }

        public static PowerExpr operator ^(Expr value,  BigInteger power) {
            throw new CantImplicitlyCreateExpressionException();
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
            : base(0) {
            Args = args;
        }
    }

    public class MultExpr : Expr {
        public readonly ImmutableArray<Expr> Args;
        public MultExpr(ImmutableArray<Expr> args) 
            : base(0) {
            Args = args;
        }
    }

    public class DivExpr : Expr {
        public readonly Expr Numerator, Denominator;
        public DivExpr(Expr numerator, Expr denominator)
            : base(0) {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    public class PowerExpr : Expr {
        public readonly Expr Value;
        public readonly BigInteger Power;
        public PowerExpr(Expr value, BigInteger power)
            : base(0) {
            if(power < 1)
                throw new PowerShouldBePositiveException();
            Value = value;
            Power = power;
        }
    }

    public class SqrtExpr : Expr {
        public readonly Expr Value;
        public SqrtExpr(Expr value)
            : base(0) {
            Value = value;
        }
    }

    public class ParamExpr : Expr {
        public static implicit operator ParamExpr(string name) {
            return new ParamExpr(name);
        }
        public readonly string Name;
        public ParamExpr(string name)
            : base(name.GetHashCode()) {
            Name = name;
        }
    }

    public class ConstExpr : Expr {
        public readonly BigRational Value;
        public ConstExpr(BigRational value)
            : base(value.GetHashCode()) {
            Value = value;
        }
    }

    public static class ExprExtensions {
        public static SqrtExpr Sqrt(Expr value) {
            throw new CantImplicitlyCreateExpressionException();
            //return new SqrtExpr(value);
        }
        [DebuggerNonUserCode]
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
        [DebuggerNonUserCode]
        public static T MatchDefault<T>(this Expr expr, Func<Expr, T> @default, Func<AddExpr, T> add = null, Func<MultExpr, T> mult = null, Func<DivExpr, T> div = null, Func<PowerExpr, T> power = null, Func<SqrtExpr, T> sqrt = null, Func<ParamExpr, T> param = null, Func<ConstExpr, T> @const = null) {
            return expr.MatchStrict<T>(
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

        public static Expr Build(Expression<Func<Expr>> f) 
            => BuildExpr(f);
        public static Expr Build(Expression<Func<Expr, Expr>> f, Expr x1) 
            =>  BuildExpr(f, x1);
        public static Expr Build(Expression<Func<Expr, Expr, Expr>> f, Expr x1, Expr x2) 
            => BuildExpr(f, x1, x2);
        public static Expr Build(Expression<Func<Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3) 
            => BuildExpr(f, x1, x2, x3);
        public static Expr Build(Expression<Func<Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4) 
            => BuildExpr(f, x1, x2, x3, x4); 
        public static Expr Build(Expression<Func<Expr, Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4, Expr x5) 
            => BuildExpr(f, x1, x2, x3, x4, x5);
        public static Expr Build(Expression<Func<Expr, Expr, Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4, Expr x5, Expr x6) 
            => BuildExpr(f, x1, x2, x3, x4, x5, x6);


        static Expr BuildExpr(LambdaExpression expression, params Expr[] args) {
            if(expression.Parameters.Count != args.Length) {
                throw new InvalidOperationException();
            }
            var argsDict = expression.Parameters.Select((x, i) => new { Parameter = x, Expr = args[i] }).ToImmutableDictionary(x => x.Parameter, x => x.Expr);
            return BuildCore(expression.Body, x => argsDict[x]);
        }
        static readonly Expression<Func<Expr, Expr>> SqrtExpr = x => Sqrt(x);
        static Expr BuildCore(Expression expression, Func<ParameterExpression, Expr> getArgs) {
            if(expression.NodeType == ExpressionType.Parameter) {
                return getArgs((ParameterExpression)expression);
            }
            if(expression.NodeType == ExpressionType.Convert) {
                return new ConstExpr(GetConst(expression));
            }
            if(expression.NodeType == ExpressionType.Call) {
                var call = expression as MethodCallExpression;
                if(call != null && call.Method == ((MethodCallExpression)SqrtExpr.Body).Method) {
                    return new SqrtExpr(BuildCore(call.Arguments.Single(), getArgs));
                }
            }
            if(expression.NodeType == ExpressionType.Negate) {
                var unary = expression as UnaryExpression;
                return Minus(BuildCore(unary.Operand, getArgs));
            }
            if(expression.NodeType == ExpressionType.Add) {
                var binary = expression as BinaryExpression;
                return Add(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Subtract) {
                var binary = expression as BinaryExpression;
                return Subtract(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Multiply) {
                var binary = expression as BinaryExpression;
                return Multiply(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Divide) {
                var binary = expression as BinaryExpression;
                return Divide(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.ExclusiveOr) {
                var binary = expression as BinaryExpression;
                return Power(BuildCore(binary.Left, getArgs), GetConst(binary.Right));
            }
            throw new InvalidExpressionException();
        }
        static int GetConst(Expression expression) {
            var unary = expression as UnaryExpression;
            var constant = unary.Operand as ConstantExpression;
            return (int)constant.Value;
        }

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
    public class CantImplicitlyCreateExpressionException : ApplicationException { }
    public class PowerShouldBePositiveException : ApplicationException { }
    public class InvalidExpressionException : ApplicationException { }
}