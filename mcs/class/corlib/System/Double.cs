//
// System.Double.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Bob Smith       (bob@thestuff.net)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) Bob Smith.    http://www.thestuff.net
//

using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	
	[Serializable]
	public struct Double : IComparable, IFormattable, IConvertible {
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
			if (v == null)
				return 1;
			
			if (!(v is System.Double))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Double"));

			if (IsPositiveInfinity(value) && IsPositiveInfinity((double) v)){
				return 0;
			}

			if (IsNegativeInfinity(value) && IsNegativeInfinity((double) v)){
				return 0;
			}

			if (IsNaN((double) v)) {
				if (IsNaN(value))
					return 0;
				else
					return 1;
			}

			return (int) (value - ((double) v));
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Double))
				return false;

			if (IsNaN ((double)o)) {
				if (IsNaN(value))
					return true;
				else
					return false;
			}

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

		[MonoTODO]
		public static double Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			if (s == null) throw new ArgumentNullException();
			if (style > NumberStyles.Any)
			{
				throw new ArgumentException();
			}
			NumberFormatInfo format = NumberFormatInfo.GetInstance(provider);
			if (format == null) throw new Exception("How did this happen?");
			if (s == format.NaNSymbol) return Double.NaN;
			if (s == format.PositiveInfinitySymbol) return Double.PositiveInfinity;
			if (s == format.NegativeInfinitySymbol) return Double.NegativeInfinity;
			string[] sl;
			long integral = 0;
			long fraction = 0;
			long exponent = 1;
			double retval = 0;
			if ((style & NumberStyles.AllowLeadingWhite) != 0)
			{
				s.TrimStart(null);
			}
			if ((style & NumberStyles.AllowTrailingWhite) != 0)
			{
				s.TrimEnd(null);
			}
			sl = s.Split(new Char[] {'e', 'E'}, 2);
			if (sl.Length > 1)
			{
				if ((style & NumberStyles.AllowExponent) == 0)
				{
					throw new FormatException();
				}
				exponent = long.Parse(sl[1], NumberStyles.AllowLeadingSign, format);
			}
			s = sl[0];
			sl = s.Split(format.NumberDecimalSeparator.ToCharArray(), 2);
			if (sl.Length > 1)
			{
				if ((style & NumberStyles.AllowDecimalPoint) == 0)
				{
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
			if (exponent != 1) retval *= Math.Pow(10, exponent);
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
		private static extern string ToStringImpl (double value);

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Double;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType(value, conversionType, provider);
		}
		
		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean(value);
		}
		
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte(value);
		}
		
		char IConvertible.ToChar (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		[CLSCompliant(false)]
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal(value);
		}
		
		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble(value);
		}
		
		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16(value);
		}
		
		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32(value);
		}
		
		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64(value);
		}
		
		[CLSCompliant(false)] 
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte(value);
		}
		
		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle(value);
		}
		
/*
		string IConvertible.ToString (IFormatProvider provider)
		{
			return ToString(provider);
		}
*/

		[CLSCompliant(false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16(value);
		}
		
		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32(value);
		}
		
		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64(value);
		}
	}
}
