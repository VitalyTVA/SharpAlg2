using SharpAlg.Native.Numbers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Core_Number)]
    public static class NumberFactory {
        public static readonly Number Zero;
        public static readonly Number One;
        public static readonly Number Two;
        public static readonly Number MinusOne;
        public static readonly Number Pi;
        static NumberFactory() {
            Zero = LongIntegerNumber.Zero;
            One = LongIntegerNumber.One;
            Two = LongIntegerNumber.Two;
            MinusOne = LongIntegerNumber.MinusOne;
            Pi = Math.PI;
        }

        public static Number GetFloat(Number n, Func<double, double> evaluator) {
            return evaluator(n.ToFloat().ConvertCast<FloatNumber>().value);
        }
        public static double ToDouble(this Number number) {
            return ((FloatNumber)GetFloat(number, x => x)).value;
        }
        public static Number FromString(string s) {
            return PlatformHelper.Parse(s);
        }
        public static Number FromIntString(string s) {
            return LongIntegerNumber.FromLongIntStringCore(s);
        }

    }
}
