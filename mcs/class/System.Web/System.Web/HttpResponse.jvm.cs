using System;
using System.Collections.Generic;
using System.Text;

namespace System.Web
{
	partial class HttpResponse
	{
		internal void SetWorkerRequest (HttpWorkerRequest wr) {
			WorkerRequest = wr;
		}

	}
}
