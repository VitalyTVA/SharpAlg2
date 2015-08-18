using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;
using CoreBuilder = SharpAlg.Geo.Core.Builder.CoreBuilder;
using Numerics;
using static SharpAlg.Geo.Core.ExprExtensions;
using static SharpAlg.Geo.Core.Utility;
using System.Collections.Generic;

namespace SharpAlg.Geo.Core {
    public class SingleDivTransformer {
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        //public static int Success, Fail;
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        //public static Func<CoreBuilder, Transformer> GetFactory(bool openBraces) {
        //    return builder => {
        //        var transformer = new SingleDivTransformer(builder, openBraces);
        //        return new Transformer(
        //            add: Func((ExprList x) => transformer.Add(x)).Memoize2(() => Success++, () => Fail++, LinqExtensions.CreateEnumerableComparer<Expr>()),
        //            mult: Func((ExprList x) => transformer.Mult(x)).Memoize2(() => Success++, () => Fail++, LinqExtensions.CreateEnumerableComparer<Expr>()),
        //            power: Func((PowerInfo x) => transformer.Power(x.Value, x.Power)).Memoize2(() => Success++, () => Fail++),
        //            div: Func((DivInfo x) => transformer.Div(x.Num, x.Den)).Memoize2(() => Success++, () => Fail++),
        //            sqrt: Func((Expr x) => transformer.Sqrt(x)).Memoize2(() => Success++, () => Fail++)
        //        );
        //    };
        //}
        public static Func<CoreBuilder, Transformer> GetFactory(bool openBraces) {
            return builder => {
                var transformer = new SingleDivTransformer(builder, openBraces);
                return new Transformer(
                    add: Func((ExprList x) => transformer.Add(x)),
                    mult: Func((ExprList x) => transformer.Mult(x)),
                    power: Func((PowerInfo x) => transformer.Power(x.Value, x.Power)),
                    div: Func((DivInfo x) => transformer.Div(x.Num, x.Den)),
                    sqrt: Func((Expr x) => transformer.Sqrt(x))
                );
            };
        }
        readonly bool openBraces;
        readonly CoreBuilder builder;

        SingleDivTransformer(CoreBuilder builder, bool openBraces) {
            this.openBraces = openBraces;
            this.builder = builder;
        }

        Expr Sqrt(Expr e) {
            var @const = e.AsConst();
            if(@const != null && @const.Value == BigRational.Zero)
                return Expr.Zero;
            return builder.Sqrt(e);
        }

        Expr Power(Expr val, BigInteger pow) {
            if(pow == BigInteger.One)
                return val;
            return val.MatchDefault(
                @default: x => builder.Power(x, pow),
                @const: x => Const(BigRational.Pow(x, pow)),
                mult: args => Mult(args.Select(x => Power(x, pow)).ToExprList()),
                power: (v, p) => builder.Power(v, pow * p)
            );
        }

        ExprList MergeAddArgs(IEnumerable<Expr> args) {
            return MergeArgs(builder, args, x => x.AsAdd(), BigRational.Zero, BigRational.Add,
                y => y.Select(x => x.ExprOrMultToKoeffMultInfo(builder))
                    .GroupBy(x => x.Mult, LinqExtensions.CreateEnumerableComparer<Expr>())
                    .Select(x => Mult(Const(x.Aggregate(BigRational.Zero, (acc, val) => acc + val.Koeff)).Yield().Concat(x.Key).ToExprList())));
        }
        ExprList MergeMultArgs(IEnumerable<Expr> args) {
            return MergeArgs(builder, args, x => x.AsMult(), BigRational.One, BigRational.Multiply,
                y => y.Select(x => x.ExprOrPowerToPower())
                    .GroupBy(x => x.Value)
                    .Select(x => Power(x.Key, x.Aggregate(BigInteger.Zero, (acc, val) => acc + val.Power))));
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
            return (@const == aggregateSeed && other.Any() ? other : Const(@const).Yield().Concat(other)).ToExprList();
        }

        Expr Mult(ExprList args) {
            if(openBraces) {
                var openBraceArgs = OpenBraces(args.Select(x => x.ExprOrAddToAdd()));
                if(openBraceArgs.Length > 1) {
                    return Add(openBraceArgs.ToExprList());
                }
                return Mult_NoOpenBraces(openBraceArgs.Single().ExprOrMultToMult());
            }
            return Mult_NoOpenBraces(args);
        }
        Expr Mult_NoOpenBraces(IEnumerable<Expr> openBraceArgs) {
            var mergedArgsNum = MergeMultArgs(openBraceArgs.Select(x => x.ExprOrDivToDiv().Num));
            var mergedArgsDen = MergeMultArgs(openBraceArgs.Select(x => x.ExprOrDivToDiv().Den));
            if(Equals(mergedArgsNum.First(), Expr.Zero))
                return Expr.Zero;
            return Div(
                mergedArgsNum.Length == 1 ? mergedArgsNum.Single() : builder.Multiply(mergedArgsNum),
                mergedArgsDen.Length == 1 ? mergedArgsDen.Single() : builder.Multiply(mergedArgsDen)
            );
        }

        ExprList OpenBraces(IEnumerable<ExprList> addArgs) {
            if(!addArgs.Any())
                return ImmutableArray<Expr>.Empty;
            return addArgs.Tail()
                .Aggregate<ExprList, IEnumerable<Expr>>(addArgs.First(), (acc, val) => acc.SelectMany(x => val, (x, y) => Mult_NoOpenBraces(ImmutableArray.Create(x, y))))
                .ToExprList();
        }

        Expr Add(ExprList args) {
            var mergedArgs = MergeAddArgs(args);
            return mergedArgs.Length == 1 ? mergedArgs.Single() : builder.Add(mergedArgs);
        }

        Expr Div(Expr num, Expr den) {
            if(Equals(num, Expr.Zero))
                return Expr.Zero;
            if(Equals(den, Expr.One))
                return num;

            var numDiv = num.ExprOrDivToDiv();
            var denDiv = den.ExprOrDivToDiv();

            var numWithCoeff = Mult(MakeExprList(numDiv.Num, denDiv.Den)).ExprOrMultToKoeffMultInfo(builder);
            var denWithCoeff = Mult(MakeExprList(numDiv.Den, denDiv.Num)).ExprOrMultToKoeffMultInfo(builder);
            var gcd = Gcd(numWithCoeff.Mult, denWithCoeff.Mult);

            var finalNum = Mult(Const(numWithCoeff.Koeff / denWithCoeff.Koeff).Yield().Concat(Divide(numWithCoeff.Mult, gcd)).ToExprList());
            var finalDen = Mult(Divide(denWithCoeff.Mult, gcd));

            if(Equals(finalDen, Expr.One))
                return finalNum;

            return builder.Divide(finalNum, finalDen);
        }
        static PowerInfo[] Gcd(ExprList x, ExprList y) {
            var xInfoList = x.Select(a => a.ExprOrPowerToPower());
            var yInfoList = y.Select(a => a.ExprOrPowerToPower());
            return xInfoList
                .Join(yInfoList, a => a.Value, a => a.Value, (a, b) => new PowerInfo(a.Value, BigInteger.Min(a.Power, b.Power)))
                .ToArray();
        }
        ExprList Divide(ExprList x, PowerInfo[] gcd) {
            var xInfoList = x.Select(a => a.ExprOrPowerToPower());
            var result = xInfoList.Select(a => {
                var foundGcdPart = gcd.Select(b => (PowerInfo?)b).FirstOrDefault(b => Equals(a.Value, b.Value.Value));
                if(foundGcdPart != null)
                    return new PowerInfo(a.Value, a.Power - foundGcdPart.Value.Power);
                return a;
            }).Where(a => a.Power > 0)
            .Select(a => Power(a.Value, a.Power))
            .ToExprList();
            return result.Length > 0 ? result : MakeExprList(Expr.One);
        }
    }
    public static class DefaultTransformer {
        public static Func<CoreBuilder, Transformer> GetFactory() {
            return builder => new Transformer( 
                add: args => builder.Add(MergeAddArgsSimple(args)),
                mult: args => builder.Multiply(MergeMultArgsSimple(args)),
                power: powerInfo => builder.Power(powerInfo.Value, powerInfo.Power),
                div: divInfo => builder.Divide(divInfo.Num, divInfo.Den),
                sqrt: (e) => builder.Sqrt(e)
            );
        }
        static ExprList MergeAddArgsSimple(ExprList args) {
            return MergeArgsSimple(args, x => x.AsAdd());
        }
        static ExprList MergeMultArgsSimple(ExprList args) {
            return MergeArgsSimple(args, x => x.AsMult());
        }
        static ExprList MergeArgsSimple(ExprList args, Func<Expr, ExprList?> getArgs) {
            return args
                .SelectMany(x => getArgs(x) ?? x.Yield())
                .ToExprList();
        }
    }
    public class Transformer {
        public readonly Func<ExprList, Expr> Add;
        public readonly Func<ExprList, Expr> Mult;
        public readonly Func<DivInfo, Expr> Div;
        public readonly Func<Expr, Expr> Sqrt;
        public readonly Func<PowerInfo, Expr> Power;

        public Transformer(
            Func<ExprList, Expr> add,
            Func<ExprList, Expr> mult,
            Func<DivInfo, Expr> div,
            Func<Expr, Expr> sqrt,
            Func<PowerInfo, Expr> power) {
            Add = add;
            Mult = mult;
            Power = power;
            Div = div;
            Sqrt = sqrt;
        }
    }
}