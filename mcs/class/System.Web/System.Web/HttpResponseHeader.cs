// 
// System.Web.HttpResponseHeader
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
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

namespace System.Web {
	class HttpResponseHeader {
		string header;
		string val;
		int header_id;
		static char [] CRLF = { '\r', '\n' };

		internal HttpResponseHeader (int KnowHeaderId, string val)
		{
			header_id = KnowHeaderId;
			this.val = val;
		}

		internal HttpResponseHeader (string header, string val) {
			this.header = header;
			this.val = val;
		}

		internal string Name {
			get {
				if (null == header)
					return HttpWorkerRequest.GetKnownResponseHeaderName (header_id);

				return header;
			}
		}

		internal string Value {
			get { return val; }
			set { val = value; }
		}

		internal void SendContent (HttpWorkerRequest WorkerRequest)
		{
			// use URL encoding on the value (see bug #75392)
			// but only for CR and LF. Other characters are left untouched.
			string actual_val = val;
			if (actual_val != null) {
				int crlf = actual_val.IndexOfAny (CRLF);
				if (crlf >= 0) {
					actual_val = actual_val.Replace ("\r", "%0d");
					actual_val = actual_val.Replace ("\n", "%0a");
				}
			}

			if (null != header) {
				WorkerRequest.SendUnknownResponseHeader (header, actual_val);
			} else {
				WorkerRequest.SendKnownResponseHeader (header_id, actual_val);
			}
		}
	}
}
