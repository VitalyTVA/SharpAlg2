using SharpKit.JavaScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpAlg.Native {
    [JsType(JsMode.Clr, Filename = SR.JS_Core_Utils)]
    public class Tuple<T1, T2> {
        readonly T1 item1;
        readonly T2 item2;
        public T1 Item1 {
            get { return this.item1; }
        }
        public T2 Item2 {
            get { return this.item2; }
        }

        public Tuple(T1 item1, T2 item2) {
            this.item1 = item1;
            this.item2 = item2;
        }

        public override bool Equals(object obj) {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        public override int GetHashCode() {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        bool Equals(object other, IEqualityComparer comparer) {
            if(other == null) {
                return false;
            }
            Tuple<T1, T2> tuple = other as Tuple<T1, T2>;
            if(tuple == null) {
                return false;
            }
            return (comparer.Equals(this.item1, tuple.item1) && comparer.Equals(this.item2, tuple.item2));
        }

        int GetHashCode(IEqualityComparer comparer) {
            return comparer.GetHashCode(this.item1) ^ comparer.GetHashCode(this.item2);
        }


        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.item1);
            sb.Append(", ");
            sb.Append(this.item2);
            sb.Append(")");
            return sb.ToString();
        }
    }
}
