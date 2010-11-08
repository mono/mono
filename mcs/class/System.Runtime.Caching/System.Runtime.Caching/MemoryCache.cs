//
// MemoryCache.cs
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
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Caching.Configuration;
using System.Threading;

namespace System.Runtime.Caching
{
	public class MemoryCache : ObjectCache, IEnumerable, IDisposable
	{
		const long DEFAULT_TIMER_PERIOD = 20000; // .NET's default, ms

		static long totalPhysicalMemory;
		static int numCPUs;
		
		string name;
		MemoryCacheContainer[] containers;
		DefaultCacheCapabilities defaultCaps;
		MemoryCachePerformanceCounters perfCounters;
		bool noPerformanceCounters;
		bool emulateOneCPU;
		
		static ulong TotalPhysicalMemory {
			get {
				if (totalPhysicalMemory == 0)
					DetermineTotalPhysicalMemory ();

				return (ulong)totalPhysicalMemory;
			}
		}

		internal long TimerPeriod {
			get; private set;
		}
		
		public static MemoryCache Default { get; private set; }

		// LAMESPEC: this value is represented in bytes, not megabytes
		public long CacheMemoryLimit { get; private set; }
		
		public override DefaultCacheCapabilities DefaultCacheCapabilities {
			get { return defaultCaps; }
		}
		
		public override object this [string key] {
			get { return FindContainer (key).Get (key); }
			set { Set (key, value, ObjectCache.InfiniteAbsoluteExpiration); }
		}
		
		public override string Name {
			get { return name; }
		}
		
		public long PhysicalMemoryLimit { get; private set; }
		public TimeSpan PollingInterval { get; private set; }

		static MemoryCache ()
		{
			numCPUs = Environment.ProcessorCount;
			Default = new MemoryCache ();
		}

		MemoryCache ()
		{
			name = "Default";
			CommonInit (name);
		}
			
		public MemoryCache (string name, NameValueCollection config = null)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("is an empty string.", "name");

			if (String.Compare (name, "default", StringComparison.OrdinalIgnoreCase) == 0)
				throw new ArgumentException ("'default' is a reserved name.", "name");
			
			this.name = name;
			CommonInit (name, config);
		}

		void CommonInit (string name, NameValueCollection config = null)
		{
			this.defaultCaps = DefaultCacheCapabilities.InMemoryProvider |
				DefaultCacheCapabilities.CacheEntryChangeMonitors |
				DefaultCacheCapabilities.AbsoluteExpirations |
				DefaultCacheCapabilities.SlidingExpirations |
				DefaultCacheCapabilities.CacheEntryRemovedCallback |
				DefaultCacheCapabilities.CacheEntryUpdateCallback;

			GetValuesFromConfig (name, config);
			containers = new MemoryCacheContainer [numCPUs];
			perfCounters = new MemoryCachePerformanceCounters (name.ToLowerInvariant (), noPerformanceCounters);
		}
		
		static void DetermineTotalPhysicalMemory ()
		{
			var pc = new PerformanceCounter ("Mono Memory", "Total Physical Memory");
			long memBytes = pc.RawValue;

			if (memBytes == 0)
				memBytes = 134217728L; // 128MB, the runtime default when it's
						       // impossible to determine the physical
						       // memory size

			Interlocked.CompareExchange (ref totalPhysicalMemory, memBytes, 0);
		}

		bool ParseBoolConfigValue (string paramName, string name, NameValueCollection config, bool doTrow)
		{
			string value = config [name];
			if (String.IsNullOrEmpty (value))
				return false;

			try {
				return Boolean.Parse (value);
			} catch {
				return false;
			}
		}
		
		bool ParseInt32ConfigValue (string paramName, string name, NameValueCollection config, int maxValue, bool doThrow,  out int parsed)
		{
			parsed = -1;
			string value = config [name];
			if (String.IsNullOrEmpty (value))
				return false;
				
			try {
				parsed = (int)UInt32.Parse (value);
			} catch (Exception ex) {
				if (doThrow)
					throw new ArgumentException (
						String.Format ("Invalid configuration: {0}=\"{1}\". The {0} value must be a non-negative 32-bit integer", name, value),
						paramName,
						ex);
				return false;
			}

			if (parsed < 0 || (uint)parsed > (uint)maxValue) {
				if (doThrow)
					throw new ArgumentException (
						String.Format ("Invalid configuration: {0}=\"{1}\". The {0} value cannot be greater than '{2}'.", name, value, maxValue),
						paramName);
				return false;
			}
			
			return true;
		}
		
		bool ParseTimeSpanConfigValue (string paramName, string name, NameValueCollection config, out TimeSpan parsed)
		{
			string value = config [name];
			if (String.IsNullOrEmpty (value)) {
				parsed = TimeSpan.MinValue;
				return false;
			}
			
			try {
				parsed = TimeSpan.Parse (value);
				return true;
			} catch (Exception ex) {
				throw new ArgumentException (
					String.Format ("Invalid configuration: {0}=\"{1}\". The {0} value must be a time interval that can be parsed by System.TimeSpan.Parse", name, value),
					paramName,
					ex);
			}
		}
		
		void GetValuesFromConfig (string name, NameValueCollection config)
		{
			var mcs = ConfigurationManager.GetSection ("system.runtime.caching/memoryCache") as MemoryCacheSection;
			MemoryCacheSettingsCollection settings = mcs != null ? mcs.NamedCaches : null;
			MemoryCacheElement element = settings != null ? settings [name] : null;

			if (element != null && config == null) {
				CacheMemoryLimit = (long)element.CacheMemoryLimitMegabytes * 1048576L;
				PhysicalMemoryLimit = (long)element.PhysicalMemoryLimitPercentage;
				PollingInterval = element.PollingInterval;
			}
			
			if (config != null) {
				int parsed;

				if (ParseInt32ConfigValue ("config", "cacheMemoryLimitMegabytes", config, Int32.MaxValue, true, out parsed))
					CacheMemoryLimit = parsed * 1048576L;
				else if (element != null)
					CacheMemoryLimit = (long)element.CacheMemoryLimitMegabytes * 1048576L;

				if (ParseInt32ConfigValue ("config", "physicalMemoryLimitPercentage", config, 100, true, out parsed))
					PhysicalMemoryLimit = parsed;
				else if (element != null)
					PhysicalMemoryLimit = (long)element.PhysicalMemoryLimitPercentage;

				TimeSpan ts;
				if (ParseTimeSpanConfigValue ("config", "pollingInterval", config, out ts))
					PollingInterval = ts;
				else if (element != null)
					PollingInterval = element.PollingInterval;

				// Those are Mono-specific
				if (!String.IsNullOrEmpty (config ["__MonoDisablePerformanceCounters"]))
					noPerformanceCounters = true;

				if (ParseInt32ConfigValue ("config", "__MonoTimerPeriod", config, Int32.MaxValue, false, out parsed))
					TimerPeriod = (long)(parsed * 1000);
				else
					TimerPeriod = DEFAULT_TIMER_PERIOD;

				if (ParseBoolConfigValue ("config", "__MonoEmulateOneCPU", config, false))
					emulateOneCPU = true;
			} else
				TimerPeriod = DEFAULT_TIMER_PERIOD;

			if (CacheMemoryLimit == 0) {
				// Calculated using algorithm described in this blog entry:
				//
				//  http://blogs.msdn.com/tmarq/archive/2007/06/25/some-history-on-the-asp-net-cache-memory-limits.aspx
				//
				ulong physicalRam = TotalPhysicalMemory;
				ulong maxCacheSize;
				
				// Determine the upper bound
				if (Helpers.Is64Bit)
					maxCacheSize = 0x10000000000UL; // 1TB
				else if (physicalRam > 0x80000000UL) // 2GB
					maxCacheSize = 0x70800000UL; // 1800MB
				else
					maxCacheSize = 0x32000000UL; // 800MB

				physicalRam = (physicalRam * 3) / 5; // 60%
				CacheMemoryLimit = (long)Math.Min (physicalRam, maxCacheSize);
			}

			if (PhysicalMemoryLimit == 0)
				PhysicalMemoryLimit = 98;

			if (PollingInterval == TimeSpan.Zero)
				PollingInterval = TimeSpan.FromMinutes (2);
		}
			
		public override CacheItem AddOrGetExisting (CacheItem item, CacheItemPolicy policy)
		{
			if (item == null)
				throw new ArgumentNullException ("item");

			string key = item.Key;
			return new CacheItem (key, DoAddOrGetExisting (key, item.Value, policy));
		}
		
		public override object AddOrGetExisting (string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");
			
			return DoAddOrGetExisting (key, value, policy, regionName);
		}
		
		public override object AddOrGetExisting (string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");
			
			var policy = new CacheItemPolicy ();
			policy.AbsoluteExpiration = absoluteExpiration;

			return DoAddOrGetExisting (key, value, policy, regionName);
		}

		object DoAddOrGetExisting (string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			if (value == null)
				throw new ArgumentNullException ("value");
			
			if (policy != null) {
				ValidatePolicy (policy, false);
				if (policy.AbsoluteExpiration < DateTimeOffset.Now)
					return null;
			}
			
			return FindContainer (key).AddOrGetExisting (key, value, policy);
		}
		
		public override bool Contains (string key, string regionName = null)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");
			
			return FindContainer (key).ContainsKey (key);
		}
		
		public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor (IEnumerable <string> keys, string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");

			if (keys == null)
				throw new ArgumentNullException ("keys");

			int count = 0;
			foreach (string key in keys) {
				if (key == null)
					throw new ArgumentException ("The collection 'keys' contains a null element.");
				count++;
			}
			if (count == 0)
				throw new ArgumentException ("The collection 'keys' is empty");
			
			return new MemoryCacheEntryChangeMonitor (this, keys);
		}
		
		public void Dispose ()
		{
			foreach (MemoryCacheContainer container in containers) {
				if (container == null)
					continue;

				container.Dispose ();
			}

			perfCounters.Dispose ();
		}

		MemoryCacheContainer FindContainer (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			if (emulateOneCPU || numCPUs == 1) {
				if (containers [0] == null)
					containers [0] = new MemoryCacheContainer (this, 0, perfCounters);

				return containers [0];
			}
			
			int containerIdx = Math.Abs (key.GetHashCode () % numCPUs);
			if (containers [containerIdx] == null)
				containers [containerIdx] = new MemoryCacheContainer (this, containerIdx, perfCounters);

			return containers [containerIdx];
		}
		
		public override object Get (string key, string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");

			if (key == null)
				throw new ArgumentNullException ("key");
			
			return FindContainer (key).Get (key);
		}

		internal MemoryCacheEntry GetEntry (string key)
		{
			return FindContainer (key).GetEntry (key);
		}
		
		public override CacheItem GetCacheItem (string key, string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");

			if (key == null)
				throw new ArgumentNullException ("key");
			
			object value = Get (key);
			if (value == null)
				return null;
			
			return new CacheItem (key, value);
		}
		
		public override long GetCount (string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");
			
			long ret = 0;
			MemoryCacheContainer container;
			for (int i = 0; i < numCPUs; i++) {
				container = containers [i];
				if (container == null)
					continue;

				ret += container.Count;
			}

			return ret;
		}
		
		protected override IEnumerator <KeyValuePair <string,object>> GetEnumerator ()
		{
			var dict = new Dictionary <string, object> ();

			CopyEntries (dict);
			return dict.GetEnumerator ();
		}
		
		public override IDictionary <string,object> GetValues (IEnumerable <string> keys, string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");

			if (keys == null)
				throw new ArgumentNullException ("keys");

			MemoryCacheEntry entry;
			var ret = new Dictionary <string, object> ();
			foreach (string key in keys) {
				if (key == null)
					throw new ArgumentException ("The collection 'keys' contains a null element.");

				entry = GetEntry (key);
				
				// LAMESPEC: MSDN says the number of items in the returned dictionary should be the same as in the 
				// 'keys' collection - this is not the case. The returned dictionary contains only entries for keys
				// that exist in the cache.
				if (entry == null)
					continue;

				ret.Add (key, entry.Value);
			}

			if (ret.Count == 0)
				return null;
			
			return ret;
		}
		
		public override object Remove (string key, string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");

			if (key == null)
				throw new ArgumentNullException ("key");
			
			return FindContainer (key).Remove (key);
		}

		internal void Remove (MemoryCacheEntry entry)
		{
			if (entry == null)
				return;

			string key = entry.Key;
			FindContainer (key).Remove (key);
		}
		
		public override void Set (CacheItem item, CacheItemPolicy policy)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			
			Set (item.Key, item.Value, policy);
		}
		
		public override void Set (string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			if (regionName != null)
				throw new NotSupportedException ("The parameter regionName must be null.");

			if (key == null)
				throw new ArgumentNullException ("key");

			if (value == null)
				throw new ArgumentNullException ("value");

			if (policy != null) {
				ValidatePolicy (policy, true);
				if (policy.AbsoluteExpiration < DateTimeOffset.Now)
					return;
			}
			
			FindContainer (key).Set (key, value, policy);
		}
		
		public override void Set (string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			var policy = new CacheItemPolicy ();
			policy.AbsoluteExpiration = absoluteExpiration;

			Set (key, value, policy, regionName);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			// As weird as it is, this method doesn't call the instance GetEnumerator ()
			// - tests show it returns a Hashtable enumerator for some reason.
			var dict = new Hashtable ();
			
			CopyEntries (dict);
			return dict.GetEnumerator ();
		}

		void CopyEntries (IDictionary dict)
		{
			foreach (MemoryCacheContainer container in containers) {
				if (container == null)
					continue;

				container.CopyEntries (dict);
			}
		}
		
		public long Trim (int percent)
		{
			long ret = 0;

			// We should probably sort the containers by their least recently used
			// items, but that is a performance overkill so we'll just resort to a more
			// naive method - each container is trimmed independently of the others.
			foreach (MemoryCacheContainer container in containers) {
				if (container == null)
					continue;

				ret += container.Trim (percent);
			}

			return ret;
		}

		void ValidatePolicy (CacheItemPolicy policy, bool allowUpdateCallback)
		{
			CacheEntryUpdateCallback updateCallback = policy.UpdateCallback;
			if (!allowUpdateCallback && updateCallback != null)
				throw new ArgumentException ("CacheItemPolicy.UpdateCallback must be null.", "policy");

			if (updateCallback != null && policy.RemovedCallback != null)
				throw new ArgumentException ("Only one callback can be specified. Either RemovedCallback or UpdateCallback must be null.", "policy");
			
			DateTimeOffset absoluteExpiration = policy.AbsoluteExpiration;
			TimeSpan slidingExpiration = policy.SlidingExpiration;
				
			if (absoluteExpiration != ObjectCache.InfiniteAbsoluteExpiration &&
			    slidingExpiration != TimeSpan.Zero)
				throw new ArgumentException (
					"policy",
					"'AbsoluteExpiration' must be ObjectCache.InfiniteAbsoluteExpiration or 'SlidingExpiration' must be TimeSpan.Zero"
				);

			long ticks = slidingExpiration.Ticks;
			if (ticks < 0 || ticks > 315360000000000)
				throw new ArgumentOutOfRangeException (
					"policy",
					"SlidingExpiration must be greater than or equal to '00:00:00' and less than or equal to '365.00:00:00'."
				);
				
			CacheItemPriority priority = policy.Priority;
			if (priority < CacheItemPriority.Default || priority > CacheItemPriority.NotRemovable)
				throw new ArgumentOutOfRangeException (
					"policy",
					"'Priority' must be greater than or equal to 'Default' and less than or equal to 'NotRemovable'"
				);
		}		
	}
}
