// ObjectTest.cs - NUnit Test Cases for the System.Object struct
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{

public class ObjectTest : TestCase
{
	public ObjectTest() {}

	protected override void SetUp() 
	{
	}

	protected override void TearDown() 
	{
	}

	public void TestCtor() {
		Object o = new Object();
		AssertNotNull("Can I at least get an _Object_, please?", o);
	}

	public void TestEquals1() {
		{
			Object x = new Object();
			Object y = new Object();
			Assert("Object should equal itself",
			       x.Equals(x));
			Assert("object should not equal null",
			       !x.Equals(null));
			Assert("Different objects should not equal 1",
			       !x.Equals(y));
			Assert("Different objects should not equal 2",
			       !y.Equals(x));
		}
		{
			double x = Double.NaN;
			double y = Double.NaN;
			Assert("NaNs should always equal each other",
			       ((Object)x).Equals(y));
		}
	}
	public void TestEquals2() {
		{
			Object x = new Object();
			Object y = new Object();
			Assert("Object should equal itself",
			       Object.Equals(x,x));
			Assert("object should not equal null",
			       !Object.Equals(x,null));
			Assert("null should not equal object",
			       !Object.Equals(null,x));
			Assert("Different objects should not equal 1",
			       !Object.Equals(x,y));
			Assert("Different objects should not equal 2",
			       !Object.Equals(y,x));
			Assert("null should not equal null",
			       Object.Equals(null,null));
		}
		{
			double x = Double.NaN;
			double y = Double.NaN;
			Assert("NaNs should always equal each other",
			       Object.Equals(x,y));
		}
	}

	public void TestGetHashCode() {
		Object x = new Object();
		AssertEquals("Object's hash code should not change",
			     x.GetHashCode(), x.GetHashCode());
	}

	public void TestGetType() {
		Object x = new Object();
		AssertNotNull("Should get a type for Object", x.GetType());
		AssertEquals("Bad name for Object type", "System.Object",
			     x.GetType().ToString());
	}

	public void TestReferenceEquals() {
		Object x = new Object();
		Object y = new Object();
		Assert("Object should equal itself",
		       Object.ReferenceEquals(x,x));
		Assert("object should not equal null",
		       !Object.ReferenceEquals(x,null));
		Assert("null should not equal object",
		       !Object.ReferenceEquals(null,x));
		Assert("Different objects should not equal 1",
		       !Object.ReferenceEquals(x,y));
		Assert("Different objects should not equal 2",
		       !Object.ReferenceEquals(y,x));
		Assert("null should not equal null",
		       Object.ReferenceEquals(null,null));
	}

	public void TestToString() {
		Object x = new Object();
		Object y = new Object();
		AssertEquals("All Objects should have same string rep",
			     x.ToString(), y.ToString());
	}
}
}
