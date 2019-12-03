//
// BackgroundWorkerTest.cs
//
// Author:
// 	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//

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
		ManualResetEvent m;
		bool runworkercalled;
		SynchronizationContext old_context;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			old_context = AsyncOperationManager.SynchronizationContext;
			AsyncOperationManager.SynchronizationContext = new SynchronizationContext ();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown ()
		{
			AsyncOperationManager.SynchronizationContext = old_context;
		}

		[SetUp]
		public void Setup ()
		{
			m = new ManualResetEvent (false);
			runworkercalled = false;
		}

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

		[Test]
#if WASM
		[NUnit.Framework.CategoryAttribute ("MultiThreaded")]
#endif
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

		[Test]
#if WASM
		[NUnit.Framework.CategoryAttribute ("MultiThreaded")]
#endif
		public void ExceptionBackgroundWorker ()
		{
			BackgroundWorker bw = new BackgroundWorker ();

			bw.WorkerSupportsCancellation = true;
			bw.DoWork += new DoWorkEventHandler (bw_DoWork);
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler (bw_RunWorkerCompletedException);

			runworkercalled = false;
			bw.RunWorkerAsync ("exception");

			// We don't want to hang forever if the test fails.
			Assert.AreEqual (true, m.WaitOne (10000, false), "A");

			Assert.AreEqual (true, runworkercalled, "B");
			Assert.AreEqual (false, bw.IsBusy, "C");
		}

		void bw_RunWorkerCompletedException (object sender, RunWorkerCompletedEventArgs e)
		{
			runworkercalled = true;
			Assert.AreEqual (false, e.Cancelled, "A1");

			try
			{
				object o = e.Result;
				Assert.Fail ("There should be an TargetInvocationException");
			}
			catch (TargetInvocationException)
			{ }

			Assert.IsNotNull (e.Error, "A3");

			m.Set ();
		}

		[Test]
#if WASM
		[NUnit.Framework.CategoryAttribute ("MultiThreaded")]
#endif
		public void CompleteBackgroundWorker ()
		{
			BackgroundWorker bw = new BackgroundWorker ();

			bw.WorkerSupportsCancellation = true;
			bw.DoWork += new DoWorkEventHandler (bw_DoWork);
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler (bw_RunWorkerCompletedSuccess);

			runworkercalled = false;
			bw.RunWorkerAsync ();

			// We don't want to hang forever if the test fails.
			Assert.AreEqual (true, m.WaitOne (10000, false), "A");

			Assert.AreEqual (true, runworkercalled, "B");
			Assert.AreEqual (false, bw.IsBusy, "C");
		}

		void bw_RunWorkerCompletedSuccess (object sender, RunWorkerCompletedEventArgs e)
		{
			runworkercalled = true;
			Assert.AreEqual (false, e.Cancelled, "A1");

			Assert.AreEqual ("B", e.Result, "A2");
			Assert.IsNull (e.Error, "A3");

			m.Set ();
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

