//
// System.Net.FileWebRequest
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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
		private string method = "GET";
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
		
		protected FileWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			SerializationInfo info = serializationInfo;

			method = info.GetString ("method");
			uri = (Uri) info.GetValue ("uri", typeof (Uri));
			timeout = info.GetInt32 ("timeout");
			connectionGroup = info.GetString ("connectionGroup");
			webHeaders = (WebHeaderCollection) info.GetValue ("webHeaders", typeof (WebHeaderCollection));
		}
		
		// Properties
		
		// currently not used according to spec
		public override string ConnectionGroupName { 
			get { return connectionGroup; }
			set { connectionGroup = value; }
		}
		
		public override long ContentLength { 
			get { 
				try {
					return Int64.Parse (webHeaders ["Content-Length"]); 
				} catch (Exception) {
					return 0;
				}
			}
			set { 
				if (value < 0)
					throw new ArgumentException ("value");
				webHeaders ["Content-Length"] = Convert.ToString (value);
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
			set { this.method = value; }
		}
		
		// currently not used according to spec
		public override bool PreAuthenticate { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		// currently not used according to spec
		public override IWebProxy Proxy { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public override Uri RequestUri { 
			get { return this.uri; }
		}
		
		public override int Timeout { 
			get { return timeout; }
			set { 
				if (value < 0)
					throw new ArgumentException ("value");
				timeout = value;
			}
		}
		
		// Methods
		
		private delegate Stream GetRequestStreamCallback ();
		private delegate WebResponse GetResponseCallback ();

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{		
			if (method == null || (!method.Equals ("PUT") && !method.Equals ("POST")))
				throw new ProtocolViolationException ("Cannot send file when method is: " + this.method + ". Method must be PUT.");
			// workaround for bug 24943
			Exception e = null;
			lock (this) {
				if (asyncResponding || webResponse != null)
					e = new InvalidOperationException ("This operation cannot be performed after the request has been submitted.");
				else if (requesting)
					e = new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				else
					requesting = true;
			}
			if (e != null)
				throw e;
			/*
			lock (this) {
				if (asyncResponding || webResponse != null)
					throw new InvalidOperationException ("This operation cannot be performed after the request has been submitted.");
				if (requesting)
					throw new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				requesting = true;
			}
			*/
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
						FileMode.CreateNew,
						FileAccess.Write, 
						FileShare.Read);
			return this.requestStream;
		}
		
		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			// workaround for bug 24943
			Exception e = null;
			lock (this) {
				if (asyncResponding)
					e = new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				else 
					asyncResponding = true;
			}
			if (e != null)
				throw e;
			/*
			lock (this) {
				if (asyncResponding)
					throw new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				asyncResponding = true;
			}
			*/
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
			WebResponse webResponse = cb.EndInvoke(asyncResult);
			asyncResponding = false;
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
			FileStream fileStream = new FileWebStream (
							this,
						     	FileMode.Open, 
							FileAccess.Read, 
							FileShare.Read);
 			this.webResponse = new FileWebResponse (this.uri, fileStream);
 			return (WebResponse) this.webResponse;
		}
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;

			info.AddValue ("method", method);
			info.AddValue ("uri", uri, typeof (Uri));
			info.AddValue ("timeout", timeout);
			info.AddValue ("connectionGroup", connectionGroup);
			info.AddValue ("webHeaders", webHeaders, typeof (WebHeaderCollection));
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
