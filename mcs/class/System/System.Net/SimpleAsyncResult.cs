//
// SimpleAsyncResult.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Martin Baulig (martin.baulig@xamarin.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
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

using System.IO;
using System.Threading;

namespace System.Net
{
	delegate void SimpleAsyncCallback (SimpleAsyncResult result);

	delegate bool SimpleAsyncFunc (SimpleAsyncResult result);

	class SimpleAsyncResult : IAsyncResult
	{
		ManualResetEvent handle;
		bool synch;
		bool isCompleted;
		SimpleAsyncCallback cb;
		object state;
		bool callbackDone;
		Exception exc;
		object locker = new object ();

		SimpleAsyncResult (SimpleAsyncCallback cb)
		{
			this.cb = cb;
		}

		protected SimpleAsyncResult (AsyncCallback cb, object state)
		{
			this.state = state;
			this.cb = result => {
				if (cb != null)
					cb (this);
			};
		}

		public static void Run (SimpleAsyncFunc func, SimpleAsyncCallback callback)
		{
			var result = new SimpleAsyncResult (callback);
			try {
				if (!func (result))
					result.SetCompleted (true);
			} catch (Exception ex) {
				result.SetCompleted (true, ex);
			}
		}

		public static void RunWithLock (object locker, SimpleAsyncFunc func, SimpleAsyncCallback callback)
		{
			Run (inner => {
				bool running = func (inner);
				if (running)
					Monitor.Exit (locker);
				return running;
			}, inner => {
				if (inner.GotException) {
					if (inner.CompletedSynchronously)
						Monitor.Exit (locker);
					callback (inner);
					return;
				}

				try {
					if (!inner.CompletedSynchronously)
						Monitor.Enter (locker);

					callback (inner);
				} finally {
					Monitor.Exit (locker);
				}
			});
		}

		protected void Reset_internal ()
		{
			callbackDone = false;
			exc = null;
			lock (locker) {
				isCompleted = false;
				if (handle != null)
					handle.Reset ();
			}
		}

		internal void SetCompleted (bool synch, Exception e)
		{
			SetCompleted_internal (synch, e);
			DoCallback_private ();
		}

		internal void SetCompleted (bool synch)
		{
			SetCompleted_internal (synch);
			DoCallback_private ();
		}

		protected void SetCompleted_internal (bool synch, Exception e)
		{
			this.synch = synch;
			exc = e;
			lock (locker) {
				isCompleted = true;
				if (handle != null)
					handle.Set ();
			}
		}

		protected void SetCompleted_internal (bool synch)
		{
			this.synch = synch;
			exc = null;
			lock (locker) {
				isCompleted = true;
				if (handle != null)
					handle.Set ();
			}
		}

		void DoCallback_private ()
		{
			if (callbackDone)
				return;
			callbackDone = true;
			if (cb == null)
				return;
			cb (this);
		}

		protected void DoCallback_internal ()
		{
			if (!callbackDone && cb != null) {
				callbackDone = true;
				cb (this);
			}
		}

		internal void WaitUntilComplete ()
		{
			if (IsCompleted)
				return;

			AsyncWaitHandle.WaitOne ();
		}

		internal bool WaitUntilComplete (int timeout, bool exitContext)
		{
			if (IsCompleted)
				return true;

			return AsyncWaitHandle.WaitOne (timeout, exitContext);
		}

		public object AsyncState {
			get { return state; }
		}

		public WaitHandle AsyncWaitHandle {
			get {
				lock (locker) {
					if (handle == null)
						handle = new ManualResetEvent (isCompleted);
				}

				return handle;
			}
		}

		public bool CompletedSynchronously {
			get { return synch; }
		}

		public bool IsCompleted {
			get {
				lock (locker) {
					return isCompleted;
				}
			}
		}

		internal bool GotException {
			get { return (exc != null); }
		}

		internal Exception Exception {
			get { return exc; }
		}
	}
}

