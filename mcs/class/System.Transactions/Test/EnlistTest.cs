//
// Tests for combination of volatile & durable resource manangers
//
// Author:
//	Ankit Jain	<JAnkit@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

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

			irm.Check ( 0, 0, 0, 1, 0, "irm" );
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
			irm.Volatile = false;
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
			irm.Volatile = false;

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
			irm.Volatile = false;
			irm.FailSPC = true;
			irm.UseSingle = true;
			try {
				using (TransactionScope scope = new TransactionScope ()) {
					irm.Value = 2;

					scope.Complete ();
				}
			}
			catch (TransactionAbortedException) {
				irm.Check ( 1, 0, 0, 0, 0, "irm" );
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

			irm [0].Volatile = false;
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

			irm [0].Volatile = false;
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
					irm [i].Check ( 0, 1, 0, 1, 0, "irm [" + i + "]" );
			}
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

			irm [0].Volatile = false;
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
					irm [i].Check ( 0, 1, 0, 1, 0, "irm [" + i + "]" );

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
			irm[1].Volatile = false;

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
					irm[i].Check(0, 1, 0, 1, 0, "irm [" + i + "]");

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

			irm [0].Volatile = false;
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
				irm [0].Check ( 0, 0, 0, 1, 0, "irm [0]");

				/* irm [1] & [2] get prepare,
				 * [2] -> ForceRollback,
				 * [1] & [3] get rollback,
				 * [0](durable) gets rollback */
				irm [1].Check ( 0, 1, 0, 1, 0, "irm [1]" );
				irm [2].Check ( 0, 1, 0, 0, 0, "irm [2]" );
				irm [3].Check ( 0, 0, 0, 1, 0, "irm [3]" );

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

			irm [0].Volatile = false;
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

				irm [0].Check ( 1, 0, 0, 0, 0, "irm [0]" );
				irm [1].Check ( 0, 1, 0, 1, 0, "irm [1]" );
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
			irm [0].Volatile = false;
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

				irm [0].Check ( 1, 0, 0, 0, 0, "irm [0]" );
				irm [1].Check ( 0, 1, 0, 1, 0, "irm [1]" );
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

			irm [0].Volatile = false;
			irm [1].Volatile = false;

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
			irm.Volatile = false;

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

	}
}

