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

public class ByteTest : TestCase
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

	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public ByteTest() {}

	protected override void SetUp() 
	{
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
		Results1[2] = "0" + sep + "000000e+000";
		Results1[3] = "0" + sep + "00";
		Results1[5] = "0" + sep + "00";
		Results1[6] = "0" + sep + "00 %";
		Results2[2] = "2" + sep + "55000e+002";
		Results2[3] = "255" + sep + "00000";
		Results2[3] = "255" + sep + "00000";
		Results2[5] = "255" + sep + "00000";
		string gsep = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		Results2[6] = "25" + gsep + "500" + sep + "00000 %";
	}

        protected override void TearDown () {
	}

	public void TestMinMax()
	{
		AssertEquals(Byte.MinValue, MyByte2);
		AssertEquals(Byte.MaxValue, MyByte3);
	}
	
	public void TestCompareTo()
	{
		Assert(MyByte3.CompareTo(MyByte2) > 0);
		Assert(MyByte2.CompareTo(MyByte2) == 0);
		Assert(MyByte1.CompareTo((Byte)42) == 0);
		Assert(MyByte2.CompareTo(MyByte3) < 0);
		try {
			MyByte2.CompareTo(100);
			Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert(typeof(ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert(MyByte1.Equals(MyByte1));
		Assert(MyByte1.Equals((Byte)42));
		Assert(MyByte1.Equals((Int16)42) == false);
		Assert(MyByte1.Equals(MyByte2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MyByte1.GetHashCode();
			MyByte2.GetHashCode();
			MyByte3.GetHashCode();
		}
		catch {
			Fail("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert("MyByte1="+MyByte1+", MyString1="+MyString1+", Parse="+Byte.Parse(MyString1) , MyByte1 == Byte.Parse(MyString1));
		Assert("MyByte2", MyByte2 == Byte.Parse(MyString2));
		Assert("MyByte3", MyByte3 == Byte.Parse(MyString3));

		try {
			Byte.Parse(null);
			Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert("Should get ArgumentNullException", typeof(ArgumentNullException) == e.GetType());
		}
		try {
			Byte.Parse("not-a-number");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert("not-a-number", typeof(FormatException) == e.GetType());
		}

		//test Parse(string s, NumberStyles style)
		AssertEquals(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ",
				(byte)42, Byte.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ",
						NumberStyles.Currency));
		try {
			Byte.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 and NumberStyles.Integer", typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert(" 42 and Nfi", 42 == Byte.Parse(" 42 ", Nfi));
		try {
			Byte.Parse("%42", Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert("%42 and Nfi", typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert("NumberStyles.HexNumber", 16 == Byte.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			Byte.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer, Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42, NumberStyles.Integer, Nfi", typeof(FormatException) == e.GetType());
		}
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
		AssertEquals("Compare failed for MyString1 and MyByte1", MyString1, MyByte1.ToString());
		AssertEquals("Compare failed for MyString2 and MyByte2", MyString2, MyByte2.ToString());
		AssertEquals("Compare failed for MyString3 and MyByte3", MyString3, MyByte3.ToString());
		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			AssertEquals("Compare failed for Formats1["+i.ToString()+"]", Results1[i], MyByte2.ToString(Formats1[i]));
			AssertEquals("Compare failed for Formats2["+i.ToString()+"]", Results2[i], MyByte3.ToString(Formats2[i]));
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			AssertEquals("Compare failed for Formats1["+i.ToString()+"] with Nfi", Results1_Nfi[i], MyByte2.ToString(Formats1[i], Nfi));
			AssertEquals("Compare failed for Formats2["+i.ToString()+"] with Nfi", Results2_Nfi[i], MyByte3.ToString(Formats2[i], Nfi));
		}
		try {
			MyByte1.ToString("z");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			AssertEquals("Exception is the wrong type", typeof(FormatException), e.GetType());
		}
		
	}
}

}
