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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Threading;

namespace System.Windows.Forms {

	internal class AsyncMethodResult : IAsyncResult {

		private Mutex handle;
		private object state;
		private bool completed;
		private object return_value;

		public AsyncMethodResult ()
		{
			handle = new Mutex ();
		}

		public virtual WaitHandle AsyncWaitHandle {
			get {
				lock (this) {
					return handle;
				}
			}
		}

		public object AsyncState {
			get { return state; }
			set { state = value; }
		}

		public bool CompletedSynchronously {
			get { return false; }
		}

		public bool IsCompleted {
			get {
				lock (this) {
					return completed;
				}
			}
		}
		
		public object EndInvoke ()
		{
			handle.WaitOne ();
			return return_value;
		}

		public void Complete (object result)
		{
			lock (this) {
				completed = true;
				return_value = result;
				handle.ReleaseMutex ();
			}
		}
	}

}


