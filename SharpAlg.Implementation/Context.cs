using SharpKit.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpAlg.Native {
    [JsType(JsMode.Clr, Filename = SR.JS_Implementation)]
    public class Context : IContext {
        Dictionary<string, Expr> names = new Dictionary<string, Expr>();
        Dictionary<string, Function> functions = new Dictionary<string, Function>();

        public bool ReadOnly {
            get; set;
        }
        public Context() { }

        public Context Register(Function func) {
            CheckReadonly();
            functions[func.Name] = func;
            return this;
        }
        public Function GetFunction(string name) {
            return functions.TryGetValue(name); ;
        }

        public Context Register(string name, Expr value) {
            CheckReadonly();
            names[name] = value;
            return this;
        }
        public Expr GetValue(string name) {
            return names.TryGetValue(name);
        }
        void CheckReadonly() {
            if(ReadOnly)
                throw new InvalidOperationException(); //TODO correct exception and text
        }
    }
}
