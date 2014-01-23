// Int64Test.cs - NUnit Test Cases for the System.Int64 struct
//
// Author: Martin Weindel (martin.weindel@t-online.de)
//
// (C) Martin Weindel, 2001
// 
// tests ToString and Parse function with the culture independent 
// NumberFormatInfo.InvariantInfo

using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;

    /// <summary>
    /// Tests for System.Int64
    /// </summary>
namespace MonoTests.System
{

[TestFixture]
public class Int64Test 
{
	private const Int64 MyInt64_1 = -42;
	private const Int64 MyInt64_2 = -9223372036854775808;
	private const Int64 MyInt64_3 = 9223372036854775807;
	private const string MyString1 = "-42";
	private const string MyString2 = "-9223372036854775808";
	private const string MyString3 = "9223372036854775807";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {"", "-9223372036854775808", "-9.223372e+018", "-9223372036854775808.00",
	                                  "-9223372036854775808", "-9,223,372,036,854,775,808.00", "-922,337,203,685,477,580,800.00 %", "8000000000000000"};
	private string[] Results2 = {"", "9223372036854775807", "9.22337e+018", "9223372036854775807.00000",
	                                  "9.2234e+18", "9,223,372,036,854,775,807.00000", "922,337,203,685,477,580,700.00000 %", "7fffffffffffffff"};
	private string[] ResultsNfi1 = {"("+NumberFormatInfo.InvariantInfo.CurrencySymbol+"9,223,372,036,854,775,808.00)", "-9223372036854775808", "-9.223372e+018", "-9223372036854775808.00",
	                                  "-9223372036854775808", "-9,223,372,036,854,775,808.00", "-922,337,203,685,477,580,800.00 %", "8000000000000000"};
	private string[] ResultsNfi2 = {""+NumberFormatInfo.InvariantInfo.CurrencySymbol+"9,223,372,036,854,775,807.00000", "9223372036854775807", "9.22337e+018", "9223372036854775807.00000",
	                                  "9.2234e+18", "9,223,372,036,854,775,807.00000", "922,337,203,685,477,580,700.00000 %", "7fffffffffffffff"};

	private long[] vals
        = { 0, Int64.MaxValue, Int64.MinValue,
              1L, 12L, 123L, 1234L, -123L, 
              1234567890123456L, 6543210987654321L };

	private const long val1 = -1234567L;
	private const long val2 = 1234567L;
	private const string sval1Test1 = "  -1,234,567   ";
	private const string sval1Test2 = "  -1234567   ";
	//private const string sval1Test3 = "  -12345,,,,67   "; // interesting: this case works on SDK Beta2, but the specification says nothing about this case
	private const string sval1Test4 = "  -12345 67   ";
	private  string sval1Test5 = "  -"+NumberFormatInfo.InvariantInfo.CurrencySymbol+"1,234,567.00 ";
	private  string sval1Test6 = "("+NumberFormatInfo.InvariantInfo.CurrencySymbol+"1,234,567.00)";
	private const string sval1Test7 = "-1,234,567.00";
	private const string sval1UserPercent1 = "-%%%1~2~3~4~5~6~7~0~0;0";
	private const string sval2UserPercent1 = "%%%1~2~3~4~5~6~7~0~0;0";
	private const NumberStyles style1 =  NumberStyles.AllowLeadingWhite | NumberStyles.AllowLeadingSign
					| NumberStyles.AllowTrailingWhite | NumberStyles.AllowThousands;
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	private NumberFormatInfo NfiUser;

	private CultureInfo old_culture;

	[SetUp]
	public void SetUp () 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		int cdd = NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits;
		string csym = NumberFormatInfo.CurrentInfo.CurrencySymbol;
		string csuffix = (cdd > 0 ? "." : "").PadRight(cdd + (cdd > 0 ? 1 : 0), '0');
		
		string decimals = new String ('0', NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
		string perPattern = new string[] {"n %","n%","%n"} [NumberFormatInfo.CurrentInfo.PercentPositivePattern];
		
		Results1[0] = "(" + csym + "9,223,372,036,854,775,808" + csuffix + ")";
		Results1[3] = "-9223372036854775808." + decimals;
		Results1[5] = "-9,223,372,036,854,775,808." + decimals;
		Results1[6] = perPattern.Replace ("n","-922,337,203,685,477,580,800.00");
		
		Results2[0] = csym + "9,223,372,036,854,775,807.00000";
		Results2[6] = perPattern.Replace ("n","922,337,203,685,477,580,700.00000");
		
		NfiUser = new NumberFormatInfo();
		NfiUser.CurrencyDecimalDigits = 3;
		NfiUser.CurrencyDecimalSeparator = ":";
		NfiUser.CurrencyGroupSeparator = "/";
		NfiUser.CurrencyGroupSizes = new int[] { 2,1,0 };
		NfiUser.CurrencyNegativePattern = 10;  // n $-
		NfiUser.CurrencyPositivePattern = 3;  // n $
		NfiUser.CurrencySymbol = "XYZ";
		NfiUser.PercentDecimalDigits = 1;
		NfiUser.PercentDecimalSeparator = ";";
		NfiUser.PercentGroupSeparator = "~";
		NfiUser.PercentGroupSizes = new int[] {1};
		NfiUser.PercentNegativePattern = 2;
		NfiUser.PercentPositivePattern = 2;
		NfiUser.PercentSymbol = "%%%";
	}

	[TearDown]
	public void TearDown ()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	[Test]
	public void TestMinMax()
	{
		
		Assert.AreEqual(Int64.MinValue, MyInt64_2);
		Assert.AreEqual(Int64.MaxValue, MyInt64_3);
	}

	[Test]	
	public void TestCompareTo()
	{
		Assert.IsTrue(MyInt64_3.CompareTo(MyInt64_2) > 0);
		Assert.IsTrue(MyInt64_2.CompareTo(MyInt64_2) == 0);
		Assert.IsTrue(MyInt64_1.CompareTo((object)(Int64)(-42)) == 0);
		Assert.IsTrue(MyInt64_2.CompareTo(MyInt64_3) < 0);
		try {
			MyInt64_2.CompareTo((object)(Int16)100);
			Assert.Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentException) == e.GetType());
		}
	}

	[Test]
	public void TestEquals()
	{
		Assert.IsTrue(MyInt64_1.Equals(MyInt64_1));
		Assert.IsTrue(MyInt64_1.Equals((object)(Int64)(-42)));
		Assert.IsTrue(MyInt64_1.Equals((object)(SByte)(-42)) == false);
		Assert.IsTrue(MyInt64_1.Equals(MyInt64_2) == false);
	}

	[Test]	
	public void TestGetHashCode()
	{
		try {
			MyInt64_1.GetHashCode();
			MyInt64_2.GetHashCode();
			MyInt64_3.GetHashCode();
		}
		catch {
			Assert.Fail("GetHashCode should not raise an exception here");
		}
	}

	[Test]	
    public void TestRoundTripGeneral() 
    {
        foreach(long lv in vals) 
        {
            string s = lv.ToString(Nfi);
            long lv2 = Int64.Parse(s);
            Assert.IsTrue(lv == lv2);
            long lv3 = Int64.Parse(s, NumberStyles.Integer, Nfi);
            Assert.IsTrue(lv == lv3);
        }
    }

	[Test]
    public void TestRoundTripHex() 
    {
        foreach(long lv in vals) 
        {
            string s = lv.ToString("x", Nfi);
            long lv2 = Int64.Parse(s, NumberStyles.HexNumber, Nfi);
            Assert.IsTrue(lv == lv2);
        }
    }

	[Test]
    public void TestParseNull()
    {
        try 
        {
            Int64.Parse(null);
            Assert.Fail("Should raise System.ArgumentNullException"); 
        } 
        catch (ArgumentNullException) 
        {
            // ok
        }
    }

	[Test]
    public void TestParse()
    {
        long lv;

        lv = Int64.Parse(sval1Test1, style1, Nfi);
        Assert.AreEqual(val1, lv, "Long value should be equal for Test1");

	try
        {
            lv = Int64.Parse(sval1Test1, Nfi);
            Assert.Fail("Should raise FormatException 1");
        }
        catch (FormatException)
        {
            // ok
        }

        lv = Int64.Parse(sval1Test2, style1, Nfi);
        Assert.AreEqual(val1, lv, "Value should be the same for Test2 with style1");
        lv = Int64.Parse(sval1Test2, Nfi);
        Assert.AreEqual(val1, lv, "Value should be the same for Test2 without style1");

	try
        {
            lv = Int64.Parse(sval1Test4, style1, Nfi);
            Assert.Fail("Should raise FormatException 3");
        }
        catch (FormatException)
        {
            // ok
        }

        lv = Int64.Parse(sval1Test5, NumberStyles.Currency, Nfi);
        Assert.AreEqual(val1, lv, "Value should be the same for Test5 and currency style");

	//test Parse(string s)
	Assert.IsTrue(MyInt64_1 == Int64.Parse(MyString1));
	Assert.IsTrue(MyInt64_2 == Int64.Parse(MyString2));
	Assert.IsTrue(MyInt64_3 == Int64.Parse(MyString3));
	try {
		Int64.Parse(null);
		Assert.Fail("#1:Should raise a System.ArgumentNullException");
	}
	catch (Exception e) {
		Assert.IsTrue(typeof(ArgumentNullException) == e.GetType(), "#2");
	}
	try {
		Int64.Parse("not-a-number");
		Assert.Fail("#3:Should raise a System.FormatException");
	}
	catch (Exception e) {
		Assert.IsTrue(typeof(FormatException) == e.GetType(), "#4");
	}
	//test Parse(string s, NumberStyles style)
	try {
		double OverInt = (double)Int64.MaxValue + 1;
		Int64.Parse(OverInt.ToString(), NumberStyles.Float);
		Assert.Fail("#5:Should raise a System.OverflowException");
	}
	catch (Exception e) {
		Assert.IsTrue(typeof(OverflowException) == e.GetType(), "#6");
	}
	try {
		Int64.Parse("10000000000000000", NumberStyles.HexNumber);
		Assert.Fail("#7:Should raise a System.OverflowException");
	}
	catch (Exception e) {
		Assert.IsTrue(typeof(OverflowException) == e.GetType(), "#8");
	}
	try {
		double OverInt = (double)Int64.MaxValue + 1;
		Int64.Parse(OverInt.ToString(), NumberStyles.Integer);
		Assert.Fail("#9:Should raise a System.FormatException");
	}
	catch (Exception e) {
		Assert.IsTrue(typeof(FormatException) == e.GetType(), "#10");
	}
	Assert.AreEqual((long)42, Int64.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ", NumberStyles.Currency), "A1");
	try {
		Int64.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer);
		Assert.Fail("#11:Should raise a System.FormatException");
	}
	catch (Exception e) {
		Assert.IsTrue(typeof(FormatException) == e.GetType(), "#12");
	}
	//test Parse(string s, IFormatProvider provider)
	Assert.IsTrue(-42 == Int64.Parse(" -42 ", Nfi), "A2");
	try {
		Int64.Parse("%42", Nfi);
		Assert.Fail("#13:Should raise a System.FormatException");
	}
	catch (Exception e) {
		Assert.IsTrue(typeof(FormatException) == e.GetType(), "#14");
	}
	//test Parse(string s, NumberStyles style, IFormatProvider provider)
	Assert.IsTrue(16 == Int64.Parse(" 10 ", NumberStyles.HexNumber, Nfi), "A3");
	try {
		Int64.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer, Nfi);
		Assert.Fail("#15:Should raise a System.FormatException");
	}
	catch (Exception e) {
		Assert.IsTrue(typeof(FormatException) == e.GetType(), "#16");
	}
	try {
		long.Parse ("9223372036854775808");
		Assert.Fail ("#17:should raise an OverflowException");
	} catch (Exception e) {
		Assert.IsTrue(typeof(OverflowException) == e.GetType(), "#18");
	}
	try {
		long.Parse ("9223372036854775808", CultureInfo.InvariantCulture);
		Assert.Fail ("#19:should raise an OverflowException");
	} catch (Exception e) {
		Assert.IsTrue(typeof(OverflowException) == e.GetType(), "#20");
	}

	// Pass a DateTimeFormatInfo, it is unable to format
	// numbers, but we should not crash
	
	Int64.Parse ("123", new DateTimeFormatInfo ());
	
	Assert.AreEqual (734561, Int64.Parse ("734561\0"), "#21");
	Assert.AreEqual (734561, Int64.Parse ("734561\0\0\0    \0"), "#22");
	Assert.AreEqual (734561, Int64.Parse ("734561\0\0\0    "), "#23");
	Assert.AreEqual (734561, Int64.Parse ("734561\0\0\0"), "#24");

	Assert.AreEqual (0, Int64.Parse ("0+", NumberStyles.Any), "#30");
    }

	[Test]
	public void TestParseExponent ()
	{
		Assert.AreEqual (2, long.Parse ("2E0", NumberStyles.AllowExponent), "A#1");
		Assert.AreEqual (20, long.Parse ("2E1", NumberStyles.AllowExponent), "A#2");
		Assert.AreEqual (200, long.Parse ("2E2", NumberStyles.AllowExponent), "A#3");
		Assert.AreEqual (2000000, long.Parse ("2E6", NumberStyles.AllowExponent), "A#4");
		Assert.AreEqual (200, long.Parse ("2E+2", NumberStyles.AllowExponent), "A#5");
		Assert.AreEqual (2, long.Parse ("2", NumberStyles.AllowExponent), "A#6");
		Assert.AreEqual (21, long.Parse ("2.1E1", NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent), "A#7");
		Assert.AreEqual (520, long.Parse (".52E3", NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent), "A#8");
		Assert.AreEqual (32500000, long.Parse ("32.5E6", NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent), "A#9");
		Assert.AreEqual (890, long.Parse ("8.9000E2", NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent), "A#10");		

		try {
			long.Parse ("2E");
			Assert.Fail ("B#1");
		} catch (FormatException) {
		}

		try {
			long.Parse ("2E3.0", NumberStyles.AllowExponent); // decimal notation for the exponent
			Assert.Fail ("B#2");
		} catch (FormatException) {
		}

		try {
			long.Parse ("2E 2", NumberStyles.AllowExponent);
			Assert.Fail ("B#3");
		} catch (FormatException) {
		}

		try {
			long.Parse ("2E2 ", NumberStyles.AllowExponent);
			Assert.Fail ("B#4");
		} catch (FormatException) {
		}

		try {
			long.Parse ("2E66", NumberStyles.AllowExponent); // final result overflow
			Assert.Fail ("B#5");
		} catch (OverflowException) {
		}

		try {
			long exponent = (long) Int32.MaxValue + 10;
			long.Parse ("2E" + exponent.ToString (), NumberStyles.AllowExponent);
			Assert.Fail ("B#6");
		} catch (OverflowException) {
		}

		try {
			long.Parse ("2E-1", NumberStyles.AllowExponent); // negative exponent
			Assert.Fail ("B#7");
		} catch (OverflowException) {
		}
		
		try {
			long.Parse ("2 math e1", NumberStyles.AllowExponent);
			Assert.Fail ("B#8");
		} catch (FormatException) {
		}

		try {
			long.Parse ("2.09E1",  NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent);
			Assert.Fail ("B#9");
		} catch (OverflowException) {
		}
	}

	[Test]
	public void TestTryParse()
	{
		long result;

		Assert.AreEqual (true, long.TryParse (MyString1, out result));
		Assert.AreEqual (MyInt64_1, result);
		Assert.AreEqual (true, long.TryParse (MyString2, out result));
		Assert.AreEqual (MyInt64_2, result);
		Assert.AreEqual (true, long.TryParse (MyString3, out result));
		Assert.AreEqual (MyInt64_3, result);

		Assert.AreEqual (true, long.TryParse ("1", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, long.TryParse (" 1", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, long.TryParse ("     1", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, long.TryParse ("1    ", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, long.TryParse ("+1", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, long.TryParse ("-1", out result));
		Assert.AreEqual (-1, result);
		Assert.AreEqual (true, long.TryParse ("  -1", out result));
		Assert.AreEqual (-1, result);
		Assert.AreEqual (true, long.TryParse ("  -1  ", out result));
		Assert.AreEqual (-1, result);
		Assert.AreEqual (true, long.TryParse ("  -1  ", out result));
		Assert.AreEqual (-1, result);

		result = 1;
		Assert.AreEqual (false, long.TryParse (null, out result));
		Assert.AreEqual (0, result);

		Assert.AreEqual (false, long.TryParse ("not-a-number", out result));

		double OverInt = (double)long.MaxValue + 1;
		Assert.AreEqual (false, long.TryParse (OverInt.ToString (), out result));
		Assert.AreEqual (false, long.TryParse (OverInt.ToString (), NumberStyles.None, CultureInfo.InvariantCulture, out result));

		Assert.AreEqual (false, long.TryParse ("$42", NumberStyles.Integer, null, out result));
		Assert.AreEqual (false, long.TryParse ("%42", NumberStyles.Integer, Nfi, out result));
		Assert.AreEqual (false, long.TryParse ("$42", NumberStyles.Integer, Nfi, out result));
		Assert.AreEqual (false, long.TryParse (" - 1 ", out result));
		Assert.AreEqual (false, long.TryParse (" - ", out result));
		Assert.AreEqual (true, long.TryParse ("100000000", NumberStyles.HexNumber, Nfi, out result));
		Assert.AreEqual (true, long.TryParse ("10000000000", out result));
		Assert.AreEqual (true, long.TryParse ("-10000000000", out result));
		Assert.AreEqual (true, long.TryParse ("7fffffff", NumberStyles.HexNumber, Nfi, out result));
		Assert.AreEqual (int.MaxValue, result);
		Assert.AreEqual (true, long.TryParse ("80000000", NumberStyles.HexNumber, Nfi, out result));
		Assert.AreEqual (2147483648, result);
		Assert.AreEqual (true, long.TryParse ("ffffffff", NumberStyles.HexNumber, Nfi, out result));
		Assert.AreEqual (uint.MaxValue, result);
		Assert.AreEqual (true, long.TryParse ("100000000", NumberStyles.HexNumber, Nfi, out result));
		Assert.IsFalse (long.TryParse ("-", NumberStyles.AllowLeadingSign, Nfi, out result));
		Assert.IsFalse (long.TryParse (Nfi.CurrencySymbol + "-", NumberStyles.AllowLeadingSign | NumberStyles.AllowCurrencySymbol, Nfi, out result));
	}	

	[Test]
    public void TestToString() 
    {
        string s;

        s = val1.ToString("c", Nfi);
        Assert.IsTrue(s.Equals(sval1Test6), "val1 does not become sval1Test6");

        s = val1.ToString("n", Nfi);
        Assert.AreEqual(sval1Test7, s, "val1 does not become sval1Test7");

	//test ToString()
	Assert.AreEqual(MyString1, MyInt64_1.ToString(), "MyInt64_1.ToString()");
	Assert.AreEqual(MyString2, MyInt64_2.ToString(), "MyInt64_2.ToString()");
	Assert.AreEqual(MyString3, MyInt64_3.ToString(), "MyInt64_3.ToString()");
	//test ToString(string format)
	for (int i=0; i < Formats1.Length; i++) {
		Assert.AreEqual(Results1[i], MyInt64_2.ToString(Formats1[i]), "MyInt64_2.ToString(Formats1["+i+"])");
		Assert.AreEqual(Results2[i], MyInt64_3.ToString(Formats2[i]), "MyInt64_3.ToString(Formats2["+i+"])");
	}
	//test ToString(string format, IFormatProvider provider);
	for (int i=0; i < Formats1.Length; i++) {
		Assert.AreEqual(ResultsNfi1[i], MyInt64_2.ToString(Formats1[i], Nfi), "MyInt64_2.ToString(Formats1["+i+"], Nfi)");
		Assert.AreEqual(ResultsNfi2[i], MyInt64_3.ToString(Formats2[i], Nfi), "MyInt64_3.ToString(Formats2["+i+"], Nfi)");
	}
	try {
		MyInt64_1.ToString("z");
		Assert.Fail("Should raise a System.FormatException");
	}
	catch (Exception e) {
		Assert.AreEqual(typeof(FormatException), e.GetType(), "Exception is wrong type");
	}
    }

	[Test]
	public void TestUserCurrency ()
	{
		string s = "";
		long v;
		s = val1.ToString ("c", NfiUser);
		Assert.AreEqual ("1234/5/67:000 XYZ-", s, "Currency value type 1 is not what we want to try to parse");
		v = Int64.Parse ("1234/5/67:000   XYZ-", NumberStyles.Currency, NfiUser);
		Assert.AreEqual (val1, v);

		s = val2.ToString ("c", NfiUser);
		Assert.AreEqual ("1234/5/67:000 XYZ", s, "Currency value type 2 is not what we want to try to parse");
		v = Int64.Parse (s, NumberStyles.Currency, NfiUser);
		Assert.AreEqual (val2, v);
	}

	[Test]
    public void TestUserPercent()
    {
        string s;

        s = val1.ToString("p", NfiUser);
        Assert.IsTrue(s.Equals(sval1UserPercent1));

        s = val2.ToString("p", NfiUser);
        Assert.IsTrue(s.Equals(sval2UserPercent1));
    }

		[Test]
		public void Parse_MaxValue ()
		{
			Assert.AreEqual (Int64.MaxValue, Int64.Parse ("9223372036854775807"), "9223372036854775807");
		}

		[Test]
		public void Parse_MinValue ()
		{
			Assert.AreEqual (Int64.MinValue, Int64.Parse ("-9223372036854775808"), "-9223372036854775808,10");
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void Parse_OverByOneMaxValue ()
		{
			Int64.Parse ("9223372036854775808");
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void Parse_WayOverMaxValue ()
		{
			Int64.Parse ("1" + Int64.MaxValue.ToString ());
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void Parse_OverByOneMinValue ()
		{
			Int64.Parse ("-9223372036854775809");
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void Parse_WayOverMinValue ()
		{
			Int64.Parse (Int64.MinValue.ToString () + "1");
		}

		[Test]
		public void ToString_Defaults () 
		{
			Int64 i = 254;
			// everything defaults to "G"
			string def = i.ToString ("G");
			Assert.AreEqual (def, i.ToString (), "ToString()");
			Assert.AreEqual (def, i.ToString ((IFormatProvider)null), "ToString((IFormatProvider)null)");
			Assert.AreEqual (def, i.ToString ((string)null), "ToString((string)null)");
			Assert.AreEqual (def, i.ToString (String.Empty), "ToString(empty)");
			Assert.AreEqual (def, i.ToString (null, null), "ToString(null,null)");
			Assert.AreEqual (def, i.ToString (String.Empty, null), "ToString(empty,null)");

			Assert.AreEqual ("254", def, "ToString(G)");
		}
	}
}
