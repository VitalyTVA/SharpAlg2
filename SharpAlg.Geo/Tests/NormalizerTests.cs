using NUnit.Framework;
using SharpAlg.Geo.Core;
using System;
using System.Collections;
using System.Linq.Expressions;
//using static SharpAlg.Geo.Core.Utility;

namespace SharpAlg.Geo.Tests {
    public static class MyFactoryClass {
    }
    [TestFixture]
    public static class IsNormalTests {
        [Test, TestCaseSource("TestCases")]
        public static bool IsNormal(Func<Builder, Expr> getExpr) {
            return getExpr(Builder.CreateSimple()).IsNormal();
        }

        public static IEnumerable TestCases {
            get {
                yield return MakeIsNormalTestCase(true, x => 1);
                yield return MakeIsNormalTestCase(true, x => x);

                yield return MakeIsNormalTestCase(true, x => 2 * x);
                yield return MakeIsNormalTestCase(false, x => x * 2);

                yield return MakeIsNormalTestCase(true, (x, y) => x * y);
                yield return MakeIsNormalTestCase(false, (x, y) => x * (y + 1));
                yield return MakeIsNormalTestCase(false, (x, y) => y * x);
                yield return MakeIsNormalTestCase(false, (x, y) => x * x);

                yield return MakeIsNormalTestCase(true, (x, y) => (x ^ 2) * y);
                yield return MakeIsNormalTestCase(false, (x, y) => y * (x ^ 2));
                yield return MakeIsNormalTestCase(false, (x, y) => (x ^ 2) * x);

                yield return MakeIsNormalTestCase(true, x => x ^ 2);
                yield return MakeIsNormalTestCase(false, x => (x + 1) ^ 2);

                //yield return MakeIsNormalTestCase(false, x => ((Expr)2 ^ 2) * x);

                //yield return MakeIsNormalTestCase(false, (x, y) => x * y * x);
                //yield return MakeIsNormalTestCase(false, (x, y) => x * y * (x ^ 2));
                //yield return MakeIsNormalTestCase(true, (x, y, z) => x * y * z);
                //yield return MakeIsNormalTestCase(true, (x, y, z) => 5 * x * y * z);
                //yield return MakeIsNormalTestCase(false, (x, y, z) => x * 5 * y * z);
                //yield return MakeIsNormalTestCase(false, (x, y) => 5 * 6 * y * z);
                //yield return MakeIsNormalTestCase(true, (x, y, z) => x * (y ^ 2) * (z ^ 3));
            }
        }
        static TestCaseData MakeIsNormalTestCase(bool isNormal, Expression<Func<Expr, Expr>> expr) {
            return MakeIsNormalTestCaseCore(isNormal, b => b.Build(expr), expr);
        }
        static TestCaseData MakeIsNormalTestCase(bool isNormal, Expression<Func<Expr, Expr, Expr>> expr) {
            return MakeIsNormalTestCaseCore(isNormal, b => b.Build(expr), expr);
        }
        //static TestCaseData MakeIsNormalTestCase(bool isNormal, Expression<Func<Expr, Expr, Expr, Expr>> expr) {
        //    return MakeIsNormalTestCaseCore(isNormal, b => b.Build(expr));
        //}
        static TestCaseData MakeIsNormalTestCaseCore(bool isNormal, Func<Builder, Expr> getExpr, LambdaExpression expr) {
            return new TestCaseData(getExpr)
                .Returns(isNormal)
                .SetName(expr.ToString());
        }
    }
}