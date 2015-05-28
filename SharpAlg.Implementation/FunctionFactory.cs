using SharpKit.JavaScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpAlg.Native {
    [JsType(JsMode.Clr, Filename = SR.JS_Implementation)]
    public static class FunctionFactory {
        public const string FactorialName = "factorial";
        public const string LnName = "ln";
        public const string ExpName = "exp";
        public const string PiName = "Pi";
        public const string SinName = "sin";
        public const string CosName = "cos";
        public static FunctionExpr Factorial(Expr argument) {
            return Expr.Function(FactorialName, argument);
        }
        public static FunctionExpr Ln(Expr argument) {
            return Expr.Function(LnName, argument);
        }
        public static FunctionExpr Exp(Expr argument) {
            return Expr.Function(ExpName, argument);
        }
    }
}