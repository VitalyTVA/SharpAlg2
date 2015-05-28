using SharpAlg.Native.Builder;
using SharpKit.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    [JsType(JsMode.Prototype, Filename = SR.JS_Implementation)]
    public class DiffExpressionVisitor : IDiffExpressionVisitor {
        string parameterName;
        bool autoParameterName;
        IContext Context { get { return builder.Context; } }
        private bool HasParameter { get { return !string.IsNullOrEmpty(parameterName); } }
        readonly ExprBuilder builder;
        public ExprBuilder Builder {
            get {
                return builder;
            }
        }
        public DiffExpressionVisitor(ExprBuilder builder, string parameterName) {
            this.builder = builder;
            this.parameterName = parameterName;
            autoParameterName = !HasParameter;
        }
        public Expr Constant(ConstantExpr constant) {
            return Expr.Zero;
        }
        public Expr Parameter(ParameterExpr parameter) {
            if(!HasParameter) {
                parameterName = parameter.ParameterName;
                autoParameterName = true;
            }
            if(parameterName == parameter.ParameterName) {
                return Expr.One;
            } else {
                if(autoParameterName)
                    throw new ExpressionDefferentiationException("Expression contains more than one independent variable");
                return Expr.Zero;
            }
        }
        public Expr Add(AddExpr multi) {
            Expr result = null;
            multi.Args.Accumulate(x => {
                result = x.Visit(this);
            }, x => {
                result = Builder.Add(result, x.Visit(this));
            });
            return result;
        }
        public Expr Multiply(MultiplyExpr multi) {
            var tail = multi.Tail();
            var expr1 = Builder.Multiply(multi.Args.First().Visit(this), tail);
            var expr2 = Builder.Multiply(multi.Args.First(), tail.Visit(this));
            return Builder.Add(expr1, expr2);
        }
        public Expr Power(PowerExpr power) {
            Expr sum1 = Builder.Multiply(power.Right.Visit(this), FunctionFactory.Ln(power.Left));
            Expr sum2 = Builder.Divide(Builder.Multiply(power.Right, power.Left.Visit(this)), power.Left);
            Expr sum = Builder.Add(sum1, sum2);
            return Builder.Multiply(power, sum);
        }
        public Expr Function(FunctionExpr functionExpr) {
            return Context.GetFunction(functionExpr.FunctionName)
                .ConvertAs<ISupportDiff>().Return(
                x => x.Diff(this, functionExpr.Args),
                () => { throw new InvalidOperationException(); }); //TODO exception and message
        }
    }
    [JsType(JsMode.Clr, Filename = SR.JS_Implementation)]
    public class ExpressionDefferentiationException : Exception {
        public ExpressionDefferentiationException()
            : base() {
        }
        public ExpressionDefferentiationException(string message)
            : base(message) {
        }
    }

}
