//
// System.Single.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System
{
	[Serializable]
	public struct Single : IComparable, IFormattable, IConvertible
	{
		public const float Epsilon = 1.4e-45f;
		public const float MaxValue =  3.40282346638528859e38f;
		public const float MinValue = -3.40282346638528859e38f;
		public const float NaN = 0.0f / 0.0f;
		public const float PositiveInfinity =  1.0f / 0.0f;
		public const float NegativeInfinity = -1.0f / 0.0f;

		internal float m_value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;

			if (!(v is System.Single))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Single."));

			float fv = (float)v;

			if (this.m_value == fv)
				return 0;
			else if (this.m_value > fv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Single))
				return false;

			return ((float) o) == m_value;
		}

		public override int GetHashCode ()
		{
			return (int) m_value;
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
			double parsed_value = Double.Parse (
				s, (NumberStyles.Float | NumberStyles.AllowThousands), null);
			if (parsed_value > (double) float.MaxValue)
				throw new OverflowException();

			return (float) parsed_value;
		}

		public static float Parse (string s, IFormatProvider provider)
		{
			double parsed_value = Double.Parse (
				s, (NumberStyles.Float | NumberStyles.AllowThousands), provider);
			if (parsed_value > (double) float.MaxValue)
				throw new OverflowException();

			return (float) parsed_value;
		}
		
		public static float Parse (string s, NumberStyles style)
		{
			double parsed_value = Double.Parse (s, style, null);
			if (parsed_value > (double) float.MaxValue)
				throw new OverflowException();

			return (float) parsed_value;
		}

		public static float Parse (string s, NumberStyles style, IFormatProvider provider) 
		{
			double parsed_value = Double.Parse (s, style, provider);
			if (parsed_value > (double) float.MaxValue)
				throw new OverflowException();

			return (float) parsed_value;
		}

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			if (provider is CultureInfo)
				return SingleFormatter.NumberToString (format, ((CultureInfo) provider).NumberFormat, m_value);
			else
				return SingleFormatter.NumberToString (format, (NumberFormatInfo) provider, m_value);
		}

		// ============= IConvertible Methods ============ //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Single;
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (m_value);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte (m_value);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar (m_value);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return System.Convert.ToDateTime (m_value);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (m_value);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (m_value);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (m_value);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (m_value);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (m_value);
		}

		[CLSCompliant (false)]
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (m_value);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (m_value);
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (m_value, conversionType, provider);
		}

		[CLSCompliant (false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (m_value);
		}

		[CLSCompliant (false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (m_value);
		}

		[CLSCompliant (false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (m_value);
		}
	}
}
