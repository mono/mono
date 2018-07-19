// created on 12/18/2004 at 16:28
using System;
using System.Threading;
using System.Diagnostics;

namespace Microsoft.Build.Utilities
{
	internal delegate void ProcessEventHandler(object sender, string message);

	internal class ProcessWrapper : Process, IProcessAsyncOperation
	{
		ManualResetEvent endEventOut = new ManualResetEvent (false);
		ManualResetEvent endEventErr = new ManualResetEvent (false);
		ManualResetEvent endEventExit = new ManualResetEvent (false);
		bool done;
		bool disposed;
		object lockObj = new object ();

		public ProcessWrapper ()
		{
		}

		public new void Start ()
		{
			CheckDisposed ();

			base.EnableRaisingEvents = true;

			base.Exited += (s, args) => {
				try {
					endEventExit.Set ();
					WaitHandle.WaitAll (new WaitHandle[] { endEventOut, endEventErr });
				} catch (ObjectDisposedException) {
					return; // we already called Dispose
				}

				OnExited (this, EventArgs.Empty);
			};

			base.OutputDataReceived += (s, args) => {
				if (args.Data == null) {
					try {
						endEventOut.Set ();
					} catch (ObjectDisposedException) {
						return; // we already called Dispose
					}
				} else {
					ProcessEventHandler handler = OutputStreamChanged;
					if (handler != null)
						handler (this, args.Data + Environment.NewLine);
				}
			};

			base.ErrorDataReceived += (s, args) => {
				if (args.Data == null) {
					try {
						endEventErr.Set ();
					} catch (ObjectDisposedException) {
						return; // we already called Dispose
					}
				} else {
					ProcessEventHandler handler = ErrorStreamChanged;
					if (handler != null)
						handler (this, args.Data + Environment.NewLine);
				}
			};

			base.Start ();

			base.BeginOutputReadLine ();
			base.BeginErrorReadLine ();
		}

		public void WaitForOutput (int milliseconds)
		{
			CheckDisposed ();
			WaitForExit (milliseconds);
			WaitHandle.WaitAll (new WaitHandle[] { endEventOut, endEventErr, endEventExit }, milliseconds);
		}

		public void WaitForOutput ()
		{
			WaitForOutput (-1);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposed)
				return;

			if (!done)
				((IAsyncOperation)this).Cancel ();

			// if we race with base.Exited, we don't want to hang on WaitAll (endEventOut, endEventErr)
			endEventOut.Set ();
			endEventErr.Set ();

			endEventOut.Close ();
			endEventErr.Close ();
			endEventExit.Close ();

			disposed = true;

			base.Dispose (disposing);
		}

		void CheckDisposed ()
		{
			if (disposed)
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
					try {
						base.CancelOutputRead ();
					} catch (InvalidOperationException) {
						// Ignore: might happen if Start wasn't called
					}
					try {
						base.CancelErrorRead ();
					} catch (InvalidOperationException) {
						// Ignore: might happen if Start wasn't called
					}
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
			lock (lockObj) {
				done = true;
				try {
					OperationHandler handler = completedEvent;
					if (handler != null)
						handler (this);
				} catch {
					// Ignore
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
