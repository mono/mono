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

using System.Collections;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Util;
using System.Web.Compilation;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Web.Caching {
	
	internal sealed class OutputCacheModule : IHttpModule
	{
		CacheItemRemovedCallback response_removed;
		
#if NET_2_0
		static object keysCacheLock = new object ();
		Dictionary <string, string> keysCache;
		Dictionary <string, string> entriesToInvalidate;
#endif
		
		public OutputCacheModule ()
		{
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

#if NET_2_0
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

			context.Cache.Remove (entry);
			if (!String.IsNullOrEmpty (cacheValue))
				context.InternalCache.Remove (cacheValue);
		}
#endif

		void OnResolveRequestCache (object o, EventArgs args)
		{
			HttpApplication app = (HttpApplication) o;
			HttpContext context = app.Context;
			
			string vary_key = context.Request.FilePath;
			CachedVaryBy varyby = context.Cache [vary_key] as CachedVaryBy;
			string key;
			CachedRawResponse c;

			if (varyby == null)
				return;

			key = varyby.CreateKey (vary_key, context);
			c = context.InternalCache [key] as CachedRawResponse;
			if (c == null)
				return;

#if NET_2_0
			lock (keysCacheLock) {
				string invValue;
				if (entriesToInvalidate != null && entriesToInvalidate.TryGetValue (vary_key, out invValue) && String.Compare (invValue, key, StringComparison.Ordinal) == 0) {
					context.Cache.Remove (vary_key);
					context.InternalCache.Remove (key);
					entriesToInvalidate.Remove (vary_key);
					return;
				}
			}
#endif
			
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
				} else if (isIgnored) {
					return;
				}
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

#if NET_2_0
					if (d.Callback == null)
						continue;

					string s = d.Callback (context);
					if (s == null || s.Length == 0)
						continue;

					byte[] bytes = outEnc.GetBytes (s);
					response.BinaryWrite (bytes, 0, bytes.Length);
#endif
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
			HttpApplication app = (HttpApplication) o;
			HttpContext context = app.Context;

			if (context.Response.IsCached && context.Response.StatusCode == 200 && 
			    !context.Trace.IsEnabled)
				DoCacheInsert (context);
		}

		void DoCacheInsert (HttpContext context)
		{
			string vary_key = context.Request.FilePath;
			string key;
			CachedVaryBy varyby = context.Cache [vary_key] as CachedVaryBy;
			CachedRawResponse prev = null;
			bool lookup = true;
#if NET_2_0
			string cacheKey = null, cacheValue = null;
#endif
			
			if (varyby == null) {
				string path = context.Request.MapPath (vary_key);
				string [] files = new string [] { path };
				string [] keys = new string [0];
				varyby = new CachedVaryBy (context.Response.Cache, vary_key);
				context.Cache.Insert (vary_key, varyby,
							      new CacheDependency (files, keys),
							      Cache.NoAbsoluteExpiration,
							      Cache.NoSlidingExpiration,
							      CacheItemPriority.Normal, null);
				lookup = false;
#if NET_2_0
				cacheKey = vary_key;
#endif
			} 

			key = varyby.CreateKey (vary_key, context);

			if (lookup)
				prev = context.InternalCache [key] as CachedRawResponse;
			
			if (prev == null) {
				CachedRawResponse c = context.Response.GetCachedResponse ();
				if (c != null) {
					string [] files = new string [] { };
					string [] keys = new string [] { vary_key };
					bool sliding = context.Response.Cache.Sliding;

					context.InternalCache.Insert (key, c, new CacheDependency (files, keys),
								      (sliding ? Cache.NoAbsoluteExpiration :
								       context.Response.Cache.Expires),
								      (sliding ? TimeSpan.FromSeconds (
									      context.Response.Cache.Duration) :
								       Cache.NoSlidingExpiration),
								      CacheItemPriority.Normal, response_removed);
					c.VaryBy = varyby;
					varyby.ItemList.Add (key);
#if NET_2_0
					cacheValue = key;
#endif
				}
			}
			
#if NET_2_0
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
#endif
		}

		static void OnRawResponseRemoved (string key, object value, CacheItemRemovedReason reason)
		{
			CachedRawResponse c = (CachedRawResponse) value;

			c.VaryBy.ItemList.Remove (key);			
			if (c.VaryBy.ItemList.Count != 0)
				return;			

			HttpRuntime.Cache.Remove (c.VaryBy.Key);
		}
	}
}

