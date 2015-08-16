using NUnit.Framework;
using SharpAlg.Geo.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public static class IsNormalTests {
        [Test, Explicit]
        public static void Explicit() {
            Assert.IsTrue(Builder.CreateSimple().Build((x, y, z) => sqrt(x / (y + z)) + sqrt(x / y)).IsNormal());
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
                yield return MakeIsNormalTestCase(false, (x, y) => x + x);
                yield return MakeIsNormalTestCase(true, (x, y, z) => x + y + z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x + (y ^ 2) * z);
                yield return MakeIsNormalTestCase(true, (x, y, z) => (y ^ 2) * z + x);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x + z * (y ^ 2));
                yield return MakeIsNormalTestCase(true, (x, y, z) => y * z + y);
                yield return MakeIsNormalTestCase(false, (x, y, z) => y + y * z);
                yield return MakeIsNormalTestCase(true, (x, y, z) => x + 5 * y + z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x + y * 5 + z);
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * y + 5 * z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * y + z * 5);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x + (y / z));
                yield return MakeIsNormalTestCase(true, (x, y) => x + y + 5);
                yield return MakeIsNormalTestCase(false, (x, y) => x + 5 + y);
                yield return MakeIsNormalTestCase(false, (x, y) => y + x + 5);
                yield return MakeIsNormalTestCase(false, (x, y) => (Expr)5 + 6);
                yield return MakeIsNormalTestCase(false, (x, y) => y + x);
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * y + x * z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * z + x * y);

                yield return MakeIsNormalTestCase(true, (x, y, z) => x * (y ^ 2) + x * (z ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * (z ^ 2) + x * (y ^ 2));
                yield return MakeIsNormalTestCase(true, (x, y, z) => (x ^ 2) * y + x * (z ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * (z ^ 2) + (x ^ 2) * y);

                yield return MakeIsNormalTestCase(true, (x, y, z) => x * (y ^ 2) * z + x * y * (z ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * y * (z ^ 2) + x * (y ^ 2) * z);

                yield return MakeIsNormalTestCase(true, (x, y, z) => x * y * z + x * (z ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * (z ^ 2) + x * y * z);
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * (y ^ 2) + x * y * z);
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * y * z + x * (y ^ 2));
                #endregion

                #region div
                yield return MakeIsNormalTestCase(true, (x, y) => x / y);
                yield return MakeIsNormalTestCase(true, (x, y) => (x + y) / (y ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y) => (y + x) / (y ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x / (y / z));
                yield return MakeIsNormalTestCase(false, (x, y) => (x + (y / x)) / y);

                yield return MakeIsNormalTestCase(true, (x, y) => (5 * x) / y);
                yield return MakeIsNormalTestCase(true, (x, y) => x / (5 * y));
                #endregion

                #region sqrt
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(x + y) ^ 2);
                yield return MakeIsNormalTestCase(true, (x, y) => sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(y + x));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(x + y) * x);
                yield return MakeIsNormalTestCase(false, (x, y) => x * sqrt(x + y) * y);
                yield return MakeIsNormalTestCase(true, (x, y) => x * sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y) => x * sqrt(y + x));

                yield return MakeIsNormalTestCase(true, (x, y, z) => sqrt((x + y) / z));
                yield return MakeIsNormalTestCase(true, (x, y, z) => sqrt(x / (y + z)));
                yield return MakeIsNormalTestCase(false, (x, y, z) => sqrt((y + x) / z));
                yield return MakeIsNormalTestCase(false, (x, y, z) => sqrt(x / (z + y)));
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * sqrt((x + y) / z));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * sqrt((y + x) / z));

                yield return MakeIsNormalTestCase(true, (x, y, z) => sqrt(x / (y + z)) + sqrt(x / y));
                yield return MakeIsNormalTestCase(false, (x, y, z) => sqrt(x / y) + sqrt(x / (y + z)));
                yield return MakeIsNormalTestCase(true, (x, y, z) => sqrt(x / y) + sqrt(x));
                yield return MakeIsNormalTestCase(false, (x, y, z) => sqrt(x) + sqrt(x / y));

                yield return MakeIsNormalTestCase(true, (x, y) => x * sqrt(x + y) + y);
                yield return MakeIsNormalTestCase(false, (x, y) => y + x * sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y) => x * sqrt(y + x) + y);
                yield return MakeIsNormalTestCase(true, (x, y, z) => sqrt(x + y + z) + sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y, z) => sqrt(x + y) + sqrt(x + y  + z));
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * sqrt(x + y + z) + x * sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * sqrt(x + y) + x * sqrt(x + y + z));
                yield return MakeIsNormalTestCase(true, (x, y) => sqrt(x + y) + sqrt(x));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(x) + sqrt(x + y));

                yield return MakeIsNormalTestCase(true, (x, y) => x * sqrt(x + y) + x);
                yield return MakeIsNormalTestCase(false, (x, y) => x  + x * sqrt(x + y));
                yield return MakeIsNormalTestCase(true, (x, y) => x  + sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(x + y) + x);

                yield return MakeIsNormalTestCase(true, (x, y, z) => sqrt(x + y) + sqrt(x + z));
                yield return MakeIsNormalTestCase(false, (x, y, z) => sqrt(x + z) + sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y, z) => sqrt(x + y) + sqrt(x + y));
                yield return MakeIsNormalTestCase(true, (x, y, z) => sqrt(y) + sqrt(z));
                yield return MakeIsNormalTestCase(false, (x, y, z) => sqrt(z) + sqrt(y));

                yield return MakeIsNormalTestCase(true, (x, y) => sqrt(x * y));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(y * x));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(x * y + y * x));
                yield return MakeIsNormalTestCase(true, (x, y) => sqrt(5 * x * y));
                yield return MakeIsNormalTestCase(false, (x, y) => x + sqrt(x * 5 * y));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(x * 5 * y));
                yield return MakeIsNormalTestCase(true, (x, y) => sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(y + x));

                yield return MakeIsNormalTestCase(true, (x, y) => sqrt(sqrt(x * y)));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(sqrt(y * x)));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(sqrt(x * y + y * x)));
                yield return MakeIsNormalTestCase(true, (x, y) => sqrt(sqrt(5 * x * y)));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(x + sqrt(x * 5 * y)));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(sqrt(x * 5 * y)));
                yield return MakeIsNormalTestCase(true, (x, y) => sqrt(sqrt(x + y)));
                yield return MakeIsNormalTestCase(false, (x, y) => sqrt(sqrt(y + x)));
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

        [Test, Ignore] //TODO memoize
        public static void LargeNormalExpression() {
            var builder = Builder.CreateRealLife();
            var x = Param("x");
            var y = Param("y");
            var expr = builder.Build((a, b) => sqrt(a + b), x, y);
            for(int i = 0; i < 17; i++) {
                expr = builder.Build((a, b, e) => sqrt(a * e + b * e), x, y, expr);
            }
            Assert.IsTrue(expr.IsNormal());
        }
    }
}