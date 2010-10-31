//
// System.Net.HttpListenerRequest
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

#if NET_2_0 && SECURITY_DEP

using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
namespace System.Net {
	public sealed class HttpListenerRequest
	{
		string [] accept_types;
//		int client_cert_error;
//		bool no_get_certificate;
		Encoding content_encoding;
		long content_length;
		bool cl_set;
		CookieCollection cookies;
		WebHeaderCollection headers;
		string method;
		Stream input_stream;
		Version version;
		NameValueCollection query_string; // check if null is ok, check if read-only, check case-sensitiveness
		string raw_url;
		Uri url;
		Uri referrer;
		string [] user_languages;
		HttpListenerContext context;
		bool is_chunked;
		bool ka_set;
		bool keep_alive;
		static byte [] _100continue = Encoding.ASCII.GetBytes ("HTTP/1.1 100 Continue\r\n\r\n");
		static readonly string [] no_body_methods = new string [] {
			"GET", "HEAD", "DELETE" };

		internal HttpListenerRequest (HttpListenerContext context)
		{
			this.context = context;
			headers = new WebHeaderCollection ();
			input_stream = Stream.Null;
			version = HttpVersion.Version10;
		}

		static char [] separators = new char [] { ' ' };

		internal void SetRequestLine (string req)
		{
			string [] parts = req.Split (separators, 3);
			if (parts.Length != 3) {
				context.ErrorMessage = "Invalid request line (parts).";
				return;
			}

			method = parts [0];
			foreach (char c in method){
				int ic = (int) c;

				if ((ic >= 'A' && ic <= 'Z') ||
				    (ic > 32 && c < 127 && c != '(' && c != ')' && c != '<' &&
				     c != '<' && c != '>' && c != '@' && c != ',' && c != ';' &&
				     c != ':' && c != '\\' && c != '"' && c != '/' && c != '[' &&
				     c != ']' && c != '?' && c != '=' && c != '{' && c != '}'))
					continue;

				context.ErrorMessage = "(Invalid verb)";
				return;
			}

			raw_url = parts [1];
			if (parts [2].Length != 8 || !parts [2].StartsWith ("HTTP/")) {
				context.ErrorMessage = "Invalid request line (version).";
				return;
			}

			try {
				version = new Version (parts [2].Substring (5));
				if (version.Major < 1)
					throw new Exception ();
			} catch {
				context.ErrorMessage = "Invalid request line (version).";
				return;
			}
		}

		void CreateQueryString (string query)
		{
			query_string = new NameValueCollection ();
			if (query == null || query.Length == 0)
				return;

			if (query [0] == '?')
				query = query.Substring (1);
			string [] components = query.Split ('&');
			foreach (string kv in components) {
				int pos = kv.IndexOf ('=');
				if (pos == -1) {
					query_string.Add (null, HttpUtility.UrlDecode (kv));
				} else {
					string key = HttpUtility.UrlDecode (kv.Substring (0, pos));
					string val = HttpUtility.UrlDecode (kv.Substring (pos + 1));
					
					query_string.Add (key, val);
				}
			}
		}

		internal void FinishInitialization ()
		{
			string host = UserHostName;
			if (version > HttpVersion.Version10 && (host == null || host.Length == 0)) {
				context.ErrorMessage = "Invalid host name";
				return;
			}

			string path;
			Uri raw_uri = null;
			if (Uri.MaybeUri (raw_url) && Uri.TryCreate (raw_url, UriKind.Absolute, out raw_uri))
				path = raw_uri.PathAndQuery;
			else
				path = raw_url;

			if ((host == null || host.Length == 0))
				host = UserHostAddress;

			if (raw_uri != null)
				host = raw_uri.Host;
	
			int colon = host.IndexOf (':');
			if (colon >= 0)
				host = host.Substring (0, colon);

			string base_uri = String.Format ("{0}://{1}:{2}",
								(IsSecureConnection) ? "https" : "http",
								host,
								LocalEndPoint.Port);

			if (!Uri.TryCreate (base_uri + path, UriKind.Absolute, out url)){
				context.ErrorMessage = "Invalid url: " + base_uri + path;
				return;
			}

			CreateQueryString (url.Query);

			string t_encoding = null;
			if (version >= HttpVersion.Version11) {
				t_encoding = Headers ["Transfer-Encoding"];
				// 'identity' is not valid!
				if (t_encoding != null && t_encoding != "chunked") {
					context.Connection.SendError (null, 501);
					return;
				}
			}

			is_chunked = (t_encoding == "chunked");

			foreach (string m in no_body_methods)
				if (string.Compare (method, m, StringComparison.InvariantCultureIgnoreCase) == 0)
					return;

			if (!is_chunked && !cl_set) {
				context.Connection.SendError (null, 411);
				return;
			}

			if (is_chunked || content_length > 0) {
				input_stream = context.Connection.GetRequestStream (is_chunked, content_length);
			}

			if (Headers ["Expect"] == "100-continue") {
				ResponseStream output = context.Connection.GetResponseStream ();
				output.InternalWrite (_100continue, 0, _100continue.Length);
			}
		}

		internal static string Unquote (String str) {
			int start = str.IndexOf ('\"');
			int end = str.LastIndexOf ('\"');
			if (start >= 0 && end >=0)
				str = str.Substring (start + 1, end - 1);
			return str.Trim ();
		}

		internal void AddHeader (string header)
		{
			int colon = header.IndexOf (':');
			if (colon == -1 || colon == 0) {
				context.ErrorMessage = "Bad Request";
				context.ErrorStatus = 400;
				return;
			}

			string name = header.Substring (0, colon).Trim ();
			string val = header.Substring (colon + 1).Trim ();
			string lower = name.ToLower (CultureInfo.InvariantCulture);
			headers.SetInternal (name, val);
			switch (lower) {
				case "accept-language":
					user_languages = val.Split (','); // yes, only split with a ','
					break;
				case "accept":
					accept_types = val.Split (','); // yes, only split with a ','
					break;
				case "content-length":
					try {
						//TODO: max. content_length?
						content_length = Int64.Parse (val.Trim ());
						if (content_length < 0)
							context.ErrorMessage = "Invalid Content-Length.";
						cl_set = true;
					} catch {
						context.ErrorMessage = "Invalid Content-Length.";
					}

					break;
				case "referer":
					try {
						referrer = new Uri (val);
					} catch {
						referrer = new Uri ("http://someone.is.screwing.with.the.headers.com/");
					}
					break;
				case "cookie":
					if (cookies == null)
						cookies = new CookieCollection();

					string[] cookieStrings = val.Split(new char[] {',', ';'});
					Cookie current = null;
					int version = 0;
					foreach (string cookieString in cookieStrings) {
						string str = cookieString.Trim ();
						if (str.Length == 0)
							continue;
						if (str.StartsWith ("$Version")) {
							version = Int32.Parse (Unquote (str.Substring (str.IndexOf ('=') + 1)));
						} else if (str.StartsWith ("$Path")) {
							if (current != null)
								current.Path = str.Substring (str.IndexOf ('=') + 1).Trim ();
						} else if (str.StartsWith ("$Domain")) {
							if (current != null)
								current.Domain = str.Substring (str.IndexOf ('=') + 1).Trim ();
						} else if (str.StartsWith ("$Port")) {
							if (current != null)
								current.Port = str.Substring (str.IndexOf ('=') + 1).Trim ();
						} else {
							if (current != null) {
								cookies.Add (current);
							}
							current = new Cookie ();
							int idx = str.IndexOf ('=');
							if (idx > 0) {
								current.Name = str.Substring (0, idx).Trim ();
								current.Value =  str.Substring (idx + 1).Trim ();
							} else {
								current.Name = str.Trim ();
								current.Value = String.Empty;
							}
							current.Version = version;
						}
					}
					if (current != null) {
						cookies.Add (current);
					}
					break;
			}
		}

		// returns true is the stream could be reused.
		internal bool FlushInput ()
		{
			if (!HasEntityBody)
				return true;

			int length = 2048;
			if (content_length > 0)
				length = (int) Math.Min (content_length, (long) length);

			byte [] bytes = new byte [length];
			while (true) {
				// TODO: test if MS has a timeout when doing this
				try {
					if (InputStream.Read (bytes, 0, length) <= 0)
						return true;
				} catch {
					return false;
				}
			}
		}

		public string [] AcceptTypes {
			get { return accept_types; }
		}

		[MonoTODO ("Always returns 0")]
		public int ClientCertificateError {
			get {
/*				
				if (no_get_certificate)
					throw new InvalidOperationException (
						"Call GetClientCertificate() before calling this method.");
				return client_cert_error;
*/
				return 0;
			}
		}

		public Encoding ContentEncoding {
			get {
				if (content_encoding == null)
					content_encoding = Encoding.Default;
				return content_encoding;
			}
		}

		public long ContentLength64 {
			get { return content_length; }
		}

		public string ContentType {
			get { return headers ["content-type"]; }
		}

		public CookieCollection Cookies {
			get {
				// TODO: check if the collection is read-only
				if (cookies == null)
					cookies = new CookieCollection ();
				return cookies;
			}
		}

		public bool HasEntityBody {
			get { return (content_length > 0 || is_chunked); }
		}

		public NameValueCollection Headers {
			get { return headers; }
		}

		public string HttpMethod {
			get { return method; }
		}

		public Stream InputStream {
			get { return input_stream; }
		}

		[MonoTODO ("Always returns false")]
		public bool IsAuthenticated {
			get { return false; }
		}

		public bool IsLocal {
			get { return IPAddress.IsLoopback (RemoteEndPoint.Address); }
		}

		public bool IsSecureConnection {
			get { return context.Connection.IsSecure; } 
		}

		public bool KeepAlive {
			get {
				if (ka_set)
					return keep_alive;

				ka_set = true;
				// 1. Connection header
				// 2. Protocol (1.1 == keep-alive by default)
				// 3. Keep-Alive header
				string cnc = headers ["Connection"];
				if (!String.IsNullOrEmpty (cnc)) {
					keep_alive = (0 == String.Compare (cnc, "keep-alive", StringComparison.OrdinalIgnoreCase));
				} else if (version == HttpVersion.Version11) {
					keep_alive = true;
				} else {
					cnc = headers ["keep-alive"];
					if (!String.IsNullOrEmpty (cnc))
						keep_alive = (0 != String.Compare (cnc, "closed", StringComparison.OrdinalIgnoreCase));
				}
				return keep_alive;
			}
		}

		public IPEndPoint LocalEndPoint {
			get { return context.Connection.LocalEndPoint; }
		}

		public Version ProtocolVersion {
			get { return version; }
		}

		public NameValueCollection QueryString {
			get { return query_string; }
		}

		public string RawUrl {
			get { return raw_url; }
		}

		public IPEndPoint RemoteEndPoint {
			get { return context.Connection.RemoteEndPoint; }
		}

		[MonoTODO ("Always returns Guid.Empty")]
		public Guid RequestTraceIdentifier {
			get { return Guid.Empty; }
		}

		public Uri Url {
			get { return url; }
		}

		public Uri UrlReferrer {
			get { return referrer; }
		}

		public string UserAgent {
			get { return headers ["user-agent"]; }
		}

		public string UserHostAddress {
			get { return LocalEndPoint.ToString (); }
		}

		public string UserHostName {
			get { return headers ["host"]; }
		}

		public string [] UserLanguages {
			get { return user_languages; }
		}

		public IAsyncResult BeginGetClientCertificate (AsyncCallback requestCallback, Object state)
		{
			return null;
		}
#if SECURITY_DEP
		public X509Certificate2 EndGetClientCertificate (IAsyncResult asyncResult)
		{
			return null;
			// set no_client_certificate once done.
		}

		public X509Certificate2 GetClientCertificate ()
		{
			// set no_client_certificate once done.

			// InvalidOp if call in progress.
			return null;
		}
#endif
	}
}
#endif

