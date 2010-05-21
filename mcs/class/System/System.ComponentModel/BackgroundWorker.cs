//
// BackgroundWorker.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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
#if NET_2_0
using System.Collections.Generic;
using System.Threading;

namespace System.ComponentModel
{
#if !NET_2_1
	[DefaultEvent ("DoWork")]
	public class BackgroundWorker : Component
#else
	public class BackgroundWorker
#endif
	{
		public BackgroundWorker ()
		{
		}

		AsyncOperation async;
		bool cancel_pending, report_progress = false, support_cancel = false, complete = false;

		public event DoWorkEventHandler DoWork;
		public event ProgressChangedEventHandler ProgressChanged;
		public event RunWorkerCompletedEventHandler RunWorkerCompleted;

#if !NET_2_1
		[Browsable (false)]
#endif
		public bool CancellationPending {
			get { return cancel_pending; }
		}

#if !NET_2_1
		[Browsable (false)]
#endif
		public bool IsBusy {
			get { return async != null; }
		}

		[DefaultValue (false)]
		public bool WorkerReportsProgress {
			get { return report_progress; }
			set { report_progress = value; }
		}

		[DefaultValue (false)]
		public bool WorkerSupportsCancellation {
			get { return support_cancel; }
			set { support_cancel = value; }
		}

		public void CancelAsync ()
		{
			if (!support_cancel)
				throw new InvalidOperationException ("This background worker does not support cancellation.");

			if (!IsBusy)
				return;

			cancel_pending = true;
		}

		public void ReportProgress (int percentProgress)
		{
			ReportProgress (percentProgress, null);
		}

		public void ReportProgress (int percentProgress, object userState)
		{
			if (!WorkerReportsProgress)
				throw new InvalidOperationException ("This background worker does not report progress.");

			if (complete)
				throw new InvalidOperationException ("The background worker has ended.");

			ProgressChangedEventArgs pcea = new ProgressChangedEventArgs (percentProgress, userState);
			if (async == null) {
				// we can report progress before a call to RunWorkerAsync - but we do it sync
				OnProgressChanged (pcea);
			} else {
				async.Post (delegate (object o) {
					ProgressChangedEventArgs e = o as ProgressChangedEventArgs;
					OnProgressChanged (e);
				}, pcea);
			}
		}

		public void RunWorkerAsync ()
		{
			RunWorkerAsync (null);
		}

		delegate void ProcessWorkerEventHandler (object argument, AsyncOperation async, SendOrPostCallback callback);

		void ProcessWorker (object argument, AsyncOperation async, SendOrPostCallback callback)
		{
			// do worker
			Exception error = null;
			DoWorkEventArgs e = new DoWorkEventArgs (argument);
			try {
				OnDoWork (e);
			} catch (Exception ex) {
				error = ex;
				e.Cancel = false;
			}
			callback (new object [] {
				new RunWorkerCompletedEventArgs (
					e.Result, error, e.Cancel),
				async});
		}

		void CompleteWorker (object state)
		{
			object [] args = (object []) state;
			RunWorkerCompletedEventArgs e =
				args [0] as RunWorkerCompletedEventArgs;
			AsyncOperation async = args [1] as AsyncOperation;

			SendOrPostCallback callback = delegate (object darg) {
				this.async = null;
				complete = true;
				OnRunWorkerCompleted (darg as RunWorkerCompletedEventArgs);
			};

			async.PostOperationCompleted (callback, e);

			cancel_pending = false;
		}

		public void RunWorkerAsync (object argument)
		{
			if (IsBusy)
				throw new InvalidOperationException ("The background worker is busy.");

			async = AsyncOperationManager.CreateOperation (this);

			complete = false;
			ProcessWorkerEventHandler handler =
				new ProcessWorkerEventHandler (ProcessWorker);
			handler.BeginInvoke (argument, async, CompleteWorker, null, null);
		}

		protected virtual void OnDoWork (DoWorkEventArgs e)
		{
			if (DoWork != null)
				DoWork (this, e);
		}

		protected virtual void OnProgressChanged (ProgressChangedEventArgs e)
		{
			if (ProgressChanged != null)
				ProgressChanged (this, e);
		}

		protected virtual void OnRunWorkerCompleted (RunWorkerCompletedEventArgs e)
		{
			if (RunWorkerCompleted != null)
				RunWorkerCompleted (this, e);
		}
	}
}
#endif
