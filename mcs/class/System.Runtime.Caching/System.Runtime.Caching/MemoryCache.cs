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
using System.Reflection;

namespace System.Runtime.Caching
{
	public class MemoryCache : ObjectCache, IEnumerable, IDisposable
	{
		string name;
			
		public static MemoryCache Default { get; private set; }
		public long CacheMemoryLimit { get; private set; }
		
		public override DefaultCacheCapabilities DefaultCacheCapabilities {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public override object this [string key] {
			[TargetedPatchingOptOut ("Performance critical to inline this type of method across NGen image boundaries")]
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public override string Name {
			[TargetedPatchingOptOut ("Performance critical to inline this type of method across NGen image boundaries")]
			get { return name; }
		}
		
		public long PhysicalMemoryLimit { get; private set; }
		public TimeSpan PollingInterval { get; private set; }

		static MemoryCache ()
		{
			Default = new MemoryCache ();
		}

		MemoryCache ()
		{
			name = "Default";
			GetValuesFromConfig (name, null);
		}
			
		public MemoryCache (string name, NameValueCollection config)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("is an empty string.", "name");

			if (String.Compare (name, "default", StringComparison.OrdinalIgnoreCase) == 0)
				throw new ArgumentException ("'default' is a reserved name.", "name");

			GetValuesFromConfig (name, config);
		}

		bool ParseConfigValue (string name, string value, out long parsed)
		{
			if (String.IsNullOrEmpty (value)) {
				parsed = -1;
				return false;
			}
				
			try {
				parsed = Int64.Parse (value);
				return true;
			} catch (Exception ex) {
				throw new ArgumentException ("config value could not be parsed.", name, ex);
			}
		}

		void GetValuesFromConfig (string name, NameValueCollection config)
		{
			// TODO: retrieve configuration entries from the config files

			if (config != null) {
				// TODO: check when entries/names are considered to be
				// invalid
				long parsed;

				if (ParseConfigValue ("CacheMemoryLimitMegabytes", config ["CacheMemoryLimitMegabytes"], out parsed))
					CacheMemoryLimit = parsed;

				if (ParseConfigValue ("PhysicalMemoryLimitPercentage", config ["PhysicalMemoryLimitPercentage"], out parsed))
					PhysicalMemoryLimit = parsed;

				string interval = config ["PollingInterval"];
				if (!String.IsNullOrEmpty (interval)) {
					try {
						PollingInterval = TimeSpan.Parse (interval);
					} catch (Exception ex) {
						throw new ArgumentException ("config value could not be parsed.", "PollingInterval", ex);
					}
				}
			}
		}
			
		public override CacheItem AddOrGetExisting (CacheItem item, CacheItemPolicy policy)
		{
			throw new NotImplementedException ();
		}
		
		public override object AddOrGetExisting (string key, object value, CacheItemPolicy policy, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public override object AddOrGetExisting (string key, object value, DateTimeOffset absoluteExpiration, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public override bool Contains (string key, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor (IEnumerable <string> keys, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		
		[TargetedPatchingOptOut ("Performance critical to inline this type of method across NGen image boundaries")]
		public override object Get (string key, string regionName)
		{
			throw new NotImplementedException ();
				
		}
		
		public override CacheItem GetCacheItem (string key, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public override long GetCount (string regionName)
		{
			throw new NotImplementedException ();
		}
		
		protected override IEnumerator <KeyValuePair <string,object>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		public override IDictionary <string,object> GetValues (IEnumerable <string> keys, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public override object Remove (string key, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public override void Set (CacheItem item, CacheItemPolicy policy)
		{
			throw new NotImplementedException ();
		}
		
		public override void Set (string key, object value, CacheItemPolicy policy, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public override void Set (string key, object value, DateTimeOffset absoluteExpiration, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		public long Trim (int percent)
		{
			throw new NotImplementedException ();
		}
	}
}
