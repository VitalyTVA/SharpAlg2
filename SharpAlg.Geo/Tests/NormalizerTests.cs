﻿using NUnit.Framework;
using SharpAlg.Geo.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo.Tests {
    public static class MyFactoryClass {
    }
    [TestFixture]
    public static class IsNormalTests {
        [Test, Explicit]
        public static void Explicit() {
            Assert.IsTrue(Builder.CreateSimple().Build((x, y, z) => Sqrt(x + y + z) + Sqrt(x + y)).IsNormal());
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
                yield return MakeIsNormalTestCase(false, (x, y) => Sqrt(x + y) ^ 2);
                yield return MakeIsNormalTestCase(true, (x, y) => Sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y) => Sqrt(y + x));
                yield return MakeIsNormalTestCase(false, (x, y, z) => Sqrt((x + y) / z));
                yield return MakeIsNormalTestCase(false, (x, y) => Sqrt(x + y) * x);
                yield return MakeIsNormalTestCase(false, (x, y) => x * Sqrt(x + y) * y);
                yield return MakeIsNormalTestCase(true, (x, y) => x * Sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * Sqrt((x + y) / z));
                yield return MakeIsNormalTestCase(false, (x, y) => x * Sqrt(y + x));

                yield return MakeIsNormalTestCase(true, (x, y) => x * Sqrt(x + y) + y);
                yield return MakeIsNormalTestCase(false, (x, y) => y + x * Sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y) => x * Sqrt(y + x) + y);
                yield return MakeIsNormalTestCase(true, (x, y, z) => Sqrt(x + y + z) + Sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y, z) => Sqrt(x + y) + Sqrt(x + y  + z));
                yield return MakeIsNormalTestCase(true, (x, y, z) => x * Sqrt(x + y + z) + x * Sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y, z) => x * Sqrt(x + y) + x * Sqrt(x + y + z));
                yield return MakeIsNormalTestCase(true, (x, y) => Sqrt(x + y) + Sqrt(x));
                yield return MakeIsNormalTestCase(false, (x, y) => Sqrt(x) + Sqrt(x + y));

                yield return MakeIsNormalTestCase(true, (x, y) => x * Sqrt(x + y) + x);
                yield return MakeIsNormalTestCase(false, (x, y) => x  + x * Sqrt(x + y));
                yield return MakeIsNormalTestCase(true, (x, y) => x  + Sqrt(x + y));
                yield return MakeIsNormalTestCase(false, (x, y) => Sqrt(x + y) + x);

                //yield return MakeIsNormalTestCase(true, (x, y, z) => Sqrt(x + y) + Sqrt(x + z));
                //yield return MakeIsNormalTestCase(false, (x, y, z) => Sqrt(x + y) + Sqrt(x + z));
                //yield return MakeIsNormalTestCase(true, (x, y, z) => Sqrt(y) + Sqrt(z));
                //yield return MakeIsNormalTestCase(false, (x, y, z) => Sqrt(y) + Sqrt(z));
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