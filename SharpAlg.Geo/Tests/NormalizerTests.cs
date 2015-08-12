using NUnit.Framework;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class NormalizerTests : ExprTestsBase {
        [Test]
        public void IsNormal_Trivial() {
            builder.AssertIsNormal(true, x => 1);
            builder.AssertIsNormal(true, x => x);
        }
        [Test]
        public void IsNormal_Product() {
            builder.AssertIsNormal(true, x => 1);
            builder.AssertIsNormal(true, x => x);

            builder.AssertIsNormal(true, x => 2 * x);
            builder.AssertIsNormal(false, x => x * 2);
            builder.AssertIsNormal(true, (x, y) => x * y);
            builder.AssertIsNormal(false, (x, y) => x * (y + 1));
            //builder.AssertIsNormal(false, (x, y) => y * x);
        }

    }
}