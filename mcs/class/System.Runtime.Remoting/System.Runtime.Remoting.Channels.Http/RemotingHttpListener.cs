//
// RemotingHttpListener.cs
// 
// Author:
//   Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.IO;
using MonoHttp;

namespace System.Runtime.Remoting.Channels.Http
{
	class RemotingHttpListener : IHttpListenerContextBinder, IDisposable
	{
		IPEndPoint endpoint;
		Socket sock;
		HttpServerTransportSink sink;

		public RemotingHttpListener (IPAddress addr, int port, HttpServerTransportSink sink)
		{
			this.sink = sink;

			endpoint = new IPEndPoint (addr, port);
			sock = new Socket (addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			sock.Bind (endpoint);
			sock.Listen (500);
			sock.BeginAccept (OnAccept, this);
		}

		public int AssignedPort
		{
			get { return ((IPEndPoint)sock.LocalEndPoint).Port; }
		}

		//from HttpListener
		static void OnAccept (IAsyncResult ares)
		{
			RemotingHttpListener epl = (RemotingHttpListener)ares.AsyncState;
			Socket accepted = null;
			try {
				accepted = epl.sock.EndAccept (ares);
			} catch {
				// Anything to do here?
			} finally {
				try {
					epl.sock.BeginAccept (OnAccept, epl);
				} catch {
					if (accepted != null) {
						try {
							accepted.Close ();
						} catch { }
						accepted = null;
					}
				}
			}

			if (accepted == null)
				return;

			HttpConnection conn = new HttpConnection (accepted, epl);
			conn.BeginReadRequest ();
		}

		//when the connection's processed headers and the stream, it calls this 
		public bool BindContext (MonoHttp.HttpListenerContext context)
		{
			sink.HandleRequest (context);
			return true;
		}

		//when connection's closed, it calls this
		public void UnbindContext (MonoHttp.HttpListenerContext context)
		{
			//do nothing, we should have called Close anyway
		}

		public void Dispose ()
		{
			sock.Close ();
		}
	}
}
