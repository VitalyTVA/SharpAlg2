using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo.Core {
    public class Builder {
        public Expr Build(Expression<Func<Expr>> f)
            => BuildExpr(f);
        public Expr Build(Expression<Func<Expr, Expr>> f, Expr x1)
            => BuildExpr(f, x1);
        public Expr Build(Expression<Func<Expr, Expr, Expr>> f, Expr x1, Expr x2)
            => BuildExpr(f, x1, x2);
        public Expr Build(Expression<Func<Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3)
            => BuildExpr(f, x1, x2, x3);
        public Expr Build(Expression<Func<Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4)
            => BuildExpr(f, x1, x2, x3, x4);
        public Expr Build(Expression<Func<Expr, Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4, Expr x5)
            => BuildExpr(f, x1, x2, x3, x4, x5);
        public Expr Build(Expression<Func<Expr, Expr, Expr, Expr, Expr, Expr, Expr>> f, Expr x1, Expr x2, Expr x3, Expr x4, Expr x5, Expr x6)
            => BuildExpr(f, x1, x2, x3, x4, x5, x6);

        Expr BuildExpr(LambdaExpression expression, params Expr[] args) {
            if(expression.Parameters.Count != args.Length) {
                throw new InvalidOperationException();
            }
            var argsDict = expression.Parameters.Select((x, i) => new { Parameter = x, Expr = args[i] }).ToImmutableDictionary(x => x.Parameter, x => x.Expr);
            return BuildCore(expression.Body, x => argsDict[x]);
        }
        static readonly Expression<Func<Expr, Expr>> SqrtExpr = x => Sqrt(x);

        int count = 0;

        Expr BuildCore(Expression expression, Func<ParameterExpression, Expr> getArgs) {
            count++;
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
            var binary = expression as BinaryExpression;
            if(expression.NodeType == ExpressionType.Add) {
                return Add(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Subtract) {
                return Subtract(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Multiply) {
                return Multiply(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.Divide) {
                return Divide(BuildCore(binary.Left, getArgs), BuildCore(binary.Right, getArgs));
            }
            if(expression.NodeType == ExpressionType.ExclusiveOr) {
                return Power(BuildCore(binary.Left, getArgs), GetConst(binary.Right));
            }
            throw new InvalidExpressionException();
        }
        static int GetConst(Expression expression) {
            var unary = expression as UnaryExpression;
            var constant = unary.Operand as ConstantExpression;
            return (int)constant.Value;
        }

        public Expr Add(params Expr[] args) {
            return new AddExpr(this, ImmutableArray.Create(args));
        }
        public Expr Subtract(Expr a, Expr b) {
            return Add(a, Minus(b));
        }
    }
}
