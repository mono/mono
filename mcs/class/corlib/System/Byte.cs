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
	
	public struct Byte : IComparable, IFormattable {
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

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}

		public static byte Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static byte Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static byte Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public override string ToString ()
		{
			// TODO: Implement me

			return "";
		}

		public string ToString (IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}

		public string ToString (string format)
		{
			// TODO: Implement me.
			return "";
		}

		public string ToString (string format, IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}
	}
}
