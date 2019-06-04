//
// System.Net.EndPointListener
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo.mono@gmail.com)
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2012 Xamarin, Inc. (http://xamarin.com)
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

using System.IO;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net {
	sealed class EndPointListener
	{
		HttpListener listener;
		IPEndPoint endpoint;
		Socket sock;
		Hashtable prefixes;  // Dictionary <ListenerPrefix, HttpListener>
		ArrayList unhandled; // List<ListenerPrefix> unhandled; host = '*'
		ArrayList all;       // List<ListenerPrefix> all;  host = '+'
		X509Certificate cert;
		bool secure;
		Dictionary<HttpConnection, HttpConnection> unregistered;

		public EndPointListener (HttpListener listener, IPAddress addr, int port, bool secure)
		{
			this.listener = listener;

			if (secure) {
				this.secure = secure;
				cert = listener.LoadCertificateAndKey (addr, port);
			}

			endpoint = new IPEndPoint (addr, port);
			sock = new Socket (addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			sock.Bind (endpoint);
			sock.Listen (500);
			SocketAsyncEventArgs args = new SocketAsyncEventArgs ();
			args.UserToken = this;
			args.Completed += OnAccept;
			Socket dummy = null;
			Accept (sock, args, ref dummy);
			prefixes = new Hashtable ();
			unregistered = new Dictionary<HttpConnection, HttpConnection> ();
		}

		internal HttpListener Listener {
			get { return listener; }
		}

		static void Accept (Socket socket, SocketAsyncEventArgs e, ref Socket accepted) {
			e.AcceptSocket = null;
			bool asyn;
			try {
				asyn = socket.AcceptAsync(e);
			} catch {
				if (accepted != null) {
					try {
						accepted.Close ();
					} catch {
					}
					accepted = null;
				}
				return;
			}
			if (!asyn) {
				ProcessAccept(e);
			}
		}


		static void ProcessAccept (SocketAsyncEventArgs args) 
		{
			Socket accepted = null;
			if (args.SocketError == SocketError.Success)
				accepted = args.AcceptSocket;

			EndPointListener epl = (EndPointListener) args.UserToken;


			Accept (epl.sock, args, ref accepted);
			if (accepted == null)
				return;

			if (epl.secure && epl.cert == null) {
				accepted.Close ();
				return;
			}
			HttpConnection conn;
			try {
				conn = new HttpConnection (accepted, epl, epl.secure, epl.cert);
			} catch {
				accepted.Close ();
				return;
			}
			lock (epl.unregistered) {
				epl.unregistered [conn] = conn;
			}
			conn.BeginReadRequest ();
		}

		static void OnAccept (object sender, SocketAsyncEventArgs e) 
		{
			ProcessAccept (e);
		}

		internal void RemoveConnection (HttpConnection conn) 
		{
			lock (unregistered) {
				unregistered.Remove (conn);
			}
		}

		public bool BindContext (HttpListenerContext context)
		{
			HttpListenerRequest req = context.Request;
			ListenerPrefix prefix;
			HttpListener listener = SearchListener (req.Url, out prefix);
			if (listener == null)
				return false;

			context.Listener = listener;
			context.Connection.Prefix = prefix;
			return true;
		}

		public void UnbindContext (HttpListenerContext context)
		{
			if (context == null || context.Request == null)
				return;

			context.Listener.UnregisterContext (context);
		}

		HttpListener SearchListener (Uri uri, out ListenerPrefix prefix)
		{
			prefix = null;
			if (uri == null)
				return null;

			string host = uri.Host;
			int port = uri.Port;
			string path = WebUtility.UrlDecode (uri.AbsolutePath);
			string path_slash = path [path.Length - 1] == '/' ? path : path + "/";
			
			HttpListener best_match = null;
			int best_length = -1;

			if (host != null && host != "") {
				Hashtable p_ro = prefixes;
				foreach (ListenerPrefix p in p_ro.Keys) {
					string ppath = p.Path;
					if (ppath.Length < best_length)
						continue;

					if (p.Host != host || p.Port != port)
						continue;

					if (path.StartsWith (ppath) || path_slash.StartsWith (ppath)) {
						best_length = ppath.Length;
						best_match = (HttpListener) p_ro [p];
						prefix = p;
					}
				}
				if (best_length != -1)
					return best_match;
			}

			ArrayList list = unhandled;
			best_match = MatchFromList (host, path, list, out prefix);
			if (path != path_slash && best_match == null)
				best_match = MatchFromList (host, path_slash, list, out prefix);
			if (best_match != null)
				return best_match;

			list = all;
			best_match = MatchFromList (host, path, list, out prefix);
			if (path != path_slash && best_match == null)
				best_match = MatchFromList (host, path_slash, list, out prefix);
			if (best_match != null)
				return best_match;

			return null;
		}

		HttpListener MatchFromList (string host, string path, ArrayList list, out ListenerPrefix prefix)
		{
			prefix = null;
			if (list == null)
				return null;

			HttpListener best_match = null;
			int best_length = -1;
			
			foreach (ListenerPrefix p in list) {
				string ppath = p.Path;
				if (ppath.Length < best_length)
					continue;

				if (path.StartsWith (ppath)) {
					best_length = ppath.Length;
					best_match = p.Listener;
					prefix = p;
				}
			}

			return best_match;
		}

		void AddSpecial (ArrayList coll, ListenerPrefix prefix)
		{
			if (coll == null)
				return;

			foreach (ListenerPrefix p in coll) {
				if (p.Path == prefix.Path) //TODO: code
					throw new HttpListenerException (400, "Prefix already in use.");
			}
			coll.Add (prefix);
		}

		bool RemoveSpecial (ArrayList coll, ListenerPrefix prefix)
		{
			if (coll == null)
				return false;

			int c = coll.Count;
			for (int i = 0; i < c; i++) {
				ListenerPrefix p = (ListenerPrefix) coll [i];
				if (p.Path == prefix.Path) {
					coll.RemoveAt (i);
					return true;
				}
			}
			return false;
		}

		void CheckIfRemove ()
		{
			if (prefixes.Count > 0)
				return;

			ArrayList list = unhandled;
			if (list != null && list.Count > 0)
				return;

			list = all;
			if (list != null && list.Count > 0)
				return;

			EndPointManager.RemoveEndPoint (this, endpoint);
		}

		public void Close ()
		{
			sock.Close ();
			lock (unregistered) {
				//
				// Clone the list because RemoveConnection can be called from Close
				//
				var connections = new List<HttpConnection> (unregistered.Keys);

				foreach (HttpConnection c in connections)
					c.Close (true);
				unregistered.Clear ();
			}
		}

		public void AddPrefix (ListenerPrefix prefix, HttpListener listener)
		{
			ArrayList current;
			ArrayList future;
			if (prefix.Host == "*") {
				do {
					current = unhandled;
					future = (current != null) ? (ArrayList) current.Clone () : new ArrayList ();
					prefix.Listener = listener;
					AddSpecial (future, prefix);
				} while (Interlocked.CompareExchange (ref unhandled, future, current) != current);
				return;
			}

			if (prefix.Host == "+") {
				do {
					current = all;
					future = (current != null) ? (ArrayList) current.Clone () : new ArrayList ();
					prefix.Listener = listener;
					AddSpecial (future, prefix);
				} while (Interlocked.CompareExchange (ref all, future, current) != current);
				return;
			}

			Hashtable prefs, p2;
			do {
				prefs = prefixes;
				if (prefs.ContainsKey (prefix)) {
					HttpListener other = (HttpListener) prefs [prefix];
					if (other != listener) // TODO: code.
						throw new HttpListenerException (400, "There's another listener for " + prefix);
					return;
				}
				p2 = (Hashtable) prefs.Clone ();
				p2 [prefix] = listener;
			} while (Interlocked.CompareExchange (ref prefixes, p2, prefs) != prefs);
		}

		public void RemovePrefix (ListenerPrefix prefix, HttpListener listener)
		{
			ArrayList current;
			ArrayList future;
			if (prefix.Host == "*") {
				do {
					current = unhandled;
					future = (current != null) ? (ArrayList) current.Clone () : new ArrayList ();
					if (!RemoveSpecial (future, prefix))
						break; // Prefix not found
				} while (Interlocked.CompareExchange (ref unhandled, future, current) != current);
				CheckIfRemove ();
				return;
			}

			if (prefix.Host == "+") {
				do {
					current = all;
					future = (current != null) ? (ArrayList) current.Clone () : new ArrayList ();
					if (!RemoveSpecial (future, prefix))
						break; // Prefix not found
				} while (Interlocked.CompareExchange (ref all, future, current) != current);
				CheckIfRemove ();
				return;
			}

			Hashtable prefs, p2;
			do {
				prefs = prefixes;
				if (!prefs.ContainsKey (prefix))
					break;

				p2 = (Hashtable) prefs.Clone ();
				p2.Remove (prefix);
			} while (Interlocked.CompareExchange (ref prefixes, p2, prefs) != prefs);
			CheckIfRemove ();
		}
	}
}

