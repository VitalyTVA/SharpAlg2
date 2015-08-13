using NUnit.Framework;
using SharpAlg.Geo.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
//using static SharpAlg.Geo.Core.Utility;

namespace SharpAlg.Geo.Tests {
    public static class MyFactoryClass {
    }
    [TestFixture]
    public static class IsNormalTests {
        [Test, Explicit]
        public static void Explicit() {
            Builder.CreateSimple().Build((x, y, z) => x + y + 5).IsNormal();
        }
        [Test, TestCaseSource("TestCases")]
        public static bool IsNormal(Expr expr) {
            return expr.IsNormal();
        }

        public static IEnumerable<TestCaseData> TestCases {
            get {
                #region trivial
                yield return MakeIsNormalTestCase(true, x => 1);
                yield return MakeIsNormalTestCase(true, x => x);
                #endregion

                #region mult
                yield return MakeIsNormalTestCase(true, x => 2 * x);
                yield return MakeIsNormalTestCase(false, x => ((Expr)2 ^ 2) * x);
                yield return MakeIsNormalTestCase(false, x => ((Expr)2 + 1) * x);
                yield return MakeIsNormalTestCase(false, x => x * 2);

                yield return MakeIsNormalTestCase(true, (x, y) => x * y);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * (y / z));
                yield return MakeIsNormalTestCase(false, (x, y) => x * (y + 1));
                yield return MakeIsNormalTestCase(false, (x, y) => y * x);
                yield return MakeIsNormalTestCase(false, (x, y) => x * x);

                yield return MakeIsNormalTestCase(true, (x, y) => (x ^ 2) * y);
                yield return MakeIsNormalTestCase(false, (x, y) => y * (x ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y) => (x ^ 2) * x);

                yield return MakeIsNormalTestCase(true, x => x ^ 2);
                yield return MakeIsNormalTestCase(false, x => (x + 1) ^ 2);

                yield return MakeIsNormalTestCase(false, (x, y) => x * y * x);
                yield return MakeIsNormalTestCase(false, (x, y) => x * y * (x ^ 2));
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * y * z);
                yield return MakeIsNormalTestCase(true, (x, y, z) => (x ^ 2) * y * z);
                yield return MakeIsNormalTestCase(true, (x, y, z) => 5 * x * y * z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * 5 * y * z);
                yield return MakeIsNormalTestCase(false, (x, y) => (Expr)5 * 6 * x * y);
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * (y ^ 2) * (z ^ 3));
                #endregion

                #region add
                yield return MakeIsNormalTestCase(true, (x, y, z) => x + y + z);
                yield return MakeIsNormalTestCase(true, (x, y, z) => x + (y ^ 2) * z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x + z * (y ^ 2));
                yield return MakeIsNormalTestCase(true, (x, y, z) => x + 5 * y + z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x + y * 5 + z);
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * y + 5 * z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * y + z * 5);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x + (y / z));
                yield return MakeIsNormalTestCase(true, (x, y) => x + y + 5);
                yield return MakeIsNormalTestCase(false, (x, y) => x + 5 + y);
                yield return MakeIsNormalTestCase(false, (x, y) => y + x + 5);

                yield return MakeIsNormalTestCase(false, (x, y) => y + x);
                #endregion

                #region div
                yield return MakeIsNormalTestCase(true, (x, y) => x / y);
                yield return MakeIsNormalTestCase(true, (x, y) => (x + y) / (y ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x / (y / z));
                yield return MakeIsNormalTestCase(false, (x, y) => (x + (y / x)) / y);

                yield return MakeIsNormalTestCase(true, (x, y) => (5 * x) / y);
                //yield return MakeIsNormalTestCase(false, (x, y) => x / (5 * y));
                #endregion
            }
        }
        static TestCaseData MakeIsNormalTestCase(bool isNormal, Expression<Func<Expr, Expr>> expr) {
            return MakeIsNormalTestCaseCore(isNormal, b => b.Build(expr));
        }
        static TestCaseData MakeIsNormalTestCase(bool isNormal, Expression<Func<Expr, Expr, Expr>> expr) {
            return MakeIsNormalTestCaseCore(isNormal, b => b.Build(expr));
        }
        static TestCaseData MakeIsNormalTestCase(bool isNormal, Expression<Func<Expr, Expr, Expr, Expr>> expr) {
            return MakeIsNormalTestCaseCore(isNormal, b => b.Build(expr));
        }
        static TestCaseData MakeIsNormalTestCaseCore(bool isNormal, Func<Builder, Expr> getExpr) {
            var expr = getExpr(Builder.CreateSimple());
            return new TestCaseData(expr)
                .Returns(isNormal)
                .SetName(isNormal.ToString() + ": " + expr.ToString());
        }
    }
}