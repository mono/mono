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
		
		protected FileWebResponse () { }
		
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
		
		[MonoTODO]
		protected FileWebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
		
		// Properties
		
		public override long ContentLength {		
			get {
				try { return this.contentLength; }
				finally { CheckDisposed (); }
			}
		}
		
		public override string ContentType {		
			get {
				try { return "binary/octet-stream"; }
				finally { CheckDisposed (); }
			}
		}
		
		public override WebHeaderCollection Headers {		
			get {
				try { return this.webHeaders; }
				finally { CheckDisposed (); }
			}
		}
		
		public override Uri ResponseUri {		
			get {
				try { return this.responseUri; }
				finally { CheckDisposed (); }
			}
		}		

		// Methods

		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			try {
				throw new NotImplementedException ();
			} finally { CheckDisposed (); }
		}		

		public override Stream GetResponseStream()
		{
			try { return this.fileStream; }
			finally { CheckDisposed (); }
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