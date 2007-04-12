//
// System.Web.HeadersCollection
//
// Authors:
//   Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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
using System.Runtime.Serialization;

namespace System.Web
{
	class HeadersCollection : BaseParamsCollection
	{

		public HeadersCollection(HttpRequest request):base(request)
		{
		}

		protected override void InsertInfo()
		{
			HttpWorkerRequest worker_request = _request.WorkerRequest;
			if (null != worker_request) 
			{
				for (int i = 0; i < HttpWorkerRequest.RequestHeaderMaximum; i++) {
					string hval = worker_request.GetKnownRequestHeader (i);

					if (hval == null || hval == "")
						continue;

					Add (HttpWorkerRequest.GetKnownRequestHeaderName (i), hval);
				}

				string [] [] unknown = worker_request.GetUnknownRequestHeaders ();
				if (unknown != null && unknown.GetUpperBound (0) != -1) {
					int top = unknown.GetUpperBound (0) + 1;

					for (int i = 0; i < top; i++) {
						// should check if unknown [i] is not null, but MS does not. 

						Add (unknown [i] [0], unknown [i] [1]);
					}
				}
				Protect ();
			}
		}

		protected override string InternalGet(string name)
		{
			int headerIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(name);
			string headerValue = null;
			if (headerIndex >= 0)
				headerValue = _request.WorkerRequest.GetKnownRequestHeader(headerIndex);
			if (headerValue == null)
				headerValue = _request.WorkerRequest.GetUnknownRequestHeader(name);
			return headerValue;			
		}
	}
}
