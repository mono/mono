//
// System.Net.WebAsyncResult
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.IO;
using System.Threading;

namespace System.Net
{
	class WebAsyncResult : IAsyncResult
	{
		ManualResetEvent handle;
		bool synch;
		bool isCompleted;
		AsyncCallback cb;
		object state;
		int nbytes;
		IAsyncResult innerAsyncResult;
		bool callbackDone;
		Exception exc;
		HttpWebRequest request;
		HttpWebResponse response;
		Stream writeStream;
		byte [] buffer;
		int offset;
		int size;

		public WebAsyncResult (AsyncCallback cb, object state)
		{
			this.cb = cb;
			this.state = state;
		}

		public WebAsyncResult (HttpWebRequest request, AsyncCallback cb, object state)
		{
			this.request = request;
			this.cb = cb;
			this.state = state;
		}

		public WebAsyncResult (AsyncCallback cb, object state, byte [] buffer, int offset, int size)
		{
			this.cb = cb;
			this.state = state;
			this.buffer = buffer;
			this.offset = offset;
			this.size = size;
		}

		internal void SetCompleted (bool synch, Exception e)
		{
			isCompleted = true;
			this.synch = synch;
			exc = e;
			((ManualResetEvent) AsyncWaitHandle).Set ();
		}
		
		internal void Reset ()
		{
			isCompleted = false;
			callbackDone = false;
			exc = null;
			request = null;
			response = null;
			writeStream = null;
			exc = null;
			if (handle != null)
				handle.Reset ();
		}

		internal void SetCompleted (bool synch, int nbytes)
		{
			isCompleted = true;
			this.synch = synch;
			this.nbytes = nbytes;
			exc = null;
			((ManualResetEvent) AsyncWaitHandle).Set ();
		}
		
		internal void SetCompleted (bool synch, Stream writeStream)
		{
			isCompleted = true;
			this.synch = synch;
			this.writeStream = writeStream;
			exc = null;
			((ManualResetEvent) AsyncWaitHandle).Set ();
		}
		
		internal void SetCompleted (bool synch, HttpWebResponse response)
		{
			isCompleted = true;
			this.synch = synch;
			this.response = response;
			exc = null;
			((ManualResetEvent) AsyncWaitHandle).Set ();
		}
		
		internal void DoCallback ()
		{
			if (!callbackDone && cb != null) {
				callbackDone = true;
				cb (this);
			}
		}
		
		internal void WaitUntilComplete ()
		{
			if (isCompleted)
				return;

			AsyncWaitHandle.WaitOne ();
		}

		internal bool WaitUntilComplete (int timeout, bool exitContext)
		{
			if (isCompleted)
				return true;

			return AsyncWaitHandle.WaitOne (timeout, exitContext);
		}

		public object AsyncState {
			get { return state; }
		}

		public WaitHandle AsyncWaitHandle {
			get {
				if (handle == null) {
					lock (this) {
						if (handle == null)
							handle = new ManualResetEvent (isCompleted);
					}
				}
				
				return handle;
			}
		}

		public bool CompletedSynchronously {
			get { return synch; }
		}

		public bool IsCompleted {
			get { return isCompleted; }
		}

		internal bool GotException {
			get { return (exc != null); }
		}
		
		internal Exception Exception {
			get { return exc; }
		}
		
		internal int NBytes {
			get { return nbytes; }
			set { nbytes = value; }
		}

		internal IAsyncResult InnerAsyncResult {
			get { return innerAsyncResult; }
			set { innerAsyncResult = value; }
		}

		internal Stream WriteStream {
			get { return writeStream; }
		}

		internal HttpWebResponse Response {
			get { return response; }
		}

		internal byte [] Buffer {
			get { return buffer; }
		}

		internal int Offset {
			get { return offset; }
		}

		internal int Size {
			get { return size; }
		}
	}
}

