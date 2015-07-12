using SharpAlg.Native;
using SharpAlg.Native.Builder;
using System;
using System.Collections.Immutable;
using System.Linq;
using RealPoint = System.Windows.Point;

namespace SharpAlg.Geo {
    public class ImmutableContext : IContext {
        public static readonly ImmutableContext Empty = new ImmutableContext(ImmutableDictionary<string, Expr>.Empty);
        ImmutableDictionary<string, Expr> names;
        ImmutableContext(ImmutableDictionary<string, Expr> names) {
            this.names = names;
        }
        Function IContext.GetFunction(string name) {
            return null;
        }
        Expr IContext.GetValue(string name) {
            return names.TryGetValue(name);
        }
        public double GetValue(string name) {
            return ((IContext)this).GetValue(name).Evaluate(this).ToDouble();
        }
        public ImmutableContext Register(string name, Expr value) {
            return new ImmutableContext(names.Add(name, value));
        }
    }
}
