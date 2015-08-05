﻿using System;
using RealPoint = System.Windows.Point;
using static SharpAlg.Geo.Core.ExprExtensions;
using Numerics;
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
        public readonly Expr A, B, C;
        public Line(Expr a, Expr b, Expr c) {
            A = a;
            B = b;
            C = c;
        }
        public override string ToString() {
            return new Builder().Build((A, B, C, x, y) => A * x + B * y + C, A, B, C, Param("x"), Param("y")).ToString();
        }
    }

    public class Circle {
        public readonly Expr X, Y, R;
        public Point Center { get { return new Point(X, Y); } }
        public Circle(Expr x, Expr y, Expr r) {
            X = x;
            Y = y;
            R = r;
        }
        public override string ToString() {
            return new Builder().Build((X, Y, R, x, y) => ((x - X) ^ 2) + ((y - Y) ^ 2) - R, X, Y, R, Param("x"), Param("y")).ToString();
        }
    }

    public static class FunctorExtensions {
        public static Tuple<TResult, TResult> FMap<T, TResult>(this Tuple<T, T> x, Func<T, TResult> f) {
            return Tuple.Create(f(x.Item1), f(x.Item2));
        }
    }

    internal static class ExprHelper {
        public static Expr Square(this Expr e) {
            return Power(e, 2);
        }
        public static ImmutableContext RegisterPoint(this ImmutableContext context, Point p, double x, double y) {
            return context
                .RegisterValue((ParamExpr)p.X, x)
                .RegisterValue((ParamExpr)p.Y, y);
        }
        public static ImmutableContext RegisterValue(this ImmutableContext context, ParamExpr parameter, double value) {
            return context
                .Register(parameter.Name, value);
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
        public static Circle Offset(this Circle c, Point offset) {
            var center = c.Center.Offset(offset);
            return new Circle(center.X, center.Y, c.R);
        }
    }
}