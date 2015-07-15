using System;
using System.Collections.Immutable;
using System.Linq;
using RealPoint = System.Windows.Point;
using static SharpAlg.Geo.Core.ExprExtensions;
using NewExpr = SharpAlg.Geo.Core.Expr;
using Numerics;

namespace SharpAlg.Geo {
    public class Point {
        public static Point FromName(char name) {
            if(!char.IsUpper(name))
                throw new InvalidOperationException();
            return new Point(Param(name + "x"), Param(name + "y"));
        }
        public readonly NewExpr X, Y;
        public Point(NewExpr x, NewExpr y) {
            this.X = x;
            this.Y = y;
        }
        public override string ToString() {
            return string.Format("({0}, {1})", X, Y);
        }
    }

    public class Line {
        public static Line FromPoints(Point p1, Point p2) {
            var a = Subtract(p1.Y, p2.Y);
            var b = Subtract(p2.X, p1.X);
            var c = Subtract(Multiply(p1.X, p2.Y), Multiply(p2.X, p1.Y));
            return new Line(a, b, c);
        }
        public readonly NewExpr A, B, C;
        public Line(NewExpr a, NewExpr b, NewExpr c) {
            this.A = a;
            this.B = b;
            this.C = c;
        }
        public override string ToString() {
            return Build((A, B, C, x, y) => A * x + B * y + C, A, B, C, Param("x"), Param("y")).ToString();
        }
    }

    public class Circle {
        public static Circle FromPoints(Point p1, Point p2) {
            var r = Add(
                        Subtract(p1.X, p2.X).Square(),
                        Subtract(p1.Y, p2.Y).Square()
                    );
            return new Circle(p1.X, p1.Y, r);
        }
        public readonly NewExpr X, Y, R;
        public Point Center { get { return new Point(X, Y); } }
        public Circle(NewExpr x, NewExpr y, NewExpr r) {
            this.X = x;
            this.Y = y;
            this.R = r;
        }
        public override string ToString() {
            return Build((X, Y, R, x, y) => ((x - X) ^ 2) + ((y - Y) ^ 2) - R, X, Y, R, Param("x"), Param("y")).ToString();
        }
    }
    public static class LinesOperations {
        public static Point Intersect(this Line l1, Line l2) {
            var x = Build((A1, B1, C1, A2, B2, C2) => (B1 * C2 - B2 * C1) / (A1 * B2 - A2 * B1), l1.A, l1.B, l1.C, l2.A, l2.B, l2.C);
            var y = Build((A1, B1, C1, A2, B2, C2) => (C1 * A2 - C2 * A1) / (A1 * B2 - A2 * B1), l1.A, l1.B, l1.C, l2.A, l2.B, l2.C);
            return new Point(x, y);
        }
        public static NewExpr TangentBetween(Line l1, Line l2) {
            return Build((A1, B1, A2, B2) => (A1 * B2 - A2 * B1) / (A1 * A2 + B1 * B2), l1.A, l1.B, l2.A, l2.B);
        }
        public static NewExpr CotangentBetween(Line l1, Line l2) {
            return Build((A1, B1, A2, B2) => (A1 * A2 + B1 * B2) / (A1 * B2 - A2 * B1), l1.A, l1.B, l2.A, l2.B);
        }
        public static NewExpr GetY(Line l, NewExpr x) {
            return Build((A, B, C, X) => -(A * X + C) / B, l.A, l.B, l.C, x);
        }
    }
    public static class LineCircleIntersector {
        public static System.Tuple<Point, Point> Intersect(this Line l, Circle c) {
            var eqXA = Build((A, B) => (B ^ 2) + (A ^ 2), l.A, l.B);
            var eqXB = Build((A, B, C, X, Y) => 2 * Y * A * B - 2 * X * (B ^ 2) + 2 * C * A, l.A, l.B, l.C, c.X, c.Y);
            var eqXC = Build((A, B, C, X, Y, R) => 2 * Y * B * C + (Y ^ 2) * (B ^ 2) + (C ^ 2) + (X ^ 2) * (B ^ 2) - R * (B ^ 2), l.A, l.B, l.C, c.X, c.Y, c.R);
            var xRoots = QuadraticEquationHelper.Solve(eqXA, eqXB, eqXC);
            var yRoots = xRoots.FMap(root => LinesOperations.GetY(l, root));
            return Tuple.Create(new Point(xRoots.Item1, yRoots.Item1), new Point(xRoots.Item2, yRoots.Item2));
        }
    }
    public static class CirclesIntersector {
        public static System.Tuple<Point, Point> Intersect(this Circle c1, Circle c2) {
            var c = c2.Offset(c1.Center.Invert());
            var eqA = Build((X0, Y0) => 4 * (X0 ^ 2) + 4 * (Y0 ^ 2), c.X, c.Y);
            var eqYB = Build((X0, Y0, R1, R2) => -4 * (Y0 ^ 3) - 4 * R1 * Y0 + 4 * Y0 * R2 - 4 * (X0 ^ 2) * Y0, c.X, c.Y, c1.R, c.R);
            var eqXB = Build((X0, Y0, R1, R2) => -4 * (X0 ^ 3) - 4 * R1 * X0 + 4 * X0 * R2 - 4 * (Y0 ^ 2) * X0, c.X, c.Y, c1.R, c.R);
            var eqYC = Build((X0, Y0, R1, R2) => (X0 ^ 4) + (R1 ^ 2) - 2 * (Y0 ^ 2) * R2 + 2 * (X0 ^ 2) * (Y0 ^ 2) - 2 * (X0 ^ 2) * R2 + (Y0 ^ 4) + (R2 ^ 2) + 2 * R1 * (Y0 ^ 2) - 2 * R1 * R2 - 2 * R1 * (X0 ^ 2), c.X, c.Y, c1.R, c.R);
            var eqXC = Build((X0, Y0, R1, R2) => (Y0 ^ 4) + (R1 ^ 2) - 2 * (X0 ^ 2) * R2 + 2 * (Y0 ^ 2) * (X0 ^ 2) - 2 * (Y0 ^ 2) * R2 + (X0 ^ 4) + (R2 ^ 2) + 2 * R1 * (X0 ^ 2) - 2 * R1 * R2 - 2 * R1 * (Y0 ^ 2), c.X, c.Y, c1.R, c.R);
            var xRoots = QuadraticEquationHelper.Solve(eqA, eqXB, eqXC);
            var yRoots = QuadraticEquationHelper.Solve(eqA, eqYB, eqYC);
            return Tuple.Create(
                new Point(xRoots.Item1, yRoots.Item2),
                new Point(xRoots.Item2, yRoots.Item1)
            ).FMap(x => x.Offset(c1.Center));
        }
    }

    public static class QuadraticEquationHelper {
        public static System.Tuple<NewExpr, NewExpr> Solve(NewExpr a, NewExpr b, NewExpr c) {
            var d = Build((A, B, C) => Sqrt((B ^ 2) - 4 * A * C), a, b, c);
            var x1 = Build((A, B, D) => (-B + D) / (2 * A), a, b, d);
            var x2 = Build((A, B, D) => (-B - D) / (2 * A), a, b, d);
            return Tuple.Create(x1, x2);
        }
    }

    public static class Functor {
        public static Point FMap(this Point x, Func<NewExpr, NewExpr> f) {
            return new Point(f(x.X), f(x.Y));
        }
        public static System.Tuple<TResult, TResult> FMap<T, TResult>(this System.Tuple<T, T> x, Func<T, TResult> f) {
            return Tuple.Create(f(x.Item1), f(x.Item2));
        }
    }

    public static class ExprHelper {
        public static NewExpr Square(this NewExpr e) {
            return Core.ExprExtensions.Power(e, 2);
        }
        public static NewExpr GetHalf(this NewExpr e) {
            return Core.ExprExtensions.Multiply(e, Const(new BigRational(1, 2)));
        }
        public static ImmutableContext RegisterPoint(this ImmutableContext context, Point p, double x, double y) {
            return context
                .RegisterValue((Core.ParamExpr)p.X, x)
                .RegisterValue((Core.ParamExpr)p.Y, y);
        }
        public static ImmutableContext RegisterValue(this ImmutableContext context, Core.ParamExpr parameter, double value) {
            return context
                .Register(parameter.Name, SharpAlg.Native.Expr.Constant(value));
        }
        public static ImmutableContext RegisterValue(this ImmutableContext context, string name, double value) {
            return context.RegisterValue(Param(name), value);
        }
        public static RealPoint ToRealPoint(this Point p, ImmutableContext context) {
            return new RealPoint(p.X.ToReal(context), p.Y.ToReal(context));
        }
        public static Point Offset(this Point p, Point offset) {
            return new Point(Add(p.X, offset.X), Add(p.Y, offset.Y));
        }
        public static Point Invert(this Point p) {
            return new Point(Minus(p.X), Minus(p.Y));
        }
        public static Point Middle(Point p1, Point p2) {
            return new Point(Add(p1.X, p2.X).GetHalf(), Add(p1.Y, p2.Y).GetHalf());
        }
        public static Circle Offset(this Circle c, Point offset) {
            var center = c.Center.Offset(offset);
            return new Circle(center.X, center.Y, c.R);
        }
    }
}