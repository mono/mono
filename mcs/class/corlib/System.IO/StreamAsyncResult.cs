//
// System.IO/StreamAsyncResult.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
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

