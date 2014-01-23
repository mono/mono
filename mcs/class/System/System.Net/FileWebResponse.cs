//
// System.Net.FileWebResponse
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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
		private bool disposed;
		Exception exception;
		
		// Constructors
		
		internal FileWebResponse (Uri responseUri, FileStream fileStream)
		{
			try {
				this.responseUri = responseUri;
				this.fileStream = fileStream;
				this.contentLength = fileStream.Length;
				this.webHeaders = new WebHeaderCollection ();
				this.webHeaders.Add ("Content-Length", Convert.ToString (contentLength));
				this.webHeaders.Add ("Content-Type", "application/octet-stream");
			} catch (Exception e) {
				throw new WebException (e.Message, e);
			}
		}

		internal FileWebResponse (Uri responseUri, WebException exception)
		{
			this.responseUri = responseUri;
			this.exception = exception;
		}
		
		[Obsolete ("Serialization is obsoleted for this type", false)]
		protected FileWebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;

			responseUri = (Uri) info.GetValue ("responseUri", typeof (Uri));
			contentLength = info.GetInt64 ("contentLength");
			webHeaders = (WebHeaderCollection) info.GetValue ("webHeaders", typeof (WebHeaderCollection));
		}
		
		// Properties
		internal bool HasError {
			get { return exception != null; }
		}

		internal Exception Error {
			get { return exception; }
		}
		
		public override long ContentLength {
			get {
				CheckDisposed ();
				return this.contentLength;
			}
		}
		
		public override string ContentType {
			get {
				CheckDisposed ();
				return "application/octet-stream";
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
			GetObjectData (serializationInfo, streamingContext);
		}

		protected override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
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

#if TARGET_JVM //enable overrides for extenders
		public override void Dispose()
#else
		void IDisposable.Dispose()
#endif
		{
			Dispose (true);
			
			// see spec, suppress finalization of this object.
			GC.SuppressFinalize (this);  
		}
		
#if NET_4_0
		protected override
#endif		
		void Dispose (bool disposing)
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
#if NET_4_0
			base.Dispose (disposing);
#endif
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}		
	}
}
