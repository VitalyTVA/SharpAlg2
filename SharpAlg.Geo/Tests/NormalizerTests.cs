using NUnit.Framework;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class NormalizerTests : ExprTestsBase {
        [Test]
        public void IsNormal() {
            builder.AssertIsNormal(true, x => 1);
            builder.AssertIsNormal(true, x => x);
        }
        
    }
}