// BooleanTest.cs - NUnit Test Cases for the System.Double class
//
// Bob Doan <bdoan@sicompos.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System {

	[TestFixture]
	public class DoubleTest {
		
		private const Double d_zero = 0.0;
		private const Double d_neg = -1234.5678;
		private const Double d_pos = 1234.9999;
		private const Double d_pos2 = 1234.9999;
		private const Double d_nan = Double.NaN;
		private const Double d_pinf = Double.PositiveInfinity;
		private const Double d_ninf = Double.NegativeInfinity;
		private const String s = "What Ever";
		private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
		
		
		private string[] string_values = {
			"1", ".1", "1.1", "-12", "44.444432", ".000021121", 
			"   .00001", "  .223    ", "         -221.3233",
			" 1.7976931348623157e308 ", "+1.7976931348623157E308", "-1.7976931348623157e308",
			"4.9406564584124650e-324",
			"6.28318530717958647692528676655900577",
			"1e-05",
		};
		
		private double[] double_values = {
			1, .1, 1.1, -12, 44.444432, .000021121,
			.00001, .223, -221.3233,
			1.7976931348623157e308, 1.7976931348623157e308, -1.7976931348623157e308,
			4.9406564584124650e-324,
			6.28318530717958647692528676655900577,
			1e-05
		};

		[SetUp]
		public void GetReady()
		{
			string sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
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
			Assertion.AssertEquals("Epsilon Field has wrong value", 3.9406564584124654e-324, Double.Epsilon);
			Assertion.AssertEquals("MaxValue Field has wrong value", 1.7976931348623157e+308, Double.MaxValue);
			Assertion.AssertEquals("MinValue Field has wrong value", -1.7976931348623157e+308, Double.MinValue);
			Assertion.AssertEquals("NegativeInfinity Field has wrong value",  (double)-1.0 / (double)(0.0), Double.NegativeInfinity);		
			Assertion.AssertEquals("PositiveInfinity Field has wrong value",  (double)1.0 / (double)(0.0), Double.PositiveInfinity);		
		}

		[Test]
		public void CompareTo () {
			//If you do int foo =  d_ninf.CompareTo(d_pinf); Assertion.Assert(".." foo < 0, true) this works.... WHY???
			Assertion.Assert("CompareTo Infinity failed", d_ninf.CompareTo(d_pinf) < 0);		

			Assertion.Assert("CompareTo Failed01", d_neg.CompareTo(d_pos) < 0);
			Assertion.Assert("CompareTo NaN Failed", d_nan.CompareTo(d_neg) < 0);				

			Assertion.AssertEquals("CompareTo Failed02", 0, d_pos.CompareTo(d_pos2));		
			Assertion.AssertEquals("CompareTo Failed03", 0, d_pinf.CompareTo(d_pinf));		
			Assertion.AssertEquals("CompareTo Failed04", 0, d_ninf.CompareTo(d_ninf));		
			Assertion.AssertEquals("CompareTo Failed05", 0, d_nan.CompareTo(d_nan));		

			Assertion.Assert("CompareTo Failed06", d_pos.CompareTo(d_neg) > 0);		
			Assertion.Assert("CompareTo Failed07", d_pos.CompareTo(d_nan) > 0);		
			Assertion.Assert("CompareTo Failed08", d_pos.CompareTo(null) > 0);		
			
			try {
				d_pos.CompareTo(s);
				Assertion.Fail("CompareTo should raise a System.ArgumentException");
			}
			catch (Exception e) {
				Assertion.AssertEquals("CompareTo should be a System.ArgumentException", typeof(ArgumentException), e.GetType());
			}		
			
		}

		[Test]
		public void Equals () {
			Assertion.AssertEquals("Equals Failed", true, d_pos.Equals(d_pos2));
			Assertion.AssertEquals("Equals Failed", false, d_pos.Equals(d_neg));
			Assertion.AssertEquals("Equals Failed", false, d_pos.Equals(s));
			
		}

		[Test]
		public void TestTypeCode () {
			Assertion.AssertEquals("GetTypeCode Failed", TypeCode.Double, d_pos.GetTypeCode());		
		}

		[Test]
		public void IsInfinity() {
			Assertion.AssertEquals("IsInfinity Failed", true, Double.IsInfinity(Double.PositiveInfinity));
			Assertion.AssertEquals("IsInfinity Failed", true, Double.IsInfinity(Double.NegativeInfinity));
			Assertion.AssertEquals("IsInfinity Failed", false, Double.IsInfinity(12));		
		}

		[Test]
		public void IsNan() {
			Assertion.AssertEquals("IsNan Failed", true, Double.IsNaN(Double.NaN));
			Assertion.AssertEquals("IsNan Failed", false, Double.IsNaN(12));
			Assertion.AssertEquals("IsNan Failed", false, Double.IsNaN(Double.PositiveInfinity));
		}

		[Test]
		public void IsNegativeInfinity() {
			Assertion.AssertEquals("IsNegativeInfinity Failed", true, Double.IsNegativeInfinity(Double.NegativeInfinity));
			Assertion.AssertEquals("IsNegativeInfinity Failed", false, Double.IsNegativeInfinity(12));		
		}

		[Test]
		public void IsPositiveInfinity() {
			Assertion.AssertEquals("IsPositiveInfinity Failed", true, Double.IsPositiveInfinity(Double.PositiveInfinity));
			Assertion.AssertEquals("IsPositiveInfinity Failed", false, Double.IsPositiveInfinity(12));		
		}

		[Test]
		public void Parse() {
			int i=0;
			try {
				for(i=0;i<string_values.Length;i++) {			
					Assertion.AssertEquals("Parse Failed", double_values[i], Double.Parse(string_values[i]));
				}
			} catch (Exception e) {
				Assertion.Fail("TestParse: i=" + i + " failed with e = " + e.ToString());
			}
			
			try {
				Assertion.AssertEquals("Parse Failed NumberStyles.Float", 10.1111, Double.Parse(" 10.1111 ", NumberStyles.Float, Nfi));
			} catch (Exception e) {
				Assertion.Fail("TestParse: Parse Failed NumberStyles.Float with e = " + e.ToString());
			}

			try {
				Assertion.AssertEquals("Parse Failed NumberStyles.AllowThousands", 1234.5678, Double.Parse("1,234.5678", NumberStyles.Float | NumberStyles.AllowThousands, Nfi));
			} catch (Exception e) {
				Assertion.Fail("TestParse: Parse Failed NumberStyles.AllowThousands with e = " + e.ToString());
			}
		
			try {
				Double.Parse(null);
				Assertion.Fail("Parse should raise a ArgumentNullException");
			}
			catch (Exception e) {
				Assertion.Assert("Parse should be a ArgumentNullException", typeof(ArgumentNullException) == e.GetType());
			}		

			try {
				Double.Parse("save the elk");
				Assertion.Fail("Parse should raise a FormatException");
			}
			catch (Exception e) {
				Assertion.Assert("Parse should be a FormatException", typeof(FormatException) == e.GetType());
			}		

			string sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
			double ovf_plus = 0;
			try {
				ovf_plus = Double.Parse("1" + sep + "79769313486232e308");
				Assertion.Fail("Parse should have raised an OverflowException +");
			}
			catch (Exception e) {
				Assertion.AssertEquals("Should be an OverflowException + for " + ovf_plus, typeof(OverflowException), e.GetType());
			}		

			try {
				Double.Parse("-1" + sep + "79769313486232e308");
				Assertion.Fail("Parse should have raised an OverflowException -");
			}
			catch (Exception e) {
				Assertion.AssertEquals("Should be an OverflowException -", typeof(OverflowException), e.GetType());
			}		
		}

		[Test]
		public void TestToString() {
			//ToString is not yet Implemented......
			//Assertion.AssertEquals("ToString Failed", "1234.9999", d_pos.ToString());
			double d;
			try {
				d = 3.1415;
				d.ToString ("X");
				d.ToString ("D");
				Assertion.Fail("Should have thrown FormatException");
			} catch (FormatException) {
				/* do nothing, this is what we expect */
			} catch (Exception e) {
				Assertion.Fail("Unexpected exception e: " + e);
			}

			CustomFormatHelper();
			
		}
		
		private class Element {
			public double value;
			public string format;
			public string result;
			public Element (double value, string format, string result) {
				this.value = value;
				this.format = format;
				this.result = result;
			}
		}

		public void CustomFormatHelper () {

			NumberFormatInfo nfi = new NumberFormatInfo();

			nfi.NaNSymbol = "NaN";
			nfi.PositiveSign = "+";
			nfi.NegativeSign = "-";
			nfi.PerMilleSymbol = "x";
			nfi.PositiveInfinitySymbol = "Infinity";
			nfi.NegativeInfinitySymbol = "-Infinity";

			nfi.NumberDecimalDigits = 5; 
			nfi.NumberDecimalSeparator = ".";
			nfi.NumberGroupSeparator = ",";
			nfi.NumberGroupSizes = new int[] {3};
			nfi.NumberNegativePattern = 2;

			nfi.CurrencyDecimalDigits = 2;
			nfi.CurrencyDecimalSeparator = ".";
			nfi.CurrencyGroupSeparator = ",";
			nfi.CurrencyGroupSizes = new int[] {3};
			nfi.CurrencyNegativePattern = 8;
			nfi.CurrencyPositivePattern = 3;
			nfi.CurrencySymbol = "$";

			nfi.PercentDecimalDigits = 5; 
			nfi.PercentDecimalSeparator = ".";
			nfi.PercentGroupSeparator = ",";
			nfi.PercentGroupSizes = new int[] {3};
			nfi.PercentNegativePattern = 0;
			nfi.PercentPositivePattern = 0;
			nfi.PercentSymbol = "%";

			ArrayList list = new ArrayList();
			list.Add(new Element(123d, "#####", "123"));
			list.Add(new Element(123d, "00000", "00123"));
			list.Add(new Element(123d, "(###) ### - ####", "()  - 123"));
			list.Add(new Element(123d, "#.##", "123"));
			list.Add(new Element(123d, "0.00", "123.00"));
			list.Add(new Element(123d, "00.00", "123.00"));
			list.Add(new Element(123d, "#,#", "123"));
			list.Add(new Element(123d, "#,,", ""));
			list.Add(new Element(123d, "#,,,", ""));
			list.Add(new Element(123d, "#,##0,,", "0"));
			list.Add(new Element(123d, "#0.##%", "12300%"));
			list.Add(new Element(123d, "0.###E+0", "1.23E+2"));
			list.Add(new Element(123d, "0.###E+000", "1.23E+002"));
			list.Add(new Element(123d, "0.###E-000", "1.23E002"));
			list.Add(new Element(123d, "[##-##-##]", "[-1-23]"));
			list.Add(new Element(123d, "##;(##)", "123"));
			list.Add(new Element(123d, "##;(##)", "123"));
			list.Add(new Element(1234567890d, "#####", "1234567890"));
			list.Add(new Element(1234567890d, "00000", "1234567890"));
			list.Add(new Element(1234567890d,
						"(###) ### - ####", "(123) 456 - 7890"));
			list.Add(new Element(1234567890d, "#.##", "1234567890"));
			list.Add(new Element(1234567890d, "0.00", "1234567890.00"));
			list.Add(new Element(1234567890d, "00.00", "1234567890.00"));
			list.Add(new Element(1234567890d, "#,#", "1,234,567,890"));
			list.Add(new Element(1234567890d, "#,,", "1235"));
			list.Add(new Element(1234567890d, "#,,,", "1"));
			list.Add(new Element(1234567890d, "#,##0,,", "1,235"));
			list.Add(new Element(1234567890d, "#0.##%", "123456789000%"));
			list.Add(new Element(1234567890d, "0.###E+0", "1.235E+9"));
			list.Add(new Element(1234567890d, "0.###E+000", "1.235E+009"));
			list.Add(new Element(1234567890d, "0.###E-000", "1.235E009"));
			list.Add(new Element(1234567890d, "[##-##-##]", "[123456-78-90]"));
			list.Add(new Element(1234567890d, "##;(##)", "1234567890"));
			list.Add(new Element(1234567890d, "##;(##)", "1234567890"));
			list.Add(new Element(1.2d, "#####", "1"));
			list.Add(new Element(1.2d, "00000", "00001"));
			list.Add(new Element(1.2d, "(###) ### - ####", "()  - 1"));
			list.Add(new Element(1.2d, "#.##", "1.2"));
			list.Add(new Element(1.2d, "0.00", "1.20"));
			list.Add(new Element(1.2d, "00.00", "01.20"));
			list.Add(new Element(1.2d, "#,#", "1"));
			list.Add(new Element(1.2d, "#,,", ""));
			list.Add(new Element(1.2d, "#,,,", ""));
			list.Add(new Element(1.2d, "#,##0,,", "0"));
			list.Add(new Element(1.2d, "#0.##%", "120%"));
			list.Add(new Element(1.2d, "0.###E+0", "1.2E+0"));
			list.Add(new Element(1.2d, "0.###E+000", "1.2E+000"));
			list.Add(new Element(1.2d, "0.###E-000", "1.2E000"));
			list.Add(new Element(1.2d, "[##-##-##]", "[--1]"));
			list.Add(new Element(1.2d, "##;(##)", "1"));
			list.Add(new Element(1.2d, "##;(##)", "1"));
			list.Add(new Element(0.086d, "#####", ""));
			list.Add(new Element(0.086d, "00000", "00000"));
			list.Add(new Element(0.086d, "(###) ### - ####", "()  - "));
			list.Add(new Element(0.086d, "#.##", ".09"));
			list.Add(new Element(0.086d, "0.00", "0.09"));
			list.Add(new Element(0.086d, "00.00", "00.09"));
			list.Add(new Element(0.086d, "#,#", ""));
			list.Add(new Element(0.086d, "#,,", ""));
			list.Add(new Element(0.086d, "#,,,", ""));
			list.Add(new Element(0.086d, "#,##0,,", "0"));
			list.Add(new Element(0.086d, "#0.##%", "8.6%"));
			list.Add(new Element(0.086d, "0.###E+0", "8.6E-2"));
			list.Add(new Element(0.086d, "0.###E+000", "8.6E-002"));
			list.Add(new Element(0.086d, "0.###E-000", "8.6E-002"));
			list.Add(new Element(0.086d, "[##-##-##]", "[--]"));
			list.Add(new Element(0.086d, "##;(##)", ""));
			list.Add(new Element(0.086d, "##;(##)", ""));
			list.Add(new Element(86000d, "#####", "86000"));
			list.Add(new Element(86000d, "00000", "86000"));
			list.Add(new Element(86000d, "(###) ### - ####", "() 8 - 6000"));
			list.Add(new Element(86000d, "#.##", "86000"));
			list.Add(new Element(86000d, "0.00", "86000.00"));
			list.Add(new Element(86000d, "00.00", "86000.00"));
			list.Add(new Element(86000d, "#,#", "86,000"));
			list.Add(new Element(86000d, "#,,", ""));
			list.Add(new Element(86000d, "#,,,", ""));
			list.Add(new Element(86000d, "#,##0,,", "0"));
			list.Add(new Element(86000d, "#0.##%", "8600000%"));
			list.Add(new Element(86000d, "0.###E+0", "8.6E+4"));
			list.Add(new Element(86000d, "0.###E+000", "8.6E+004"));
			list.Add(new Element(86000d, "0.###E-000", "8.6E004"));
			list.Add(new Element(86000d, "[##-##-##]", "[8-60-00]"));
			list.Add(new Element(86000d, "##;(##)", "86000"));
			list.Add(new Element(86000d, "##;(##)", "86000"));
			list.Add(new Element(123456d, "#####", "123456"));
			list.Add(new Element(123456d, "00000", "123456"));
			list.Add(new Element(123456d, "(###) ### - ####", "() 12 - 3456"));
			list.Add(new Element(123456d, "#.##", "123456"));
			list.Add(new Element(123456d, "0.00", "123456.00"));
			list.Add(new Element(123456d, "00.00", "123456.00"));
			list.Add(new Element(123456d, "#,#", "123,456"));
			list.Add(new Element(123456d, "#,,", ""));
			list.Add(new Element(123456d, "#,,,", ""));
			list.Add(new Element(123456d, "#,##0,,", "0"));
			list.Add(new Element(123456d, "#0.##%", "12345600%"));
			list.Add(new Element(123456d, "0.###E+0", "1.235E+5"));
			list.Add(new Element(123456d, "0.###E+000", "1.235E+005"));
			list.Add(new Element(123456d, "0.###E-000", "1.235E005"));
			list.Add(new Element(123456d, "[##-##-##]", "[12-34-56]"));
			list.Add(new Element(123456d, "##;(##)", "123456"));
			list.Add(new Element(123456d, "##;(##)", "123456"));
			list.Add(new Element(1234d, "#####", "1234"));
			list.Add(new Element(1234d, "00000", "01234"));
			list.Add(new Element(1234d, "(###) ### - ####", "()  - 1234"));
			list.Add(new Element(1234d, "#.##", "1234"));
			list.Add(new Element(1234d, "0.00", "1234.00"));
			list.Add(new Element(1234d, "00.00", "1234.00"));
			list.Add(new Element(1234d, "#,#", "1,234"));
			list.Add(new Element(1234d, "#,,", ""));
			list.Add(new Element(1234d, "#,,,", ""));
			list.Add(new Element(1234d, "#,##0,,", "0"));
			list.Add(new Element(1234d, "#0.##%", "123400%"));
			list.Add(new Element(1234d, "0.###E+0", "1.234E+3"));
			list.Add(new Element(1234d, "0.###E+000", "1.234E+003"));
			list.Add(new Element(1234d, "0.###E-000", "1.234E003"));
			list.Add(new Element(1234d, "[##-##-##]", "[-12-34]"));
			list.Add(new Element(1234d, "##;(##)", "1234"));
			list.Add(new Element(1234d, "##;(##)", "1234"));
			list.Add(new Element(-1234d, "#####", "-1234"));
			list.Add(new Element(-1234d, "00000", "-01234"));
			list.Add(new Element(-1234d, "(###) ### - ####", "-()  - 1234"));
			list.Add(new Element(-1234d, "#.##", "-1234"));
			list.Add(new Element(-1234d, "0.00", "-1234.00"));
			list.Add(new Element(-1234d, "00.00", "-1234.00"));
			list.Add(new Element(-1234d, "#,#", "-1,234"));
			list.Add(new Element(-1234d, "#,,", ""));
			list.Add(new Element(-1234d, "#,,,", ""));
			list.Add(new Element(-1234d, "#,##0,,", "0"));
			list.Add(new Element(-1234d, "#0.##%", "-123400%"));
			list.Add(new Element(-1234d, "0.###E+0", "-1.234E+3"));
			list.Add(new Element(-1234d, "0.###E+000", "-1.234E+003"));
			list.Add(new Element(-1234d, "0.###E-000", "-1.234E003"));
			list.Add(new Element(-1234d, "[##-##-##]", "-[-12-34]"));
			list.Add(new Element(-1234d, "##;(##)", "(1234)"));
			list.Add(new Element(-1234d, "##;(##)", "(1234)"));
			list.Add(new Element(12345678901234567890.123d,
						"#####", "12345678901234600000"));
			list.Add(new Element(12345678901234567890.123d,
						"00000", "12345678901234600000"));
			list.Add(new Element(12345678901234567890.123d,
						"(###) ### - ####", "(1234567890123) 460 - 0000"));
			list.Add(new Element(12345678901234567890.123d,
						"#.##", "12345678901234600000"));
			list.Add(new Element(12345678901234567890.123d,
						"0.00", "12345678901234600000.00"));
			list.Add(new Element(12345678901234567890.123d,
						"00.00", "12345678901234600000.00"));
			list.Add(new Element(12345678901234567890.123d,
						"#,#", "12,345,678,901,234,600,000"));
			list.Add(new Element(12345678901234567890.123d,
						"#,,", "12345678901235"));
			list.Add(new Element(12345678901234567890.123d,
						"#,,,", "12345678901"));
			list.Add(new Element(12345678901234567890.123d,
						"#,##0,,", "12,345,678,901,235"));
			list.Add(new Element(12345678901234567890.123d,
						"#0.##%", "1234567890123460000000%"));
			list.Add(new Element(12345678901234567890.123d,
						"0.###E+0", "1.235E+19"));
			list.Add(new Element(12345678901234567890.123d,
						"0.###E+000", "1.235E+019"));
			list.Add(new Element(12345678901234567890.123d,
						"0.###E-000", "1.235E019"));
			list.Add(new Element(12345678901234567890.123d,
						"[##-##-##]", "[1234567890123460-00-00]"));
			list.Add(new Element(12345678901234567890.123d,
						"##;(##)", "12345678901234600000"));
			list.Add(new Element(12345678901234567890.123d,
						"##;(##)", "12345678901234600000"));
			foreach (Element e in list) {
				Assertion.AssertEquals(
						"ToString Failed: '" + e.value + "' Should be \"" + e.result + "\" with \"" + e.format + "\"",
						e.result, e.value.ToString(e.format, nfi));
			}
			
		}

	}
	
}
