//
// System.IO/FileStreamAsyncResult.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
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

		public byte [] Buffer;
		public int Offset;
		public int Count;
		public int OriginalCount;
		public int BytesRead;

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

