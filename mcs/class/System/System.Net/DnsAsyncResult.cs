//
// System.Net.DnsAsyncResult
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo.mono@gmail.com)
//
// Copyright (c) 2011 Gonzalo Paniagua Javier
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
using Mono.Net.Dns;

namespace System.Net
{
	class DnsAsyncResult : IAsyncResult
	{
		static WaitCallback internal_cb = new WaitCallback (CB);
		ManualResetEvent handle;
		bool synch;
		bool is_completed;
		AsyncCallback callback;
		object state;
		IPHostEntry entry;
		Exception exc;

		public DnsAsyncResult (AsyncCallback cb, object state)
		{
			this.callback = cb;
			this.state = state;
		}

		public void SetCompleted (bool synch, IPHostEntry entry, Exception e)
		{
			this.synch = synch;
			this.entry = entry;
			exc = e;
			lock (this) {
				if (is_completed)
					return;
				is_completed = true;
				if (handle != null)
					handle.Set ();
			}
			if (callback != null)
				ThreadPool.QueueUserWorkItem (internal_cb, this);
		}

		public void SetCompleted (bool synch, Exception e)
		{
			SetCompleted (synch, null, e);
		}

		public void SetCompleted (bool synch, IPHostEntry entry)
		{
			SetCompleted (synch, entry, null);
		}

		static void CB (object _this)
		{
			DnsAsyncResult ares = (DnsAsyncResult) _this;
			ares.callback (ares);
		}

		public object AsyncState {
			get { return state; }
		}

		public WaitHandle AsyncWaitHandle {
			get {
				lock (this) {
					if (handle == null)
						handle = new ManualResetEvent (is_completed);
				}
				return handle;
			}
		}

		public Exception Exception {
			get { return exc; }
		}

		public IPHostEntry HostEntry {
			get { return entry; }
		}

		public bool CompletedSynchronously {
			get { return synch; }
		}

		public bool IsCompleted {
			get {
				lock (this) {
					return is_completed;
				}
			}
		}
	}
}

