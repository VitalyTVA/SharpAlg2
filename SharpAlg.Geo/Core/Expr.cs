﻿using System.Collections.Immutable;
using System.Numerics;
using Numerics;

namespace SharpAlg.Geo.Core {
    public abstract class Expr {
        public static AddExpr operator +(Expr a, Expr b) {
            return new AddExpr(ImmutableArray.Create(a, b));
        }

    }

    public class AddExpr : Expr {
        public readonly ImmutableArray<Expr> Args;
        public AddExpr(ImmutableArray<Expr> args) {
            Args = args;
        }
    }

    public class MultExpr : Expr {
        public readonly ImmutableArray<Expr> Args;
        public MultExpr(ImmutableArray<Expr> args) {
            Args = args;
        }
    }

    public class DivExpr : Expr {
        public readonly Expr Numerator, Denominator;
        public DivExpr(Expr numerator, Expr denominator) {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    public class PowerExpr : Expr {
        public readonly Expr Value;
        public readonly BigInteger Power;
        public PowerExpr(Expr value, BigInteger power) {
            Value = value;
            Power = power;
        }
    }

    public class SqrtExpr : Expr {
        public readonly Expr Value;
        public SqrtExpr(Expr value) {
            Value = value;
        }
    }

    public class ParamExpr : Expr {
        public static implicit operator ParamExpr(string name) {
            return new ParamExpr(name);
        }
        public readonly string Name;
        public ParamExpr(string name) {
            Name = name;
        }
    }

    public class ConstExpr : Expr {
        public static implicit operator ConstExpr(int val) {
            return new ConstExpr(val);
        }

        public readonly BigRational Value;
        public ConstExpr(BigRational value) {
            Value = value;
        }
    }
}