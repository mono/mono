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
		enum QueueOperation
		{
			Enqueue,
			Dequeue,
			Disable,
			Peek,
			QueueSize
		}
		
		sealed class TestItem
		{
			public int ListIndex;
			public int QueueCount;
			public QueueOperation Operation;
			public bool IsDisabled;
			public bool IsNull;
			public bool Disable;
			public int OperationCount;
			public string Guid;
		}

		sealed class TestCacheItem : CacheItem
		{
			public Guid Guid;

			public TestCacheItem ()
			{
				Guid = Guid.NewGuid ();
			}

			public override string ToString ()
			{
				return String.Format ("CacheItem [{0}]\n[{1}][{2}][{3}]", this.Guid, Key, Disabled, ExpiresAt > 0 ? new DateTime (ExpiresAt).ToString () : "0");
			}
		}
		
		void RunTest (List <TestItem> tests, List <TestCacheItem> list)
		{
			var queue = new CacheItemPriorityQueue ();

			foreach (TestItem item in tests)
				RunItem (item, queue, list);
		}

		void RunItem (TestItem item, CacheItemPriorityQueue queue, List <TestCacheItem> list)
		{
			TestCacheItem ci;
			string messagePrefix = String.Format ("{0}-{1:00000}-", item.Operation, item.OperationCount);
			
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
