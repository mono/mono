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

		public SoapWebRequest (string uri) : this (new Uri (uri)) {}

		public SoapWebRequest (Uri uri) : base ()
		{
			this.uri = uri;
			context = new SoapContext (null);
		}

		[MonoTODO("incomplete - only call base class")]
		public override IAsyncResult BeginGetRequestStream (AsyncCallback cb, object state) 
		{
			return base.BeginGetRequestStream (cb, state);
		}
		
		[MonoTODO("incomplete - only call base class")]
		public override IAsyncResult BeginGetResponse (AsyncCallback cb, object state) 
		{
			return base.BeginGetResponse (cb, state);
		}
		
		[MonoTODO("incomplete - only call base class")]
		public override Stream EndGetRequestStream (IAsyncResult asyncResult) 
		{
			return base.EndGetRequestStream (asyncResult);
		}
		
		[MonoTODO("incomplete - only call base class")]
		public override WebResponse EndGetResponse (IAsyncResult asyncResult) 
		{
			return base.EndGetResponse (asyncResult);
		}

		[MonoTODO("incomplete - only call base class")]
		public override Stream GetRequestStream () 
		{
			return base.GetRequestStream ();
		}

		[MonoTODO("incomplete - only call base class")]
		public override WebResponse GetResponse () 
		{
			return base.GetResponse ();
		}

		[MonoTODO("incomplete - only call base class")]
		public override string ConnectionGroupName {
			get { return base.ConnectionGroupName; }
			set { base.ConnectionGroupName = value; }
		}

		[MonoTODO("incomplete - only call base class")]
		public override string ContentType { 
			get { return base.ContentType; }
			set { base.ContentType = value; } 
		}

		[MonoTODO("incomplete - only call base class")]
		public override ICredentials Credentials { 
			get { return base.Credentials; }
			set { base.Credentials = value; } 
		}

		[MonoTODO("incomplete - only call base class")]
		public override WebHeaderCollection Headers { 
			get { return base.Headers; }
		}

		[MonoTODO("incomplete - only call base class")]
		public override string Method { 
			get { return base.Method; } 
			set { base.Method = value; }
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

		[MonoTODO("incomplete - only call base class")]
		public override bool PreAuthenticate { 
			get { return base.PreAuthenticate; }
			set { base.PreAuthenticate = value; }
		}

		public WebRequest Request { 
			get { return null; } 
		}

		public override Uri RequestUri { 
			get { return uri; }
		}

		public SoapContext SoapContext { 
			get { return context; }
		}

		[MonoTODO("incomplete - only call base class")]
		public override int Timeout { 
			get { return base.Timeout; }
			set { base.Timeout = value; }
		}
	}
}
