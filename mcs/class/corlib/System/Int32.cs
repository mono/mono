//
// System.Int32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct Int32 : IComparable, IFormattable {
		public const int MinValue = -2147483648;
		public const int MaxValue = 0x7fffffff;
		
		public int value;

		public int CompareTo (object v)
		{
			if (!(v is System.Int32))
				throw new ArgumentException ("Value is not a System.Int32");

			return value - (int) v;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int32))
				return false;

			return ((int) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int32;
		}

		public static int Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static int Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static int Parse (string s, NumberStyles style, IFormatProvider fp)
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
