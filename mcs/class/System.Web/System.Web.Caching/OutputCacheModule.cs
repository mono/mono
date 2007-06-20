//
// System.Web.Caching.OutputCacheModule
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Util;
using System.Collections;

namespace System.Web.Caching {
	
	internal sealed class OutputCacheModule : IHttpModule {

		private CacheItemRemovedCallback response_removed;
		
		public OutputCacheModule ()
		{
		}

		public void Dispose ()
		{
		}

		public void Init (HttpApplication app)
		{
			app.ResolveRequestCache += new EventHandler(OnResolveRequestCache);
			app.UpdateRequestCache += new EventHandler(OnUpdateRequestCache);
 
			response_removed = new CacheItemRemovedCallback (OnRawResponseRemoved);
		}

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
			c = context.Cache [key] as CachedRawResponse;

			if (c == null)
				return;

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
			
			context.Response.ClearContent ();
			context.Response.BinaryWrite (c.GetData (), 0, c.ContentLength);

			context.Response.ClearHeaders ();
			c.DateHeader.Value = TimeUtil.ToUtcTimeString (DateTime.Now);
			context.Response.SetCachedHeaders (c.Headers);

			context.Response.StatusCode = c.StatusCode;
			context.Response.StatusDescription = c.StatusDescription;
				
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

		private void DoCacheInsert (HttpContext context)
		{
			string vary_key = context.Request.FilePath;
			string key;
			CachedVaryBy varyby = context.Cache [vary_key] as CachedVaryBy;
			CachedRawResponse prev = null;
			bool lookup = true;
			
			if (varyby == null) {
				string path = context.Request.MapPath (vary_key);
				string [] files = new string [] { path };
				string [] keys = new string [0];
				varyby = new CachedVaryBy (context.Response.Cache, vary_key);
				context.InternalCache.Insert (vary_key, varyby,
							      new CacheDependency (files, keys),
							      Cache.NoAbsoluteExpiration,
							      Cache.NoSlidingExpiration,
							      CacheItemPriority.Normal, null);
				lookup = false;
			} 
			
			key = varyby.CreateKey (vary_key, context);

			if (lookup)
				prev = context.Cache [key] as CachedRawResponse;
			
			if (prev == null) {
				CachedRawResponse c = context.Response.GetCachedResponse ();
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
			} 
		}

		private void OnRawResponseRemoved (string key, object value, CacheItemRemovedReason reason)
		{
			CachedRawResponse c = (CachedRawResponse) value;

			c.VaryBy.ItemList.Remove (key);			
			if (c.VaryBy.ItemList.Count != 0)
				return;
			
			Cache cache = HttpRuntime.Cache;
			cache.Remove (c.VaryBy.Key);
		}
	}
}

