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
	
	public struct Double : IComparable, IFormattable { //, IConvertible {
		public const double Epsilon = 4.9406564584124650e-324;
		public const double MaxValue =  1.7976931348623157e308;
		public const double MinValue = -1.7976931348623157e308;
		public const double NaN = 0.0d / 0.0d;
		public const double NegativeInfinity = -1.0d / 0.0d;
		public const double PositiveInfinity = 1.0d / 0.0d;
		
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

		public static bool IsInfinity (double d)
		{
			return (d == PositiveInfinity || d == NegativeInfinity);
		}

		public static bool IsNaN (double d)
		{
			return (d != d);
		}

		public static bool IsNegativeInfinity (double d)
		{
			return (d < 0.0d && (d == NegativeInfinity || d == PositiveInfinity));
		}

		public static bool IsPositiveInfinity (double d)
		{
			return (d > 0.0d && (d == NegativeInfinity || d == PositiveInfinity));
		}

		public static double Parse (string s)
		{
			return Parse (s, (NumberStyles.Float | NumberStyles.AllowThousands), null);
		}

		public static double Parse (string s, IFormatProvider fp)
		{
			return Parse (s, (NumberStyles.Float | NumberStyles.AllowThousands), fp);
		}

		public static double Parse (string s, NumberStyles style) 
		{
			return Parse (s, style, null);
		}

		public static double Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString (null, fp);
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
			return TypeCode.Double;
		}
	}
}
