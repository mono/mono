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
	public class DecimalTest : Assertion
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
		private NumberFormatInfo NfiUser;

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
					AssertEquals ("A01 tab[" + i + "].format = '" + tab [i].format + "')", tab [i].str, s);
				} catch (OverflowException) {
					Fail (tab [i].d.ToString (tab [i].format, nfi) + " (format = '" + tab [i].format + "'): unexpected exception !");
				} catch (NUnit.Framework.AssertionException e) {
					throw e;
				} catch (Exception e) {
					Fail ("Unexpected Exception when i = " + i + ". e = " + e);
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
					Fail ("CurrencyNegativePattern #" + i + " failed: " +
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
					Fail ("CurrencyPositivePattern #" + i + " failed: " +
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
				AssertEquals ("NumberNegativePattern #" + i, ergNumberNegativePattern [i], d.ToString ("N", nfi2));
			}
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
					Fail ("PercentNegativePattern #" + i + " failed: " +
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
					Fail ("PercentPositivePattern #" + i + " failed: " +
						d.ToString ("P", nfi2) + " != " + ergPercentPositivePattern [i]);
				}
			}
		}

		ParseTest [] tab = {
				new ParseTest("1.2345", 1.2345m),
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
						Fail (tab [i].str + ": missing exception !");
					} else if (d != tab [i].d) {
						Fail (tab [i].str + " != " + d);
					}
				} catch (OverflowException) {
					if (!tab [i].exceptionFlag) {
						Fail (tab [i].str + ": unexpected exception !");
					}
				}
			}

			try {
				d = Decimal.Parse (null);
				Fail ("Expected ArgumentNullException");
			} catch (ArgumentNullException) {
				//ok
			}

			try {
				d = Decimal.Parse ("123nx");
				Fail ("Expected FormatException");
			} catch (FormatException) {
				//ok
			}

			try {
				d = Decimal.Parse ("79228162514264337593543950336");
				Fail ("Expected OverflowException" + d);
			} catch (OverflowException) {
				//ok
			}
		}

		[Test]
		public void TestConstants ()
		{
			AssertEquals ("Zero", 0m, Decimal.Zero);
			AssertEquals ("One", 1m, Decimal.One);
			AssertEquals ("MinusOne", -1m, Decimal.MinusOne);
			AssertEquals ("MaxValue", 79228162514264337593543950335m, Decimal.MaxValue);
			AssertEquals ("MinValue", -79228162514264337593543950335m, Decimal.MinValue);
			Assert ("MinusOne 2", -1m == Decimal.MinusOne);
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
					Fail ("Int32 -> Decimal: " + itab [i] + " != " + d);
				} else {
					int n = (int) d;
					if (n != itab [i]) {
						Fail ("Decimal -> Int32: " + d + " != " + itab [i]);
					}
				}
			}

			d = new Decimal (Int32.MaxValue);
			Assert ((int) d == Int32.MaxValue);

			d = new Decimal (Int32.MinValue);
			Assert ((int) d == Int32.MinValue);
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
					Fail ("UInt32 -> Decimal: " + itab [i] + " != " + d);
				} else {
					uint n = (uint) d;
					if (n != itab [i]) {
						Fail ("Decimal -> UInt32: " + d + " != " + itab [i]);
					}
				}
			}

			d = new Decimal (UInt32.MaxValue);
			Assert ((uint) d == UInt32.MaxValue);

			d = new Decimal (UInt32.MinValue);
			Assert ((uint) d == UInt32.MinValue);
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
					Fail ("Int64 -> Decimal: " + itab [i] + " != " + d);
				} else {
					long n = (long) d;
					if (n != itab [i]) {
						Fail ("Decimal -> Int64: " + d + " != " + itab [i]);
					}
				}
			}

			d = new Decimal (Int64.MaxValue);
			Assert ((long) d == Int64.MaxValue);

			d = new Decimal (Int64.MinValue);
			Assert ((long) d == Int64.MinValue);
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
					Fail ("UInt64 -> Decimal: " + itab [i] + " != " + d);
				} else {
					ulong n = (ulong) d;
					if (n != itab [i]) {
						Fail ("Decimal -> UInt64: " + d + " != " + itab [i]);
					}
				}
			}

			d = new Decimal (UInt64.MaxValue);
			Assert ((ulong) d == UInt64.MaxValue);

			d = new Decimal (UInt64.MinValue);
			Assert ((ulong) d == UInt64.MinValue);
		}

		[Test]
		public void TestConstructSingle ()
		{
			Decimal d;

			d = new Decimal (-1.2345678f);
			AssertEquals ("A#01", -1.234568m, (decimal) d);

			d = 3;
			AssertEquals ("A#02", 3.0f, (float) d);

			d = new Decimal (0.0f);
			AssertEquals ("A#03", 0m, (decimal) d);
			AssertEquals ("A#04", 0.0f, (float) d);

			d = new Decimal (1.0f);
			AssertEquals ("A#05", 1m, (decimal) d);
			AssertEquals ("A#06", 1.0f, (float) d);

			d = new Decimal (-1.2345678f);
			AssertEquals ("A#07", -1.234568m, (decimal) d);
			AssertEquals ("A#08", -1.234568f, (float) d);

			d = new Decimal (1.2345673f);
			AssertEquals ("A#09", 1.234567m, (decimal) d);

			d = new Decimal (1.2345673e7f);
			AssertEquals ("A#10", 12345670m, (decimal) d);

			d = new Decimal (1.2345673e-17f);
			AssertEquals ("A#11", 0.00000000000000001234567m, (decimal) d);
			AssertEquals ("A#12", 1.234567e-17f, (float) d);

			// test exceptions
			try {
				d = new Decimal (Single.MaxValue);
				Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (Single.NaN);
				Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (Single.PositiveInfinity);
				Fail ();
			} catch (OverflowException) {
			}
		}

		[Test]
		public void TestConstructSingleRounding_NowWorking ()
		{
			decimal d;

			d = new Decimal (1765.23454f);
			AssertEquals ("failed banker's rule rounding test 2", 1765.234m, d);

			d = new Decimal (0.00017652356f);
			AssertEquals ("06", 0.0001765236m, d);

			d = new Decimal (0.000176523554f);
			AssertEquals ("failed banker's rule rounding test 3", 0.0001765236m, d);

			d = new Decimal (0.00017652354f);
			AssertEquals ("08", 0.0001765235m, d);

			d = new Decimal (0.00017652346f);
			AssertEquals ("09", 0.0001765235m, d);

			d = new Decimal (0.000176523454f);
			AssertEquals ("failed banker's rule rounding test 4", 0.0001765234m, d);

			d = new Decimal (0.00017652344f);
			AssertEquals ("11", 0.0001765234m, d);
		}

		public void TestConstructSingleRounding ()
		{
			decimal d;

			d = new Decimal (1765.2356f);
			Assert ("01", d == 1765.236m);

			d = new Decimal (1765.23554f);
			Assert ("failed banker's rule rounding test 1", d == 1765.236m);

			d = new Decimal (1765.2354f);
			Assert ("03", d == 1765.235m);

			d = new Decimal (1765.2346f);
			Assert ("04", d == 1765.235m);

			d = new Decimal (1765.2344f);
			Assert ("05", d == 1765.234m);

			d = new Decimal (3.7652356e10f);
			Assert ("12", d == 37652360000m);

			d = new Decimal (3.7652356e20f);
			Assert ("13", d == 376523600000000000000m);

			d = new Decimal (3.76523554e20f);
			Assert ("failed banker's rule rounding test 5", d == 376523600000000000000m);

			d = new Decimal (3.7652352e20f);
			Assert ("15", d == 376523500000000000000m);

			d = new Decimal (3.7652348e20f);
			Assert ("16", d == 376523500000000000000m);

			d = new Decimal (3.76523454e20f);
			Assert ("failed banker's rule rounding test 6", d == 376523400000000000000m);

			d = new Decimal (3.7652342e20f);
			Assert ("18", d == 376523400000000000000m);
		}

		[Test]
		public void TestConstructDouble ()
		{
			Decimal d;

			d = new Decimal (0.0);
			Assert ((decimal) d == 0m);

			d = new Decimal (1.0);
			Assert ((decimal) d == 1m);
			Assert (1.0 == (double) d);

			d = new Decimal (-1.2345678901234);
			Assert ((decimal) d == -1.2345678901234m);
			Assert (-1.2345678901234 == (double) d);

			d = new Decimal (1.2345678901234);
			Assert ((decimal) d == 1.2345678901234m);

			d = new Decimal (1.2345678901234e8);
			Assert ((decimal) d == 123456789.01234m);
			Assert (1.2345678901234e8 == (double) d);

			d = new Decimal (1.2345678901234e16);
			Assert ((decimal) d == 12345678901234000m);
			Assert (1.2345678901234e16 == (double) d);

			d = new Decimal (1.2345678901234e24);
			Assert ((decimal) d == 1234567890123400000000000m);
			Assert (1.2345678901234e24 == (double) d);

			d = new Decimal (1.2345678901234e28);
			Assert ((decimal) d == 1.2345678901234e28m);
			Assert (1.2345678901234e28 == (double) d);

			d = new Decimal (7.2345678901234e28);
			Assert ((decimal) d == 7.2345678901234e28m);
			Assert (new Decimal ((double) d) == d);

			d = new Decimal (1.2345678901234e-8);
			Assert ((decimal) d == 1.2345678901234e-8m);

			d = new Decimal (1.2345678901234e-14);
			Assert ((decimal) d == 1.2345678901234e-14m);
			Assert (1.2345678901234e-14 == (double) d);

			d = new Decimal (1.2342278901234e-25);
			AssertEquals ("A10", d, 1.234e-25m);

			// test exceptions
			try {
				d = new Decimal (8e28);
				Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (8e48);
				Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (Double.NaN);
				Fail ();
			} catch (OverflowException) {
			}

			try {
				d = new Decimal (Double.PositiveInfinity);
				Fail ();
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
				AssertEquals ("A01", 1765.23123456786m, d);

				TestNum++;
				d = new Decimal (1765.2312345678554);
				AssertEquals ("A02, failed banker's rule rounding test 1", 1765.23123456786m, d);
				AssertEquals ("A03", 1765.23123456786, (double) d);

				TestNum++;
				d = new Decimal (1765.231234567853);
				Assert (d == 1765.23123456785m);

				TestNum++;
				d = new Decimal (1765.231234567847);
				Assert (d == 1765.23123456785m);

				TestNum++;
				d = new Decimal (1765.231234567843);
				Assert (d == 1765.23123456784m);

				TestNum++;
				d = new Decimal (1.765231234567857e-9);
				Assert (d == 1.76523123456786e-9m);

				TestNum++;
				d = new Decimal (1.7652312345678554e-9);
				Assert ("failed banker's rule rounding test 3", d == 1.76523123456786e-9m);

				TestNum++;
				d = new Decimal (1.765231234567853e-9);
				Assert (d == 1.76523123456785e-9m);

				TestNum++;
				d = new Decimal (1.765231234567857e+24);
				Assert (d == 1.76523123456786e+24m);

				TestNum++;
				d = new Decimal (1.7652312345678554e+24);
				Assert ("failed banker's rule rounding test 4", d == 1.76523123456786e+24m);

				TestNum++;
				d = new Decimal (1.765231234567853e+24);
				Assert (d == 1.76523123456785e+24m);

				TestNum++;
				d = new Decimal (1765.2312345678454);
				Assert (d == 1765.23123456785m);
			} catch (Exception e) {
				Fail ("At TestNum = " + TestNum + " unexpected exception. e = " + e);
			}
		}

		[Test]
		public void TestNegate ()
		{
			decimal d;

			d = new Decimal (12345678);
			Assert ((decimal) Decimal.Negate (d) == -12345678m);
		}

		[Test]
		public void TestPartConstruct ()
		{
			decimal d;

			d = new Decimal (parts0);
			Assert (d == 0);

			d = new Decimal (parts1);
			Assert (d == 1);

			d = new Decimal (parts2);
			Assert (d == 4294967296m);

			d = new Decimal (parts3);
			Assert (d == 18446744073709551616m);

			d = new Decimal (parts4);
			Assert (d == 0m);

			d = new Decimal (parts5);
			Assert (d == 18446744078004518913m);

			d = new Decimal (partsMaxValue);
			Assert (d == Decimal.MaxValue);

			d = new Decimal (partsMinValue);
			Assert (d == Decimal.MinValue);

			d = new Decimal (parts6);
			int [] erg = Decimal.GetBits (d);
			for (int i = 0; i < 4; i++) {
				Assert (erg [i] == parts6 [i]);
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
					Fail ("Floor: Floor(" + dtab [i, 0] + ") != " + d);
				}
				d = Decimal.Truncate (dtab [i, 0]);
				if (d != dtab [i, 2]) {
					Fail ("Truncate: Truncate(" + dtab [i, 0] + ") != " + d);
				}
			}
		}

		[Test]
		public void Truncate ()
		{
			decimal dd = 249.9m;
			decimal dt = Decimal.Truncate (dd);
			AssertEquals ("Original", 249.9m, dd);
			AssertEquals ("Truncate", 249m, dt);
			AssertEquals ("Cast-Byte", 249, (byte) dd);
			AssertEquals ("Cast-Char", 249, (char) dd);
			AssertEquals ("Cast-Int16", 249, (short) dd);
			AssertEquals ("Cast-UInt16", 249, (ushort) dd);
			AssertEquals ("Cast-Int32", 249, (int) dd);
			AssertEquals ("Cast-UInt32", 249, (uint) dd);
			AssertEquals ("Cast-Int64", 249, (long) dd);
			AssertEquals ("Cast-UInt64", 249, (ulong) dd);
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
					Fail ("Round: Round(" + dtab [i, 0] + "," + (int) dtab [i, 1] + ") != " + d);
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
					Fail ("FailRound: Round(" + dtab [i, 0] + "," + (int) dtab [i, 1] + ") != " + d);
				}
			}
		}

		[Test]
		public void ParseInt64 ()
		{
			long max = Int64.MaxValue;
			Decimal dmax = Decimal.Parse (max.ToString ());
			AssertEquals ("Int64.MaxValue", Int64.MaxValue, Decimal.ToInt64 (dmax));

			long min = Int64.MinValue;
			Decimal dmin = Decimal.Parse (min.ToString ());
			AssertEquals ("Int64.MinValue", Int64.MinValue, Decimal.ToInt64 (dmin));

			dmax += 1.1m;
			dmax = Decimal.Parse (dmax.ToString ());
			AssertEquals ("Int64.MaxValue+1.1", Int64.MaxValue, Decimal.ToInt64 (dmax - 1.1m));

			dmin -= 1.1m;
			dmin = Decimal.Parse (dmin.ToString ());
			AssertEquals ("Int64.MinValue-1.1", Int64.MinValue, Decimal.ToInt64 (dmin + 1.1m));
		}

		[Test]
		public void ToByte ()
		{
			Decimal d = 254.9m;
			AssertEquals ("Decimal.ToByte", 254, Decimal.ToByte (d));
			AssertEquals ("Convert.ToByte", 255, Convert.ToByte (d));
			AssertEquals ("IConvertible.ToByte", 255, (d as IConvertible).ToByte (null));
		}

		[Test]
		public void ToSByte ()
		{
			Decimal d = 126.9m;
			AssertEquals ("Decimal.ToSByte", 126, Decimal.ToSByte (d));
			AssertEquals ("Convert.ToSByte", 127, Convert.ToSByte (d));
			AssertEquals ("IConvertible.ToSByte", 127, (d as IConvertible).ToSByte (null));
			d = -d;
			AssertEquals ("-Decimal.ToSByte", -126, Decimal.ToSByte (d));
			AssertEquals ("-Convert.ToSByte", -127, Convert.ToSByte (d));
			AssertEquals ("-IConvertible.ToSByte", -127, (d as IConvertible).ToSByte (null));
		}

		[Test]
		public void ToInt16 ()
		{
			Decimal d = 254.9m;
			AssertEquals ("Decimal.ToInt16", 254, Decimal.ToInt16 (d));
			AssertEquals ("Convert.ToInt16", 255, Convert.ToInt16 (d));
			AssertEquals ("IConvertible.ToInt16", 255, (d as IConvertible).ToInt16 (null));
			d = -d;
			AssertEquals ("-Decimal.ToInt16", -254, Decimal.ToInt16 (d));
			AssertEquals ("-Convert.ToInt16", -255, Convert.ToInt16 (d));
			AssertEquals ("-IConvertible.ToInt16", -255, (d as IConvertible).ToInt16 (null));
		}

		[Test]
		public void ToUInt16 ()
		{
			Decimal d = 254.9m;
			AssertEquals ("Decimal.ToUInt16", 254, Decimal.ToUInt16 (d));
			AssertEquals ("Convert.ToUInt16", 255, Convert.ToUInt16 (d));
			AssertEquals ("IConvertible.ToUInt16", 255, (d as IConvertible).ToUInt16 (null));
		}

		[Test]
		public void ToInt32 ()
		{
			Decimal d = 254.9m;
			AssertEquals ("Decimal.ToInt32", 254, Decimal.ToInt32 (d));
			AssertEquals ("Convert.ToInt32", 255, Convert.ToInt32 (d));
			AssertEquals ("IConvertible.ToInt32", 255, (d as IConvertible).ToInt32 (null));
			d = -d;
			AssertEquals ("-Decimal.ToInt32", -254, Decimal.ToInt32 (d));
			AssertEquals ("-Convert.ToInt32", -255, Convert.ToInt32 (d));
			AssertEquals ("-IConvertible.ToInt32", -255, (d as IConvertible).ToInt32 (null));
		}

		[Test]
		public void ToUInt32 ()
		{
			Decimal d = 254.9m;
			AssertEquals ("Decimal.ToUInt32", 254, Decimal.ToUInt32 (d));
			AssertEquals ("Convert.ToUInt32", 255, Convert.ToUInt32 (d));
			AssertEquals ("IConvertible.ToUInt32", 255, (d as IConvertible).ToUInt32 (null));
		}

		[Test]
		public void ToInt64 ()
		{
			Decimal d = 254.9m;
			AssertEquals ("Decimal.ToInt64", 254, Decimal.ToInt64 (d));
			AssertEquals ("Convert.ToInt64", 255, Convert.ToInt64 (d));
			AssertEquals ("IConvertible.ToInt64", 255, (d as IConvertible).ToInt64 (null));
			d = -d;
			AssertEquals ("-Decimal.ToInt64", -254, Decimal.ToInt64 (d));
			AssertEquals ("-Convert.ToInt64", -255, Convert.ToInt64 (d));
			AssertEquals ("-IConvertible.ToInt64", -255, (d as IConvertible).ToInt64 (null));
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
			AssertEquals ("Decimal.ToUInt64", 254, Decimal.ToUInt64 (d));
			AssertEquals ("Convert.ToUInt64", 255, Convert.ToUInt64 (d));
			AssertEquals ("IConvertible.ToUInt64", 255, (d as IConvertible).ToUInt64 (null));
		}

		[Test]
		public void ToSingle ()
		{
			Decimal d = 254.9m;
			AssertEquals ("Decimal.ToSingle", 254.9f, Decimal.ToSingle (d));
			AssertEquals ("Convert.ToSingle", 254.9f, Convert.ToSingle (d));
			AssertEquals ("IConvertible.ToSingle", 254.9f, (d as IConvertible).ToSingle (null));
			d = -d;
			AssertEquals ("-Decimal.ToSingle", -254.9f, Decimal.ToSingle (d));
			AssertEquals ("-Convert.ToSingle", -254.9f, Convert.ToSingle (d));
			AssertEquals ("-IConvertible.ToSingle", -254.9f, (d as IConvertible).ToSingle (null));
		}

		[Test]
		public void ToDouble ()
		{
			Decimal d = 254.9m;
			AssertEquals ("Decimal.ToDouble", 254.9d, Decimal.ToDouble (d));
			AssertEquals ("Convert.ToDouble", 254.9d, Convert.ToDouble (d));
			AssertEquals ("IConvertible.ToDouble", 254.9d, (d as IConvertible).ToDouble (null));
			d = -d;
			AssertEquals ("-Decimal.ToDouble", -254.9d, Decimal.ToDouble (d));
			AssertEquals ("-Convert.ToDouble", -254.9d, Convert.ToDouble (d));
			AssertEquals ("-IConvertible.ToDouble", -254.9d, (d as IConvertible).ToDouble (null));
		}

		[Test]
		public void ToString_Defaults ()
		{
			Decimal d = 254.9m;
			// everything defaults to "G"
			string def = d.ToString ("G");
			AssertEquals ("ToString()", def, d.ToString ());
			AssertEquals ("ToString((IFormatProvider)null)", def, d.ToString ((IFormatProvider) null));
			AssertEquals ("ToString((string)null)", def, d.ToString ((string) null));
			AssertEquals ("ToString(empty)", def, d.ToString (String.Empty));
			AssertEquals ("ToString(null,null)", def, d.ToString (null, null));
			AssertEquals ("ToString(empty,null)", def, d.ToString (String.Empty, null));

			AssertEquals ("ToString()", "254.9", def);
		}

		[Test]
		public void CastTruncRounding ()
		{
			// casting truncs decimal value (not normal nor banker's rounding)
			AssertEquals ("254.9==254", 254, (long) (254.9m));
			AssertEquals ("-254.9=-254", -254, (long) (-254.9m));
			AssertEquals ("255.9==256", 255, (long) (255.9m));
			AssertEquals ("-255.9=-256", -255, (long) (-255.9m));
		}

		[Test]
		public void ParseFractions ()
		{
			decimal d1 = Decimal.Parse ("0.523456789012345467890123456789", CultureInfo.InvariantCulture);
			AssertEquals ("f1", 0.5234567890123454678901234568m, d1);
			decimal d2 = Decimal.Parse ("0.49214206543486529434634231456", CultureInfo.InvariantCulture);
			AssertEquals ("f2", 0.4921420654348652943463423146m, d2);
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

#if NET_2_0
		[Test]
		[Category ("TargetJvmNotWorking")]
		public void TryParse ()
		{
			Decimal r;
		
			// These should return false
			AssertEquals (false, Decimal.TryParse ("79228162514264337593543950336", out r));
			AssertEquals (false, Decimal.TryParse ("123nx", NumberStyles.Number, CultureInfo.InvariantCulture, out r));
			AssertEquals (false, Decimal.TryParse (null, NumberStyles.Number, CultureInfo.InvariantCulture, out r));

			// These should pass
			for (int i = 0; i < tab.Length; i++) {
				AssertEquals (!tab [i].exceptionFlag,
					Decimal.TryParse (tab [i].str, tab [i].style,
					NumberFormatInfo.InvariantInfo, out r));
			}
		}
#endif

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

			AssertEquals ("254.9 % 12.1", 0.8m, Decimal.Remainder (p1, p2));
			AssertEquals ("-254.9 % 12.1", -0.8m, Decimal.Remainder (n1, p2));
			AssertEquals ("254.9 % -12.1", 0.8m, Decimal.Remainder (p1, n2));
			AssertEquals ("-254.9 % -12.1", -0.8m, Decimal.Remainder (n1, n2));

			AssertEquals ("12.1 % 254.9", 12.1m, Decimal.Remainder (p2, p1));
			AssertEquals ("-12.1 % 254.9", -12.1m, Decimal.Remainder (n2, p1));
			AssertEquals ("12.1 % -254.9", 12.1m, Decimal.Remainder (p2, n1));
			AssertEquals ("-12.1 % -254.9", -12.1m, Decimal.Remainder (n2, n1));
#if NET_2_0
			AssertEquals ("12.1 % 12.1", 0.0m, Decimal.Remainder (p1, p1));
			AssertEquals ("-12.1 % 12.1", 0.0m, Decimal.Remainder (n1, p1));
			AssertEquals ("12.1 % -12.1", 0.0m, Decimal.Remainder (p1, n1));
			AssertEquals ("-12.1 % -12.1", 0.0m, Decimal.Remainder (n1, n1));
#else
			AssertEquals ("12.1 % 12.1", 0, Decimal.Remainder (p1, p1));
			AssertEquals ("-12.1 % 12.1", 0, Decimal.Remainder (n1, p1));
			AssertEquals ("12.1 % -12.1", 0, Decimal.Remainder (p1, n1));
			AssertEquals ("-12.1 % -12.1", 0, Decimal.Remainder (n1, n1));
#endif
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

			AssertEquals ("254.9 / 12.1", c1, Decimal.Divide (p1, p2));
			AssertEquals ("-254.9 / 12.1", -c1, Decimal.Divide (n1, p2));
			AssertEquals ("254.9 / -12.1", -c1, Decimal.Divide (p1, n2));
			AssertEquals ("-254.9 / -12.1", c1, Decimal.Divide (n1, n2));

			AssertEquals ("12.1 / 254.9", c2, Decimal.Divide (p2, p1));
			AssertEquals ("-12.1 / 254.9", -c2, Decimal.Divide (n2, p1));
			AssertEquals ("12.1 / -254.9", -c2, Decimal.Divide (p2, n1));
			AssertEquals ("-12.1 / -254.9", c2, Decimal.Divide (n2, n1));

			AssertEquals ("12.1 / 12.1", 1, Decimal.Divide (p1, p1));
			AssertEquals ("-12.1 / 12.1", -1, Decimal.Divide (n1, p1));
			AssertEquals ("12.1 / -12.1", -1, Decimal.Divide (p1, n1));
			AssertEquals ("-12.1 / -12.1", 1, Decimal.Divide (n1, n1));
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
			AssertEquals ("5,5_,00", 6, Decimal.Round (five, 0));
			AssertEquals ("5,5_,01", 5.6m, Decimal.Round (five, 1));
			AssertEquals ("5,5_,02", 5.56m, Decimal.Round (five, 2));
			AssertEquals ("5,5_,03", 5.556m, Decimal.Round (five, 3));
			AssertEquals ("5,5_,04", 5.5556m, Decimal.Round (five, 4));
			AssertEquals ("5,5_,05", 5.55556m, Decimal.Round (five, 5));
			AssertEquals ("5,5_,06", 5.555556m, Decimal.Round (five, 6));
			AssertEquals ("5,5_,07", 5.5555556m, Decimal.Round (five, 7));
			AssertEquals ("5,5_,08", 5.55555556m, Decimal.Round (five, 8));
			AssertEquals ("5,5_,09", 5.555555556m, Decimal.Round (five, 9));
			AssertEquals ("5,5_,10", 5.5555555556m, Decimal.Round (five, 10));
			AssertEquals ("5,5_,11", 5.55555555556m, Decimal.Round (five, 11));
			AssertEquals ("5,5_,12", 5.555555555556m, Decimal.Round (five, 12));
			AssertEquals ("5,5_,13", 5.5555555555556m, Decimal.Round (five, 13));
			AssertEquals ("5,5_,14", 5.55555555555556m, Decimal.Round (five, 14));
			AssertEquals ("5,5_,15", 5.555555555555556m, Decimal.Round (five, 15));
			AssertEquals ("5,5_,16", 5.5555555555555556m, Decimal.Round (five, 16));
			AssertEquals ("5,5_,17", 5.55555555555555556m, Decimal.Round (five, 17));
			AssertEquals ("5,5_,18", 5.555555555555555556m, Decimal.Round (five, 18));
			AssertEquals ("5,5_,19", 5.5555555555555555556m, Decimal.Round (five, 19));
			AssertEquals ("5,5_,20", 5.55555555555555555556m, Decimal.Round (five, 20));
			AssertEquals ("5,5_,21", 5.555555555555555555556m, Decimal.Round (five, 21));
			AssertEquals ("5,5_,22", 5.5555555555555555555556m, Decimal.Round (five, 22));
			AssertEquals ("5,5_,23", 5.55555555555555555555556m, Decimal.Round (five, 23));
			AssertEquals ("5,5_,24", 5.555555555555555555555556m, Decimal.Round (five, 24));
			AssertEquals ("5,5_,25", 5.5555555555555555555555556m, Decimal.Round (five, 25));
			AssertEquals ("5,5_,26", 5.55555555555555555555555556m, Decimal.Round (five, 26));
			AssertEquals ("5,5_,27", 5.555555555555555555555555556m, Decimal.Round (five, 27));
			AssertEquals ("5.5_,28", 5.5555555555555555555555555555m, Decimal.Round (five, 28));
		}

		[Test]
		public void Round_EvenValue ()
		{
			AssertEquals ("2,2_5,00", 2, Decimal.Round (2.5m, 0));
			AssertEquals ("2,2_5,01", 2.2m, Decimal.Round (2.25m, 1));
			AssertEquals ("2,2_5,02", 2.22m, Decimal.Round (2.225m, 2));
			AssertEquals ("2,2_5,03", 2.222m, Decimal.Round (2.2225m, 3));
			AssertEquals ("2,2_5,04", 2.2222m, Decimal.Round (2.22225m, 4));
			AssertEquals ("2,2_5,05", 2.22222m, Decimal.Round (2.222225m, 5));
			AssertEquals ("2,2_5,06", 2.222222m, Decimal.Round (2.2222225m, 6));
			AssertEquals ("2,2_5,07", 2.2222222m, Decimal.Round (2.22222225m, 7));
			AssertEquals ("2,2_5,08", 2.22222222m, Decimal.Round (2.222222225m, 8));
			AssertEquals ("2,2_5,09", 2.222222222m, Decimal.Round (2.2222222225m, 9));
			AssertEquals ("2,2_5,10", 2.2222222222m, Decimal.Round (2.22222222225m, 10));
			AssertEquals ("2,2_5,11", 2.22222222222m, Decimal.Round (2.222222222225m, 11));
			AssertEquals ("2,2_5,12", 2.222222222222m, Decimal.Round (2.2222222222225m, 12));
			AssertEquals ("2,2_5,13", 2.2222222222222m, Decimal.Round (2.22222222222225m, 13));
			AssertEquals ("2,2_5,14", 2.22222222222222m, Decimal.Round (2.222222222222225m, 14));
			AssertEquals ("2,2_5,15", 2.222222222222222m, Decimal.Round (2.2222222222222225m, 15));
			AssertEquals ("2,2_5,16", 2.2222222222222222m, Decimal.Round (2.22222222222222225m, 16));
			AssertEquals ("2,2_5,17", 2.22222222222222222m, Decimal.Round (2.222222222222222225m, 17));
			AssertEquals ("2,2_5,18", 2.222222222222222222m, Decimal.Round (2.2222222222222222225m, 18));
			AssertEquals ("2,2_5,19", 2.2222222222222222222m, Decimal.Round (2.22222222222222222225m, 19));
			AssertEquals ("2,2_5,20", 2.22222222222222222222m, Decimal.Round (2.222222222222222222225m, 20));
			AssertEquals ("2,2_5,21", 2.222222222222222222222m, Decimal.Round (2.2222222222222222222225m, 21));
			AssertEquals ("2,2_5,22", 2.2222222222222222222222m, Decimal.Round (2.22222222222222222222225m, 22));
			AssertEquals ("2,2_5,23", 2.22222222222222222222222m, Decimal.Round (2.222222222222222222222225m, 23));
			AssertEquals ("2,2_5,24", 2.222222222222222222222222m, Decimal.Round (2.2222222222222222222222225m, 24));
			AssertEquals ("2,2_5,25", 2.2222222222222222222222222m, Decimal.Round (2.22222222222222222222222225m, 25));
			AssertEquals ("2,2_5,26", 2.22222222222222222222222222m, Decimal.Round (2.222222222222222222222222225m, 26));
			AssertEquals ("2,2_5,27", 2.222222222222222222222222222m, Decimal.Round (2.2222222222222222222222222225m, 27));
			AssertEquals ("2,2_5,28", 2.2222222222222222222222222222m, Decimal.Round (2.22222222222222222222222222225m, 28));
		}

		[Test]
		public void Round_OddValue_Negative ()
		{
			decimal five = -5.5555555555555555555555555555m;
			AssertEquals ("-5,5_,00", -6, Decimal.Round (five, 0));
			AssertEquals ("-5,5_,01", -5.6m, Decimal.Round (five, 1));
			AssertEquals ("-5,5_,02", -5.56m, Decimal.Round (five, 2));
			AssertEquals ("-5,5_,03", -5.556m, Decimal.Round (five, 3));
			AssertEquals ("-5,5_,04", -5.5556m, Decimal.Round (five, 4));
			AssertEquals ("-5,5_,05", -5.55556m, Decimal.Round (five, 5));
			AssertEquals ("-5,5_,06", -5.555556m, Decimal.Round (five, 6));
			AssertEquals ("-5,5_,07", -5.5555556m, Decimal.Round (five, 7));
			AssertEquals ("-5,5_,08", -5.55555556m, Decimal.Round (five, 8));
			AssertEquals ("-5,5_,09", -5.555555556m, Decimal.Round (five, 9));
			AssertEquals ("-5,5_,10", -5.5555555556m, Decimal.Round (five, 10));
			AssertEquals ("-5,5_,11", -5.55555555556m, Decimal.Round (five, 11));
			AssertEquals ("-5,5_,12", -5.555555555556m, Decimal.Round (five, 12));
			AssertEquals ("-5,5_,13", -5.5555555555556m, Decimal.Round (five, 13));
			AssertEquals ("-5,5_,14", -5.55555555555556m, Decimal.Round (five, 14));
			AssertEquals ("-5,5_,15", -5.555555555555556m, Decimal.Round (five, 15));
			AssertEquals ("-5,5_,16", -5.5555555555555556m, Decimal.Round (five, 16));
			AssertEquals ("-5,5_,17", -5.55555555555555556m, Decimal.Round (five, 17));
			AssertEquals ("-5,5_,18", -5.555555555555555556m, Decimal.Round (five, 18));
			AssertEquals ("-5,5_,19", -5.5555555555555555556m, Decimal.Round (five, 19));
			AssertEquals ("-5,5_,20", -5.55555555555555555556m, Decimal.Round (five, 20));
			AssertEquals ("-5,5_,21", -5.555555555555555555556m, Decimal.Round (five, 21));
			AssertEquals ("-5,5_,22", -5.5555555555555555555556m, Decimal.Round (five, 22));
			AssertEquals ("-5,5_,23", -5.55555555555555555555556m, Decimal.Round (five, 23));
			AssertEquals ("-5,5_,24", -5.555555555555555555555556m, Decimal.Round (five, 24));
			AssertEquals ("-5,5_,25", -5.5555555555555555555555556m, Decimal.Round (five, 25));
			AssertEquals ("-5,5_,26", -5.55555555555555555555555556m, Decimal.Round (five, 26));
			AssertEquals ("-5,5_,27", -5.555555555555555555555555556m, Decimal.Round (five, 27));
			AssertEquals ("-5.5_,28", -5.5555555555555555555555555555m, Decimal.Round (five, 28));
		}

		[Test]
		public void Round_EvenValue_Negative ()
		{
			AssertEquals ("-2,2_5,00", -2, Decimal.Round (-2.5m, 0));
			AssertEquals ("-2,2_5,01", -2.2m, Decimal.Round (-2.25m, 1));
			AssertEquals ("-2,2_5,02", -2.22m, Decimal.Round (-2.225m, 2));
			AssertEquals ("-2,2_5,03", -2.222m, Decimal.Round (-2.2225m, 3));
			AssertEquals ("-2,2_5,04", -2.2222m, Decimal.Round (-2.22225m, 4));
			AssertEquals ("-2,2_5,05", -2.22222m, Decimal.Round (-2.222225m, 5));
			AssertEquals ("-2,2_5,06", -2.222222m, Decimal.Round (-2.2222225m, 6));
			AssertEquals ("-2,2_5,07", -2.2222222m, Decimal.Round (-2.22222225m, 7));
			AssertEquals ("-2,2_5,08", -2.22222222m, Decimal.Round (-2.222222225m, 8));
			AssertEquals ("-2,2_5,09", -2.222222222m, Decimal.Round (-2.2222222225m, 9));
			AssertEquals ("-2,2_5,10", -2.2222222222m, Decimal.Round (-2.22222222225m, 10));
			AssertEquals ("-2,2_5,11", -2.22222222222m, Decimal.Round (-2.222222222225m, 11));
			AssertEquals ("-2,2_5,12", -2.222222222222m, Decimal.Round (-2.2222222222225m, 12));
			AssertEquals ("-2,2_5,13", -2.2222222222222m, Decimal.Round (-2.22222222222225m, 13));
			AssertEquals ("-2,2_5,14", -2.22222222222222m, Decimal.Round (-2.222222222222225m, 14));
			AssertEquals ("-2,2_5,15", -2.222222222222222m, Decimal.Round (-2.2222222222222225m, 15));
			AssertEquals ("-2,2_5,16", -2.2222222222222222m, Decimal.Round (-2.22222222222222225m, 16));
			AssertEquals ("-2,2_5,17", -2.22222222222222222m, Decimal.Round (-2.222222222222222225m, 17));
			AssertEquals ("-2,2_5,18", -2.222222222222222222m, Decimal.Round (-2.2222222222222222225m, 18));
			AssertEquals ("-2,2_5,19", -2.2222222222222222222m, Decimal.Round (-2.22222222222222222225m, 19));
			AssertEquals ("-2,2_5,20", -2.22222222222222222222m, Decimal.Round (-2.222222222222222222225m, 20));
			AssertEquals ("-2,2_5,21", -2.222222222222222222222m, Decimal.Round (-2.2222222222222222222225m, 21));
			AssertEquals ("-2,2_5,22", -2.2222222222222222222222m, Decimal.Round (-2.22222222222222222222225m, 22));
			AssertEquals ("-2,2_5,23", -2.22222222222222222222222m, Decimal.Round (-2.222222222222222222222225m, 23));
			AssertEquals ("-2,2_5,24", -2.222222222222222222222222m, Decimal.Round (-2.2222222222222222222222225m, 24));
			AssertEquals ("-2,2_5,25", -2.2222222222222222222222222m, Decimal.Round (-2.22222222222222222222222225m, 25));
			AssertEquals ("-2,2_5,26", -2.22222222222222222222222222m, Decimal.Round (-2.222222222222222222222222225m, 26));
			AssertEquals ("-2,2_5,27", -2.222222222222222222222222222m, Decimal.Round (-2.2222222222222222222222222225m, 27));
			AssertEquals ("-2,2_5,28", -2.2222222222222222222222222222m, Decimal.Round (-2.22222222222222222222222222225m, 28));
		}

		[Test] // bug #59425
		public void ParseAndKeepPrecision ()
		{
			string value = "5";
			AssertEquals (value, value, Decimal.Parse (value).ToString ());
			value += '.';
			for (int i = 0; i < 28; i++) {
				value += "0";
				AssertEquals (i.ToString (), value, Decimal.Parse (value).ToString ());
			}

			value = "-5";
			AssertEquals (value, value, Decimal.Parse (value).ToString ());
			value += '.';
			for (int i = 0; i < 28; i++) {
				value += "0";
				AssertEquals ("-" + i.ToString (), value, Decimal.Parse (value).ToString ());
			}
		}

		[Test]
		public void ToString_G ()
		{
			AssertEquals ("00", "1.0", (1.0m).ToString ());
			AssertEquals ("01", "0.1", (0.1m).ToString ());
			AssertEquals ("02", "0.01", (0.01m).ToString ());
			AssertEquals ("03", "0.001", (0.001m).ToString ());
			AssertEquals ("04", "0.0001", (0.0001m).ToString ());
			AssertEquals ("05", "0.00001", (0.00001m).ToString ());
			AssertEquals ("06", "0.000001", (0.000001m).ToString ());
			AssertEquals ("07", "0.0000001", (0.0000001m).ToString ());
			AssertEquals ("08", "0.00000001", (0.00000001m).ToString ());
			AssertEquals ("09", "0.000000001", (0.000000001m).ToString ());
			AssertEquals ("10", "0.0000000001", (0.0000000001m).ToString ());
			AssertEquals ("11", "0.00000000001", (0.00000000001m).ToString ());
			AssertEquals ("12", "0.000000000001", (0.000000000001m).ToString ());
			AssertEquals ("13", "0.0000000000001", (0.0000000000001m).ToString ());
			AssertEquals ("14", "0.00000000000001", (0.00000000000001m).ToString ());
			AssertEquals ("15", "0.000000000000001", (0.000000000000001m).ToString ());
			AssertEquals ("16", "0.0000000000000001", (0.0000000000000001m).ToString ());
			AssertEquals ("17", "0.00000000000000001", (0.00000000000000001m).ToString ());
			AssertEquals ("18", "0.000000000000000001", (0.000000000000000001m).ToString ());
			AssertEquals ("19", "0.0000000000000000001", (0.0000000000000000001m).ToString ());
			AssertEquals ("20", "0.00000000000000000001", (0.00000000000000000001m).ToString ());
			AssertEquals ("21", "0.000000000000000000001", (0.000000000000000000001m).ToString ());
			AssertEquals ("22", "0.0000000000000000000001", (0.0000000000000000000001m).ToString ());
			AssertEquals ("23", "0.00000000000000000000001", (0.00000000000000000000001m).ToString ());
			AssertEquals ("24", "0.000000000000000000000001", (0.000000000000000000000001m).ToString ());
			AssertEquals ("25", "0.0000000000000000000000001", (0.0000000000000000000000001m).ToString ());
			AssertEquals ("26", "0.00000000000000000000000001", (0.00000000000000000000000001m).ToString ());
			AssertEquals ("27", "0.000000000000000000000000001", (0.000000000000000000000000001m).ToString ());
			AssertEquals ("28", "0.0000000000000000000000000001", (0.0000000000000000000000000001m).ToString ());
		}

#if NET_2_0
		[Test]
		public void MidpointRoundingAwayFromZero ()
		{
			MidpointRounding m = MidpointRounding.AwayFromZero;
			AssertEquals ("#1", 4, Math.Round (3.5M, m));
			AssertEquals ("#2", 3, Math.Round (2.8M, m));
			AssertEquals ("#3", 3, Math.Round (2.5M, m));
			AssertEquals ("#4", 2, Math.Round (2.1M, m));
			AssertEquals ("#5", -2, Math.Round (-2.1M, m));
			AssertEquals ("#6", -3, Math.Round (-2.5M, m));
			AssertEquals ("#7", -3, Math.Round (-2.8M, m));
			AssertEquals ("#8", -4, Math.Round (-3.5M, m));

			AssertEquals ("#9", 3.1M, Math.Round (3.05M, 1, m));
			AssertEquals ("#10", 2.1M, Math.Round (2.08M, 1, m));
			AssertEquals ("#11", 2.1M, Math.Round (2.05M, 1, m));
			AssertEquals ("#12", 2.0M, Math.Round (2.01M, 1, m));
			AssertEquals ("#13", -2.0M, Math.Round (-2.01M, 1, m));
			AssertEquals ("#14", -2.1M, Math.Round (-2.05M, 1, m));
			AssertEquals ("#15", -2.1M, Math.Round (-2.08M, 1, m));
			AssertEquals ("#16", -3.1M, Math.Round (-3.05M, 1, m));
		}
#endif
	}
}
