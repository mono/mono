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
	    * Generate SessionID:s in a good (more random) way.
*/
using System;
using System.Collections;

namespace System.Web.SessionState
{
	// Container object, containing the current session state and when it was last accessed.
	public class SessionContainer
	{
		private HttpSessionState _state;
		private long _last_access;

		public SessionContainer (HttpSessionState state)
		{
			_state = state;
			this.Touch ();
		}

		public void Touch ()
		{
			_last_access = DateTime.Now.Millisecond;	    
		}

		public HttpSessionState SessionState {
			get {
				//Check if we should abandon it.
				if (_state != null &&
				    (DateTime.Now.Millisecond - _last_access) > (_state.Timeout * 60 * 1000))
					_state.Abandon ();

				return _state;
			}
			set {
				_state=value;
				this.Touch ();
			}
		}
	}


	public class SessionInProcHandler : ISessionHandler
	{
		protected Hashtable _sessionTable;
		const string COOKIE_NAME = "ASPSESSION"; // The name of the cookie.
		// The length of a session, in minutes. After this length, it's abandoned due to idle.
		const int SESSION_LIFETIME = 45;

		public void Dispose ()
		{
			_sessionTable = null;
		}

		public void Init (HttpApplication context)
		{
			_sessionTable = new Hashtable();
		}


		//this is the code that actually do stuff.
		public void UpdateContext (HttpContext context)
		{
			SessionContainer container = null;

			//first we try to get the cookie.
			// if we have a cookie, we look it up in the table.
			if (context.Request.Cookies [COOKIE_NAME] != null) {
				container = (SessionContainer) _sessionTable [context.Request.Cookies [COOKIE_NAME].Value];

				// if we have a session, and it is not expired, set isNew to false and return it.
				if (container!=null && container.SessionState!=null && !container.SessionState.IsAbandoned) {
					// Can we do this? It feels safe, but what do I know.
					container.SessionState.IsNewSession = false;
					// update the timestamp.
					container.Touch ();
					 // Can we do this? It feels safe, but what do I know.
					context.Session = container.SessionState;
					return; // and we're done
				} else if(container!=null) {
					//A empty or expired session, lets kill it.
					_sessionTable[context.Request.Cookies[COOKIE_NAME]]=null;
				}
			}

			// else we create a new session.
			string sessionID = System.Guid.NewGuid ().ToString ();
			container = new SessionContainer (new HttpSessionState (sessionID, // unique identifier
										new SessionDictionary(), // dictionary
										new HttpStaticObjectsCollection(),
										SESSION_LIFETIME, //lifetime befor death.
										true, //new session
										false, // is cookieless
										SessionStateMode.InProc,
										false)); //readonly
			// puts it in the table.
			_sessionTable [sessionID]=container;

			// and returns it.
			context.Session = container.SessionState;


			// sets the session cookie. We're assuming that session scope is the default mode.
			context.Response.AppendCookie (new HttpCookie (COOKIE_NAME,sessionID));

			// And we're done!
		}
	}
}

