//
// System.UInt16.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System
{
	[Serializable]
	[CLSCompliant (false)]
	public struct UInt16 : IComparable, IFormattable, IConvertible
	{
		public const ushort MaxValue = 0xffff;
		public const ushort MinValue = 0;

		internal ushort value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if(!(value is System.UInt16))
				throw new ArgumentException (Locale.GetText ("Value is not a System.UInt16."));

			return this.value - ((ushort) value);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.UInt16))
				return false;

			return ((ushort) obj) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ushort Parse (string s)
		{
			ushort val = 0;
			int len;
			int i;
			bool digits_seen = false;
			bool has_negative_sign = false;

			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));

			len = s.Length;

			char c;
			for (i = 0; i < len; i++) {
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}

			if (i == len)
				throw new FormatException ();

			if (s [i] == '+')
				i++;
			else
				if (s[i] == '-') {
					i++;
					has_negative_sign = true;
				}

			for (; i < len; i++) {
				c = s [i];

				if (c >= '0' && c <= '9') {
					ushort d = (ushort) (c - '0');

					val = checked ((ushort) (val * 10 + d));
					digits_seen = true;
				}
				else {
					if (Char.IsWhiteSpace (c)) {
						for (i++; i < len; i++) {
							if (!Char.IsWhiteSpace (s [i]))
								throw new FormatException ();
						}
						break;
					}
					else
						throw new FormatException ();
				}
			}
			if (!digits_seen)
				throw new FormatException ();

			if (has_negative_sign)
				throw new OverflowException ();

			return val;
		}

		[CLSCompliant (false)]
		public static ushort Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		[CLSCompliant (false)]
		public static ushort Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		[CLSCompliant (false)]
		public static ushort Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			uint tmpResult = UInt32.Parse (s, style, provider);
			if (tmpResult > UInt16.MaxValue || tmpResult < UInt16.MinValue)
				throw new OverflowException (Locale.GetText ("Value too large or too small."));

			return (ushort) tmpResult;
		}

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance (provider);

			if (format == null)
				format = "G";
			
			return IntegerFormatter.NumberToString (format, nfi, value);
		}

		// =========== IConvertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt16;
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (value);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte (value);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar (value);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return System.Convert.ToDateTime (value);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (value);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (value);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (value);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (value);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (value);
		}

		[CLSCompliant (false)]
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (value);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (value);
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (value, conversionType, provider);
		}

		[CLSCompliant (false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (value);
		}

		[CLSCompliant (false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (value);
		}

		[CLSCompliant (false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (value);
		}
	}
}
