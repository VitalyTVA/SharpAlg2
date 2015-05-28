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
    public class FactorialFunction : SingleArgumentFunction, ISupportConvolution {
        public FactorialFunction()
            : base(FunctionFactory.FactorialName) {
        }
        protected override Number Evaluate(Number arg) {
            Number result = NumberFactory.One;
            for(Number i = NumberFactory.Two; i <= arg; i = i + NumberFactory.One) {
                result = result * i;
            }
            return result;
        }
        //TODO factorial differentiation

        public Expr Convolute(IContext context, IEnumerable<Expr> args) {
            return args
                .First()
                .ConvertAs<ConstantExpr>()
                .If(x => x.Value.IsInteger)
                .Return(x => Expr.Constant(Evaluate(x.Value)), () => null);
        }
    }
}
