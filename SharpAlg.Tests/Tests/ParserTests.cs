using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Linq.Expressions;
using SharpAlg;
using SharpAlg.Native;
using SharpKit.JavaScript;
using System.IO;
using System.Text;
using SharpAlg.Native.Parser;
using SharpAlg.Native.Builder;
using Parser = SharpAlg.Native.Parser.Parser;

namespace SharpAlg.Tests {
    [JsType(JsMode.Clr, Filename = SR.JSTestsName)]
    [TestFixture]
    public class ParserTests {
        [Test]
        public void ParseNumericTest() {
            Parse("1")
                .AssertValue(1, Expr.One);
            Parse("9 + 13")
                .AssertValue(22, Expr.Add(ExprTestHelper.AsConstant(9), ExprTestHelper.AsConstant(13)));
            Parse("9 + 13 + 117")
                .AssertValue(139, Expr.Add(Expr.Add(ExprTestHelper.AsConstant(9), ExprTestHelper.AsConstant(13)), ExprTestHelper.AsConstant(117)));
            //Parse("x")
            //    .AssertSingleSyntaxError(GetNumberExpectedMessage(1));
            Parse("+")
                .AssertSingleSyntaxError(ParserTestHelper.GetNumberExpectedMessage(1));
            Parse("9+")
                .AssertSingleSyntaxError(ParserTestHelper.GetNumberExpectedMessage(3));
            Parse("9 + ")
                .AssertSingleSyntaxError(ParserTestHelper.GetNumberExpectedMessage(5));

            Parse("13 - 9")
                .AssertValue(4, Expr.Subtract(ExprTestHelper.AsConstant(13), ExprTestHelper.AsConstant(9)));
            Parse("130 - 9 - 2")
                .AssertValue(119, Expr.Subtract(Expr.Subtract(ExprTestHelper.AsConstant(130), ExprTestHelper.AsConstant(9)), ExprTestHelper.AsConstant(2)));
            Parse("130 - 9 + 12 - 4")
                .AssertValue(129, Expr.Subtract(Expr.Add(Expr.Subtract(ExprTestHelper.AsConstant(130), ExprTestHelper.AsConstant(9)), ExprTestHelper.AsConstant(12)), ExprTestHelper.AsConstant(4)));
            Parse("13 -")
                .AssertSingleSyntaxError(ParserTestHelper.GetNumberExpectedMessage(5));

            Parse("2 * 3")
                .AssertValue(6, Expr.Multiply(ExprTestHelper.AsConstant(2), ExprTestHelper.AsConstant(3)));

            Parse("6 / 2")
                .AssertValue(3, Expr.Divide(ExprTestHelper.AsConstant(6), ExprTestHelper.AsConstant(2)));
            Parse("2 ^ 3")
                .AssertValue(8, Expr.Power(ExprTestHelper.AsConstant(2), ExprTestHelper.AsConstant(3)));
            Parse(".234")
                .AssertValue(.234, ExprTestHelper.AsConstant(.234));
            Parse("0.234")
                .AssertValue(.234, ExprTestHelper.AsConstant(.234));
            Parse("-0.234")
                .AssertValue(-.234);
            Parse("-.234")
                .AssertValue(-.234);

            Parse("-x * 2")
                .AssertValue(null, Expr.Multiply(Expr.MinusOne, "x * 2".ParseNoConvolution()));
            Parse("-x ^ 2")
                .AssertValue(null, Expr.Minus("x ^ 2".Parse()));
        }
        public void TODO() {
           "(-x) ^ 2".Parse().AssertSimpleStringRepresentation("x ^ 2");
           "(-x) ^ 3".Parse().AssertSimpleStringRepresentation("-x ^ 3");
        }
        [Test]
        public void OperationsPriorityTest() {
            Parse("1 + 2 * 3")
                .AssertValue(7, Expr.Add(Expr.One, Expr.Multiply(ExprTestHelper.AsConstant(2), ExprTestHelper.AsConstant(3))));

            Parse("1 + 6 / 2")
                .AssertValue(4, Expr.Add(Expr.One, Expr.Divide(ExprTestHelper.AsConstant(6), ExprTestHelper.AsConstant(2))));

            Parse("2 * 3 * 4 / 6 / 2 - 4 / 2")
               .AssertValue(0);

            Parse("2 * 2 ^ 3")
                .AssertValue(16, Expr.Multiply(ExprTestHelper.AsConstant(2), Expr.Power(ExprTestHelper.AsConstant(2), ExprTestHelper.AsConstant(3))));
            Parse("2 + 2 ^ 3")
                .AssertValue(10, Expr.Add(ExprTestHelper.AsConstant(2), Expr.Power(ExprTestHelper.AsConstant(2), ExprTestHelper.AsConstant(3))));
        }
        [Test]
        public void FunctionTest() {
            Parse("ln(1)")
                .AssertValue(null, Expr.Function("ln", Expr.One));
            Parse("ln(x ^ 2 + y * z)")
                .AssertValue(null, Expr.Function("ln", "x ^ 2 + y * z".Parse()));
            Parse("ln()")
                .AssertValue(null, Expr.Function("ln", new Expr[0]));
                //.AssertSingleSyntaxError(ParserTestHelper.GetNumberExpectedMessage(4));
        }
        [Test]
        public void FunctionMultiArgsTest() {
            Parse("someFunc(x, 2, y + x)")
                .AssertValue(null, Expr.Function("someFunc", new Expr[] { "x".Parse(), "2".Parse(), "y + x".Parse() }));
        }
        [Test]
        public void FactorialTest() {
            Parse("x!")
                .AssertValue(null, FunctionFactory.Factorial(Expr.Parameter("x")));
            Parse("x ^ y!")
                .AssertValue(null, Expr.Power(Expr.Parameter("x"), FunctionFactory.Factorial(Expr.Parameter("y"))));
            Parse("!x")
                .AssertSingleSyntaxError(ParserTestHelper.GetNumberExpectedMessage(1));
            Parse("3!")
                .AssertValue(6, FunctionFactory.Factorial(ExprTestHelper.AsConstant(3)));
            Parse("2 ^ 3!")
                .AssertValue(64);
        }
        [Test]
        public void ParenthesesTest() {
            Parse("(1 + 2) * 3")
                .AssertValue(9, Expr.Multiply(Expr.Add(Expr.One, ExprTestHelper.AsConstant(2)), ExprTestHelper.AsConstant(3)));
            Parse("(2 + 4) / (4 / (1 + 1))")
                .AssertValue(3);
        }
        [Test]
        public void ExpressionsWithParameterTest() {
            var context = ContextFactory.CreateEmpty()
                .Register("x", ExprTestHelper.AsConstant(9))
                .Register("someName", ExprTestHelper.AsConstant(13));

            Parse("x")
                .AssertValue(9, Expr.Parameter("x"), context);

            Parse("x * someName")
                .AssertValue(117, Expr.Multiply(Expr.Parameter("x"), Expr.Parameter("someName")), context);

            Parse("(x - 4) * (someName + x)")
                .AssertValue(110, null, context);

            Parse("-x")
                .AssertValue(-9, Expr.Minus(Expr.Parameter("x")), context);
            Parse("-9")
                .AssertValue(-9, Expr.Minus(ExprTestHelper.AsConstant(9)), context);
            Parse("-(x + 1)")
                .AssertValue(-10, Expr.Minus(Expr.Add(Expr.Parameter("x"), ExprTestHelper.AsConstant(1))), context);
            Parse("-(x * 2)")
                .AssertValue(-18, Expr.Minus(Expr.Multiply(Expr.Parameter("x"), ExprTestHelper.AsConstant(2))), context);
            Parse("--(x + 1)")
                .AssertSingleSyntaxError(ParserTestHelper.GetNumberExpectedMessage(2));
            Parse("-(-(x + 1))")
                .AssertValue(10, null, context);
        }
        SharpAlg.Native.Parser.Parser Parse(string expression) {
            return ParserTestHelper.ParseNoConvolutionCore(expression);
        }
    }
    [JsType(JsMode.Clr, Filename = SR.JSTestsName)]
    public static class ParserTestHelper {
        public static SharpAlg.Native.Parser.Parser AssertValue(this SharpAlg.Native.Parser.Parser parser, double? value, Expr expectedExpr = null, Context context = null) {
            return parser
                .IsEqual(x => x.errors.Errors, string.Empty)
                .IsEqual(x => x.errors.Count, 0)
                .IsEqual(x => value != null ? x.Expr.Evaluate(context) : null, value != null ? ExprTestHelper.AsNumber(value.Value) : null)
                .IsTrue(x => expectedExpr == null || x.Expr.ExprEquals(expectedExpr));
        }
        public static SharpAlg.Native.Parser.Parser AssertSingleSyntaxError(this SharpAlg.Native.Parser.Parser parser, string text) {
            return parser.AssertSyntaxErrors(text, 1);
        }
        public static SharpAlg.Native.Parser.Parser AssertSyntaxErrors(this SharpAlg.Native.Parser.Parser parser, string text, int errorCount) {
            return parser.IsEqual(x => x.errors.Count, errorCount).IsEqual(x => x.errors.Errors, text);
        }
        public static Expr ParseNoConvolution(this string expression) {
            return ExpressionExtensions.GetExpression(ParseNoConvolutionCore(expression));
        }
        public static SharpAlg.Native.Parser.Parser ParseNoConvolutionCore(string expression) {
            return ExpressionExtensions.ParseCore(expression, new TrivialExprBuilder(ContextFactory.Empty));
        }
        public static string GetNumberExpectedMessage(int column) {
            return ErrorsBase.GetErrorText(1, column, "invalid Terminal\r\n");
        }
    }
}
