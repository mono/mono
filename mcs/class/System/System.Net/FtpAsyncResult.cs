//
// System.Net.FtpAsyncResult.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.IO;
using System.Threading;
using System.Net;

namespace System.Net 
{
	class FtpAsyncResult : IAsyncResult
	{
		FtpWebResponse response;
		ManualResetEvent waitHandle;
		Exception exception;
		AsyncCallback callback;
		Stream stream;
		object state;
		bool completed;
		bool synch;
		object locker = new object ();

		public FtpAsyncResult (AsyncCallback callback, object state)
		{
			this.callback = callback;
			this.state = state;
		}
		
		public object AsyncState {
			get {
				return state;
			}
		}

		public WaitHandle AsyncWaitHandle {
			get {
				lock (locker) {
					if (waitHandle == null)
						waitHandle = new ManualResetEvent (false);
				}
				
				return waitHandle;
			}
		}

		public bool CompletedSynchronously {
			get {
				return synch;
			}
		}

		public bool IsCompleted {
			get {
				lock (locker) {
					return completed;
				}
			}
		}

		internal bool GotException {
			get {
				return exception != null;
			}
		}

		internal Exception Exception {
			get {
				return exception;
			}
		}

		internal FtpWebResponse Response {
			get {
				return response;
			}
			set {
				response = value;
			}
		}

		internal Stream Stream {
			get {
				return stream;
			}

			set { stream = value; }
		}

		internal void WaitUntilComplete ()
		{
			if (IsCompleted)
				return;

			AsyncWaitHandle.WaitOne ();
		}

		internal bool WaitUntilComplete (int timeout, bool exitContext)
		{
			if (IsCompleted)
				return true;
			
			return AsyncWaitHandle.WaitOne (timeout, exitContext);
		}

		internal void SetCompleted (bool synch, Exception exc, FtpWebResponse response)
		{
			this.synch = synch;
			this.exception = exc;
			this.response = response;
			lock (locker) {
				completed = true;
				if (waitHandle != null)
					waitHandle.Set ();
			}
			DoCallback ();
		}

		internal void SetCompleted (bool synch, FtpWebResponse response)
		{
			SetCompleted (synch, null, response);
		}

		internal void SetCompleted (bool synch, Exception exc)
		{
			SetCompleted (synch, exc, null);
		}

		internal void DoCallback ()
		{
			if (callback != null)
				try {
					callback (this);
				}
				catch (Exception) {
				}
		}

		// Cleanup resources
		internal void Reset () 
		{
			exception = null;
			synch = false;
			response = null;
			state = null;
			
			lock (locker) {
				completed = false;
				if (waitHandle != null)
					waitHandle.Reset ();
			}
		}
		
	}
}


