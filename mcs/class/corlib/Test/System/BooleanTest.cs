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
	public BooleanTest () {}

	protected override void SetUp ()
	{
	}

	public void TestStrings ()
	{
		AssertEquals("Wrong False string", "False", Boolean.FalseString);
		AssertEquals("Wrong True string", "True", Boolean.TrueString);
	}
	
	public void TestCompareTo() {
		Boolean t=true,f=false;
		String s = "What Ever";
		AssertEquals("CompareTo Failed", true, f.CompareTo(t) < 0);
		AssertEquals("CompareTo Failed", 0, f.CompareTo(f));
		AssertEquals("CompareTo Failed", 0, t.CompareTo(t));
		AssertEquals("CompareTo Failed", true, t.CompareTo(f) > 0);
		AssertEquals("CompareTo Failed", true, t.CompareTo(null) > 0);
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
		AssertEquals("Equals Failed", true, t.Equals(t));
		AssertEquals("Equals Failed", true, f.Equals(f));
		AssertEquals("Equals Failed", false, f.Equals(t));
		AssertEquals("Equals Failed", false, t.Equals(null));
		AssertEquals("Equals Failed", false, t.Equals(s));
	}

	public void TestGetHashCode() {
		Boolean t=true, f=false;
		AssertEquals("GetHashCode True failed", 1, t.GetHashCode());
		AssertEquals("GetHashCode True failed", 0, f.GetHashCode());
	}

	public void TestGetType() {
		Boolean t=true, f=false;
		AssertEquals("GetType failed", true, Object.ReferenceEquals(t.GetType(), f.GetType()));
	}

	public void TestGetTypeCode() {
		Boolean b=true;
		AssertEquals("GetTypeCode failed", TypeCode.Boolean, b.GetTypeCode());
	}

	public void TestParse() {
		AssertEquals("Parse True failed", true, Boolean.Parse("True"));
		AssertEquals("Parse True failed", true, Boolean.Parse(" True"));
		AssertEquals("Parse True failed", true, Boolean.Parse("True "));
		AssertEquals("Parse True failed", true, Boolean.Parse("tRuE"));
		AssertEquals("Parse False failed", false, Boolean.Parse("False"));
		AssertEquals("Parse False failed", false, Boolean.Parse(" False"));
		AssertEquals("Parse False failed", false, Boolean.Parse("False "));
		AssertEquals("Parse False failed", false, Boolean.Parse("fAlSe"));
		
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
		AssertEquals("ToString True Failed", Boolean.TrueString, t.ToString());
		AssertEquals("ToString False Failed", Boolean.FalseString, f.ToString());
	}
}

}
