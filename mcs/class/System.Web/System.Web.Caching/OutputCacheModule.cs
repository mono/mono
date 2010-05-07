//
// System.Web.Caching.OutputCacheModule
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//  Marek Habersack <mhabersack@novell.com>
//
// (C) 2003-2009 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.Util;
using System.Web.Compilation;

namespace System.Web.Caching
{	
	sealed class OutputCacheModule : IHttpModule
	{
		OutputCacheProvider provider;
		CacheItemRemovedCallback response_removed;
		static object keysCacheLock = new object ();
		Dictionary <string, string> keysCache;
		Dictionary <string, string> entriesToInvalidate;
#if !NET_4_0
		internal OutputCacheProvider InternalProvider {
			get { return provider; }
		}
#endif
		public OutputCacheModule ()
		{
		}

		OutputCacheProvider FindCacheProvider (HttpApplication app)
		{				
#if NET_4_0
			HttpContext ctx = HttpContext.Current;
			if (app == null) {
				app = ctx != null ? ctx.ApplicationInstance : null;

				if (app == null)
					throw new InvalidOperationException ("Unable to find output cache provider.");
			}

			string providerName = app.GetOutputCacheProviderName (ctx);
			if (String.IsNullOrEmpty (providerName))
				throw new ProviderException ("Invalid OutputCacheProvider name. Name must not be null or an empty string.");
			
			if (String.Compare (providerName, OutputCache.DEFAULT_PROVIDER_NAME, StringComparison.Ordinal) == 0) {
				if (provider == null)
					provider = new InMemoryOutputCacheProvider ();
				return provider;
			}

			OutputCacheProviderCollection providers = OutputCache.Providers;
			OutputCacheProvider ret = providers != null ? providers [providerName] : null;

			if (ret == null)
				throw new ProviderException (String.Format ("OutputCacheProvider named '{0}' cannot be found.", providerName));

			return ret;
#else
			if (provider == null)
				provider = new InMemoryOutputCacheProvider ();
			
			return provider;
#endif
		}
		
		public void Dispose ()
		{
		}

		public void Init (HttpApplication context)
		{
			context.ResolveRequestCache += new EventHandler(OnResolveRequestCache);
			context.UpdateRequestCache += new EventHandler(OnUpdateRequestCache);
			response_removed = new CacheItemRemovedCallback (OnRawResponseRemoved);
		}

		void OnBuildManagerRemoveEntry (BuildManagerRemoveEntryEventArgs args)
		{
			string entry = args.EntryName;
			HttpContext context = args.Context;
			string cacheValue;

			lock (keysCacheLock) {
				if (!keysCache.TryGetValue (entry, out cacheValue))
					return;

				keysCache.Remove (entry);
				if (context == null) {
					if (entriesToInvalidate == null) {
						entriesToInvalidate = new Dictionary <string, string> (StringComparer.Ordinal);
						entriesToInvalidate.Add (entry, cacheValue);
						return;
					} else if (!entriesToInvalidate.ContainsKey (entry)) {
						entriesToInvalidate.Add (entry, cacheValue);
						return;
					}
				}
			}

			OutputCacheProvider provider = FindCacheProvider (context != null ? context.ApplicationInstance : null);
			provider.Remove (entry);
			if (!String.IsNullOrEmpty (cacheValue))
				provider.Remove (cacheValue);
		}

		void OnResolveRequestCache (object o, EventArgs args)
		{
			HttpApplication app = o as HttpApplication;
			HttpContext context = app != null ? app.Context : null;

			if (context == null)
				return;

			OutputCacheProvider provider = FindCacheProvider (app);
			string vary_key = context.Request.FilePath;
			CachedVaryBy varyby = provider.Get (vary_key) as CachedVaryBy;
			string key;
			CachedRawResponse c;

			if (varyby == null)
				return;

			key = varyby.CreateKey (vary_key, context);
			c = provider.Get (key) as CachedRawResponse;
			if (c == null)
				return;

			lock (keysCacheLock) {
				string invValue;
				if (entriesToInvalidate != null && entriesToInvalidate.TryGetValue (vary_key, out invValue) && String.Compare (invValue, key, StringComparison.Ordinal) == 0) {
					provider.Remove (vary_key);
					provider.Remove (key);
					entriesToInvalidate.Remove (vary_key);
					return;
				}
			}
			
			ArrayList callbacks = c.Policy.ValidationCallbacks;
			if (callbacks != null && callbacks.Count > 0) {
				bool isValid = true;
				bool isIgnored = false;

				foreach (Pair p in callbacks) {
					HttpCacheValidateHandler validate = (HttpCacheValidateHandler)p.First;
					object data = p.Second;
					HttpValidationStatus status = HttpValidationStatus.Valid;

					try {
						validate (context, data, ref status);
					} catch {
						// MS.NET hides the exception
						isValid = false;
						break;
					}

					if (status == HttpValidationStatus.Invalid) {
						isValid = false;
						break;
					} else if (status == HttpValidationStatus.IgnoreThisRequest) {
						isIgnored = true;
					}
				}

				if (!isValid) {
					OnRawResponseRemoved (key, c, CacheItemRemovedReason.Removed);
					return;
				} else if (isIgnored)
					return;
			}

			HttpResponse response = context.Response;			
			response.ClearContent ();
			IList cachedData = c.GetData ();
			if (cachedData != null) {
				Encoding outEnc = WebEncoding.ResponseEncoding;
				
				foreach (CachedRawResponse.DataItem d in cachedData) {
					if (d.Length > 0) {
						response.BinaryWrite (d.Buffer, 0, (int)d.Length);
						continue;
					}

					if (d.Callback == null)
						continue;

					string s = d.Callback (context);
					if (s == null || s.Length == 0)
						continue;

					byte[] bytes = outEnc.GetBytes (s);
					response.BinaryWrite (bytes, 0, bytes.Length);
				}
			}
			
			response.ClearHeaders ();
			response.SetCachedHeaders (c.Headers);
			response.StatusCode = c.StatusCode;
			response.StatusDescription = c.StatusDescription;
				
			app.CompleteRequest ();
		}

		void OnUpdateRequestCache (object o, EventArgs args)
		{
			HttpApplication app = o as HttpApplication;
			HttpContext context = app != null ? app.Context : null;
			HttpResponse response = context != null ? context.Response : null;
			
			if (response != null && response.IsCached && response.StatusCode == 200 && !context.Trace.IsEnabled)
				DoCacheInsert (context, app, response);
		}

		void DoCacheInsert (HttpContext context, HttpApplication app, HttpResponse response)
		{
			string vary_key = context.Request.FilePath;
			string key;
			OutputCacheProvider provider = FindCacheProvider (app);
			CachedVaryBy varyby = provider.Get (vary_key) as CachedVaryBy;
			CachedRawResponse prev = null;
			bool lookup = true;
			string cacheKey = null, cacheValue = null;
			HttpCachePolicy cachePolicy = response.Cache;
			
			if (varyby == null) {
				varyby = new CachedVaryBy (cachePolicy, vary_key);
				provider.Add (vary_key, varyby, Cache.NoAbsoluteExpiration);
				lookup = false;
				cacheKey = vary_key;
			} 

			key = varyby.CreateKey (vary_key, context);

			if (lookup)
				prev = provider.Get (key) as CachedRawResponse;
			
			if (prev == null) {
				CachedRawResponse c = response.GetCachedResponse ();
				if (c != null) {
					string [] keys = new string [] { vary_key };
					DateTime utcExpiry, absoluteExpiration;
					TimeSpan slidingExpiration;

					c.VaryBy = varyby;
					varyby.ItemList.Add (key);

					if (cachePolicy.Sliding) {
						slidingExpiration = TimeSpan.FromSeconds (cachePolicy.Duration);
						absoluteExpiration = Cache.NoAbsoluteExpiration;
						utcExpiry = DateTime.UtcNow + slidingExpiration;
					} else {
						slidingExpiration = Cache.NoSlidingExpiration;
						absoluteExpiration = cachePolicy.Expires;
						utcExpiry = absoluteExpiration.ToUniversalTime ();
					}

					provider.Set (key, c, utcExpiry);
					HttpRuntime.InternalCache.Insert (key, c, new CacheDependency (null, keys), absoluteExpiration, slidingExpiration,
									  CacheItemPriority.Normal, response_removed);
					cacheValue = key;
				}
			}
			
			if (cacheKey != null) {
				lock (keysCacheLock) {
					if (keysCache == null) {
						BuildManager.RemoveEntry += new BuildManagerRemoveEntryEventHandler (OnBuildManagerRemoveEntry);
						keysCache = new Dictionary <string, string> (StringComparer.Ordinal);
						keysCache.Add (cacheKey, cacheValue);
					} else if (!keysCache.ContainsKey (cacheKey))
						keysCache.Add (cacheKey, cacheValue);
				}
			}
		}

		void OnRawResponseRemoved (string key, object value, CacheItemRemovedReason reason)
		{
			CachedRawResponse c = value as CachedRawResponse;
			CachedVaryBy varyby = c != null ? c.VaryBy : null;
			if (varyby == null)
				return;

			List <string> itemList = varyby.ItemList;
			OutputCacheProvider provider = FindCacheProvider (null);
			
			itemList.Remove (key);
			provider.Remove (key);
			
			if (itemList.Count != 0)
				return;			

			provider.Remove (varyby.Key);
		}
	}
}
