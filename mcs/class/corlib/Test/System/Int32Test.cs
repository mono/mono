// Int32Test.cs - NUnit Test Cases for the System.Int32 struct
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

public class Int32Test : TestCase
{
	private const Int32 MyInt32_1 = -42;
	private const Int32 MyInt32_2 = -2147483648;
	private const Int32 MyInt32_3 = 2147483647;
	private const string MyString1 = "-42";
	private const string MyString2 = "-2147483648";
	private const string MyString3 = "2147483647";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {"($2,147,483,648.00)", "-2147483648", "-2.147484e+009", "-2147483648.00",
	                                  "-2147483648", "-2,147,483,648.00", "-214,748,364,800.00 %", "80000000"};
	private string[] Results2 = {"$2,147,483,647.00000", "2147483647", "2.14748e+009", "2147483647.00000",
	                                  "2.1475e+09", "2,147,483,647.00000", "214,748,364,700.00000 %", "7fffffff"};
	private string[] ResultsNfi1 = {"($2,147,483,648.00)", "-2147483648", "-2.147484e+009", "-2147483648.00",
	                                  "-2147483648", "(2,147,483,648.00)", "-214,748,364,800.00 %", "80000000"};
	private string[] ResultsNfi2 = {"$2,147,483,647.00000", "2147483647", "2.14748e+009", "2147483647.00000",
	                                  "2.1475e+09", "2,147,483,647.00000", "214,748,364,700.00000 %", "7fffffff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public Int32Test(string name) : base(name) {}

	protected override void SetUp() 
	{
	}

	public static ITest Suite {
		get { 
			return new TestSuite(typeof(Int32Test)); 
		}
	}
    
	public void TestMinMax()
	{
		
		AssertEquals(Int32.MinValue, MyInt32_2);
		AssertEquals(Int32.MaxValue, MyInt32_3);
	}
	
	public void TestCompareTo()
	{
		Assert("MyInt32_3.CompareTo(MyInt32_2) > 0", MyInt32_3.CompareTo(MyInt32_2) > 0);
		Assert("MyInt32_2.CompareTo(MyInt32_2) == 0", MyInt32_2.CompareTo(MyInt32_2) == 0);
		Assert("MyInt32_1.CompareTo((Int32)(-42)) == 0", MyInt32_1.CompareTo((Int32)(-42)) == 0);
		Assert("MyInt32_2.CompareTo(MyInt32_3) < 0", MyInt32_2.CompareTo(MyInt32_3) < 0);
		try {
			MyInt32_2.CompareTo((Int16)100);
			Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert("typeof(ArgumentException) == e.GetType()", typeof(ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert(MyInt32_1.Equals(MyInt32_1));
		Assert(MyInt32_1.Equals((Int32)(-42)));
		Assert(MyInt32_1.Equals((SByte)(-42)) == false);
		Assert(MyInt32_1.Equals(MyInt32_2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MyInt32_1.GetHashCode();
			MyInt32_2.GetHashCode();
			MyInt32_3.GetHashCode();
		}
		catch {
			Fail("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert(MyInt32_1 == Int32.Parse(MyString1));
		Assert(MyInt32_2 == Int32.Parse(MyString2));
		Assert(MyInt32_3 == Int32.Parse(MyString3));

		Assert (1 == Int32.Parse ("1"));
		Assert (1 == Int32.Parse (" 1"));
		Assert (1 == Int32.Parse ("     1"));
		Assert (1 == Int32.Parse ("1    "));
		Assert (1 == Int32.Parse ("+1"));
		Assert (-1 == Int32.Parse ("-1"));
		Assert (-1 == Int32.Parse ("  -1"));
		Assert (-1 == Int32.Parse ("  -1  "));
		Assert (-1 == Int32.Parse ("  -1  "));

		try {
			Int32.Parse(null);
			Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert(typeof(ArgumentNullException) == e.GetType());
		}
		try {
			Int32.Parse("not-a-number");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		try {
			double OverInt = (double)Int32.MaxValue + 1;
			Int32.Parse(OverInt.ToString());
			Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert(typeof(OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		Assert(42 == Int32.Parse(" $42 ", NumberStyles.Currency));
		try {
			Int32.Parse("$42", NumberStyles.Integer);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert(-42 == Int32.Parse(" -42 ", Nfi));
		try {
			Int32.Parse("%42", Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert(16 == Int32.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			Int32.Parse("$42", NumberStyles.Integer, Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}

		try {
			Int32.Parse (" - 1 ");
			Fail ("Should raise FormatException");
		} catch (Exception e){
			Assert (typeof (FormatException) == e.GetType ());
		}

		try {
			Int32.Parse (" - ");
			Fail ("Should raise FormatException");
		} catch (Exception e){
			Assert (typeof (FormatException) == e.GetType ());
		}
	}
	
	public void TestToString()
	{
		//test ToString()
		Assert(String.Compare(MyString1, MyInt32_1.ToString()) == 0);
		Assert(String.Compare(MyString2, MyInt32_2.ToString()) == 0);
		Assert(String.Compare(MyString3, MyInt32_3.ToString()) == 0);
		//test ToString(string format)
		/*
		TODO: These tests are culture sensitive.  Need to find a way to determine the culture
			of the system to decide the correct expected result.
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(Results1[i], MyInt32_2.ToString(Formats1[i])) == 0);
			Assert(String.Compare(Results2[i], MyInt32_3.ToString(Formats2[i])) == 0);
		}
		*/
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(ResultsNfi1[i], MyInt32_2.ToString(Formats1[i], Nfi)) == 0);
			Assert(String.Compare(ResultsNfi2[i], MyInt32_3.ToString(Formats2[i], Nfi)) == 0);
		}
		try {
			MyInt32_1.ToString("z");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
	}
}

}
