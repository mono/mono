//
// System.Web.QueueManager
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003,2004 Novell, Inc (http://www.novell.com)
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
using System.Threading;
using System.Web.Configuration;

namespace System.Web
{
	class QueueManager
	{
		int minFree;
		int minLocalFree;
		int queueLimit;
		Queue queue;
		bool disposing;

		public QueueManager ()
		{
			HttpRuntimeConfig config;
			config = (HttpRuntimeConfig) HttpContext.GetAppConfig ("system.web/httpRuntime");
			minFree = config.MinFreeThreads;
			minLocalFree = config.MinLocalRequestFreeThreads;
			queueLimit = config.AppRequestQueueLimit;
			queue = new Queue (queueLimit);
		}

		// TODO: handle local connections, just check for 127.0.0.1
		bool CanExecuteRequest ()
		{
			if (disposing)
				return false;
				
			int threads, cports;
			ThreadPool.GetAvailableThreads (out threads, out cports);
			return (threads > minFree); // || (local && threads > minLocalFree);
		}

		public HttpWorkerRequest GetNextRequest (HttpWorkerRequest req)
		{
			if (!CanExecuteRequest ()) {
				if (req != null) {
					lock (queue) {
						Queue (req);
					}
				}

				return null;
			}

			HttpWorkerRequest result;
			lock (queue) {
				result = Dequeue ();
				if (result != null) {
					if (req != null)
						Queue (req);
				} else {
					result = req;
				}
			}

			return result;
		}
		
		void Queue (HttpWorkerRequest wr)
		{
			if (queue.Count < queueLimit) {
				queue.Enqueue (wr);
				return;
			}

			HttpRuntime.FinishUnavailable (wr);
		}

		HttpWorkerRequest Dequeue ()
		{
			if (queue.Count > 0)
				return (HttpWorkerRequest) queue.Dequeue ();

			return null;
		}

		public void Dispose ()
		{
			if (disposing)
				return;

			disposing = true;
			HttpWorkerRequest wr;
			while ((wr = GetNextRequest (null)) != null)
				HttpRuntime.FinishUnavailable (wr);

			queue = null;
		}
	}
}

