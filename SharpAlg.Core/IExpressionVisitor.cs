
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public interface IExpressionVisitor<T> {
        T Constant(ConstantExpr constant);
        T Parameter(ParameterExpr parameter);
        T Add(AddExpr multi);
        T Multiply(MultiplyExpr multi);
        T Power(PowerExpr power);
        T Function(FunctionExpr functionExpr);
    }
}