//
// System.Double.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct Double : IComparable, IFormattable {
		/*
		 * FIXME: The MinValue and MaxValue are wrong!
		 */
		public const double MinValue = -1.79769313486232e307;
		public const double MaxValue =  1.79769313486232e307;
		public const double NaN = (double) 0xfff8000000000000;
		public const double NegativeInfinity = (double) 0xfff0000000000000;
		public const double PositiveInfinity = (double) 0x7ff0000000000000;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public double value;

		public int CompareTo (object v)
		{
			if (!(v is System.Double))
				throw new ArgumentException ("Value is not a System.Double");

			return (int) (value - ((double) v));
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Double))
				return false;

			return ((double) o) == value;
		}

		public override int GetHashCode ()
		{
			return (int) value;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Double;
		}

		public static float Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static float Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static float Parse (string s, NumberStyles style, IFormatProvider fp)
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
