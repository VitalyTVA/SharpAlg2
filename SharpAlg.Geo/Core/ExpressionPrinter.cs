﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        //class ExpressionWrapperVisitor : IExpressionVisitor<bool> {
        //    readonly ExpressionOrder order;
        //    readonly OperationPriority priority;
        //    readonly IContext context;
        //    public ExpressionWrapperVisitor(IContext context, OperationPriority priority, ExpressionOrder order) {
        //        this.context = context;
        //        this.order = order;
        //        this.priority = priority;
        //    }
        //    public bool Constant(ConstantExpr constant) {
        //        if(constant.Value.IsFraction)
        //            return ShouldWrap(OperationPriority.Power);
        //        if(order == ExpressionOrder.Head)
        //            return false;
        //        return constant.Value < NumberFactory.Zero;
        //    }
        //    public bool Parameter(ParameterExpr parameter) {
        //        return false;
        //    }
        //    public bool Add(AddExpr multi) {
        //        return ShouldWrap(OperationPriority.Add);
        //    }
        //    public bool Multiply(MultiplyExpr multi) {
        //        if(IsMinusExpression(multi))
        //            return true;
        //        return ShouldWrap(OperationPriority.Multiply);
        //    }
        //    public bool Power(PowerExpr power) {
        //        if(IsInverseExpression(power))
        //            return ShouldWrap(OperationPriority.Multiply);
        //        return ShouldWrap(OperationPriority.Power);
        //    }
        //    public bool Function(FunctionExpr functionExpr) {
        //        if(IsFactorial(context, functionExpr))
        //            return ShouldWrap(OperationPriority.Factorial);
        //        return false;
        //    }
        //    bool ShouldWrap(OperationPriority exprPriority) {
        //        return priority >= exprPriority;
        //    }
        //}
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
        static bool IsMinusExpression(MultExpr multi) {
            throw new NotImplementedException();
            //return multi.Args.Count() == 2 && Expr.MinusOne.ExprEquals(multi.Args.ElementAt(0));
        }
        static bool IsInverseExpression(PowerExpr power) {
            throw new NotImplementedException();
            //return Expr.MinusOne.ExprEquals(power.Right);
        }
        public static string Add(AddExpr multi) {
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
        public static string Multiply(MultExpr multi) {
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
        public static string Power(PowerExpr power) {
            throw new NotImplementedException();
            //if(IsInverseExpression(power)) {
            //    return String.Format("1 / {0}", WrapFromMultiply(power.Left, ExpressionOrder.Default));
            //}
            //return string.Format("{0} ^ {1}", WrapFromPower(power.Left), WrapFromPower(power.Right));
        }
        public static string Parameter(ParamExpr parameter) {
            return parameter.Name;
        }
        public static string Sqrt(SqrtExpr sqrtExpr) {
            throw new NotImplementedException();
            //var sb = new StringBuilder(functionExpr.FunctionName);
            //if(!(context.GetFunction(functionExpr.FunctionName) is IConstantFunction)) {
            //    sb.Append("(");
            //    functionExpr.Args.Accumulate(x => {
            //        sb.Append(x.Visit(this));
            //    }, x => {
            //        sb.Append(", ");
            //        sb.Append(x.Visit(this));
            //    });
            //    sb.Append(")");
            //}
            //return sb.ToString();
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
        public static OperationPriority GetPriority(BinaryOperation operation) {
            switch(operation) {
                case BinaryOperation.Add:
                    return OperationPriority.Add;
                case BinaryOperation.Multiply:
                    return OperationPriority.Multiply;
                default:
                    throw new NotImplementedException();
            }
        }
        //string WrapFromAdd(Expr expr) {
        //    return Wrap(expr, OperationPriority.Add, ExpressionOrder.Default);
        //}
        //string WrapFromMultiply(Expr expr, ExpressionOrder order) {
        //    return Wrap(expr, OperationPriority.Multiply, order);
        //}
        //string WrapFromPower(Expr expr) {
        //    return Wrap(expr, OperationPriority.Power, ExpressionOrder.Default);
        //}
        //string Wrap(Expr expr, OperationPriority currentPriority, ExpressionOrder order) {
        //    bool wrap = expr.Visit(new ExpressionWrapperVisitor(context, currentPriority, order));
        //    string s = expr.Visit(this);
        //    if(wrap)
        //        return "(" + s + ")";
        //    return s;
        //}
    }
}