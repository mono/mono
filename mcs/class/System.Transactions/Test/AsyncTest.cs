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
using NUnit.Framework;
using System.Threading;

namespace  MonoTests.System.Transactions {

	[TestFixture]
	// https://bugzilla.novell.com/show_bug.cgi?id=463999
	[Category ("NotWorking")]
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
		[ExpectedException ( typeof ( InvalidOperationException ) )]
		public void AsyncFail1 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			CommittableTransaction ct = new CommittableTransaction ();
			/* Set ambient Tx */
			Transaction.Current = ct;

			/* Enlist */
			irm.Value = 2;

			IAsyncResult ar = ct.BeginCommit ( null, null );
			IAsyncResult ar2 = ct.BeginCommit ( null, null );
		}


		[Test]
		[ExpectedException (typeof (TransactionAbortedException))]
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

			ct.EndCommit ( ar );
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
			mr.WaitOne (new TimeSpan (0, 0, 60), true);

			Assert.IsTrue ( called, "callback not called" );
			Assert.AreEqual ( 5, state, "state not preserved" );

			if ( delayedException.GetType () != typeof ( TransactionAbortedException ) )
				Assert.Fail ( "Expected TransactionAbortedException, got {0}", delayedException.GetType () );
		}

		[Test]
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
			mr.WaitOne (new TimeSpan (0, 2, 0), true);

			Assert.IsTrue (called, "callback not called" );
			Assert.AreEqual ( 5, state, "State not received back");

			if ( delayedException != null )
				throw new Exception ("", delayedException );
		}

		[Test]
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

