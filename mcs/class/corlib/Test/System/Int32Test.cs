// Int32Test.cs - NUnit Test Cases for the System.Int32 struct
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
	private string[] Results1 = {null,
					"-2147483648", "-2.147484e+009", "-2147483648.00",
					"-2147483648", "-2,147,483,648.00", "-214,748,364,800.00 %", "80000000"};
	private string[] Results2 = {null,
					"2147483647", "2.14748e+009", "2147483647.00000",
					"2.1475e+09", "2,147,483,647.00000", "214,748,364,700.00000 %", "7fffffff"};
	private string[] ResultsNfi1 = {"("+NumberFormatInfo.InvariantInfo.CurrencySymbol+"2,147,483,648.00)",
					"-2147483648", "-2.147484e+009", "-2147483648.00",
					"-2147483648", "-2,147,483,648.00", "-214,748,364,800.00 %", "80000000"};
	private string[] ResultsNfi2 = {NumberFormatInfo.InvariantInfo.CurrencySymbol+"2,147,483,647.00000",
					"2147483647", "2.14748e+009", "2147483647.00000",
					"2.1475e+09", "2,147,483,647.00000", "214,748,364,700.00000 %", "7fffffff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public Int32Test() {}

	private CultureInfo old_culture;

	protected override void SetUp() 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		// We can't initialize this until we set the culture.
		Results1 [0] = "("+NumberFormatInfo.CurrentInfo.CurrencySymbol+"2,147,483,648.00)";
		Results2 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol+"2,147,483,647.00000";
	}

	protected override void TearDown()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	public void TestMinMax()
	{
		
		AssertEquals("#A01", Int32.MinValue, MyInt32_2);
		AssertEquals("#A02", Int32.MaxValue, MyInt32_3);
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
		Assert ("#B01", MyInt32_1.Equals (MyInt32_1));
		Assert ("#B02", MyInt32_1.Equals ((Int32)(-42)));
		Assert ("#B03", MyInt32_1.Equals ((SByte)(-42)) == false);
		Assert ("#B04", MyInt32_1.Equals (MyInt32_2) == false);
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
		AssertEquals ("#C01", MyInt32_1, Int32.Parse (MyString1));
		AssertEquals ("#C02", MyInt32_2, Int32.Parse (MyString2));
		AssertEquals ("#C03", MyInt32_3, Int32.Parse (MyString3));

		AssertEquals ("#C04", 1, Int32.Parse ("1"));
		AssertEquals ("#C05", 1, Int32.Parse (" 1"));
		AssertEquals ("#C06", 1, Int32.Parse ("     1"));
		AssertEquals ("#C07", 1, Int32.Parse ("1    "));
		AssertEquals ("#C08", 1, Int32.Parse ("+1"));
		AssertEquals ("#C09", -1, Int32.Parse ("-1"));
		AssertEquals ("#C10", -1, Int32.Parse ("  -1"));
		AssertEquals ("#C11", -1, Int32.Parse ("  -1  "));
		AssertEquals ("#C12", -1, Int32.Parse ("  -1  "));

		try {
			Int32.Parse(null);
			Fail ("#C13: Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert ("#C14", typeof (ArgumentNullException) == e.GetType());
		}
		try {
			Int32.Parse("not-a-number");
			Fail ("#C15: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert ("#C16", typeof (FormatException) == e.GetType());
		}
		try {
			double OverInt = (double)Int32.MaxValue + 1;
			Int32.Parse(OverInt.ToString());
			Fail ("#C17: Should raise a System.OverflowException");
		}
		catch (Exception e) {
			AssertEquals ("#C18", typeof (OverflowException), e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		AssertEquals ("#C19", 42, Int32.Parse (" $42 ", NumberStyles.Currency));
		try {
			Int32.Parse("$42", NumberStyles.Integer);
			Fail ("#C20: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert ("#C21", typeof (FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		AssertEquals ("#C22", -42, Int32.Parse (" -42 ", Nfi));
		try {
			Int32.Parse("%42", Nfi);
			Fail ("#C23: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert ("#C24", typeof (FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		AssertEquals ("#C25", 16, Int32.Parse (" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			Int32.Parse("$42", NumberStyles.Integer, Nfi);
			Fail ("#C26: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert("#C27", typeof (FormatException) == e.GetType());
		}

		try {
			Int32.Parse (" - 1 ");
			Fail ("#C28: Should raise FormatException");
		} catch (Exception e){
			Assert ("#C29", typeof (FormatException) == e.GetType ());
		}

		try {
			Int32.Parse (" - ");
			Fail ("#C30: Should raise FormatException");
		} catch (Exception e){
			Assert ("#C31", typeof (FormatException) == e.GetType ());
		}
	}
	
	public void TestToString()
	{
		//test ToString()
		AssertEquals ("#D01", MyString1, MyInt32_1.ToString ());
		AssertEquals ("#D02", MyString2, MyInt32_2.ToString ());
		AssertEquals ("#D03", MyString3, MyInt32_3.ToString ());

		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			AssertEquals ("#D04(" + i + "," + Formats1 [i] + ")",
				      ResultsNfi1 [i], MyInt32_2.ToString (Formats1 [i], Nfi));
			AssertEquals ("#D05(" + i + "," + Formats2 [i] + ")",
				      ResultsNfi2 [i], MyInt32_3.ToString (Formats2 [i], Nfi));
		}

		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			AssertEquals ("#D06(" + i + ")", Results1 [i],
				      MyInt32_2.ToString(Formats1[i]));
			AssertEquals ("#D07(" + i + ")", Results2 [i],
				      MyInt32_3.ToString(Formats2[i]));
		}

		try {
			MyInt32_1.ToString("z");
			Fail ("#D08: Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert ("#D09", typeof (FormatException) == e.GetType());
		}
	}

	public void TestCustomToString()
	{
		int i = 123;

		AssertEquals ("Custom format string 00000", "00123", i.ToString ("00000"));
		AssertEquals ("Custom format string ####", "123", i.ToString ("####"));
		AssertEquals ("Custom format string ####", "0123", i.ToString ("0###"));
		AssertEquals ("Custom format string ####", "0123", i.ToString ("#0###"));
		AssertEquals ("Custom format string ####", "000123", i.ToString ("0#0###"));
	}
}

}
