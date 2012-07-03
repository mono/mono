 // 
 // Rational.cs
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
using System.Threading;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    sealed class Rational
    {
        private static readonly Lazy<Rational> plusInfinity  = new Lazy<Rational>(() => new Rational(Kind.PlusInfinity), false);
        private static readonly Lazy<Rational> minusInfinity = new Lazy<Rational>(() => new Rational(Kind.MinusInfinity), false);
        public static readonly Rational Zero = new Rational (0L);
        public static readonly Rational One = new Rational (1L);
        public static readonly Rational MinusOne = new Rational (-1L);

        public static Rational PlusInfinity { get { return plusInfinity.Value; } }
        public static Rational MinusInfinity { get { return minusInfinity.Value; } }

        private readonly Kind kind;
        private readonly long up;
        private readonly long down;

        private Rational (Kind kind)
        {
            this.kind = kind;
            this.up = 0L;
            this.down = 0L;
        }

        private Rational(long number)
        {
            this.kind = Kind.Normal;
            this.up = number;
            this.down = 1L;
        }

        private Rational (long nominator, long denominator)
        {
            if (denominator == 0L)
            {
                this.kind = nominator > 0L ? Kind.PlusInfinity : Kind.MinusInfinity;
                this.up = 0L;
                this.down = 0L;
                
                return;
            }

            if (nominator == 0L)
            {
                this.kind = Kind.Normal;
                this.up = 0L;
                this.down = 1L;

                return;
            }

            int sign = Math.Sign (nominator) * Math.Sign (denominator);

            nominator = nominator == long.MaxValue
                            ? (sign >= 0 ? long.MaxValue : long.MinValue)
                            : (sign * Math.Abs (nominator));

            if (denominator != long.MinValue)
                denominator = Math.Abs (denominator);
            else
            {
                nominator = 0L;
                denominator = 1L;
            }

            this.kind = Kind.Normal;
            if (nominator % denominator == 0)
            {
                this.up = nominator / denominator;
                this.down = 1L;
            } else
            {
                long gcd = (nominator   == 0L || nominator   == long.MaxValue || nominator == long.MinValue
                         || denominator == 0L || denominator == long.MaxValue)
                               ? 1L
                               : GCD (Math.Abs (nominator), Math.Abs (denominator));
                
                this.up = nominator / gcd;
                this.down = denominator / gcd;
            }
        }

        private long GCD (long a, long b)
        {
            var aa = (ulong)a;
            var bb = (ulong)b;

            var pow = 0;

            while (((aa | bb) & 1L) == 0L)//while both divide by 2
            {
                aa >>= 1;
                bb >>= 1;
                ++pow;
            }

            while ((aa & 1L) == 0L) //get rid of other 2's factors
                aa >>= 1;

            do
            {
                while ((bb & 1L) == 0L)
                    bb >>= 1;

                ulong cc;
                if (aa < bb)
                    cc = bb - aa;
                else
                {
                    var tmp = aa - bb;
                    aa = bb;
                    cc = tmp;
                }
            
                bb = cc >> 1;
            }
            while (bb != 0L);

            return (long) aa << pow;
        }

        public static Rational For(long number)
        {
            return new Rational (number);
        }

        public static Rational For (long nominator, long denominator)
        {
            switch (denominator)
            {
                case 0L:
                    throw new ArithmeticException ();
                case 1L:
                    return Rational.For (nominator);
                default:
                    return new Rational (nominator, denominator);
            }
        }

        public static bool operator == (Rational l, Rational r)
        {
            if (ReferenceEquals(l, null) || ReferenceEquals(r, null))
                return ReferenceEquals (l, null) && ReferenceEquals (r, null);

            if (l.kind != r.kind)
                return false;

            if (l.kind != Kind.Normal)
                return true;

            return l.up   == r.up 
                && l.down == r.down;
        }

        public static bool operator != (Rational l, Rational r)
        {
            return !(l == r);
        }

        public override string ToString()
        {
            switch (kind)
            {
                case Kind.MinusInfinity:
                    return "-oo" + (up == -1L || down == 0L ? "" : string.Format ("({0} / {1})", up, down));
                case Kind.PlusInfinity:
                    return "+oo" + (up == 1L  || down == 0L ? "" : string.Format ("({0} / {1})", up, down));
                default:
                    if (IsInteger)
                        return up.ToString();
                    return string.Format ("({0} / {1})", up, down);
            }
        }

        public bool IsInteger
        {
            get { return !IsInfinity && (up % down == 0L); }
        }

        public bool IsInfinity
        {
            get { return kind == Kind.PlusInfinity || kind == Kind.MinusInfinity; }
        }

        private enum Kind
        {
            Normal,
            PlusInfinity,
            MinusInfinity,
        }
    }
}