using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo.Core {
    public static class BuilderExtensions {
        public static Expr Build(this Builder builder, Expression<Func<Expr>> f)
            => builder.BuildExpr(f);
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr>> f, Expr x1)
            => builder.BuildExpr(f, x1);
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr>> f, Expr x1, Expr x2)
            => builder.BuildExpr(f, x1, x2);
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3)
            => builder.BuildExpr(f, x1, x2, x3);
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4)
            => builder.BuildExpr(f, x1, x2, x3, x4);
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4, Expr x5)
            => builder.BuildExpr(f, x1, x2, x3, x4, x5);
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4, Expr x5, Expr x6)
            => builder.BuildExpr(f, x1, x2, x3, x4, x5, x6);

        static Expr BuildExpr(this Builder builder, LambdaExpression expression, params Expr[] args) {
            if(expression.Parameters.Count != args.Length) {
                throw new InvalidOperationException();
            }
            var argsDict = expression.Parameters.Select((x, i) => new { Parameter = x, Expr = args[i] }).ToImmutableDictionary(x => x.Parameter, x => x.Expr);
            return builder.BuildCore(expression.Body, x => argsDict[x]);
        }
        static readonly Expression<Func<Expr, Expr>> SqrtExpression = x => Sqrt(x);


        static Expr BuildCore(this Builder builder, Expression expression, Func<ParameterExpression, Expr> getArgs) {
            if(expression.NodeType == ExpressionType.Parameter) {
                return getArgs((ParameterExpression)expression);
            }
            if(expression.NodeType == ExpressionType.Convert) {
                return Const(GetConst(expression));
            }
            if(expression.NodeType == ExpressionType.Call) {
                var call = expression as MethodCallExpression;
                if(call != null && call.Method == ((MethodCallExpression)SqrtExpression.Body).Method) {
                    return builder.Sqrt(builder.BuildCore(call.Arguments.Single(), getArgs));
                }
            }
            if(expression.NodeType == ExpressionType.Negate) {
                var unary = expression as UnaryExpression;
                return builder.Minus(builder.BuildCore(unary.Operand, getArgs));
            }
            var binary = expression as BinaryExpression;
            if(expression.NodeType == ExpressionType.Add) {
                return builder.Add(builder.BuildCore(binary.Left, getArgs), builder.BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Subtract) {
                return builder.Subtract(builder.BuildCore(binary.Left, getArgs), builder.BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Multiply) {
                return builder.Multiply(builder.BuildCore(binary.Left, getArgs), builder.BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Divide) {
                return builder.Divide(builder.BuildCore(binary.Left, getArgs), builder.BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.ExclusiveOr) {
                return builder.Power(builder.BuildCore(binary.Left, getArgs), GetConst(binary.Right));
            }
            throw new InvalidExpressionException();
        }
        static int GetConst(Expression expression) {
            var unary = expression as UnaryExpression;
            var constant = unary.Operand as ConstantExpression;
            return (int)constant.Value;
        }

        public static Expr Subtract(this Builder builder, Expr a, Expr b) {
            return builder.Add(a, builder.Minus(b));
        }
        public static Expr Minus(this Builder builder, Expr a) {
            return builder.Multiply(-1, a);
        }

    }
}
