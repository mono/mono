//
// System.Single.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.Runtime.ConstrainedExecution;

namespace System
{
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct Single : IComparable, IFormattable, IConvertible, IComparable <float>, IEquatable <float>
	{
		public const float Epsilon = 1.4e-45f;
		public const float MaxValue =  3.40282346638528859e38f;
		public const float MinValue = -3.40282346638528859e38f;
		public const float NaN = 0.0f / 0.0f;
		public const float PositiveInfinity =  1.0f / 0.0f;
		public const float NegativeInfinity = -1.0f / 0.0f;

		// Maximum allowed rounding-error so that float.MaxValue can round-trip successfully; calculated 
		// using: (double.Parse (float.MaxValue.ToString ("r")) - (double) float.MaxValue).ToString ("r")
		private const double MaxValueEpsilon = 3.6147112457961776e29d;

		internal float m_value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is System.Single))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Single."));

			float fv = (float)value;

			if (IsPositiveInfinity (m_value) && IsPositiveInfinity (fv))
				return 0;

			if (IsNegativeInfinity (m_value) && IsNegativeInfinity (fv))
				return 0;

			if (IsNaN (fv))
				if (IsNaN (m_value))
					return 0;
				else
					return 1;

			if (IsNaN (m_value))
				if (IsNaN (fv))
					return 0;
				else
					return -1;

			if (this.m_value == fv)
				return 0;
			else if (this.m_value > fv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.Single))
				return false;

			float value = (float) obj;

			if (IsNaN (value))
				return IsNaN (m_value);

			return (value == m_value);
		}

		public int CompareTo (float value)
		{
			if (IsPositiveInfinity (m_value) && IsPositiveInfinity (value))
				return 0;

			if (IsNegativeInfinity (m_value) && IsNegativeInfinity (value))
				return 0;

			if (IsNaN (value))
				if (IsNaN (m_value))
					return 0;
				else
					return 1;

			if (IsNaN (m_value))
				if (IsNaN (value))
					return 0;
				else
					return -1;

			if (this.m_value == value)
				return 0;
			else if (this.m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (float obj)
		{
			if (IsNaN (obj))
				return IsNaN (m_value);

			return obj == m_value;
		}

		public unsafe override int GetHashCode ()
		{
			float f = m_value;
			return *((int*)&f);
		}

#if	NET_4_0
		public static bool operator==(float left, float right)
		{
			return left == right;
		}

		public static bool operator!=(float left, float right)
		{
			return left != right;
		}

		public static bool operator>(float left, float right)
		{
			return left > right;
		}

		public static bool operator>=(float left, float right)
		{
			return left >= right;
		}

		public static bool operator<(float left, float right)
		{
			return left < right;
		}

		public static bool operator<=(float left, float right)
		{
			return left <= right;
		}
#endif

		public static bool IsInfinity (float f)
		{
			return (f == PositiveInfinity || f == NegativeInfinity);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static bool IsNaN (float f)
		{
#pragma warning disable 1718
			return (f != f);
#pragma warning restore
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
			if (parsed_value - (double) float.MaxValue > MaxValueEpsilon && (!double.IsPositiveInfinity (parsed_value)))
				throw new OverflowException();

			return (float) parsed_value;
		}

		public static float Parse (string s, IFormatProvider provider)
		{
			double parsed_value = Double.Parse (
				s, (NumberStyles.Float | NumberStyles.AllowThousands), provider);
			if (parsed_value - (double) float.MaxValue > MaxValueEpsilon && (!double.IsPositiveInfinity (parsed_value)))
				throw new OverflowException();

			return (float) parsed_value;
		}
		
		public static float Parse (string s, NumberStyles style)
		{
			double parsed_value = Double.Parse (s, style, null);
			if (parsed_value - (double) float.MaxValue > MaxValueEpsilon && (!double.IsPositiveInfinity (parsed_value)))
				throw new OverflowException();

			return (float) parsed_value;
		}

		public static float Parse (string s, NumberStyles style, IFormatProvider provider) 
		{
			double parsed_value = Double.Parse (s, style, provider);
			if (parsed_value - (double) float.MaxValue > MaxValueEpsilon && (!double.IsPositiveInfinity (parsed_value)))
				throw new OverflowException();

			return (float) parsed_value;
		}
		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out float result)
		{
			double parsed_value;
			Exception exc;
			if (!Double.Parse (s, style, provider, true, out parsed_value, out exc)) {
				result = 0;
				return false;
			} else if (parsed_value - (double) float.MaxValue > MaxValueEpsilon && (!double.IsPositiveInfinity (parsed_value))) {
				result = 0;
				return false;
			}
			result = (float) parsed_value;
			return true;
		}

		public static bool TryParse (string s, out float result)
		{
			return TryParse (s, NumberStyles.Any, null, out result);
		}

		public override string ToString ()
		{
			return NumberFormatter.NumberToString (m_value, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return NumberFormatter.NumberToString (m_value, provider);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			return NumberFormatter.NumberToString (format, m_value, provider);
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

		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (m_value);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (m_value);
		}

		object IConvertible.ToType (Type targetType, IFormatProvider provider)
		{
			if (targetType == null)
				throw new ArgumentNullException ("targetType");
			return System.Convert.ToType (m_value, targetType, provider, false);
		}

		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (m_value);
		}

		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (m_value);
		}

		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (m_value);
		}
	}
}
