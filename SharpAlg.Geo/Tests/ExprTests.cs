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
            Build(x => 9 / x).AssertSimpleStringRepresentation("9 / x");
            Build((x, y, z) => x + y * z).AssertSimpleStringRepresentation("x + y * z");
            Build((x, y, z) => (x + y) * z).AssertSimpleStringRepresentation("(x + y) * z");
            Build((x, y, z) => z * (x + y)).AssertSimpleStringRepresentation("z * (x + y)");
            Build(x => x ^ 4).AssertSimpleStringRepresentation("x ^ 4");
            Build((x, y) => x * (y ^ 3)).AssertSimpleStringRepresentation("x * y ^ 3");
            Build((x, y) => x * y ^ 3).AssertSimpleStringRepresentation("(x * y) ^ 3");
            Build((x, y, z) => x + y + z).AssertSimpleStringRepresentation("x + y + z");
            Build((x, y, z) => x - y - z).AssertSimpleStringRepresentation("x - y - z");
            Build((x, y, z) => x / y / z).AssertSimpleStringRepresentation("(x / y) / z");
            Build((x, y) => 1 + 2 * x + 3 * y).AssertSimpleStringRepresentation("1 + 2 * x + 3 * y");
            Build((x, y) => (x + 1) ^ (2 * 3)).AssertSimpleStringRepresentation("(x + 1) ^ 6");

            Build(x => 9 - (-x)).AssertSimpleStringRepresentation("9 - (-x)");
            Build(x => 9 / (1 / x)).AssertSimpleStringRepresentation("9 / (1 / x)");
            Build(x => 9 + 1 / x).AssertSimpleStringRepresentation("9 + 1 / x");
            Build(x => 9 * (-x)).AssertSimpleStringRepresentation("9 * (-x)");
            Build(x => 9 - (1 / x)).AssertSimpleStringRepresentation("9 - 1 / x");
            Build(x => 9 / (-x)).AssertSimpleStringRepresentation("9 / (-x)");
            Build(x => x * (-1)).AssertSimpleStringRepresentation("x * (-1)");

            Build(x => x ^ 2 ^ 3).AssertSimpleStringRepresentation("(x ^ 2) ^ 3");
            Build(x => (-2) * x).AssertSimpleStringRepresentation("-2 * x");
            Build(x => -2 * (x + 1)).AssertSimpleStringRepresentation("-2 * (x + 1)");
            Build((x, y) => -x + y).AssertSimpleStringRepresentation("-x + y");
            Build(x => 1 / (3 + x)).AssertSimpleStringRepresentation("1 / (3 + x)");
            Build(x => (2 + x) / (3 + x)).AssertSimpleStringRepresentation("(2 + x) / (3 + x)");
            Build(x => 2 * x / (3 + x)).AssertSimpleStringRepresentation("(2 * x) / (3 + x)");
            Build((x, y, z) => 2 * x / (y * z)).AssertSimpleStringRepresentation("(2 * x) / (y * z)");
            Build((x, y) => (x ^ 3) / (y ^ 4)).AssertSimpleStringRepresentation("x ^ 3 / y ^ 4");

            //"1 / 3 ^ x".Parse().AssertSimpleStringRepresentation("1 / 3 ^ x");
            //"1 / (4 * x)".Parse().AssertSimpleStringRepresentation("1/4 / x");
            //"t * (-x)".Parse().AssertSimpleStringRepresentation("-t * x");
            //"t * (-2) * x".Parse().AssertSimpleStringRepresentation("-2 * t * x");
            //"z + t * (-x)".Parse().AssertSimpleStringRepresentation("z - t * x");
            //"z + t * (-2) * x".Parse().AssertSimpleStringRepresentation("z - 2 * t * x");
            //"(- x * t) ^ z".Parse().AssertSimpleStringRepresentation("(-x * t) ^ z");

            //Expr.Multiply(new Expr[] { ExprTestHelper.AsConstant(2), Expr.Parameter("x"), Expr.Power(Expr.Multiply(Expr.Parameter("y"), Expr.Parameter("z")), Expr.MinusOne) }).AssertSimpleStringRepresentation("2 * x / (y * z)");
            //Expr.Power(Expr.Multiply(ExprTestHelper.AsConstant(3), Expr.Parameter("x")), Expr.MinusOne).AssertSimpleStringRepresentation("1 / (3 * x)");

            //"someFunc(x, x + y, x ^ y)".Parse().AssertSimpleStringRepresentation("someFunc(x, x + y, x ^ y)");

            //"2*x/3*y".Parse().AssertSimpleStringRepresentation("2/3 * x * y");
            //"2*x/(-3)*y".Parse().AssertSimpleStringRepresentation("-2/3 * x * y");
            //"2/3 + y".Parse().AssertSimpleStringRepresentation("2/3 + y");
            //"2/3^2".Parse().AssertSimpleStringRepresentation("2/9");
            //"(-4/6)^2".Parse().AssertSimpleStringRepresentation("4/9");
            //"(2/3)^x".Parse().AssertSimpleStringRepresentation("(2/3) ^ x");
            //"(-2/3)^x".Parse().AssertSimpleStringRepresentation("(-2/3) ^ x");
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