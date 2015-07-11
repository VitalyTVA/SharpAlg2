using System;
using System.Numerics;
using Numerics;
using NUnit.Framework;
using SharpAlg.Geo.Core;
using static SharpAlg.Geo.Core.ExprExtensions;

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
        }
    }
}