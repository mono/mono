// BooleanTest.cs - NUnit Test Cases for the System.Boolean class
//
// Bob Doan <bdoan@sicompos.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{

public class BooleanTest : TestCase
{
	public BooleanTest (string name) : base (name) {}

	protected override void SetUp ()
	{
	}

	public static ITest Suite {
		get {
			return new TestSuite (typeof (BooleanTest));
		}
	}

	public void TestStrings ()
	{
		AssertEquals("Wrong False string", Boolean.FalseString, "False");
		AssertEquals("Wrong True string", Boolean.TrueString, "True");
	}
	
	public void TestCompareTo() {
		Boolean t=true,f=false;
		String s = "What Ever";
		AssertEquals("CompareTo Failed", f.CompareTo(t) < 0, true);
		AssertEquals("CompareTo Failed", f.CompareTo(f), 0);
		AssertEquals("CompareTo Failed", t.CompareTo(t), 0);
		AssertEquals("CompareTo Failed", t.CompareTo(f) > 0, true);		
		AssertEquals("CompareTo Failed", t.CompareTo(null) > 0, true);
		try {
			t.CompareTo(s);
			Fail("CompareTo should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert("CompareTo should be a System.ArgumentException", typeof(ArgumentException) == e.GetType());
		}		
	}

	public void TestEquals() {
		Boolean t=true, f=false;
		String s = "What Ever";
		AssertEquals("Equals Failed", t.Equals(t), true);
		AssertEquals("Equals Failed", f.Equals(f), true);
		AssertEquals("Equals Failed", f.Equals(t), false);
		AssertEquals("Equals Failed", t.Equals(null), false);
		AssertEquals("Equals Failed", t.Equals(s), false);
	}

	public void TestGetHashCode() {
		Boolean t=true, f=false;
		AssertEquals("GetHashCode True failed", t.GetHashCode(), 1);
		AssertEquals("GetHashCode True failed", f.GetHashCode(), 0);
	}

	public void TestGetType() {
		Boolean t=true, f=false;
		AssertEquals("GetType failed", Object.ReferenceEquals(t.GetType(), f.GetType()), true);
	}

	public void TestGetTypeCode() {
		Boolean b=true;
		AssertEquals("GetTypeCode failed", b.GetTypeCode(), TypeCode.Boolean);
	}

	public void TestParse() {
		AssertEquals("Parse True failed", Boolean.Parse("True"), true);
		AssertEquals("Parse True failed", Boolean.Parse(" True"), true);
		AssertEquals("Parse True failed", Boolean.Parse("True "), true);
		AssertEquals("Parse True failed", Boolean.Parse("tRuE"), true);
		AssertEquals("Parse False failed", Boolean.Parse("False"), false);
		AssertEquals("Parse False failed", Boolean.Parse(" False"), false);
		AssertEquals("Parse False failed", Boolean.Parse("False "), false);
		AssertEquals("Parse False failed", Boolean.Parse("fAlSe"), false);
		
		try {
			Boolean.Parse("not-t-or-f");
			Fail("Parse should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert("Parse should be a System.FormatException", typeof(FormatException) == e.GetType());
		}		

		try {
			Boolean.Parse(null);
			Fail("Parse should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert("Parse should be a System.ArgumentNullException", typeof(ArgumentNullException) == e.GetType());
		}		
	}
	
	public void TestToString() {
		Boolean t=true,f=false;
		AssertEquals("ToString True Failed", t.ToString(), Boolean.TrueString);
		AssertEquals("ToString False Failed", f.ToString(), Boolean.FalseString);
	}
}

}
