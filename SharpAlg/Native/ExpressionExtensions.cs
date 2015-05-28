using SharpAlg.Native.Builder;
using SharpAlg.Native.Parser;
using SharpAlg.Native.Printer;
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JSNativeName)]
    public static class ExpressionExtensions {
        public static Number Evaluate(this Expr expr, IContext context = null) {
            return expr.Visit(new ExpressionEvaluator(context ?? ContextFactory.Default));
        }
        public static Expr Diff(this Expr expr, string parameterName = null) {
            return expr.Diff(ExprBuilderFactory.CreateDefault(), parameterName);
        }
        public static string Print(this Expr expr, Context context = null) {
            return expr.Visit(ExpressionPrinter.Create(context ?? ContextFactory.Default));
        }
        public static Expr Parse(this string expression, ExprBuilder builder = null) {
            return GetExpression(ParseCore(expression, builder ?? ExprBuilderFactory.CreateDefault()));
        }
        public static Expr GetExpression(Parser.Parser parser) {
            if(parser.errors.Count > 0)
                throw new InvalidOperationException("String can not be parsed"); //TODO message
            return parser.Expr;
        }
        public static Parser.Parser ParseCore(this string expression, ExprBuilder builder) {
            Scanner scanner = new Scanner(expression);
            Parser.Parser parser = new Parser.Parser(scanner, builder);
            parser.Parse();
            return parser;
        }
    }
}