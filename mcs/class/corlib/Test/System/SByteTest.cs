// SByteTest.cs - NUnit Test Cases for the System.SByte struct
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{

[TestFixture]
public class SByteTest 
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
	
	[Test]
	public void TestMinMax()
	{
		Assert.AreEqual(SByte.MinValue, MySByte2);
		Assert.AreEqual(SByte.MaxValue, MySByte3);
	}
	
	[Test]
	public void TestCompareTo()
	{
		Assert.IsTrue(MySByte3.CompareTo(MySByte2) > 0);
		Assert.IsTrue(MySByte2.CompareTo(MySByte2) == 0);
		Assert.IsTrue(MySByte1.CompareTo((SByte)(-42)) == 0);
		Assert.IsTrue(MySByte2.CompareTo(MySByte3) < 0);
		try {
			MySByte2.CompareTo((object)(int)100);
			Assert.Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentException) == e.GetType());
		}
	}

	[Test]
	public void TestEquals()
	{
		Assert.IsTrue(MySByte1.Equals(MySByte1));
		Assert.IsTrue(MySByte1.Equals((SByte)(-42)));
		Assert.IsTrue(MySByte1.Equals((Int16)(-42)) == false);
		Assert.IsTrue(MySByte1.Equals(MySByte2) == false);
	}

	[Test]	
	public void TestGetHashCode()
	{
		try {
			MySByte1.GetHashCode();
			MySByte2.GetHashCode();
			MySByte3.GetHashCode();
		}
		catch {
			Assert.Fail("GetHashCode should not raise an exception here");
		}
	}

	[Test]	
	public void TestParse()
	{
		//test Parse(string s)
		Assert.IsTrue(MySByte1 == SByte.Parse(MyString1));
		Assert.IsTrue(MySByte2 == SByte.Parse(MyString2));
		Assert.IsTrue(MySByte3 == SByte.Parse(MyString3));
		try {
			SByte.Parse(null);
			Assert.Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentNullException) == e.GetType());
		}
		try {
			SByte.Parse("not-a-number");
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		try {
			int OverInt = SByte.MaxValue + 1;
			SByte.Parse(OverInt.ToString());
			Assert.Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		Assert.AreEqual((sbyte)42, SByte.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ", NumberStyles.Currency), "A1");
		try {
			SByte.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert.IsTrue(-42 == SByte.Parse(" -42 ", Nfi));
		try {
			SByte.Parse("%42", Nfi);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert.IsTrue(16 == SByte.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			SByte.Parse(NumberFormatInfo.CurrentInfo.CurrencySymbol+"42", NumberStyles.Integer, Nfi);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}

		try {
			SByte.Parse ("1FF", NumberStyles.HexNumber);
			Assert.Fail ("#1");
		} catch (OverflowException) {
		}
	}

	[Test]
	public void Parse_MinMax () 
	{
		Assert.AreEqual (SByte.MinValue, SByte.Parse ("-128"), "MinValue");
		Assert.AreEqual (SByte.MaxValue, SByte.Parse ("127"), "MaxValue");
		Assert.AreEqual (-1, SByte.Parse ("FF", NumberStyles.HexNumber), "MaxHex");
	}

	[Test]	
	public void TestToString()
	{
		//test ToString()
		Assert.IsTrue(String.Compare(MyString1, MySByte1.ToString()) == 0, "MyString1, MySByte1.ToString()");
		Assert.IsTrue(String.Compare(MyString2, MySByte2.ToString()) == 0, "MyString2, MySByte2.ToString()");
		Assert.IsTrue(String.Compare(MyString3, MySByte3.ToString()) == 0, "MyString3, MySByte3.ToString()");
		//test ToString(string format)
		/*
		TODO: These tests depend on the culture of the system running the test.
			So, this needs to be tested in a different way.
		for (int i=0; i < Formats1.Length; i++) {
			Assert.IsTrue("i="+i+", Results1[i]="+Results1[i]+", MySByte2.ToString(Formats1[i])="+MySByte2.ToString(Formats1[i]), String.Compare(Results1[i], MySByte2.ToString(Formats1[i])) == 0);
			Assert.IsTrue(String.Compare(Results2[i], MySByte3.ToString(Formats2[i])) == 0, "Results2[i], MySByte3.ToString(Formats2[i])");
		}
		*/
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert.IsTrue(String.Compare(ResultsNfi1[i], MySByte2.ToString(Formats1[i], Nfi)) == 0, "i="+i+", ResultsNfi1[i]="+ResultsNfi1[i]+", MySByte2.ToString(Formats1[i]="+Formats1[i]+"): Expected "+ResultsNfi1[i]+" but got "+MySByte2.ToString(Formats1[i], Nfi));
			Assert.IsTrue(String.Compare(ResultsNfi2[i], MySByte3.ToString(Formats2[i], Nfi)) == 0, "ResultsNfi2[i], MySByte3.ToString(Formats2[i], Nfi):"+ResultsNfi2[i]+"<==>"+MySByte3.ToString(Formats2[i], Nfi));
		}
		try {
			MySByte1.ToString("z");
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType(), "typeof(FormatException) == e.GetType()");
		}
	}

	[Test]
	public void ToString_Defaults () 
	{
		SByte i = 100;
		// everything defaults to "G"
		string def = i.ToString ("G");
		Assert.AreEqual (def, i.ToString (), "ToString()");
		Assert.AreEqual (def, i.ToString ((IFormatProvider)null), "ToString((IFormatProvider)null)");
		Assert.AreEqual (def, i.ToString ((string)null), "ToString((string)null)");
		Assert.AreEqual (def, i.ToString (String.Empty), "ToString(empty)");
		Assert.AreEqual (def, i.ToString (null, null), "ToString(null,null)");
		Assert.AreEqual (def, i.ToString (String.Empty, null), "ToString(empty,null)");

		Assert.AreEqual ("100", def, "ToString(G)");
	}
		
	[Test]
	public void Bug3677 ()
	{
		Assert.AreEqual (-29, sbyte.Parse("E3", NumberStyles.HexNumber), "HexNumber");
	}
}

}
