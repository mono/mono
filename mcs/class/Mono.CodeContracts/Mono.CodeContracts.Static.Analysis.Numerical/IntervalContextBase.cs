// 
// IntervalContextBase.cs
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

using System;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class IntervalContextBase<TInterval, TNumeric>
                where TInterval : IntervalBase<TInterval, TNumeric> {
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

                /// <summary>
                /// [-1, +oo)
                /// </summary>
                public abstract TInterval GreaterEqualThanMinusOne { get; }

                public abstract TInterval For (long value);
                public abstract TInterval For (long lower, long upper);
                public abstract TInterval For (long lower, TNumeric upper);
                public abstract TInterval For (TNumeric lower, long upper);
                public abstract TInterval For (TNumeric value);
                public abstract TInterval For (TNumeric lower, TNumeric upper);

                public abstract bool IsGreaterThanZero (TNumeric value);
                public abstract bool IsGreaterEqualThanZero (TNumeric value);

                public abstract bool IsLessThanZero (TNumeric value);
                public abstract bool IsLessEqualThanZero (TNumeric value);

                public abstract bool IsLessEqualThanZero (TInterval value);

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

                public abstract TInterval ApplyConversion (ExpressionOperator conv, TInterval intv);

                public virtual FlatDomain<bool> IsLessThan (TInterval a, TInterval b)
                {
                        if (a.IsNormal () || b.IsNormal ())
                                return ProofOutcome.Top;

                        if (IsLessThan (a.UpperBound, b.LowerBound))
                                return true;
                        if (IsLessEqualThan (b.UpperBound, a.LowerBound))
                                return false;

                        return ProofOutcome.Top;
                }

                public virtual FlatDomain<bool> IsLessEqualThan (TInterval a, TInterval b)
                {
                        if (a.IsNormal () || b.IsNormal ())
                                return ProofOutcome.Top;

                        if (IsLessEqualThan (a.UpperBound, b.LowerBound))
                                return true;
                        if (IsLessThan (b.UpperBound, a.LowerBound))
                                return false;

                        return ProofOutcome.Top;
                }

                public virtual FlatDomain<bool> IsEqualThan (TInterval a, TInterval b)
                {
                        throw new NotImplementedException ();
                }

                public abstract bool IsMaxInt32 (TInterval value);

                public abstract TInterval RightOpen (TNumeric lowerBound);
                public abstract TInterval LeftOpen (TNumeric lowerBound);
       }
}