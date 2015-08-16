
using Numerics;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using ExprList = System.Collections.Immutable.ImmutableArray<SharpAlg.Geo.Core.Expr>;

namespace SharpAlg.Geo.Core {
    public struct DivInfo {
        public static bool operator !=(DivInfo left, DivInfo right) {
            return !(left == right);
        }

        public static bool operator ==(DivInfo left, DivInfo right) {
            return 
                Equals(left.Num, right.Num) &&
                Equals(left.Den, right.Den);
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
                Equals(left.Value, right.Value) &&
                Equals(left.Power, right.Power);
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
                Equals(left.Param, right.Param) &&
                Equals(left.Power, right.Power);
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
namespace SharpAlg.Geo.Core {
    public struct ParamPowerInfoListWithSqrt {
        public static bool operator !=(ParamPowerInfoListWithSqrt left, ParamPowerInfoListWithSqrt right) {
            return !(left == right);
        }

        public static bool operator ==(ParamPowerInfoListWithSqrt left, ParamPowerInfoListWithSqrt right) {
            return 
                Equals(left.ParamPowerInfoList, right.ParamPowerInfoList) &&
                Equals(left.Sqrt, right.Sqrt);
        }

        public readonly IEnumerable<ParamPowerInfo?> ParamPowerInfoList;
        public readonly Expr Sqrt;

        public ParamPowerInfoListWithSqrt(IEnumerable<ParamPowerInfo?> _ParamPowerInfoList, Expr _Sqrt) {
            ParamPowerInfoList = _ParamPowerInfoList;
            Sqrt = _Sqrt;
        }

        public override bool Equals(object obj) {
            if (!(obj is ParamPowerInfoListWithSqrt))
                return false;
            return this == (ParamPowerInfoListWithSqrt)obj;
        }

        public override int GetHashCode() {
            return 
                ParamPowerInfoList.GetHashCode() ^
                Sqrt.GetHashCode();

        }
    }
}
namespace SharpAlg.Geo.Core {
    public struct KoeffMultInfo {
        public static bool operator !=(KoeffMultInfo left, KoeffMultInfo right) {
            return !(left == right);
        }

        public static bool operator ==(KoeffMultInfo left, KoeffMultInfo right) {
            return 
                Equals(left.Koeff, right.Koeff) &&
                Equals(left.Mult, right.Mult);
        }

        public readonly BigRational Koeff;
        public readonly ExprList Mult;

        public KoeffMultInfo(BigRational _Koeff, ExprList _Mult) {
            Koeff = _Koeff;
            Mult = _Mult;
        }

        public override bool Equals(object obj) {
            if (!(obj is KoeffMultInfo))
                return false;
            return this == (KoeffMultInfo)obj;
        }

        public override int GetHashCode() {
            return 
                Koeff.GetHashCode() ^
                Mult.GetHashCode();

        }
    }
}

