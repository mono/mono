//
// System.Web.SessionState.SessionInProcHandler
//
// Authors:
//	Stefan Görling (stefan@gorling.se)
//
// (C) 2003 Stefan Görling
//

/*
	This is a rather lazy implementation, but it does the trick for me.

	TODO:
	    * Remove abandoned sessions., preferably by a worker thread sleeping most of the time.
	    * Increase session security, for example by using UserAgent i hashcode.
*/
using System;
using System.IO;
using System.Collections;

namespace System.Web.SessionState
{
	// Container object, containing the current session state and when it was last accessed.
	internal class SessionContainer
	{
		private HttpSessionState _state;
		private DateTime last_access;

		public SessionContainer (HttpSessionState state)
		{
			_state = state;
			this.Touch ();
		}

		public void Touch ()
		{
			last_access = DateTime.Now;
		}

		public HttpSessionState SessionState {
			get {
				//Check if we should abandon it.
				if (_state != null && last_access.AddMinutes (_state.Timeout) < DateTime.Now)
					_state.Abandon ();

				return _state;
			}
			set {
				_state=value;
				this.Touch ();
			}
		}
	}


	internal class SessionInProcHandler : ISessionHandler
	{
		protected Hashtable _sessionTable;
		// The length of a session, in minutes. After this length, it's abandoned due to idle.
		const int SESSION_LIFETIME = 45;

		private SessionConfig config;
		
		public void Dispose ()
		{
			_sessionTable = null;
		}

		public void Init (HttpApplication context, SessionConfig config)
		{
			this.config = config;
			_sessionTable = (Hashtable) AppDomain.CurrentDomain.GetData (".MonoSessionInProc");
			if (_sessionTable == null)
				_sessionTable = new Hashtable();
		}

		public void UpdateHandler (HttpContext context, SessionStateModule module)
		{
		}

		//this is the code that actually do stuff.
		public HttpSessionState UpdateContext (HttpContext context, SessionStateModule module,
							bool required, bool read_only, ref bool isNew)
		{
			SessionContainer container = null;
			string id = SessionId.Lookup (context.Request, config.CookieLess);
			
			//first we try to get the cookie.
			// if we have a cookie, we look it up in the table.
			if (id != null) {
				container = (SessionContainer) _sessionTable [id];

				// if we have a session, and it is not expired, set isNew to false and return it.
				if (container != null && container.SessionState != null && 
				    !container.SessionState.IsAbandoned) {
					if (required)
						container.SessionState.SetNewSession (false);
					// update the timestamp.
					container.Touch ();
					if (required)
						return container.SessionState;

					return null;
				} else if (container!=null) {
					_sessionTable.Remove (id);
				}
			}

			if (!required)
				return null;

			// else we create a new session.
			string sessionID = SessionId.Create (module.Rng);
			HttpSessionState state = new HttpSessionState (sessionID, // unique identifier
							new SessionDictionary(), // dictionary
							HttpApplicationFactory.ApplicationState.SessionObjects,
							SESSION_LIFETIME, //lifetime before death.
							true, //new session
							false, // is cookieless
							SessionStateMode.InProc,
							read_only); //readonly

			container = new SessionContainer (state);
			_sessionTable [sessionID]=container;
			AppDomain.CurrentDomain.SetData (".MonoSessionInProc", _sessionTable);
			isNew = true;
			return container.SessionState;
		}
	}
}

