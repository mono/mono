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

namespace System.Runtime.Remoting.Channels.Http
{
	sealed class RemotingHttpListener : IDisposable
	{
		HttpServerTransportSink sink;
		HttpListener listener;
		int local_port;

		public RemotingHttpListener (IPAddress addr, int port, HttpServerTransportSink sink)
		{
			this.sink = sink;
			bool find_port = false;
			if (port == 0)
				find_port = true;

			string address = null;
			if (addr == IPAddress.Any)
				address = "*";
#if NET_2_0
			else if (addr == IPAddress.IPv6Any)
				address = "*";
#endif
			else
				address = addr.ToString ();

			listener = new HttpListener ();
			while (true) {
				Random rnd = null;
				if (find_port) {
					if (rnd == null)
						rnd = new Random ();
					port = rnd.Next (1025, 65000);
				}
				try {
					listener.Prefixes.Add (String.Format ("http://{0}:{1}/", address, port));
					listener.Start ();
					local_port = port;
					break;
				} catch (Exception) {
					if (!find_port)
						throw;
					listener.Prefixes.Clear ();
					// Port already in use
				}
			}
			listener.BeginGetContext (new AsyncCallback (OnGetContext), null);
		}

		public int AssignedPort
		{
			get { return local_port; }
		}

		void OnGetContext (IAsyncResult ares)
		{
			if (listener == null)
				return; // already disposed

			HttpListenerContext context = null;
			try {
				context = listener.EndGetContext (ares);
				listener.BeginGetContext (new AsyncCallback (OnGetContext), null);
			} catch {
				// Listener was closed
			}

			if (context != null) {
				try {
					sink.HandleRequest (context);
				} catch {
					try {
						context.Response.Close ();
					} catch {}
				}
			}
		}

		public void Dispose ()
		{
			if (listener != null) {
				listener.Close ();
				listener = null;
			}
		}
	}
}
