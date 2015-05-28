
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SharpAlg.Native.Numbers {
    internal sealed class FloatNumber : Number {
        internal readonly double value;
        public FloatNumber(double value) {
            this.value = value;
        }
        protected override int NumberType { get { return FloatNumberType; } }
        protected override Number ConvertToCore(int type) {
            throw new NotImplementedException();
        }
        public override string ToString() {
            return PlatformHelper.ToInvariantString(value);
        }
        protected override Number Add(Number n) {
            return BinaryOperation(n, (x, y) => x + y);
        }
        protected override Number Subtract(Number n) {
            return BinaryOperation(n, (x, y) => x - y);
        }
        protected override Number Multiply(Number n) {
            return BinaryOperation(n, (x, y) => x * y);
        }
        protected override Number Divide(Number n) {
            return BinaryOperation(n, (x, y) => x / y);
        }
        protected override Number Power(Number n) {
            return BinaryOperation(n, (x, y) => Math.Pow(x, y));
        }
        protected override int Compare(Number n) {
            return BinaryOperation<int>(n, (x, y) => Math.Sign(x - y));
        }
        T BinaryOperation<T>(Number n, Func<double, double, T> operation) {
            return operation(value, n.ConvertCast<FloatNumber>().value);
        }
        Number BinaryOperation(Number n, Func<double, double, double> operation) {
            return new FloatNumber(BinaryOperation<double>(n, operation));
        }
    }
}
