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

namespace System.Web.SessionState
{
	[MonoTODO]
	public sealed class SessionStateModule : IHttpModule, IRequiresSessionState
	{
		static SessionConfig config;
		Type handlerType;
		ISessionHandler handler;
		
		public SessionStateModule ()
		{
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

				if (config.Mode == SessionStateMode.StateServer ||
				    config.Mode == SessionStateMode.SQLServer)
					throw new NotSupportedException ("Only Off and InProc modes supported.");
				
				if (config.Mode == SessionStateMode.InProc) {
					handlerType = typeof (SessionInProcHandler);
					app.AddOnAcquireRequestStateAsync (
							new BeginEventHandler (OnBeginAcquireState),
							new EndEventHandler (OnEndAcquireState));

					app.ReleaseRequestState += new EventHandler (OnReleaseRequestState);
					app.EndRequest += new EventHandler (OnEndRequest);
				}
			}

			if (handlerType != null && handler == null) {
				handler = (ISessionHandler) Activator.CreateInstance (handlerType);
				handler.Init(app); //initialize
			}
		}

		void OnReleaseRequestState (object o, EventArgs args)
		{
		}

		void OnEndRequest (object o, EventArgs args)
		{
		}

		IAsyncResult OnBeginAcquireState (object o, EventArgs args, AsyncCallback cb, object data)
		{

			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;

			if (handler!=null)
			    handler.UpdateContext (context);
			
			// In the future, we might want to move the Async stuff down to
			// the interface level, if we're going to support other than
			// InProc, we might actually want to do things async, now we
			// simply fake it.
			HttpAsyncResult result=new HttpAsyncResult (cb,this);
			result.Complete (true, o, null);
			if (Start != null)
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




