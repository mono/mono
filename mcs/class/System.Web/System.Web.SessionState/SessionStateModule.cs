//
// System.Web.SessionState.SesionStateModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Stefan Görling (stefan@gorling.se)
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Stefan Görling (http://www.gorling.se)

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

using System.Web;
using System.Web.Caching;
using System.Web.Util;
using System.Security.Cryptography;

namespace System.Web.SessionState
{
	public sealed class SessionStateModule : IHttpModule
	{
		internal static readonly string CookieName = "ASPSESSION";
		internal static readonly string HeaderName = "AspFilterSessionId";
		
		static SessionConfig config;
		static Type handlerType;
		ISessionHandler handler;
		
		private RandomNumberGenerator rng;
		
		public SessionStateModule ()
		{
			rng = new RNGCryptoServiceProvider ();
		}

		internal RandomNumberGenerator Rng {
			get { return rng; }
		}

		public void Dispose ()
		{
		    if (handler!=null)
			handler.Dispose();
		}

		public void Init (HttpApplication app)
		{
			if (config == null) {
				config = (SessionConfig) HttpContext.GetAppConfig ("system.web/sessionState");
				if (config ==  null)
					config = new SessionConfig (null);

				if (config.Mode == SessionStateMode.StateServer)
					handlerType = typeof (SessionStateServerHandler);

				if (config.Mode == SessionStateMode.SQLServer)
					handlerType = typeof (SessionSQLServerHandler);
				
				if (config.Mode == SessionStateMode.InProc)
					handlerType = typeof (SessionInProcHandler);

                                if (config.Mode == SessionStateMode.Off)
                                        return;
			}

			if (config.CookieLess)
				app.BeginRequest += new EventHandler (OnBeginRequest);
			
			app.AddOnAcquireRequestStateAsync (
				new BeginEventHandler (OnBeginAcquireState),
				new EndEventHandler (OnEndAcquireState));

			app.ReleaseRequestState += new EventHandler (OnReleaseRequestState);
			app.EndRequest += new EventHandler (OnEndRequest);
			
			if (handlerType != null && handler == null) {
				handler = (ISessionHandler) Activator.CreateInstance (handlerType);
				handler.Init (this, app, config); //initialize
			}
		}

		void OnBeginRequest (object o, EventArgs args)
		{
			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;
			string base_path = context.Request.BaseVirtualDir;
			string id = UrlUtils.GetSessionId (base_path);

			if (id == null)
				return;
			
			context.Request.SetFilePath (UrlUtils.RemoveSessionId (base_path,
								     context.Request.FilePath));
			context.Request.SetHeader (HeaderName, id);
		}
		
		void OnReleaseRequestState (object o, EventArgs args)
		{
			if (handler == null)
				return;

			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;
			handler.UpdateHandler (context, this);
		}

		void OnEndRequest (object o, EventArgs args)
		{
		}

		IAsyncResult OnBeginAcquireState (object o, EventArgs args, AsyncCallback cb, object data)
		{
			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;

			bool required = (context.Handler is IRequiresSessionState);
			bool read_only = (context.Handler is IReadOnlySessionState);
			
			bool isNew = false;
			HttpSessionState session = null;
			if (handler != null)
				session = handler.UpdateContext (context, this, required, read_only, ref isNew);

			if (session != null) {
				if (isNew)
					session.SetNewSession (true);

				if (read_only)
					session = session.Clone ();
					
				context.SetSession (session);

				if (isNew && config.CookieLess) {
					string id = context.Session.SessionID;
					context.Request.SetHeader (HeaderName, id);
					context.Response.Redirect (UrlUtils.InsertSessionId (id,
								   context.Request.FilePath));
				} else if (isNew) {
					string id = context.Session.SessionID;
					HttpCookie cookie = new HttpCookie (CookieName, id);
					cookie.Path = UrlUtils.GetDirectory (context.Request.ApplicationPath);
					context.Response.AppendCookie (cookie);
				}
			}
			
			// In the future, we might want to move the Async stuff down to
			// the interface level, if we're going to support other than
			// InProc, we might actually want to do things async, now we
			// simply fake it.
			HttpAsyncResult result=new HttpAsyncResult (cb,this);
			result.Complete (true, o, null);
			if (isNew && Start != null)
				Start (this, args);

			return result;
		}

		void OnEndAcquireState (IAsyncResult result) { }

		internal void OnSessionRemoved (string key, object value, CacheItemRemovedReason reason)
		{
			OnEnd ();
		}

		internal void OnEnd ()
		{
			if (End != null)
				End (this, EventArgs.Empty);
		}
		
		public event EventHandler Start;
		public event EventHandler End;
	}
}

