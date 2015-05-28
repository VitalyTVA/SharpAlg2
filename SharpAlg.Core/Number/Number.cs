
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Core_Number)]
    public abstract class Number {
        protected const int IntegerNumberType = 0;
        protected const int FractionNumberType = 1;
        protected const int FloatNumberType = 2;
        static void ToSameType(ref Number n1, ref Number n2) {
            var type = Math.Max(n1.NumberType, n2.NumberType);
            n1 = n1.ConvertTo(type);
            n2 = n2.ConvertTo(type);
        }
        public static bool operator ==(Number n1, Number n2) {
            if((object)n1 != null && (object)n2 != null)
                ToSameType(ref n1, ref n2);
            return object.Equals(n1, n2);
        }
        public static bool operator !=(Number n1, Number n2) {
            return !(n1 == n2);
        }
        public static bool operator >=(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return !n1.Less(n2);
        }
        public static bool operator <=(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return !n1.Greater(n2);
        }
        public static bool operator <(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return n1.Less(n2);
        }
        public static bool operator >(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return n1.Greater(n2);
        }
        public static int Compare(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return n1.Compare(n2);
        }
        public static Number operator *(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return n1.Multiply(n2);
        }
        public static Number operator /(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return n1.Divide(n2);
        }
        public static Number operator +(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return n1.Add(n2);
        }
        public static Number operator -(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return n1.Subtract(n2);
        }
        public static Number operator ^(Number n1, Number n2) {
            ToSameType(ref n1, ref n2);
            return n1.Power(n2);
        }

        protected Number() {
        }
        internal Number ToFloat() {
            return ConvertTo(FloatNumberType);
        }
        protected Number ConvertTo(int type) {
            if(type < NumberType)
                throw new NotImplementedException();
            if(type == NumberType)
                return this;
            return ConvertToCore(type);
        }

        protected abstract Number ConvertToCore(int type);
        protected abstract int NumberType { get; }
        protected abstract Number Add(Number n);
        protected abstract Number Subtract(Number n);
        protected abstract Number Multiply(Number n);
        protected abstract Number Divide(Number n);
        protected abstract Number Power(Number n);
        protected abstract int Compare(Number n);

        bool Less(Number n) {
            return Compare(n) < 0;
        }
        bool Greater(Number n) {
            return Compare(n) > 0;
        }
        public sealed override bool Equals(object obj) {
            var this_ = this;
            var other = obj as Number;
            ToSameType(ref this_, ref other);
            return this_.Compare(other) == 0;
        }
        public sealed override int GetHashCode() {
            throw new NotSupportedException();
        }
        public bool IsInteger { get { return NumberType == IntegerNumberType; } }
        public bool IsFraction { get { return NumberType == FractionNumberType; } }
        public bool IsFloat { get { return NumberType == FloatNumberType; } }
    }
}