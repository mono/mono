//
// System.IO/StreamAsyncResult.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Threading;

namespace System.IO
{
	class StreamAsyncResult : IAsyncResult {
		object state;
		bool completed;
		bool done;
		Exception exc;
		int nbytes = -1;
		ManualResetEvent wh;

		public StreamAsyncResult (object state)
		{
			this.state = state;
		}

		public void SetComplete (Exception e)
		{
			exc = e;
			completed = true;
			lock (this) {
				if (wh != null)
					wh.Set ();
			}
		}
		
		public void SetComplete (Exception e, int nbytes)
		{
			this.nbytes = nbytes;
			SetComplete (e);
		}

		public object AsyncState {
			get { return state; }
		}

		public WaitHandle AsyncWaitHandle {
			get {
				lock (this) {
					if (wh == null)
						wh = new ManualResetEvent (completed);

					return wh;
				}
			}
		}

		public virtual bool CompletedSynchronously {
			get { return true; }
		}

		public bool IsCompleted {
			get { return completed; }
		}

		public Exception Exception {
			get { return exc; }
		}

		public int NBytes {
			get { return nbytes; }
		}

		public bool Done {
			get { return done; }
			set { done = value; }
		}
	}
}

