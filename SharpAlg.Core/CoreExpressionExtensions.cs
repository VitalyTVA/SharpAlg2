using SharpAlg.Native.Builder;
using SharpKit.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    [JsType(JsMode.Clr, Filename = SR.JS_Core)]
    public static class CoreExpressionExtensions {
        public static Expr Tail(this MultiplyExpr multi) {
            return Expr.Multiply(multi.Args.Tail());
        }
        public static Expr Tail(this AddExpr multi) {
            return Expr.Add(multi.Args.Tail());
        }
    }
}
