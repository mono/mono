//
// Mono.Http.GzipWebRequest
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Net;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using ICSharpCode.SharpZipLib.GZip;

namespace Mono.Http
{
	[Serializable]
	public class GZipWebRequest : WebRequest, ISerializable
	{
		WebRequest request;
		bool enabled;

		public GZipWebRequest (WebRequest request)
		{
			if (request == null)
				throw new ArgumentNullException ("request");

			this.request = request;
			enabled = true;
		}

		protected GZipWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			SerializationInfo info = serializationInfo;
			request = (WebRequest) info.GetValue ("request", typeof (WebRequest));
			enabled = info.GetBoolean ("enabled");
		}

		public override string ConnectionGroupName { 
			get { return request.ConnectionGroupName; }
			set { request.ConnectionGroupName = value; }
		}
		
		public override long ContentLength { 
			get { return request.ContentLength; }
			set { request.ContentLength = value; }
		}
		
		public override string ContentType { 
			get { return request.ContentType; }
			set { request.ContentType = value; }
		}
		
		public override ICredentials Credentials { 
			get { return request.Credentials; }
			set { request.Credentials = value; }
		}
		
		public override WebHeaderCollection Headers { 
			get { return request.Headers; }
			set { request.Headers = value; }
		}
		
		public override string Method { 
			get { return request.Method; }
			set { request.Method = value; }
		}
		
		public override bool PreAuthenticate { 
			get { return request.PreAuthenticate; }
			set { request.PreAuthenticate = value; }
		}
		
		public override IWebProxy Proxy { 
			get { return request.Proxy; }
			set { request.Proxy = value; }
		}
		
		public override Uri RequestUri { 
			get { return request.RequestUri; }
		}
		
		public override int Timeout { 
			get { return request.Timeout; }
			set { request.Timeout = value; }
		}
		
		public WebRequest RealRequest {
			get { return request; }
		}

		public bool EnableCompression {
			get { return enabled; }
			set { enabled = value; }
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			info.AddValue ("request", request, typeof (WebRequest));
			info.AddValue ("enabled", enabled);
		}
		
		public override void Abort ()
		{
			request.Abort ();
		}
		
		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			return request.BeginGetRequestStream (callback, state);
		}
		
		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			return request.EndGetRequestStream (asyncResult);
		}
		
		public override Stream GetRequestStream ()
		{
			return request.GetRequestStream ();
		}
		
		void CheckHeader ()
		{
			if (!enabled)
				return;

			string accept = request.Headers ["Accept-Encoding"];
			if (accept == null || accept == "")
				request.Headers ["Accept-Encoding"] = "gzip; q=1.0, identity";
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			CheckHeader ();
			return request.BeginGetResponse (callback, state);
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			WebResponse response = request.EndGetResponse (asyncResult);
			bool compressed = (String.Compare (response.Headers ["Content-Encoding"], "gzip", true) == 0);
			return new GZipWebResponse (response, compressed);
		}
		
		public override WebResponse GetResponse ()
		{
			IAsyncResult result = BeginGetResponse (null, null);
			return EndGetResponse (result);
		}
	}
}

