// ByteTest.cs - NUnit Test Cases for the System.Byte struct
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Globalization;

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
	private string[] Results1 = {"$0.00", "0", "0.000000e+000", "0.00",
	                                  "0", "0.00", "0.00 %", "0"};
	private string[] Results2 = {"$255.00000", "00255", "2.55000e+002", "255.00000",
	                                  "255", "255.00000", "25,500.00000 %", "000ff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public ByteTest(string name) : base(name) {}

	protected override void SetUp() 
	{
	}

	public static ITest Suite {
		get { 
			return new TestSuite(typeof(ByteTest)); 
		}
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
			Assert(typeof(System.ArgumentException) == e.GetType());
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
		Assert(MyByte1 == Byte.Parse(MyString1));
		Assert(MyByte2 == Byte.Parse(MyString2));
		Assert(MyByte3 == Byte.Parse(MyString3));
		try {
			Byte.Parse(null);
			Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert(typeof(System.ArgumentNullException) == e.GetType());
		}
		try {
			Byte.Parse("not-a-number");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		try {
			int OverInt = Byte.MaxValue + 1;
			Byte.Parse(OverInt.ToString());
			Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert(typeof(System.OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		Assert(42 == Byte.Parse(" $42 ", NumberStyles.Currency));
		try {
			Byte.Parse("$42", NumberStyles.Integer);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert(42 == Byte.Parse(" 42 ", Nfi));
		try {
			Byte.Parse("%42", Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert(16 == Byte.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			Byte.Parse("$42", NumberStyles.Integer, Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
	}
	
	public void TestToString()
	{
		//test ToString()
		Assert(String.Compare(MyString1, MyByte1.ToString()) == 0);
		Assert(String.Compare(MyString2, MyByte2.ToString()) == 0);
		Assert(String.Compare(MyString3, MyByte3.ToString()) == 0);
		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(Results1[i], MyByte2.ToString(Formats1[i])) == 0);
			Assert(String.Compare(Results2[i], MyByte3.ToString(Formats2[i])) == 0);
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(Results1[i], MyByte2.ToString(Formats1[i], Nfi)) == 0);
			Assert(String.Compare(Results2[i], MyByte3.ToString(Formats2[i], Nfi)) == 0);
		}
		try {
			MyByte1.ToString("z");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		
	}
}

