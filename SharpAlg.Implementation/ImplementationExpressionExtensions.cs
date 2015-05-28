using SharpAlg.Native.Builder;
using SharpKit.JavaScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    [JsType(JsMode.Clr, Filename = SR.JS_Implementation)]
    public static class ImplementationExpressionExtensions {
        public static bool ExprEquals(this Expr expr1, Expr expr2) {
            return expr1.Visit(new ExpressionEqualityComparer(expr2));
        }
        public static bool ExprEquivalent(this Expr expr1, Expr expr2) {
            return expr1.Visit(new ExpressionEquivalenceComparer(expr2));
        }
        public static Expr Diff(this Expr expr, ExprBuilder builder, string parameterName = null) {
            return expr.Visit(new DiffExpressionVisitor(builder, parameterName));
        }
    }
}
