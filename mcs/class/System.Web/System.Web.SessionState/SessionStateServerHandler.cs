//
// System.Web.SessionState.SessionStateServerHandler
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc, (http://www.novell.com)
//


using System;
using System.IO;
using System.Reflection;
using System.Configuration;
using System.Runtime.Remoting;

namespace System.Web.SessionState {

	internal class SessionStateServerHandler : ISessionHandler
	{
		const string CookieName = "ASPSESSION";
		const int DefTimeout = 600;

		private RemoteStateServer state_server;
		private SessionConfig config;
		
		public void Dispose ()
		{
		}

		public void Init (HttpApplication context, SessionConfig config)
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
			if (context.Session == null)
				return;
			
			string id = context.Session.SessionID;
			SessionDictionary dict = context.Session.SessionDictionary;
			HttpStaticObjectsCollection sobjs = context.Session.StaticObjects;
			state_server.Update (id, dict.ToByteArray (), sobjs.ToByteArray ());
		}

		public bool UpdateContext (HttpContext context, SessionStateModule module)
		{
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
					session = new HttpSessionState (id, dict, sobjs, 
							config.Timeout, false, config.CookieLess,
							SessionStateMode.StateServer, false);
					context.SetSession (session);
					return false;
				}
			}
			
			id = SessionId.Create (module.Rng);
			dict = new SessionDictionary ();
			sobjs = new HttpStaticObjectsCollection ();
			item = new StateServerItem (dict.ToByteArray (), sobjs.ToByteArray (), config.Timeout);
			
			state_server.Insert (id, item);

			session = new HttpSessionState (id, dict, sobjs, config.Timeout,
					false, config.CookieLess, SessionStateMode.StateServer, false);
			
			context.SetSession (session);
			context.Session.IsNewSession = true;
			context.Response.AppendCookie (new HttpCookie (CookieName, id));

			return true;
		}

		private string GetId (HttpContext context)
		{
			if (!config.CookieLess &&
					context.Request.Cookies [CookieName] != null)
				return context.Request.Cookies [CookieName].Value;
			return null;
		}

		private byte[] GetDictData (SessionDictionary dict)
		{
			MemoryStream stream = null;
			try {
				stream = new MemoryStream ();
				dict.Serialize (new BinaryWriter (stream));
				return stream.GetBuffer ();
			} catch {
				throw;
			} finally {
				if (stream != null)
					stream.Close ();
			}
		}

		private void GetConData (string cons, out string proto, out string server, out string port)
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

