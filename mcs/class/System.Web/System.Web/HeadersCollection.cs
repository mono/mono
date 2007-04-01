using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace System.Web
{
	/// <summary>
	/// Summary description for HeadersCollection.
	/// </summary>
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
