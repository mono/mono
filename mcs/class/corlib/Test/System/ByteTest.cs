// ByteTest.cs - NUnit Test Cases for the System.Byte struct
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
public class ByteTest 
{
	private const Byte MyByte1 = 42;
	private const Byte MyByte2 = 0;
	private const Byte MyByte3 = 255;
	private const string MyString1 = "42";
	private const string MyString2 = "0";
	private const string MyString3 = "255";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {	"",
					"0", "0.000000e+000", "0.00",
					"0", "0.00", "0.00 %", "0"};
	private string[] Results1_Nfi = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"0.00",
					"0", "0.000000e+000", "0.00",
					"0", "0.00", "0.00 %", "0"};
	private string[] Results2 = {	"",
					"00255", "2.55000e+002", "255.00000",
					"255", "255.00000", "25,500.00000 %", "000ff"};
	private string[] Results2_Nfi = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"255.00000", 
					"00255", "2.55000e+002", "255.00000",
					"255", "255.00000", "25,500.00000 %", "000ff"};

	private CultureInfo old_culture;
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;

	[SetUp]
	public void SetUp ()
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		CultureInfo EnUs = new CultureInfo ("en-us", false);
		EnUs.NumberFormat.NumberDecimalDigits = 2;
		Thread.CurrentThread.CurrentCulture = EnUs;

		int cdd = NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits;
		string sep = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
		string csym = NumberFormatInfo.CurrentInfo.CurrencySymbol;
		string csuffix = (cdd > 0 ? sep : "").PadRight(cdd + (cdd > 0 ? 1 : 0), '0');
		switch (NumberFormatInfo.CurrentInfo.CurrencyPositivePattern) {
			case 0: // $n
				Results1[0] = csym + "0" + csuffix;
				Results2[0] = csym + "255" + sep + "00000";
				break;
			case 1: // n$
				Results1[0] = "0" + csuffix + csym;
				Results2[0] = "255" + sep + "00000" + csym;
				break;
			case 2: // $ n
				Results1[0] = csym + " 0" + csuffix;
				Results2[0] = csym + " 255" + sep + "00000";
				break;
			case 3: // n $
				Results1[0] = "0" + csuffix + " " + csym;
				Results2[0] = "255" + sep + "00000 " + csym;
				break;
		}
		
		sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
		string decimals = new String ('0', NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
		string perPattern = new string[] {"n %","n%","%n"} [NumberFormatInfo.CurrentInfo.PercentPositivePattern];
		
		Results1[2] = "0" + sep + "000000e+000";
		Results1[3] = "0" + sep + decimals;
		Results1[5] = "0" + sep + decimals;
		Results1[6] = perPattern.Replace ("n","0" + sep + "00");
		
		Results2[2] = "2" + sep + "55000e+002";
		Results2[3] = "255" + sep + "00000";
		Results2[3] = "255" + sep + "00000";
		Results2[5] = "255" + sep + "00000";
		string gsep = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		Results2[6] = perPattern.Replace ("n","25" + gsep + "500" + sep + "00000");
	}

	[TearDown]
	public void TearDown ()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	public void TestMinMax()
	{
		Assert.AreEqual(Byte.MinValue, MyByte2);
		Assert.AreEqual(Byte.MaxValue, MyByte3);
	}
	
	public void TestCompareTo()
	{
		Assert.IsTrue (MyByte3.CompareTo(MyByte2) > 0);
		Assert.IsTrue (MyByte2.CompareTo(MyByte2) == 0);
		Assert.IsTrue (MyByte1.CompareTo((object)(Byte)42) == 0);
		Assert.IsTrue (MyByte2.CompareTo(MyByte3) < 0);
		try {
			MyByte2.CompareTo((object)100);
			Assert.Fail ("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert.IsTrue (typeof(ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert.IsTrue (MyByte1.Equals(MyByte1));
		Assert.IsTrue (MyByte1.Equals((object)(Byte)42));
		Assert.IsTrue (MyByte1.Equals((object)(Int16)42) == false);
		Assert.IsTrue (MyByte1.Equals(MyByte2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MyByte1.GetHashCode();
			MyByte2.GetHashCode();
			MyByte3.GetHashCode();
		}
		catch {
			Assert.Fail ("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert.IsTrue (MyByte1 == Byte.Parse(MyString1), "MyByte1="+MyByte1+", MyString1="+MyString1+", Parse="+Byte.Parse(MyString1));
		Assert.IsTrue(MyByte2 == Byte.Parse(MyString2), "MyByte2");
		Assert.IsTrue(MyByte3 == Byte.Parse(MyString3), "MyByte3");

		try {
			Byte.Parse(null);
			Assert.Fail ("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentNullException) == e.GetType(), "Should get ArgumentNullException");
		}
		try {
			Byte.Parse("not-a-number");
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType(), "not-a-number");
		}

		//test Parse(string s, NumberStyles style)
		Assert.AreEqual((byte)42, Byte.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ", NumberStyles.Currency),
						" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ");
		try {
			Byte.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer);
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue (typeof(FormatException) == e.GetType(), NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 and NumberStyles.Integer");
		}
		//test Parse(string s, IFormatProvider provider)
		Assert.IsTrue(42 == Byte.Parse(" 42 ", Nfi), " 42 and Nfi");
		try {
			Byte.Parse("%42", Nfi);
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType(), "%42 and Nfi");
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert.IsTrue(16 == Byte.Parse(" 10 ", NumberStyles.HexNumber, Nfi), "NumberStyles.HexNumber");
		try {
			Byte.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer, Nfi);
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue (typeof(FormatException) == e.GetType(), NumberFormatInfo.CurrentInfo.CurrencySymbol+"42, NumberStyles.Integer, Nfi");
		}

		Assert.AreEqual (734, Int64.Parse ("734\0"), "#1");
		Assert.AreEqual (734, Int64.Parse ("734\0\0\0    \0"), "#2");
		Assert.AreEqual (734, Int64.Parse ("734\0\0\0    "), "#3");
		Assert.AreEqual (734, Int64.Parse ("734\0\0\0"), "#4");
	}

	[Test]
	[ExpectedException (typeof(OverflowException))]
	public void ParseOverflow()
	{
		int OverInt = Byte.MaxValue + 1;
		Byte.Parse(OverInt.ToString());
	}


	public void TestToString()
	{
		//test ToString()
		Assert.AreEqual(MyString1, MyByte1.ToString(), "Compare failed for MyString1 and MyByte1");
		Assert.AreEqual(MyString2, MyByte2.ToString(), "Compare failed for MyString2 and MyByte2");
		Assert.AreEqual(MyString3, MyByte3.ToString(), "Compare failed for MyString3 and MyByte3");
		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			Assert.AreEqual(Results1[i], MyByte2.ToString(Formats1[i]), "Compare failed for Formats1["+i.ToString()+"]");
			Assert.AreEqual(Results2[i], MyByte3.ToString(Formats2[i]), "Compare failed for Formats2["+i.ToString()+"]");
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert.AreEqual(Results1_Nfi[i], MyByte2.ToString(Formats1[i], Nfi), "Compare failed for Formats1["+i.ToString()+"] with Nfi");
			Assert.AreEqual(Results2_Nfi[i], MyByte3.ToString(Formats2[i], Nfi), "Compare failed for Formats2["+i.ToString()+"] with Nfi");
		}
		try {
			MyByte1.ToString("z");
			Assert.Fail ("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.AreEqual(typeof(FormatException), e.GetType(), "Exception is the wrong type");
		}
		
	}

	[Test]
	public void ToString_Defaults () 
	{
		byte i = 254;
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
