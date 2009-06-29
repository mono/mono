// UInt16Test.cs - NUnit Test Cases for the System.UInt16 struct
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
public class UInt16Test 
{
	private const UInt16 MyUInt16_1 = 42;
	private const UInt16 MyUInt16_2 = 0;
	private const UInt16 MyUInt16_3 = 65535;
	private const string MyString1 = "42";
	private const string MyString2 = "0";
	private const string MyString3 = "65535";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {null,
				     "0", "0.000000e+000", "0.00",
				     "0", "0.00", "0.00 %", "0"};
	private string[] Results2 = {null,
				     "65535", "6.55350e+004", "65535.00000",
				     "65535", "65,535.00000", "6,553,500.00000 %", "0ffff"};
	private string[] ResultsNfi1 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"0.00",
					"0", "0.000000e+000", "0.00",
					"0", "0.00", "0.00 %", "0"};
	private string[] ResultsNfi2 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"65,535.00000",
					"65535", "6.55350e+004", "65535.00000",
					"65535", "65,535.00000", "6,553,500.00000 %", "0ffff"};

	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	private CultureInfo old_culture;

	[TestFixtureSetUp]
	public void SetUp () 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		string decimals = new String ('0', NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
		string perPattern = new string[] {"n %","n%","%n"} [NumberFormatInfo.CurrentInfo.PercentPositivePattern];
		
		Results1 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol + "0.00";
		Results1 [3] = "0." + decimals;
		Results1 [5] = "0." + decimals;
		Results1 [6] = perPattern.Replace ("n","0.00");
		
		Results2 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol + "65,535.00000";
		Results2 [6] = perPattern.Replace ("n","6,553,500.00000");
	}

	[TestFixtureTearDown]
	public void TearDown ()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	public void TestMinMax()
	{
		
		Assert.AreEqual(UInt16.MinValue, MyUInt16_2);
		Assert.AreEqual(UInt16.MaxValue, MyUInt16_3);
	}
	
	public void TestCompareTo()
	{
		Assert.IsTrue(MyUInt16_3.CompareTo(MyUInt16_2) > 0);
		Assert.IsTrue(MyUInt16_2.CompareTo(MyUInt16_2) == 0);
		Assert.IsTrue(MyUInt16_1.CompareTo((UInt16)(42)) == 0);
		Assert.IsTrue(MyUInt16_2.CompareTo(MyUInt16_3) < 0);
		try {
			MyUInt16_2.CompareTo((object)100);
			Assert.Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert.IsTrue(MyUInt16_1.Equals(MyUInt16_1));
		Assert.IsTrue(MyUInt16_1.Equals((object)(UInt16)(42)));
		Assert.IsTrue(MyUInt16_1.Equals((object)(SByte)(42)) == false);
		Assert.IsTrue(MyUInt16_1.Equals(MyUInt16_2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MyUInt16_1.GetHashCode();
			MyUInt16_2.GetHashCode();
			MyUInt16_3.GetHashCode();
		}
		catch {
			Assert.Fail("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert.IsTrue(MyUInt16_1 == UInt16.Parse(MyString1));
		Assert.IsTrue(MyUInt16_2 == UInt16.Parse(MyString2));
		Assert.IsTrue(MyUInt16_3 == UInt16.Parse(MyString3));
		try {
			UInt16.Parse(null);
			Assert.Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentNullException) == e.GetType());
		}
		try {
			UInt16.Parse("not-a-number");
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		try {
			int OverInt = UInt16.MaxValue + 1;
			UInt16.Parse(OverInt.ToString());
			Assert.Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		Assert.IsTrue(42 == UInt16.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ", NumberStyles.Currency));
		try {
			UInt16.Parse("$42", NumberStyles.Integer);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert.IsTrue(42 == UInt16.Parse(" 42 ", Nfi));
		try {
			UInt16.Parse("%42", Nfi);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert.IsTrue(16 == UInt16.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			UInt16.Parse("$42", NumberStyles.Integer, Nfi);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
	}
	
	public void TestToString()
	{
		//test ToString()
		Assert.AreEqual(MyString1, MyUInt16_1.ToString(), "A1");
		Assert.AreEqual(MyString2, MyUInt16_2.ToString(), "A2");
		Assert.AreEqual(MyString3, MyUInt16_3.ToString(), "A3");
		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			Console.WriteLine ("d:" + NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
			Assert.AreEqual(Results1[i], MyUInt16_2.ToString(Formats1[i]), "A4:"+i.ToString());
			Assert.AreEqual(Results2[i], MyUInt16_3.ToString(Formats2[i]), "A5:"+i.ToString());
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert.AreEqual(ResultsNfi1[i], MyUInt16_2.ToString(Formats1[i], Nfi), "A6:"+i.ToString());
			Assert.AreEqual(ResultsNfi2[i], MyUInt16_3.ToString(Formats2[i], Nfi), "A7:"+i.ToString());
		}
		try {
			MyUInt16_1.ToString("z");
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType(), "A8");
		}
	}

	[Test]
	public void ToString_Defaults () 
	{
		UInt16 i = 254;
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
