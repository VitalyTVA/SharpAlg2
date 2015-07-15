using NUnit.Framework;
using System.Linq;
using SharpAlg.Native;
using SharpAlg.Native.Builder;
using System.Windows;
using RealPoint = System.Windows.Point;
using NewExpr = SharpAlg.Geo.Core.Expr;
using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class ProblemsTests {
        [Test, Explicit]
        public void GetAllMapleCommand() {
            var total = string.Concat(new[] {
                MiddleOfLineSegment_Maple(),
                AngleBisection_Maple(),
                Perpendicular_Maple(),
                Perpendicular2_Maple(),
            }.Select(x => "restart;" + x + "\r\n\r\n\r\n"));
        }
        #region middle
        [Test]
        public void MiddleOfLineSegment_Calc() {
            var x = Point.FromName('X');
            var y = Point.FromName('Y');
            var point = GetMiddleOfLineSegmentZeroAssertion(x, y);
            AssertHelper.ArePointsEqual(new RealPoint(0, 0), point.ToRealPoint(ImmutableContext.Empty.RegisterPoint(x, 1, 2).RegisterPoint(y, 5, 9)));
        }
        string MiddleOfLineSegment_Maple() {
            var res = GetMiddleOfLineSegmentZeroAssertion(Point.FromName('X'), Point.FromName('Y'));
            return string.Format("simplify({0}); simplify({1});", res.X, res.Y);
        }
        Point GetMiddleOfLineSegmentZeroAssertion(Point p1, Point p2) {
            var l1 = Line.FromPoints(p1, p2);

            var c1 = Circle.FromPoints(p1, p2);
            var c2 = Circle.FromPoints(p2, p1);

            var c1_c2 = c1.Intersect(c2);
            var l2 = Line.FromPoints(c1_c2.Item1, c1_c2.Item2);

            var l1_l2 = l1.Intersect(l2);

            var expected = ExprHelper.Middle(p1, p2);
            return l1_l2.Offset(expected.Invert());

        }
        #endregion

        #region angle bisector
        [Test]
        public void AngleBisection_Calc() {
            var a = Point.FromName('A');
            var b = Point.FromName('B');
            var c = Point.FromName('C');
            var value = GetAngleBisectionZeroAssertion(a, b, c);
            Assert.AreEqual(0, value.ToReal(ImmutableContext.Empty.RegisterPoint(a, 1, 2).RegisterPoint(b, 5, 9).RegisterPoint(c, 3, 7)), AssertHelper.Delta);
        }
        string AngleBisection_Maple() {
            var res = GetAngleBisectionZeroAssertion(Point.FromName('A'), Point.FromName('B'), Point.FromName('C'));
            //var res = GetAngleBisectionZeroAssertion(new Point(Expr.Zero, Expr.Zero), Point.FromName('B'), Point.FromName('C'));
            return string.Format("simplify({0});", res);
            //Clipboard.SetText(mappleCommand);
        }
        NewExpr GetAngleBisectionZeroAssertion(Point A, Point B, Point C) {
            var l1 = Line.FromPoints(A, B);
            var l2 = Line.FromPoints(A, C);

            var c = Circle.FromPoints(A, C);
            var c_l2 = l1.Intersect(c).Item1;

            var middle = ExprHelper.Middle(C, c_l2);//TODO make real
            var bisectrissa = Line.FromPoints(A, middle);

            return Add(LinesOperations.TangentBetween(l1, bisectrissa), LinesOperations.TangentBetween(l2, bisectrissa));
        }
        #endregion

        #region perpendicular
        [Test]
        public void Perpendicular_Calc() {
            var a = Point.FromName('A');
            var b = Point.FromName('B');
            var c = Point.FromName('C');
            var value = GetPerpendicularZeroAssertion(a, b, c);
            Assert.AreEqual(0, value.ToReal(ImmutableContext.Empty.RegisterPoint(a, 1, 2).RegisterPoint(b, 5, 9).RegisterPoint(c, 3, 7)), AssertHelper.Delta);
        }
        string Perpendicular_Maple() {
            //var res = GetPerpendocularZeroAssertion(new Point(Expr.Zero, Expr.Zero), Point.FromName('B'), Point.FromName('C'));
            var res = GetPerpendicularZeroAssertion(new Point(0, 0), new Point(-1, 0), Point.FromName('C'));
            return string.Format(@"
assume(Cx>0);
assume(Cy>0);
simplify({0});
", res);
        }
        NewExpr GetPerpendicularZeroAssertion(Point A, Point B, Point C) {
            var l1 = Line.FromPoints(A, B);

            var c = Circle.FromPoints(C, A);

            var D = l1.Intersect(c).Item1; //????
            var l2 = Line.FromPoints(C, D);
            var E = l2.Intersect(c).Item2; //????1

            var l3 = Line.FromPoints(E, A);

            var cotangent = LinesOperations.CotangentBetween(l1, l3);
            return cotangent;

        } 
        #endregion

        #region perpendicular
        [Test]
        public void Perpendicular2_Calc() {
            var res = GetPerpendocularZeroAssertion2(new Line(0, 1, 4), new Circle(0, 0, 25));
            Assert.AreEqual(0, res.ToReal(ImmutableContext.Empty));
            //Assert.IsTrue(res.ExprEquals(Expr.Zero));
        }
        [Test]
        public void Perpendicular2_Calc2() {
            var res = GetPerpendocularZeroAssertion2(new Line(0, 1, 4), new Circle(0, 0, 25));
            Assert.AreEqual(0, res.ToReal(ImmutableContext.Empty));
        }
        string Perpendicular2_Maple() {
            //var res = GetPerpendocularZeroAssertion2(new Line(Expr.Parameter("k"), Expr.One, Expr.Parameter("b")), new Circle(Expr.Zero, Expr.Zero, Expr.Parameter("R")));
            var res = GetPerpendocularZeroAssertion2(new Line(0, 1, Param("b")), new Circle(Param("X0"), Param("Y0"), Param("R")));
            return string.Format(@"
assume(R>0);
simplify({0});
", res);
        }
        NewExpr GetPerpendocularZeroAssertion2(Line l, Circle c) {
            var A = l.Intersect(c).Item2;
            var B = l.Intersect(c).Item1;
            var l2 = Line.FromPoints(c.Center, B);
            var C = l2.Intersect(c).Item2;
            var l3 = Line.FromPoints(A, C);

            var cotangent = LinesOperations.CotangentBetween(l, l3);
            return cotangent;
        }
        #endregion
    }
}
