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

using System;
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
		WebConnectionStream stream;
		
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
			if (st != null)
				st.Close ();
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

			string val = webHeaders ["Set-Cookie"];
			if (val != null && val.Trim () != "")
				SetCookie (val);

			val = webHeaders ["Set-Cookie2"];
			if (val != null && val.Trim () != "")
				SetCookie2 (val);
		}
		
		static string [] SplitValue (string input)
		{
			string [] result = new string [2];
			int eq = input.IndexOf ('=');
			if (eq == -1) {
				result [0] = "invalid";
			} else {
				result [0] = input.Substring (0, eq).Trim ().ToUpper ();
				result [1] = input.Substring (eq + 1);
			}
			
			return result;
		}
		
		[MonoTODO ("Parse dates")]
		void SetCookie (string cookie_str)
		{
			string[] parts = null;
			Collections.Queue options = null;
			Cookie cookie = null;

			options = new Collections.Queue (cookie_str.Split (';'));
			parts = SplitValue ((string) options.Dequeue()); // NAME=VALUE must be first

			cookie = new Cookie (parts[0], parts[1]);

			while (options.Count > 0) {
				parts = SplitValue ((string) options.Dequeue());
				switch (parts [0]) {
					case "COMMENT":
						if (cookie.Comment == null)
							cookie.Comment = parts[1];
					break;
					case "COMMENTURL":
						if (cookie.CommentUri == null)
							cookie.CommentUri = new Uri(parts[1]);
					break;
					case "DISCARD":
						cookie.Discard = true;
					break;
					case "DOMAIN":
						if (cookie.Domain == "")
							cookie.Domain = parts[1];
					break;
					case "MAX-AGE": // RFC Style Set-Cookie2
						if (cookie.Expires == DateTime.MinValue)
							cookie.Expires = cookie.TimeStamp.AddSeconds (Int32.Parse (parts[1]));
					break;
					case "EXPIRES": // Netscape Style Set-Cookie
						if (cookie.Expires == DateTime.MinValue) {
							//FIXME: Does DateTime parse something like: "Sun, 17-Jan-2038 19:14:07 GMT"?
							//cookie.Expires = DateTime.ParseExact (parts[1]);
							cookie.Expires = DateTime.Now.AddDays (1);
						}
					break;
					case "PATH":
							cookie.Path = parts[1];
					break;
					case "PORT":
						if (cookie.Port == null)
							cookie.Port = parts[1];
					break;
					case "SECURE":
						cookie.Secure = true;
					break;
					case "VERSION":
						cookie.Version = Int32.Parse (parts[1]);
					break;
				} // switch
			} // while

			if (cookieCollection == null)
				cookieCollection = new CookieCollection();

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
	}	
}

