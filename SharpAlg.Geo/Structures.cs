
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

