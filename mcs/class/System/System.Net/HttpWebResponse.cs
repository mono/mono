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
			this.uri = uri;
			this.method = method;
			this.responseStream = responseStream;
			
			// TODO: parse headers from responseStream
			
			this.statusCode = HttpStatusCode.OK;
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
				
				// LAMESPEC: a simple test reveal this always 
				// returns an empty collection. It is not filled 
				// with the values from the Set-Cookie or 
				// Set-Cookie2 response headers, which is a bit
				// of a shame..
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
					// TODO: accept more than rfc1123 dates
					DateTime dt = DateTime.ParseExact (dtStr, "r", null);
					return dt;
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
	}	
}