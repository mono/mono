//
// System.Web.SessionState.SesionStateModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System.Web;

namespace System.Web.SessionState
{
	[MonoTODO]
	public sealed class SessionStateModule : IHttpModule
	{
		static SessionConfig config;
		Type handlerType;
		object handler;
		
		class SessionInProcHandler {}
		
		public SessionStateModule ()
		{
		}

		public void Dispose ()
		{
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

			if (handlerType != null && handler == null)
				handler = Activator.CreateInstance (handlerType);
		}

		void OnReleaseRequestState (object o, EventArgs args)
		{
		}

		void OnEndRequest (object o, EventArgs args)
		{
		}

		IAsyncResult OnBeginAcquireState (object o, EventArgs args, AsyncCallback cb, object data)
		{
			return null;
		}

		void OnEndAcquireState (IAsyncResult result)
		{
		}

		public event EventHandler Start;
		public event EventHandler End;
	}
}

