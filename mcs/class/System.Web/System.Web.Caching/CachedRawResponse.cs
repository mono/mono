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
		private CachedVaryBy varyby;
		private int status_code;
		private string status_desc;
		private int content_length;
		private ArrayList headers;
		private HttpResponseHeader date_header;
		private byte[] buffer;
		
		internal CachedRawResponse (HttpCachePolicy policy)
		{
			this.policy = policy;
			this.varyby = varyby;
			this.buffer = new byte [HttpWriter.MaxBufferSize];
		}

		internal HttpCachePolicy Policy {
			get { return policy; }
			set { policy = value; }
		}

		internal CachedVaryBy VaryBy {
			get { return varyby; }
			set { varyby = value; }
		}
		
		internal int StatusCode {
			get { return status_code; }
			set { status_code = value; }
		}

		internal string StatusDescription {
			get { return status_desc; }
			set { status_desc = value; }
		}

		internal int ContentLength {
			get { return content_length; }
			set { content_length = value; }
		}
		
		internal ArrayList Headers {
			get { return headers; }
		}

		internal HttpResponseHeader DateHeader {
			get { return date_header; }
			set { date_header = value; }
		}
		
		internal void SetHeaders (ArrayList headers) {
			this.headers = headers;
		}

		internal void SetData (byte[] buffer)
		{
			this.buffer = buffer;
		}
		
		internal byte[] GetData ()
		{
			return buffer;
		}
	}
}

