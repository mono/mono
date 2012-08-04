using System;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    class IntervalContext : IntervalRationalContextBase<Interval> {
        public override Interval TopValue { get { return Interval.TopValue;}}

        public override Interval BottomValue { get { return Interval.BottomValue;} }

        public override Interval Zero { get { return Interval.For (Rational.Zero); } }

        public override Interval One { get { return Interval.For (Rational.One); } }

        public override Interval Positive { get { return Interval.For (0, Rational.PlusInfinity); } }

        public override Interval Negative { get { return Interval.For (Rational.MinusInfinity, 0); } }

        public override Interval GreaterEqualThanMinusOne { get { return Interval.For (Rational.MinusOne, Rational.PlusInfinity); } }

        public override Interval For (long value)
        {
            return Interval.For (value);
        }

        public override Interval For (long lower, long upper)
        {
            return Interval.For (lower, upper);
        }

        public override Interval For (long lower, Rational upper)
        {
            return Interval.For(lower, upper);
        }

        public override Interval For (Rational lower, long upper)
        {
            return Interval.For(lower, upper);
        }

        public override Interval For (Rational value)
        {
            return Interval.For (value);
        }

        public override Interval For (Rational lower, Rational upper)
        {
            return Interval.For (lower, upper);
        }

        public override Interval Add (Interval a, Interval b)
        {
            return a + b;
        }

        public override Interval Sub (Interval a, Interval b)
        {
            return a - b;
        }

        public override Interval Div (Interval a, Interval b)
        {
            return a / b;
        }

        public override Interval Rem (Interval a, Interval b)
        {
            throw new NotImplementedException();
        }

        public override Interval Mul (Interval a, Interval b)
        {
            return a * b;
        }

        public override Interval Not (Interval value)
        {
            if (!value.IsNormal())
                return value;

            int intValue;
            if (value.TryGetSingletonFiniteInt32 (out intValue))
                return Interval.For (intValue != 0 ? 0 : 1);

            return Interval.TopValue;
        }

        public override Interval UnaryMinus (Interval value)
        {
            return -value;
        }

        public override Interval ApplyConversion (ExpressionOperator conv, Interval intv)
        {
            return Interval.ApplyConversion (conv, intv);
        }

        public override Interval LeftOpen (Rational upperBound)
        {
            return Interval.For (Rational.MinusInfinity, upperBound);
        }

        public override Interval RightOpen (Rational lowerBound)
        {
            return Interval.For(lowerBound, Rational.PlusInfinity);
        }
    }
}