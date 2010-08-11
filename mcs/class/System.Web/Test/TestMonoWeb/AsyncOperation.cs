using System;
using System.Threading;
using System.Web;

namespace TestMonoWeb
{
	class AsynchOperation : IAsyncResult {
		private bool _completed;
		private Object _state;
		private AsyncCallback _callback;
		private HttpContext _context;

		bool IAsyncResult.IsCompleted { get { return _completed; } }
		WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
		Object IAsyncResult.AsyncState { get { return _state; } }
		bool IAsyncResult.CompletedSynchronously { get { return false; } }

		public HttpContext Context {
			get {
				return _context;
			}
		}

		public AsynchOperation(AsyncCallback callback, HttpContext context, Object state) {
			_callback = callback;
			_context = context;
			_state = state;
			_completed = false;
		}

		public void StartAsyncWork() {
			ThreadPool.QueueUserWorkItem(new WaitCallback(DoSomething), null /*workItemState*/);
		}

		private void DoSomething(Object workItemState) {
			// Just for testing..
			Thread.Sleep(100);
			_completed = true;
			try {
				_callback(this);
			} catch {}
		}
	}
}
