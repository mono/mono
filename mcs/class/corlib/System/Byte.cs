//
// System.Byte.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	[Serializable]
	public struct Byte : IComparable, IFormattable, IConvertible {
		
		public const byte MinValue = 0;
		public const byte MaxValue = 255;
		
		internal byte value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;

			if (!(v is System.Byte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Byte"));

			byte xv = (byte) v;

			if (value == xv)
				return 0;
			if (value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Byte))
				return false;

			return ((byte) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public static byte Parse (string s)
		{
			byte val = 0;
			int len;
			int i;
			bool digits_seen = false;
			
			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));

			len = s.Length;

			// look for the first non-whitespace character
			char c;
			for (i = 0; i < len; i++){
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}
			
			// if it's all whitespace, then throw exception
			if (i == len)
				throw new FormatException ();

			// look for the optional '+' sign
			if (s [i] == '+')
				i++;

			// we should just have numerals followed by whitespace now
			for (; i < len; i++){
				c = s [i];

				if (c >= '0' && c <= '9'){
					// shift left and accumulate every time we find a numeral
					byte d = (byte) (c - '0');
					
					val = checked ((byte) (val * 10 + d));
					digits_seen = true;
				} else {
					// after the last numeral, only whitespace is allowed
					if (Char.IsWhiteSpace (c)){
						for (i++; i < len; i++){
							if (!Char.IsWhiteSpace (s [i]))
								throw new FormatException ();
						}
						break;
					} else
						throw new FormatException ();
				}
			}

			// if all we had was a '+' sign, then throw exception
			if (!digits_seen)
				throw new FormatException ();
			
			return val;
		}

		public static byte Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static byte Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static byte Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			uint tmpResult = UInt32.Parse (s, style, fp);
			if (tmpResult > Byte.MaxValue || tmpResult < Byte.MinValue)
				throw new OverflowException ("Value too large or too small.");

			return (byte) tmpResult;
		}

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		public string ToString (string format, IFormatProvider fp)
		{
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance( fp );
			
			if ( format == null )
				format = "G";
			
			return IntegerFormatter.NumberToString(format, nfi, value);
		}

		// =========== IConvertible Methods =========== //
		
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType(value, conversionType, provider);
		}
		
		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean(value);
		}
		
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return value;
		}
		
		char IConvertible.ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar(value);
		}
		
		[CLSCompliant(false)]
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal(value);
		}
		
		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble(value);
		}
		
		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16(value);
		}
		
		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32(value);
		}
		
		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64(value);
		}
		
		[CLSCompliant(false)] 
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte(value);
		}
		
		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle(value);
		}
		
/*
		string IConvertible.ToString (IFormatProvider provider)
		{
			return ToString("G", provider);
		}
*/

		[CLSCompliant(false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16(value);
		}
		
		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32(value);
		}
		
		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64(value);
		}
	}
}
