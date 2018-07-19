// BigIntegerTest.cs
//
// Authors:
// Rodrigo Kumpera <rkumpera@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//

using System;
using System.Numerics;
using System.Globalization;
using System.Threading;
using NUnit.Framework;


namespace MonoTests.System.Numerics
{
	[TestFixture]
	public class BigIntegerTest
	{
		static byte[] huge_a = new byte[] {0x1D, 0x33, 0xFB, 0xFE, 0xB1, 0x2, 0x85, 0x44, 0xCA, 0xDC, 0xFB, 0x70, 0xD, 0x39, 0xB1, 0x47, 0xB6, 0xE6, 0xA2, 0xD1, 0x19, 0x1E, 0x9F, 0xE4, 0x3C, 0x1E, 0x16, 0x56, 0x13, 0x9C, 0x4D, 0xD3, 0x5C, 0x74, 0xC9, 0xBD, 0xFA, 0x56, 0x40, 0x58, 0xAC, 0x20, 0x6B, 0x55, 0xA2, 0xD5, 0x41, 0x38, 0xA4, 0x6D, 0xF6, 0x8C, };

		static byte[] huge_b = new byte[] {0x96, 0x5, 0xDA, 0xFE, 0x93, 0x17, 0xC1, 0x93, 0xEC, 0x2F, 0x30, 0x2D, 0x8F, 0x28, 0x13, 0x99, 0x70, 0xF4, 0x4C, 0x60, 0xA6, 0x49, 0x24, 0xF9, 0xB3, 0x4A, 0x41, 0x67, 0xDC, 0xDD, 0xB1, 0xA5, 0xA6, 0xC0, 0x3D, 0x57, 0x9A, 0xCB, 0x29, 0xE2, 0x94, 0xAC, 0x6C, 0x7D, 0xEF, 0x3E, 0xC6, 0x7A, 0xC1, 0xA8, 0xC8, 0xB0, 0x20, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7, };

		static byte[] huge_add = new byte[] {0xB3, 0x38, 0xD5, 0xFD, 0x45, 0x1A, 0x46, 0xD8, 0xB6, 0xC, 0x2C, 0x9E, 0x9C, 0x61, 0xC4, 0xE0, 0x26, 0xDB, 0xEF, 0x31, 0xC0, 0x67, 0xC3, 0xDD, 0xF0, 0x68, 0x57, 0xBD, 0xEF, 0x79, 0xFF, 0x78, 0x3, 0x35, 0x7, 0x15, 0x95, 0x22, 0x6A, 0x3A, 0x41, 0xCD, 0xD7, 0xD2, 0x91, 0x14, 0x8, 0xB3, 0x65, 0x16, 0xBF, 0x3D, 0x20, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7, };

		static byte[] a_m_b = new byte[] { 0x87, 0x2D, 0x21, 0x0, 0x1E, 0xEB, 0xC3, 0xB0, 0xDD, 0xAC, 0xCB, 0x43, 0x7E, 0x10, 0x9E, 0xAE, 0x45, 0xF2, 0x55, 0x71, 0x73, 0xD4, 0x7A, 0xEB, 0x88, 0xD3, 0xD4, 0xEE, 0x36, 0xBE, 0x9B, 0x2D, 0xB6, 0xB3, 0x8B, 0x66, 0x60, 0x8B, 0x16, 0x76, 0x17, 0x74, 0xFE, 0xD7, 0xB2, 0x96, 0x7B, 0xBD, 0xE2, 0xC4, 0x2D, 0xDC, 0xDE, 0x6A, 0x19, 0xB3, 0x1E, 0x1F, 0xB4, 0xB6, 0x2A, 0xA5, 0x48, };
		static byte[] b_m_a = new byte[] { 0x79, 0xD2, 0xDE, 0xFF, 0xE1, 0x14, 0x3C, 0x4F, 0x22, 0x53, 0x34, 0xBC, 0x81, 0xEF, 0x61, 0x51, 0xBA, 0xD, 0xAA, 0x8E, 0x8C, 0x2B, 0x85, 0x14, 0x77, 0x2C, 0x2B, 0x11, 0xC9, 0x41, 0x64, 0xD2, 0x49, 0x4C, 0x74, 0x99, 0x9F, 0x74, 0xE9, 0x89, 0xE8, 0x8B, 0x1, 0x28, 0x4D, 0x69, 0x84, 0x42, 0x1D, 0x3B, 0xD2, 0x23, 0x21, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7, };

		static byte[] huge_mul = new byte[] { 0xFE, 0x83, 0xE1, 0x9B, 0x8D, 0x61, 0x40, 0xD1, 0x60, 0x19, 0xBD, 0x38, 0xF0, 0xFF, 0x90, 0xAE, 0xDD, 0xAE, 0x73, 0x2C, 0x20, 0x23, 0xCF, 0x6, 0x7A, 0xB4, 0x1C, 0xE7, 0xD9, 0x64, 0x96, 0x2C, 0x87, 0x7E, 0x1D, 0xB3, 0x8F, 0xD4, 0x33, 0xBA, 0xF4, 0x22, 0xB4, 0xDB, 0xC0, 0x5B, 0xA5, 0x64, 0xA0, 0xBC, 0xCA, 0x3E, 0x94, 0x95, 0xDA, 0x49, 0xE2, 0xA8, 0x33, 0xA2, 0x6A, 0x33, 0xB1, 0xF2, 0xEA, 0x99, 0x32, 0xD0, 0xB2, 0xAE, 0x55, 0x75, 0xBD, 0x19, 0xFC, 0x9A, 0xEC, 0x54, 0x87, 0x2A, 0x6, 0xCC, 0x78, 0xDA, 0x88, 0xBB, 0xAB, 0xA5, 0x47, 0xEF, 0xC7, 0x2B, 0xC7, 0x5B, 0x32, 0x31, 0xCD, 0xD9, 0x53, 0x96, 0x1A, 0x9D, 0x9A, 0x57, 0x40, 0x51, 0xB6, 0x5D, 0xC, 0x17, 0xD1, 0x86, 0xE9, 0xA4, 0x20, };

		static byte[] huge_div = new byte[] { 0x0, };
		static byte[] huge_rem = new byte[] { 0x1D, 0x33, 0xFB, 0xFE, 0xB1, 0x2, 0x85, 0x44, 0xCA, 0xDC, 0xFB, 0x70, 0xD, 0x39, 0xB1, 0x47, 0xB6, 0xE6, 0xA2, 0xD1, 0x19, 0x1E, 0x9F, 0xE4, 0x3C, 0x1E, 0x16, 0x56, 0x13, 0x9C, 0x4D, 0xD3, 0x5C, 0x74, 0xC9, 0xBD, 0xFA, 0x56, 0x40, 0x58, 0xAC, 0x20, 0x6B, 0x55, 0xA2, 0xD5, 0x41, 0x38, 0xA4, 0x6D, 0xF6, 0x8C, };
		static byte[][] add_a = new byte[][] {
			new byte[] {1},
			new byte[] {0xFF},
			huge_a
		};

		static byte[][] add_b = new byte[][] {
			new byte[] {1},
			new byte[] {1},
			huge_b
		};

		static byte[][] add_c = new byte[][] {
			new byte[] {2},
			new byte[] {0},
			huge_add
		};

		private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
		private NumberFormatInfo NfiUser;

		[TestFixtureSetUp]
		public void SetUpFixture() 
		{
			NfiUser = new NumberFormatInfo ();
			NfiUser.CurrencyDecimalDigits = 3;
			NfiUser.CurrencyDecimalSeparator = ":";
			NfiUser.CurrencyGroupSeparator = "/";
			NfiUser.CurrencyGroupSizes = new int[] { 2, 1, 0 };
			NfiUser.CurrencyNegativePattern = 10;  // n $-
			NfiUser.CurrencyPositivePattern = 3;  // n $
			NfiUser.CurrencySymbol = "XYZ";
			NfiUser.PercentDecimalDigits = 1;
			NfiUser.PercentDecimalSeparator = ";";
			NfiUser.PercentGroupSeparator = "~";
			NfiUser.PercentGroupSizes = new int[] { 1 };
			NfiUser.PercentNegativePattern = 2;
			NfiUser.PercentPositivePattern = 2;
			NfiUser.PercentSymbol = "%%%";
			NfiUser.NumberDecimalSeparator = ".";
		}

		[Test]
		public void Mul () {
			long[] values = new long [] { -1000000000L, -1000, -1, 0, 1, 1000, 100000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = a * b;
					Assert.AreEqual (values [i] * values [j], (long)c, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestHugeMul () {
			var a = new BigInteger (huge_a);
			var b = new BigInteger (huge_b);
			Assert.AreEqual (huge_mul, (a * b).ToByteArray (), "#1");
		}


		[Test]
		public void DivRem () {
			long[] values = new long [] { -10000000330L, -5000, -1, 0, 1, 1000, 333, 10234544400L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					if (values [j] == 0)
						continue;
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					BigInteger c, d;
					c = BigInteger.DivRem (a, b, out d);

					Assert.AreEqual (values [i] / values [j], (long)c, "#a_" + i + "_" + j);
					Assert.AreEqual (values [i] % values [j], (long)d, "#b_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestHugeDivRem () {
			var a = new BigInteger (huge_a);
			var b = new BigInteger (huge_b);
			BigInteger c, d;
			c = BigInteger.DivRem (a, b, out d);

			Assert.AreEqual (huge_div, c.ToByteArray (), "#1");
			Assert.AreEqual (huge_rem, d.ToByteArray (), "#2");
		}

		[Test]
		public void Pow () {
			try {
				BigInteger.Pow (1, -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {}
			
			Assert.AreEqual (1, (int)BigInteger.Pow (99999, 0), "#2");
			Assert.AreEqual (99999, (int)BigInteger.Pow (99999, 1), "#5");
			Assert.AreEqual (59049, (int)BigInteger.Pow (3, 10), "#4");
			Assert.AreEqual (177147, (int)BigInteger.Pow (3, 11), "#5");
			Assert.AreEqual (-177147, (int)BigInteger.Pow (-3, 11), "#6");
		}

		[Test]
		public void ModPow () {
			try {
				BigInteger.ModPow (1, -1, 5);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {}
			try {
				BigInteger.ModPow (1, 5, 0);
				Assert.Fail ("#2");
			} catch (DivideByZeroException) {}

			Assert.AreEqual (4L, (long)BigInteger.ModPow (3, 2, 5), "#2");
			Assert.AreEqual (20L, (long)BigInteger.ModPow (555, 10, 71), "#3");
			Assert.AreEqual (20L, (long)BigInteger.ModPow (-555, 10, 71), "#3");
			Assert.AreEqual (-24L, (long)BigInteger.ModPow (-555, 11, 71), "#3");
		}

		[Test]
		public void GCD () {	
			Assert.AreEqual (999999, (int)BigInteger.GreatestCommonDivisor (999999, 0), "#1");
			Assert.AreEqual (999999, (int)BigInteger.GreatestCommonDivisor (0, 999999), "#2");
			Assert.AreEqual (1, (int)BigInteger.GreatestCommonDivisor (999999, 1), "#3");
			Assert.AreEqual (1, (int)BigInteger.GreatestCommonDivisor (1, 999999), "#4");
			Assert.AreEqual (1, (int)BigInteger.GreatestCommonDivisor (1, 0), "#5");
			Assert.AreEqual (1, (int)BigInteger.GreatestCommonDivisor (0, 1), "#6");

			Assert.AreEqual (1, (int)BigInteger.GreatestCommonDivisor (999999, -1), "#7");
			Assert.AreEqual (1, (int)BigInteger.GreatestCommonDivisor (-1, 999999), "#8");
			Assert.AreEqual (1, (int)BigInteger.GreatestCommonDivisor (-1, 0), "#9");
			Assert.AreEqual (1, (int)BigInteger.GreatestCommonDivisor (0, -1), "#10");

			Assert.AreEqual (2, (int)BigInteger.GreatestCommonDivisor (12345678, 8765432), "#11");
			Assert.AreEqual (2, (int)BigInteger.GreatestCommonDivisor (-12345678, 8765432), "#12");
			Assert.AreEqual (2, (int)BigInteger.GreatestCommonDivisor (12345678, -8765432), "#13");
			Assert.AreEqual (2, (int)BigInteger.GreatestCommonDivisor (-12345678, -8765432), "#14");

			Assert.AreEqual (40, (int)BigInteger.GreatestCommonDivisor (5581 * 40, 6671 * 40), "#15");

			Assert.AreEqual (5, (int)BigInteger.GreatestCommonDivisor (-5, 0), "#16");
			Assert.AreEqual (5, (int)BigInteger.GreatestCommonDivisor (0, -5), "#17");
		}

		[Test]
		public void Log () {	
			double delta = 0.000000000000001d;

			Assert.AreEqual (double.NegativeInfinity, BigInteger.Log (0), "#1");
			Assert.AreEqual (0d, BigInteger.Log (1), "#2");
			Assert.AreEqual (double.NaN, BigInteger.Log (-1), "#3");
			Assert.AreEqual (2.3025850929940459d, BigInteger.Log (10), delta, "#4");
			Assert.AreEqual (6.9077552789821368d, BigInteger.Log (1000), delta, "#5");
			Assert.AreEqual (double.NaN, BigInteger.Log (-234), "#6");
		}

		[Test]
		public void LogN () {	
			double delta = 0.000000000000001d;

			Assert.AreEqual (double.NaN, BigInteger.Log (10, 1), "#1");
			Assert.AreEqual (double.NaN, BigInteger.Log (10, 0), "#2");
			Assert.AreEqual (double.NaN, BigInteger.Log (10, -1), "#3");

			Assert.AreEqual (double.NaN, BigInteger.Log (10, double.NaN), "#4");
			Assert.AreEqual (double.NaN, BigInteger.Log (10, double.NegativeInfinity), "#5");
			Assert.AreEqual (double.NaN, BigInteger.Log (10, double.PositiveInfinity), "#6");

			Assert.AreEqual (0d, BigInteger.Log (1, 0), "#7");
			Assert.AreEqual (double.NaN, BigInteger.Log (1, double.NegativeInfinity), "#8");
			Assert.AreEqual (0, BigInteger.Log (1, double.PositiveInfinity), "#9");
			Assert.AreEqual (double.NaN, BigInteger.Log (1, double.NaN), "#10");

			Assert.AreEqual (-2.5129415947320606d, BigInteger.Log (10, 0.4), delta, "#11");
		}

		[Test]
		public void DivRemByZero () {
			try {
				BigInteger c, d;
				c = BigInteger.DivRem (100, 0, out d);
				Assert.Fail ("#1");
			} catch (DivideByZeroException) {}
		}

		[Test]
		public void TestAdd () {
			for (int i = 0; i < add_a.Length; ++i) {
				var a = new BigInteger (add_a [i]);
				var b = new BigInteger (add_b [i]);
				var c = new BigInteger (add_c [i]);

				Assert.AreEqual (c, a + b, "#" + i + "a");
				Assert.AreEqual (c, b + a, "#" + i + "b");
				Assert.AreEqual (c, BigInteger.Add (a, b), "#" + i + "c");
				Assert.AreEqual (add_c [i], (a + b).ToByteArray (), "#" + i + "d");
			}
		}

		[Test]
		public void TestAdd2 () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = a + b;
					Assert.AreEqual (values [i] + values [j], (long)c, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestHugeSub () {
			var a = new BigInteger (huge_a);
			var b = new BigInteger (huge_b);
			Assert.AreEqual (a_m_b, (a - b).ToByteArray (), "#1");
			Assert.AreEqual (b_m_a, (b - a).ToByteArray (), "#2");
		}

		[Test]
		public void TestSub () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = a - b;
					var d = BigInteger.Subtract (a, b);

					Assert.AreEqual (values [i] - values [j], (long)c, "#_" + i + "_" + j);
					Assert.AreEqual (values [i] - values [j], (long)d, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestMin () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = BigInteger.Min (a, b);

					Assert.AreEqual (Math.Min (values [i], values [j]), (long)c, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestMax () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = BigInteger.Max (a, b);

					Assert.AreEqual (Math.Max (values [i], values [j]), (long)c, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestAbs () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				var a = new BigInteger (values [i]);
				var c = BigInteger.Abs (a);

				Assert.AreEqual (Math.Abs (values [i]), (long)c, "#_" + i);
			}
		}

		[Test]
		public void TestNegate () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				var a = new BigInteger (values [i]);
				var c = -a;
				var d = BigInteger.Negate (a);

				Assert.AreEqual (-values [i], (long)c, "#_" + i);
				Assert.AreEqual (-values [i], (long)d, "#_" + i);
			}
		}

		[Test]
		public void TestInc () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				var a = new BigInteger (values [i]);
				var b = ++a;

				Assert.AreEqual (++values [i], (long)b, "#_" + i);
			}
		}

		[Test]
		public void TestDec () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				var a = new BigInteger (values [i]);
				var b = --a;

				Assert.AreEqual (--values [i], (long)b, "#_" + i);
			}
		}

		[Test]
		public void TestBitwiseOps () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L, 0xFFFF00000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);


					Assert.AreEqual (values [i] | values [j], (long)(a | b) , "#b_" + i + "_" + j);
					Assert.AreEqual (values [i] & values [j], (long)(a & b) , "#a_" + i + "_" + j);
					Assert.AreEqual (values [i] ^ values [j], (long)(a ^ b) , "#c_" + i + "_" + j);
					Assert.AreEqual (~values [i], (long)~a , "#d_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestLeftShift () {
			Assert.AreEqual (new byte[] {0x00, 0x28},
				(new BigInteger(0x0A) << 10).ToByteArray (), "#1");
			Assert.AreEqual (new byte[] {0x00, 0xD8},
				(new BigInteger(-10) << 10).ToByteArray (), "#2");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0xFF},
				(new BigInteger(-1) << 16).ToByteArray (), "#3");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A},
				(new BigInteger(0x0A) << 80).ToByteArray (), "#4");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF6},
				(new BigInteger(-10) << 80).ToByteArray (), "#5");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF},
				(new BigInteger(-1) << 80).ToByteArray (), "#6");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0xD9},
				(new BigInteger(-1234) << 75).ToByteArray (), "#7");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x91, 0x00},
				(new BigInteger(0x1234) << 75).ToByteArray (), "#8");

			Assert.AreEqual (new byte[] {0xFF, 0x00}, (new BigInteger(0xFF00) << -8).ToByteArray (), "#9");
		}

		[Test]
		public void TestRightShift () {
			Assert.AreEqual (new byte[] {0x16, 0xB0, 0x4C, 0x02},
				(new BigInteger(1234567899L) >> 5).ToByteArray (), "#1");

			Assert.AreEqual (new byte[] {0x2C, 0x93, 0x00},
				(new BigInteger(1234567899L) >> 15).ToByteArray (), "#2");

			Assert.AreEqual (new byte[] {0xFF, 0xFF, 0x7F},
				(new BigInteger(long.MaxValue - 100) >> 40).ToByteArray (), "#3");

			Assert.AreEqual (new byte[] {0xE9, 0x4F, 0xB3, 0xFD},
				(new BigInteger(-1234567899L) >> 5).ToByteArray (), "#4");

			Assert.AreEqual (new byte[] {0xD3, 0x6C, 0xFF},
				(new BigInteger(-1234567899L) >> 15).ToByteArray (), "#5");

			Assert.AreEqual (new byte[] {0x00, 0x00, 0x80},
				(new BigInteger(long.MinValue + 100) >> 40).ToByteArray (), "#6");

			Assert.AreEqual (new byte[] { 0xFF },
				(new BigInteger(-1234567899L) >> 90).ToByteArray (), "#7");

			Assert.AreEqual (new byte[] {0x00},
				(new BigInteger(999999) >> 90).ToByteArray (), "#8");

			Assert.AreEqual (new byte[] {0x00, 0x00, 0xFF, 0x00}, (new BigInteger(0xFF00) >> -8).ToByteArray (), "#9");
		}

		[Test]
		public void CompareOps () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					
					Assert.AreEqual (values [i].CompareTo (values [j]), a.CompareTo (b), "#a_" + i + "_" + j);
					Assert.AreEqual (values [i].CompareTo (values [j]), BigInteger.Compare (a, b), "#b_" + i + "_" + j);

					Assert.AreEqual (values [i] < values [j], a < b, "#c_" + i + "_" + j);
					Assert.AreEqual (values [i] <= values [j], a <= b, "#d_" + i + "_" + j);
					Assert.AreEqual (values [i] == values [j], a == b, "#e_" + i + "_" + j);
					Assert.AreEqual (values [i] != values [j], a != b, "#f_" + i + "_" + j);
					Assert.AreEqual (values [i] >= values [j], a >= b, "#g_" + i + "_" + j);
					Assert.AreEqual (values [i] > values [j], a > b, "#h_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void CompareOps2 () {
			BigInteger a = new BigInteger (100000000000L);
			BigInteger b = new BigInteger (28282828282UL);

			Assert.IsTrue (a >= b, "#1");
			Assert.IsTrue (a >= b, "#2");
			Assert.IsFalse (a < b, "#3");
			Assert.IsFalse (a <= b, "#4");
			Assert.AreEqual (1, a.CompareTo (b), "#5");
		}

		[Test]
		public void CompareULong () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L, 0xAA00000000L };
			ulong[] uvalues = new ulong [] {0, 1, 1000, 100000000000L, 999999, 28282828282, 0xAA00000000, ulong.MaxValue };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < uvalues.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = uvalues [j];
					var c = new BigInteger (b);
					
					Assert.AreEqual (a.CompareTo (c), a.CompareTo (b), "#a_" + i + "_" + j);

					Assert.AreEqual (a > c, a > b, "#b_" + i + "_" + j);
					Assert.AreEqual (a < c, a < b, "#c_" + i + "_" + j);
					Assert.AreEqual (a <= c, a <= b, "#d_" + i + "_" + j);
					Assert.AreEqual (a == c, a == b, "#e_" + i + "_" + j);
					Assert.AreEqual (a != c, a != b, "#f_" + i + "_" + j);
					Assert.AreEqual (a >= c, a >= b, "#g_" + i + "_" + j);

					Assert.AreEqual (c > a, b > a, "#ib_" + i + "_" + j);
					Assert.AreEqual (c < a, b < a, "#ic_" + i + "_" + j);
					Assert.AreEqual (c <= a, b <= a, "#id_" + i + "_" + j);
					Assert.AreEqual (c == a, b == a, "#ie_" + i + "_" + j);
					Assert.AreEqual (c != a, b != a, "#if_" + i + "_" + j);
					Assert.AreEqual (c >= a, b >= a, "#ig_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void CompareLong () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 9999999, 100000000000L, 0xAA00000000, long.MaxValue, long.MinValue };

			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = values [j];
					var c = new BigInteger (b);
					
					Assert.AreEqual (a.CompareTo (c), a.CompareTo (b), "#a_" + i + "_" + j);

					Assert.AreEqual (a > c, a > b, "#b_" + i + "_" + j);
					Assert.AreEqual (a < c, a < b, "#c_" + i + "_" + j);
					Assert.AreEqual (a <= c, a <= b, "#d_" + i + "_" + j);
					Assert.AreEqual (a == c, a == b, "#e_" + i + "_" + j);
					Assert.AreEqual (a != c, a != b, "#f_" + i + "_" + j);
					Assert.AreEqual (a >= c, a >= b, "#g_" + i + "_" + j);

					Assert.AreEqual (c > a, b > a, "#ib_" + i + "_" + j);
					Assert.AreEqual (c < a, b < a, "#ic_" + i + "_" + j);
					Assert.AreEqual (c <= a, b <= a, "#id_" + i + "_" + j);
					Assert.AreEqual (c == a, b == a, "#ie_" + i + "_" + j);
					Assert.AreEqual (c != a, b != a, "#if_" + i + "_" + j);
					Assert.AreEqual (c >= a, b >= a, "#ig_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestEquals () {
				var a = new BigInteger (10);
				var b = new BigInteger (10);
				var c = new BigInteger (-10);

				Assert.AreEqual (a, b, "#1");
				Assert.AreNotEqual (a, c, "#2");
				Assert.AreNotEqual (a, 10, "#3");
		}

		[Test]
		public void ByteArrayCtor ()
		{
			try {
				new BigInteger (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {}

			Assert.AreEqual (0, (int)new BigInteger (new byte [0]), "#2");

			Assert.AreEqual (0, (int)new BigInteger (new byte [1]), "#3");

			Assert.AreEqual (0, (int)new BigInteger (new byte [2]), "#4");
		}

		[Test]
		public void IntCtorRoundTrip ()
		{
			int[] values = new int [] {
				int.MinValue, -0x2F33BB, -0x1F33, -0x33, 0, 0x33,
				0x80, 0x8190, 0xFF0011, 0x1234, 0x11BB99, 0x44BB22CC,
				int.MaxValue };
			foreach (var val in values) {
				var a = new BigInteger (val);
				var b = new BigInteger (a.ToByteArray ());

				Assert.AreEqual (val, (int)a, "#a_" + val);
				Assert.AreEqual (val, (int)b, "#b_" + val);
			}
		}

		[Test]
		public void LongCtorRoundTrip ()
		{
			long[] values = new long [] {
				0, long.MinValue, long.MaxValue, -1, 1L + int.MaxValue, -1L + int.MinValue, 0x1234, 0xFFFFFFFFL, 0x1FFFFFFFFL, -0xFFFFFFFFL, -0x1FFFFFFFFL,
				0x100000000L, -0x100000000L, 0x100000001L, -0x100000001L, 4294967295L, -4294967295L, 4294967296L, -4294967296L };
			foreach (var val in values) {
				try {
					var a = new BigInteger (val);
					var b = new BigInteger (a.ToByteArray ());

					Assert.AreEqual (val, (long)a, "#a_" + val);
					Assert.AreEqual (val, (long)b, "#b_" + val);
					Assert.AreEqual (a, b, "#a  == #b (" + val + ")");
				} catch (Exception) {
					Assert.Fail ("could not roundtrip {0}", val);
				}
			}
		}

		[Test]
		public void ByteArrayCtorRoundTrip ()
		{
			var arr = new byte [] { 1,2,3,4,5,6,7,8,9 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#1");

			arr = new byte [] { 1,2,3,4,5,6,7,8,0xFF, 0x0};
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#2");

			arr = new byte [] { 1,2,3,4,5,6,7,8,9, 0xF0 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#3");

			arr = new byte [] { 1};
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#4");

			arr = new byte [] { 1,2 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#5");

			arr = new byte [] { 1,2,3 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#6");

			arr = new byte [] { 1,2,3,4 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#7");

			arr = new byte [] { 1,2,3,4,5 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#8");

			arr = new byte [] { 1,2,3,4,5,6 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#9");

			arr = new byte [] { 1,2,3,4,5,6,7 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#10");

			arr = new byte [] { 1,2,3,4,5 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#11");

			arr = new byte [] { 0 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#12");

			arr = new byte [] { 0xFF, 00 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#13");

			arr = new byte [] { 1, 0, 0, 0, 0, 0, };
			Assert.AreEqual (new byte [1] {1}, new BigInteger (arr).ToByteArray (), "#14");
		}

		[Test]
		public void TestIntCtorProperties ()
		{
			BigInteger a = new BigInteger (10);
			Assert.IsTrue (a.IsEven, "#1");
			Assert.IsFalse (a.IsOne, "#2");
			Assert.IsFalse (a.IsPowerOfTwo, "#3");
			Assert.IsFalse (a.IsZero, "#4");
			Assert.AreEqual (1, a.Sign, "#5");

			Assert.IsFalse (new BigInteger (11).IsEven, "#6");
			Assert.IsTrue (new BigInteger (1).IsOne, "#7");
			Assert.IsTrue (new BigInteger (32).IsPowerOfTwo, "#8");
			Assert.IsTrue (new BigInteger (0).IsZero, "#9");
			Assert.IsTrue (new BigInteger ().IsZero, "#9b");
			Assert.AreEqual (0, new BigInteger (0).Sign, "#10");
			Assert.AreEqual (-1, new BigInteger (-99999).Sign, "#11");

			Assert.IsFalse (new BigInteger (0).IsPowerOfTwo, "#12");
			Assert.IsFalse (new BigInteger ().IsPowerOfTwo, "#12b");
			Assert.IsFalse (new BigInteger (-16).IsPowerOfTwo, "#13");
			Assert.IsTrue (new BigInteger (1).IsPowerOfTwo, "#14");
		}

		[Test]
		public void TestIntCtorToString ()
		{
			Assert.AreEqual ("5555", new BigInteger (5555).ToString (), "#1");
			Assert.AreEqual ("-99999", new BigInteger (-99999).ToString (), "#2");
		}

		[Test]
		public void TestToStringFmt ()
		{
			Assert.AreEqual ("123456789123456", new BigInteger (123456789123456).ToString ("D2"), "#1");
			Assert.AreEqual ("0000000005", new BigInteger (5).ToString ("d10"), "#2");
			Assert.AreEqual ("0A8", new BigInteger (168).ToString ("X"), "#3");
			Assert.AreEqual ("0", new BigInteger (0).ToString ("X"), "#4");
			Assert.AreEqual ("0", new BigInteger ().ToString ("X"), "#4b");
			Assert.AreEqual ("1", new BigInteger (1).ToString ("X"), "#5");
			Assert.AreEqual ("0A", new BigInteger (10).ToString ("X"), "#6");
			Assert.AreEqual ("F6", new BigInteger (-10).ToString ("X"), "#7");

			Assert.AreEqual ("10000000000000000000000000000000000000000000000000000000", BigInteger.Pow (10, 55).ToString ("G"), "#8");

			Assert.AreEqual ("10000000000000000000000000000000000000000000000000000000", BigInteger.Pow (10, 55).ToString ("R"), "#9");


			Assert.AreEqual ("000000000A", new BigInteger (10).ToString ("X10"), "#10");
			Assert.AreEqual ("0000000010", new BigInteger (10).ToString ("G10"), "#11");
		}

		[Test]
		public void TestToStringFmtProvider ()
		{
			NumberFormatInfo info = new NumberFormatInfo ();
			info.NegativeSign = ">";
			info.PositiveSign = "%";

			Assert.AreEqual ("10", new BigInteger (10).ToString (info), "#1");
			Assert.AreEqual (">10", new BigInteger (-10).ToString (info), "#2");
			Assert.AreEqual ("0A", new BigInteger (10).ToString ("X", info), "#3");
			Assert.AreEqual ("F6", new BigInteger (-10).ToString ("X", info), "#4");
			Assert.AreEqual ("10", new BigInteger (10).ToString ("G", info), "#5");
			Assert.AreEqual (">10", new BigInteger (-10).ToString ("G", info), "#6");
			Assert.AreEqual ("10", new BigInteger (10).ToString ("D", info), "#7");
			Assert.AreEqual (">10", new BigInteger (-10).ToString ("D", info), "#8");
			Assert.AreEqual ("10", new BigInteger (10).ToString ("R", info), "#9");
			Assert.AreEqual (">10", new BigInteger (-10).ToString ("R", info), "#10");

			info = new NumberFormatInfo ();
			info.NegativeSign = "#$%";
			Assert.AreEqual ("#$%10", new BigInteger (-10).ToString (info), "#2");
			Assert.AreEqual ("#$%10", new BigInteger (-10).ToString (null, info), "#2");

			info = new NumberFormatInfo ();
			Assert.AreEqual ("-10", new BigInteger (-10).ToString (info), "#2");

		}

		[Test]
		public void TestToIntOperator ()
		{
			try {
				int v = (int)new BigInteger (huge_a);
				Assert.Fail ("#1");
			} catch (OverflowException) {}

			try {
				int v = (int)new BigInteger (1L + int.MaxValue);
				Assert.Fail ("#2");
			} catch (OverflowException) {}

			try {
				int v = (int)new BigInteger (-1L + int.MinValue);
				Assert.Fail ("#3");
			} catch (OverflowException) {}

			Assert.AreEqual (int.MaxValue, (int)new BigInteger (int.MaxValue), "#4");
			Assert.AreEqual (int.MinValue, (int)new BigInteger (int.MinValue), "#5");
		}


		[Test]
		public void TestToLongOperator ()
		{
			try {
				long v = (long)new BigInteger (huge_a);
				Assert.Fail ("#1");
			} catch (OverflowException) {}

			//long.MaxValue + 1
			try {
				long v = (long)new BigInteger (new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00 });
				Assert.Fail ("#2");
			} catch (OverflowException) {}

			//TODO long.MinValue - 1
			try {
				long v = (long)new BigInteger (new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0xFF });
				Assert.Fail ("#3");
			} catch (OverflowException) {}

			Assert.AreEqual (long.MaxValue, (long)new BigInteger (long.MaxValue), "#4");
			Assert.AreEqual (long.MinValue, (long)new BigInteger (long.MinValue), "#5");
		}

		[Test]
		public void TestIntCtorToByteArray ()
		{
			Assert.AreEqual (new byte[] { 0xFF }, new BigInteger (-1).ToByteArray (), "#1");
			Assert.AreEqual (new byte[] { 0xD4, 0xFE }, new BigInteger (-300).ToByteArray (), "#2");
			Assert.AreEqual (new byte[] { 0x80, 0x00 }, new BigInteger (128).ToByteArray (), "#3");
			Assert.AreEqual (new byte[] { 0x00, 0x60 }, new BigInteger (0x6000).ToByteArray (), "#4");
			Assert.AreEqual (new byte[] { 0x00, 0x80, 0x00 }, new BigInteger (0x8000).ToByteArray (), "#5");
			Assert.AreEqual (new byte[] { 0xDD, 0xBC, 0x00, 0x7A }, new BigInteger (0x7A00BCDD).ToByteArray (), "#6");
			Assert.AreEqual (new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, new BigInteger (int.MaxValue).ToByteArray (), "#7");
			Assert.AreEqual (new byte[] { 0x00, 0x00, 0x00, 0x80 }, new BigInteger (int.MinValue).ToByteArray (), "#8");
			Assert.AreEqual (new byte[] { 0x01, 0x00, 0x00, 0x80 }, new BigInteger (int.MinValue + 1).ToByteArray (), "#9");
			Assert.AreEqual (new byte[] { 0x7F }, new BigInteger (0x7F).ToByteArray (), "#10");
			Assert.AreEqual (new byte[] { 0x45, 0xCC, 0xD0 }, new BigInteger (-0x2F33BB).ToByteArray (), "#11");
			Assert.AreEqual (new byte[] { 0 }, new BigInteger (0).ToByteArray (), "#12");
			Assert.AreEqual (new byte[] { 0 }, new BigInteger ().ToByteArray (), "#13");
		}

		[Test]
		public void TestLongCtorToByteArray ()
		{
			Assert.AreEqual (new byte[] { 0x01 }, new BigInteger (0x01L).ToByteArray (), "#1");
			Assert.AreEqual (new byte[] { 0x02, 0x01 }, new BigInteger (0x0102L).ToByteArray (), "#2");
			Assert.AreEqual (new byte[] { 0x03, 0x02, 0x01 }, new BigInteger (0x010203L).ToByteArray (), "#3");
			Assert.AreEqual (new byte[] { 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x01020304L).ToByteArray (), "#4");
			Assert.AreEqual (new byte[] { 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x0102030405L).ToByteArray (), "#5");
			Assert.AreEqual (new byte[] { 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x010203040506L).ToByteArray (), "#6");
			Assert.AreEqual (new byte[] { 0x07, 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x01020304050607L).ToByteArray (), "#7");
			Assert.AreEqual (new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x0102030405060708L).ToByteArray (), "#8");

			Assert.AreEqual (new byte[] { 0xFF }, new BigInteger (-0x01L).ToByteArray (), "#1m");
			Assert.AreEqual (new byte[] { 0xFE, 0xFE}, new BigInteger (-0x0102L).ToByteArray (), "#2m");
			Assert.AreEqual (new byte[] { 0xFD, 0xFD, 0xFE }, new BigInteger (-0x010203L).ToByteArray (), "#3m");
			Assert.AreEqual (new byte[] { 0xFC, 0xFC, 0xFD, 0xFE}, new BigInteger (-0x01020304L).ToByteArray (), "#4m");
			Assert.AreEqual (new byte[] { 0xFB, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger (-0x0102030405L).ToByteArray (), "#5m");
			Assert.AreEqual (new byte[] { 0xFA, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger (-0x010203040506L).ToByteArray (), "#6m");
			Assert.AreEqual (new byte[] { 0xF9, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger (-0x01020304050607L).ToByteArray (), "#7m");
			Assert.AreEqual (new byte[] { 0xF8, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger (-0x0102030405060708L).ToByteArray (), "#8m");


			Assert.AreEqual (new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, new BigInteger (long.MaxValue).ToByteArray (), "#9");
			Assert.AreEqual (new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, new BigInteger (long.MinValue).ToByteArray (), "#10");

			Assert.AreEqual (new byte[] { 0xFF, 0xFF, 0xFF, 0x7F, 0xFF }, new BigInteger (-2147483649L).ToByteArray (), "11");
		}

		[Test]
		public void IComparable () {
			var a = new BigInteger (99);
			Assert.AreEqual (-1, a.CompareTo (100), "#1");
			Assert.AreEqual (1, a.CompareTo (null), "#2");
		}

		[Test]
		public void ShortOperators () {
			Assert.AreEqual (22, (int)new BigInteger ((short)22), "#1");
			Assert.AreEqual (-22, (int)new BigInteger ((short)-22), "#2");

			try {
				short x = (short)new BigInteger (10000000);
				Assert.Fail ("#3");
			} catch (OverflowException) {}

			try {
				short x = (short)new BigInteger (-10000000);
				Assert.Fail ("#4");
			} catch (OverflowException) {}
		}

		[Test]
		public void DoubleCtor () {
			try {
				new BigInteger (double.NaN);
				Assert.Fail ("#1");
			} catch (OverflowException) {}
			try {
				new BigInteger (double.NegativeInfinity);
				Assert.Fail ("#2");
			} catch (OverflowException) {}
			try {
				new BigInteger (double.PositiveInfinity);
				Assert.Fail ("#3");
			} catch (OverflowException) {}

			Assert.AreEqual (10000, (int)new BigInteger (10000.2), "#4");
			Assert.AreEqual (10000, (int)new BigInteger (10000.9), "#5");

			Assert.AreEqual (10000, (int)new BigInteger (10000.2), "#6");
			Assert.AreEqual (0, (int)new BigInteger (0.9), "#7");

			Assert.AreEqual (12345678999L, (long)new BigInteger (12345678999.33), "#8");
		}

		[Test]
		public void DoubleConversion () {
			Assert.AreEqual (999d, (double)new BigInteger (999), "#1");
			Assert.AreEqual (double.PositiveInfinity, (double)BigInteger.Pow (2, 1024), "#2");
			Assert.AreEqual (double.NegativeInfinity, (double)BigInteger.Pow (-2, 1025), "#3");

			Assert.AreEqual (0d, (double)BigInteger.Zero, "#4");
			Assert.AreEqual (1d, (double)BigInteger.One, "#5");
			Assert.AreEqual (-1d, (double)BigInteger.MinusOne, "#6");
			
			var result1 = BitConverter.Int64BitsToDouble (-4337852273739220173);
			Assert.AreEqual (result1, (double)new BigInteger (new byte[]{53, 152, 137, 177, 240, 81, 75, 198}), "#7");
			var result2 = BitConverter.Int64BitsToDouble (4893382453283402035);
			Assert.AreEqual (result2, (double)new BigInteger (new byte[]{53, 152, 137, 177, 240, 81, 75, 198, 0}), "#8");
			
			var result3 = BitConverter.Int64BitsToDouble (5010775143622804480);
			var result4 = BitConverter.Int64BitsToDouble (5010775143622804481);
			var result5 = BitConverter.Int64BitsToDouble (5010775143622804482);
			Assert.AreEqual (result3, (double)new BigInteger (new byte[]{0, 0, 0, 0, 16, 128, 208, 159, 60, 46, 59, 3}), "#9");
			Assert.AreEqual (result3, (double)new BigInteger (new byte[]{0, 0, 0, 0, 17, 128, 208, 159, 60, 46, 59, 3}), "#10");
			Assert.AreEqual (result3, (double)new BigInteger (new byte[]{0, 0, 0, 0, 24, 128, 208, 159, 60, 46, 59, 3}), "#11");
			Assert.AreEqual (result4, (double)new BigInteger (new byte[]{0, 0, 0, 0, 32, 128, 208, 159, 60, 46, 59, 3}), "#12");
			Assert.AreEqual (result4, (double)new BigInteger (new byte[]{0, 0, 0, 0, 48, 128, 208, 159, 60, 46, 59, 3}), "#13");
			Assert.AreEqual (result5, (double)new BigInteger (new byte[]{0, 0, 0, 0, 64, 128, 208, 159, 60, 46, 59, 3}), "#14");
			
			Assert.AreEqual (BitConverter.Int64BitsToDouble (-2748107935317889142), (double)new BigInteger (huge_a), "#15");
			Assert.AreEqual (BitConverter.Int64BitsToDouble (-2354774254443231289), (double)new BigInteger (huge_b), "#16");
			Assert.AreEqual (BitConverter.Int64BitsToDouble (8737073938546854790), (double)new BigInteger (huge_mul), "#17");
			
			Assert.AreEqual (BitConverter.Int64BitsToDouble (6912920136897069886), (double)(2278888483353476799 * BigInteger.Pow (2, 451)), "#18");
			Assert.AreEqual (double.PositiveInfinity, (double)(843942696292817306 * BigInteger.Pow (2, 965)), "#19");
		}

		[Test]
		public void DecimalCtor () {
			Assert.AreEqual (999, (int)new BigInteger (999.99m), "#1");
			Assert.AreEqual (-10000, (int)new BigInteger (-10000m), "#2");
			Assert.AreEqual (0, (int)new BigInteger (0m), "#3");
		}

		[Test]
		public void DecimalConversion () {
			Assert.AreEqual (999m, (decimal)new BigInteger (999), "#1");

			try {
				var x = (decimal)BigInteger.Pow (2, 1024);
				Assert.Fail ("#2");
			} catch (OverflowException) {}

			try {
				var x = (decimal)BigInteger.Pow (-2, 1025);
				Assert.Fail ("#3");
			} catch (OverflowException) {}

			Assert.AreEqual (0m, (decimal)new BigInteger (0), "#4");
			Assert.AreEqual (1m, (decimal)new BigInteger (1), "#5");
			Assert.AreEqual (-1m, (decimal)new BigInteger (-1), "#6");
			Assert.AreEqual (9999999999999999999999999999m,
				(decimal)new BigInteger (9999999999999999999999999999m), "#7");
			Assert.AreEqual (0m, (decimal)new BigInteger (), "#8");
		}

		[SetCulture ("pt-BR")]
		[Test]
		public void Parse_pt_BR () 
		{
			Parse ();
		}

		[Test]
		public void Parse () {
			try {
				BigInteger.Parse (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {}

			try {
				BigInteger.Parse ("");
				Assert.Fail ("#2");
			} catch (FormatException) {}


			try {
				BigInteger.Parse ("  ");
				Assert.Fail ("#3");
			} catch (FormatException) {}

			try {
				BigInteger.Parse ("hh");
				Assert.Fail ("#4");
			} catch (FormatException) {}

			try {
				BigInteger.Parse ("-");
				Assert.Fail ("#5");
			} catch (FormatException) {}

			try {
				BigInteger.Parse ("-+");
				Assert.Fail ("#6");
			} catch (FormatException) {}

			Assert.AreEqual (10, (int)BigInteger.Parse("+10"), "#7");
			Assert.AreEqual (10, (int)BigInteger.Parse("10 "), "#8");
			Assert.AreEqual (-10, (int)BigInteger.Parse("-10 "), "#9");
			Assert.AreEqual (10, (int)BigInteger.Parse("    10 "), "#10");
			Assert.AreEqual (-10, (int)BigInteger.Parse("  -10 "), "#11");

			Assert.AreEqual (-1, (int)BigInteger.Parse("F", NumberStyles.AllowHexSpecifier), "#12");
			Assert.AreEqual (-8, (int)BigInteger.Parse("8", NumberStyles.AllowHexSpecifier), "#13");
			Assert.AreEqual (8, (int)BigInteger.Parse("08", NumberStyles.AllowHexSpecifier), "#14");
			Assert.AreEqual (15, (int)BigInteger.Parse("0F", NumberStyles.AllowHexSpecifier), "#15");
			Assert.AreEqual (-1, (int)BigInteger.Parse("FF", NumberStyles.AllowHexSpecifier), "#16");
			Assert.AreEqual (255, (int)BigInteger.Parse("0FF", NumberStyles.AllowHexSpecifier), "#17");

			Assert.AreEqual (-17, (int)BigInteger.Parse("   (17)   ", NumberStyles.AllowParentheses | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite), "#18");
			Assert.AreEqual (-23, (int)BigInteger.Parse("  -23  ", NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite), "#19");

			Assert.AreEqual (300000, (int)BigInteger.Parse("3E5", NumberStyles.AllowExponent), "#20");
			var dsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
			Assert.AreEqual (250, (int)BigInteger.Parse("2" + dsep + "5E2", NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint), "#21");//2.5E2 = 250
			Assert.AreEqual (25, (int)BigInteger.Parse("2500E-2", NumberStyles.AllowExponent), "#22");

			Assert.AreEqual ("136236974127783066520110477975349088954559032721408", BigInteger.Parse("136236974127783066520110477975349088954559032721408", NumberStyles.None).ToString(), "#23");
			Assert.AreEqual ("136236974127783066520110477975349088954559032721408", BigInteger.Parse("136236974127783066520110477975349088954559032721408").ToString(), "#24");

			try {
				BigInteger.Parse ("2E3.0", NumberStyles.AllowExponent); // decimal notation for the exponent
				Assert.Fail ("#25");
			} catch (FormatException) {
			}

			try {
				Int32.Parse ("2" + dsep + "09E1",  NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent);
				Assert.Fail ("#26");
			} catch (OverflowException) {
			}
		}

		[Test]
		public void TryParse () {
			BigInteger x = BigInteger.One;
			Assert.IsFalse (BigInteger.TryParse ((string)null, out x), "#1");
			Assert.AreEqual (0, (int)x, "#1a");
			Assert.IsFalse (BigInteger.TryParse ("", out x), "#2");
			Assert.IsFalse (BigInteger.TryParse (" ", out x), "#3");
			Assert.IsFalse (BigInteger.TryParse (" -", out x), "#4");
			Assert.IsFalse (BigInteger.TryParse (" +", out x), "#5");
			Assert.IsFalse (BigInteger.TryParse (" FF", out x), "#6");

			Assert.IsTrue (BigInteger.TryParse (" 99", out x), "#7");
			Assert.AreEqual (99, (int)x, "#8");

			Assert.IsTrue (BigInteger.TryParse ("+133", out x), "#9");
			Assert.AreEqual (133, (int)x, "#10");

			Assert.IsTrue (BigInteger.TryParse ("-010", out x), "#11");
			Assert.AreEqual (-10, (int)x, "#12");

			//Number style and format provider

			Assert.IsFalse (BigInteger.TryParse ("null", NumberStyles.None, null, out x), "#13");
			Assert.AreEqual (0, (int)x, "#14");
			Assert.IsFalse (BigInteger.TryParse ("-10", NumberStyles.None, null, out x), "#15");
			Assert.IsFalse (BigInteger.TryParse ("(10)", NumberStyles.None, null, out x), "#16");
			Assert.IsFalse (BigInteger.TryParse (" 10", NumberStyles.None, null, out x), "#17");
			Assert.IsFalse (BigInteger.TryParse ("10 ", NumberStyles.None, null, out x), "#18");
			
			Assert.IsTrue (BigInteger.TryParse ("-10", NumberStyles.AllowLeadingSign, null, out x), "#19");
			Assert.AreEqual (-10, (int)x, "#20");
			Assert.IsTrue (BigInteger.TryParse ("(10)", NumberStyles.AllowParentheses, null, out x), "#21");
			Assert.AreEqual (-10, (int)x, "#22");
			Assert.IsTrue (BigInteger.TryParse (" 10", NumberStyles.AllowLeadingWhite, null, out x), "#23");
			Assert.AreEqual (10, (int)x, "#24");
			Assert.IsTrue (BigInteger.TryParse ("10 ", NumberStyles.AllowTrailingWhite, null, out x), "#25");
			Assert.AreEqual (10, (int)x, "#26");

			Assert.IsFalse (BigInteger.TryParse ("$10", NumberStyles.None, null, out x), "#26");
			Assert.IsFalse (BigInteger.TryParse ("$10", NumberStyles.None, Nfi, out x), "#27");
			Assert.IsFalse (BigInteger.TryParse ("%10", NumberStyles.None, Nfi, out x), "#28");
			Assert.IsFalse (BigInteger.TryParse ("10 ", NumberStyles.None, null, out x), "#29");

			Assert.IsTrue (BigInteger.TryParse ("10", NumberStyles.None, null, out x), "#30");
			Assert.AreEqual (10, (int)x, "#31");
			Assert.IsTrue (BigInteger.TryParse (Nfi.CurrencySymbol + "10", NumberStyles.AllowCurrencySymbol, Nfi, out x), "#32");
			Assert.AreEqual (10, (int)x, "#33");
			Assert.IsFalse (BigInteger.TryParse ("%10", NumberStyles.AllowCurrencySymbol, Nfi, out x), "#34");
		}

		[Test]
		public void TestUserCurrency ()
		{
			const int val1 = -1234567;
			const int val2 = 1234567;

			string s = "";
			BigInteger v;
			s = val1.ToString ("c", NfiUser);
			Assert.AreEqual ("1234/5/67:000 XYZ-", s, "Currency value type 1 is not what we want to try to parse");
			v = BigInteger.Parse ("1234/5/67:000   XYZ-", NumberStyles.Currency, NfiUser);
			Assert.AreEqual (val1, (int)v);

			s = val2.ToString ("c", NfiUser);
			Assert.AreEqual ("1234/5/67:000 XYZ", s, "Currency value type 2 is not what we want to try to parse");
			v = BigInteger.Parse (s, NumberStyles.Currency, NfiUser);
			Assert.AreEqual (val2, (int)v);
		}

		[Test]
		public void TryParseWeirdCulture () {
			var old = Thread.CurrentThread.CurrentCulture;
			var cur = (CultureInfo)old.Clone ();

			NumberFormatInfo ninfo = new NumberFormatInfo ();
			ninfo.NegativeSign = ">";
			ninfo.PositiveSign = "%";
			cur.NumberFormat = ninfo;

			Thread.CurrentThread.CurrentCulture = cur;
		
			BigInteger x = BigInteger.Zero;

			try {
				Assert.IsTrue (BigInteger.TryParse ("%11", out x), "#1");
				Assert.AreEqual (11, (int)x, "#2");

				Assert.IsTrue (BigInteger.TryParse (">11", out x), "#3");
				Assert.AreEqual (-11, (int)x, "#4");
			} finally {
				Thread.CurrentThread.CurrentCulture = old;
			}
		}

		[Test]
		public void CompareToLongToWithBigNumber () {
			var a = BigInteger.Parse ("123456789123456789"); 
			var b = BigInteger.Parse ("-123456789123456789");
			Assert.AreEqual (1, a.CompareTo (2000));
			Assert.AreEqual (1, a.CompareTo (-2000));
			Assert.AreEqual (-1, b.CompareTo (2000));
			Assert.AreEqual (-1, b.CompareTo (-2000));
		}

		[Test]
		public void LeftShiftByInt ()
		{
			var v = BigInteger.Parse("230794411440927908251127453634");

			Assert.AreEqual ("230794411440927908251127453634", (v << 0).ToString (), "#0");
			Assert.AreEqual ("461588822881855816502254907268", (v << 1).ToString (), "#1");
			Assert.AreEqual ("923177645763711633004509814536", (v << 2).ToString (), "#2");
			Assert.AreEqual ("1846355291527423266009019629072", (v << 3).ToString (), "#3");
			Assert.AreEqual ("3692710583054846532018039258144", (v << 4).ToString (), "#4");
			Assert.AreEqual ("7385421166109693064036078516288", (v << 5).ToString (), "#5");
			Assert.AreEqual ("14770842332219386128072157032576", (v << 6).ToString (), "#6");
			Assert.AreEqual ("29541684664438772256144314065152", (v << 7).ToString (), "#7");
			Assert.AreEqual ("59083369328877544512288628130304", (v << 8).ToString (), "#8");
			Assert.AreEqual ("118166738657755089024577256260608", (v << 9).ToString (), "#9");
			Assert.AreEqual ("236333477315510178049154512521216", (v << 10).ToString (), "#10");
			Assert.AreEqual ("472666954631020356098309025042432", (v << 11).ToString (), "#11");
			Assert.AreEqual ("945333909262040712196618050084864", (v << 12).ToString (), "#12");
			Assert.AreEqual ("1890667818524081424393236100169728", (v << 13).ToString (), "#13");
			Assert.AreEqual ("3781335637048162848786472200339456", (v << 14).ToString (), "#14");
			Assert.AreEqual ("7562671274096325697572944400678912", (v << 15).ToString (), "#15");
			Assert.AreEqual ("15125342548192651395145888801357824", (v << 16).ToString (), "#16");
			Assert.AreEqual ("30250685096385302790291777602715648", (v << 17).ToString (), "#17");
			Assert.AreEqual ("60501370192770605580583555205431296", (v << 18).ToString (), "#18");
			Assert.AreEqual ("121002740385541211161167110410862592", (v << 19).ToString (), "#19");
			Assert.AreEqual ("242005480771082422322334220821725184", (v << 20).ToString (), "#20");
			Assert.AreEqual ("484010961542164844644668441643450368", (v << 21).ToString (), "#21");
			Assert.AreEqual ("968021923084329689289336883286900736", (v << 22).ToString (), "#22");
			Assert.AreEqual ("1936043846168659378578673766573801472", (v << 23).ToString (), "#23");
			Assert.AreEqual ("3872087692337318757157347533147602944", (v << 24).ToString (), "#24");
			Assert.AreEqual ("7744175384674637514314695066295205888", (v << 25).ToString (), "#25");
			Assert.AreEqual ("15488350769349275028629390132590411776", (v << 26).ToString (), "#26");
			Assert.AreEqual ("30976701538698550057258780265180823552", (v << 27).ToString (), "#27");
			Assert.AreEqual ("61953403077397100114517560530361647104", (v << 28).ToString (), "#28");
			Assert.AreEqual ("123906806154794200229035121060723294208", (v << 29).ToString (), "#29");
			Assert.AreEqual ("247813612309588400458070242121446588416", (v << 30).ToString (), "#30");
			Assert.AreEqual ("495627224619176800916140484242893176832", (v << 31).ToString (), "#31");
			Assert.AreEqual ("991254449238353601832280968485786353664", (v << 32).ToString (), "#32");
			Assert.AreEqual ("1982508898476707203664561936971572707328", (v << 33).ToString (), "#33");
			Assert.AreEqual ("3965017796953414407329123873943145414656", (v << 34).ToString (), "#34");
			Assert.AreEqual ("7930035593906828814658247747886290829312", (v << 35).ToString (), "#35");
			Assert.AreEqual ("15860071187813657629316495495772581658624", (v << 36).ToString (), "#36");
			Assert.AreEqual ("31720142375627315258632990991545163317248", (v << 37).ToString (), "#37");
			Assert.AreEqual ("63440284751254630517265981983090326634496", (v << 38).ToString (), "#38");
			Assert.AreEqual ("126880569502509261034531963966180653268992", (v << 39).ToString (), "#39");
			Assert.AreEqual ("253761139005018522069063927932361306537984", (v << 40).ToString (), "#40");
			Assert.AreEqual ("507522278010037044138127855864722613075968", (v << 41).ToString (), "#41");
			Assert.AreEqual ("1015044556020074088276255711729445226151936", (v << 42).ToString (), "#42");
			Assert.AreEqual ("2030089112040148176552511423458890452303872", (v << 43).ToString (), "#43");
			Assert.AreEqual ("4060178224080296353105022846917780904607744", (v << 44).ToString (), "#44");
			Assert.AreEqual ("8120356448160592706210045693835561809215488", (v << 45).ToString (), "#45");
			Assert.AreEqual ("16240712896321185412420091387671123618430976", (v << 46).ToString (), "#46");
			Assert.AreEqual ("32481425792642370824840182775342247236861952", (v << 47).ToString (), "#47");
			Assert.AreEqual ("64962851585284741649680365550684494473723904", (v << 48).ToString (), "#48");
			Assert.AreEqual ("129925703170569483299360731101368988947447808", (v << 49).ToString (), "#49");
			Assert.AreEqual ("259851406341138966598721462202737977894895616", (v << 50).ToString (), "#50");
			Assert.AreEqual ("519702812682277933197442924405475955789791232", (v << 51).ToString (), "#51");
			Assert.AreEqual ("1039405625364555866394885848810951911579582464", (v << 52).ToString (), "#52");
			Assert.AreEqual ("2078811250729111732789771697621903823159164928", (v << 53).ToString (), "#53");
			Assert.AreEqual ("4157622501458223465579543395243807646318329856", (v << 54).ToString (), "#54");
			Assert.AreEqual ("8315245002916446931159086790487615292636659712", (v << 55).ToString (), "#55");
			Assert.AreEqual ("16630490005832893862318173580975230585273319424", (v << 56).ToString (), "#56");
			Assert.AreEqual ("33260980011665787724636347161950461170546638848", (v << 57).ToString (), "#57");
			Assert.AreEqual ("66521960023331575449272694323900922341093277696", (v << 58).ToString (), "#58");
			Assert.AreEqual ("133043920046663150898545388647801844682186555392", (v << 59).ToString (), "#59");
			Assert.AreEqual ("266087840093326301797090777295603689364373110784", (v << 60).ToString (), "#60");
			Assert.AreEqual ("532175680186652603594181554591207378728746221568", (v << 61).ToString (), "#61");
			Assert.AreEqual ("1064351360373305207188363109182414757457492443136", (v << 62).ToString (), "#62");
			Assert.AreEqual ("2128702720746610414376726218364829514914984886272", (v << 63).ToString (), "#63");
			Assert.AreEqual ("4257405441493220828753452436729659029829969772544", (v << 64).ToString (), "#64");
			Assert.AreEqual ("8514810882986441657506904873459318059659939545088", (v << 65).ToString (), "#65");
			Assert.AreEqual ("17029621765972883315013809746918636119319879090176", (v << 66).ToString (), "#66");
			Assert.AreEqual ("34059243531945766630027619493837272238639758180352", (v << 67).ToString (), "#67");
			Assert.AreEqual ("68118487063891533260055238987674544477279516360704", (v << 68).ToString (), "#68");
			Assert.AreEqual ("136236974127783066520110477975349088954559032721408", (v << 69).ToString (), "#69");
		}


		[Test]
		public void RightShiftByInt ()
		{
			var v = BigInteger.Parse("230794411440927908251127453634");
			v = v * BigInteger.Pow (2, 70);

			Assert.AreEqual ("272473948255566133040220955950698177909118065442816", (v >> 0).ToString (), "#0");
			Assert.AreEqual ("136236974127783066520110477975349088954559032721408", (v >> 1).ToString (), "#1");
			Assert.AreEqual ("68118487063891533260055238987674544477279516360704", (v >> 2).ToString (), "#2");
			Assert.AreEqual ("34059243531945766630027619493837272238639758180352", (v >> 3).ToString (), "#3");
			Assert.AreEqual ("17029621765972883315013809746918636119319879090176", (v >> 4).ToString (), "#4");
			Assert.AreEqual ("8514810882986441657506904873459318059659939545088", (v >> 5).ToString (), "#5");
			Assert.AreEqual ("4257405441493220828753452436729659029829969772544", (v >> 6).ToString (), "#6");
			Assert.AreEqual ("2128702720746610414376726218364829514914984886272", (v >> 7).ToString (), "#7");
			Assert.AreEqual ("1064351360373305207188363109182414757457492443136", (v >> 8).ToString (), "#8");
			Assert.AreEqual ("532175680186652603594181554591207378728746221568", (v >> 9).ToString (), "#9");
			Assert.AreEqual ("266087840093326301797090777295603689364373110784", (v >> 10).ToString (), "#10");
			Assert.AreEqual ("133043920046663150898545388647801844682186555392", (v >> 11).ToString (), "#11");
			Assert.AreEqual ("66521960023331575449272694323900922341093277696", (v >> 12).ToString (), "#12");
			Assert.AreEqual ("33260980011665787724636347161950461170546638848", (v >> 13).ToString (), "#13");
			Assert.AreEqual ("16630490005832893862318173580975230585273319424", (v >> 14).ToString (), "#14");
			Assert.AreEqual ("8315245002916446931159086790487615292636659712", (v >> 15).ToString (), "#15");
			Assert.AreEqual ("4157622501458223465579543395243807646318329856", (v >> 16).ToString (), "#16");
			Assert.AreEqual ("2078811250729111732789771697621903823159164928", (v >> 17).ToString (), "#17");
			Assert.AreEqual ("1039405625364555866394885848810951911579582464", (v >> 18).ToString (), "#18");
			Assert.AreEqual ("519702812682277933197442924405475955789791232", (v >> 19).ToString (), "#19");
			Assert.AreEqual ("259851406341138966598721462202737977894895616", (v >> 20).ToString (), "#20");
			Assert.AreEqual ("129925703170569483299360731101368988947447808", (v >> 21).ToString (), "#21");
			Assert.AreEqual ("64962851585284741649680365550684494473723904", (v >> 22).ToString (), "#22");
			Assert.AreEqual ("32481425792642370824840182775342247236861952", (v >> 23).ToString (), "#23");
			Assert.AreEqual ("16240712896321185412420091387671123618430976", (v >> 24).ToString (), "#24");
			Assert.AreEqual ("8120356448160592706210045693835561809215488", (v >> 25).ToString (), "#25");
			Assert.AreEqual ("4060178224080296353105022846917780904607744", (v >> 26).ToString (), "#26");
			Assert.AreEqual ("2030089112040148176552511423458890452303872", (v >> 27).ToString (), "#27");
			Assert.AreEqual ("1015044556020074088276255711729445226151936", (v >> 28).ToString (), "#28");
			Assert.AreEqual ("507522278010037044138127855864722613075968", (v >> 29).ToString (), "#29");
			Assert.AreEqual ("253761139005018522069063927932361306537984", (v >> 30).ToString (), "#30");
			Assert.AreEqual ("126880569502509261034531963966180653268992", (v >> 31).ToString (), "#31");
			Assert.AreEqual ("63440284751254630517265981983090326634496", (v >> 32).ToString (), "#32");
			Assert.AreEqual ("31720142375627315258632990991545163317248", (v >> 33).ToString (), "#33");
			Assert.AreEqual ("15860071187813657629316495495772581658624", (v >> 34).ToString (), "#34");
			Assert.AreEqual ("7930035593906828814658247747886290829312", (v >> 35).ToString (), "#35");
			Assert.AreEqual ("3965017796953414407329123873943145414656", (v >> 36).ToString (), "#36");
			Assert.AreEqual ("1982508898476707203664561936971572707328", (v >> 37).ToString (), "#37");
			Assert.AreEqual ("991254449238353601832280968485786353664", (v >> 38).ToString (), "#38");
			Assert.AreEqual ("495627224619176800916140484242893176832", (v >> 39).ToString (), "#39");
			Assert.AreEqual ("247813612309588400458070242121446588416", (v >> 40).ToString (), "#40");
			Assert.AreEqual ("123906806154794200229035121060723294208", (v >> 41).ToString (), "#41");
			Assert.AreEqual ("61953403077397100114517560530361647104", (v >> 42).ToString (), "#42");
			Assert.AreEqual ("30976701538698550057258780265180823552", (v >> 43).ToString (), "#43");
			Assert.AreEqual ("15488350769349275028629390132590411776", (v >> 44).ToString (), "#44");
			Assert.AreEqual ("7744175384674637514314695066295205888", (v >> 45).ToString (), "#45");
			Assert.AreEqual ("3872087692337318757157347533147602944", (v >> 46).ToString (), "#46");
			Assert.AreEqual ("1936043846168659378578673766573801472", (v >> 47).ToString (), "#47");
			Assert.AreEqual ("968021923084329689289336883286900736", (v >> 48).ToString (), "#48");
			Assert.AreEqual ("484010961542164844644668441643450368", (v >> 49).ToString (), "#49");
			Assert.AreEqual ("242005480771082422322334220821725184", (v >> 50).ToString (), "#50");
			Assert.AreEqual ("121002740385541211161167110410862592", (v >> 51).ToString (), "#51");
			Assert.AreEqual ("60501370192770605580583555205431296", (v >> 52).ToString (), "#52");
			Assert.AreEqual ("30250685096385302790291777602715648", (v >> 53).ToString (), "#53");
			Assert.AreEqual ("15125342548192651395145888801357824", (v >> 54).ToString (), "#54");
			Assert.AreEqual ("7562671274096325697572944400678912", (v >> 55).ToString (), "#55");
			Assert.AreEqual ("3781335637048162848786472200339456", (v >> 56).ToString (), "#56");
			Assert.AreEqual ("1890667818524081424393236100169728", (v >> 57).ToString (), "#57");
			Assert.AreEqual ("945333909262040712196618050084864", (v >> 58).ToString (), "#58");
			Assert.AreEqual ("472666954631020356098309025042432", (v >> 59).ToString (), "#59");
			Assert.AreEqual ("236333477315510178049154512521216", (v >> 60).ToString (), "#60");
			Assert.AreEqual ("118166738657755089024577256260608", (v >> 61).ToString (), "#61");
			Assert.AreEqual ("59083369328877544512288628130304", (v >> 62).ToString (), "#62");
			Assert.AreEqual ("29541684664438772256144314065152", (v >> 63).ToString (), "#63");
			Assert.AreEqual ("14770842332219386128072157032576", (v >> 64).ToString (), "#64");
			Assert.AreEqual ("7385421166109693064036078516288", (v >> 65).ToString (), "#65");
			Assert.AreEqual ("3692710583054846532018039258144", (v >> 66).ToString (), "#66");
			Assert.AreEqual ("1846355291527423266009019629072", (v >> 67).ToString (), "#67");
			Assert.AreEqual ("923177645763711633004509814536", (v >> 68).ToString (), "#68");
			Assert.AreEqual ("461588822881855816502254907268", (v >> 69).ToString (), "#69");
		}

		[Test]
		public void Bug10887 ()
		{
			BigInteger b = 0;
			for(int i = 1; i <= 16; i++)
				b = b * 256 + i;
			BigInteger p = BigInteger.Pow (2, 32);
			Assert.AreEqual ("1339673755198158349044581307228491536", b.ToString (), "#1");
			Assert.AreEqual ("1339673755198158349044581307228491536", ((b << 32) / p).ToString (), "#2");
			Assert.AreEqual ("1339673755198158349044581307228491536", (b * p >> 32).ToString (), "#3");
		}

		[Test]
		public void DefaultCtorWorks ()
		{
			var a = new BigInteger ();
			Assert.AreEqual (BigInteger.One, ++a, "#1");

			a = new BigInteger ();
			Assert.AreEqual (BigInteger.MinusOne, --a, "#2");

			a = new BigInteger ();
			Assert.AreEqual (BigInteger.MinusOne, ~a, "#3");

			a = new BigInteger ();
			Assert.AreEqual ("0", a.ToString (), "#4");

			a = new BigInteger ();
#pragma warning disable 1718
			Assert.AreEqual (true, a == a, "#5");
#pragma warning restore

			a = new BigInteger ();
#pragma warning disable 1718
			Assert.AreEqual (false, a < a, "#6");
#pragma warning restore

			a = new BigInteger ();
			Assert.AreEqual (true, a < 10L, "#7");

			a = new BigInteger ();
			Assert.AreEqual (true, a.IsEven, "#8");

			a = new BigInteger ();
			Assert.AreEqual (0, (int)a, "#9");

			a = new BigInteger ();
			Assert.AreEqual (0, (uint)a, "#10");

			a = new BigInteger ();
			Assert.AreEqual (0, (ulong)a, "#11");

			a = new BigInteger ();
			Assert.AreEqual (true, a.Equals (a), "#12");

			a = new BigInteger ();
			Assert.AreEqual (a, BigInteger.Min (a, a), "#13");

			a = new BigInteger ();
			Assert.AreEqual (a, BigInteger.GreatestCommonDivisor (a, a), "#14");

			a = new BigInteger ();
			Assert.AreEqual (BigInteger.Zero.GetHashCode (), a.GetHashCode (), "#15");

			a = new BigInteger ();
			Assert.AreEqual (BigInteger.Zero, a, "#16");
		}

		[Test]
		public void Bug16526 ()
		{
			var x = BigInteger.Pow(2, 63);
			x *= -1;
			x -= 1;
			Assert.AreEqual ("-9223372036854775809", x.ToString (), "#1");
			try {
				x = (long)x;
				Assert.Fail ("#2 Must OVF");
			} catch (OverflowException) {
			}
		}
	}
}
