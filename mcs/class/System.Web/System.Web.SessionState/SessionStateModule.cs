//
// System.Web.SessionState.SesionStateModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Stefan Görling (stefan@gorling.se)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Stefan Görling (http://www.gorling.se)

using System.Web;
using System.Security.Cryptography;

namespace System.Web.SessionState
{
	[MonoTODO]
	public sealed class SessionStateModule : IHttpModule, IRequiresSessionState
	{
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

		[MonoTODO]
		public void Init (HttpApplication app)
		{
			if (config == null) {
				config = (SessionConfig) HttpContext.GetAppConfig ("system.web/sessionState");
				if (config ==  null)
					config = new SessionConfig (null);

				if (config.Mode == SessionStateMode.StateServer)
					throw new NotSupportedException ("StateServer mode is not supported.");

				if (config.Mode == SessionStateMode.SQLServer)
					handlerType = typeof (SessionSQLServerHandler);
				
				if (config.Mode == SessionStateMode.InProc)
					handlerType = typeof (SessionInProcHandler);
			}
				
			app.AddOnAcquireRequestStateAsync (
				new BeginEventHandler (OnBeginAcquireState),
				new EndEventHandler (OnEndAcquireState));

			app.ReleaseRequestState += new EventHandler (OnReleaseRequestState);
			app.EndRequest += new EventHandler (OnEndRequest);
			
			if (handlerType != null && handler == null) {
				handler = (ISessionHandler) Activator.CreateInstance (handlerType);
				handler.Init(app, config); //initialize
			}
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

			bool isNew = false;
			if (handler != null)
			    isNew = handler.UpdateContext (context, this);
			
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

		void OnEndAcquireState (IAsyncResult result)
		{
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




