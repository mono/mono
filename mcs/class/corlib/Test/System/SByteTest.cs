// SByteTest.cs - NUnit Test Cases for the System.SByte struct
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{

public class SByteTest : TestCase
{
	private const SByte MySByte1 = -42;
	private const SByte MySByte2 = -128;
	private const SByte MySByte3 = 127;
	private const string MyString1 = "-42";
	private const string MyString2 = "-128";
	private const string MyString3 = "127";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {"("+NumberFormatInfo.CurrentInfo.CurrencySymbol+"128.00)",
					"-128", "-1.280000e+002", "-128.00",
					"-128", "-128.00", "-12,800.00 %", "80"};
	private string[] Results2 = {NumberFormatInfo.CurrentInfo.CurrencySymbol+"127.00000",
					"00127", "1.27000e+002", "127.00000",
					"127", "127.00000", "12,700.00000 %", "0007f"};
	private string[] ResultsNfi1 = {"("+NumberFormatInfo.InvariantInfo.CurrencySymbol+"128.00)", 
					"-128", "-1.280000e+002", "-128.00",
					"-128", "-128.00", "-12,800.00 %", "80"};
	private string[] ResultsNfi2 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"127.00000",
					"00127", "1.27000e+002", "127.00000",
					"127", "127.00000", "12,700.00000 %", "0007f"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public SByteTest() {}

	protected override void SetUp() 
	{
	}

	public void TestMinMax()
	{
		AssertEquals(SByte.MinValue, MySByte2);
		AssertEquals(SByte.MaxValue, MySByte3);
	}
	
	public void TestCompareTo()
	{
		Assert(MySByte3.CompareTo(MySByte2) > 0);
		Assert(MySByte2.CompareTo(MySByte2) == 0);
		Assert(MySByte1.CompareTo((SByte)(-42)) == 0);
		Assert(MySByte2.CompareTo(MySByte3) < 0);
		try {
			MySByte2.CompareTo(100);
			Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert(typeof(ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert(MySByte1.Equals(MySByte1));
		Assert(MySByte1.Equals((SByte)(-42)));
		Assert(MySByte1.Equals((Int16)(-42)) == false);
		Assert(MySByte1.Equals(MySByte2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MySByte1.GetHashCode();
			MySByte2.GetHashCode();
			MySByte3.GetHashCode();
		}
		catch {
			Fail("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert(MySByte1 == SByte.Parse(MyString1));
		Assert(MySByte2 == SByte.Parse(MyString2));
		Assert(MySByte3 == SByte.Parse(MyString3));
		try {
			SByte.Parse(null);
			Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert(typeof(ArgumentNullException) == e.GetType());
		}
		try {
			SByte.Parse("not-a-number");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		try {
			int OverInt = SByte.MaxValue + 1;
			SByte.Parse(OverInt.ToString());
			Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert(typeof(OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		AssertEquals("A1", (sbyte)42, SByte.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ", NumberStyles.Currency));
		try {
			SByte.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert(-42 == SByte.Parse(" -42 ", Nfi));
		try {
			SByte.Parse("%42", Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert(16 == SByte.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			SByte.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer, Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
	}
	
	public void TestToString()
	{
		//test ToString()
		Assert("MyString1, MySByte1.ToString()", String.Compare(MyString1, MySByte1.ToString()) == 0);
		Assert("MyString2, MySByte2.ToString()", String.Compare(MyString2, MySByte2.ToString()) == 0);
		Assert("MyString3, MySByte3.ToString()", String.Compare(MyString3, MySByte3.ToString()) == 0);
		//test ToString(string format)
		/*
		TODO: These tests depend on the culture of the system running the test.
			So, this needs to be tested in a different way.
		for (int i=0; i < Formats1.Length; i++) {
			Assert("i="+i+", Results1[i]="+Results1[i]+", MySByte2.ToString(Formats1[i])="+MySByte2.ToString(Formats1[i]), String.Compare(Results1[i], MySByte2.ToString(Formats1[i])) == 0);
			Assert("Results2[i], MySByte3.ToString(Formats2[i])", String.Compare(Results2[i], MySByte3.ToString(Formats2[i])) == 0);
		}
		*/
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert("i="+i+", ResultsNfi1[i]="+ResultsNfi1[i]+", MySByte2.ToString(Formats1[i]="+Formats1[i]+"): Expected "+ResultsNfi1[i]+" but got "+MySByte2.ToString(Formats1[i], Nfi), String.Compare(ResultsNfi1[i], MySByte2.ToString(Formats1[i], Nfi)) == 0);
			Assert("ResultsNfi2[i], MySByte3.ToString(Formats2[i], Nfi):"+ResultsNfi2[i]+"<==>"+MySByte3.ToString(Formats2[i], Nfi), String.Compare(ResultsNfi2[i], MySByte3.ToString(Formats2[i], Nfi)) == 0);
		}
		try {
			MySByte1.ToString("z");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert("typeof(FormatException) == e.GetType()", typeof(FormatException) == e.GetType());
		}
	}
}

}
