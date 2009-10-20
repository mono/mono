//
// System.Web.Caching.CachedRawResponse
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//  Marek Habersack <mhabersack@novell.com>
//
// (C) 2003-2009 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Web.Caching
{
	sealed class CachedRawResponse
	{
		public sealed class DataItem
		{
			public readonly byte[] Buffer;
			public readonly long Length;
#if NET_2_0
			public readonly HttpResponseSubstitutionCallback Callback;
#endif
			
			public DataItem (byte[] buffer, long length)
			{
				Buffer = buffer;
				Length = length;
			}

#if NET_2_0
			public DataItem (HttpResponseSubstitutionCallback callback) : this (null, 0)
			{
				Callback = callback;
			}
#endif
		}
		
		HttpCachePolicy policy;
		CachedVaryBy varyby;
		int status_code;
		string status_desc;
		int content_length;
		NameValueCollection headers;
#if NET_2_0
		List <DataItem> data;
#else
		ArrayList data;
#endif

		IList Data {
			get {
				if (data == null) {
#if NET_2_0
					data = new List <DataItem> ();
#else
					data = new ArrayList ();
#endif
				}

				return data;
			}
		}
		
		public CachedRawResponse (HttpCachePolicy policy)
		{
			this.policy = policy;
		}

		public HttpCachePolicy Policy {
			get { return policy; }
			set { policy = value; }
		}

		public CachedVaryBy VaryBy {
			get { return varyby; }
			set { varyby = value; }
		}
		
		public int StatusCode {
			get { return status_code; }
			set { status_code = value; }
		}

		public string StatusDescription {
			get { return status_desc; }
			set { status_desc = value; }
		}

		public NameValueCollection Headers {
			get { return headers; }
		}

		public void SetHeaders (NameValueCollection headers)
		{
			this.headers = headers;
		}

		public void SetData (MemoryStream ms)
		{
			if (ms == null)
				return;
			
			Data.Add (new DataItem (ms.GetBuffer (), ms.Length));
		}

#if NET_2_0
		public void SetData (HttpResponseSubstitutionCallback callback)
		{
			if (callback == null)
				return;

			Data.Add (new DataItem (callback));
		}
#endif
		
		public IList GetData ()
		{
			int count = data != null ? data.Count :0;
			if (count == 0)
				return null;

			return data;
		}
	}
}

