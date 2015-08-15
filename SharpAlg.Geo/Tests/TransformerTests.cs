using NUnit.Framework;
using SharpAlg.Geo.Core;
using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class SingleDivTransfomerTests : ExprTestsBase {
        protected override Builder CreateBuilder() {
            return Builder.CreateRealLife();
        }
        [Test]
        public void ElementaryConvolution() {
            builder.Build(x => x * 1).AssertSimpleStringRepresentation("x");
            builder.Build(x => 1 * x).AssertSimpleStringRepresentation("x");
            builder.Build((x, y) => x * (y * 1)).AssertSimpleStringRepresentation("x * y");
            builder.Build((x, y) => x * (1 * y)).AssertSimpleStringRepresentation("x * y");

            builder.Build(x => x  + 0).AssertSimpleStringRepresentation("x");
            builder.Build(x => 0 + x).AssertSimpleStringRepresentation("x");
            builder.Build((x, y) => x  + (y + 0)).AssertSimpleStringRepresentation("x + y");
            builder.Build((x, y) => x + (0 + y)).AssertSimpleStringRepresentation("x + y");
        }
        [Test]
        public void SingleDiv() {
            Assert.AreEqual(3, builder.Build((x, y, z) => x + y + z).ToAdd().Length);
            Assert.AreEqual(3, builder.Build((x, y, z) => x * y * z).ToMult().Length);

            builder.Build((x, y, z) => x / y).AssertSimpleStringRepresentation("x / y");
            builder.Build((x, y, z) => x / (y / z)).AssertSimpleStringRepresentation("(x * z) / y");
            builder.Build((x, y, z) => (x / y) / z).AssertSimpleStringRepresentation("x / (y * z)");
            builder.Build((x, y, z, w) => (x / y) / (z / w)).AssertSimpleStringRepresentation("(x * w) / (y * z)");
            builder.Build((x, y, z, w) => x / (y / (z * w))).AssertSimpleStringRepresentation("(x * z * w) / y");
            builder.Build((x, y, z, w) => (x / (y * w)) / z).AssertSimpleStringRepresentation("x / (y * w * z)");
            builder.Build((x, y, z, w, v) => (x / y) / (z / (w * v))).AssertSimpleStringRepresentation("(x * w * v) / (y * z)");
            builder.Build((x, y, z, w, v) => (x / y) / ((z * w) / v)).AssertSimpleStringRepresentation("(x * v) / (y * z * w)");

            builder.Build((x, y) => Sqrt(x / y)).AssertSimpleStringRepresentation("sqrt(x / y)");
        }
    }
}