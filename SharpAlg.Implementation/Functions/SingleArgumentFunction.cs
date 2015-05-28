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
    public abstract class SingleArgumentFunction : Function, ISupportCheckArgs {
        static bool IsValidArgsCount<T>(IEnumerable<T> args) {
            return args != null && args.Count() == 1;
        }
        public override Number Evaluate(IExpressionEvaluator evaluator, IEnumerable<Expr> args) {
            return EvaluateCore(args.Select(x => x.Visit(evaluator)));
        }
        protected static void CheckArgsCount<T>(IEnumerable<T> args) {
            if(!IsValidArgsCount<T>(args))
                throw new InvalidArgumentCountException(); //TODO message
        }
        protected SingleArgumentFunction(string name)
            : base(name) {
        }
        Number EvaluateCore(IEnumerable<Number> args) {
            CheckArgsCount(args);
            return Evaluate(args.Single());
        }
        protected abstract Number Evaluate(Number arg);

        public string Check(IEnumerable<Expr> args) {
            return IsValidArgsCount(args) ? string.Empty : string.Format("Error, (in {0}) expecting 1 argument, got {1}", Name, (args != null ? args.Count() : 0));
        }
    }
}
