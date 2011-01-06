// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell Inc. http://novell.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Web.Caching;

using NUnit.Framework;

namespace MonoTests.System.Web.Caching
{
	[TestFixture]
	public partial class CacheItemPriorityQueueTest
	{
		sealed class TestCacheItem : CacheItem
		{
			public Guid Guid;

			public TestCacheItem (ref int index, object[] data)
			{
				Key = LoadField <string> (index++, data);
				AbsoluteExpiration = DateTime.Parse (LoadField <string> (index++, data));
				SlidingExpiration = TimeSpan.Parse (LoadField <string> (index++, data));
				Priority = LoadField <CacheItemPriority> (index++, data);
				LastChange = DateTime.Parse (LoadField <string> (index++, data));
				ExpiresAt = LoadField <long> (index++, data);
				Disabled = LoadField <bool> (index++, data);
				Guid = new Guid (LoadField <string> (index++, data));
			}

			public override string ToString ()
			{
				return String.Format ("CacheItem [{0}]\n[{1}][{2}][{3}]", this.Guid, Key, Disabled, ExpiresAt > 0 ? new DateTime (ExpiresAt).ToString () : "0");
			}

			T LoadField <T> (int index, object[] data)
			{
				if (data == null || data.Length <= index)
					throw new ArgumentOutOfRangeException ("index");
                
				object o = data [index];
				if (o == null)
					return default (T);
                
				if (o.GetType () != typeof (T))
					throw new InvalidOperationException (String.Format ("Field at index {0} is not a {1}", index, typeof (T)));
                
				return (T)o;
			}
		}
		
		void RunTest (string[] tests, object[] list)
		{
			var queue = new CacheItemPriorityQueue ();
			var cacheItems = new List <TestCacheItem> ();
			int index = 0;

			while (index < list.Length)
				cacheItems.Add (new TestCacheItem (ref index, list));

			for (int i = 0; i < tests.Length; i++)
				RunItem (new CacheItemPriorityQueueTestItem (tests [i]), queue, cacheItems, i);
		}

		void RunItem (CacheItemPriorityQueueTestItem item, CacheItemPriorityQueue queue, List <TestCacheItem> list, int testNum)
		{
			TestCacheItem ci;
			string messagePrefix = String.Format ("{0}-{1:00000}-{2:00000}-", item.Operation, item.OperationCount, testNum);
			
			switch (item.Operation) {
				case QueueOperation.Enqueue:
					queue.Enqueue (list [item.ListIndex]);
					Assert.AreEqual (item.QueueCount, queue.Count, messagePrefix + "1");
					Assert.AreEqual (item.Guid, ((TestCacheItem)queue.Peek ()).Guid.ToString (), messagePrefix + "2");
					break;
					
				case QueueOperation.Dequeue:
					ci = (TestCacheItem)queue.Dequeue ();
					if (item.IsNull)
						Assert.IsNull (ci, messagePrefix + "1");
					else {
						Assert.IsNotNull (ci, messagePrefix + "2");
						Assert.AreEqual (item.Guid, ci.Guid.ToString (), messagePrefix + "3");
						Assert.AreEqual (item.IsDisabled, ci.Disabled, messagePrefix + "4");
					}
					Assert.AreEqual (item.QueueCount, queue.Count, messagePrefix + "5");
					break;
					
				case QueueOperation.Disable:
					ci = list [item.ListIndex];
					if (item.IsNull)
						Assert.IsNull (ci, messagePrefix + "1");
					else {
						Assert.IsNotNull (ci, messagePrefix + "2");
						Assert.AreEqual (item.Guid, ci.Guid.ToString (), messagePrefix + "3");
						Assert.AreEqual (item.IsDisabled, ci.Disabled, messagePrefix + "4");
						ci.Disabled = item.Disable;
					}
					break;

				case QueueOperation.Peek:
					ci = (TestCacheItem)queue.Peek ();
					if (item.IsNull)
						Assert.IsNull (ci, messagePrefix + "1");
					else {
						Assert.IsNotNull (ci, messagePrefix + "2");
						Assert.AreEqual (item.Guid, ci.Guid.ToString (), messagePrefix + "3");
						Assert.AreEqual (item.IsDisabled, ci.Disabled, messagePrefix + "4");
					}
					Assert.AreEqual (item.QueueCount, queue.Count, messagePrefix + "5");
					break;

				case QueueOperation.QueueSize:
					Assert.AreEqual (item.QueueCount, queue.Count, "Queue size after sequence");
					break;
					
				default:
					Assert.Fail ("Unknown QueueOperation: {0}", item.Operation);
					break;
			}
		}
	}
}
