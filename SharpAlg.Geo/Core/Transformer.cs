using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;
using CoreBuilder = SharpAlg.Geo.Core.Builder.CoreBuilder;

namespace SharpAlg.Geo.Core {
    public class Transformer {
        public static readonly Transformer Default = new Transformer(
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


        public static readonly Transformer SingleDiv = new Transformer(
            add: (b, args) => b.Add(MergeAddArgs(args)),
            mult: (b, args) => b.Multiply(MergeMultArgs(args)),
            power: (b, val, pow) => b.Power(val, pow),
            div: SingleDiv_Div,
            sqrt: (b, e) => b.Sqrt(e)
        );

        //static Expr SingleDiv_Div(CoreBuilder b, Expr num, Expr den) {
        //    var numDiv = num.ExprOrDivToDiv();
        //    var denDiv = den.ExprOrDivToDiv();
        //    return b.Divide(
        //        b.Multiply(MergeMultArgs(numDiv.Num, denDiv.Den)),
        //        b.Multiply(MergeMultArgs(numDiv.Den, denDiv.Num))
        //    );
        private static Expr SingleDiv_Div(CoreBuilder b, Expr num, Expr den) {
            var numDiv = num.AsDiv();
            var denDiv = den.AsDiv();
            if(numDiv != null && denDiv != null)
                return b.Divide(
                    b.Multiply(MergeMultArgs(numDiv.Value.Num, denDiv.Value.Den)),
                    b.Multiply(MergeMultArgs(numDiv.Value.Den, denDiv.Value.Num))
                );
            if(numDiv == null && denDiv != null)
                return b.Divide(
                    b.Multiply(MergeMultArgs(num, denDiv.Value.Den)),
                    denDiv.Value.Num
                );
            if(numDiv != null && denDiv == null)
                return b.Divide(
                    numDiv.Value.Num,
                    b.Multiply(MergeMultArgs(numDiv.Value.Den, den))
                );
            return b.Divide(num, den);
        }

        static ExprList MergeAddArgs(params Expr[] args) {
            return MergeArgs(args, x => x.AsAdd(), Expr.Zero);
        }
        static ExprList MergeMultArgs(params Expr[] args) {
            return MergeArgs(args, x => x.AsMult(), Expr.One);
        }
        static ExprList MergeArgs(Expr[] args, Func<Expr, ExprList?> getArgs, Expr filterOutValue) {
            return args
                .Where(x => !Equals(x, filterOutValue))
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