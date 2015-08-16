﻿using NUnit.Framework;
using SharpAlg.Geo.Core;
using static SharpAlg.Geo.Core.ExprExtensions;

namespace SharpAlg.Geo.Tests {
    [TestFixture]
    public class SingleDivTransfomerTests : ExprTestsBase {
        protected override Builder CreateBuilder() {
            return Builder.CreateRealLife();
        }
        [Test]
        public void ElementaryConvolution_Mult() {
            builder.Build(x => (Expr)1 * 1).AssertSimpleStringRepresentation("1");
            builder.Build(x => x * 1).AssertSimpleStringRepresentation("x");
            builder.Build(x => 1 * x).AssertSimpleStringRepresentation("x");
            builder.Build((x, y) => x * (y * 1)).AssertSimpleStringRepresentation("x * y");
            builder.Build((x, y) => x * (1 * y)).AssertSimpleStringRepresentation("x * y");

            builder.Build((x, y) => x * 2 * y * 3).AssertSimpleStringRepresentation("6 * x * y");
            builder.Build(x => x * 0).AssertSimpleStringRepresentation("0");
            builder.Build(x => 0 * x).AssertSimpleStringRepresentation("0");
        }
        [Test]
        public void ElementaryConvolution_Power() {
            builder.Build(x => x ^ 1).AssertSimpleStringRepresentation("x");
            builder.Build(x => (Expr)3 ^ 2).AssertSimpleStringRepresentation("9");

            builder.Build(x => (-x) ^ 2).AssertSimpleStringRepresentation("x ^ 2");
            builder.Build(x => (-5 * x) ^ 2).AssertSimpleStringRepresentation("25 * (x ^ 2)");
            builder.Build((x, y) => (-x * y) ^ 4).AssertSimpleStringRepresentation("(x ^ 4) * (y ^ 4)");
            builder.Build((x, y) => (-x * y) ^ 3).AssertSimpleStringRepresentation("-(x ^ 3) * (y ^ 3)");
            builder.Build(x => ((-x) ^ 2) ^ 3).AssertSimpleStringRepresentation("x ^ 6");
            builder.Build((x, y) => ((x * y) ^ 2) ^ 5).AssertSimpleStringRepresentation("(x ^ 10) * (y ^ 10)");
        }
        [Test]
        public void ElementaryConvolution_Add() {
            builder.Build(x => (Expr)0 + 0).AssertSimpleStringRepresentation("0");
            builder.Build(x => x + 0).AssertSimpleStringRepresentation("x").ToParam();
            builder.Build(x => x - 0).AssertSimpleStringRepresentation("x").ToParam();
            builder.Build(x => x + 2 - 2).AssertSimpleStringRepresentation("x").ToParam();
            builder.Build(x => 0 + x).AssertSimpleStringRepresentation("x");
            builder.Build(x => -0 + x).AssertSimpleStringRepresentation("x");
            builder.Build((x, y) => x  + (y + 0)).AssertSimpleStringRepresentation("x + y");
            builder.Build((x, y) => x + (0 + y)).AssertSimpleStringRepresentation("x + y");
            builder.Build((x, y) => x + 2 + y + 3).AssertSimpleStringRepresentation("5 + x + y");
        }
        [Test]
        public void ElementaryConvolution_Sqrt() {
            builder.Build(x => sqrt(0)).AssertSimpleStringRepresentation("0");
        }
        [Test]
        public void Group_Mult() {
            builder.Build((x, y) => 3 * x * y * 6 * (x ^ 2) * (y ^ 5)).AssertSimpleStringRepresentation("18 * (x ^ 3) * (y ^ 6)");
            builder.Build((x, y) => (3 / x) * (y / (x ^ 2))).AssertSimpleStringRepresentation("(3 * y) / (x ^ 3)");
        }
        [Test]
        public void Group_Add() {
            builder.Build((x, y) => 3 + x  + (y ^ 2) + 6 + 2 * x + 5 * (y ^ 2)).AssertSimpleStringRepresentation("9 + 3 * x + 6 * (y ^ 2)");
        }
        [Test]
        public void Mult() {
            builder.Build((x, y, z) => (x / y) * z).AssertSimpleStringRepresentation("(x * z) / y");
            builder.Build((x, y, z) => x * (y / z)).AssertSimpleStringRepresentation("(x * y) / z");
            builder.Build((x, y, z, w) => (x / y) * (z / w)).AssertSimpleStringRepresentation("(x * z) / (y * w)");
        }
        [Test]
        public void Div() {
            Assert.AreEqual(3, builder.Build((x, y, z) => x + y + z).ToAdd().Length);
            Assert.AreEqual(3, builder.Build((x, y, z) => x * y * z).ToMult().Length);

            builder.Build(x => x / 1).AssertSimpleStringRepresentation("x");
            builder.Build((x, y) => (x + y) / 1).AssertSimpleStringRepresentation("x + y");
            builder.Build((x, y, z) => x / y).AssertSimpleStringRepresentation("x / y");
            builder.Build((x, y, z) => x / (y / z)).AssertSimpleStringRepresentation("(x * z) / y");
            builder.Build((x, y, z) => (x / y) / z).AssertSimpleStringRepresentation("x / (y * z)");
            builder.Build((x, y, z, w) => (x / y) / (z / w)).AssertSimpleStringRepresentation("(x * w) / (y * z)");
            builder.Build((x, y, z, w) => x / (y / (z * w))).AssertSimpleStringRepresentation("(x * z * w) / y");
            builder.Build((x, y, z, w) => (x / (y * w)) / z).AssertSimpleStringRepresentation("x / (y * w * z)");
            builder.Build((x, y, z, w, v) => (x / y) / (z / (w * v))).AssertSimpleStringRepresentation("(x * w * v) / (y * z)");
            builder.Build((x, y, z, w, v) => (x / y) / ((z * w) / v)).AssertSimpleStringRepresentation("(x * v) / (y * z * w)");

            builder.Build((x, y) => sqrt(x / y)).AssertSimpleStringRepresentation("sqrt(x / y)");

            builder.Build(x => 0 / x).AssertSimpleStringRepresentation("0");
        }
        [Test]
        public void DivSimplify() {
            builder.Build((x, y, z, w) => (10 * (x ^ 5) * y * z) / (4 * (x ^ 2) * (y ^ 3) * w))
                .AssertSimpleStringRepresentation("(5/2 * (x ^ 3) * z) / ((y ^ 2) * w)");
            builder.Build(x => (x ^ 5) / x)
                .AssertSimpleStringRepresentation("x ^ 4");



            //builder.Build(a => ((-1 / 16 * sqrt(48 * (a ^ 6))) / a) / ((-1 / 8 * sqrt(48 * (a ^ 6))) / (a ^ 2)))
            //    .AssertSimpleStringRepresentation("x ^ 4");
        }
    }
}