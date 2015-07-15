using NUnit.Framework;
using RealPoint = System.Windows.Point;
using static SharpAlg.Geo.Core.ExprExtensions;
using static SharpAlg.Geo.Tests.ExprTestExtensions;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class IntersectionsTests {
        [Test]
        public void LinesIntersetions() {
            var A = Point.FromName('A');
            var B = Point.FromName('B');
            var C = Point.FromName('C');
            var D = Point.FromName('D');
            var m = Line.FromPoints(A, B);
            var n = Line.FromPoints(C, D);
            var X = m.Intersect(n);
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
            var A = Point.FromName('A');
            var B = Point.FromName('B');
            var C = Point.FromName('C');
            var D = Point.FromName('D');
            var line = Line.FromPoints(A, B);
            var circle = Circle.FromPoints(C, D);
            var X = line.Intersect(circle);
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
            var A = Point.FromName('A');
            var B = Point.FromName('B');
            var C = Point.FromName('C');
            var D = Point.FromName('D');
            var c1 = Circle.FromPoints(A, B);
            var c2 = Circle.FromPoints(C, D);
            var X = c1.Intersect(c2);
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
        //    var c = Circle.FromPoints(C, D).With(x => new Circle(Expr.Constant(x.X.Evaluate()), Expr.Constant(x.Y.Evaluate()), Expr.Constant(x.R.Evaluate())));
        //    var X = l.Intersect(c);
        //    var context = ContextFactory.CreateEmpty();
        //    Assert.AreEqual(new RealPoint(5, 7), X.Item1.ToRealPoint(context));
        //}
        [Test]
        public void QuadraticEquation() {

            var roots = QuadraticEquationHelper.Solve((Core.ParamExpr)"A", (Core.ParamExpr)"B", (Core.ParamExpr)"C");
            var context = ImmutableContext.Empty
                .RegisterValue("A", 1)
                .RegisterValue("B", -2)
                .RegisterValue("C", -3);
            Assert.AreEqual(3, roots.Item1.ToReal(context));
            Assert.AreEqual(-1, roots.Item2.ToReal(context));

            context = ImmutableContext.Empty
                .RegisterValue("A", 2)
                .RegisterValue("B", -4)
                .RegisterValue("C", -6);
            Assert.AreEqual(3, roots.Item1.ToReal(context));
            Assert.AreEqual(-1, roots.Item2.ToReal(context));

            roots = QuadraticEquationHelper.Solve(Build(X => X + 1), Build(Y => Y -2), Build(Z => Z / 2));
            context = ImmutableContext.Empty
                .RegisterValue("X", 0)
                .RegisterValue("Y", 0)
                .RegisterValue("Z", -6);
            Assert.AreEqual(3, roots.Item1.ToReal(context));
            Assert.AreEqual(-1, roots.Item2.ToReal(context));
        }
        [Test, Ignore]
        public void Middle1() {
            var p1 = new Point(0, 0);
            var p2 = new Point(0, Param("a"));
            var l1 = Line.FromPoints(p1, p2);
            Assert.AreEqual("-a * x", l1.ToString());

            var c1 = Circle.FromPoints(p1, p2);
            var c2 = Circle.FromPoints(p2, p1);
            Assert.AreEqual("x ^ 2 + y ^ 2 - a ^ 2", c1.ToString());
            Assert.AreEqual("x ^ 2 + (y - a) ^ 2 - a ^ 2", c2.ToString());

            var c1_c2 = c1.Intersect(c2);
            Assert.AreEqual("(1/8 * (48 * a ^ 6) ^ (1/2) * a ^ (-2), 1/2 * a)", c1_c2.Item1.ToString());
            Assert.AreEqual("(-1/8 * (48 * a ^ 6) ^ (1/2) * a ^ (-2), 1/2 * a)", c1_c2.Item2.ToString());
            var l2 = Line.FromPoints(c1_c2.Item1, c1_c2.Item2);

            var l1_l2 = l1.Intersect(l2);
            Assert.AreEqual("(0, 1/2 * a)", l1_l2.ToString()); 
        }
        [Test, Ignore]
        public void Middle2() {
            var p1 = new Point(Param("b"), 0);
            var p2 = new Point(0, Param("a"));
            var l1 = Line.FromPoints(p1, p2);
            Assert.AreEqual("-a * x - b * y + b * a", l1.ToString());

            var c1 = Circle.FromPoints(p1, p2);
            var c2 = Circle.FromPoints(p2, p1);
            Assert.AreEqual("(x - b) ^ 2 + y ^ 2 - b ^ 2 - a ^ 2", c1.ToString());
            Assert.AreEqual("x ^ 2 + (y - a) ^ 2 - b ^ 2 - a ^ 2", c2.ToString());

            var c1_c2 = c1.Intersect(c2);
            //Assert.AreEqual("(1/8 * 48 ^ (1/2) * a, 1/2 * a)", c1_c2.Item1.ToString());
            //Assert.AreEqual("(-1/8 * 48 ^ (1/2) * a, 1/2 * a)", c1_c2.Item2.ToString());
            var l2 = Line.FromPoints(c1_c2.Item1, c1_c2.Item2);

            var l1_l2 = l1.Intersect(l2);
            //Assert.AreEqual("(0, 1/2 * a)", l1_l2.ToString());
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