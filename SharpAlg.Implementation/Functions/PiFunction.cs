using SharpAlg.Native.Builder;
using SharpKit.JavaScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    [JsType(JsMode.Clr, Filename = SR.JS_Implementation_Functions)]
    public class PiFunction : ConstantFunction {
        public PiFunction()
            : base(FunctionFactory.PiName) {
        }
        protected override Number Value { get { return NumberFactory.Pi; } }
    }
}
