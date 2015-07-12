using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;

namespace SharpAlg.Geo.Core {
    public static class ExpressionPrinter {
        public enum ExpressionOrder {
            Head, Default
        }
        public enum BinaryOperation {
            Add, Multiply
        }
        public enum BinaryOperationEx {
            Add, Subtract, Multiply, Divide
        }
        public enum OperationPriority {
            None, Add, Multiply, Power, Factorial
        }

        //#region inner classes
        ////(JsMode.Prototype, Filename = SR.JS_Implementation_Printer)]
        static class ExpressionWrapperVisitor {
            public static bool ShouldWrap(Expr expr, OperationPriority priority, ExpressionOrder order) {
                throw new NotImplementedException();
            }
            //readonly ExpressionOrder order;
            //readonly OperationPriority priority;
            //readonly IContext context;
            //public ExpressionWrapperVisitor(IContext context, OperationPriority priority, ExpressionOrder order) {
            //    this.context = context;
            //    this.order = order;
            //    this.priority = priority;
            //}
            //public bool Constant(ConstantExpr constant) {
            //    if(constant.Value.IsFraction)
            //        return ShouldWrap(OperationPriority.Power);
            //    if(order == ExpressionOrder.Head)
            //        return false;
            //    return constant.Value < NumberFactory.Zero;
            //}
            //public bool Parameter(ParameterExpr parameter) {
            //    return false;
            //}
            //public bool Add(AddExpr multi) {
            //    return ShouldWrap(OperationPriority.Add);
            //}
            //public bool Multiply(MultiplyExpr multi) {
            //    if(IsMinusExpression(multi))
            //        return true;
            //    return ShouldWrap(OperationPriority.Multiply);
            //}
            //public bool Power(PowerExpr power) {
            //    if(IsInverseExpression(power))
            //        return ShouldWrap(OperationPriority.Multiply);
            //    return ShouldWrap(OperationPriority.Power);
            //}
            //public bool Function(FunctionExpr functionExpr) {
            //    if(IsFactorial(context, functionExpr))
            //        return ShouldWrap(OperationPriority.Factorial);
            //    return false;
            //}
            //bool ShouldWrap(OperationPriority exprPriority) {
            //    return priority >= exprPriority;
            //}
        }
        ////(JsMode.Prototype, Filename = SR.JS_Implementation_Printer)]
        //abstract class UnaryExpressionExtractor : DefaultExpressionVisitor<UnaryExpressionInfo> {
        //    protected abstract BinaryOperation Operation { get; }
        //    protected UnaryExpressionExtractor() {
        //    }
        //    public override UnaryExpressionInfo Constant(ConstantExpr constant) {
        //        return constant.Value >= NumberFactory.Zero || Operation != BinaryOperation.Add ?
        //            base.Constant(constant) :
        //            new UnaryExpressionInfo(Expr.Constant(NumberFactory.Zero - constant.Value), BinaryOperationEx.Subtract);
        //    }
        //    protected override UnaryExpressionInfo GetDefault(Expr expr) {
        //        return new UnaryExpressionInfo(expr, ExpressionEvaluator.GetBinaryOperationEx(Operation));
        //    }
        //}
        ////(JsMode.Prototype, Filename = SR.JS_Implementation_Printer)]
        //class MultiplyUnaryExpressionExtractor : UnaryExpressionExtractor {
        //    public static readonly MultiplyUnaryExpressionExtractor MultiplyInstance = new MultiplyUnaryExpressionExtractor();
        //    protected override BinaryOperation Operation { get { return BinaryOperation.Multiply; } }
        //    protected MultiplyUnaryExpressionExtractor() {
        //    }
        //    public override UnaryExpressionInfo Power(PowerExpr power) {
        //        if(IsInverseExpression(power)) {
        //            return new UnaryExpressionInfo(power.Left, BinaryOperationEx.Divide);
        //        }
        //        return base.Power(power);
        //    }
        //}
        ////(JsMode.Prototype, Filename = SR.JS_Implementation_Printer)]
        //class AddUnaryExpressionExtractor : UnaryExpressionExtractor {
        //    static readonly AddUnaryExpressionExtractor AddInstance = new AddUnaryExpressionExtractor();
        //    public static UnaryExpressionInfo ExtractAddUnaryInfo(Expr expr) {
        //        return expr.Visit(AddInstance);
        //    }
        //    protected override BinaryOperation Operation { get { return BinaryOperation.Add; } }
        //    AddUnaryExpressionExtractor() {
        //    }
        //    public override UnaryExpressionInfo Multiply(MultiplyExpr multi) {
        //        ConstantExpr headConstant = multi.Args.First() as ConstantExpr;
        //        if(headConstant.Return(x => x.Value < NumberFactory.Zero, () => false)) {
        //            ConstantExpr exprConstant = Expr.Constant(NumberFactory.Zero - headConstant.Value);
        //            Expr expr = headConstant.ExprEquals(Expr.MinusOne) ?
        //                multi.Tail() :
        //                Expr.Multiply(exprConstant.AsEnumerable<Expr>().Concat(multi.Args.Tail()));
        //            return new UnaryExpressionInfo(expr, BinaryOperationEx.Subtract);
        //        }
        //        return base.Multiply(multi);
        //    }
        //}
        ////(JsMode.Prototype, Filename = SR.JS_Implementation_Printer)]
        //class UnaryExpressionInfo {
        //    public UnaryExpressionInfo(Expr expr, BinaryOperationEx operation) {
        //        Operation = operation;
        //        Expr = expr;
        //    }
        //    public Expr Expr { get; private set; }
        //    public BinaryOperationEx Operation { get; private set; }
        //}
        //#endregion
        public static string Print(Expr expr) {
            throw new NotImplementedException();
        }

        static bool IsMinusExpression(MultExpr multi) {
            return multi.Args.Count() == 2 && 
                (multi.Args.ElementAt(0) as ConstExpr).If(x => x.Value == BigInteger.MinusOne).ReturnSuccess();
        }
        static string Add(AddExpr multi) {
            throw new NotImplementedException();
            //var sb = new StringBuilder();
            //multi.Args.Accumulate(x => {
            //    sb.Append(x.Visit(this));
            //}, x => {
            //    UnaryExpressionInfo info = AddUnaryExpressionExtractor.ExtractAddUnaryInfo(x);
            //    sb.Append(GetBinaryOperationSymbol(info.Operation));
            //    sb.Append(WrapFromAdd(info.Expr));
            //});
            //return sb.ToString();
        }
        static string Multiply(MultExpr multi) {
            throw new NotImplementedException();
            //if(multi.Args.First().ExprEquals(Expr.MinusOne)) {
            //    string exprText = WrapFromAdd(multi.Tail());
            //    return String.Format("-{0}", exprText);
            //}
            //var sb = new StringBuilder();
            //multi.Args.Accumulate(x => {
            //    sb.Append(WrapFromMultiply(x, ExpressionOrder.Head));
            //}, x => {
            //    UnaryExpressionInfo info = x.Visit(MultiplyUnaryExpressionExtractor.MultiplyInstance);
            //    sb.Append(GetBinaryOperationSymbol(info.Operation));
            //    sb.Append(WrapFromMultiply(info.Expr, ExpressionOrder.Default));
            //});
            //return sb.ToString();
        }
        static string Power(PowerExpr power) {
            return string.Format("{0} ^ {1}", WrapFromPower(power.Value), power.Power);
        }
        static string Parameter(ParamExpr parameter) {
            return parameter.Name;
        }
        static string Sqrt(SqrtExpr sqrtExpr) {
            return string.Format("sqrt({0})", Print(sqrtExpr.Value));
        }
        static string GetBinaryOperationSymbol(BinaryOperationEx operation) {
            switch(operation) {
                case BinaryOperationEx.Add:
                    return " + ";
                case BinaryOperationEx.Subtract:
                    return " - ";
                case BinaryOperationEx.Multiply:
                    return " * ";
                case BinaryOperationEx.Divide:
                    return " / ";
                default:
                    throw new NotImplementedException();
            }
        }
        static OperationPriority GetPriority(BinaryOperation operation) {
            switch(operation) {
                case BinaryOperation.Add:
                    return OperationPriority.Add;
                case BinaryOperation.Multiply:
                    return OperationPriority.Multiply;
                default:
                    throw new NotImplementedException();
            }
        }
        static string WrapFromAdd(Expr expr) {
            return Wrap(expr, OperationPriority.Add, ExpressionOrder.Default);
        }
        static string WrapFromMultiply(Expr expr, ExpressionOrder order) {
            return Wrap(expr, OperationPriority.Multiply, order);
        }
        static string WrapFromPower(Expr expr) {
            return Wrap(expr, OperationPriority.Power, ExpressionOrder.Default);
        }
        static string Wrap(Expr expr, OperationPriority currentPriority, ExpressionOrder order) {
            bool wrap = ExpressionWrapperVisitor.ShouldWrap(expr, currentPriority, order);
            string s = Print(expr);
            if(wrap)
                return "(" + s + ")";
            return s;
        }
    }
}