
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SharpAlg.Native.Numbers {
    public sealed class LongIntegerNumber : Number {
        internal static readonly LongIntegerNumber Zero = new LongIntegerNumber(new int[0], false);
        internal static readonly LongIntegerNumber One = new LongIntegerNumber(new int[] { 1 }, false);
        internal static readonly LongIntegerNumber Two = new LongIntegerNumber(new int[] { 2 }, false);
        internal static readonly LongIntegerNumber MinusOne = new LongIntegerNumber(new int[] { 1 }, true);
        internal static Number FromLongIntStringCore(string s) {
            IList<int> digits = new List<int>(); //TODO - set capacity
            int currentPart = 0;
            int currentIndex = 0;
            int currentPower = 1;
            int ZeroCode = PlatformHelper.CharToInt('0');
            int lastIndex = s[0] == '-' ? 1 : 0;
            for(int i = s.Length - 1; i >= lastIndex; i--) {
                currentPart += (PlatformHelper.CharToInt(s[i]) - ZeroCode) * currentPower;
                currentIndex++;
                currentPower *= LongIntegerNumber.Base;
                if(currentIndex == LongIntegerNumber.BaseCount) {
                    currentIndex = 0;
                    digits.Add(currentPart);
                    currentPart = 0;
                    currentPower = 1;
                }
            }
            if(currentIndex > 0 && currentPart > 0) {
                digits.Add(currentPart);
            }
            return new LongIntegerNumber(digits, lastIndex == 1);
        }

        internal const int Base = 10;
        internal const int BaseCount = 4;
        internal const int BaseFull = 10000;
        readonly bool isNegative;
        readonly IList<int> parts;
        LongIntegerNumber(IList<int> parts, bool isNegative) {
            this.isNegative = isNegative;
            this.parts = parts;
        }
        protected override int NumberType { get { return IntegerNumberType; } }
        protected override Number ConvertToCore(int type) {
            if(type == Number.FloatNumberType) {
                double result = 0;
                int count = parts.Count;
                for(int i = count - 1; i >= 0; i--) {
                    result = result * BaseFull + parts[i];
                }
                return new FloatNumber(isNegative ? -result : result);
            }
            if(type == Number.FractionNumberType) {
                return new FractionNumber(this, One);
            }
            throw new NotImplementedException();
        }
        protected override int Compare(Number n) {
            var other = (LongIntegerNumber)n;
            return Compare(this, other);
        }
        static int Compare(LongIntegerNumber n1, LongIntegerNumber n2) {
            if(!n1.parts.Any() && !n2.parts.Any())
                return 0;
            if(!n2.parts.Any())
                return n1.isNegative ? -1 : 1;
            if(!n1.parts.Any())
                return n2.isNegative ? 1 : -1;

            if(n1.isNegative && !n2.isNegative)
                return -1;
            if(!n1.isNegative && n2.isNegative)
                return 1;
            int partsComparisonResult = CompareCore(n1.parts, n2.parts);
            return n1.isNegative ? -partsComparisonResult : partsComparisonResult;
        }
        static int CompareCore(IList<int> parts1, IList<int> parts2) {
            int lenDifference = parts1.Count - parts2.Count;
            if(lenDifference != 0)
                return lenDifference;
            int count = parts1.Count;
            for(int i = count - 1; i >= 0; i--) {
                int difference = parts1[i] - parts2[i];
                if(difference != 0)
                    return difference;
            }
            return 0;
        }
        public override string ToString() {
            int startIndex = parts.Count - 1;
            StringBuilder sb = new StringBuilder();
            if(parts.Count == 0)
                return "0";
            if(isNegative)
                sb.Append("-");
            for(int i = startIndex; i >= 0; i--) {
                string stringValue = parts[i].ToString();
                if(i != startIndex) {
                    for(int j = BaseCount - stringValue.Length; j > 0; j--) {
                        sb.Append('0');
                    }
                }
                sb.Append(stringValue);
            }
            return sb.ToString();
        }
        protected override Number Add(Number n) {
            var longNumber = n.ConvertCast<LongIntegerNumber>();
            if(!isNegative && !longNumber.isNegative)
                return AddCore(longNumber);
            if(longNumber.isNegative) {
                var invertedRight = new LongIntegerNumber(longNumber.parts, false);
                if(this < invertedRight)
                    return new LongIntegerNumber(invertedRight.Subtract(this).ConvertCast<LongIntegerNumber>().parts, true);
            } else {
                var invertedLeft = new LongIntegerNumber(this.parts, false);
                if(longNumber < invertedLeft)
                    return new LongIntegerNumber(invertedLeft.Subtract(longNumber).ConvertCast<LongIntegerNumber>().parts, true);
            }
            return AddCore(longNumber);
        }
        protected override Number Subtract(Number n) {
            var longNumber = n.ConvertCast<LongIntegerNumber>();
            return Add(new LongIntegerNumber(longNumber.parts, !longNumber.isNegative));
        }
        Number AddCore(LongIntegerNumber longNumber) {
            return new LongIntegerNumber(AddImpl(this.parts, longNumber.parts, isNegative, longNumber.isNegative), false);
        }
        static IList<int> AddImpl(IList<int> left, IList<int> right, bool isLeftNegative, bool isRightNegative) {
            int count = Math.Max(left.Count, right.Count);
            List<int> result = new List<int>();
            int carry = 0;
            for(int i = 0; i < count; i++) {
                int resultPart = GetPart(left, i, isLeftNegative) + GetPart(right, i, isRightNegative) + carry;//TODO optimization - stop when one is empty
                if(resultPart >= BaseFull) {
                    resultPart = resultPart - BaseFull;
                    carry = 1;
                } else if(resultPart < 0) {
                    resultPart = resultPart + BaseFull;
                    carry = -1;
                } else {
                    carry = 0;
                }
                result.Add(resultPart);
            }
            if(carry > 0)
                result.Add(carry);
            for(int i = result.Count - 1; i >= 0; i--) {
                if(result[i] == 0)
                    result.RemoveAt(i);
                else
                    break;
            }
            return result;
        }
        protected override Number Multiply(Number n) {
            var longNumber = n.ConvertCast<LongIntegerNumber>();
            return new LongIntegerNumber(MultiplyCore(this.parts, longNumber.parts), isNegative ^ longNumber.isNegative);
        }

        static IList<int> MultiplyCore(IList<int> left, IList<int> right) {
            if(right.Count == 0)
                return Zero.parts;
            IList<int> result = new int[0];
            int rightCount = right.Count;
            for(int i = 0; i < rightCount; i++) {
                result = AddImpl(result, MultiplyOneDigit(left, right[i], i), false, false);
            }
            return result;
        }
        static IList<int> MultiplyOneDigit(IList<int> left, int digit, int shift) {
            List<int> result = new List<int>();
            int carry = 0;
            int leftCount = left.Count;
            AddTrailingZeros(result, shift);
            for(int leftIndex = 0; leftIndex < leftCount; leftIndex++) {
                int resultPart = left[leftIndex] * digit + carry;
                if(resultPart >= BaseFull) {
                    int remain = resultPart % BaseFull;
                    carry = (resultPart - remain) / BaseFull; //TODO move to platfrom helper
                    resultPart = remain;
                } else {
                    carry = 0;
                }
                result.Add(resultPart);
            }
            if(carry > 0)
                result.Add(carry);
            return result;
        }
        static void AddTrailingZeros(IList<int> result, int shift) {
            for(int i = 0; i < shift; i++) {
                result.Add(0);
            }
        }
        static void ShiftLeft(IList<int> result) {
            result.Insert(0, 0);
        }
        static void ShiftRight(IList<int> result) {
            result.RemoveAt(0);
        }
        protected override Number Divide(Number n) {
            return Divide(n, true);
        }
        public LongIntegerNumber IntDivide(Number n) {
            return (LongIntegerNumber)Divide(n, false);
        }
        internal LongIntegerNumber Modulo(Number n) {
            var longNumber = n.ConvertCast<LongIntegerNumber>();
            LongIntegerNumber remain = Zero;
            DivieImpl(parts, longNumber.parts, out remain);
            return remain;
        }
        Number Divide(Number n, bool allowFraction) {
            var longNumber = n.ConvertCast<LongIntegerNumber>();
            bool isResultNegative = isNegative ^ longNumber.isNegative;
            if(allowFraction && CompareCore(parts, longNumber.parts) < 0)
                return FractionNumber.Create(new LongIntegerNumber(parts, isResultNegative), new LongIntegerNumber(longNumber.parts, false)); ;
            return DivieCore(parts, longNumber.parts, isResultNegative, allowFraction);
        }
        static Number DivieCore(IList<int> dividentParts, IList<int> originalDivisor, bool isNegative, bool allowFraction) {
            LongIntegerNumber remain = Zero;
            IList<int> result = DivieImpl(dividentParts, originalDivisor, out remain);
            if(allowFraction && remain != Zero)
                return FractionNumber.Create(new LongIntegerNumber(dividentParts, isNegative), new LongIntegerNumber(originalDivisor, false));
            return new LongIntegerNumber(result, isNegative);
        }
        static IList<int> DivieImpl(IList<int> dividentParts, IList<int> originalDivisor, out LongIntegerNumber remain) {
            remain = new LongIntegerNumber(dividentParts, false);
            IList<int> divisor = new List<int>(originalDivisor);
            int shiftCount = 0;
            while(CompareCore(remain.parts, divisor) >= 0) {
                ShiftLeft(divisor);
                shiftCount++;
            }
            ShiftRight(divisor);
            shiftCount--;
            List<int> result = new List<int>();
            while(divisor.Count >= originalDivisor.Count) {
                int digit = FindDigit(remain, divisor);
                result.Insert(0, digit);
                Number temp = (new LongIntegerNumber(divisor, false)).Multiply(new LongIntegerNumber(new int[] { digit }, false));
                remain = (LongIntegerNumber)remain.Subtract(temp);
                if(CompareCore(remain.parts, divisor) < 0)
                    ShiftRight(divisor);
            }
            return result;
        }
        static int Div(int x, int y) {
            int remain = x % y;
            return (x - remain) / y; //TODO
        }
        static int Bisect(int lowDigit, int topDigit) {
            return lowDigit + Div(topDigit - lowDigit, 2);
        }
        static int FindDigit(LongIntegerNumber divident, IList<int> divisorParts) {
            LongIntegerNumber divisor = new LongIntegerNumber(divisorParts, false);
            if(divident == Zero || divisor > divident) {
                return 0;
            }
            int dividentPart = ((divident.parts.Count == divisor.parts.Count) ? divident.parts.Last() : divident.parts.Last() * BaseFull + divident.parts[divident.parts.Count - 2]);
            int divisorPart = divisor.parts.Last();
            int lowDigit = Div(dividentPart, divisorPart + 1); //TODO optimize - no need to add 1 if divisor has zeros at the end
#if DEBUG
            int checkCount = 0;
#endif
            int topDigit = Div(dividentPart + 1, divisorPart);
            int digit = Bisect(lowDigit, topDigit);
            while(true) {
                Number mult = divisor.Multiply(new LongIntegerNumber(new int[] { digit }, false));
                LongIntegerNumber remain = (LongIntegerNumber)divident.Subtract(mult);
                if(remain.isNegative) {
#if DEBUG
                    checkCount++;
#endif
                    topDigit = digit;
                    digit = Bisect(lowDigit, topDigit);
                    continue;
                }
                int comparisonResult = remain.Compare(divisor);
                if(comparisonResult >= 0) {
#if DEBUG
                    checkCount++;
#endif
                    lowDigit = digit + 1;
                    digit = Bisect(lowDigit, topDigit);
                    continue;
                }
                break;
            }
#if DEBUG
            if(checkCount > 15)
                throw new InvalidOperationException();
#endif

            return digit;
        }
        protected override Number Power(Number n) {
            var b = n.ConvertCast<LongIntegerNumber>();
            return FastPower(this, b);
        }
        internal static Number FastPower(Number a, LongIntegerNumber b) {
            Number re = One;
            while(b != Zero) {
                if(b.parts.First() % 2 == 1)
                    re = re * a;
                a = (a * a);
                b = b.IntDivide(Two);
            }
            return b.isNegative ? (One / re) : re;
        }
        static int GetPart(IList<int> parts, int index, bool isNegative) {
            return index < parts.Count ? (isNegative ? -parts[index] : parts[index]) : 0;
        }
    }
}
