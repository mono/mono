//
// System.Web.Caching.CachedRawResponse
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;
using System.Text;
using System.Collections;

namespace System.Web.Caching {

	internal class CachedRawResponse {

		private HttpCachePolicy policy;
		private Hashtable vparams;
		private int status_code;
		private string status_desc;
		private ArrayList headers;
		private byte[] buffer;
		
		internal CachedRawResponse (HttpCachePolicy policy, HttpRequest request)
		{
			this.policy = policy;
			this.buffer = new byte [HttpWriter.MaxBufferSize];
			SetVariedParams (request);
		}

		internal HttpCachePolicy Policy {
			get { return policy; }
			set { policy = value; }
		}
		internal int StatusCode {
			get { return status_code; }
			set { status_code = value; }
		}

		internal string StatusDescription {
			get { return status_desc; }
			set { status_desc = value; }
		}

		internal ArrayList Headers {
			get { return headers; }
		}
		
		internal void SetHeaders (ArrayList headers) {
			this.headers = headers;
		}

		internal void SetData (byte[] buffer)
		{
			buffer.CopyTo (this.buffer, 0);
		}
		
		internal void SetResponseHeaders (HttpResponse response)
		{
			foreach (HttpResponseHeader hdr in headers)
				response.AppendHeader (hdr.Name, hdr.Value);
		}
		
		internal byte[] GetData ()
		{
			return buffer;
		}

		internal void SetVariedParams (HttpRequest request)
		{
			if (policy.VaryByParams.IgnoreParams)
				return;

			vparams = new Hashtable ();
			foreach (string key in request.Params.Keys) {
				if (!policy.VaryByParams [key])
					continue;
				vparams [key] = request.Params [key];
			}
		}
		
		internal bool ParamsVary (HttpRequest request)
		{
			if (policy.VaryByParams.IgnoreParams)
				return false;

			foreach (string key in vparams) {
				string val = request.Params [key];
				if (val == null || val != vparams [key])
					return true;
			}

			return false;
		}
	}
}

