//
// System.UInt64.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	[CLSCompliant(false)]
	public struct UInt64 : IComparable, IFormattable, IConvertible {
		public const ulong MaxValue = 0xffffffffffffffff;
		public const ulong MinValue = 0;
		
		public ulong value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;

			if (!(v is System.UInt64))
				throw new ArgumentException (Locale.GetText ("Value is not a System.UInt64"));

			if (value == (ulong) v)
				return 0;

			if (value < (ulong) v)
				return -1;

			return 1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.UInt64))
				return false;

			return ((ulong) o) == value;
		}

		public override int GetHashCode ()
		{
			return (int)(value & 0xffffffff) ^ (int)(value >> 32);
		}

		public static ulong Parse (string s)
		{
			return Parse (s, NumberStyles.Integer, null);
		}

		public static ulong Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static ulong Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static ulong Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			ulong val = 0;
			int len;
			int i;
			bool digits_seen = false;
			
			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));

			len = s.Length;

			char c;
			i = 0;
			if ((style & NumberStyles.AllowLeadingWhite) != 0)
				for (i = 0; i < len; i++){
					c = s [i];
					if (!Char.IsWhiteSpace (c))
						break;
				}
			
			if (i == len)
				throw new FormatException ();

			if ((style & NumberStyles.AllowLeadingSign) != 0 && (s [i] == '+'))
				i++;

			for (; i < len; i++){
				c = s [i];

				if ((style & NumberStyles.AllowHexSpecifier) != 0) {
					if (c >= '0' && c <= '9') {
						uint d = (uint) (c - '0');
						val = checked (val * 16 + d);
						digits_seen = true;
					} else if (c >= 'a' && c <= 'f') {
						uint d = (uint) (c - 'a');
						val = checked (val * 16 + 10 + d);
						digits_seen = true;
					} else if (c >= 'A' && c <= 'F') {
						uint d = (uint) (c - 'A');
						val = checked (val * 16 + 10 + d);
						digits_seen = true;
					} else
						break;
				} else if (c >= '0' && c <= '9'){
					uint d = (uint) (c - '0');
					
					val = checked (val * 10 + d);
					digits_seen = true;
				} else {
					break;
				}
			}
			if (!digits_seen)
				throw new FormatException ();
			if (i < len) {
				if ((style & NumberStyles.AllowTrailingWhite) != 0 && Char.IsWhiteSpace (s [i])){
					for (i++; i < len; i++){
						if (!Char.IsWhiteSpace (s [i]))
							throw new FormatException ();
					}
				} else
					throw new FormatException ();
			}
	
			return val;

		}

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString (null, fp);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
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
			return TypeCode.UInt64;
		}
		public bool     ToBoolean  (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (value);
		}
		public byte     ToByte     (IFormatProvider provider)
		{
			return System.Convert.ToByte (value);
		}
		public char     ToChar     (IFormatProvider provider)
		{
			return System.Convert.ToChar (value);
		}
		public DateTime ToDateTime (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}
		public decimal  ToDecimal  (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (value);
		}
		public double   ToDouble   (IFormatProvider provider)
		{
			return System.Convert.ToDouble (value);
		}
		public short    ToInt16    (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (value);
		}
		public int      ToInt32    (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (value);
		}
		public long     ToInt64    (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (value);
		}
    		[CLSCompliant(false)]
		public sbyte    ToSByte    (IFormatProvider provider)
		{
			return System.Convert.ToSByte (value);
		}
		public float    ToSingle   (IFormatProvider provider)
		{
			return System.Convert.ToSingle (value);
		}
		public object   ToType     (Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}
    		[CLSCompliant(false)]
		public ushort   ToUInt16   (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (value);
		}
    		[CLSCompliant(false)]
		public uint     ToUInt32   (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (value);
		}
    		[CLSCompliant(false)]
		public ulong    ToUInt64   (IFormatProvider provider)
		{
			return value;
		}
	}
}
