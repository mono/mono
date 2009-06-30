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
public class BooleanTest  {

	[Test]
	public void Strings ()
	{
		Assert.AreEqual("False", Boolean.FalseString, "Wrong False string");
		Assert.AreEqual("True", Boolean.TrueString, "Wrong True string");
	}
	
	[Test]
	public void CompareTo ()
	{
		Boolean t=true,f=false;
		Assert.IsTrue (f.CompareTo(t) < 0, "f.CompareTo(t) < 0");
		Assert.IsTrue (f.CompareTo(f) == 0, "f.CompareTo(f)");
		Assert.IsTrue (t.CompareTo(t) == 0, "t.CompareTo(t) == 0");
		Assert.IsTrue (t.CompareTo(f) > 0, "t.CompareTo(f) > 0");
		Assert.IsTrue (t.CompareTo(null) > 0, "t.CompareTo(null) > 0");

		byte[] array = new byte [1] { 0x02 };
		bool t2 = BitConverter.ToBoolean (array, 0);
		Assert.IsTrue (f.CompareTo(t2) < 0, "f.CompareTo(t2) < 0");
		Assert.IsTrue (t2.CompareTo(t2) == 0, "t2.CompareTo(t2) == 0");
		Assert.IsTrue (t2.CompareTo(f) > 0, "t2.CompareTo(f) > 0");
		Assert.IsTrue (t2.CompareTo(null) > 0, "t2.CompareTo(null) > 0");
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
		Assert.IsTrue (t.Equals(t), "t.Equals(t)");
		Assert.IsTrue (f.Equals(f), "f.Equals(f)");
		Assert.IsTrue (!t.Equals(f), "!t.Equals(f)");
		Assert.IsTrue (!f.Equals(t), "!f.Equals(t)");
		Assert.IsTrue (!t.Equals(null), "!t.Equals(null)");
		Assert.IsTrue (!f.Equals(null), "!f.Equals(null)");
		Assert.IsTrue (!t.Equals(s), "!t.Equals(s)");
		Assert.IsTrue (!f.Equals(s), "!f.Equals(s)");

		byte[] array = new byte [1] { 0x02 };
		bool t2 = BitConverter.ToBoolean (array, 0);
		Assert.IsTrue (t2.Equals(t2), "t2.Equals(t2)");
		Assert.IsTrue (t.Equals(t2), "t.Equals(t2)");
		Assert.IsTrue (t2.Equals(t), "t2.Equals(t)");
		Assert.IsTrue (!f.Equals(t2), "!f.Equals(t2)");
	}

#pragma warning disable 1718
	[Test]
	public void TestEqualOperator ()
	{
		Boolean t=true, f=false;
		Assert.IsTrue (t==t, "t==t");
		Assert.IsTrue (f==f, "f==f");
		Assert.IsTrue (t!=f, "t!=f");
		Assert.IsTrue (f!=t, "f!=t");

		byte[] array = new byte [1] { 0x02 };
		bool t2 = BitConverter.ToBoolean (array, 0);
		Assert.IsTrue (t2==t2, "t2==t2");
		Assert.IsTrue (t==t2, "t==t2");
		Assert.IsTrue (t2==t, "t2==t");
		Assert.IsTrue (f!=t2, "f!=t2");
	}
#pragma warning restore 1718

	[Test]
	public void TestGetHashCode ()
	{
		Boolean t=true, f=false;
		Assert.AreEqual(1, t.GetHashCode(), "GetHashCode True failed");
		Assert.AreEqual(0, f.GetHashCode(), "GetHashCode True failed");
	}

	[Test]
	public void TestGetType ()
	{
		Boolean t=true, f=false;
		Assert.AreEqual(true, Object.ReferenceEquals(t.GetType(), f.GetType()), "GetType failed");
	}

	[Test]
	public void GetTypeCode ()
	{
		Boolean b=true;
		Assert.AreEqual(TypeCode.Boolean, b.GetTypeCode(), "GetTypeCode failed");
	}

	[Test]
	public void Parse () 
	{
		Assert.AreEqual(true, Boolean.Parse("True"), "Parse True failed");
		Assert.AreEqual(true, Boolean.Parse(" True"), "Parse True failed");
		Assert.AreEqual(true, Boolean.Parse("True "), "Parse True failed");
		Assert.AreEqual(true, Boolean.Parse("tRuE"), "Parse True failed");
		Assert.AreEqual(false, Boolean.Parse("False"), "Parse False failed");
		Assert.AreEqual(false, Boolean.Parse(" False"), "Parse False failed");
		Assert.AreEqual(false, Boolean.Parse("False "), "Parse False failed");
		Assert.AreEqual(false, Boolean.Parse("fAlSe"), "Parse False failed");
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
		Assert.AreEqual(Boolean.TrueString, t.ToString(), "ToString True Failed");
		Assert.AreEqual(Boolean.FalseString, f.ToString(), "ToString False Failed");
	}
}

}
