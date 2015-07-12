using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;

namespace SharpAlg.Geo.Core {
    public static class ExprPrinter {
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
        static class MultiplyUnaryExpressionExtractor {
            public static UnaryExpressionInfo ExtractMultiplyUnaryInfo(Expr expr) {
                throw new NotImplementedException();
            }
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
        }
        static class AddUnaryExpressionExtractor  {
        //    static readonly AddUnaryExpressionExtractor AddInstance = new AddUnaryExpressionExtractor();
            public static UnaryExpressionInfo ExtractAddUnaryInfo(Expr expr) {
                throw new NotImplementedException();
                //return expr.Visit(AddInstance);
            }
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
        }
        class UnaryExpressionInfo {
            public UnaryExpressionInfo(Expr expr, BinaryOperationEx operation) {
                Operation = operation;
                Expr = expr;
            }
            public Expr Expr { get; private set; }
            public BinaryOperationEx Operation { get; private set; }
        }
        public static string Print(Expr expr) {
            return expr.MatchStrict(
                Add,
                Multiply,
                div => { throw new NotImplementedException(); },
                Power,
                Sqrt,
                Parameter,
                Constant
            );
        }

        static bool IsMinusExpression(MultExpr multi) {
            return multi.Args.Count() == 2 && IsMinusOne(multi.Args.First());
        }
        static bool IsMinusOne(Expr expr) {
            return (expr as ConstExpr).If(x => x.Value == BigInteger.MinusOne).ReturnSuccess();
        }
        static string Add(AddExpr multi) {
            var sb = new StringBuilder();
            sb.Append(Print(multi.Args.First()));
            foreach(var expr in multi.Args.Skip(1)) {
                UnaryExpressionInfo info = AddUnaryExpressionExtractor.ExtractAddUnaryInfo(expr);
                sb.Append(GetBinaryOperationSymbol(info.Operation));
                sb.Append(WrapFromAdd(info.Expr));
            }
            return sb.ToString();
        }
        static string Multiply(MultExpr multi) {
            if(IsMinusOne(multi.Args.First())) {
                string exprText = WrapFromAdd(multi.Tail());
                return String.Format("-{0}", exprText);
            }
            var sb = new StringBuilder();
            sb.Append(WrapFromMultiply(multi.Args.First(), ExpressionOrder.Head));
            foreach(var expr in multi.Args.Skip(1)) {
                UnaryExpressionInfo info = MultiplyUnaryExpressionExtractor.ExtractMultiplyUnaryInfo(expr);
                sb.Append(GetBinaryOperationSymbol(info.Operation));
                sb.Append(WrapFromMultiply(info.Expr, ExpressionOrder.Default));
            }
            return sb.ToString();
        }
        static string Power(PowerExpr power) {
            return string.Format("{0} ^ {1}", WrapFromPower(power.Value), power.Power);
        }
        static string Parameter(ParamExpr parameter) {
            return parameter.Name;
        }
        static string Constant(ConstExpr constant) {
            return constant.Value.ToString();
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
            bool wrap = ShouldWrap(expr, currentPriority, order);
            string s = Print(expr);
            if(wrap)
                return "(" + s + ")";
            return s;
        }
        static bool ShouldWrap(Expr expr, OperationPriority priority, ExpressionOrder order) {
            Func<OperationPriority, bool> shouldWrap = x => priority > x;
            return expr.MatchStrict(
                add: x => shouldWrap(OperationPriority.Add),
                mult: x => IsMinusExpression(x) || shouldWrap(OperationPriority.Multiply),
                div: x => { throw new NotImplementedException(); },
                power: x => shouldWrap(OperationPriority.Power),
                sqrt: x => false,
                param: x => false,
                @const: x => {
                    if(x.Value.IsFraction())
                        return shouldWrap(OperationPriority.Power);
                    if(order == ExpressionOrder.Head)
                        return false;
                    return x.Value < 0;
                }
            );
        }
    }
}