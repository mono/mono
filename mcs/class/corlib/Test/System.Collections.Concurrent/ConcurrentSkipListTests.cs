#if NET_4_0
// ConcurrentSkipListTests.cs
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

using NUnit;
using NUnit.Framework;

namespace MonoTests.System.Collections.Concurrent
{
	[TestFixtureAttribute]
	public class ConcurrentSkipListTests
	{
		ConcurrentSkipList<int> skiplist;

		[SetUpAttribute]
		public void Setup()
		{
			skiplist = new ConcurrentSkipList<int>();
		}

		void AddStuff()
		{
			skiplist.TryAdd(1);
			skiplist.TryAdd(2);
			skiplist.TryAdd(3);
			skiplist.TryAdd(4);
		}

		[TestAttribute]
		public void AddTestCase()
		{
			Assert.IsTrue(skiplist.TryAdd(1), "#1");
			Assert.AreEqual(1, skiplist.Count, "#2");
		}

		[TestAttribute]
		public void RemoveTestCase()
		{
			Assert.IsFalse(skiplist.Remove(2), "#1");
			Assert.IsFalse(skiplist.Remove(3), "#2");
			
			AddStuff();
			int count = skiplist.Count;
			Assert.IsTrue(skiplist.Remove(1), "#3");
			Assert.IsFalse(skiplist.Remove(1), "#4");
			Assert.IsTrue(skiplist.Remove(4), "#5");
			Assert.AreEqual(count - 2, skiplist.Count, "#6");
		}
		
		[Test]
		public void ContainsTestCase()
		{
			AddStuff();
			Assert.IsTrue(skiplist.Contains(1), "#1");
			Assert.IsTrue(skiplist.Contains(2), "#2");
			Assert.IsTrue(skiplist.Contains(3), "#3");
			Assert.IsTrue(skiplist.Contains(4), "#4");
		}

		[TestAttribute]
		public void EnumerateTestCase()
		{
			AddStuff();
			
			string s = string.Empty;
			foreach (int i in skiplist)
				s += i.ToString();

			Assert.AreEqual("1234", s);
		}

		[TestAttribute]
		public void ToArrayTestCase()
		{
			int[] expected = new int[] { 1, 2, 3, 4 };
			AddStuff();
			int[] array = skiplist.ToArray();
			CollectionAssert.AreEqual(expected, array, "#1");

			Array.Clear(array, 0, array.Length);
			skiplist.CopyTo(array, 0);
			CollectionAssert.AreEqual(expected, array, "#2");
		}
		 
	}
}
#endif
