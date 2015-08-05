using Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SharpAlg.Geo.Core {
    public static class HashCodeProvider {
        readonly static int ParamSalt, ConstSalt, PowerSalt, DivSalt, SqrtSalt, AddSalt, MultSalt;
        static HashCodeProvider() {
            var rnd = new Random(0);
            Func<int> getSalt = () => rnd.Next(int.MinValue, int.MaxValue);
            ParamSalt = getSalt();
            ConstSalt = getSalt();
            PowerSalt = getSalt();
            DivSalt = getSalt();
            SqrtSalt = getSalt();
            AddSalt = getSalt();
            MultSalt = getSalt();
        }

        public static int ParamHash(string name)
            => SingleHash(ParamSalt, name);
        public static int ConstHash(BigRational value)
            => SingleHash(ConstSalt, value);
        public static int PowerHash(Expr value, BigInteger power)
            => PairHash(PowerSalt, value, power);
        public static int DivHash(Expr numerator, Expr denominator)
            => PairHash(DivSalt, numerator, denominator);
        public static int AddHash(IEnumerable<Expr> args)
            => SequenceHash(AddSalt, args);
        public static int MultHash(IEnumerable<Expr> args)
            => SequenceHash(MultSalt, args);
        public static int SqrtHash(Expr value)
            => SingleHash(SqrtSalt, value);

        static int PairHash<T1, T2>(int salt, T1 value1, T2 value2) => salt ^ value1.GetHashCode() ^ value2.GetHashCode();
        static int SingleHash<T>(int salt, T value) => salt ^ value.GetHashCode();
        static int SequenceHash<T>(int salt, IEnumerable<T> args) => args.Aggregate(salt, (hash, x) => hash ^ x.GetHashCode());
    }
}
