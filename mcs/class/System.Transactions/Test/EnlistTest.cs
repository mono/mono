//
// Tests for combination of volatile & durable resource manangers
//
// Author:
//	Ankit Jain	<JAnkit@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

// This define is needed when running this tests under MS.NET as right
// now we always wrap thrown exceptions insde TransactionAbortedException.
// However, MS implementation in some cases, directly relays the exception
// thrown by RM without wrapping it. Not sure if this is a bug or a feature.
//#define MS_EXCEPTIONS_BEHAVIOR

using System;
using System.Transactions;
using NUnit.Framework;

namespace MonoTests.System.Transactions {

	[TestFixture]
	public class EnlistTest {

		#region Vol1_Dur0

		/* Single volatile resource, SPC happens */
		[Test]
		public void Vol1_Dur0 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			irm.UseSingle = true;
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				scope.Complete ();
			}
			irm.CheckSPC ("irm");
		}

		[Test]
		public void Vol1_Dur0_2PC ()
		{
			IntResourceManager irm = new IntResourceManager (1);

			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				scope.Complete ();
			}
			irm.Check2PC ("irm");
		}

		/* Single volatile resource, SPC happens */
		[Test]
		public void Vol1_Dur0_Fail1 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			irm.UseSingle = true;
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				/* Not completing this..
				scope.Complete ();*/
			}

			irm.Check ( 0, 0, 0, 1, 0, 0, 0, "irm" );
		}

		[Test]
		[ExpectedException ( typeof ( TransactionAbortedException ) )]
		public void Vol1_Dur0_Fail2 ()
		{
			IntResourceManager irm = new IntResourceManager (1);

			irm.FailPrepare = true;

			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				scope.Complete ();
			}
		}

		[Test]
		[ExpectedException ( typeof ( TransactionAbortedException ) )]
		public void Vol1_Dur0_Fail3 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			irm.UseSingle = true;
			irm.FailSPC = true;

			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				scope.Complete ();
			}
		}

		#endregion

		#region Vol2_Dur0

		/* >1 volatile, 2PC */
		[Test]
		public void Vol2_Dur0_SPC ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (3);

			irm.UseSingle = true;
			irm2.UseSingle = true;
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;
				irm2.Value = 6;

				scope.Complete ();
			}
			irm.Check2PC ( "irm" );
			irm2.Check2PC ( "irm2" );
		}

		#endregion

		#region Vol0_Dur1
		/* 1 durable */
		[Test]
		public void Vol0_Dur1 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			irm.Type = ResourceManagerType.Durable;
			irm.UseSingle = true;

			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				scope.Complete ();
			}

			irm.CheckSPC ( "irm" );
		}

		/* We support only 1 durable with 2PC
		 * On .net, this becomes a distributed transaction
		 */ 
		[Test]
		[Category ("NotWorking")]
		public void Vol0_Dur1_2PC ()
		{
			IntResourceManager irm = new IntResourceManager (1);

			/* Durable resource enlisted with a IEnlistedNotification
			 * object
			 */
			irm.Type = ResourceManagerType.Durable;

			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				scope.Complete ();
			}
		}

		[Test]
		public void Vol0_Dur1_Fail ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			/* Durable resource enlisted with a IEnlistedNotification
			 * object
			 */
			irm.Type = ResourceManagerType.Durable;
			irm.FailSPC = true;
			irm.UseSingle = true;
			try {
				using (TransactionScope scope = new TransactionScope ()) {
					irm.Value = 2;

					scope.Complete ();
				}
			}
			catch (TransactionAbortedException) {
				irm.Check ( 1, 0, 0, 0, 0, 0, 0, "irm" );
				return;
			}

			Assert.Fail ();
		}
		#endregion

		#region Vol2_Dur1
		/* >1vol + 1 durable */
		[Test]
		public void Vol2_Dur1 ()
		{
			IntResourceManager [] irm = new IntResourceManager [4];
			irm [0] = new IntResourceManager ( 1 );
			irm [1] = new IntResourceManager ( 3 );
			irm [2] = new IntResourceManager ( 5 );
			irm [3] = new IntResourceManager ( 7 );

			irm [0].Type = ResourceManagerType.Durable;
			for ( int i = 0; i < 4; i++ )
				irm [i].UseSingle = true;

			using (TransactionScope scope = new TransactionScope ()) {
				irm [0].Value = 2;
				irm [1].Value = 6;
				irm [2].Value = 10;
				irm [3].Value = 14;

				scope.Complete ();
			}

			irm [0].CheckSPC ( "irm [0]" );

			/* Volatile RMs get 2PC */
			for (int i = 1; i < 4; i++)
				irm [i].Check2PC ( "irm [" + i + "]" );
		}

		/* >1vol + 1 durable
		 * Durable fails SPC
		 */
		[Test]
		public void Vol2_Dur1_Fail1 ()
		{
			IntResourceManager [] irm = new IntResourceManager [4];
			irm [0] = new IntResourceManager (1);
			irm [1] = new IntResourceManager (3);
			irm [2] = new IntResourceManager (5);
			irm [3] = new IntResourceManager (7);

			irm [0].Type = ResourceManagerType.Durable;
			irm [0].FailSPC = true;

			for ( int i = 0; i < 4; i++ )
				irm [i].UseSingle = true;

			/* Durable RM irm[0] does Abort on SPC, so
			 * all volatile RMs get Rollback */
			try {
				using (TransactionScope scope = new TransactionScope ()) {
					irm [0].Value = 2;
					irm [1].Value = 6;
					irm [2].Value = 10;
					irm [3].Value = 14;

					scope.Complete ();
				}
			}
			catch (TransactionAbortedException) {
				irm [0].CheckSPC ( "irm [0]" );
				/* Volatile RMs get 2PC Prepare, and then get rolled back */
				for (int i = 1; i < 4; i++)
					irm [i].Check ( 0, 1, 0, 1, 0, 0, 0, "irm [" + i + "]" );

				return;
			}

			Assert.Fail();
		}

		/* >1vol + 1 durable 
		 * durable doesn't complete SPC
		 */
		[Test]
		[Ignore ( "Correct this test, it should throw TimeOutException or something" )]
		public void Vol2_Dur1_Fail2 ()
		{
			TransactionAbortedException exception = null;
			IntResourceManager [] irm = new IntResourceManager [4];
			irm [0] = new IntResourceManager (1);
			irm [1] = new IntResourceManager (3);
			irm [2] = new IntResourceManager (5);
			irm [3] = new IntResourceManager (7);

			irm [0].Type = ResourceManagerType.Durable;
			irm [0].IgnoreSPC = true;

			for ( int i = 0; i < 4; i++ )
				irm [i].UseSingle = true;

			/* Durable RM irm[2] does on SPC, so
			 * all volatile RMs get Rollback */
			try {
				using (TransactionScope scope = new TransactionScope ( TransactionScopeOption.Required, new TimeSpan ( 0, 0, 5 ) )) {
					irm [0].Value = 2;
					irm [1].Value = 6;
					irm [2].Value = 10;
					irm [3].Value = 14;

					scope.Complete ();
				}
			}
			catch (TransactionAbortedException ex) {
				irm [0].CheckSPC ( "irm [0]" );

				/* Volatile RMs get 2PC Prepare, and then get rolled back */
				for (int i = 1; i < 4; i++)
					irm [i].Check ( 0, 1, 0, 1, 0, 0, 0, "irm [" + i + "]" );

				exception = ex;
			}

			Assert.IsNotNull(exception, "Expected TransactionAbortedException not thrown!");
			Assert.IsNotNull(exception.InnerException, "TransactionAbortedException has no inner exception!");
			Assert.AreEqual(typeof(TimeoutException), exception.InnerException.GetType());
		}

		/* Same as Vol2_Dur1_Fail2, but with a volatile manager timming out */
		[Test]
		[Ignore ( "Correct this test, it should throw TimeOutException or something" )]
		public void Vol2_Dur1_Fail2b()
		{
			TransactionAbortedException exception = null;
			IntResourceManager[] irm = new IntResourceManager[4];
			irm[0] = new IntResourceManager(1);
			irm[1] = new IntResourceManager(3);
			irm[2] = new IntResourceManager(5);
			irm[3] = new IntResourceManager(7);

			irm[0].IgnoreSPC = true;
			irm[1].Type = ResourceManagerType.Durable;

			for (int i = 0; i < 4; i++)
				irm[i].UseSingle = true;

			/* Durable RM irm[2] does on SPC, so
			 * all volatile RMs get Rollback */
			try
			{
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 0, 5)))
				{
					irm[0].Value = 2;
					irm[1].Value = 6;
					irm[2].Value = 10;
					irm[3].Value = 14;

					scope.Complete();
				}
			}
			catch (TransactionAbortedException ex)
			{
				irm[0].CheckSPC("irm [0]");

				/* Volatile RMs get 2PC Prepare, and then get rolled back */
				for (int i = 1; i < 4; i++)
					irm[i].Check(0, 1, 0, 1, 0, 0, 0, "irm [" + i + "]");

				exception = ex;
			}

			Assert.IsNotNull(exception, "Expected TransactionAbortedException not thrown!");
			Assert.IsNotNull(exception.InnerException, "TransactionAbortedException has no inner exception!");
			Assert.AreEqual(typeof(TimeoutException), exception.InnerException.GetType());
		}

		/* >1vol + 1 durable
		 * Volatile fails Prepare
		 */
		[Test]
		public void Vol2_Dur1_Fail3 ()
		{
			IntResourceManager [] irm = new IntResourceManager [4];
			irm [0] = new IntResourceManager ( 1 );
			irm [1] = new IntResourceManager ( 3 );
			irm [2] = new IntResourceManager ( 5 );
			irm [3] = new IntResourceManager ( 7 );

			irm [0].Type = ResourceManagerType.Durable;
			irm [2].FailPrepare = true;

			for ( int i = 0; i < 4; i++ )
				irm [i].UseSingle = true;

			/* Durable RM irm[2] does on SPC, so
			 * all volatile RMs get Rollback */
			try {
				using (TransactionScope scope = new TransactionScope ()) {
					irm [0].Value = 2;
					irm [1].Value = 6;
					irm [2].Value = 10;
					irm [3].Value = 14;

					scope.Complete ();
				}
			}
			catch (TransactionAbortedException) {
				irm [0].Check ( 0, 0, 0, 1, 0, 0, 0, "irm [0]");

				/* irm [1] & [2] get prepare,
				 * [2] -> ForceRollback,
				 * [1] & [3] get rollback,
				 * [0](durable) gets rollback */
				irm [1].Check ( 0, 1, 0, 1, 0, 0, 0, "irm [1]" );
				irm [2].Check ( 0, 1, 0, 0, 0, 0, 0, "irm [2]" );
				irm [3].Check ( 0, 0, 0, 1, 0, 0, 0, "irm [3]" );

				return;
			}

			Assert.Fail ( "Expected TransactionAbortedException" );
		}

		[Test]
		public void Vol2_Dur1_Fail4 ()
		{
			IntResourceManager [] irm = new IntResourceManager [2];
			irm [0] = new IntResourceManager ( 1 );
			irm [1] = new IntResourceManager ( 3 );

			irm [0].Type = ResourceManagerType.Durable;
			irm [0].FailSPC = true;
			irm [0].FailWithException = true;

			for ( int i = 0; i < 2; i++ )
				irm [i].UseSingle = true;

			/* Durable RM irm[2] does on SPC, so
			 * all volatile RMs get Rollback */
			try {
				using ( TransactionScope scope = new TransactionScope () ) {
					irm [0].Value = 2;
					irm [1].Value = 6;

					scope.Complete ();
				}
			}
			catch ( TransactionAbortedException e) {
				Assert.IsNotNull ( e.InnerException, "Expected e.InnerException == NotSupportedException, but got None");
				Assert.AreEqual ( typeof ( NotSupportedException ), e.InnerException.GetType (), "Expected e.InnerException == NotSupportedException, but got " + e.GetType () );

				irm [0].Check ( 1, 0, 0, 0, 0, 0, 0, "irm [0]" );
				irm [1].Check ( 0, 1, 0, 1, 0, 0, 0, "irm [1]" );
				return;
			}

			Assert.Fail ( "Expected TransactionAbortedException" );
		}

		[Test]
		public void Vol2_Dur1_Fail5 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();
			IntResourceManager [] irm = new IntResourceManager [2];
			irm [0] = new IntResourceManager ( 1 );
			irm [1] = new IntResourceManager ( 3 );

			Transaction.Current = ct;
			irm [0].Type = ResourceManagerType.Durable;
			irm [0].FailSPC = true;
			irm [0].FailWithException = true;

			for ( int i = 0; i < 2; i++ )
				irm [i].UseSingle = true;

			/* Durable RM irm[2] does on SPC, so
			 * all volatile RMs get Rollback */
			
			using ( TransactionScope scope = new TransactionScope () ) {
				irm [0].Value = 2;
				irm [1].Value = 6;

				scope.Complete ();
			}

			try {
				ct.Commit ();
			}
			catch ( TransactionAbortedException e ) {
				Assert.IsNotNull ( e.InnerException, "Expected e.InnerException == NotSupportedException, but got None" );
				Assert.AreEqual ( typeof ( NotSupportedException ), e.InnerException.GetType (), "Expected e.InnerException == NotSupportedException, but got " + e.GetType () );

				irm [0].Check ( 1, 0, 0, 0, 0, 0, 0, "irm [0]" );
				irm [1].Check ( 0, 1, 0, 1, 0, 0, 0, "irm [1]" );
				try {
					ct.Commit ();
				}
				catch (InvalidOperationException x ) {
					Assert.IsNull ( x.InnerException);
					Transaction.Current = null;
					return;
				}
				Assert.Fail ( "Should not be reached" );
			}

			Assert.Fail ( "Expected TransactionAbortedException" );
		}

		#endregion

		#region Promotable Single Phase Enlistment
		[Test]
		public void Vol0_Dur0_Pspe1 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			irm.Type = ResourceManagerType.Promotable;
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				scope.Complete ();
			}
			irm.Check ( 1, 0, 0, 0, 0, 1, 0, "irm" );
		}

		[Test]
		public void Vol1_Dur0_Pspe1 ()
		{
			IntResourceManager irm0 = new IntResourceManager (1);
			IntResourceManager irm1 = new IntResourceManager (1);
			irm1.Type = ResourceManagerType.Promotable;
			using (TransactionScope scope = new TransactionScope ()) {
				irm0.Value = 2;
				irm1.Value = 8;

				scope.Complete ();
			}
			irm1.Check ( 1, 0, 0, 0, 0, 1, 0, "irm1" );
		}

		[Test]
		public void Vol0_Dur1_Pspe1 ()
		{
			IntResourceManager irm0 = new IntResourceManager (1);
			IntResourceManager irm1 = new IntResourceManager (1);
			irm0.Type = ResourceManagerType.Durable;
			irm0.UseSingle = true;
			irm1.Type = ResourceManagerType.Promotable;
			using (TransactionScope scope = new TransactionScope ()) {
				irm0.Value = 8;
				irm1.Value = 2;
				Assert.AreEqual(0, irm1.NumEnlistFailed, "PSPE enlist did not fail although durable RM was already enlisted");
			}
		}

		[Test]
		public void Vol0_Dur0_Pspe2 ()
		{
			IntResourceManager irm0 = new IntResourceManager (1);
			IntResourceManager irm1 = new IntResourceManager (1);
			irm0.Type = ResourceManagerType.Promotable;
			irm1.Type = ResourceManagerType.Promotable;
			using (TransactionScope scope = new TransactionScope ()) {
				irm0.Value = 8;
				irm1.Value = 2;
				Assert.AreEqual(0, irm1.NumEnlistFailed, "PSPE enlist did not fail although PSPE RM was already enlisted");
			}
		}
		#endregion

		#region Others
		/* >1vol  
		 * > 1 durable, On .net this becomes a distributed transaction
		 * We don't support this in mono yet. 
		 */
		[Test]
		[Category ("NotWorking")]
		public void Vol0_Dur2 ()
		{
			IntResourceManager [] irm = new IntResourceManager [2];
			irm [0] = new IntResourceManager ( 1 );
			irm [1] = new IntResourceManager ( 3 );

			irm [0].Type = ResourceManagerType.Durable;
			irm [1].Type = ResourceManagerType.Durable;

			for ( int i = 0; i < 2; i++ )
				irm [i].UseSingle = true;

			using (TransactionScope scope = new TransactionScope ()) {
				irm [0].Value = 2;
				irm [1].Value = 6;

				scope.Complete ();
			}
		}

		[Test]
		public void TransactionDispose ()
		{
			CommittableTransaction ct = new CommittableTransaction ();
			IntResourceManager irm = new IntResourceManager (1);
			irm.Type = ResourceManagerType.Durable;

			ct.Dispose ();
			irm.Check  (0, 0, 0, 0, "Dispose transaction");
		}

		[Test]
		public void TransactionDispose2 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();
			IntResourceManager irm = new IntResourceManager (1);

			Transaction.Current = ct;
			irm.Value = 5;

			try {
				ct.Dispose ();
			} finally {
				Transaction.Current = null;
			}

			irm.Check (0, 0, 1, 0, "Dispose transaction");
			Assert.AreEqual (1, irm.Value);
		}

		[Test]
		public void TransactionDispose3 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();
			IntResourceManager irm = new IntResourceManager (1);

			try {
				Transaction.Current = ct;
				irm.Value = 5;
				ct.Commit ();
				ct.Dispose ();
			} finally {
				Transaction.Current = null;
			}

			irm.Check (1, 1, 0, 0, "Dispose transaction");
			Assert.AreEqual (5, irm.Value);
		}
		#endregion

		#region TransactionCompleted
		[Test]
		public void TransactionCompleted_Committed ()
		{
			bool called = false;
			using (var ts = new TransactionScope ())
			{
				var tr = Transaction.Current;
				tr.TransactionCompleted += (s, e) => called = true;
				ts.Complete ();
			}

			Assert.IsTrue (called, "TransactionCompleted event handler not called!");
		}

		[Test]
		public void TransactionCompleted_Rollback ()
		{
			bool called = false;
			using (var ts = new TransactionScope ())
			{
				var tr = Transaction.Current;
				tr.TransactionCompleted += (s, e) => called = true;
				// Not calling ts.Complete() on purpose..
			}

			Assert.IsTrue (called, "TransactionCompleted event handler not called!");
		}
		#endregion

		#region Success/Failure behavior tests
		#region Success/Failure behavior Vol1_Dur0 Cases
		[Test]
		public void Vol1SPC_Committed()
		{
			bool called = false;
			TransactionStatus status = TransactionStatus.Active;
			var rm = new IntResourceManager(1)
			{
				UseSingle = true,
				Type = ResourceManagerType.Volatile
			};

			using (var ts = new TransactionScope())
			{
				rm.Value = 2;
				var tr = Transaction.Current;
				tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
				ts.Complete();
			}

			rm.Check(1, 0, 0, 0, 0, 0, 0, "rm");
			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.AreEqual(TransactionStatus.Committed, status, "TransactionStatus != Commited");
		}

		[Test]
		public void Vol1_Committed()
		{
			bool called = false;
			TransactionStatus status = TransactionStatus.Active;
			var rm = new IntResourceManager(1)
			{
				Type = ResourceManagerType.Volatile,
			};

			using (var ts = new TransactionScope())
			{
				rm.Value = 2;
				var tr = Transaction.Current;
				tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
				ts.Complete();
			}

			rm.Check(0, 1, 1, 0, 0, 0, 0, "rm");
			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.AreEqual(TransactionStatus.Committed, status, "TransactionStatus != Commited");
		}

		[Test]
		public void Vol1_Rollback()
		{
			bool called = false;
			TransactionStatus status = TransactionStatus.Active;
			var rm = new IntResourceManager(1)
			{
				Type = ResourceManagerType.Volatile,
			};

			using (var ts = new TransactionScope())
			{
				rm.Value = 2;
				var tr = Transaction.Current;
				tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
				// Not calling ts.Complete() on purpose..
			}

			rm.Check(0, 0, 0, 1, 0, 0, 0, "rm");

			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.AreEqual(TransactionStatus.Aborted, status, "TransactionStatus != Aborted");
		}

		[Test]
		public void Vol1SPC_Throwing_On_Commit()
		{
			bool called = false;
			Exception ex = null;
			TransactionStatus status = TransactionStatus.Active;
			var rm = new IntResourceManager(1)
			{
				UseSingle = true,
				FailSPC = true,
				FailWithException = true,
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm.Value = 2;
					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm.Check(1, 0, 0, 0, 0, 0, 0, "rm");

			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.AreEqual(TransactionStatus.Aborted, status, "TransactionStatus != Aborted");
			Assert.IsNotNull(ex, "Exception not thrown");
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException is null");
			Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException, "Invalid inner exception thrown");
		}

		[Test]
		public void Vol1_Throwing_On_Commit()
		{
			bool called = false;
			TransactionStatus status = TransactionStatus.Active;
			Exception ex = null;
			var rm = new IntResourceManager(1)
			{
				FailCommit = true,
				FailWithException = true,
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm.Value = 2;
					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm.Check(0, 1, 1, 0, 0, 0, 0, "rm");

			// MS.NET wont call TransactionCompleted event in this particular case.
			Assert.IsFalse(called, "TransactionCompleted event handler _was_ called!?!?!");
			Assert.IsNotNull(ex, "Exception not thrown");
#if MS_EXCEPTIONS_BEHAVIOR
			// MS.NET will relay the exception thrown by RM instead of wrapping it on a TransactionAbortedException.
			Assert.IsInstanceOfType(typeof(NotSupportedException), ex, "Invalid exception thrown");
#else
			// Mono wrapps the exception into a TransactionAbortedException.
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid type of exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException not thrown");
			Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException, "Invalid type of inner exception thrown");
#endif
		}

		[Test]
		public void Vol1_Throwing_On_Rollback()
		{
			bool called = false;
			TransactionStatus status = TransactionStatus.Active;
			Exception ex = null;
			var rm = new IntResourceManager(1)
			{
				FailRollback = true,
				FailWithException = true,
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm.Value = 2;
					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					// Not calling ts.Complete() on purpose..
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm.Check(0, 0, 0, 1, 0, 0, 0, "rm");

			// MS.NET wont call TransactionCompleted event in this particular case.
			Assert.IsFalse(called, "TransactionCompleted event handler _was_ called!?!?!");
			Assert.IsNotNull(ex, "Exception not thrown");
			// MS.NET will relay the exception thrown by RM instead of wrapping it on a TransactionAbortedException.
			Assert.IsInstanceOfType(typeof(NotSupportedException), ex, "Invalid exception thrown");
		}

		[Test]
		public void Vol1_Throwing_On_Prepare()
		{
			bool called = false;
			TransactionStatus status = TransactionStatus.Active;
			Exception ex = null;
			var rm = new IntResourceManager(1)
			{
				FailPrepare = true,
				FailWithException = true,
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm.Value = 2;
					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm.Check(0, 1, 0, 0, 0, 0, 0, "rm");

			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.IsNotNull(ex, "Exception not thrown");
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException is null");
			Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException, "Invalid inner exception thrown");
			Assert.AreEqual(TransactionStatus.Aborted, status, "TransactionStatus != Aborted");
		}
		#endregion

		#region Success/Failure behavior Vol2_Dur0 Cases
		[Test]
		public void Vol2SPC_Committed()
		{
			TransactionStatus status = TransactionStatus.Active;
			bool called = false;
			var rm1 = new IntResourceManager(1)
			{
				UseSingle = true,
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(2)
			{
				UseSingle = true,
				Type = ResourceManagerType.Volatile
			};

			using (var ts = new TransactionScope())
			{
				rm1.Value = 11;
				rm2.Value = 22;
				var tr = Transaction.Current;
				tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
				ts.Complete();
			}

			// There can be only one *Single* PC enlistment,
			// so TM will downgrade both to normal enlistments.
			rm1.Check(0, 1, 1, 0, 0, 0, 0, "rm1");
			rm2.Check(0, 1, 1, 0, 0, 0, 0, "rm2");

			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.AreEqual(TransactionStatus.Committed, status, "TransactionStatus != Committed");
		}

		[Test]
		public void Vol2_Committed()
		{
			TransactionStatus status = TransactionStatus.Active;
			bool called = false;
			var rm1 = new IntResourceManager(1)
			{
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(1)
			{
				Type = ResourceManagerType.Volatile
			};

			using (var ts = new TransactionScope())
			{
				rm1.Value = 11;
				rm2.Value = 22;
				var tr = Transaction.Current;
				tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
				ts.Complete();
			}
			rm1.Check(0, 1, 1, 0, 0, 0, 0, "rm1");
			rm2.Check(0, 1, 1, 0, 0, 0, 0, "rm2");

			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.AreEqual(TransactionStatus.Committed, status, "TransactionStatus != Committed");
		}

		[Test]
		public void Vol2_Rollback()
		{
			TransactionStatus status = TransactionStatus.Active;
			bool called = false;
			var rm1 = new IntResourceManager(1)
			{
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(1)
			{
				Type = ResourceManagerType.Volatile
			};

			using (var ts = new TransactionScope())
			{
				rm1.Value = 11;
				rm2.Value = 22;
				var tr = Transaction.Current;
				tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
				// Not calling ts.Complete() on purpose..
			}

			rm1.Check(0, 0, 0, 1, 0, 0, 0, "rm1");
			rm2.Check(0, 0, 0, 1, 0, 0, 0, "rm2");

			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.AreEqual(TransactionStatus.Aborted, status, "TransactionStatus != Aborted");
		}

		[Test]
		public void Vol2SPC_Throwing_On_Commit()
		{
			TransactionStatus status = TransactionStatus.Active;
			bool called = false;
			Exception ex = null;
			var rm1 = new IntResourceManager(1)
			{
				UseSingle = true,
				FailCommit = true,
				FailWithException = true,
				ThrowThisException = new InvalidOperationException("rm1"),
				Type = ResourceManagerType.Volatile,
			};
			var rm2 = new IntResourceManager(2)
			{
				UseSingle = true,
				Type = ResourceManagerType.Volatile,
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm1.Value = 11;
					rm2.Value = 22;

					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			// There can be only one *Single* PC enlistment,
			// so TM will downgrade both to normal enlistments.
			rm1.Check(0, 1, 1, 0, 0, 0, 0, "rm1");
			rm2.Check(0, 1, 0, 0, 0, 0, 0, "rm2");

			// MS.NET wont call TransactionCompleted event in this particular case.
			Assert.IsFalse(called, "TransactionCompleted event handler _was_ called!?!?!");
			Assert.IsNotNull(ex, "Exception not thrown");
#if MS_EXCEPTIONS_BEHAVIOR
			// MS.NET will relay the exception thrown by RM instead of wrapping it on a TransactionAbortedException.
			Assert.AreEqual(rm1.ThrowThisException, ex, "Exception does not come from the expected RM");
#else
			// Mono wrapps the exception into a TransactionAbortedException.
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid type of exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException not thrown");
			Assert.AreEqual(rm1.ThrowThisException, ex.InnerException, "Exception does not come from the expected RM \n Ex: {0}", ex);
#endif
		}

		[Test]
		public void Vol2_Throwing_On_Commit()
		{
			bool called = false;
			Exception ex = null;
			var rm1 = new IntResourceManager(1)
			{
				FailCommit = true,
				FailWithException = true,
				ThrowThisException = new InvalidOperationException("rm1"),
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(2)
			{
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm1.Value = 11;
					rm2.Value = 22;

					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => called = true;
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm1.Check(0, 1, 1, 0, 0, 0, 0, "rm1");
			rm2.Check(0, 1, 0, 0, 0, 0, 0, "rm2");

			// MS.NET wont call TransactionCompleted event in this particular case.
			Assert.IsFalse(called, "TransactionCompleted event handler _was_ called!?!?!");
			Assert.IsNotNull(ex, "Exception not thrown");
#if MS_EXCEPTIONS_BEHAVIOR
			// MS.NET will relay the exception thrown by RM instead of wrapping it on a TransactionAbortedException.
			Assert.AreEqual(rm1.ThrowThisException, ex, "Exception does not come from the expected RM \n Ex: {0}", ex);
#else
			// Mono wrapps the exception into a TransactionAbortedException.
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid type of exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException not thrown");
			Assert.AreEqual(rm1.ThrowThisException, ex.InnerException, "Exception does not come from the expected RM \n Ex: {0}", ex);
#endif
		}

		[Test]
		public void Vol2_Throwing_On_Rollback()
		{
			bool called = false;
			Exception ex = null;
			var rm1 = new IntResourceManager(1)
			{
				FailRollback = true,
				FailWithException = true,
				ThrowThisException = new InvalidOperationException("rm1"),
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(2)
			{
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm1.Value = 11;
					rm2.Value = 22;

					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => called = true;
					// Not calling ts.Complete() on purpose..
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm1.Check(0, 0, 0, 1, 0, 0, 0, "rm1");
			rm2.Check(0, 0, 0, 0, 0, 0, 0, "rm2");

			// MS.NET wont call TransactionCompleted event in this particular case.
			Assert.IsFalse(called, "TransactionCompleted event handler _was_ called!?!?!");
			Assert.IsNotNull(ex, "Exception not thrown");
			// MS.NET will relay the exception thrown by RM instead of wrapping it on a TransactionAbortedException.
			Assert.AreEqual(rm1.ThrowThisException, ex, "Exception does not come from the expected RM \n Ex: {0}", ex);

		}

		[Test]
		public void Vol2_Throwing_On_First_Prepare()
		{
			TransactionStatus status = TransactionStatus.Active;
			bool called = false;
			Exception ex = null;
			var rm1 = new IntResourceManager(1)
			{
				FailPrepare = true,
				FailWithException = true,
				ThrowThisException = new InvalidOperationException("rm1"),
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(2)
			{
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm1.Value = 11;
					rm2.Value = 22;

					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm1.Check(0, 1, 0, 0, 0, 0, 0, "rm1");
			rm2.Check(0, 0, 0, 1, 0, 0, 0, "rm2");

			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.IsNotNull(ex, "Exception not thrown");
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException is null");
			Assert.IsInstanceOfType(typeof(InvalidOperationException), ex.InnerException, "Invalid inner exception thrown");
			Assert.AreEqual(TransactionStatus.Aborted, status, "TransactionStatus != Aborted");
		}

		[Test]
		public void Vol2_Throwing_On_Second_Prepare()
		{
			TransactionStatus status = TransactionStatus.Active;
			bool called = false;
			Exception ex = null;
			var rm1 = new IntResourceManager(1)
			{
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(2)
			{
				FailPrepare = true,
				FailWithException = true,
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm1.Value = 11;
					rm2.Value = 22;

					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm1.Check(0, 1, 0, 1, 0, 0, 0, "rm1");
			rm2.Check(0, 1, 0, 0, 0, 0, 0, "rm2");

			Assert.IsTrue(called, "TransactionCompleted event handler not called!");
			Assert.IsNotNull(ex, "Exception not thrown");
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException is null");
			Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException, "Invalid inner exception thrown");
			Assert.AreEqual(TransactionStatus.Aborted, status, "TransactionStatus != Aborted");
		}

		[Test]
		public void Vol2_Throwing_On_First_Prepare_And_Second_Rollback()
		{
			TransactionStatus status = TransactionStatus.Active;
			bool called = false;
			Exception ex = null;
			var rm1 = new IntResourceManager(1)
			{
				FailPrepare = true,
				FailWithException = true,
				ThrowThisException = new InvalidOperationException("rm1"),
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(2)
			{
				FailRollback = true,
				FailWithException = true,
				ThrowThisException = new InvalidOperationException("rm2"),
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm1.Value = 11;
					rm2.Value = 22;

					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm1.Check(0, 1, 0, 0, 0, 0, 0, "rm1");
			rm2.Check(0, 0, 0, 1, 0, 0, 0, "rm2");

			// MS.NET wont call TransactionCompleted event in this particular case.
			Assert.IsFalse(called, "TransactionCompleted event handler _was_ called!?!?!");
			Assert.IsNotNull(ex, "Exception not thrown");
#if MS_EXCEPTIONS_BEHAVIOR
			// MS.NET will relay the exception thrown by RM instead of wrapping it on a TransactionAbortedException.
			Assert.AreEqual(rm2.ThrowThisException, ex, "Exception does not come from the expected RM");
#else
			// Mono wrapps the exception into a TransactionAbortedException.
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid type of exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException not thrown");
			Assert.AreEqual(rm2.ThrowThisException, ex.InnerException, "Exception does not come from the expected RM \n Ex: {0}", ex.InnerException);
#endif
		}

		[Test]
		public void Vol2_Throwing_On_First_Rollback_And_Second_Prepare()
		{
			TransactionStatus status = TransactionStatus.Active;
			bool called = false;
			Exception ex = null;
			var rm1 = new IntResourceManager(1)
			{
				FailRollback = true,
				FailWithException = true,
				ThrowThisException = new InvalidOperationException("rm1"),
				Type = ResourceManagerType.Volatile
			};
			var rm2 = new IntResourceManager(2)
			{
				FailPrepare = true,
				FailWithException = true,
				ThrowThisException = new InvalidOperationException("rm2"),
				Type = ResourceManagerType.Volatile
			};

			try
			{
				using (var ts = new TransactionScope())
				{
					rm1.Value = 11;
					rm2.Value = 22;

					var tr = Transaction.Current;
					tr.TransactionCompleted += (s, e) => { called = true; status = e.Transaction.TransactionInformation.Status; };
					ts.Complete();
				}
			}
			catch (Exception _ex)
			{
				ex = _ex;
			}

			rm1.Check(0, 1, 0, 1, 0, 0, 0, "rm1");
			rm2.Check(0, 1, 0, 0, 0, 0, 0, "rm2");

			// MS.NET wont call TransactionCompleted event in this particular case.
			Assert.IsFalse(called, "TransactionCompleted event handler _was_ called!?!?!");
			Assert.IsNotNull(ex, "Exception not thrown");
#if MS_EXCEPTIONS_BEHAVIOR
			// MS.NET will relay the exception thrown by RM instead of wrapping it on a TransactionAbortedException.
			Assert.AreEqual(rm1.ThrowThisException, ex, "Exception does not come from the expected RM");
#else
			// Mono wrapps the exception into a TransactionAbortedException.
			Assert.IsInstanceOfType(typeof(TransactionAbortedException), ex, "Invalid type of exception thrown");
			Assert.IsNotNull(ex.InnerException, "InnerException not thrown");
			Assert.AreEqual(rm1.ThrowThisException, ex.InnerException, "Exception does not come from the expected RM \n Ex: {0}", ex);
#endif
		}


		#endregion

		#endregion

	}
}

