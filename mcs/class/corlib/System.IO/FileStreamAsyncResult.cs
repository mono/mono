//
// System.IO/FileStreamAsyncResult.cs
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
	class FileStreamAsyncResult : IAsyncResult {
		/* Same structure in the runtime */
		object state;
		bool completed;
		bool done;
		Exception exc;
		ManualResetEvent wh;
		AsyncCallback cb;
		bool completedSynch;
		
#pragma warning disable 649
		public byte [] Buffer;
		public int Offset;
		public int Count;
		public int OriginalCount;
		public int BytesRead;
#pragma warning restore 649		

		AsyncCallback realcb;

		public FileStreamAsyncResult (AsyncCallback cb, object state)
		{
			this.state = state;
			this.realcb = cb;
			if (realcb != null)
				this.cb = new AsyncCallback (CBWrapper);
			wh = new ManualResetEvent (false);
		}

		static void CBWrapper (IAsyncResult ares)
		{
			FileStreamAsyncResult res = (FileStreamAsyncResult) ares;
			res.realcb.BeginInvoke (ares, null, null);
		}

		public void SetComplete (Exception e)
		{
			exc = e;
			completed = true;
			wh.Set ();
			if (cb != null)
				cb (this);
		}
		
		public void SetComplete (Exception e, int nbytes)
		{
			this.BytesRead = nbytes;
			SetComplete (e);
		}

		public void SetComplete (Exception e, int nbytes, bool synch)
		{
			completedSynch = synch;
			SetComplete (e, nbytes);
		}

		public object AsyncState {
			get { return state; }
		}

		public bool CompletedSynchronously {
			get { return completedSynch; }
		}

		public WaitHandle AsyncWaitHandle {
			get { return wh; }
		}

		public bool IsCompleted {
			get { return completed; }
		}

		public Exception Exception {
			get { return exc; }
		}

		public bool Done {
			get { return done; }
			set { done = value; }
		}
	}
}

