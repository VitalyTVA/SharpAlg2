using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;
using CoreBuilder = SharpAlg.Geo.Core.Builder.CoreBuilder;

namespace SharpAlg.Geo.Core {
    public class Transformer {
        public static readonly Transformer Default = new Transformer(
            add: (b, args) => b.Add(MergeArgs(args, x => x.AsAdd())),
            mult: (b, args) => b.Multiply(MergeArgs(args, x => x.AsMult())),
            power: (b, val, pow) => b.Power(val, pow),
            div: (b, n, d) => b.Divide(n, d),
            sqrt: (b, e) => b.Sqrt(e)
        );
        public static readonly Transformer SingleDiv = new Transformer(
            add: (b, args) => b.Add(MergeArgs(args, x => x.AsAdd())),
            mult: (b, args) => b.Multiply(MergeArgs(args, x => x.AsMult())),
            power: (b, val, pow) => b.Power(val, pow),
            div: SingleDiv_Div,
            sqrt: (b, e) => b.Sqrt(e)
        );

        private static Expr SingleDiv_Div(CoreBuilder b, Expr num, Expr den) {
            var rNum = num;
            var rDen = den;
            num.AsDiv().Do(x => {
                rNum = x.Num;
                rDen = b.Multiply(ImmutableArray.Create(x.Den, den));
            });
            den.AsDiv().Do(x => {
                rDen = x.Num;
                rNum = b.Multiply(ImmutableArray.Create(num, x.Den));
            });
            return b.Divide(rNum, rDen);
        }

        static ExprList MergeArgs(Expr[] args, Func<Expr, ExprList?> getArgs) {
            return args
                .SelectMany(x => getArgs(x) ?? x.Yield())
                .ToImmutableArray();
        }
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