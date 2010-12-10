//
// System.Web.QueueManager
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web.Configuration;

namespace System.Web
{
	sealed class QueueManager
	{
		// keep the defaults in sync with the ones in HttpRuntimeSection.cs
		int minFree = 8;
		int minLocalFree = 4;
		int queueLimit = 5000;
		Queue queue;
		bool disposing;
		Exception initialException;
		PerformanceCounter requestsQueuedCounter;
		
		public QueueManager ()
		{
			Exception ex = null;
			
			try {
				HttpRuntimeSection config = HttpRuntime.Section;
				if (config != null) {
					minFree = config.MinFreeThreads;
					minLocalFree = config.MinLocalRequestFreeThreads;
					queueLimit = config.AppRequestQueueLimit;
				}
			} catch (Exception e) {
				ex = e;
			}

			try {
				queue = new Queue (queueLimit);
			} catch (Exception e) {
				if (ex == null) {
					initialException = e;
				} else {
					StringBuilder sb = new StringBuilder ("Several exceptions occurred:\n");
					sb.AppendFormat ("--- Exception Q1:\n{0}\n", ex.ToString ());
					sb.AppendFormat ("--- Exception Q2:\n{0}\n", e.ToString ());
					initialException = new Exception (sb.ToString ());
				}
			}

			if (initialException == null && ex != null)
				initialException = ex;

			requestsQueuedCounter = new PerformanceCounter ("ASP.NET", "Requests Queued");
			requestsQueuedCounter.RawValue = 0;
		}

		public bool HasException {
			get { return initialException != null; }
		}

		public Exception InitialException {
			get { return initialException; }
		}
		
		bool CanExecuteRequest (HttpWorkerRequest req)
		{
			if (disposing)
				return false;
				
#if TARGET_J2EE
			return true; // The J2EE app server manages the thread pool
#else
			int threads, cports;
			ThreadPool.GetAvailableThreads (out threads, out cports);
			bool local = (req != null && req.GetLocalAddress () == "127.0.0.1");
			return (threads > minFree) || (local && threads > minLocalFree);
#endif
		}

		public HttpWorkerRequest GetNextRequest (HttpWorkerRequest req)
		{
			if (!CanExecuteRequest (req)) {
				if (!disposing && req != null) {
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
				requestsQueuedCounter.Increment ();
				return;
			}

			HttpRuntime.FinishUnavailable (wr);
		}

		HttpWorkerRequest Dequeue ()
		{
			if (queue.Count > 0) {
				HttpWorkerRequest request = (HttpWorkerRequest) queue.Dequeue ();
				requestsQueuedCounter.Decrement ();
				return request;
			}

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

