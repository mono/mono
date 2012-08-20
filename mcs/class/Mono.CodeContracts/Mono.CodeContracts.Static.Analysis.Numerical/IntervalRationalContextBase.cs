// 
// IntervalRationalContextBase.cs
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
        abstract class IntervalRationalContextBase<TInterval> : IntervalContextBase<TInterval, Rational>
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

                public override bool IsLessEqualThanZero (TInterval value)
                {
                        if (value.IsNormal ())
                                return IsLessEqualThanZero (value.UpperBound);

                        return false;
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

                public override bool IsMaxInt32 (TInterval value)
                {
                        return value.IsSinglePoint && value.LowerBound.IsInteger && (int) value.LowerBound.NextInt32 == int.MaxValue;
                }
                }
}