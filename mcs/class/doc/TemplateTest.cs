// this is a template for making NUnit version 2 tests.  Text enclosed in curly
// brackets (and the brackets themselves) should be replaced by appropriate
// code.

// {File Name}.cs - NUnit Test Cases for {explain here}
//
// {Author Name} ({Author email Address})
//
// (C) {Copyright holder}
// 

// these are the standard namespaces you will need.  You may need to add more
// depending on your tests.
using NUnit.Framework;
using System;

// all test namespaces start with "MonoTests."  Append the Namespace that
// contains the class you are testing, e.g. MonoTests.System.Collections
namespace MonoTests.{Namespace}
{

// the class name should end with "Test" and start with the name of the class
// you are testing, e.g. CollectionBaseTest
[TestFixture]
public class {Class to be tested}Test : Assertion {
	
	// this method is run before each [Test] method is called. You can put
	// variable initialization, etc. here that is common to each test.
	// Just leave the method empty if you don't need to use it.
	// The name of the method does not matter; the attribute does.
	[SetUp]
	public void GetReady() {}

	// this method is run after each Test* method is called. You can put
	// clean-up code, etc. here.  Whatever needs to be done after each test.
	// Just leave the method empty if you don't need to use it.
	// The name of the method does not matter; the attribute does.
	[TearDown]
	public void Clean() {}

	// this is just one of probably many test methods in your test class.
	// each test method must be adorned with [Test].  All methods in your class
	// adorned with [Test] will be automagically called by the NUnit
	// framework.
	[Test]
	public void {Something} {
		// inside here you will exercise your class and then call Assert()
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
