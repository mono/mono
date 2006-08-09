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
	public class ReadOnlyCollectionBaseTest : TestCase 	{
		// We need a concrete class to test the abstract base class
		public class ConcreteReadOnlyCollection : ReadOnlyCollectionBase 
		{
#if NET_2_0
			public override int Count { get { return -1; }}
#endif
		}

		// Make sure that the Count is 0 for a new object
		public void TestZeroCountOnNew() 
		{
			ConcreteReadOnlyCollection myCollection;
			myCollection = new ConcreteReadOnlyCollection();
#if NET_2_0
			Assert (-1 == myCollection.Count);
#else						
			Assert ( 0 == myCollection.Count);
#endif					
		}

		// Make sure we get an object from GetEnumerator()
		public void TestGetEnumerator() 
		{
			ConcreteReadOnlyCollection myCollection;
			myCollection = new ConcreteReadOnlyCollection();
			Assert(null != myCollection.GetEnumerator());
		}
	}
}
