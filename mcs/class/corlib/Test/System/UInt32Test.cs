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

[TestFixture]
public class UInt32Test 
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
	
	private CultureInfo old_culture;

	[SetUp]
	public void SetUp () 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		// We can't initialize this until we set the culture.
		
		string decimals = new String ('0', NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
		string perPattern = new string[] {"n %","n%","%n"} [NumberFormatInfo.CurrentInfo.PercentPositivePattern];
		
		Results1 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol + "0.00";
		Results1 [3] = "0." + decimals;
		Results1 [5] = "0." + decimals;
		Results1 [6] = perPattern.Replace ("n","0.00");
		
		Results2 [0] = NumberFormatInfo.CurrentInfo.CurrencySymbol + "4,294,967,295.00000";
		Results2 [6] = perPattern.Replace ("n","429,496,729,500.00000");
	}

	[TearDown]
	public void TearDown ()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	[Test]
	public void TestMinMax()
	{
		
		Assert.AreEqual(UInt32.MinValue, MyUInt32_2);
		Assert.AreEqual(UInt32.MaxValue, MyUInt32_3);
	}

	[Test]
	public void TestCompareTo()
	{
		Assert.IsTrue(MyUInt32_3.CompareTo(MyUInt32_2) > 0);
		Assert.IsTrue(MyUInt32_2.CompareTo(MyUInt32_2) == 0);
		Assert.IsTrue(MyUInt32_1.CompareTo((UInt32)(42)) == 0);
		Assert.IsTrue(MyUInt32_2.CompareTo(MyUInt32_3) < 0);
		Assert.IsTrue (1 == UInt32.Parse ("1"));
		Assert.IsTrue (1 == UInt32.Parse (" 1"));
		Assert.IsTrue (1 == UInt32.Parse ("     1"));
		Assert.IsTrue (1 == UInt32.Parse ("1    "));
		Assert.IsTrue (1 == UInt32.Parse ("+1"));

		try {
			UInt32.Parse (" + 1 ");
			Assert.Fail ("Should raise FormatException1");
		} catch (Exception e){
			Assert.IsTrue (typeof (FormatException) == e.GetType ());
		}

		try {
			UInt32.Parse (" + ");
			Assert.Fail ("Should raise FormatException");
		} catch (Exception e){
			Assert.IsTrue (typeof (FormatException) == e.GetType ());
		}
		try {
			MyUInt32_2.CompareTo((object)(Int16)100);
			Assert.Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentException) == e.GetType());
		}
	}

	[Test]
	public void TestEquals()
	{
		Assert.IsTrue(MyUInt32_1.Equals(MyUInt32_1));
		Assert.IsTrue(MyUInt32_1.Equals((object)(UInt32)(42)));
		Assert.IsTrue(MyUInt32_1.Equals((object)(SByte)(42)) == false);
		Assert.IsTrue(MyUInt32_1.Equals(MyUInt32_2) == false);
	}

	[Test]
	public void TestGetHashCode()
	{
		try {
			MyUInt32_1.GetHashCode();
			MyUInt32_2.GetHashCode();
			MyUInt32_3.GetHashCode();
		}
		catch {
			Assert.Fail("GetHashCode should not raise an exception here");
		}
	}

	[Test]
	public void TestParse()
	{
		//test Parse(string s)
		Assert.IsTrue(MyUInt32_1 == UInt32.Parse(MyString1), "Parse problem on \""+MyString1+"\"");
		Assert.IsTrue(MyUInt32_2 == UInt32.Parse(MyString2), "Parse problem on \""+MyString2+"\"");
		Assert.IsTrue(MyUInt32_3 == UInt32.Parse(MyString3), "Parse problem on \""+MyString3+"\"");
		try {
			UInt32.Parse(null);
			Assert.Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(ArgumentNullException) == e.GetType(), "Did not get ArgumentNullException type");
		}
		try {
			UInt32.Parse("not-a-number");
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType(), "Did not get FormatException type");
		}
		try {
			// TODO: Use this after ToString() is completed. For now, hard code string that generates
			// exception.
			//double OverInt = (double)UInt32.MaxValue + 1;
			//UInt32.Parse(OverInt.ToString());
			UInt32.Parse("4294967296");
			Assert.Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(OverflowException) == e.GetType(), "Did not get OverflowException type on '"+"4294967296"+"'. Instead, got: '"+e.GetType()+"'");
		}
		//test Parse(string s, NumberStyles style)
		Assert.IsTrue(42 == UInt32.Parse(" "+NumberFormatInfo.CurrentInfo.CurrencySymbol+"42 ", NumberStyles.Currency));
		try {
			UInt32.Parse("$42", NumberStyles.Integer);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert.IsTrue(42 == UInt32.Parse(" 42 ", Nfi));
		try {
			UInt32.Parse("%42", Nfi);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert.IsTrue(16 == UInt32.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			UInt32.Parse("$42", NumberStyles.Integer, Nfi);
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
		// Pass a DateTimeFormatInfo, it is unable to format
		// numbers, but we should not crash
		
		UInt32.Parse ("123", new DateTimeFormatInfo ());

		Assert.AreEqual (734561, UInt32.Parse ("734561\0"), "C#43");
		Assert.AreEqual (734561, UInt32.Parse ("734561\0\0\0    \0"), "C#44");
		Assert.AreEqual (734561, UInt32.Parse ("734561\0\0\0    "), "C#45");
		Assert.AreEqual (734561, UInt32.Parse ("734561\0\0\0"), "C#46");

		Assert.AreEqual (0, UInt32.Parse ("0+", NumberStyles.Any), "#50");
	}

	[Test]
	public void TestParseExponent ()
	{
		Assert.AreEqual (2, uint.Parse ("2E0", NumberStyles.AllowExponent), "A#1");
		Assert.AreEqual (20, uint.Parse ("2E1", NumberStyles.AllowExponent), "A#2");
		Assert.AreEqual (200, uint.Parse ("2E2", NumberStyles.AllowExponent), "A#3");
		Assert.AreEqual (2000000, uint.Parse ("2E6", NumberStyles.AllowExponent), "A#4");
		Assert.AreEqual (200, uint.Parse ("2E+2", NumberStyles.AllowExponent), "A#5");
		Assert.AreEqual (2, uint.Parse ("2", NumberStyles.AllowExponent), "A#6");

		try {
			uint.Parse ("2E");
			Assert.Fail ("B#1");
		} catch (FormatException) {
		}

		try {
			uint.Parse ("2E3.0", NumberStyles.AllowExponent); // decimal notation for the exponent
			Assert.Fail ("B#2");
		} catch (FormatException) {
		}

		try {
			uint.Parse ("2E 2", NumberStyles.AllowExponent);
			Assert.Fail ("B#3");
		} catch (FormatException) {
		}

		try {
			uint.Parse ("2E2 ", NumberStyles.AllowExponent);
			Assert.Fail ("B#4");
		} catch (FormatException) {
		}

		try {
			uint.Parse ("2E66", NumberStyles.AllowExponent); // final result overflow
			Assert.Fail ("B#5");
		} catch (OverflowException) {
		}

		try {
			long exponent = (long) Int32.MaxValue + 10;
			uint.Parse ("2E" + exponent.ToString (), NumberStyles.AllowExponent);
			Assert.Fail ("B#6");
		} catch (OverflowException) {
		}

		try {
			uint.Parse ("2E-1", NumberStyles.AllowExponent); // negative exponent
			Assert.Fail ("B#7");
		} catch (OverflowException) {
		}
		
		try {
			uint.Parse ("2 math e1", NumberStyles.AllowExponent);
			Assert.Fail ("B#8");
		} catch (FormatException) {
		}
	}

	[Test]
	public void TestToString()
	{
		int TestNumber = 1;
		try {
			//test ToString()
			Assert.AreEqual(MyString1, MyUInt32_1.ToString());
			TestNumber++;
			Assert.AreEqual(MyString2, MyUInt32_2.ToString());
			TestNumber++;
			Assert.AreEqual(MyString3, MyUInt32_3.ToString());
		} catch (Exception e) {
			Assert.Fail("TestToString: Assert.Failed on TestNumber=" + TestNumber 
				+ " with exception: " + e.ToString());
		}

		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			try {
				Assert.AreEqual(Results1[i], MyUInt32_2.ToString(Formats1[i]));
			} catch (Exception e) {
				Assert.Fail("TestToString: MyUInt32_2.ToString(Formats1[i]) i=" + i 
					+ ". e = " + e.ToString());
			}

			try {
				Assert.AreEqual(Results2[i], MyUInt32_3.ToString(Formats2[i]));
			} catch (Exception e) {
				Assert.Fail("TestToString: MyUInt32_3.ToString(Formats2[i]) i=" + i
					+ ". e = " + e.ToString());
			}
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert.AreEqual(ResultsNfi1[i], MyUInt32_2.ToString(Formats1[i], Nfi));
			Assert.AreEqual(ResultsNfi2[i], MyUInt32_3.ToString(Formats2[i], Nfi));
		}
		try {
			MyUInt32_1.ToString("z");
			Assert.Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert.IsTrue(typeof(FormatException) == e.GetType());
		}
	}

	[Test]
	public void ToString_Defaults () 
	{
		UInt32 i = 254;
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
