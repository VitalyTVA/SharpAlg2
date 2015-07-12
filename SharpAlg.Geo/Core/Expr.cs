using System;
using System.Collections.Immutable;
using System.Numerics;
using Numerics;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using LegacyExpr = SharpAlg.Native.Expr;

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

        public override string ToString() {
            return Native.ExpressionExtensions.Print(this.ToLegacy());
        }
    }

    public class AddExpr : Expr {
        public readonly ImmutableArray<Expr> Args;
        public AddExpr(ImmutableArray<Expr> args) {
            Args = args;
        }
    }

    public class MultExpr : Expr {
        public readonly ImmutableArray<Expr> Args;
        public MultExpr(ImmutableArray<Expr> args) {
            Args = args;
        }
    }

    public class DivExpr : Expr {
        public readonly Expr Numerator, Denominator;
        public DivExpr(Expr numerator, Expr denominator) {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    public class PowerExpr : Expr {
        public readonly Expr Value;
        public readonly BigInteger Power;
        public PowerExpr(Expr value, BigInteger power) {
            if(power < 1)
                throw new PowerShouldBePositiveException();
            Value = value;
            Power = power;
        }
    }

    public class SqrtExpr : Expr {
        public readonly Expr Value;
        public SqrtExpr(Expr value) {
            Value = value;
        }
    }

    public class ParamExpr : Expr {
        public static implicit operator ParamExpr(string name) {
            return new ParamExpr(name);
        }
        public readonly string Name;
        public ParamExpr(string name) {
            Name = name;
        }
    }

    public class ConstExpr : Expr {
        public readonly BigRational Value;
        public ConstExpr(BigRational value) {
            Value = value;
        }
    }

    public static class ExprExtensions {
        public static SqrtExpr Sqrt(Expr value) {
            throw new CantImplicitlyCreateExpressionException();
            //return new SqrtExpr(value);
        }
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
        public static LegacyExpr ToLegacy(this Expr expr) {
            return expr.MatchStrict(
                add: x => LegacyExpr.Add(x.Args.Select(ToLegacy)),
                mult: x => LegacyExpr.Multiply(x.Args.Select(ToLegacy)),
                div: x => LegacyExpr.Divide(x.Numerator.ToLegacy(), x.Denominator.ToLegacy()),
                power: x => LegacyExpr.Power(x.Value.ToLegacy(), new ConstExpr(x.Power).ToLegacy()),
                sqrt: x => LegacyExpr.Function("sqrt", x.Value.ToLegacy()),
                param: x => LegacyExpr.Parameter(x.Name),
                @const: x => Native.ExpressionExtensions.Parse(x.Value.ToString())
            );
        }
        public static Expr Build(Expression<Func<Expr>> f) {
            return BuildExpr(f);
        }
        public static Expr Build(Expression<Func<Expr, Expr>> f, Expr x1) {
            return BuildExpr(f, x1);
        }
        public static Expr Build(Expression<Func<Expr, Expr, Expr>> f, Expr x1, Expr x2) {
            return BuildExpr(f, x1, x2);
        }
        public static Expr Build(Expression<Func<Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3) {
            return BuildExpr(f, x1, x2, x3);
        }

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
        public static Expr Tail(this MultExpr multi) {
            return Multiply(multi.Args.Skip(1).ToArray());
        }
    }
    public class CantImplicitlyCreateExpressionException : ApplicationException { }
    public class PowerShouldBePositiveException : ApplicationException { }
    public class InvalidExpressionException : ApplicationException { }
}