//
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JSNativeName)]
    public static class ContextFactory {
        public static Context CreateEmpty() {
            return new Context();
        }
        public static Context CreateDefault() {
            return new Context()
                .Register(Functions.Factorial)
                .Register(Functions.Ln)
                .Register(Functions.Diff)
                .Register(Functions.Exp)
                .Register(Functions.Pi)
                .Register(Functions.Sin)
                .Register(Functions.Cos)
                ;
        }
        public static readonly Context Empty;
        public static readonly Context Default;
        static ContextFactory() {
            Default = CreateDefault();
            Default.ReadOnly = true;

            Empty = CreateEmpty();
            Empty.ReadOnly = true;
        }
    }
}
