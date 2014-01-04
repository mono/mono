//
// HttpTaskAsyncHandler.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
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
using System;
using System.ComponentModel;
using System.Runtime;
using System.Threading.Tasks;

namespace System.Web {
	public abstract class HttpTaskAsyncHandler : IHttpAsyncHandler, IHttpHandler {
		public virtual bool IsReusable
		{
			get
			{
				return false;
			}
		}

		protected HttpTaskAsyncHandler ()
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ProcessRequest (HttpContext context)
		{
			throw
				new NotSupportedException (
					String.Format ("This type can't execute synchronously: {0}", this.GetType ()));
		}

		public abstract Task ProcessRequestAsync (HttpContext context);
		static Task<TResult> ToApm<TResult> (Task<TResult> task, AsyncCallback callback, object state)
		{
			var tcs = new TaskCompletionSource<TResult> (state);

			task.ContinueWith (delegate
			{
				if (task.IsFaulted) tcs.TrySetException (task.Exception.InnerExceptions);
				else if (task.IsCanceled) tcs.TrySetCanceled ();
				else tcs.TrySetResult (task.Result);
			}, System.Threading.CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

			return tcs.Task;
		}

		IAsyncResult IHttpAsyncHandler.BeginProcessRequest (HttpContext context, AsyncCallback cb, object extraData)
		{
			return (IAsyncResult) ToApm<int> (this.ProcessRequestAsync (context).ContinueWith<int> ((t) => 0), cb, extraData);
		}

		void IHttpAsyncHandler.EndProcessRequest (IAsyncResult result)
		{
			int r = ((Task<int>) result).Result;
			return;
		}
	}
}
