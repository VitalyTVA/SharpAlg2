﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;
using CoreBuilder = SharpAlg.Geo.Core.Builder.CoreBuilder;
using Numerics;
using static SharpAlg.Geo.Core.ExprExtensions;
using System.Collections.Generic;

namespace SharpAlg.Geo.Core {
    public static class SingleDivTransformer {
        public static readonly Transformer Instance = new Transformer(
            add: Add,
            mult: Mult,
            power: (b, val, pow) => Power(b, val, pow),
            div: Div,
            sqrt: (b, e) => b.Sqrt(e)
        );

        static Expr Power(CoreBuilder b, Expr val, BigInteger pow) {
            if(pow == BigInteger.One)
                return val;
            return val.MatchDefault(
                @default: x => b.Power(x, pow),
                @const: x => Const(BigRational.Pow(x, pow)),
                mult: args => Mult(b, args.Select(x => Power(b, x, pow)).ToArray()),
                power: (v, p) => b.Power(v, pow * p)
            );
        }

        static ExprList MergeAddArgs(CoreBuilder b, IEnumerable<Expr> args) {
            return MergeArgs(b, args, x => x.AsAdd(), BigRational.Zero, BigRational.Add, 
                y => y.Select(x => x.ExprOrMultToKoeffMultInfo(b))
                    .GroupBy(x => x.Mult)
                    .Select(x => Mult(b, new Expr[] { Const(x.Aggregate(BigRational.Zero, (acc, val) => acc + val.Koeff)), x.Key })));
        }
        static ExprList MergeMultArgs(CoreBuilder b, IEnumerable<Expr> args) {
            return MergeArgs(b, args, x => x.AsMult(), BigRational.One, BigRational.Multiply, 
                y => y.Select(x => x.ExprOrPowerToPower())
                    .GroupBy(x => x.Value)
                    .Select(x => Power(b, x.Key, x.Aggregate(BigInteger.Zero, (acc, val) => acc + val.Power))));
        }
        static ExprList MergeArgs(
            CoreBuilder b,
            IEnumerable<Expr> args, 
            Func<Expr, ExprList?> getArgs, 
            BigRational aggregateSeed, 
            Func<BigRational, BigRational, BigRational> aggregate,
            Func<IEnumerable<Expr>, IEnumerable<Expr>> group) 
        {
            var mergedArgs = args
                .SelectMany(x => getArgs(x) ?? x.Yield());
            var @const = mergedArgs
                .Select(x => x.AsConst())
                .Where(x => x != null)
                .Select(x => x.Value)
                .Aggregate(aggregateSeed, aggregate);
            var other = group(mergedArgs.Where(x => !x.IsConst()));
            return (@const == aggregateSeed && other.Any() ? other : Const(@const).Yield().Concat(other)).ToImmutableArray();
        }

        static Expr Mult(CoreBuilder b, params Expr[] args) {
            var mergedArgsNum = MergeMultArgs(b, args.Select(x => x.ExprOrDivToDiv().Num));
            var mergedArgsDen = MergeMultArgs(b, args.Select(x => x.ExprOrDivToDiv().Den));
            if(Equals(mergedArgsNum.First(), Expr.Zero))
                return Expr.Zero;
            return Div(b,
                mergedArgsNum.Length == 1 ? mergedArgsNum.Single() : b.Multiply(mergedArgsNum),
                mergedArgsDen.Length == 1 ? mergedArgsDen.Single() : b.Multiply(mergedArgsDen)
            );
        }

        static Expr Add(CoreBuilder b, params Expr[] args) {
            var mergedArgs = MergeAddArgs(b, args);
            return mergedArgs.Length == 1 ? mergedArgs.Single() : b.Add(mergedArgs);
        }

        static Expr Div(CoreBuilder b, Expr num, Expr den) {
            if(Equals(den, Expr.One))
                return num;
            var numDiv = num.ExprOrDivToDiv();
            var denDiv = den.ExprOrDivToDiv();
            return b.Divide(
                Mult(b, numDiv.Num, denDiv.Den),
                Mult(b, numDiv.Den, denDiv.Num)
            );
        }
    }
    public static class DefaultTransformer {
        public static readonly Transformer Instance = new Transformer(
            add: (b, args) => b.Add(MergeAddArgsSimple(args)),
            mult: (b, args) => b.Multiply(MergeMultArgsSimple(args)),
            power: (b, val, pow) => b.Power(val, pow),
            div: (b, n, d) => b.Divide(n, d),
            sqrt: (b, e) => b.Sqrt(e)
        );
        static ExprList MergeAddArgsSimple(params Expr[] args) {
            return MergeArgsSimple(args, x => x.AsAdd());
        }
        static ExprList MergeMultArgsSimple(params Expr[] args) {
            return MergeArgsSimple(args, x => x.AsMult());
        }
        static ExprList MergeArgsSimple(Expr[] args, Func<Expr, ExprList?> getArgs) {
            return args
                .SelectMany(x => getArgs(x) ?? x.Yield())
                .ToImmutableArray();
        }
    }
    public class Transformer {
        public readonly Func<CoreBuilder, Expr[], Expr> Add;
        public readonly Func<CoreBuilder, Expr[], Expr> Mult;
        public readonly Func<CoreBuilder, Expr, Expr, Expr> Div;
        public readonly Func<CoreBuilder, Expr, Expr> Sqrt;
        public readonly Func<CoreBuilder, Expr, BigInteger, Expr> Power;

        public Transformer(
            Func<CoreBuilder, Expr[], Expr> add,
            Func<CoreBuilder, Expr[], Expr> mult,
            Func<CoreBuilder, Expr, Expr, Expr> div,
            Func<CoreBuilder, Expr, Expr> sqrt,
            Func<CoreBuilder, Expr, BigInteger, Expr> power) {
            Add = add;
            Mult = mult;
            Power = power;
            Div = div;
            Sqrt = sqrt;
        }
    }
}