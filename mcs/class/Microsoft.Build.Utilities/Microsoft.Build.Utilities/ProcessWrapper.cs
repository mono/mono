// created on 12/18/2004 at 16:28
using System;
using System.Threading;
using System.Diagnostics;

namespace Microsoft.Build.Utilities
{
	internal delegate void ProcessEventHandler(object sender, string message);

	internal class ProcessWrapper : Process, IProcessAsyncOperation
	{
		private Thread captureOutputThread;
		private Thread captureErrorThread;
		ManualResetEvent endEventOut = new ManualResetEvent (false);
		ManualResetEvent endEventErr = new ManualResetEvent (false);
		bool done;
		object lockObj = new object ();

		public ProcessWrapper ()
		{
		}

		public new void Start ()
		{
			CheckDisposed ();
			base.Start ();

			captureOutputThread = new Thread (new ThreadStart(CaptureOutput));
			captureOutputThread.IsBackground = true;
			captureOutputThread.Start ();

			if (ErrorStreamChanged != null) {
				captureErrorThread = new Thread (new ThreadStart(CaptureError));
				captureErrorThread.IsBackground = true;
				captureErrorThread.Start ();
			} else {
				endEventErr.Set ();
			}
		}

		public void WaitForOutput (int milliseconds)
		{
			CheckDisposed ();
			WaitForExit (milliseconds);
			WaitHandle.WaitAll (new WaitHandle[] {endEventOut});
		}

		public void WaitForOutput ()
		{
			WaitForOutput (-1);
		}

		private void CaptureOutput ()
		{
			try {
				if (OutputStreamChanged != null) {
					char[] buffer = new char [1024];
					int nr;
					while ((nr = StandardOutput.Read (buffer, 0, buffer.Length)) > 0) {
						if (OutputStreamChanged != null)
							OutputStreamChanged (this, new string (buffer, 0, nr));
					}
				}
			} catch (ThreadAbortException) {
				// There is no need to keep propagating the abort exception
				Thread.ResetAbort ();
			} finally {
				// WORKAROUND for "Bug 410743 - wapi leak in System.Diagnostic.Process"
				// Process leaks when an exit event is registered
				WaitHandle.WaitAll (new WaitHandle[] {endEventErr});

				OnExited (this, EventArgs.Empty);

				//call this AFTER the exit event, or the ProcessWrapper may get disposed and abort this thread
				if (endEventOut != null)
					endEventOut.Set ();
			}
		}

		private void CaptureError ()
		{
			try {
				char[] buffer = new char [1024];
				int nr;
				while ((nr = StandardError.Read (buffer, 0, buffer.Length)) > 0) {
					if (ErrorStreamChanged != null)
						ErrorStreamChanged (this, new string (buffer, 0, nr));
				}
			} finally {
				endEventErr.Set ();
			}
		}

		protected override void Dispose (bool disposing)
		{
			lock (lockObj) {
				if (endEventOut == null)
					return;
			}

			if (!done)
				((IAsyncOperation)this).Cancel ();

			captureOutputThread = captureErrorThread = null;
			endEventOut.Close ();
			endEventErr.Close ();
			endEventOut = endEventErr = null;

			base.Dispose (disposing);
		}

		void CheckDisposed ()
		{
			if (endEventOut == null)
				throw new ObjectDisposedException ("ProcessWrapper");
		}

		int IProcessAsyncOperation.ExitCode {
			get { return ExitCode; }
		}

		int IProcessAsyncOperation.ProcessId {
			get { return Id; }
		}

		void IAsyncOperation.Cancel ()
		{
			try {
				if (!done) {
					try {
						Kill ();
					} catch {
						// Ignore
					}
					if (captureOutputThread != null)
						captureOutputThread.Abort ();
					if (captureErrorThread != null)
						captureErrorThread.Abort ();
				}
			} catch (Exception ex) {
				//FIXME: Log
				Console.WriteLine (ex.ToString ());
				//LoggingService.LogError (ex.ToString ());
			}
		}

		void IAsyncOperation.WaitForCompleted ()
		{
			WaitForOutput ();
		}

		void OnExited (object sender, EventArgs args)
		{
			try {
				if (!HasExited)
					WaitForExit ();
			} catch {
				// Ignore
			} finally {
				lock (lockObj) {
					done = true;
					try {
						if (completedEvent != null)
							completedEvent (this);
					} catch {
						// Ignore
					}
				}
			}
		}

		event OperationHandler IAsyncOperation.Completed {
			add {
				bool raiseNow = false;
				lock (lockObj) {
					if (done)
						raiseNow = true;
					else
						completedEvent += value;
				}
				if (raiseNow)
					value (this);
			}
			remove {
				lock (lockObj) {
					completedEvent -= value;
				}
			}
		}

		bool IAsyncOperation.Success {
			get { return done ? ExitCode == 0 : false; }
		}

		bool IAsyncOperation.SuccessWithWarnings {
			get { return false; }
		}

		bool IAsyncOperation.IsCompleted {
			get { return done; }
		}

		event OperationHandler completedEvent;

		public event ProcessEventHandler OutputStreamChanged;
		public event ProcessEventHandler ErrorStreamChanged;
	}
}
