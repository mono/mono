//
// System.Net.WebConnection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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

#if SECURITY_DEP

extern alias MonoSecurity;

using MonoSecurity::Mono.Security.Protocol.Tls;
#endif

using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace System.Net
{
	enum ReadState
	{
		None,
		Status,
		Headers,
		Content,
		Aborted
	}

	class WebConnection
	{
		ServicePoint sPoint;
		Stream nstream;
		Socket socket;
		object socketLock = new object ();
		WebExceptionStatus status;
		WaitCallback initConn;
		bool keepAlive;
		byte [] buffer;
		static AsyncCallback readDoneDelegate = new AsyncCallback (ReadDone);
		EventHandler abortHandler;
		AbortHelper abortHelper;
		internal WebConnectionData Data;
		bool chunkedRead;
		ChunkStream chunkStream;
		Queue queue;
		bool reused;
		int position;
		bool busy;		
		HttpWebRequest priority_request;		
		NetworkCredential ntlm_credentials;
		bool ntlm_authenticated;
		bool unsafe_sharing;

		enum NtlmAuthState
		{
			None,
			Challenge,
			Response
		}
		NtlmAuthState connect_ntlm_auth_state;
		HttpWebRequest connect_request;

		bool ssl;
		bool certsAvailable;
		Exception connect_exception;
		static object classLock = new object ();
		static Type sslStream;
		static PropertyInfo piClient;
		static PropertyInfo piServer;
		static PropertyInfo piTrustFailure;

#if MONOTOUCH
                static MethodInfo start_wwan;

                static WebConnection ()
                {
                        Type type = Type.GetType ("MonoTouch.ObjCRuntime.Runtime, monotouch");
			if (type != null)
	                        start_wwan = type.GetMethod ("StartWWAN", new Type [] { typeof (System.Uri) });
                }
#endif

		public WebConnection (WebConnectionGroup group, ServicePoint sPoint)
		{
			this.sPoint = sPoint;
			buffer = new byte [4096];
			Data = new WebConnectionData ();
			initConn = new WaitCallback (state => {
				try {
					InitConnection (state);
				} catch {}
				});
			queue = group.Queue;
			abortHelper = new AbortHelper ();
			abortHelper.Connection = this;
			abortHandler = new EventHandler (abortHelper.Abort);
		}

		class AbortHelper {
			public WebConnection Connection;

			public void Abort (object sender, EventArgs args)
			{
				WebConnection other = ((HttpWebRequest) sender).WebConnection;
				if (other == null)
					other = Connection;
				other.Abort (sender, args);
			}
		}

		bool CanReuse ()
		{
			// The real condition is !(socket.Poll (0, SelectMode.SelectRead) || socket.Available != 0)
			// but if there's data pending to read (!) we won't reuse the socket.
			return (socket.Poll (0, SelectMode.SelectRead) == false);
		}
		
		void Connect (HttpWebRequest request)
		{
			lock (socketLock) {
				if (socket != null && socket.Connected && status == WebExceptionStatus.Success) {
					// Take the chunked stream to the expected state (State.None)
					if (CanReuse () && CompleteChunkedRead ()) {
						reused = true;
						return;
					}
				}

				reused = false;
				if (socket != null) {
					socket.Close();
					socket = null;
				}

				chunkStream = null;
				IPHostEntry hostEntry = sPoint.HostEntry;

				if (hostEntry == null) {
#if MONOTOUCH
					if (start_wwan != null) {
						start_wwan.Invoke (null, new object [1] { sPoint.Address });
						hostEntry = sPoint.HostEntry;
					}
					if (hostEntry == null) {
#endif
						status = sPoint.UsesProxy ? WebExceptionStatus.ProxyNameResolutionFailure :
									    WebExceptionStatus.NameResolutionFailure;
						return;
#if MONOTOUCH
					}
#endif
				}

				//WebConnectionData data = Data;
				foreach (IPAddress address in hostEntry.AddressList) {
					try {
						socket = new Socket (address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					} catch (Exception se) {
						// The Socket ctor can throw if we run out of FD's
						if (!request.Aborted)
								status = WebExceptionStatus.ConnectFailure;
						connect_exception = se;
						return;
					}
					IPEndPoint remote = new IPEndPoint (address, sPoint.Address.Port);
					socket.NoDelay = !sPoint.UseNagleAlgorithm;
					try {
						sPoint.KeepAliveSetup (socket);
					} catch {
						// Ignore. Not supported in all platforms.
					}

					if (!sPoint.CallEndPointDelegate (socket, remote)) {
						socket.Close ();
						socket = null;
						status = WebExceptionStatus.ConnectFailure;
					} else {
						try {
							if (request.Aborted)
								return;
							socket.Connect (remote);
							status = WebExceptionStatus.Success;
							break;
						} catch (ThreadAbortException) {
							// program exiting...
							Socket s = socket;
							socket = null;
							if (s != null)
								s.Close ();
							return;
						} catch (ObjectDisposedException) {
							// socket closed from another thread
							return;
						} catch (Exception exc) {
							Socket s = socket;
							socket = null;
							if (s != null)
								s.Close ();
							if (!request.Aborted)
								status = WebExceptionStatus.ConnectFailure;
							connect_exception = exc;
						}
					}
				}
			}
		}

		static void EnsureSSLStreamAvailable ()
		{
			lock (classLock) {
				if (sslStream != null)
					return;

#if NET_2_1 && SECURITY_DEP
				sslStream = typeof (HttpsClientStream);
#else
				// HttpsClientStream is an internal glue class in Mono.Security.dll
				sslStream = Type.GetType ("Mono.Security.Protocol.Tls.HttpsClientStream, " +
							Consts.AssemblyMono_Security, false);

				if (sslStream == null) {
					string msg = "Missing Mono.Security.dll assembly. " +
							"Support for SSL/TLS is unavailable.";

					throw new NotSupportedException (msg);
				}
#endif
				piClient = sslStream.GetProperty ("SelectedClientCertificate");
				piServer = sslStream.GetProperty ("ServerCertificate");
				piTrustFailure = sslStream.GetProperty ("TrustFailure");
			}
		}

		bool CreateTunnel (HttpWebRequest request, Uri connectUri,
		                   Stream stream, out byte[] buffer)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("CONNECT ");
			sb.Append (request.Address.Host);
			sb.Append (':');
			sb.Append (request.Address.Port);
			sb.Append (" HTTP/");
			if (request.ServicePoint.ProtocolVersion == HttpVersion.Version11)
				sb.Append ("1.1");
			else
				sb.Append ("1.0");

			sb.Append ("\r\nHost: ");
			sb.Append (request.Address.Authority);

			bool ntlm = false;
			var challenge = Data.Challenge;
			Data.Challenge = null;
			var auth_header = request.Headers ["Proxy-Authorization"];
			bool have_auth = auth_header != null;
			if (have_auth) {
				sb.Append ("\r\nProxy-Authorization: ");
				sb.Append (auth_header);
				ntlm = auth_header.ToUpper ().Contains ("NTLM");
			} else if (challenge != null && Data.StatusCode == 407) {
				ICredentials creds = request.Proxy.Credentials;
				have_auth = true;

				if (connect_request == null) {
					// create a CONNECT request to use with Authenticate
					connect_request = (HttpWebRequest)WebRequest.Create (
						connectUri.Scheme + "://" + connectUri.Host + ":" + connectUri.Port + "/");
					connect_request.Method = "CONNECT";
					connect_request.Credentials = creds;
				}

				for (int i = 0; i < challenge.Length; i++) {
					var auth = AuthenticationManager.Authenticate (challenge [i], connect_request, creds);
					if (auth == null)
						continue;
					ntlm = (auth.Module.AuthenticationType == "NTLM");
					sb.Append ("\r\nProxy-Authorization: ");
					sb.Append (auth.Message);
					break;
				}
			}

			if (ntlm) {
				sb.Append ("\r\nProxy-Connection: keep-alive");
				connect_ntlm_auth_state++;
			}

			sb.Append ("\r\n\r\n");

			Data.StatusCode = 0;
			byte [] connectBytes = Encoding.Default.GetBytes (sb.ToString ());
			stream.Write (connectBytes, 0, connectBytes.Length);

			int status;
			WebHeaderCollection result = ReadHeaders (stream, out buffer, out status);
			if ((!have_auth || connect_ntlm_auth_state == NtlmAuthState.Challenge) &&
			    result != null && status == 407) { // Needs proxy auth
				var connectionHeader = result ["Connection"];
				if (socket != null && !string.IsNullOrEmpty (connectionHeader) &&
				    connectionHeader.ToLower() == "close") {
					// The server is requesting that this connection be closed
					socket.Close();
					socket = null;
				}

				Data.StatusCode = status;
				Data.Challenge = result.GetValues_internal ("Proxy-Authenticate", false);
				return false;
			} else if (status != 200) {
				string msg = String.Format ("The remote server returned a {0} status code.", status);
				HandleError (WebExceptionStatus.SecureChannelFailure, null, msg);
				return false;
			}

			return (result != null);
		}

		WebHeaderCollection ReadHeaders (Stream stream, out byte [] retBuffer, out int status)
		{
			retBuffer = null;
			status = 200;

			byte [] buffer = new byte [1024];
			MemoryStream ms = new MemoryStream ();
			bool gotStatus = false;
			WebHeaderCollection headers = null;

			while (true) {
				int n = stream.Read (buffer, 0, 1024);
				if (n == 0) {
					HandleError (WebExceptionStatus.ServerProtocolViolation, null, "ReadHeaders");
					return null;
				}
				
				ms.Write (buffer, 0, n);
				int start = 0;
				string str = null;
				headers = new WebHeaderCollection ();
				while (ReadLine (ms.GetBuffer (), ref start, (int) ms.Length, ref str)) {
					if (str == null) {
						int contentLen = 0;
						try	{
							contentLen = int.Parse(headers["Content-Length"]);
						}
						catch {
							contentLen = 0;
						}

						if (ms.Length - start - contentLen > 0)	{
							// we've read more data than the response header and conents,
							// give back extra data to the caller
							retBuffer = new byte[ms.Length - start - contentLen];
							Buffer.BlockCopy(ms.GetBuffer(), start + contentLen, retBuffer, 0, retBuffer.Length);
						}
						else {
							// haven't read in some or all of the contents for the response, do so now
							FlushContents(stream, contentLen - (int)(ms.Length - start));
						}

						return headers;
					}

					if (gotStatus) {
						headers.Add (str);
						continue;
					}

					int spaceidx = str.IndexOf (' ');
					if (spaceidx == -1) {
						HandleError (WebExceptionStatus.ServerProtocolViolation, null, "ReadHeaders2");
						return null;
					}

					status = (int) UInt32.Parse (str.Substring (spaceidx + 1, 3));
					gotStatus = true;
				}
			}
		}

		void FlushContents(Stream stream, int contentLength)
		{
			while (contentLength > 0) {
				byte[] contentBuffer = new byte[contentLength];
				int bytesRead = stream.Read(contentBuffer, 0, contentLength);
				if (bytesRead > 0) {
					contentLength -= bytesRead;
				}
				else {
					break;
				}
			}
		}

		bool CreateStream (HttpWebRequest request)
		{
			try {
				NetworkStream serverStream = new NetworkStream (socket, false);

				if (request.Address.Scheme == Uri.UriSchemeHttps) {
					ssl = true;
					EnsureSSLStreamAvailable ();
					if (!reused || nstream == null || nstream.GetType () != sslStream) {
						byte [] buffer = null;
						if (sPoint.UseConnect) {
							bool ok = CreateTunnel (request, sPoint.Address, serverStream, out buffer);
							if (!ok)
								return false;
						}

						object[] args = new object [4] { serverStream,
										request.ClientCertificates,
										request, buffer};
						nstream = (Stream) Activator.CreateInstance (sslStream, args);
#if SECURITY_DEP
						SslClientStream scs = (SslClientStream) nstream;
						var helper = new ServicePointManager.ChainValidationHelper (request);
						scs.ServerCertValidation2 += new CertificateValidationCallback2 (helper.ValidateChain);
#endif
						certsAvailable = false;
					}
					// we also need to set ServicePoint.Certificate 
					// and ServicePoint.ClientCertificate but this can
					// only be done later (after handshake - which is
					// done only after a read operation).
				} else {
					ssl = false;
					nstream = serverStream;
				}
			} catch (Exception) {
				if (!request.Aborted)
					status = WebExceptionStatus.ConnectFailure;
				return false;
			}

			return true;
		}
		
		void HandleError (WebExceptionStatus st, Exception e, string where)
		{
			status = st;
			lock (this) {
				if (st == WebExceptionStatus.RequestCanceled)
					Data = new WebConnectionData ();
			}

			if (e == null) { // At least we now where it comes from
				try {
#if TARGET_JVM
					throw new Exception ();
#else
					throw new Exception (new System.Diagnostics.StackTrace ().ToString ());
#endif
				} catch (Exception e2) {
					e = e2;
				}
			}

			HttpWebRequest req = null;
			if (Data != null && Data.request != null)
				req = Data.request;

			Close (true);
			if (req != null) {
				req.FinishedReading = true;
				req.SetResponseError (st, e, where);
			}
		}
		
		static void ReadDone (IAsyncResult result)
		{
			WebConnection cnc = (WebConnection)result.AsyncState;
			WebConnectionData data = cnc.Data;
			Stream ns = cnc.nstream;
			if (ns == null) {
				cnc.Close (true);
				return;
			}

			int nread = -1;
			try {
				nread = ns.EndRead (result);
			} catch (ObjectDisposedException) {
				return;
			} catch (Exception e) {
				if (e.InnerException is ObjectDisposedException)
					return;

				cnc.HandleError (WebExceptionStatus.ReceiveFailure, e, "ReadDone1");
				return;
			}

			if (nread == 0) {
				cnc.HandleError (WebExceptionStatus.ReceiveFailure, null, "ReadDone2");
				return;
			}

			if (nread < 0) {
				cnc.HandleError (WebExceptionStatus.ServerProtocolViolation, null, "ReadDone3");
				return;
			}

			int pos = -1;
			nread += cnc.position;
			if (data.ReadState == ReadState.None) { 
				Exception exc = null;
				try {
					pos = GetResponse (data, cnc.sPoint, cnc.buffer, nread);
				} catch (Exception e) {
					exc = e;
				}

				if (exc != null || pos == -1) {
					cnc.HandleError (WebExceptionStatus.ServerProtocolViolation, exc, "ReadDone4");
					return;
				}
			}

			if (data.ReadState == ReadState.Aborted) {
				cnc.HandleError (WebExceptionStatus.RequestCanceled, null, "ReadDone");
				return;
			}

			if (data.ReadState != ReadState.Content) {
				int est = nread * 2;
				int max = (est < cnc.buffer.Length) ? cnc.buffer.Length : est;
				byte [] newBuffer = new byte [max];
				Buffer.BlockCopy (cnc.buffer, 0, newBuffer, 0, nread);
				cnc.buffer = newBuffer;
				cnc.position = nread;
				data.ReadState = ReadState.None;
				InitRead (cnc);
				return;
			}

			cnc.position = 0;

			WebConnectionStream stream = new WebConnectionStream (cnc);
			bool expect_content = ExpectContent (data.StatusCode, data.request.Method);
			string tencoding = null;
			if (expect_content)
				tencoding = data.Headers ["Transfer-Encoding"];

			cnc.chunkedRead = (tencoding != null && tencoding.IndexOf ("chunked", StringComparison.OrdinalIgnoreCase) != -1);
			if (!cnc.chunkedRead) {
				stream.ReadBuffer = cnc.buffer;
				stream.ReadBufferOffset = pos;
				stream.ReadBufferSize = nread;
				try {
					stream.CheckResponseInBuffer ();
				} catch (Exception e) {
					cnc.HandleError (WebExceptionStatus.ReceiveFailure, e, "ReadDone7");
				}
			} else if (cnc.chunkStream == null) {
				try {
					cnc.chunkStream = new ChunkStream (cnc.buffer, pos, nread, data.Headers);
				} catch (Exception e) {
					cnc.HandleError (WebExceptionStatus.ServerProtocolViolation, e, "ReadDone5");
					return;
				}
			} else {
				cnc.chunkStream.ResetBuffer ();
				try {
					cnc.chunkStream.Write (cnc.buffer, pos, nread);
				} catch (Exception e) {
					cnc.HandleError (WebExceptionStatus.ServerProtocolViolation, e, "ReadDone6");
					return;
				}
			}

			data.stream = stream;
			
			if (!expect_content)
				stream.ForceCompletion ();

			data.request.SetResponseData (data);
		}

		static bool ExpectContent (int statusCode, string method)
		{
			if (method == "HEAD")
				return false;
			return (statusCode >= 200 && statusCode != 204 && statusCode != 304);
		}

		internal void GetCertificates () 
		{
			// here the SSL negotiation have been done
			X509Certificate client = (X509Certificate) piClient.GetValue (nstream, null);
			X509Certificate server = (X509Certificate) piServer.GetValue (nstream, null);
			sPoint.SetCertificates (client, server);
			certsAvailable = (server != null);
		}

		internal static void InitRead (object state)
		{
			WebConnection cnc = (WebConnection) state;
			Stream ns = cnc.nstream;

			try {
				int size = cnc.buffer.Length - cnc.position;
				ns.BeginRead (cnc.buffer, cnc.position, size, readDoneDelegate, cnc);
			} catch (Exception e) {
				cnc.HandleError (WebExceptionStatus.ReceiveFailure, e, "InitRead");
			}
		}
		
		static int GetResponse (WebConnectionData data, ServicePoint sPoint,
		                        byte [] buffer, int max)
		{
			int pos = 0;
			string line = null;
			bool lineok = false;
			bool isContinue = false;
			bool emptyFirstLine = false;
			do {
				if (data.ReadState == ReadState.Aborted)
					return -1;

				if (data.ReadState == ReadState.None) {
					lineok = ReadLine (buffer, ref pos, max, ref line);
					if (!lineok)
						return 0;

					if (line == null) {
						emptyFirstLine = true;
						continue;
					}
					emptyFirstLine = false;
					data.ReadState = ReadState.Status;

					string [] parts = line.Split (' ');
					if (parts.Length < 2)
						return -1;

					if (String.Compare (parts [0], "HTTP/1.1", true) == 0) {
						data.Version = HttpVersion.Version11;
						sPoint.SetVersion (HttpVersion.Version11);
					} else {
						data.Version = HttpVersion.Version10;
						sPoint.SetVersion (HttpVersion.Version10);
					}

					data.StatusCode = (int) UInt32.Parse (parts [1]);
					if (parts.Length >= 3)
						data.StatusDescription = String.Join (" ", parts, 2, parts.Length - 2);
					else
						data.StatusDescription = "";

					if (pos >= max)
						return pos;
				}

				emptyFirstLine = false;
				if (data.ReadState == ReadState.Status) {
					data.ReadState = ReadState.Headers;
					data.Headers = new WebHeaderCollection ();
					ArrayList headers = new ArrayList ();
					bool finished = false;
					while (!finished) {
						if (ReadLine (buffer, ref pos, max, ref line) == false)
							break;
					
						if (line == null) {
							// Empty line: end of headers
							finished = true;
							continue;
						}
					
						if (line.Length > 0 && (line [0] == ' ' || line [0] == '\t')) {
							int count = headers.Count - 1;
							if (count < 0)
								break;

							string prev = (string) headers [count] + line;
							headers [count] = prev;
						} else {
							headers.Add (line);
						}
					}

					if (!finished)
						return 0;

					foreach (string s in headers)
						data.Headers.SetInternal (s);

					if (data.StatusCode == (int) HttpStatusCode.Continue) {
						sPoint.SendContinue = true;
						if (pos >= max)
							return pos;

						if (data.request.ExpectContinue) {
							data.request.DoContinueDelegate (data.StatusCode, data.Headers);
							// Prevent double calls when getting the
							// headers in several packets.
							data.request.ExpectContinue = false;
						}

						data.ReadState = ReadState.None;
						isContinue = true;
					}
					else {
						data.ReadState = ReadState.Content;
						return pos;
					}
				}
			} while (emptyFirstLine || isContinue);

			return -1;
		}
		
		void InitConnection (object state)
		{
			HttpWebRequest request = (HttpWebRequest) state;
			request.WebConnection = this;

			if (request.Aborted)
				return;

			keepAlive = request.KeepAlive;
			Data = new WebConnectionData (request);
		retry:
			Connect (request);
			if (request.Aborted)
				return;

			if (status != WebExceptionStatus.Success) {
				if (!request.Aborted) {
					request.SetWriteStreamError (status, connect_exception);
					Close (true);
				}
				return;
			}
			
			if (!CreateStream (request)) {
				if (request.Aborted)
					return;

				WebExceptionStatus st = status;
				if (Data.Challenge != null)
					goto retry;

				Exception cnc_exc = connect_exception;
				connect_exception = null;
				request.SetWriteStreamError (st, cnc_exc);
				Close (true);
				return;
			}

			request.SetWriteStream (new WebConnectionStream (this, request));
		}

#if MONOTOUCH
		static bool warned_about_queue = false;
#endif

		internal EventHandler SendRequest (HttpWebRequest request)
		{
			if (request.Aborted)
				return null;

			lock (this) {
				if (!busy) {
					busy = true;
					status = WebExceptionStatus.Success;
					ThreadPool.QueueUserWorkItem (initConn, request);
				} else {
					lock (queue) {
#if MONOTOUCH
						if (!warned_about_queue) {
							warned_about_queue = true;
							Console.WriteLine ("WARNING: An HttpWebRequest was added to the ConnectionGroup queue because the connection limit was reached.");
						}
#endif
						queue.Enqueue (request);
					}
				}
			}

			return abortHandler;
		}
		
		void SendNext ()
		{
			lock (queue) {
				if (queue.Count > 0) {
					SendRequest ((HttpWebRequest) queue.Dequeue ());
				}
			}
		}

		internal void NextRead ()
		{
			lock (this) {
				if (Data.request != null)
					Data.request.FinishedReading = true;
				string header = (sPoint.UsesProxy) ? "Proxy-Connection" : "Connection";
				string cncHeader = (Data.Headers != null) ? Data.Headers [header] : null;
				bool keepAlive = (Data.Version == HttpVersion.Version11 && this.keepAlive);
				if (cncHeader != null) {
					cncHeader = cncHeader.ToLower ();
					keepAlive = (this.keepAlive && cncHeader.IndexOf ("keep-alive", StringComparison.Ordinal) != -1);
				}

				if ((socket != null && !socket.Connected) ||
				   (!keepAlive || (cncHeader != null && cncHeader.IndexOf ("close", StringComparison.Ordinal) != -1))) {
					Close (false);
				}

				busy = false;
				if (priority_request != null) {
					SendRequest (priority_request);
					priority_request = null;
				} else {
					SendNext ();
				}
			}
		}
		
		static bool ReadLine (byte [] buffer, ref int start, int max, ref string output)
		{
			bool foundCR = false;
			StringBuilder text = new StringBuilder ();

			int c = 0;
			while (start < max) {
				c = (int) buffer [start++];

				if (c == '\n') {			// newline
					if ((text.Length > 0) && (text [text.Length - 1] == '\r'))
						text.Length--;

					foundCR = false;
					break;
				} else if (foundCR) {
					text.Length--;
					break;
				}

				if (c == '\r')
					foundCR = true;
					

				text.Append ((char) c);
			}

			if (c != '\n' && c != '\r')
				return false;

			if (text.Length == 0) {
				output = null;
				return (c == '\n' || c == '\r');
			}

			if (foundCR)
				text.Length--;

			output = text.ToString ();
			return true;
		}


		internal IAsyncResult BeginRead (HttpWebRequest request, byte [] buffer, int offset, int size, AsyncCallback cb, object state)
		{
			Stream s = null;
			lock (this) {
				if (Data.request != request)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				if (nstream == null)
					return null;
				s = nstream;
			}

			IAsyncResult result = null;
			if (!chunkedRead || (!chunkStream.DataAvailable && chunkStream.WantMore)) {
				try {
					result = s.BeginRead (buffer, offset, size, cb, state);
					cb = null;
				} catch (Exception) {
					HandleError (WebExceptionStatus.ReceiveFailure, null, "chunked BeginRead");
					throw;
				}
			}

			if (chunkedRead) {
				WebAsyncResult wr = new WebAsyncResult (cb, state, buffer, offset, size);
				wr.InnerAsyncResult = result;
				if (result == null) {
					// Will be completed from the data in ChunkStream
					wr.SetCompleted (true, (Exception) null);
					wr.DoCallback ();
				}
				return wr;
			}

			return result;
		}
		
		internal int EndRead (HttpWebRequest request, IAsyncResult result)
		{
			Stream s = null;
			lock (this) {
				if (Data.request != request)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				if (nstream == null)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				s = nstream;
			}

			int nbytes = 0;
			WebAsyncResult wr = null;
			IAsyncResult nsAsync = ((WebAsyncResult) result).InnerAsyncResult;
			if (chunkedRead && (nsAsync is WebAsyncResult)) {
				wr = (WebAsyncResult) nsAsync;
				IAsyncResult inner = wr.InnerAsyncResult;
				if (inner != null && !(inner is WebAsyncResult))
					nbytes = s.EndRead (inner);
			} else if (!(nsAsync is WebAsyncResult)) {
				nbytes = s.EndRead (nsAsync);
				wr = (WebAsyncResult) result;
			}

			if (chunkedRead) {
				bool done = (nbytes == 0);
				try {
					chunkStream.WriteAndReadBack (wr.Buffer, wr.Offset, wr.Size, ref nbytes);
					if (!done && nbytes == 0 && chunkStream.WantMore)
						nbytes = EnsureRead (wr.Buffer, wr.Offset, wr.Size);
				} catch (Exception e) {
					if (e is WebException)
						throw e;

					throw new WebException ("Invalid chunked data.", e,
								WebExceptionStatus.ServerProtocolViolation, null);
				}

				if ((done || nbytes == 0) && chunkStream.ChunkLeft != 0) {
					HandleError (WebExceptionStatus.ReceiveFailure, null, "chunked EndRead");
					throw new WebException ("Read error", null, WebExceptionStatus.ReceiveFailure, null);
				}
			}

			return (nbytes != 0) ? nbytes : -1;
		}

		// To be called on chunkedRead when we can read no data from the ChunkStream yet
		int EnsureRead (byte [] buffer, int offset, int size)
		{
			byte [] morebytes = null;
			int nbytes = 0;
			while (nbytes == 0 && chunkStream.WantMore) {
				int localsize = chunkStream.ChunkLeft;
				if (localsize <= 0) // not read chunk size yet
					localsize = 1024;
				else if (localsize > 16384)
					localsize = 16384;

				if (morebytes == null || morebytes.Length < localsize)
					morebytes = new byte [localsize];

				int nread = nstream.Read (morebytes, 0, localsize);
				if (nread <= 0)
					return 0; // Error

				chunkStream.Write (morebytes, 0, nread);
				nbytes += chunkStream.Read (buffer, offset + nbytes, size - nbytes);
			}

			return nbytes;
		}

		bool CompleteChunkedRead()
		{
			if (!chunkedRead || chunkStream == null)
				return true;

			while (chunkStream.WantMore) {
				int nbytes = nstream.Read (buffer, 0, buffer.Length);
				if (nbytes <= 0)
					return false; // Socket was disconnected

				chunkStream.Write (buffer, 0, nbytes);
			}

			return true;
  		}

		internal IAsyncResult BeginWrite (HttpWebRequest request, byte [] buffer, int offset, int size, AsyncCallback cb, object state)
		{
			Stream s = null;
			lock (this) {
				if (Data.request != request)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				if (nstream == null)
					return null;
				s = nstream;
			}

			IAsyncResult result = null;
			try {
				result = s.BeginWrite (buffer, offset, size, cb, state);
			} catch (Exception) {
				status = WebExceptionStatus.SendFailure;
				throw;
			}

			return result;
		}

		internal void EndWrite2 (HttpWebRequest request, IAsyncResult result)
		{
			if (request.FinishedReading)
				return;

			Stream s = null;
			lock (this) {
				if (Data.request != request)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				if (nstream == null)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				s = nstream;
			}

			try {
				s.EndWrite (result);
			} catch (Exception exc) {
				status = WebExceptionStatus.SendFailure;
				if (exc.InnerException != null)
					throw exc.InnerException;
				throw;
			}
		}

		internal bool EndWrite (HttpWebRequest request, IAsyncResult result)
		{
			if (request.FinishedReading)
				return true;

			Stream s = null;
			lock (this) {
				if (Data.request != request)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				if (nstream == null)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				s = nstream;
			}

			try {
				s.EndWrite (result);
				return true;
			} catch {
				status = WebExceptionStatus.SendFailure;
				return false;
			}
		}

		internal int Read (HttpWebRequest request, byte [] buffer, int offset, int size)
		{
			Stream s = null;
			lock (this) {
				if (Data.request != request)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				if (nstream == null)
					return 0;
				s = nstream;
			}

			int result = 0;
			try {
				bool done = false;
				if (!chunkedRead) {
					result = s.Read (buffer, offset, size);
					done = (result == 0);
				}

				if (chunkedRead) {
					try {
						chunkStream.WriteAndReadBack (buffer, offset, size, ref result);
						if (!done && result == 0 && chunkStream.WantMore)
							result = EnsureRead (buffer, offset, size);
					} catch (Exception e) {
						HandleError (WebExceptionStatus.ReceiveFailure, e, "chunked Read1");
						throw;
					}

					if ((done || result == 0) && chunkStream.WantMore) {
						HandleError (WebExceptionStatus.ReceiveFailure, null, "chunked Read2");
						throw new WebException ("Read error", null, WebExceptionStatus.ReceiveFailure, null);
					}
				}
			} catch (Exception e) {
				HandleError (WebExceptionStatus.ReceiveFailure, e, "Read");
			}

			return result;
		}

		internal bool Write (HttpWebRequest request, byte [] buffer, int offset, int size, ref string err_msg)
		{
			err_msg = null;
			Stream s = null;
			lock (this) {
				if (Data.request != request)
					throw new ObjectDisposedException (typeof (NetworkStream).FullName);
				if (nstream == null)
					return false;
				s = nstream;
			}

			try {
				s.Write (buffer, offset, size);
				// here SSL handshake should have been done
				if (ssl && !certsAvailable)
					GetCertificates ();
			} catch (Exception e) {
				err_msg = e.Message;
				WebExceptionStatus wes = WebExceptionStatus.SendFailure;
				string msg = "Write: " + err_msg;
				if (e is WebException) {
					HandleError (wes, e, msg);
					return false;
				}

				// if SSL is in use then check for TrustFailure
				if (ssl && (bool) piTrustFailure.GetValue (nstream, null)) {
					wes = WebExceptionStatus.TrustFailure;
					msg = "Trust failure";
				}

				HandleError (wes, e, msg);
				return false;
			}
			return true;
		}

		internal void Close (bool sendNext)
		{
			lock (this) {
				if (nstream != null) {
					try {
						nstream.Close ();
					} catch {}
					nstream = null;
				}

				if (socket != null) {
					try {
						socket.Close ();
					} catch {}
					socket = null;
				}

				if (ntlm_authenticated)
					ResetNtlm ();
				if (Data != null) {
					lock (Data) {
						Data.ReadState = ReadState.Aborted;
					}
				}
				busy = false;
				Data = new WebConnectionData ();
				if (sendNext)
					SendNext ();
				
				connect_request = null;
				connect_ntlm_auth_state = NtlmAuthState.None;
			}
		}

		void Abort (object sender, EventArgs args)
		{
			lock (this) {
				lock (queue) {
					HttpWebRequest req = (HttpWebRequest) sender;
					if (Data.request == req || Data.request == null) {
						if (!req.FinishedReading) {
							status = WebExceptionStatus.RequestCanceled;
							Close (false);
							if (queue.Count > 0) {
								Data.request = (HttpWebRequest) queue.Dequeue ();
								SendRequest (Data.request);
							}
						}
						return;
					}

					req.FinishedReading = true;
					req.SetResponseError (WebExceptionStatus.RequestCanceled, null, "User aborted");
					if (queue.Count > 0 && queue.Peek () == sender) {
						queue.Dequeue ();
					} else if (queue.Count > 0) {
						object [] old = queue.ToArray ();
						queue.Clear ();
						for (int i = old.Length - 1; i >= 0; i--) {
							if (old [i] != sender)
								queue.Enqueue (old [i]);
						}
					}
				}
			}
		}

		internal void ResetNtlm ()
		{
			ntlm_authenticated = false;
			ntlm_credentials = null;
			unsafe_sharing = false;
		}

		internal bool Busy {
			get { lock (this) return busy; }
		}
		
		internal bool Connected {
			get {
				lock (this) {
					return (socket != null && socket.Connected);
				}
			}
		}

		// -Used for NTLM authentication
		internal HttpWebRequest PriorityRequest {
			set { priority_request = value; }
		}

		internal bool NtlmAuthenticated {
			get { return ntlm_authenticated; }
			set { ntlm_authenticated = value; }
		}

		internal NetworkCredential NtlmCredential {
			get { return ntlm_credentials; }
			set { ntlm_credentials = value; }
		}

		internal bool UnsafeAuthenticatedConnectionSharing {
			get { return unsafe_sharing; }
			set { unsafe_sharing = value; }
		}
		// -
	}
}

