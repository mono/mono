//
// System.SByte.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct SByte : IComparable, IFormattable {
		public const sbyte MinValue = -128;
		public const sbyte MaxValue = 127;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public sbyte value;

		public int CompareTo (object v)
		{
			if (!(v is System.SByte))
				throw new ArgumentException ("Value is not a System.SByte");

			return value - ((sbyte) v);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.SByte))
				return false;

			return ((sbyte) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}

		public static sbyte Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static sbyte Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static sbyte Parse (string s, NumberStyles style, IFormatProvider fp)
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
