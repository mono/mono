//
// SoapWebResponse.cs: Soap Web Response List
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using Microsoft.Web.Services;
using System;
using System.IO;
using System.Net;

namespace Microsoft.Web.Services {

	public class SoapWebResponse : WebResponse {

		private SoapWebRequest request;
		private Stream stream;
		private WebResponse response;

		internal SoapWebResponse (SoapWebRequest soapRequest) 
		{
			request = soapRequest;
			response = soapRequest.GetResponse ();
			stream = response.GetResponseStream ();
		}

		public override Stream GetResponseStream () 
		{
			return stream;
		}

		public override long ContentLength { 
			get { return stream.Length; }
		}

		public override string ContentType { 
			get { return request.Request.ContentType; }
		}

		public override WebHeaderCollection Headers {
			get { return response.Headers; }
		}

		public SoapContext SoapContext { 
			get { return null; }
		}
	} 
}
