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
	
	public struct Single : IComparable, IFormattable { //, IConvertible {
		public const float Epsilon = 1.4e-45f;
		public const float MaxValue =  3.40282346638528859e38f;
		public const float MinValue = -3.40282346638528859e38f;
		public const float NaN = 0.0f / 0.0f;
		public const float PositiveInfinity =  1.0f / 0.0f;
		public const float NegativeInfinity = -1.0f / 0.0f;
			
		// VES needs to know about value.  public is workaround
		// so source will compile
		public float value;
	       		
		public int CompareTo (object v)
		{
			if (v == null)
				return 1;

			if (!(v is System.Single))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Single"));

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

		public static bool IsInfinity (float f)
		{
			return (f == PositiveInfinity || f == NegativeInfinity);
		}

		public static bool IsNaN (float f)
		{
			return (f != f);
		}

		public static bool IsNegativeInfinity (float f)
		{
			return (f < 0.0f && (f == NegativeInfinity || f == PositiveInfinity));
		}

		public static bool IsPositiveInfinity (float f)
		{
			return (f > 0.0f && (f == NegativeInfinity || f == PositiveInfinity));
		}

		public static float Parse (string s)
		{
			return Parse (s, (NumberStyles.Float | NumberStyles.AllowThousands), null);
		}

		public static float Parse (string s, IFormatProvider fp)
		{
			return Parse (s, (NumberStyles.Float | NumberStyles.AllowThousands), fp);
		}
		
		public static float Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		//[MonoTODO]
		public static float Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return ToString(null, null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString(null, fp);
		}

		public string ToString (string format)
		{
			return ToString(format, null);
		}

		//[MonoTODO]
		public string ToString (string format, IFormatProvider fp)
		{
			// TODO: Implement me.
			throw new NotImplementedException ();
		}

		// ============= IConvertible Methods ============ //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Single;
		}
	}
}
