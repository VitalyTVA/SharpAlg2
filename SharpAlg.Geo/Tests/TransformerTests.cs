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
        public void ElementaryConvolutionMult() {
            builder.Build(x => (Expr)1 * 1).AssertSimpleStringRepresentation("1");
            builder.Build(x => x * 1).AssertSimpleStringRepresentation("x");
            builder.Build(x => 1 * x).AssertSimpleStringRepresentation("x");
            builder.Build((x, y) => x * (y * 1)).AssertSimpleStringRepresentation("x * y");
            builder.Build((x, y) => x * (1 * y)).AssertSimpleStringRepresentation("x * y");

            builder.Build((x, y) => x * 2 * y * 3).AssertSimpleStringRepresentation("6 * x * y");
            builder.Build(x => x * 0).AssertSimpleStringRepresentation("0");
            builder.Build(x => 0 * x).AssertSimpleStringRepresentation("0");
        }
        [Test]
        public void ElementaryConvolutionPower() {
            builder.Build(x => x ^ 1).AssertSimpleStringRepresentation("x");
            builder.Build(x => (Expr)3 ^ 2).AssertSimpleStringRepresentation("9");

            builder.Build(x => (-x) ^ 2).AssertSimpleStringRepresentation("x ^ 2");
            builder.Build(x => (-5 * x) ^ 2).AssertSimpleStringRepresentation("25 * (x ^ 2)");
            builder.Build((x, y) => (-x * y) ^ 4).AssertSimpleStringRepresentation("(x ^ 4) * (y ^ 4)");
            builder.Build((x, y) => (-x * y) ^ 3).AssertSimpleStringRepresentation("-(x ^ 3) * (y ^ 3)");
        }
        [Test]
        public void ElementaryConvolutionAdd() {
            builder.Build(x => (Expr)0 + 0).AssertSimpleStringRepresentation("0");
            builder.Build(x => x + 0).AssertSimpleStringRepresentation("x").ToParam();
            builder.Build(x => x - 0).AssertSimpleStringRepresentation("x").ToParam();
            builder.Build(x => x + 2 - 2).AssertSimpleStringRepresentation("x").ToParam();
            builder.Build(x => 0 + x).AssertSimpleStringRepresentation("x");
            builder.Build(x => -0 + x).AssertSimpleStringRepresentation("x");
            builder.Build((x, y) => x  + (y + 0)).AssertSimpleStringRepresentation("x + y");
            builder.Build((x, y) => x + (0 + y)).AssertSimpleStringRepresentation("x + y");
            builder.Build((x, y) => x + 2 + y + 3).AssertSimpleStringRepresentation("5 + x + y");
        }
        [Test]
        public void Mult() {
            builder.Build((x, y, z) => (x / y) * z).AssertSimpleStringRepresentation("(x * z) / y");
            builder.Build((x, y, z) => x * (y / z)).AssertSimpleStringRepresentation("(x * y) / z");
            builder.Build((x, y, z, w) => (x / y) * (z / w)).AssertSimpleStringRepresentation("(x * z) / (y * w)");
        }
        [Test]
        public void Div() {
            Assert.AreEqual(3, builder.Build((x, y, z) => x + y + z).ToAdd().Length);
            Assert.AreEqual(3, builder.Build((x, y, z) => x * y * z).ToMult().Length);

            builder.Build(x => x / 1).AssertSimpleStringRepresentation("x");
            builder.Build((x, y) => (x + y) / 1).AssertSimpleStringRepresentation("x + y");
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