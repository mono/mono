//
// System.Web.QueueManager
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
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

		// TODO: handle local connections
		public bool CanExecuteRequest (bool local)
		{
			if (disposing)
				return false;
				
			int threads, cports;
			ThreadPool.GetAvailableThreads (out threads, out cports);
			return (threads > minFree) || (local && threads > minLocalFree);
		}
		
		public void Queue (HttpWorkerRequest wr)
		{
			lock (queue) {
				if (queue.Count < queueLimit) {
					queue.Enqueue (wr);
					return;
				}
			}

			HttpRuntime.FinishUnavailable (wr);
		}

		public HttpWorkerRequest Dequeue ()
		{
			lock (queue) {
				if (queue.Count > 0)
					return (HttpWorkerRequest) queue.Dequeue ();
			}

			return null;
		}

		public void Dispose ()
		{
			if (disposing)
				return;

			disposing = true;
			HttpWorkerRequest wr;
			while ((wr = Dequeue ()) != null)
				HttpRuntime.FinishUnavailable (wr);

			queue = null;
		}
	}
}

