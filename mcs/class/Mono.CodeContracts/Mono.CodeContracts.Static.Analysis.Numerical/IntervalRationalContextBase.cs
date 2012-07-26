namespace Mono.CodeContracts.Static.Analysis.Numerical {
    internal abstract class IntervalRationalContextBase<TInterval> : IntervalContextBase<TInterval, Rational>
        where TInterval : IntervalBase<TInterval, Rational> {
        public override bool IsGreaterThanZero (Rational value)
        {
            return value > Rational.Zero;
        }

        public override bool IsGreaterEqualThanZero (Rational value)
        {
            return value >= Rational.Zero;
        }

        public override bool IsLessThanZero (Rational value)
        {
            return value < Rational.Zero;
        }

        public override bool IsLessEqualThanZero (Rational value)
        {
            return value <= Rational.Zero;
        }

        public override bool IsLessThan (Rational a, Rational b)
        {
            return a < b;
        }

        public override bool IsLessEqualThan (Rational a, Rational b)
        {
            return a <= b;
        }

        public override bool IsZero (Rational value)
        {
            return value.IsZero;
        }

        public override bool IsNotZero (Rational value)
        {
            return !value.IsZero;
        }

        public override bool IsPlusInfinity (Rational value)
        {
            return value.IsPlusInfinity;
        }

        public override bool IsMinusInfinity (Rational value)
        {
            return value.IsMinusInfinity;
        }

        public override bool AreEqual (Rational a, Rational b)
        {
            return a == b;
        }
        }
}