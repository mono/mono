//
// System.Double.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Bob Smith       (bob@thestuff.net)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) Bob Smith.    http://www.thestuff.net
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

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace System {
	
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct Double : IComparable, IFormattable, IConvertible, IComparable <double>, IEquatable <double>
	{
		public const double Epsilon = 4.9406564584124650e-324;
		public const double MaxValue =  1.7976931348623157e308;
		public const double MinValue = -1.7976931348623157e308;
		public const double NaN = 0.0d / 0.0d;
		public const double NegativeInfinity = -1.0d / 0.0d;
		public const double PositiveInfinity = 1.0d / 0.0d;
		
		internal double m_value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			
			if (!(value is System.Double))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Double"));

			double dv = (double)value;

			if (IsPositiveInfinity(m_value) && IsPositiveInfinity(dv))
				return 0;

			if (IsNegativeInfinity(m_value) && IsNegativeInfinity(dv))
				return 0;

			if (IsNaN(dv))
				if (IsNaN(m_value))
					return 0;
				else
					return 1;

			if (IsNaN(m_value))
				if (IsNaN(dv))
					return 0;
				else
					return -1;

			if (m_value > dv) return 1;
			else if (m_value < dv) return -1;
			else return 0;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.Double))
				return false;

			double value = (double) obj;

			if (IsNaN (value))
				return IsNaN (m_value);

			return (value == m_value);
		}

		public int CompareTo (double value)
		{
			if (IsPositiveInfinity(m_value) && IsPositiveInfinity(value))
				return 0;

			if (IsNegativeInfinity(m_value) && IsNegativeInfinity(value))
				return 0;

			if (IsNaN(value))
				if (IsNaN(m_value))
					return 0;
				else
					return 1;

			if (IsNaN(m_value))
				if (IsNaN(value))
					return 0;
				else
					return -1;

			if (m_value > value) return 1;
			else if (m_value < value) return -1;
			else return 0;
		}

		public bool Equals (double obj)
		{
			if (IsNaN (obj)) {
				if (IsNaN(m_value))
					return true;
				else
					return false;
			}

			return obj == m_value;
		}

		public override unsafe int GetHashCode ()
		{
			double d = m_value;
			return (*((long*)&d)).GetHashCode ();
		}

#if	NET_4_0
		public static bool operator==(double left, double right)
		{
			return left == right;
		}

		public static bool operator!=(double left, double right)
		{
			return left != right;
		}

		public static bool operator>(double left, double right)
		{
			return left > right;
		}

		public static bool operator>=(double left, double right)
		{
			return left >= right;
		}

		public static bool operator<(double left, double right)
		{
			return left < right;
		}

		public static bool operator<=(double left, double right)
		{
			return left <= right;
		}
#endif

		public static bool IsInfinity (double d)
		{
			return (d == PositiveInfinity || d == NegativeInfinity);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static bool IsNaN (double d)
		{
#pragma warning disable 1718
			return (d != d);
#pragma warning restore
		}

		public static bool IsNegativeInfinity (double d)
		{
			return (d < 0.0d && (d == NegativeInfinity || d == PositiveInfinity));
		}

		public static bool IsPositiveInfinity (double d)
		{
			return (d > 0.0d && (d == NegativeInfinity || d == PositiveInfinity));
		}

		public static double Parse (string s)
		{
			return Parse (s, (NumberStyles.Float | NumberStyles.AllowThousands), null);
		}

		public static double Parse (string s, IFormatProvider provider)
		{
			return Parse (s, (NumberStyles.Float | NumberStyles.AllowThousands), provider);
		}

		public static double Parse (string s, NumberStyles style) 
		{
			return Parse (s, style, null);
		}

		// We're intentionally using constants here to avoid some bigger headaches in mcs.
		// This struct must be compiled before System.Enum so we can't use enums here.
		private const int State_AllowSign = 1;
		private const int State_Digits = 2;
		private const int State_Decimal = 3;
		private const int State_ExponentSign = 4;
		private const int State_Exponent = 5;
		private const int State_ConsumeWhiteSpace = 6;
		private const int State_Exit = 7;
		
		public static double Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			Exception exc;
			double result;
			
			if (!Parse (s, style, provider, false, out result, out exc))
				throw exc;

			return result;
		}
		
		// FIXME: check if digits are group in correct numbers between the group separators
		internal static bool Parse (string s, NumberStyles style, IFormatProvider provider, bool tryParse, out double result, out Exception exc)
		{
			result = 0;
			exc = null;
			
			if (s == null) {
				if (!tryParse)
					exc = new ArgumentNullException ("s");
				return false;
			}
			if (s.Length == 0) {
				if (!tryParse)
					exc = new FormatException ();
				return false;
			}
			// yes it's counter intuitive (buggy?) but even TryParse actually throws in this case
			if ((style & NumberStyles.AllowHexSpecifier) != 0) {
				string msg = Locale.GetText ("Double doesn't support parsing with '{0}'.", "AllowHexSpecifier");
				throw new ArgumentException (msg);
			}
			if (style > NumberStyles.Any) {
				if (!tryParse)
					exc = new ArgumentException();
				return false;
			}

			NumberFormatInfo format = NumberFormatInfo.GetInstance(provider);
			if (format == null) throw new Exception("How did this happen?");
			
			//
			// validate and prepare string for C
			//
			int len = s.Length;
			int didx = 0;
			int sidx = 0;
			char c;
			bool allow_leading_white = (style & NumberStyles.AllowLeadingWhite) != 0;
			bool allow_trailing_white = ((style & NumberStyles.AllowTrailingWhite) != 0);
			
			if (allow_leading_white) {
				while (sidx < len && Char.IsWhiteSpace (s [sidx]))
				       sidx++;

				if (sidx == len) {
					if (!tryParse)
						exc = Int32.GetFormatException ();
					return false;
				}
			}
			int sEndPos = s.Length - 1;
			if (allow_trailing_white)
				while (Char.IsWhiteSpace (s [sEndPos]))
					sEndPos--;

			if (TryParseStringConstant (format.NaNSymbol, s, sidx, sEndPos)) {
				result = double.NaN;
				return true;
			}
			if (TryParseStringConstant (format.PositiveInfinitySymbol, s, sidx, sEndPos)) {
				result = double.PositiveInfinity;
				return true;
			}
			if (TryParseStringConstant (format.NegativeInfinitySymbol, s, sidx, sEndPos)) {
				result = double.NegativeInfinity;
				return true;
			}

			byte [] b = new byte [len + 1];

			//
			// Machine state
			//
			int state = State_AllowSign;

			//
			// Setup
			//
			string decimal_separator = null;
			string group_separator = null;
			string currency_symbol = null;
			int decimal_separator_len = 0;
			int group_separator_len = 0;
			int currency_symbol_len = 0;
			if ((style & NumberStyles.AllowDecimalPoint) != 0){
				decimal_separator = format.NumberDecimalSeparator;
				decimal_separator_len = decimal_separator.Length;
			}
			if ((style & NumberStyles.AllowThousands) != 0){
				group_separator = format.NumberGroupSeparator;
				group_separator_len = group_separator.Length;
			}
			if ((style & NumberStyles.AllowCurrencySymbol) != 0){
				currency_symbol = format.CurrencySymbol;
				currency_symbol_len = currency_symbol.Length;
			}
			string positive = format.PositiveSign;
			string negative = format.NegativeSign;
			
			for (; sidx < len; sidx++){
				c = s [sidx];

				if (c == '\0') {
					sidx = len;
					continue;
				}

				switch (state){
				case State_AllowSign:
					if ((style & NumberStyles.AllowLeadingSign) != 0){
						if (c == positive [0] &&
						    s.Substring (sidx, positive.Length) == positive){
							state = State_Digits;
							sidx += positive.Length-1;
							continue;
						}

						if (c == negative [0] &&
						    s.Substring (sidx, negative.Length) == negative){
							state = State_Digits;
							b [didx++] = (byte) '-';
							sidx += negative.Length-1;
							continue;
						}
					}
					state = State_Digits;
					goto case State_Digits;
					
				case State_Digits:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}
					if (c == 'e' || c == 'E')
						goto case State_Decimal;
					
					if (decimal_separator_len > 0 &&
					    decimal_separator [0] == c) {
						if (String.CompareOrdinal (s, sidx, decimal_separator, 0, decimal_separator_len) == 0) {
							b [didx++] = (byte) '.';
							sidx += decimal_separator_len-1;
							state = State_Decimal; 
							break;
						}
					}
					if (group_separator_len > 0 &&
					    group_separator [0] == c){
						if (s.Substring (sidx, group_separator_len) ==
						    group_separator){
							sidx += group_separator_len-1;
							state = State_Digits; 
							break;
						}
					}
					if (currency_symbol_len > 0 &&
					    currency_symbol [0] == c){
						if (s.Substring (sidx, currency_symbol_len) ==
						    currency_symbol){
							sidx += currency_symbol_len-1;
							state = State_Digits; 
							break;
						}
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State_ConsumeWhiteSpace;

					if (!tryParse)
						exc = new FormatException ("Unknown char: " + c);
					return false;

				case State_Decimal:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}

					if (c == 'e' || c == 'E'){
						if ((style & NumberStyles.AllowExponent) == 0) {
							if (!tryParse)
								exc = new FormatException ("Unknown char: " + c);
							return false;
						}
						b [didx++] = (byte) c;
						state = State_ExponentSign;
						break;
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State_ConsumeWhiteSpace;
					
					if (!tryParse)
						exc = new FormatException ("Unknown char: " + c);
					return false;

				case State_ExponentSign:
					if (Char.IsDigit (c)){
						state = State_Exponent;
						goto case State_Exponent;
					}

					if (c == positive [0] &&
					    s.Substring (sidx, positive.Length) == positive){
						state = State_Digits;
						sidx += positive.Length-1;
						continue;
					}

					if (c == negative [0] &&
					    s.Substring (sidx, negative.Length) == negative){
						state = State_Digits;
						b [didx++] = (byte) '-';
						sidx += negative.Length-1;
						continue;
					}

					if (Char.IsWhiteSpace (c))
						goto case State_ConsumeWhiteSpace;
					
					if (!tryParse)
						exc = new FormatException ("Unknown char: " + c);
					return false;
					
				case State_Exponent:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State_ConsumeWhiteSpace;
					
					if (!tryParse)
						exc = new FormatException ("Unknown char: " + c);
					return false;

				case State_ConsumeWhiteSpace:
					if (allow_trailing_white && Char.IsWhiteSpace (c)) {
						state = State_ConsumeWhiteSpace;
						break;
					}
					
					if (!tryParse)
						exc = new FormatException ("Unknown char");
					return false;
				}

				if (state == State_Exit)
					break;
			}

			b [didx] = 0;
			unsafe {
				fixed (byte *p = &b [0]){
					double retVal;
					if (!ParseImpl (p, out retVal)) {
						if (!tryParse)
							exc = Int32.GetFormatException ();
						return false;
					}
					if (IsPositiveInfinity(retVal) || IsNegativeInfinity(retVal)) {
						if (!tryParse)
							exc = new OverflowException ();
						return false;
					}

					result = retVal;
					return true;
				}
			}
		}

		static bool TryParseStringConstant (string format, string s, int start, int end)
		{
			return end - start + 1 == format.Length && String.CompareOrdinal (format, 0, s, start, format.Length) == 0;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe private static extern bool ParseImpl (byte *byte_ptr, out double value);
		
		public static bool TryParse (string s,
					     NumberStyles style,
					     IFormatProvider provider,
					     out double result)
		{
			Exception exc;
			if (!Parse (s, style, provider, true, out result, out exc)) {
				result = 0;
				return false;
			}

			return true;
		}

		public static bool TryParse (string s, out double result)
		{
			return TryParse (s, NumberStyles.Any, null, out result);
		}

		public override string ToString ()
		{
			return NumberFormatter.NumberToString (m_value, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return NumberFormatter.NumberToString (m_value, provider);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			return NumberFormatter.NumberToString (format, m_value, provider);
		}

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Double;
		}

		object IConvertible.ToType (Type targetType, IFormatProvider provider)
		{
			if (targetType == null)
				throw new ArgumentNullException ("targetType");
			return System.Convert.ToType (m_value, targetType, provider, false);
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (m_value);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte (m_value);
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
			return System.Convert.ToDecimal (m_value);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (m_value);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (m_value);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (m_value);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (m_value);
		}

		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (m_value);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (m_value);
		}

		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (m_value);
		}
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (m_value);
		}
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (m_value);
		}
	}
}
