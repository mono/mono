//
// BackgroundWorkerTest.cs
//
// Author:
// 	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//

#if NET_2_0

using System;
using System.Threading;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class BackgroundWorkerTest
	{
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReportProgressNoReportingSupported ()
		{
			BackgroundWorker b = new BackgroundWorker ();
			Assert.IsFalse (b.IsBusy, "#1");
			b.ReportProgress (0);
		}

		[Test]
		public void ReportProgressNonBusy ()
		{
			BackgroundWorker b = new BackgroundWorker ();
			b.WorkerReportsProgress = true;
			Assert.IsFalse (b.IsBusy, "#1");
			b.ReportProgress (0);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CancelAsyncNoCancellationSupported ()
		{
			BackgroundWorker b = new BackgroundWorker ();
			Assert.IsFalse (b.IsBusy, "#1");
			b.CancelAsync ();
		}

		[Test]
		public void CancelAsyncNonBusy ()
		{
			BackgroundWorker b = new BackgroundWorker ();
			b.WorkerSupportsCancellation = true;
			Assert.IsFalse (b.IsBusy, "#1");
			b.CancelAsync ();
		}
		
		ManualResetEvent m = new ManualResetEvent (false);
		bool runworkercalled = false;
		
		[Test]
		public void CancelBackgroundWorker ()
		{
			BackgroundWorker bw = new BackgroundWorker ();
			
			bw.WorkerSupportsCancellation = true;
			bw.DoWork += new DoWorkEventHandler (bw_DoWork);
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler (bw_RunWorkerCompleted);
			
			runworkercalled = false;
			bw.RunWorkerAsync ("cancel");
			
			// We don't want to hang forever if the test fails.
			Assert.AreEqual (true, m.WaitOne (10000, false), "A");
			
			Assert.AreEqual (true, runworkercalled, "B");
			Assert.AreEqual (false, bw.IsBusy, "C");
		}

		void bw_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			runworkercalled = true;
			Assert.AreEqual (true, e.Cancelled, "A1");

			try {
				object o = e.Result;
				Assert.Fail ("There should be an IOE for cancelling the operation");
			}
			catch (InvalidOperationException)
			{ }
			
			Assert.IsNull (e.Error, "A3");
			
			m.Set ();
		}

		ManualResetEvent m2 = new ManualResetEvent (false);
		bool runworkercalled2 = false;

		[Test]
		public void ExceptionBackgroundWorker ()
		{
			BackgroundWorker bw = new BackgroundWorker ();

			bw.WorkerSupportsCancellation = true;
			bw.DoWork += new DoWorkEventHandler (bw_DoWork);
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler (bw_RunWorkerCompletedException);

			runworkercalled2 = false;
			bw.RunWorkerAsync ("exception");

			// We don't want to hang forever if the test fails.
			Assert.AreEqual (true, m2.WaitOne (10000, false), "A");

			Assert.AreEqual (true, runworkercalled2, "B");
			Assert.AreEqual (false, bw.IsBusy, "C");
		}

		void bw_RunWorkerCompletedException (object sender, RunWorkerCompletedEventArgs e)
		{
			runworkercalled2 = true;
			Assert.AreEqual (false, e.Cancelled, "A1");

			try
			{
				object o = e.Result;
				Assert.Fail ("There should be an TargetInvocationException");
			}
			catch (TargetInvocationException)
			{ }

			Assert.IsNotNull (e.Error, "A3");

			m2.Set ();
		}

		ManualResetEvent m3 = new ManualResetEvent (false);
		bool runworkercalled3 = false;

		[Test]
		public void CompleteBackgroundWorker ()
		{
			BackgroundWorker bw = new BackgroundWorker ();

			bw.WorkerSupportsCancellation = true;
			bw.DoWork += new DoWorkEventHandler (bw_DoWork);
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler (bw_RunWorkerCompletedSuccess);

			runworkercalled3 = false;
			bw.RunWorkerAsync ();

			// We don't want to hang forever if the test fails.
			Assert.AreEqual (true, m3.WaitOne (10000, false), "A");

			Assert.AreEqual (true, runworkercalled3, "B");
			Assert.AreEqual (false, bw.IsBusy, "C");
		}

		void bw_RunWorkerCompletedSuccess (object sender, RunWorkerCompletedEventArgs e)
		{
			runworkercalled3 = true;
			Assert.AreEqual (false, e.Cancelled, "A1");

			Assert.AreEqual ("B", e.Result, "A2");
			Assert.IsNull (e.Error, "A3");

			m3.Set ();
		}

		void bw_DoWork (object sender, DoWorkEventArgs e)
		{
		
			if ((string)e.Argument == "cancel") {
				e.Cancel = true;
				e.Result = "A";
			} else if ((string)e.Argument == "exception") {
				throw new ApplicationException ("Whoops!");
			} else
				e.Result = "B";
		}
	}
}

#endif
