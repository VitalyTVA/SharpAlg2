using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;
using CoreBuilder = SharpAlg.Geo.Core.Builder.CoreBuilder;
using Numerics;
using static SharpAlg.Geo.Core.ExprExtensions;
using System.Collections.Generic;

namespace SharpAlg.Geo.Core {
    public class SingleDivTransformer {
        public static Transformer Create(bool openBraces) {
            var transformer = new SingleDivTransformer(openBraces);
            return new Transformer(
                add: transformer.Add,
                mult: transformer.Mult,
                power: (b, val, pow) => transformer.Power(b, val, pow),
                div: transformer.Div,
                sqrt: (b, e) => Sqrt(b, e)
            );
        }
        readonly bool openBraces;

        SingleDivTransformer(bool openBraces) {
            this.openBraces = openBraces;
        }

        static Expr Sqrt(CoreBuilder b, Expr e) {
            var @const = e.AsConst();
            if(@const != null && @const.Value == BigRational.Zero)
                return Expr.Zero;
            return b.Sqrt(e);
        }

        Expr Power(CoreBuilder b, Expr val, BigInteger pow) {
            if(pow == BigInteger.One)
                return val;
            return val.MatchDefault(
                @default: x => b.Power(x, pow),
                @const: x => Const(BigRational.Pow(x, pow)),
                mult: args => Mult(b, args.Select(x => Power(b, x, pow)).ToArray()),
                power: (v, p) => b.Power(v, pow * p)
            );
        }

        ExprList MergeAddArgs(CoreBuilder builber, IEnumerable<Expr> args) {
            return MergeArgs(builber, args, x => x.AsAdd(), BigRational.Zero, BigRational.Add,
                y => y.Select(x => x.ExprOrMultToKoeffMultInfo(builber))
                    .GroupBy(x => x.Mult, LinqExtensions.CreateEnumerableComparer<Expr>())
                    .Select(x => Mult(builber, Const(x.Aggregate(BigRational.Zero, (acc, val) => acc + val.Koeff)).Yield().Concat(x.Key).ToArray())));
        }
        ExprList MergeMultArgs(CoreBuilder builder, IEnumerable<Expr> args) {
            return MergeArgs(builder, args, x => x.AsMult(), BigRational.One, BigRational.Multiply,
                y => y.Select(x => x.ExprOrPowerToPower())
                    .GroupBy(x => x.Value)
                    .Select(x => Power(builder, x.Key, x.Aggregate(BigInteger.Zero, (acc, val) => acc + val.Power))));
        }
        static ExprList MergeArgs(
            CoreBuilder b,
            IEnumerable<Expr> args,
            Func<Expr, ExprList?> getArgs,
            BigRational aggregateSeed,
            Func<BigRational, BigRational, BigRational> aggregate,
            Func<IEnumerable<Expr>, IEnumerable<Expr>> group) {
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

        Expr Mult(CoreBuilder b, params Expr[] args) {
            if(openBraces) {
                var openBraceArgs = OpenBraces(b, args.Select(x => x.ExprOrAddToAdd()));
                if(openBraceArgs.Length > 1) {
                    return Add(b, openBraceArgs.ToArray());
                }
                return Mult_NoOpenBraces(b, openBraceArgs.Single().ExprOrMultToMult());
            }
            return Mult_NoOpenBraces(b, args);
        }
        Expr Mult_NoOpenBraces(CoreBuilder b, IEnumerable<Expr> openBraceArgs) {
            var mergedArgsNum = MergeMultArgs(b, openBraceArgs.Select(x => x.ExprOrDivToDiv().Num));
            var mergedArgsDen = MergeMultArgs(b, openBraceArgs.Select(x => x.ExprOrDivToDiv().Den));
            if(Equals(mergedArgsNum.First(), Expr.Zero))
                return Expr.Zero;
            return Div(b,
                mergedArgsNum.Length == 1 ? mergedArgsNum.Single() : b.Multiply(mergedArgsNum),
                mergedArgsDen.Length == 1 ? mergedArgsDen.Single() : b.Multiply(mergedArgsDen)
            );
        }

        ExprList OpenBraces(CoreBuilder b, IEnumerable<ExprList> addArgs) {
            if(!addArgs.Any())
                return ImmutableArray<Expr>.Empty;
            return addArgs.Tail()
                .Aggregate<ExprList, IEnumerable<Expr>>(addArgs.First(), (acc, val) => acc.SelectMany(x => val, (x, y) => Mult_NoOpenBraces(b, ImmutableArray.Create(x, y))))
                .ToImmutableArray();
        }

        Expr Add(CoreBuilder b, params Expr[] args) {
            var mergedArgs = MergeAddArgs(b, args);
            return mergedArgs.Length == 1 ? mergedArgs.Single() : b.Add(mergedArgs);
        }

        Expr Div(CoreBuilder b, Expr num, Expr den) {
            if(Equals(num, Expr.Zero))
                return Expr.Zero;
            if(Equals(den, Expr.One))
                return num;

            var numDiv = num.ExprOrDivToDiv();
            var denDiv = den.ExprOrDivToDiv();

            var numWithCoeff = Mult(b, numDiv.Num, denDiv.Den).ExprOrMultToKoeffMultInfo(b);
            var denWithCoeff = Mult(b, numDiv.Den, denDiv.Num).ExprOrMultToKoeffMultInfo(b);
            var gcd = Gcd(numWithCoeff.Mult, denWithCoeff.Mult);

            var finalNum = Mult(b, Const(numWithCoeff.Koeff / denWithCoeff.Koeff).Yield().Concat(Divide(b, numWithCoeff.Mult, gcd)).ToArray());
            var finalDen = Mult(b, Divide(b, denWithCoeff.Mult, gcd));

            if(Equals(finalDen, Expr.One))
                return finalNum;

            return b.Divide(finalNum, finalDen);
        }
        static PowerInfo[] Gcd(ExprList x, ExprList y) {
            var xInfoList = x.Select(a => a.ExprOrPowerToPower());
            var yInfoList = y.Select(a => a.ExprOrPowerToPower());
            return xInfoList
                .Join(yInfoList, a => a.Value, a => a.Value, (a, b) => new PowerInfo(a.Value, BigInteger.Min(a.Power, b.Power)))
                .ToArray();
        }
        Expr[] Divide(CoreBuilder builder, ExprList x, PowerInfo[] gcd) {
            var xInfoList = x.Select(a => a.ExprOrPowerToPower());
            var result = xInfoList.Select(a => {
                var foundGcdPart = gcd.Select(b => (PowerInfo?)b).FirstOrDefault(b => Equals(a.Value, b.Value.Value));
                if(foundGcdPart != null)
                    return new PowerInfo(a.Value, a.Power - foundGcdPart.Value.Power);
                return a;
            }).Where(a => a.Power > 0)
            .Select(a => Power(builder, a.Value, a.Power))
            .ToArray();
            return result.Length > 0 ? result : new[] { Expr.One };
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