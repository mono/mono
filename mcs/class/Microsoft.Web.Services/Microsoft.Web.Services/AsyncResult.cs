//
// Microsoft.Web.Services.AsyncResult.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Threading;

namespace Microsoft.Web.Services
{
	public class AsyncResult : IAsyncResult
	{
		private AsyncCallback _callback;
		private bool _completed;
		private bool _completedSync;
		private bool _endCalled;
		private ManualResetEvent _event;
		private Exception _exception;
		private object _state;

		protected AsyncResult (object s) : this (null, s)
		{
		}

		protected AsyncResult (AsyncCallback call, object s) : base ()
		{
			_callback = call;
			_state = s;
		}

		protected AsyncResult () : this (null, null)
		{
		}

		protected void Complete (bool csync, Exception e)
		{
			_completed = true;
			_completedSync = csync;
			_exception = e;

			if(_event != null) {
				_event.Set ();
			}
			try {
				if(_callback != null) {
					_callback (this);
				}
			} catch (Exception ex) {
				ThreadPool.QueueUserWorkItem (new WaitCallback (ThrowException), this);
			}
		}

		protected void Complete (bool csync)
		{
			this.Complete (csync, null);
		}

		public static void End (IAsyncResult result)
		{
			if(result == null) {
				throw new ArgumentNullException ("result");
			}
			AsyncResult mws_result = (AsyncResult) result;

			if(mws_result == null) {
				throw new ArgumentException ("Invalid result");
			}

			if(mws_result._endCalled == true) {
				throw new InvalidOperationException ("Async Operation already finished");
			}

			mws_result._endCalled = true;

			if(mws_result._completed == true) {
				mws_result.AsyncWaitHandle.WaitOne ();
			}

			if(mws_result._exception != null) {
				throw mws_result._exception;
			}
		}
		
		private void ThrowException (object o)
		{
			Exception e = (Exception) o;
			throw e;
		}

		public object AsyncState {
			get { return _state; }
		}

		public WaitHandle AsyncWaitHandle {
			get {
				if(_event == null) {
					bool complete = _completed;

					lock (this) {
						_event = new ManualResetEvent (_completed);
					}
					if(complete == true || _completed == false) {
						_event.Set ();
					}
				}
				return _event;
			}
		}

		public bool CompletedSynchronously {
			get { return _completedSync; }
		}

		public bool IsCompleted {
			get { return _completed; }
		}
	}
}
