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
    public class LnFunction : SingleArgumentDifferentiableFunction {
        public LnFunction()
            : base(FunctionFactory.LnName) {
        }
        protected override Number Evaluate(Number arg) {
            return NumberFactory.GetFloat(arg, x => Math.Log(x));
        }
        protected override Expr DiffCore(ExprBuilder builder, Expr arg) {
            return builder.Inverse(arg);
        }

        static Expr PowerConvolution(IContext context, Expr arg) {
            return arg
                .ConvertAs<PowerExpr>()
                .Return(x => Expr.Multiply(x.Right, FunctionFactory.Ln(x.Left)), () => null);
        }
        static Expr InverseFunctionConvolution(IContext context, Expr arg) {
            return arg
                .ConvertAs<FunctionExpr>()
                .If(x => context.GetFunction(x.FunctionName) is ExpFunction)
                .Return(x => x.Args.First(), () => null);
        }
        protected override Expr SpecificConvolution(IContext context, Expr arg) {
            return PowerConvolution(context, arg) ?? InverseFunctionConvolution(context, arg);
        }
        protected override Number ConvertConstant(Number n) {
            return n == NumberFactory.One ? NumberFactory.Zero : null;
        }
    }
}
