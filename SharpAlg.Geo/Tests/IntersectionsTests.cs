using NUnit.Framework;
using RealPoint = System.Windows.Point;
using static SharpAlg.Geo.Core.ExprExtensions;
using SharpAlg.Geo.Core;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class IntersectionsTests : ExprFunctionalTestsBase {
        [Test]
        public void LinesIntersetions() {
            var A = builder.MakePoint('A');
            var B = builder.MakePoint('B');
            var C = builder.MakePoint('C');
            var D = builder.MakePoint('D');
            var m = builder.MakeLine(A, B);
            var n = builder.MakeLine(C, D);
            var X = builder.IntersectLines(m, n);
            var context = ImmutableContext.Empty
                .RegisterPoint(A, 2, 1)
                .RegisterPoint(B, 8, 4)
                .RegisterPoint(C, 3, 5)
                .RegisterPoint(D, 5, -1); 
            Assert.AreEqual(new RealPoint(4, 2), X.ToRealPoint(context));
        }
        [Test]
        public void LineAndCircleIntersection1() {
            AssertLineAndCircleIntersection(
                new RealPoint(-9, 5),
                new RealPoint(12, 8),
                new RealPoint(2, 3),
                new RealPoint(6, 6),
                new RealPoint(5, 7),
                new RealPoint(-2, 6)
            );
        }
        [Test]
        public void LineAndCircleIntersection2() {
            AssertLineAndCircleIntersection(
                new RealPoint(3, 4),
                new RealPoint(6, 0),
                new RealPoint(3, 4),
                new RealPoint(6, 0),
                new RealPoint(6, 0),
                new RealPoint(0, 8)
            );
        }
        void AssertLineAndCircleIntersection(RealPoint a, RealPoint b, RealPoint c, RealPoint d, RealPoint x1, RealPoint x2) {
            var A = builder.MakePoint('A');
            var B = builder.MakePoint('B');
            var C = builder.MakePoint('C');
            var D = builder.MakePoint('D');
            var line = builder.MakeLine(A, B);
            var circle = builder.MakeCircle(C, D);
            var X = builder.IntersectLineAndCircle(line, circle);
            var context = ImmutableContext.Empty
                .RegisterPoint(A, a.X, a.Y)
                .RegisterPoint(B, b.X, b.Y)
                .RegisterPoint(C, c.X, c.Y)
                .RegisterPoint(D, d.X, d.Y);
            AssertHelper.ArePointsEqual(x1, X.Item1.ToRealPoint(context));
            AssertHelper.ArePointsEqual(x2, X.Item2.ToRealPoint(context));
        }
        [Test]
        public void CirclesIntersection1() {
            AssertCirclesIntersection(
                new RealPoint(0, 0),
                new RealPoint(0, 5),
                new RealPoint(7, 7),
                new RealPoint(7, 2),
                new RealPoint(4, 3),
                new RealPoint(3, 4)
            );
        }
        [Test]
        public void CirclesIntersection2() {
            AssertCirclesIntersection(
                new RealPoint(0, 0),
                new RealPoint(0, 5),
                new RealPoint(8, 0),
                new RealPoint(3, 0),
                new RealPoint(4, -3),
                new RealPoint(4, 3)
            );
        }
        [Test]
        public void CirclesIntersection3() {
            AssertCirclesIntersection(
                new RealPoint(0 + 1, 0 + 2),
                new RealPoint(0 + 1, 5 + 2),
                new RealPoint(7 + 1, 7 + 2),
                new RealPoint(7 + 1, 2 + 2),
                new RealPoint(4 + 1, 3 + 2),
                new RealPoint(3 + 1, 4 + 2)
            );
        }
        [Test]
        public void CirclesIntersection4() {
            AssertCirclesIntersection(
                new RealPoint(-1, 2),
                new RealPoint(-1, -2),
                new RealPoint(3, -1),
                new RealPoint(3, 4),
                new RealPoint(2.4796363335788034, -1.8928484447717376042),
                new RealPoint(-1.9196363335788034, 3.9728484447717376042)
            );
        }
        void AssertCirclesIntersection(RealPoint a, RealPoint b, RealPoint c, RealPoint d, RealPoint x1, RealPoint x2) {
            var A = builder.MakePoint('A');
            var B = builder.MakePoint('B');
            var C = builder.MakePoint('C');
            var D = builder.MakePoint('D');
            var c1 = builder.MakeCircle(A, B);
            var c2 = builder.MakeCircle(C, D);
            var X = builder.IntersectCircles(c1, c2);
            var context = ImmutableContext.Empty
                .RegisterPoint(A, a.X, a.Y)
                .RegisterPoint(B, b.X, b.Y)
                .RegisterPoint(C, c.X, c.Y)
                .RegisterPoint(D, d.X, d.Y);
            AssertHelper.ArePointsEqual(x1, X.Item1.ToRealPoint(context));
            AssertHelper.ArePointsEqual(x2, X.Item2.ToRealPoint(context));
        }
        //int RootOf(int x) {
        //    return x;
        //}
        //[Test]
        //public void LineAndCircleIntersection2() {
        //    var A = Point.FromValues(-9, 5);
        //    var B = Point.FromValues(12, 8);
        //    var C = Point.FromValues(2, 3);
        //    var D = Point.FromValues(6, 6);
        //    var l = Line.FromPoints(A, B).With(x => new Line(Expr.Constant(x.A.Evaluate()), Expr.Constant(x.B.Evaluate()), Expr.Constant(x.C.Evaluate())));
        //    var c = builder.FromPoints(C, D).With(x => new Circle(Expr.Constant(x.X.Evaluate()), Expr.Constant(x.Y.Evaluate()), Expr.Constant(x.R.Evaluate())));
        //    var X = l.Intersect(c);
        //    var context = ContextFactory.CreateEmpty();
        //    Assert.AreEqual(new RealPoint(5, 7), X.Item1.ToRealPoint(context));
        //}
        [Test]
        public void QuadraticEquation() {

            var roots =  builder.SolveQuadraticEquation(Param("A"), Param("B"), Param("C"));
            var context = ImmutableContext.Empty
                .Register("A", 1)
                .Register("B", -2)
                .Register("C", -3);
            Assert.AreEqual(3, roots.Item1.ToReal(context));
            Assert.AreEqual(-1, roots.Item2.ToReal(context));

            context = ImmutableContext.Empty
                .Register("A", 2)
                .Register("B", -4)
                .Register("C", -6);
            Assert.AreEqual(3, roots.Item1.ToReal(context));
            Assert.AreEqual(-1, roots.Item2.ToReal(context));

            context = ImmutableContext.Empty
                .Register("A", -2)
                .Register("B", 4)
                .Register("C", 6);
            Assert.AreEqual(-1, roots.Item1.ToReal(context));
            Assert.AreEqual(3, roots.Item2.ToReal(context));

            context = ImmutableContext.Empty
                .Register("A", 1)
                .Register("B", 1)
                .Register("C", 1);
            Assert.IsNaN(roots.Item1.ToReal(context));
            Assert.IsNaN(roots.Item2.ToReal(context));

            roots = builder.SolveQuadraticEquation(builder.Build(X => X + 1), builder.Build(Y => Y -2), builder.Build(Z => Z / 2));
            context = ImmutableContext.Empty
                .Register("X", 0)
                .Register("Y", 0)
                .Register("Z", -6);
            Assert.AreEqual(3, roots.Item1.ToReal(context));
            Assert.AreEqual(-1, roots.Item2.ToReal(context));
        }
        [Test, Ignore]
        public void Middle1() {
            var p1 = new Point(0, 0);
            var p2 = new Point(0, Param("a"));
            var l1 = builder.MakeLine(p1, p2);
            Assert.AreEqual("-a * x", l1.ToString());

            var c1 = builder.MakeCircle(p1, p2);
            var c2 = builder.MakeCircle(p2, p1);
            Assert.AreEqual("x ^ 2 + y ^ 2 - a ^ 2", c1.ToString());
            Assert.AreEqual("x ^ 2 + (y - a) ^ 2 - a ^ 2", c2.ToString());

            var c1_c2 = builder.IntersectCircles(c1, c2);
            Assert.AreEqual("(1/8 * (48 * a ^ 6) ^ (1/2) * a ^ (-2), 1/2 * a)", c1_c2.Item1.ToString());
            Assert.AreEqual("(-1/8 * (48 * a ^ 6) ^ (1/2) * a ^ (-2), 1/2 * a)", c1_c2.Item2.ToString());
            var l2 = builder.MakeLine(c1_c2.Item1, c1_c2.Item2);

            var l1_l2 = builder.IntersectLines(l1, l2);
            Assert.AreEqual("(0, 1/2 * a)", l1_l2.ToString()); 
        }
        [Test, Ignore]
        public void Middle2() {
            var p1 = new Point(Param("b"), 0);
            var p2 = new Point(0, Param("a"));
            var l1 = builder.MakeLine(p1, p2);
            Assert.AreEqual("-a * x - b * y + b * a", l1.ToString());

            var c1 = builder.MakeCircle(p1, p2);
            var c2 = builder.MakeCircle(p2, p1);
            Assert.AreEqual("(x - b) ^ 2 + y ^ 2 - b ^ 2 - a ^ 2", c1.ToString());
            Assert.AreEqual("x ^ 2 + (y - a) ^ 2 - b ^ 2 - a ^ 2", c2.ToString());

            var c1_c2 = builder.IntersectCircles(c1, c2);
            //Assert.AreEqual("(1/8 * 48 ^ (1/2) * a, 1/2 * a)", c1_c2.Item1.ToString());
            //Assert.AreEqual("(-1/8 * 48 ^ (1/2) * a, 1/2 * a)", c1_c2.Item2.ToString());
            var l2 = builder.MakeLine(c1_c2.Item1, c1_c2.Item2);

            //var l1_l2 = 
                builder.IntersectLines(l1, l2);
            //Assert.AreEqual("(0, 1/2 * a)", l1_l2.ToString());
        }
        [Test]
        public void LongIntersectionsSequence() {
            var a = Param("A");
            var b = Param("B");
            var c = Param("C");
            var x = Param("X");
            var y = Param("Y");
            var r = Param("R");

            var center = new Point(x, y);
            var line = new Line(builder, a, b, c);
            const int iterations = 42;
            for(int i = 0; i < iterations; i++) {
                center = builder.IntersectLineAndCircle(line, new Circle(builder, center.X, center.Y, r)).Item1;
            }
            Assert.IsFalse(center.X.IsNormal());
            Assert.IsFalse(center.Y.IsNormal());
            var context = ImmutableContext.Empty
                .RegisterValue(a, -3.0d / 4.0d)
                .RegisterValue(b, 1)
                .RegisterValue(c, -2)
                .RegisterValue(x, 4)
                .RegisterValue(y, 5)
                .RegisterValue(r, 25);
            Assert.AreEqual(new RealPoint(4 + iterations * 4, 5 + iterations * 3), center.ToRealPoint(context));
        }
    }

    //Rewriter/Convolute tests
    //Convoulte test/refactoring (Functor)
    //"(2 * x^2) ^ (1 / 2)".Parse().AssertSimpleStringRepresentation("2 ^ (1/2) * x");
    //(-1/8 * (48 * a ^ 6) ^ (1/2) * a ^ (-2), 1/2 * a)
    //-1/8 * 48 ^ (1/2)
    //Middle1/Middle2 duplicated code
    //Test GetMidpoint, TangentBetween, CotangentBetween
    //Test Convolute in LineCircleIntersector
    //IFunctor
    public static class AssertHelper {
        public const double Delta = 0.0000000001;
        public static void ArePointsEqual(RealPoint p1, RealPoint p2, double delta = Delta) {
            Assert.AreEqual(p1.X, p2.X, delta);
            Assert.AreEqual(p1.Y, p2.Y, delta);        
        }
    }
}