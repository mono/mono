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
	
	public struct UInt64 : IComparable, IFormattable { //, IConvertible {
		public const ulong MaxValue = 0xffffffffffffffff;
		public const ulong MinValue = 0;
		
		public ulong value;

		public int CompareTo (object v)
		{
			if (!(v is System.UInt64))
				throw new ArgumentException ("Value is not a System.UInt64");

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
			return TypeCode.UInt64;
		}
	}
}
