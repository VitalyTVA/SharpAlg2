using System;
using System.Numerics;
using Numerics;
using NUnit.Framework;
using SharpAlg.Geo.Core;
using System.Linq.Expressions;
using System.Linq;
using static SharpAlg.Geo.Core.ExprExtensions;
using static SharpAlg.Geo.Core.Utility;
using System.Diagnostics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;
using System.Collections.Immutable;

namespace SharpAlg.Geo.Tests {
    public class ExprTestsBase {
        protected Builder builder;
        [SetUp]
        public void SetUp() {
            builder = CreateBuilder();
        }
        protected virtual Builder CreateBuilder() {
            return Builder.CreateSimple();
        }
    }
    public class ExprFunctionalTestsBase : ExprTestsBase {
        protected override Builder CreateBuilder() {
            return Builder.CreateRealLife();
        }
    }
    [TestFixture]
    public class ExprTests : ExprTestsBase {
        [Test]
        public void BasicStuffTest() {
            Expr five = 5;
            Assert.AreEqual((BigRational)5, five.ToConst());

            var a = Param("A");
            Assert.AreEqual("A", a.ToParam());

            var a_plus_five = builder.Build(x => x + 5, a).ToAdd();
            Assert.AreEqual(2, a_plus_five.Length);
            Assert.AreSame(a, a_plus_five[0]);
            Assert.AreEqual((BigRational)5, a_plus_five[1].ToConst());

            var five_minus_a = builder.Build(x => 5 - x, a).ToAdd();
            Assert.AreEqual(2, five_minus_a.Length);
            Assert.AreEqual((BigRational)5, five_minus_a[0].ToConst());
            Assert.AreEqual((BigRational)(-1), five_minus_a[1].ToMult()[0].ToConst());
            Assert.AreSame(a, five_minus_a[1].ToMult()[1]);


            var five_mult_a = builder.Build(x => 5 * x, a).ToMult();
            Assert.AreEqual(2, five_mult_a.Length);
            Assert.AreEqual((BigRational)5, five_mult_a[0].ToConst());
            Assert.AreSame(a, five_mult_a[1]);

            var sqrt_a = builder.Build(x => Sqrt(x), a).ToSqrt();
            Assert.AreSame(a, sqrt_a);

            var a_power_five = builder.Build(x => x ^ 5, a).ToPower();
            Assert.AreSame(a, a_power_five.Value);
            Assert.AreEqual((BigInteger)5, a_power_five.Power);

            var a_div_five = builder.Build((x, y) => x / y, a, five).ToDiv();
            Assert.AreSame(a, a_div_five.Num);
            //if(a_div_five == a_div_five) { }

            var minus_a = builder.Build(x => -x, a).ToMult();
            Assert.AreEqual(2, minus_a.Length);
            Assert.AreEqual((BigRational)(-1), minus_a[0].ToConst());
            Assert.AreSame(a, minus_a[1]);

            Assert.Throws<PowerShouldBePositiveException>(() => { builder.Build(x => x ^ 0, a); });
            Assert.Throws<PowerShouldBePositiveException>(() => { builder.Build(x => x ^ -1, a); });
            Assert.Throws<InvalidExpressionException>(() => { builder.Build(x => SomeFunc(x), a); });
            Assert.Throws<ArgumentNullException>(() => Param(string.Empty));
        }
        static Expr SomeFunc(Expr e) {
            throw new NotImplementedException();
        }
        [Test]
        public void ToStringTest() {
            builder.Build(() => 9).AssertSimpleStringRepresentation("9");
            builder.Build(() => -9).AssertSimpleStringRepresentation("-9");
            builder.Build(x => x).AssertSimpleStringRepresentation("x");
            builder.Build(x => -x).AssertSimpleStringRepresentation("-x");
            builder.Build(x => 9 + x).AssertSimpleStringRepresentation("9 + x");
            builder.Build(x => 9 - x).AssertSimpleStringRepresentation("9 - x");
            builder.Build(x => -(9 - x)).AssertSimpleStringRepresentation("-(9 - x)");
            builder.Build(x => 9 * x).AssertSimpleStringRepresentation("9 * x");
            builder.Build((x, y, z) => x + y * z).AssertSimpleStringRepresentation("x + y * z");
            builder.Build((x, y, z) => (x + y) * z).AssertSimpleStringRepresentation("(x + y) * z");
            builder.Build((x, y, z) => z * (x + y)).AssertSimpleStringRepresentation("z * (x + y)");
            builder.Build(x => x ^ 4).AssertSimpleStringRepresentation("x ^ 4");
            builder.Build((x, y) => x * (y ^ 3)).AssertSimpleStringRepresentation("x * (y ^ 3)");
            builder.Build((x, y) => x * y ^ 3).AssertSimpleStringRepresentation("(x * y) ^ 3");
            builder.Build(x => x ^ 2 ^ 3).AssertSimpleStringRepresentation("(x ^ 2) ^ 3");
            builder.Build((x, y, z) => x + y + z).AssertSimpleStringRepresentation("x + y + z");
            builder.Build((x, y, z) => x - y - z).AssertSimpleStringRepresentation("x - y - z");
            builder.Build((x, y) => 1 + 2 * x + 3 * y).AssertSimpleStringRepresentation("1 + 2 * x + 3 * y");
            builder.Build((x, y) => (x + 1) ^ (2 * 3)).AssertSimpleStringRepresentation("(x + 1) ^ 6");

            builder.Build(x => 9 - (-x)).AssertSimpleStringRepresentation("9 - (-x)");
            builder.Build(x => 9 * (-x)).AssertSimpleStringRepresentation("9 * (-1) * x");
            builder.Build(x => 9 / (-x)).AssertSimpleStringRepresentation("9 / (-x)");
            builder.Build(x => x * (-1)).AssertSimpleStringRepresentation("x * (-1)");

            builder.Build(x => (-2) * x).AssertSimpleStringRepresentation("-2 * x");
            builder.Build(x => -2 * (x + 1)).AssertSimpleStringRepresentation("-2 * (x + 1)");
            builder.Build((x, y) => -x + y).AssertSimpleStringRepresentation("-x + y");

            builder.Build((x, y) => y * (-x)).AssertSimpleStringRepresentation("y * (-1) * x");
            builder.Build((x, y) => -y * x).AssertSimpleStringRepresentation("-y * x");

            builder.Build((x, y) =>Sqrt(x + y)).AssertSimpleStringRepresentation("sqrt(x + y)");
        }
        [Test]
        public void ToStringTest_Div() {
            builder.Build(x => 9 / x).AssertSimpleStringRepresentation("9 / x");
            builder.Build((x, y, z) => x / y / z).AssertSimpleStringRepresentation("(x / y) / z");
            builder.Build(x => 9 / (1 / x)).AssertSimpleStringRepresentation("9 / (1 / x)");
            builder.Build(x => 9 + 1 / x).AssertSimpleStringRepresentation("9 + 1 / x");
            builder.Build(x => 9 - (1 / x)).AssertSimpleStringRepresentation("9 - 1 / x");
            builder.Build(x => 9 / (-x)).AssertSimpleStringRepresentation("9 / (-x)");
            builder.Build(x => 1 / (3 + x)).AssertSimpleStringRepresentation("1 / (3 + x)");
            builder.Build(x => (2 + x) / (3 + x)).AssertSimpleStringRepresentation("(2 + x) / (3 + x)");
            builder.Build(x => 2 * x / (3 + x)).AssertSimpleStringRepresentation("(2 * x) / (3 + x)");
            builder.Build((x, y, z) => 2 * x / (y * z)).AssertSimpleStringRepresentation("(2 * x) / (y * z)");
            builder.Build((x, y) => (x ^ 3) / (y ^ 4)).AssertSimpleStringRepresentation("(x ^ 3) / (y ^ 4)");
            builder.Build(x => 1 / (x ^ 3)).AssertSimpleStringRepresentation("1 / (x ^ 3)");
            builder.Build(x => 2 * x / (3 + x)).AssertSimpleStringRepresentation("(2 * x) / (3 + x)");
            builder.Build((x, y, z) => x / y / z).AssertSimpleStringRepresentation("(x / y) / z");
            builder.Build(x => 1 / (3 * x)).AssertSimpleStringRepresentation("1 / (3 * x)");
        }
        [Test]
        public void ToStringTest2() {
            var x = Param("x");
            var y = Param("y");
            var z = Param("z");
            builder.Multiply(-1, x, y).AssertSimpleStringRepresentation("-x * y");
            builder.Multiply(-2, x, y).AssertSimpleStringRepresentation("-2 * x * y");
            builder.Multiply(Const(new BigRational(2, 3)), x, y).AssertSimpleStringRepresentation("2/3 * x * y");
            builder.Multiply(Const(new BigRational(-2, 3)), x, y).AssertSimpleStringRepresentation("-2/3 * x * y");
            builder.Add(Const(new BigRational(2, 3)), x, y).AssertSimpleStringRepresentation("2/3 + x + y");
            Const(new BigRational(2, 9)).AssertSimpleStringRepresentation("2/9");
            builder.Power(Const(new BigRational(2, 3)), 4).AssertSimpleStringRepresentation("(2/3) ^ 4");
            builder.Power(Const(new BigRational(-2, 3)), 4).AssertSimpleStringRepresentation("(-2/3) ^ 4");

            builder.Add(z, builder.Multiply(-1, x, y)).AssertSimpleStringRepresentation("z - x * y");
            builder.Add(z, builder.Multiply(-2, x, y)).AssertSimpleStringRepresentation("z - 2 * x * y");
            builder.Power(builder.Multiply(-1, x, y), 2).AssertSimpleStringRepresentation("(-x * y) ^ 2");
        }
        [Test]
        public void ToStringTest2_Div() {
            var x = Param("x");
            var y = Param("y");
            var z = Param("z");

            builder.Divide(2, 9).AssertSimpleStringRepresentation("2 / 9");
            builder.Power(builder.Divide(2, 3), 4).AssertSimpleStringRepresentation("(2 / 3) ^ 4");
            builder.Power(builder.Divide(-2, 3), 4).AssertSimpleStringRepresentation("((-2) / 3) ^ 4");
            builder.Divide(builder.Multiply(2, x), builder.Multiply(y, z)).AssertSimpleStringRepresentation("(2 * x) / (y * z)");
        }

        [Test]
        public static void ParamEqualsAndGetHashCode() {
            var expr = Param("a");
            Assert.AreEqual(expr, Param("a"));
            Assert.AreNotEqual(expr, Param("b"));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, Param("a"));
            AssertHashCodesAreNotEqual(expr, Param("b"));

        }
        [Test]
        public void SqrtEqualsAndGetHashCode() {
            var expr = builder.Sqrt(Param("a"));
            Assert.AreEqual(expr, builder.Sqrt(Param("a")));
            Assert.AreNotEqual(expr, CreateBuilder().Sqrt(Param("a")));
            Assert.AreNotEqual(expr, builder.Sqrt(Param("b")));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Sqrt(Param("a")));
            AssertHashCodesAreNotEqual(expr, builder.Sqrt(Param("b")));
        }
        [Test]
        public static void ConstEqualsAndGetHashCode() {
            Expr expr = 1;
            Assert.AreEqual(expr, (Expr)1);
            Assert.AreNotEqual(expr, (Expr)2);
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, 1);
            AssertHashCodesAreNotEqual(expr, 2);
        }
        [Test]
        public void PowerEqualsAndGetHashCode() {
            Expr expr = builder.Power(Param("a"), 2);
            Assert.AreEqual(expr, builder.Power(Param("a"), 2));
            Assert.AreNotEqual(expr, CreateBuilder().Power(Param("a"), 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Power(Param("a"), 2));
            AssertHashCodesAreNotEqual(expr, builder.Power(Param("b"), 2));
            AssertHashCodesAreNotEqual(expr, builder.Power(Param("a"), 3));
        }
        [Test]
        public void AddEqualsAndGetHashCode() {
            Expr expr = builder.Add(Param("a"), 2);
            Assert.AreEqual(expr, builder.Add(Param("a"), 2));
            Assert.AreNotEqual(expr, CreateBuilder().Add(Param("a"), 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Add(Param("a"), 2));
            AssertHashCodesAreNotEqual(expr, builder.Add(Param("b"), 2));
            AssertHashCodesAreNotEqual(expr, builder.Add(Param("a"), 3));
        }
        [Test]
        public void MultEquals() {
            Expr expr = builder.Multiply(Param("a"), 2);
            Assert.AreEqual(expr, builder.Multiply(Param("a"), 2));
            Assert.AreNotEqual(expr, CreateBuilder().Multiply(Param("a"), 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Multiply(Param("a"), 2));
            AssertHashCodesAreNotEqual(expr, builder.Multiply(Param("b"), 2));
            AssertHashCodesAreNotEqual(expr, builder.Multiply(Param("a"), 3));
        }
        [Test]
        public void DivEquals() {
            Expr expr = builder.Divide(Param("a"), 2);
            Assert.AreEqual(expr, builder.Divide(Param("a"), 2));
            Assert.AreNotEqual(expr, CreateBuilder().Divide(Param("a"), 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Divide(Param("a"), 2));
            AssertHashCodesAreNotEqual(expr, builder.Divide(Param("b"), 2));
            AssertHashCodesAreNotEqual(expr, builder.Divide(Param("a"), 3));
        }
        [Test]
        public void HashCodeSalt() {
            AssertHashCodesAreNotEqual(Param("b"), builder.Sqrt(Param("b")));
            AssertHashCodesAreNotEqual(2, builder.Sqrt(2));

            AssertHashCodesAreNotEqual(Param("b"), builder.Add(Param("b")));
            AssertHashCodesAreNotEqual(builder.Multiply(Param("b")), builder.Add(Param("b")));
            AssertHashCodesAreNotEqual(builder.Divide(Param("b"), 2), builder.Add(Param("b"), 2));
        }
        [DebuggerStepThrough]
        static void AssertHashCodesAreEqual<T>(T a, T b) {
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
        [DebuggerStepThrough]
        static void AssertHashCodesAreNotEqual<T>(T a, T b) {
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void CannotMixExprsFromDifferentBuilders() {
            var expr = Builder.Simple.Add(Builder.Simple.Power(Param("a"), 2), Param("b"));
            Action<TestDelegate> assertThrows = x => Assert.Throws<CannotMixExpressionsFromDifferentBuildersException>(x);
            assertThrows(() => builder.Add(1, expr));
            assertThrows(() => builder.Multiply(1, expr));
            assertThrows(() => builder.Divide(1, expr));
            assertThrows(() => builder.Divide(expr, 1));
            assertThrows(() => builder.Power(expr, 2));
            assertThrows(() => builder.Sqrt(expr));
        }

        [Test]
        public void EvaluationMemoization() {
            var getParamCount = 0;
            Func<string, double> getParam = x => {
                getParamCount++;
                if(string.Equals(x, "a", StringComparison.Ordinal)) {
                    return 4;
                }
                if(string.Equals(x, "b", StringComparison.Ordinal)) {
                    return 3;
                }
                throw new InvalidOperationException();
            };
            var sqrtExpr = builder.Build(a => Sqrt(a));
            var addExpr = builder.Build(b => b + 1);
            var expr = builder.Build((x, y) => x + y + 1 / x + 1 / y, sqrtExpr, addExpr);
            Assert.AreEqual(6.75, expr.ToReal(getParam));
            Assert.AreEqual(2, getParamCount);
        }
        [Test]
        public void AddExprMemoization() {
            var addArgs = builder.Build(x => Sqrt(x + 1) + 1 / (x + 1)).ToAdd();
            Assert.AreSame(addArgs.First().ToSqrt(), addArgs.Last().ToDiv().Den);
            var e2 = builder.Build(x => x + 1);
            Assert.AreSame(addArgs.First().ToSqrt(), e2);
            Assert.AreNotSame(e2, CreateBuilder().Build(x => x + 1));
            Assert.AreNotSame(e2, builder.Build(x => x + 2));
        }
        [Test]
        public void MultExprMemoization() {
            var addArgs = builder.Build(x => Sqrt(2 * x) + 1 / (2 * x)).ToAdd();
            Assert.AreSame(addArgs.First().ToSqrt(), addArgs.Last().ToDiv().Den);
        }
        [Test]
        public void SqrtExprMemoization() {
            var addArgs = builder.Build(x => Sqrt(2 * x) + 1 / Sqrt(2 * x)).ToAdd();
            Assert.AreSame(addArgs.First(), addArgs.Last().ToDiv().Den);
        }
        [Test]
        public void PowerExprMemoization() {
            var addArgs = builder.Build(x => (x ^ 2) + 1 /(x ^ 2)).ToAdd();
            Assert.AreSame(addArgs.First(), addArgs.Last().ToDiv().Den);
            Assert.AreNotSame(addArgs.First(), builder.Build(x => x ^ 3));
        }
        [Test]
        public void DivExprMemoization() {
            var addArgs = builder.Build(x => (x / 3) + ((x / 3) ^ 2)).ToAdd();
            Assert.AreSame(addArgs.First(), addArgs.Last().ToPower().Value);
            //Assert.AreNotSame(e.Args.First(), builder.Build(x => x ^ 3));
        }
        [Test]
        public void MergeAddExpr() {
            var assert = Action((string[] expected, Expr expr) 
                => CollectionAssert.AreEqual(expected, expr.ToAdd().Select(x => x.ToParam())));
            assert(new[] { "x", "y", "z" }, builder.Build((x, y, z) => x + y + z));
            assert(new[] { "x", "y", "z" }, builder.Build((x, y, z) => (x + y) + z));
            assert(new[] { "x", "y", "z", "w" }, builder.Build((x, y, z, w) => (x + (y + z)) + w));
        }
        [Test]
        public void MergeMultExpr() {
            var assert = Action((string[] expected, Expr expr)
                => CollectionAssert.AreEqual(expected, expr.ToMult().Select(x => x.ToParam())));
            assert(new[] { "x", "y", "z" }, builder.Build((x, y, z) => x * y * z));
            assert(new[] { "x", "y", "z" }, builder.Build((x, y, z) => (x * y) * z));
            assert(new[] { "x", "y", "z", "w" }, builder.Build((x, y, z, w) => (x * (y * z)) * w));
        }
    }
    public static class ExprTestExtensions {
        //[DebuggerStepThrough]
        //public static void AssertIsNormal(this Builder builder, bool isNormal, Expression<Func<Expr, Expr>> f) {
        //    AssertIsNormal(isNormal, builder.Build(f));
        //}
        //[DebuggerStepThrough]
        //public static void AssertIsNormal(this Builder builder, bool isNormal, Expression<Func<Expr, Expr, Expr>> f) {
        //    AssertIsNormal(isNormal, builder.Build(f));
        //}
        //[DebuggerStepThrough]
        //public static void AssertIsNormal(this Builder builder, bool isNormal, Expression<Func<Expr, Expr, Expr, Expr>> f) {
        //    AssertIsNormal(isNormal, builder.Build(f));
        //}
        //public static void AssertIsNormal(this Builder builder, bool isNormal, Expression<Func<Expr, Expr, Expr, Expr, Expr>> f) {
        //    AssertIsNormal(isNormal, builder.Build(f));
        //}
        //[DebuggerStepThrough]
        //static void AssertIsNormal(bool isNormal, Expr expr) {
        //    Assert.AreEqual(isNormal, expr.IsNormal());
        //}
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr>> f) {
            return builder.Build(f, GetParameters(f).Single());
        }
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr>> f) {
            var parameters = GetParameters(f);
            return builder.Build(f, parameters[0], parameters[1]);
        }
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr, Expr>> f) {
            var parameters = GetParameters(f);
            return builder.Build(f, parameters[0], parameters[1], parameters[2]);
        }
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr, Expr, Expr>> f) {
            var parameters = GetParameters(f);
            return builder.Build(f, parameters[0], parameters[1], parameters[2], parameters[3]);
        }
        public static Expr Build(this Builder builder, Expression<Func<Expr, Expr, Expr, Expr, Expr, Expr>> f) {
            var parameters = GetParameters(f);
            return builder.Build(f, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]);
        }
        static ExprList GetParameters(LambdaExpression expression) {
            return expression.Parameters.Select(x => Param(x.Name)).ToImmutableArray();
        }
        public static Expr AssertSimpleStringRepresentation(this Expr expr, string str) {
            Assert.AreEqual(str, expr.ToString());
            return expr;
        }
    }
}