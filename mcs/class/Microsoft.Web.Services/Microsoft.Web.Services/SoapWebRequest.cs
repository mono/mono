//
// SoapWebRequest.cs: Soap Web Request
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Net;

namespace Microsoft.Web.Services {

	public class SoapWebRequest : WebRequest {

		private Uri uri;
		private SoapContext context;
		private Pipeline pipeline;
		private WebRequest request;

		public SoapWebRequest (string uri) : this (new Uri (uri)) {}

		public SoapWebRequest (Uri uri) : base ()
		{
			this.uri = uri;
			context = new SoapContext (null);
		}

		[MonoTODO]
		public override IAsyncResult BeginGetRequestStream (AsyncCallback cb, object state) 
		{
			return Request.BeginGetRequestStream (cb, state);
		}
		
		[MonoTODO]
		public override IAsyncResult BeginGetResponse (AsyncCallback cb, object state) 
		{
			return Request.BeginGetResponse (cb, state);
		}
		
		[MonoTODO]
		public override Stream EndGetRequestStream (IAsyncResult asyncResult) 
		{
			return Request.EndGetRequestStream (asyncResult);
		}
		
		[MonoTODO]
		public override WebResponse EndGetResponse (IAsyncResult asyncResult) 
		{
			return Request.EndGetResponse (asyncResult);
		}

		public override Stream GetRequestStream () 
		{
			Stream s = Request.GetRequestStream ();
			SoapEnvelope env = new SoapEnvelope (context);
			return new ChainStream (s, env, Pipeline);
		}

		public override WebResponse GetResponse () 
		{
			return new SoapWebResponse (this);
		}

		public override string ConnectionGroupName {
			get { return Request.ConnectionGroupName; }
			set { Request.ConnectionGroupName = value; }
		}

		public override string ContentType { 
			get { return Request.ContentType; }
			set { Request.ContentType = value; } 
		}

		public override ICredentials Credentials { 
			get { return Request.Credentials; }
			set { Request.Credentials = value; } 
		}

		public override WebHeaderCollection Headers { 
			get { return Request.Headers; }
		}

		public override string Method { 
			get { return Request.Method; } 
			set { Request.Method = value; }
		}

		public Pipeline Pipeline { 
			get { 
				// if none set, then get the default pipeline
				if (pipeline == null)
					pipeline = new Pipeline ();
				return pipeline; 
			} 
			set { pipeline = value; }
		}

		public override bool PreAuthenticate { 
			get { return Request.PreAuthenticate; }
			set { Request.PreAuthenticate = value; }
		}

		public WebRequest Request { 
			get { 
				if (request == null)
					request = WebRequest.Create (uri);
				return request;
			} 
		}

		public override Uri RequestUri { 
			get { return uri; }
		}

		public SoapContext SoapContext { 
			get { return context; }
		}

		public override int Timeout { 
			get { return Request.Timeout; }
			set { Request.Timeout = value; }
		}
	}
}
