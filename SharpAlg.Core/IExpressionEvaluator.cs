
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public interface IExpressionEvaluator : IExpressionVisitor<Number> {
        IContext Context { get; }
    }
}
