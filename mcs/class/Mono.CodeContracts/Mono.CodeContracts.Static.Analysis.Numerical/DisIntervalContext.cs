// 
// DisIntervalContext.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class DisIntervalContext : IntervalRationalContextBase<DisInterval> {
                public static readonly DisIntervalContext Instance = new DisIntervalContext ();

                DisIntervalContext ()
                {
                }

                public override DisInterval TopValue { get { return DisInterval.TopValue; } }
                public override DisInterval BottomValue { get { return DisInterval.BottomValue; } }

                public override DisInterval Zero { get { return DisInterval.For (Interval.For (Rational.Zero)); } }
                public override DisInterval One { get { return DisInterval.For (Interval.For (Rational.One)); } }

                public override DisInterval Positive { get { return DisInterval.For (Interval.For (Rational.Zero, Rational.PlusInfinity)); } }
                public override DisInterval Negative { get { return DisInterval.For (Interval.For (Rational.MinusInfinity, Rational.Zero)); } }

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
                        return DisInterval.For (Interval.For (lower, upper));
                }

                public override DisInterval For (Rational lower, long upper)
                {
                        return DisInterval.For (Interval.For (lower, upper));
                }

                public override DisInterval For (Rational value)
                {
                        return DisInterval.For (Interval.For (value));
                }

                public override DisInterval For (Rational lower, Rational upper)
                {
                        return DisInterval.For (Interval.For (lower, upper));
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
                        return TopValue;
                }

                public override DisInterval Mul (DisInterval a, DisInterval b)
                {
                        return a * b;
                }

                public override DisInterval Not (DisInterval value)
                {
                        if (!value.IsNormal ())
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