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
	
	public struct Byte : IComparable, IFormattable { //, IConvertible {
		public const byte MinValue = 0;
		public const byte MaxValue = 255;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public byte value;

		public int CompareTo (object v)
		{
			if (!(v is System.Byte))
				throw new ArgumentException ("Value is not a System.Byte");

			return value - ((byte) v);
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
			return Parse (s, NumberStyles.Integer, null);
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
			// TODO: Implement me
			return 0;
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
			// TODO: Implement me.
			return "";
		}

		// =========== IConvertible Methods =========== //
		
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}
	}
}
