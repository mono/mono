// 
// Interval.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        /// <summary>
        /// Represents a closed interval of <see cref="Rational"/> values.
        /// </summary>
        public class Interval : IntervalBase<Interval, Rational>, IEquatable<Interval> {
                static Interval cached_top_value;
                static Interval cached_bottom_value;

                readonly bool is_bottom;
                readonly bool is_top;

                Interval (Rational lowerBound, Rational upperBound)
                        : base (lowerBound, upperBound)
                {
                        if (lowerBound.IsMinusInfinity && upperBound.IsPlusInfinity ||
                            lowerBound.IsMinusInfinity && upperBound.IsMinusInfinity ||
                            lowerBound.IsPlusInfinity && upperBound.IsPlusInfinity) {
                                LowerBound = Rational.MinusInfinity;
                                UpperBound = Rational.PlusInfinity;
                                is_top = true;
                        }

                        is_bottom = LowerBound > UpperBound;
                }

                public static Interval TopValue
                {
                        get
                        {
                                if (ReferenceEquals (cached_top_value, null))
                                        cached_top_value = new Interval (Rational.MinusInfinity, Rational.PlusInfinity);
                                return cached_top_value;
                        }
                }

                public static Interval BottomValue
                {
                        get
                        {
                                if (ReferenceEquals (cached_bottom_value, null))
                                        cached_bottom_value = new Interval (Rational.PlusInfinity,
                                                                            Rational.MinusInfinity);
                                return cached_bottom_value;
                        }
                }

                public override Interval Top { get { return TopValue; } }
                public override Interval Bottom { get { return BottomValue; } }

                public override bool IsTop { get { return is_top; } }
                public override bool IsBottom { get { return is_bottom; } }

                public override bool LessEqual (Interval that)
                {
                        bool result;
                        if (this.TryTrivialLessEqual (that, out result))
                                return result;

                        //less equal <==> is included in
                        return LowerBound >= that.LowerBound && UpperBound <= that.UpperBound;
                }

                public bool LessEqual (IEnumerable<Interval> right)
                {
                        return right.Any (LessEqual);
                }

                public override Interval Join (Interval that, bool widening, out bool weaker)
                {
                        weaker = false;

                        return widening ? Widen (that) : Join (that);
                }

                public override Interval Join (Interval that)
                {
                        Interval result;
                        if (this.TryTrivialJoin (that, out result))
                                return result;

                        return For (Rational.Min (LowerBound, that.LowerBound),
                                    Rational.Max (UpperBound, that.UpperBound));
                }

                public override Interval Widen (Interval that)
                {
                        Interval result;
                        if (this.TryTrivialJoin (that, out result))
                                return result;

                        return For (LowerBound < that.LowerBound
                                            ? ThresholdDB.GetPrevious (LowerBound)
                                            : that.LowerBound,
                                    UpperBound > that.UpperBound
                                            ? ThresholdDB.GetNext (UpperBound)
                                            : that.UpperBound);
                }

                public override Interval Meet (Interval that)
                {
                        Interval result;
                        if (this.TryTrivialMeet (that, out result))
                                return result;

                        return For (
                                Rational.Max (LowerBound, that.LowerBound),
                                Rational.Min (UpperBound, that.UpperBound));
                }

                public static Interval For (Rational lowerBound, Rational upperBound)
                {
                        return new Interval (lowerBound, upperBound);
                }

                public static Interval For (Rational lowerBound, long upperBound)
                {
                        return For (lowerBound, Rational.For (upperBound));
                }

                public static Interval For (long lowerBound, long upperBound)
                {
                        return For (Rational.For (lowerBound), Rational.For (upperBound));
                }

                public static Interval For (long lower, Rational upperBound)
                {
                        return For (Rational.For (lower), upperBound);
                }

                public static Interval For (Rational value)
                {
                        return For (value, value);
                }

                public static Interval For (long value)
                {
                        return For (Rational.For (value));
                }

                public static Interval operator + (Interval l, Interval r)
                {
                        if (l.IsBottom || r.IsBottom)
                                return BottomValue;

                        Rational lower, upper;
                        if (l.IsTop || r.IsTop
                            || !Rational.TryAdd (l.LowerBound, r.LowerBound, out lower)
                            || !Rational.TryAdd (l.UpperBound, r.UpperBound, out upper))
                                return TopValue;

                        return For (lower, upper);
                }

                public static Interval operator - (Interval l, Interval r)
                {
                        if (l.IsBottom || r.IsBottom)
                                return BottomValue;

                        Rational lower, upper;
                        if (l.IsTop || r.IsTop
                            || !Rational.TrySubtract (l.LowerBound, r.UpperBound, out lower)
                            || !Rational.TrySubtract (l.UpperBound, r.LowerBound, out upper))
                                return TopValue;

                        return For (lower, upper);
                }

                public static Interval operator / (Interval l, Interval r)
                {
                        if (l.IsBottom || r.IsBottom)
                                return BottomValue;

                        Rational a, b, c, d;
                        if (l.IsTop || r.IsTop || r.Includes (0)
                            || !Rational.TryDivide (l.LowerBound, r.UpperBound, out a)
                            || !Rational.TryDivide (l.LowerBound, r.LowerBound, out b)
                            || !Rational.TryDivide (l.UpperBound, r.UpperBound, out c)
                            || !Rational.TryDivide (l.UpperBound, r.LowerBound, out d))

                                return TopValue;

                        var lower = Rational.Min (Rational.Min (a, b), Rational.Min (c, d));
                        var upper = Rational.Max (Rational.Max (a, b), Rational.Max (c, d));

                        return For (lower, upper);
                }

                public static Interval operator * (Interval l, Interval r)
                {
                        if (l.IsBottom || r.IsBottom)
                                return BottomValue;

                        Rational lower, upper;
                        if (l.IsTop || r.IsTop
                            || !Rational.TryMultiply (l.LowerBound, r.LowerBound, out lower)
                            || !Rational.TryMultiply (l.UpperBound, r.UpperBound, out upper))
                                return TopValue;

                        return For (lower, upper);
                }

                public static Interval operator - (Interval l)
                {
                        if (!l.IsNormal ())
                                return l;

                        Rational lower;
                        Rational upper;
                        if (!Rational.TryUnaryMinus (l.UpperBound, out lower) ||
                            !Rational.TryUnaryMinus (l.LowerBound, out upper))
                                return TopValue;

                        return For (lower, upper);
                }

                public static bool AreConsecutiveIntegers (Interval prev, Interval next)
                {
                        if (!prev.IsNormal () || !next.IsNormal () ||
                            !prev.UpperBound.IsInteger || !next.LowerBound.IsInteger)
                                return false;

                        return prev.UpperBound + Rational.One == next.LowerBound;
                }

                public static Interval ApplyConversion (ExpressionOperator conv, Interval intv)
                {
                        if (intv.is_bottom)
                                return intv;

                        switch (conv) {
                        case ExpressionOperator.ConvertToInt32:
                                return intv.RefineWithTypeRanges (int.MinValue, int.MaxValue);
                        default:
                                return intv;
                        }
                }

                public bool TryGetSingletonFiniteInt32 (out int value)
                {
                        int lower;
                        int upper;
                        if (IsFiniteAndInt32 (out lower, out upper) && lower == upper)
                                return true.With (lower, out value);

                        return false.Without (out value);
                }

                public bool Includes (long x)
                {
                        return this.IsNormal () && LowerBound <= x && x <= UpperBound;
                }

                public bool Includes (int x)
                {
                        return this.IsNormal () && LowerBound <= x && x <= UpperBound;
                }

                public bool OverlapsWith (Interval that)
                {
                        return !Meet (that).IsBottom;
                }

                public bool OnTheLeftOf (Interval that)
                {
                        if (!this.IsNormal () || !that.IsNormal ())
                                return false;

                        return UpperBound <= that.LowerBound;
                }

                public override Interval ImmutableVersion ()
                {
                        return this;
                }

                public override Interval Clone ()
                {
                        return this;
                }

                public override bool Equals (object obj)
                {
                        if (ReferenceEquals (null, obj))
                                return false;
                        if (ReferenceEquals (this, obj))
                                return true;
                        return Equals ((Interval) obj);
                }

                public bool Equals (Interval that)
                {
                        if (ReferenceEquals (this, that))
                                return true;
                        if (ReferenceEquals (that, null))
                                return false;

                        return LowerBound == that.LowerBound && UpperBound == that.UpperBound;
                }

                public override int GetHashCode ()
                {
                        return (LowerBound.GetHashCode () * 397) ^ UpperBound.GetHashCode ();
                }

                public override void Dump (TextWriter tw)
                {
                        tw.WriteLine (ToString ());
                }

                protected override bool IsFiniteBound (Rational n)
                {
                        return n.IsInteger;
                }

                bool IsFiniteAndInt32 (out int lower, out int upper)
                {
                        if (IsFinite && LowerBound.IsInt32 && UpperBound.IsInt32) {
                                try {
                                        lower = (int) LowerBound;
                                        upper = (int) UpperBound;
                                        return true;
                                }
                                catch (ArithmeticException) {
                                }
                        }

                        return false.With (0, out lower).
                                With (0, out upper);
                }

                Interval RefineWithTypeRanges (int min, int max)
                {
                        var lower = LowerBound.IsInfinity || !LowerBound.IsInRange (min, max)
                                            ? Rational.MinusInfinity
                                            : LowerBound.PreviousInt32;

                        var upper = UpperBound.IsInfinity || !UpperBound.IsInRange (min, max)
                                            ? Rational.PlusInfinity
                                            : UpperBound.NextInt32;

                        return For (lower, upper);
                }

                internal static class Ranges {
                        static Interval int8Range;
                        static Interval int32Range;
                        static Interval int64Range;

                        public static Interval Int8Range { get { return int8Range ?? (int8Range = For (sbyte.MinValue, sbyte.MaxValue)); } }
                        public static Interval Int32Range { get { return int32Range ?? (int32Range = For (int.MinValue, int.MaxValue)); } }
                        public static Interval Int64Range { get { return int64Range ?? (int64Range = For (int.MinValue, int.MaxValue)); } }

                        public static Interval GetIntervalForType (TypeNode type, IMetaDataProvider provider)
                        {
                                if (provider.Equal (provider.System_Int8, type))
                                        return Int8Range;
                                if (provider.Equal (provider.System_Int32, type))
                                        return Int32Range;
                                if (provider.Equal (provider.System_Int64, type))
                                        return Int64Range;

                                return TopValue;
                        }
                }
        }
}