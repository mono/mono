//
// System.Net.HttpWebResponse
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public class HttpWebResponse : WebResponse, ISerializable, IDisposable
	{
		private Uri uri;
		private WebHeaderCollection webHeaders;
		private CookieCollection cookieCollection = null;
		private string method = null;
		private Version version = null;
		private HttpStatusCode statusCode;
		private string statusDescription = null;

		private Stream responseStream;		
		private bool disposed = false;
		
		// Constructors
		
		internal HttpWebResponse (Uri uri, string method, Stream responseStream) 
		{ 
			Text.StringBuilder value = null;
			string last = null;
			string line = null;
			string[] protocol, header;

			this.uri = uri;
			this.method = method;
			this.responseStream = responseStream;
			this.webHeaders = new WebHeaderCollection();

			line = ReadHttpLine(responseStream);
			protocol = line.Split (' ');
			
			switch (protocol[0]) {
				case "HTTP/1.0":
					this.version = HttpVersion.Version10;
					break;
				case "HTTP/1.1":
					this.version = HttpVersion.Version11;
					break;
				default:
					throw new WebException ("Unrecognized HTTP Version");
			}
			
			this.statusCode = (HttpStatusCode) Int32.Parse (protocol[1]);
			while ((line = ReadHttpLine(responseStream)).Length != 0) {
				if (!Char.IsWhiteSpace (line[0])) { // new header
					header = line.Split (new char[] {':'}, 2);
					if (header.Length != 2)
						throw new WebException ("Bad HTTP Header");
					if (last != null) { // not the first header
						if (last.Equals ("Set-Cookie"))
							SetCookie (value.ToString());
						else if (last.Equals ("Set-Cookie2"))
							SetCookie2 (value.ToString());
						else //don't save Set-Cookie headers
							this.webHeaders[last] = value.ToString();
					}
					last = header[0];
					value = new Text.StringBuilder (header[1].Trim());
				}
				else
					value.Append (line.Trim());
			}
			
			this.webHeaders[last] = value.ToString(); // otherwise we miss the last header
		}
		
		protected HttpWebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}
		
		// Properties
		
		public string CharacterSet {
			// Content-Type   = "Content-Type" ":" media-type
			// media-type     = type "/" subtype *( ";" parameter )
			// parameter      = attribute "=" value
			// 3.7.1. default is ISO-8859-1
			get { 
				try {
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
				} finally {
					CheckDisposed ();
				}
			}
		}
		
		public string ContentEncoding {
			get { 
				try { return webHeaders ["Content-Encoding"]; }
				finally { CheckDisposed (); }
			}
		}
		
		public override long ContentLength {		
			get { 
				try {
					return Int64.Parse (webHeaders ["Content-Length"]); 
				} catch (Exception) {
					return -1;
				} finally {
					CheckDisposed ();
				}
			}
		}
		
		public override string ContentType {		
			get { 
				try { return webHeaders ["Content-Type"]; }
				finally { CheckDisposed (); }
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
				// ?? don't understand how you can set cookies on a response.
				throw new NotSupportedException ();
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
				try {
					return webHeaders ["Server"]; 
				} finally {
					CheckDisposed ();
				}
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
			try {
				return base.GetHashCode ();
			} finally {
				CheckDisposed ();
			}
		}
		
		public string GetResponseHeader (string headerName)
		{
			try {
				return webHeaders [headerName];
			} finally {
				CheckDisposed ();
			}
		}
		
		public override Stream GetResponseStream ()
		{
			try {
				if (method.Equals ("HEAD")) // see par 4.3 & 9.4
					return Stream.Null;  
				return responseStream;
			} finally {
				CheckDisposed ();
			}
		}
		
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			CheckDisposed ();
			throw new NotImplementedException ();
		}		


		// Cleaning up stuff

		~HttpWebResponse ()
		{
			Dispose (false);
		}		
		
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
				// statusCode = null;
				statusDescription = null;
			}
			
			// release unmanaged resources
			Stream stream = responseStream;
			responseStream = null;
			if (stream != null)
				stream.Close ();  // also closes webRequest			
		}
		
		private void CheckDisposed () 
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		private static string ReadHttpLine (Stream stream)
		{
			Text.StringBuilder line = new Text.StringBuilder();
			byte last = (byte)'\n';
			bool read_last = false;
			byte[] buf = new byte[1]; // one at a time to not snarf too much
			
			while (stream.Read (buf, 0, buf.Length) != 0) {
				if (buf[0] == '\r') {
					if ((last = (byte)stream.ReadByte ()) == '\n') // headers; not at EOS
						break;
					read_last = true;
				}

				line.Append (Convert.ToChar(buf[0]));
				if (read_last) {
					line.Append (Convert.ToChar (last));
					read_last = false;
				}
			}
			
			return line.ToString();
		}

		private void SetCookie (string cookie_str)
		{
			string[] parts = null;
			Collections.Queue options = null;
			Cookie cookie = null;

			options = new Collections.Queue (cookie_str.Split (';'));
			parts = ((string)options.Dequeue()).Split ('='); // NAME=VALUE must be first

			cookie = new Cookie (parts[0], parts[1]);

			while (options.Count > 0) {
				parts = ((string)options.Dequeue()).Split ('=');
				switch (parts[0].ToUpper()) { // cookie options are case-insensitive
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
						if (cookie.Domain == null)
							cookie.Domain = parts[1];
					break;
					case "MAX-AGE": // RFC Style Set-Cookie2
						if (cookie.Expires == DateTime.MinValue)
							cookie.Expires = cookie.TimeStamp.AddSeconds (Int32.Parse (parts[1]));
					break;
					case "EXPIRES": // Netscape Style Set-Cookie
						if (cookie.Expires == DateTime.MinValue)
							cookie.Expires = DateTime.Parse (parts[1]);
					break;
					case "PATH":
						if (cookie.Path == null)
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

			if (cookie.Domain == null)
				cookie.Domain = uri.Host;

			cookieCollection.Add (cookie);
		}

		private void SetCookie2 (string cookies_str)
		{
			string[] cookies = cookies_str.Split (',');
	
			foreach (string cookie_str in cookies)
				SetCookie (cookie_str);

		}
	}	
}
