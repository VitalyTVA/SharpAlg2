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
            //return new AddExpr(ImmutableArray.Create(a, b));
        }

        public static MultExpr operator *(Expr a, Expr b) {
            throw new CantImplicitlyCreateExpressionException();
            //return new MultExpr(ImmutableArray.Create(a, b));
        }

        public static MultExpr operator -(Expr a) {
            throw new CantImplicitlyCreateExpressionException();
            //return new MultExpr(ImmutableArray.Create(-1, a));
        }
        public static AddExpr operator -(Expr a, Expr b) {
            throw new CantImplicitlyCreateExpressionException();
            //return a + (-b);
        }

        public static DivExpr operator /(Expr a, Expr b) {
            throw new CantImplicitlyCreateExpressionException();
            //return new DivExpr(a, b);
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
        public static LegacyExpr ToLegacy(this Expr expr) {
            var add = expr as AddExpr;
            if(add != null)
                return LegacyExpr.Add(add.Args.Select(ToLegacy));
            var mult = expr as MultExpr;
            if(mult != null)
                return LegacyExpr.Multiply(mult.Args.Select(ToLegacy));
            var div = expr as DivExpr;
            if(div != null)
                return LegacyExpr.Divide(div.Numerator.ToLegacy(), div.Denominator.ToLegacy());
            var power = expr as PowerExpr;
            if(power != null)
                return LegacyExpr.Power(power.Value.ToLegacy(), new ConstExpr(power.Power).ToLegacy());
            //var sqrt = expr as SqrtExpr;
            //if(sqrt != null)
            //    return LegacyExpr.Power(sqrt.Value.ToLegacy(), ExprHelper.Half);
            var param = expr as ParamExpr;
            if(param != null)
                return LegacyExpr.Parameter(param.Name);
            var @const = expr as ConstExpr;
            if(@const != null)
                return Native.ExpressionExtensions.Parse(@const.Value.ToString());
            throw new InvalidOperationException();
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
                return Negate(unary.Operand, getArgs);
            }
            if(expression.NodeType == ExpressionType.Add) {
                var binary = expression as BinaryExpression;
                return new AddExpr(ImmutableArray.Create(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs)));
            }
            if(expression.NodeType == ExpressionType.Subtract) {
                var binary = expression as BinaryExpression;
                return new AddExpr(ImmutableArray.Create(BuildCore(binary.Left, getArgs), Negate(binary.Right, getArgs)));
            }
            if(expression.NodeType == ExpressionType.Multiply) {
                var binary = expression as BinaryExpression;
                return new MultExpr(ImmutableArray.Create(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs)));
            }
            if(expression.NodeType == ExpressionType.Divide) {
                var binary = expression as BinaryExpression;
                return new DivExpr(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.ExclusiveOr) {
                var binary = expression as BinaryExpression;
                return new PowerExpr(BuildCore(binary.Left, getArgs), GetConst(binary.Right));
            }
            throw new InvalidExpressionException();
        }
        static Expr Negate(Expression expression, Func<ParameterExpression, Expr> getArgs) {
            return new MultExpr(ImmutableArray.Create(-1, (BuildCore(expression, getArgs))));
        }
        static int GetConst(Expression expression) {
            var unary = expression as UnaryExpression;
            var constant = unary.Operand as ConstantExpression;
            return (int)constant.Value;
        }
    }
    public class CantImplicitlyCreateExpressionException : ApplicationException { }
    public class PowerShouldBePositiveException : ApplicationException { }
    public class InvalidExpressionException : ApplicationException { }
}