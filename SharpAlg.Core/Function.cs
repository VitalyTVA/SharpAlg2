using SharpAlg.Native.Builder;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public abstract class Function {
        protected Function(string name) {
            Name = name;
        }
        public abstract Number Evaluate(IExpressionEvaluator evaluator, IEnumerable<Expr> args);
        
        public string Name { get; private set; }
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public interface IDiffExpressionVisitor : IExpressionVisitor<Expr> {
        ExprBuilder Builder { get; }
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public interface ISupportDiff {
        Expr Diff(IDiffExpressionVisitor diffVisitor, IEnumerable<Expr> args);
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public interface ISupportCheckArgs {
        string Check(IEnumerable<Expr> args);
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public interface IConstantFunction {
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public interface ISupportConvolution {
        Expr Convolute(IContext context, IEnumerable<Expr> args);
    }
    ////(JsMode.Clr, Filename = SR.JS_Core)]
    //public interface ISupportCustomPrinting {
    //    Expr GetPrintableExpression(IContext context, IEnumerable<Expr> args);
    //}
}
