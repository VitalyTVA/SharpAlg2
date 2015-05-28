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
    public class InvalidArgumentCountException : Exception {
        public InvalidArgumentCountException() {

        }
        public InvalidArgumentCountException(string message)
            : base(message) {

        }
    }
}
