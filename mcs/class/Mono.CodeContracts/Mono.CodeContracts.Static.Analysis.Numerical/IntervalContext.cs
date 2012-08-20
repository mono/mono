// 
// IntervalContext.cs
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
        class IntervalContext : IntervalRationalContextBase<Interval> {
                public static readonly IntervalContext Instance = new IntervalContext ();

                IntervalContext ()
                {
                }

                public override Interval TopValue { get { return Interval.TopValue; } }
                public override Interval BottomValue { get { return Interval.BottomValue; } }

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
                        return Interval.For (lower, upper);
                }

                public override Interval For (Rational lower, long upper)
                {
                        return Interval.For (lower, upper);
                }

                public override Interval For (Rational value)
                {
                        return Interval.For (value);
                }

                public override Interval For (Rational lower, Rational upper)
                {
                        return Interval.For (lower, upper);
                }

                public override Interval LeftOpen (Rational upperBound)
                {
                        return Interval.For (Rational.MinusInfinity, upperBound);
                }

                public override Interval RightOpen (Rational lowerBound)
                {
                        return Interval.For (lowerBound, Rational.PlusInfinity);
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

                public override Interval Mul (Interval a, Interval b)
                {
                        return a * b;
                }

                public override Interval Not (Interval value)
                {
                        if (!value.IsNormal ())
                                return value;

                        int intValue;
                        if (value.TryGetSingletonFiniteInt32 (out intValue))
                                return Interval.For (intValue != 0 ? 0 : 1);

                        return Interval.TopValue;
                }

                public override Interval Rem (Interval a, Interval b)
                {
                        return TopValue;
                }

                public override Interval UnaryMinus (Interval value)
                {
                        return -value;
                }

                public override Interval ApplyConversion (ExpressionOperator conv, Interval intv)
                {
                        return Interval.ApplyConversion (conv, intv);
                }
        }
}