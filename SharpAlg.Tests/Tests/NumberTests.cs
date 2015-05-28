using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Linq.Expressions;
using SharpAlg;
using SharpAlg.Native;
//
using SharpAlg.Native.Builder;
using System.Diagnostics;

namespace SharpAlg.Tests {
    //(JsMode.Clr, Filename = SR.JSTestsName)]
    [TestFixture]
    public class NumberTests {
        [Test]
        public void DoubleOperationsTest() {
            "1.2".Add("2.3").AssertFloatNumber("3.5");
            "9.0".Multiply("13.0").AssertFloatNumber("117");
            "9.0".Subtract("13.0").AssertFloatNumber("-4");
            "3.0".Divide("2.0").AssertFloatNumber("1.5");
            "1593668734.0".Divide("1287293.0").AssertFloatNumber("1238");
            "117.0".Power("5.0").AssertFloatNumber("21924480357");
            "1000000001.0".Add("500000001.0").AssertFloatNumber("1500000002");
            "-1000000001.0".Add("500000001.0").AssertFloatNumber("-500000000");

            "100000000001.0".Equal("100000000001.0").IsTrue();
            "100000000001.0".Equal("100000000000.0").IsFalse();
            "100000000001.0".NotEqual("100000000000.0").IsTrue();
            "100000000001.0".NotEqual("100000000001.0").IsFalse();

            "100000000001.0".Less("100000000001.0").IsFalse();
            "100000000001.0".Less("100000000002.0").IsTrue();
            "100000000001.0".LessOrEqual("100000000002.0").IsTrue();
            "100000000002.0".LessOrEqual("100000000001.0").IsFalse();

            "100000000001.0".Greater("100000000001.0").IsFalse();
            "100000000002.0".Greater("100000000001.0").IsTrue();
            "100000000001.0".GreaterOrEqual("100000000001.0").IsTrue();
            "100000000001.0".GreaterOrEqual("100000000002.0").IsFalse();
        }
        [Test]
        public void IntOperationsTest() {
            "1".Add("2").AssertIntegerNumber("3");
            "9".Subtract("13").AssertIntegerNumber("-4");
            "9".Multiply("13").AssertIntegerNumber("117");
            "-9".Multiply("-13").AssertIntegerNumber("117");
            "9".Multiply("0").AssertIntegerNumber("0");
            "0".Multiply("9").AssertIntegerNumber("0");
            "9".Multiply("-13").AssertIntegerNumber("-117");
            "-9".Multiply("13").AssertIntegerNumber("-117");
            "1593668734".Divide("1287293").AssertIntegerNumber("1238");
            "117".Power("5").AssertIntegerNumber("21924480357");
            "1000000001".Add("500000001").AssertIntegerNumber("1500000002");
            "-1000000001".Add("500000001").AssertIntegerNumber("-500000000");

            "100000000001".Equal("100000000001").IsTrue();
            "100000000001".Equal("100000000000").IsFalse();
            "100000000001".NotEqual("100000000000").IsTrue();
            "100000000001".NotEqual("100000000001").IsFalse();

            "100000000001".Less("100000000001").IsFalse();
            "100000000001".Less("100000000002").IsTrue();
            "100000000001".LessOrEqual("100000000002").IsTrue();
            "100000000002".LessOrEqual("100000000001").IsFalse();

            "100000000001".Greater("100000000001").IsFalse();
            "100000000002".Greater("100000000001").IsTrue();
            "100000000001".GreaterOrEqual("100000000001").IsTrue();
            "100000000001".GreaterOrEqual("100000000002").IsFalse();

            "999999999999999999999999".Multiply("999").AssertIntegerNumber("998999999999999999999999001");
            "-999999999999999999999999".Multiply("-999").AssertIntegerNumber("998999999999999999999999001");
            "999999999999999999999999".Multiply("-999").AssertIntegerNumber("-998999999999999999999999001");
            "-999999999999999999999999".Multiply("999").AssertIntegerNumber("-998999999999999999999999001");
            "10000000000000100000000001".Multiply("500").AssertIntegerNumber("5000000000000050000000000500"); //TODO - long arithmetic
            "9999999239994399999991239999999".Multiply("888834888882318888543888888").AssertIntegerNumber("8888348213303695859491006407241101393874673214452576111112"); //TODO - long arithmetic

            "-1".Power("-1").AssertIntegerNumber("-1");
            "1".Power("-1").AssertIntegerNumber("1");
            "1".Power("-2").AssertIntegerNumber("1");

            "1".FromString().IsFalse(x => x.IsFloat).IsTrue(x => x.IsInteger).IsFalse(x => x.IsFraction);
        }
        [Test]
        public void LongIntOperationsTest() {
            "123456789123456789123456789".FromString().AssertIntegerNumber("123456789123456789123456789");
            "-123456789123456789123456789".FromString().AssertIntegerNumber("-123456789123456789123456789");
            "100000000000000000000000009".FromString().AssertIntegerNumber("100000000000000000000000009");
            "0".FromString().AssertIntegerNumber("0");
            "-0".FromString().AssertIntegerNumber("0");

            "9999".Add("1").AssertIntegerNumber("10000");
            "9999".Add("9999").AssertIntegerNumber("19998");
            "999999999".Add("999999999").AssertIntegerNumber("1999999998");
            "1000000010000000000".Add("999999999999999999").AssertIntegerNumber("2000000009999999999");
            "123123123123".Add("231231231231").AssertIntegerNumber("354354354354");
            "123123123123".Add("231231231231123456").AssertIntegerNumber("231231354354246579");
            "231231231231123456".Add("123123123123").AssertIntegerNumber("231231354354246579");
            "123456789123456789123456789".Add("123456789123456789123456789").AssertIntegerNumber("246913578246913578246913578");

            "123456789123456789123456789".Subtract("123456789123456789123456789").AssertIntegerNumber("0");
            "123456789123456789123456789".Subtract("123456789123456789123456788").AssertIntegerNumber("1");
            "123456789123456789123456789123456789123456789".Subtract("123456789123456789023456789123456789123456788").AssertIntegerNumber("100000000000000000000000001");
            "234567892345678923456789".Subtract("123456781234567812345678").AssertIntegerNumber("111111111111111111111111");
            "100000000000000000".Subtract("99999999999999999").AssertIntegerNumber("1");
            "19999999999999999900000000000000000".Subtract("9999999999999999999999999999999999").AssertIntegerNumber("9999999999999999900000000000000001");

            "123456789123456789123456789".Add("-123456789123456789123456789").AssertIntegerNumber("0");
            "123456789123456789123456789".Add("-123456789123456789123456788").AssertIntegerNumber("1");
            "123456789123456789123456789123456789123456789".Add("-123456789123456789023456789123456789123456788").AssertIntegerNumber("100000000000000000000000001");
            "234567892345678923456789".Add("-123456781234567812345678").AssertIntegerNumber("111111111111111111111111");
            "100000000000000000".Add("-99999999999999999").AssertIntegerNumber("1");
            "19999999999999999900000000000000000".Add("-9999999999999999999999999999999999").AssertIntegerNumber("9999999999999999900000000000000001");

            "9999".Subtract("-1").AssertIntegerNumber("10000");
            "9999".Subtract("-9999").AssertIntegerNumber("19998");
            "999999999".Subtract("-999999999").AssertIntegerNumber("1999999998");
            "1000000010000000000".Subtract("-999999999999999999").AssertIntegerNumber("2000000009999999999");
            "123123123123".Subtract("-231231231231").AssertIntegerNumber("354354354354");
            "123123123123".Subtract("-231231231231123456").AssertIntegerNumber("231231354354246579");
            "231231231231123456".Subtract("-123123123123").AssertIntegerNumber("231231354354246579");
            "123456789123456789123456789".Subtract("-123456789123456789123456789").AssertIntegerNumber("246913578246913578246913578");

            "-1".Less("1").IsTrue();
            "-1".Greater("1").IsFalse();
            "1".Less("-1").IsFalse();
            "-1".Less("1").IsTrue();
            "-1".Less("0").IsTrue();
            "-1".Greater("0").IsFalse();
            "1".Less("0").IsFalse();
            "1".Greater("0").IsTrue();
            "0".Less("-1").IsFalse();
            "0".Greater("-1").IsTrue();
            "0".Less("1").IsTrue();
            "0".Greater("1").IsFalse();
            "0".Less("0").IsFalse();
            "0".Greater("0").IsFalse();
            "0".GreaterOrEqual("0").IsTrue();
            "0".LessOrEqual("0").IsTrue();
            "0".Equal("0").IsTrue();
            "100".Less("1000000000000000000000001").IsTrue();
            "-100".Greater("-1000000000000000000000001").IsTrue();
            "1000000000000000000001".Equal("1000000000000000000001").IsTrue();
            "1000000000000000000001".Equal("1000000000000000000000").IsFalse();
            "1000000000000000000001".NotEqual("1000000000000000000000").IsTrue();
            "1000000000000000000001".NotEqual("1000000000000000000001").IsFalse();
            "1000000000000000000001".Less("1000000000000000000001").IsFalse();
            "1000000000000000000001".Less("1000000000000000000002").IsTrue();
            "1000000000000000000001".LessOrEqual("1000000000000000000002").IsTrue();
            "1000000000000000000002".LessOrEqual("1000000000000000000001").IsFalse();
            "1000000000000000000001".Greater("1000000000000000000001").IsFalse();
            "1000000000000000000002".Greater("1000000000000000000001").IsTrue();
            "1000000000000000000001".GreaterOrEqual("1000000000000000000001").IsTrue();
            "1000000000000000000001".GreaterOrEqual("1000000000000000000002").IsFalse();
            "100000000000000000".Less("99999999999999999").IsFalse();

            "0".Subtract("1").AssertIntegerNumber("-1");
            "-1".Add("0").AssertIntegerNumber("-1");
            "-1".Add("-10000000000000").AssertIntegerNumber("-10000000000001");
            "-10000000000000".Add("-1").AssertIntegerNumber("-10000000000001");
            "0".Subtract("999999999999999999999").AssertIntegerNumber("-999999999999999999999");
            "1".Subtract("999999999999999999999").AssertIntegerNumber("-999999999999999999998");
            "-1".Subtract("999999999999999999999").AssertIntegerNumber("-1000000000000000000000");
            "1000000000000000000000000000".Subtract("999999999999999999999999999999999999999999").AssertIntegerNumber("-999999999999998999999999999999999999999999");
            "-1000000000000000000000000000".Subtract("999999999999999999999999999999999999999999").AssertIntegerNumber("-1000000000000000999999999999999999999999999");
            "-999999999".Subtract("999999999").AssertIntegerNumber("-1999999998");
            "-999999999".Subtract("-999999999").AssertIntegerNumber("0");

            "117".Divide("9").AssertIntegerNumber("13");
            "-117".Divide("-9").AssertIntegerNumber("13");
            "-117".Divide("9").AssertIntegerNumber("-13");
            "117".Divide("-9").AssertIntegerNumber("-13");
            "99999".Divide("3").AssertIntegerNumber("33333");
            "99999999999999999999999999999".Divide("3").AssertIntegerNumber("33333333333333333333333333333");
            "888848888848888488884888888".Divide("2").AssertIntegerNumber("444424444424444244442444444");
            "1000002".Divide("3").AssertIntegerNumber("333334");
            "100000000000000000000000002".Divide("3").AssertIntegerNumber("33333333333333333333333334");
            "1234840820348902398409233209380984".Divide("1234840820348902398409233209380984").AssertIntegerNumber("1");
            "10000000".IntDivide("1009999").AssertIntegerNumber("9");
            "10000000".Divide("1009999").AssertFractionNumber("10000000/1009999");
            "100000000000".IntDivide("1009999").AssertIntegerNumber("99009");
            "100000000000".Divide("1009999").AssertFractionNumber("100000000000/1009999");
            "1000000000000000000000000".IntDivide("1009999").AssertIntegerNumber("990099990198010097");
            "1000000000000000000000000".Divide("1009999").AssertFractionNumber("1000000000000000000000000/1009999");
            "8888348213303695859491006407241101393874673214452576111112".Divide("888834888882318888543888888").AssertIntegerNumber("9999999239994399999991239999999");
            "300000".Divide("30").AssertIntegerNumber("10000");
            "1341046897309863686".IntDivide("1697420285").AssertIntegerNumber("790050000");
            "1341046897309863686".Divide("1697420285").AssertFractionNumber("1341046897309863686/1697420285");
            "450436426101345047".IntDivide("1073592397").AssertIntegerNumber("419560000");
            "450436426101345047".Divide("1073592397").AssertFractionNumber("450436426101345047/1073592397");

            "2".Power("50").AssertIntegerNumber("1125899906842624");
            "-2".Power("3").AssertIntegerNumber("-8");
            "-2".Power("4").AssertIntegerNumber("16");
            "-2".Power("5").AssertIntegerNumber("-32");
            "-2".Power("6").AssertIntegerNumber("64");
            "132124324".Power("15").AssertIntegerNumber("65274217536749135507709536991541526352990992087301082499764789926799099562303792713876561988745245818772486712415039258624");
            "2".Power("-1").AssertFractionNumber("1/2");
            "2".Power("-50").AssertFractionNumber("1/1125899906842624");
            "-2".Power("-3").AssertFractionNumber("-1/8");
            "-2".Power("-4").AssertFractionNumber("1/16");
            "-2".Power("-5").AssertFractionNumber("-1/32");
            "-2".Power("-6").AssertFractionNumber("1/64");
            //TODO divide by zero
        }
        [Test]
        public void RandomLongDivision() {
            Random rnd = new Random();
            for(int i = 0; i < 100; i++) {
                RandomLongDivisionCore(rnd, 0, int.MaxValue, int.MaxValue);
                RandomLongDivisionCore(rnd, int.MaxValue, int.MaxValue, int.MaxValue);
                RandomLongDivisionCore(rnd, int.MaxValue, int.MaxValue, 10000);
                RandomLongDivisionCore(rnd, int.MaxValue, int.MaxValue, 10);
            }
        }
        static void RandomLongDivisionCore(Random rnd, int maxDivident1, int maxDivident2, int maxDivisor) {
            long x = rnd.Next(maxDivident1) * (long)int.MaxValue + rnd.Next(maxDivident2);
            long y = rnd.Next(maxDivisor);
            if(y != 0) { //TODO not only x > y
                try {
                    x.ToString().IntDivide(y.ToString()).AssertIntegerNumber((x / y).ToString());

                    long gcd = GCD(x, y);
                    long x_ = x / gcd;
                    long y_ = y / gcd;
                    if(y_ > 1)
                        x.ToString().Divide(y.ToString()).AssertFractionNumber(x_.ToString() + "/" + y_.ToString());
                    else
                        x.ToString().Divide(y.ToString()).AssertIntegerNumber(x_.ToString());
                } catch(Exception e) {
                    Debug.WriteLine(x + "/" + y);
                    throw e;
                }
            }
        }
        static long GCD(long a, long b) {
            long c;
            while(b > 0) {
                c = a % b;
                a = b;
                b = c;
            }
            return a;
        }
        [Test]
        public void FloatIntOperationsTest() {
            "1.0".Add("2").AssertFloatNumber("3");
            "1".Add("2.0").AssertFloatNumber("3");
            "1".Add("2.3").AssertFloatNumber("3.3");
            "-1".Multiply("13.0").AssertFloatNumber("-13");
            "-9".Multiply("13.0").AssertFloatNumber("-117");
            "9".Multiply("13.0").AssertFloatNumber("117");
            "9.0".Multiply("13").AssertFloatNumber("117");
            "9".Subtract("13.0").AssertFloatNumber("-4");
            "9.0".Subtract("13").AssertFloatNumber("-4");
            "3".Divide("2.0").AssertFloatNumber("1.5");
            "3.0".Divide("2").AssertFloatNumber("1.5");
            "4.0".Divide("2").AssertFloatNumber("2");
            "117".Power("5.0").AssertFloatNumber("21924480357");
            "117.0".Power("5").AssertFloatNumber("21924480357");

            "100000000001".Equal("100000000001.0").IsTrue();
            "100000000001.0".Equal("100000000000").IsFalse();
            "100000000001".NotEqual("100000000000.0").IsTrue();
            "100000000001.0".NotEqual("100000000001").IsFalse();

            "100000000001".Less("100000000001.0").IsFalse();
            "100000000001.0".Less("100000000002").IsTrue();
            "100000000001".LessOrEqual("100000000002.0").IsTrue();
            "100000000002.0".LessOrEqual("100000000001").IsFalse();

            "100000000001".Greater("100000000001.0").IsFalse();
            "100000000002.0".Greater("100000000001").IsTrue();
            "100000000001".GreaterOrEqual("100000000001.0").IsTrue();
            "100000000001.0".GreaterOrEqual("100000000002").IsFalse();

            "1".Divide("2").IsTrue(x => x == "0.5".FromString());

            "1.0".FromString().IsTrue(x => x.IsFloat).IsFalse(x => x.IsInteger).IsFalse(x => x.IsFraction);
        }
        [Test]
        public void FractionTest() {
            "1".Divide("2").AssertFractionNumber("1/2");
            "-9".Divide("117").AssertFractionNumber("-1/13");
            "1234840820348902398409233209380984".Divide("1234840820348902398409233209380985").AssertFractionNumber("1234840820348902398409233209380984/1234840820348902398409233209380985");
            "1234840820348902398409233209380984".Divide("1234840821348902398409233209380984").AssertFractionNumber("154355102543612799801154151172623/154355102668612799801154151172623");
            "1000000".Divide("1009999").AssertFractionNumber("1000000/1009999");
            "4".Divide("2").AssertIntegerNumber("2");
            "5".Divide("3").AssertFractionNumber("5/3");
            "5".Divide("-3").AssertFractionNumber("-5/3");
            "-5".Divide("3").AssertFractionNumber("-5/3");
            "-5".Divide("-3").AssertFractionNumber("5/3");
            "6".Divide("4").AssertFractionNumber("3/2");
            "8".Divide("30").AssertFractionNumber("4/15");

            ("2".FromString() < "5".Divide("2")).IsTrue();
            ("2".FromString() > "5".Divide("2")).IsFalse();
            ("3".Divide("4") > "2".Divide("3")).IsTrue();
            ("3".Divide("4") < "2".Divide("3")).IsFalse();
            ("3".Divide("4") == "3".Divide("4")).IsTrue();

            ("1".Divide("3") + "1".Divide("2")).AssertFractionNumber("5/6");
            ("1".Divide("2") + "1".Divide("2")).AssertIntegerNumber("1");
            ("4".Divide("3") + "7".Divide("2")).AssertFractionNumber("29/6");
            ("1".Divide("4") + "1".Divide("4")).AssertFractionNumber("1/2");
            ("1".Divide("3") - "1".Divide("2")).AssertFractionNumber("-1/6");
            ("2".Divide("3") * "-7".Divide("9")).AssertFractionNumber("-14/27");
            ("2".Divide("3") * "-3".Divide("2")).AssertIntegerNumber("-1");
            ("2".Divide("3") / "-7".Divide("9")).AssertFractionNumber("-6/7");
            ("2".Divide("3") / "-2".Divide("3")).AssertIntegerNumber("-1");

            ("2".FromString() ^ "1".Divide("2")).IsFloatEqual(x => x, "1.414213562");
            ("1".FromString() ^ "1".Divide("2")).AssertIntegerNumber("1");
            ("1".FromString() ^ "5".Divide("6")).AssertIntegerNumber("1");
            ("2".Divide("3") ^ "5".FromString()).AssertFractionNumber("32/243");
            ("2".Divide("3") ^ "-5".FromString()).AssertFractionNumber("243/32");
            ("2".Divide("3") ^ "1".FromString()).AssertFractionNumber("2/3");
            ("2".Divide("3") ^ "-1".FromString()).AssertFractionNumber("3/2");
            ("4".FromString() ^ "1".Divide("2")).AssertFloatNumber("2");

            "1".Divide("2").IsFalse(x => x.IsFloat).IsFalse(x => x.IsInteger).IsTrue(x => x.IsFraction);
        }
    }
    //(JsMode.Clr, Filename = SR.JSTestsName)]
    public static class NumberTestHelper {
        public static Number AssertFractionNumber(this Number n, string expected) {
            return n.IsEqual(x => x.ToString(), expected).IsEqual(x => x.GetType().Name, "FractionNumber");
        }
        public static Number AssertFloatNumber(this Number n, string expected) {
            return n.IsEqual(x => x.ToString(), expected).IsEqual(x => x.GetType().Name, "FloatNumber");
        }
        public static Number AssertIntegerNumber(this Number n, string expected) {
            return n.IsEqual(x => x.ToString(), expected).IsEqual(x => x.GetType().Name, "LongIntegerNumber");
        }
        public static Number Add(this string s1, string s2) {
            return FromString(s1) + FromString(s2); 
        }
        public static Number Subtract(this string s1, string s2) {
            return FromString(s1) - FromString(s2);
        }
        public static Number Multiply(this string s1, string s2) {
            return FromString(s1) * FromString(s2);
        }
        public static Number Divide(this string s1, string s2) {
            return FromString(s1) / FromString(s2);
        }
        public static Number IntDivide(this string s1, string s2) {
            return ((SharpAlg.Native.Numbers.LongIntegerNumber)FromString(s1)).IntDivide(((SharpAlg.Native.Numbers.LongIntegerNumber)FromString(s2)));
        }
        public static Number Power(this string s1, string s2) {
            return FromString(s1) ^ FromString(s2);
        }
        public static bool Equal(this string s1, string s2) {
            return FromString(s1) == FromString(s2);
        }
        public static bool NotEqual(this string s1, string s2) {
            return FromString(s1) != FromString(s2);
        }
        public static bool Less(this string s1, string s2) {
            return FromString(s1) < FromString(s2);
        }
        public static bool LessOrEqual(this string s1, string s2) {
            return FromString(s1) <= FromString(s2);
        }
        public static bool Greater(this string s1, string s2) {
            return FromString(s1) > FromString(s2);
        }
        public static bool GreaterOrEqual(this string s1, string s2) {
            return FromString(s1) >= FromString(s2);
        }
        public static Number FromString(this string s) {
            return s.Contains(".") ? NumberFactory.FromString(s) : NumberFactory.FromIntString(s);
        }
    }
}
