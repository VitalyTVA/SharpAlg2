using NUnit.Framework;
using System.Linq;
using RealPoint = System.Windows.Point;
using static SharpAlg.Geo.Core.ExprExtensions;
using SharpAlg.Geo.Core;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class ProblemsTests : ExprTestsBase {
        [Test, Explicit]
        public void GetAllMapleCommand() {
            var t = string.Concat(new[] {
                MiddleOfLineSegment_Maple(),
                AngleBisection_Maple(),
                Perpendicular_Maple(),
                Perpendicular2_Maple(),
            }.Select(x => "restart;" + x + "\r\n\r\n\r\n"));
            t.ToString();
        }
        #region middle
        [Test]
        public void MiddleOfLineSegment_Calc() {
            var x = builder.MakePoint('X');
            var y = builder.MakePoint('Y');
            var point = GetMiddleOfLineSegmentZeroAssertion(x, y);
            AssertHelper.ArePointsEqual(new RealPoint(0, 0), point.ToRealPoint(ImmutableContext.Empty.RegisterPoint(x, 1, 2).RegisterPoint(y, 5, 9)));
        }
        string MiddleOfLineSegment_Maple() {
            var res = GetMiddleOfLineSegmentZeroAssertion(builder.MakePoint('X'), builder.MakePoint('Y'));
            return string.Format("simplify({0}); simplify({1});", res.X, res.Y);
        }
        Point GetMiddleOfLineSegmentZeroAssertion(Point p1, Point p2) {
            var l1 = builder.MakeLine(p1, p2);

            var c1 = builder.MakeCircle(p1, p2);
            var c2 = builder.MakeCircle(p2, p1);

            var c1_c2 = builder.IntersectCircles(c1, c2);
            var l2 = builder.MakeLine(c1_c2.Item1, c1_c2.Item2);

            var l1_l2 = builder.IntersectLines(l1, l2);

            var expected = builder.Middle(p1, p2);
            return builder.Offset(l1_l2, builder.Invert(expected));

        }
        #endregion

        #region angle bisector
        [Test]
        public void AngleBisection_Calc() {
            var a = builder.MakePoint('A');
            var b = builder.MakePoint('B');
            var c = builder.MakePoint('C');
            var value = GetAngleBisectionZeroAssertion(a, b, c);
            Assert.AreEqual(0, value.ToReal(ImmutableContext.Empty.RegisterPoint(a, 1, 2).RegisterPoint(b, 5, 9).RegisterPoint(c, 3, 7)), AssertHelper.Delta);
        }
        string AngleBisection_Maple() {
            var res = GetAngleBisectionZeroAssertion(builder.MakePoint('A'), builder.MakePoint('B'), builder.MakePoint('C'));
            //var res = GetAngleBisectionZeroAssertion(new Point(Expr.Zero, Expr.Zero), builder.FromName('B'), builder.FromName('C'));
            return string.Format("simplify({0});", res);
            //Clipboard.SetText(mappleCommand);
        }
        Expr GetAngleBisectionZeroAssertion(Point A, Point B, Point C) {
            var l1 = builder.MakeLine(A, B);
            var l2 = builder.MakeLine(A, C);

            var c = builder.MakeCircle(A, C);
            var c_l2 = builder.IntersectLineAndCircle(l1, c).Item1;

            var middle = builder.Middle(C, c_l2);//TODO make real
            var bisectrissa = builder.MakeLine(A, middle);

            return builder.Add(builder.TangentBetweenLines(l1, bisectrissa), builder.TangentBetweenLines(l2, bisectrissa));
        }
        #endregion

        #region perpendicular
        [Test]
        public void Perpendicular_Calc() {
            var a = builder.MakePoint('A');
            var b = builder.MakePoint('B');
            var c = builder.MakePoint('C');
            var value = GetPerpendicularZeroAssertion(a, b, c);
            Assert.AreEqual(0, value.ToReal(ImmutableContext.Empty.RegisterPoint(a, 1, 2).RegisterPoint(b, 5, 9).RegisterPoint(c, 3, 7)), AssertHelper.Delta);
        }
        string Perpendicular_Maple() {
            //var res = GetPerpendocularZeroAssertion(new Point(Expr.Zero, Expr.Zero), builder.FromName('B'), builder.FromName('C'));
            var res = GetPerpendicularZeroAssertion(new Point(0, 0), new Point(-1, 0), builder.MakePoint('C'));
            return string.Format(@"
assume(Cx>0);
assume(Cy>0);
simplify({0});
", res);
        }
        Expr GetPerpendicularZeroAssertion(Point A, Point B, Point C) {
            var l1 = builder.MakeLine(A, B);

            var c = builder.MakeCircle(C, A);

            var D = builder.IntersectLineAndCircle(l1, c).Item1; //????
            var l2 = builder.MakeLine(C, D);
            var E = builder.IntersectLineAndCircle(l2, c).Item2; //????1

            var l3 = builder.MakeLine(E, A);

            var cotangent = builder.CotangentBetweenLine(l1, l3);
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
        Expr GetPerpendocularZeroAssertion2(Line l, Circle c) {
            var A = builder.IntersectLineAndCircle(l, c).Item2;
            var B = builder.IntersectLineAndCircle(l, c).Item1;
            var l2 = builder.MakeLine(c.Center, B);
            var C = builder.IntersectLineAndCircle(l2, c).Item2;
            var l3 = builder.MakeLine(A, C);

            var cotangent = builder.CotangentBetweenLine(l, l3);
            return cotangent;
        }
        #endregion
    }
}
