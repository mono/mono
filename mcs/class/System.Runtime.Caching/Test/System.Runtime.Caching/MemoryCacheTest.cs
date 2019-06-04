//
// MemoryCacheTest.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading;

using NUnit.Framework;
using MonoTests.Common;

namespace MonoTests.System.Runtime.Caching
{
	[TestFixture]
	public class MemoryCacheTest
	{
		[Test]
		public void ConstructorParameters ()
		{
			MemoryCache mc;
			Assert.Throws<ArgumentNullException> (() => {
				mc = new MemoryCache (null);
			}, "#A1");

			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache (String.Empty);
			}, "#A2");

			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("default");
			}, "#A3");

			var config = new NameValueCollection ();
			config.Add ("CacheMemoryLimitMegabytes", "invalid");
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-1");

			config.Clear ();
			config.Add ("PhysicalMemoryLimitPercentage", "invalid");
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-2");

			config.Clear ();
			config.Add ("PollingInterval", "invalid");
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-3");

			config.Clear ();
			config.Add ("CacheMemoryLimitMegabytes", "-1");
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-4");

			config.Clear ();
			config.Add ("CacheMemoryLimitMegabytes", UInt64.MaxValue.ToString ());
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-5");

			config.Clear ();
			config.Add ("PhysicalMemoryLimitPercentage", "-1");
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-6");

			config.Clear ();
			config.Add ("PhysicalMemoryLimitPercentage", UInt64.MaxValue.ToString ());
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-7");

			config.Clear ();
			config.Add ("PhysicalMemoryLimitPercentage", UInt32.MaxValue.ToString ());
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-8");

			config.Clear ();
			config.Add ("PhysicalMemoryLimitPercentage", "-10");
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-9");

			config.Clear ();
			config.Add ("PhysicalMemoryLimitPercentage", "0");
			// Just make sure it doesn't throw any exception
			mc = new MemoryCache ("MyCache", config);

			config.Clear ();
			config.Add ("PhysicalMemoryLimitPercentage", "101");
			Assert.Throws<ArgumentException> (() => {
				mc = new MemoryCache ("MyCache", config);
			}, "#A4-10");

			// Just make sure it doesn't throw any exception
			config.Clear ();
			config.Add ("UnsupportedSetting", "123");
			mc = new MemoryCache ("MyCache", config);
		}

		[Test]
		public void Defaults ()
		{
			var mc = new MemoryCache ("MyCache");
			Assert.AreEqual ("MyCache", mc.Name, "#A1");
			// Value of this property is different from system to system
			//Assert.AreEqual (0, mc.CacheMemoryLimit, "#A3");
			Assert.AreEqual (TimeSpan.FromMinutes (2), mc.PollingInterval, "#A4");
			Assert.AreEqual (
				DefaultCacheCapabilities.InMemoryProvider |
				DefaultCacheCapabilities.CacheEntryChangeMonitors |
				DefaultCacheCapabilities.AbsoluteExpirations |
				DefaultCacheCapabilities.SlidingExpirations |
				DefaultCacheCapabilities.CacheEntryRemovedCallback |
				DefaultCacheCapabilities.CacheEntryUpdateCallback,
				mc.DefaultCacheCapabilities, "#A1");
		}

		[Test]
		public void DefaultInstanceDefaults ()
		{
			var mc = MemoryCache.Default;
			Assert.AreEqual ("Default", mc.Name, "#A1");
			// Value of this property is different from system to system
			//Assert.AreEqual (0, mc.CacheMemoryLimit, "#A3");
			Assert.AreEqual (TimeSpan.FromMinutes (2), mc.PollingInterval, "#A4");
			Assert.AreEqual (
				DefaultCacheCapabilities.InMemoryProvider |
				DefaultCacheCapabilities.CacheEntryChangeMonitors |
				DefaultCacheCapabilities.AbsoluteExpirations |
				DefaultCacheCapabilities.SlidingExpirations |
				DefaultCacheCapabilities.CacheEntryRemovedCallback |
				DefaultCacheCapabilities.CacheEntryUpdateCallback,
				mc.DefaultCacheCapabilities, "#A1");
		}

		[Test]
		public void ConstructorValues ()
		{
			var config = new NameValueCollection ();
			config.Add ("CacheMemoryLimitMegabytes", "1");
			config.Add ("pollingInterval", "00:10:00");

			var mc = new MemoryCache ("MyCache", config);
			Assert.AreEqual (1048576, mc.CacheMemoryLimit, "#A2");
			Assert.AreEqual (TimeSpan.FromMinutes (10), mc.PollingInterval, "#A3");

			config.Clear ();
			config.Add ("PhysicalMemoryLimitPercentage", "10");
			config.Add ("CacheMemoryLimitMegabytes", "5");
			config.Add ("PollingInterval", "01:10:00");

			mc = new MemoryCache ("MyCache", config);
			Assert.AreEqual (10, mc.PhysicalMemoryLimit, "#B1");
			Assert.AreEqual (5242880, mc.CacheMemoryLimit, "#B2");
			Assert.AreEqual (TimeSpan.FromMinutes (70), mc.PollingInterval, "#B3");
		}

		[Test]
		public void Indexer ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<ArgumentNullException> (() => {
				mc [null] = "value";
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				object v = mc [null];
			}, "#A1-2");

			Assert.Throws<ArgumentNullException> (() => {
				mc ["key"] = null;
			}, "#A1-3");

			mc.Calls.Clear ();
			mc ["key"] = "value";
			Assert.AreEqual (3, mc.Calls.Count, "#A2-1");
			Assert.AreEqual ("set_this [string key]", mc.Calls [0], "#A2-2");
			Assert.AreEqual ("Set (string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)", mc.Calls [1], "#A2-3");
			Assert.AreEqual ("Set (string key, object value, CacheItemPolicy policy, string regionName = null)", mc.Calls [2], "#A2-4");
			Assert.IsTrue (mc.Contains ("key"), "#A2-5");

			mc.Calls.Clear ();
			object value = mc ["key"];
			Assert.AreEqual (1, mc.Calls.Count, "#A3-1");
			Assert.AreEqual ("get_this [string key]", mc.Calls [0], "#A3-2");
			Assert.AreEqual ("value", value, "#A3-3");
		}

		[Test]
		public void Contains ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<ArgumentNullException> (() => {
				mc.Contains (null);
			}, "#A1-1");

			Assert.Throws<NotSupportedException> (() => {
				mc.Contains ("key", "region");
			}, "#A1-2");

			mc.Set ("key", "value", ObjectCache.InfiniteAbsoluteExpiration);
			Assert.IsTrue (mc.Contains ("key"), "#A2");

			var cip = new CacheItemPolicy ();
			cip.Priority = CacheItemPriority.NotRemovable;
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (500);
			mc.Set ("key", "value", cip);
			Assert.IsTrue (mc.Contains ("key"), "#B1-1");
			Thread.Sleep (1000);
			// The call below removes the expired entry and returns false
			Assert.IsFalse (mc.Contains ("key"), "#B1-2");
		}

		[Test]
		public void CreateCacheEntryChangeMonitor ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<NotSupportedException> (() => {
				mc.CreateCacheEntryChangeMonitor (new string [] { "key" }, "region");
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				mc.CreateCacheEntryChangeMonitor (null);
			}, "#A1-2");

			Assert.Throws<ArgumentException> (() => {
				mc.CreateCacheEntryChangeMonitor (new string [] {});
			}, "#A1-3");

			Assert.Throws<ArgumentException> (() => {
				mc.CreateCacheEntryChangeMonitor (new string [] { "key", null });
			}, "#A1-4");

			mc.Set ("key1", "value1", ObjectCache.InfiniteAbsoluteExpiration);
			mc.Set ("key2", "value2", ObjectCache.InfiniteAbsoluteExpiration);
			mc.Set ("key3", "value3", ObjectCache.InfiniteAbsoluteExpiration);

			CacheEntryChangeMonitor monitor = mc.CreateCacheEntryChangeMonitor (new string [] { "key1", "key2" });
			Assert.IsNotNull (monitor, "#A2-1");
			Assert.AreEqual ("System.Runtime.Caching.MemoryCacheEntryChangeMonitor", monitor.GetType ().ToString (), "#A2-2");
			Assert.AreEqual (2, monitor.CacheKeys.Count, "#A2-3");
			Assert.AreEqual ("key1", monitor.CacheKeys [0], "#A2-3-1");
			Assert.AreEqual ("key2", monitor.CacheKeys [1], "#A2-3-2");
			Assert.IsNull (monitor.RegionName, "#A2-4");
			// Since this comparison can fail from time to time, leaving it commented out
			//Assert.AreEqual (DateTimeOffset.UtcNow.ToString (), monitor.LastModified.ToString (), "#A2-5");
			Assert.IsFalse (monitor.HasChanged, "#A2-5");

			// The actual unique id is constructed from key names followed by the hex value of ticks of their last modifed time
			Assert.IsFalse (String.IsNullOrEmpty (monitor.UniqueId), "#A2-6");

			// There seems to be a bug in .NET 4.0 regarding the code below. MSDN says that non-existing keys will cause the
			// returned monitor instance to be marked as changed, but instead this exception is thrown:
			//
			// MonoTests.System.Runtime.Caching.MemoryCacheTest.CreateCacheEntryChangeMonitor:
			// System.ArgumentOutOfRangeException : The UTC time represented when the offset is applied must be between year 0 and 10,000.
			// Parameter name: offset
			// 
			// at System.DateTimeOffset.ValidateDate(DateTime dateTime, TimeSpan offset)
			// at System.DateTimeOffset..ctor(DateTime dateTime)
			// at System.Runtime.Caching.MemoryCacheEntryChangeMonitor.InitDisposableMembers(MemoryCache cache)
			// at System.Runtime.Caching.MemoryCache.CreateCacheEntryChangeMonitor(IEnumerable`1 keys, String regionName)
			// at MonoTests.Common.PokerMemoryCache.CreateCacheEntryChangeMonitor(IEnumerable`1 keys, String regionName) in C:\Users\grendel\documents\visual studio 2010\Projects\System.Runtime.Caching.Test\System.Runtime.Caching.Test\Common\PokerMemoryCache.cs:line 113
			// at MonoTests.System.Runtime.Caching.MemoryCacheTest.CreateCacheEntryChangeMonitor() in C:\Users\grendel\documents\visual studio 2010\Projects\System.Runtime.Caching.Test\System.Runtime.Caching.Test\System.Runtime.Caching\MemoryCacheTest.cs:line 275
			//
			// It's probably caused by the code passing a DateTime.MinValue to DateTimeOffset constructor for non-existing entries.
			// Until this (apparent) bug is fixed, Mono is going to implement the buggy behavior.
			//
#if false
			monitor = mc.CreateCacheEntryChangeMonitor (new string [] { "key1", "doesnotexist" });
			Assert.IsNotNull (monitor, "#A3-1");
			Assert.AreEqual ("System.Runtime.Caching.MemoryCacheEntryChangeMonitor", monitor.GetType ().ToString (), "#A3-2");
			Assert.AreEqual (1, monitor.CacheKeys.Count, "#A3-3");
			Assert.AreEqual ("key1", monitor.CacheKeys [0], "#A3-3-1");
			Assert.IsNull (monitor.RegionName, "#A3-4");
			Assert.IsTrue (monitor.HasChanged, "#A3-5");
#endif
		}

		[Test]
		public void AddOrGetExisting_String_Object_DateTimeOffset_String ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<ArgumentNullException> (() => {
				mc.AddOrGetExisting (null, "value", DateTimeOffset.Now);
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				mc.AddOrGetExisting ("key", null, DateTimeOffset.Now);
			}, "#A1-2");
			
			Assert.Throws<NotSupportedException> (() => {
				mc.AddOrGetExisting ("key", "value", DateTimeOffset.Now, "region");
			}, "#A1-3");

			object value = mc.AddOrGetExisting ("key3_A2-1", "value", DateTimeOffset.Now.AddMinutes (1));
			Assert.IsTrue (mc.Contains ("key3_A2-1"), "#A2-1");
			Assert.IsNull (value, "#A2-2");

			mc.Calls.Clear ();
			value = mc.AddOrGetExisting ("key3_A2-1", "value2", DateTimeOffset.Now.AddMinutes (1));
			Assert.IsTrue (mc.Contains ("key3_A2-1"), "#A3-1");
			Assert.IsNotNull (value, "#A3-2");
			Assert.AreEqual ("value", value, "#A3-3");
			Assert.AreEqual (2, mc.Calls.Count, "#A3-4");
			Assert.AreEqual ("AddOrGetExisting (string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)", mc.Calls [0], "#A3-5");

			value = mc.AddOrGetExisting ("key_expired", "value", DateTimeOffset.MinValue);
			Assert.IsFalse (mc.Contains ("key_expired"), "#A4-1");
			Assert.IsNull (value, "#A4-1");
		}

		[Test]
		public void AddOrGetExisting_String_Object_CacheItemPolicy_String ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<ArgumentNullException> (() => {
				mc.AddOrGetExisting (null, "value", null);
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				mc.AddOrGetExisting ("key", null, null);
			}, "#A1-2");

			var cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTime.Now.AddMinutes (1);
			cip.SlidingExpiration = TimeSpan.FromMinutes (1);

			Assert.Throws<ArgumentException> (() => {
				mc.AddOrGetExisting ("key", "value", cip);
			}, "#A1-3");

			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.MinValue;
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.AddOrGetExisting ("key3", "value", cip);
			}, "#A1-4");

			Assert.Throws<NotSupportedException> (() => {
				mc.AddOrGetExisting ("key", "value", null, "region");
			}, "#A1-5");

			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromDays (500);
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.AddOrGetExisting ("key3", "value", cip);
			}, "#A1-6");

			cip = new CacheItemPolicy ();
			cip.Priority = (CacheItemPriority) 20;
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.AddOrGetExisting ("key3", "value", cip);
			}, "#A1-7");

			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromTicks (0L);
			mc.AddOrGetExisting ("key3_A2-1", "value", cip);
			Assert.IsTrue (mc.Contains ("key3_A2-1"), "#A2-1");

			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromDays (365);
			mc.AddOrGetExisting ("key3_A2-2", "value", cip);
			Assert.IsTrue (mc.Contains ("key3_A2-2"), "#A2-2");

			cip = new CacheItemPolicy ();
			cip.RemovedCallback = (CacheEntryRemovedArguments arguments) => { };
			object value = mc.AddOrGetExisting ("key3_A2-3", "value", cip);
			Assert.IsTrue (mc.Contains ("key3_A2-3"), "#A2-3");
			Assert.IsNull (value, "#A2-4");

			mc.Calls.Clear ();
			value = mc.AddOrGetExisting ("key3_A2-3", "value2", null);
			Assert.IsTrue (mc.Contains ("key3_A2-3"), "#A3-1");
			Assert.IsNotNull (value, "#A3-2");
			Assert.AreEqual ("value", value, "#A3-3");
			Assert.AreEqual (2, mc.Calls.Count, "#A3-4");
			Assert.AreEqual ("AddOrGetExisting (string key, object value, CacheItemPolicy policy, string regionName = null)", mc.Calls [0], "#A3-5");

			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.MinValue;
			value = mc.AddOrGetExisting ("key_expired", "value", cip);
			Assert.IsFalse (mc.Contains ("key_expired"), "#A4-1");
			Assert.IsNull (value, "#A4-1");
		}

		[Test]
		public void AddOrGetExisting_CacheItem_CacheItemPolicy ()
		{
			var mc = new PokerMemoryCache ("MyCache");
			CacheItem ci, ci2;

			Assert.Throws<ArgumentNullException> (() => {
				ci = mc.AddOrGetExisting (null, new CacheItemPolicy ());
			}, "#A1");

			ci = new CacheItem ("key", "value");
			ci2 = mc.AddOrGetExisting (ci, null);

			// LAMESPEC: MSDN says it should return null if the entry does not exist yet.
			//
			Assert.IsNotNull (ci2, "#A2-1"); 
			Assert.AreNotEqual (ci, ci2, "#A2-2");
			Assert.IsNull (ci2.Value, "#A2-3");
			Assert.IsTrue (mc.Contains (ci.Key), "#A2-4");
			Assert.AreEqual (ci.Key, ci2.Key, "#A2-5");

			ci = new CacheItem ("key", "value");
			ci2 = mc.AddOrGetExisting (ci, null);
			Assert.IsNotNull (ci2, "#A3-1");
			Assert.AreNotEqual (ci, ci2, "#A3-2");
			Assert.IsNotNull (ci2.Value, "#A3-3");
			Assert.AreEqual (ci.Value, ci2.Value, "#A3-4");
			Assert.AreEqual (ci.Key, ci2.Key, "#A3-5");

			Assert.Throws<ArgumentNullException> (() => {
				ci = new CacheItem (null, "value");
				ci2 = mc.AddOrGetExisting (ci, null);
			}, "#A4");

			ci = new CacheItem (String.Empty, "value");
			ci2 = mc.AddOrGetExisting (ci, null);
			Assert.IsNotNull (ci2, "#A5-1");
			Assert.AreNotEqual (ci, ci2, "#A5-2");
			Assert.IsNull (ci2.Value, "#A5-3");
			Assert.IsTrue (mc.Contains (ci.Key), "#A5-4");
			Assert.AreEqual (ci.Key, ci2.Key, "#A5-5");

			ci = new CacheItem ("key2", null);

			// Thrown from:
			// at System.Runtime.Caching.MemoryCacheEntry..ctor(String key, Object value, DateTimeOffset absExp, TimeSpan slidingExp, CacheItemPriority priority, Collection`1 dependencies, CacheEntryRemovedCallback removedCallback, MemoryCache cache)
			// at System.Runtime.Caching.MemoryCache.AddOrGetExistingInternal(String key, Object value, CacheItemPolicy policy)
			// at System.Runtime.Caching.MemoryCache.AddOrGetExisting(CacheItem item, CacheItemPolicy policy)
			// at MonoTests.System.Runtime.Caching.MemoryCacheTest.AddOrGetExisting_CacheItem_CacheItemPolicy() in C:\Users\grendel\documents\visual studio 2010\Projects\System.Runtime.Caching.Test\System.Runtime.Caching.Test\System.Runtime.Caching\MemoryCacheTest.cs:line 211
			Assert.Throws<ArgumentNullException> (() => {
				ci2 = mc.AddOrGetExisting (ci, null);
			}, "#B1");
			
			ci = new CacheItem ("key3", "value");
			var cip = new CacheItemPolicy ();
			cip.UpdateCallback = (CacheEntryUpdateArguments arguments) => { };
			Assert.Throws<ArgumentException> (() => {
				ci2 = mc.AddOrGetExisting (ci, cip);
			}, "#B2");

			ci = new CacheItem ("key3", "value");
			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.Now;
			cip.SlidingExpiration = TimeSpan.FromTicks (DateTime.Now.Ticks);
			Assert.Throws<ArgumentException> (() => {
				mc.AddOrGetExisting (ci, cip);
			}, "#B3");

			ci = new CacheItem ("key3", "value");
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.MinValue;
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.AddOrGetExisting (ci, cip);
			}, "#B4-1");

			ci = new CacheItem ("key4_#B4-2", "value");
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromTicks (0L);
			mc.AddOrGetExisting (ci, cip);
			Assert.IsTrue (mc.Contains ("key4_#B4-2"), "#B4-2");

			ci = new CacheItem ("key3", "value");
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromDays (500);
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.AddOrGetExisting (ci, cip);
			}, "#B5-1");

			ci = new CacheItem ("key5_#B5-2", "value");
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromDays (365);
			mc.AddOrGetExisting (ci, cip);
			Assert.IsTrue (mc.Contains ("key5_#B5-2"), "#B5-2");

			ci = new CacheItem ("key3", "value");
			cip = new CacheItemPolicy ();
			cip.Priority = (CacheItemPriority)20;
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.AddOrGetExisting (ci, cip);
			}, "#B6");

			ci = new CacheItem ("key3_B7", "value");
			cip = new CacheItemPolicy ();
			cip.RemovedCallback = (CacheEntryRemovedArguments arguments) => { };
			ci2 = mc.AddOrGetExisting (ci, cip);
			Assert.IsTrue (mc.Contains ("key3_B7"), "#B7");

			// LAMESPEC: MSDN says it should return null if the entry does not exist yet.
			//
			Assert.IsNotNull (ci2, "#C1-1");
			Assert.AreNotEqual (ci, ci2, "#C1-2");
			Assert.IsNull (ci2.Value, "#C1-3");
			Assert.IsTrue (mc.Contains (ci.Key), "#C1-4");
			Assert.AreEqual (ci.Key, ci2.Key, "#C1-5");

			// The entry is never inserted as its expiration date is before now
			ci = new CacheItem ("key_D1", "value_D1");
			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.MinValue;
			ci2 = mc.AddOrGetExisting (ci, cip);
			Assert.IsFalse (mc.Contains ("key_D1"), "#D1-1");
			Assert.IsNotNull (ci2, "#D1-2");
			Assert.IsNull (ci2.Value, "#D1-3");
			Assert.AreEqual ("key_D1", ci2.Key, "#D1-4");

			mc.Calls.Clear ();
			ci = new CacheItem ("key_D2", "value_D2");
			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.MaxValue;
			mc.AddOrGetExisting (ci, cip);
			Assert.IsTrue (mc.Contains ("key_D2"), "#D2-1");
			Assert.AreEqual (2, mc.Calls.Count, "#D2-2");
			Assert.AreEqual ("AddOrGetExisting (CacheItem item, CacheItemPolicy policy)", mc.Calls [0], "#D2-3");
		}

		[Test]
		public void Set_String_Object_CacheItemPolicy_String ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<NotSupportedException> (() => {
				mc.Set ("key", "value", new CacheItemPolicy (), "region");
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				mc.Set (null, "value", new CacheItemPolicy ());
			}, "#A1-2");

			Assert.Throws<ArgumentNullException> (() => {
				mc.Set ("key", null, new CacheItemPolicy ());
			}, "#A1-3");

			var cip = new CacheItemPolicy ();
			cip.UpdateCallback = (CacheEntryUpdateArguments arguments) => { };
			cip.RemovedCallback = (CacheEntryRemovedArguments arguments) => { };
			Assert.Throws<ArgumentException> (() => {
				mc.Set ("key", "value", cip);
			}, "#A1-4");

			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.MinValue;
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.Set ("key", "value", cip);
			}, "#A1-5");

			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromTicks (0L);
			mc.Set ("key_A1-6", "value", cip);
			Assert.IsTrue (mc.Contains ("key_A1-6"), "#A1-6");

			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromDays (500);
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.Set ("key", "value", cip);
			}, "#A1-7");
			
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromDays (365);
			mc.Set ("key_A1-8", "value", cip);
			Assert.IsTrue (mc.Contains ("key_A1-8"), "#A1-8");

			cip = new CacheItemPolicy ();
			cip.Priority = (CacheItemPriority) 20;
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.Set ("key", "value", cip);
			}, "#A1-9");
			
			cip = new CacheItemPolicy ();
			cip.RemovedCallback = (CacheEntryRemovedArguments arguments) => { };
			mc.Set ("key_A2", "value_A2", cip);
			Assert.IsTrue (mc.Contains ("key_A2"), "#A2");

			mc.Set ("key_A3", "value_A3", new CacheItemPolicy ());
			Assert.IsTrue (mc.Contains ("key_A3"), "#A3-1");
			Assert.AreEqual ("value_A3", mc.Get ("key_A3"), "#A3-2");

			// The entry is never inserted as its expiration date is before now
			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.MinValue;
			mc.Set ("key_A4", "value_A4", cip);
			Assert.IsFalse (mc.Contains ("key_A4"), "#A4");

			mc.Calls.Clear ();
			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.MaxValue;
			mc.Set ("key_A5", "value_A5", cip);
			Assert.IsTrue (mc.Contains ("key_A5"), "#A5-1");
			Assert.AreEqual (2, mc.Calls.Count, "#A5-2");
			Assert.AreEqual ("Set (string key, object value, CacheItemPolicy policy, string regionName = null)", mc.Calls [0], "#A5-3");
		}

		[Test]
		public void Set_String_Object_DateTimeOffset_String ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<NotSupportedException> (() => {
				mc.Set ("key", "value", DateTimeOffset.MaxValue, "region");
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				mc.Set (null, "value", DateTimeOffset.MaxValue);
			}, "#A1-2");

			Assert.Throws<ArgumentNullException> (() => {
				mc.Set ("key", null, DateTimeOffset.MaxValue);
			}, "#A1-3");
			
			// The entry is never inserted as its expiration date is before now
			mc.Set ("key_A2", "value_A2", DateTimeOffset.MinValue);
			Assert.IsFalse (mc.Contains ("key_A2"), "#A2");

			mc.Calls.Clear ();
			mc.Set ("key", "value", DateTimeOffset.MaxValue);

			Assert.AreEqual (2, mc.Calls.Count, "#A2-1");
			Assert.AreEqual ("Set (string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)", mc.Calls [0], "#A2-2");
			Assert.AreEqual ("Set (string key, object value, CacheItemPolicy policy, string regionName = null)", mc.Calls [1], "#A2-3");
		}

		[Test]
		public void Set_CacheItem_CacheItemPolicy ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<ArgumentNullException> (() => {
				mc.Set (null, new CacheItemPolicy ());
			}, "#A1-1");

			// Actually thrown from the Set (string, object, CacheItemPolicy, string) overload
			var ci = new CacheItem (null, "value");
			Assert.Throws<ArgumentNullException> (() => {
				mc.Set (ci, new CacheItemPolicy ());
			}, "#A1-2");

			ci = new CacheItem ("key", null);
			Assert.Throws<ArgumentNullException> (() => {
				mc.Set (ci, new CacheItemPolicy ());
			}, "#A1-3");

			ci = new CacheItem ("key", "value");
			var cip = new CacheItemPolicy ();
			cip.UpdateCallback = (CacheEntryUpdateArguments arguments) => { };
			cip.RemovedCallback = (CacheEntryRemovedArguments arguments) => { };
			Assert.Throws<ArgumentException> (() => {
				mc.Set (ci, cip);
			}, "#A1-4");

			ci = new CacheItem ("key", "value");
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.MinValue;
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.Set (ci, cip);
			}, "#A1-5");

			ci = new CacheItem ("key_A1-6", "value");
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromTicks (0L);
			mc.Set (ci, cip);
			Assert.IsTrue (mc.Contains ("key_A1-6"), "#A1-6");

			ci = new CacheItem ("key", "value");
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromDays (500);
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.Set (ci, cip);
			}, "#A1-7");

			ci = new CacheItem ("key_A1-8", "value");
			cip = new CacheItemPolicy ();
			cip.SlidingExpiration = TimeSpan.FromDays (365);
			mc.Set (ci, cip);
			Assert.IsTrue (mc.Contains ("key_A1-8"), "#A1-8");

			ci = new CacheItem ("key", "value");
			cip = new CacheItemPolicy ();
			cip.Priority = (CacheItemPriority) 20;
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mc.Set (ci, cip);
			}, "#A1-9");

			ci = new CacheItem ("key_A2", "value_A2");
			cip = new CacheItemPolicy ();
			cip.RemovedCallback = (CacheEntryRemovedArguments arguments) => { };
			mc.Set (ci, cip);
			Assert.IsTrue (mc.Contains ("key_A2"), "#A2");

			ci = new CacheItem ("key_A3", "value_A3");
			mc.Set (ci, new CacheItemPolicy ());
			Assert.IsTrue (mc.Contains ("key_A3"), "#A3-1");
			Assert.AreEqual ("value_A3", mc.Get ("key_A3"), "#A3-2");

			// The entry is never inserted as its expiration date is before now
			ci = new CacheItem ("key_A4", "value");
			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.MinValue;
			mc.Set (ci, cip);
			Assert.IsFalse (mc.Contains ("key_A4"), "#A4");

			ci = new CacheItem ("key_A5", "value");
			mc.Calls.Clear ();
			mc.Set (ci, new CacheItemPolicy ());

			Assert.AreEqual (2, mc.Calls.Count, "#A5-1");
			Assert.AreEqual ("Set (CacheItem item, CacheItemPolicy policy)", mc.Calls [0], "#A5-2");
			Assert.AreEqual ("Set (string key, object value, CacheItemPolicy policy, string regionName = null)", mc.Calls [1], "#A5-3");
		}

		[Test]
		public void Remove ()
		{
			var mc = new PokerMemoryCache ("MyCache");
		
			Assert.Throws<NotSupportedException> (() => {
				mc.Remove ("key", "region");
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				mc.Remove (null);
			}, "#A1-2");

			bool callbackInvoked;
			CacheEntryRemovedReason reason = (CacheEntryRemovedReason) 1000;
			var cip = new CacheItemPolicy ();
			cip.Priority = CacheItemPriority.NotRemovable;
			mc.Set ("key2", "value1", cip);
			object value = mc.Remove ("key2");

			Assert.IsNotNull (value, "#B1-1");
			Assert.IsFalse (mc.Contains ("key2"), "#B1-2");

			cip = new CacheItemPolicy ();
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				callbackInvoked = true;
				reason = args.RemovedReason;
			};

			mc.Set ("key", "value", cip);
			callbackInvoked = false;
			reason = (CacheEntryRemovedReason) 1000;
			value = mc.Remove ("key");
			Assert.IsNotNull (value, "#C1-1");
			Assert.IsTrue (callbackInvoked, "#C1-2");
			Assert.AreEqual (CacheEntryRemovedReason.Removed, reason, "#C1-3");

			cip = new CacheItemPolicy ();
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				callbackInvoked = true;
				reason = args.RemovedReason;
				throw new ApplicationException ("test");
			};

			mc.Set ("key", "value", cip);
			callbackInvoked = false;
			reason = (CacheEntryRemovedReason) 1000;
			value = mc.Remove ("key");
			Assert.IsNotNull (value, "#C2-1");
			Assert.IsTrue (callbackInvoked, "#C2-2");
			Assert.AreEqual (CacheEntryRemovedReason.Removed, reason, "#C2-3");

			// LAMESPEC: UpdateCallback is not called on remove
			cip = new CacheItemPolicy ();
			cip.UpdateCallback = (CacheEntryUpdateArguments args) => {
				callbackInvoked = true;
				reason = args.RemovedReason;
			};

			mc.Set ("key", "value", cip);
			callbackInvoked = false;
			reason = (CacheEntryRemovedReason) 1000;
			value = mc.Remove ("key");
			Assert.IsNotNull (value, "#D1-1");
			Assert.IsFalse (callbackInvoked, "#D1-2");

			cip = new CacheItemPolicy ();
			cip.UpdateCallback = (CacheEntryUpdateArguments args) => {
				callbackInvoked = true;
				reason = args.RemovedReason;
				throw new ApplicationException ("test");
			};

			mc.Set ("key", "value", cip);
			callbackInvoked = false;
			reason = (CacheEntryRemovedReason) 1000;
			value = mc.Remove ("key");
			Assert.IsNotNull (value, "#D2-1");
			Assert.IsFalse (callbackInvoked, "#D2-2");
		}

		[Test]
		public void TimedExpiration ()
		{
			bool expired = false;
			CacheEntryRemovedReason reason = CacheEntryRemovedReason.CacheSpecificEviction;
			int sleepPeriod = 1100;

			var mc = new PokerMemoryCache ("MyCache");
			var cip = new CacheItemPolicy ();

			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				expired = true;
				reason = args.RemovedReason;
			};
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (50);
			mc.Set ("key", "value", cip);
			Thread.Sleep (500);

			Assert.IsFalse (expired, "#A1");
			object value = mc.Get ("key");

			Assert.IsNull (value, "#A2-1");
			Assert.IsTrue (expired, "#A2-2");
			Assert.AreEqual (CacheEntryRemovedReason.Expired, reason, "A2-3");

			expired = false;
			cip = new CacheItemPolicy ();
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				expired = true;
				reason = args.RemovedReason;
			};
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (50);
			mc.Set ("key", "value", cip);
			Thread.Sleep (sleepPeriod);

			Assert.IsNull (mc.Get ("key"), "#A3-0");
			Assert.IsTrue (expired, "#A3-1");
			Assert.AreEqual (CacheEntryRemovedReason.Expired, reason, "#A3-2");

			int expiredCount = 0;
			object expiredCountLock = new object ();
			CacheEntryRemovedCallback removedCb = (CacheEntryRemovedArguments args) => {
				lock (expiredCountLock) {
					expiredCount++;
				}
			};

			cip = new CacheItemPolicy ();
			cip.RemovedCallback = removedCb;
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (20);
			mc.Set ("key1", "value1", cip);

			cip = new CacheItemPolicy ();
			cip.RemovedCallback = removedCb;
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (200);
			mc.Set ("key2", "value2", cip);

			cip = new CacheItemPolicy ();
			cip.RemovedCallback = removedCb;
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (600);
			mc.Set ("key3", "value3", cip);

			cip = new CacheItemPolicy ();
			cip.RemovedCallback = removedCb;
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (sleepPeriod + 500);
			mc.Set ("key4", "value4", cip);
			
			Thread.Sleep (sleepPeriod);
			Assert.IsNull (mc.Get ("key1"), "#A4-1");
			Assert.IsNull (mc.Get ("key2"), "#A4-2");
			Assert.IsNull (mc.Get ("key3"), "#A4-3");
			Assert.IsNotNull (mc.Get ("key4"), "#A4-4");
			Assert.AreEqual (3, expiredCount, "#A4");
		}

		[Test]
		public void GetEnumerator ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			// This one is a Hashtable enumerator
			IEnumerator enumerator = ((IEnumerable) mc).GetEnumerator ();

			// This one is a Dictionary <string, object> enumerator
			IEnumerator enumerator2 = mc.DoGetEnumerator ();

			Assert.IsNotNull (enumerator, "#A1-1");
			Assert.IsNotNull (enumerator2, "#A1-2");
			Assert.IsTrue (enumerator.GetType () != enumerator2.GetType (), "#A1-3");

			mc.Set ("key1", "value1", null);
			mc.Set ("key2", "value2", null);
			mc.Set ("key3", "value3", null);

			bool expired = false;
			var cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTime.Now.AddMilliseconds (50);
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				expired = true;
			};

			mc.Set ("key4", "value4", cip);
			Thread.Sleep (500);

			enumerator = ((IEnumerable) mc).GetEnumerator ();
			int count = 0;
			while (enumerator.MoveNext ()) {
				count++;
			}

			Assert.IsFalse (expired, "#A2-1");
			Assert.AreEqual (3, count, "#A2-2");

			expired = false;
			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTime.Now.AddMilliseconds (50);
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				expired = true;
			};

			mc.Set ("key5", "value5", cip);
			Thread.Sleep (500);

			enumerator2 = mc.DoGetEnumerator ();
			count = 0;
			while (enumerator2.MoveNext ()) {
				count++;
			}

			Assert.IsFalse (expired, "#A3-1");
			Assert.AreEqual (3, count, "#A3-2");
		}

		[Test]
		public void GetValues ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<ArgumentNullException> (() => {
				mc.GetValues ((string[]) null);
			}, "#A1-1");

			Assert.Throws<NotSupportedException> (() => {
				mc.GetValues (new string[] {}, "region");
			}, "#A1-2");

			Assert.Throws<ArgumentException> (() => {
				mc.GetValues (new string [] { "key", null });
			}, "#A1-3");

			IDictionary<string, object> value = mc.GetValues (new string[] {});
			Assert.IsNull (value, "#A2");

			mc.Set ("key1", "value1", null);
			mc.Set ("key2", "value2", null);
			mc.Set ("key3", "value3", null);

			Assert.IsTrue (mc.Contains ("key1"), "#A3-1");
			Assert.IsTrue (mc.Contains ("key2"), "#A3-2");
			Assert.IsTrue (mc.Contains ("key3"), "#A3-2");

			value = mc.GetValues (new string [] { "key1", "key3" });
			Assert.IsNotNull (value, "#A4-1");
			Assert.AreEqual (2, value.Count, "#A4-2");
			Assert.AreEqual ("value1", value ["key1"], "#A4-3");
			Assert.AreEqual ("value3", value ["key3"], "#A4-4");
			Assert.AreEqual (typeof (Dictionary<string, object>), value.GetType (), "#A4-5");

			// LAMESPEC: MSDN says the number of items in the returned dictionary should be the same as in the 
			// 'keys' collection - this is not the case. The returned dictionary contains only entries for keys
			// that exist in the cache.
			value = mc.GetValues (new string [] { "key1", "key3", "nosuchkey" });
			Assert.IsNotNull (value, "#A5-1");
			Assert.AreEqual (2, value.Count, "#A5-2");
			Assert.AreEqual ("value1", value ["key1"], "#A5-3");
			Assert.AreEqual ("value3", value ["key3"], "#A5-4");
			Assert.IsFalse (value.ContainsKey ("Key1"), "#A5-5");
		}

		[Test]
		public void Get ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<NotSupportedException> (() => {
				mc.Get ("key", "region");
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				mc.Get (null);
			}, "#A1-2");

			object value;
			mc.Set ("key", "value", null);
			value = mc.Get ("key");
			Assert.IsNotNull (value, "#A2-1");
			Assert.AreEqual ("value", value, "#A2-2");

			value = mc.Get ("nosuchkey");
			Assert.IsNull (value, "#A3");

			var cip = new CacheItemPolicy ();
			bool callbackInvoked;
			CacheEntryRemovedReason reason = (CacheEntryRemovedReason)1000;

			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (50);
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				callbackInvoked = true;
				reason = args.RemovedReason;
			};
			mc.Set ("key", "value", cip);
			Thread.Sleep (500);

			callbackInvoked = false;
			reason = (CacheEntryRemovedReason) 1000;
			value = mc.Get ("key");
			Assert.IsNull (value, "#B1-1");
			Assert.IsTrue (callbackInvoked, "#B1-2");
			Assert.AreEqual (CacheEntryRemovedReason.Expired, reason, "#B1-3");

			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (50);
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				callbackInvoked = true;
				reason = args.RemovedReason;
				throw new ApplicationException ("test");
			};

			mc.Set ("key", "value", cip);
			Thread.Sleep (500);

			callbackInvoked = false;
			reason = (CacheEntryRemovedReason) 1000;
			value = mc.Get ("key");
			Assert.IsNull (value, "#B2-1");
			Assert.IsTrue (callbackInvoked, "#B2-2");
			Assert.AreEqual (CacheEntryRemovedReason.Expired, reason, "#B2-3");
		}

		[Test]
		public void GetCacheItem ()
		{
			var mc = new PokerMemoryCache ("MyCache");

			Assert.Throws<NotSupportedException> (() => {
				mc.GetCacheItem ("key", "region");
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				mc.GetCacheItem (null);
			}, "#A1-2");

			CacheItem value;
			mc.Set ("key", "value", null);
			value = mc.GetCacheItem ("key");
			Assert.IsNotNull (value, "#A2-1");
			Assert.AreEqual ("value", value.Value, "#A2-2");
			Assert.AreEqual ("key", value.Key, "#A2-3");

			value = mc.GetCacheItem ("doesnotexist");
			Assert.IsNull (value, "#A3");

			var cip = new CacheItemPolicy ();
			bool callbackInvoked;
			CacheEntryRemovedReason reason = (CacheEntryRemovedReason) 1000;

			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (50);
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				callbackInvoked = true;
				reason = args.RemovedReason;
			};
			mc.Set ("key", "value", cip);
			Thread.Sleep (500);

			callbackInvoked = false;
			reason = (CacheEntryRemovedReason) 1000;
			value = mc.GetCacheItem ("key");
			Assert.IsNull (value, "#B1-1");
			Assert.IsTrue (callbackInvoked, "#B1-2");
			Assert.AreEqual (CacheEntryRemovedReason.Expired, reason, "#B1-3");

			cip = new CacheItemPolicy ();
			cip.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds (50);
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				callbackInvoked = true;
				reason = args.RemovedReason;
				throw new ApplicationException ("test");
			};

			mc.Set ("key", "value", cip);
			Thread.Sleep (500);

			callbackInvoked = false;
			reason = (CacheEntryRemovedReason) 1000;
			value = mc.GetCacheItem ("key");
			Assert.IsNull (value, "#B2-1");
			Assert.IsTrue (callbackInvoked, "#B2-2");
			Assert.AreEqual (CacheEntryRemovedReason.Expired, reason, "#B2-3");
		}

		[Test]
		public void ChangeMonitors ()
		{
			bool removed = false;
			var mc = new PokerMemoryCache ("MyCache");
			var cip = new CacheItemPolicy ();
			var monitor = new PokerChangeMonitor ();
			cip.ChangeMonitors.Add (monitor);
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				removed = true;
			};

			mc.Set ("key", "value", cip);
			Assert.AreEqual (0, monitor.Calls.Count, "#A1");

			monitor.SignalChange ();
			Assert.IsTrue (removed, "#A2");

			bool onChangedCalled = false;
			monitor = new PokerChangeMonitor ();
			monitor.NotifyOnChanged ((object state) => {
				onChangedCalled = true;
			});

			cip = new CacheItemPolicy ();
			cip.ChangeMonitors.Add (monitor);

			// Thrown by ChangeMonitor.NotifyOnChanged
			Assert.Throws<InvalidOperationException> (() => {
				mc.Set ("key1", "value1", cip);
			}, "#A3");
		}

		// NOTE: on Windows with 2 or more CPUs this test will most probably fail.
		[Test]
		public void Trim ()
		{
			var config = new NameValueCollection ();
			config ["__MonoEmulateOneCPU"] = "true";
			var mc = new MemoryCache ("MyCache", config);

			for (int i = 0; i < 10; i++)
				mc.Set ("key" + i.ToString (), "value" + i.ToString (), null);

			Assert.AreEqual (10, mc.GetCount (), "#A1-1");
			long trimmed = mc.Trim (50);
			Assert.AreEqual (5, trimmed, "#A1-2");
			Assert.AreEqual (5, mc.GetCount (), "#A1-3");

			mc = new MemoryCache ("MyCache", config);
			// Only entries 11- are considered for removal
			for (int i = 0; i < 11; i++)
				mc.Set ("key" + i.ToString (), "value" + i.ToString (), null);

			Assert.AreEqual (11, mc.GetCount (), "#A2-1");
			trimmed = mc.Trim (50);
			Assert.AreEqual (6, trimmed, "#A2-2");
			Assert.AreEqual (5, mc.GetCount (), "#A2-3");

			mc = new MemoryCache ("MyCache", config);
			// Only entries 11- are considered for removal
			for (int i = 0; i < 125; i++)
				mc.Set ("key" + i.ToString (), "value" + i.ToString (), null);

			Assert.AreEqual (125, mc.GetCount (), "#A3-1");
			trimmed = mc.Trim (50);
			Assert.AreEqual (63, trimmed, "#A3-2");
			Assert.AreEqual (62, mc.GetCount (), "#A3-3");

			// Testing the removal order
			mc = new MemoryCache ("MyCache", config);
			var removed = new List <string> ();
			var cip = new CacheItemPolicy ();
			cip.RemovedCallback = (CacheEntryRemovedArguments args) => {
				removed.Add (args.CacheItem.Key);
			};

			for (int i = 0; i < 50; i++)
				mc.Set ("key" + i.ToString (), "value" + i.ToString (), cip);

			object value;
			for (int i = 0; i < 50; i++)
				value = mc.Get ("key" + i.ToString ());

			trimmed = mc.Trim (50);
			Assert.AreEqual (25, mc.GetCount (), "#A4-1");
			Assert.AreEqual (25, trimmed, "#A4-2");
			Assert.AreEqual (25, removed.Count, "#A4-3");

			for (int i = 0; i < 25; i++)
				Assert.AreEqual ("key" + i.ToString (), removed [i], "#A5-" + i.ToString ());
		}
		
		[Test]
		public void TestCacheShrink ()
		{
			const int HEAP_RESIZE_THRESHOLD = 8192 + 2;
			const int HEAP_RESIZE_SHORT_ENTRIES = 2048;
			const int HEAP_RESIZE_LONG_ENTRIES = HEAP_RESIZE_THRESHOLD - HEAP_RESIZE_SHORT_ENTRIES;			
			
			var config = new NameValueCollection ();
			config["cacheMemoryLimitMegabytes"] = 0.ToString ();
			config["physicalMemoryLimitPercentage"] = 100.ToString ();
			config["pollingInterval"] = new TimeSpan (0, 0, 1).ToString ();
			
			using (var mc = new MemoryCache ("TestCacheShrink",  config)) {	
				Assert.AreEqual (0, mc.GetCount (), "#CS1");
							
				// add some short duration entries
				for (int i = 0; i < HEAP_RESIZE_SHORT_ENTRIES; i++) {
					var expireAt = DateTimeOffset.Now.AddSeconds (3);
					mc.Add ("short-" + i, i.ToString (), expireAt);
				}
				
				Assert.AreEqual (HEAP_RESIZE_SHORT_ENTRIES, mc.GetCount (), "#CS2");
							
				// add some long duration entries				
				for (int i = 0; i < HEAP_RESIZE_LONG_ENTRIES; i++) {
					var expireAt = DateTimeOffset.Now.AddSeconds (12);
					mc.Add ("long-" + i, i.ToString (), expireAt);
				}															
				
				Assert.AreEqual (HEAP_RESIZE_LONG_ENTRIES + HEAP_RESIZE_SHORT_ENTRIES, mc.GetCount(), "#CS3");
				
				// wait for the cache thread to expire the short duration items, this will also shrink the size of the cache
				global::System.Threading.Thread.Sleep (5 * 1000);
				
				for (int i = 0; i < HEAP_RESIZE_SHORT_ENTRIES; i++) {
					Assert.IsNull (mc.Get ("short-" + i), "#CS4-" + i);
				}
				Assert.AreEqual (HEAP_RESIZE_LONG_ENTRIES, mc.GetCount (), "#CS4");	
				
				// add some new items into the cache, this will grow the cache again
				for (int i = 0; i < HEAP_RESIZE_LONG_ENTRIES; i++) {				
					mc.Add("final-" + i, i.ToString (), DateTimeOffset.Now.AddSeconds (4));
				}			
				
				Assert.AreEqual (HEAP_RESIZE_LONG_ENTRIES + HEAP_RESIZE_LONG_ENTRIES, mc.GetCount (), "#CS5");	
			}
		}

		[Test]
		public void TestExpiredGetValues ()
		{
			var config = new NameValueCollection ();
			config["cacheMemoryLimitMegabytes"] = 0.ToString ();
			config["physicalMemoryLimitPercentage"] = 100.ToString ();
			config["pollingInterval"] = new TimeSpan (0, 0, 10).ToString ();
			
			using (var mc = new MemoryCache ("TestExpiredGetValues",  config)) {
				Assert.AreEqual (0, mc.GetCount (), "#EGV1");

				var keys = new List<string> ();

				// add some short duration entries
				for (int i = 0; i < 10; i++) {
					var key = "short-" + i;
					var expireAt = DateTimeOffset.Now.AddSeconds (1);
					mc.Add (key, i.ToString (), expireAt);

					keys.Add (key);
				}

				Assert.AreEqual (10, mc.GetCount (), "#EGV2");

				global::System.Threading.Thread.Sleep (4 * 1000);

				// we have waited but the items won't be expired by the timer since it wont have fired yet
				Assert.AreEqual (10, mc.GetCount (), "#EGV3");

				// calling GetValues() will expire the items since we are now past their expiresAt
				mc.GetValues (keys);

				Assert.AreEqual (0, mc.GetCount (), "#EGV4");
			}
		}

		[Test]
		public void TestCacheExpiryOrdering ()
		{
			var config = new NameValueCollection ();
			config["cacheMemoryLimitMegabytes"] = 0.ToString ();
			config["physicalMemoryLimitPercentage"] = 100.ToString ();
			config["pollingInterval"] = new TimeSpan (0, 0, 1).ToString ();

			using (var mc = new MemoryCache ("TestCacheExpiryOrdering",  config)) {
				Assert.AreEqual (0, mc.GetCount (), "#CEO1");

				// add long lived items into the cache first
				for (int i = 0; i < 100; i++) {
					var cip = new CacheItemPolicy ();
					cip.SlidingExpiration = new TimeSpan (0, 0, 10);
					mc.Add ("long-" + i, i, cip);
				}

				Assert.AreEqual (100, mc.GetCount (), "#CEO2");

				// add shorter lived items into the cache, these should expire first
				for (int i = 0; i < 100; i++) {
					var cip = new CacheItemPolicy ();
					cip.SlidingExpiration = new TimeSpan(0, 0, 1);
					mc.Add ("short-" + i, i, cip);
				}

				Assert.AreEqual (200, mc.GetCount (), "#CEO3");

				global::System.Threading.Thread.Sleep (4 * 1000);

				for (int i = 0; i < 100; i++) {
					Assert.IsNull (mc.Get ("short-" + i), "#CEO4-" + i);
				}
				Assert.AreEqual (100, mc.GetCount (), "#CEO4");
			}
		}

		[Test]
		public void TestCacheSliding ()
		{    
			var config = new NameValueCollection ();
			config["cacheMemoryLimitMegabytes"] = 0.ToString ();
			config["physicalMemoryLimitPercentage"] = 100.ToString ();
			config["pollingInterval"] = new TimeSpan (0, 0, 1).ToString ();

			using (var mc = new MemoryCache ("TestCacheSliding",  config)) {
				Assert.AreEqual (0, mc.GetCount (), "#CSL1");

				var cip = new CacheItemPolicy();
				// The sliding expiration timeout has to be greater than 1 second because
				// .NET implementation ignores timeouts updates smaller than
				// CacheExpires.MIN_UPDATE_DELTA which is equal to 1.
				cip.SlidingExpiration = new TimeSpan (0, 0, 2);
				mc.Add("slidingtest", "42", cip);

				mc.Add("expire1", "1", cip);
				mc.Add("expire2", "2", cip);
				mc.Add("expire3", "3", cip);
				mc.Add("expire4", "4", cip);
				mc.Add("expire5", "5", cip);

				Assert.AreEqual (6, mc.GetCount (), "#CSL2");

				for (int i = 0; i < 50; i++) {
					global::System.Threading.Thread.Sleep (100);

					var item = mc.Get ("slidingtest");
					Assert.AreNotEqual (null, item, "#CSL3-" + i);
				}

				Assert.IsNull (mc.Get ("expire1"), "#CSL4-1");
				Assert.IsNull (mc.Get ("expire2"), "#CSL4-2");
				Assert.IsNull (mc.Get ("expire3"), "#CSL4-3");
				Assert.IsNull (mc.Get ("expire4"), "#CSL4-4");
				Assert.IsNull (mc.Get ("expire5"), "#CSL4-5");
				Assert.AreEqual (1, mc.GetCount (), "#CSL4");

				global::System.Threading.Thread.Sleep (4 * 1000);

				Assert.IsNull (mc.Get ("slidingtest"), "#CSL5a");
				Assert.AreEqual (0, mc.GetCount (), "#CSL5");
			}
		}
	}
}
