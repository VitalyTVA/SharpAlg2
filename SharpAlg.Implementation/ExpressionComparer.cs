using SharpKit.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    [JsType(JsMode.Clr, Filename = SR.JS_Implementation)]
    public class ExpressionEqualityComparer : IExpressionVisitor<bool> {
        protected readonly Expr expr;
        public ExpressionEqualityComparer(Expr expr) {
            this.expr = expr;
        }
        public bool Constant(ConstantExpr constant) {
            return DoEqualityCheck(constant, (x1, x2) => object.Equals(x1.Value, x2.Value));
        }
        public bool Add(AddExpr multi) {
            return CompareMultiExpr(multi);
        }
        public bool Multiply(MultiplyExpr multi) {
            return CompareMultiExpr(multi);
        }
        public bool Power(PowerExpr power) {
            return DoEqualityCheck(power, (x1, x2) => EqualsCore(x1.Left, x2.Left) && EqualsCore(x1.Right, x2.Right));
        }
        public bool Parameter(ParameterExpr parameter) {
            return DoEqualityCheck(parameter, (x1, x2) => x1.ParameterName == x2.ParameterName);
        }
        public bool Function(FunctionExpr functionExpr) {
            return DoEqualityCheck(functionExpr, (x1, x2) => x1.FunctionName == x2.FunctionName && x1.Args.EnumerableEqual(x2.Args, EqualsCore));
        }
        protected bool DoEqualityCheck<T>(T expr2, Func<T, T, bool> equalityCheck) where T : Expr {
            var other = expr as T;
            return other != null && equalityCheck(other, expr2);
        }
        protected bool EqualsCore(Expr expr1, Expr expr2) {
            return expr1.Visit(Clone(expr2));
        }
        protected virtual Func<IEnumerable<Expr>, IEnumerable<Expr>, bool> GetArgsEqualComparer() {
            return (x, y) => x.EnumerableEqual(y, EqualsCore);
        }
        protected virtual ExpressionEqualityComparer Clone(Expr expr) {
            return new ExpressionEqualityComparer(expr);
        }
        bool CompareMultiExpr<T>(T multi) where T : MultiExpr {
            return DoEqualityCheck<T>(multi, (x1, x2) => {
                return GetArgsEqualComparer()(x1.Args, x2.Args);
            });
        }
    }
    [JsType(JsMode.Clr, Filename = SR.JS_Implementation)]
    public class ExpressionEquivalenceComparer : ExpressionEqualityComparer {
        public ExpressionEquivalenceComparer(Expr expr)
            : base(expr) {
        }
        protected override Func<IEnumerable<Expr>, IEnumerable<Expr>, bool> GetArgsEqualComparer() {
            return (x, y) => x.SetEqual(y, EqualsCore);
        }
        protected override ExpressionEqualityComparer Clone(Expr expr) {
            return new ExpressionEquivalenceComparer(expr);
        }
    }
}
