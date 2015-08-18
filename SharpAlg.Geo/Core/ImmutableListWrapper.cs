using System.Collections;
using System.Collections.Generic;

namespace SharpAlg.Geo.Core {
    public partial struct ImmutableListWrapper<T> : IEnumerable<T> {
        public static bool operator !=(ImmutableListWrapper<T> left, ImmutableListWrapper<T> right) {
            return !(left == right);
        }
        public static bool operator ==(ImmutableListWrapper<T> left, ImmutableListWrapper<T> right) {
            return Equals(left.list, right.list);
        }
        readonly IList<T> list;
        public ImmutableListWrapper(IList<T> list) {
            this.list = list;
        }

        public override bool Equals(object obj) {
            if(!(obj is ImmutableListWrapper<T>))
                return false;
            return this == (ImmutableListWrapper<T>)obj;
        }

        public T this[int index] { get { return list[index]; } }
        public int Length { get { return list.Count; } }

        public override int GetHashCode() {
            return list.GetHashCode();

        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return list.GetEnumerator();
        }
    }
}
