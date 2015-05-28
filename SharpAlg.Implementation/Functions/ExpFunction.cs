using SharpAlg.Native.Builder;
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Implementation_Functions)]
    public class ExpFunction : SingleArgumentDifferentiableFunction {
        public ExpFunction()
            : base(FunctionFactory.ExpName) {
        }
        protected override Number Evaluate(Number arg) {
            return NumberFactory.GetFloat(arg, x => Math.Exp(x));
        }
        protected override Expr DiffCore(ExprBuilder builder, Expr arg) {
            return FunctionFactory.Exp(arg);
        }
        static Expr MultiplyConvoultion(IContext context, Expr arg) {
            return arg
                .With(x => MultiplyExpressionExtractor.ExtractMultiply(x))
                .ConvertAs<MultiplyExpr>()
                .With(x => {
                    FunctionExpr lnExpr = x.Args
                        .Where(y => y is FunctionExpr)
                        .Cast<FunctionExpr>()
                        .FirstOrDefault(y => context.GetFunction(y.FunctionName) is LnFunction);
                    if(lnExpr != null) {
                        return new ConvolutionExprBuilder(context).Power(lnExpr.Args.First(), Expr.Multiply(x.Args.Where(y => y != lnExpr)));
                    }
                    return null;
                });
        }
        protected override Expr SpecificConvolution(IContext context, Expr arg) {
            return MultiplyConvoultion(context, arg);
        }
        protected override Number ConvertConstant(Number n) {
            return n == NumberFactory.Zero ? NumberFactory.One : null;
        }

    }

}
