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

namespace MonoTests.System.Collections {
	public class ReadOnlyCollectionBaseTest {
		// We need a concrete class to test the abstract base class
		public class ConcreteReadOnlyCollection : ReadOnlyCollectionBase 
		{
#if NET_2_0
			public override int Count { get { return -1; }}
#endif
		}

		// Make sure that the Count is 0 for a new object
		[Test]
		public void TestZeroCountOnNew() 
		{
			ConcreteReadOnlyCollection myCollection;
			myCollection = new ConcreteReadOnlyCollection();
#if NET_2_0
			Assert.IsTrue (-1 == myCollection.Count);
#else						
			Assert.IsTrue ( 0 == myCollection.Count);
#endif					
		}

		// Make sure we get an object from GetEnumerator()
		[Test]
		public void TestGetEnumerator() 
		{
			ConcreteReadOnlyCollection myCollection;
			myCollection = new ConcreteReadOnlyCollection();
			Assert.IsTrue (null != myCollection.GetEnumerator());
		}
	}
}
