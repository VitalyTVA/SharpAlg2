using System;
using System.Numerics;
using Numerics;
using NUnit.Framework;
using SharpAlg.Geo.Core;
using System.Linq.Expressions;
using System.Collections.Immutable;
using System.Linq;
using static SharpAlg.Geo.Core.ExprExtensions;
using static SharpAlg.Geo.Tests.ExprTestExtensions;
using System.Diagnostics;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class ExprTests {
        [Test]
        public void BasicStuffTest() {
            Expr five = 5;
            Assert.AreEqual((BigRational)5, ((ConstExpr)five).Value);

            ParamExpr a = "A";
            Assert.AreEqual("A", a.Name);

            AddExpr a_plus_five = (AddExpr)Build(x => x + 5, a);
            Assert.AreEqual(2, a_plus_five.Args.Length);
            Assert.AreSame(a, a_plus_five.Args[0]);
            Assert.AreEqual((BigRational)5, ((ConstExpr)a_plus_five.Args[1]).Value);

            AddExpr five_minus_a = (AddExpr)Build(x => 5 - x, a);
            Assert.AreEqual(2, five_minus_a.Args.Length);
            Assert.AreEqual((BigRational)5, ((ConstExpr)five_minus_a.Args[0]).Value);
            Assert.AreEqual((BigRational)(-1), ((ConstExpr)(five_minus_a.Args[1] as MultExpr).Args[0]).Value);
            Assert.AreSame(a, (five_minus_a.Args[1] as MultExpr).Args[1]);


            MultExpr five_mult_a = (MultExpr)Build(x => 5 * x, a);
            Assert.AreEqual(2, five_mult_a.Args.Length);
            Assert.AreEqual((BigRational)5, ((ConstExpr)five_mult_a.Args[0]).Value);
            Assert.AreSame(a, five_mult_a.Args[1]);

            SqrtExpr sqrt_a = (SqrtExpr)Build(x => Sqrt(x), a);
            Assert.AreSame(a, sqrt_a.Value);

            PowerExpr a_power_five = (PowerExpr)Build(x => x ^ 5, a);
            Assert.AreSame(a, a_power_five.Value);
            Assert.AreEqual((BigInteger)5, a_power_five.Power);

            DivExpr a_div_five = (DivExpr)Build((x, y) => x / y, a, five);
            Assert.AreSame(a, a_plus_five.Args[0]);

            MultExpr minus_a = (MultExpr)Build(x => -x, a);
            Assert.AreEqual(2, minus_a.Args.Length);
            Assert.AreEqual((BigRational)(-1), ((ConstExpr)minus_a.Args[0]).Value);
            Assert.AreSame(a, minus_a.Args[1]);

            Assert.Throws<PowerShouldBePositiveException>(() => { var t = Build(x => x ^ 0, a); });
            Assert.Throws<PowerShouldBePositiveException>(() => { var t = Build(x => x ^ -1, a); });
            Assert.Throws<InvalidExpressionException>(() => { var t = Build(x => SomeFunc(x), a); });
        }
        static Expr SomeFunc(Expr e) {
            throw new NotImplementedException();
        }
        [Test]
        public void ToStringTest() {
            Build(() => 9).AssertSimpleStringRepresentation("9");
            Build(() => -9).AssertSimpleStringRepresentation("-9");
            Build(x => x).AssertSimpleStringRepresentation("x");
            Build(x => -x).AssertSimpleStringRepresentation("-x");
            Build(x => 9 + x).AssertSimpleStringRepresentation("9 + x");
            Build(x => 9 - x).AssertSimpleStringRepresentation("9 - x");
            Build(x => -(9 - x)).AssertSimpleStringRepresentation("-(9 - x)");
            Build(x => 9 * x).AssertSimpleStringRepresentation("9 * x");
            Build((x, y, z) => x + y * z).AssertSimpleStringRepresentation("x + y * z");
            Build((x, y, z) => (x + y) * z).AssertSimpleStringRepresentation("(x + y) * z");
            Build((x, y, z) => z * (x + y)).AssertSimpleStringRepresentation("z * (x + y)");
            Build(x => x ^ 4).AssertSimpleStringRepresentation("x ^ 4");
            Build((x, y) => x * (y ^ 3)).AssertSimpleStringRepresentation("x * y ^ 3");
            Build((x, y) => x * y ^ 3).AssertSimpleStringRepresentation("(x * y) ^ 3");
            Build(x => x ^ 2 ^ 3).AssertSimpleStringRepresentation("(x ^ 2) ^ 3");
            Build((x, y, z) => x + y + z).AssertSimpleStringRepresentation("x + y + z");
            Build((x, y, z) => x - y - z).AssertSimpleStringRepresentation("x - y - z");
            Build((x, y) => 1 + 2 * x + 3 * y).AssertSimpleStringRepresentation("1 + 2 * x + 3 * y");
            Build((x, y) => (x + 1) ^ (2 * 3)).AssertSimpleStringRepresentation("(x + 1) ^ 6");

            Build(x => 9 - (-x)).AssertSimpleStringRepresentation("9 - (-x)");
            Build(x => 9 * (-x)).AssertSimpleStringRepresentation("9 * (-x)");
            Build(x => x * (-1)).AssertSimpleStringRepresentation("x * (-1)");

            Build(x => (-2) * x).AssertSimpleStringRepresentation("-2 * x");
            Build(x => -2 * (x + 1)).AssertSimpleStringRepresentation("-2 * (x + 1)");
            Build((x, y) => -x + y).AssertSimpleStringRepresentation("-x + y");

            Build((x, y) => y * (-x)).AssertSimpleStringRepresentation("y * (-x)");
            Build((x, y) => -y * x).AssertSimpleStringRepresentation("(-y) * x");


            Build((x, y) =>Sqrt(x + y)).AssertSimpleStringRepresentation("sqrt(x + y)");
        }
        [Test]
        public void ToStringTest_Div() {
            Build(x => 9 / x).AssertSimpleStringRepresentation("9 / x");
            Build((x, y, z) => x / y / z).AssertSimpleStringRepresentation("(x / y) / z");
            Build(x => 9 / (1 / x)).AssertSimpleStringRepresentation("9 / (1 / x)");
            Build(x => 9 + 1 / x).AssertSimpleStringRepresentation("9 + 1 / x");
            Build(x => 9 - (1 / x)).AssertSimpleStringRepresentation("9 - 1 / x");
            Build(x => 9 / (-x)).AssertSimpleStringRepresentation("9 / (-x)");
            Build(x => 1 / (3 + x)).AssertSimpleStringRepresentation("1 / (3 + x)");
            Build(x => (2 + x) / (3 + x)).AssertSimpleStringRepresentation("(2 + x) / (3 + x)");
            Build(x => 2 * x / (3 + x)).AssertSimpleStringRepresentation("(2 * x) / (3 + x)");
            Build((x, y, z) => 2 * x / (y * z)).AssertSimpleStringRepresentation("(2 * x) / (y * z)");
            Build((x, y) => (x ^ 3) / (y ^ 4)).AssertSimpleStringRepresentation("x ^ 3 / y ^ 4");
            Build(x => 1 / (x ^ 3)).AssertSimpleStringRepresentation("1 / x ^ 3");
            Build(x => 2 * x / (3 + x)).AssertSimpleStringRepresentation("(2 * x) / (3 + x)");
            Build((x, y, z) => x / y / z).AssertSimpleStringRepresentation("(x / y) / z");
            Build(x => 1 / (3 * x)).AssertSimpleStringRepresentation("1 / (3 * x)");
        }
        [Test]
        public void ToStringTest2() {
            var x = new ParamExpr("x");
            var y = new ParamExpr("y");
            var z = new ParamExpr("z");
            Multiply(-1, x, y).AssertSimpleStringRepresentation("-x * y");
            Multiply(-2, x, y).AssertSimpleStringRepresentation("-2 * x * y");
            Multiply(Const(new BigRational(2, 3)), x, y).AssertSimpleStringRepresentation("2/3 * x * y");
            Multiply(Const(new BigRational(-2, 3)), x, y).AssertSimpleStringRepresentation("-2/3 * x * y");
            Add(Const(new BigRational(2, 3)), x, y).AssertSimpleStringRepresentation("2/3 + x + y");
            Const(new BigRational(2, 9)).AssertSimpleStringRepresentation("2/9");
            Power(Const(new BigRational(2, 3)), 4).AssertSimpleStringRepresentation("(2/3) ^ 4");
            Power(Const(new BigRational(-2, 3)), 4).AssertSimpleStringRepresentation("(-2/3) ^ 4");

            Add(z, Multiply(-1, x, y)).AssertSimpleStringRepresentation("z - x * y");
            Add(z, Multiply(-2, x, y)).AssertSimpleStringRepresentation("z - 2 * x * y");
            Power(Multiply(-1, x, y), 2).AssertSimpleStringRepresentation("(-x * y) ^ 2");
        }
        [Test]
        public void ToStringTest2_Div() {
            var x = new ParamExpr("x");
            var y = new ParamExpr("y");
            var z = new ParamExpr("z");

            Divide(2, 9).AssertSimpleStringRepresentation("2 / 9");
            Power(Divide(2, 3), 4).AssertSimpleStringRepresentation("(2 / 3) ^ 4");
            Power(Divide(-2, 3), 4).AssertSimpleStringRepresentation("((-2) / 3) ^ 4");
            Divide(Multiply(2, x), Multiply(y, z)).AssertSimpleStringRepresentation("(2 * x) / (y * z)");
        }

        [Test]
        public void ParamEqualsAndGetHashCode() {
            var expr = (ParamExpr)"a";
            Assert.AreNotEqual(expr, (ParamExpr)"a");
            Assert.AreNotEqual(expr, (ParamExpr)"b");
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, (ParamExpr)"a");
            AssertHashCodesAreNotEqual(expr, ((ParamExpr)"b"));

        }
        [Test]
        public void SqrtEqualsAndGetHashCode() {
            var expr = new SqrtExpr((ParamExpr)"a");
            Assert.AreNotEqual(expr, new SqrtExpr((ParamExpr)"a"));
            Assert.AreNotEqual(expr, new SqrtExpr((ParamExpr)"a"));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, new SqrtExpr((ParamExpr)"a"));
            AssertHashCodesAreNotEqual(expr, new SqrtExpr((ParamExpr)"b"));
        }
        [Test]
        public void ConstEqualsAndGetHashCode() {
            Expr expr = 1;
            Assert.AreNotEqual(expr, (Expr)1);
            Assert.AreNotEqual(expr, (Expr)2);
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, ((Expr)1));
            AssertHashCodesAreNotEqual(expr, (Expr)2);
        }
        [Test]
        public void PowerEqualsAndGetHashCode() {
            Expr expr = Power((ParamExpr)"a", 2);
            Assert.AreNotEqual(expr, Power((ParamExpr)"a", 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, Power((ParamExpr)"a", 2));
            AssertHashCodesAreNotEqual(expr, Power((ParamExpr)"baaaa", 2));
            AssertHashCodesAreNotEqual(expr, Power((ParamExpr)"a", 3));
        }
        [Test]
        public void AddEqualsAndGetHashCode() {
            Expr expr = Add((ParamExpr)"a", 2);
            Assert.AreNotEqual(expr, Add((ParamExpr)"a", 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, Add((ParamExpr)"a", 2));
            AssertHashCodesAreNotEqual(expr, Add((ParamExpr)"b", 2));
            AssertHashCodesAreNotEqual(expr, Add((ParamExpr)"a", 3));
        }
        [Test]
        public void MultEquals() {
            Expr expr = Multiply((ParamExpr)"a", 2);
            Assert.AreNotEqual(expr, Multiply((ParamExpr)"a", 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, Multiply((ParamExpr)"a", 2));
            AssertHashCodesAreNotEqual(expr, Multiply((ParamExpr)"b", 2));
            AssertHashCodesAreNotEqual(expr, Multiply((ParamExpr)"a", 3));
        }
        [Test]
        public void DivEquals() {
            Expr expr = Divide((ParamExpr)"a", 2);
            Assert.AreNotEqual(expr, Divide((ParamExpr)"a", 2));
            Assert.AreEqual(expr, expr);

            AssertHashCodesAreEqual(expr, expr);
            AssertHashCodesAreEqual(expr, Divide((ParamExpr)"a", 2));
            AssertHashCodesAreNotEqual(expr, Divide((ParamExpr)"b", 2));
            AssertHashCodesAreNotEqual(expr, Divide((ParamExpr)"a", 3));
        }
        [Test]
        public void HashCodeSalt() {
            AssertHashCodesAreNotEqual<Expr>((ParamExpr)"b", new SqrtExpr((ParamExpr)"b"));
            AssertHashCodesAreNotEqual<Expr>(2, new SqrtExpr(2));

            AssertHashCodesAreNotEqual<Expr>((ParamExpr)"b", Add((ParamExpr)"b"));
            AssertHashCodesAreNotEqual<Expr>(Multiply((ParamExpr)"b"), Add((ParamExpr)"b"));
            AssertHashCodesAreNotEqual<Expr>(Divide((ParamExpr)"b", 2), Add((ParamExpr)"b", 2));
        }
        [DebuggerStepThrough]
        static void AssertHashCodesAreEqual<T>(T a, T b) {
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
        [DebuggerStepThrough]
        static void AssertHashCodesAreNotEqual<T>(T a, T b) {
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
    public static class ExprTestExtensions {
        public static Expr Build(Expression<Func<Expr, Expr>> f) {
            return ExprExtensions.Build(f, GetParameters(f).Single());
        }
        public static Expr Build(Expression<Func<Expr, Expr, Expr>> f) {
            var parameters = GetParameters(f);
            return ExprExtensions.Build(f, parameters[0], parameters[1]);
        }
        public static Expr Build(Expression<Func<Expr, Expr, Expr, Expr>> f) {
            var parameters = GetParameters(f);
            return ExprExtensions.Build(f, parameters[0], parameters[1], parameters[2]);
        }
        static ImmutableArray<Expr> GetParameters(LambdaExpression expression) {
            return expression.Parameters.Select(x => (ParamExpr)x.Name).ToImmutableArray<Expr>();
        }
        public static Expr AssertSimpleStringRepresentation(this Expr expr, string str) {
            Assert.AreEqual(str, expr.ToString());
            return expr;
        }
    }
}