//
// System.Net.FileWebResponse
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
	public class FileWebResponse : WebResponse, ISerializable, IDisposable
	{
		private Uri responseUri;
		private FileStream fileStream;
		private long contentLength;
		private WebHeaderCollection webHeaders;
		private bool disposed = false;
		
		// Constructors
		
		internal FileWebResponse (Uri responseUri, FileStream fileStream)
		{
			try {
				this.responseUri = responseUri;
				this.fileStream = fileStream;
				this.contentLength = fileStream.Length;
				this.webHeaders = new WebHeaderCollection ();
				this.webHeaders.Add ("Content-Length", Convert.ToString (contentLength));
				this.webHeaders.Add ("Content-Type", "binary/octet-stream");
			} catch (Exception e) {
				throw new WebException (e.Message, e);
			}
		}
		
		protected FileWebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;

			responseUri = (Uri) info.GetValue ("responseUri", typeof (Uri));
			contentLength = info.GetInt64 ("contentLength");
			webHeaders = (WebHeaderCollection) info.GetValue ("webHeaders", typeof (WebHeaderCollection));
		}
		
		// Properties
		
		public override long ContentLength {		
			get {
				CheckDisposed ();
				return this.contentLength;
			}
		}
		
		public override string ContentType {		
			get {
				CheckDisposed ();
				return "binary/octet-stream";
			}
		}
		
		public override WebHeaderCollection Headers {		
			get {
				CheckDisposed ();
				return this.webHeaders;
			}
		}
		
		public override Uri ResponseUri {		
			get {
				CheckDisposed ();
				return this.responseUri;
			}
		}		

		// Methods

		void ISerializable.GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;

			info.AddValue ("responseUri", responseUri, typeof (Uri));
			info.AddValue ("contentLength", contentLength);
			info.AddValue ("webHeaders", webHeaders, typeof (WebHeaderCollection));
		}		

		public override Stream GetResponseStream()
		{
			CheckDisposed ();
			return this.fileStream;
		}
				
		// Cleaning up stuff
		
		~FileWebResponse ()
		{
			Dispose (false);
		}		
		
		public override void Close()
		{
			((IDisposable) this).Dispose ();
		}

		void IDisposable.Dispose()
		{
			Dispose (true);
			
			// see spec, suppress finalization of this object.
			GC.SuppressFinalize (this);  
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (this.disposed)
				return;
			this.disposed = true;
			
			if (disposing) {
				// release managed resources
				this.responseUri = null;
				this.webHeaders = null;
			}
			
			// release unmanaged resources
			FileStream stream = fileStream;
			fileStream = null;
			if (stream != null)
				stream.Close (); // also closes webRequest
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}		
	}
}
