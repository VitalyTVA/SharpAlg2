using Numerics;
using NUnit.Framework;
using SharpAlg.Geo.Core;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class ExprTests {
        [Test]
        public void BasicStuffTest() {
            ConstExpr five = 5;
            Assert.AreEqual((BigRational)5, five.Value);

            ParamExpr a = "A";
            Assert.AreEqual("A", a.Name);

            var a_plus_five = a + five;
            CollectionAssert.AreEqual(new Expr[] { a, five }, a_plus_five.Args);
        }
    }
}