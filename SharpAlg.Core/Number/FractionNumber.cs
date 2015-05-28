
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SharpAlg.Native.Numbers {
    internal sealed class FractionNumber : Number {
        readonly LongIntegerNumber denominator;
        readonly LongIntegerNumber numerator;
        static LongIntegerNumber GCD(LongIntegerNumber a, LongIntegerNumber b) {
            LongIntegerNumber c;
            while(b > NumberFactory.Zero) {
                c = a.Modulo(b);
                a = b;
                b = c;
            }
            return a;
        }
        public static Number Create(LongIntegerNumber numerator, LongIntegerNumber denominator) {
            var gcd = GCD(numerator, denominator);
            LongIntegerNumber numerator_ = (LongIntegerNumber)numerator.IntDivide(gcd);
            LongIntegerNumber denominator_ = (LongIntegerNumber)denominator.IntDivide(gcd);
            return denominator_ == NumberFactory.One ? (Number)numerator_ : new FractionNumber(numerator_, denominator_);
        }
        internal FractionNumber(LongIntegerNumber numerator, LongIntegerNumber denominator) {
            if(denominator < NumberFactory.Zero)
                throw new ArgumentException("denominator");
            this.denominator = denominator;
            this.numerator = numerator;
        }
        public override string ToString() {
            return numerator.ToString() + "/" + denominator.ToString();
        }
        protected override Number ConvertToCore(int type) {
            return numerator.ToFloat() / denominator.ToFloat();
        }

        protected override int NumberType {
            get { return FractionNumberType; }
        }

        protected override Number Add(Number n) {
            return BinaryOperation(n, (x, y) => {
                LongIntegerNumber numerator = (x.numerator * y.denominator + x.denominator * y.numerator).ConvertCast<LongIntegerNumber>();
                LongIntegerNumber denominator = (x.denominator * y.denominator).ConvertCast<LongIntegerNumber>();
                return Create(numerator, denominator);
            });
        }

        protected override Number Subtract(Number n) {
            var other = n.ConvertCast<FractionNumber>();
            return this + new FractionNumber((NumberFactory.Zero - other.numerator).ConvertCast<LongIntegerNumber>(), other.denominator.ConvertCast<LongIntegerNumber>());
        }

        protected override Number Multiply(Number n) {
            return BinaryOperation(n, (x, y) => {
                LongIntegerNumber numerator = (x.numerator * y.numerator).ConvertCast<LongIntegerNumber>();
                LongIntegerNumber denominator = (x.denominator * y.denominator).ConvertCast<LongIntegerNumber>();
                return Create(numerator, denominator);
            });
        }
        protected override Number Divide(Number n) {
            return BinaryOperation(n, (x, y) => {
                LongIntegerNumber numerator = (x.numerator * y.denominator).ConvertCast<LongIntegerNumber>();
                LongIntegerNumber denominator = (x.denominator * y.numerator).ConvertCast<LongIntegerNumber>();
                if(denominator < NumberFactory.Zero) {
                    denominator = (NumberFactory.Zero - denominator).ConvertCast<LongIntegerNumber>();
                    numerator = (NumberFactory.Zero - numerator).ConvertCast<LongIntegerNumber>();
                }
                return Create(numerator, denominator);
            });
        }

        protected override Number Power(Number n) {
            if(numerator == LongIntegerNumber.One && denominator == LongIntegerNumber.One)
                return NumberFactory.One;

            var other = n.ConvertCast<FractionNumber>();
            if(other.denominator == LongIntegerNumber.One)
                return LongIntegerNumber.FastPower(this, other.numerator);

            return ToFloat() ^ n.ToFloat();
        }

        protected override int Compare(Number n) {
            return BinaryOperation(n, (x, y) => Compare(x.numerator * y.denominator, x.denominator * y.numerator));
        }
        T BinaryOperation<T>(Number n, Func<FractionNumber, FractionNumber, T> operation) {
            return operation(this, n.ConvertCast<FractionNumber>());
        }
        Number BinaryOperation(Number n, Func<FractionNumber, FractionNumber, Number> operation) {
            return BinaryOperation<Number>(n, operation);
        }
    }
}
