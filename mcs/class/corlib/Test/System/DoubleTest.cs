// DoubleTest.cs - NUnit Test Cases for the System.Double class
//
// Bob Doan <bdoan@sicompos.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class DoubleTest
	{
		private const Double d_zero = 0.0;
		private const Double d_neg = -1234.5678;
		private const Double d_pos = 1234.9999;
		private const Double d_pos2 = 1234.9999;
		private const Double d_nan = Double.NaN;
		private const Double d_pinf = Double.PositiveInfinity;
		private const Double d_ninf = Double.NegativeInfinity;
		private const String s = "What Ever";
		private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;

		private string [] string_values;
		private string [] string_values_fail = {
			"",     // empty
			"- 1.0", // Inner whitespace
			"3 5" // Inner whitespace 2
		};

		private double [] double_values = {
			1, .1, 1.1, -12, 44.444432, .000021121,
			.00001, .223, -221.3233,
			1.7976931348623157e308, 1.7976931348623157e308, -1.7976931348623157e308,
			4.9406564584124650e-324,
			6.28318530717958647692528676655900577,
			1e-05,
			0,
		};

		[SetUp]
		public void Setup ()
		{
			string sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
			string_values = new string [15];
			string_values [0] = "1";
			string_values [1] = sep + "1";
			string_values [2] = "1" + sep + "1";
			string_values [3] = "-12";
			string_values [4] = "44" + sep + "444432";
			string_values [5] = sep + "000021121";
			string_values [6] = "   " + sep + "00001";
			string_values [7] = "  " + sep + "223    ";
			string_values [8] = "         -221" + sep + "3233";
			string_values [9] = " 1" + sep + "7976931348623157e308 ";
			string_values [10] = "+1" + sep + "7976931348623157E308";
			string_values [11] = "-1" + sep + "7976931348623157e308";
			string_values [12] = "4" + sep + "9406564584124650e-324";
			string_values [13] = "6" + sep + "28318530717958647692528676655900577";
			string_values [14] = "1e-05";
		}

		[Test]
		public void PublicFields ()
		{
			Assert.AreEqual (3.9406564584124654e-324, Double.Epsilon, "#1");
			Assert.AreEqual (1.7976931348623157e+308, Double.MaxValue, "#2");
			Assert.AreEqual (-1.7976931348623157e+308, Double.MinValue, "#3");
			Assert.AreEqual ((double) -1.0 / (double) (0.0), Double.NegativeInfinity, "#4");
			Assert.AreEqual ((double) 1.0 / (double) (0.0), Double.PositiveInfinity, "#5");
		}

		[Test]
		public void CompareTo ()
		{
			//If you do int foo =  d_ninf.CompareTo(d_pinf); Assertion.Assert(".." foo < 0, true) this works.... WHY???
			Assert.IsTrue (d_ninf.CompareTo (d_pinf) < 0, "#A1");
			Assert.IsTrue (d_neg.CompareTo (d_pos) < 0, "#A2");
			Assert.IsTrue (d_nan.CompareTo (d_neg) < 0, "#A3");

			Assert.AreEqual (0, d_pos.CompareTo (d_pos2), "#B1");
			Assert.AreEqual (0, d_pinf.CompareTo (d_pinf), "#B2");
			Assert.AreEqual (0, d_ninf.CompareTo (d_ninf), "#B3");
			Assert.AreEqual (0, d_nan.CompareTo (d_nan), "#B4");

			Assert.IsTrue (d_pos.CompareTo (d_neg) > 0, "#C1");
			Assert.IsTrue (d_pos.CompareTo (d_nan) > 0, "#C2");
			Assert.IsTrue (d_pos.CompareTo (null) > 0, "#C3");

			try {
				d_pos.CompareTo (s);
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
			}
		}

		[Test]
		public void Equals ()
		{
			Assert.IsTrue (d_pos.Equals (d_pos2), "#1");
			Assert.IsFalse (d_pos.Equals (d_neg), "#2");
			Assert.IsFalse (d_pos.Equals (s), "#3");
		}

		[Test]
		public void TestTypeCode ()
		{
			Assert.AreEqual (TypeCode.Double, d_pos.GetTypeCode ());
		}

		[Test]
		public void IsInfinity ()
		{
			Assert.IsTrue (Double.IsInfinity (Double.PositiveInfinity), "#1");
			Assert.IsTrue (Double.IsInfinity (Double.NegativeInfinity), "#2");
			Assert.IsFalse (Double.IsInfinity (12), "#3");
		}

		[Test]
		public void IsNan ()
		{
			Assert.IsTrue (Double.IsNaN (Double.NaN), "#1");
			Assert.IsFalse (Double.IsNaN (12), "#2");
			Assert.IsFalse (Double.IsNaN (Double.PositiveInfinity), "#3");
		}

		[Test]
		public void IsNegativeInfinity ()
		{
			Assert.IsTrue (Double.IsNegativeInfinity (Double.NegativeInfinity), "#1");
			Assert.IsFalse (Double.IsNegativeInfinity (12), "#2");
		}

		[Test]
		public void IsPositiveInfinity ()
		{
			Assert.IsTrue (Double.IsPositiveInfinity (Double.PositiveInfinity), "#1");
			Assert.IsFalse (Double.IsPositiveInfinity (12), "#2");
		}

		[Test]
		public void Parse ()
		{
			int i = 0;
			try {
				for (i = 0; i < string_values.Length; i++) {
					Assert.AreEqual (double_values [i], Double.Parse (string_values [i]), "#A1");
				}
			} catch (Exception e) {
				Assert.Fail ("#A2: i=" + i + " failed with e = " + e.ToString ());
			}

			try {
				Assert.AreEqual (10.1111, Double.Parse (" 10.1111 ", NumberStyles.Float, Nfi), "#B1");
			} catch (Exception e) {
				Assert.Fail ("#B2: Parse Failed NumberStyles.Float with e = " + e.ToString ());
			}

			try {
				Assert.AreEqual (1234.5678, Double.Parse ("1,234.5678", NumberStyles.Float | NumberStyles.AllowThousands, Nfi), "#C1");
			} catch (Exception e) {
				Assert.Fail ("#C2: Parse Failed NumberStyles.AllowThousands with e = " + e.ToString ());
			}

			try {
				Double.Parse (null);
				Assert.Fail ("#D1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
			}

			try {
				Double.Parse ("save the elk");
				Assert.Fail ("#E1");
			} catch (FormatException ex) {
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
			}

			string sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
			double ovf_plus = 0;
			try {
				ovf_plus = Double.Parse ("1" + sep + "79769313486232e308");
				Assert.Fail ("#F1");
			} catch (OverflowException ex) {
				Assert.AreEqual (typeof (OverflowException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
			}

			try {
				Double.Parse ("-1" + sep + "79769313486232e308");
				Assert.Fail ("#G1");
			} catch (OverflowException ex) {
				Assert.AreEqual (typeof (OverflowException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
			}

			for (i = 0; i < string_values_fail.Length; ++i) {
				try {
					Double.Parse (string_values_fail [i]);
					Assert.Fail ("#H1: " + string_values_fail [i]);
				} catch (FormatException ex) {
					Assert.AreEqual (typeof (FormatException), ex.GetType (), "#H2");
					Assert.IsNull (ex.InnerException, "#H3");
					Assert.IsNotNull (ex.Message, "#H4");
				}
			}
		}

		[Test]
		public void ParseAllowWhitespaces ()
		{
			var nf = CultureInfo.CurrentCulture.NumberFormat;
			NumberStyles style = NumberStyles.Float;
			double.Parse (" 32 ");
			double.Parse (string.Format ("  {0}  ", nf.PositiveInfinitySymbol));
			double.Parse (string.Format ("  {0}  ", nf.NegativeInfinitySymbol));
			double.Parse (string.Format ("  {0}  ", nf.NaNSymbol));
		}

		[Test] // bug #81630
		public void Parse_Whitespace ()
		{
			try {
				double.Parse (" ");
				Assert.Fail ("#1");
			} catch (FormatException ex) {
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // //bug #81777
		public void Parse_TrailingGarbage ()
		{
			try {
				double.Parse ("22 foo");
				Assert.Fail ("#1");
			} catch (FormatException ex) {
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Parse_Infinity ()
		{
			double value;
			IFormatProvider german = new CultureInfo ("de-DE");
			var res = double.Parse ("+unendlich", NumberStyles.Float, german);
			Assert.AreEqual (double.PositiveInfinity, res);
		}

		[Test]
		public void TestToString ()
		{
			try {
				double d = 3.1415;
				d.ToString ("X");
				Assert.Fail ("#A1");
			} catch (FormatException ex) {
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				double d = 3.1415;
				d.ToString ("D");
				Assert.Fail ("#B1");
			} catch (FormatException ex) {
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			CustomFormatHelper ();
		}

		private class Element
		{
			public double value;
			public string format;
			public string result;

			public Element (double value, string format, string result)
			{
				this.value = value;
				this.format = format;
				this.result = result;
			}
		}

		public void CustomFormatHelper ()
		{
			NumberFormatInfo nfi = new NumberFormatInfo ();

			nfi.NaNSymbol = "NaN";
			nfi.PositiveSign = "+";
			nfi.NegativeSign = "-";
			nfi.PerMilleSymbol = "x";
			nfi.PositiveInfinitySymbol = "Infinity";
			nfi.NegativeInfinitySymbol = "-Infinity";

			nfi.NumberDecimalDigits = 5;
			nfi.NumberDecimalSeparator = ".";
			nfi.NumberGroupSeparator = ",";
			nfi.NumberGroupSizes = new int [] { 3 };
			nfi.NumberNegativePattern = 2;

			nfi.CurrencyDecimalDigits = 2;
			nfi.CurrencyDecimalSeparator = ".";
			nfi.CurrencyGroupSeparator = ",";
			nfi.CurrencyGroupSizes = new int [] { 3 };
			nfi.CurrencyNegativePattern = 8;
			nfi.CurrencyPositivePattern = 3;
			nfi.CurrencySymbol = "$";

			nfi.PercentDecimalDigits = 5;
			nfi.PercentDecimalSeparator = ".";
			nfi.PercentGroupSeparator = ",";
			nfi.PercentGroupSizes = new int [] { 3 };
			nfi.PercentNegativePattern = 0;
			nfi.PercentPositivePattern = 0;
			nfi.PercentSymbol = "%";

			ArrayList list = new ArrayList ();
			list.Add (new Element (123d, "#####", "123"));
			list.Add (new Element (123d, "00000", "00123"));
			list.Add (new Element (123d, "(###) ### - ####", "()  - 123"));
			list.Add (new Element (123d, "#.##", "123"));
			list.Add (new Element (123d, "0.00", "123.00"));
			list.Add (new Element (123d, "00.00", "123.00"));
			list.Add (new Element (123d, "#,#", "123"));
			list.Add (new Element (123d, "#,,", ""));
			list.Add (new Element (123d, "#,,,", ""));
			list.Add (new Element (123d, "#,##0,,", "0"));
			list.Add (new Element (123d, "#0.##%", "12300%"));
			list.Add (new Element (123d, "0.###E+0", "1.23E+2"));
			list.Add (new Element (123d, "0.###E+000", "1.23E+002"));
			list.Add (new Element (123d, "0.###E-000", "1.23E002"));
			list.Add (new Element (123d, "[##-##-##]", "[-1-23]"));
			list.Add (new Element (123d, "##;(##)", "123"));
			list.Add (new Element (123d, "##;(##)", "123"));
			list.Add (new Element (1234567890d, "#####", "1234567890"));
			list.Add (new Element (1234567890d, "00000", "1234567890"));
			list.Add (new Element (1234567890d,
						"(###) ### - ####", "(123) 456 - 7890"));
			list.Add (new Element (1234567890d, "#.##", "1234567890"));
			list.Add (new Element (1234567890d, "0.00", "1234567890.00"));
			list.Add (new Element (1234567890d, "00.00", "1234567890.00"));
			list.Add (new Element (1234567890d, "#,#", "1,234,567,890"));
			list.Add (new Element (1234567890d, "#,,", "1235"));
			list.Add (new Element (1234567890d, "#,,,", "1"));
			list.Add (new Element (1234567890d, "#,##0,,", "1,235"));
			list.Add (new Element (1234567890d, "#0.##%", "123456789000%"));
			list.Add (new Element (1234567890d, "0.###E+0", "1.235E+9"));
			list.Add (new Element (1234567890d, "0.###E+000", "1.235E+009"));
			list.Add (new Element (1234567890d, "0.###E-000", "1.235E009"));
			list.Add (new Element (1234567890d, "[##-##-##]", "[123456-78-90]"));
			list.Add (new Element (1234567890d, "##;(##)", "1234567890"));
			list.Add (new Element (1234567890d, "##;(##)", "1234567890"));
			list.Add (new Element (1.2d, "#####", "1"));
			list.Add (new Element (1.2d, "00000", "00001"));
			list.Add (new Element (1.2d, "(###) ### - ####", "()  - 1"));
			list.Add (new Element (1.2d, "#.##", "1.2"));
			list.Add (new Element (1.2d, "0.00", "1.20"));
			list.Add (new Element (1.2d, "00.00", "01.20"));
			list.Add (new Element (1.2d, "#,#", "1"));
			list.Add (new Element (1.2d, "#,,", ""));
			list.Add (new Element (1.2d, "#,,,", ""));
			list.Add (new Element (1.2d, "#,##0,,", "0"));
			list.Add (new Element (1.2d, "#0.##%", "120%"));
			list.Add (new Element (1.2d, "0.###E+0", "1.2E+0"));
			list.Add (new Element (1.2d, "0.###E+000", "1.2E+000"));
			list.Add (new Element (1.2d, "0.###E-000", "1.2E000"));
			list.Add (new Element (1.2d, "[##-##-##]", "[--1]"));
			list.Add (new Element (1.2d, "##;(##)", "1"));
			list.Add (new Element (1.2d, "##;(##)", "1"));
			list.Add (new Element (0.086d, "#####", ""));
			list.Add (new Element (0.086d, "00000", "00000"));
			list.Add (new Element (0.086d, "(###) ### - ####", "()  - "));
			list.Add (new Element (0.086d, "#.##", ".09"));
			list.Add (new Element (0.086d, "#.#", ".1"));
			list.Add (new Element (0.086d, "0.00", "0.09"));
			list.Add (new Element (0.086d, "00.00", "00.09"));
			list.Add (new Element (0.086d, "#,#", ""));
			list.Add (new Element (0.086d, "#,,", ""));
			list.Add (new Element (0.086d, "#,,,", ""));
			list.Add (new Element (0.086d, "#,##0,,", "0"));
			list.Add (new Element (0.086d, "#0.##%", "8.6%"));
			list.Add (new Element (0.086d, "0.###E+0", "8.6E-2"));
			list.Add (new Element (0.086d, "0.###E+000", "8.6E-002"));
			list.Add (new Element (0.086d, "0.###E-000", "8.6E-002"));
			list.Add (new Element (0.086d, "[##-##-##]", "[--]"));
			list.Add (new Element (0.086d, "##;(##)", ""));
			list.Add (new Element (0.086d, "##;(##)", ""));
			list.Add (new Element (86000d, "#####", "86000"));
			list.Add (new Element (86000d, "00000", "86000"));
			list.Add (new Element (86000d, "(###) ### - ####", "() 8 - 6000"));
			list.Add (new Element (86000d, "#.##", "86000"));
			list.Add (new Element (86000d, "0.00", "86000.00"));
			list.Add (new Element (86000d, "00.00", "86000.00"));
			list.Add (new Element (86000d, "#,#", "86,000"));
			list.Add (new Element (86000d, "#,,", ""));
			list.Add (new Element (86000d, "#,,,", ""));
			list.Add (new Element (86000d, "#,##0,,", "0"));
			list.Add (new Element (86000d, "#0.##%", "8600000%"));
			list.Add (new Element (86000d, "0.###E+0", "8.6E+4"));
			list.Add (new Element (86000d, "0.###E+000", "8.6E+004"));
			list.Add (new Element (86000d, "0.###E-000", "8.6E004"));
			list.Add (new Element (86000d, "[##-##-##]", "[8-60-00]"));
			list.Add (new Element (86000d, "##;(##)", "86000"));
			list.Add (new Element (86000d, "##;(##)", "86000"));
			list.Add (new Element (123456d, "#####", "123456"));
			list.Add (new Element (123456d, "00000", "123456"));
			list.Add (new Element (123456d, "(###) ### - ####", "() 12 - 3456"));
			list.Add (new Element (123456d, "#.##", "123456"));
			list.Add (new Element (123456d, "0.00", "123456.00"));
			list.Add (new Element (123456d, "00.00", "123456.00"));
			list.Add (new Element (123456d, "#,#", "123,456"));
			list.Add (new Element (123456d, "#,,", ""));
			list.Add (new Element (123456d, "#,,,", ""));
			list.Add (new Element (123456d, "#,##0,,", "0"));
			list.Add (new Element (123456d, "#0.##%", "12345600%"));
			list.Add (new Element (123456d, "0.###E+0", "1.235E+5"));
			list.Add (new Element (123456d, "0.###E+000", "1.235E+005"));
			list.Add (new Element (123456d, "0.###E-000", "1.235E005"));
			list.Add (new Element (123456d, "[##-##-##]", "[12-34-56]"));
			list.Add (new Element (123456d, "##;(##)", "123456"));
			list.Add (new Element (123456d, "##;(##)", "123456"));
			list.Add (new Element (1234d, "#####", "1234"));
			list.Add (new Element (1234d, "00000", "01234"));
			list.Add (new Element (1234d, "(###) ### - ####", "()  - 1234"));
			list.Add (new Element (1234d, "#.##", "1234"));
			list.Add (new Element (1234d, "0.00", "1234.00"));
			list.Add (new Element (1234d, "00.00", "1234.00"));
			list.Add (new Element (1234d, "#,#", "1,234"));
			list.Add (new Element (1234d, "#,,", ""));
			list.Add (new Element (1234d, "#,,,", ""));
			list.Add (new Element (1234d, "#,##0,,", "0"));
			list.Add (new Element (1234d, "#0.##%", "123400%"));
			list.Add (new Element (1234d, "0.###E+0", "1.234E+3"));
			list.Add (new Element (1234d, "0.###E+000", "1.234E+003"));
			list.Add (new Element (1234d, "0.###E-000", "1.234E003"));
			list.Add (new Element (1234d, "[##-##-##]", "[-12-34]"));
			list.Add (new Element (1234d, "##;(##)", "1234"));
			list.Add (new Element (1234d, "##;(##)", "1234"));
			list.Add (new Element (-1234d, "#####", "-1234"));
			list.Add (new Element (-1234d, "00000", "-01234"));
			list.Add (new Element (-1234d, "(###) ### - ####", "-()  - 1234"));
			list.Add (new Element (-1234d, "#.##", "-1234"));
			list.Add (new Element (-1234d, "0.00", "-1234.00"));
			list.Add (new Element (-1234d, "00.00", "-1234.00"));
			list.Add (new Element (-1234d, "#,#", "-1,234"));
			list.Add (new Element (-1234d, "#,,", ""));
			list.Add (new Element (-1234d, "#,,,", ""));
			list.Add (new Element (-1234d, "#,##0,,", "0"));
			list.Add (new Element (-1234d, "#0.##%", "-123400%"));
			list.Add (new Element (-1234d, "0.###E+0", "-1.234E+3"));
			list.Add (new Element (-1234d, "0.###E+000", "-1.234E+003"));
			list.Add (new Element (-1234d, "0.###E-000", "-1.234E003"));
			list.Add (new Element (-1234d, "[##-##-##]", "-[-12-34]"));
			list.Add (new Element (-1234d, "##;(##)", "(1234)"));
			list.Add (new Element (-1234d, "##;(##)", "(1234)"));
			list.Add (new Element (12345678901234567890.123d,
						"#####", "12345678901234600000"));
			list.Add (new Element (12345678901234567890.123d,
						"00000", "12345678901234600000"));
			list.Add (new Element (12345678901234567890.123d,
						"(###) ### - ####", "(1234567890123) 460 - 0000"));
			list.Add (new Element (12345678901234567890.123d,
						"#.##", "12345678901234600000"));
			list.Add (new Element (12345678901234567890.123d,
						"0.00", "12345678901234600000.00"));
			list.Add (new Element (12345678901234567890.123d,
						"00.00", "12345678901234600000.00"));
			list.Add (new Element (12345678901234567890.123d,
						"#,#", "12,345,678,901,234,600,000"));
			list.Add (new Element (12345678901234567890.123d,
						"#,,", "12345678901235"));
			list.Add (new Element (12345678901234567890.123d,
						"#,,,", "12345678901"));
			list.Add (new Element (12345678901234567890.123d,
						"#,##0,,", "12,345,678,901,235"));
			list.Add (new Element (12345678901234567890.123d,
						"#0.##%", "1234567890123460000000%"));
			list.Add (new Element (12345678901234567890.123d,
						"0.###E+0", "1.235E+19"));
			list.Add (new Element (12345678901234567890.123d,
						"0.###E+000", "1.235E+019"));
			list.Add (new Element (12345678901234567890.123d,
						"0.###E-000", "1.235E019"));
			list.Add (new Element (12345678901234567890.123d,
						"[##-##-##]", "[1234567890123460-00-00]"));
			list.Add (new Element (12345678901234567890.123d,
						"##;(##)", "12345678901234600000"));
			list.Add (new Element (12345678901234567890.123d,
						"##;(##)", "12345678901234600000"));
			foreach (Element e in list) {
				Assert.AreEqual (e.result, e.value.ToString (e.format, nfi),
					"ToString Failed: '" + e.value + "' Should be \"" + e.result + "\" with \"" + e.format + "\"");
			}
		}

		[Test]
		public void ToString_Defaults ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("nl-BE");
			try {
				Double i = 254.9d;
				// everything defaults to "G"
				string def = i.ToString ("G");
				Assert.AreEqual (def, i.ToString (), "#1");
				Assert.AreEqual (def, i.ToString ((IFormatProvider) null), "#2");
				Assert.AreEqual (def, i.ToString ((string) null), "#3");
				Assert.AreEqual (def, i.ToString (String.Empty), "#4");
				Assert.AreEqual (def, i.ToString (null, null), "#5");
				Assert.AreEqual (def, i.ToString (String.Empty, null), "#6");
				Assert.AreEqual ("254,9", def, "#7");
			} finally {
				// restore original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		public void TestRoundtrip () // bug #320433
		{
			Assert.AreEqual ("10.78", 10.78.ToString ("R", NumberFormatInfo.InvariantInfo));
		}

		[Test] // bug #72955
		public void LongLongValueRoundtrip ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("nl-BE");
			try {
				double d = 0.0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000222;
				Assert.AreEqual ("1,97626258336499E-323", d.ToString ("R"));
			} finally {
				// restore original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HexNumber_WithHexToParse ()
		{
			// from bug #72221
			double d;
			Assert.IsTrue (Double.TryParse ("0dead", NumberStyles.HexNumber, null, out d), "#1");
			Assert.AreEqual (57842, d, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HexNumber_NoHexToParse ()
		{
			double d;
			Assert.IsTrue (Double.TryParse ("0", NumberStyles.HexNumber, null, out d), "#1");
			Assert.AreEqual (0, d, "#2");
		}

		[Test]
		public void TryParseBug78546 ()
		{
			double value;
			Assert.IsFalse (Double.TryParse ("error", NumberStyles.Integer,
				null, out value));
		}

		[Test]
		public void TryParse_NonDigitStrings ()
		{
			double value;
			Assert.IsFalse (Double.TryParse ("string", NumberStyles.Any, null, out value), "#1");
			Assert.IsFalse (Double.TryParse ("with whitespace", NumberStyles.Any, null, out value), "#2");
			
			Assert.IsFalse (Double.TryParse ("string", out value), "#3");
			Assert.IsFalse (Double.TryParse ("with whitespace", out value), "#4");
		}

					
		[Test] // bug #77721
		public void ParseCurrency ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			try {
				NumberFormatInfo f = NumberFormatInfo.CurrentInfo;
				double d = double.Parse ("$4.56", NumberStyles.Currency, f);
				Assert.AreEqual (4.56, d);
			} finally {
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		public void ParseEmptyNumberGroupSeparator ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			try {
				var nf = new NumberFormatInfo ();
				nf.NumberDecimalSeparator = ".";
				nf.NumberGroupSeparator = "";
				double d = double.Parse ("4.5", nf);
				Assert.AreEqual (4.5, d);
			} finally {
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}
	}
}
