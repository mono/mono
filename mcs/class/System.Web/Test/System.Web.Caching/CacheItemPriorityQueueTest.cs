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
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Caching;

using NUnit.Framework;

namespace MonoTests.System.Web.Caching
{
	[TestFixture]
	public partial class CacheItemPriorityQueueTest
	{
		const string DATA_DIR = "CacheItemPriorityQueueTestData";
		static readonly string dataDir;
		
		sealed class TestCacheItem : CacheItem
		{
			public Guid Guid;

			public TestCacheItem (string dataLine)
			{
				string[] data = dataLine.Split (',');
				int index = 0;
				
				Key = LoadField <string> (index++, data);
				AbsoluteExpiration = new DateTime (LoadField <long> (index++, data));
				SlidingExpiration = new TimeSpan (LoadField <long> (index++, data));
				Priority = LoadField <CacheItemPriority> (index++, data);
				LastChange = new DateTime (LoadField <long> (index++, data));
				ExpiresAt = LoadField <long> (index++, data);
				Disabled = LoadField <bool> (index++, data);
				Guid = new Guid (LoadField <string> (index++, data));
				if (data.Length > index)
					PriorityQueueIndex = LoadField <int> (index++, data);
				else
					PriorityQueueIndex = -1;
			}

			public override string ToString ()
			{
				return String.Format ("CacheItem [{0}]\n[{1}][{2}][{3}]", this.Guid, Key, Disabled, ExpiresAt > 0 ? new DateTime (ExpiresAt).ToString () : "0");
			}

			T LoadField <T> (int index, string[] data)
			{
				if (data == null || data.Length <= index)
					throw new ArgumentOutOfRangeException ("index");
                
				string s = data [index];
				if (String.IsNullOrEmpty (s))
					return default (T);
                
				TypeConverter cvt = TypeDescriptor.GetConverter (typeof (T));
				if (cvt == null)
					throw new InvalidOperationException (String.Format ("Cannot find converter for type {0}, field {1}", typeof (T), index));
				if (!cvt.CanConvertFrom (typeof (string)))
					throw new InvalidOperationException (String.Format ("Converter for type {0} cannot convert from string, field {1}", typeof (T), index));
				
				return (T)cvt.ConvertFromString (s);
			}
		}

		static CacheItemPriorityQueueTest ()
		{
			dataDir =
				Path.Combine (
					Path.Combine (
						Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), "Test"),
						"System.Web.Caching"),
					DATA_DIR);
		}
		
		void RunTest (string testsFileName, string listFileName)
		{
			var queue = new CacheItemPriorityQueue ();
			var cacheItems = new List <TestCacheItem> ();
			string listPath = Path.Combine (dataDir, listFileName);
			string testsPath = Path.Combine (dataDir, testsFileName);
			string line;
			
			using (var sr = new StreamReader (listPath, Encoding.UTF8)) {
				while ((line = sr.ReadLine ()) != null) {
					if (line [0] == '#')
						continue;
					cacheItems.Add (new TestCacheItem (line));
				}
			}

			using (var sr = new StreamReader (testsPath, Encoding.UTF8)) {
				int i = 0;
				while ((line = sr.ReadLine ()) != null) {
					if (line [0] == '#')
						continue;
					RunItem (new CacheItemPriorityQueueTestItem (line), queue, cacheItems, i++);
				}
			}
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

				case QueueOperation.Update:
					ci = list [item.ListIndex];
					queue.Update (ci);
					if (item.IsNull)
						Assert.IsNull (ci, messagePrefix + "1");
					else {
						Assert.IsNotNull (ci, messagePrefix + "2");
						Assert.AreEqual (item.Guid, ci.Guid.ToString (), messagePrefix + "3");
						Assert.AreEqual (item.ExpiresAt, ci.ExpiresAt, messagePrefix + "4");
						Assert.AreEqual (item.PriorityQueueIndex, ci.PriorityQueueIndex, messagePrefix + "5");
					}
					break;
					
				default:
					Assert.Fail ("Unknown QueueOperation: {0}", item.Operation);
					break;
			}
		}
	}
}
