using System;
using System.Numerics;
using Numerics;
using NUnit.Framework;
using SharpAlg.Geo.Core;
using System.Linq.Expressions;
using System.Linq;
using static SharpAlg.Geo.Core.ExprExtensions;
using System.Diagnostics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;
using System.Collections.Immutable;

namespace SharpAlg.Geo.Tests {
    public class ExprTestsBase {
        protected IBuilder builder;
        [SetUp]
        public void SetUp() {
            builder = new CachingBuilder();
        }
    }
    [TestFixture]
    public class ExprTests : ExprTestsBase {
        [Test]
        public void BasicStuffTest() {
            Expr five = 5;
            Assert.AreEqual((BigRational)5, ((ConstExpr)five).Value);

            ParamExpr a = (ParamExpr)"A";
            Assert.AreEqual("A", a.Name);

            AddExpr a_plus_five = (AddExpr)builder.Build(x => x + 5, a);
            Assert.AreEqual(2, a_plus_five.Args.Length);
            Assert.AreSame(a, a_plus_five.Args[0]);
            Assert.AreEqual((BigRational)5, ((ConstExpr)a_plus_five.Args[1]).Value);

            AddExpr five_minus_a = (AddExpr)builder.Build(x => 5 - x, a);
            Assert.AreEqual(2, five_minus_a.Args.Length);
            Assert.AreEqual((BigRational)5, ((ConstExpr)five_minus_a.Args[0]).Value);
            Assert.AreEqual((BigRational)(-1), ((ConstExpr)(five_minus_a.Args[1] as MultExpr).Args[0]).Value);
            Assert.AreSame(a, (five_minus_a.Args[1] as MultExpr).Args[1]);


            MultExpr five_mult_a = (MultExpr)builder.Build(x => 5 * x, a);
            Assert.AreEqual(2, five_mult_a.Args.Length);
            Assert.AreEqual((BigRational)5, ((ConstExpr)five_mult_a.Args[0]).Value);
            Assert.AreSame(a, five_mult_a.Args[1]);

            SqrtExpr sqrt_a = (SqrtExpr)builder.Build(x => Sqrt(x), a);
            Assert.AreSame(a, sqrt_a.Value);

            PowerExpr a_power_five = (PowerExpr)builder.Build(x => x ^ 5, a);
            Assert.AreSame(a, a_power_five.Value);
            Assert.AreEqual((BigInteger)5, a_power_five.Power);

            DivExpr a_div_five = (DivExpr)builder.Build((x, y) => x / y, a, five);
            Assert.AreSame(a, a_div_five.Numerator);
            //if(a_div_five == a_div_five) { }

            MultExpr minus_a = (MultExpr)builder.Build(x => -x, a);
            Assert.AreEqual(2, minus_a.Args.Length);
            Assert.AreEqual((BigRational)(-1), ((ConstExpr)minus_a.Args[0]).Value);
            Assert.AreSame(a, minus_a.Args[1]);

            Assert.Throws<PowerShouldBePositiveException>(() => { builder.Build(x => x ^ 0, a); });
            Assert.Throws<PowerShouldBePositiveException>(() => { builder.Build(x => x ^ -1, a); });
            Assert.Throws<InvalidExpressionException>(() => { builder.Build(x => SomeFunc(x), a); });
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
            builder.Build((x, y) => x * (y ^ 3)).AssertSimpleStringRepresentation("x * y ^ 3");
            builder.Build((x, y) => x * y ^ 3).AssertSimpleStringRepresentation("(x * y) ^ 3");
            builder.Build(x => x ^ 2 ^ 3).AssertSimpleStringRepresentation("(x ^ 2) ^ 3");
            builder.Build((x, y, z) => x + y + z).AssertSimpleStringRepresentation("x + y + z");
            builder.Build((x, y, z) => x - y - z).AssertSimpleStringRepresentation("x - y - z");
            builder.Build((x, y) => 1 + 2 * x + 3 * y).AssertSimpleStringRepresentation("1 + 2 * x + 3 * y");
            builder.Build((x, y) => (x + 1) ^ (2 * 3)).AssertSimpleStringRepresentation("(x + 1) ^ 6");

            builder.Build(x => 9 - (-x)).AssertSimpleStringRepresentation("9 - (-x)");
            builder.Build(x => 9 * (-x)).AssertSimpleStringRepresentation("9 * (-x)");
            builder.Build(x => x * (-1)).AssertSimpleStringRepresentation("x * (-1)");

            builder.Build(x => (-2) * x).AssertSimpleStringRepresentation("-2 * x");
            builder.Build(x => -2 * (x + 1)).AssertSimpleStringRepresentation("-2 * (x + 1)");
            builder.Build((x, y) => -x + y).AssertSimpleStringRepresentation("-x + y");

            builder.Build((x, y) => y * (-x)).AssertSimpleStringRepresentation("y * (-x)");
            builder.Build((x, y) => -y * x).AssertSimpleStringRepresentation("(-y) * x");

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
            builder.Build((x, y) => (x ^ 3) / (y ^ 4)).AssertSimpleStringRepresentation("x ^ 3 / y ^ 4");
            builder.Build(x => 1 / (x ^ 3)).AssertSimpleStringRepresentation("1 / x ^ 3");
            builder.Build(x => 2 * x / (3 + x)).AssertSimpleStringRepresentation("(2 * x) / (3 + x)");
            builder.Build((x, y, z) => x / y / z).AssertSimpleStringRepresentation("(x / y) / z");
            builder.Build(x => 1 / (3 * x)).AssertSimpleStringRepresentation("1 / (3 * x)");
        }
        [Test]
        public void ToStringTest2() {
            var x = new ParamExpr("x");
            var y = new ParamExpr("y");
            var z = new ParamExpr("z");
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
            var x = new ParamExpr("x");
            var y = new ParamExpr("y");
            var z = new ParamExpr("z");

            builder.Divide(2, 9).AssertSimpleStringRepresentation("2 / 9");
            builder.Power(builder.Divide(2, 3), 4).AssertSimpleStringRepresentation("(2 / 3) ^ 4");
            builder.Power(builder.Divide(-2, 3), 4).AssertSimpleStringRepresentation("((-2) / 3) ^ 4");
            builder.Divide(builder.Multiply(2, x), builder.Multiply(y, z)).AssertSimpleStringRepresentation("(2 * x) / (y * z)");
        }

        [Test]
        public static void ParamEqualsAndGetHashCode() {
            var expr = (ParamExpr)"a";
            Assert.AreEqual(expr, (ParamExpr)"a");
            Assert.AreNotEqual(expr, (ParamExpr)"b");
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, (ParamExpr)"a");
            AssertHashCodesAreNotEqual(expr, (ParamExpr)"b");

        }
        [Test]
        public void SqrtEqualsAndGetHashCode() {
            var expr = builder.Sqrt((ParamExpr)"a");
            Assert.AreEqual(expr, builder.Sqrt((ParamExpr)"a"));
            Assert.AreNotEqual(expr, ((IBuilder)new CachingBuilder()).Sqrt((ParamExpr)"a"));
            Assert.AreNotEqual(expr, builder.Sqrt((ParamExpr)"b"));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Sqrt((ParamExpr)"a"));
            AssertHashCodesAreNotEqual(expr, builder.Sqrt((ParamExpr)"b"));
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
            Expr expr = builder.Power((ParamExpr)"a", 2);
            Assert.AreEqual(expr, builder.Power((ParamExpr)"a", 2));
            Assert.AreNotEqual(expr, ((IBuilder)new CachingBuilder()).Power((ParamExpr)"a", 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Power((ParamExpr)"a", 2));
            AssertHashCodesAreNotEqual(expr, builder.Power((ParamExpr)"b", 2));
            AssertHashCodesAreNotEqual(expr, builder.Power((ParamExpr)"a", 3));
        }
        [Test]
        public void AddEqualsAndGetHashCode() {
            Expr expr = builder.Add((ParamExpr)"a", 2);
            Assert.AreEqual(expr, builder.Add((ParamExpr)"a", 2));
            Assert.AreNotEqual(expr, ((IBuilder)new CachingBuilder()).Add((ParamExpr)"a", 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Add((ParamExpr)"a", 2));
            AssertHashCodesAreNotEqual(expr, builder.Add((ParamExpr)"b", 2));
            AssertHashCodesAreNotEqual(expr, builder.Add((ParamExpr)"a", 3));
        }
        [Test]
        public void MultEquals() {
            Expr expr = builder.Multiply((ParamExpr)"a", 2);
            Assert.AreEqual(expr, builder.Multiply((ParamExpr)"a", 2));
            Assert.AreNotEqual(expr, ((IBuilder)new CachingBuilder()).Multiply((ParamExpr)"a", 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Multiply((ParamExpr)"a", 2));
            AssertHashCodesAreNotEqual(expr, builder.Multiply((ParamExpr)"b", 2));
            AssertHashCodesAreNotEqual(expr, builder.Multiply((ParamExpr)"a", 3));
        }
        [Test]
        public void DivEquals() {
            Expr expr = builder.Divide((ParamExpr)"a", 2);
            Assert.AreEqual(expr, builder.Divide((ParamExpr)"a", 2));
            Assert.AreNotEqual(expr, ((IBuilder)new CachingBuilder()).Divide((ParamExpr)"a", 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, builder.Divide((ParamExpr)"a", 2));
            AssertHashCodesAreNotEqual(expr, builder.Divide((ParamExpr)"b", 2));
            AssertHashCodesAreNotEqual(expr, builder.Divide((ParamExpr)"a", 3));
        }
        [Test]
        public void HashCodeSalt() {
            AssertHashCodesAreNotEqual((ParamExpr)"b", builder.Sqrt((ParamExpr)"b"));
            AssertHashCodesAreNotEqual(2, builder.Sqrt(2));

            AssertHashCodesAreNotEqual((ParamExpr)"b", builder.Add((ParamExpr)"b"));
            AssertHashCodesAreNotEqual(builder.Multiply((ParamExpr)"b"), builder.Add((ParamExpr)"b"));
            AssertHashCodesAreNotEqual(builder.Divide((ParamExpr)"b", 2), builder.Add((ParamExpr)"b", 2));
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
            var expr = SimpleBuilder.Instance.Add(Param("a"), Param("b"));
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
            var e1 = (AddExpr)builder.Build(x => Sqrt(x + 1) + 1 / (x + 1));
            Assert.AreSame((e1.Args.First() as SqrtExpr).Value, (e1.Args.Last() as DivExpr).Denominator);
            var e2 = builder.Build(x => x + 1);
            Assert.AreSame((e1.Args.First() as SqrtExpr).Value, e2);
            Assert.AreNotSame(e2, new CachingBuilder().Build(x => x + 1));
            Assert.AreNotSame(e2, builder.Build(x => x + 2));
        }
        [Test]
        public void MultExprMemoization() {
            var e = (AddExpr)builder.Build(x => Sqrt(2 * x) + 1 / (2 * x));
            Assert.AreSame((e.Args.First() as SqrtExpr).Value, (e.Args.Last() as DivExpr).Denominator);
        }
        [Test]
        public void SqrtExprMemoization() {
            var e = (AddExpr)builder.Build(x => Sqrt(2 * x) + 1 / Sqrt(2 * x));
            Assert.AreSame(e.Args.First(), (e.Args.Last() as DivExpr).Denominator);
        }
        [Test]
        public void PowerExprMemoization() {
            var e = (AddExpr)builder.Build(x => (x ^ 2) + 1 /(x ^ 2));
            Assert.AreSame(e.Args.First(), (e.Args.Last() as DivExpr).Denominator);
            Assert.AreNotSame(e.Args.First(), builder.Build(x => x ^ 3));
        }
        [Test]
        public void DivExprMemoization() {
            var e = (AddExpr)builder.Build(x => (x / 3) + ((x / 3) ^ 2));
            Assert.AreSame(e.Args.First(), (e.Args.Last() as PowerExpr).Value);
            //Assert.AreNotSame(e.Args.First(), builder.Build(x => x ^ 3));
        }
    }
    public static class ExprTestExtensions {
        public static Expr Build(this IBuilder builder, Expression<Func<Expr, Expr>> f) {
            return builder.Build(f, GetParameters(f).Single());
        }
        public static Expr Build(this IBuilder builder, Expression<Func<Expr, Expr, Expr>> f) {
            var parameters = GetParameters(f);
            return builder.Build(f, parameters[0], parameters[1]);
        }
        public static Expr Build(this IBuilder builder, Expression<Func<Expr, Expr, Expr, Expr>> f) {
            var parameters = GetParameters(f);
            return builder.Build(f, parameters[0], parameters[1], parameters[2]);
        }
        static ExprList GetParameters(LambdaExpression expression) {
            return expression.Parameters.Select(x => (ParamExpr)x.Name).ToImmutableArray<Expr>();
        }
        public static Expr AssertSimpleStringRepresentation(this Expr expr, string str) {
            Assert.AreEqual(str, expr.ToString());
            return expr;
        }
    }
}