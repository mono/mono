// 
// System.Web.Services.Protocols.WebClientAsyncResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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
				Monitor.Wait (this);
			}
		}

		#endregion // Methods
	}
}
