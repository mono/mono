#define EMBEDDED_IN_1_0

//
// System.Net.ListenerAsyncResult
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2005 Ximian, Inc (http://www.ximian.com)
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

#if EMBEDDED_IN_1_0

using System.Threading;
using System; using System.Net; namespace MonoHttp {
	class ListenerAsyncResult : IAsyncResult {
		ManualResetEvent handle;
		bool synch;
		bool completed;
		AsyncCallback cb;
		object state;
		Exception exception;
		HttpListenerContext context;
		object locker = new object ();

		public ListenerAsyncResult (AsyncCallback cb, object state)
		{
			this.cb = cb;
			this.state = state;
		}

		internal void Complete (string error)
		{
			//FIXME: error_code?
			exception = new HttpListenerException (0, error);
			lock (locker) {
				completed = true;
				if (handle != null)
					handle.Set ();

				if (cb != null)
					ThreadPool.QueueUserWorkItem (InvokeCallback, this);
			}
		}

		static void InvokeCallback (object o)
		{
			ListenerAsyncResult ares = (ListenerAsyncResult) o;
			ares.cb (ares);
		}

		internal void Complete (HttpListenerContext context)
		{
			Complete (context, false);
		}

		internal void Complete (HttpListenerContext context, bool synch)
		{
			this.synch = synch;
			this.context = context;
			lock (locker) {
				completed = true;
				if (handle != null)
					handle.Set ();

				if ((context.Listener.AuthenticationSchemes == AuthenticationSchemes.Basic || context.Listener.AuthenticationSchemes == AuthenticationSchemes.Negotiate) && context.Request.Headers ["Authorization"] == null) {
					context.Listener.EndGetContext (this);
					context.Response.StatusCode = 401;
					context.Response.Headers ["WWW-Authenticate"] = AuthenticationSchemes.Basic + " realm=\"\"";
					context.Response.OutputStream.Close ();
					context.Listener.BeginGetContext (cb, state);
				} else if (cb != null)
					ThreadPool.QueueUserWorkItem (InvokeCallback, this);
			}
		}

		internal HttpListenerContext GetContext ()
		{
			if (exception != null)
				throw exception;

			return context;
		}
		
		public object AsyncState {
			get { return state; }
		}

		public WaitHandle AsyncWaitHandle {
			get {
				lock (locker) {
					if (handle == null)
						handle = new ManualResetEvent (completed);
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
					return completed;
				}
			}
		}
	}
}
#endif

