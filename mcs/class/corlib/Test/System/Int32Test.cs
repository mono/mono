// Int32Test.cs - NUnit Test Cases for the System.Int32 struct
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{

[TestFixture]
public class Int32Test 
{
	private const Int32 MyInt32_1 = -42;
	private const Int32 MyInt32_2 = -2147483648;
	private const Int32 MyInt32_3 = 2147483647;
	private const string MyString1 = "-42";
	private const string MyString2 = "-2147483648";
	private const string MyString3 = "2147483647";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {null,
					"-2147483648", "-2.147484e+009", "-2147483648.00",
					"-2147483648", "-2,147,483,648.00", "-214,748,364,800.00 %", "80000000"};
	private string[] Results2 = {null,
					"2147483647", "2.14748e+009", "2147483647.00000",
					"2.1475e+09", "2,147,483,647.00000", "214,748,364,700.00000 %", "7fffffff"};
	private string[] ResultsNfi1 = {"("+NumberFormatInfo.InvariantInfo.CurrencySymbol+"2,147,483,648.00)",
					"-2147483648", "-2.147484e+009", "-2147483648.00",
					"-2147483648", "-2,147,483,648.00", "-214,748,364,800.00 %", "80000000"};
	private string[] ResultsNfi2 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"2,147,483,647.00000",
					"2147483647", "2.14748e+009", "2147483647.00000",
					"2.1475e+09", "2,147,483,647.00000", "214,748,364,700.00000 %", "7fffffff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	private NumberFormatInfo NfiUser;
	
	private CultureInfo old_culture;

	[TestFixtureSetUp]
	public void SetUpFixture() 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		// We can't initialize this until we set the culture.
		
		string decimals = new String ('0', NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
		string perPattern = new string[] {"n %","n%","%n"} [NumberFormatInfo.CurrentInfo.PercentPositivePattern];
		
		Results1 [0] = "("+NumberFormatInfo.CurrentInfo.CurrencySymbol+"2,147,483,648.00)";
		Results1 [3] = "-2147483648." + decimals;
		Results1 [5] = "-2,147,483,648." + decimals;
		Results1 [6] = perPattern.Replace ("n","-214,748,364,800.00");
		
		Results2 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol+"2,147,483,647.00000";
		Results2 [6] = perPattern.Replace ("n","214,748,364,700.00000");

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
	}
	
	[SetUp]
	public void Setup ()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
	}

	[TestFixtureTearDown]
	public void TearDown()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	[Test]
	public void TestMinMax()
	{
		
		Assert.AreEqual(Int32.MinValue, MyInt32_2, "#A01");
		Assert.AreEqual(Int32.MaxValue, MyInt32_3, "#A02");
	}

	[Test]	
	public void TestCompareTo()
	{
		Assert.IsTrue(MyInt32_3.CompareTo(MyInt32_2) > 0, "MyInt32_3.CompareTo(MyInt32_2) > 0");
		Assert.IsTrue(MyInt32_2.CompareTo(MyInt32_2) == 0, "MyInt32_2.CompareTo(MyInt32_2) == 0");
		Assert.IsTrue(MyInt32_1.CompareTo((object)(Int32)(-42)) == 0, "MyInt32_1.CompareTo((Int32)(-42)) == 0");
		Assert.IsTrue(MyInt32_2.CompareTo(MyInt32_3) < 0, "MyInt32_2.CompareTo(MyInt32_3) < 0");
		try {
			MyInt32_2.CompareTo((object)(Int16)100);
			Assert.Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentException) == e.GetType(), "typeof(ArgumentException) == e.GetType()");
		}
	}

	[Test]
	public void TestEquals()
	{
		Assert.IsTrue (MyInt32_1.Equals (MyInt32_1), "#B01");
		Assert.IsTrue (MyInt32_1.Equals ((Int32)(-42)), "#B02");
		Assert.IsTrue (MyInt32_1.Equals ((object)(SByte)(-42)) == false, "#B03");
		Assert.IsTrue (MyInt32_1.Equals (MyInt32_2) == false, "#B04");
	}

	[Test]	
	public void TestGetHashCode()
	{
		try {
			MyInt32_1.GetHashCode();
			MyInt32_2.GetHashCode();
			MyInt32_3.GetHashCode();
		}
		catch {
			Assert.Fail("GetHashCode should not raise an exception here");
		}
	}

	[Test]	
	public void TestParse()
	{
		//test Parse(string s)
		Assert.AreEqual (MyInt32_1, Int32.Parse (MyString1), "#C01");
		Assert.AreEqual (MyInt32_2, Int32.Parse (MyString2), "#C02");
		Assert.AreEqual (MyInt32_3, Int32.Parse (MyString3), "#C03");

		Assert.AreEqual (1, Int32.Parse ("1"), "#C04");
		Assert.AreEqual (1, Int32.Parse (" 1"), "#C05");
		Assert.AreEqual (1, Int32.Parse ("     1"), "#C06");
		Assert.AreEqual (1, Int32.Parse ("1    "), "#C07");
		Assert.AreEqual (1, Int32.Parse ("+1"), "#C08");
		Assert.AreEqual (-1, Int32.Parse ("-1"), "#C09");
		Assert.AreEqual (-1, Int32.Parse ("  -1"), "#C10");
		Assert.AreEqual (-1, Int32.Parse ("  -1  "), "#C11");
		Assert.AreEqual (-1, Int32.Parse ("  -1  "), "#C12");

		try {
			Int32.Parse(null);
			Assert.Fail ("#C13: Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert.IsTrue (typeof (ArgumentNullException) == e.GetType(), "#C14");
		}
		try {
			Int32.Parse("not-a-number");
			Assert.Fail ("#C15: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue (typeof (FormatException) == e.GetType(), "#C16");
		}
		try {
			double OverInt = (double)Int32.MaxValue + 1;
			Int32.Parse(OverInt.ToString());
			Assert.Fail ("#C17: Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert.AreEqual (typeof (OverflowException), e.GetType(), "#C18");
		}
		//test Parse(string s, NumberStyles style)
		Assert.AreEqual (42, Int32.Parse (" $42 ", NumberStyles.Currency), "#C19");
		try {
			Int32.Parse("$42", NumberStyles.Integer);
			Assert.Fail ("#C20: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue (typeof (FormatException) == e.GetType(), "#C21");
		}
		//test Parse(string s, IFormatProvider provider)
		Assert.AreEqual (-42, Int32.Parse (" -42 ", Nfi), "#C22");
		try {
			Int32.Parse("%42", Nfi);
			Assert.Fail ("#C23: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue (typeof (FormatException) == e.GetType(), "#C24");
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert.AreEqual (16, Int32.Parse (" 10 ", NumberStyles.HexNumber, Nfi), "#C25");
		try {
			Int32.Parse("$42", NumberStyles.Integer, Nfi);
			Assert.Fail ("#C26: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof (FormatException) == e.GetType(), "#C27");
		}

		try {
			Int32.Parse (" - 1 ");
			Assert.Fail ("#C28: Should raise FormatException");
		} catch (Exception e){
			Assert.IsTrue (typeof (FormatException) == e.GetType (), "#C29");
		}

		try {
			Int32.Parse (" - ");
			Assert.Fail ("#C30: Should raise FormatException");
		} catch (Exception e){
			Assert.IsTrue (typeof (FormatException) == e.GetType (), "#C31");
		}
		Assert.AreEqual (-123, Int32.Parse ("ffffff85", NumberStyles.HexNumber, Nfi), "#C32");
		try {
			Int32.Parse ("100000000", NumberStyles.HexNumber, Nfi);
			Assert.Fail ("#C33: Should raise OverflowException");
		} catch (Exception e){
			Assert.IsTrue (typeof (OverflowException) == e.GetType (), "#C34");
		}
		try {
			Int32.Parse ("2147483648");
			Assert.Fail ("C#35: should raise OverflowException");
		} catch (Exception e) {
			Assert.IsTrue (typeof (OverflowException) == e.GetType (), "C#36");
		}
		try {
			Int32.Parse ("2147483648", CultureInfo.InvariantCulture);
			Assert.Fail ("C#37: should raise OverflowException");
		} catch (Exception e) {
			Assert.IsTrue (typeof (OverflowException) == e.GetType (), "C#38");
		}

		try {
			Int32.Parse (null);
			Assert.Fail ("C#39: Should raise an ArgumentNullException");
		} catch (Exception e){
			Assert.IsTrue (typeof (ArgumentNullException) == e.GetType (), "C#40");
		}

		try {
			Int32.Parse ("123", (NumberStyles) 60000);
			Assert.Fail ("C#41 Should raise an ArgumentException");
		} catch (Exception e){
			Assert.IsTrue (typeof (ArgumentException) == e.GetType (), "C#42");
		}

		// Pass a DateTimeFormatInfo, it is unable to format
		// numbers, but we should not crash
		
		Int32.Parse ("123", new DateTimeFormatInfo ());

		Assert.AreEqual (734561, Int32.Parse ("734561\0"), "C#43");
		Assert.AreEqual (734561, Int32.Parse ("734561\0\0\0    \0"), "C#44");
		Assert.AreEqual (734561, Int32.Parse ("734561\0\0\0    "), "C#45");
		Assert.AreEqual (734561, Int32.Parse ("734561\0\0\0"), "C#46");

		Assert.AreEqual (0, Int32.Parse ("0+", NumberStyles.Any), "#50");
	}

    [Test]
	public void TestParseExponent ()
	{
		Assert.AreEqual (2, Int32.Parse ("2E0", NumberStyles.AllowExponent), "A#1");
		Assert.AreEqual (20, Int32.Parse ("2E1", NumberStyles.AllowExponent), "A#2");
		Assert.AreEqual (200, Int32.Parse ("2E2", NumberStyles.AllowExponent), "A#3");
		Assert.AreEqual (2000000, Int32.Parse ("2E6", NumberStyles.AllowExponent), "A#4");
		Assert.AreEqual (200, Int32.Parse ("2E+2", NumberStyles.AllowExponent), "A#5");
		Assert.AreEqual (2, Int32.Parse ("2", NumberStyles.AllowExponent), "A#6");

		try {
			Int32.Parse ("2E");
			Assert.Fail ("B#1");
		} catch (FormatException) {
		}

		try {
			Int32.Parse ("2E3.0", NumberStyles.AllowExponent); // decimal notation for the exponent
			Assert.Fail ("B#2");
		} catch (FormatException) {
		}

		try {
			Int32.Parse ("2E 2", NumberStyles.AllowExponent);
			Assert.Fail ("B#3");
		} catch (FormatException) {
		}

		try {
			Int32.Parse ("2E2 ", NumberStyles.AllowExponent);
			Assert.Fail ("B#4");
		} catch (FormatException) {
		}

		try {
			Int32.Parse ("2E66", NumberStyles.AllowExponent); // final result overflow
			Assert.Fail ("B#5");
		} catch (OverflowException) {
		}

		try {
			long exponent = (long)Int32.MaxValue + 10;
			Int32.Parse ("2E" + exponent.ToString (), NumberStyles.AllowExponent);
			Assert.Fail ("B#6");
		} catch (OverflowException) {
		}

		try {
			Int32.Parse ("2E-1", NumberStyles.AllowExponent); // negative exponent
			Assert.Fail ("B#7");
		} catch (OverflowException) {
		}

		try {
			Int32.Parse ("2 math e1", NumberStyles.AllowExponent);
			Assert.Fail ("B#8");
		} catch (FormatException) {
		}
	}

	[Test]
	public void TestTryParse()
	{
		int result;

		Assert.AreEqual (true, Int32.TryParse (MyString1, out result));
		Assert.AreEqual (MyInt32_1, result);
		Assert.AreEqual (true, Int32.TryParse (MyString2, out result));
		Assert.AreEqual (MyInt32_2, result);
		Assert.AreEqual (true, Int32.TryParse (MyString3, out result));
		Assert.AreEqual (MyInt32_3, result);

		Assert.AreEqual (true, Int32.TryParse ("1", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, Int32.TryParse (" 1", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, Int32.TryParse ("     1", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, Int32.TryParse ("1    ", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, Int32.TryParse ("+1", out result));
		Assert.AreEqual (1, result);
		Assert.AreEqual (true, Int32.TryParse ("-1", out result));
		Assert.AreEqual (-1, result);
		Assert.AreEqual (true, Int32.TryParse ("  -1", out result));
		Assert.AreEqual (-1, result);
		Assert.AreEqual (true, Int32.TryParse ("  -1  ", out result));
		Assert.AreEqual (-1, result);
		Assert.AreEqual (true, Int32.TryParse ("  -1  ", out result));
		Assert.AreEqual (-1, result);

		result = 1;
		Assert.AreEqual (false, Int32.TryParse (null, out result));
		Assert.AreEqual (0, result);

		Assert.AreEqual (false, Int32.TryParse ("not-a-number", out result));

		double OverInt = (double)Int32.MaxValue + 1;
		Assert.AreEqual (false, Int32.TryParse (OverInt.ToString (), out result));
		Assert.AreEqual (false, Int32.TryParse (OverInt.ToString (), NumberStyles.None, CultureInfo.InvariantCulture, out result));

		Assert.AreEqual (false, Int32.TryParse ("$42", NumberStyles.Integer, null, out result));
		Assert.AreEqual (false, Int32.TryParse ("%42", NumberStyles.Integer, Nfi, out result));
		Assert.AreEqual (false, Int32.TryParse ("$42", NumberStyles.Integer, Nfi, out result));
		Assert.AreEqual (false, Int32.TryParse (" - 1 ", out result));
		Assert.AreEqual (false, Int32.TryParse (" - ", out result));
		Assert.AreEqual (false, Int32.TryParse ("100000000", NumberStyles.HexNumber, Nfi, out result));
		Assert.AreEqual (false, Int32.TryParse ("10000000000", out result));
		Assert.AreEqual (false, Int32.TryParse ("-10000000000", out result));
		Assert.AreEqual (true, Int32.TryParse ("7fffffff", NumberStyles.HexNumber, Nfi, out result));
		Assert.AreEqual (Int32.MaxValue, result);
		Assert.AreEqual (true, Int32.TryParse ("80000000", NumberStyles.HexNumber, Nfi, out result));
		Assert.AreEqual (Int32.MinValue, result);
		Assert.AreEqual (true, Int32.TryParse ("ffffffff", NumberStyles.HexNumber, Nfi, out result));
		Assert.AreEqual (-1, result);
		Assert.AreEqual (false, Int32.TryParse ("100000000", NumberStyles.HexNumber, Nfi, out result));
	}

	[Test]	
	public void TestToString()
	{
		//test ToString()
		Assert.AreEqual (MyString1, MyInt32_1.ToString (), "#D01");
		Assert.AreEqual (MyString2, MyInt32_2.ToString (), "#D02");
		Assert.AreEqual (MyString3, MyInt32_3.ToString (), "#D03");

		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert.AreEqual (ResultsNfi1 [i], MyInt32_2.ToString (Formats1 [i], Nfi),
							 "#D04(" + i + "," + Formats1 [i] + ")");
			Assert.AreEqual (ResultsNfi2 [i], MyInt32_3.ToString (Formats2 [i], Nfi), 
							 "#D05(" + i + "," + Formats2 [i] + ")");
		}

		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			Assert.AreEqual (Results1 [i], MyInt32_2.ToString(Formats1[i]), "#D06(" + i + ")");
			Assert.AreEqual (Results2 [i], MyInt32_3.ToString(Formats2[i]),
							 "#D07(" + i + ")");
				      
		}

		try {
			MyInt32_1.ToString("z");
			Assert.Fail ("#D08: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue (typeof (FormatException) == e.GetType(), "#D09");
		}
	}

	[Test]
	public void TestCustomToString()
	{
		int i = 123;

		Assert.AreEqual ("00123", i.ToString ("00000"), "Custom format string 00000");
		Assert.AreEqual ("123", i.ToString ("####"), "Custom format string ####");
		Assert.AreEqual ("0123", i.ToString ("0###"), "Custom format string ####");
		Assert.AreEqual ("0123", i.ToString ("#0###"), "Custom format string ####");
		Assert.AreEqual ("000123", i.ToString ("0#0###"), "Custom format string ####");
	}

	[Test]
	public void TestSections ()
	{
		int hundred = 100;
		int neghund = -100;
		
		Assert.IsTrue ( hundred.ToString ("#;#") == "100", "#TS1");
		Assert.IsTrue ( hundred.ToString ("-#;#") == "-100", "#TS2");
		Assert.IsTrue ( neghund.ToString ("#;#") == "100", "#TS3");
		Assert.IsTrue ( neghund.ToString ("#;-#") == "-100", "#TS3");
	}
	
	[Test]
	public void ToString_Defaults () 
	{
		Int32 i = 254;
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

	[Test]
	public void ParseRespectCurrentCulture ()
	{
		var old = Thread.CurrentThread.CurrentCulture;
		var cur = (CultureInfo)old.Clone ();

		NumberFormatInfo ninfo = new NumberFormatInfo ();
		ninfo.NegativeSign = ">";
		ninfo.PositiveSign = "%";
		cur.NumberFormat = ninfo;

		Thread.CurrentThread.CurrentCulture = cur;

		int val = 0;

		try {
			Assert.IsTrue (int.TryParse (">11", out val), "#1");
			Assert.AreEqual (-11, val, "#2");
			Assert.IsTrue (int.TryParse ("%11", out val), "#3");
			Assert.AreEqual (11, val, "#4");
		} finally {
			Thread.CurrentThread.CurrentCulture = old;
		}
	}

	[Test]
	public void TestUserCurrency ()
	{
		const int val1 = -1234567;
		const int val2 = 1234567;

		string s = "";
		int v;
		s = val1.ToString ("c", NfiUser);
		Assert.AreEqual ("1234/5/67:000 XYZ-", s, "Currency value type 1 is not what we want to try to parse");
		v = Int32.Parse ("1234/5/67:000   XYZ-", NumberStyles.Currency, NfiUser);
		Assert.AreEqual (val1, v);

		s = val2.ToString ("c", NfiUser);
		Assert.AreEqual ("1234/5/67:000 XYZ", s, "Currency value type 2 is not what we want to try to parse");
		v = Int32.Parse (s, NumberStyles.Currency, NfiUser);
		Assert.AreEqual (val2, v);
	}
}

}
