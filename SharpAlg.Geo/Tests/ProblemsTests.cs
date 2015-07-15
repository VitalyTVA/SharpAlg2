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
            var x = NewPoint.FromName('X');
            var y = NewPoint.FromName('Y');
            var point = GetMiddleOfLineSegmentZeroAssertion(x, y);
            AssertHelper.ArePointsEqual(new RealPoint(0, 0), point.ToRealPoint(ImmutableContext.Empty.RegisterPoint(x, 1, 2).RegisterPoint(y, 5, 9)));
        }
        string MiddleOfLineSegment_Maple() {
            var res = GetMiddleOfLineSegmentZeroAssertion(NewPoint.FromName('X'), NewPoint.FromName('Y'));
            return string.Format("simplify({0}); simplify({1});", res.X, res.Y);
        }
        NewPoint GetMiddleOfLineSegmentZeroAssertion(NewPoint p1, NewPoint p2) {
            var l1 = NewLine.FromPoints(p1, p2);

            var c1 = NewCircle.FromPoints(p1, p2);
            var c2 = NewCircle.FromPoints(p2, p1);

            var c1_c2 = c1.Intersect(c2);
            var l2 = NewLine.FromPoints(c1_c2.Item1, c1_c2.Item2);

            var l1_l2 = l1.Intersect(l2);

            var expected = ExprHelper.Middle(p1, p2);
            return l1_l2.Offset(expected.Invert());

        }
        #endregion

        #region angle bisector
        [Test]
        public void AngleBisection_Calc() {
            var a = NewPoint.FromName('A');
            var b = NewPoint.FromName('B');
            var c = NewPoint.FromName('C');
            var value = GetAngleBisectionZeroAssertion(a, b, c);
            Assert.AreEqual(0, value.ToReal(ImmutableContext.Empty.RegisterPoint(a, 1, 2).RegisterPoint(b, 5, 9).RegisterPoint(c, 3, 7)), AssertHelper.Delta);
        }
        string AngleBisection_Maple() {
            var res = GetAngleBisectionZeroAssertion(NewPoint.FromName('A'), NewPoint.FromName('B'), NewPoint.FromName('C'));
            //var res = GetAngleBisectionZeroAssertion(new Point(Expr.Zero, Expr.Zero), Point.FromName('B'), Point.FromName('C'));
            return string.Format("simplify({0});", res);
            //Clipboard.SetText(mappleCommand);
        }
        NewExpr GetAngleBisectionZeroAssertion(NewPoint A, NewPoint B, NewPoint C) {
            var l1 = NewLine.FromPoints(A, B);
            var l2 = NewLine.FromPoints(A, C);

            var c = NewCircle.FromPoints(A, C);
            var c_l2 = l1.Intersect(c).Item1;

            var middle = ExprHelper.Middle(C, c_l2);//TODO make real
            var bisectrissa = NewLine.FromPoints(A, middle);

            return Add(NewLinesOperations.TangentBetween(l1, bisectrissa), NewLinesOperations.TangentBetween(l2, bisectrissa));
        }
        #endregion

        #region perpendicular
        [Test]
        public void Perpendicular_Calc() {
            var a = NewPoint.FromName('A');
            var b = NewPoint.FromName('B');
            var c = NewPoint.FromName('C');
            var value = GetPerpendicularZeroAssertion(a, b, c);
            Assert.AreEqual(0, value.ToReal(ImmutableContext.Empty.RegisterPoint(a, 1, 2).RegisterPoint(b, 5, 9).RegisterPoint(c, 3, 7)), AssertHelper.Delta);
        }
        string Perpendicular_Maple() {
            //var res = GetPerpendocularZeroAssertion(new Point(Expr.Zero, Expr.Zero), Point.FromName('B'), Point.FromName('C'));
            var res = GetPerpendicularZeroAssertion(new NewPoint(0, 0), new NewPoint(-1, 0), NewPoint.FromName('C'));
            return string.Format(@"
assume(Cx>0);
assume(Cy>0);
simplify({0});
", res);
        }
        NewExpr GetPerpendicularZeroAssertion(NewPoint A, NewPoint B, NewPoint C) {
            var l1 = NewLine.FromPoints(A, B);

            var c = NewCircle.FromPoints(C, A);

            var D = l1.Intersect(c).Item1; //????
            var l2 = NewLine.FromPoints(C, D);
            var E = l2.Intersect(c).Item2; //????1

            var l3 = NewLine.FromPoints(E, A);

            var cotangent = NewLinesOperations.CotangentBetween(l1, l3);
            return cotangent;

        } 
        #endregion

        #region perpendicular
        [Test]
        public void Perpendicular2_Calc() {
            var res = GetPerpendocularZeroAssertion2(new Line(Expr.Zero, Expr.One, 4d.AsConst()), new Circle(Expr.Zero, Expr.Zero, 25d.AsConst()));
            Assert.AreEqual(0, res.ToReal(ImmutableContext.Empty));
            //Assert.IsTrue(res.ExprEquals(Expr.Zero));
        }
        [Test, Ignore]
        public void Perpendicular2_Calc2() {
            var res = GetPerpendocularZeroAssertion2(new Line(Expr.Zero, Expr.One, 4d.AsConst()), new Circle(Expr.Zero, Expr.Zero, 25d.AsConst()));
            Assert.IsTrue(res.ExprEquals(Expr.Zero));
        }
        string Perpendicular2_Maple() {
            //var res = GetPerpendocularZeroAssertion2(new Line(Expr.Parameter("k"), Expr.One, Expr.Parameter("b")), new Circle(Expr.Zero, Expr.Zero, Expr.Parameter("R")));
            var res = GetPerpendocularZeroAssertion2(new Line(Expr.Zero, Expr.One, Expr.Parameter("b")), new Circle(Expr.Parameter("X0"), Expr.Parameter("Y0"), Expr.Parameter("R")));
            return string.Format(@"
assume(R>0);
simplify({0});
", res.Print());
        }
        Expr GetPerpendocularZeroAssertion2(Line l, Circle c) {
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
