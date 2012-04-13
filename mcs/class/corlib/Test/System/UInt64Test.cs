// UInt64Test.cs - NUnit Test Cases for the System.UInt64 struct
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;

namespace MonoTests.System
{

[TestFixture]
public class UInt64Test 
{
	private const UInt64 MyUInt64_1 = 42;
	private const UInt64 MyUInt64_2 = 0;
	private const UInt64 MyUInt64_3 = 18446744073709551615;
	private const string MyString1 = "42";
	private const string MyString2 = "0";
	private const string MyString3 = "18446744073709551615";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {"",
					"0", "0.000000e+000", "0.00",
					"0", "0.00", "0.00 %", "0"};
	private string[] ResultsNfi1 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"0.00",
					"0", "0.000000e+000", "0.00",
					"0", "0.00", "0.00 %", "0"};
	private string[] Results2 = {"",
					"18446744073709551615", "1.84467e+019", "18446744073709551615.00000",
					"1.8447e+19", "18,446,744,073,709,551,615.00000",
					"1,844,674,407,370,955,161,500.00000 %", "ffffffffffffffff"};
	private string[] ResultsNfi2 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"18,446,744,073,709,551,615.00000",
					"18446744073709551615", "1.84467e+019", "18446744073709551615.00000",
					"1.8447e+19", "18,446,744,073,709,551,615.00000",
					"1,844,674,407,370,955,161,500.00000 %", "ffffffffffffffff"};

	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	private CultureInfo old_culture;

	[SetUp]
	public void SetUp() 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		// We can't initialize this until we set the culture.
		
		string decimals = new String ('0', NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
		string perPattern = new string[] {"n %","n%","%n"} [NumberFormatInfo.CurrentInfo.PercentPositivePattern];
		
		Results1 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol + "0.00";
		Results1 [3] = "0." + decimals;
		Results1 [5] = "0." + decimals;
		Results1 [6] = perPattern.Replace ("n","0.00");
		
		Results2 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol + "18,446,744,073,709,551,615.00000";
		Results2 [6] = perPattern.Replace ("n","1,844,674,407,370,955,161,500.00000");
	}

	[TearDown]
	public void TearDown ()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	[Test]
	public void TestMinMax()
	{
		Assert.AreEqual(UInt64.MinValue, MyUInt64_2);
		Assert.AreEqual(UInt64.MaxValue, MyUInt64_3);
	}

	[Test]
	public void TestCompareTo()
	{
		Assert.IsTrue(MyUInt64_3.CompareTo(MyUInt64_2) > 0);
		Assert.IsTrue(MyUInt64_2.CompareTo(MyUInt64_2) == 0);
		Assert.IsTrue(MyUInt64_1.CompareTo((UInt64)(42)) == 0);
		Assert.IsTrue(MyUInt64_2.CompareTo(MyUInt64_3) < 0);
		try {
			MyUInt64_2.CompareTo((object)(Int16)100);
			Assert.Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentException) == e.GetType());
		}
	}

	[Test]
	public void TestEquals()
	{
		Assert.IsTrue(MyUInt64_1.Equals(MyUInt64_1));
		Assert.IsTrue(MyUInt64_1.Equals((object)(UInt64)(42)));
		Assert.IsTrue(MyUInt64_1.Equals((object)(SByte)(42)) == false);
		Assert.IsTrue(MyUInt64_1.Equals(MyUInt64_2) == false);
	}

	[Test]
	public void TestGetHashCode()
	{
		try {
			MyUInt64_1.GetHashCode();
			MyUInt64_2.GetHashCode();
			MyUInt64_3.GetHashCode();
		}
		catch {
			Assert.Fail("GetHashCode should not raise an exception here");
		}
	}

	[Test]
	public void TestParse()
	{
		//test Parse(string s)
		Assert.IsTrue(MyUInt64_1 == UInt64.Parse(MyString1));
		Assert.IsTrue(MyUInt64_2 == UInt64.Parse(MyString2));
		Assert.IsTrue(MyUInt64_3 == UInt64.Parse(MyString3));
		try {
			UInt64.Parse(null);
			Assert.Fail("Should raise a ArgumentNullException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentNullException) == e.GetType());
		}
		try {
			UInt64.Parse("not-a-number");
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		try {
			double OverInt = (double)UInt64.MaxValue + 1;
			UInt64.Parse(OverInt.ToString(), NumberStyles.Float);
			Assert.Fail("Should raise a OverflowException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(OverflowException) == e.GetType());
		}
		try {
			double OverInt = (double)UInt64.MaxValue + 1;
			UInt64.Parse(OverInt.ToString(), NumberStyles.Integer);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		Assert.IsTrue(42 == UInt64.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ", NumberStyles.Currency));
		try {
			UInt64.Parse("$42", NumberStyles.Integer);
			Assert.Fail("Should raise a FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert.IsTrue(42 == UInt64.Parse(" 42 ", Nfi));
		try {
			UInt64.Parse("%42", Nfi);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert.IsTrue(16 == UInt64.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			UInt64.Parse("$42", NumberStyles.Integer, Nfi);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}

		// Pass a DateTimeFormatInfo, it is unable to format
		// numbers, but we should not crash
		
		UInt64.Parse ("123", new DateTimeFormatInfo ());

		Assert.AreEqual (734561, UInt64.Parse ("734561\0"), "C#43");
		Assert.AreEqual (734561, UInt64.Parse ("734561\0\0\0    \0"), "C#44");
		Assert.AreEqual (734561, UInt64.Parse ("734561\0\0\0    "), "C#45");
		Assert.AreEqual (734561, UInt64.Parse ("734561\0\0\0"), "C#46");

		Assert.AreEqual (0, UInt64.Parse ("0+", NumberStyles.Any), "#50");
	}

	[Test]
	public void TestParseExponent ()
	{
		Assert.AreEqual (2, ulong.Parse ("2E0", NumberStyles.AllowExponent), "A#1");
		Assert.AreEqual (20, ulong.Parse ("2E1", NumberStyles.AllowExponent), "A#2");
		Assert.AreEqual (200, ulong.Parse ("2E2", NumberStyles.AllowExponent), "A#3");
		Assert.AreEqual (2000000, ulong.Parse ("2E6", NumberStyles.AllowExponent), "A#4");
		Assert.AreEqual (200, ulong.Parse ("2E+2", NumberStyles.AllowExponent), "A#5");
		Assert.AreEqual (2, ulong.Parse ("2", NumberStyles.AllowExponent), "A#6");

		try {
			ulong.Parse ("2E");
			Assert.Fail ("B#1");
		} catch (FormatException) {
		}

		try {
			ulong.Parse ("2E3.0", NumberStyles.AllowExponent); // decimal notation for the exponent
			Assert.Fail ("B#2");
		} catch (FormatException) {
		}

		try {
			ulong.Parse ("2E 2", NumberStyles.AllowExponent);
			Assert.Fail ("B#3");
		} catch (FormatException) {
		}

		try {
			ulong.Parse ("2E2 ", NumberStyles.AllowExponent);
			Assert.Fail ("B#4");
		} catch (FormatException) {
		}

		try {
			ulong.Parse ("2E66", NumberStyles.AllowExponent); // final result overflow
			Assert.Fail ("B#5");
		} catch (OverflowException) {
		}

		try {
			long exponent = (long) Int32.MaxValue + 10;
			ulong.Parse ("2E" + exponent.ToString (), NumberStyles.AllowExponent);
			Assert.Fail ("B#6");
		} catch (OverflowException) {
		}

		try {
			ulong.Parse ("2E-1", NumberStyles.AllowExponent); // negative exponent
			Assert.Fail ("B#7");
		} catch (OverflowException) {
		}
		
		try {
			ulong.Parse ("2 math e1", NumberStyles.AllowExponent);
			Assert.Fail ("B#8");
		} catch (FormatException) {
		}
	}

	[Test]
	public void TestToString()
	{
		//test ToString()
		Assert.AreEqual(MyString1, MyUInt64_1.ToString(), "A");
		Assert.AreEqual(MyString2, MyUInt64_2.ToString(), "B");
		Assert.AreEqual(MyString3, MyUInt64_3.ToString(), "C");
		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			Assert.AreEqual(Results1[i], MyUInt64_2.ToString(Formats1[i]), "D");
			Assert.AreEqual(Results2[i], MyUInt64_3.ToString(Formats2[i]), "E: format #" + i);
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert.AreEqual(ResultsNfi1[i], MyUInt64_2.ToString(Formats1[i], Nfi), "F");
			Assert.AreEqual(ResultsNfi2[i], MyUInt64_3.ToString(Formats2[i], Nfi), "G");
		}
		try {
			MyUInt64_1.ToString("z");
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType(), "H");
		}
	}

	[Test]
	public void ToString_Defaults () 
	{
		UInt64 i = 254;
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
