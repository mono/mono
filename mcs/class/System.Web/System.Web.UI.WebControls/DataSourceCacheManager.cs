//
// System.Web.UI.WebControls.DataSourceCacheManager
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// Mainsoft (www.mainsoft.com)
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
using System.Web;
using System.Web.UI;
using System.Web.Caching;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI.WebControls
{
	internal class DataSourceCacheManager
	{
		readonly int cacheDuration;
		readonly string cacheKeyDependency;
		readonly string controlID;
		readonly DataSourceCacheExpiry cacheExpirationPolicy;
		readonly Control owner;
		readonly HttpContext context;

		internal DataSourceCacheManager (int cacheDuration, string cacheKeyDependency,
			DataSourceCacheExpiry cacheExpirationPolicy, Control owner, HttpContext context)
		{
			this.cacheDuration = cacheDuration;
			this.cacheKeyDependency = cacheKeyDependency;
			this.cacheExpirationPolicy = cacheExpirationPolicy;
			this.controlID = owner.UniqueID;
			this.owner = owner;
			this.context = context;

			if (DataCache [controlID] == null)
				DataCache [controlID] = new object ();
		}

		internal void Expire ()
		{
			DataCache [controlID] = new object ();
		}

		internal object GetCachedObject (string methodName, ParameterCollection parameters)
		{
			return DataCache [GetKeyFromParameters (methodName, parameters)];
		}

		internal void SetCachedObject (string methodName, ParameterCollection parameters, object o)
		{
			if (o == null)
				return;

			string key = GetKeyFromParameters (methodName, parameters);

			if (DataCache [key] != null)
				DataCache.Remove (key);

			DateTime absoluteExpiration = Cache.NoAbsoluteExpiration;
			TimeSpan slidindExpiraion = Cache.NoSlidingExpiration;

			if (cacheDuration > 0) {
				if (cacheExpirationPolicy == DataSourceCacheExpiry.Absolute)
					absoluteExpiration = DateTime.Now.AddSeconds (cacheDuration);
				else
					slidindExpiraion = new TimeSpan (0, 0, cacheDuration);
			}

			string [] dependencies;
			if (cacheKeyDependency.Length > 0)
				dependencies = new string [] { cacheKeyDependency };
			else
				dependencies = new string [] { };

			DataCache.Add (key, o, new CacheDependency (new string [] { }, dependencies),
				       absoluteExpiration, slidindExpiraion, CacheItemPriority.Default, null);
		}

		static Cache DataCache {
			get {
				if (HttpContext.Current != null)
					return HttpContext.Current.InternalCache;

				throw new InvalidOperationException ("HttpContext.Current is null.");
			}
		}

		string GetKeyFromParameters (string methodName, ParameterCollection parameters)
		{
			StringBuilder sb = new StringBuilder (methodName);

			if (owner != null)
				sb.Append (owner.ID);

			for (int i = 0; i < parameters.Count; i++) {
				sb.Append (parameters [i].Name);
				sb.Append (parameters [i].GetValue (context, owner));
			}

			return sb.ToString ();
		}
	}
}

