//
// System.SByte.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	[CLSCompliant(false)]
	public struct SByte : IComparable, IFormattable { //, IConvertible {
		public static Type Type = typeof (sbyte);

		public const sbyte MinValue = -128;
		public const sbyte MaxValue = 127;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public sbyte value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.SByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.SByte"));

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

		public static sbyte Parse (string s)
		{
			sbyte val = 0;
			int len;
			int i;
			bool neg = false;
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

			c = s [i];
			if (c == '+')
				i++;
			else if (c == '-'){
				neg = true;
				i++;
			}
			
			for (; i < len; i++){
				c = s [i];

				if (c >= '0' && c <= '9'){
					val = checked ((sbyte) (val * 10 + (c - '0')));
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
			
			if (neg)
				val = checked ((sbyte) -val);

			return val;
		}

		public static sbyte Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static sbyte Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		//[MonoTODO]
		public static sbyte Parse (string s, NumberStyles style, IFormatProvider fp)
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

		// =========== ICovnertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}
	}
}
