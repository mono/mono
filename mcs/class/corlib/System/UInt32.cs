//
// System.UInt32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	[CLSCompliant(false)]
	public struct UInt32 : IComparable, IFormattable { //, IConvertible {
		public static Type Type = typeof (uint);

		public const uint MaxValue = 0xffffffff;
		public const uint MinValue = 0;
		
		public uint value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.UInt32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.UInt32"));

			if (value == (uint) v)
				return 0;

			if (value < (uint) v)
				return -1;

			return 1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.UInt32))
				return false;

			return ((uint) o) == value;
		}

		public override int GetHashCode ()
		{
			return (int) value;
		}

		public static uint Parse (string s)
		{
			uint val = 0;
			int len;
			int i;
			bool digits_seen = false;
			
			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));

			len = s.Length;

			char c;
			for (i = 0; i < len; i++){
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}
			
			if (i == len)
				throw new FormatException ();

			if (s [i] == '+')
				i++;

			for (; i < len; i++){
				c = s [i];

				if (c >= '0' && c <= '9'){
					uint d = (uint) (c - '0');
					
					val = checked (val * 10 + d);
					digits_seen = true;
				} else {
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
			if (!digits_seen)
				throw new FormatException ();
			
			return val;

		}

		public static uint Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static uint Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}
		
		public static uint Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return ToString ("G", null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString ("G", fp);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider fp)
		{
			string fmt;
			NumberFormatInfo nfi;
			
			fmt = (format == null) ? "G" : format;
			
			if (fp == null)
				nfi = NumberFormatInfo.CurrentInfo;
			else {
				nfi = (NumberFormatInfo) fp.GetFormat (Type);
				
				if (nfi == null)
					nfi = NumberFormatInfo.CurrentInfo;
			}

			return IntegerFormatter.NumberToString (fmt, nfi, value);
		}

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt32;
		}				
	}
}
