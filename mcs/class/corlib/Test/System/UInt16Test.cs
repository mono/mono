// UInt16Test.cs - NUnit Test Cases for the System.UInt16 struct
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Globalization;

public class UInt16Test : TestCase
{
	private const UInt16 MyUInt16_1 = 42;
	private const UInt16 MyUInt16_2 = 0;
	private const UInt16 MyUInt16_3 = 65535;
	private const string MyString1 = "42";
	private const string MyString2 = "0";
	private const string MyString3 = "65535";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {"$0.00", "0", "0.000000e+000", "0.00",
	                                  "0", "0.00", "0.00 %", "0"};
	private string[] Results2 = {"$65,535.00000", "65535", "6.55350e+004", "65535.00000",
	                                  "65535", "65,535.00000", "6,553,500.00000 %", "0ffff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public UInt16Test(string name) : base(name) {}

	protected override void SetUp() 
	{
	}

	public static ITest Suite {
		get { 
			return new TestSuite(typeof(UInt16Test)); 
		}
	}
    
	public void TestMinMax()
	{
		
		AssertEquals(UInt16.MinValue, MyUInt16_2);
		AssertEquals(UInt16.MaxValue, MyUInt16_3);
	}
	
	public void TestCompareTo()
	{
		Assert(MyUInt16_3.CompareTo(MyUInt16_2) > 0);
		Assert(MyUInt16_2.CompareTo(MyUInt16_2) == 0);
		Assert(MyUInt16_1.CompareTo((UInt16)(42)) == 0);
		Assert(MyUInt16_2.CompareTo(MyUInt16_3) < 0);
		try {
			MyUInt16_2.CompareTo(100);
			Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert(typeof(System.ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert(MyUInt16_1.Equals(MyUInt16_1));
		Assert(MyUInt16_1.Equals((UInt16)(42)));
		Assert(MyUInt16_1.Equals((SByte)(42)) == false);
		Assert(MyUInt16_1.Equals(MyUInt16_2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MyUInt16_1.GetHashCode();
			MyUInt16_2.GetHashCode();
			MyUInt16_3.GetHashCode();
		}
		catch {
			Fail("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert(MyUInt16_1 == UInt16.Parse(MyString1));
		Assert(MyUInt16_2 == UInt16.Parse(MyString2));
		Assert(MyUInt16_3 == UInt16.Parse(MyString3));
		try {
			UInt16.Parse(null);
			Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert(typeof(System.ArgumentNullException) == e.GetType());
		}
		try {
			UInt16.Parse("not-a-number");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		try {
			int OverInt = UInt16.MaxValue + 1;
			UInt16.Parse(OverInt.ToString());
			Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert(typeof(System.OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		Assert(42 == UInt16.Parse(" $42 ", NumberStyles.Currency));
		try {
			UInt16.Parse("$42", NumberStyles.Integer);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert(42 == UInt16.Parse(" 42 ", Nfi));
		try {
			UInt16.Parse("%42", Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert(16 == UInt16.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			UInt16.Parse("$42", NumberStyles.Integer, Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
	}
	
	public void TestToString()
	{
		//test ToString()
		Assert(String.Compare(MyString1, MyUInt16_1.ToString()) == 0);
		Assert(String.Compare(MyString2, MyUInt16_2.ToString()) == 0);
		Assert(String.Compare(MyString3, MyUInt16_3.ToString()) == 0);
		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(Results1[i], MyUInt16_2.ToString(Formats1[i])) == 0);
			Assert(String.Compare(Results2[i], MyUInt16_3.ToString(Formats2[i])) == 0);
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(Results1[i], MyUInt16_2.ToString(Formats1[i], Nfi)) == 0);
			Assert(String.Compare(Results2[i], MyUInt16_3.ToString(Formats2[i], Nfi)) == 0);
		}
		try {
			MyUInt16_1.ToString("z");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
	}
}

