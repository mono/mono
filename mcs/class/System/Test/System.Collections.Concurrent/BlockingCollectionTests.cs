#if NET_4_0
// BlockingCollectionTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.System.Collections.Concurrent
{
	[TestFixture()]
	public class BlockingCollectionTests
	{
		BlockingCollection<int> defaultCollection;
		BlockingCollection<int> boundedCollection;
		
		[SetUpAttribute]
		public void Setup()
		{
			defaultCollection = new BlockingCollection<int>();
			boundedCollection = new BlockingCollection<int>(10);
		}
		
		[TestAttribute]
		public void DefaultAddTestCase()
		{
			defaultCollection.Add(1);
			defaultCollection.Add(2);
			Assert.AreEqual(2, defaultCollection.Count, "#1");

		}
		
		[TestAttribute]
		public void BoundedAddTestCase()
		{
			boundedCollection.Add(1);
			boundedCollection.Add(2);
			Assert.AreEqual(2, boundedCollection.Count, "#1");
		}
		
		[TestAttribute]
		public void BoundedIsFullTestCase()
		{
			boundedCollection.Add(1);
			boundedCollection.Add(2);
			boundedCollection.Add(3);
			boundedCollection.Add(4);
			boundedCollection.Add(5);
			boundedCollection.Add(6);
			boundedCollection.Add(7);
			boundedCollection.Add(8);
			boundedCollection.Add(9);
			boundedCollection.Add(10);
			Assert.AreEqual(boundedCollection.BoundedCapacity, boundedCollection.Count, "#1");
		}
		
		[TestAttribute]
		public void TakeTestCase()
		{
			defaultCollection.Add(1);
			defaultCollection.Add(2);
			boundedCollection.Add(1);
			boundedCollection.Add(2);
			
			int value = defaultCollection.Take();
			Assert.AreEqual(1, value, "#1");
			value = boundedCollection.Take();
			Assert.AreEqual(1, value, "#2");
		}
		
		[TestAttribute, ExpectedExceptionAttribute(typeof(InvalidOperationException))]
		public void DefaultAddCompletedTestCase()
		{
			defaultCollection.Add(1);
			defaultCollection.Add(2);
			defaultCollection.CompleteAdding();
			Assert.IsTrue(defaultCollection.IsAddingCompleted, "#1");
			
			defaultCollection.Add(3);
		}
		
		[TestAttribute, ExpectedExceptionAttribute(typeof(InvalidOperationException))]
		public void BoundedAddCompletedTestCase()
		{
			boundedCollection.Add(1);
			boundedCollection.Add(2);
			boundedCollection.Add(3);
			boundedCollection.Add(4);
			boundedCollection.Add(5);
			boundedCollection.Add(6);
			boundedCollection.Add(7);
			boundedCollection.Add(8);
			boundedCollection.Add(9);
			boundedCollection.Add(10);
			boundedCollection.CompleteAdding();
			Assert.IsTrue(boundedCollection.IsAddingCompleted, "#1");
			
			boundedCollection.Add(3);
		}
		
		[TestAttribute]
		public void IsCompletedTestCase()
		{
			defaultCollection.Add(1);
			defaultCollection.Add(2);
			
			defaultCollection.CompleteAdding();
			Assert.IsFalse(defaultCollection.IsCompleted, "#3");
			
			defaultCollection.Take();
			defaultCollection.Take();
			
			Assert.IsTrue(defaultCollection.IsAddingCompleted, "#1");
			Assert.AreEqual(0, defaultCollection.Count, "#2");
			Assert.IsTrue(defaultCollection.IsCompleted, "#4");
		}
		
		[TestAttribute]
		public void IsCompletedEmptyTestCase ()
		{
			defaultCollection.CompleteAdding ();
			Assert.IsTrue (defaultCollection.IsCompleted);
		}

		[TestAttribute]
		public void ConsumingEnumerableTestCase()
		{
			defaultCollection.Add(1);
			defaultCollection.Add(2);
			defaultCollection.Add(3);
			defaultCollection.Add(4);
			defaultCollection.Add(5);
			defaultCollection.Add(6);
			defaultCollection.CompleteAdding ();
			
			IEnumerable<int> enumerable = defaultCollection.GetConsumingEnumerable();
			Assert.IsNotNull(enumerable, "#1");
			int i = 1;
			foreach (int j in enumerable) {
				int temp = i++;
				Assert.AreEqual(temp, j, "#" + temp);
			}
			Assert.AreEqual(0, defaultCollection.Count, "#" + i);
		}
	}
}
#endif
