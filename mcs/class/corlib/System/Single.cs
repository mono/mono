//
// System.Single.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct Single : IComparable, IFormattable {
		public const float MinValue = (float) -3.402823e38;
		public const float MaxValue = (float) 3.402823e38;
		public const float NaN = (float) 0xffc00000;
		public const float NegativeInfinity = (float) 0xff800000;
		public const float PositiveInfinity = (float) 0x7f800000;
			
		// VES needs to know about value.  public is workaround
		// so source will compile
		public float value;

		
		
		public int CompareTo (object v)
		{
			if (!(v is System.Single))
				throw new ArgumentException ("Value is not a System.Single");

			return (int) (value - ((float) v));
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Single))
				return false;

			return ((float) o) == value;
		}

		public override int GetHashCode ()
		{
			return (int) value;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Single;
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
