// UInt32Test.cs - NUnit Test Cases for the System.UInt32 struct
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

public class UInt32Test : TestCase
{
	private const UInt32 MyUInt32_1 = 42;
	private const UInt32 MyUInt32_2 = 0;
	private const UInt32 MyUInt32_3 = 4294967295;
	private const string MyString1 = "42";
	private const string MyString2 = "0";
	private const string MyString3 = "4294967295";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {"",
					"0", "0.000000e+000", "0.00",
					"0", "0.00", "0.00 %", "0"};
	private string[] ResultsNfi1 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"0.00",
					"0", "0.000000e+000", "0.00",
					"0", "0.00", "0.00 %", "0"};
	private string[] Results2 = {"",
					"4294967295", "4.29497e+009", "4294967295.00000",
					"4.295e+09", "4,294,967,295.00000", "429,496,729,500.00000 %", "ffffffff"};
	private string[] ResultsNfi2 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"4,294,967,295.00000",
					"4294967295", "4.29497e+009", "4294967295.00000",
					"4.295e+09", "4,294,967,295.00000", "429,496,729,500.00000 %", "ffffffff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public UInt32Test() {}

	private CultureInfo old_culture;

	protected override void SetUp() 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		// We can't initialize this until we set the culture.
		Results1 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol+"0.00";
		Results2 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol+"4,294,967,295.00000";
	}

	protected override void TearDown()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	public void TestMinMax()
	{
		
		AssertEquals(UInt32.MinValue, MyUInt32_2);
		AssertEquals(UInt32.MaxValue, MyUInt32_3);
	}
	
	public void TestCompareTo()
	{
		Assert(MyUInt32_3.CompareTo(MyUInt32_2) > 0);
		Assert(MyUInt32_2.CompareTo(MyUInt32_2) == 0);
		Assert(MyUInt32_1.CompareTo((UInt32)(42)) == 0);
		Assert(MyUInt32_2.CompareTo(MyUInt32_3) < 0);
		Assert (1 == UInt32.Parse ("1"));
		Assert (1 == UInt32.Parse (" 1"));
		Assert (1 == UInt32.Parse ("     1"));
		Assert (1 == UInt32.Parse ("1    "));
		Assert (1 == UInt32.Parse ("+1"));

		try {
			UInt32.Parse (" + 1 ");
			Fail ("Should raise FormatException1");
		} catch (Exception e){
			Assert (typeof (FormatException) == e.GetType ());
		}

		try {
			UInt32.Parse (" + ");
			Fail ("Should raise FormatException");
		} catch (Exception e){
			Assert (typeof (FormatException) == e.GetType ());
		}
		try {
			MyUInt32_2.CompareTo((Int16)100);
			Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert(typeof(ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert(MyUInt32_1.Equals(MyUInt32_1));
		Assert(MyUInt32_1.Equals((UInt32)(42)));
		Assert(MyUInt32_1.Equals((SByte)(42)) == false);
		Assert(MyUInt32_1.Equals(MyUInt32_2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MyUInt32_1.GetHashCode();
			MyUInt32_2.GetHashCode();
			MyUInt32_3.GetHashCode();
		}
		catch {
			Fail("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert("Parse problem on \""+MyString1+"\"", MyUInt32_1 == UInt32.Parse(MyString1));
		Assert("Parse problem on \""+MyString2+"\"", MyUInt32_2 == UInt32.Parse(MyString2));
		Assert("Parse problem on \""+MyString3+"\"", MyUInt32_3 == UInt32.Parse(MyString3));
		try {
			UInt32.Parse(null);
			Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert("Did not get ArgumentNullException type", typeof(ArgumentNullException) == e.GetType());
		}
		try {
			UInt32.Parse("not-a-number");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert("Did not get FormatException type", typeof(FormatException) == e.GetType());
		}
		try {
			// TODO: Use this after ToString() is completed. For now, hard code string that generates
			// exception.
			//double OverInt = (double)UInt32.MaxValue + 1;
			//UInt32.Parse(OverInt.ToString());
			UInt32.Parse("4294967296");
			Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert("Did not get OverflowException type on '"+"4294967296"+"'. Instead, got: '"+e.GetType()+"'", typeof(OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		Assert(42 == UInt32.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ", NumberStyles.Currency));
		try {
			UInt32.Parse("$42", NumberStyles.Integer);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert(42 == UInt32.Parse(" 42 ", Nfi));
		try {
			UInt32.Parse("%42", Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert(16 == UInt32.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			UInt32.Parse("$42", NumberStyles.Integer, Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
	}
	
	public void TestToString()
	{
		int TestNumber = 1;
		try {
			//test ToString()
			AssertEquals(MyString1, MyUInt32_1.ToString());
			TestNumber++;
			AssertEquals(MyString2, MyUInt32_2.ToString());
			TestNumber++;
			AssertEquals(MyString3, MyUInt32_3.ToString());
		} catch (Exception e) {
			Fail("TestToString: Failed on TestNumber=" + TestNumber 
				+ " with exception: " + e.ToString());
		}

		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			try {
				AssertEquals(Results1[i], MyUInt32_2.ToString(Formats1[i]));
			} catch (Exception e) {
				Fail("TestToString: MyUInt32_2.ToString(Formats1[i]) i=" + i 
					+ ". e = " + e.ToString());
			}

			try {
				AssertEquals(Results2[i], MyUInt32_3.ToString(Formats2[i]));
			} catch (Exception e) {
				Fail("TestToString: MyUInt32_3.ToString(Formats2[i]) i=" + i
					+ ". e = " + e.ToString());
			}
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			AssertEquals(ResultsNfi1[i], MyUInt32_2.ToString(Formats1[i], Nfi));
			AssertEquals(ResultsNfi2[i], MyUInt32_3.ToString(Formats2[i], Nfi));
		}
		try {
			MyUInt32_1.ToString("z");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
	}
}


}
