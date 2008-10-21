//
// System.Web.SessionState.SesionStateModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Stefan Görling (stefan@gorling.se)
//	Jackson Harper (jackson@ximian.com)
//
// Copyright (C) 2002-2006 Novell, Inc (http://www.novell.com)
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

#if !NET_2_0
using System.ComponentModel;
using System.Web.Configuration;
using System.Web.Caching;
using System.Web.Util;
using System.Security.Cryptography;
using System.Security.Permissions;

namespace System.Web.SessionState
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class SessionStateModule : IHttpModule
	{
		internal const string CookieName = "ASPSESSION";
		internal const string HeaderName = "AspFilterSessionId";

		static readonly object locker = new object ();
		static readonly object startEvent = new object ();
		static readonly object endEvent = new object ();
		
		EventHandlerList events = new EventHandlerList ();
		
		public event EventHandler Start {
			add { events.AddHandler (startEvent, value); }
			remove { events.RemoveHandler (startEvent, value); }
		}

		// This event is public, but only Session_[On]End in global.asax will be invoked if present.
		public event EventHandler End {
			add { events.AddHandler (endEvent, value); }
			remove { events.RemoveHandler (endEvent, value); }
		}

#if TARGET_J2EE		
		static SessionConfig config {
			get {
				return (SessionConfig)AppDomain.CurrentDomain.GetData("SessionStateModule.config");
			}
			set {
				AppDomain.CurrentDomain.SetData("SessionStateModule.config", value);
			}
		}
		static Type handlerType {
			get {
				return (Type)AppDomain.CurrentDomain.GetData("SessionStateModule.handlerType");
			}
			set {
				AppDomain.CurrentDomain.SetData("SessionStateModule.handlerType", value);
			}
		}
#else
		static SessionConfig config;
		static Type handlerType;
#endif		
		ISessionHandler handler;
		bool sessionForStaticFiles;
		
		static RandomNumberGenerator rng = RandomNumberGenerator.Create ();
		
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public SessionStateModule ()
		{
		}

		internal RandomNumberGenerator Rng {
			get { return rng; }
		}

		public void Dispose ()
		{
		    if (handler!=null)
			handler.Dispose();
		}

		SessionConfig GetConfig ()
		{
			lock (locker) {
				if (config != null)
					return config;

				config = (SessionConfig) HttpContext.GetAppConfig ("system.web/sessionState");
				if (config ==  null)
					config = new SessionConfig (null);

#if TARGET_J2EE
				if (config.Mode == SessionStateMode.SQLServer || config.Mode == SessionStateMode.StateServer)
					throw new NotImplementedException("You must use web.xml to specify session state handling");
#else
				if (config.Mode == SessionStateMode.StateServer)
					handlerType = typeof (SessionStateServerHandler);

				if (config.Mode == SessionStateMode.SQLServer)
					handlerType = typeof (SessionSQLServerHandler);
#endif
				if (config.Mode == SessionStateMode.InProc)
					handlerType = typeof (SessionInProcHandler);

				return config;
			}
		}

		[EnvironmentPermission (SecurityAction.Assert, Read = "MONO_XSP_STATIC_SESSION")]
		public void Init (HttpApplication app)
		{
			sessionForStaticFiles = (Environment.GetEnvironmentVariable ("MONO_XSP_STATIC_SESSION") != null);
			SessionConfig cfg = GetConfig ();
			if (handlerType == null)
				return;

			if (cfg.CookieLess)
				app.BeginRequest += new EventHandler (OnBeginRequest);

			app.AcquireRequestState += new EventHandler (OnAcquireState);
			app.ReleaseRequestState += new EventHandler (OnReleaseRequestState);
			app.EndRequest += new EventHandler (OnEndRequest);
			
			if (handlerType != null && handler == null) {
				handler = (ISessionHandler) Activator.CreateInstance (handlerType);
				handler.Init (this, app, cfg); //initialize
			}
		}

		void OnBeginRequest (object o, EventArgs args)
		{
			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;
			string file_path = context.Request.FilePath;
			string base_path = VirtualPathUtility.GetDirectory (file_path);
			string id = UrlUtils.GetSessionId (base_path);

			if (id == null)
				return;
			
			string new_path = UrlUtils.RemoveSessionId (base_path, file_path);
			context.Request.SetFilePath (new_path);
			context.Request.SetHeader (HeaderName, id);
			context.Response.SetAppPathModifier (id);
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

		void OnAcquireState (object o, EventArgs args)
		{
			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;

			bool required = (context.Handler is IRequiresSessionState);
			
			// This is a hack. Sites that use Session in global.asax event handling code
			// are not supposed to get a Session object for static files, but seems that
			// IIS handles those files before getting there and thus they are served without
			// error.
			// As a workaround, setting MONO_XSP_STATIC_SESSION variable make this work
			// on mono, but you lose performance when serving static files.
			if (sessionForStaticFiles && context.Handler is StaticFileHandler)
				required = true;
			// hack end

			if (!required)
				return;
			
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

				HttpRequest request = context.Request;
				HttpResponse response = context.Response;
				string id = context.Session.SessionID;
				if (isNew && config.CookieLess) {
					request.SetHeader (HeaderName, id);
					UriBuilder newUri = new UriBuilder (request.Url);
					newUri.Path = UrlUtils.InsertSessionId (id, request.FilePath);
					response.Redirect (newUri.Uri.PathAndQuery);
				} else if (isNew) {
					HttpCookie cookie = new HttpCookie (CookieName, id);
					cookie.Path = request.ApplicationPath;
					context.Response.AppendCookie (cookie);
				}

				if (isNew) {
					OnSessionStart ();
					HttpSessionState hss = application.Session;
					
					if (hss != null)
						handler.Touch (hss.SessionID, hss.Timeout);
				}
			}
		}

		void OnSessionStart ()
		{
			EventHandler eh = events [startEvent] as EventHandler;
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnSessionRemoved (string key, object value, CacheItemRemovedReason reason)
		{
			SessionConfig cfg = GetConfig ();

			// Only invoked for InProc (see msdn2 docs on SessionStateModule.End)
			if (cfg.Mode == SessionStateMode.InProc)
				HttpApplicationFactory.InvokeSessionEnd (value);
		}		
	}
}

#endif
