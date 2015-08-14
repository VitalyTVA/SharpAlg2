using NUnit.Framework;
using SharpAlg.Geo.Core;
//using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class SingleDivTransfomerTests : ExprTestsBase {
        protected override Builder CreateBuilder() {
            return Builder.CreateRealLife(Transformer.SingleDiv);
        }
    }
}