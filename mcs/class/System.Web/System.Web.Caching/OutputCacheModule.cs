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
			HttpAsyncResult result;
			
			string key = context.Request.FilePath;
			CachedRawResponse c = context.Cache [key] as CachedRawResponse;

			if (c != null && context.Timestamp < c.Policy.Expires &&
					c.ParamsVary (context.Request)) {
				
				context.Response.ClearContent ();
				context.Response.BinaryWrite (c.GetData ());

				context.Response.ClearHeaders ();
				context.Response.SetCachedHeaders (c.Headers);
				context.Response.StatusCode = c.StatusCode;
				context.Response.StatusDescription = c.StatusDescription;
				
				app.CompleteRequest ();
			} else if (c != null) {
				context.Cache.Remove (key);
			}
			
			result = new HttpAsyncResult (cb,this);
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
			string key = context.Request.FilePath;
			CachedRawResponse prev = context.Cache [key] as CachedRawResponse;
			
			if (context.Response.IsCached && IsExpired (context, prev)) {
				CachedRawResponse c = context.Response.GetCachedResponse ();
				
				context.Cache.InsertPrivate (key, c, null,
						context.Response.Cache.Expires,
						Cache.NoSlidingExpiration,
						CacheItemPriority.Normal, null);
			}
			
			HttpAsyncResult result = new HttpAsyncResult (cb, this);
			result.Complete (true, o, null);
			
			return result;
		}

		void OnEndUpdateCache (IAsyncResult result)
		{
		}

		private bool IsExpired (HttpContext context, CachedRawResponse crr)
		{
			if (crr == null || context.Timestamp > crr.Policy.Expires)
				return true;
			return false;
		}

	}
}

