//
// System.Net.HttpConnection
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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

#if SECURITY_DEP

using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Tls;

namespace System.Net {
	sealed class HttpConnection
	{
		const int BufferSize = 8192;
		Socket sock;
		Stream stream;
		EndPointListener epl;
		MemoryStream ms;
		byte [] buffer;
		HttpListenerContext context;
		StringBuilder current_line;
		ListenerPrefix prefix;
		RequestStream i_stream;
		ResponseStream o_stream;
		bool chunked;
		int reuses;
		bool context_bound;
		bool secure;
		AsymmetricAlgorithm key;
		int s_timeout = 90000; // 90k ms for first request, 15k ms from then on
		Timer timer;

		public HttpConnection (Socket sock, EndPointListener epl, bool secure, X509Certificate2 cert, AsymmetricAlgorithm key)
		{
			this.sock = sock;
			this.epl = epl;
			this.secure = secure;
			this.key = key;
			if (secure == false) {
				stream = new NetworkStream (sock, false);
			} else {
				SslServerStream ssl_stream = new SslServerStream (new NetworkStream (sock, false), cert, false, false);
				ssl_stream.PrivateKeyCertSelectionDelegate += OnPVKSelection;
				stream = ssl_stream;
			}
			timer = new Timer (OnTimeout, null, Timeout.Infinite, Timeout.Infinite);
			Init ();
		}

		AsymmetricAlgorithm OnPVKSelection (X509Certificate certificate, string targetHost)
		{
			return key;
		}

		void Init ()
		{
			context_bound = false;
			i_stream = null;
			o_stream = null;
			prefix = null;
			chunked = false;
			ms = new MemoryStream ();
			position = 0;
			input_state = InputState.RequestLine;
			line_state = LineState.None;
			context = new HttpListenerContext (this);
		}

		public int Reuses {
			get { return reuses; }
		}

		public IPEndPoint LocalEndPoint {
			get { return (IPEndPoint) sock.LocalEndPoint; }
		}

		public IPEndPoint RemoteEndPoint {
			get { return (IPEndPoint) sock.RemoteEndPoint; }
		}

		public bool IsSecure {
			get { return secure; }
		}

		public ListenerPrefix Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		void OnTimeout (object unused)
		{
			Unbind ();
			try {
				sock.Close (); // stream disposed
			} catch {
			}
		}

		public void BeginReadRequest ()
		{
			if (buffer == null)
				buffer = new byte [BufferSize];
			try {
				if (reuses == 1)
					s_timeout = 15000;
				timer.Change (s_timeout, Timeout.Infinite);
				stream.BeginRead (buffer, 0, BufferSize, OnRead, this);
			} catch {
				CloseSocket ();
			}
		}

		public RequestStream GetRequestStream (bool chunked, long contentlength)
		{
			if (i_stream == null) {
				byte [] buffer = ms.GetBuffer ();
				int length = (int) ms.Length;
				ms = null;
				if (chunked) {
					this.chunked = true;
					context.Response.SendChunked = true;
					i_stream = new ChunkedInputStream (context, stream, buffer, position, length - position);
				} else {
					i_stream = new RequestStream (stream, buffer, position, length - position, contentlength);
				}
			}
			return i_stream;
		}

		public ResponseStream GetResponseStream ()
		{
			// TODO: can we get this stream before reading the input?
			if (o_stream == null) {
				HttpListener listener = context.Listener;
				bool ign = (listener == null) ? true : listener.IgnoreWriteExceptions;
				o_stream = new ResponseStream (stream, context.Response, ign);
			}
			return o_stream;
		}

		void OnRead (IAsyncResult ares)
		{
			timer.Change (Timeout.Infinite, Timeout.Infinite);
			HttpConnection cnc = (HttpConnection) ares.AsyncState;
			int nread = -1;
			try {
				nread = stream.EndRead (ares);
				ms.Write (buffer, 0, nread);
				if (ms.Length > 32768) {
					SendError ("Bad request", 400);
					Close (true);
					return;
				}
			} catch {
				if (ms != null && ms.Length > 0)
					SendError ();
				if (sock != null)
					CloseSocket ();
				return;
			}

			if (nread == 0) {
				//if (ms.Length > 0)
				//	SendError (); // Why bother?
				CloseSocket ();
				return;
			}

			if (ProcessInput (ms)) {
				if (!context.HaveError)
					context.Request.FinishInitialization ();

				if (context.HaveError) {
					SendError ();
					Close (true);
					return;
				}

				if (!epl.BindContext (context)) {
					SendError ("Invalid host", 400);
					Close (true);
				}
				context_bound = true;
				return;
			}
			stream.BeginRead (buffer, 0, BufferSize, OnRead, cnc);
		}

		enum InputState {
			RequestLine,
			Headers
		}

		enum LineState {
			None,
			CR,
			LF
		}

		InputState input_state = InputState.RequestLine;
		LineState line_state = LineState.None;
		int position;

		// true -> done processing
		// false -> need more input
		bool ProcessInput (MemoryStream ms)
		{
			byte [] buffer = ms.GetBuffer ();
			int len = (int) ms.Length;
			int used = 0;
			string line;

			try {
				line = ReadLine (buffer, position, len - position, ref used);
				position += used;
			} catch {
				context.ErrorMessage = "Bad request";
				context.ErrorStatus = 400;
				return true;
			}

			do {
				if (line == null)
					break;
				if (line == "") {
					if (input_state == InputState.RequestLine)
						continue;
					current_line = null;
					ms = null;
					return true;
				}

				if (input_state == InputState.RequestLine) {
					context.Request.SetRequestLine (line);
					input_state = InputState.Headers;
				} else {
					try {
						context.Request.AddHeader (line);
					} catch (Exception e) {
						context.ErrorMessage = e.Message;
						context.ErrorStatus = 400;
						return true;
					}
				}

				if (context.HaveError)
					return true;

				if (position >= len)
					break;
				try {
					line = ReadLine (buffer, position, len - position, ref used);
					position += used;
				} catch {
					context.ErrorMessage = "Bad request";
					context.ErrorStatus = 400;
					return true;
				}
			} while (line != null);

			if (used == len) {
				ms.SetLength (0);
				position = 0;
			}
			return false;
		}

		string ReadLine (byte [] buffer, int offset, int len, ref int used)
		{
			if (current_line == null)
				current_line = new StringBuilder ();
			int last = offset + len;
			used = 0;
			for (int i = offset; i < last && line_state != LineState.LF; i++) {
				used++;
				byte b = buffer [i];
				if (b == 13) {
					line_state = LineState.CR;
				} else if (b == 10) {
					line_state = LineState.LF;
				} else {
					current_line.Append ((char) b);
				}
			}

			string result = null;
			if (line_state == LineState.LF) {
				line_state = LineState.None;
				result = current_line.ToString ();
				current_line.Length = 0;
			}

			return result;
		}

		public void SendError (string msg, int status)
		{
			try {
				HttpListenerResponse response = context.Response;
				response.StatusCode = status;
				response.ContentType = "text/html";
				string description = HttpListenerResponse.GetStatusDescription (status);
				string str;
				if (msg != null)
					str = String.Format ("<h1>{0} ({1})</h1>", description, msg);
				else
					str = String.Format ("<h1>{0}</h1>", description);

				byte [] error = context.Response.ContentEncoding.GetBytes (str);
				response.Close (error, false);
			} catch {
				// response was already closed
			}
		}

		public void SendError ()
		{
			SendError (context.ErrorMessage, context.ErrorStatus);
		}

		void Unbind ()
		{
			if (context_bound) {
				epl.UnbindContext (context);
				context_bound = false;
			}
		}

		public void Close ()
		{
			Close (false);
		}

		void CloseSocket ()
		{
			if (sock == null)
				return;

			try {
				sock.Close ();
			} catch {
			} finally {
				sock = null;
			}
		}

		internal void Close (bool force_close)
		{
			if (sock != null) {
				Stream st = GetResponseStream ();
				st.Close ();
				o_stream = null;
			}

			if (sock != null) {
				force_close |= !context.Request.KeepAlive;
				if (!force_close)
					force_close = (context.Response.Headers ["connection"] == "close");
				/*
				if (!force_close) {
//					bool conn_close = (status_code == 400 || status_code == 408 || status_code == 411 ||
//							status_code == 413 || status_code == 414 || status_code == 500 ||
//							status_code == 503);

					force_close |= (context.Request.ProtocolVersion <= HttpVersion.Version10);
				}
				*/

				if (!force_close && context.Request.FlushInput ()) {
					if (chunked && context.Response.ForceCloseChunked == false) {
						// Don't close. Keep working.
						reuses++;
						Unbind ();
						Init ();
						BeginReadRequest ();
						return;
					}

					reuses++;
					Unbind ();
					Init ();
					BeginReadRequest ();
					return;
				}

				Socket s = sock;
				sock = null;
				try {
					if (s != null)
						s.Shutdown (SocketShutdown.Both);
				} catch {
				} finally {
					if (s != null)
						s.Close ();
				}
				Unbind ();
				return;
			}
		}
	}
}
#endif

