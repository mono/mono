// Int16Test.cs - NUnit Test Cases for the System.Int16 struct
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
public class Int16Test 
{
	private const Int16 MyInt16_1 = -42;
	private const Int16 MyInt16_2 = -32768;
	private const Int16 MyInt16_3 = 32767;
	private const string MyString1 = "-42";
	private const string MyString2 = "-32768";
	private const string MyString3 = "32767";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {null, "-32768", "-3.276800e+004", "-32768.00",
	                                  "-32768", "-32,768.00", "-3,276,800.00 %", "8000"};
	private string[] Results2 = {null, "32767", "3.27670e+004", "32767.00000",
	                                  "32767", "32,767.00000", "3,276,700.00000 %", "07fff"};
	private string[] ResultsNfi1 = {"("+NumberFormatInfo.InvariantInfo.CurrencySymbol+"32,768.00)", "-32768", "-3.276800e+004", "-32768.00",
	                                  "-32768", "-32,768.00", "-3,276,800.00 %", "8000"};
	private string[] ResultsNfi2 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"32,767.00000", "32767", "3.27670e+004", "32767.00000",
	                                  "32767", "32,767.00000", "3,276,700.00000 %", "07fff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	private CultureInfo old_culture;

	[TestFixtureSetUp]
	public void SetUpFixture () 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		// We can't initialize this until we set the culture.
		Results1 [0] = "("+NumberFormatInfo.CurrentInfo.CurrencySymbol+"32,768.00)";
		Results2 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol+"32,767.00000";
	}
	
	[SetUp]
	public void Setup ()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
	}

	[TestFixtureTearDown]
	public void TearDown ()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	[Test]
	public void TestMinMax()
	{
		Assert.AreEqual(Int16.MinValue, MyInt16_2);
		Assert.AreEqual(Int16.MaxValue, MyInt16_3);
	}

	[Test]	
	public void TestCompareTo()
	{
		Assert.IsTrue(MyInt16_3.CompareTo(MyInt16_2) > 0);
		Assert.IsTrue(MyInt16_2.CompareTo(MyInt16_2) == 0);
		Assert.IsTrue(MyInt16_1.CompareTo((Int16)(-42)) == 0);
		Assert.IsTrue(MyInt16_2.CompareTo(MyInt16_3) < 0);
		try {
			MyInt16_2.CompareTo((object)100);
			Assert.Fail ("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentException) == e.GetType());
		}
	}

	[Test]
	public void TestEquals()
	{
		Assert.IsTrue(MyInt16_1.Equals(MyInt16_1));
		Assert.IsTrue(MyInt16_1.Equals((object)(Int16)(-42)));
		Assert.IsTrue(MyInt16_1.Equals((object)(SByte)(-42)) == false);
		Assert.IsTrue(MyInt16_1.Equals(MyInt16_2) == false);
	}

	[Test]	
	public void TestGetHashCode()
	{
		try {
			MyInt16_1.GetHashCode();
			MyInt16_2.GetHashCode();
			MyInt16_3.GetHashCode();
		}
		catch {
			Assert.Fail ("GetHashCode should not raise an exception here");
		}
	}

	[Test]	
	public void TestParse()
	{
		//test Parse(string s)
		Assert.IsTrue(MyInt16_1 == Int16.Parse(MyString1));
		Assert.IsTrue(MyInt16_2 == Int16.Parse(MyString2));
		Assert.IsTrue(MyInt16_3 == Int16.Parse(MyString3));
		try {
			Int16.Parse(null);
			Assert.Fail ("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentNullException) == e.GetType());
		}
		try {
			Int16.Parse("not-a-number");
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		try {
			int OverInt = Int16.MaxValue + 1;
			Int16.Parse(OverInt.ToString());
			Assert.Fail ("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		Assert.IsTrue(42 == Int16.Parse(" $42 ", NumberStyles.Currency));
		try {
			Int16.Parse("$42", NumberStyles.Integer);
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert.IsTrue(-42 == Int16.Parse(" -42 ", Nfi));
		try {
			Int16.Parse("%42", Nfi);
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert.IsTrue(16 == Int16.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			Int16.Parse("$42", NumberStyles.Integer, Nfi);
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}

		Assert.AreEqual (7345, Int16.Parse ("7345\0"), "#1");
		Assert.AreEqual (7345, Int16.Parse ("7345\0\0\0    \0"), "#2");
		Assert.AreEqual (7345, Int16.Parse ("7345\0\0\0    "), "#3");
		Assert.AreEqual (7345, Int16.Parse ("7345\0\0\0"), "#4");

		Assert.AreEqual (0, Int16.Parse ("0+", NumberStyles.Any), "#5");
	}

	[Test]
	public void TestParseExponent ()
	{
		Assert.AreEqual (2, Int16.Parse ("2E0", NumberStyles.AllowExponent), "A#1");
		Assert.AreEqual (20, Int16.Parse ("2E1", NumberStyles.AllowExponent), "A#2");
		Assert.AreEqual (200, Int16.Parse ("2E2", NumberStyles.AllowExponent), "A#3");
		Assert.AreEqual (200, Int16.Parse ("2E+2", NumberStyles.AllowExponent), "A#4");
		Assert.AreEqual (2, Int16.Parse ("2", NumberStyles.AllowExponent), "A#5");

		try {
			Int16.Parse ("2E");
			Assert.Fail ("B#1");
		} catch (FormatException) {
		}

		try {
			Int16.Parse ("2E3.0", NumberStyles.AllowExponent); // decimal notation for the exponent
			Assert.Fail ("B#2");
		} catch (FormatException) {
		}

		try {
			Int16.Parse ("2E 2", NumberStyles.AllowExponent);
			Assert.Fail ("B#3");
		} catch (FormatException) {
		}

		try {
			Int16.Parse ("2E2 ", NumberStyles.AllowExponent);
			Assert.Fail ("B#4");
		} catch (FormatException) {
		}

		try {
			Int16.Parse ("2E66", NumberStyles.AllowExponent); // final result overflow
			Assert.Fail ("B#5");
		} catch (OverflowException) {
		}

		try {
			long exponent = (long) Int32.MaxValue + 10;
			Int16.Parse ("2E" + exponent.ToString (), NumberStyles.AllowExponent);
			Assert.Fail ("B#6");
		} catch (OverflowException) {
		}

		try {
			Int16.Parse ("2E-1", NumberStyles.AllowExponent); // negative exponent
			Assert.Fail ("B#7");
		} catch (OverflowException) {
		}
		
		try {
			Int16.Parse ("2 math e1", NumberStyles.AllowExponent);
			Assert.Fail ("B#8");
		} catch (FormatException) {
		}
	}

	[Test]
	public void Parse_MinMax ()
	{
		Assert.AreEqual (Int16.MinValue, Int16.Parse ("-32768"), "MinValue");
		Assert.AreEqual (Int16.MaxValue, Int16.Parse ("32767"), "MaxValue");
		Assert.AreEqual (-1, Int16.Parse ("FFFF", NumberStyles.HexNumber), "MaxHex");
	}

	[Test]	
	public void TestToString()
	{
		//test ToString()
		Assert.IsTrue(String.Compare(MyString1, MyInt16_1.ToString()) == 0);
		Assert.IsTrue(String.Compare(MyString2, MyInt16_2.ToString()) == 0);
		Assert.IsTrue(String.Compare(MyString3, MyInt16_3.ToString()) == 0);
		//test ToString(string format)
		/*
		TODO: These tests are culture sensitive.  Need to find a way to determine the culture
			of the system to decide the correct expected result.
		for (int i=0; i < Formats1.Length; i++) {
			Assert.IsTrue(String.Compare(Results1[i], MyInt16_2.ToString(Formats1[i])) == 0);
			Assert.IsTrue(String.Compare(Results2[i], MyInt16_3.ToString(Formats2[i])) == 0);
		}
		*/
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert.IsTrue(String.Compare(ResultsNfi1[i], MyInt16_2.ToString(Formats1[i], Nfi)) == 0, "i="+i+", ResultsNfi1[i]="+ResultsNfi1[i]+", MyInt16_2.ToString(Formats1[i]="+Formats1[i]+"): Expected "+ResultsNfi1[i]+" but got "+MyInt16_2.ToString(Formats1[i], Nfi));
			Assert.IsTrue(String.Compare(ResultsNfi2[i], MyInt16_3.ToString(Formats2[i], Nfi)) == 0, "i="+i+", ResultsNfi2[i]="+ResultsNfi2[i]+", MyInt16_3.ToString(Formats2[i]="+Formats2[i]+"): Expected "+ResultsNfi2[i]+" but got "+MyInt16_3.ToString(Formats2[i], Nfi));
		}
		try {
			MyInt16_1.ToString("z");
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
	}

	[Test]
	public void ToString_Defaults () 
	{
		Int16 i = 254;
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
	public void Bug3677 ()
	{
		Assert.AreEqual (-7197, short.Parse("E3E3", NumberStyles.HexNumber), "HexNumber");
	}
}

}
