using SharpAlg.Native.Builder;
using SharpKit.JavaScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {

    [JsType(JsMode.Clr, Filename = SR.JSNativeName)]
    public static class Functions {
        static Function factorial;
        public static Function Factorial { get { return factorial ?? (factorial = new FactorialFunction()); } }

        static Function ln;
        public static Function Ln { get { return ln ?? (ln = new LnFunction()); } }

        static Function diff;
        public static Function Diff { get { return diff ?? (diff = new DiffFunction()); } }

        static Function exp;
        public static Function Exp { get { return exp ?? (exp = new ExpFunction()); } }

        static Function pi;
        public static Function Pi { get { return pi ?? (pi = new PiFunction()); } }

        static Function sin;
        public static Function Sin { get { return sin ?? (sin = new SinFunction()); } }

        static Function cos;
        public static Function Cos { get { return cos ?? (cos = new CosFunction()); } }
    }
}
