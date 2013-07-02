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
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;

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
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct Decimal: IFormattable, IConvertible, IComparable, IComparable<Decimal>, IEquatable <Decimal>
#if NET_4_0
		, IDeserializationCallback
#endif
	{
		public const decimal MinValue = -79228162514264337593543950335m;
		public const decimal MaxValue =  79228162514264337593543950335m;

		public const decimal MinusOne = -1;
		public const decimal One = 1;
		public const decimal Zero = 0;

		private static readonly Decimal MaxValueDiv10 = MaxValue / 10;

		// some constants
		private const uint MAX_SCALE = 28;
		private const uint SIGN_FLAG = 0x80000000;
		private const int SCALE_SHIFT = 16;
		private const uint RESERVED_SS32_BITS = 0x7F00FFFF;

		// internal representation of decimal
		private uint flags;
		private uint hi;
		private uint lo;
		private uint mid;

		public Decimal (int lo, int mid, int hi, bool isNegative, byte scale)
		{
			unchecked 
			{
				this.lo = (uint) lo;
				this.mid = (uint) mid;
				this.hi = (uint) hi;

				if (scale > MAX_SCALE) 
					throw new ArgumentOutOfRangeException (Locale.GetText ("scale must be between 0 and 28"));

				flags = scale;
				flags <<= SCALE_SHIFT;
				if (isNegative) flags |= SIGN_FLAG;
			}
		}

		public Decimal (int value) 
		{
			unchecked 
			{
				hi = mid = 0;
				if (value < 0) 
				{
					flags = SIGN_FLAG;
					lo = ((uint)~value) + 1;
				}
				else 
				{
					flags = 0;
					lo = (uint) value;
				}
			}
		}

		[CLSCompliant(false)]
		public Decimal (uint value) 
		{
			lo = value;
			flags = hi = mid = 0;
		}

		public Decimal (long value) 
		{
			unchecked 
			{
				hi = 0;
				if (value < 0) 
				{
					flags = SIGN_FLAG;
					ulong u = ((ulong)~value) + 1;
					lo = (uint)u;
					mid = (uint)(u >> 32);
				}
				else 
				{
					flags = 0;
					ulong u = (ulong)value;
					lo = (uint)u;
					mid = (uint)(u >> 32);
				}
			}
		}

		[CLSCompliant(false)]
		public Decimal (ulong value) 
		{
			unchecked 
			{
				flags = hi = 0;
				lo = (uint)value;
				mid = (uint)(value >> 32);
			}
		}

		public Decimal (float value) 
		{
#if false
			//
			// We cant use the double2decimal method
			// because it incorrectly turns the floating point
			// value 1.23456789E-25F which should be:
			//    0.0000000000000000000000001235
			// into the incorrect:
			//   0.0000000000000000000000001234
			//
			//    The code currently parses the double value 0.6 as
			//    0.600000000000000
			//
			// And we have a patch for that called (trim
			if (double2decimal (out this, value, 7) != 0)
				throw new OverflowException ();
#else
			if (value > (float)Decimal.MaxValue || value < (float)Decimal.MinValue ||
				float.IsNaN (value) || float.IsNegativeInfinity (value) || float.IsPositiveInfinity (value)) {
				throw new OverflowException (Locale.GetText (
					"Value {0} is greater than Decimal.MaxValue or less than Decimal.MinValue", value));
			}
			
			// we must respect the precision (double2decimal doesn't)
			Decimal d = Decimal.Parse (value.ToString (CultureInfo.InvariantCulture),
					NumberStyles.Float, CultureInfo.InvariantCulture);
			flags = d.flags;
			hi = d.hi;
			lo = d.lo;
			mid = d.mid;
#endif
		}

		public Decimal (double value) 
		{
#if true
			//
			// We cant use the double2decimal method
			// because it incorrectly turns the floating point
			// value 1.23456789E-25F which should be:
			//    0.0000000000000000000000001235
			// into the incorrect:
			//   0.0000000000000000000000001234
			//
			//    The code currently parses the double value 0.6 as
			//    0.600000000000000
			//
			// And we have a patch for that called (trim
			if (double2decimal (out this, value, 15) != 0)
				throw new OverflowException ();
#else
			if (value > (double)Decimal.MaxValue || value < (double)Decimal.MinValue ||
				double.IsNaN (value) || double.IsNegativeInfinity (value) || double.IsPositiveInfinity (value)) {
				throw new OverflowException (Locale.GetText (
					"Value {0} is greater than Decimal.MaxValue or less than Decimal.MinValue", value));
			}
			// we must respect the precision (double2decimal doesn't)
			Decimal d = Decimal.Parse (value.ToString (CultureInfo.InvariantCulture),
					NumberStyles.Float, CultureInfo.InvariantCulture);
			flags = d.flags;
			hi = d.hi;
			lo = d.lo;
			mid = d.mid;
#endif
		}

		public Decimal (int[] bits) 
		{
			if (bits == null) 
			{
				throw new ArgumentNullException (Locale.GetText ("bits is a null reference"));
			}

			if (bits.Length != 4) 
			{
				throw new ArgumentException (Locale.GetText ("bits does not contain four values"));
			}

			unchecked {
				lo = (uint) bits[0];
				mid = (uint) bits[1];
				hi = (uint) bits[2];
				flags = (uint) bits[3];
				byte scale = (byte)(flags >> SCALE_SHIFT);
				if (scale > MAX_SCALE || (flags & RESERVED_SS32_BITS) != 0) 
				{
					throw new ArgumentException (Locale.GetText ("Invalid bits[3]"));
				}
			}
		}

		public static decimal FromOACurrency (long cy)
		{
			return (decimal)cy / (decimal)10000;
		}

		public static int[] GetBits (Decimal d) 
		{
			unchecked 
			{
				return new int[] { (int)d.lo, (int)d.mid, (int)d.hi, (int)d.flags };
			}
		}

		public static Decimal Negate (Decimal d) 
		{
			d.flags ^= SIGN_FLAG;
			return d;
		}

		public static Decimal Add (Decimal d1, Decimal d2) 
		{
			if (decimalIncr (ref d1, ref d2) == 0)
				return d1;
			else
				throw new OverflowException (Locale.GetText ("Overflow on adding decimal number"));
		}

		public static Decimal Subtract (Decimal d1, Decimal d2) 
		{
			d2.flags ^= SIGN_FLAG;
			int result = decimalIncr (ref d1, ref d2);
			if (result == 0)
				return d1;
			else
				throw new OverflowException (Locale.GetText ("Overflow on subtracting decimal numbers ("+result+")"));
		}

		public override int GetHashCode () 
		{
			return (int) (flags ^ hi ^ lo ^ mid);
		}

		public static Decimal operator + (Decimal d1, Decimal d2)
		{
			return Add (d1, d2);
		}

		public static Decimal operator -- (Decimal d) 
		{
			return Add(d, MinusOne);
		}

		public static Decimal operator ++ (Decimal d) 
		{
			return Add (d, One);
		}

		public static Decimal operator - (Decimal d1, Decimal d2) 
		{
			return Subtract (d1, d2);
		}

		public static Decimal operator - (Decimal d) 
		{
			return Negate (d);
		}

		public static Decimal operator + (Decimal d) 
		{
			return d;
		}

		public static Decimal operator * (Decimal d1, Decimal d2)
		{
			return Multiply (d1, d2);
		}
		
		public static Decimal operator / (Decimal d1, Decimal d2) 
		{
			return Divide (d1, d2);
		}
		
		public static Decimal operator % (Decimal d1, Decimal d2) 
		{
			return Remainder (d1, d2);
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

		public static explicit operator byte (Decimal value)
		{
			ulong result = u64 (value);
			return checked ((byte) result);
		}

		[CLSCompliant (false)]
		public static explicit operator sbyte (Decimal value)
		{
			long result = s64 (value);
			return checked ((sbyte) result);
		}

		public static explicit operator char (Decimal value) 
		{
			ulong result = u64 (value);
			return checked ((char) result);
		}

		public static explicit operator short (Decimal value) 
		{
			long result = s64 (value);
			return checked ((short) result);
		}

		[CLSCompliant (false)]
		public static explicit operator ushort (Decimal value) 
		{
			ulong result = u64 (value);
			return checked ((ushort) result);
		}

		public static explicit operator int (Decimal value) 
		{
			long result = s64 (value);
			return checked ((int) result);
		}

		[CLSCompliant(false)]
		public static explicit operator uint (Decimal value) 
		{
			ulong result = u64 (value);
			return checked ((uint) result);
		}

		public static explicit operator long (Decimal value) 
		{
			return s64 (value);
		}

		[CLSCompliant(false)]
		public static explicit operator ulong (Decimal value) 
		{
			return u64 (value);
		}

		public static implicit operator Decimal (byte value) 
		{
			return new Decimal (value);
		}

		[CLSCompliant(false)]
		public static implicit operator Decimal (sbyte value) 
		{
			return new Decimal (value);
		}

		public static implicit operator Decimal (short value) 
		{
			return new Decimal (value);
		}

		[CLSCompliant(false)]
		public static implicit operator Decimal (ushort value) 
		{
			return new Decimal (value);
		}

		public static implicit operator Decimal (char value) 
		{
			return new Decimal (value);
		}

		public static implicit operator Decimal (int value) 
		{
			return new Decimal (value);
		}

		[CLSCompliant(false)]
		public static implicit operator Decimal (uint value) 
		{
			return new Decimal (value);
		}

		public static implicit operator Decimal (long value) 
		{
			return new Decimal (value);
		}

		[CLSCompliant(false)]
		public static implicit operator Decimal (ulong value) 
		{
			return new Decimal (value);
		}

		public static explicit operator Decimal (float value) 
		{
			return new Decimal (value);
		}

		public static explicit operator Decimal (double value)
		{
			return new Decimal (value);
		}

		public static explicit operator float (Decimal value)
		{
			return (float) (double) value;
		}

		public static explicit operator double (Decimal value)
		{
			return decimal2double (ref value);
		}


		public static bool operator != (Decimal d1, Decimal d2) 
		{
			return !Equals (d1, d2);
		}

		public static bool operator == (Decimal d1, Decimal d2) 
		{
			return Equals (d1, d2);
		}

		public static bool operator > (Decimal d1, Decimal d2) 
		{
			return Compare (d1, d2) > 0;
		}

		public static bool operator >= (Decimal d1, Decimal d2) 
		{
			return Compare (d1, d2) >= 0;
		}

		public static bool operator < (Decimal d1, Decimal d2) 
		{
			return Compare (d1, d2) < 0;
		}

		public static bool operator <= (Decimal d1, Decimal d2) 
		{
			return Compare (d1, d2) <= 0;
		}

		public static bool Equals (Decimal d1, Decimal d2) 
		{
			return Compare (d1, d2) == 0;
		}

		public override bool Equals (object value) 
		{
			if (!(value is Decimal))
				return false;

			return Equals ((Decimal) value, this);
		}

		// avoid unmanaged call
		private bool IsZero () 
		{
			return ((hi == 0) && (lo == 0) && (mid == 0));
		}

		// avoid unmanaged call
		private bool IsNegative () 
		{
			return ((flags & 0x80000000) == 0x80000000);
		}

		public static Decimal Floor (Decimal d) 
		{
			decimalFloorAndTrunc (ref d, 1);
			return d;
		}

		public static Decimal Truncate (Decimal d) 
		{
			decimalFloorAndTrunc (ref d, 0);
			return d;
		}

		public static Decimal Round (Decimal d, int decimals) 
		{
			return Round (d, decimals, MidpointRounding.ToEven);
		}

		public static Decimal Round (Decimal d, int decimals, MidpointRounding mode) 
		{
			if ((mode != MidpointRounding.ToEven) && (mode != MidpointRounding.AwayFromZero))
				throw new ArgumentException ("The value '" + mode + "' is not valid for this usage of the type MidpointRounding.", "mode");

			if (decimals < 0 || decimals > 28) {
				throw new ArgumentOutOfRangeException ("decimals", "[0,28]");
			}

			bool negative = d.IsNegative ();
			if (negative)
				d.flags ^= SIGN_FLAG;

			// Moved from Math.cs because it's easier to fix the "sign"
			// issue here :( as the logic is OK only for positive numbers
			decimal p = (decimal) Math.Pow (10, decimals);
			decimal int_part = Decimal.Floor (d);
			decimal dec_part = d - int_part;
			dec_part *= 10000000000000000000000000000M;
			dec_part = Decimal.Floor(dec_part);
			dec_part /= (10000000000000000000000000000M / p);
			dec_part = Math.Round (dec_part, mode);
			dec_part /= p;
			decimal result = int_part + dec_part;

			// that fixes the precision/scale (which we must keep for output)
			// (moved and adapted from System.Data.SqlTypes.SqlMoney)
			long scaleDiff = decimals - ((result.flags & 0x7FFF0000) >> 16);
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
			result.flags = (uint)((decimals - scaleDiff) << SCALE_SHIFT);

			if (negative)
				result.flags ^= SIGN_FLAG;
			return result;
		}

		public static Decimal Round (Decimal d)
		{
			return Math.Round (d);
		}

		public static Decimal Round (Decimal d, MidpointRounding mode)
		{
			return Math.Round (d, mode);
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
			if (d1.IsZero ())
				return Decimal.Zero;

			d1.flags ^= SIGN_FLAG;
			d1.flags ^= SIGN_FLAG;

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
				d1.flags ^= SIGN_FLAG;
			if (d2.IsNegative ())
				d2.flags ^= SIGN_FLAG;

			Decimal result;
			if (d1 == d2) {
				return Decimal.Zero;
			}
			else if (d2 > d1) {
				result = d1;
			}
			else {
				if (decimalDiv (out result, ref d1, ref d2) != 0)
					throw new OverflowException ();
				result = Decimal.Truncate (result);

				// FIXME: not really performant here
				result = d1 - result * d2;
			}

			if (negative)
				result.flags ^= SIGN_FLAG;
			return result;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static int Compare (Decimal d1, Decimal d2) 
		{
			return decimalCompare (ref d1, ref d2);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is Decimal))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Decimal"));

			return Compare (this, (Decimal)value);
		}

		public int CompareTo (Decimal value)
		{
			return Compare (this, value);
		}

		public bool Equals (Decimal value) 
		{
			return Equals (value, this);
		}

		public static Decimal Ceiling (Decimal d)
		{
			return Math.Ceiling (d);
		}

		public static Decimal Parse (string s) 
		{
			return Parse (s, NumberStyles.Number, null);
		}

		public static Decimal Parse (string s, NumberStyles style) 
		{
			return Parse (s, style, null);
		}

		public static Decimal Parse (string s, IFormatProvider provider) 
		{
			return Parse (s, NumberStyles.Number, provider);
		}

		static void ThrowAtPos (int pos)
		{
			throw new FormatException (String.Format (Locale.GetText ("Invalid character at position {0}"), pos));
		}

		static void ThrowInvalidExp ()
		{
			throw new FormatException (Locale.GetText ("Invalid exponent"));
		}

		private static string stripStyles (string s, NumberStyles style, NumberFormatInfo nfi, 
		  out int decPos, out bool isNegative, out bool expFlag, out int exp, bool throwex)
		{
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
				int index = s.IndexOfOrdinalUnchecked (nfi.CurrencySymbol);
				if (index >= 0) 
				{
					s = s.Remove (index, nfi.CurrencySymbol.Length);
					hasCurrency = true;
				}
			}

			string decimalSep = (hasCurrency) ? nfi.CurrencyDecimalSeparator : nfi.NumberDecimalSeparator;
			string groupSep = (hasCurrency) ? nfi.CurrencyGroupSeparator : nfi.NumberGroupSeparator;
			string negativeSign = nfi.NegativeSign;
			string positiveSign = nfi.PositiveSign;

			// If we don't have a group separator defined, it has the same effect as if it wasn't allowed.
			if (string.IsNullOrEmpty(groupSep)) allowedThousands = false;

			int pos = 0;
			int len = s.Length;

			StringBuilder sb = new StringBuilder (len);

			// leading
			while (pos < len) 
			{
				char ch = s[pos];
				if (Char.IsDigit (ch))
				{
					break; // end of leading
				}
				else if (allowedLeadingWhiteSpace && Char.IsWhiteSpace (ch))
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
				else if (allowedLeadingSign && !string.IsNullOrEmpty (negativeSign) && ch == negativeSign[0] && !hasSign)
				{
					int slen = negativeSign.Length;
					if (slen == 1 || s.IndexOfOrdinalUnchecked (negativeSign, pos, slen) == pos)
					{
						hasSign = true;
						isNegative = true;
						pos += slen;
					}
				}
				else if (allowedLeadingSign && !string.IsNullOrEmpty (positiveSign) && ch == positiveSign[0] && !hasSign)
				{
					int slen = positiveSign.Length;
					if (slen == 1 || s.IndexOfOrdinalUnchecked (positiveSign, pos, slen) == pos)
					{
						hasSign = true;
						pos += slen;
					}
				}
				else if (allowedDecimalPoint && ch == decimalSep[0])
				{
					int slen = decimalSep.Length;
					if (slen != 1 && s.IndexOfOrdinalUnchecked (decimalSep, pos, slen) != pos) 
					{
						if (throwex)
							ThrowAtPos (pos);
						else
							return null;
					}
					break;
				}
				else
				{
					if (throwex)
						ThrowAtPos (pos);
					else
						return null;
				}
			}

			if (pos == len) {
				if (throwex)
					throw new FormatException (Locale.GetText ("No digits found"));
				else
					return null;
			}

			// digits 
			while (pos < len)
			{
				char ch = s[pos];
				if (Char.IsDigit (ch)) 
				{
					sb.Append(ch);
					pos++;
				}
				else if (allowedThousands && ch == groupSep[0] && ch != decimalSep [0]) 
				{
					int slen = groupSep.Length;
					if (slen != 1 && s.IndexOfOrdinalUnchecked(groupSep, pos, slen) != pos) 
					{
						if (throwex)
							ThrowAtPos (pos);
						else
							return null;
					}
					pos += slen;
				}
				else if (allowedDecimalPoint && ch == decimalSep[0] && !hasDecimalPoint)
				{
					int slen = decimalSep.Length;
					if (slen == 1 || s.IndexOfOrdinalUnchecked(decimalSep, pos, slen) == pos) 
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
					pos++;
					if (pos >= len){
						if (throwex)
							ThrowInvalidExp ();
						else
							return null;
					}
					ch = s[pos];
					bool isNegativeExp = false;
					if (!string.IsNullOrEmpty (positiveSign) && ch == positiveSign[0])
					{
						int slen = positiveSign.Length;
						if (slen == 1 || s.IndexOfOrdinalUnchecked (positiveSign, pos, slen) == pos)
						{
							pos += slen;
							if (pos >= len) {
								if (throwex)
									ThrowInvalidExp ();
								else
									return null;
							}
						}
					}
					else if (!string.IsNullOrEmpty (negativeSign) && ch == negativeSign[0])
					{
						int slen = negativeSign.Length;
						if (slen == 1 || s.IndexOfOrdinalUnchecked (negativeSign, pos, slen) == pos)
						{
							pos += slen;
							if (pos >= len) {
								if (throwex)
									ThrowInvalidExp ();
								else
									return null;
							}
							isNegativeExp = true;
						}
					}
					ch = s[pos];
					if (!Char.IsDigit(ch)) {
						if (throwex)
							ThrowInvalidExp ();
						else
							return null;
					}

					exp = ch - '0';
					pos++;
					while (pos < len && Char.IsDigit (s[pos])) 
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
				if (allowedTrailingWhiteSpace && Char.IsWhiteSpace (ch)) 
				{
					pos++;
				}
				else if (allowedParentheses && ch == ')' && hasOpeningParentheses) 
				{
					hasOpeningParentheses = false;
					pos++;
				}
				else if (allowedTrailingSign && !string.IsNullOrWhiteSpace (negativeSign) && ch == negativeSign[0] && !hasSign)
				{
					int slen = negativeSign.Length;
					if (slen == 1 || s.IndexOfOrdinalUnchecked(negativeSign, pos, slen) == pos)
					{
						hasSign = true;
						isNegative = true;
						pos += slen;
					}
				}
				else if (allowedTrailingSign && !string.IsNullOrWhiteSpace (positiveSign) && ch == positiveSign[0] && !hasSign)
				{
					int slen = positiveSign.Length;
					if (slen == 1 || s.IndexOfOrdinalUnchecked (positiveSign, pos, slen) == pos)
					{
						hasSign = true;
						pos += slen;
					}
				}
				else
				{
					// trailing zero characters are allowed
					if (ch == 0){
						while (++pos < len && s [pos] == 0)
							;
						if (pos == len)
							break;
					}
					
					if (throwex)
						ThrowAtPos (pos);
					else
						return null;
				}
			}

			if (hasOpeningParentheses) {
				if (throwex)
					throw new FormatException (Locale.GetText ("Closing Parentheses not found"));
				else
					return null;
			}

			if (!hasDecimalPoint)
				decPos = sb.Length;

			return sb.ToString ();
		}

		public static Decimal Parse (string s, NumberStyles style, IFormatProvider provider) 
		{
			if (s == null)
				throw new ArgumentNullException ("s");

			if ((style & NumberStyles.AllowHexSpecifier) != 0)
				throw new ArgumentException ("Decimal.TryParse does not accept AllowHexSpecifier", "style");

			Decimal result;
			PerformParse (s, style, provider, out result, true);
			return result;
		}
	
		public static bool TryParse (string s, out Decimal result)
		{
			if (s == null){
				result = 0;
				return false;
			}
			return PerformParse (s, NumberStyles.Number, null, out result, false);
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out decimal result)
		{
			if (s == null || (style & NumberStyles.AllowHexSpecifier) != 0){
				result = 0;
				return false;
			}

			return PerformParse (s, style, provider, out result, false);
		}

		static bool PerformParse (string s, NumberStyles style, IFormatProvider provider, out Decimal res, bool throwex) 
		{
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance (provider);

			int iDecPos, exp;
			bool isNegative, expFlag;
			s = stripStyles(s, style, nfi, out iDecPos, out isNegative, out expFlag, out exp, throwex);
			if (s == null){
				res = 0;
				return false;
			}

			if (iDecPos < 0){
				if (throwex)
					throw new Exception (Locale.GetText ("Error in System.Decimal.Parse"));
				res = 0;
				return false;
			}

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
				if (String.CompareOrdinal (s, 0, "79228162514264337593543950335", 0, max + 1) <= 0) {
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
			if (string2decimal (out result, s, (uint)iDecPos, 0) != 0){
				if (throwex)
					throw new OverflowException ();
				res = 0;
				return false;
			}

			if (expFlag) {
				if (decimalSetExponent (ref result, exp) != 0){
					if (throwex)
						throw new OverflowException ();
					res = 0;
					return false;
				}
			}

			if (isNegative)
				result.flags ^= SIGN_FLAG;

			res = result;
			return true;
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

		public static double ToDouble (decimal d)
		{
			return Convert.ToDouble (d);
		}

		public static short ToInt16 (decimal value)
		{
			if (value > Int16.MaxValue || value < Int16.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue or less than Int16.MinValue"));

			// return truncated value
			return (Int16)(Decimal.Truncate (value));
		}

		public static int ToInt32 (decimal d)
		{
			if (d > Int32.MaxValue || d < Int32.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int32.MaxValue or less than Int32.MinValue"));

			// return truncated value
			return (Int32)(Decimal.Truncate (d));
		}
	
		public static long ToInt64 (decimal d)
		{
			if (d > Int64.MaxValue || d < Int64.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int64.MaxValue or less than Int64.MinValue"));

			// return truncated value
			return (Int64)(Decimal.Truncate (d));
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
	
		public static float ToSingle (decimal d)
		{
			return Convert.ToSingle (d);
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
		public static uint ToUInt32 (decimal d)
		{
			if (d > UInt32.MaxValue || d < UInt32.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt32.MaxValue or less than UInt32.MinValue"));

			// return truncated value
			return (UInt32)(Decimal.Truncate (d));
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64 (decimal d)
		{
			if (d > UInt64.MaxValue || d < UInt64.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt64.MaxValue or less than UInt64.MinValue"));

			// return truncated value
			return (UInt64)(Decimal.Truncate (d));
		}

		object IConvertible.ToType (Type targetType, IFormatProvider provider)
		{
			if (targetType == null)
				throw new ArgumentNullException ("targetType");
			return Convert.ToType (this, targetType, provider, false);
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
			return NumberFormatter.NumberToString (format, this, provider);
		}

		public override string ToString () 
		{
			return ToString ("G", null);
		}

		public string ToString (string format) 
		{
			return ToString (format, null);
		}

		public string ToString (IFormatProvider provider) 
		{
			return ToString ("G", provider);
		}
		
#if NET_4_0
		void IDeserializationCallback.OnDeserialization(object sender)
		{
		}
#endif

#if !MSTEST
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int decimal2UInt64 (ref Decimal val, out ulong result);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int decimal2Int64 (ref Decimal val, out long result);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int double2decimal (out Decimal erg, double val, int digits);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int decimalIncr (ref Decimal d1, ref Decimal d2);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern int string2decimal (out Decimal val, String sDigits, uint decPos, int sign);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern int decimalSetExponent (ref Decimal val, int exp);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern double decimal2double (ref Decimal val);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void decimalFloorAndTrunc (ref Decimal val, int floorFlag);

//		[MethodImplAttribute(MethodImplOptions.InternalCall)]
//		private static extern void decimalRound (ref Decimal val, int decimals);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int decimalMult (ref Decimal pd1, ref Decimal pd2);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int decimalDiv (out Decimal pc, ref Decimal pa, ref Decimal pb);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int decimalIntDiv (out Decimal pc, ref Decimal pa, ref Decimal pb);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int decimalCompare (ref Decimal d1, ref Decimal d2);
#else
		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimal2UInt64")]
		private static extern int decimal2UInt64 (ref Decimal val, out ulong result);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimal2Int64")]
		private static extern int decimal2Int64 (ref Decimal val, out long result);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="double2decimal")]
		private static extern int double2decimal (out Decimal erg, double val, int digits);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimalIncr")]
		private static extern int decimalIncr (ref Decimal d1, ref Decimal d2);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="string2decimal")]
		internal static extern int string2decimal (out Decimal val,
		    [MarshalAs(UnmanagedType.LPWStr)]String sDigits,
		    uint decPos, int sign);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimalSetExponent")]
		internal static extern int decimalSetExponent (ref Decimal val, int exp);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimal2double")]
		private static extern double decimal2double (ref Decimal val);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimalFloorAndTrunc")]
		private static extern void decimalFloorAndTrunc (ref Decimal val, int floorFlag);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimalRound")]
		private static extern void decimalRound (ref Decimal val, int decimals);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimalMult")]
		private static extern int decimalMult (ref Decimal pd1, ref Decimal pd2);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimalDiv")]
		private static extern int decimalDiv (out Decimal pc, ref Decimal pa, ref Decimal pb);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimalIntDiv")]
		private static extern int decimalIntDiv (out Decimal pc, ref Decimal pa, ref Decimal pb);

		//![MethodImplAttribute(MethodImplOptions.InternalCall)]
		[DllImport("libdec", EntryPoint="decimalCompare")]
		private static extern int decimalCompare (ref Decimal d1, ref Decimal d2);

#endif
	}
}
