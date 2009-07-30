//
// System.Web.Caching.CachedRawResponse
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace System.Web.Caching {

	internal sealed class CachedRawResponse {
		static readonly byte[] emptyBuffer = new byte[0];
		HttpCachePolicy policy;
		CachedVaryBy varyby;
		int status_code;
		string status_desc;
		int content_length;
		NameValueCollection headers;
		byte[] buffer;
		
		internal CachedRawResponse (HttpCachePolicy policy)
		{
			this.policy = policy;
			this.buffer = emptyBuffer;
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
		
		internal NameValueCollection Headers {
			get { return headers; }
		}

		internal void SetHeaders (NameValueCollection headers) {
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

