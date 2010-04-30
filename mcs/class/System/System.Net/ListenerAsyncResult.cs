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

#if NET_2_0 && SECURITY_DEP

using System.Threading;
namespace System.Net {
	class ListenerAsyncResult : IAsyncResult {
		ManualResetEvent handle;
		bool synch;
		bool completed;
		AsyncCallback cb;
		object state;
		Exception exception;
		HttpListenerContext context;
		object locker = new object ();
		ListenerAsyncResult forward;

		public ListenerAsyncResult (AsyncCallback cb, object state)
		{
			this.cb = cb;
			this.state = state;
		}

		internal void Complete (string error)
		{
			if (forward != null) {
				forward.Complete (error);
				return;
			}
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
			if (ares.forward != null) {
				InvokeCallback (ares.forward);
				return;
			}
			try {
				ares.cb (ares);
			} catch {
			}
		}

		internal void Complete (HttpListenerContext context)
		{
			Complete (context, false);
		}

		internal void Complete (HttpListenerContext context, bool synch)
		{
			if (forward != null) {
				forward.Complete (context, synch);
				return;
			}
			this.synch = synch;
			this.context = context;
			lock (locker) {
				AuthenticationSchemes schemes = context.Listener.SelectAuthenticationScheme (context);
				if ((schemes == AuthenticationSchemes.Basic || context.Listener.AuthenticationSchemes == AuthenticationSchemes.Negotiate) && context.Request.Headers ["Authorization"] == null) {
					context.Response.StatusCode = 401;
					context.Response.Headers ["WWW-Authenticate"] = schemes + " realm=\"" + context.Listener.Realm + "\"";
					context.Response.OutputStream.Close ();
					IAsyncResult ares = context.Listener.BeginGetContext (cb, state);
					this.forward = (ListenerAsyncResult) ares;
					lock (forward.locker) {
						if (handle != null)
							forward.handle = handle;
					}
					ListenerAsyncResult next = forward;
					for (int i = 0; next.forward != null; i++) {
						if (i > 20)
							Complete ("Too many authentication errors");
						next = next.forward;
					}
				} else {
					completed = true;
					if (handle != null)
						handle.Set ();

					if (cb != null)
						ThreadPool.QueueUserWorkItem (InvokeCallback, this);
				}
			}
		}

		internal HttpListenerContext GetContext ()
		{
			if (forward != null)
				return forward.GetContext ();
			if (exception != null)
				throw exception;

			return context;
		}
		
		public object AsyncState {
			get {
				if (forward != null)
					return forward.AsyncState;
				return state;
			}
		}

		public WaitHandle AsyncWaitHandle {
			get {
				if (forward != null)
					return forward.AsyncWaitHandle;

				lock (locker) {
					if (handle == null)
						handle = new ManualResetEvent (completed);
				}
				
				return handle;
			}
		}

		public bool CompletedSynchronously {
			get {
				if (forward != null)
					return forward.CompletedSynchronously;
				return synch;
			}

		}

		public bool IsCompleted {
			get {
				if (forward != null)
					return forward.IsCompleted;

				lock (locker) {
					return completed;
				}
			}
		}
	}
}
#endif

