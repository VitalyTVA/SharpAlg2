using SharpAlg.Native;
using SharpAlg.Native.Builder;
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
            return new Point(Expr.Parameter(name + "x"), Expr.Parameter(name + "y"));
        }
        public readonly Expr X, Y;
        public Point(Expr x, Expr y) {
            this.X = x;
            this.Y = y;
        }
        public override string ToString() {
            return string.Format("({0}, {1})", X.Print(), Y.Print());
        }
    }
    public class NewPoint {
        public static NewPoint FromName(char name) {
            if(!char.IsUpper(name))
                throw new InvalidOperationException();
            return new NewPoint(Param(name + "x"), Param(name + "y"));
        }
        public readonly NewExpr X, Y;
        public NewPoint(NewExpr x, NewExpr y) {
            this.X = x;
            this.Y = y;
        }
        public override string ToString() {
            return string.Format("({0}, {1})", X, Y);
        }
    }

    public class Line {
        public static Line FromPoints(Point p1, Point p2) {
            var a = Expr.Subtract(p1.Y, p2.Y);
            var b = Expr.Subtract(p2.X, p1.X);
            var c = Expr.Subtract(Expr.Multiply(p1.X, p2.Y), Expr.Multiply(p2.X, p1.Y));
            return new Line(a, b, c).FMap(x => x.Convolute());
        }
        public readonly Expr A, B, C;
        public Line(Expr a, Expr b, Expr c) {
            this.A = a;
            this.B = b;
            this.C = c;
        }
        public override string ToString() {
            return this.PrintObject((c, o) => c.RegisterLine(o, "A", "B", "C"),  "A*x + B*y + C");
        }
    }

    public class NewLine {
        public static NewLine FromPoints(NewPoint p1, NewPoint p2) {
            var a = Subtract(p1.Y, p2.Y);
            var b = Subtract(p2.X, p1.X);
            var c = Subtract(Multiply(p1.X, p2.Y), Multiply(p2.X, p1.Y));
            return new NewLine(a, b, c);
        }
        public readonly NewExpr A, B, C;
        public NewLine(NewExpr a, NewExpr b, NewExpr c) {
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
            var r = Expr.Add(
                        Expr.Subtract(p1.X, p2.X).Square(),
                        Expr.Subtract(p1.Y, p2.Y).Square()
                    );
            return new Circle(p1.X, p1.Y, r).FMap(x => x.Convolute());
        }
        public readonly Expr X, Y, R;
        public Point Center { get { return new Point(X, Y); } }
        public Circle(Expr x, Expr y, Expr r) {
            this.X = x;
            this.Y = y;
            this.R = r;
        }
        public override string ToString() {
            return this.PrintObject((c, o) => c.RegisterCircle(o, "X", "Y", "R"), "(x - X)^2 + (y - Y)^2 - R");
        }
    }

    public class NewCircle {
        public static NewCircle FromPoints(NewPoint p1, NewPoint p2) {
            var r = Add(
                        Subtract(p1.X, p2.X).Square(),
                        Subtract(p1.Y, p2.Y).Square()
                    );
            return new NewCircle(p1.X, p1.Y, r);
        }
        public readonly NewExpr X, Y, R;
        public NewPoint Center { get { return new NewPoint(X, Y); } }
        public NewCircle(NewExpr x, NewExpr y, NewExpr r) {
            this.X = x;
            this.Y = y;
            this.R = r;
        }
        public override string ToString() {
            return Build((X, Y, R, x, y) => ((x - X) ^ 2) + ((y - Y) ^ 2) - R, X, Y, R, Param("x"), Param("y")).ToString();
        }
    }
    public static class NewLinesOperations {
        public static NewPoint Intersect(this NewLine l1, NewLine l2) {
            var x = Build((A1, B1, C1, A2, B2, C2) => (B1 * C2 - B2 * C1) / (A1 * B2 - A2 * B1), l1.A, l1.B, l1.C, l2.A, l2.B, l2.C);
            var y = Build((A1, B1, C1, A2, B2, C2) => (C1 * A2 - C2 * A1) / (A1 * B2 - A2 * B1), l1.A, l1.B, l1.C, l2.A, l2.B, l2.C);
            return new NewPoint(x, y);
        }
        public static NewExpr TangentBetween(NewLine l1, NewLine l2) {
            return Build((A1, B1, A2, B2) => (A1 * B2 - A2 * B1) / (A1 * A2 + B1 * B2), l1.A, l1.B, l2.A, l2.B);
        }
        public static NewExpr CotangentBetween(NewLine l1, NewLine l2) {
            return Build((A1, B1, A2, B2) => (A1 * A2 + B1 * B2) / (A1 * B2 - A2 * B1), l1.A, l1.B, l2.A, l2.B);
        }
        public static NewExpr GetY(NewLine l, NewExpr x) {
            return Build((A, B, C, X) => -(A * X + C) / B, l.A, l.B, l.C, x);
        }
    }
    public static class NewLineCircleIntersector {
        public static System.Tuple<NewPoint, NewPoint> Intersect(this NewLine l, NewCircle c) {
            var eqXA = Build((A, B) => (B ^ 2) + (A ^ 2), l.A, l.B);
            var eqXB = Build((A, B, C, X, Y) => 2 * Y * A * B - 2 * X * (B ^ 2) + 2 * C * A, l.A, l.B, l.C, c.X, c.Y);
            var eqXC = Build((A, B, C, X, Y, R) => 2 * Y * B * C + (Y ^ 2) * (B ^ 2) + (C ^ 2) + (X ^ 2) * (B ^ 2) - R * (B ^ 2), l.A, l.B, l.C, c.X, c.Y, c.R);
            var xRoots = QuadraticEquationHelper.Solve(eqXA, eqXB, eqXC);
            var yRoots = xRoots.FMap(root => NewLinesOperations.GetY(l, root));
            return Tuple.Create(new NewPoint(xRoots.Item1, yRoots.Item1), new NewPoint(xRoots.Item2, yRoots.Item2));
        }
    }
    public static class NewCirclesIntersector {
        public static System.Tuple<NewPoint, NewPoint> Intersect(this NewCircle c1, NewCircle c2) {
            var c = c2.Offset(c1.Center.Invert());
            var eqA = Build((X0, Y0) => 4 * (X0 ^ 2) + 4 * (Y0 ^ 2), c.X, c.Y);
            var eqYB = Build((X0, Y0, R1, R2) => -4 * (Y0 ^ 3) - 4 * R1 * Y0 + 4 * Y0 * R2 - 4 * (X0 ^ 2) * Y0, c.X, c.Y, c1.R, c.R);
            var eqXB = Build((X0, Y0, R1, R2) => -4 * (X0 ^ 3) - 4 * R1 * X0 + 4 * X0 * R2 - 4 * (Y0 ^ 2) * X0, c.X, c.Y, c1.R, c.R);
            var eqYC = Build((X0, Y0, R1, R2) => (X0 ^ 4) + (R1 ^ 2) - 2 * (Y0 ^ 2) * R2 + 2 * (X0 ^ 2) * (Y0 ^ 2) - 2 * (X0 ^ 2) * R2 + (Y0 ^ 4) + (R2 ^ 2) + 2 * R1 * (Y0 ^ 2) - 2 * R1 * R2 - 2 * R1 * (X0 ^ 2), c.X, c.Y, c1.R, c.R);
            var eqXC = Build((X0, Y0, R1, R2) => (Y0 ^ 4) + (R1 ^ 2) - 2 * (X0 ^ 2) * R2 + 2 * (Y0 ^ 2) * (X0 ^ 2) - 2 * (Y0 ^ 2) * R2 + (X0 ^ 4) + (R2 ^ 2) + 2 * R1 * (X0 ^ 2) - 2 * R1 * R2 - 2 * R1 * (Y0 ^ 2), c.X, c.Y, c1.R, c.R);
            var xRoots = QuadraticEquationHelper.Solve(eqA, eqXB, eqXC);
            var yRoots = QuadraticEquationHelper.Solve(eqA, eqYB, eqYC);
            return Tuple.Create(
                new NewPoint(xRoots.Item1, yRoots.Item2),
                new NewPoint(xRoots.Item2, yRoots.Item1)
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
        public static Point FMap(this Point x, Func<Expr, Expr> f) {
            return new Point(f(x.X), f(x.Y));
        }
        public static Line FMap(this Line x, Func<Expr, Expr> f) {
            return new Line(f(x.A), f(x.B), f(x.C));
        }
        public static NewPoint FMap(this NewPoint x, Func<NewExpr, NewExpr> f) {
            return new NewPoint(f(x.X), f(x.Y));
        }
        //public static NewLine FMap(this NewLine x, Func<NewExpr, NewExpr> f) {
        //    return new NewLine(f(x.A), f(x.B), f(x.C));
        //}
        public static Circle FMap(this Circle x, Func<Expr, Expr> f) {
            return new Circle(f(x.X), f(x.Y), f(x.R));
        }
        public static System.Tuple<TResult, TResult> FMap<T, TResult>(this System.Tuple<T, T> x, Func<T, TResult> f) {
            return Tuple.Create(f(x.Item1), f(x.Item2));
        }
    }

    public static class ExprHelper {
        public static readonly Expr Half = "1/2".Parse();
        public static readonly Expr Two = "2".Parse();
        public static Expr Sqrt(this Expr e) {
            return Expr.Power(e, Half);
        }
        public static Expr Square(this Expr e) {
            return Expr.Power(e, Two);
        }
        public static NewExpr Square(this NewExpr e) {
            return Core.ExprExtensions.Power(e, 2);
        }
        public static Expr Multiply(this Expr e, Expr multiplier) {
            return Expr.Multiply(e, multiplier);
        }
        public static Expr Power(this Expr e, Expr power) {
            return Expr.Power(e, power);
        }
        public static Expr GetHalf(this Expr e) {
            return e.Multiply(Half);
        }
        public static NewExpr GetHalf(this NewExpr e) {
            return Core.ExprExtensions.Multiply(e, Const(new BigRational(1, 2)));
        }
        //public static bool IsPrimitive(this Point p) {
        //    return p.X is ParameterExpr && p.Y is ParameterExpr;
        //}
        public static ImmutableContext RegisterPoint(this ImmutableContext context, Point p, double x, double y) {
            return context
                .RegisterValue((ParameterExpr)p.X, x)
                .RegisterValue((ParameterExpr)p.Y, y);
        }
        public static ImmutableContext RegisterPoint(this ImmutableContext context, NewPoint p, double x, double y) {
            return context
                .RegisterValue((Core.ParamExpr)p.X, x)
                .RegisterValue((Core.ParamExpr)p.Y, y);
        }
        public static ImmutableContext RegisterValue(this ImmutableContext context, ParameterExpr parameter, double value) {
            return context
                .Register(parameter.ParameterName, Expr.Constant(value));
        }
        public static ImmutableContext RegisterValue(this ImmutableContext context, Core.ParamExpr parameter, double value) {
            return context
                .Register(parameter.Name, Expr.Constant(value));
        }
        public static ImmutableContext RegisterValue(this ImmutableContext context, string name, double value) {
            return context.RegisterValue(Expr.Parameter(name), value);
        }
        public static ImmutableContext RegisterLine(this ImmutableContext context, Line l, string a, string b, string c) {
            return context
                .Register(a, l.A)
                .Register(b, l.B)
                .Register(c, l.C);
        }
        public static ImmutableContext RegisterCircle(this ImmutableContext context, Circle c, string x, string y, string r) {
            return context
                .Register(x, c.X)
                .Register(y, c.Y)
                .Register(r, c.R);
        }
        public static RealPoint ToRealPoint(this Point p, ImmutableContext context) {
            return new RealPoint(p.X.ToReal(context), p.Y.ToReal(context));
        }
        public static RealPoint ToRealPoint(this NewPoint p, ImmutableContext context) {
            return new RealPoint(p.X.ToReal(context), p.Y.ToReal(context));
        }
        public static double ToReal(this Expr expr, ImmutableContext context) {
            return expr.Evaluate(context).ToDouble();
        }
        public static Expr AsConst(this double value) {
            return Expr.Constant(value);
        }
        public static Expr Convolute(this Expr expr) {
            return expr;
        }
        public static Point Offset(this Point p, Point offset) {
            return new Point(Expr.Add(p.X, offset.X), Expr.Add(p.Y, offset.Y));
        }
        public static NewPoint Offset(this NewPoint p, NewPoint offset) {
            return new NewPoint(Add(p.X, offset.X), Add(p.Y, offset.Y));
        }
        public static Point Invert(this Point p) {
            return new Point(Expr.Minus(p.X), Expr.Minus(p.Y));
        }
        public static NewPoint Invert(this NewPoint p) {
            return new NewPoint(Minus(p.X), Minus(p.Y));
        }
        public static Point Middle(Point p1, Point p2) {
            return new Point(Expr.Add(p1.X, p2.X).GetHalf(), Expr.Add(p1.Y, p2.Y).GetHalf());
        }
        public static NewPoint Middle(NewPoint p1, NewPoint p2) {
            return new NewPoint(Add(p1.X, p2.X).GetHalf(), Add(p1.Y, p2.Y).GetHalf());
        }
        public static Circle Offset(this Circle c, Point offset) {
            var center = c.Center.Offset(offset);
            return new Circle(center.X, center.Y, c.R);
        }
        public static NewCircle Offset(this NewCircle c, NewPoint offset) {
            var center = c.Center.Offset(offset);
            return new NewCircle(center.X, center.Y, c.R);
        }
        public static Expr Substitute(this Expr expr, IContext context) {
            return ExprSubstitutor.Substitute(expr, context);
        }
        public static Point Substitute(this Point p, IContext context) {
            return p.FMap(x => x.Substitute(context));
        }
        public static System.Tuple<Point, Point> Substitute(this System.Tuple<Point, Point> p, IContext context) {
            return p.FMap(x => x.Substitute(context));
        }
        public static string PrintObject<T>(this T obj, Func<ImmutableContext, T, ImmutableContext> register, string expr) {
            var context = register(ImmutableContext.Empty, obj);
            return expr.Transform(context);
        }
        static string Transform(this string expr, IContext context) { 
            return expr.Parse().Substitute(context).Convolute().Print();
        }
    }
    public class ExprSubstitutor : IExpressionVisitor<Expr> {
        public static Expr Substitute(Expr expr, IContext context) {
            return expr.Visit(new ExprSubstitutor(context));
        }
        readonly IContext context;
        ExprSubstitutor(IContext context) {
            this.context = context;
        }
        Expr IExpressionVisitor<Expr>.Constant(ConstantExpr constant) {
            return constant;
        }

        Expr IExpressionVisitor<Expr>.Parameter(ParameterExpr parameter) {
            return context.GetValue(parameter.ParameterName) ?? parameter;
        }

        Expr IExpressionVisitor<Expr>.Add(AddExpr multi) {
            return Expr.Add(multi.Args.Select(x => x.Visit(this)));
        }

        Expr IExpressionVisitor<Expr>.Multiply(MultiplyExpr multi) {
            return Expr.Multiply(multi.Args.Select(x => x.Visit(this)));
        }

        Expr IExpressionVisitor<Expr>.Power(PowerExpr power) {
            return Expr.Power(power.Left.Visit(this), power.Right.Visit(this));
        }

        Expr IExpressionVisitor<Expr>.Function(FunctionExpr functionExpr) {
            throw new NotImplementedException();
        }
    }
    public class ExprRewriter : IExpressionVisitor<Expr> {
        readonly ExprBuilder builder;
        public ExprRewriter(ExprBuilder builder) {
            this.builder = builder;
        }
        Expr IExpressionVisitor<Expr>.Constant(ConstantExpr constant) {
            return constant;
        }

        Expr IExpressionVisitor<Expr>.Parameter(ParameterExpr parameter) {
            return parameter;
        }

        Expr IExpressionVisitor<Expr>.Add(AddExpr multi) {
            return multi.Args.Select(x => x.Visit(this)).Aggregate((x, y) => builder.Add(x, y));
        }

        Expr IExpressionVisitor<Expr>.Multiply(MultiplyExpr multi) {
            return multi.Args.Select(x => x.Visit(this)).Aggregate((x, y) => builder.Multiply(x, y));
        }

        Expr IExpressionVisitor<Expr>.Power(PowerExpr power) {
            return builder.Power(power.Left.Visit(this), power.Right.Visit(this));
        }

        Expr IExpressionVisitor<Expr>.Function(FunctionExpr functionExpr) {
            throw new NotImplementedException();
            //return builder.Function(functionExpr.FunctionName, functionExpr.Args);
        }
    }
}