//
// System.Net.HttpWebResponse
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Lawrence Pit
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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

using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace System.Net 
{
	[Serializable]
	public class HttpWebResponse : WebResponse, ISerializable, IDisposable
	{
		Uri uri;
		WebHeaderCollection webHeaders;
		CookieCollection cookieCollection;
		string method;
		Version version;
		HttpStatusCode statusCode;
		string statusDescription;
		long contentLength = -1;
		string contentType;

		bool disposed = false;
		Stream stream;
		
		// Constructors
		
		internal HttpWebResponse (Uri uri, string method, WebConnectionData data, bool cookiesSet)
		{
			this.uri = uri;
			this.method = method;
			webHeaders = data.Headers;
			version = data.Version;
			statusCode = (HttpStatusCode) data.StatusCode;
			statusDescription = data.StatusDescription;
			stream = data.stream;
			if (cookiesSet) {
				FillCookies ();
			} else if (webHeaders != null) {
				webHeaders.RemoveInternal ("Set-Cookie");
				webHeaders.RemoveInternal ("Set-Cookie2");
			}
		}

		protected HttpWebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;

			uri = (Uri) info.GetValue ("uri", typeof (Uri));
			contentLength = info.GetInt64 ("contentLength");
			contentType = info.GetString ("contentType");
			method = info.GetString ("method");
			statusDescription = info.GetString ("statusDescription");
			cookieCollection = (CookieCollection) info.GetValue ("cookieCollection", typeof (CookieCollection));
			version = (Version) info.GetValue ("version", typeof (Version));
			statusCode = (HttpStatusCode) info.GetValue ("statusCode", typeof (HttpStatusCode));
		}
		
		// Properties
		
		public string CharacterSet {
			// Content-Type   = "Content-Type" ":" media-type
			// media-type     = type "/" subtype *( ";" parameter )
			// parameter      = attribute "=" value
			// 3.7.1. default is ISO-8859-1
			get { 
				CheckDisposed ();
				string contentType = ContentType;
				if (contentType == null)
					return "ISO-8859-1";
				string val = contentType.ToLower (); 					
				int pos = val.IndexOf ("charset=");
				if (pos == -1)
					return "ISO-8859-1";
				pos += 8;
				int pos2 = val.IndexOf (';', pos);
				return (pos2 == -1)
				     ? contentType.Substring (pos) 
				     : contentType.Substring (pos, pos2 - pos);
			}
		}
		
		public string ContentEncoding {
			get { 
				CheckDisposed ();
				return webHeaders ["Content-Encoding"];
			}
		}
		
		public override long ContentLength {		
			get {
				CheckDisposed ();
				if (contentLength != -1)
					return contentLength;

				try {
					contentLength = (long) UInt64.Parse (webHeaders ["Content-Length"]); 
				} catch (Exception) {
					return -1;
				}

				return contentLength;
			}
		}
		
		public override string ContentType {		
			get {
				CheckDisposed ();
				if (contentType == null)
					contentType = webHeaders ["Content-Type"];

				return contentType;
			}
		}
		
		public CookieCollection Cookies {
			get { 
				CheckDisposed ();
				
				if (cookieCollection == null)
					cookieCollection = new CookieCollection ();
				return cookieCollection;
			}
			set {
				CheckDisposed ();
				cookieCollection = value;
			}
		}
		
		public override WebHeaderCollection Headers {		
			get { 
				CheckDisposed ();
				return webHeaders; 
			}
		}
		
		public DateTime LastModified {
			get {
				CheckDisposed ();
				try {
					string dtStr = webHeaders ["Last-Modified"];
					return MonoHttpDate.Parse (dtStr);
				} catch (Exception) {
					return DateTime.Now;	
				}
			}
		}
		
		public string Method {
			get { 
				CheckDisposed ();
				return method; 
			}
		}
		
		public Version ProtocolVersion {
			get { 
				CheckDisposed ();
				return version; 
			}
		}
		
		public override Uri ResponseUri {		
			get { 
				CheckDisposed ();
				return uri; 
			}
		}		
		
		public string Server {
			get { 
				CheckDisposed ();
				return webHeaders ["Server"]; 
			}
		}
		
		public HttpStatusCode StatusCode {
			get { 
				CheckDisposed ();
				return statusCode; 
			}
		}
		
		public string StatusDescription {
			get { 
				CheckDisposed ();
				return statusDescription; 
			}
		}

		// Methods
		
		public override int GetHashCode ()
		{
			CheckDisposed ();
			return base.GetHashCode ();
		}
		
		public string GetResponseHeader (string headerName)
		{
			CheckDisposed ();
			string value = webHeaders [headerName];
			return (value != null) ? value : "";
		}
		
		public override Stream GetResponseStream ()
		{
			CheckDisposed ();
			if (stream == null)
				return Stream.Null;  
			if (0 == String.Compare (method, "HEAD", true)) // see par 4.3 & 9.4
				return Stream.Null;  

			return stream;
		}
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;

			info.AddValue ("uri", uri);
			info.AddValue ("contentLength", contentLength);
			info.AddValue ("contentType", contentType);
			info.AddValue ("method", method);
			info.AddValue ("statusDescription", statusDescription);
			info.AddValue ("cookieCollection", cookieCollection);
			info.AddValue ("version", version);
			info.AddValue ("statusCode", statusCode);
		}		


		// Cleaning up stuff

		public override void Close ()
		{
			((IDisposable) this).Dispose ();
		}
		
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);  
		}
		
		protected virtual void Dispose (bool disposing) 
		{
			if (this.disposed)
				return;
			this.disposed = true;
			
			if (disposing) {
				// release managed resources
				uri = null;
				webHeaders = null;
				cookieCollection = null;
				method = null;
				version = null;
				statusDescription = null;
			}
			
			// release unmanaged resources
			Stream st = stream;
			stream = null;
			if (st != null) {
				WebConnectionStream wce = st as WebConnectionStream;
				if (wce != null) {
					try {
						wce.ReadAll ();
					} catch {}
				}
				st.Close ();
			}
		}
		
		private void CheckDisposed () 
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		void FillCookies ()
		{
			if (webHeaders == null)
				return;

			string [] values = webHeaders.GetValues ("Set-Cookie");
			if (values != null) {
				foreach (string va in values)
					SetCookie (va);
			}

			values = webHeaders.GetValues ("Set-Cookie2");
			if (values != null) {
				foreach (string va in values)
					SetCookie2 (va);
			}
		}
		
		void SetCookie (string header)
		{
			string [] name_values = header.Trim ().Split (';');
			int length = name_values.Length;
			Cookie cookie = null;
			int pos;
			for (int i = 0; i < length; i++) {
				pos = 0;
				string name_value = name_values [i].Trim ();
				if (name_value == "")
					continue;

				string name = GetCookieName (name_value, name_value.Length, ref pos);
				string value = GetCookieValue (name_value, name_value.Length, ref pos);
				if (cookie == null) {
					cookie = new Cookie (name, value);
					continue;
				}

				name = name.ToUpper ();
				switch (name) {
				case "COMMENT":
					if (cookie.Comment == null)
						cookie.Comment = value;
					break;
				case "COMMENTURL":
					if (cookie.CommentUri == null)
						cookie.CommentUri = new Uri (value);
					break;
				case "DISCARD":
					cookie.Discard = true;
					break;
				case "DOMAIN":
					if (cookie.Domain == "")
						cookie.Domain = value;
					break;
				case "MAX-AGE": // RFC Style Set-Cookie2
					if (cookie.Expires == DateTime.MinValue)
						cookie.Expires = cookie.TimeStamp.AddSeconds (Int32.Parse (value));
					break;
				case "EXPIRES": // Netscape Style Set-Cookie
					if (cookie.Expires != DateTime.MinValue)
						break;
					try {
						cookie.Expires = DateTime.ParseExact (value, "r", CultureInfo.InvariantCulture);
					} catch {
						try { 
						cookie.Expires = DateTime.ParseExact (value,
								"ddd, dd'-'MMM'-'yyyy HH':'mm':'ss 'GMT'",
								CultureInfo.InvariantCulture);
						} catch {
							cookie.Expires = DateTime.Now.AddDays (1);
						}
					}
					break;
				case "PATH":
					cookie.Path = value;
					break;
				case "PORT":
					if (cookie.Port == null)
						cookie.Port = value;
					break;
				case "SECURE":
					cookie.Secure = true;
					break;
				case "VERSION":
					cookie.Version = Int32.Parse (value);
					break;
				}
			}

			if (cookieCollection == null)
				cookieCollection = new CookieCollection ();

			if (cookie.Domain == "")
				cookie.Domain = uri.Host;

			cookieCollection.Add (cookie);
		}

		void SetCookie2 (string cookies_str)
		{
			string [] cookies = cookies_str.Split (',');
	
			foreach (string cookie_str in cookies)
				SetCookie (cookie_str);
		}

		static string GetCookieValue (string str, int length, ref int i)
		{
			if (i >= length)
				return null;

			int k = i;
			while (k < length && Char.IsWhiteSpace (str [k]))
				k++;

			int begin = k;
			while (k < length && str [k] != ';')
				k++;

			i = k;
			return str.Substring (begin, i - begin).Trim ();
		}

		static string GetCookieName (string str, int length, ref int i)
		{
			if (i >= length)
				return null;

			int k = i;
			while (k < length && Char.IsWhiteSpace (str [k]))
				k++;

			int begin = k;
			while (k < length && str [k] != ';' &&  str [k] != '=')
				k++;

			i = k + 1;
			return str.Substring (begin, k - begin).Trim ();
		}
	}	
}

