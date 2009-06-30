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

[TestFixture]
public class ObjectTest
{
	public ObjectTest() {}

	[Test]
	public void TestCtor() {
		Object o = new Object();
		Assert.IsNotNull(o, "Can I at least get an _Object_, please?");
	}

	[Test]
	public void TestEquals1() {
		{
			Object x = new Object();
			Object y = new Object();
			Assert.IsTrue(x.Equals(x), "Object should equal itself");
			       
			Assert.IsTrue(!x.Equals(null), "object should not equal null");
			       
			Assert.IsTrue(!x.Equals(y), "Different objects should not equal 1");
			       
			Assert.IsTrue(!y.Equals(x), "Different objects should not equal 2");
			       
		}
		{
			double x = Double.NaN;
			double y = Double.NaN;
			Assert.IsTrue(((Object)x).Equals(y), "NaNs should always equal each other");
			       
		}
	}

	[Test]
	public void TestEquals2() {
		{
			Object x = new Object();
			Object y = new Object();
			Assert.IsTrue(Object.Equals(x,x), "Object should equal itself");
			       
			Assert.IsTrue(!Object.Equals(x,null), "object should not equal null");
			       
			Assert.IsTrue(!Object.Equals(null,x), "null should not equal object");
			       
			Assert.IsTrue(!Object.Equals(x,y), "Different objects should not equal 1");
			       
			Assert.IsTrue(!Object.Equals(y,x), "Different objects should not equal 2");
			       
			Assert.IsTrue(Object.Equals(null,null), "null should equal null");
			       
		}
		{
			double x = Double.NaN;
			double y = Double.NaN;
			Assert.IsTrue(Object.Equals(x,y), "NaNs should always equal each other");
			       
		}
	}

	[Test]
	public void TestGetHashCode() {
		Object x = new Object();
		Assert.AreEqual(x.GetHashCode(), x.GetHashCode(), "Object's hash code should not change");
	}

	[Test]
	public void TestGetType() {
		Object x = new Object();
		Assert.IsNotNull(x.GetType(), "Should get a type for Object");
		Assert.AreEqual(x.GetType().ToString(), "System.Object", "Bad name for Object type");
			     
	}

	[Test]
	public void TestReferenceEquals() {
		Object x = new Object();
		Object y = new Object();
		Assert.IsTrue(Object.ReferenceEquals(x,x), "Object should equal itself");
		       
		Assert.IsTrue(!Object.ReferenceEquals(x,null), "object should not equal null");
		       
		Assert.IsTrue(!Object.ReferenceEquals(null,x), "null should not equal object");
		       
		Assert.IsTrue(!Object.ReferenceEquals(x,y), "Different objects should not equal 1");
		       
		Assert.IsTrue(!Object.ReferenceEquals(y,x), "Different objects should not equal 2");
		       
		Assert.IsTrue(Object.ReferenceEquals(null,null), "null should not equal null");
		       
	}

	[Test]
	public void TestToString() {
		Object x = new Object();
		Object y = new Object();
		Assert.AreEqual(x.ToString(), y.ToString(), "All Objects should have same string rep");
	}
#if NET_2_0
	class Foo<T> {}

	[Test]
	public void TestToStringOnGenericInstances ()
	{
		Foo<Object> foo = new Foo<Object> ();
		Assert.AreEqual ("MonoTests.System.ObjectTest+Foo`1[System.Object]", foo.ToString (), "Bad ToString of generic instance");
			
	}
#endif
}
}
