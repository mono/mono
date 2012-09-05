// 
// Rational.cs
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

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        /// <summary>
        /// Represents a rational number.
        /// </summary>
        public struct Rational : IEquatable<Rational> {
                public static readonly Rational Zero = new Rational (0L);
                public static readonly Rational One = new Rational (1L);
                public static readonly Rational MinusOne = new Rational (-1L);

                public static Rational PlusInfinity = new Rational (Kind.PlusInfinity);
                public static Rational MinusInfinity = new Rational (Kind.MinusInfinity);

                public static Rational MaxValue = new Rational (long.MaxValue);
                public static Rational MinValue = new Rational (long.MinValue);

                readonly long down;
                readonly Kind kind;
                readonly long up;

                Rational (Kind kind)
                {
                        if (kind == Kind.Normal)
                                throw new ArgumentException (
                                        "Kind should be equal to Kind.PlusInfinity or Kind.MinusInfinity", "kind");

                        this.kind = kind;
                        this.up = 0L;
                        this.down = 0L;
                }

                Rational (long number)
                        : this (number, 1L)
                {
                }

                Rational (long nominator, long denominator)
                {
                        if (denominator == 0L)
                                throw new ArgumentException ("Denominator should not be equal to 0");

                        if (nominator == 0L) {
                                this.kind = Kind.Normal;
                                this.up = 0L;
                                this.down = 1L;

                                return;
                        }

                        this.kind = Kind.Normal;

                        int sign = System.Math.Sign (nominator) * System.Math.Sign (denominator);

                        if (nominator == long.MinValue)
                                nominator = sign >= 0 ? long.MaxValue : long.MinValue;
                        else
                                nominator = sign * System.Math.Abs (nominator);

                        if (denominator == long.MinValue)
                                denominator = long.MaxValue;
                        else
                                denominator = System.Math.Abs (denominator);

                        if (nominator % denominator == 0) {
                                this.up = nominator / denominator;
                                this.down = 1L;

                                return;
                        }

                        long gcd = (nominator == 0L || nominator == long.MaxValue ||
                                    denominator == 0L || denominator == long.MaxValue)
                                           ? 1L
                                           : GCD (System.Math.Abs (nominator), System.Math.Abs (denominator));

                        this.up = nominator / gcd;
                        this.down = denominator / gcd;
                }

                public bool IsInteger { get { return !this.IsInfinity && (this.up % this.down == 0L); } }

                public bool IsInt32 { get { return this.IsInteger && this.up >= int.MinValue && this.up <= int.MaxValue; } }

                public bool IsInfinity { get { return this.kind == Kind.PlusInfinity || this.kind == Kind.MinusInfinity; } }
                public bool IsPlusInfinity { get { return this.kind == Kind.PlusInfinity; } }
                public bool IsMinusInfinity { get { return this.kind == Kind.MinusInfinity; } }

                public bool IsZero { get { return this.kind == Kind.Normal && this.up == 0L; } }

                public bool IsMaxValue { get { return this.kind == Kind.Normal && this.up == long.MaxValue && this.down == 1L; } }
                public bool IsMinValue { get { return this.kind == Kind.Normal && this.up == -long.MaxValue && this.down == 1L; } }

                public int Sign { get { return GetSign (this); } }

                public Rational NextInt32
                {
                        get
                        {
                                if (this.IsInfinity)
                                        return this;
                                var next = (long) System.Math.Ceiling ((double) this);

                                return For (next >= int.MaxValue ? int.MaxValue : next);
                        }
                }

                public Rational PreviousInt32
                {
                        get
                        {
                                if (this.IsInfinity)
                                        return this;

                                var prev = (long) System.Math.Floor ((double) this);

                                return For (prev <= int.MinValue ? int.MinValue : prev);
                        }
                }

                public Rational NextInt64
                {
                        get
                        {
                                if (this.IsInfinity)
                                        return this;
                                double next = System.Math.Ceiling ((double) this);

                                return For (next >= (double) long.MaxValue ? long.MaxValue : (long) System.Math.Truncate (next));
                        }
                }

                public long Up { get { return up; } }
                public long Down { get { return down; } }

                public bool IsInRange (long min, long max)
                {
                        return min <= this && this <= max;
                }

                public static Rational For (long number)
                {
                        return new Rational (number);
                }

                public static Rational For (long nominator, long denominator)
                {
                        switch (denominator) {
                                case 0L:
                                        return new Rational (nominator >= 0 ? Kind.PlusInfinity : Kind.MinusInfinity);
                                default:
                                        return new Rational (nominator, denominator);
                        }
                }

                public static bool operator == (Rational l, Rational r)
                {
                        if (l.kind != r.kind)
                                return false;

                        if (l.kind != Kind.Normal)
                                return true;

                        return l.up == r.up && l.down == r.down;
                }

                public static bool operator != (Rational l, Rational r)
                {
                        return !(l == r);
                }

                public static bool operator < (Rational l, Rational r)
                {
                        if (l.IsMinusInfinity && !r.IsMinusInfinity
                            || r.IsPlusInfinity && !l.IsPlusInfinity)
                                return true;
                        if (l.IsPlusInfinity || r.IsMinusInfinity)
                                return false;
                        if (l.down == r.down)
                                return l.up < r.up;
                        if (l.up <= 0L && r.up > 0L)
                                return true;
                        if (l.up < 0L && r.up == 0L)
                                return true;

                        try {
                                return checked(l.up * r.down) < checked(r.up * l.down);
                        }
                        catch (ArithmeticException) {
                                return (decimal) l.up / l.down < (decimal) r.up / r.down;
                        }
                }

                public static bool operator <= (Rational l, Rational r)
                {
                        if (l.IsMinusInfinity || r.IsPlusInfinity)
                                return true;
                        if (l.IsPlusInfinity || r.IsMinusInfinity)
                                return false;

                        if (l.down == r.down)
                                return l.up <= r.up;

                        try {
                                return checked(l.up * r.down) <= checked(r.up * l.down);
                        }
                        catch (ArithmeticException) {
                                return (decimal) l.up / l.down <= (decimal) r.up / r.down;
                        }
                }

                public static bool operator >= (Rational l, Rational r)
                {
                        return r <= l;
                }

                public static bool operator <= (Rational l, long r)
                {
                        switch (l.kind) {
                                case Kind.PlusInfinity:
                                        return false;
                                case Kind.MinusInfinity:
                                        return true;
                                default:
                                        try {
                                                return l.up <= checked(l.down * r);
                                        }
                                        catch (ArithmeticException) {
                                                return (decimal) l.up / l.down <= r;
                                        }
                        }
                }

                public static bool operator <= (long l, Rational r)
                {
                        switch (r.kind) {
                                case Kind.PlusInfinity:
                                        return true;
                                case Kind.MinusInfinity:
                                        return false;
                                default:
                                        try {
                                                return r.up >= checked(r.down * l);
                                        }
                                        catch (ArithmeticException) {
                                                return (decimal) r.up / r.down >= l;
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
                        switch (l.kind) {
                                case Kind.PlusInfinity:
                                        return false;
                                case Kind.MinusInfinity:
                                        return true;
                                default:
                                        try {
                                                return l.up < checked(r * l.down);
                                        }
                                        catch {
                                                return (decimal) l.up / l.down < r;
                                        }
                        }
                }

                public static bool operator > (Rational l, long r)
                {
                        return r < l;
                }

                public static bool operator < (long l, Rational r)
                {
                        switch (r.kind) {
                                case Kind.PlusInfinity:
                                        return true;
                                case Kind.MinusInfinity:
                                        return false;
                                default:
                                        try {
                                                return checked(l * r.down) < r.up;
                                        }
                                        catch {
                                                return l < (decimal) r.up / r.down;
                                        }
                        }
                }

                public static bool operator > (long l, Rational r)
                {
                        return r < l;
                }

                public static Rational operator + (Rational l, Rational r)
                {
                        Rational result;
                        if (TryAdd (l, r, out result))
                                return result;

                        throw new ArithmeticException ();
                }

                public static Rational operator - (Rational l, Rational r)
                {
                        Rational result;
                        if (TrySubtract (l, r, out result))
                                return result;

                        throw new ArithmeticException ();
                }

                public static Rational operator * (Rational l, Rational r)
                {
                        Rational result;
                        if (TryMultiply (l, r, out result))
                                return result;

                        throw new ArithmeticException ();
                }

                public static Rational operator / (Rational l, Rational r)
                {
                        Rational result;
                        if (TryDivide (l, r, out result))
                                return result;

                        throw new ArithmeticException ();
                }

                public static Rational operator - (Rational l, long i)
                {
                        if (l.kind == Kind.Normal && l.down == 1L)
                                try {
                                        return For (checked(l.up - i));
                                }
                                catch (ArithmeticException) {
                                }

                        return l - For (i);
                }

                public static Rational operator + (Rational l, long i)
                {
                        if (l.kind == Kind.Normal && l.down == 1L)
                                try {
                                        return For (checked(l.up + i));
                                }
                                catch (ArithmeticException) {
                                }

                        return l + For (i);
                }

                public static Rational operator - (Rational value)
                {
                        Rational result;
                        if (TryUnaryMinus (value, out result))
                                return result;

                        throw new ArithmeticException ();
                }

                public static explicit operator double (Rational r)
                {
                        switch (r.kind) {
                                case Kind.PlusInfinity:
                                        return double.PositiveInfinity;
                                case Kind.MinusInfinity:
                                        return double.NegativeInfinity;
                                default:
                                        return (double) r.up / r.down;
                        }
                }

                public static explicit operator long (Rational r)
                {
                        if (r.down == 0L)
                                return r.up >= 0L ? long.MaxValue : long.MinValue;

                        if (!r.IsInteger)
                                return (long) System.Math.Round ((double) r.up / r.down);

                        return r.up;
                }

                public static explicit operator int (Rational r)
                {
                        if (r.down != 0L)
                                return (int) System.Math.Round ((double) r.up / r.down);

                        return r.up >= 0L ? int.MaxValue : int.MinValue;
                }

                public static implicit operator Rational (long l)
                {
                        return For (l);
                }

                public static Rational Abs (Rational a)
                {
                        switch (a.kind) {
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

                public static Rational Min (Rational a, Rational b)
                {
                        return a < b ? a : b;
                }

                public static bool TryAdd (Rational l, Rational r, out Rational result)
                {
                        if (l.IsZero)
                                return true.With (r, out result);

                        if (r.IsZero || l.IsInfinity)
                                return true.With (l, out result);

                        if (r.IsInfinity)
                                return true.With (r, out result);

                        if (l.IsMaxValue && r > 0L || r.IsMaxValue && l > 0L)
                                return true.With (PlusInfinity, out result);

                        long nom;
                        long denom;
                        try {
                                if (l.down == r.down) {
                                        if (l.up == r.up && (r.down & 1L) == 0L) {
                                                nom = l.up;
                                                denom = l.down >> 1;
                                        }
                                        else {
                                                nom = checked (l.up + r.up);
                                                denom = l.down;
                                        }
                                }
                                else {
                                        nom = checked (l.up * r.down + r.up * l.down);
                                        denom = checked (l.down * r.down);
                                }
                        }
                        catch (ArithmeticException) {
                                try {
                                        long gcd = GCD (l.down, r.down);
                                        nom =
                                                checked (
                                                        l.up * unchecked (r.down / gcd) +
                                                        r.up * unchecked (l.down / gcd));
                                        denom = checked ((l.down / gcd) * r.down);
                                }
                                catch (ArithmeticException) {
                                        return false.Without (out result);
                                }
                        }

                        return true.With (denom == 1L ? For (nom) : For (nom, denom), out result);
                }

                public static bool TrySubtract (Rational l, Rational r, out Rational result)
                {
                        if (r.IsZero)
                                return true.With (l, out result);

                        if (l.IsZero)
                                return true.With (-r, out result);

                        if (l == r)
                                return true.With (Zero, out result);

                        if (r < 0L && !r.IsMinValue)
                                return TryAdd (l, Abs (r), out result);

                        if (l.IsInfinity)
                                return true.With (l, out result);

                        if (r.IsInfinity)
                                return true.With (-r, out result);

                        if (l.IsMinValue && r > 0L)
                                return true.With (MinusInfinity, out result);

                        long nom;
                        long denom;
                        try {
                                if (l.down == r.down) {
                                        nom = checked (l.up - r.up);
                                        denom = l.down;
                                }
                                else {
                                        nom = checked (l.up * r.down - r.up * l.down);
                                        denom = checked (l.down * r.down);
                                }
                        }
                        catch (ArithmeticException) {
                                return false.Without (out result);
                        }

                        return true.With (For (nom, denom), out result);
                }

                public static bool TryDivide (Rational l, Rational r, out Rational result)
                {
                        if (r == One)
                                return true.With (l, out result);

                        if (r.IsZero)
                                return false.Without (out result);

                        if (l.IsZero || r.IsInfinity)
                                return true.With (Zero, out result);

                        if (l.IsPlusInfinity)
                                return true.With (r.Sign > 0 ? PlusInfinity : MinusInfinity, out result);

                        if (l.IsMinusInfinity)
                                return true.With (r.Sign > 0 ? MinusInfinity : PlusInfinity, out result);

                        long nom;
                        long denom;

                        if (l.up == r.up) {
                                // (a/b)/(a/c) = (c/b)

                                nom = r.down;
                                denom = l.down;
                        }
                        else if (l.down == r.down) {
                                // (a/c)/(b/c) = (a/b)

                                nom = l.up;
                                denom = r.up;
                        }
                        else {
                                // (x/y) / (e/f) == (x/e) * (f/y)

                                Rational a = For (l.up, r.up);
                                Rational b = For (r.down, l.down);

                                try {
                                        return TryMultiply (a, b, out result);
                                }
                                catch (ArithmeticException) {
                                        return false.Without (out result);
                                }
                        }

                        return true.With (For (nom, denom), out result);
                }

                public static bool TryMultiply (Rational l, Rational r, out Rational result)
                {
                        if (l.IsZero || r.IsZero)
                                return true.With (Zero, out result);

                        if (l == One)
                                return true.With (r, out result);
                        if (r == One)
                                return true.With (l, out result);

                        if (l.IsPlusInfinity) {
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

                        if (l.IsMinusInfinity) {
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

                        if (r.IsPlusInfinity) {
                                if (l.IsZero)
                                        result = Zero;
                                else
                                        result = l.Sign > 0 ? PlusInfinity : MinusInfinity;

                                return true;
                        }

                        if (r.IsMinusInfinity) {
                                if (l.IsZero)
                                        result = Zero;
                                else
                                        result = l.Sign > 0 ? MinusInfinity : PlusInfinity;

                                return true;
                        }

                        long nom;
                        long denom;

                        try {
                                Rational a = For (l.up, r.down);
                                Rational b = For (r.up, l.down);

                                nom = checked(a.up * b.up);
                                denom = checked(a.down * b.down);
                        }
                        catch (ArithmeticException) {
                                return false.Without (out result);
                        }

                        return true.With (For (nom, denom), out result);
                }

                public static bool TryUnaryMinus (Rational value, out Rational result)
                {
                        if (value.IsZero)
                                return true.With (value, out result);

                        switch (value.kind) {
                                case Kind.PlusInfinity:
                                        return true.With (MinusInfinity, out result);
                                case Kind.MinusInfinity:
                                        return true.With (PlusInfinity, out result);
                        }

                        if (value.IsMinValue)
                                return true.With (MaxValue, out result);
                        if (value.IsMaxValue)
                                return true.With (MinValue, out result);

                        return true.With (For (-value.up, value.down), out result);
                }

                static int GetSign (Rational r)
                {
                        switch (r.kind) {
                                case Kind.PlusInfinity:
                                        return 1;
                                case Kind.MinusInfinity:
                                        return -1;
                                default:
                                        return System.Math.Sign (r.up) * System.Math.Sign (r.down);
                        }
                }

                static long GCD (long a, long b)
                {
                        var aa = (ulong) a;
                        var bb = (ulong) b;

                        int pow = 0;

                        while (((aa | bb) & 1L) == 0L) //while both divide by 2
                        {
                                aa >>= 1;
                                bb >>= 1;
                                ++pow;
                        }

                        while ((aa & 1L) == 0L) //get rid of other 2's factors
                                aa >>= 1;

                        do {
                                while ((bb & 1L) == 0L)
                                        bb >>= 1;

                                ulong cc;
                                if (aa < bb)
                                        cc = bb - aa;
                                else {
                                        ulong tmp = aa - bb;
                                        aa = bb;
                                        cc = tmp;
                                }

                                bb = cc >> 1;
                        } while (bb != 0L);

                        return (long) aa << pow;
                }

                public override bool Equals (object obj)
                {
                        if (ReferenceEquals (null, obj))
                                return false;
                        if (ReferenceEquals (this, obj))
                                return true;
                        return obj is Rational && this.Equals ((Rational) obj);
                }

                public bool Equals (Rational other)
                {
                        return this == other;
                }

                public override int GetHashCode ()
                {
                        unchecked {
                                int hashCode = this.kind.GetHashCode ();
                                hashCode = (hashCode * 397) ^ this.up.GetHashCode ();
                                hashCode = (hashCode * 1001) ^ this.down.GetHashCode ();
                                return hashCode;
                        }
                }

                public override string ToString ()
                {
                        switch (this.kind) {
                                case Kind.MinusInfinity:
                                        return "-oo" +
                                               (this.up == -1L || this.down == 0L
                                                        ? ""
                                                        : string.Format ("({0} / {1})", this.up, this.down));
                                case Kind.PlusInfinity:
                                        return "+oo" +
                                               (this.up == 1L || this.down == 0L
                                                        ? ""
                                                        : string.Format ("({0} / {1})", this.up, this.down));
                                default:
                                        return this.IsInteger
                                                       ? this.up.ToString ()
                                                       : string.Format ("({0} / {1})", this.up, this.down);
                        }
                }

                enum Kind {
                        Normal,
                        PlusInfinity,
                        MinusInfinity,
                }
        }

        static class RationalExtensions {
                public static TExpr ToExpression<TVar, TExpr>(this Rational value, IExpressionEncoder<TVar, TExpr> encoder )
                {
                        if (value.IsInteger)
                                return encoder.ConstantFor ((long) value);
                        if (value.IsPlusInfinity)
                                return encoder.CompoundFor (ExpressionType.Int32, ExpressionOperator.Div,
                                                            encoder.ConstantFor (1L), encoder.ConstantFor (0L));
                        if (value.IsMinusInfinity)
                                return encoder.CompoundFor (ExpressionType.Int32, ExpressionOperator.Div,
                                                            encoder.ConstantFor (-1L), encoder.ConstantFor (0L));

                        TExpr l = encoder.ConstantFor (value.Up);
                        TExpr r = encoder.ConstantFor (value.Down);

                        return encoder.CompoundFor (ExpressionType.Int32, ExpressionOperator.Div, l, r);
                }
        }
}
