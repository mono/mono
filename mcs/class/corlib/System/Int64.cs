//
// System.Int64.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {
	
	public struct Int64 : ValueType, IComparable, IFormattable {
		public const long MinValue = 0x8000000000000000;
		public const long MaxValue = 0x7fffffffffffffff;
		
		long value;

		public int CompareTo (object v)
		{
			if (!(value is System.Int64))
				throw new ArgumentException ("Value is not a System.Int64");

			return value - (long) v;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int64))
				return false;

			return ((long) o) == value;
		}

		public override int GetHashCode ()
		{
			return (value & 0xffffffff) ^ (value >> 32);
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int64;
		}

		public static long Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static long Parse (string s, IFormatProvider)
		{
			// TODO: Implement me
			return 0;
		}

		public static long Parse (string s, NumberStyles s, fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static long Parse (string s, NumberStyles s, IFormatProvider fp)
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
