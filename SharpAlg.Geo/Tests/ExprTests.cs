using Numerics;
using NUnit.Framework;
using SharpAlg.Geo.Core;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class ExprTests {
        [Test]
        public void BasicStuffTest() {
            Expr five = 5;
            Assert.AreEqual((BigRational)5, ((ConstExpr)five).Value);

            ParamExpr a = "A";
            Assert.AreEqual("A", a.Name);

            var a_plus_five = a + 5;
            Assert.AreEqual(2, a_plus_five.Args.Length);
            Assert.AreSame(a, a_plus_five.Args[0]);
            Assert.AreEqual((BigRational)5, ((ConstExpr)a_plus_five.Args[1]).Value);
        }
    }
}