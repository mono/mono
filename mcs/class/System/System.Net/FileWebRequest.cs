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

namespace System.Net 
{
	[Serializable]
	public class FileWebRequest : WebRequest, ISerializable
	{
		private Uri uri;
		private WebHeaderCollection webHeaders;
		
		private ICredentials credentials;
		private string connectionGroup;
		private string method;
		private int timeout;
		private bool open = false;
		
		// Constructors
		
		internal FileWebRequest (Uri uri) 
		{ 
			this.uri = uri;
			this.webHeaders = new WebHeaderCollection ();
			this.method = "GET";
			this.timeout = System.Threading.Timeout.Infinite; 
		}		
		
		[MonoTODO]
		protected FileWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			throw new NotImplementedException ();
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

// TODO: bit simplistic this, need to add code to check for exceptions..
// TODO: use Timeout value
// TODO: as the spec says GetResponse can throw an exception on timeout, 
// and because in theory one could start an async GetResponse, change 
// properties, and start a sync GetResponse it seems best that GetResponse 
// actually works via the async methods and the async delegates delegate 
// to a private get response method. timeout value probably isn't an 
// issue here, but it is in HttpWebRequest.

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{		
			GetRequestStreamCallback c = new GetRequestStreamCallback (this.GetRequestStream);
			return c.BeginInvoke (callback, state);
		}
		
		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			GetResponseCallback c = new GetResponseCallback (this.GetResponse);
			return c.BeginInvoke (callback, state);
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			AsyncResult async = (AsyncResult) asyncResult;
			GetRequestStreamCallback cb = (GetRequestStreamCallback) async.AsyncDelegate;
			asyncResult.AsyncWaitHandle.WaitOne ();
			return cb.EndInvoke(asyncResult);
		}
		
		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			AsyncResult async = (AsyncResult) asyncResult;
			GetResponseCallback cb = (GetResponseCallback) async.AsyncDelegate;
			asyncResult.AsyncWaitHandle.WaitOne ();
			return cb.EndInvoke(asyncResult);		
		}
		
		public override Stream GetRequestStream()
		{
			if (method == null || (!method.Equals ("PUT") && !method.Equals ("POST")))
				throw new ProtocolViolationException ("Cannot send file when method is: " + this.method + ". Method must be PUT.");
			if (open)
				throw new WebException ("Stream already open");
			open = true;
			FileStream fileStream = new FileWebStream (
							this,
						     	FileMode.CreateNew, 
							FileAccess.Write, 
							FileShare.Read,
							4096,
							false);
			return fileStream;					
		}
		
		public override WebResponse GetResponse()
		{
			if (method == null || !method.Equals ("GET"))
				throw new ProtocolViolationException ("Cannot retrieve file when method is: " + this.method + ". Method must be GET.");
			if (open)
				throw new WebException ("Stream already open");
			FileStream fileStream = new FileWebStream (
							this,
						     	FileMode.Open, 
							FileAccess.Read, 
							FileShare.Read,
							4096,
							false);
 			return new FileWebResponse (this, fileStream);
		}
		
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
		
		internal void Close ()
		{
			open = false;
		}
		
		internal bool IsClosed ()
		{
			return !open;
		}
		
		// to catch the Close called on the FileStream
		internal class FileWebStream : FileStream
		{
			FileWebRequest webRequest;
			
			internal FileWebStream (FileWebRequest webRequest,    
					   	FileMode mode,
					   	FileAccess access,
					   	FileShare share,
					   	int bufferSize,
					   	bool useAsync)
				: base (webRequest.RequestUri.LocalPath, 
					mode, access, share, bufferSize, useAsync)					   	
			{
				this.webRequest = webRequest;
			}
					   	
			public override void Close() 
			{
				base.Close ();
				webRequest.Close ();
			}
		}
	}
}