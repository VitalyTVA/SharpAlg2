
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpAlg.Native.Builder {
    //(JsMode.Prototype, Filename = SR.JS_Core)]
    public abstract class ExprBuilder {
        public abstract IContext Context { get; }
        public abstract Expr Parameter(string parameterName);
        public abstract Expr Power(Expr left, Expr right);
        public abstract Expr Add(Expr left, Expr right);
        public abstract Expr Multiply(Expr left, Expr right);
        public abstract Expr Function(string functionName, IEnumerable<Expr> arguments);
        public Expr Subtract(Expr left, Expr right) {
            return Add(left, Minus(right));
        }
        public Expr Divide(Expr left, Expr right) {
            return Multiply(left, Inverse(right));
        }
        public Expr Minus(Expr expr) {
            return Multiply(Expr.MinusOne, expr);
        }
        public Expr Inverse(Expr expr) {
            return Power(expr, Expr.MinusOne);
        }
    }
}