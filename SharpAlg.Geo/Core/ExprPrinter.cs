using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SharpAlg.Geo.Core {
    public static class ExprPrinter {
        enum ExpressionOrder {
            Head, Default
        }
        enum BinaryOperation {
            Add, Multiply
        }
        enum BinaryOperationEx {
            Add, Subtract, Multiply, Divide
        }
        enum OperationPriority {
            None, Add, Multiply, Divide, Power
        }

        static class UnaryExpressionExtractor {
            public static UnaryExpressionInfo ExtractMultiplyUnaryInfo(Expr expr) {
                Func<Expr, UnaryExpressionInfo> getDefault = x => GetDefault(BinaryOperation.Multiply, x);
                Func<ConstExpr, UnaryExpressionInfo> getConstant = x => Constant(BinaryOperation.Multiply, x);
                return expr.MatchDefault(
                    getDefault,
                    @const: getConstant
                );
            }
            public static UnaryExpressionInfo ExtractAddUnaryInfo(Expr expr) {
                Func<Expr, UnaryExpressionInfo> getDefault = x => GetDefault(BinaryOperation.Add, x);
                Func<ConstExpr, UnaryExpressionInfo> getConstant = x => Constant(BinaryOperation.Add, x);
                return expr.MatchDefault(
                    getDefault,
                    mult: multi => {
                        ConstExpr headConstant = multi.Args.First() as ConstExpr;
                        if(headConstant.Return(x => x.Value < 0, () => false)) {
                            var exprConstant = ExprExtensions.Const(-headConstant.Value);
                            Expr expr2 = (headConstant.Value == BigInteger.MinusOne) ?
                                multi.Tail() :
                                ExprExtensions.Multiply(exprConstant.Yield().Concat(multi.Args.Tail()).ToArray());
                            return new UnaryExpressionInfo(expr2, BinaryOperationEx.Subtract);
                        }
                        return getDefault(multi);
                    },
                    @const: getConstant
                );
            }

            static UnaryExpressionInfo Constant(BinaryOperation operation, ConstExpr constant) {
                return constant.Value >= 0 || operation != BinaryOperation.Add ?
                    GetDefault(operation, constant) :
                    new UnaryExpressionInfo(ExprExtensions.Const(0 - constant.Value), BinaryOperationEx.Subtract);
            }
            static UnaryExpressionInfo GetDefault(BinaryOperation operation, Expr expr) {
                return new UnaryExpressionInfo(expr, GetBinaryOperationEx(operation));
            }
        }
        class UnaryExpressionInfo {
            public UnaryExpressionInfo(Expr expr, BinaryOperationEx operation) {
                Operation = operation;
                Expr = expr;
            }
            public Expr Expr { get; private set; }
            public BinaryOperationEx Operation { get; private set; }
        }
        public static string Print(this Expr expr) {
            return expr.MatchStrict(
                Add,
                Multiply,
                Divide,
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
            sb.Append(multi.Args.First().Print());
            foreach(var expr in multi.Args.Tail()) {
                UnaryExpressionInfo info = UnaryExpressionExtractor.ExtractAddUnaryInfo(expr);
                sb.Append(GetBinaryOperationSymbol(info.Operation));
                sb.Append(WrapFromAdd(info.Expr));
            }
            return sb.ToString();
        }
        static string Multiply(MultExpr multi) {
            if(IsMinusOne(multi.Args.First())) {
                string exprText = WrapFromAdd(multi.Tail());
                return string.Format("-{0}", exprText);
            }
            var sb = new StringBuilder();
            sb.Append(WrapFromMultiply(multi.Args.First(), ExpressionOrder.Head));
            foreach(var expr in multi.Args.Tail()) {
                UnaryExpressionInfo info = UnaryExpressionExtractor.ExtractMultiplyUnaryInfo(expr);
                sb.Append(GetBinaryOperationSymbol(info.Operation));
                sb.Append(WrapFromMultiply(info.Expr, ExpressionOrder.Default));
            }
            return sb.ToString();
        }
        static string Divide(DivExpr div) {
            return string.Format("{0} / {1}", WrapFromDivide(div.Numerator), WrapFromDivide(div.Denominator));
        }
        static string Power(PowerExpr power) {
            return string.Format("{0} ^ {1}", WrapFromPower(power.Value), power.Power);
        }
        static string Parameter(ParamExpr parameter) {
            return parameter.Name;
        }
        static string Constant(ConstExpr constant) {
            return constant.Value.IsFraction() ? constant.Value.ToString() : constant.Value.Numerator.ToString();
        }
        static string Sqrt(SqrtExpr sqrtExpr) {
            return string.Format("sqrt({0})", sqrtExpr.Value.Print());
        }
        static BinaryOperationEx GetBinaryOperationEx(BinaryOperation operation) {
            switch(operation) {
                case BinaryOperation.Add:
                    return BinaryOperationEx.Add;
                case BinaryOperation.Multiply:
                    return BinaryOperationEx.Multiply;
                default:
                    throw new NotImplementedException();
            }
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
        static string WrapFromAdd(Expr expr) {
            return Wrap(expr, OperationPriority.Add, ExpressionOrder.Default);
        }
        static string WrapFromDivide(Expr expr) {
            return Wrap(expr, OperationPriority.Divide, ExpressionOrder.Default);
        }
        static string WrapFromMultiply(Expr expr, ExpressionOrder order) {
            return Wrap(expr, OperationPriority.Multiply, order);
        }
        static string WrapFromPower(Expr expr) {
            return Wrap(expr, OperationPriority.Power, ExpressionOrder.Default);
        }
        static string Wrap(Expr expr, OperationPriority currentPriority, ExpressionOrder order) {
            bool wrap = ShouldWrap(expr, currentPriority, order);
            string s = expr.Print();
            if(wrap)
                return "(" + s + ")";
            return s;
        }
        static bool ShouldWrap(Expr expr, OperationPriority priority, ExpressionOrder order) {
            Func<OperationPriority, bool> shouldWrap = x => priority >= x;
            return expr.MatchStrict(
                add: x => shouldWrap(OperationPriority.Add),
                mult: x => IsMinusExpression(x) || shouldWrap(OperationPriority.Multiply),
                div: x => shouldWrap(OperationPriority.Divide),
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