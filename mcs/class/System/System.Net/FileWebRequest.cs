//
// System.Net.FileWebRequest
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
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace System.Net 
{
	[Serializable]
	public class FileWebRequest : WebRequest, ISerializable
	{
		private Uri uri;
		private WebHeaderCollection webHeaders;
		
		private ICredentials credentials;
		private string connectionGroup;
		private long contentLength;
		private FileAccess fileAccess = FileAccess.Read;
		private string method = "GET";
		private IWebProxy proxy;
		private bool preAuthenticate;
		private int timeout = 100000;
		
		private Stream requestStream;
		private FileWebResponse webResponse;
		private AutoResetEvent requestEndEvent;
		private bool requesting;
		private bool asyncResponding;
		
		// Constructors
		
		internal FileWebRequest (Uri uri) 
		{ 
			this.uri = uri;
 			this.webHeaders = new WebHeaderCollection ();
		}
		
		[Obsolete ("Serialization is obsoleted for this type", false)]
		protected FileWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			SerializationInfo info = serializationInfo;
			webHeaders = (WebHeaderCollection) info.GetValue ("headers", typeof (WebHeaderCollection));
			proxy = (IWebProxy) info.GetValue ("proxy", typeof (IWebProxy));
			uri = (Uri) info.GetValue ("uri", typeof (Uri));
			connectionGroup = info.GetString ("connectionGroupName");
			method = info.GetString ("method");
			contentLength = info.GetInt64 ("contentLength");
			timeout = info.GetInt32 ("timeout");
			fileAccess = (FileAccess) info.GetValue ("fileAccess", typeof (FileAccess));
			preAuthenticate = info.GetBoolean ("preauthenticate");
		}
		
		// Properties
		
		// currently not used according to spec
		public override string ConnectionGroupName {
			get { return connectionGroup; }
			set { connectionGroup = value; }
		}
		
		public override long ContentLength {
			get { return contentLength; }
			set {
				if (value < 0)
					throw new ArgumentException ("The Content-Length value must be greater than or equal to zero.", "value");
				contentLength =  value;
			}
		}
		
		public override string ContentType { 
			get { return webHeaders ["Content-Type"]; }
			set { webHeaders ["Content-Type"] = value; }
		}
		
		public override ICredentials Credentials { 
			get { return credentials; }
			set { credentials = value; }
		}
		
		public override WebHeaderCollection Headers { 
			get { return webHeaders; }
		}
		
		// currently not used according to spec
		public override string Method { 
			get { return this.method; }
			set {
				if (value == null || value.Length == 0)
					throw new ArgumentException ("Cannot set null or blank "
						+ "methods on request.", "value");
				this.method = value;
			}
		}
		
		// currently not used according to spec
		public override bool PreAuthenticate { 
			get { return preAuthenticate; }
			set { preAuthenticate = value; }
		}
		
		// currently not used according to spec
		public override IWebProxy Proxy {
			get { return proxy; }
			set { proxy = value; }
		}
		
		public override Uri RequestUri { 
			get { return this.uri; }
		}
		
		public override int Timeout { 
			get { return timeout; }
			set { 
				if (value < -1)
					throw new ArgumentOutOfRangeException ("Timeout can be "
						+ "only set to 'System.Threading.Timeout.Infinite' "
						+ "or a value >= 0.");
				timeout = value;
			}
		}

		public override bool UseDefaultCredentials
		{
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}
		// Methods
		
		private delegate Stream GetRequestStreamCallback ();
		private delegate WebResponse GetResponseCallback ();

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}
		
		/* LAMESPEC: Docs suggest this was present in 1.1 and
		 * 1.0 profiles, but the masterinfos say otherwise
		 */
		[MonoTODO]
		public override void Abort ()
		{
			throw GetMustImplement ();
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			if (string.Compare ("GET", method, true) == 0 ||
				string.Compare ("HEAD", method, true) == 0 ||
				string.Compare ("CONNECT", method, true) == 0)
				throw new ProtocolViolationException ("Cannot send a content-body with this verb-type.");
			lock (this) {
				if (asyncResponding || webResponse != null)
					throw new InvalidOperationException ("This operation cannot be performed after the request has been submitted.");
				if (requesting)
					throw new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				requesting = true;
			}
			GetRequestStreamCallback c = new GetRequestStreamCallback (this.GetRequestStreamInternal);
			return c.BeginInvoke (callback, state);
		}
		
		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();
			AsyncResult async = (AsyncResult) asyncResult;
			GetRequestStreamCallback cb = (GetRequestStreamCallback) async.AsyncDelegate;
			return cb.EndInvoke (asyncResult);
		}

		public override Stream GetRequestStream()
		{
			IAsyncResult asyncResult = BeginGetRequestStream (null, null);
			if (!(asyncResult.AsyncWaitHandle.WaitOne (timeout, false))) {
				throw new WebException("The request timed out", WebExceptionStatus.Timeout);
			}
			return EndGetRequestStream (asyncResult);
		}
		
		internal Stream GetRequestStreamInternal ()
		{
			this.requestStream = new FileWebStream (
						this,
						FileMode.Create,
						FileAccess.Write, 
						FileShare.Read);
			return this.requestStream;
		}
		
		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			lock (this) {
				if (asyncResponding)
					throw new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				asyncResponding = true;
			}
			GetResponseCallback c = new GetResponseCallback (this.GetResponseInternal);
			return c.BeginInvoke (callback, state);
		}
		
		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();
			AsyncResult async = (AsyncResult) asyncResult;
			GetResponseCallback cb = (GetResponseCallback) async.AsyncDelegate;
			FileWebResponse webResponse = (FileWebResponse) cb.EndInvoke(asyncResult);
			asyncResponding = false;
			if (webResponse.HasError)
				throw webResponse.Error;
			return webResponse;
		}
		
		public override WebResponse GetResponse ()
		{
			IAsyncResult asyncResult = BeginGetResponse (null, null);
			if (!(asyncResult.AsyncWaitHandle.WaitOne (timeout, false))) {
				throw new WebException("The request timed out", WebExceptionStatus.Timeout);
			}
			return EndGetResponse (asyncResult);
		}
		
		WebResponse GetResponseInternal ()
		{
			if (webResponse != null)
				return webResponse;
			lock (this) {
				if (requesting) {
					requestEndEvent = new AutoResetEvent (false);
				}
			}
			if (requestEndEvent != null) {
				requestEndEvent.WaitOne ();
			}
			FileStream fileStream = null;
			try {
				fileStream = new FileWebStream (this, FileMode.Open, FileAccess.Read, FileShare.Read);
				this.webResponse = new FileWebResponse (this.uri, fileStream);
			} catch (Exception ex) {
				this.webResponse = new FileWebResponse (this.uri, new WebException (ex.Message, ex));
			}
			return this.webResponse;
		}
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			GetObjectData (serializationInfo, streamingContext);
		}

		protected override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;
			info.AddValue ("headers", webHeaders, typeof (WebHeaderCollection));
			info.AddValue ("proxy", proxy, typeof (IWebProxy));
			info.AddValue ("uri", uri, typeof (Uri));
			info.AddValue ("connectionGroupName", connectionGroup);
			info.AddValue ("method", method);
			info.AddValue ("contentLength", contentLength);
			info.AddValue ("timeout", timeout);
			info.AddValue ("fileAccess", fileAccess);
			info.AddValue ("preauthenticate", false);
		}
		
		internal void Close ()
		{
			// already done in class below
			// if (requestStream != null) {
			// 	requestStream.Close ();
			// }

			lock (this) {
				requesting = false;
				if (requestEndEvent != null) 
					requestEndEvent.Set ();
				// requestEndEvent = null;
			}
		}
		
		// to catch the Close called on the FileStream
		internal class FileWebStream : FileStream
		{
			FileWebRequest webRequest;
			
			internal FileWebStream (FileWebRequest webRequest,    
					   	FileMode mode,
					   	FileAccess access,
					   	FileShare share)
				: base (webRequest.RequestUri.LocalPath, 
					mode, access, share)
			{
				this.webRequest = webRequest;
			}
			
			public override void Close() 
			{
				base.Close ();
				FileWebRequest req = webRequest;
				webRequest = null;
				if (req != null)
					req.Close ();
			}
		}
	}
}
