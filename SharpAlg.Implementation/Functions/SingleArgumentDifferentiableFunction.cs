using SharpAlg.Native.Builder;
using SharpKit.JavaScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    [JsType(JsMode.Clr, Filename = SR.JS_Implementation_Functions)]
    public abstract class SingleArgumentDifferentiableFunction : SingleArgumentFunction, ISupportDiff, ISupportConvolution {
        protected SingleArgumentDifferentiableFunction(string name)
            : base(name) {
        }
        public Expr Diff(IDiffExpressionVisitor diffVisitor, IEnumerable<Expr> args) {
            CheckArgsCount(args);
            Expr arg = args.Single();
            return diffVisitor.Builder.Multiply(arg.Visit(diffVisitor), DiffCore(diffVisitor.Builder, arg)); //TODO use builder
        }
        protected abstract Expr DiffCore(ExprBuilder builder, Expr arg);
        public Expr Convolute(IContext context, IEnumerable<Expr> args) {
            var arg = args.Single();
            return EvalConvolution(arg) ??
                ConstantConvolution(arg) ??
                SpecificConvolution(context, arg);
        }
        ConstantExpr EvalConvolution(Expr arg) {
            return arg.ConvertAs<ConstantExpr>().If(x => x.Value.IsFloat).Return(x => Expr.Constant(Evaluate(x.Value)), () => null);
        }
        ConstantExpr ConstantConvolution(Expr arg) {
            return ConvolutionExprBuilder.GetConstValue(arg).With(x => ConvertConstant(x)).With(x => Expr.Constant(x));
        }
        protected virtual Number ConvertConstant(Number n) {
            return null;
        }
        protected virtual Expr SpecificConvolution(IContext context, Expr arg) {
            return null;
        }
    }
}
