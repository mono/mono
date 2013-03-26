// DecimalTest.cs - NUnit Test Cases for the System.Decimal struct
//
// Authors:
//	Martin Weindel (martin.weindel@t-online.de)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Martin Weindel, 2001
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using NUnit.Framework;
using System;

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MonoTests.System
{
	internal struct ParseTest
	{
		public ParseTest (String str, bool exceptionFlag)
		{
			this.str = str;
			this.exceptionFlag = exceptionFlag;
			this.style = NumberStyles.Number;
			this.d = 0;
		}

		public ParseTest (String str, Decimal d)
		{
			this.str = str;
			this.exceptionFlag = false;
			this.style = NumberStyles.Number;
			this.d = d;
		}

		public ParseTest (String str, Decimal d, NumberStyles style)
		{
			this.str = str;
			this.exceptionFlag = false;
			this.style = style;
			this.d = d;
		}

		public String str;
		public Decimal d;
		public NumberStyles style;
		public bool exceptionFlag;
	}

	internal struct ToStringTest
	{
		public ToStringTest (String format, Decimal d, String str)
		{
			this.format = format;
			this.d = d;
			this.str = str;
		}

		public String format;
		public Decimal d;
		public String str;
	}

	[TestFixture]
	public class DecimalTest
	{
		private const int negativeBitValue = unchecked ((int) 0x80000000);
		private const int negativeScale4Value = unchecked ((int) 0x80040000);
		private int [] parts0 = { 0, 0, 0, 0 }; //Positive Zero.
		private int [] parts1 = { 1, 0, 0, 0 };
		private int [] parts2 = { 0, 1, 0, 0 };
		private int [] parts3 = { 0, 0, 1, 0 };
		private int [] parts4 = { 0, 0, 0, negativeBitValue }; // Negative zero.
		private int [] parts5 = { 1, 1, 1, 0 };
		private int [] partsMaxValue = { -1, -1, -1, 0 };
		private int [] partsMinValue = { -1, -1, -1, negativeBitValue };
		private int [] parts6 = { 1234, 5678, 8888, negativeScale4Value };
		private NumberFormatInfo NfiUser, NfiBroken;

		private CultureInfo old_culture;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			old_culture = Thread.CurrentThread.CurrentCulture;

			// Set culture to en-US and don't let the user override.
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

			NfiUser = new NumberFormatInfo ();
			NfiUser.CurrencyDecimalDigits = 3;
			NfiUser.CurrencyDecimalSeparator = ",";
			NfiUser.CurrencyGroupSeparator = "_";
			NfiUser.CurrencyGroupSizes = new int [] { 2, 1, 0 };
			NfiUser.CurrencyNegativePattern = 10;
			NfiUser.CurrencyPositivePattern = 3;
			NfiUser.CurrencySymbol = "XYZ";
			NfiUser.NumberDecimalSeparator = "##";
			NfiUser.NumberDecimalDigits = 4;
			NfiUser.NumberGroupSeparator = "__";
			NfiUser.NumberGroupSizes = new int [] { 2, 1 };
			NfiUser.PercentDecimalDigits = 1;
			NfiUser.PercentDecimalSeparator = ";";
			NfiUser.PercentGroupSeparator = "~";
			NfiUser.PercentGroupSizes = new int [] { 1 };
			NfiUser.PercentNegativePattern = 2;
			NfiUser.PercentPositivePattern = 2;
			NfiUser.PercentSymbol = "%%%";

			NfiBroken = new NumberFormatInfo ();
			NfiBroken.NumberDecimalSeparator = ".";
			NfiBroken.NumberGroupSeparator = ".";
			NfiBroken.CurrencyDecimalSeparator = ".";
			NfiBroken.CurrencyGroupSeparator = ".";
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			Thread.CurrentThread.CurrentCulture = old_culture;
		}

		[Test]
		public void TestToString ()
		{
			ToStringTest [] tab = {
				new ToStringTest ("F", 12.345678m, "12.35"),
				new ToStringTest ("F3", 12.345678m, "12.346"),
				new ToStringTest ("F0", 12.345678m, "12"),
				new ToStringTest ("F7", 12.345678m, "12.3456780"),
				new ToStringTest ("g", 12.345678m, "12.345678"),
				new ToStringTest ("E", 12.345678m, "1.234568E+001"),
				new ToStringTest ("E3", 12.345678m, "1.235E+001"),
				new ToStringTest ("E0", 12.345678m, "1E+001"),
				new ToStringTest ("e8", 12.345678m, "1.23456780e+001"),
				new ToStringTest ("F", 0.0012m, "0.00"),
				new ToStringTest ("F3", 0.0012m, "0.001"),
				new ToStringTest ("F0", 0.0012m, "0"),
				new ToStringTest ("F6", 0.0012m, "0.001200"),
				new ToStringTest ("e", 0.0012m, "1.200000e-003"),
				new ToStringTest ("E3", 0.0012m, "1.200E-003"),
				new ToStringTest ("E0", 0.0012m, "1E-003"),
				new ToStringTest ("E6", 0.0012m, "1.200000E-003"),
				new ToStringTest ("F4", -0.001234m, "-0.0012"),
				new ToStringTest ("E3", -0.001234m, "-1.234E-003"),
				new ToStringTest ("g", -0.000012m, "-0.000012"),
				new ToStringTest ("g0", -0.000012m, "-1.2e-05"),
				new ToStringTest ("g2", -0.000012m, "-1.2e-05"),
				new ToStringTest ("g20", -0.000012m, "-1.2e-05"),
				new ToStringTest ("g", -0.00012m, "-0.00012"),
				new ToStringTest ("g4", -0.00012m, "-0.00012"),
				new ToStringTest ("g7", -0.00012m, "-0.00012"),
				new ToStringTest ("g", -0.0001234m, "-0.0001234"),
				new ToStringTest ("g", -0.0012m, "-0.0012"),
				new ToStringTest ("g", -0.001234m, "-0.001234"),
				new ToStringTest ("g", -0.012m, "-0.012"),
				new ToStringTest ("g4", -0.012m, "-0.012"),
				new ToStringTest ("g", -0.12m, "-0.12"),
				new ToStringTest ("g", -1.2m, "-1.2"),
				new ToStringTest ("g4", -120m, "-120"),
				new ToStringTest ("g", -12.000m, "-12.000"),
				new ToStringTest ("g0", -12.000m, "-12"),
				new ToStringTest ("g6", -12.000m, "-12"),
				new ToStringTest ("g", -12m, "-12"),
				new ToStringTest ("g", -120m, "-120"),
				new ToStringTest ("g", -1200m, "-1200"),
				new ToStringTest ("g4", -1200m, "-1200"),
				new ToStringTest ("g", -1234m, "-1234"),
				new ToStringTest ("g", -12000m, "-12000"),
				new ToStringTest ("g4", -12000m, "-1.2e+04"),
				new ToStringTest ("g5", -12000m, "-12000"),
				new ToStringTest ("g", -12345m, "-12345"),
				new ToStringTest ("g", -120000m, "-120000"),
				new ToStringTest ("g4", -120000m, "-1.2e+05"),
				new ToStringTest ("g5", -120000m, "-1.2e+05"),
				new ToStringTest ("g6", -120000m, "-120000"),
				new ToStringTest ("g", -123456.1m, "-123456.1"),
				new ToStringTest ("g5", -123456.1m, "-1.2346e+05"),
				new ToStringTest ("g6", -123456.1m, "-123456"),
				new ToStringTest ("g", -1200000m, "-1200000"),
				new ToStringTest ("g", -123456.1m, "-123456.1"),
				new ToStringTest ("g", -123456.1m, "-123456.1"),
				new ToStringTest ("g", -1234567.1m, "-1234567.1"),
				new ToStringTest ("g", -12000000m, "-12000000"),
				new ToStringTest ("g", -12345678.1m, "-12345678.1"),
				new ToStringTest ("g", -12000000000000000000m, "-12000000000000000000"),
				new ToStringTest ("F", -123, "-123.00"),
				new ToStringTest ("F3", -123, "-123.000"),
				new ToStringTest ("F0", -123, "-123"),
				new ToStringTest ("E3", -123, "-1.230E+002"),
				new ToStringTest ("E0", -123, "-1E+002"),
				new ToStringTest ("E", -123, "-1.230000E+002"),
				new ToStringTest ("F3", Decimal.MinValue, "-79228162514264337593543950335.000"),
				new ToStringTest ("F", Decimal.MinValue, "-79228162514264337593543950335.00"),
				new ToStringTest ("F0", Decimal.MinValue, "-79228162514264337593543950335"),
				new ToStringTest ("E", Decimal.MinValue, "-7.922816E+028"),
				new ToStringTest ("E3", Decimal.MinValue, "-7.923E+028"),
				new ToStringTest ("E28", Decimal.MinValue, "-7.9228162514264337593543950335E+028"),
#if !TARGET_JVM // TargetJvmNotWorking
				new ToStringTest ("E30", Decimal.MinValue, "-7.922816251426433759354395033500E+028"),
#endif
				new ToStringTest ("E0", Decimal.MinValue, "-8E+028"),
				new ToStringTest ("N3", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335.000"),
				new ToStringTest ("N0", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335"),
				new ToStringTest ("N", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335.00"),
				new ToStringTest ("n3", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335.000"),
				new ToStringTest ("n0", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335"),
				new ToStringTest ("n", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335.00"),
				new ToStringTest ("C", 123456.7890m, NumberFormatInfo.InvariantInfo.CurrencySymbol + "123,456.79"),
				new ToStringTest ("C", -123456.7890m, "(" + NumberFormatInfo.InvariantInfo.CurrencySymbol + "123,456.79)"),
				new ToStringTest ("C3", 1123456.7890m, NumberFormatInfo.InvariantInfo.CurrencySymbol + "1,123,456.789"),
				new ToStringTest ("P", 123456.7891m, "12,345,678.91 %"),
				new ToStringTest ("P", -123456.7892m, "-12,345,678.92 %"),
				new ToStringTest ("P3", 1234.56789m, "123,456.789 %"),
			};

			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

			for (int i = 0; i < tab.Length; i++) {
				try {
					string s = tab [i].d.ToString (tab [i].format, nfi);
					Assert.AreEqual (tab [i].str, s, "A01 tab[" + i + "].format = '" + tab [i].format + "')");
				} catch (OverflowException) {
					Assert.Fail (tab [i].d.ToString (tab [i].format, nfi) + " (format = '" + tab [i].format + "'): unexpected exception !");
				} catch (NUnit.Framework.AssertionException e) {
					throw e;
				} catch (Exception e) {
					Assert.Fail ("Unexpected Exception when i = " + i + ". e = " + e);
				}
			}
		}

		[Test]
		public void TestCurrencyPattern ()
		{
			NumberFormatInfo nfi2 = (NumberFormatInfo) NfiUser.Clone ();
			Decimal d = -1234567.8976m;
			string [] ergCurrencyNegativePattern = new String [16] {
				"(XYZ1234_5_67,898)", "-XYZ1234_5_67,898", "XYZ-1234_5_67,898", "XYZ1234_5_67,898-",
				"(1234_5_67,898XYZ)", "-1234_5_67,898XYZ", "1234_5_67,898-XYZ", "1234_5_67,898XYZ-",
				"-1234_5_67,898 XYZ", "-XYZ 1234_5_67,898", "1234_5_67,898 XYZ-", "XYZ 1234_5_67,898-",
				"XYZ -1234_5_67,898", "1234_5_67,898- XYZ", "(XYZ 1234_5_67,898)", "(1234_5_67,898 XYZ)",
			};

			for (int i = 0; i < ergCurrencyNegativePattern.Length; i++) {
				nfi2.CurrencyNegativePattern = i;
				if (d.ToString ("C", nfi2) != ergCurrencyNegativePattern [i]) {
					Assert.Fail ("CurrencyNegativePattern #" + i + " failed: " +
						d.ToString ("C", nfi2) + " != " + ergCurrencyNegativePattern [i]);
				}
			}

			d = 1234567.8976m;
			string [] ergCurrencyPositivePattern = new String [4] {
				"XYZ1234_5_67,898", "1234_5_67,898XYZ", "XYZ 1234_5_67,898", "1234_5_67,898 XYZ",
			};

			for (int i = 0; i < ergCurrencyPositivePattern.Length; i++) {
				nfi2.CurrencyPositivePattern = i;
				if (d.ToString ("C", nfi2) != ergCurrencyPositivePattern [i]) {
					Assert.Fail ("CurrencyPositivePattern #" + i + " failed: " +
						d.ToString ("C", nfi2) + " != " + ergCurrencyPositivePattern [i]);
				}
			}
		}

		[Test]
		public void TestNumberNegativePattern ()
		{
			NumberFormatInfo nfi2 = (NumberFormatInfo) NfiUser.Clone ();
			Decimal d = -1234.89765m;
			string [] ergNumberNegativePattern = new String [5] {
				"(1__2__34##8977)", "-1__2__34##8977", "- 1__2__34##8977", "1__2__34##8977-", "1__2__34##8977 -",
			};

			for (int i = 0; i < ergNumberNegativePattern.Length; i++) {
				nfi2.NumberNegativePattern = i;
				Assert.AreEqual (ergNumberNegativePattern [i], d.ToString ("N", nfi2), "NumberNegativePattern #" + i);
			}
		}

		[Test]
		public void TestBrokenNFI ()
		{
			Assert.AreEqual (5.3m, decimal.Parse ("5.3", NumberStyles.Number, NfiBroken), "Parsing with broken NFI");
		}
		
		[Test]
		[Category ("TargetJvmNotWorking")]
		public void TestPercentPattern ()
		{
			NumberFormatInfo nfi2 = (NumberFormatInfo) NfiUser.Clone ();
			Decimal d = -1234.8976m;
			string [] ergPercentNegativePattern = new String [3] {
				"-1~2~3~4~8~9;8 %%%", "-1~2~3~4~8~9;8%%%", "-%%%1~2~3~4~8~9;8"
			};

			for (int i = 0; i < ergPercentNegativePattern.Length; i++) {
				nfi2.PercentNegativePattern = i;
				if (d.ToString ("P", nfi2) != ergPercentNegativePattern [i]) {
					Assert.Fail ("PercentNegativePattern #" + i + " failed: " +
						d.ToString ("P", nfi2) + " != " + ergPercentNegativePattern [i]);
				}
			}

			d = 1234.8976m;
			string [] ergPercentPositivePattern = new String [3] {
				"1~2~3~4~8~9;8 %%%", "1~2~3~4~8~9;8%%%", "%%%1~2~3~4~8~9;8"
			};

			for (int i = 0; i < ergPercentPositivePattern.Length; i++) {
				nfi2.PercentPositivePattern = i;
				if (d.ToString ("P", nfi2) != ergPercentPositivePattern [i]) {
					Assert.Fail ("PercentPositivePattern #" + i + " failed: " +
						d.ToString ("P", nfi2) + " != " + ergPercentPositivePattern [i]);
				}
			}
		}

		ParseTest [] tab = {
				new ParseTest("1.2345", 1.2345m),
				new ParseTest("1.2345\0", 1.2345m),
				new ParseTest("1.2345\0\0\0\0", 1.2345m),
				new ParseTest("-9876543210", -9876543210m),
				new ParseTest(NumberFormatInfo.InvariantInfo.CurrencySymbol 
					+ " (  79,228,162,514,264,337,593,543,950,335.000 ) ", 
					Decimal.MinValue, NumberStyles.Currency),
				new ParseTest("1.234567890e-10", (Decimal)1.234567890e-10, NumberStyles.Float),
				new ParseTest("1.234567890e-24", 1.2346e-24m, NumberStyles.Float),
				new ParseTest("  47896396.457983645462346E10  ", 478963964579836454.62346m, NumberStyles.Float),
				new ParseTest("-7922816251426433759354395033.250000000000001", -7922816251426433759354395033.3m),
				new ParseTest("-00000000000000795033.2500000000000000", -795033.25m),
				new ParseTest("-000000000000001922816251426433759354395033.300000000000000", -1922816251426433759354395033.3m),
				new ParseTest("-7922816251426433759354395033.150000000000", -7922816251426433759354395033.2m),
				new ParseTest("-7922816251426433759354395033.2400000000000", -7922816251426433759354395033.2m),
				new ParseTest("-7922816251426433759354395033.2600000000000", -7922816251426433759354395033.3m)
		};

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void TestParse ()
		{

			Decimal d;
			for (int i = 0; i < tab.Length; i++) {
				try {
					d = Decimal.Parse (tab [i].str, tab [i].style, NumberFormatInfo.InvariantInfo);
					if (tab [i].exceptionFlag) {
						Assert.Fail (tab [i].str + ": missing exception !");
					} else if (d != tab [i].d) {
						Assert.Fail (tab [i].str + " != " + d);
					}
				} catch (OverflowException) {
					if (!tab [i].exceptionFlag) {
						Assert.Fail (tab [i].str + ": unexpected exception !");
					}
				}
			}

			try {
				d = Decimal.Parse (null);
				Assert.Fail ("Expected ArgumentNullException");
			} catch (ArgumentNullException) {
				//ok
			}

			try {
				d = Decimal.Parse ("123nx");
				Assert.Fail ("Expected FormatException");
			} catch (FormatException) {
				//ok
			}

			try {
				d = Decimal.Parse ("79228162514264337593543950336");
				Assert.Fail ("Expected OverflowException" + d);
			} catch (OverflowException) {
				//ok
			}

			try {
				d = Decimal.Parse ("5\05");
				Assert.Fail ("Expected FormatException" + d);
			} catch (FormatException) {
				//ok
			}
		}

		[Test]
		public void TestConstants ()
		{
			Assert.AreEqual (0m, Decimal.Zero, "Zero");
			Assert.AreEqual (1m, Decimal.One, "One");
			Assert.AreEqual (-1m, Decimal.MinusOne, "MinusOne");
			Assert.AreEqual (79228162514264337593543950335m, Decimal.MaxValue, "MaxValue");
			Assert.AreEqual (-79228162514264337593543950335m, Decimal.MinValue, "MinValue");
			Assert.IsTrue (-1m == Decimal.MinusOne, "MinusOne 2");
		}

		[Test]
		public void TestConstructInt32 ()
		{
			decimal [] dtab = { 0m, 1m, -1m, 123456m, -1234567m };
			int [] itab = { 0, 1, -1, 123456, -1234567 };

			Decimal d;

			for (int i = 0; i < dtab.GetLength (0); i++) {
				d = new Decimal (itab [i]);
				if ((decimal) d != dtab [i]) {
					Assert.Fail ("Int32 -> Decimal: " + itab [i] + " != " + d);
				} else {
					int n = (int) d;
					if (n != itab [i]) {
						Assert.Fail ("Decimal -> Int32: " + d + " != " + itab [i]);
					}
				}
			}

			d = new Decimal (Int32.MaxValue);
			Assert.IsTrue ((int) d == Int32.MaxValue);

			d = new Decimal (Int32.MinValue);
			Assert.IsTrue ((int) d == Int32.MinValue);
		}

		[Test]
		public void TestConstructUInt32 ()
		{
			decimal [] dtab = { 0m, 1m, 123456m, 123456789m };
			uint [] itab = { 0, 1, 123456, 123456789 };

			Decimal d;

			for (int i = 0; i < dtab.GetLength (0); i++) {
				d = new Decimal (itab [i]);
				if ((decimal) d != dtab [i]) {
					Assert.Fail ("UInt32 -> Decimal: " + itab [i] + " != " + d);
				} else {
					uint n = (uint) d;
					if (n != itab [i]) {
						Assert.Fail ("Decimal -> UInt32: " + d + " != " + itab [i]);
					}
				}
			}

			d = new Decimal (UInt32.MaxValue);
			Assert.IsTrue ((uint) d == UInt32.MaxValue);

			d = new Decimal (UInt32.MinValue);
			Assert.IsTrue ((uint) d == UInt32.MinValue);
		}

		[Test]
		public void TestConstructInt64 ()
		{
			decimal [] dtab = { 0m, 1m, -1m, 9876543m, -9876543210m, 12345678987654321m };
			long [] itab = { 0, 1, -1, 9876543, -9876543210L, 12345678987654321L };

			Decimal d;

			for (int i = 0; i < dtab.GetLength (0); i++) {
				d = new Decimal (itab [i]);
				if ((decimal) d != dtab [i]) {
					Assert.Fail ("Int64 -> Decimal: " + itab [i] + " != " + d);
				} else {
					long n = (long) d;
					if (n != itab [i]) {
						Assert.Fail ("Decimal -> Int64: " + d + " != " + itab [i]);
					}
				}
			}

			d = new Decimal (Int64.MaxValue);
			Assert.IsTrue ((long) d == Int64.MaxValue);

			d = new Decimal (Int64.MinValue);
			Assert.IsTrue ((long) d == Int64.MinValue);
		}

		[Test]
		public void TestConstructUInt64 ()
		{
			decimal [] dtab = { 0m, 1m, 987654321m, 123456789876543210m };
			ulong [] itab = { 0, 1, 987654321, 123456789876543210L };

			Decimal d;

			for (int i = 0; i < dtab.GetLength (0); i++) {
				d = new Decimal (itab [i]);
				if ((decimal) d != dtab [i]) {
					Assert.Fail ("UInt64 -> Decimal: " + itab [i] + " != " + d);
				} else {
					ulong n = (ulong) d;
					if (n != itab [i]) {
						Assert.Fail ("Decimal -> UInt64: " + d + " != " + itab [i]);
					}
				}
			}

			d = new Decimal (UInt64.MaxValue);
			Assert.IsTrue ((ulong) d == UInt64.MaxValue);

			d = new Decimal (UInt64.MinValue);
			Assert.IsTrue ((ulong) d == UInt64.MinValue);
		}

		[Test]
		public void TestConstructSingle ()
		{
			Decimal d;

			d = new Decimal (-1.2345678f);
			Assert.AreEqual (-1.234568m, (decimal) d, "A#01");

			d = 3;
			Assert.AreEqual (3.0f, (float) d, "A#02");

			d = new Decimal (0.0f);
			Assert.AreEqual (0m, (decimal) d, "A#03");
			Assert.AreEqual (0.0f, (float) d, "A#04");

			d = new Decimal (1.0f);
			Assert.AreEqual (1m, (decimal) d, "A#05");
			Assert.AreEqual (1.0f, (float) d, "A#06");

			d = new Decimal (-1.2345678f);
			Assert.AreEqual (-1.234568m, (decimal) d, "A#07");
			Assert.AreEqual (-1.234568f, (float) d, "A#08");

			d = new Decimal (1.2345673f);
			Assert.AreEqual (1.234567m, (decimal) d, "A#09");

			d = new Decimal (1.2345673e7f);
			Assert.AreEqual (12345670m, (decimal) d, "A#10");

			d = new Decimal (1.2345673e-17f);
			Assert.AreEqual (0.00000000000000001234567m, (decimal) d, "A#11");
			Assert.AreEqual (1.234567e-17f, (float) d, "A#12");

			// test exceptions
			try {
				d = new Decimal (Single.MaxValue);
				Assert.Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (Single.NaN);
				Assert.Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (Single.PositiveInfinity);
				Assert.Fail ();
			} catch (OverflowException) {
			}
		}

		[Test]
		public void TestConstructSingleRounding_NowWorking ()
		{
			decimal d;

			d = new Decimal (1765.23454f);
			Assert.AreEqual (1765.234m, d, "failed banker's rule rounding test 2");

			d = new Decimal (0.00017652356f);
			Assert.AreEqual (0.0001765236m, d, "06");

			d = new Decimal (0.000176523554f);
			Assert.AreEqual (0.0001765236m, d, "failed banker's rule rounding test 3");

			d = new Decimal (0.00017652354f);
			Assert.AreEqual (0.0001765235m, d, "08");

			d = new Decimal (0.00017652346f);
			Assert.AreEqual (0.0001765235m, d, "09");

			d = new Decimal (0.000176523454f);
			Assert.AreEqual (0.0001765234m, d, "failed banker's rule rounding test 4");

			d = new Decimal (0.00017652344f);
			Assert.AreEqual (0.0001765234m, d, "11");
		}

		public void TestConstructSingleRounding ()
		{
			decimal d;

			d = new Decimal (1765.2356f);
			Assert.IsTrue (d == 1765.236m, "01");

			d = new Decimal (1765.23554f);
			Assert.IsTrue (d == 1765.236m, "failed banker's rule rounding test 1");

			d = new Decimal (1765.2354f);
			Assert.IsTrue (d == 1765.235m, "03");

			d = new Decimal (1765.2346f);
			Assert.IsTrue (d == 1765.235m, "04");

			d = new Decimal (1765.2344f);
			Assert.IsTrue (d == 1765.234m, "05");

			d = new Decimal (3.7652356e10f);
			Assert.IsTrue (d == 37652360000m, "12");

			d = new Decimal (3.7652356e20f);
			Assert.IsTrue (d == 376523600000000000000m, "13");

			d = new Decimal (3.76523554e20f);
			Assert.IsTrue (d == 376523600000000000000m, "failed banker's rule rounding test 5");

			d = new Decimal (3.7652352e20f);
			Assert.IsTrue (d == 376523500000000000000m, "15");

			d = new Decimal (3.7652348e20f);
			Assert.IsTrue (d == 376523500000000000000m, "16");

			d = new Decimal (3.76523454e20f);
			Assert.IsTrue (d == 376523400000000000000m, "failed banker's rule rounding test 6");

			d = new Decimal (3.7652342e20f);
			Assert.IsTrue (d == 376523400000000000000m, "18");
		}

		[Test]
		[SetCulture("en-US")]
		public void TestConstructDouble ()
		{
			Decimal d;

			d = new Decimal (0.0);
			Assert.IsTrue ((decimal) d == 0m);

			d = new Decimal (1.0);
			Assert.IsTrue ((decimal) d == 1m);
			Assert.IsTrue (1.0 == (double) d);

			d = new Decimal (-1.2345678901234);
			Assert.IsTrue ((decimal) d == -1.2345678901234m);
			Assert.IsTrue (-1.2345678901234 == (double) d);

			d = new Decimal (1.2345678901234);
			Assert.IsTrue ((decimal) d == 1.2345678901234m);

			d = new Decimal (1.2345678901234e8);
			Assert.IsTrue ((decimal) d == 123456789.01234m);
			Assert.IsTrue (1.2345678901234e8 == (double) d);

			d = new Decimal (1.2345678901234e16);
			Assert.IsTrue ((decimal) d == 12345678901234000m);
			Assert.IsTrue (1.2345678901234e16 == (double) d);

			d = new Decimal (1.2345678901234e24);
			Assert.IsTrue ((decimal) d == 1234567890123400000000000m);
			Assert.IsTrue (1.2345678901234e24 == (double) d);

			d = new Decimal (1.2345678901234e28);
			Assert.IsTrue ((decimal) d == 1.2345678901234e28m);
			Assert.IsTrue (1.2345678901234e28 == (double) d);

			d = new Decimal (7.2345678901234e28);
			Assert.IsTrue ((decimal) d == 7.2345678901234e28m);
			Assert.IsTrue (new Decimal ((double) d) == d);

			d = new Decimal (1.2345678901234e-8);
			Assert.IsTrue ((decimal) d == 1.2345678901234e-8m);

			d = new Decimal (1.2345678901234e-14);
			Assert.IsTrue ((decimal) d == 1.2345678901234e-14m);
			Assert.IsTrue (1.2345678901234e-14 == (double) d);

			d = new Decimal (1.2342278901234e-25);
			Assert.AreEqual (d, 1.234e-25m, "A10");

			//
			// Make sure that 0.6 is turned into
			// the 96 bit value 6 with the magnitude set to
			//
			double mydouble = 0.6;
			d = new Decimal (mydouble);
			int [] bits = Decimal.GetBits (d);
			Assert.AreEqual (bits [0], 6, "f1");
			Assert.AreEqual (bits [1], 0, "f2");
			Assert.AreEqual (bits [2], 0, "f3");
			Assert.AreEqual (bits [3], 65536, "f4");

			//
			// Make sure that we properly parse this value
			// this in particular exposes a bug in the
			// unmanaged version which rounds to 1234 instead
			// of 1235, here to make sure we do not regress
			// on the future.
			//
			mydouble = 1.2345679329684657e-25;
			d = new Decimal (mydouble);
			Assert.AreEqual (d.ToString (), "0.0000000000000000000000001235", "f5");
			
			// test exceptions
			try {
				d = new Decimal (8e28);
				Assert.Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (8e48);
				Assert.Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (Double.NaN);
				Assert.Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (Double.PositiveInfinity);
				Assert.Fail ();
			} catch (OverflowException) {
			}
		}

		[Test]
		public void TestConstructDoubleRound ()
		{
			decimal d;
			int TestNum = 1;

			try {
				d = new Decimal (1765.231234567857);
				Assert.AreEqual (1765.23123456786m, d, "A01");

				TestNum++;
				d = new Decimal (1765.2312345678554);
				Assert.AreEqual (1765.23123456786m, d, "A02, failed banker's rule rounding test 1");
				Assert.AreEqual (1765.23123456786, (double) d, "A03");

				TestNum++;
				d = new Decimal (1765.231234567853);
				Assert.IsTrue (d == 1765.23123456785m);

				TestNum++;
				d = new Decimal (1765.231234567847);
				Assert.IsTrue (d == 1765.23123456785m);

				TestNum++;
				d = new Decimal (1765.231234567843);
				Assert.IsTrue (d == 1765.23123456784m);

				TestNum++;
				d = new Decimal (1.765231234567857e-9);
				Assert.IsTrue (d == 1.76523123456786e-9m);

				TestNum++;
				d = new Decimal (1.7652312345678554e-9);
				Assert.IsTrue (d == 1.76523123456786e-9m, "failed banker's rule rounding test 3");

				TestNum++;
				d = new Decimal (1.765231234567853e-9);
				Assert.IsTrue (d == 1.76523123456785e-9m);

				TestNum++;
				d = new Decimal (1.765231234567857e+24);
				Assert.IsTrue (d == 1.76523123456786e+24m);

				TestNum++;
				d = new Decimal (1.7652312345678554e+24);
				Assert.IsTrue (d == 1.76523123456786e+24m, "failed banker's rule rounding test 4");

				TestNum++;
				d = new Decimal (1.765231234567853e+24);
				Assert.IsTrue (d == 1.76523123456785e+24m);

				TestNum++;
				d = new Decimal (1765.2312345678454);
				Assert.IsTrue (d == 1765.23123456785m);
			} catch (Exception e) {
				Assert.Fail ("At TestNum = " + TestNum + " unexpected exception. e = " + e);
			}
		}

		[Test]
		public void TestNegate ()
		{
			decimal d;

			d = new Decimal (12345678);
			Assert.IsTrue ((decimal) Decimal.Negate (d) == -12345678m);
		}

		[Test]
		public void TestPartConstruct ()
		{
			decimal d;

			d = new Decimal (parts0);
			Assert.IsTrue (d == 0);

			d = new Decimal (parts1);
			Assert.IsTrue (d == 1);

			d = new Decimal (parts2);
			Assert.IsTrue (d == 4294967296m);

			d = new Decimal (parts3);
			Assert.IsTrue (d == 18446744073709551616m);

			d = new Decimal (parts4);
			Assert.IsTrue (d == 0m);

			d = new Decimal (parts5);
			Assert.IsTrue (d == 18446744078004518913m);

			d = new Decimal (partsMaxValue);
			Assert.IsTrue (d == Decimal.MaxValue);

			d = new Decimal (partsMinValue);
			Assert.IsTrue (d == Decimal.MinValue);

			d = new Decimal (parts6);
			int [] erg = Decimal.GetBits (d);
			for (int i = 0; i < 4; i++) {
				Assert.IsTrue (erg [i] == parts6 [i]);
			}
		}

		[Test]
		public void TestFloorTruncate ()
		{
			decimal [,] dtab = {
				{0m, 0m, 0m}, {1m, 1m, 1m}, {-1m, -1m, -1m}, {1.1m, 1m, 1m}, 
				{-1.000000000001m, -2m, -1m}, {12345.67890m,12345m,12345m},
				{-123456789012345.67890m, -123456789012346m, -123456789012345m},
				{Decimal.MaxValue, Decimal.MaxValue, Decimal.MaxValue},
				{Decimal.MinValue, Decimal.MinValue, Decimal.MinValue},
				{6.999999999m, 6m, 6m}, {-6.999999999m, -7m, -6m}, 
				{0.00001m, 0m, 0m}, {-0.00001m, -1m, 0m}
			};

			decimal d;

			for (int i = 0; i < dtab.GetLength (0); i++) {
				d = Decimal.Floor (dtab [i, 0]);
				if (d != dtab [i, 1]) {
					Assert.Fail ("Floor: Floor(" + dtab [i, 0] + ") != " + d);
				}
				d = Decimal.Truncate (dtab [i, 0]);
				if (d != dtab [i, 2]) {
					Assert.Fail ("Truncate: Truncate(" + dtab [i, 0] + ") != " + d);
				}
			}
		}

		[Test]
		public void Truncate ()
		{
			decimal dd = 249.9m;
			decimal dt = Decimal.Truncate (dd);
			Assert.AreEqual (249.9m, dd, "Original");
			Assert.AreEqual (249m, dt, "Truncate");
			Assert.AreEqual (249, (byte) dd, "Cast-Byte");
			Assert.AreEqual (249, (char) dd, "Cast-Char");
			Assert.AreEqual (249, (short) dd, "Cast-Int16");
			Assert.AreEqual (249, (ushort) dd, "Cast-UInt16");
			Assert.AreEqual (249, (int) dd, "Cast-Int32");
			Assert.AreEqual (249, (uint) dd, "Cast-UInt32");
			Assert.AreEqual (249, (long) dd, "Cast-Int64");
			Assert.AreEqual (249, (ulong) dd, "Cast-UInt64");
		}

		[Test]
		public void TestRound ()
		{
			decimal [,] dtab = { 
				{1m, 0, 1m}, {1.234567890m, 1, 1.2m}, 
				{1.234567890m, 2, 1.23m}, {1.23450000001m, 3, 1.235m}, 
				{1.2355m, 3, 1.236m}, 
				{1.234567890m, 4, 1.2346m}, {1.23567890m, 2, 1.24m}, 
				{47893764694.4578563236436621m, 7, 47893764694.4578563m},
				{-47893764694.4578563236436621m, 9, -47893764694.457856324m},
				{-47893764694.4578m, 5, -47893764694.4578m}
			};

			decimal d;

			for (int i = 0; i < dtab.GetLength (0); i++) {
				d = Decimal.Round (dtab [i, 0], (int) dtab [i, 1]);
				if (d != dtab [i, 2]) {
					Assert.Fail ("Round: Round(" + dtab [i, 0] + "," + (int) dtab [i, 1] + ") != " + d);
				}
			}
		}

		[Test]
		public void TestRoundFailures ()
		{
			decimal [,] dtab = {
				{1.2345m, 3, 1.234m} 
			};

			decimal d;

			for (int i = 0; i < dtab.GetLength (0); i++) {
				d = Decimal.Round (dtab [i, 0], (int) dtab [i, 1]);
				if (d != dtab [i, 2]) {
					Assert.Fail ("FailRound: Round(" + dtab [i, 0] + "," + (int) dtab [i, 1] + ") != " + d);
				}
			}
		}

		[Test]
		public void ParseInt64 ()
		{
			long max = Int64.MaxValue;
			Decimal dmax = Decimal.Parse (max.ToString ());
			Assert.AreEqual (Int64.MaxValue, Decimal.ToInt64 (dmax), "Int64.MaxValue");

			long min = Int64.MinValue;
			Decimal dmin = Decimal.Parse (min.ToString ());
			Assert.AreEqual (Int64.MinValue, Decimal.ToInt64 (dmin), "Int64.MinValue");

			dmax += 1.1m;
			dmax = Decimal.Parse (dmax.ToString ());
			Assert.AreEqual (Int64.MaxValue, Decimal.ToInt64 (dmax - 1.1m), "Int64.MaxValue+1.1");

			dmin -= 1.1m;
			dmin = Decimal.Parse (dmin.ToString ());
			Assert.AreEqual (Int64.MinValue, Decimal.ToInt64 (dmin + 1.1m), "Int64.MinValue-1.1");
		}

		[Test]
		public void ToByte ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254, Decimal.ToByte (d), "Decimal.ToByte");
			Assert.AreEqual (255, Convert.ToByte (d), "Convert.ToByte");
			Assert.AreEqual (255, (d as IConvertible).ToByte (null), "IConvertible.ToByte");
		}

		[Test]
		public void ToSByte ()
		{
			Decimal d = 126.9m;
			Assert.AreEqual (126, Decimal.ToSByte (d), "Decimal.ToSByte");
			Assert.AreEqual (127, Convert.ToSByte (d), "Convert.ToSByte");
			Assert.AreEqual (127, (d as IConvertible).ToSByte (null), "IConvertible.ToSByte");
			d = -d;
			Assert.AreEqual (-126, Decimal.ToSByte (d), "-Decimal.ToSByte");
			Assert.AreEqual (-127, Convert.ToSByte (d), "-Convert.ToSByte");
			Assert.AreEqual (-127, (d as IConvertible).ToSByte (null), "-IConvertible.ToSByte");
		}

		[Test]
		public void ToInt16 ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254, Decimal.ToInt16 (d), "Decimal.ToInt16");
			Assert.AreEqual (255, Convert.ToInt16 (d), "Convert.ToInt16");
			Assert.AreEqual (255, (d as IConvertible).ToInt16 (null), "IConvertible.ToInt16");
			d = -d;
			Assert.AreEqual (-254, Decimal.ToInt16 (d), "-Decimal.ToInt16");
			Assert.AreEqual (-255, Convert.ToInt16 (d), "-Convert.ToInt16");
			Assert.AreEqual (-255, (d as IConvertible).ToInt16 (null), "-IConvertible.ToInt16");
		}

		[Test]
		public void ToUInt16 ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254, Decimal.ToUInt16 (d), "Decimal.ToUInt16");
			Assert.AreEqual (255, Convert.ToUInt16 (d), "Convert.ToUInt16");
			Assert.AreEqual (255, (d as IConvertible).ToUInt16 (null), "IConvertible.ToUInt16");
		}

		[Test]
		public void ToInt32 ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254, Decimal.ToInt32 (d), "Decimal.ToInt32");
			Assert.AreEqual (255, Convert.ToInt32 (d), "Convert.ToInt32");
			Assert.AreEqual (255, (d as IConvertible).ToInt32 (null), "IConvertible.ToInt32");
			d = -d;
			Assert.AreEqual (-254, Decimal.ToInt32 (d), "-Decimal.ToInt32");
			Assert.AreEqual (-255, Convert.ToInt32 (d), "-Convert.ToInt32");
			Assert.AreEqual (-255, (d as IConvertible).ToInt32 (null), "-IConvertible.ToInt32");
		}

		[Test]
		public void ToUInt32 ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254, Decimal.ToUInt32 (d), "Decimal.ToUInt32");
			Assert.AreEqual (255, Convert.ToUInt32 (d), "Convert.ToUInt32");
			Assert.AreEqual (255, (d as IConvertible).ToUInt32 (null), "IConvertible.ToUInt32");
		}

		[Test]
		public void ToInt64 ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254, Decimal.ToInt64 (d), "Decimal.ToInt64");
			Assert.AreEqual (255, Convert.ToInt64 (d), "Convert.ToInt64");
			Assert.AreEqual (255, (d as IConvertible).ToInt64 (null), "IConvertible.ToInt64");
			d = -d;
			Assert.AreEqual (-254, Decimal.ToInt64 (d), "-Decimal.ToInt64");
			Assert.AreEqual (-255, Convert.ToInt64 (d), "-Convert.ToInt64");
			Assert.AreEqual (-255, (d as IConvertible).ToInt64 (null), "-IConvertible.ToInt64");
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt64_TooBig ()
		{
			Decimal d = (Decimal) Int64.MaxValue;
			d += 1.1m;
			long value = Decimal.ToInt64 (d);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt64_TooSmall ()
		{
			Decimal d = (Decimal) Int64.MinValue;
			d -= 1.1m;
			long value = Decimal.ToInt64 (d);
		}

		[Test]
		public void ToUInt64 ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254, Decimal.ToUInt64 (d), "Decimal.ToUInt64");
			Assert.AreEqual (255, Convert.ToUInt64 (d), "Convert.ToUInt64");
			Assert.AreEqual (255, (d as IConvertible).ToUInt64 (null), "IConvertible.ToUInt64");
		}

		[Test]
		public void ToSingle ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254.9f, Decimal.ToSingle (d), "Decimal.ToSingle");
			Assert.AreEqual (254.9f, Convert.ToSingle (d), "Convert.ToSingle");
			Assert.AreEqual (254.9f, (d as IConvertible).ToSingle (null), "IConvertible.ToSingle");
			d = -d;
			Assert.AreEqual (-254.9f, Decimal.ToSingle (d), "-Decimal.ToSingle");
			Assert.AreEqual (-254.9f, Convert.ToSingle (d), "-Convert.ToSingle");
			Assert.AreEqual (-254.9f, (d as IConvertible).ToSingle (null), "-IConvertible.ToSingle");
		}

		[Test]
		public void ToDouble ()
		{
			Decimal d = 254.9m;
			Assert.AreEqual (254.9d, Decimal.ToDouble (d), "Decimal.ToDouble");
			Assert.AreEqual (254.9d, Convert.ToDouble (d), "Convert.ToDouble");
			Assert.AreEqual (254.9d, (d as IConvertible).ToDouble (null), "IConvertible.ToDouble");
			d = -d;
			Assert.AreEqual (-254.9d, Decimal.ToDouble (d), "-Decimal.ToDouble");
			Assert.AreEqual (-254.9d, Convert.ToDouble (d), "-Convert.ToDouble");
			Assert.AreEqual (-254.9d, (d as IConvertible).ToDouble (null), "-IConvertible.ToDouble");
		}

		[Test]
		[SetCulture("en-US")]
		public void ToString_Defaults ()
		{
			Decimal d = 254.9m;
			// everything defaults to "G"
			string def = d.ToString ("G");
			Assert.AreEqual (def, d.ToString (), "ToString()");
			Assert.AreEqual (def, d.ToString ((IFormatProvider) null), "ToString((IFormatProvider)null)");
			Assert.AreEqual (def, d.ToString ((string) null), "ToString((string)null)");
			Assert.AreEqual (def, d.ToString (String.Empty), "ToString(empty)");
			Assert.AreEqual (def, d.ToString (null, null), "ToString(null,null)");
			Assert.AreEqual (def, d.ToString (String.Empty, null), "ToString(empty,null)");

			Assert.AreEqual ("254.9", def, "ToString()");
		}

		[Test]
		public void CastTruncRounding ()
		{
			// casting truncs decimal value (not normal nor banker's rounding)
			Assert.AreEqual (254, (long) (254.9m), "254.9==254");
			Assert.AreEqual (-254, (long) (-254.9m), "-254.9=-254");
			Assert.AreEqual (255, (long) (255.9m), "255.9==256");
			Assert.AreEqual (-255, (long) (-255.9m), "-255.9=-256");
		}

		[Test]
		public void ParseFractions ()
		{
			decimal d1 = Decimal.Parse ("0.523456789012345467890123456789", CultureInfo.InvariantCulture);
			Assert.AreEqual (0.5234567890123454678901234568m, d1, "f1");
			decimal d2 = Decimal.Parse ("0.49214206543486529434634231456", CultureInfo.InvariantCulture);
			Assert.AreEqual (0.4921420654348652943463423146m, d2, "f2");
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void Parse_Int64_Overflow ()
		{
			// Int64.MaxValue + 1 + small fraction to allow 30 digits
			// 123456789012345678901234567890
			decimal d = Decimal.Parse ("9223372036854775808.0000000009", CultureInfo.InvariantCulture);
			long l = (long) d;
		}
/* Not yet fixed
		[Test]
		public void ParseEmptyNumberGroupSeparator ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			try {
				var nf = new NumberFormatInfo ();
				nf.NumberDecimalSeparator = ".";
				nf.NumberGroupSeparator = "";
				decimal d = decimal.Parse ("4.5", nf);
				Assert.AreEqual (4.5, d);
			} finally {
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}
*/

		[Test]
		public void ParseCultureSeparator ()
		{
			Assert.AreEqual (2.2m, decimal.Parse ("2.2", new CultureInfo("es-MX")));
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void TryParse ()
		{
			Decimal r;
		
			// These should return false
			Assert.AreEqual (false, Decimal.TryParse ("79228162514264337593543950336", out r));
			Assert.AreEqual (false, Decimal.TryParse ("123nx", NumberStyles.Number, CultureInfo.InvariantCulture, out r));
			Assert.AreEqual (false, Decimal.TryParse (null, NumberStyles.Number, CultureInfo.InvariantCulture, out r));

			// These should pass
			for (int i = 0; i < tab.Length; i++) {
				Assert.AreEqual (!tab [i].exceptionFlag,
					Decimal.TryParse (tab [i].str, tab [i].style,
					NumberFormatInfo.InvariantInfo, out r));
			}
		}

		[Test]
		[ExpectedException (typeof (DivideByZeroException))]
		public void Remainder_ByZero ()
		{
			Decimal.Remainder (254.9m, 0m);
		}

		[Test]
		public void Remainder ()
		{
			decimal p1 = 254.9m;
			decimal p2 = 12.1m;
			decimal n1 = -254.9m;
			decimal n2 = -12.1m;

			Assert.AreEqual (0.8m, Decimal.Remainder (p1, p2), "254.9 % 12.1");
			Assert.AreEqual (-0.8m, Decimal.Remainder (n1, p2), "-254.9 % 12.1");
			Assert.AreEqual (0.8m, Decimal.Remainder (p1, n2), "254.9 % -12.1");
			Assert.AreEqual (-0.8m, Decimal.Remainder (n1, n2), "-254.9 % -12.1");

			Assert.AreEqual (12.1m, Decimal.Remainder (p2, p1), "12.1 % 254.9");
			Assert.AreEqual (-12.1m, Decimal.Remainder (n2, p1), "-12.1 % 254.9");
			Assert.AreEqual (12.1m, Decimal.Remainder (p2, n1), "12.1 % -254.9");
			Assert.AreEqual (-12.1m, Decimal.Remainder (n2, n1), "-12.1 % -254.9");

			Assert.AreEqual (0.0m, Decimal.Remainder (p1, p1), "12.1 % 12.1");
			Assert.AreEqual (0.0m, Decimal.Remainder (n1, p1), "-12.1 % 12.1");
			Assert.AreEqual (0.0m, Decimal.Remainder (p1, n1), "12.1 % -12.1");
			Assert.AreEqual (0.0m, Decimal.Remainder (n1, n1), "-12.1 % -12.1");
		}

		[Test]
		public void Remainder2 ()
		{
			decimal a = 20.0M;
			decimal b = 10.0M;
			decimal c = 10.00M;

			Assert.AreEqual (0.00m, a % c, "20.0M % 10.00M");
		
		}

		[Test]
		[ExpectedException (typeof (DivideByZeroException))]
		public void Divide_ByZero ()
		{
			Decimal.Divide (254.9m, 0m);
		}

		[Test]
		public void Divide ()
		{
			decimal p1 = 254.9m;
			decimal p2 = 12.1m;
			decimal n1 = -254.9m;
			decimal n2 = -12.1m;

			decimal c1 = 21.066115702479338842975206612m;
			decimal c2 = 0.0474695959199686151431934092m;

			Assert.AreEqual (c1, Decimal.Divide (p1, p2), "254.9 / 12.1");
			Assert.AreEqual (-c1, Decimal.Divide (n1, p2), "-254.9 / 12.1");
			Assert.AreEqual (-c1, Decimal.Divide (p1, n2), "254.9 / -12.1");
			Assert.AreEqual (c1, Decimal.Divide (n1, n2), "-254.9 / -12.1");

			Assert.AreEqual (c2, Decimal.Divide (p2, p1), "12.1 / 254.9");
			Assert.AreEqual (-c2, Decimal.Divide (n2, p1), "-12.1 / 254.9");
			Assert.AreEqual (-c2, Decimal.Divide (p2, n1), "12.1 / -254.9");
			Assert.AreEqual (c2, Decimal.Divide (n2, n1), "-12.1 / -254.9");

			Assert.AreEqual (1, Decimal.Divide (p1, p1), "12.1 / 12.1");
			Assert.AreEqual (-1, Decimal.Divide (n1, p1), "-12.1 / 12.1");
			Assert.AreEqual (-1, Decimal.Divide (p1, n1), "12.1 / -12.1");
			Assert.AreEqual (1, Decimal.Divide (n1, n1), "-12.1 / -12.1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Round_InvalidDecimals_Negative ()
		{
			Decimal.Round (254.9m, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Round_InvalidDecimals_TooHigh ()
		{
			Decimal.Round (254.9m, 29);
		}

		[Test]
		public void Round_OddValue ()
		{
			decimal five = 5.5555555555555555555555555555m;
			Assert.AreEqual (6, Decimal.Round (five, 0), "5,5_,00");
			Assert.AreEqual (5.6m, Decimal.Round (five, 1), "5,5_,01");
			Assert.AreEqual (5.56m, Decimal.Round (five, 2), "5,5_,02");
			Assert.AreEqual (5.556m, Decimal.Round (five, 3), "5,5_,03");
			Assert.AreEqual (5.5556m, Decimal.Round (five, 4), "5,5_,04");
			Assert.AreEqual (5.55556m, Decimal.Round (five, 5), "5,5_,05");
			Assert.AreEqual (5.555556m, Decimal.Round (five, 6), "5,5_,06");
			Assert.AreEqual (5.5555556m, Decimal.Round (five, 7), "5,5_,07");
			Assert.AreEqual (5.55555556m, Decimal.Round (five, 8), "5,5_,08");
			Assert.AreEqual (5.555555556m, Decimal.Round (five, 9), "5,5_,09");
			Assert.AreEqual (5.5555555556m, Decimal.Round (five, 10), "5,5_,10");
			Assert.AreEqual (5.55555555556m, Decimal.Round (five, 11), "5,5_,11");
			Assert.AreEqual (5.555555555556m, Decimal.Round (five, 12), "5,5_,12");
			Assert.AreEqual (5.5555555555556m, Decimal.Round (five, 13), "5,5_,13");
			Assert.AreEqual (5.55555555555556m, Decimal.Round (five, 14), "5,5_,14");
			Assert.AreEqual (5.555555555555556m, Decimal.Round (five, 15), "5,5_,15");
			Assert.AreEqual (5.5555555555555556m, Decimal.Round (five, 16), "5,5_,16");
			Assert.AreEqual (5.55555555555555556m, Decimal.Round (five, 17), "5,5_,17");
			Assert.AreEqual (5.555555555555555556m, Decimal.Round (five, 18), "5,5_,18");
			Assert.AreEqual (5.5555555555555555556m, Decimal.Round (five, 19), "5,5_,19");
			Assert.AreEqual (5.55555555555555555556m, Decimal.Round (five, 20), "5,5_,20");
			Assert.AreEqual (5.555555555555555555556m, Decimal.Round (five, 21), "5,5_,21");
			Assert.AreEqual (5.5555555555555555555556m, Decimal.Round (five, 22), "5,5_,22");
			Assert.AreEqual (5.55555555555555555555556m, Decimal.Round (five, 23), "5,5_,23");
			Assert.AreEqual (5.555555555555555555555556m, Decimal.Round (five, 24), "5,5_,24");
			Assert.AreEqual (5.5555555555555555555555556m, Decimal.Round (five, 25), "5,5_,25");
			Assert.AreEqual (5.55555555555555555555555556m, Decimal.Round (five, 26), "5,5_,26");
			Assert.AreEqual (5.555555555555555555555555556m, Decimal.Round (five, 27), "5,5_,27");
			Assert.AreEqual (5.5555555555555555555555555555m, Decimal.Round (five, 28), "5.5_,28");
		}

		[Test]
		public void Round_EvenValue ()
		{
			Assert.AreEqual (2, Decimal.Round (2.5m, 0), "2,2_5,00");
			Assert.AreEqual (2.2m, Decimal.Round (2.25m, 1), "2,2_5,01");
			Assert.AreEqual (2.22m, Decimal.Round (2.225m, 2), "2,2_5,02");
			Assert.AreEqual (2.222m, Decimal.Round (2.2225m, 3), "2,2_5,03");
			Assert.AreEqual (2.2222m, Decimal.Round (2.22225m, 4), "2,2_5,04");
			Assert.AreEqual (2.22222m, Decimal.Round (2.222225m, 5), "2,2_5,05");
			Assert.AreEqual (2.222222m, Decimal.Round (2.2222225m, 6), "2,2_5,06");
			Assert.AreEqual (2.2222222m, Decimal.Round (2.22222225m, 7), "2,2_5,07");
			Assert.AreEqual (2.22222222m, Decimal.Round (2.222222225m, 8), "2,2_5,08");
			Assert.AreEqual (2.222222222m, Decimal.Round (2.2222222225m, 9), "2,2_5,09");
			Assert.AreEqual (2.2222222222m, Decimal.Round (2.22222222225m, 10), "2,2_5,10");
			Assert.AreEqual (2.22222222222m, Decimal.Round (2.222222222225m, 11), "2,2_5,11");
			Assert.AreEqual (2.222222222222m, Decimal.Round (2.2222222222225m, 12), "2,2_5,12");
			Assert.AreEqual (2.2222222222222m, Decimal.Round (2.22222222222225m, 13), "2,2_5,13");
			Assert.AreEqual (2.22222222222222m, Decimal.Round (2.222222222222225m, 14), "2,2_5,14");
			Assert.AreEqual (2.222222222222222m, Decimal.Round (2.2222222222222225m, 15), "2,2_5,15");
			Assert.AreEqual (2.2222222222222222m, Decimal.Round (2.22222222222222225m, 16), "2,2_5,16");
			Assert.AreEqual (2.22222222222222222m, Decimal.Round (2.222222222222222225m, 17), "2,2_5,17");
			Assert.AreEqual (2.222222222222222222m, Decimal.Round (2.2222222222222222225m, 18), "2,2_5,18");
			Assert.AreEqual (2.2222222222222222222m, Decimal.Round (2.22222222222222222225m, 19), "2,2_5,19");
			Assert.AreEqual (2.22222222222222222222m, Decimal.Round (2.222222222222222222225m, 20), "2,2_5,20");
			Assert.AreEqual (2.222222222222222222222m, Decimal.Round (2.2222222222222222222225m, 21), "2,2_5,21");
			Assert.AreEqual (2.2222222222222222222222m, Decimal.Round (2.22222222222222222222225m, 22), "2,2_5,22");
			Assert.AreEqual (2.22222222222222222222222m, Decimal.Round (2.222222222222222222222225m, 23), "2,2_5,23");
			Assert.AreEqual (2.222222222222222222222222m, Decimal.Round (2.2222222222222222222222225m, 24), "2,2_5,24");
			Assert.AreEqual (2.2222222222222222222222222m, Decimal.Round (2.22222222222222222222222225m, 25), "2,2_5,25");
			Assert.AreEqual (2.22222222222222222222222222m, Decimal.Round (2.222222222222222222222222225m, 26), "2,2_5,26");
			Assert.AreEqual (2.222222222222222222222222222m, Decimal.Round (2.2222222222222222222222222225m, 27), "2,2_5,27");
			Assert.AreEqual (2.2222222222222222222222222222m, Decimal.Round (2.22222222222222222222222222225m, 28), "2,2_5,28");
		}

		[Test]
		public void Round_OddValue_Negative ()
		{
			decimal five = -5.5555555555555555555555555555m;
			Assert.AreEqual (-6, Decimal.Round (five, 0), "-5,5_,00");
			Assert.AreEqual (-5.6m, Decimal.Round (five, 1), "-5,5_,01");
			Assert.AreEqual (-5.56m, Decimal.Round (five, 2), "-5,5_,02");
			Assert.AreEqual (-5.556m, Decimal.Round (five, 3), "-5,5_,03");
			Assert.AreEqual (-5.5556m, Decimal.Round (five, 4), "-5,5_,04");
			Assert.AreEqual (-5.55556m, Decimal.Round (five, 5), "-5,5_,05");
			Assert.AreEqual (-5.555556m, Decimal.Round (five, 6), "-5,5_,06");
			Assert.AreEqual (-5.5555556m, Decimal.Round (five, 7), "-5,5_,07");
			Assert.AreEqual (-5.55555556m, Decimal.Round (five, 8), "-5,5_,08");
			Assert.AreEqual (-5.555555556m, Decimal.Round (five, 9), "-5,5_,09");
			Assert.AreEqual (-5.5555555556m, Decimal.Round (five, 10), "-5,5_,10");
			Assert.AreEqual (-5.55555555556m, Decimal.Round (five, 11), "-5,5_,11");
			Assert.AreEqual (-5.555555555556m, Decimal.Round (five, 12), "-5,5_,12");
			Assert.AreEqual (-5.5555555555556m, Decimal.Round (five, 13), "-5,5_,13");
			Assert.AreEqual (-5.55555555555556m, Decimal.Round (five, 14), "-5,5_,14");
			Assert.AreEqual (-5.555555555555556m, Decimal.Round (five, 15), "-5,5_,15");
			Assert.AreEqual (-5.5555555555555556m, Decimal.Round (five, 16), "-5,5_,16");
			Assert.AreEqual (-5.55555555555555556m, Decimal.Round (five, 17), "-5,5_,17");
			Assert.AreEqual (-5.555555555555555556m, Decimal.Round (five, 18), "-5,5_,18");
			Assert.AreEqual (-5.5555555555555555556m, Decimal.Round (five, 19), "-5,5_,19");
			Assert.AreEqual (-5.55555555555555555556m, Decimal.Round (five, 20), "-5,5_,20");
			Assert.AreEqual (-5.555555555555555555556m, Decimal.Round (five, 21), "-5,5_,21");
			Assert.AreEqual (-5.5555555555555555555556m, Decimal.Round (five, 22), "-5,5_,22");
			Assert.AreEqual (-5.55555555555555555555556m, Decimal.Round (five, 23), "-5,5_,23");
			Assert.AreEqual (-5.555555555555555555555556m, Decimal.Round (five, 24), "-5,5_,24");
			Assert.AreEqual (-5.5555555555555555555555556m, Decimal.Round (five, 25), "-5,5_,25");
			Assert.AreEqual (-5.55555555555555555555555556m, Decimal.Round (five, 26), "-5,5_,26");
			Assert.AreEqual (-5.555555555555555555555555556m, Decimal.Round (five, 27), "-5,5_,27");
			Assert.AreEqual (-5.5555555555555555555555555555m, Decimal.Round (five, 28), "-5.5_,28");
		}

		[Test]
		public void Round_EvenValue_Negative ()
		{
			Assert.AreEqual (-2, Decimal.Round (-2.5m, 0), "-2,2_5,00");
			Assert.AreEqual (-2.2m, Decimal.Round (-2.25m, 1), "-2,2_5,01");
			Assert.AreEqual (-2.22m, Decimal.Round (-2.225m, 2), "-2,2_5,02");
			Assert.AreEqual (-2.222m, Decimal.Round (-2.2225m, 3), "-2,2_5,03");
			Assert.AreEqual (-2.2222m, Decimal.Round (-2.22225m, 4), "-2,2_5,04");
			Assert.AreEqual (-2.22222m, Decimal.Round (-2.222225m, 5), "-2,2_5,05");
			Assert.AreEqual (-2.222222m, Decimal.Round (-2.2222225m, 6), "-2,2_5,06");
			Assert.AreEqual (-2.2222222m, Decimal.Round (-2.22222225m, 7), "-2,2_5,07");
			Assert.AreEqual (-2.22222222m, Decimal.Round (-2.222222225m, 8), "-2,2_5,08");
			Assert.AreEqual (-2.222222222m, Decimal.Round (-2.2222222225m, 9), "-2,2_5,09");
			Assert.AreEqual (-2.2222222222m, Decimal.Round (-2.22222222225m, 10), "-2,2_5,10");
			Assert.AreEqual (-2.22222222222m, Decimal.Round (-2.222222222225m, 11), "-2,2_5,11");
			Assert.AreEqual (-2.222222222222m, Decimal.Round (-2.2222222222225m, 12), "-2,2_5,12");
			Assert.AreEqual (-2.2222222222222m, Decimal.Round (-2.22222222222225m, 13), "-2,2_5,13");
			Assert.AreEqual (-2.22222222222222m, Decimal.Round (-2.222222222222225m, 14), "-2,2_5,14");
			Assert.AreEqual (-2.222222222222222m, Decimal.Round (-2.2222222222222225m, 15), "-2,2_5,15");
			Assert.AreEqual (-2.2222222222222222m, Decimal.Round (-2.22222222222222225m, 16), "-2,2_5,16");
			Assert.AreEqual (-2.22222222222222222m, Decimal.Round (-2.222222222222222225m, 17), "-2,2_5,17");
			Assert.AreEqual (-2.222222222222222222m, Decimal.Round (-2.2222222222222222225m, 18), "-2,2_5,18");
			Assert.AreEqual (-2.2222222222222222222m, Decimal.Round (-2.22222222222222222225m, 19), "-2,2_5,19");
			Assert.AreEqual (-2.22222222222222222222m, Decimal.Round (-2.222222222222222222225m, 20), "-2,2_5,20");
			Assert.AreEqual (-2.222222222222222222222m, Decimal.Round (-2.2222222222222222222225m, 21), "-2,2_5,21");
			Assert.AreEqual (-2.2222222222222222222222m, Decimal.Round (-2.22222222222222222222225m, 22), "-2,2_5,22");
			Assert.AreEqual (-2.22222222222222222222222m, Decimal.Round (-2.222222222222222222222225m, 23), "-2,2_5,23");
			Assert.AreEqual (-2.222222222222222222222222m, Decimal.Round (-2.2222222222222222222222225m, 24), "-2,2_5,24");
			Assert.AreEqual (-2.2222222222222222222222222m, Decimal.Round (-2.22222222222222222222222225m, 25), "-2,2_5,25");
			Assert.AreEqual (-2.22222222222222222222222222m, Decimal.Round (-2.222222222222222222222222225m, 26), "-2,2_5,26");
			Assert.AreEqual (-2.222222222222222222222222222m, Decimal.Round (-2.2222222222222222222222222225m, 27), "-2,2_5,27");
			Assert.AreEqual (-2.2222222222222222222222222222m, Decimal.Round (-2.22222222222222222222222222225m, 28), "-2,2_5,28");
		}

		[Test] // bug #59425
		[SetCulture("en-US")]
		public void ParseAndKeepPrecision ()
		{
			string value = "5";
			Assert.AreEqual (value, value, Decimal.Parse (value).ToString ());
			value += '.';
			for (int i = 0; i < 28; i++) {
				value += "0";
				Assert.AreEqual (value, Decimal.Parse (value).ToString (), i.ToString ());
			}

			value = "-5";
			Assert.AreEqual (value, value, Decimal.Parse (value).ToString ());
			value += '.';
			for (int i = 0; i < 28; i++) {
				value += "0";
				Assert.AreEqual (value, Decimal.Parse (value).ToString (), "-" + i.ToString ());
			}
		}

		[Test]
		[SetCulture("en-US")]
		public void ToString_G ()
		{
			Assert.AreEqual ("1.0", (1.0m).ToString (), "00");
			Assert.AreEqual ("0.1", (0.1m).ToString (), "01");
			Assert.AreEqual ("0.01", (0.01m).ToString (), "02");
			Assert.AreEqual ("0.001", (0.001m).ToString (), "03");
			Assert.AreEqual ("0.0001", (0.0001m).ToString (), "04");
			Assert.AreEqual ("0.00001", (0.00001m).ToString (), "05");
			Assert.AreEqual ("0.000001", (0.000001m).ToString (), "06");
			Assert.AreEqual ("0.0000001", (0.0000001m).ToString (), "07");
			Assert.AreEqual ("0.00000001", (0.00000001m).ToString (), "08");
			Assert.AreEqual ("0.000000001", (0.000000001m).ToString (), "09");
			Assert.AreEqual ("0.0000000001", (0.0000000001m).ToString (), "10");
			Assert.AreEqual ("0.00000000001", (0.00000000001m).ToString (), "11");
			Assert.AreEqual ("0.000000000001", (0.000000000001m).ToString (), "12");
			Assert.AreEqual ("0.0000000000001", (0.0000000000001m).ToString (), "13");
			Assert.AreEqual ("0.00000000000001", (0.00000000000001m).ToString (), "14");
			Assert.AreEqual ("0.000000000000001", (0.000000000000001m).ToString (), "15");
			Assert.AreEqual ("0.0000000000000001", (0.0000000000000001m).ToString (), "16");
			Assert.AreEqual ("0.00000000000000001", (0.00000000000000001m).ToString (), "17");
			Assert.AreEqual ("0.000000000000000001", (0.000000000000000001m).ToString (), "18");
			Assert.AreEqual ("0.0000000000000000001", (0.0000000000000000001m).ToString (), "19");
			Assert.AreEqual ("0.00000000000000000001", (0.00000000000000000001m).ToString (), "20");
			Assert.AreEqual ("0.000000000000000000001", (0.000000000000000000001m).ToString (), "21");
			Assert.AreEqual ("0.0000000000000000000001", (0.0000000000000000000001m).ToString (), "22");
			Assert.AreEqual ("0.00000000000000000000001", (0.00000000000000000000001m).ToString (), "23");
			Assert.AreEqual ("0.000000000000000000000001", (0.000000000000000000000001m).ToString (), "24");
			Assert.AreEqual ("0.0000000000000000000000001", (0.0000000000000000000000001m).ToString (), "25");
			Assert.AreEqual ("0.00000000000000000000000001", (0.00000000000000000000000001m).ToString (), "26");
			Assert.AreEqual ("0.000000000000000000000000001", (0.000000000000000000000000001m).ToString (), "27");
			Assert.AreEqual ("0.0000000000000000000000000001", (0.0000000000000000000000000001m).ToString (), "28");
		}

		[Test]
		public void MidpointRoundingAwayFromZero ()
		{
			MidpointRounding m = MidpointRounding.AwayFromZero;
			Assert.AreEqual (4, Math.Round (3.5M, m), "#1");
			Assert.AreEqual (3, Math.Round (2.8M, m), "#2");
			Assert.AreEqual (3, Math.Round (2.5M, m), "#3");
			Assert.AreEqual (2, Math.Round (2.1M, m), "#4");
			Assert.AreEqual (-2, Math.Round (-2.1M, m), "#5");
			Assert.AreEqual (-3, Math.Round (-2.5M, m), "#6");
			Assert.AreEqual (-3, Math.Round (-2.8M, m), "#7");
			Assert.AreEqual (-4, Math.Round (-3.5M, m), "#8");

			Assert.AreEqual (3.1M, Math.Round (3.05M, 1, m), "#9");
			Assert.AreEqual (2.1M, Math.Round (2.08M, 1, m), "#10");
			Assert.AreEqual (2.1M, Math.Round (2.05M, 1, m), "#11");
			Assert.AreEqual (2.0M, Math.Round (2.01M, 1, m), "#12");
			Assert.AreEqual (-2.0M, Math.Round (-2.01M, 1, m), "#13");
			Assert.AreEqual (-2.1M, Math.Round (-2.05M, 1, m), "#14");
			Assert.AreEqual (-2.1M, Math.Round (-2.08M, 1, m), "#15");
			Assert.AreEqual (-3.1M, Math.Round (-3.05M, 1, m), "#16");
		}

		[Test] // bug #4814
		[SetCulture("")]
		public void Parse_NumberGroupSeparatorIsEmpty_DoNotThrowIndexOutOfRangeException ()
		{
			NumberFormatInfo nf = new NumberFormatInfo ();
			nf.NumberGroupSeparator = "";
			Decimal.Parse ("1.5", nf);
		}

		[Test] // bug #4814
		[SetCulture("")]
		public void Parse_CurrencyGroupSeparatorIsEmpty_DoNotThrowIndexOutOfRangeException ()
		{
			NumberFormatInfo nf = new NumberFormatInfo ();
			nf.CurrencyGroupSeparator = "";
			Decimal.Parse ("\u00A41.5", NumberStyles.Currency, nf);
		}

		[Test] // bug #4814
		[SetCulture("")]
		public void Parse_LeadingSign_PositiveSignIsEmpty_DoNotThrowIndexOutOfRangeException ()
		{
			NumberFormatInfo nf = new NumberFormatInfo ();
			nf.PositiveSign = "";
			try {
				Decimal.Parse ("+15", nf);
			} catch (FormatException) {
				return;
			}

			Assert.Fail ("Expected FormatException");
		}

		[Test] // bug #4814
		[SetCulture("")]
		public void Parse_LeadingSign_NegativeSignIsEmpty_DoNotThrowIndexOutOfRangeException ()
		{
			NumberFormatInfo nf = new NumberFormatInfo ();
			nf.NegativeSign = "";
			try {
				Decimal.Parse ("-15", nf);
			} catch (FormatException) {
				return;
			}

			Assert.Fail ("Expected FormatException");
		}

		[Test] // bug #4814
		[SetCulture("")]
		public void Parse_TrailingSign_PositiveSignIsEmpty_DoNotThrowIndexOutOfRangeException ()
		{
			NumberFormatInfo nf = new NumberFormatInfo ();
			nf.PositiveSign = "";
			try {
				Decimal.Parse ("15+", nf);
			} catch (FormatException) {
				return;
			}

			Assert.Fail ("Expected FormatException");
		}

		[Test] // bug #4814
		[SetCulture("")]
		public void Parse_TrailingSign_NegativeSignIsEmpty_DoNotThrowIndexOutOfRangeException ()
		{
			NumberFormatInfo nf = new NumberFormatInfo ();
			nf.NegativeSign = "";
			try {
				Decimal.Parse ("15-", nf);
			} catch (FormatException) {
				return;
			}

			Assert.Fail ("Expected FormatException");
		}

		[Test]
		[SetCulture("en-US")]
		public void ParseZeros ()
		{
			var d = Decimal.Parse ("0.000");
			var bits = Decimal.GetBits (d);
			Assert.AreEqual (0, bits[0], "#1");
			Assert.AreEqual (0, bits[1], "#2");
			Assert.AreEqual (0, bits[2], "#3");
			Assert.AreEqual (196608, bits[3], "#4");
			Assert.AreEqual ("0.000", d.ToString (), "#5");

			d = Decimal.Parse("0.000000000000000000000000000000000000000000000000000000000000000000");
			Assert.AreEqual ("0.0000000000000000000000000000", d.ToString (), "#10");

			d = Decimal.Parse ("0.");
			Assert.AreEqual ("0", d.ToString (), "#11");
		}
	}
}
