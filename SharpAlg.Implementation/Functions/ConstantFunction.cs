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
    public abstract class ConstantFunction : Function, ISupportDiff, IConstantFunction {
        public ConstantFunction(string name)
            : base(name) {
        }
        public sealed override Number Evaluate(IExpressionEvaluator evaluator, IEnumerable<Expr> args) {
            return Value;
        }

        public Expr Diff(IDiffExpressionVisitor diffVisitor, IEnumerable<Expr> args) {
            return Expr.Zero;
        }

        protected abstract Number Value { get; }
    }
}
