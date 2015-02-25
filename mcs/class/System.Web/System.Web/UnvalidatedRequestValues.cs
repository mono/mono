//
// System.Web.UnvalidatedRequestValues.cs
//
// Author:
//   Mike Morano <mmorano@mikeandwan.us>
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
using System.Collections.Specialized;


namespace System.Web {
	public sealed class UnvalidatedRequestValues {
		public HttpCookieCollection Cookies { get; internal set; }
		public HttpFileCollection Files { get; internal set; }
		public NameValueCollection Form { get; internal set; }
		public NameValueCollection Headers { get; internal set; }
		public string Path { get; internal set; }
		public string PathInfo { get; internal set; }
		public NameValueCollection QueryString { get; internal set; }
		public string RawUrl { get; internal set; }
		public Uri Url { get; internal set; }

		public string this[string field] { 
			get {
				if (Form != null && Form [field] != null) {
	                    		return Form [field];
	                	}

				if (Cookies != null && Cookies [field] != null) {
	                		return Cookies [field].Value;
	                	}

				if (QueryString != null && QueryString [field] != null) {
	                		return QueryString [field];
	                	}

	                	// msdn docs also suggest the ServerVariables are inspected by this indexer,
	                	// but that seems odd given what is available in this class

	                	return null;
	        	}
	        }
	}
}
