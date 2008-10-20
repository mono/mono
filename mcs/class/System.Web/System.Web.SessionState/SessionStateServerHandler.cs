//
// System.Web.SessionState.SessionStateServerHandler
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc, (http://www.novell.com)
//

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
using System;
using System.IO;
using System.Reflection;
using System.Configuration;
using System.Web.Configuration;
using System.Runtime.Remoting;

namespace System.Web.SessionState {

	internal class SessionStateServerHandler : ISessionHandler
	{
		const string CookieName = "ASPSESSION";

		RemoteStateServer state_server;
#if NET_2_0
		SessionStateSection config;
#else
		SessionConfig config;
#endif
		
		public void Dispose ()
		{
		}

		public void Init (SessionStateModule module, HttpApplication context,
#if NET_2_0
				  SessionStateSection config
#else
				  SessionConfig config
#endif
				  )
		{
			this.config = config;
			RemotingConfiguration.Configure (null);
			string cons, proto, server, port;
			GetConData (config.StateConnectionString, out proto, out server, out port);
			cons = String.Format ("{0}://{1}:{2}/StateServer", proto, server, port);
			state_server = (RemoteStateServer) Activator.GetObject (typeof (RemoteStateServer), cons);
		}

		public void UpdateHandler (HttpContext context, SessionStateModule module)
		{
			HttpSessionState session = context.Session;
			if (session == null || session.IsReadOnly)
				return;
			
			string id = session.SessionID;
			if (!session._abandoned) {
				SessionDictionary dict = session.SessionDictionary;
				HttpStaticObjectsCollection sobjs = session.StaticObjects;
				state_server.Update (id, dict.ToByteArray (), sobjs.ToByteArray ());
			} else {
				state_server.Remove (id);
			}
		}

		public HttpSessionState UpdateContext (HttpContext context, SessionStateModule module,
							bool required, bool read_only, ref bool isNew)
		{
			if (!required)
				return null;

			StateServerItem item = null;
			HttpSessionState session = null;
			SessionDictionary dict = null;
			HttpStaticObjectsCollection sobjs = null;
			string id = GetId (context);

			if (id != null) {
				item = state_server.Get (id);
				if (item != null) {
					dict = SessionDictionary.FromByteArray (item.DictionaryData);
					sobjs = HttpStaticObjectsCollection.FromByteArray (item.StaticObjectsData);
					session = new HttpSessionState (id, dict,
							HttpApplicationFactory.ApplicationState.SessionObjects,
#if NET_2_0
							(int)config.Timeout.TotalMinutes, // XXX is this right?  we lose some precision here, but since the timeout is in minutes *anyway*...
#else
							config.Timeout,
#endif
							false, config.CookieLess,
							SessionStateMode.StateServer, read_only);

					return session;
				}
			}
			
			id = SessionId.Create ();
			dict = new SessionDictionary ();
			sobjs = HttpApplicationFactory.ApplicationState.SessionObjects;
			item = new StateServerItem (dict.ToByteArray (), sobjs.ToByteArray (),
#if NET_2_0
						    (int)config.Timeout.TotalMinutes
#else
						    config.Timeout
#endif
						    );
			
			state_server.Insert (id, item);

			session = new HttpSessionState (id, dict, sobjs,
#if NET_2_0
							(int)config.Timeout.TotalMinutes,
#else
							config.Timeout,
#endif
							true,
							config.CookieLess, SessionStateMode.StateServer,
							read_only);
			
			isNew = true;
			return session;
		}

		public void Touch (string sessionId, int timeout)
		{
			if (state_server == null)
				return;

			state_server.Touch (sessionId, timeout);
		}
		
		string GetId (HttpContext context)
		{
			if (!config.CookieLess &&
					context.Request.Cookies [CookieName] != null)
				return context.Request.Cookies [CookieName].Value;

			return context.Request.Headers [SessionStateModule.HeaderName];
		}

		void GetConData (string cons, out string proto, out string server, out string port)
		{
			int ei = cons.IndexOf ('=');
			int ci = cons.IndexOf (':');

			if (ei < 0 || ci < 0)
				throw new Exception ("Invalid StateConnectionString");
			
			proto = cons.Substring (0, ei);
			server = cons.Substring (ei+1, ci - ei - 1);
			port = cons.Substring (ci+1, cons.Length - ci - 1);

			if (proto == "tcpip")
				proto = "tcp";
		}
	} 
}
#endif
