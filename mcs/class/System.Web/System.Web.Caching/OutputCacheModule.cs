//
// System.Web.Caching.OutputCacheModule
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Web;

namespace System.Web.Caching {
	
	internal sealed class OutputCacheModule : IHttpModule {

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
			
			if (c != null && context.Timestamp < c.Policy.Expires) {
				
				context.Response.ClearContent ();
				context.Response.BinaryWrite (c.GetData (), 0, c.ContentLength);

				context.Response.ClearHeaders ();
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

			if (context.Response.IsCached && context.Response.StatusCode == 200)
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
				varyby = new CachedVaryBy (context.Response.Cache);
				context.Cache.InsertPrivate (vary_key, varyby, null,
						Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
						CacheItemPriority.Normal, null);
				lookup = false;
			} 
			
			key = varyby.CreateKey (vary_key, context);

			if (lookup)
				prev = context.Cache [key] as CachedRawResponse;
			
			if (IsExpired (context, prev)) {
				CachedRawResponse c = context.Response.GetCachedResponse ();
				
				context.Cache.InsertPrivate (key, c, null,
						context.Response.Cache.Expires,
						Cache.NoSlidingExpiration,
						CacheItemPriority.Normal, null);
			} 
		}

		private bool IsExpired (HttpContext context, CachedRawResponse crr)
		{
			if (crr == null || context.Timestamp > crr.Policy.Expires)
				return true;
			return false;
		}
	}
}

