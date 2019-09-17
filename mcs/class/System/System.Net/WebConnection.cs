//
// System.Net.WebConnection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Martin Baulig <mabaul@microsoft.com>
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2017 Xamarin Inc. (http://www.xamarin.com)
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
using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Diagnostics;
using Mono.Net.Security;

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

	class WebConnection : IDisposable
	{
		NetworkCredential ntlm_credentials;
		bool ntlm_authenticated;
		bool unsafe_sharing;
		Stream networkStream;
		Socket socket;
		MonoTlsStream monoTlsStream;
		WebConnectionTunnel tunnel;
		int disposed;

		public ServicePoint ServicePoint {
			get;
		}

#if MONOTOUCH && !MONOTOUCH_TV && !MONOTOUCH_WATCH
		[System.Runtime.InteropServices.DllImport ("__Internal")]
		static extern void xamarin_start_wwan (string uri);
#endif

		public WebConnection (ServicePoint sPoint)
		{
			ServicePoint = sPoint;
		}

#if MONO_WEB_DEBUG
		internal static bool EnableWebDebug {
			get; set;
		}

		static WebConnection ()
		{
			if (Environment.GetEnvironmentVariable ("MONO_WEB_DEBUG") != null)
				EnableWebDebug = true;
		}
#endif

		[Conditional ("MONO_WEB_DEBUG")]
		internal static void Debug (string message, params object[] args)
		{
#if MONO_WEB_DEBUG
			if (EnableWebDebug)
				Console.Error.WriteLine (string.Format (message, args));
#endif
		}

		[Conditional ("MONO_WEB_DEBUG")]
		internal static void Debug (string message)
		{
#if MONO_WEB_DEBUG
			if (EnableWebDebug)
				Console.Error.WriteLine (message);
#endif
		}

		bool CanReuse ()
		{
			// The real condition is !(socket.Poll (0, SelectMode.SelectRead) || socket.Available != 0)
			// but if there's data pending to read (!) we won't reuse the socket.
			return (socket.Poll (0, SelectMode.SelectRead) == false);
		}

		bool CheckReusable ()
		{
			if (socket != null && socket.Connected) {
				try {
					if (CanReuse ())
						return true;
				} catch { }
			}

			return false;
		}

		async Task Connect (WebOperation operation, CancellationToken cancellationToken)
		{
			IPHostEntry hostEntry = ServicePoint.HostEntry;

			if (hostEntry == null || hostEntry.AddressList.Length == 0) {
#if MONOTOUCH && !MONOTOUCH_TV && !MONOTOUCH_WATCH
					xamarin_start_wwan (ServicePoint.Address.ToString ());
					hostEntry = ServicePoint.HostEntry;
					if (hostEntry == null) {
#endif
				throw GetException (ServicePoint.UsesProxy ? WebExceptionStatus.ProxyNameResolutionFailure :
						    WebExceptionStatus.NameResolutionFailure, null);
#if MONOTOUCH && !MONOTOUCH_TV && !MONOTOUCH_WATCH
					}
#endif
			}

			Exception connectException = null;

			foreach (IPAddress address in hostEntry.AddressList) {
				operation.ThrowIfDisposed (cancellationToken);

				try {
					socket = new Socket (address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				} catch (Exception se) {
					// The Socket ctor can throw if we run out of FD's
					throw GetException (WebExceptionStatus.ConnectFailure, se);
				}
				IPEndPoint remote = new IPEndPoint (address, ServicePoint.Address.Port);
				socket.NoDelay = !ServicePoint.UseNagleAlgorithm;
				try {
					ServicePoint.KeepAliveSetup (socket);
				} catch {
					// Ignore. Not supported in all platforms.
				}

				if (!ServicePoint.CallEndPointDelegate (socket, remote)) {
					Interlocked.Exchange (ref socket, null)?.Close ();
					continue;
				} else {
					try {
						operation.ThrowIfDisposed (cancellationToken);

						/*
						 * Socket.Tasks.cs from CoreFX introduces a new internal
						 * BeginConnect(EndPoint) overload, which will replace
						 * the one we're using from SocketTaskExtensions.cs.
						 *
						 * Our implementation of Socket.BeginConnect() does not
						 * invoke the callback when the request failed synchronously.
						 *
						 * Explicitly use our implementation from SocketTaskExtensions.cs here.
						 */
						await Task.Factory.FromAsync (
							(targetEndPoint, callback, state) => ((Socket)state).BeginConnect (targetEndPoint, callback, state),
							asyncResult => ((Socket)asyncResult.AsyncState).EndConnect (asyncResult),
							remote, socket).ConfigureAwait (false);
					} catch (ObjectDisposedException) {
						throw;
					} catch (Exception exc) {
						Interlocked.Exchange (ref socket, null)?.Close ();
						// Something went wrong, but we might have multiple IP Addresses
						// and need to probe them all.
						connectException = GetException (WebExceptionStatus.ConnectFailure, exc);
						continue;
					}
				}

				if (socket != null)
					return;
			}

			if (connectException == null)
				connectException = GetException (WebExceptionStatus.ConnectFailure, null);

			throw connectException;
		}

#if MONO_WEB_DEBUG
		static int nextID, nextRequestID;
		readonly int id = ++nextID;
		public int ID => disposed != 0 ? -id : id;
#else
		internal readonly int ID;
#endif

		async Task<bool> CreateStream (WebOperation operation, bool reused, CancellationToken cancellationToken)
		{
#if MONO_WEB_DEBUG
			var requestID = ++nextRequestID;
#else
			var requestID = 0;
#endif

			try {
				var stream = new NetworkStream (socket, false);

				Debug ($"WC CREATE STREAM: Cnc={ID} {requestID} {reused} socket={socket.ID}");

				if (operation.Request.Address.Scheme == Uri.UriSchemeHttps) {
					if (!reused || monoTlsStream == null) {
						if (ServicePoint.UseConnect) {
							if (tunnel == null)
								tunnel = new WebConnectionTunnel (operation.Request, ServicePoint.Address);
							await tunnel.Initialize (stream, cancellationToken).ConfigureAwait (false);
							if (!tunnel.Success)
								return false;
						}
						monoTlsStream = new MonoTlsStream (operation.Request, stream);
						networkStream = await monoTlsStream.CreateStream (tunnel, cancellationToken).ConfigureAwait (false);
					}
					return true;
				}

				networkStream = stream;
				return true;
			} catch (Exception ex) {
				ex = HttpWebRequest.FlattenException (ex);
				Debug ($"WC CREATE STREAM EX: Cnc={ID} {requestID} {operation.Aborted} - {ex.Message}");
				if (operation.Aborted || monoTlsStream == null)
					throw GetException (WebExceptionStatus.ConnectFailure, ex);
				throw GetException (monoTlsStream.ExceptionStatus, ex);
			} finally {
				Debug ($"WC CREATE STREAM DONE: Cnc={ID} {requestID}");
			}
		}

		internal async Task<WebRequestStream> InitConnection (WebOperation operation, CancellationToken cancellationToken)
		{
			Debug ($"WC INIT CONNECTION: Cnc={ID} Req={operation.Request.ID} Op={operation.ID}");

			bool reset = true;
		retry:
			operation.ThrowIfClosedOrDisposed (cancellationToken);

			var reused = CheckReusable ();
			Debug ($"WC INIT CONNECTION #1: Cnc={ID} Op={operation.ID} - {reused} - {operation.WriteBuffer != null} {operation.IsNtlmChallenge}");
			if (!reused) {
				CloseSocket ();
				if (reset)
					Reset ();
				try {
					await Connect (operation, cancellationToken).ConfigureAwait (false);
					Debug ($"WC INIT CONNECTION #2: Cnc={ID} Op={operation.ID} {socket.LocalEndPoint}");
				} catch (Exception ex) {
					Debug ($"WC INIT CONNECTION #2 FAILED: Cnc={ID} Op={operation.ID} - {ex.Message}\n{ex}");
					throw;
				}
			}

			var success = await CreateStream (operation, reused, cancellationToken).ConfigureAwait (false);

			Debug ($"WC INIT CONNECTION #3: Cnc={ID} Op={operation.ID} - {success}");
			if (!success) {
				if (tunnel?.Challenge == null)
					throw GetException (WebExceptionStatus.ProtocolError, null);

				if (tunnel.CloseConnection)
					CloseSocket ();
				reset = false;
				goto retry;
			}

			networkStream.ReadTimeout = operation.Request.ReadWriteTimeout;

			return new WebRequestStream (this, operation, networkStream, tunnel);
		}

		internal static WebException GetException (WebExceptionStatus status, Exception error)
		{
			if (error == null)
				return new WebException ($"Error: {status}", status);
			if (error is WebException wex)
				return wex;
			return new WebException ($"Error: {status} ({error.Message})", status,
						 WebExceptionInternalStatus.RequestFatal, error);
		}

		internal static bool ReadLine (byte[] buffer, ref int start, int max, ref string output)
		{
			bool foundCR = false;
			StringBuilder text = new StringBuilder ();

			int c = 0;
			while (start < max) {
				c = (int)buffer[start++];

				if (c == '\n') {                        // newline
					if ((text.Length > 0) && (text[text.Length - 1] == '\r'))
						text.Length--;

					foundCR = false;
					break;
				} else if (foundCR) {
					text.Length--;
					break;
				}

				if (c == '\r')
					foundCR = true;


				text.Append ((char)c);
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

		internal bool CanReuseConnection (WebOperation operation)
		{
			lock (this) {
				if (Closed || currentOperation != null)
					return false;
				if (!NtlmAuthenticated)
					return true;

				NetworkCredential cnc_cred = NtlmCredential;
				var request = operation.Request;

				bool isProxy = (request.Proxy != null && !request.Proxy.IsBypassed (request.RequestUri));
				ICredentials req_icreds = (!isProxy) ? request.Credentials : request.Proxy.Credentials;
				NetworkCredential req_cred = (req_icreds != null) ? req_icreds.GetCredential (request.RequestUri, "NTLM") : null;

				if (cnc_cred == null || req_cred == null ||
					cnc_cred.Domain != req_cred.Domain || cnc_cred.UserName != req_cred.UserName ||
					cnc_cred.Password != req_cred.Password) {
					return false;
				}

				bool req_sharing = request.UnsafeAuthenticatedConnectionSharing;
				bool cnc_sharing = UnsafeAuthenticatedConnectionSharing;
				return !(req_sharing == false || req_sharing != cnc_sharing);
			}
		}

		bool PrepareSharingNtlm (WebOperation operation)
		{
			if (operation == null || !NtlmAuthenticated)
				return true;

			bool needs_reset = false;
			NetworkCredential cnc_cred = NtlmCredential;
			var request = operation.Request;

			bool isProxy = (request.Proxy != null && !request.Proxy.IsBypassed (request.RequestUri));
			ICredentials req_icreds = (!isProxy) ? request.Credentials : request.Proxy.Credentials;
			NetworkCredential req_cred = (req_icreds != null) ? req_icreds.GetCredential (request.RequestUri, "NTLM") : null;

			if (cnc_cred == null || req_cred == null ||
				cnc_cred.Domain != req_cred.Domain || cnc_cred.UserName != req_cred.UserName ||
				cnc_cred.Password != req_cred.Password) {
				needs_reset = true;
			}

			if (!needs_reset) {
				bool req_sharing = request.UnsafeAuthenticatedConnectionSharing;
				bool cnc_sharing = UnsafeAuthenticatedConnectionSharing;
				needs_reset = (req_sharing == false || req_sharing != cnc_sharing);
			}

			return needs_reset;
		}

		void Reset ()
		{
			lock (this) {
				tunnel = null;
				ResetNtlm ();
			}
		}

		void Close (bool reset)
		{
			lock (this) {
				CloseSocket ();
				if (reset)
					Reset ();
			}
		}

		void CloseSocket ()
		{
			lock (this) {
				Debug ($"WC CLOSE SOCKET: Cnc={ID} NS={networkStream} TLS={monoTlsStream}");
				if (networkStream != null) {
					try {
						networkStream.Dispose ();
					} catch { }
					networkStream = null;
				}

				if (monoTlsStream != null) {
					try {
						monoTlsStream.Dispose ();
					} catch { }
					monoTlsStream = null;
				}

				if (socket != null) {
					try {
						socket.Dispose ();
					} catch { }
					socket = null;
				}

				monoTlsStream = null;
			}
		}

		DateTime idleSince;
		WebOperation currentOperation;

		public bool Closed => disposed != 0;

		public bool Busy {
			get { return currentOperation != null; }
		}

		public DateTime IdleSince {
			get { return idleSince; }
		}

		public bool StartOperation (WebOperation operation, bool reused)
		{
			lock (this) {
				if (Closed)
					return false;
				if (Interlocked.CompareExchange (ref currentOperation, operation, null) != null)
					return false;

				idleSince = DateTime.UtcNow + TimeSpan.FromDays (3650);

				if (reused && !PrepareSharingNtlm (operation)) {
					Debug ($"WC START - CAN'T REUSE: Cnc={ID} Op={operation.ID}");
					Close (true);
				}

				operation.RegisterRequest (ServicePoint, this);
				Debug ($"WC START: Cnc={ID} Op={operation.ID}");
			}

			operation.Run ();
			return true;
		}

		public bool Continue (WebOperation next)
		{
			lock (this) {
				if (Closed)
					return false;

				Debug ($"WC CONTINUE: Cnc={ID} connected={socket?.Connected} next={next?.ID} current={currentOperation?.ID}");
				if (socket == null || !socket.Connected || !PrepareSharingNtlm (next)) {
					Close (true);
					return false;
				}

				currentOperation = next;

				if (next == null)
					return true;

				// Ok, we got another connection.  Let's run it!
				next.RegisterRequest (ServicePoint, this);
			}

			next.Run ();
			return true;
		}

		void Dispose (bool disposing)
		{
			if (Interlocked.CompareExchange (ref disposed, 1, 0) != 0)
				return;
			Debug ($"WC DISPOSE: Cnc={ID}");
			Close (true);
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		void ResetNtlm ()
		{
			ntlm_authenticated = false;
			ntlm_credentials = null;
			unsafe_sharing = false;
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

