using SharpAlg.Geo.Core;
using System;
using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo {
    internal static class PrimitiveOperations {
        internal static Point Offset(this Builder builder, Point p, Point offset) {
            return new Point(Add(p.X, offset.X), Add(p.Y, offset.Y));
        }
        internal static Point Invert(this Builder builder, Point p) {
            return new Point(Minus(p.X), Minus(p.Y));
        }
        internal static Circle Offset(this Builder builder, Circle c, Point offset) {
            var center = builder.Offset(c.Center, offset);
            return new Circle(center.X, center.Y, c.R);
        }
        internal static Point Middle(this Builder builder, Point p1, Point p2) {
            return new Point(builder.Mean(p1.X, p2.X), builder.Mean(p1.Y, p2.Y));
        }
        static Expr Mean(this Builder builder, Expr a, Expr b) => builder.Build((x, y) => (x + y) / 2, a, b);

        internal static Circle MakeCircle(this Builder builder, Point p1, Point p2) {
            var r = builder.Build((x1, x2, y1, y2) => ((x1 - x2) ^ 2) + ((y1 - y2) ^ 2), p1.X, p2.X, p1.Y, p2.Y);
            return new Circle(p1.X, p1.Y, r);
        }
        internal static Point MakePoint(this Builder builder, char name) {
            if(!char.IsUpper(name))
                throw new InvalidOperationException();
            return new Point(Param(name + "x"), Param(name + "y"));
        }
        internal static Line MakeLine(this Builder builder, Point p1, Point p2) {
            var a = Subtract(p1.Y, p2.Y);
            var b = Subtract(p2.X, p1.X);
            var c = Subtract(Multiply(p1.X, p2.Y), Multiply(p2.X, p1.Y));
            return new Line(a, b, c);
        }
        internal static Point IntersectLines(this Builder builder, Line l1, Line l2) {
            var x = builder.Build((A1, B1, C1, A2, B2, C2) => (B1 * C2 - B2 * C1) / (A1 * B2 - A2 * B1), l1.A, l1.B, l1.C, l2.A, l2.B, l2.C);
            var y = builder.Build((A1, B1, C1, A2, B2, C2) => (C1 * A2 - C2 * A1) / (A1 * B2 - A2 * B1), l1.A, l1.B, l1.C, l2.A, l2.B, l2.C);
            return new Point(x, y);
        }
        internal static Tuple<Point, Point> IntersectLineAndCircle(this Builder builder, Line l, Circle c) {
            var eqXA = builder.Build((A, B) => (B ^ 2) + (A ^ 2), l.A, l.B);
            var eqXB = builder.Build((A, B, C, X, Y) => 2 * Y * A * B - 2 * X * (B ^ 2) + 2 * C * A, l.A, l.B, l.C, c.X, c.Y);
            var eqXC = builder.Build((A, B, C, X, Y, R) => 2 * Y * B * C + (Y ^ 2) * (B ^ 2) + (C ^ 2) + (X ^ 2) * (B ^ 2) - R * (B ^ 2), l.A, l.B, l.C, c.X, c.Y, c.R);
            var xRoots = builder.SolveQuadraticEquation(eqXA, eqXB, eqXC);
            var yRoots = xRoots.FMap(root => builder.GetLineYByX(l, root));
            return Tuple.Create(new Point(xRoots.Item1, yRoots.Item1), new Point(xRoots.Item2, yRoots.Item2));
        }
        internal static Tuple<Point, Point> IntersectCircles(this Builder builder, Circle c1, Circle c2) {
            var c = builder.Offset(c2, builder.Invert(c1.Center));
            var eqA = builder.Build((X0, Y0) => 4 * (X0 ^ 2) + 4 * (Y0 ^ 2), c.X, c.Y);
            var eqYB = builder.Build((X0, Y0, R1, R2) => -4 * (Y0 ^ 3) - 4 * R1 * Y0 + 4 * Y0 * R2 - 4 * (X0 ^ 2) * Y0, c.X, c.Y, c1.R, c.R);
            var eqXB = builder.Build((X0, Y0, R1, R2) => -4 * (X0 ^ 3) - 4 * R1 * X0 + 4 * X0 * R2 - 4 * (Y0 ^ 2) * X0, c.X, c.Y, c1.R, c.R);
            var eqYC = builder.Build((X0, Y0, R1, R2) => (X0 ^ 4) + (R1 ^ 2) - 2 * (Y0 ^ 2) * R2 + 2 * (X0 ^ 2) * (Y0 ^ 2) - 2 * (X0 ^ 2) * R2 + (Y0 ^ 4) + (R2 ^ 2) + 2 * R1 * (Y0 ^ 2) - 2 * R1 * R2 - 2 * R1 * (X0 ^ 2), c.X, c.Y, c1.R, c.R);
            var eqXC = builder.Build((X0, Y0, R1, R2) => (Y0 ^ 4) + (R1 ^ 2) - 2 * (X0 ^ 2) * R2 + 2 * (Y0 ^ 2) * (X0 ^ 2) - 2 * (Y0 ^ 2) * R2 + (X0 ^ 4) + (R2 ^ 2) + 2 * R1 * (X0 ^ 2) - 2 * R1 * R2 - 2 * R1 * (Y0 ^ 2), c.X, c.Y, c1.R, c.R);
            var xRoots = builder.SolveQuadraticEquation(eqA, eqXB, eqXC);
            var yRoots = builder.SolveQuadraticEquation(eqA, eqYB, eqYC);
            return Tuple.Create(
                new Point(xRoots.Item1, yRoots.Item2),
                new Point(xRoots.Item2, yRoots.Item1)
            ).FMap(x => builder.Offset(x, c1.Center));
        }
        internal static Expr TangentBetweenLines(this Builder builder, Line l1, Line l2) {
            return builder.Build((A1, B1, A2, B2) => (A1 * B2 - A2 * B1) / (A1 * A2 + B1 * B2), l1.A, l1.B, l2.A, l2.B);
        }
        internal static Expr CotangentBetweenLine(this Builder builder, Line l1, Line l2) {
            return builder.Build((A1, B1, A2, B2) => (A1 * A2 + B1 * B2) / (A1 * B2 - A2 * B1), l1.A, l1.B, l2.A, l2.B);
        }
        internal static Expr GetLineYByX(this Builder builder, Line l, Expr x) {
            return builder.Build((A, B, C, X) => -(A * X + C) / B, l.A, l.B, l.C, x);
        }
        internal static Tuple<Expr, Expr> SolveQuadraticEquation(this Builder builder, Expr a, Expr b, Expr c) {
            var d = builder.Build((A, B, C) => Sqrt((B ^ 2) - 4 * A * C), a, b, c);
            var x1 = builder.Build((A, B, D) => (-B + D) / (2 * A), a, b, d);
            var x2 = builder.Build((A, B, D) => (-B - D) / (2 * A), a, b, d);
            return Tuple.Create(x1, x2);
        }
    }
}
