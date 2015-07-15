using SharpAlg.Native;
using SharpAlg.Native.Builder;
using System;
using System.Collections.Immutable;
using System.Linq;
using RealPoint = System.Windows.Point;

namespace SharpAlg.Geo {
    public class ImmutableContext {
        public static readonly ImmutableContext Empty = new ImmutableContext(ImmutableDictionary<string, double>.Empty);
        ImmutableDictionary<string, double> values;
        ImmutableContext(ImmutableDictionary<string, double> names) {
            this.values = names;
        }
        public double GetValue(string name) {
            return values[name];
        }
        public ImmutableContext Register(string name, double value) {
            return new ImmutableContext(values.Add(name, value));
        }
    }
}
