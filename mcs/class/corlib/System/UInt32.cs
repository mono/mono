//
// System.UInt32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {
	
	public struct Int32 : ValueType, IComparable, IFormattable {
		public const uint MinValue = 0;
		public const uint MaxValue = 0xffffffff;
		
		uint value;

		public int CompareTo (object v)
		{
			if (!(value is System.UInt32))
				throw new ArgumentException ("Value is not a System.UInt32");

			return value - (uint) v;
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

		public static uint Parse (string s, IFormatProvider)
		{
			// TODO: Implement me
			return 0;
		}

		public static uint Parse (string s, NumberStyles s, fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static uint Parse (string s, NumberStyles s, IFormatProvider fp)
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
