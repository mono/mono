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
		private Uri uri;
		private WebHeaderCollection webHeaders;
		private CookieCollection cookieCollection = null;
		private string method = null;
		private Version version = null;
		private HttpStatusCode statusCode;
		private string statusDescription = null;
		bool chunked;

		private HttpWebResponseStream responseStream;		
		private bool disposed = false;
		
		// Constructors
		
		internal HttpWebResponse (Uri uri, string method, Socket socket, int timeout, EventHandler onClose)
		{ 
			Text.StringBuilder value = null;
			string last = null;
			string line = null;
			string[] protocol, header;

			this.uri = uri;
			this.method = method;
			this.webHeaders = new WebHeaderCollection();

			responseStream = new HttpWebResponseStream (socket, onClose);
			if (!socket.Poll (timeout, SelectMode.SelectRead))
				throw new WebException("The request timed out", WebExceptionStatus.Timeout);

			line = ReadHttpLine (responseStream);
			protocol = line.Split (' ');
			
			switch (protocol[0]) {
				case "HTTP/1.0":
					this.version = HttpVersion.Version10;
					break;
				case "HTTP/1.1":
					this.version = HttpVersion.Version11;
					break;
				default:
					throw new WebException ("Unrecognized HTTP Version: " + line);
			}
			
			this.statusCode = (HttpStatusCode) Int32.Parse (protocol[1]);
			while ((line = ReadHttpLine (responseStream)).Length != 0) {
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
					if (last == "Transfer-Encoding" && header [1].IndexOf ("chunked") != -1)
						chunked = true;
				}
				else
					value.Append (line.Trim());
			}
			
			responseStream.Chunked = chunked;
			this.webHeaders[last] = value.ToString(); // otherwise we miss the last header
		}
		
		protected HttpWebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			uri = (Uri) serializationInfo.GetValue ("uri", typeof (Uri));
			webHeaders = (WebHeaderCollection) serializationInfo.GetValue ("webHeaders",
											typeof (WebHeaderCollection));
			cookieCollection = (CookieCollection) serializationInfo.GetValue ("cookieCollection",
											   typeof (CookieCollection));
			method = serializationInfo.GetString ("method");
			version = (Version) serializationInfo.GetValue ("version", typeof (Version));
			statusCode = (HttpStatusCode) serializationInfo.GetValue ("statusCode", typeof (HttpStatusCode));
			statusDescription = serializationInfo.GetString ("statusDescription");
			chunked = serializationInfo.GetBoolean ("chunked");
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
				try {
					return Int64.Parse (webHeaders ["Content-Length"]); 
				} catch (Exception) {
					return -1;
				}
			}
		}
		
		public override string ContentType {		
			get {
				CheckDisposed ();
				return webHeaders ["Content-Type"];
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
			return webHeaders [headerName];
		}
		
		public override Stream GetResponseStream ()
		{
			CheckDisposed ();
			if (method.Equals ("HEAD")) // see par 4.3 & 9.4
				return Stream.Null;  
			return responseStream;
		}
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			CheckDisposed ();
			serializationInfo.AddValue ("uri", uri);
			serializationInfo.AddValue ("webHeaders", webHeaders);
			serializationInfo.AddValue ("cookieCollection", cookieCollection);
			serializationInfo.AddValue ("method", method);
			serializationInfo.AddValue ("version", version);
			serializationInfo.AddValue ("statusCode", statusCode);
			serializationInfo.AddValue ("statusDescription", statusDescription);
			serializationInfo.AddValue ("chunked", chunked);
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
				statusDescription = null;
			}
			
			// release unmanaged resources
			Stream stream = responseStream;
			responseStream = null;
			if (stream != null)
				stream.Close ();
		}
		
		private void CheckDisposed () 
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		private static string ReadHttpLine (Stream stream)
		{
			StringBuilder line = new StringBuilder();
			byte last = (byte)'\n';
			bool read_last = false;
			int c;
			
			while ((c = stream.ReadByte ()) != -1) {
				if (c == '\r') {
					if ((last = (byte) stream.ReadByte ()) == '\n') // headers; not at EOS
						break;
					read_last = true;
				}

				line.Append ((char) c);
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

		class HttpWebResponseStream : NetworkStream
		{
			bool disposed;
			bool chunked;
			int chunkSize;
			int chunkLeft;
			bool readingChunkSize;
			EventHandler onClose;

			public HttpWebResponseStream (Socket socket, EventHandler onClose)
				: base (socket, FileAccess.Read, false)
			{
				this.onClose = onClose;
				chunkSize = -1;
				chunkLeft = 0;
			}

			public bool Chunked {
				get { return chunked; }
				set { chunked = value; }
			}
			
			protected override void Dispose (bool disposing)
			{
				if (disposed)
					return;

				disposed = true;
				if (disposing) {
					/* This does not work !??
					if (Socket.Connected)
						Socket.Shutdown (SocketShutdown.Receive);
					*/

					if (onClose != null)
						onClose (this, EventArgs.Empty);
				}

				onClose = null;
				base.Dispose (disposing);
			}

			void ReadChunkSize ()
			{
				bool cr = false;
				bool lf = false;
				int size = 0;
				// 8 hex digits should be enough
				for (int i = 0; i < 10; i++) {
					char c = Char.ToUpper ((char) ReadByte ());
					if (c == '\r') {
						if (!cr) {
							cr = true;
							continue;
						}
						throw new IOException ("Bad stream: 2 CR");
					} 
					
					if (c == '\n' && cr == true) {
						if (!lf) {
							lf = true;
							break;
						}

						throw new IOException ("Bad stream: got LF but no CR");
					}
					
					if (i < 8 && ((c >= '0' && c <= '9') || c >= 'A' && c <= 'F')) {
						size = size << 4;
						if (c >= 'A' && c <= 'F')
							size += c - 'A' + 10;
						else
							size += c - '0';
						continue;
					}

					throw new IOException ("Bad stream: got " + c);
				}

				if (!cr || !lf)
					throw new IOException ("Bad stream: no CR or LF after chunk size");

				chunkSize = size;
				chunkLeft = size;
			}

			int GetMaxSizeFromChunkLeft (int requestedSize)
			{
				if (!chunked)
					return requestedSize;

				if (chunkSize == -1 || chunkLeft == 0) {
					lock (this) {
						if (chunkSize == -1 || chunkLeft == 0) {
							readingChunkSize = true;
							try {
								ReadChunkSize ();
							} finally {
								readingChunkSize = false;
							}
						}
					}
				}

				return (chunkLeft < requestedSize) ? chunkLeft : requestedSize;
			}
			
			public override IAsyncResult BeginRead (byte [] buffer, int offset, int size,
								AsyncCallback callback, object state)
			{
				CheckDisposed ();				
				IAsyncResult retval;

				if (buffer == null)
					throw new ArgumentNullException ("buffer is null");

				int len = buffer.Length;
				if (offset < 0 || offset >= len)
					throw new ArgumentOutOfRangeException ("offset exceeds the size of buffer");

				if (offset + size < 0 || offset+size > len)
					throw new ArgumentOutOfRangeException ("offset+size exceeds the size of buffer");

				if (!readingChunkSize)
					size = GetMaxSizeFromChunkLeft (size);

				try {
					retval = base.BeginRead (buffer, offset, size, callback, state);
				} catch {
					throw new IOException ("BeginReceive failure");
				}

				return retval;
			}

			public override int EndRead (IAsyncResult ar)
			{
				CheckDisposed ();
				int res;

				if (ar == null)
					throw new ArgumentNullException ("async result is null");

				try {
					res = base.EndRead (ar);
				} catch (Exception e) {
					throw new IOException ("EndRead failure", e);
				}

				AdjustChunkLeft (res);
				return res;
			}

			public override int Read (byte [] buffer, int offset, int size)
			{
				CheckDisposed ();
				int res;

				if (buffer == null)
					throw new ArgumentNullException ("buffer is null");

				if (offset < 0 || offset >= buffer.Length)
					throw new ArgumentOutOfRangeException ("offset exceeds the size of buffer");

				if (offset + size < 0 || offset + size > buffer.Length)
					throw new ArgumentOutOfRangeException ("offset+size exceeds the size of buffer");

				if (!readingChunkSize)
					size = GetMaxSizeFromChunkLeft (size);

				try {
					res = base.Read (buffer, offset, size);
				} catch (Exception e) {
					throw new IOException ("Read failure", e);
				}

				AdjustChunkLeft (res);

				return res;
			}

			void CheckDisposed ()
			{
				if (disposed)
					throw new ObjectDisposedException (GetType ().FullName);
			}

			void AdjustChunkLeft (int read)
			{
				if (!chunked)
					return;

				chunkLeft -= read;
				if (chunkLeft < 0)
					chunkLeft = 0;
			}
		}
	}	
}

