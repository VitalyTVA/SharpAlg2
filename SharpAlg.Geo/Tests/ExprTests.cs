﻿using System.Numerics;
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

            AddExpr a_plus_five = a + 5;
            Assert.AreEqual(2, a_plus_five.Args.Length);
            Assert.AreSame(a, a_plus_five.Args[0]);
            Assert.AreEqual((BigRational)5, ((ConstExpr)a_plus_five.Args[1]).Value);

            MultExpr five_mult_a = 5 * a;
            Assert.AreEqual(2, five_mult_a.Args.Length);
            Assert.AreEqual((BigRational)5, ((ConstExpr)five_mult_a.Args[0]).Value);
            Assert.AreSame(a, five_mult_a.Args[1]);

            SqrtExpr sqrt_a = Sqrt(a);
            Assert.AreSame(a, sqrt_a.Value);

            PowerExpr a_power_five = a ^ 5;
            Assert.AreSame(a, a_power_five.Value);
            Assert.AreEqual((BigInteger)5, a_power_five.Power);
        }
    }
}