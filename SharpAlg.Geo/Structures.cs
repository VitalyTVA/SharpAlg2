
using System.Numerics;

namespace SharpAlg.Geo.Core {
    public struct DivInfo {
        public static bool operator !=(DivInfo left, DivInfo right) {
            return !(left == right);
        }

        public static bool operator ==(DivInfo left, DivInfo right) {
            return 
                left.Num == right.Num &&
                left.Den == right.Den;
        }

        public readonly Expr Num;
        public readonly Expr Den;

        public DivInfo(Expr _Num, Expr _Den) {
            Num = _Num;
            Den = _Den;
        }

        public override bool Equals(object obj) {
            if (!(obj is DivInfo))
                return false;
            return this == (DivInfo)obj;
        }

        public override int GetHashCode() {
            return 
                Num.GetHashCode() ^
                Den.GetHashCode();

        }
    }
}
namespace SharpAlg.Geo.Core {
    public struct PowerInfo {
        public static bool operator !=(PowerInfo left, PowerInfo right) {
            return !(left == right);
        }

        public static bool operator ==(PowerInfo left, PowerInfo right) {
            return 
                left.Value == right.Value &&
                left.Power == right.Power;
        }

        public readonly Expr Value;
        public readonly BigInteger Power;

        public PowerInfo(Expr _Value, BigInteger _Power) {
            Value = _Value;
            Power = _Power;
        }

        public override bool Equals(object obj) {
            if (!(obj is PowerInfo))
                return false;
            return this == (PowerInfo)obj;
        }

        public override int GetHashCode() {
            return 
                Value.GetHashCode() ^
                Power.GetHashCode();

        }
    }
}
namespace SharpAlg.Geo.Core {
    public struct ParamPowerInfo {
        public static bool operator !=(ParamPowerInfo left, ParamPowerInfo right) {
            return !(left == right);
        }

        public static bool operator ==(ParamPowerInfo left, ParamPowerInfo right) {
            return 
                left.Param == right.Param &&
                left.Power == right.Power;
        }

        public readonly string Param;
        public readonly BigInteger Power;

        public ParamPowerInfo(string _Param, BigInteger _Power) {
            Param = _Param;
            Power = _Power;
        }

        public override bool Equals(object obj) {
            if (!(obj is ParamPowerInfo))
                return false;
            return this == (ParamPowerInfo)obj;
        }

        public override int GetHashCode() {
            return 
                Param.GetHashCode() ^
                Power.GetHashCode();

        }
    }
}

