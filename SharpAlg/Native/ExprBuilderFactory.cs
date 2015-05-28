//
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpAlg.Native.Builder {
    //(JsMode.Prototype, Filename = SR.JSNativeName)]
    public class ExprBuilderFactory {
        public static ExprBuilder CreateDefault() {
            return new ConvolutionExprBuilder(ContextFactory.Default);
        }
        public static ExprBuilder CreateEmpty() {
            return new ConvolutionExprBuilder(ContextFactory.Empty);
        }
        public static ExprBuilder Create(IContext context) {
            return new ConvolutionExprBuilder(context);
        }
    }
}
