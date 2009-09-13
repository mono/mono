#define EMBEDDED_IN_1_0

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

#if EMBEDDED_IN_1_0

using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
#if !EMBEDDED_IN_1_0
using Mono.Security.Protocol.Tls;
#endif

using System; using System.Net; namespace MonoHttp {

	interface IHttpListenerContextBinder {
		bool BindContext (HttpListenerContext context);
		void UnbindContext (HttpListenerContext context);
	}

	sealed class HttpConnection
	{
		const int BufferSize = 8192;
		Socket sock;
		Stream stream;
		IHttpListenerContextBinder epl;
		MemoryStream ms;
		byte [] buffer;
		HttpListenerContext context;
		StringBuilder current_line;
		ListenerPrefix prefix;
		RequestStream i_stream;
		ResponseStream o_stream;
		bool chunked;
		int chunked_uses;
		bool context_bound;
		bool secure;
		AsymmetricAlgorithm key;

#if EMBEDDED_IN_1_0
		public HttpConnection (Socket sock, IHttpListenerContextBinder epl)
		{
			this.sock = sock;
			this.epl = epl;
			stream = new NetworkStream (sock, false);
			Init ();
		}
#else
		public HttpConnection (Socket sock, IHttpListenerContextBinder epl, bool secure, X509Certificate2 cert, AsymmetricAlgorithm key)
		{
			this.sock = sock;
			this.epl = epl;
			this.secure = secure;
			this.key = key;
			if (secure == false) {
				stream = new NetworkStream (sock, false);
			} else {
#if EMBEDDED_IN_1_0
				throw new NotImplementedException ();
#else
				SslServerStream ssl_stream = new SslServerStream (new NetworkStream (sock, false), cert, false, false);
				ssl_stream.PrivateKeyCertSelectionDelegate += OnPVKSelection;
				stream = ssl_stream;
#endif
			}
			Init ();
		}
#endif

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

		public int ChunkedUses {
			get { return chunked_uses; }
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

		public void BeginReadRequest ()
		{
			if (buffer == null)
				buffer = new byte [BufferSize];
			try {
				stream.BeginRead (buffer, 0, BufferSize, OnRead, this);
			} catch {
				sock.Close (); // stream disposed
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
				bool ign = false;// ? true : listener.IgnoreWriteExceptions;
				o_stream = new ResponseStream (stream, context.Response, ign);
			}
			return o_stream;
		}

		void OnRead (IAsyncResult ares)
		{
			// TODO: set a limit on ms length.
			HttpConnection cnc = (HttpConnection) ares.AsyncState;
			int nread = -1;
			try {
				nread = stream.EndRead (ares);
				ms.Write (buffer, 0, nread);
			} catch (Exception e) {
				//Console.WriteLine (e);
				if (ms.Length > 0)
					SendError ();
				sock.Close ();
				return;
			}

			if (nread == 0) {
				//if (ms.Length > 0)
				//	SendError (); // Why bother?
				sock.Close ();
				return;
			}

			if (ProcessInput (ms)) {
				if (!context.HaveError)
					context.Request.FinishInitialization ();

				if (context.HaveError) {
					SendError ();
					Close ();
					return;
				}

				if (!epl.BindContext (context)) {
					SendError ("Invalid host", 400);
					Close ();
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
			while ((line = ReadLine (buffer, position, len - position, ref used)) != null) {
				position += used;
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
					context.Request.AddHeader (line);
				}

				if (context.HaveError)
					return true;

				if (position >= len)
					break;
			}

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

		internal void Close (bool force_close)
		{
			if (sock != null) {
				Stream st = GetResponseStream ();
				st.Close ();
				o_stream = null;
			}

			if (sock != null) {
				if (!force_close && chunked && context.Response.ForceCloseChunked == false) {
					// Don't close. Keep working.
					chunked_uses++;
					Unbind ();
					Init ();
					BeginReadRequest ();
					return;
				}

				if (force_close || context.Response.Headers ["connection"] == "close") {
					Socket s = sock;
					sock = null;
					try {
						s.Shutdown (SocketShutdown.Both);
					} catch {
					} finally {
						s.Close ();
					}
					Unbind ();
				} else {
					Unbind ();
					Init ();
					BeginReadRequest ();
					return;
				}
			}
		}
	}
}
#endif

