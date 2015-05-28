
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SharpAlg.Native {
    public static class PlatformHelper {
        public static string ToInvariantString(this double d) {
            return d.ToString(CultureInfo.InvariantCulture);
        }
        public static double Parse(string s) {
            return double.Parse(s, CultureInfo.InvariantCulture);
        }
        public static char IntToChar(int n) {
            return (char)n;
        }
        public static int CharToInt(char c) {
            return c;
        }
        public static string GetMessage(this Exception e) {
            return e.Message;
        }
    }
}
