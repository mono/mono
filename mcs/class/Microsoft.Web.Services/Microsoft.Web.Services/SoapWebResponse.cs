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
		private MemoryStream ms;
		private WebResponse response;

		internal SoapWebResponse (SoapWebRequest soapRequest) 
		{
			request = soapRequest;
			response = soapRequest.Request.GetResponse ();
		}

		public override Stream GetResponseStream () 
		{
			SoapEnvelope envelope = new SoapEnvelope ();
			Stream s = response.GetResponseStream ();
			envelope.Load (s);
			request.Pipeline.ProcessInputMessage (envelope);

			ms = new MemoryStream ();
			envelope.Save (ms);
			ms.Position = 0; // ready to be read
			return ms;
		}

		public override long ContentLength { 
			get { return ((ms == null) ? 0 : ms.Length); }
		}

		public override string ContentType { 
			get { return request.Request.ContentType; }
		}

		public override WebHeaderCollection Headers {
			get { return response.Headers; }
		}

		public SoapContext SoapContext { 
			get { return request.SoapContext; }
		}
	} 
}
