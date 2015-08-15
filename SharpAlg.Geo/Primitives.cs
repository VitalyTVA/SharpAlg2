using System;
using RealPoint = System.Windows.Point;
using static SharpAlg.Geo.Core.ExprExtensions;
using SharpAlg.Geo.Core;

namespace SharpAlg.Geo {
    public class Point {
        public readonly Expr X, Y;
        public Point(Expr x, Expr y) {
            X = x;
            Y = y;
        }
        public override string ToString() {
            return string.Format("({0}, {1})", X, Y);
        }
    }

    public class Line {
        readonly Builder builder;
        public readonly Expr A, B, C;
        public Line(Builder builder, Expr a, Expr b, Expr c) {
            this.builder = builder;
            A = a;
            B = b;
            C = c;
        }
        public override string ToString() {
            return builder.Build((A, B, C, x, y) => A * x + B * y + C, A, B, C, Param("x"), Param("y")).ToString();
        }
    }

    public class Circle {
        readonly Builder builder;
        public readonly Expr X, Y, R;
        public Point Center { get { return new Point(X, Y); } }
        public Circle(Builder builder, Expr x, Expr y, Expr r) {
            this.builder = builder;
            X = x;
            Y = y;
            R = r;
        }
        public override string ToString() {
            return builder.Build((X, Y, R, x, y) => ((x - X) ^ 2) + ((y - Y) ^ 2) - R, X, Y, R, Param("x"), Param("y")).ToString();
        }
    }

    internal static class FunctorExtensions {
        internal static Tuple<TResult, TResult> FMap<T, TResult>(this Tuple<T, T> x, Func<T, TResult> f) {
            return Tuple.Create(f(x.Item1), f(x.Item2));
        }
    }

    internal static class ExprHelper {
        internal static ImmutableContext RegisterPoint(this ImmutableContext context, Point p, double x, double y) {
            return context
                .RegisterValue(p.X, x)
                .RegisterValue(p.Y, y);
        }
        internal static ImmutableContext RegisterValue(this ImmutableContext context, Expr parameter, double value) {
            return context
                .Register(parameter.ToParam(), value);
        }
        internal static RealPoint ToRealPoint(this Point p, ImmutableContext context) {
            return new RealPoint(p.X.ToReal(context), p.Y.ToReal(context));
        }
    }
}