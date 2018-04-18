//
// ObjectCacheTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Runtime.Caching;

using NUnit.Framework;
using MonoTests.Common;

namespace MonoTests.System.Runtime.Caching
{
	[TestFixture]
	public class ObjectCacheTest
	{
		[Test]
		public void Host ()
		{
			Assert.IsTrue (ObjectCache.Host == null, "#A1");

			AppDomainTools.RunInSeparateDomain (Host_SetToNull, "Host_SetToNull");
			AppDomainTools.RunInSeparateDomain (Host_SetToProvider, "Host_SetToProvider");
		}

		static void Host_SetToNull ()
		{
			Assert.Throws<ArgumentNullException> (() => {
				ObjectCache.Host = null;
			}, "#A2");
		}

		static void Host_SetToProvider ()
		{
			var tns1 = new TestNotificationSystem ();
			var tns2 = new TestNotificationSystem ();
			ObjectCache.Host = tns1;
			Assert.IsNotNull (ObjectCache.Host, "#A3-1");
			Assert.AreEqual (tns1, ObjectCache.Host, "#A3-2");

			Assert.Throws<InvalidOperationException> (() => {
				ObjectCache.Host = tns2;
			}, "#A4");
		}

		[Test]
		public void Add_CacheItem_CacheItemPolicy ()
		{
			var poker = new PokerObjectCache ();
			bool ret;

			ret = poker.Add (null, null);
			Assert.IsTrue (ret, "#A1-1");
			Assert.AreEqual ("AddOrGetExisting (CacheItem value, CacheItemPolicy policy)", poker.MethodCalled, "#A1-2");

			var item = new CacheItem ("key", 1234);
			ret = poker.Add (item, null);
			Assert.IsTrue (ret, "#A2-1");
			Assert.AreEqual ("AddOrGetExisting (CacheItem value, CacheItemPolicy policy)", poker.MethodCalled, "#A2-2");

			ret = poker.Add (item, null);
			Assert.IsFalse (ret, "#A3-1");
			Assert.AreEqual ("AddOrGetExisting (CacheItem value, CacheItemPolicy policy)", poker.MethodCalled, "#A3-2");
		}

		[Test]
		public void Add_String_Object_CacheItemPolicy_String ()
		{
			var poker = new PokerObjectCache ();
			bool ret;

			ret = poker.Add (null, null, null, null);
			Assert.IsTrue (ret, "#A1-1");
			Assert.AreEqual ("AddOrGetExisting (string key, object value, CacheItemPolicy policy, string regionName = null)", poker.MethodCalled, "#A1-2");

			ret = poker.Add ("key", 1234, null, null);
			Assert.IsTrue (ret, "#A2-1");
			Assert.AreEqual ("AddOrGetExisting (string key, object value, CacheItemPolicy policy, string regionName = null)", poker.MethodCalled, "#A2-2");

			ret = poker.Add ("key", 1234, null, null);
			Assert.IsFalse (ret, "#A2-1");
			Assert.AreEqual ("AddOrGetExisting (string key, object value, CacheItemPolicy policy, string regionName = null)", poker.MethodCalled, "#A2-2");
		}

		[Test]
		public void Add_String_Object_DateTimeOffset_String ()
		{
			var poker = new PokerObjectCache ();
			bool ret;

			ret = poker.Add (null, null, DateTimeOffset.Now, null);
			Assert.IsTrue (ret, "#A1-1");
			Assert.AreEqual ("AddOrGetExisting (string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)", poker.MethodCalled, "#A1-2");

			ret = poker.Add ("key", 1234, DateTimeOffset.Now, null);
			Assert.IsTrue (ret, "#A2-1");
			Assert.AreEqual ("AddOrGetExisting (string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)", poker.MethodCalled, "#A2-2");

			ret = poker.Add ("key", 1234, DateTimeOffset.Now, null);
			Assert.IsFalse (ret, "#A2-1");
			Assert.AreEqual ("AddOrGetExisting (string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)", poker.MethodCalled, "#A2-2");
		}

		[Test]
		public void GetValues ()
		{
			var poker = new PokerObjectCache ();

			IDictionary<string, object> values = poker.GetValues (null, (string []) null);
			Assert.IsNotNull (values, "#A1-1");
			Assert.AreEqual (0, values.Count, "#A1-2");
			Assert.AreEqual ("IDictionary<string, object> GetValues (IEnumerable<string> keys, string regionName = null)", poker.MethodCalled, "#A1-3");

			poker.Add ("key1", 1, null);
			poker.Add ("key2", 2, null);
			poker.Add ("key3", 3, null);

			values = poker.GetValues (new string [] { "key1", "key2", "key3" });
			Assert.IsNotNull (values, "#A2-1");
			Assert.AreEqual (3, values.Count, "#A2-2");
			Assert.AreEqual ("IDictionary<string, object> GetValues (IEnumerable<string> keys, string regionName = null)", poker.MethodCalled, "#A2-3");

			values = poker.GetValues (new string [] { "key1", "key22", "key3" });
			Assert.IsNotNull (values, "#A3-1");
			Assert.AreEqual (2, values.Count, "#A3-2");
			Assert.AreEqual ("IDictionary<string, object> GetValues (IEnumerable<string> keys, string regionName = null)", poker.MethodCalled, "#A3-3");
		}

		[Test]
		public void Defaults ()
		{
			Assert.AreEqual (DateTimeOffset.MaxValue, ObjectCache.InfiniteAbsoluteExpiration, "#A1");
			Assert.AreEqual (TimeSpan.Zero, ObjectCache.NoSlidingExpiration, "#A2");
		}
	}
}
