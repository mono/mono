//
// System.Decimal.cs
//
// Represents a floating-point decimal data type with up to 29 
// significant digits, suitable for financial and commercial calculations.
//
// Author:
//   Martin Weindel (martin.weindel@t-online.de)
//
// (C) 2001 Martin Weindel
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Text;
using System.Runtime.CompilerServices;
#if MSTEST
using System.Runtime.InteropServices;
#endif


namespace System
{
    /// <summary>
    /// Represents a floating-point decimal data type with up to 29 significant
    /// digits, suitable for financial and commercial calculations
    /// </summary>
	[Serializable]
    public struct Decimal: IFormattable, IConvertible,
#if NET_2_0
	IComparable, IComparable<Decimal>
#else
	IComparable
#endif
    {
#if BOOTSTRAP_WITH_OLDLIB

	// LAMESPEC: the attributes aren't mentioned, but show up in CorCompare
	// Unfortunately, corcompare starts throwing security exceptions when
	// these attributes are present...

	[DecimalConstantAttribute(0, 1, unchecked((uint)-1), unchecked((uint)-1), unchecked((uint)-1))]
        public static readonly Decimal MinValue = new Decimal(-1, -1, -1, true, 0);
	[DecimalConstantAttribute(0, 0, unchecked((uint)-1), unchecked((uint)-1), unchecked((uint)-1))]
        public static readonly Decimal MaxValue = new Decimal(-1, -1, -1, false, 0);
	[DecimalConstantAttribute(0, 1, 0, 0, 1)]
        public static readonly Decimal MinusOne = new Decimal(1, 0, 0, true, 0);
	[DecimalConstantAttribute(0, 0, 0, 0, 1)]
        public static readonly Decimal One = new Decimal(1, 0, 0, false, 0);
	[DecimalConstantAttribute(0, 0, 0, 0, 0)]
        public static readonly Decimal Zero = new Decimal(0, 0, 0, false, 0);
#else
        public const decimal MinValue = -79228162514264337593543950335m;
        public const decimal MaxValue =  79228162514264337593543950335m;
        
        public const decimal MinusOne = -1;
        public const decimal One = 1;
        public const decimal Zero = 0;
#endif

	private static readonly Decimal MaxValueDiv10 = MaxValue / 10;

        // maximal decimal value as double
        private static readonly double dDecMaxValue = 7.922816251426433759354395033e28;
        // epsilon decimal value as double
        private static readonly double dDecEpsilon = 0.5e-28;  // == 0.5 * 1 / 10^28

        // some constants
        private const int DECIMAL_DIVIDE_BY_ZERO = 5;
        private const uint MAX_SCALE = 28;
        private const int iMAX_SCALE = 28;
        private const uint SIGN_FLAG = 0x80000000;
        private const uint SCALE_MASK = 0x00FF0000;
        private const int SCALE_SHIFT = 16;
        private const uint RESERVED_SS32_BITS = 0x7F00FFFF;

        // internal representation of decimal
        private uint ss32;
        private uint hi32;
        private uint lo32;
        private uint mid32;

        public Decimal(int lo, int mid, int hi, bool isNegative, byte scale)
        {
            unchecked 
            {
                lo32 = (uint) lo;
                mid32 = (uint) mid;
                hi32 = (uint) hi;
            
                if (scale > MAX_SCALE) 
                {
			throw new ArgumentOutOfRangeException (Locale.GetText ("scale must be between 0 and 28"));
                }

                ss32 = scale;
                ss32 <<= SCALE_SHIFT;
                if (isNegative) ss32 |= SIGN_FLAG;
            }
        }

        public Decimal(int val) 
        {
            unchecked 
            {
                hi32 = mid32 = 0;
                if (val < 0) 
                {
                    ss32 = SIGN_FLAG;
                    lo32 = ((uint)~val) + 1;
                }
                else 
                {
                    ss32 = 0;
                    lo32 = (uint) val;
                }
            }
        }

        [CLSCompliant(false)]
        public Decimal(uint val) 
        {
            lo32 = val;
            ss32 = hi32 = mid32 = 0;
        }

        public Decimal(long val) 
        {
            unchecked 
            {
                hi32 = 0;
                if (val < 0) 
                {
                    ss32 = SIGN_FLAG;
                    ulong u = ((ulong)~val) + 1;
                    lo32 = (uint)u;
                    mid32 = (uint)(u >> 32);
                }
                else 
                {
                    ss32 = 0;
                    ulong u = (ulong)val;
                    lo32 = (uint)u;
                    mid32 = (uint)(u >> 32);
                }
            }
        }

        [CLSCompliant(false)]
        public Decimal(ulong uval) 
        {
            unchecked 
            {
                ss32 = hi32 = 0;
                lo32 = (uint)uval;
                mid32 = (uint)(uval >> 32);
            }
        }

        public Decimal (float val) 
        {
		if (val > (float)Decimal.MaxValue || val < (float)Decimal.MinValue) {
			throw new OverflowException (Locale.GetText (
				"Value is greater than Decimal.MaxValue or less than Decimal.MinValue"));
		}
		// we must respect the precision (double2decimal doesn't)
		Decimal d = Decimal.Parse (val.ToString (CultureInfo.InvariantCulture),
				NumberStyles.Float, CultureInfo.InvariantCulture);
		ss32 = d.ss32;
		hi32 = d.hi32;
		lo32 = d.lo32;
		mid32 = d.mid32;
	}

	public Decimal (double val) 
	{
		if (val > (double)Decimal.MaxValue || val < (double)Decimal.MinValue) {
			throw new OverflowException (Locale.GetText (
				"Value is greater than Decimal.MaxValue or less than Decimal.MinValue"));
		}
		// we must respect the precision (double2decimal doesn't)
		Decimal d = Decimal.Parse (val.ToString (CultureInfo.InvariantCulture),
				NumberStyles.Float, CultureInfo.InvariantCulture);
		ss32 = d.ss32;
		hi32 = d.hi32;
		lo32 = d.lo32;
		mid32 = d.mid32;
        }

        public Decimal(int[] bits) 
        {
            if (bits == null) 
            {
		throw new ArgumentNullException(Locale.GetText ("Bits is a null reference"));
            }

            if (bits.GetLength(0) != 4) 
            {
                throw new ArgumentException(Locale.GetText ("bits does not contain four values"));
            }

            unchecked {
                lo32 = (uint) bits[0];
                mid32 = (uint) bits[1];
                hi32 = (uint) bits[2];
                ss32 = (uint) bits[3];
                byte scale = (byte)(ss32 >> SCALE_SHIFT);
                if (scale > MAX_SCALE || (ss32 & RESERVED_SS32_BITS) != 0) 
                {
                    throw new ArgumentException(Locale.GetText ("Invalid bits[3]"));
                }
            }
        }

		public static decimal FromOACurrency(long cy)
		{
			return (decimal)cy / (decimal)10000;
		}
	
        public static int[] GetBits(Decimal d) 
        {
            unchecked 
            {
                return new int[] { (int)d.lo32, (int)d.mid32, (int)d.hi32, 
                                     (int)d.ss32 };
            }
        }

        public static Decimal Negate(Decimal d) 
        {
            d.ss32 ^= SIGN_FLAG;
            return d;
        }


        public static Decimal Add(Decimal d1, Decimal d2) 
        {
            if (decimalIncr(ref d1, ref d2) == 0)
                return d1;
            else
                throw new OverflowException(Locale.GetText ("Overflow on adding decimal number"));
        }

        public static Decimal Subtract(Decimal d1, Decimal d2) 
        {
            d2.ss32 ^= SIGN_FLAG;
	    int result = decimalIncr(ref d1, ref d2);
            if (result == 0)
                return d1;
            else
                throw new OverflowException(Locale.GetText ("Overflow on subtracting decimal numbers ("+result+")"));
        }

	public override int GetHashCode () 
	{
		return (int) (ss32 ^ hi32 ^ lo32 ^ mid32);
	}

        public static Decimal operator +(Decimal d1, Decimal d2)
        {
            return Add(d1, d2);
        }

        public static Decimal operator --(Decimal d) 
        {
            return Add(d, MinusOne);
        }

        public static Decimal operator ++(Decimal d) 
        {
            return Add(d, One);
        }

        public static Decimal operator -(Decimal d1, Decimal d2) 
        {
            return Subtract(d1, d2);
        }

        public static Decimal operator -(Decimal d) 
        {
            return Negate(d);
        }

        public static Decimal operator +(Decimal d) 
        {
            return d;
        }

        public static Decimal operator *(Decimal d1, Decimal d2)
        {
            return Multiply(d1, d2);
        }

        public static Decimal operator /(Decimal d1, Decimal d2) 
        {
            return Divide(d1, d2);
        }

        public static Decimal operator %(Decimal d1, Decimal d2) 
        {
            return Remainder(d1, d2);
        }

	private static ulong u64 (Decimal value) 
	{
		ulong result;

		decimalFloorAndTrunc (ref value, 0);
		if (decimal2UInt64 (ref value, out result) != 0) {
			throw new System.OverflowException ();
		}
		return result;
	}

	private static long s64 (Decimal value) 
	{
		long result;

		decimalFloorAndTrunc (ref value, 0);
		if (decimal2Int64 (ref value, out result) != 0) {
			throw new System.OverflowException ();
		}
		return result;
	}

	public static explicit operator byte (Decimal val)
	{
		ulong result = u64 (val);
		return checked ((byte) result);
	}

	[CLSCompliant (false)]
	public static explicit operator sbyte (Decimal val)
	{
		long result = s64 (val);
		return checked ((sbyte) result);
	}

	public static explicit operator char (Decimal val) 
	{
		ulong result = u64 (val);
		return checked ((char) result);
	}

	public static explicit operator short (Decimal val) 
	{
		long result = s64 (val);
		return checked ((short) result);
	}

	[CLSCompliant (false)]
	public static explicit operator ushort (Decimal val) 
	{
		ulong result = u64 (val);
		return checked ((ushort) result);
	}

	public static explicit operator int (Decimal val) 
	{
		long result = s64 (val);
		return checked ((int) result);
	}

	[CLSCompliant(false)]
	public static explicit operator uint (Decimal val) 
	{
		ulong result = u64 (val);
		return checked ((uint) result);
	}

	public static explicit operator long (Decimal val) 
	{
		return s64 (val);
        }

	[CLSCompliant(false)]
	public static explicit operator ulong (Decimal val) 
	{
		return u64 (val);
	}

        public static implicit operator Decimal(byte val) 
        {
            return new Decimal(val);
        }

	[CLSCompliant(false)]
        public static implicit operator Decimal(sbyte val) 
        {
            return new Decimal(val);
        }

        public static implicit operator Decimal(short val) 
        {
            return new Decimal(val);
        }

	[CLSCompliant(false)]
        public static implicit operator Decimal(ushort val) 
        {
            return new Decimal(val);
        }

        public static implicit operator Decimal(char val) 
        {
            return new Decimal(val);
        }

        public static implicit operator Decimal(int val) 
        {
            return new Decimal(val);
        }

	[CLSCompliant(false)]
        public static implicit operator Decimal(uint val) 
        {
            return new Decimal(val);
        }

        public static implicit operator Decimal(long val) 
        {
            return new Decimal(val);
        }

	[CLSCompliant(false)]
        public static implicit operator Decimal(ulong val) 
        {
            return new Decimal(val);
        }

        public static explicit operator Decimal(float val) 
        {
            return new Decimal(val);
        }

        public static explicit operator Decimal(double val)
        {
            return new Decimal(val);
        }

        public static explicit operator float(Decimal val)
        {
            return (float) (double) val;
        }

        public static explicit operator double(Decimal val)
        {
            return decimal2double(ref val);
        }


        public static bool operator !=(Decimal d1, Decimal d2) 
        {
            return !Equals(d1, d2);
        }

        public static bool operator ==(Decimal d1, Decimal d2) 
        {
            return Equals(d1, d2);
        }

        public static bool operator >(Decimal d1, Decimal d2) 
        {
            return decimalCompare(ref d1, ref d2) > 0;
        }

        public static bool operator >=(Decimal d1, Decimal d2) 
        {
            return decimalCompare(ref d1, ref d2) >= 0;
        }

        public static bool operator <(Decimal d1, Decimal d2) 
        {
            return decimalCompare(ref d1, ref d2) < 0;
        }

        public static bool operator <=(Decimal d1, Decimal d2) 
        {
            return decimalCompare(ref d1, ref d2) <= 0;
        }

        public static bool Equals(Decimal d1, Decimal d2) 
        {
            return decimalCompare(ref d1, ref d2) == 0;
        }

        public override bool Equals(object o) 
        {
            if (!(o is Decimal))
                return false;

            return Equals((Decimal) o, this);
        }

	// avoid unmanaged call
	private bool IsZero () 
	{
		return ((hi32 == 0) && (lo32 == 0) && (mid32 == 0));
	}

	// avoid unmanaged call
	private bool IsNegative () 
	{
		return ((ss32 & 0x80000000) == 0x80000000);
	}

        public static Decimal Floor(Decimal d) 
        {
            decimalFloorAndTrunc(ref d, 1);
            return d;
        }

        public static Decimal Truncate(Decimal d) 
        {
            decimalFloorAndTrunc(ref d, 0);
            return d;
        }

        public static Decimal Round (Decimal d, int decimals) 
        {
		if (decimals < 0 || decimals > 28) {
			throw new ArgumentOutOfRangeException ("decimals", "[0,28]");
		}

		bool negative = d.IsNegative ();
		if (negative)
			d.ss32 ^= SIGN_FLAG;

		// Moved from Math.cs because it's easier to fix the "sign"
		// issue here :( as the logic is OK only for positive numbers
		decimal p = (decimal) Math.Pow (10, decimals);
		decimal int_part = Decimal.Floor (d);
		decimal dec_part = d - int_part;
		dec_part *= 10000000000000000000000000000M;
		dec_part = Decimal.Floor(dec_part);
		dec_part /= (10000000000000000000000000000M / p);
		dec_part = Math.Round (dec_part);
		dec_part /= p;
		decimal result = int_part + dec_part;

		// that fixes the precision/scale (which we must keep for output)
		// (moved and adapted from System.Data.SqlTypes.SqlMoney)
		long scaleDiff = decimals - ((result.ss32 & 0x7FFF0000) >> 16);
		// integrify
		if (scaleDiff > 0) {
			// note: here we always work with positive numbers
			while (scaleDiff > 0) {
				if (result > MaxValueDiv10)
					break;
				result *= 10;
				scaleDiff--;
			}
		}
		else if (scaleDiff < 0) {
			while (scaleDiff < 0) {
				result /= 10;
				scaleDiff++;
			}
		}
		result.ss32 = (uint)((decimals - scaleDiff) << SCALE_SHIFT);

		if (negative)
			result.ss32 ^= SIGN_FLAG;
		return result;
        }

        public static Decimal Multiply (Decimal d1, Decimal d2) 
        {
		if (d1.IsZero () || d2.IsZero ())
			return Decimal.Zero;

		if (decimalMult (ref d1, ref d2) != 0)
			throw new OverflowException ();
		return d1;
        }

        public static Decimal Divide (Decimal d1, Decimal d2) 
        {
		if (d2.IsZero ())
			throw new DivideByZeroException ();
		if (d2.IsZero ())
			return Decimal.Zero;
		if (d1 == d2)
			return Decimal.One;

		d1.ss32 ^= SIGN_FLAG;
		if (d1 == d2)
			return Decimal.MinusOne;
		d1.ss32 ^= SIGN_FLAG;

		Decimal result;
		if (decimalDiv (out result, ref d1, ref d2) != 0)
			throw new OverflowException ();

		return result;
        }

        public static Decimal Remainder (Decimal d1, Decimal d2) 
        {
		if (d2.IsZero ())
			throw new DivideByZeroException ();
		if (d1.IsZero ())
			return Decimal.Zero;

		bool negative = d1.IsNegative ();
		if (negative)
			d1.ss32 ^= SIGN_FLAG;
		if (d2.IsNegative ())
			d2.ss32 ^= SIGN_FLAG;

		Decimal result;
		if (d1 == d2) {
			return Decimal.Zero;
		}
		else if (d2 > d1) {
			result = d1;
		}
		else {
			if (decimalIntDiv (out result, ref d1, ref d2) != 0)
				throw new OverflowException ();

			// FIXME: not really performant here
			result = d1 - result * d2;
		}

		if (negative)
			result.ss32 ^= SIGN_FLAG;
		return result;
        }

        public static int Compare(Decimal d1, Decimal d2) 
        {
            return decimalCompare(ref d1, ref d2);
        }

        public int CompareTo(object val)
        {
	    if (val == null)
		return 1;
	    
            if (!(val is Decimal))
                throw new ArgumentException (Locale.GetText ("Value is not a System.Decimal"));

            Decimal d2 = (Decimal)val;
            return decimalCompare(ref this, ref d2);
        }

#if NET_2_0
	public int CompareTo(Decimal value)
	{
	    return decimalCompare(ref this, ref value);
	}

        public bool Equals(Decimal value) 
        {
            return Equals(value, this);
        }
#endif

        public static Decimal Parse(string s) 
        {
            return Parse(s, NumberStyles.Number, null);
        }

        public static Decimal Parse(string s, NumberStyles style) 
        {
            return Parse(s, style, null);
        }

        public static Decimal Parse(string s, IFormatProvider provider) 
        {
            return Parse(s, NumberStyles.Number, provider);
        }

        private static string stripStyles(string s, NumberStyles style, NumberFormatInfo nfi, 
            out int decPos, out bool isNegative, out bool expFlag, out int exp)
        {
            string invalidChar = Locale.GetText ("Invalid character at position ");
            string invalidExponent = Locale.GetText ("Invalid exponent");
            isNegative = false;
            expFlag = false;
            exp = 0;
            decPos = -1;

            bool hasSign = false;
            bool hasOpeningParentheses = false;
            bool hasDecimalPoint = false;
            bool allowedLeadingWhiteSpace = ((style & NumberStyles.AllowLeadingWhite) != 0);
            bool allowedTrailingWhiteSpace = ((style & NumberStyles.AllowTrailingWhite) != 0);
            bool allowedLeadingSign = ((style & NumberStyles.AllowLeadingSign) != 0);
            bool allowedTrailingSign = ((style & NumberStyles.AllowTrailingSign) != 0);
            bool allowedParentheses = ((style & NumberStyles.AllowParentheses) != 0);
            bool allowedThousands = ((style & NumberStyles.AllowThousands) != 0);
            bool allowedDecimalPoint = ((style & NumberStyles.AllowDecimalPoint) != 0);
            bool allowedExponent = ((style & NumberStyles.AllowExponent) != 0);

            /* get rid of currency symbol */
            bool hasCurrency = false;
            if ((style & NumberStyles.AllowCurrencySymbol) != 0)
            {
                int index = s.IndexOf(nfi.CurrencySymbol);
                if (index >= 0) 
                {
                    s = s.Remove(index, nfi.CurrencySymbol.Length);
                    hasCurrency = true;
                }
            }

            string decimalSep = (hasCurrency) ? nfi.CurrencyDecimalSeparator : nfi.NumberDecimalSeparator;
            string groupSep = (hasCurrency) ? nfi.CurrencyGroupSeparator : nfi.NumberGroupSeparator;

            int pos = 0;
            int len = s.Length;

            StringBuilder sb = new StringBuilder(len);

            // leading
            while (pos < len) 
            {
                char ch = s[pos];
                if (Char.IsDigit(ch)) 
                {
                    break; // end of leading
                }
                else if (allowedLeadingWhiteSpace && Char.IsWhiteSpace(ch)) 
                {
                    pos++;
                }
                else if (allowedParentheses && ch == '(' && !hasSign && !hasOpeningParentheses) 
                {
                    hasOpeningParentheses = true;
                    hasSign = true;
                    isNegative = true;
                    pos++;
                }
                else if (allowedLeadingSign && ch == nfi.NegativeSign[0] && !hasSign) 
                {
                    int slen = nfi.NegativeSign.Length;
                    if (slen == 1 || s.IndexOf(nfi.NegativeSign, pos, slen) == pos) 
                    {
                        hasSign = true;
                        isNegative = true;
                        pos += slen;
                    }
                }
                else if (allowedLeadingSign && ch == nfi.PositiveSign[0] && !hasSign) 
                {
                    int slen = nfi.PositiveSign.Length;
                    if (slen == 1 || s.IndexOf(nfi.PositiveSign, pos, slen) == pos) 
                    {
                        hasSign = true;
                        pos += slen;
                    }
                }
                else if (allowedDecimalPoint && ch == decimalSep[0])
                {
                    int slen = decimalSep.Length;
                    if (slen != 1 && s.IndexOf(decimalSep, pos, slen) != pos) 
                    {
                        throw new FormatException(invalidChar + pos);
                    }
                    break;
                }
                else
                {
                    throw new FormatException(invalidChar + pos);
                }
            }

            if (pos == len)
		throw new FormatException(Locale.GetText ("No digits found"));

            // digits 
            while (pos < len)
            {
                char ch = s[pos];
                if (Char.IsDigit(ch)) 
                {
                    sb.Append(ch);
                    pos++;
                }
                else if (allowedThousands && ch == groupSep[0]) 
                {
                    int slen = groupSep.Length;
                    if (slen != 1 && s.IndexOf(groupSep, pos, slen) != pos) 
                    {
                        throw new FormatException(invalidChar + pos);
                    }
                    pos += slen;
                }
                else if (allowedDecimalPoint && ch == decimalSep[0] && !hasDecimalPoint)
                {
                    int slen = decimalSep.Length;
                    if (slen == 1 || s.IndexOf(decimalSep, pos, slen) == pos) 
                    {
                        decPos = sb.Length;
                        hasDecimalPoint = true;
                        pos += slen;
                    }
                }
                else
                {
                    break;
                }
            }

            // exponent
            if (pos < len)
            {
                char ch = s[pos];
                if (allowedExponent && Char.ToUpperInvariant (ch) == 'E')
                {
                    expFlag = true;
                    pos++; if (pos >= len) throw new FormatException(invalidExponent);
                    ch = s[pos];
                    bool isNegativeExp = false;
                    if (ch == nfi.PositiveSign[0])
                    {
                        int slen = nfi.PositiveSign.Length;
                        if (slen == 1 || s.IndexOf(nfi.PositiveSign, pos, slen) == pos) 
                        {
                            pos += slen;  if (pos >= len) throw new FormatException(invalidExponent);
                        }
                    }
                    else if (ch == nfi.NegativeSign[0])
                    {
                        int slen = nfi.NegativeSign.Length;
                        if (slen == 1 || s.IndexOf(nfi.NegativeSign, pos, slen) == pos) 
                        {
                            pos += slen; if (pos >= len) throw new FormatException(invalidExponent);
                            isNegativeExp = true;
                        }
                    }
                    ch = s[pos];
                    if (!Char.IsDigit(ch)) throw new FormatException(invalidExponent);
                    exp = ch - '0';
                    pos++;
                    while (pos < len && Char.IsDigit(s[pos])) 
                    {
                        exp *= 10;
                        exp += s[pos] - '0';
                        pos++;
                    }
                    if (isNegativeExp) exp *= -1;
                }
            }

            // trailing
            while (pos < len)
            {
                char ch = s[pos];
                if (allowedTrailingWhiteSpace && Char.IsWhiteSpace(ch)) 
                {
                    pos++;
                }
                else if (allowedParentheses && ch == ')' && hasOpeningParentheses) 
                {
                    hasOpeningParentheses = false;
                    pos++;
                }
                else if (allowedTrailingSign && ch == nfi.NegativeSign[0] && !hasSign) 
                {
                    int slen = nfi.NegativeSign.Length;
                    if (slen == 1 || s.IndexOf(nfi.NegativeSign, pos, slen) == pos) 
                    {
                        hasSign = true;
                        isNegative = true;
                        pos += slen;
                    }
                }
                else if (allowedTrailingSign && ch == nfi.PositiveSign[0] && !hasSign) 
                {
                    int slen = nfi.PositiveSign.Length;
                    if (slen == 1 || s.IndexOf(nfi.PositiveSign, pos, slen) == pos) 
                    {
                        hasSign = true;
                        pos += slen;
                    }
                }
                else
                {
                    throw new FormatException(invalidChar + pos);
                }
            }

            if (hasOpeningParentheses) throw new FormatException (
		    Locale.GetText ("Closing Parentheses not found"));
	    
            if (!hasDecimalPoint) decPos = sb.Length;

            return sb.ToString();
        }

	public static Decimal Parse (string s, NumberStyles style, IFormatProvider provider) 
	{
		if (s == null)
			throw new ArgumentNullException ("s");

		NumberFormatInfo nfi = NumberFormatInfo.GetInstance (provider);

		int iDecPos, exp;
		bool isNegative, expFlag;
		s = stripStyles(s, style, nfi, out iDecPos, out isNegative, out expFlag, out exp);

		if (iDecPos < 0)
			throw new Exception (Locale.GetText ("Error in System.Decimal.Parse"));

		// first we remove leading 0
		int len = s.Length;
		int i = 0;
		while ((i < iDecPos) && (s [i] == '0'))
			i++;
		if ((i > 1) && (len > 1)) {
			s = s.Substring (i, len - i);
			iDecPos -= i;
		}

		// first 0. may not be here but is part of the maximum length
		int max = ((iDecPos == 0) ? 27 : 28);
		len = s.Length;
		if (len >= max + 1) {
			// number lower than MaxValue (base-less) can have better precision
			if (String.Compare (s, 0, "79228162514264337593543950335", 0, max + 1,
				false, CultureInfo.InvariantCulture) <= 0) {
				max++;
			}
		}

		// then we trunc the string
		if ((len > max) && (iDecPos < len)) {
			int round = (s [max] - '0');
			s = s.Substring (0, max);

			bool addone = false;
			if (round > 5) {
				addone = true;
			}
			else if (round == 5) {
				if (isNegative) {
					addone = true;
				}
				else {
					// banker rounding applies :(
					int previous = (s [max - 1] - '0');
					addone = ((previous & 0x01) == 0x01);
				}
			}
			if (addone) {
				char[] array = s.ToCharArray ();
				int p = max - 1;
				while (p >= 0) {
					int b = (array [p] - '0');
					if (array [p] != '9') {
						array [p] = (char)(b + '1');
						break;
					}
					else {
						array [p--] = '0';
					}
				}
				if ((p == -1) && (array [0] == '0')) {
					iDecPos++;
					s = "1".PadRight (iDecPos, '0');
				}
				else
					s = new String (array);
			}
		}

		Decimal result;
		// always work in positive (rounding issues)
		if (string2decimal (out result, s, (uint)iDecPos, 0) != 0)
			throw new OverflowException ();

		if (expFlag) {
			if (decimalSetExponent (ref result, exp) != 0)
				throw new OverflowException ();
		}

		if (isNegative)
			result.ss32 ^= SIGN_FLAG;
		return result;
        }

	public TypeCode GetTypeCode ()
	{
	    return TypeCode.Decimal;
	}

	public static byte ToByte (decimal value)
	{
		if (value > Byte.MaxValue || value < Byte.MinValue)
			throw new OverflowException (Locale.GetText (
				"Value is greater than Byte.MaxValue or less than Byte.MinValue"));
	  
		// return truncated value
		return (byte)(Decimal.Truncate (value));
	}

	public static double ToDouble (decimal value)
	{
		return Convert.ToDouble (value);
	}

	public static short ToInt16 (decimal value)
	{
		if (value > Int16.MaxValue || value < Int16.MinValue)
			throw new OverflowException (Locale.GetText (
				"Value is greater than Int16.MaxValue or less than Int16.MinValue"));
	  
		// return truncated value
		return (Int16)(Decimal.Truncate (value));
	}

	public static int ToInt32 (decimal value)
	{
		if (value > Int32.MaxValue || value < Int32.MinValue)
			throw new OverflowException (Locale.GetText (
				"Value is greater than Int32.MaxValue or less than Int32.MinValue"));
	  
		// return truncated value
		return (Int32)(Decimal.Truncate (value));
	}
	
	public static long ToInt64 (decimal value)
	{
		if (value > Int64.MaxValue || value < Int64.MinValue)
			throw new OverflowException (Locale.GetText (
				"Value is greater than Int64.MaxValue or less than Int64.MinValue"));
	  
		// return truncated value
		return (Int64)(Decimal.Truncate (value));
	}

	public static long ToOACurrency (decimal value)
	{
		return (long) (value * 10000);
	}

	[CLSCompliant(false)]
	public static sbyte ToSByte (decimal value)
	{
		if (value > SByte.MaxValue || value < SByte.MinValue)
			throw new OverflowException (Locale.GetText (
				"Value is greater than SByte.MaxValue or less than SByte.MinValue"));
	  
		// return truncated value
		return (SByte)(Decimal.Truncate (value));
	}
	
	public static float ToSingle (decimal value)
	{
		return Convert.ToSingle (value);
	}

	[CLSCompliant(false)]
	public static ushort ToUInt16 (decimal value)
	{
		if (value > UInt16.MaxValue || value < UInt16.MinValue)
			throw new OverflowException (Locale.GetText (
				"Value is greater than UInt16.MaxValue or less than UInt16.MinValue"));
	  
		// return truncated value
		return (UInt16)(Decimal.Truncate (value));
	}

	[CLSCompliant(false)]
	public static uint ToUInt32 (decimal value)
	{
		if (value > UInt32.MaxValue || value < UInt32.MinValue)
			throw new OverflowException (Locale.GetText (
				"Value is greater than UInt32.MaxValue or less than UInt32.MinValue"));
	  
		// return truncated value
		return (UInt32)(Decimal.Truncate (value));
	}

	[CLSCompliant(false)]
	public static ulong ToUInt64 (decimal value)
	{
		if (value > UInt64.MaxValue || value < UInt64.MinValue)
			throw new OverflowException (Locale.GetText (
				"Value is greater than UInt64.MaxValue or less than UInt64.MinValue"));
	  
		// return truncated value
		return (UInt64)(Decimal.Truncate (value));
	}
		
	object IConvertible.ToType (Type conversionType, IFormatProvider provider)
	{
	    return Convert.ToType (this, conversionType, provider);
	}

	bool IConvertible.ToBoolean (IFormatProvider provider)
	{
	    return Convert.ToBoolean (this);
	}

	byte IConvertible.ToByte (IFormatProvider provider)
	{
	    return Convert.ToByte (this);
	}

	char IConvertible.ToChar (IFormatProvider provider)
	{
	    throw new InvalidCastException ();
	}

	DateTime IConvertible.ToDateTime (IFormatProvider provider)
	{
	    throw new InvalidCastException ();
	}

	decimal IConvertible.ToDecimal (IFormatProvider provider)
	{
	    return this;
	}

	double IConvertible.ToDouble (IFormatProvider provider)
	{
	    return Convert.ToDouble (this);
	}

	short IConvertible.ToInt16 (IFormatProvider provider)
	{
	    return Convert.ToInt16 (this);
	}

	int IConvertible.ToInt32 (IFormatProvider provider)
	{
	    return Convert.ToInt32 (this);
	}

	long IConvertible.ToInt64 (IFormatProvider provider)
	{
	    return Convert.ToInt64 (this);
	}

	sbyte IConvertible.ToSByte (IFormatProvider provider)
	{
	    return Convert.ToSByte (this);
	}

	float IConvertible.ToSingle (IFormatProvider provider)
	{
	    return Convert.ToSingle (this);
	}

	ushort IConvertible.ToUInt16 (IFormatProvider provider)
	{
	    return Convert.ToUInt16 (this);
	}

	uint IConvertible.ToUInt32 (IFormatProvider provider)
	{
	    return Convert.ToUInt32 (this);
	}

	ulong IConvertible.ToUInt64 (IFormatProvider provider)
	{
	    return Convert.ToUInt64 (this);
	}

        public string ToString (string format, IFormatProvider provider) 
        {
		NumberFormatInfo nfi = NumberFormatInfo.GetInstance (provider);
            
		// use "G" for null or empty string
		if ((format == null) || (format.Length == 0))
			format = "G";	
			
		return DecimalFormatter.NumberToString (format, nfi, this);
        }

        public override string ToString() 
        {
            return ToString("G", null);
        }

        public string ToString(string format) 
        {
            return ToString(format, null);
        }

        public string ToString(IFormatProvider provider) 
        {
            return ToString("G", provider);
        }

#if !MSTEST
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int decimal2UInt64(ref Decimal val, 
            out ulong result);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int decimal2Int64(ref Decimal val, 
            out long result);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int double2decimal(out Decimal erg, 
            double val, int digits);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int decimalIncr(ref Decimal d1, ref Decimal d2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int decimal2string(ref Decimal val, 
            int digits, int decimals, char[] bufDigits, int bufSize, out int decPos, out int sign);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int string2decimal(out Decimal val, String sDigits, uint decPos, int sign);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int decimalSetExponent(ref Decimal val, int exp);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern double decimal2double(ref Decimal val);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void decimalFloorAndTrunc(ref Decimal val, 
            int floorFlag);
        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void decimalRound(ref Decimal val, int decimals);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int decimalMult(ref Decimal pd1, ref Decimal pd2);
        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int decimalDiv(out Decimal pc, ref Decimal pa, ref Decimal pb);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int decimalIntDiv(out Decimal pc, ref Decimal pa, ref Decimal pb);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int decimalCompare(ref Decimal d1, ref Decimal d2);
#else
        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimal2UInt64")]
        private static extern int decimal2UInt64(ref Decimal val, 
            out ulong result);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimal2Int64")]
        private static extern int decimal2Int64(ref Decimal val, 
            out long result);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="double2decimal")]
        private static extern int double2decimal(out Decimal erg, 
            double val, int digits);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimalIncr")]
        private static extern int decimalIncr(ref Decimal d1, ref Decimal d2);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimal2string")]
        internal static extern int decimal2string(ref Decimal val, 
            int digits, int decimals,
            [MarshalAs(UnmanagedType.LPWStr)]StringBuilder bufDigits, 
            int bufSize, out int decPos, out int sign);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="string2decimal")]
        internal static extern int string2decimal(out Decimal val,
            [MarshalAs(UnmanagedType.LPWStr)]String sDigits,
            uint decPos, int sign);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimalSetExponent")]
        internal static extern int decimalSetExponent(ref Decimal val, int exp);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimal2double")]
        private static extern double decimal2double(ref Decimal val);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimalFloorAndTrunc")]
        private static extern void decimalFloorAndTrunc(ref Decimal val, 
            int floorFlag);
        
        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimalRound")]
        private static extern void decimalRound(ref Decimal val, int decimals);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimalMult")]
        private static extern int decimalMult(ref Decimal pd1, ref Decimal pd2);
        
        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimalDiv")]
        private static extern int decimalDiv(out Decimal pc, ref Decimal pa, ref Decimal pb);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimalIntDiv")]
        private static extern int decimalIntDiv(out Decimal pc, ref Decimal pa, ref Decimal pb);

        //![MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("libdec", EntryPoint="decimalCompare")]
        private static extern int decimalCompare(ref Decimal d1, ref Decimal d2);

#endif
    }
}

