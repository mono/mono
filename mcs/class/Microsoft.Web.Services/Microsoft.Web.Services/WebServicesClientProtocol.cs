//
// WebServicesClientProtocol.cs: Web Services Client Protocol
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Net;
using System.Web.Services.Protocols;

namespace Microsoft.Web.Services {

	public class WebServicesClientProtocol : SoapHttpClientProtocol {

		private Pipeline _pipeline;
		private SoapContext _requestContext;
		private SoapContext _responseContext;

		public WebServicesClientProtocol () {}

		public Pipeline Pipeline { 
			get { 
				if (_pipeline == null)
					_pipeline = new Pipeline ();
				return _pipeline; 
			}
			set {
				if (value == null)
					throw new System.ArgumentNullException ("value");
				_pipeline = value;
			}
		}

		public SoapContext RequestSoapContext {
			get { 
				if (_requestContext == null)
					_requestContext = new SoapContext (); 
				return _requestContext;
			}
		}

		public SoapContext ResponseSoapContext { 
			get { 
				if (_responseContext == null)
					_responseContext = new SoapContext (); 
				return _responseContext;
			}
		}

		[MonoTODO("something is missing")]
		public new string Url { 
			get { return base.Url; }
			set { base.Url = value; }
		}

		protected override WebRequest GetWebRequest (Uri uri) 
		{
			SoapWebRequest request = new SoapWebRequest (uri);
			RequestSoapContext.CopyTo (request.SoapContext);
			request.Pipeline = Pipeline;
			return request;
		}

		protected override WebResponse GetWebResponse (WebRequest request) 
		{
			WebResponse response = request.GetResponse ();
			//response.SoapContext.CopyTo (ResponseSoapContext);
			return response;
		}

		[MonoTODO("do not support IAsyncResult")]
		protected override WebResponse GetWebResponse (WebRequest request, IAsyncResult result) 
		{
			SoapWebResponse response = (SoapWebResponse) request.GetResponse ();
			response.SoapContext.CopyTo (ResponseSoapContext);
			return response;
		}
	}
}
