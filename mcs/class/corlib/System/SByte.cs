//
// System.SByte.cs
//
// Author:
// Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System.Globalization;

namespace System
{
	[CLSCompliant(false)]
	[Serializable]
	public struct SByte : IComparable, IFormattable, IConvertible
	{
		public const sbyte MinValue = -128;
		public const sbyte MaxValue = 127;

		internal sbyte value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;

			if (!(v is System.SByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.SByte."));

			sbyte xv = (sbyte) v;
			if (value == xv)
				return 0;
			if (value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.SByte))
				return false;

			return ((sbyte) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s)
		{
			int ival = 0;
			int len;
			int i;
			bool neg = false;
			bool digits_seen = false;

			if (s == null)
				throw new ArgumentNullException ("s");

			len = s.Length;

			char c;
			for (i = 0; i < len; i++) {
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}

			if (i == len)
				throw new FormatException ();

			c = s [i];
			if (c == '+')
				i++;
			else if (c == '-') {
				neg = true;
				i++;
			}

			for (; i < len; i++) {
				c = s [i];

				if (c >= '0' && c <= '9') {
					ival = checked (ival * 10 - (int) (c - '0'));
					digits_seen = true;
				} else {
					if (Char.IsWhiteSpace (c)) {
						for (i++; i < len; i++) {
							if (!Char.IsWhiteSpace (s [i]))
								throw new FormatException ();
						}
						break;
					} else
						throw new FormatException ();
				}
			}
			if (!digits_seen)
				throw new FormatException ();

			ival = neg ? ival : -ival;
			if (ival < SByte.MinValue || ival > SByte.MaxValue)
				throw new OverflowException ();

			return (sbyte) ival;
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			int tmpResult = Int32.Parse (s, style, provider);
			if (tmpResult > SByte.MaxValue || tmpResult < SByte.MinValue)
				throw new OverflowException (Locale.GetText ("Value too large or too small."));

			return (sbyte) tmpResult;
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

		// =========== ICovnertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.SByte;
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
			return value;
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
