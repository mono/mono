//
// System.Math.cs
//
// Author:
//   Bob Smith (bob@thestuff.net)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2001 Bob Smith.  http://www.thestuff.net
//

using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System
{
//	[CLSCompliant(false)]
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

                public static double Ceiling(double a)
                {
			if (Double.IsNaN(a)){
				return Double.NaN;
			}

			if (Double.IsNegativeInfinity(a)){
				return Double.NegativeInfinity;
			}

			if (Double.IsPositiveInfinity(a)){
				return Double.PositiveInfinity;
			}

			double b = (double)((long)a);
                        return (b < a)? b+1: b;
                }
                public static double Floor(double d) {
			if (Double.IsNaN(d)){
				return Double.NaN;
			}

			if (Double.IsNegativeInfinity(d)){
				return Double.NegativeInfinity;
			}

			if (Double.IsPositiveInfinity(d)){
				return Double.PositiveInfinity;
			}

			double b = (double)((long)d);
			return (d < 0 && d != b) ? --b : b;
                }
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
                        if (a == 0) return Double.NegativeInfinity;
                        else if (a < 0) return Double.NaN;
                        return Log(a)/Log(newBase);
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
                        return (val1 > val2)? val1: val2;
                }
                public static float Max(float val1, float val2)
                {
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
                        return (val1 < val2)? val1: val2;
                }
                public static float Min(float val1, float val2)
                {
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

                public static decimal Round(decimal d)
                {
                        decimal r = (decimal)((long)d);
                        decimal a = Abs (d - r);
                        if (a > .5M) return (r >= 0 ? ++r : --r);
                        else if (a <.5M) return r;
                        else
                        {
                                if (r%2 == 0) return r;
                                else return ++r;
                        }
                }
                public static decimal Round(decimal d, int decimals)
                {
                        long p = 1;
                        int c;
                        decimal retval = d;
                        if (decimals < 0 || decimals > 15)
                                throw new ArgumentOutOfRangeException(Locale.GetText (
					"Value is too small or too big."));
                        else if (decimals == 0)
                                return Math.Round(d);
                        for (c=0; c<decimals; c++) p*=10;
                        retval*=p;
                        retval=Math.Round(retval);
                        retval/=p;
                        return retval;
                }
                public static double Round(double d)
                {
                        double r = (double)((long)d);
                        double a = Abs (d - r);
                        if (a > .5) return (r >= 0 ? ++r : --r);
                        else if (a <.5) return r;
                        else
                        {
                                if (r%2 == 0) return r;
                                else return ++r;
                        }
                }
                public static double Round(double value, int digits) {
                        long p = 1;
                        int c;
                        double retval = value;
                        if (digits < 0 || digits > 15)
                                throw new ArgumentOutOfRangeException(Locale.GetText (
					"Value is too small or too big."));
                        else if (digits == 0)
                                return Math.Round(value);
                        for (c=0; c<digits; c++) p*=10;
                        retval*=p;
                        retval=Math.Round(retval);
                        retval/=p;
                        return retval;
                }
                public static int Sign(decimal value)
                {
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }
                public static int Sign(double value)
                {
                        if (value > 0) return 1;
                        return (value == 0)? 0: -1;
                }
                public static int Sign(float value)
                {
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
                private extern static double Pow (double x, double y);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
                public extern static double Sqrt (double x);
        }
}
