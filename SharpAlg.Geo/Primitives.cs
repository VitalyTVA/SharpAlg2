using SharpAlg.Native;
using SharpAlg.Native.Builder;
using System;
using System.Collections.Immutable;
using System.Linq;
using RealPoint = System.Windows.Point;
using static SharpAlg.Geo.Core.ExprExtensions;
using NewExpr = SharpAlg.Geo.Core.Expr;

namespace SharpAlg.Geo {
    public class Point {
        public static Point FromName(char name) {
            if(!char.IsUpper(name))
                throw new InvalidOperationException();
            return new Point(Expr.Parameter(name + "x"), Expr.Parameter(name + "y"));
        }
        public static Point FromValues(double x, double y) {
            return new Point(x.AsConst(), y.AsConst());
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
    public static class LinesOperations {
        static readonly Point Intersection;
        static readonly Expr Tangent;
        static readonly Expr Cotangent;
        static readonly Expr YExpr = "-(A*x+C)/B".Parse();
        static LinesOperations() {
            Intersection = GetIntersection();
            Tangent = GetTangent();
            Cotangent = GetCotangent();
        }
        static Point GetIntersection() {
            const string divider = "(A1*B2-A2*B1)";
            var x = ("(B1*C2-B2*C1)/" + divider).Parse();
            var y = ("(C1*A2-C2*A1)/" + divider).Parse();
            return new Point(x, y);
        }
        public static Point Intersect(this Line l1, Line l2) {
            var context = ImmutableContext.Empty
                .RegisterLine(l1, "A1", "B1", "C1")
                .RegisterLine(l2, "A2", "B2", "C2");
            return Intersection.Substitute(context).FMap(x => x.Convolute());
        }

        static Expr GetTangent() {
            return "(A1*B2-A2*B1)/(A1*A2 + B1*B2)".Parse();
        }
        static Expr GetCotangent() {
            return "(A1*A2 + B1*B2)/(A1*B2-A2*B1)".Parse();
        }
        public static Expr TangentBetween(Line l1, Line l2) {
            return GetTwoLinesExpression(l1, l2, Tangent);
        }
        public static Expr CotangentBetween(Line l1, Line l2) {
            return GetTwoLinesExpression(l1, l2, Cotangent);
        }
        static Expr GetTwoLinesExpression(Line l1, Line l2, Expr expr) {
            var context = ImmutableContext.Empty
                .RegisterLine(l1, "A1", "B1", "C1")
                .RegisterLine(l2, "A2", "B2", "C2");
            return expr.Substitute(context).Convolute();
        }

        public static Expr GetY(Expr x) {
            return YExpr.Substitute(ImmutableContext.Empty.Register("x", x));
        }
    }
    public static class LineCircleIntersector {
        static readonly System.Tuple<Point, Point> Intersections;
        static LineCircleIntersector() {
            var eqXA = "B^2+A^2".Parse();
            var eqXB = "2*Y*A*B-2*X*B^2+2*C*A".Parse();
            var eqXC = "2*Y*B*C+Y^2*B^2+C^2+X^2*B^2-R*B^2".Parse();
            var xRoots = QuadraticEquationHelper.Solve(eqXA, eqXB, eqXC);
            var yRoots = xRoots.FMap(x => LinesOperations.GetY(x));
            Intersections = Tuple.Create(new Point(xRoots.Item1, yRoots.Item1), new Point(xRoots.Item2, yRoots.Item2));
        }
        public static System.Tuple<Point, Point> Intersect(this Line l, Circle c) {
            var context = ImmutableContext.Empty
                .RegisterLine(l, "A", "B", "C")
                .RegisterCircle(c, "X", "Y", "R");
            return Intersections
                .Substitute(context)
                .FMap(tuple => tuple.FMap(x => x.Convolute()));
        }
    }
    public static class CirclesIntersector {
        static readonly System.Tuple<Point, Point> Intersections;
        static CirclesIntersector() {
            var eqA = "4*X0^2+4*Y0^2".Parse();
            var eqYB = "-4*Y0^3-4*R1*Y0+4*Y0*R2-4*X0^2*Y0".Parse();
            var eqXB = "-4*X0^3-4*R1*X0+4*X0*R2-4*Y0^2*X0".Parse();
            var eqYC = "X0^4+R1^2-2*Y0^2*R2+2*X0^2*Y0^2-2*X0^2*R2+Y0^4+R2^2+2*R1*Y0^2-2*R1*R2-2*R1*X0^2".Parse();
            var eqXC = "Y0^4+R1^2-2*X0^2*R2+2*Y0^2*X0^2-2*Y0^2*R2+X0^4+R2^2+2*R1*X0^2-2*R1*R2-2*R1*Y0^2".Parse();
            var xRoots = QuadraticEquationHelper.Solve(eqA, eqXB, eqXC);
            var yRoots = QuadraticEquationHelper.Solve(eqA, eqYB, eqYC);
            Intersections = Tuple.Create(
                new Point(xRoots.Item1, yRoots.Item2),
                new Point(xRoots.Item2, yRoots.Item1)
            );

        }
        public static System.Tuple<Point, Point> Intersect(this Circle c1, Circle c2) {
            var c = c2.Offset(c1.Center.Invert());
            var context = ImmutableContext.Empty
                .Register("R1", c1.R)
                .RegisterCircle(c, "X0", "Y0", "R2");
            return Intersections
                .Substitute(context)
                .FMap(x => x.Offset(c1.Center))
                .FMap(tuple => tuple.FMap(x => x.Convolute()));
        }
    }

    public static class QuadraticEquationHelper {
        static readonly System.Tuple<Expr, Expr> Roots;
        static QuadraticEquationHelper() {
            var A = new Core.ParamExpr("A");
            var B = new Core.ParamExpr("B");
            var C = new Core.ParamExpr("C");
            var D = Build((a, b, c) => Sqrt((b ^ 2) - 4 * a * c), A, B, C);
            var x1 = Build((a, b, d) => (-b + d) / (2 * a), A, B, D).ToLegacy();
            var x2 = Build((a, b, d) => (-b - d) / (2 * a), A, B, D).ToLegacy();
            Roots = Tuple.Create(x1, x2);
        }
        public static System.Tuple<Expr, Expr> Solve(Expr a, Expr b, Expr c) {
            var context = ImmutableContext.Empty
                 .Register("A", a)
                 .Register("B", b)
                 .Register("C", c);
            return Roots.FMap(x => x.Substitute(context));
        }
    }

    public static class Functor {
        public static Point FMap(this Point x, Func<Expr, Expr> f) {
            return new Point(f(x.X), f(x.Y));
        }
        public static Line FMap(this Line x, Func<Expr, Expr> f) {
            return new Line(f(x.A), f(x.B), f(x.C));
        }
        public static Circle FMap(this Circle x, Func<Expr, Expr> f) {
            return new Circle(f(x.X), f(x.Y), f(x.R));
        }
        public static System.Tuple<T, T> FMap<T>(this System.Tuple<T, T> x, Func<T, T> f) {
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
        public static Expr Multiply(this Expr e, Expr multiplier) {
            return Expr.Multiply(e, multiplier);
        }
        public static Expr Power(this Expr e, Expr power) {
            return Expr.Power(e, power);
        }
        public static Expr GetHalf(this Expr e) {
            return e.Multiply(Half);
        }
        //public static bool IsPrimitive(this Point p) {
        //    return p.X is ParameterExpr && p.Y is ParameterExpr;
        //}
        public static ImmutableContext RegisterPoint(this ImmutableContext context, Point p, double x, double y) {
            return context
                .RegisterValue(p.X, x)
                .RegisterValue(p.Y, y);
        }
        public static ImmutableContext RegisterValue(this ImmutableContext context, Expr parameter, double value) {
            return context
                .Register(((ParameterExpr)parameter).ParameterName, Expr.Constant(value));
        }
        public static ImmutableContext RegisterValue(this ImmutableContext context, string nane, double value) {
            return context.RegisterValue(nane.Parse(), value);
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
        public static double ToReal(this Expr expr, ImmutableContext context) {
            return expr.Evaluate(context).ToDouble();
        }
        public static Expr AsConst(this double value) {
            return Expr.Constant(value);
        }
        public static Expr Convolute(this Expr expr) {
            return expr.Visit(new ExprRewriter(new ConvolutionExprBuilder(ContextFactory.Empty)));
        }
        public static Point Offset(this Point p, Point offset) {
            return new Point(Expr.Add(p.X, offset.X), Expr.Add(p.Y, offset.Y));
        }
        public static Point Invert(this Point p) {
            return new Point(Expr.Minus(p.X), Expr.Minus(p.Y));
        }
        public static Point Middle(Point p1, Point p2) {
            return new Point(Expr.Add(p1.X, p2.X).GetHalf(), Expr.Add(p1.Y, p2.Y).GetHalf());
        }
        public static Circle Offset(this Circle c, Point offset) {
            var center = c.Center.Offset(offset);
            return new Circle(center.X, center.Y, c.R);
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