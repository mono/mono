using System;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    internal abstract class IntervalContextBase<TInterval, TNumeric>
        where TInterval : IntervalBase<TInterval, TNumeric>
    {
        /// <summary>
        /// (-oo, +oo)
        /// </summary>
        public abstract TInterval TopValue { get; }
        /// <summary>
        /// Empty set of values
        /// </summary>
        public abstract TInterval BottomValue { get; }
        /// <summary>
        /// [0, 0]
        /// </summary>
        public abstract TInterval Zero { get; }
        /// <summary>
        /// [1, 1]
        /// </summary>
        public abstract TInterval One { get; }
        /// <summary>
        /// [0, +oo)
        /// </summary>
        public abstract TInterval Positive { get; }
        /// <summary>
        /// (-oo, 0]
        /// </summary>
        public abstract TInterval Negative { get; }

        public abstract TInterval For (long value);
        public abstract TInterval For (TNumeric value);
        public abstract TInterval For (TNumeric lower, TNumeric upper);

        public abstract bool IsGreaterThanZero (TNumeric value);
        public abstract bool IsGreaterEqualThanZero (TNumeric value);

        public abstract bool IsLessThanZero (TNumeric value);
        public abstract bool IsLessEqualThanZero (TNumeric value);

        public abstract bool IsLessThan (TNumeric a, TNumeric b);
        public abstract bool IsLessEqualThan (TNumeric a, TNumeric b);

        public abstract bool IsZero (TNumeric value);

        public abstract bool IsNotZero (TNumeric value);

        public abstract bool IsPlusInfinity (TNumeric value);

        public abstract bool IsMinusInfinity (TNumeric value);

        public abstract bool AreEqual (TNumeric a, TNumeric b);

        public abstract TInterval Add (TInterval a, TInterval b);
        public abstract TInterval Sub (TInterval a, TInterval b);
        public abstract TInterval Div (TInterval a, TInterval b);
        public abstract TInterval Rem (TInterval a, TInterval b);
        public abstract TInterval Mul (TInterval a, TInterval b);
        public abstract TInterval Not (TInterval value);

        public abstract TInterval UnaryMinus (TInterval value);

        public virtual FlatDomain<bool> IsLessThan (TInterval a, TInterval b)
        {
            if (a.IsNormal() || b.IsNormal())
                return ProofOutcome.Top;

            if (this.IsLessThan(a.UpperBound, b.LowerBound))
                return true;
            if (this.IsLessEqualThan(b.UpperBound, a.LowerBound))
                return false;

            return ProofOutcome.Top;
        }

        public virtual FlatDomain<bool> IsLessEqualThan (TInterval a, TInterval b)
        {
            if (a.IsNormal() || b.IsNormal())
                return ProofOutcome.Top;

            if (this.IsLessEqualThan(a.UpperBound, b.LowerBound))
                return true;
            if (this.IsLessThan(b.UpperBound, a.LowerBound))
                return false;

            return ProofOutcome.Top;
        }

        public virtual FlatDomain<bool> IsEqualThan (TInterval a, TInterval b)
        {
            throw new NotImplementedException ();
        }
    }
}