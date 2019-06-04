//
// System.Net.WebConnectionTunnel
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
	class WebConnectionTunnel
	{
		public HttpWebRequest Request {
			get;
		}

		public Uri ConnectUri {
			get;
		}

		public WebConnectionTunnel (HttpWebRequest request, Uri connectUri)
		{
			Request = request;
			ConnectUri = connectUri;
		}

		enum NtlmAuthState
		{
			None,
			Challenge,
			Response
		}

		HttpWebRequest connectRequest;
		NtlmAuthState ntlmAuthState;

		public bool Success {
			get;
			private set;
		}

		public bool CloseConnection {
			get;
			private set;
		}

		public int StatusCode {
			get;
			private set;
		}

		public string StatusDescription {
			get;
			private set;
		}

		public string[] Challenge {
			get;
			private set;
		}

		public WebHeaderCollection Headers {
			get;
			private set;
		}

		public Version ProxyVersion {
			get;
			private set;
		}

		public byte[] Data {
			get;
			private set;
		}

		internal async Task Initialize (Stream stream, CancellationToken cancellationToken)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("CONNECT ");
			sb.Append (Request.Address.Host);
			sb.Append (':');
			sb.Append (Request.Address.Port);
			sb.Append (" HTTP/");
			if (Request.ProtocolVersion == HttpVersion.Version11)
				sb.Append ("1.1");
			else
				sb.Append ("1.0");

			sb.Append ("\r\nHost: ");
			sb.Append (Request.Address.Authority);

			bool ntlm = false;
			var challenge = Challenge;
			Challenge = null;
			var auth_header = Request.Headers["Proxy-Authorization"];
			bool have_auth = auth_header != null;
			if (have_auth) {
				sb.Append ("\r\nProxy-Authorization: ");
				sb.Append (auth_header);
				ntlm = auth_header.ToUpper ().Contains ("NTLM");
			} else if (challenge != null && StatusCode == 407) {
				ICredentials creds = Request.Proxy.Credentials;
				have_auth = true;

				if (connectRequest == null) {
					// create a CONNECT request to use with Authenticate
					connectRequest = (HttpWebRequest)WebRequest.Create (
						ConnectUri.Scheme + "://" + ConnectUri.Host + ":" + ConnectUri.Port + "/");
					connectRequest.Method = "CONNECT";
					connectRequest.Credentials = creds;
				}

				if (creds != null) {
					for (int i = 0; i < challenge.Length; i++) {
						var auth = AuthenticationManager.Authenticate (challenge[i], connectRequest, creds);
						if (auth == null)
							continue;
						ntlm = (auth.ModuleAuthenticationType == "NTLM");
						sb.Append ("\r\nProxy-Authorization: ");
						sb.Append (auth.Message);
						break;
					}
				}
			}

			if (ntlm) {
				sb.Append ("\r\nProxy-Connection: keep-alive");
				ntlmAuthState++;
			}

			sb.Append ("\r\n\r\n");

			StatusCode = 0;
			byte[] connectBytes = Encoding.Default.GetBytes (sb.ToString ());
			await stream.WriteAsync (connectBytes, 0, connectBytes.Length, cancellationToken).ConfigureAwait (false);

			(Headers, Data, StatusCode) = await ReadHeaders (stream, cancellationToken).ConfigureAwait (false);

			if ((!have_auth || ntlmAuthState == NtlmAuthState.Challenge) && Headers != null && StatusCode == 407) { // Needs proxy auth
				var connectionHeader = Headers["Connection"];
				if (!string.IsNullOrEmpty (connectionHeader) && connectionHeader.ToLower () == "close") {
					// The server is requesting that this connection be closed
					CloseConnection = true;
				}

				Challenge = Headers.GetValues ("Proxy-Authenticate");
				Success = false;
			} else {
				Success = StatusCode == 200 && Headers != null;
			}

			if (Challenge == null && (StatusCode == 401 || StatusCode == 407)) {
				var response = new HttpWebResponse (ConnectUri, "CONNECT", (HttpStatusCode)StatusCode, Headers);
				throw new WebException (
					StatusCode == 407 ? "(407) Proxy Authentication Required" : "(401) Unauthorized",
					null, WebExceptionStatus.ProtocolError, response);
			}
		}

		async Task<(WebHeaderCollection, byte[], int)> ReadHeaders (Stream stream, CancellationToken cancellationToken)
		{
			byte[] retBuffer = null;
			int status = 200;

			byte[] buffer = new byte[1024];
			MemoryStream ms = new MemoryStream ();

			while (true) {
				cancellationToken.ThrowIfCancellationRequested ();
				int n = await stream.ReadAsync (buffer, 0, 1024, cancellationToken).ConfigureAwait (false);
				if (n == 0)
					throw WebConnection.GetException (WebExceptionStatus.ServerProtocolViolation, null);

				ms.Write (buffer, 0, n);
				int start = 0;
				string str = null;
				bool gotStatus = false;
				WebHeaderCollection headers = new WebHeaderCollection ();
				while (WebConnection.ReadLine (ms.GetBuffer (), ref start, (int)ms.Length, ref str)) {
					if (str == null) {
						int contentLen;
						var clengthHeader = headers["Content-Length"];
						if (string.IsNullOrEmpty (clengthHeader) || !int.TryParse (clengthHeader, out contentLen))
							contentLen = 0;

						if (ms.Length - start - contentLen > 0) {
							// we've read more data than the response header and conents,
							// give back extra data to the caller
							retBuffer = new byte[ms.Length - start - contentLen];
							Buffer.BlockCopy (ms.GetBuffer (), start + contentLen, retBuffer, 0, retBuffer.Length);
						} else {
							// haven't read in some or all of the contents for the response, do so now
							FlushContents (stream, contentLen - (int)(ms.Length - start));
						}

						return (headers, retBuffer, status);
					}

					if (gotStatus) {
						headers.Add (str);
						continue;
					}

					string[] parts = str.Split (' ');
					if (parts.Length < 2)
						throw WebConnection.GetException (WebExceptionStatus.ServerProtocolViolation, null);

					if (String.Compare (parts[0], "HTTP/1.1", true) == 0)
						ProxyVersion = HttpVersion.Version11;
					else if (String.Compare (parts[0], "HTTP/1.0", true) == 0)
						ProxyVersion = HttpVersion.Version10;
					else
						throw WebConnection.GetException (WebExceptionStatus.ServerProtocolViolation, null);

					status = (int)UInt32.Parse (parts[1]);
					if (parts.Length >= 3)
						StatusDescription = String.Join (" ", parts, 2, parts.Length - 2);

					gotStatus = true;
				}
			}
		}

		void FlushContents (Stream stream, int contentLength)
		{
			while (contentLength > 0) {
				byte[] contentBuffer = new byte[contentLength];
				int bytesRead = stream.Read (contentBuffer, 0, contentLength);
				if (bytesRead > 0) {
					contentLength -= bytesRead;
				} else {
					break;
				}
			}
		}
	}
}