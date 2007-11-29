// 
// System.Web.Services.Protocols.WebClientAsyncResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Net;
using System.Threading;

namespace System.Web.Services.Protocols {
	public class WebClientAsyncResult : IAsyncResult {

		#region Fields

		AsyncCallback _callback;
		object _asyncState;

		bool _completedSynchronously;
		bool _done;
		ManualResetEvent _waitHandle;
		
		internal object Result;
		internal Exception Exception;
		internal WebRequest Request;
			
		#endregion // Fields

		#region Constructors 

		internal WebClientAsyncResult (WebRequest request, AsyncCallback callback, object asyncState)
		{
			_callback = callback; 
			Request = request;
			_asyncState = asyncState;
		}

		#endregion // Constructors

		#region Properties

		public object AsyncState {
			get { return _asyncState; }
		}

		public WaitHandle AsyncWaitHandle 
		{
			get
			{
				lock (this) {
					if (_waitHandle != null) return _waitHandle;
					_waitHandle = new ManualResetEvent (_done);
					return _waitHandle;
				}
			}
		}

		public bool CompletedSynchronously 
		{
			get { return _completedSynchronously; }
		}

		public bool IsCompleted 
		{
			get { lock (this) { return _done; } }
		}

		#endregion // Properties

		#region Methods

		public void Abort ()
		{
			Request.Abort ();
		}

		internal void SetCompleted (object result, Exception exception, bool async)
		{
			lock (this)
			{
				Exception = exception;
				Result = result;
				_done = true;
				_completedSynchronously = async;
				if (_waitHandle != null) _waitHandle.Set ();
				Monitor.PulseAll (this);
			}
			if (_callback != null) _callback (this);
		}

		internal void WaitForComplete ()
		{
			lock (this)
			{
				if (_done)
					return;
				Monitor.Wait (this);
			}
		}

		#endregion // Methods
	}
}
