#region Copyright Header
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
#endregion

using System.IO;

using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class Interval : IntervalBase<Interval, Rational>
    {
        private static Interval cachedBottomValue;
        private static Interval cachedTopValue;

        public static Interval BottomValue
        {
            get
            {
                if (ReferenceEquals (cachedBottomValue, null))
                    cachedBottomValue = new Interval (Rational.PlusInfinity, Rational.MinusInfinity);
                return cachedBottomValue;
            }
        }

        public static Interval TopValue
        {
            get
            {
                if (ReferenceEquals(cachedTopValue, null))
                    cachedTopValue = new Interval(Rational.MinusInfinity, Rational.PlusInfinity);
                return cachedTopValue;
            }
        }

        private readonly bool isBottom;
        private readonly bool isTop = false;

        private Interval (Rational lower, Rational upper)
            : base (lower, upper)
        {
            if (lower.IsMinusInfinity && upper.IsPlusInfinity ||
                lower.IsMinusInfinity && upper.IsMinusInfinity ||
                lower.IsPlusInfinity && upper.IsPlusInfinity)
            {
                LowerBound = Rational.MinusInfinity;
                UpperBound = Rational.PlusInfinity;
                isTop = true;
            }
            
            isBottom = LowerBound > UpperBound;
        }

        public override Interval Top { get { throw new System.NotImplementedException (); } }

        public override Interval Bottom { get { throw new System.NotImplementedException (); } }

        public override bool IsTop { get { return isTop; } }

        public override bool IsBottom { get { return isBottom; }}

        protected override bool IsFiniteBound (Rational n)
        {
            throw new System.NotImplementedException ();
        }

        public override Interval Meet (Interval that)
        {
            Interval result;
            if (this.TryTrivialMeet(that, out result))
                return result;

            return For (Rational.Max (LowerBound, that.LowerBound), Rational.Min (UpperBound, that.UpperBound));
        }

        public override Interval ImmutableVersion ()
        {
            throw new System.NotImplementedException ();
        }

        public override Interval Clone ()
        {
            throw new System.NotImplementedException ();
        }

        public override void Dump (TextWriter tw)
        {
            throw new System.NotImplementedException ();
        }

        public override bool LessEqual (Interval that)
        {
            throw new System.NotImplementedException ();
        }

        public override Interval Join (Interval that, bool widening, out bool weaker)
        {
            weaker = false; //TODO: make something with that

            Interval result;
            if (this.TryTrivialJoin(that, out result))
                return result;

            return Interval.For(Rational.Min(LowerBound, that.LowerBound), Rational.Max(UpperBound, that.UpperBound));
        }

        public static Interval For(Rational lower, Rational upper)
        {
            return new Interval (lower, upper);
        }

        public static Interval For(Rational r)
        {
            return For (r, r);
        }

        public static Interval For(long i)
        {
            return For (Rational.For (i));
        }

        public static Interval For(long lower, long upper)
        {
            return For (Rational.For (lower), Rational.For (upper));
        }

        public static Interval For (long lower, Rational upper)
        {
            return For(Rational.For(lower), upper);
        }

        public static Interval For(Rational lower,  long upper)
        {
            return For(lower, Rational.For(upper));
        }

        public static Interval operator + (Interval l, Interval r)
        {
            if (l.IsBottom && r.IsBottom)
                return BottomValue;

            Rational lower, upper;
            if (l.IsTop || r.IsTop
                || !Rational.TryAdd(l.LowerBound, r.LowerBound, out lower)
                || !Rational.TryAdd(l.UpperBound, r.UpperBound, out upper))
                return TopValue;

            return For (lower, upper);
        }

        public bool Includes(int x)
        {
            return this.IsNormal () && LowerBound <= x && x <= UpperBound;
        }

        protected bool Equals (Interval that)
        {
            if (ReferenceEquals(this, that))
                return true;
            if (ReferenceEquals(that, null))
                return false;

            return LowerBound == that.LowerBound && UpperBound == that.UpperBound;
        }

        public override bool Equals (object obj)
        {
            if (ReferenceEquals (null, obj))
                return false;
            if (ReferenceEquals (this, obj))
                return true;
            return Equals ((Interval)obj);
        }

        public override int GetHashCode ()
        {
            return (LowerBound.GetHashCode () * 397) ^ UpperBound.GetHashCode ();
        }
    }
}