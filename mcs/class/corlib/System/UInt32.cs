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
	
	public struct UInt32 : IComparable, IFormattable {
		public const uint MinValue = 0;
		public const uint MaxValue = 0xffffffff;
		
		public uint value;

		public int CompareTo (object v)
		{
			if (!(v is System.UInt32))
				throw new ArgumentException ("Value is not a System.UInt32");

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

		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt32;
		}

		public static uint Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static uint Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static uint Parse (string s, NumberStyles style, IFormatProvider fp)
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
