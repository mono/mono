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
using System.Diagnostics.Contracts;
using System.Threading;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    sealed class Rational : IEquatable<Rational>
    {
        private static Rational plusInfinity;
        private static Rational minusInfinity;
        private static Rational maxValue;
        private static Rational minValue;

        public static readonly Rational Zero = new Rational (0L);
        public static readonly Rational One = new Rational (1L);
        public static readonly Rational MinusOne = new Rational (-1L);

        public static Rational PlusInfinity  { get { return !ReferenceEquals (plusInfinity, null) ? plusInfinity : (plusInfinity = new Rational (Kind.PlusInfinity)); } }
        public static Rational MinusInfinity { get { return !ReferenceEquals (minusInfinity, null) ? minusInfinity : (minusInfinity = new Rational (Kind.MinusInfinity)); } }

        public static Rational MaxValue { get { return !ReferenceEquals (maxValue, null) ? maxValue : (maxValue = new Rational (long.MaxValue)); } }
        public static Rational MinValue { get { return !ReferenceEquals (minValue, null) ? minValue : (minValue = new Rational (long.MinValue)); } }

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

                return;
            }

            long gcd = (nominator   == 0L || nominator   == long.MaxValue || nominator == long.MinValue
                        || denominator == 0L || denominator == long.MaxValue)
                           ? 1L
                           : GCD (Math.Abs (nominator), Math.Abs (denominator));
                
            this.up = nominator / gcd;
            this.down = denominator / gcd;
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
                    return For (nominator);
                default:
                    return new Rational (nominator, denominator);
            }
        }

        public bool IsInteger { get { return !IsInfinity && (up % down == 0L); } }

        public bool IsInfinity { get { return kind == Kind.PlusInfinity || kind == Kind.MinusInfinity; } }

        public bool IsMinusInfinity { get { return kind == Kind.MinusInfinity; } }

        public bool IsPlusInfinity { get { return kind == Kind.PlusInfinity; } }

        public bool IsZero { get { return kind == Kind.Normal && up == 0L; } }

        public bool IsMaxValue { get { return kind == Kind.Normal && up == long.MaxValue && down == 1L; } }

        public bool IsMinValue { get { return kind == Kind.Normal && up == long.MinValue && down == 1L; } }

        public int Sign
        {
            get
            {
                switch (kind)
                {
                    case Kind.PlusInfinity:
                        return 1;
                    case Kind.MinusInfinity:
                        return -1;
                    default:
                        return Math.Sign(up) * Math.Sign(down);
                }
            }
        }

        public Rational NextInt32
        {
            get
            {
                if (IsInfinity)
                    return this;
                var next = (long)Math.Ceiling((double)this);

                return For(next >= int.MaxValue ? int.MaxValue : next);
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

        public static bool operator <(Rational l, Rational r)
        {
            if (ReferenceEquals(l, r))
                return false;
            if (l.IsMinusInfinity && !r.IsMinusInfinity
             || r.IsPlusInfinity  && !l.IsPlusInfinity)
                return true;
            if (l.IsPlusInfinity || r.IsMinusInfinity)
                return false;
            if (l.down == r.down)
                return l.up < r.up;
            if (l.up <= 0L && r.up > 0L)
                return true;
            if (l.up < 0L && r.up == 0L)
                return true;

            try
            {
                return checked(l.up * r.down) < checked(r.up * l.down);
            }
            catch (ArithmeticException)
            {
                return (decimal)l.up / l.down < (decimal)r.up / r.down;
            }
        }
        
        public static bool operator <=(Rational l, Rational r)
        {
            if (ReferenceEquals(l, r))
                return true;
            if (l.IsMinusInfinity || r.IsPlusInfinity)
                return true;
            if (l.IsPlusInfinity || r.IsMinusInfinity)
                return false;

            if (l.down == r.down)
                return l.up <= r.up;
            
            try
            {
                return checked(l.up * r.down) <= checked(r.up * l.down);
            }
            catch (ArithmeticException)
            {
                return (decimal)l.up / l.down <= (decimal)r.up / r.down;
            }
        }

        public static bool operator >= (Rational l, Rational r)
        {
            return r <= l;
        }

        public static bool operator <=(Rational l, long r)
        {
            switch (l.kind)
            {
                case Kind.PlusInfinity:
                    return false;
                case Kind.MinusInfinity:
                    return true;
                default:
                    try
                    {
                        return l.up <= checked(l.down * r);
                    } catch(ArithmeticException)
                    {
                        return (decimal)l.up / l.down <= r;
                    }
            }
        }

        public static bool operator <=(long l, Rational r)
        {
            switch (r.kind)
            {
                case Kind.PlusInfinity:
                    return true;
                case Kind.MinusInfinity:
                    return false;
                default:
                    try
                    {
                        return r.up >= checked(r.down * l);
                    }
                    catch (ArithmeticException)
                    {
                        return (decimal)r.up / r.down >= l;
                    }
            }
        }

        public static bool operator >= (long l, Rational r)
        {
            return r <= l;
        }

        public static bool operator >= (Rational l, long r)
        {
            return r <= l;
        }

        public static bool operator > (Rational l, Rational r)
        {
            return r < l;
        }

        public static bool operator < (Rational l, long r)
        {
            switch (l.kind)
            {
                case Kind.PlusInfinity:
                    return false;
                case Kind.MinusInfinity:
                    return true;
                default:
                    try
                    {
                        return l.up < checked(r * l.down) ;
                    }
                    catch
                    {
                        return (decimal)l.up / l.down < r;
                    }
            }

        }

        public static bool operator > (Rational l, long r)
        {
            return r < l;
        }

        public static bool operator < (long l, Rational r)
        {
            switch (r.kind)
            {
                case Kind.PlusInfinity:
                    return true;
                case Kind.MinusInfinity:
                    return false;
                default:
                    try
                    {
                        return checked(l * r.down) < r.up;
                    }
                    catch
                    {
                        return l < (decimal) r.up / r.down;
                    }
            }
        }

        public static bool operator > (long l, Rational r)
        {
            return r < l;
        }

        public static Rational operator+(Rational l, Rational r)
        {
            Rational result;
            if (TryAdd(l,r, out result))
                return result;
            
            throw new ArithmeticException();
        }
        
        public static Rational operator -(Rational l, Rational r)
        {
            Rational result;
            if (TrySub(l, r, out result))
                return result;

            throw new ArithmeticException();
        }

        public static Rational operator * (Rational l, Rational r)
        {
            Rational result;
            if (TryMul(l, r, out result))
                return result;

            throw new ArithmeticException();
        }

        public static Rational operator / (Rational l, Rational r)
        {
            Rational result;
            if (TryDiv(l, r, out result))
                return result;

            throw new ArithmeticException();
        }

        public static Rational operator -(Rational l, long i)
        {
            if (l.kind == Kind.Normal && l.down == 1L)
                try
                {
                    return For (checked(l.up - i));
                }
                catch (ArithmeticException)
                {
                    return l - For (i);
                }

            return l - For (i);
        }

        public static Rational operator -(Rational value)
        {
            Rational result;
            if (TryUnaryMinus(value, out result))
                return result;

            throw new ArithmeticException();
        }

        public static explicit operator double(Rational r)
        {
            switch (r.kind)
            {
                case Kind.PlusInfinity:
                    return double.PositiveInfinity;
                case Kind.MinusInfinity:
                    return double.NegativeInfinity;
                default:
                    return (double)r.up / r.down;
            }
        }

        public static explicit operator long(Rational r)
        {
            if (r.down == 0L)
                return r.up >= 0L ? long.MaxValue : long.MinValue;

            if (!r.IsInteger)
                return (long)Math.Round((double)r.up / r.down);

            return r.up;
        }

        public static Rational Abs (Rational a)
        {
            switch (a.kind)
            {
                case Kind.PlusInfinity:
                case Kind.MinusInfinity:
                    return PlusInfinity;
                default:
                    return a.IsZero || a > 0L ? a : -a;
            }
        }

        public static Rational Max (Rational a, Rational b)
        {
            return a < b ? b : a;
        }

        public static Rational Min(Rational a, Rational b)
        {
            return a < b ? a : b;
        }

        public override string ToString()
        {
            switch (kind)
            {
                case Kind.MinusInfinity:
                    return "-oo" + (up == -1L || down == 0L ? "" : string.Format("({0} / {1})", up, down));
                case Kind.PlusInfinity:
                    return "+oo" + (up == 1L || down == 0L ? "" : string.Format("({0} / {1})", up, down));
                default:
                    return IsInteger ? up.ToString() : string.Format("({0} / {1})", up, down);
            }
        }

        public bool Equals(Rational other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj is Rational && Equals((Rational)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = kind.GetHashCode();
                hashCode = (hashCode * 397) ^ up.GetHashCode();
                hashCode = (hashCode * 1001) ^ down.GetHashCode();
                return hashCode;
            }
        }

        private static long GCD(long a, long b)
        {
            var aa = (ulong)a;
            var bb = (ulong)b;

            var pow = 0;

            while (((aa | bb) & 1L) == 0L) //while both divide by 2
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

            return (long)aa << pow;
        }

        public static bool TryDiv(Rational l, Rational r, out Rational result)
        {
            if (r == One)
                return true.With(l, out result);

            if (r.IsZero)
                return false.Without(out result);
            if (l.IsZero || r.IsInfinity)
                return true.With(Zero, out result);

            if (l.IsPlusInfinity)
                return true.With(r.Sign > 0 ? PlusInfinity : MinusInfinity, out result);
            if (l.IsMinusInfinity)
                return true.With(r.Sign > 0 ? MinusInfinity : PlusInfinity, out result);

            long nom;
            long denom;

            if (l.up == r.up)
            {
                nom = r.down;
                denom = l.down;
            }
            else if (l.down == r.down)
            {
                nom = l.up;
                denom = r.up;
            }
            else
            {
                Rational a = For (l.up, r.up);
                Rational b = For (r.down, l.down);
                
                try
                {
                    return TryMul (a, b, out result);
                }
                catch (ArithmeticException)
                {
                    return false.Without(out result);
                }
            }

            return true.With(For(nom, denom), out result);
        }
        public static bool TryMul(Rational l, Rational r, out Rational result)
        {
            if (l.IsZero || r.IsZero)
                return true.With(Zero, out result);

            if (l == One)
                return true.With(r, out result);
            if (r == One)
                return true.With(l, out result);

            if (l.IsPlusInfinity)
            {
                if (r.IsPlusInfinity)
                    result = PlusInfinity;
                else if (r.IsMinusInfinity)
                    result = MinusInfinity;
                else if (r.IsZero)
                    result = Zero;
                else
                    result = r.Sign > 0 ? PlusInfinity : MinusInfinity;

                return true;
            }

            if (l.IsMinusInfinity)
            {
                if (r.IsPlusInfinity)
                    result = MinusInfinity;
                else if (r.IsMinusInfinity)
                    result = PlusInfinity;
                else if (r.IsZero)
                    result = Zero;
                else
                    result = r.Sign > 0 ? MinusInfinity : PlusInfinity;

                return true;
            }

            if (r.IsPlusInfinity)
            {
                if (l.IsZero)
                    result = Zero;
                else
                    result = l.Sign > 0 ? PlusInfinity : MinusInfinity;

                return true;
            }

            if (r.IsMinusInfinity)
            {
                if (l.IsZero)
                    result = Zero;
                else
                    result = l.Sign > 0 ? MinusInfinity : PlusInfinity;

                return true;
            }

            long nom;
            long denom;

            try
            {
                Rational a = For(l.up, r.down);
                Rational b = For(r.up, l.down);

                nom = checked(a.up * b.up);
                denom = checked(a.down * b.down);
            }
            catch (ArithmeticException)
            {
                result = null;
                return false;
            }

            return true.With(For(nom, denom), out result);
        }
        public static bool TryUnaryMinus(Rational value, out Rational result)
        {
            if (value.IsZero)
                return true.With(value, out result);

            switch (value.kind)
            {
                case Kind.PlusInfinity:
                    return true.With(MinusInfinity, out result);
                case Kind.MinusInfinity:
                    return true.With(PlusInfinity, out result);
            }

            if (value.IsMinValue)
                return true.With(MaxValue, out result);
            if (value.IsMaxValue)
                return true.With(MinValue, out result);

            return true.With(For(-value.up, value.down), out result);
        }
        public static bool TryAdd(Rational l, Rational r, out Rational result)
        {
            if (l.IsZero)
                return true.With(r, out result);

            if (r.IsZero || l.IsInfinity)
                return true.With(l, out result);

            if (r.IsInfinity)
                return true.With(r, out result);

            if (l.IsMaxValue && r > 0L || r.IsMaxValue && l > 0L)
                return true.With(PlusInfinity, out result);

            long nom;
            long denom;
            try
            {
                if (l.down == r.down)
                {
                    if (l.up == r.up && (r.down & 1L) == 0L)
                    {
                        nom = l.up;
                        denom = l.down >> 1;
                    }
                    else
                    {
                        nom = checked(l.up + r.up);
                        denom = l.down;
                    }
                }
                else
                {
                    nom = checked(l.up * r.down + r.up * l.down);
                    denom = checked(l.down * r.down);
                }
            }
            catch (ArithmeticException)
            {
                try
                {
                    var gcd = GCD(l.down, r.down);
                    nom = checked(l.up * unchecked(r.down / gcd) + r.up * unchecked(l.down / gcd));
                    denom = checked((l.down / gcd) * r.down);
                }
                catch (ArithmeticException)
                {
                    result = null;
                    return false;
                }
            }

            return true.With(denom == 1L ? For(nom) : For(nom, denom), out result);
        }
        public static bool TrySub(Rational l, Rational r, out Rational result)
        {
            if (r.IsZero)
                return true.With(l, out result);

            if (l.IsZero)
                return true.With(-r, out result);

            if (l == r)
                return true.With(Zero, out result);

            if (r < 0L && !r.IsMinValue)
                return TryAdd(l, Rational.Abs(r), out result);

            if (l.IsInfinity)
                return true.With(l, out result);

            if (r.IsInfinity)
                return true.With(-r, out result);

            if (l.IsMinValue && r > 0L)
                return true.With(MinusInfinity, out result);

            long nom;
            long denom;
            try
            {
                if (l.down == r.down)
                {
                    nom = checked(l.up - r.up);
                    denom = l.down;
                }
                else
                {
                    nom = checked(l.up * r.down - r.up * l.down);
                    denom = checked(l.down * r.down);
                }
            }
            catch (ArithmeticException)
            {
                result = null;
                return false;
            }

            return true.With(For(nom, denom), out result);
        }

        private enum Kind
        {
            Normal,
            PlusInfinity,
            MinusInfinity,
        }
    }
}