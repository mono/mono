//
// System.Int16.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct Int16 : IComparable, IFormattable {
		public const short MinValue = -32768;
		public const short MaxValue =  32767;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public short value;

		public int CompareTo (object v)
		{
			if (!(v is System.Int16))
				throw new ArgumentException ("Value is not a System.Int16");

			return value - ((short) v);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int16))
				return false;

			return ((short) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int16;
		}

		public static short Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static short Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static short Parse (string s, NumberStyles style, IFormatProvider fp)
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
