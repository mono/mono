//
// System.Web.SessionState.SessionInProcHandler
//
// Authors:
//	Stefan Görling (stefan@gorling.se)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Stefan Görling
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//

using System;
using System.IO;
using System.Collections;
using System.Web.Caching;

namespace System.Web.SessionState
{
	class SessionInProcHandler : ISessionHandler
	{
		SessionConfig config;
		CacheItemRemovedCallback removedCB;
		
		public void Dispose () { }

		public void Init (SessionStateModule module, HttpApplication context, SessionConfig config)
		{
			removedCB = new CacheItemRemovedCallback (module.OnSessionRemoved);
			this.config = config;
		}

		public void UpdateHandler (HttpContext context, SessionStateModule module) { }

		public HttpSessionState UpdateContext (HttpContext context, SessionStateModule module,
							bool required, bool read_only, ref bool isNew)
		{
			if (!required)
				return null;

			Cache cache = HttpRuntime.Cache;
			HttpSessionState state = null;
			string id = SessionId.Lookup (context.Request, config.CookieLess);
			
			if (id != null) {
				state = (HttpSessionState) cache ["@@@InProc@" + id];
				if (state != null)
					return state;
			}

			// Create a new session.
			string sessionID = SessionId.Create (module.Rng);
			state = new HttpSessionState (sessionID, // unique identifier
						new SessionDictionary(), // dictionary
						HttpApplicationFactory.ApplicationState.SessionObjects,
						config.Timeout, //lifetime before death.
						true, //new session
						false, // is cookieless
						SessionStateMode.InProc,
						read_only); //readonly

			TimeSpan timeout = new TimeSpan (0, config.Timeout, 0);
			cache.Insert ("@@@InProc@" + sessionID, state, null, DateTime.Now + timeout,
					timeout, CacheItemPriority.AboveNormal, removedCB);

			isNew = true;
			return state;
		}
	}
}

