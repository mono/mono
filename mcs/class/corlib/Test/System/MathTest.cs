// MathTest.cs
//
// Jon Guymon (guymon@slackworks.com)
// Pedro Martínez Juliá (yoros@wanadoo.es)
//
// (C) 2002 Jon Guymon
// Copyright (C) 2003 Pedro Martínez Juliá <yoros@wanadoo.es>
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using System;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class MathTest
	{
		private static double double_epsilon = 2.2204460492503131e-16; /* DBL_EPSILON = 2^-52 */

		static double x = 0.1234;
		static double y = 12.345;

		[Test]
		public void TestDecimalAbs ()
		{
			decimal a = -9.0M;

			Assert.IsTrue (9.0M == Math.Abs (a), "#1");
			Assert.IsTrue (Decimal.MaxValue == Math.Abs (Decimal.MaxValue), "#2");
			Assert.IsTrue (Decimal.MaxValue == Math.Abs (Decimal.MinValue), "#3");
			Assert.IsTrue (Decimal.Zero == Math.Abs (Decimal.Zero), "#4");
			Assert.IsTrue (Decimal.One == Math.Abs (Decimal.One), "#5");
			Assert.IsTrue (Decimal.One == Math.Abs (Decimal.MinusOne), "#6");
		}

		[Test]
		public void TestDoubleAbs ()
		{
			double a = -9.0D;

			Assert.IsTrue (9.0D == Math.Abs (a), "#1");
			Assert.IsTrue (0.0D == Math.Abs (0.0D), "#2");
			Assert.IsTrue (Double.MaxValue == Math.Abs (Double.MaxValue), "#3");
			Assert.IsTrue (Double.MaxValue == Math.Abs (Double.MinValue), "#4");
			Assert.IsTrue (Double.IsPositiveInfinity (Math.Abs (Double.PositiveInfinity)), "#5");
			Assert.IsTrue (Double.IsPositiveInfinity (Math.Abs (Double.NegativeInfinity)), "#6");
			Assert.IsTrue (Double.IsNaN (Math.Abs (Double.NaN)), "#7");
		}

		[Test]
		public void TestFloatAbs ()
		{
			float a = -9.0F;

			Assert.IsTrue (9.0F == Math.Abs (a), "#1");
			Assert.IsTrue (0.0F == Math.Abs (0.0F), "#2");
			Assert.IsTrue (Single.MaxValue == Math.Abs (Single.MaxValue), "#3");
			Assert.IsTrue (Single.MaxValue == Math.Abs (Single.MinValue), "#4");
			Assert.IsTrue (Single.PositiveInfinity == Math.Abs (Single.PositiveInfinity), "#5");
			Assert.IsTrue (Single.PositiveInfinity == Math.Abs (Single.NegativeInfinity), "#6");
			Assert.IsTrue (Single.IsNaN (Math.Abs (Single.NaN)), "#7");
		}

		[Test]
		public void TestLongAbs ()
		{
			long a = -9L;
			long b = Int64.MinValue;

			Assert.IsTrue (9L == Math.Abs (a), "#1");
			try {
				Math.Abs (b);
				Assert.Fail ("#2");
			} catch (Exception e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#3");
			}
			Assert.IsTrue (Int64.MaxValue == Math.Abs (Int64.MaxValue), "#4");
		}

		[Test]
		public void TestIntAbs ()
		{
			int a = -9;
			int b = Int32.MinValue;

			Assert.IsTrue (9 == Math.Abs (a), "#1");
			try {
				Math.Abs (b);
				Assert.Fail ("#2");
			} catch (Exception e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#3");
			}
			Assert.IsTrue (Int32.MaxValue == Math.Abs (Int32.MaxValue), "#4");
		}

		[Test]
		public void TestSbyteAbs ()
		{
			sbyte a = -9;
			sbyte b = SByte.MinValue;

			Assert.IsTrue (9 == Math.Abs (a), "#1");
			try {
				Math.Abs (b);
				Assert.Fail ("#2");
			} catch (Exception e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#3");
			}
			Assert.IsTrue (SByte.MaxValue == Math.Abs (SByte.MaxValue), "#4");
		}

		[Test]
		public void TestShortAbs ()
		{
			short a = -9;
			short b = Int16.MinValue;

			Assert.IsTrue (9 == Math.Abs (a), "#1");
			try {
				Math.Abs (b);
				Assert.Fail ("#2");
			} catch (Exception e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#3");
			}
			Assert.IsTrue (Int16.MaxValue == Math.Abs (Int16.MaxValue), "#4");
		}

		[Test]
		public void TestAcos ()
		{
			double a = Math.Acos (x);
			double b = 1.4470809809523457;

			bool regularTest = (Math.Abs (a - b) <= double_epsilon);
			if (!regularTest){
				//
				// On MacOS X libc acos (0.1234) returns
				// 1.4470809809523455 (hex 0x3ff7273e62fda9ab) instead
				// of 1.4470809809523457 (hex 0x3ff7273e62fda9ac)
				//
				// For now, let it go
				//
				if (a == 1.4470809809523455)
					regularTest = true;
			}
			
			Assert.IsTrue (regularTest, a.ToString ("G99") + " != " + b.ToString ("G99"));
			
			Assert.IsTrue (double.IsNaN (Math.Acos (-1.01D)));
			Assert.IsTrue (double.IsNaN (Math.Acos (1.01D)));
			Assert.IsTrue (double.IsNaN (Math.Acos (Double.MinValue)));
			Assert.IsTrue (double.IsNaN (Math.Acos (Double.MaxValue)));
			Assert.IsTrue (double.IsNaN (Math.Acos (Double.NegativeInfinity)));
			Assert.IsTrue (double.IsNaN (Math.Acos (Double.PositiveInfinity)));
		}

		[Test]
		public void TestAsin ()
		{
			double a = Math.Asin (x);
			double b = 0.12371534584255098;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Asin (-1.01D)));
			Assert.IsTrue (double.IsNaN (Math.Asin (1.01D)));
			Assert.IsTrue (double.IsNaN (Math.Asin (Double.MinValue)));
			Assert.IsTrue (double.IsNaN (Math.Asin (Double.MaxValue)));
			Assert.IsTrue (double.IsNaN (Math.Asin (Double.NegativeInfinity)));
			Assert.IsTrue (double.IsNaN (Math.Asin (Double.PositiveInfinity)));
		}

		[Test]
		public void TestAtan ()
		{
			double a = Math.Atan (x);
			double b = 0.12277930094473837;
			double c = 1.5707963267948966;
			double d = -1.5707963267948966;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), "#1: " + a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Atan (double.NaN)), "should return NaN");
			Assert.IsTrue (Math.Abs ((double) Math.Atan (double.PositiveInfinity) - c) <= 0.0000000000000001,
				"#2: " + Math.Atan (double.PositiveInfinity).ToString ("G99") + " != " + c.ToString ("G99"));
			Assert.IsTrue (Math.Abs ((double) Math.Atan (double.NegativeInfinity) - d) <= 0.0000000000000001,
				"#3: " + Math.Atan (double.NegativeInfinity).ToString ("G99") + " != " + d.ToString ("G99"));
		}

		[Test]
		public void TestAtan2 ()
		{
			double a = Math.Atan2 (x, y);
			double b = 0.0099956168687207747;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Acos (-2D)));
			Assert.IsTrue (double.IsNaN (Math.Acos (2D)));
		}

		// The following test is for methods that are in ECMA but they are
		// not implemented in MS.NET. I leave them commented.
		/*
		public void TestBigMul () {
			int a = int.MaxValue;
			int b = int.MaxValue;

			Assert(((long)a * (long)b) == Math.BigMul(a,b));
		}
		*/

		[Test]
		public void TestCos ()
		{
			double a = Math.Cos (x);
			double b = 0.99239587670489104;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Cos (Double.NaN)));
			Assert.IsTrue (double.IsNaN (Math.Cos (Double.NegativeInfinity)));
			Assert.IsTrue (double.IsNaN (Math.Cos (Double.PositiveInfinity)));
		}

		[Test]
		public void TestCosh ()
		{
			double a = Math.Cosh (x);
			double b = 1.0076234465130722;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (Math.Cosh (double.NegativeInfinity) == double.PositiveInfinity);
			Assert.IsTrue (Math.Cosh (double.PositiveInfinity) == double.PositiveInfinity);
			Assert.IsTrue (double.IsNaN (Math.Cosh (double.NaN)));
		}

		// The following test is for methods that are in ECMA but they are
		// not implemented in MS.NET. I leave them commented.
		/*
		public void TestIntDivRem () {
			int a = 5;
			int b = 2;
			int div = 0, rem = 0;

			div = Math.DivRem (a, b, out rem);

			Assert.IsTrue (rem == 1);
			Assert.IsTrue (div == 2);
		}

		public void TestLongDivRem () {
			long a = 5;
			long b = 2;
			long div = 0, rem = 0;

			div = Math.DivRem (a, b, out rem);

			Assert.IsTrue (rem == 1);
			Assert.IsTrue (div == 2);
		}
		*/

		[Test]
		public void TestSin ()
		{
			double a = Math.Sin (x);
			double b = 0.12308705821137626;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Sin (Double.NaN)));
			Assert.IsTrue (double.IsNaN (Math.Sin (Double.NegativeInfinity)));
			Assert.IsTrue (double.IsNaN (Math.Sin (Double.PositiveInfinity)));
		}

		[Test]
		public void TestSinh ()
		{
			double a = Math.Sinh (x);
			double b = 0.12371341868561381;

			Assert.IsTrue (Math.Abs (a - b) <= 0.0000000000000001, a.ToString ("G99")
				       + " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Sinh (Double.NaN)), "SinH of Nan should be a Nan");
			Assert.IsTrue (double.IsNegativeInfinity (Math.Sinh (Double.NegativeInfinity)), "SinH of -inf should be -inf");
			Assert.IsTrue (double.IsPositiveInfinity (Math.Sinh (Double.PositiveInfinity)), "SinH of +inf should be +inf");
		}

		[Test]
		public void TestTan ()
		{
			double a = Math.Tan (x);
			double b = 0.12403019913793806;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (Double.IsNaN (Math.Tan (Double.NaN)));
			Assert.IsTrue (Double.IsNaN (Math.Tan (Double.PositiveInfinity)));
			Assert.IsTrue (Double.IsNaN (Math.Tan (Double.NegativeInfinity)));
		}

		[Test]
		public void TestTanh ()
		{
			double a = Math.Tanh (x);
			double b = 0.12277743150353424;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (Double.IsNaN (Math.Tanh (Double.NaN)),
				"Tanh(NaN) should be NaN");
			Assert.IsTrue (1 == Math.Tanh (Double.PositiveInfinity),
				"Tanh(+Infinity) should be 1");
			Assert.IsTrue (-1 == Math.Tanh (Double.NegativeInfinity),
				"Tanh(-Infinity) should be -1");
		}

		[Test]
		public void TestSqrt ()
		{
			double a = Math.Sqrt (x);
			double b = 0.35128336140500593;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (Double.IsNaN (Math.Sqrt (Double.NaN)));
			Assert.IsTrue (Double.IsPositiveInfinity (Math.Sqrt (Double.PositiveInfinity)));
			Assert.IsTrue (Double.IsNaN (Math.Sqrt (Double.NegativeInfinity)));
		}

		[Test]
		public void TestExp ()
		{
			double a = Math.Exp (x);
			double b = 1.1313368651986859;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Exp (double.NaN)));
			Assert.IsTrue (Math.Exp (double.NegativeInfinity) == 0);
			Assert.IsTrue (Math.Exp (double.PositiveInfinity) == double.PositiveInfinity);
		}

		[Test]
		public void TestCeiling ()
		{
			int iTest = 1;
			try {
				double a = Math.Ceiling (1.5);
				double b = 2;

				iTest++;
				Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
					+ " != " + b.ToString ("G99"));
				iTest++;
				Assert.IsTrue (Math.Ceiling (double.NegativeInfinity) == double.NegativeInfinity);
				iTest++;
				Assert.IsTrue (Math.Ceiling (double.PositiveInfinity) == double.PositiveInfinity);
				iTest++;
				Assert.IsTrue (double.IsNaN (Math.Ceiling (double.NaN)));

				iTest++;
				Assert.IsTrue (Double.MaxValue == Math.Ceiling (Double.MaxValue));

				iTest++;
				Assert.IsTrue (Double.MinValue == Math.Ceiling (Double.MinValue));
			} catch (Exception e) {
				Assert.Fail ("Unexpected Exception at iTest=" + iTest + ": " + e);
			}
		}

		[Test]
		public void TestDecimalCeiling()
		{
			decimal a = Math.Ceiling(1.5M);
			decimal b = 2M;

			Assert.IsTrue (a == b, "#1");
			Assert.IsTrue (Decimal.MaxValue == Math.Ceiling(Decimal.MaxValue), "#2");
			Assert.IsTrue (Decimal.MinValue == Math.Ceiling(Decimal.MinValue), "#3");
		}

		[Test]
		public void TestFloor ()
		{
			double a = Math.Floor (1.5);
			double b = 1;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (Math.Floor (double.NegativeInfinity) == double.NegativeInfinity);
			Assert.IsTrue (Math.Floor (double.PositiveInfinity) == double.PositiveInfinity);
			Assert.IsTrue (double.IsNaN (Math.Floor (double.NaN)));

			Assert.IsTrue (Double.MaxValue == Math.Floor (Double.MaxValue));

			Assert.IsTrue (Double.MinValue == Math.Floor (Double.MinValue));
		}

		[Test]
		public void TestIEEERemainder ()
		{
			double a = Math.IEEERemainder (y, x);
			double b = 0.0050000000000010592;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));

			Assert.IsTrue (double.IsNaN (Math.IEEERemainder (y, 0)), "Positive 0");

			// http://www.obtuse.com/resources/negative_zero.html
			double n0 = BitConverter.Int64BitsToDouble (Int64.MinValue);
			Assert.IsTrue (double.IsNaN (Math.IEEERemainder (n0, 0)), "Negative 0");

			// the "zero" remainder of negative number is negative
			long result = BitConverter.DoubleToInt64Bits (Math.IEEERemainder (-1, 1));
			Assert.AreEqual (Int64.MinValue, result, "Negative Dividend");
		}

		[Test]
		public void TestLog ()
		{
			double a = Math.Log (y);
			double b = 2.513251122797143;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Log (-1)));
			Assert.IsTrue (double.IsNaN (Math.Log (double.NaN)));

			// MS docs say this should be PositiveInfinity
			Assert.IsTrue (Math.Log (0) == double.NegativeInfinity);
			Assert.IsTrue (Math.Log (double.PositiveInfinity) == double.PositiveInfinity);
		}

		[Test]
		public void TestLog2 ()
		{
			double a = Math.Log (x, y);
			double b = -0.83251695325303621;

			Assert.IsTrue ((Math.Abs (a - b) <= 1e-14), a + " != " + b
				+ " because diff is " + Math.Abs (a - b));
			Assert.IsTrue (double.IsNaN (Math.Log (-1, y)));
			Assert.IsTrue (double.IsNaN (Math.Log (double.NaN, y)));
			Assert.IsTrue (double.IsNaN (Math.Log (x, double.NaN)));
			Assert.IsTrue (double.IsNaN (Math.Log (double.NegativeInfinity, y)));
			Assert.IsTrue (double.IsNaN (Math.Log (x, double.NegativeInfinity)));
			Assert.IsTrue (double.IsNaN (Math.Log (double.PositiveInfinity, double.PositiveInfinity)));
			Assert.IsTrue (double.IsNaN (Math.Log (2, 1)));

			// MS docs say this should be PositiveInfinity
			Assert.IsTrue (Math.Log (0, y) == double.NegativeInfinity);
			Assert.IsTrue (Math.Log (double.PositiveInfinity, y) == double.PositiveInfinity);

			Assert.IsTrue (Double.IsNaN (Math.Log (x, double.PositiveInfinity)));
		}

		[Test]
		public void TestLog10 ()
		{
			double a = Math.Log10 (x);
			double b = -0.90868484030277719;

			Assert.IsTrue ((Math.Abs (a - b) <= double_epsilon), a.ToString ("G99")
				+ " != " + b.ToString ("G99"));
			Assert.IsTrue (double.IsNaN (Math.Log10 (-1)));
			Assert.IsTrue (double.IsNaN (Math.Log10 (double.NaN)));

			// MS docs say this should be PositiveInfinity
			Assert.IsTrue (Math.Log10 (0) == double.NegativeInfinity);
			Assert.IsTrue (Math.Log10 (double.PositiveInfinity) == double.PositiveInfinity);

		}

		[Test]
		public void TestPow ()
		{
			double precision;
#if MONODROID
			// It fails on Nexus 9 with
			//
			//   1.3636094460602122 != 1.3636094460602119
			//
			// when using double_epsilon. Precision differs between different ARM CPUs, so we
			// will just use a more conservative value
			precision = double_epsilon * 10;
#else
			precision = double_epsilon;
#endif

			/* documentation cases : https://msdn.microsoft.com/en-us/library/system.math.pow%28v=vs.110%29.aspx */

			/* x or y = NaN -> NaN */
			Assert.IsTrue (double.IsNaN (Math.Pow (             double.NaN,              double.NaN)), "#1");
			Assert.IsTrue (double.IsNaN (Math.Pow (             double.NaN, double.NegativeInfinity)), "#2");
			Assert.IsTrue (double.IsNaN (Math.Pow (             double.NaN,                      -2)), "#2");
			Assert.IsTrue (double.IsNaN (Math.Pow (             double.NaN,                      -1)), "#3");
			Assert.IsFalse (double.IsNaN (Math.Pow (             double.NaN,                       0)), "#4");
			Assert.IsTrue (double.IsNaN (Math.Pow (             double.NaN,                       1)), "#5");
			Assert.IsTrue (double.IsNaN (Math.Pow (             double.NaN,                       2)), "#6");
			Assert.IsTrue (double.IsNaN (Math.Pow (             double.NaN, double.PositiveInfinity)), "#7");
			Assert.IsTrue (double.IsNaN (Math.Pow (double.NegativeInfinity,              double.NaN)), "#8");
			Assert.IsTrue (double.IsNaN (Math.Pow (                     -2,              double.NaN)), "#9");
			Assert.IsTrue (double.IsNaN (Math.Pow (                     -1,              double.NaN)), "#10");
			Assert.IsTrue (double.IsNaN (Math.Pow (                      0,              double.NaN)), "#11");
#if !WASM
			/* WASM returns NaN */
			Assert.IsFalse (double.IsNaN (Math.Pow (                      1,              double.NaN)), "#12");
#endif
			Assert.IsTrue (double.IsNaN (Math.Pow (                      2,              double.NaN)), "#13");
			Assert.IsTrue (double.IsNaN (Math.Pow (double.PositiveInfinity,              double.NaN)), "#14");

			/* x = Any value except NaN; y = 0 -> 1 */
			Assert.AreEqual ((double) 1, Math.Pow (2, 0), "#15");

			/* x = NegativeInfinity; y < 0 -> 0 */
			Assert.AreEqual ((double) 0, Math.Pow (double.NegativeInfinity, -2), "#16");

			/* x = NegativeInfinity; y is a positive odd integer -> NegativeInfinity */
			Assert.AreEqual (double.NegativeInfinity, Math.Pow (double.NegativeInfinity, 3), "#17");

			/* x = NegativeInfinity; y is positive but not an odd integer -> PositiveInfinity */
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (double.NegativeInfinity, 2), "#18");

			/* x < 0 but not NegativeInfinity; y is not an integer, NegativeInfinity, or PositiveInfinity -> NaN */
			Assert.IsTrue (double.IsNaN (Math.Pow (-1, 2.5)), "#19");

			/* x = -1; y = NegativeInfinity or PositiveInfinity -> NaN */
#if !WASM
			/* WASM returns NaN */
			Assert.IsFalse (double.IsNaN (Math.Pow (-1, double.PositiveInfinity)), "#20");
			Assert.IsFalse (double.IsNaN (Math.Pow (-1, double.NegativeInfinity)), "#21");
#endif

			/* -1 < x < 1; y = NegativeInfinity -> PositiveInfinity */
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (-0.5, double.NegativeInfinity), "#22");
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (+0.5, double.NegativeInfinity), "#23");

			/* -1 < x < 1; y = PositiveInfinity -> 0 */
			Assert.AreEqual ((double) 0, Math.Pow (-0.5, double.PositiveInfinity), "#24");
			Assert.AreEqual ((double) 0, Math.Pow (+0.5, double.PositiveInfinity), "#25");

			/* x < -1 or x > 1; y = NegativeInfinity -> 0 */
			Assert.AreEqual ((double) 0, Math.Pow (-2, double.NegativeInfinity), "#26");
			Assert.AreEqual ((double) 0, Math.Pow (+2, double.NegativeInfinity), "#27");

			/* x < -1 or x > 1; y = PositiveInfinity -> PositiveInfinity */
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (-2, double.PositiveInfinity), "#28");
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (+2, double.PositiveInfinity), "#29");

			/* x = 0; y < 0 -> PositiveInfinity */
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (0, -2), "#30");

			/* x = 0; y > 0 -> PositiveInfinity */
			Assert.AreEqual ((double) 0, Math.Pow (0, +2), "#31");

			/* x = 1; y is any value except NaN -> 1 */
#if !WASM
			Assert.AreEqual ((double) 1, Math.Pow (1, double.NegativeInfinity), "#32");
#endif
			Assert.AreEqual ((double) 1, Math.Pow (1,                      -2), "#33");
			Assert.AreEqual ((double) 1, Math.Pow (1,                       0), "#34");
			Assert.AreEqual ((double) 1, Math.Pow (1,                      +2), "#35");
#if !WASM
			Assert.AreEqual ((double) 1, Math.Pow (1, double.PositiveInfinity), "#36");
#endif

			/* x = PositiveInfinity; y < 0 -> 0 */
			Assert.AreEqual ((double) 0, Math.Pow (double.PositiveInfinity, -1), "#37");
			Assert.AreEqual ((double) 0, Math.Pow (double.PositiveInfinity, -2), "#38");

			/* x = PositiveInfinity; y > 0 -> PositiveInfinity */
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (double.PositiveInfinity, 1), "#39");
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (double.PositiveInfinity, 2), "#40");

			/* other cases */

			double a = Math.Pow (y, x);
			double b = 1.363609446060212;

			Assert.IsTrue (Math.Abs (a - b) <= precision, "#41 " + a.ToString ("G99") + " != " + b.ToString ("G99") + " +/- " + precision.ToString ("G99"));
			Assert.AreEqual (double.NegativeInfinity, Math.Pow (double.NegativeInfinity, 1), "#42");
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (double.NegativeInfinity, 2), "#43");

			Assert.AreEqual (Math.Pow (double.PositiveInfinity, double.NegativeInfinity), (double) 0, "#44");

			Assert.AreEqual ((double) 1, Math.Pow (-1, Double.MaxValue), "#45");
			Assert.AreEqual ((double) 1, Math.Pow (-1, Double.MinValue), "#46");
			Assert.AreEqual ((double) 0, Math.Pow (Double.MinValue, Double.MinValue), "#47");
			Assert.AreEqual (double.PositiveInfinity, Math.Pow (Double.MinValue, Double.MaxValue), "#48");

			double infinity = double.PositiveInfinity;
			Assert.AreEqual ((double) 0, Math.Pow (      0.5,  infinity), "#49");
			Assert.AreEqual (  infinity, Math.Pow (      0.5, -infinity), "#50");
			Assert.AreEqual (  infinity, Math.Pow (        2,  infinity), "#51");
			Assert.AreEqual ((double) 0, Math.Pow (        2, -infinity), "#52");
			Assert.AreEqual ((double) 1, Math.Pow ( infinity,         0), "#53");
			Assert.AreEqual ((double) 1, Math.Pow (-infinity,         0), "#54");
		}

		[Test]
		public void TestByteMax ()
		{
			byte a = 1;
			byte b = 2;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
		}

		[Test]
		public void TestDecimalMax ()
		{
			decimal a = 1.5M;
			decimal b = 2.5M;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
		}

		[Test]
		public void TestDoubleMax ()
		{
			double a = 1.5D;
			double b = 2.5D;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");

			Assert.IsTrue (Double.IsNaN (Math.Max (Double.NaN, Double.NaN)), "#3");
			Assert.IsTrue (Double.IsNaN (Math.Max (Double.NaN, a)), "#4");
			Assert.IsTrue (Double.IsNaN (Math.Max (b, Double.NaN)), "#5");
		}

		[Test]
		public void TestFloatMax ()
		{
			float a = 1.5F;
			float b = 2.5F;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
			Assert.IsTrue (Single.IsNaN (Math.Max (Single.NaN, Single.NaN)), "#3");
			Assert.IsTrue (Single.IsNaN (Math.Max (Single.NaN, a)), "#4");
			Assert.IsTrue (Single.IsNaN (Math.Max (b, Single.NaN)), "#5");
		}

		[Test]
		public void TestIntMax ()
		{
			int a = 1;
			int b = 2;
			int c = 100;
			int d = -2147483647;

			Assert.AreEqual (b, Math.Max (a, b), "#1");
			Assert.AreEqual (b, Math.Max (b, a), "#2");
			Assert.AreEqual (c, Math.Max (c, d), "#3");
			Assert.AreEqual (c, Math.Max (d, c), "#4");
		}

		[Test]
		public void TestLongMax ()
		{
			long a = 1L;
			long b = 2L;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
		}

		[Test]
		public void TestSbyteMax ()
		{
			sbyte a = 1;
			sbyte b = 2;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
		}

		[Test]
		public void TestShortMax ()
		{
			short a = 1;
			short b = 2;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
		}

		[Test]
		public void TestUintMax ()
		{
			uint a = 1U;
			uint b = 2U;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
		}

		[Test]
		public void TestUlongMax ()
		{
			ulong a = 1UL;
			ulong b = 2UL;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
		}

		[Test]
		public void TestUshortMax ()
		{
			ushort a = 1;
			ushort b = 2;

			Assert.IsTrue (b == Math.Max (a, b), "#1");
			Assert.IsTrue (b == Math.Max (b, a), "#2");
		}

		[Test]
		public void TestByteMin ()
		{
			byte a = 1;
			byte b = 2;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestDecimalMin ()
		{
			decimal a = 1.5M;
			decimal b = 2.5M;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestDoubleMin ()
		{
			double a = 1.5D;
			double b = 2.5D;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
			Assert.IsTrue (Double.IsNaN (Math.Min (Double.NaN, Double.NaN)), "#3");
			Assert.IsTrue (Double.IsNaN (Math.Min (Double.NaN, a)), "#4");
			Assert.IsTrue (Double.IsNaN (Math.Min (b, Double.NaN)), "#5");
		}

		[Test]
		public void TestFloatMin ()
		{
			float a = 1.5F;
			float b = 2.5F;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
			Assert.IsTrue (Single.IsNaN (Math.Min (Single.NaN, Single.NaN)), "#3");
			Assert.IsTrue (Single.IsNaN (Math.Min (Single.NaN, a)), "#4");
			Assert.IsTrue (Single.IsNaN (Math.Min (b, Single.NaN)), "#5");
		}

		[Test]
		public void TestIntMin ()
		{
			int a = 1;
			int b = 2;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestLongMin ()
		{
			long a = 1L;
			long b = 2L;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestSbyteMin ()
		{
			sbyte a = 1;
			sbyte b = 2;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestShortMin ()
		{
			short a = 1;
			short b = 2;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestUintMin ()
		{
			uint a = 1U;
			uint b = 2U;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestUlongMin ()
		{
			ulong a = 1UL;
			ulong b = 2UL;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestUshortMin ()
		{
			ushort a = 1;
			ushort b = 2;

			Assert.IsTrue (a == Math.Min (a, b), "#1");
			Assert.IsTrue (a == Math.Min (b, a), "#2");
		}

		[Test]
		public void TestDecimalRound ()
		{
			decimal a = 1.5M;
			decimal b = 2.5M;

			Assert.IsTrue (Math.Round (a) == 2, "#1");
			Assert.IsTrue (Math.Round (b) == 2, "#2");
			Assert.IsTrue (Decimal.MaxValue == Math.Round (Decimal.MaxValue), "#3");
			Assert.IsTrue (Decimal.MinValue == Math.Round (Decimal.MinValue), "#4");
		}

		[Test]
		public void TestDecimalRound2 ()
		{
			decimal a = 3.45M;
			decimal b = 3.46M;

			Assert.AreEqual (3.4M, Math.Round (a, 1), "#1");
			Assert.AreEqual (3.5M, Math.Round (b, 1), "#2");
		}

		[Test]
		public void TestDoubleRound ()
		{
			double a = 1.5D;
			double b = 2.5D;
			double c = 0.5000000000000001; // https://github.com/mono/mono/issues/9989

			Assert.AreEqual (2D, Math.Round (a), "#1");
			Assert.AreEqual (2D, Math.Round (b), "#2");
			Assert.AreEqual (1D, Math.Round (c), "#3");
			Assert.IsTrue (Double.MaxValue == Math.Round (Double.MaxValue), "#4");
			Assert.IsTrue (Double.MinValue == Math.Round (Double.MinValue), "#5");
		}

		[Test]
		public void TestDoubleTruncate ()
		{
			double a = 1.2D;
			double b = 2.8D;
			double c = 0D;

			Assert.AreEqual (1D, Math.Truncate (a), "#1");
			Assert.AreEqual (2D, Math.Truncate (b), "#2");

			Assert.AreEqual (-1D, Math.Truncate (a * -1D), "#3");
			Assert.AreEqual (-2D, Math.Truncate (b * -1D), "#4");

			Assert.AreEqual (0D, Math.Truncate (c), "#5");

			Assert.IsTrue (Double.MaxValue == Math.Truncate (Double.MaxValue), "#6");
			Assert.IsTrue (Double.MinValue == Math.Truncate (Double.MinValue), "#7");
		}

		[Test]
		public void TestDecimalTruncate ()
		{
			decimal a = 1.2M;
			decimal b = 2.8M;
			decimal c = 0M;

			Assert.AreEqual (1M, Math.Truncate (a), "#1");
			Assert.AreEqual (2M, Math.Truncate (b), "#2");

			Assert.AreEqual (-1M, Math.Truncate (a * -1M), "#3");
			Assert.AreEqual (-2M, Math.Truncate (b * -1M), "#4");

			Assert.AreEqual (0M, Math.Truncate (c), "#5");

			Assert.IsTrue (Decimal.MaxValue == Math.Truncate (Decimal.MaxValue), "#6");
			Assert.IsTrue (Decimal.MinValue == Math.Truncate (Decimal.MinValue), "#7");
		}

		[Test]
		public void TestDoubleRound2 ()
		{
			double a = 3.45D;
			double b = 3.46D;

			Assert.AreEqual (3.4D, Math.Round (a, 1), "#1");
			Assert.AreEqual (3.5D, Math.Round (b, 1), "#2");
			Assert.AreEqual (-0.1, Math.Round (-0.123456789, 1), "#3");
		}

		[Test]
		public void TestDoubleRound3 ()
		{
			Assert.AreEqual (1D, Math.Round (1D, 0, MidpointRounding.ToEven), "#1");
			Assert.AreEqual (1D, Math.Round (1D, 0, MidpointRounding.AwayFromZero), "#2");

			Assert.AreEqual (-1D, Math.Round (-1D, 0, MidpointRounding.ToEven), "#3");
			Assert.AreEqual (-1D, Math.Round (-1D, 0, MidpointRounding.AwayFromZero), "#4");

			Assert.AreEqual (1D, Math.Round (1D, 1, MidpointRounding.ToEven), "#5");
			Assert.AreEqual (1D, Math.Round (1D, 1, MidpointRounding.AwayFromZero), "#6");

			Assert.AreEqual (-1D, Math.Round (-1D, 1, MidpointRounding.ToEven), "#7");
			Assert.AreEqual (-1D, Math.Round (-1D, 1, MidpointRounding.AwayFromZero), "#8");

			Assert.AreEqual (1D, Math.Round (1.2345D, 0, MidpointRounding.ToEven), "#9");
			Assert.AreEqual (1D, Math.Round (1.2345D, 0, MidpointRounding.AwayFromZero), "#A");

			Assert.AreEqual (-1D, Math.Round (-1.2345D, 0, MidpointRounding.ToEven), "#B");
			Assert.AreEqual (-1D, Math.Round (-1.2345D, 0, MidpointRounding.AwayFromZero), "#C");

			Assert.AreEqual (1.2D, Math.Round (1.2345D, 1, MidpointRounding.ToEven), "#D");
			Assert.AreEqual (1.2D, Math.Round (1.2345D, 1, MidpointRounding.AwayFromZero), "#E");

			Assert.AreEqual (-1.2D, Math.Round (-1.2345D, 1, MidpointRounding.ToEven), "#F");
			Assert.AreEqual (-1.2D, Math.Round (-1.2345D, 1, MidpointRounding.AwayFromZero), "#10");

			Assert.AreEqual (1.23D, Math.Round (1.2345D, 2, MidpointRounding.ToEven), "#11");
			Assert.AreEqual (1.23D, Math.Round (1.2345D, 2, MidpointRounding.AwayFromZero), "#12");

			Assert.AreEqual (-1.23D, Math.Round (-1.2345D, 2, MidpointRounding.ToEven), "#13");
			Assert.AreEqual (-1.23D, Math.Round (-1.2345D, 2, MidpointRounding.AwayFromZero), "#14");

			Assert.AreEqual (1.234D, Math.Round (1.2345D, 3, MidpointRounding.ToEven), "#15");
			Assert.AreEqual (1.235D, Math.Round (1.2345D, 3, MidpointRounding.AwayFromZero), "#16");

			Assert.AreEqual (-1.234D, Math.Round (-1.2345D, 3, MidpointRounding.ToEven), "#17");
			Assert.AreEqual (-1.235D, Math.Round (-1.2345D, 3, MidpointRounding.AwayFromZero), "#18");

			Assert.AreEqual (1.2345D, Math.Round (1.2345D, 4, MidpointRounding.ToEven), "#19");
			Assert.AreEqual (1.2345D, Math.Round (1.2345D, 4, MidpointRounding.AwayFromZero), "#1A");

			Assert.AreEqual (-1.2345D, Math.Round (-1.2345D, 4, MidpointRounding.ToEven), "#1B");
			Assert.AreEqual (-1.2345D, Math.Round (-1.2345D, 4, MidpointRounding.AwayFromZero), "#1C");

			Assert.AreEqual (2D, Math.Round (1.5432D, 0, MidpointRounding.ToEven), "#1D");
			Assert.AreEqual (2D, Math.Round (1.5432D, 0, MidpointRounding.AwayFromZero), "#1E");

			Assert.AreEqual (-2D, Math.Round (-1.5432D, 0, MidpointRounding.ToEven), "#1F");
			Assert.AreEqual (-2D, Math.Round (-1.5432D, 0, MidpointRounding.AwayFromZero), "#20");

			Assert.AreEqual (1.5D, Math.Round (1.5432D, 1, MidpointRounding.ToEven), "#21");
			Assert.AreEqual (1.5D, Math.Round (1.5432D, 1, MidpointRounding.AwayFromZero), "#22");

			Assert.AreEqual (-1.5D, Math.Round (-1.5432D, 1, MidpointRounding.ToEven), "#23");
			Assert.AreEqual (-1.5D, Math.Round (-1.5432D, 1, MidpointRounding.AwayFromZero), "#24");

			Assert.AreEqual (1.54D, Math.Round (1.5432D, 2, MidpointRounding.ToEven), "#25");
			Assert.AreEqual (1.54D, Math.Round (1.5432D, 2, MidpointRounding.AwayFromZero), "#26");

			Assert.AreEqual (-1.54D, Math.Round (-1.5432D, 2, MidpointRounding.ToEven), "#27");
			Assert.AreEqual (-1.54D, Math.Round (-1.5432D, 2, MidpointRounding.AwayFromZero), "#28");

			Assert.AreEqual (1.543D, Math.Round (1.5432D, 3, MidpointRounding.ToEven), "#29");
			Assert.AreEqual (1.543D, Math.Round (1.5432D, 3, MidpointRounding.AwayFromZero), "#2A");

			Assert.AreEqual (-1.543D, Math.Round (-1.5432D, 3, MidpointRounding.ToEven), "#2B");
			Assert.AreEqual (-1.543D, Math.Round (-1.5432D, 3, MidpointRounding.AwayFromZero), "#2C");

			Assert.AreEqual (1.5432D, Math.Round (1.5432D, 4, MidpointRounding.ToEven), "#2D");
			Assert.AreEqual (1.5432D, Math.Round (1.5432D, 4, MidpointRounding.AwayFromZero), "#2E");

			Assert.AreEqual (-1.5432D, Math.Round (-1.5432D, 4, MidpointRounding.ToEven), "#2F");
			Assert.AreEqual (-1.5432D, Math.Round (-1.5432D, 4, MidpointRounding.AwayFromZero), "#30");

			Assert.AreEqual (63988D, Math.Round (63987.83593942D, 0, MidpointRounding.ToEven), "#31");
			Assert.AreEqual (63988D, Math.Round (63987.83593942D, 0, MidpointRounding.AwayFromZero), "#32");

			Assert.AreEqual (-63988D, Math.Round (-63987.83593942D, 0, MidpointRounding.ToEven), "#33");
			Assert.AreEqual (-63988D, Math.Round (-63987.83593942D, 0, MidpointRounding.AwayFromZero), "#34");

			Assert.AreEqual (63987.83594D, Math.Round (63987.83593942D, 5, MidpointRounding.ToEven), "#35");
			Assert.AreEqual (63987.83594D, Math.Round (63987.83593942D, 5, MidpointRounding.AwayFromZero), "#36");

			Assert.AreEqual (-63987.83594D, Math.Round (-63987.83593942D, 5, MidpointRounding.ToEven), "#37");
			Assert.AreEqual (-63987.83594D, Math.Round (-63987.83593942D, 5, MidpointRounding.AwayFromZero), "#38");

			Assert.AreEqual (63987.83593942D, Math.Round (63987.83593942D, 8, MidpointRounding.ToEven), "#39");
			Assert.AreEqual (63987.83593942D, Math.Round (63987.83593942D, 8, MidpointRounding.AwayFromZero), "#3A");

			Assert.AreEqual (-63987.83593942D, Math.Round (-63987.83593942D, 8, MidpointRounding.ToEven), "#3B");
			Assert.AreEqual (-63987.83593942D, Math.Round (-63987.83593942D, 8, MidpointRounding.AwayFromZero), "#3C");

			Assert.AreEqual (1, Math.Round (0.5, 0, MidpointRounding.AwayFromZero));
		}
		
		[Test]
		public void TestDecimalSign ()
		{
			decimal a = -5M;
			decimal b = 5M;

			Assert.IsTrue (Math.Sign (a) == -1, "#1");
			Assert.IsTrue (Math.Sign (b) == 1, "#2");
			Assert.IsTrue (Math.Sign (0M) == 0, "#3");
		}

		[Test]
		public void TestDoubleSign ()
		{
			double a = -5D;
			double b = 5D;

			Assert.IsTrue (Math.Sign (a) == -1, "#1");
			Assert.IsTrue (Math.Sign (b) == 1, "#2");
			Assert.IsTrue (Math.Sign (0D) == 0, "#3");
		}

		[Test]
		public void TestFloatSign ()
		{
			float a = -5F;
			float b = 5F;

			Assert.IsTrue (Math.Sign (a) == -1, "#1");
			Assert.IsTrue (Math.Sign (b) == 1, "#2");
			Assert.IsTrue (Math.Sign (0F) == 0, "#3");
		}

		[Test]
		public void TestIntSign ()
		{
			int a = -5;
			int b = 5;

			Assert.IsTrue (Math.Sign (a) == -1, "#1");
			Assert.IsTrue (Math.Sign (b) == 1, "#2");
			Assert.IsTrue (Math.Sign (0) == 0, "#3");
		}

		[Test]
		public void TestLongSign ()
		{
			long a = -5L;
			long b = 5L;

			Assert.IsTrue (Math.Sign (a) == -1, "#1");
			Assert.IsTrue (Math.Sign (b) == 1, "#2");
			Assert.IsTrue (Math.Sign (0L) == 0, "#3");
		}

		[Test]
		public void TestSbyteSign ()
		{
			sbyte a = -5;
			sbyte b = 5;

			Assert.IsTrue (Math.Sign (a) == -1, "#1");
			Assert.IsTrue (Math.Sign (b) == 1, "#2");
			Assert.IsTrue (Math.Sign (0) == 0, "#3");
		}

		[Test]
		public void TestShortSign ()
		{
			short a = -5;
			short b = 5;

			Assert.IsTrue (Math.Sign (a) == -1, "#1");
			Assert.IsTrue (Math.Sign (b) == 1, "#2");
			Assert.IsTrue (Math.Sign (0) == 0, "#3");
		}
	}
}
