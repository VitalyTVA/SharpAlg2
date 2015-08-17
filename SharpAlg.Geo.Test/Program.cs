using SharpAlg.Geo.Core;
using SharpAlg.Geo.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlg.Geo.Test {
    class Program {
        //static Builder builder = Builder.CreateRealLife();
        static void Main(string[] args) {
            //MiddleOfLineSegment_Calc();
            var t = new ProblemsTests();
            t.SetUp();
            t.MiddleOfLineSegment_Calc();
        }

        //static void MiddleOfLineSegment_Calc() {
        //    var x = builder.MakePoint('X');
        //    var y = builder.MakePoint('Y');
        //    var point = GetMiddleOfLineSegmentZeroAssertion(x, y);
        //    //AssertHelper.ArePointsEqual(new RealPoint(0, 0), point.ToRealPoint(ImmutableContext.Empty.RegisterPoint(x, 1, 2).RegisterPoint(y, 5, 9)));
        //}
        //static Point GetMiddleOfLineSegmentZeroAssertion(Point p1, Point p2) {
        //    var l1 = builder.MakeLine(p1, p2);

        //    var c1 = builder.MakeCircle(p1, p2);
        //    var c2 = builder.MakeCircle(p2, p1);

        //    var c1_c2 = builder.IntersectCircles(c1, c2);
        //    var l2 = builder.MakeLine(c1_c2.Item1, c1_c2.Item2);

        //    var l1_l2 = builder.IntersectLines(l1, l2);

        //    var expected = builder.Middle(p1, p2);
        //    return builder.Offset(l1_l2, builder.Invert(expected));

        //}
    }
}
