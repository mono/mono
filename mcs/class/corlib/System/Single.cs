//
// System.Single.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	
	[Serializable]
	public struct Single : IComparable, IFormattable, IConvertible {
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

		[MonoTODO]
		public static float Parse (string s, NumberStyles style, IFormatProvider fp) 
		{
			// FIXME: I copied this method from System.Double
			//        fix it for System.Single
			
			if (s == null) throw new ArgumentNullException();
			if (style > NumberStyles.Any) {
				throw new ArgumentException();
			}
			NumberFormatInfo format = NumberFormatInfo.GetInstance(fp);
			if (format == null) throw new Exception("How did this happen?");
			if (s == format.NaNSymbol) return Single.NaN;
			if (s == format.PositiveInfinitySymbol) return Single.PositiveInfinity;
			if (s == format.NegativeInfinitySymbol) return Single.NegativeInfinity;
			string[] sl;
			long integral = 0;
			long fraction = 0;
			long exponent = 1;
			float retval = 0;
			if ((style & NumberStyles.AllowLeadingWhite) != 0) {
				s.TrimStart(null);
			}
			if ((style & NumberStyles.AllowTrailingWhite) != 0) {
				s.TrimEnd(null);
			}
			sl = s.Split(new Char[] {'e', 'E'}, 2);
			if (sl.Length > 1) {
				if ((style & NumberStyles.AllowExponent) == 0) {
					throw new FormatException();
				}
				exponent = long.Parse(sl[1], NumberStyles.AllowLeadingSign, format);
			}
			s = sl[0];
			sl = s.Split(format.NumberDecimalSeparator.ToCharArray(), 2);
			if (sl.Length > 1) {
				if ((style & NumberStyles.AllowDecimalPoint) == 0) {
					throw new FormatException();
				}
				fraction = long.Parse(sl[1], NumberStyles.None, format);
			}
			NumberStyles tempstyle = NumberStyles.None;
			if ((style & NumberStyles.AllowLeadingSign) != 0){
				tempstyle = NumberStyles.AllowLeadingSign;
			}

			if (sl[0].Length > 0)
				integral = long.Parse(sl[0], tempstyle, format);
			else
				integral = 0;

			retval = fraction;

			// FIXME: what about the zeros between the decimal point 
			// and the first non-zero digit?
			while (retval >1) retval /= 10;
			if (integral < 0){
				retval -= integral;
				retval = -retval;
			}
			else retval += integral;
			if (exponent != 1) retval *= (float) Math.Pow(10, exponent);
			return retval;
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

		[MonoTODO]
		public string ToString (string format, IFormatProvider fp)
		{
			// FIXME: Need to pass format and provider info to this call too.
			return ToStringImpl(value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern string ToStringImpl (float value);

		// ============= IConvertible Methods ============ //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Single;
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (value);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte (value);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar (value);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return System.Convert.ToDateTime (value);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (value);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (value);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (value);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (value);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (value);
		}

		[CLSCompliant (false)]
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (value);
		}
		
		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (value);
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (value, conversionType, provider);
		}

		[CLSCompliant (false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (value);
		}

		[CLSCompliant (false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (value);
		}

		[CLSCompliant (false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (value);
		}
	}
}
