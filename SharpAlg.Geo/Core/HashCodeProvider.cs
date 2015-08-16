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
            => args.SequenceHash(AddSalt);
        public static int MultHash(IEnumerable<Expr> args)
            => args.SequenceHash(MultSalt);
        public static int SqrtHash(Expr value)
            => SingleHash(SqrtSalt, value);

        static int PairHash<T1, T2>(int salt, T1 value1, T2 value2) => salt ^ value1.GetHashCode() ^ value2.GetHashCode();
        static int SingleHash<T>(int salt, T value) => salt ^ value.GetHashCode();
        public static int SequenceHash<T>(this IEnumerable<T> args, int salt = 0) => args.Aggregate(salt, (hash, x) => hash ^ x.GetHashCode());
    }
    public sealed class DelegateEqualityComparer<T> : IEqualityComparer<T> {
        readonly Func<T, int> getHashCode;
        readonly Func<T, T, bool> equals;
        public DelegateEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode) {
            this.getHashCode = getHashCode;
            this.equals = equals;
        }
        bool IEqualityComparer<T>.Equals(T x, T y) {
            return equals(x, y);
        }
        int IEqualityComparer<T>.GetHashCode(T obj) {
            return getHashCode(obj);
        }
    }
    public sealed class DelegateComparer<T> : IComparer<T> {
        readonly Func<T, T, int> comparer;
        public DelegateComparer(Func<T, T, int> comparer) {
            this.comparer = comparer;
        }
        int IComparer<T>.Compare(T x, T y) {
            return comparer(x, y);
        }
    }
}
