//
// Unit tests for async methods of Transaction class
//
// Author:
//	Ankit Jain	<JAnkit@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.Transactions;
#if USE_MSUNITTEST
#if WINDOWS_PHONE || NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
#else // !WINDOWS_PHONE && !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute;
#endif // WINDOWS_PHONE || NETFX_CORE
#else // !USE_MSUNITTEST
using NUnit.Framework;
#endif // USE_MSUNITTEST
using System.Threading;

namespace  MonoTests.System.Transactions {


	// Not all tests working, see:
	// https://bugzilla.novell.com/show_bug.cgi?id=463999
	//
	// All tests marked with NotWorkingOnWindowsPhone fail on WP8
	// as a result of delegate.BeginInvoke not being supported.
	[TestFixture]
	public class AsyncTest {

		[SetUp]
		public void Setup ()
		{
			delayedException = null;
			called = false;
			mr.Reset ();
			state = 0;
			Transaction.Current = null;
		}

		[TearDown]
		public void TearDown ()
		{
			Transaction.Current = null;
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotWorkingOnWindowsPhone")]
		public void AsyncFail1 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();
			/* Set ambient Tx */
			Transaction.Current = ct;

			/* Enlist */
			irm.Value = 2;

			IAsyncResult ar = ct.BeginCommit ( null, null );
			try {
				IAsyncResult ar2 = ct.BeginCommit(null, null);
				Assert.Fail("Expected an exception of type InvalidOperationException");
			} catch (InvalidOperationException) {
			}
		}


		[Test]
		[Category ("NotWorking")]
		[Category ("NotWorkingOnWindowsPhone")]
		public void AsyncFail2 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();
			/* Set ambient Tx */
			Transaction.Current = ct;

			/* Enlist */
			irm.Value = 2;
			irm.FailPrepare = true;

			IAsyncResult ar = ct.BeginCommit ( null, null );

			try {
				ct.EndCommit(ar);
				Assert.Fail("Expected an exception of type TransactionAbortedException");
			} catch (TransactionAbortedException) {
			}
		}

		AsyncCallback callback = null;
		static int state = 0;
		/* Callback called ? */
		static bool called = false;
		static ManualResetEvent mr = new ManualResetEvent ( false );
		static Exception delayedException;

		static void CommitCallback (IAsyncResult ar)
		{
			called = true;
			CommittableTransaction ct = ar as CommittableTransaction;
			try {
				state = ( int ) ar.AsyncState;
				ct.EndCommit ( ar );
			} catch ( Exception e ) {
				delayedException = e;
			} finally {
				mr.Set ();
			}
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotWorkingOnWindowsPhone")]
		public void AsyncFail3 ()
		{
			delayedException = null;
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();
			/* Set ambient Tx */
			Transaction.Current = ct;
			
			/* Enlist */
			irm.Value = 2;
			irm.FailPrepare = true;

			callback = new AsyncCallback (CommitCallback);
			IAsyncResult ar = ct.BeginCommit ( callback, 5 );
#if WINDOWS_PHONE || NETFX_CORE
			mr.WaitOne (new TimeSpan (0, 0, 60));
#else
			mr.WaitOne (new TimeSpan (0, 0, 60), true);
#endif

			Assert.IsTrue ( called, "callback not called" );
			Assert.AreEqual ( 5, state, "state not preserved" );

			if ( delayedException.GetType () != typeof ( TransactionAbortedException ) )
				Assert.Fail ( "Expected TransactionAbortedException, got {0}", delayedException.GetType () );
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotWorkingOnWindowsPhone")]
		public void Async1 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();
			/* Set ambient Tx */
			Transaction.Current = ct;
			/* Enlist */
			irm.Value = 2;

			callback = new AsyncCallback (CommitCallback);
			IAsyncResult ar = ct.BeginCommit ( callback, 5);
#if WINDOWS_PHONE || NETFX_CORE
			mr.WaitOne(new TimeSpan(0, 2, 0));
#else
			mr.WaitOne (new TimeSpan (0, 2, 0), true);
#endif

			Assert.IsTrue (called, "callback not called" );
			Assert.AreEqual ( 5, state, "State not received back");

			if ( delayedException != null )
				throw new Exception ("", delayedException );
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotWorkingOnWindowsPhone")]
		public void Async2 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();

			using ( TransactionScope scope = new TransactionScope (ct) ) {
				irm.Value = 2;

				//scope.Complete ();

				IAsyncResult ar = ct.BeginCommit ( null, null);
				try {
					ct.EndCommit ( ar );
				}
				catch ( TransactionAbortedException) {
					irm.Check ( 0, 0, 1, 0, "irm" );
					return;
				}
			}
			Assert.Fail ( "EndCommit should've thrown an exception" );
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotWorkingOnWindowsPhone")]
		public void Async3 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();
			/* Set ambient Tx */
			Transaction.Current = ct;

			/* Enlist */
			irm.Value = 2;

			IAsyncResult ar = ct.BeginCommit ( null, null );
			ct.EndCommit ( ar );

			irm.Check ( 1, 1, 0, 0, "irm" );
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotWorkingOnWindowsPhone")]
		public void Async4 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();
			/* Set ambient Tx */
			Transaction.Current = ct;

			/* Enlist */
			irm.Value = 2;

			IAsyncResult ar = ct.BeginCommit ( null, null );
			ar.AsyncWaitHandle.WaitOne ();
			Assert.IsTrue ( ar.IsCompleted );

			irm.Check ( 1, 1, 0, 0, "irm" );
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotWorkingOnWindowsPhone")]
		public void Async5 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();
			/* Set ambient Tx */
			Transaction.Current = ct;

			/* Enlist */
			irm.Value = 2;
			irm.FailPrepare = true;

			IAsyncResult ar = ct.BeginCommit ( null, null );
			ar.AsyncWaitHandle.WaitOne ();
			Assert.IsTrue ( ar.IsCompleted );
			try {
				CommittableTransaction ctx = ar as CommittableTransaction;
				ctx.EndCommit ( ar );
			} catch ( TransactionAbortedException ) {
				irm.Check ( 1, 0, 0, 0, "irm" );
				return;
			}

			Assert.Fail ("EndCommit should've failed");
		}
	}
}

