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
		private FileWebRequest webRequest;
		private FileStream fileStream;
		private long contentLength;
		private WebHeaderCollection webHeaders;
		private bool disposed = false;
		
		// Constructors
		
		protected FileWebResponse () { }
		
		internal FileWebResponse (FileWebRequest webRequest, FileStream fileStream)
		{
			try {
				this.webRequest = webRequest;
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
				return this.webRequest.RequestUri;
			}
		}		

		// Methods
		
		public override void Close()
		{
			this.fileStream.Close ();
		}
		
		public override Stream GetResponseStream()
		{
			CheckDisposed ();
			return this.fileStream;
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				this.disposed = true;
				this.webRequest = null;
				this.webHeaders = null;
				this.Close ();
				// this.fileStream.Dispose ();
				this.fileStream = null;				
			}
		}
		
		void IDisposable.Dispose()
		{
			Dispose (true);
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException ("stream");
		}
		
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}		
	}
}