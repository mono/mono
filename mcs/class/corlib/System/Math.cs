//
// System.Math.cs
//
// Author:
//   Bob Smith (bob@thestuff.net)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Pedro Martínez Juliá (yoros@wanadoo.es)
//
// (C) 2001 Bob Smith.  http://www.thestuff.net
// Copyright (C) 2003 Pedro Martínez Juliá <yoros@wanadoo.es>
//

using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System
{
        public sealed class Math
        {
                public const double E = 2.7182818284590452354;
                public const double PI = 3.14159265358979323846;

		private Math () {}

                public static decimal Abs(decimal value)
                {
                        return (value < 0)? -value: value;
                }
                public static double Abs(double value)
                {
                        return (value < 0)? -value: value;
                }
                public static float Abs(float value)
                {
                        return (value < 0)? -value: value;
                }
                public static int Abs(int value)
                {
                        if (value == Int32.MinValue)
                                throw new OverflowException (Locale.GetText (Locale.GetText ("Value is too small")));
                        return (value < 0)? -value: value;
                }
                public static long Abs(long value)
                {
                        if (value == Int64.MinValue)
                                throw new OverflowException(Locale.GetText ("Value is too small"));
                        return (value < 0)? -value: value;
                }
		[CLSCompliant (false)]
                public static sbyte Abs(sbyte value)
                {
                        if (value == SByte.MinValue)
                                throw new OverflowException(Locale.GetText ("Value is too small"));
                        return (sbyte)((value < 0)? -value: value);
                }
                public static short Abs(short value)
                {
                        if (value == Int16.MinValue)
                                throw new OverflowException(Locale.GetText ("Value is too small"));
                        return (short)((value < 0)? -value: value);
                }

		// The following methods are defined in ECMA specs but they are
		// not implemented in MS.NET. I leave them commented.
		/*
		public static long BigMul (int a, int b) {
			return ((long)a * (long)b);
		}
		*/

		public static double Ceiling(double a) {
			double result = Floor(a);
			if (result != a) {
				result++;
			}
			return result;
		}

		// The following methods are defined in ECMA specs but they are
		// not implemented in MS.NET. I leave them commented.
		/*
		public static int DivRem (int a, int b, out int result) {
			result = (a % b);
			return (int)(a / b);
		}

		public static long DivRem (long a, long b, out long result) {
			result = (a % b);
			return (long)(a / b);
		}
		*/

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static double Floor (double value);

                public static double IEEERemainder(double x, double y)
                {
                        double r;
                        if (y == 0) return Double.NaN;
                        r = x - (y * Math.Round(x/y));
                        if (r != 0) return r;
                        return (x > 0)? 0: -0;
                }
                public static double Log(double a, double newBase)
                {
                        double result = Log(a) / Log(newBase);
                        return (result == -0)? 0: result;
                }

                public static byte Max(byte val1, byte val2)
                {
                        return (val1 > val2)? val1: val2;
                }

                public static decimal Max(decimal val1, decimal val2)
                {
                        return (val1 > val2)? val1: val2;
                }

                public static double Max(double val1, double val2)
                {
			if (Double.IsNaN(val1) || Double.IsNaN(val2)) {
				return Double.NaN;
			}
                        return (val1 > val2)? val1: val2;
                }
                public static float Max(float val1, float val2)
                {
			if (Single.IsNaN(val1) || Single.IsNaN(val2)) {
				return Single.NaN;
			}
                        return (val1 > val2)? val1: val2;
                }

                public static int Max(int val1, int val2)
                {
                        return (val1 > val2)? val1: val2;
                }

                public static long Max(long val1, long val2)
                {
                        return (val1 > val2)? val1: val2;
                }

		[CLSCompliant (false)]
                public static sbyte Max(sbyte val1, sbyte val2)
                {
                        return (val1 > val2)? val1: val2;
                }

                public static short Max(short val1, short val2)
                {
                        return (val1 > val2)? val1: val2;
                }

		[CLSCompliant (false)]
                public static uint Max(uint val1, uint val2)
                {
                        return (val1 > val2)? val1: val2;
                }

		[CLSCompliant (false)]
                public static ulong Max(ulong val1, ulong val2)
                {
                        return (val1 > val2)? val1: val2;
                }

		[CLSCompliant (false)]
                public static ushort Max(ushort val1, ushort val2)
                {
                        return (val1 > val2)? val1: val2;
                }

                public static byte Min(byte val1, byte val2)
                {
                        return (val1 < val2)? val1: val2;
                }

                public static decimal Min(decimal val1, decimal val2)
                {
                        return (val1 < val2)? val1: val2;
                }

                public static double Min(double val1, double val2)
                {
			if (Double.IsNaN(val1) || Double.IsNaN(val2)) {
				return Double.NaN;
			}
                        return (val1 < val2)? val1: val2;
                }

                public static float Min(float val1, float val2)
                {
			if (Single.IsNaN(val1) || Single.IsNaN(val2)) {
				return Single.NaN;
			}
                        return (val1 < val2)? val1: val2;
                }

                public static int Min(int val1, int val2)
                {
                        return (val1 < val2)? val1: val2;
                }

                public static long Min(long val1, long val2)
                {
                        return (val1 < val2)? val1: val2;
                }

		[CLSCompliant (false)]
                public static sbyte Min(sbyte val1, sbyte val2)
                {
                        return (val1 < val2)? val1: val2;
                }
                public static short Min(short val1, short val2)
                {
                        return (val1 < val2)? val1: val2;
                }

		[CLSCompliant (false)]
                public static uint Min(uint val1, uint val2)
                {
                        return (val1 < val2)? val1: val2;
                }

		[CLSCompliant (false)]
                public static ulong Min(ulong val1, ulong val2)
                {
                        return (val1 < val2)? val1: val2;
                }

		[CLSCompliant (false)]
                public static ushort Min(ushort val1, ushort val2)
                {
                        return (val1 < val2)? val1: val2;
                }

		public static decimal Round(decimal d) {
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

		public static decimal Round(decimal d, int decimals) {
			if (decimals < 0 || decimals > 28) {
				throw new ArgumentOutOfRangeException(
				Locale.GetText("Value is too small or too big."));
			}
			// Just call Decimal.Round(d, decimals); when it
			// rounds good.
			decimal p = (decimal) Math.Pow(10, decimals);
			decimal int_part = Decimal.Floor(d);
			decimal dec_part = d - int_part;
			dec_part *= 10000000000000000000000000000M;
			dec_part = Decimal.Floor(dec_part);
			dec_part /= (10000000000000000000000000000M / p);
			dec_part = Math.Round(dec_part);
			dec_part /= p;
			return int_part + dec_part;
		}

                [MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Round(double d);

		public static double Round(double value, int digits) {
			if (digits < 0 || digits > 15) {
				throw new ArgumentOutOfRangeException(
				Locale.GetText("Value is too small or too big."));
			}
			return Round2(value, digits);
		}

                [MethodImplAttribute (MethodImplOptions.InternalCall)]
                private extern static double Round2 (double value, int digits);

                public static int Sign(decimal value)
                {
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }
                public static int Sign(double value)
                {
                        if (Double.IsNaN(value))
                                throw new ArithmeticException("NAN");
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }
                public static int Sign(float value)
                {
                        if (Single.IsNaN(value))
                                throw new ArithmeticException("NAN");
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }
                public static int Sign(int value)
                {
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }
                public static int Sign(long value)
                {
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }

		[CLSCompliant (false)]
                public static int Sign(sbyte value)
                {
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }
                public static int Sign(short value)
                {
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }

		// internal calls 

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Sin (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Cos (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Tan (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Sinh (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Cosh (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Tanh (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Acos (double x);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Asin (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Atan (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Atan2 (double y, double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Exp (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Log (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Log10 (double x);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Pow (double x, double y);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Sqrt (double x);
        }
}
