//
// System.UInt64.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {
	
	public struct UInt64 : ValueType {
		public const ulong MinValue = 0;
		public const ulong MaxValue = 0xffffffffffffffff;
		
		ulong value;

		public int CompareTo (object v)
		{
			if (!(value is System.UInt64))
				throw new ArgumentException ("Value is not a System.UInt64");

			return value - (long) v;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.UInt64))
				return false;

			return ((ulong) o) == value;
		}

		public override int GetHashCode ()
		{
			return (value & 0xffffffff) ^ (value >> 32);
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt64;
		}

		public static ulong Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static ulong Parse (string s, IFormatProvider)
		{
			// TODO: Implement me
			return 0;
		}

		public static ulong Parse (string s, NumberStyles s, fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static ulong Parse (string s, NumberStyles s, IFormatProvider fp)
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

		publig string ToString (string format, IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}
	}
}
