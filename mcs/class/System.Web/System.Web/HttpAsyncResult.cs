// 
// System.Web.HttpAsyncResult
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
//
using System;
using System.Threading;

namespace System.Web
{
	internal class HttpAsyncResult : IAsyncResult
	{
		private object _result;
		private object _asyncState;
		private AsyncCallback _callback;
		private Exception _error;

		private bool _ready;
		private bool _readySync;

		internal HttpAsyncResult(AsyncCallback callback, object state) {
			_callback = callback;
			_asyncState = state;
		}

		internal void Complete(bool sync, object result, Exception error) {
			_ready = true;
			_readySync = sync;
			_result = result;
			_error = error;
			if (null != _callback) {
				_callback(this);
			}
		}

		internal Exception Error {
			get {
				return null;
			}
		}

		public object AsyncState {
			get {
				return _asyncState;
			}
		}

		public object AsyncObject {
			get {
				return null;
			}
		}

		public WaitHandle AsyncWaitHandle {
			get {
				return null;
			}
		}

		public bool CompletedSynchronously {
			get {
				return _readySync;
			}
		}

		public bool IsCompleted {
			get {
				return _ready;
			}
		}
	}
}
