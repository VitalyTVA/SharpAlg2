//
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpAlg.Native.Builder {
    //(JsMode.Prototype, Filename = SR.JS_Implementation_Builder)]
    public class TrivialExprBuilder : ExprBuilder {
        readonly IContext context;
        public override IContext Context { get { return context; } }
        public TrivialExprBuilder(IContext context) {
            this.context = context;
        }
        public override Expr Parameter(string parameterName) {
            return Expr.Parameter(parameterName);
        }
        public override Expr Add(Expr left, Expr right) {
            return Expr.Add(left, right);
        }
        public override Expr Multiply(Expr left, Expr right) {
            return Expr.Multiply(left, right);
        }
        public override Expr Power(Expr left, Expr right) {
            return Expr.Power(left, right);
        }
        public override Expr Function(string functionName, IEnumerable<Expr> arguments) {
            return Expr.Function(functionName, arguments);
        }
    }
}
