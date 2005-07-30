// this is a template for making NUnit version 2 tests.  Text enclosed in curly
// brackets (and the brackets themselves) should be replaced by appropriate
// code.

// DependencyObject.cs - NUnit Test Cases for attached properties
// 
// Iain McCoy (iain@mccoy.id.au)
//
// (C) iain@mccoy.id.au
// 

using NUnit.Framework;
using System;
using System.Windows;

// all test namespaces start with "MonoTests."  Append the Namespace that
// contains the class you are testing, e.g. MonoTests.System.Collections
namespace MonoTests.System.Windows
{

class X {
	public static readonly DependencyProperty AProperty = DependencyProperty.RegisterAttached("A", typeof(int), typeof(X));
	public static void SetA(DependencyObject obj, int value)
	{
		obj.SetValue(AProperty, value);
	}
	public static int GetA(DependencyObject obj)
	{
		return (int)obj.GetValue(AProperty);
	}

	public static readonly DependencyProperty BProperty = DependencyProperty.RegisterAttached("B", typeof(string), typeof(X));
	public static void SetB(DependencyObject obj, string value)
	{
		obj.SetValue(BProperty, value);
	}
	public static string GetB(DependencyObject obj)
	{
		return (string)obj.GetValue(BProperty);
	}

}

class Y : DependencyObject {
}
	
[TestFixture]
public class DependencyObjectTest {
	
	[SetUp]
	public void GetReady() {}

	[TearDown]
	public void Clean() {}

	[Test]
	public void TestAttachedProperty()
	{
		Y y1 = new Y();
		X.SetA(y1, 2);
		Assert.AreEqual(2, X.GetA(y1));
	}
	
	[Test]
	public void Test2AttachedProperties()
	{
		Y y1 = new Y();
		Y y2 = new Y();
		X.SetA(y1, 2);
		X.SetA(y2, 3);
		Assert.AreEqual(2, X.GetA(y1));
		Assert.AreEqual(3, X.GetA(y2));
	}
	
	[Test]
	public void TestEnumerationOfAttachedProperties()
	{
		int count = 0;
		Y y = new Y();
		X.SetA(y, 2);
		X.SetB(y, "Hi");

		LocalValueEnumerator e = y.GetLocalValueEnumerator();
		while (e.MoveNext()) {
			count++;
			if (e.Current.Property == X.AProperty)
				Assert.AreEqual(e.Current.Value, 2);
			else if (e.Current.Property == X.BProperty)
				Assert.AreEqual(e.Current.Value, "Hi");
			else
				Assert.Fail("Wrong sort of property" + e.Current.Property);
		}

		
		Assert.AreEqual(2, count);
	}

	// An nice way to test for exceptions the class under test should 
	// throw is:
	/*
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void OnValid() {
		ConcreteCollection myCollection;
		myCollection = new ConcreteCollection();
		....
		AssertEquals ("#UniqueID", expected, actual);
		....
		Fail ("Message");
	}
	*/

}
}
