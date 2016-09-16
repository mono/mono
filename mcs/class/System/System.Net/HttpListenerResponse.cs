//
// System.Net.HttpListenerResponse
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

using System.Globalization;
using System.IO;
using System.Text;
namespace System.Net {
	public sealed class HttpListenerResponse : IDisposable
	{
		bool disposed;
		Encoding content_encoding;
		long content_length;
		bool cl_set;
		string content_type;
		CookieCollection cookies;
		WebHeaderCollection headers = new WebHeaderCollection ();
		bool keep_alive = true;
		ResponseStream output_stream;
		Version version = HttpVersion.Version11;
		string location;
		int status_code = 200;
		string status_description = "OK";
		bool chunked;
		HttpListenerContext context;
		
		internal bool HeadersSent;
		internal object headers_lock = new object ();
		
		bool force_close_chunked;

		internal HttpListenerResponse (HttpListenerContext context)
		{
			this.context = context;
		}

		internal bool ForceCloseChunked {
			get { return force_close_chunked; }
		}

		public Encoding ContentEncoding {
			get {
				if (content_encoding == null)
					content_encoding = Encoding.Default;
				return content_encoding;
			}
			set {
				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				//TODO: is null ok?
				if (HeadersSent)
					throw new InvalidOperationException ("Cannot be changed after headers are sent.");
					
				content_encoding = value;
			}
		}

		public long ContentLength64 {
			get { return content_length; }
			set {
				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (HeadersSent)
					throw new InvalidOperationException ("Cannot be changed after headers are sent.");

				if (value < 0)
					throw new ArgumentOutOfRangeException ("Must be >= 0", "value");

				cl_set = true;
				content_length = value;
			}
		}
		
		public string ContentType {
			get { return content_type; }
			set {
				// TODO: is null ok?
				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (HeadersSent)
					throw new InvalidOperationException ("Cannot be changed after headers are sent.");

				content_type = value;
			}
		}

		// RFC 2109, 2965 + the netscape specification at http://wp.netscape.com/newsref/std/cookie_spec.html
		public CookieCollection Cookies {
			get {
				if (cookies == null)
					cookies = new CookieCollection ();
				return cookies;
			}
			set { cookies = value; } // null allowed?
		}

		public WebHeaderCollection Headers {
			get { return headers; }
			set {
		/**
		 *	"If you attempt to set a Content-Length, Keep-Alive, Transfer-Encoding, or
		 *	WWW-Authenticate header using the Headers property, an exception will be
		 *	thrown. Use the KeepAlive or ContentLength64 properties to set these headers.
		 *	You cannot set the Transfer-Encoding or WWW-Authenticate headers manually."
		*/
		// TODO: check if this is marked readonly after headers are sent.
				headers = value;
			}
		}

		public bool KeepAlive {
			get { return keep_alive; }
			set {
				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (HeadersSent)
					throw new InvalidOperationException ("Cannot be changed after headers are sent.");
					
				keep_alive = value;
			}
		}

		public Stream OutputStream {
			get {
				if (output_stream == null)
					output_stream = context.Connection.GetResponseStream ();
				return output_stream;
			}
		}
		
		public Version ProtocolVersion {
			get { return version; }
			set {
				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (HeadersSent)
					throw new InvalidOperationException ("Cannot be changed after headers are sent.");
					
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value.Major != 1 || (value.Minor != 0 && value.Minor != 1))
					throw new ArgumentException ("Must be 1.0 or 1.1", "value");

				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				version = value;
			}
		}

		public string RedirectLocation {
			get { return location; }
			set {
				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (HeadersSent)
					throw new InvalidOperationException ("Cannot be changed after headers are sent.");
					
				location = value;
			}
		}

		public bool SendChunked {
			get { return chunked; }
			set {
				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (HeadersSent)
					throw new InvalidOperationException ("Cannot be changed after headers are sent.");
					
				chunked = value;
			}
		}

		public int StatusCode {
			get { return status_code; }
			set {
				if (disposed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (HeadersSent)
					throw new InvalidOperationException ("Cannot be changed after headers are sent.");
					
				if (value < 100 || value > 999)
					throw new ProtocolViolationException ("StatusCode must be between 100 and 999.");
				status_code = value;
				status_description = HttpListenerResponseHelper.GetStatusDescription (value);
			}
		}

		public string StatusDescription {
			get { return status_description; }
			set {
				status_description = value;
			}
		}

		void IDisposable.Dispose ()
		{
			Close (true); //TODO: Abort or Close?
		}

		public void Abort ()
		{
			if (disposed)
				return;

			Close (true);
		}

		public void AddHeader (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name == "")
				throw new ArgumentException ("'name' cannot be empty", "name");
			
			//TODO: check for forbidden headers and invalid characters
			if (value.Length > 65535)
				throw new ArgumentOutOfRangeException ("value");

			headers.Set (name, value);
		}

		public void AppendCookie (Cookie cookie)
		{
			if (cookie == null)
				throw new ArgumentNullException ("cookie");
			
			Cookies.Add (cookie);
		}

		public void AppendHeader (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name == "")
				throw new ArgumentException ("'name' cannot be empty", "name");
			
			if (value.Length > 65535)
				throw new ArgumentOutOfRangeException ("value");

			headers.Add (name, value);
		}

		void Close (bool force)
		{
			disposed = true;
			context.Connection.Close (force);
		}

		public void Close ()
		{
			if (disposed)
				return;

			Close (false);
		}

		public void Close (byte [] responseEntity, bool willBlock)
		{
			if (disposed)
				return;

			if (responseEntity == null)
				throw new ArgumentNullException ("responseEntity");

			//TODO: if willBlock -> BeginWrite + Close ?
			ContentLength64 = responseEntity.Length;
			OutputStream.Write (responseEntity, 0, (int) content_length);
			Close (false);
		}

		public void CopyFrom (HttpListenerResponse templateResponse)
		{
			headers.Clear ();
			headers.Add (templateResponse.headers);
			content_length = templateResponse.content_length;
			status_code = templateResponse.status_code;
			status_description = templateResponse.status_description;
			keep_alive = templateResponse.keep_alive;
			version = templateResponse.version;
		}

		public void Redirect (string url)
		{
			StatusCode = 302; // Found
			location = url;
		}

		bool FindCookie (Cookie cookie)
		{
			string name = cookie.Name;
			string domain = cookie.Domain;
			string path = cookie.Path;
			foreach (Cookie c in cookies) {
				if (name != c.Name)
					continue;
				if (domain != c.Domain)
					continue;
				if (path == c.Path)
					return true;
			}

			return false;
		}

		internal void SendHeaders (bool closing, MemoryStream ms)
		{
			Encoding encoding = content_encoding;
			if (encoding == null)
				encoding = Encoding.Default;

			if (content_type != null) {
				if (content_encoding != null && content_type.IndexOf ("charset=", StringComparison.Ordinal) == -1) {
					string enc_name = content_encoding.WebName;
					headers.SetInternal ("Content-Type", content_type + "; charset=" + enc_name);
				} else {
					headers.SetInternal ("Content-Type", content_type);
				}
			}

			if (headers ["Server"] == null)
				headers.SetInternal ("Server", "Mono-HTTPAPI/1.0");

			CultureInfo inv = CultureInfo.InvariantCulture;
			if (headers ["Date"] == null)
				headers.SetInternal ("Date", DateTime.UtcNow.ToString ("r", inv));

			if (!chunked) {
				if (!cl_set && closing) {
					cl_set = true;
					content_length = 0;
				}

				if (cl_set)
					headers.SetInternal ("Content-Length", content_length.ToString (inv));
			}

			Version v = context.Request.ProtocolVersion;
			if (!cl_set && !chunked && v >= HttpVersion.Version11)
				chunked = true;
				
			/* Apache forces closing the connection for these status codes:
			 *	HttpStatusCode.BadRequest 		400
			 *	HttpStatusCode.RequestTimeout 		408
			 *	HttpStatusCode.LengthRequired 		411
			 *	HttpStatusCode.RequestEntityTooLarge 	413
			 *	HttpStatusCode.RequestUriTooLong 	414
			 *	HttpStatusCode.InternalServerError 	500
			 *	HttpStatusCode.ServiceUnavailable 	503
			 */
			bool conn_close = (status_code == 400 || status_code == 408 || status_code == 411 ||
					status_code == 413 || status_code == 414 || status_code == 500 ||
					status_code == 503);

			if (conn_close == false)
				conn_close = !context.Request.KeepAlive;

			// They sent both KeepAlive: true and Connection: close!?
			if (!keep_alive || conn_close) {
				headers.SetInternal ("Connection", "close");
				conn_close = true;
			}

			if (chunked)
				headers.SetInternal ("Transfer-Encoding", "chunked");

			int reuses = context.Connection.Reuses;
			if (reuses >= 100) {
				force_close_chunked = true;
				if (!conn_close) {
					headers.SetInternal ("Connection", "close");
					conn_close = true;
				}
			}

			if (!conn_close) {
				headers.SetInternal ("Keep-Alive", String.Format ("timeout=15,max={0}", 100 - reuses));
				if (context.Request.ProtocolVersion <= HttpVersion.Version10)
					headers.SetInternal ("Connection", "keep-alive");
			}

			if (location != null)
				headers.SetInternal ("Location", location);

			if (cookies != null) {
				foreach (Cookie cookie in cookies)
					headers.SetInternal ("Set-Cookie", CookieToClientString (cookie));
			}

			StreamWriter writer = new StreamWriter (ms, encoding, 256);
			writer.Write ("HTTP/{0} {1} {2}\r\n", version, status_code, status_description);
			string headers_str = FormatHeaders (headers);
			writer.Write (headers_str);
			writer.Flush ();
			int preamble = encoding.GetPreamble ().Length;
			if (output_stream == null)
				output_stream = context.Connection.GetResponseStream ();

			/* Assumes that the ms was at position 0 */
			ms.Position = preamble;
			HeadersSent = true;
		}

		static string FormatHeaders (WebHeaderCollection headers)
		{
			var sb = new StringBuilder();

			for (int i = 0; i < headers.Count ; i++) {
				string key = headers.GetKey (i);
				if (WebHeaderCollection.AllowMultiValues (key)) {
					foreach (string v in headers.GetValues (i)) {
						sb.Append (key).Append (": ").Append (v).Append ("\r\n");
					}
				} else {
					sb.Append (key).Append (": ").Append (headers.Get (i)).Append ("\r\n");
				}
			}

			return sb.Append("\r\n").ToString();
		}

		static string CookieToClientString (Cookie cookie)
		{
			if (cookie.Name.Length == 0)
				return String.Empty;

			StringBuilder result = new StringBuilder (64);

			if (cookie.Version > 0)
				result.Append ("Version=").Append (cookie.Version).Append (";");

			result.Append (cookie.Name).Append ("=").Append (cookie.Value);

			if (cookie.Path != null && cookie.Path.Length != 0)
				result.Append (";Path=").Append (QuotedString (cookie, cookie.Path));

			if (cookie.Domain != null && cookie.Domain.Length != 0)
				result.Append (";Domain=").Append (QuotedString (cookie, cookie.Domain));			

			if (cookie.Port != null && cookie.Port.Length != 0)
				result.Append (";Port=").Append (cookie.Port);	

			return result.ToString ();
		}

		static string QuotedString (Cookie cookie, string value)
		{
			if (cookie.Version == 0 || IsToken (value))
				return value;
			else 
				return "\"" + value.Replace("\"", "\\\"") + "\"";
		}	

		static string tspecials = "()<>@,;:\\\"/[]?={} \t";   // from RFC 2965, 2068

	    static bool IsToken (string value) 
		{
			int len = value.Length;
			for (int i = 0; i < len; i++) {
			    char c = value [i];
				if (c < 0x20 || c >= 0x7f || tspecials.IndexOf (c) != -1)
			      		return false;
			}
			return true;
		}

		public void SetCookie (Cookie cookie)
		{
			if (cookie == null)
				throw new ArgumentNullException ("cookie");

			if (cookies != null) {
				if (FindCookie (cookie))
					throw new ArgumentException ("The cookie already exists.");
			} else {
				cookies = new CookieCollection ();
			}

			cookies.Add (cookie);
		}
	}

	// do not inline into HttpListenerResponse as this recursively brings everything that's 
	// reachable by IDisposable.Dispose (and that's quite a lot in this case). 
	static class HttpListenerResponseHelper {

		internal static string GetStatusDescription (int code)
		{
			switch (code){
			case 100: return "Continue";
			case 101: return "Switching Protocols";
			case 102: return "Processing";
			case 200: return "OK";
			case 201: return "Created";
			case 202: return "Accepted";
			case 203: return "Non-Authoritative Information";
			case 204: return "No Content";
			case 205: return "Reset Content";
			case 206: return "Partial Content";
			case 207: return "Multi-Status";
			case 300: return "Multiple Choices";
			case 301: return "Moved Permanently";
			case 302: return "Found";
			case 303: return "See Other";
			case 304: return "Not Modified";
			case 305: return "Use Proxy";
			case 307: return "Temporary Redirect";
			case 400: return "Bad Request";
			case 401: return "Unauthorized";
			case 402: return "Payment Required";
			case 403: return "Forbidden";
			case 404: return "Not Found";
			case 405: return "Method Not Allowed";
			case 406: return "Not Acceptable";
			case 407: return "Proxy Authentication Required";
			case 408: return "Request Timeout";
			case 409: return "Conflict";
			case 410: return "Gone";
			case 411: return "Length Required";
			case 412: return "Precondition Failed";
			case 413: return "Request Entity Too Large";
			case 414: return "Request-Uri Too Long";
			case 415: return "Unsupported Media Type";
			case 416: return "Requested Range Not Satisfiable";
			case 417: return "Expectation Failed";
			case 422: return "Unprocessable Entity";
			case 423: return "Locked";
			case 424: return "Failed Dependency";
			case 500: return "Internal Server Error";
			case 501: return "Not Implemented";
			case 502: return "Bad Gateway";
			case 503: return "Service Unavailable";
			case 504: return "Gateway Timeout";
			case 505: return "Http Version Not Supported";
			case 507: return "Insufficient Storage";
			}
			return "";
		}
	}
}
#endif

