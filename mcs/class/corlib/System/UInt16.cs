//
// System.UInt16.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {
	
	public struct UInt16 : ValueType, IComparable, IFormattable {
		public const ushort MinValue = 0;
		public const ushort MaxValue = 0xffff;
		
		ushort value;

		public int CompareTo (object v)
		{
			if (!(value is System.UInt16))
				throw new ArgumentException ("Value is not a System.UInt16");

			return value - ((ushort) v);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.UInt16))
				return false;

			return ((ushort) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt16;
		}

		public static ushort Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static ushort Parse (string s, IFormatProvider)
		{
			// TODO: Implement me
			return 0;
		}

		public static ushort Parse (string s, NumberStyles s, fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static ushort Parse (string s, NumberStyles s, IFormatProvider fp)
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
