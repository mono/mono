using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Test
{
	public class InMemoryProvider : OutputCacheProvider
	{
		Dictionary<string, object> cache = new Dictionary<string, object> ();

		public override object Add (string key, object entry, DateTime utcExpiry)
		{
			object value;

			if (cache.TryGetValue (key, out value))
				return value;

			cache.Add (key, entry);
			return entry;
		}

		public override object Get (string key)
		{
			object ret;

			if (cache.TryGetValue (key, out ret))
				return ret;

			return null;
		}

		public override void Remove (string key)
		{
			if (cache.ContainsKey (key))
				cache.Remove (key);
		}

		public override void Set (string key, object entry, DateTime utcExpiry)
		{
			if (cache.ContainsKey (key))
				cache[key] = entry;
			else
				cache.Add (key, entry);
		}
	}
}