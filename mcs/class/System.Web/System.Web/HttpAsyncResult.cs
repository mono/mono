// 
// System.Web.HttpAsyncResult
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
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
using System.Threading;

namespace System.Web
{
	internal class HttpAsyncResult : IAsyncResult
	{
		private object _asyncState;
		private AsyncCallback _callback;
		private bool _ready;
		private bool _readySync;
		private Exception _error;

		internal HttpAsyncResult(AsyncCallback callback, object state) {
			_callback = callback;
			_asyncState = state;
		}

		internal void Complete(bool sync, object result, Exception error) {
			_ready = true;
			_readySync = sync;
			_error = error;
			if (null != _callback) {
				_callback(this);
			}
		}

		internal Exception Error {
			get {
				return _error;
			}
		}

		public object AsyncState {
			get {
				return _asyncState;
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
