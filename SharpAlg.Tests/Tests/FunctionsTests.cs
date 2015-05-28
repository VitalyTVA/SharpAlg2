using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Linq.Expressions;
using SharpAlg;
using SharpAlg.Native;
//
using SharpAlg.Native.Builder;
using SharpAlg.Native.Parser;

namespace SharpAlg.Tests {
    //(JsMode.Clr, Filename = SR.JSTestsName)]
    [TestFixture]
    public class FunctionsTests {
        const string STR_PiIsAConstantAndCantBeUsedAsFunction = "Pi is a constant and can't be used as function\r\n";

        [Test]
        public void TrigonometryTest() {
            "sin(1)".Parse().IsFloatEqual(x => x.Evaluate(), "0.84147").AssertSimpleStringRepresentation("sin(1)");
            "sin(1.0)".Parse().IsFloatEqual(x => x.Print(), "0.84147");
            "sin(x)".Parse().Diff().AssertSimpleStringRepresentation("cos(x)");
            "sin(-x)".Parse().Diff().AssertSimpleStringRepresentation("-cos(-x)");
            "sin(x ^ 2)".Parse().Diff().AssertSimpleStringRepresentation("2 * x * cos(x ^ 2)");
            "sin(-x ^ 2)".Parse().Diff().AssertSimpleStringRepresentation("-2 * x * cos(-x ^ 2)");

            "cos(1)".Parse().IsFloatEqual(x => x.Evaluate(), "0.54030").AssertSimpleStringRepresentation("cos(1)");
            "cos(1.0)".Parse().IsFloatEqual(x => x.Print(), "0.54030");
            "cos(x)".Parse().Diff().AssertSimpleStringRepresentation("-sin(x)");
            "cos(-x)".Parse().Diff().AssertSimpleStringRepresentation("sin(-x)");
            "cos(x ^ 2)".Parse().Diff().AssertSimpleStringRepresentation("-2 * x * sin(x ^ 2)");
            "cos(-x ^ 2)".Parse().Diff().AssertSimpleStringRepresentation("2 * x * sin(-x ^ 2)");

            //"sin(-1)".Parse().Diff().AssertSimpleStringRepresentation("-sin(1)");
            //"cos(-1)".Parse().Diff().AssertSimpleStringRepresentation("cos(1)");
            //"sin(-x)".Parse().Diff().AssertSimpleStringRepresentation("-sin(x)");
            //"cos(-x)".Parse().Diff().AssertSimpleStringRepresentation("cos(x)");
            //"sin(-2 * x)".Parse().Diff().AssertSimpleStringRepresentation("-sin(2 * x)");
            //"cos(-2 * x)".Parse().Diff().AssertSimpleStringRepresentation("cos(2 * x)");
        }

        [Test]
        public void PiTest() {
            "Pi".Parse().IsFloatEqual(x => x.Evaluate(), "3.14159");
            "Pi()".GetParser().AssertSingleSyntaxError(STR_PiIsAConstantAndCantBeUsedAsFunction);
            "Pi(1)".GetParser().AssertSingleSyntaxError(STR_PiIsAConstantAndCantBeUsedAsFunction);
            "Pi".Parse().Diff().AssertSimpleStringRepresentation("0").AssertIsInteger();
            "Pi".Parse().AssertSimpleStringRepresentation("Pi");
            "Pi + 1.0".Parse().AssertSimpleStringRepresentation("Pi + 1");
        }

        [Test]
        public void ExpTest() {
            Expr.Function("exp", new Expr[] { "1".Parse(), "2".Parse() }).Fails(x => x.Diff(), typeof(InvalidArgumentCountException));

            "exp(0.0)".Parse()
                .AssertIsFloat()
                .AssertSimpleStringRepresentation("1");
            "exp(0)".Parse()
                .AssertIsInteger()
                .IsFloatEqual(x => x.Evaluate(), "1")
                .AssertSimpleStringRepresentation("1");
            "exp(1) + 1.0".Parse()
                .IsFloatEqual(x => x.Evaluate(), "3.718281")
                .AssertSimpleStringRepresentation("exp(1) + 1");
            "exp(2)".Parse()
                .IsFloatEqual(x => x.Evaluate(), "7.389056")
                .AssertSimpleStringRepresentation("exp(2)");
            "exp(2.0)".Parse()
                .IsFloatEqual(x => x.Print(), "7.389056");
            "exp(-1)".Parse()
                .IsFloatEqual(x => x.Evaluate(), "0.367879")
                .AssertSimpleStringRepresentation("exp(-1)");
            "exp(-2)".Parse()
                .IsFloatEqual(x => x.Evaluate(), "0.135335")
                .AssertSimpleStringRepresentation("exp(-2)");

            "exp(x)".Parse()
                .AssertSimpleStringRepresentation("exp(x)")
                .Diff()
                    .AssertSimpleStringRepresentation("exp(x)");
            "exp(x^2)".Parse()
                .AssertSimpleStringRepresentation("exp(x ^ 2)")
                .Diff()
                    .AssertSimpleStringRepresentation("2 * x * exp(x ^ 2)");
            "exp(x!)".Parse()
                .AssertSimpleStringRepresentation("exp(x!)");

            "exp(ln(x))".Parse()
                .AssertSimpleStringRepresentation("x");
            "ln(exp(x))".Parse()
                .AssertSimpleStringRepresentation("x");
            "ln(exp(x^2))".Parse()
                .AssertSimpleStringRepresentation("x ^ 2");
            "ln(x^2)".Parse()
                .AssertSimpleStringRepresentation("2 * ln(x)");
            "ln(x^x)".Parse()
                .AssertSimpleStringRepresentation("x * ln(x)");

            "exp(2 * ln(x))".Parse()
                .AssertSimpleStringRepresentation("x ^ 2");

            "exp(2 * ln(x) ^ 2)".Parse()
                .AssertSimpleStringRepresentation("exp(2 * ln(x) ^ 2)");


            "exp(ln(x) * y * 2)".Parse()
                .AssertSimpleStringRepresentation("x ^ (2 * y)");

            "exp(z * ln(y) * ln(x) * 2)".Parse()
                .AssertSimpleStringRepresentation("y ^ (2 * z * ln(x))");

            "exp(ln(x^2))".Parse()
                .AssertSimpleStringRepresentation("x ^ 2");
            "exp(ln(x^x))".Parse()
                .AssertSimpleStringRepresentation("x ^ x");

        }

        [Test]
        public void LnTest() {
            "ln(1)".Parse()
                .IsEqual(x => x.Evaluate(), 0.0.AsNumber())
                .AssertIsInteger();
            "ln(3)".Parse()
                .IsFloatEqual(x => x.Evaluate(), "1.098612");

            "ln(y * x) + ln(x * y)".Parse().AssertSimpleStringRepresentation("2 * ln(y * x)");
            "ln(2.0)".Parse().IsFloatEqual(x => x.Print(), "0.693147");
            "ln(x * x) + ln(x + x)".Parse().AssertSimpleStringRepresentation("2 * ln(x) + ln(2 * x)");
            "ln(1.0)".Parse().AssertSimpleStringRepresentation("0").AssertIsFloat();
            "ln(1)".Parse().AssertSimpleStringRepresentation("0").AssertIsInteger();

            "ln(x)".Parse().Diff().AssertSimpleStringRepresentation("1 / x");
            "ln(x ^ 2 + 1)".Parse().Diff().AssertSimpleStringRepresentation("2 * x / (x ^ 2 + 1)");
            "ln(x ^ 3)".Parse().Diff().AssertSimpleStringRepresentation("3 / x");
            "ln(-x ^ 2)".Parse().Diff().AssertSimpleStringRepresentation("2 / x");

            "x * ln(2)".Parse().AssertSimpleStringRepresentation("x * ln(2)");
            "ln(x + y) * ln(x * ln(x)) ^ 2".Parse().AssertSimpleStringRepresentation("ln(x + y) * ln(x * ln(x)) ^ 2");
        }
        [Test]
        public void FactorialTest() {
            "3!".Parse().AssertSimpleStringRepresentation("6").AssertIsInteger();
            "3.5!".Parse().AssertSimpleStringRepresentation("3.5!");

            "x! + factorial(y)".Parse().AssertSimpleStringRepresentation("x! + y!");
            "x * y!".Parse().AssertSimpleStringRepresentation("x * y!");
            "x ^ (y + z)!".Parse().AssertSimpleStringRepresentation("x ^ (y + z)!");
            "(y ^ z)!".Parse().AssertSimpleStringRepresentation("(y ^ z)!");
            "y! ^ z!".Parse().AssertSimpleStringRepresentation("y! ^ z!");

            "(y * x)! + (x * y)!".Parse().AssertSimpleStringRepresentation("2 * (y * x)!");
            "someFunc(x, y * x) + someFunc(x, x * y)".Parse().AssertSimpleStringRepresentation("2 * someFunc(x, y * x)");
            "someFunc(x, y * x)! + 2 * someFunc(x, x * y)!".Parse().AssertSimpleStringRepresentation("3 * someFunc(x, y * x)!");
        }
    }
}
