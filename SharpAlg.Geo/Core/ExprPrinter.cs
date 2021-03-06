﻿using Numerics;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using ExprList = SharpAlg.Geo.Core.ImmutableListWrapper<SharpAlg.Geo.Core.Expr>;

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
                Func<BigRational, UnaryExpressionInfo> getConstant = x => Constant(BinaryOperation.Multiply, x);
                return expr.MatchDefault(
                    getDefault,
                    @const: getConstant
                );
            }
            public static UnaryExpressionInfo ExtractAddUnaryInfo(Expr expr) {
                Func<Expr, UnaryExpressionInfo> getDefault = x => GetDefault(BinaryOperation.Add, x);
                Func<BigRational, UnaryExpressionInfo> getConstant = x => Constant(BinaryOperation.Add, x);
                return expr.MatchDefault(
                    getDefault,
                    mult: args => {
                        var headConstant = args.First().AsConst();
                        if(headConstant.Return(x => x < 0, () => false)) {
                            var exprConstant = ExprExtensions.Const(-headConstant.Value);

                            var tail = GetTail(args);
                            Expr expr2 = (headConstant.Value == BigInteger.MinusOne) ?
                                tail :
                                Builder.Simple.Multiply(exprConstant.Yield().Concat(tail.ToMult()).ToExprList());
                            return new UnaryExpressionInfo(expr2, BinaryOperationEx.Subtract);
                        }
                        return getDefault(Builder.Simple.Multiply(args.ToExprList()));
                    },
                    @const: getConstant
                );
            }

            static UnaryExpressionInfo Constant(BinaryOperation operation, BigRational value) {
                return value >= 0 || operation != BinaryOperation.Add ?
                    GetDefault(operation, ExprExtensions.Const(value)) :
                    new UnaryExpressionInfo(ExprExtensions.Const(0 - value), BinaryOperationEx.Subtract);
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
                param => param,
                Constant
            );
        }
        static Expr GetTail(ExprList args) {
            return Builder.Simple.Multiply(args.Tail().ToExprList());
        }
        static bool IsMinusExpression(ExprList args) {
            return args.Count() == 2 && IsMinusOne(args.First());
        }
        static bool IsMinusOne(Expr expr) {
            return expr.AsConst().If<BigRational>(x => x == BigInteger.MinusOne).ReturnSuccess();
        }
        static string Add(ExprList args) {
            var sb = new StringBuilder();
            sb.Append(args.First().Print());
            foreach(var expr in args.Tail()) {
                UnaryExpressionInfo info = UnaryExpressionExtractor.ExtractAddUnaryInfo(expr);
                sb.Append(GetBinaryOperationSymbol(info.Operation));
                sb.Append(WrapFromAdd(info.Expr));
            }
            return sb.ToString();
        }
        static string Multiply(ExprList args) {
            if(IsMinusOne(args.First())) {
                string exprText = WrapFromAdd(GetTail(args));
                return string.Format("-{0}", exprText);
            }
            var sb = new StringBuilder();
            sb.Append(WrapFromMultiply(args.First(), ExpressionOrder.Head));
            foreach(var expr in args.Tail()) {
                UnaryExpressionInfo info = UnaryExpressionExtractor.ExtractMultiplyUnaryInfo(expr);
                sb.Append(GetBinaryOperationSymbol(info.Operation));
                sb.Append(WrapFromMultiply(info.Expr, ExpressionOrder.Default));
            }
            return sb.ToString();
        }
        static string Divide(Expr num, Expr den) {
            return string.Format("{0} / {1}", WrapFromDivide(num), WrapFromDivide(den));
        }
        static string Power(Expr value, BigInteger power) {
            return string.Format("{0} ^ {1}", WrapFromPower(value), power);
        }
        static string Constant(BigRational value) {
            return value.IsFraction() ? value.ToString() : value.Numerator.ToString();
        }
        static string Sqrt(Expr expr) {
            return string.Format("sqrt({0})", expr.Print());
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
                div: (x, y) => shouldWrap(OperationPriority.Divide),
                power: (x, y) => true, //shouldWrap(OperationPriority.Power),
                sqrt: x => false,
                param: x => false,
                @const: x => {
                    if(x.IsFraction())
                        return shouldWrap(OperationPriority.Power);
                    if(order == ExpressionOrder.Head)
                        return false;
                    return x < 0;
                }
            );
        }
    }
}