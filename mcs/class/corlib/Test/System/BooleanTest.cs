//
// BooleanTest.cs - NUnit Test Cases for the System.Boolean class
//
// Authors
//	Bob Doan <bdoan@sicompos.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System {

[TestFixture]
public class BooleanTest : Assertion {

	[Test]
	public void Strings ()
	{
		AssertEquals("Wrong False string", "False", Boolean.FalseString);
		AssertEquals("Wrong True string", "True", Boolean.TrueString);
	}
	
	[Test]
	public void CompareTo ()
	{
		Boolean t=true,f=false;
		Assert ("f.CompareTo(t) < 0", f.CompareTo(t) < 0);
		Assert ("f.CompareTo(f)", f.CompareTo(f) == 0);
		Assert ("t.CompareTo(t) == 0", t.CompareTo(t) == 0);
		Assert ("t.CompareTo(f) > 0", t.CompareTo(f) > 0);
		Assert ("t.CompareTo(null) > 0", t.CompareTo(null) > 0);

		byte[] array = new byte [1] { 0x02 };
		bool t2 = BitConverter.ToBoolean (array, 0);
		Assert ("f.CompareTo(t2) < 0", f.CompareTo(t2) < 0);
		Assert ("t2.CompareTo(t2) == 0", t2.CompareTo(t2) == 0);
		Assert ("t2.CompareTo(f) > 0", t2.CompareTo(f) > 0);
		Assert ("t2.CompareTo(null) > 0", t2.CompareTo(null) > 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CompareToInvalidString ()
	{
		true.CompareTo ("What Ever");
	}

	[Test]
	public void TestEquals ()
	{
		Boolean t=true, f=false;
		string s = "What Ever";
		Assert ("t.Equals(t)", t.Equals(t));
		Assert ("f.Equals(f)", f.Equals(f));
		Assert ("!t.Equals(f)", !t.Equals(f));
		Assert ("!f.Equals(t)", !f.Equals(t));
		Assert ("!t.Equals(null)", !t.Equals(null));
		Assert ("!f.Equals(null)", !f.Equals(null));
		Assert ("!t.Equals(s)", !t.Equals(s));
		Assert ("!f.Equals(s)", !f.Equals(s));

		byte[] array = new byte [1] { 0x02 };
		bool t2 = BitConverter.ToBoolean (array, 0);
		Assert ("t2.Equals(t2)", t2.Equals(t2));
		Assert ("t.Equals(t2)", t.Equals(t2));
		Assert ("t2.Equals(t)", t2.Equals(t));
		Assert ("!f.Equals(t2)", !f.Equals(t2));
	}

	[Test]
	public void TestEqualOperator ()
	{
		Boolean t=true, f=false;
		Assert ("t==t", t==t);
		Assert ("f==f", f==f);
		Assert ("t!=f", t!=f);
		Assert ("f!=t", f!=t);

		byte[] array = new byte [1] { 0x02 };
		bool t2 = BitConverter.ToBoolean (array, 0);
		Assert ("t2==t2", t2==t2);
		Assert ("t==t2", t==t2);
		Assert ("t2==t", t2==t);
		Assert ("f!=t2", f!=t2);
	}

	[Test]
	public void TestGetHashCode ()
	{
		Boolean t=true, f=false;
		AssertEquals("GetHashCode True failed", 1, t.GetHashCode());
		AssertEquals("GetHashCode True failed", 0, f.GetHashCode());
	}

	[Test]
	public void TestGetType ()
	{
		Boolean t=true, f=false;
		AssertEquals("GetType failed", true, Object.ReferenceEquals(t.GetType(), f.GetType()));
	}

	[Test]
	public void GetTypeCode ()
	{
		Boolean b=true;
		AssertEquals("GetTypeCode failed", TypeCode.Boolean, b.GetTypeCode());
	}

	[Test]
	public void Parse () 
	{
		AssertEquals("Parse True failed", true, Boolean.Parse("True"));
		AssertEquals("Parse True failed", true, Boolean.Parse(" True"));
		AssertEquals("Parse True failed", true, Boolean.Parse("True "));
		AssertEquals("Parse True failed", true, Boolean.Parse("tRuE"));
		AssertEquals("Parse False failed", false, Boolean.Parse("False"));
		AssertEquals("Parse False failed", false, Boolean.Parse(" False"));
		AssertEquals("Parse False failed", false, Boolean.Parse("False "));
		AssertEquals("Parse False failed", false, Boolean.Parse("fAlSe"));
	}

	[Test]
	[ExpectedException (typeof (FormatException))]
	public void ParseInvalid ()
	{
		Boolean.Parse ("not-t-or-f");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ParseNull ()
	{
		Boolean.Parse (null);
	}

	[Test]
	public void TestToString ()
	{
		Boolean t=true,f=false;
		AssertEquals("ToString True Failed", Boolean.TrueString, t.ToString());
		AssertEquals("ToString False Failed", Boolean.FalseString, f.ToString());
	}
}

}
