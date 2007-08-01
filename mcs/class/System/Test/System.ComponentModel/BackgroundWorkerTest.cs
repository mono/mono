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
	}
}

#endif
