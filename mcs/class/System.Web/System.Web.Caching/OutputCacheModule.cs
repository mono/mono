//
// System.Web.Caching.OutputCacheModule
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Web;
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
			app.AddOnResolveRequestCacheAsync (
				new BeginEventHandler (OnBeginRequestCache),
				new EndEventHandler (OnEndRequestCache));

			app.AddOnUpdateRequestCacheAsync (
				new BeginEventHandler (OnBeginUpdateCache),
				new EndEventHandler (OnEndUpdateCache));
 
			response_removed = new CacheItemRemovedCallback (OnRawResponseRemoved);
		}

		IAsyncResult OnBeginRequestCache (object o, EventArgs args, AsyncCallback cb, object data)
		{
			HttpApplication app = (HttpApplication) o;
			HttpContext context = app.Context;
			
			string vary_key = context.Request.FilePath;
			CachedVaryBy varyby = context.Cache [vary_key] as CachedVaryBy;
			string key;
			CachedRawResponse c;

			if (varyby == null)
				goto leave;

			key = varyby.CreateKey (vary_key, context);
			c = context.Cache [key] as CachedRawResponse;
			
			if (c != null) {
				
				context.Response.ClearContent ();
				context.Response.BinaryWrite (c.GetData (), 0, c.ContentLength);

				context.Response.ClearHeaders ();
				c.DateHeader.Value = TimeUtil.ToUtcTimeString (DateTime.Now);
				context.Response.SetCachedHeaders (c.Headers);

				context.Response.StatusCode = c.StatusCode;
				context.Response.StatusDescription = c.StatusDescription;
				
				app.CompleteRequest ();
			} else if (c != null) {
				context.Cache.Remove (key);
			}

		leave:
			HttpAsyncResult result = new HttpAsyncResult (cb,this);
			result.Complete (true, o, null);
			
			return result;
		}

		void OnEndRequestCache (IAsyncResult result)
		{
		}

		IAsyncResult OnBeginUpdateCache (object o, EventArgs args, AsyncCallback cb, object data)
		{
			HttpApplication app = (HttpApplication) o;
			HttpContext context = app.Context;
			HttpAsyncResult result;

			if (context.Response.IsCached && context.Response.StatusCode == 200 && 
			    !context.Trace.IsEnabled)
				DoCacheInsert (context);

			result = new HttpAsyncResult (cb, this);
			result.Complete (true, o, null);
			return result;
		}

		void OnEndUpdateCache (IAsyncResult result)
		{
		}

		private void DoCacheInsert (HttpContext context)
		{
			string vary_key = context.Request.FilePath;
			string key;
			CachedVaryBy varyby = context.Cache [vary_key] as CachedVaryBy;
			CachedRawResponse prev = null;
			bool lookup = true;
			
			if (varyby == null) {
				varyby = new CachedVaryBy (context.Response.Cache, vary_key);
				context.Cache.InsertPrivate (vary_key, varyby, null,
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
				string [] files = new string [0];
				string [] keys = new string [] { vary_key };
				bool sliding = context.Response.Cache.Sliding;

				context.Cache.InsertPrivate (key, c, new CacheDependency (files, keys),
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

