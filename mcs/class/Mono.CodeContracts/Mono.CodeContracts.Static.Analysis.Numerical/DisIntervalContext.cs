using System;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    class DisIntervalContext : IntervalRationalContextBase<DisInterval> {
        public override DisInterval TopValue { get { return DisInterval.TopValue; } }
        public override DisInterval BottomValue { get { return DisInterval.BottomValue; } }
        
        public override DisInterval Zero { get { return DisInterval.For (Interval.For (Rational.Zero)); } }
        public override DisInterval One { get { return DisInterval.For(Interval.For(Rational.One)); } }
        public override DisInterval Positive { get { return DisInterval.For(Interval.For(Rational.Zero, Rational.PlusInfinity)); ; } }
        public override DisInterval Negative { get { return DisInterval.For(Interval.For(Rational.MinusInfinity, Rational.Zero)); } }

        public override DisInterval GreaterEqualThanMinusOne { get { return DisInterval.For (Interval.For (Rational.MinusOne, Rational.PlusInfinity)); } }

        public override DisInterval For (long value)
        {
            return DisInterval.For (Interval.For (value));
        }

        public override DisInterval For (long lower, long upper)
        {
            return DisInterval.For (Interval.For (lower, upper));
        }

        public override DisInterval For (long lower, Rational upper)
        {
            return DisInterval.For(Interval.For(lower, upper));
        }

        public override DisInterval For (Rational lower, long upper)
        {
            return DisInterval.For (Interval.For (lower, upper));
        }

        public override DisInterval For (Rational value)
        {
            return DisInterval.For(Interval.For(value));
        }

        public override DisInterval For (Rational lower, Rational upper)
        {
            return DisInterval.For(Interval.For(lower, upper));
        }

        public override DisInterval Add (DisInterval a, DisInterval b)
        {
            return a + b;
        }

        public override DisInterval Sub (DisInterval a, DisInterval b)
        {
            return a - b;
        }

        public override DisInterval Div (DisInterval a, DisInterval b)
        {
            return a / b;
        }

        public override DisInterval Rem (DisInterval a, DisInterval b)
        {
            throw new NotImplementedException ();
        }

        public override DisInterval Mul (DisInterval a, DisInterval b)
        {
            return a * b;
        }

        public override DisInterval Not (DisInterval value)
        {
            if (!value.IsNormal())
                return value;

            if (value.IsNotZero)
                return Zero;

            if (value.IsPositiveOrZero)
                return Negative;

            return TopValue;
        }

        public override DisInterval UnaryMinus (DisInterval value)
        {
            return -value;
        }

        public override DisInterval ApplyConversion (ExpressionOperator conv, DisInterval intv)
        {
            return intv.Select ((i) => Interval.ApplyConversion (conv, i));
        }

        public override DisInterval RightOpen (Rational lowerBound)
        {
            return DisInterval.For (Interval.For (lowerBound, Rational.PlusInfinity));
        }

        public override DisInterval LeftOpen (Rational lowerBound)
        {
            return DisInterval.For (Interval.For (Rational.MinusInfinity, lowerBound));
        }
    }
}