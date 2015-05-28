using SharpAlg.Native;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Core)]
    //[DebuggerDisplay("Expr: {PrintDebug()}")]
    public abstract class Expr {
        public static readonly ConstantExpr Zero = new ConstantExpr(NumberFactory.Zero);
        public static readonly ConstantExpr One = new ConstantExpr(NumberFactory.One);
        public static readonly ConstantExpr MinusOne = new ConstantExpr(NumberFactory.MinusOne);
        public static ConstantExpr Constant(Number constant) {
            return new ConstantExpr(constant);
        }
        public static ParameterExpr Parameter(string parameterName) {
            return new ParameterExpr(parameterName);
        }
        public static Expr Binary(Expr left, Expr right, BinaryOperation type) {
            return Multi(left.Combine(right), type);
        }
        public static Expr Multi(IEnumerable<Expr> args, BinaryOperation type) {
            return args.Count() > 1 ? (type == BinaryOperation.Add ? (MultiExpr)new AddExpr(args) : new MultiplyExpr(args)) : args.Single();
        }
        public static Expr Add(IEnumerable<Expr> args) {
            return Multi(args, BinaryOperation.Add);
        }
        public static Expr Multiply(IEnumerable<Expr> args) {
            return Multi(args, BinaryOperation.Multiply);
        }
        public static Expr Add(Expr left, Expr right) {
            return Binary(left, right, BinaryOperation.Add);
        }
        public static Expr Subtract(Expr left, Expr right) {
            return Add(left, Minus(right));
        }
        public static Expr Multiply(Expr left, Expr right) {
            return Binary(left, right, BinaryOperation.Multiply);
        }
        public static Expr Divide(Expr left, Expr right) {
            return Multiply(left, Inverse(right));
        }
        public static PowerExpr Power(Expr left, Expr right) {
            return new PowerExpr(left, right);
        }
        public static Expr Minus(Expr expr) {
            return Multiply(Expr.MinusOne, expr);
        }
        public static Expr Inverse(Expr expr) {
            return Power(expr, Expr.MinusOne);
        }
        public static FunctionExpr Function(string functionName, Expr argument) {
            return Function(functionName, argument.AsEnumerable());
        }
        public static FunctionExpr Function(string functionName, IEnumerable<Expr> arguments) {
            return new FunctionExpr(functionName, arguments);
        }
        public abstract T Visit<T>(IExpressionVisitor<T> visitor);
#if DEBUG
        //(Code = "return \"\";")]
        //public string PrintDebug() {
        //    var type = Type.GetType("SharpAlg.Native.ExpressionExtensions, SharpAlg, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        //    var method = type.In("Print", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        //    return (string)method.Invoke(null, new object[] { this, null });
        //}
#endif
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public class ConstantExpr : Expr {
        internal ConstantExpr(Number value) {
            Value = value;
        }
        public Number Value { get; private set; }
        public override T Visit<T>(IExpressionVisitor<T> visitor) {
            return visitor.Constant(this);
        }
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public class ParameterExpr : Expr {
        internal ParameterExpr(string parameterName) {
            ParameterName = parameterName;
        }
        public string ParameterName { get; private set; }
        public override T Visit<T>(IExpressionVisitor<T> visitor) {
            return visitor.Parameter(this);
        }
    }
    public enum BinaryOperation {
        Add, Multiply
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public abstract class MultiExpr : Expr {
        internal MultiExpr(IEnumerable<Expr> args) {
            Args = args;
        }
        public IEnumerable<Expr> Args { get; private set; }
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public class AddExpr : MultiExpr {
        internal AddExpr(IEnumerable<Expr> args)
            : base(args) {
        }
        public override T Visit<T>(IExpressionVisitor<T> visitor) {
            return visitor.Add(this);
        }
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public class MultiplyExpr : MultiExpr {
        internal MultiplyExpr(IEnumerable<Expr> args)
            : base(args) {
        }
        public override T Visit<T>(IExpressionVisitor<T> visitor) {
            return visitor.Multiply(this);
        }
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public class PowerExpr : Expr {
        internal PowerExpr(Expr left, Expr right) {
            Right = right;
            Left = left;
        }
        public Expr Left { get; private set; }
        public Expr Right { get; private set; }
        public override T Visit<T>(IExpressionVisitor<T> visitor) {
            return visitor.Power(this);
        }
    }
    //(JsMode.Clr, Filename = SR.JS_Core)]
    public class FunctionExpr : Expr {
        internal FunctionExpr(string functionName, IEnumerable<Expr> arguments) {
            Args = arguments;
            FunctionName = functionName;
        }
        public string FunctionName { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }
        public override T Visit<T>(IExpressionVisitor<T> visitor) {
            return visitor.Function(this);
        }
    }
}
