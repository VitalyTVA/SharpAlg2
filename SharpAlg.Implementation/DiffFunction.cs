using SharpAlg.Native.Builder;
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Implementation)]
    public class DiffFunction : Function, ISupportConvolution {
        public DiffFunction()
            : base("diff") {
        }
        public override Number Evaluate(IExpressionEvaluator evaluator, IEnumerable<Expr> args) {
            return Convolute(evaluator.Context, args).Visit(evaluator);
        }
        public Expr Convolute(IContext context, IEnumerable<Expr> args) {
            var argsTail = args.Tail();
            if(!argsTail.All(x => x is ParameterExpr))
                throw new ExpressionDefferentiationException("All diff arguments should be parameters");//TODO correct message, go to constant
            var diffList = argsTail.Cast<ParameterExpr>();
            var builder = new ConvolutionExprBuilder(context);
            if(!diffList.Any()) {
                return args.First().Diff(builder);
            }
            Expr result = args.First();
            diffList.ForEach(x => result = result.Diff(builder, x.ParameterName));
            return result;
        }
    }
}
