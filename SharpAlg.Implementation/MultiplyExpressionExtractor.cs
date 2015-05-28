using SharpAlg.Native.Builder;
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    //(JsMode.Prototype, Filename = SR.JS_Implementation)]
    class MultiplyExpressionExtractor : DefaultExpressionVisitor<Expr> {
        static MultiplyExpressionExtractor instance;
        static MultiplyExpressionExtractor Instance { get { return instance ?? (instance = new MultiplyExpressionExtractor()); } }
        public static Expr ExtractMultiply(Expr expr) {
            return expr.Visit(Instance);
        }
        MultiplyExpressionExtractor() { }
        public override Expr Multiply(MultiplyExpr multi) {
            return multi;
        }
        protected override Expr GetDefault(Expr expr) {
            return Expr.Multiply(Expr.One, expr);
        }
    }
}
