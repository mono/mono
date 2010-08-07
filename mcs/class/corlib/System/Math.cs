//
// System.Math.cs
//
// Authors:
//   Bob Smith (bob@thestuff.net)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Pedro Martínez Juliá (yoros@wanadoo.es)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Bob Smith.  http://www.thestuff.net
// Copyright (C) 2003 Pedro Martínez Juliá <yoros@wanadoo.es>
// Copyright (C) 2004 Novell (http://www.novell.com)
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

using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace System
{
	public static class Math
	{
		public const double E = 2.7182818284590452354;
		public const double PI = 3.14159265358979323846;

		public static decimal Abs (decimal value)
		{
			return (value < 0)? -value: value;
		}

		public static double Abs (double value)
		{
			return (value < 0)? -value: value;
		}

		public static float Abs (float value)
		{
			return (value < 0)? -value: value;
		}

		public static int Abs (int value)
		{
			if (value == Int32.MinValue)
				throw new OverflowException (Locale.GetText ("Value is too small."));
			return (value < 0)? -value: value;
		}

		public static long Abs (long value)
		{
			if (value == Int64.MinValue)
				throw new OverflowException (Locale.GetText ("Value is too small."));
			return (value < 0)? -value: value;
		}

		[CLSCompliant (false)]
		public static sbyte Abs (sbyte value)
		{
			if (value == SByte.MinValue)
				throw new OverflowException (Locale.GetText ("Value is too small."));
			return (sbyte)((value < 0)? -value: value);
		}

		public static short Abs (short value)
		{
			if (value == Int16.MinValue)
				throw new OverflowException (Locale.GetText ("Value is too small."));
			return (short)((value < 0)? -value: value);
		}

		public static decimal Ceiling (decimal d)
		{
			decimal result = Floor(d);
			if (result != d) {
				result++;
			}
			return result;
		}

		public static double Ceiling (double a)
		{
			double result = Floor(a);
			if (result != a) {
				result++;
			}
			return result;
		}

		// The following methods are defined in ECMA specs but they are
		// not implemented in MS.NET. However, they are in MS.NET 1.1

		public static long BigMul (int a, int b)
		{
			return ((long)a * (long)b);
		}

		public static int DivRem (int a, int b, out int result)
		{
			result = (a % b);
			return (int)(a / b);
		}

		public static long DivRem (long a, long b, out long result)
		{
			result = (a % b);
			return (long)(a / b);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Floor (double d);

		public static double IEEERemainder (double x, double y)
		{
			double r;
			if (y == 0)
				return Double.NaN;
			r = x - (y * Math.Round(x/y));
			if (r != 0)
				return r;
			/* Int64BitsToDouble is not endian-aware, but that is fine here */
			return (x > 0) ? 0: (BitConverter.Int64BitsToDouble (Int64.MinValue));
		}

		public static double Log (double a, double newBase)
		{
			if (newBase == 1.0)
				return Double.NaN;
			double result = Log(a) / Log(newBase);
			return (result == -0)? 0: result;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static byte Max (byte val1, byte val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static decimal Max (decimal val1, decimal val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static double Max (double val1, double val2)
		{
			if (Double.IsNaN (val1) || Double.IsNaN (val2)) {
				return Double.NaN;
			}
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static float Max (float val1, float val2)
		{
			if (Single.IsNaN (val1) || Single.IsNaN (val2)) {
				return Single.NaN;
			}
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static int Max (int val1, int val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static long Max (long val1, long val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public static sbyte Max (sbyte val1, sbyte val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static short Max (short val1, short val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public static uint Max (uint val1, uint val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public static ulong Max (ulong val1, ulong val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public static ushort Max (ushort val1, ushort val2)
		{
			return (val1 > val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static byte Min (byte val1, byte val2)
		{
			return (val1 < val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static decimal Min (decimal val1, decimal val2)
		{
			return (val1 < val2)? val1: val2;
 		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static double Min (double val1, double val2)
		{
			if (Double.IsNaN (val1) || Double.IsNaN (val2)) {
				return Double.NaN;
			}
			return (val1 < val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static float Min (float val1, float val2)
		{
			if (Single.IsNaN (val1) || Single.IsNaN (val2)) {
				return Single.NaN;
			}
			return (val1 < val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static int Min (int val1, int val2)
		{
			return (val1 < val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static long Min (long val1, long val2)
		{
			return (val1 < val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public static sbyte Min (sbyte val1, sbyte val2)
		{
			return (val1 < val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static short Min (short val1, short val2)
		{
			return (val1 < val2)? val1: val2;
		}

 		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
 		[CLSCompliant (false)]
		public static uint Min (uint val1, uint val2)
		{
			return (val1 < val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public static ulong Min (ulong val1, ulong val2)
		{
			return (val1 < val2)? val1: val2;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public static ushort Min (ushort val1, ushort val2)
		{
			return (val1 < val2)? val1: val2;
		}

		public static decimal Round (decimal d)
		{
			// Just call Decimal.Round(d, 0); when it rounds well.
			decimal int_part = Decimal.Floor(d);
			decimal dec_part = d - int_part;
			if (((dec_part == 0.5M) &&
				((2.0M * ((int_part / 2.0M) -
				Decimal.Floor(int_part / 2.0M))) != 0.0M)) ||
				(dec_part > 0.5M)) {
				int_part++;
			}
			return int_part;
		}

		public static decimal Round (decimal d, int decimals)
		{
			return Decimal.Round (d, decimals);
		}

		public static decimal Round (decimal d, MidpointRounding mode)
		{
			if ((mode != MidpointRounding.ToEven) && (mode != MidpointRounding.AwayFromZero))
				throw new ArgumentException ("The value '" + mode + "' is not valid for this usage of the type MidpointRounding.", "mode");

			if (mode == MidpointRounding.ToEven)
				return Round (d);
			else
				return RoundAwayFromZero (d);
		}

		static decimal RoundAwayFromZero (decimal d)
		{
			decimal int_part = Decimal.Floor(d);
			decimal dec_part = d - int_part;
			if (int_part >= 0 && dec_part >= 0.5M)
				int_part++;
			else if (int_part < 0 && dec_part > 0.5M)
				int_part++;
			return int_part;
		}

		public static decimal Round (decimal d, int decimals, MidpointRounding mode)
		{
			return Decimal.Round (d, decimals, mode);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Round (double a);

		public static double Round (double value, int digits)
		{
			if (digits < 0 || digits > 15)
				throw new ArgumentOutOfRangeException (Locale.GetText ("Value is too small or too big."));

			return Round2(value, digits, false);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static double Round2 (double value, int digits, bool away_from_zero);


		public static double Round (double value, MidpointRounding mode)
		{
			if ((mode != MidpointRounding.ToEven) && (mode != MidpointRounding.AwayFromZero))
				throw new ArgumentException ("The value '" + mode + "' is not valid for this usage of the type MidpointRounding.", "mode");

			if (mode == MidpointRounding.ToEven)
				return Round (value);
			if (value > 0)
				return Floor (value + 0.5);
			else
				return Ceiling (value - 0.5);
		}

		public static double Round (double value, int digits, MidpointRounding mode)
		{
			if ((mode != MidpointRounding.ToEven) && (mode != MidpointRounding.AwayFromZero))
				throw new ArgumentException ("The value '" + mode + "' is not valid for this usage of the type MidpointRounding.", "mode");

			if (mode == MidpointRounding.ToEven)
				return Round (value, digits);
			else
				return Round2 (value, digits, true);
		}
		
		public static double Truncate (double d)
		{
			if (d > 0D)
				return Floor (d);
			else if (d < 0D)
				return Ceiling (d);
			else
				return d;
		}

		public static decimal Truncate (decimal d)
		{
			return Decimal.Truncate (d);
		}

		public static decimal Floor (Decimal d)
		{
			return Decimal.Floor (d);
		}

		public static int Sign (decimal value)
		{
			if (value > 0) return 1;
			return (value == 0)? 0: -1;
		}

		public static int Sign (double value)
		{
			if (Double.IsNaN (value))
				throw new ArithmeticException ("NAN");
			if (value > 0) return 1;
			return (value == 0)? 0: -1;
		}

		public static int Sign (float value)
		{
			if (Single.IsNaN (value))
				throw new ArithmeticException ("NAN");
			if (value > 0) return 1;
			return (value == 0)? 0: -1;
		}

		public static int Sign (int value)
		{
			if (value > 0) return 1;
			return (value == 0)? 0: -1;
		}

		public static int Sign (long value)
		{
			if (value > 0) return 1;
			return (value == 0)? 0: -1;
		}

		[CLSCompliant (false)]
		public static int Sign (sbyte value)
		{
			if (value > 0) return 1;
			return (value == 0)? 0: -1;
		}

		public static int Sign (short value)
		{
			if (value > 0) return 1;
			return (value == 0)? 0: -1;
		}

		// internal calls
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Sin (double a);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Cos (double d);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Tan (double a);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Sinh (double value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Cosh (double value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Tanh (double value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Acos (double d);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Asin (double d);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Atan (double d);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Atan2 (double y, double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Exp (double d);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Log (double d);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Log10 (double d);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Pow (double x, double y);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static double Sqrt (double d);
	}
}
