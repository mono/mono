//
// System.Collections.ReadOnlyCollectionBase
// Test suite for System.Collections.ReadOnlyCollectionBase
//
// Author:
//    Nick D. Drochak II
//
// (C) 2001 Nick D. Drochak II
//


using System;
using System.Collections;
using NUnit.Framework;

	public class ReadOnlyCollectionBaseTest : TestCase 	{
		public ReadOnlyCollectionBaseTest () : base ("System.Collection.ReadOnlyCollectionBase testsuite") {}
		public ReadOnlyCollectionBaseTest (String name) : base (name) {}

		// We need a concrete class to test the abstract base class
		public class ConcreteReadOnlyCollection : ReadOnlyCollectionBase 
		{
		}

		public static ITest Suite 
		{
			get {
				return new TestSuite (typeof(ReadOnlyCollectionBaseTest));
			}
		}

		// Make sure that the Count is 0 for a new object
		public void TestZeroCountOnNew() 
		{
			ConcreteReadOnlyCollection myCollection;
			myCollection = new ConcreteReadOnlyCollection();
			Assert(0 == myCollection.Count);
		}

		// Make sure we get an object from GetEnumerator()
		public void TestGetEnumerator() 
		{
			ConcreteReadOnlyCollection myCollection;
			myCollection = new ConcreteReadOnlyCollection();
			Assert(null != myCollection.GetEnumerator());
		}
	}
