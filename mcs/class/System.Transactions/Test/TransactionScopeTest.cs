//
// Unit tests for TransactionScope and Implicit/Explicit use of
// Transactions
//
// Author:
//	Ankit Jain	<JAnkit@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;
using NUnit.Framework;
using System.Transactions;

namespace MonoTests.System.Transactions
{
	[TestFixture]
	public class TransactionScopeTest
	{

		[Test]
		public void TransactionScopeWithInvalidTimeSpanThrows ()
		{
			try {
				TransactionScope scope = new TransactionScope (TransactionScopeOption.Required, TimeSpan.FromSeconds (-1));
				Assert.Fail ("Expected exception when passing TransactionScopeOption and an invalid TimeSpan.");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual ("scopeTimeout", ex.ParamName);
			}

			try {
				TransactionScope scope = new TransactionScope (null, TimeSpan.FromSeconds (-1));
				Assert.Fail ("Expected exception when passing TransactionScopeOption and an invalid TimeSpan.");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual ("scopeTimeout", ex.ParamName);
			}
		}

		[Test]
		public void TransactionScopeCommit ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			using (TransactionScope scope = new TransactionScope ()) {
				Assert.IsNotNull (Transaction.Current, "Ambient transaction does not exist");
				Assert.AreEqual (TransactionStatus.Active, Transaction.Current.TransactionInformation.Status);
				
				scope.Complete ();
			}
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (after)");
		}

		[Test]
		public void TransactionScopeAbort ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			IntResourceManager irm = new IntResourceManager (1);
			using (TransactionScope scope = new TransactionScope ()) {
				Assert.IsNotNull (Transaction.Current, "Ambient transaction does not exist");
				Assert.AreEqual (TransactionStatus.Active, Transaction.Current.TransactionInformation.Status, "transaction is not active");

				irm.Value = 2;
				/* Not completing scope here */
			}
			irm.Check ( 0, 0, 1, 0, "irm");
			Assert.AreEqual (1, irm.Value);
			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TransactionScopeCompleted1 ()
		{
			using (TransactionScope scope = new TransactionScope ()) {
				scope.Complete ();
				/* Can't access ambient transaction after scope.Complete */
				TransactionStatus status = Transaction.Current.TransactionInformation.Status;
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TransactionScopeCompleted2 ()
		{
			using (TransactionScope scope = new TransactionScope ()) {
				scope.Complete ();
				Transaction.Current = Transaction.Current;
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TransactionScopeCompleted3 ()
		{
			using (TransactionScope scope = new TransactionScope ()) {
				scope.Complete ();
				scope.Complete ();
			}
		}

		#region NestedTransactionScope tests
		[Test]
		public void NestedTransactionScope1 ()
		{
			IntResourceManager irm = new IntResourceManager (1);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				/* Complete this scope */
				scope.Complete ();
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			/* Value = 2, got committed */
			Assert.AreEqual (irm.Value, 2, "#1");
			irm.Check ( 1, 1, 0, 0, "irm" );
		}

		[Test]
		public void NestedTransactionScope2 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				/* Not-Completing this scope */
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			/* Value = 2, got rolledback */
			Assert.AreEqual (irm.Value, 1, "#2");
			irm.Check ( 0, 0, 1, 0, "irm" );
		}

		[Test]
		public void NestedTransactionScope3 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope ()) {
					irm2.Value = 20;

					scope2.Complete ();
				}

				scope.Complete ();
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			/* Both got committed */
			Assert.AreEqual (irm.Value, 2, "#3");
			Assert.AreEqual (irm2.Value, 20, "#4");
			irm.Check ( 1, 1, 0, 0, "irm" );
			irm2.Check ( 1, 1, 0, 0, "irm2" );
		}

		[Test]
		public void NestedTransactionScope4 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope ()) {
					irm2.Value = 20;

					/* Inner Tx not completed, Tx should get rolled back */
					//scope2.Complete();
				}
				/* Both rolledback */
				irm.Check ( 0, 0, 1, 0, "irm" );
				irm2.Check ( 0, 0, 1, 0, "irm2" );
				Assert.AreEqual (TransactionStatus.Aborted, Transaction.Current.TransactionInformation.Status, "#5");
				//scope.Complete ();
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");

			Assert.AreEqual (irm.Value, 1, "#6");
			Assert.AreEqual (irm2.Value, 10, "#7");
			irm.Check ( 0, 0, 1, 0, "irm" );
		}

		[Test]
		public void NestedTransactionScope5 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope ()) {
					irm2.Value = 20;
					scope2.Complete ();
				}

				Assert.AreEqual (TransactionStatus.Active, Transaction.Current.TransactionInformation.Status, "#8");
				/* Not completing outer scope
				scope.Complete (); */
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");

			Assert.AreEqual (irm.Value, 1, "#9");
			Assert.AreEqual (irm2.Value, 10, "#10");
			irm.Check ( 0, 0, 1, 0, "irm" );
			irm2.Check ( 0, 0, 1, 0, "irm2" );
		}

		[Test]
		public void NestedTransactionScope6 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope (TransactionScopeOption.RequiresNew)) {
					irm2.Value = 20;
					scope2.Complete ();
				}
				/* vr2, committed */
				irm2.Check ( 1, 1, 0, 0, "irm2" );
				Assert.AreEqual (irm2.Value, 20);

				Assert.AreEqual (TransactionStatus.Active, Transaction.Current.TransactionInformation.Status, "#11");

				scope.Complete ();
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			Assert.AreEqual (irm.Value, 2, "#12");
			irm.Check ( 1, 1, 0, 0, "irm" );
		}

		[Test]
		public void NestedTransactionScope7 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope (TransactionScopeOption.RequiresNew)) {
					irm2.Value = 20;
					/* Not completing 
					 scope2.Complete();*/
				}

				/* irm2, rolled back*/
				irm2.Check ( 0, 0, 1, 0, "irm2" );
				Assert.AreEqual (irm2.Value, 10, "#13");

				Assert.AreEqual (TransactionStatus.Active, Transaction.Current.TransactionInformation.Status, "#14");

				scope.Complete ();
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			/* ..But irm got committed */
			Assert.AreEqual (irm.Value, 2, "#15");
			irm.Check ( 1, 1, 0, 0, "irm" );
		}

		[Test]
		public void NestedTransactionScope8 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope (TransactionScopeOption.Suppress)) {
					/* Not transactional, so this WONT get committed */
					irm2.Value = 20;
					scope2.Complete ();
				}
				irm2.Check ( 0, 0, 0, 0, "irm2" );
				Assert.AreEqual (20, irm2.Value, "#16");
				Assert.AreEqual (TransactionStatus.Active, Transaction.Current.TransactionInformation.Status, "#17");

				scope.Complete ();
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists");
			Assert.AreEqual (irm.Value, 2, "#18");
			irm.Check ( 1, 1, 0, 0, "irm" );
		}

		[Test]
		public void NestedTransactionScope8a ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );
			IntResourceManager irm2 = new IntResourceManager ( 10 );

			Assert.IsNull ( Transaction.Current, "Ambient transaction exists" );
			using (TransactionScope scope = new TransactionScope (TransactionScopeOption.Suppress )) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope ()) {
					irm2.Value = 20;
					scope2.Complete ();
				}
				irm2.Check ( 1, 1, 0, 0, "irm2" );
				Assert.AreEqual ( 20, irm2.Value, "#16a" );

				scope.Complete ();
			}

			Assert.IsNull ( Transaction.Current, "Ambient transaction exists" );
			Assert.AreEqual ( 2, irm.Value, "#18a" );
			irm.Check ( 0, 0, 0, 0, "irm" );
		}

		[Test]
		public void NestedTransactionScope9 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope (TransactionScopeOption.Suppress)) {
					/* Not transactional, so this WONT get committed */
					irm2.Value = 4;
					scope2.Complete ();
				}
				irm2.Check ( 0, 0, 0, 0, "irm2" );

				using (TransactionScope scope3 = new TransactionScope (TransactionScopeOption.RequiresNew)) {
					irm.Value = 6;
					scope3.Complete ();
				}

				/* vr's value has changed as the inner scope committed = 6 */
				irm.Check ( 1, 1, 0, 0, "irm" );
				Assert.AreEqual (irm.Value, 6, "#19");
				Assert.AreEqual (irm.Actual, 6, "#20");
				Assert.AreEqual (TransactionStatus.Active, Transaction.Current.TransactionInformation.Status, "#21");

				scope.Complete ();
			}

			Assert.IsNull (Transaction.Current, "Ambient transaction exists (after)");
			Assert.AreEqual (irm.Value, 6, "#22");
			irm.Check ( 2, 2, 0, 0, "irm" );
		}

		[Test]
		[ExpectedException (typeof (TransactionAbortedException))]
		public void NestedTransactionScope10 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			bool failed = false;

			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope ()) {
					irm.Value = 4;
					/* Not completing this, so the transaction will
					 * get aborted 
					scope2.Complete (); */
				}

				using (TransactionScope scope3 = new TransactionScope ()) {
					/* Aborted transaction cannot be used for another
					 * TransactionScope 
					 */
					//Assert.Fail ("Should not reach here.");
					failed = true;
				}
			}
			Assert.IsFalse ( failed, "Aborted Tx cannot be used for another TransactionScope" );
		}

		[Test]
		public void NestedTransactionScope12 ()
		{
			IntResourceManager irm = new IntResourceManager (1);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			using (TransactionScope scope = new TransactionScope ()) {
				irm.Value = 2;

				using (TransactionScope scope2 = new TransactionScope ()) {
					irm.Value = 4;
					/* Not completing this, so the transaction will
					 * get aborted 
					scope2.Complete (); */
				}

				using (TransactionScope scope3 = new TransactionScope (TransactionScopeOption.RequiresNew)) {
					/* Using RequiresNew here, so outer transaction
					 * being aborted doesn't matter
					 */
					scope3.Complete (); 
				}
			}
		}

		[Test]
		[ExpectedException (typeof (TransactionAbortedException))]
		public void NestedTransactionScope13 ()
		{
			IntResourceManager irm = new IntResourceManager ( 1 );

			Assert.IsNull ( Transaction.Current, "Ambient transaction exists (before)" );
			using ( TransactionScope scope = new TransactionScope () ) {
				irm.Value = 2;

				using ( TransactionScope scope2 = new TransactionScope () ) {
					irm.Value = 4;
					/* Not completing this, so the transaction will
					 * get aborted 
					scope2.Complete (); */
				}

				scope.Complete ();
			}
		}
		#endregion

		/* Tests using IntResourceManager */

		[Test]
		public void RMFail1 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);
			IntResourceManager irm3 = new IntResourceManager (12);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			try {
				using (TransactionScope scope = new TransactionScope ()) {
					irm.Value = 2;
					irm2.Value = 20;
					irm3.Value = 24;

					/* Make second RM fail to prepare, this should throw
					 * TransactionAbortedException when the scope ends 
					 */
					irm2.FailPrepare = true;
					scope.Complete ();
				}
			} catch (TransactionAbortedException) {
				irm.Check ( 1, 0, 1, 0, "irm");
				irm2.Check ( 1, 0, 0, 0, "irm2");
				irm3.Check ( 0, 0, 1, 0, "irm3");
			}
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (after)");
		}

		[Test]
		public void RMFail2 ()
		{
			IntResourceManager irm = new IntResourceManager (1);
			IntResourceManager irm2 = new IntResourceManager (10);
			IntResourceManager irm3 = new IntResourceManager (12);

			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			try {
				using (TransactionScope scope = new TransactionScope (TransactionScopeOption.Required, new TimeSpan (0, 0, 30))) {
					irm.Value = 2;
					irm2.Value = 20;
					irm3.Value = 24;

					/* irm2 wont call Prepared or ForceRollback in
					 * its Prepare (), so TransactionManager will timeout
					 * waiting for it 
					 */
					irm2.IgnorePrepare = true;
					scope.Complete ();
				}
			} catch (TransactionAbortedException e) {
				/* FIXME: Not working right now.. no timeout exception thrown! */
				
				Assert.IsNotNull ( e.InnerException, "innerexception is null" );
				Assert.AreEqual (typeof (TimeoutException), e.InnerException.GetType (), "#32");

				Assert.IsNull (Transaction.Current, "Ambient transaction exists (after)");
				return;
			}

			Assert.Fail ("Expected TransactionAbortedException (TimeoutException)");
		}

		#region Explicit Transaction Tests

		[Test]
		public void ExplicitTransactionCommit ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");

			CommittableTransaction ct = new CommittableTransaction ();
			Transaction oldTransaction = Transaction.Current;
			Transaction.Current = ct;

			IntResourceManager irm = new IntResourceManager (1);
			irm.Value = 2;
			ct.Commit ();

			Assert.AreEqual (2, irm.Value, "#33");
			Assert.AreEqual (TransactionStatus.Committed, ct.TransactionInformation.Status, "#34");
			Transaction.Current = oldTransaction;
		}

		[Test]
		public void ExplicitTransactionRollback ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");

			CommittableTransaction ct = new CommittableTransaction ();
			Transaction oldTransaction = Transaction.Current;
			Transaction.Current = ct;

			IntResourceManager irm = new IntResourceManager (1);
			irm.Value = 2;
			Assert.AreEqual (TransactionStatus.Active, ct.TransactionInformation.Status, "#35");
			ct.Rollback ();

			Assert.AreEqual (1, irm.Value, "#36");
			Assert.AreEqual (TransactionStatus.Aborted, ct.TransactionInformation.Status, "#37");
			Transaction.Current = oldTransaction;
		}

		[Test]
		public void ExplicitTransaction1 ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			CommittableTransaction ct = new CommittableTransaction ();
			Transaction oldTransaction = Transaction.Current;

			Transaction.Current = ct;

			IntResourceManager irm = new IntResourceManager (1);

			irm.Value = 2;

			using (TransactionScope scope = new TransactionScope ()) {
				Assert.AreEqual (ct, Transaction.Current, "#38");
				irm.Value = 4;
				scope.Complete ();
			}

			Assert.AreEqual (ct, Transaction.Current, "#39");
			Assert.AreEqual (TransactionStatus.Active, Transaction.Current.TransactionInformation.Status, "#40");
			Assert.AreEqual (1, irm.Actual, "#41"); /* Actual value */

			ct.Commit ();
			Assert.AreEqual (4, irm.Actual, "#42"); /* New committed actual value */
			Assert.AreEqual (TransactionStatus.Committed, Transaction.Current.TransactionInformation.Status, "#43");
			Transaction.Current = oldTransaction;
		}

		[Test]
		public void ExplicitTransaction2 ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			CommittableTransaction ct = new CommittableTransaction ();
			Transaction oldTransaction = Transaction.Current;

			Transaction.Current = ct;

			IntResourceManager irm = new IntResourceManager (1);

			irm.Value = 2;
			using (TransactionScope scope = new TransactionScope ()) {
				Assert.AreEqual (ct, Transaction.Current, "#44");

				/* Not calling scope.Complete
				scope.Complete ();*/
			}

			Assert.AreEqual (TransactionStatus.Aborted, ct.TransactionInformation.Status, "#45");
			Assert.AreEqual (ct, Transaction.Current, "#46");
			Assert.AreEqual (1, irm.Actual, "#47");
			Assert.AreEqual (1, irm.NumRollback, "#48");
			irm.Check ( 0, 0, 1, 0, "irm" );
			Transaction.Current = oldTransaction;

			try {
				ct.Commit ();
			} catch (TransactionAbortedException) {
				return;
			}
			Assert.Fail ("Commit on an aborted transaction should fail");
		}

		[Test]
		public void ExplicitTransaction3 ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			CommittableTransaction ct = new CommittableTransaction ();
			Transaction oldTransaction = Transaction.Current;

			Transaction.Current = ct;

			IntResourceManager irm = new IntResourceManager (1);

			using (TransactionScope scope = new TransactionScope (TransactionScopeOption.RequiresNew)) {
				Assert.IsTrue (ct != Transaction.Current, "Scope with RequiresNew should have a new ambient transaction");

				irm.Value = 3;
				scope.Complete ();
			}

			irm.Value = 2;

			Assert.AreEqual (3, irm.Actual, "#50");

			Assert.AreEqual (ct, Transaction.Current, "#51");
			ct.Commit ();
			Assert.AreEqual (2, irm.Actual, "#52");
			Transaction.Current = oldTransaction;
		}

		[Test]
		public void ExplicitTransaction4 ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			CommittableTransaction ct = new CommittableTransaction ();
			Transaction oldTransaction = Transaction.Current;

			/* Not setting ambient transaction 
			 Transaction.Current = ct; 
			 */

			IntResourceManager irm = new IntResourceManager (1);

			using (TransactionScope scope = new TransactionScope (ct)) {
				Assert.AreEqual (ct, Transaction.Current, "#53");

				irm.Value = 2;
				scope.Complete ();
			}

			Assert.AreEqual (oldTransaction, Transaction.Current, "#54");
			Assert.AreEqual (TransactionStatus.Active, ct.TransactionInformation.Status, "#55");
			Assert.AreEqual (1, irm.Actual, "#56"); /* Actual value */

			ct.Commit ();
			Assert.AreEqual (2, irm.Actual, "#57"); /* New committed actual value */
			Assert.AreEqual (TransactionStatus.Committed, ct.TransactionInformation.Status, "#58");

			irm.Check ( 1, 1, 0, 0, "irm");
		}

		[Test]
		public void ExplicitTransaction5 ()
		{
			Assert.IsNull (Transaction.Current, "Ambient transaction exists (before)");
			CommittableTransaction ct = new CommittableTransaction ();
			Transaction oldTransaction = Transaction.Current;

			/* Not setting ambient transaction 
			 Transaction.Current = ct; 
			 */

			IntResourceManager irm = new IntResourceManager (1);

			using (TransactionScope scope = new TransactionScope (ct)) {
				Assert.AreEqual (ct, Transaction.Current, "#59");

				irm.Value = 2;

				/* Not completing this scope
				scope.Complete (); */
			}

			Assert.AreEqual (oldTransaction, Transaction.Current, "#60");
			Assert.AreEqual (TransactionStatus.Aborted, ct.TransactionInformation.Status, "#61");
			Assert.AreEqual (1, irm.Actual, "#62"); /* Actual value */

			irm.Check ( 0, 0, 1, 0, "irm");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ExplicitTransaction6 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager (1);
			irm.Value = 2;
			ct.Commit ();

			ct.Commit ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ExplicitTransaction6a ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			irm.Value = 2;
			ct.Commit ();

			/* Using a already committed transaction in a new 
			 * TransactionScope
			 */
			TransactionScope scope = new TransactionScope ( ct );
		}

		[Test]
		public void ExplicitTransaction6b ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			
			Transaction.Current = ct; 

			TransactionScope scope1 = new TransactionScope ();
			/* Enlist */
			irm.Value = 2;

			scope1.Complete ();

			try {
				ct.Commit ();
			} catch (TransactionAbortedException) {
				irm.Check ( 0, 0, 1, 0, "irm" );
				
				scope1.Dispose ();
				Transaction.Current = null;
				return;
			}
			Assert.Fail ( "Commit should've failed" );
		}

		[Test]
		public void ExplicitTransaction6c ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );

			Transaction.Current = ct;

			TransactionScope scope1 = new TransactionScope (TransactionScopeOption.RequiresNew);
			/* Enlist */
			irm.Value = 2;

			TransactionScope scope2 = new TransactionScope ();
			try {
				scope1.Dispose ();
			} catch (InvalidOperationException) {
				/* Error: TransactionScope nested incorrectly */
				irm.Check ( 0, 0, 1, 0, "irm" );
				scope2.Dispose ();
				Transaction.Current = null;
				return;
			}

			Assert.Fail ("Commit should've failed");
		}

		[Test]
		public void ExplicitTransaction6d ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );

			Transaction.Current = ct;

			TransactionScope scope1 = new TransactionScope ();
			/* Enlist */
			irm.Value = 2;

			TransactionScope scope2 = new TransactionScope ( TransactionScopeOption.RequiresNew );
			try {
				scope1.Dispose ();
			} catch (InvalidOperationException) {
				/* Error: TransactionScope nested incorrectly */
				scope2.Dispose ();
				Transaction.Current = null;
				return;
			}

			Assert.Fail ( "Commit should've failed" );
		}

		[Test]
		public void ExplicitTransaction6e ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );

			Transaction.Current = ct;

			TransactionScope scope1 = new TransactionScope ();
			/* Enlist */
			irm.Value = 2;

			TransactionScope scope2 = new TransactionScope ( TransactionScopeOption.Suppress);
			try {
				scope1.Dispose ();
			} catch (InvalidOperationException) {
				/* Error: TransactionScope nested incorrectly */
				scope2.Dispose ();
				Transaction.Current = null;
				return;
			}

			Assert.Fail ( "Commit should've failed" );
		}

		[Test]
		[ExpectedException (typeof (TransactionException))]
		public void ExplicitTransaction7 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager (1);
			irm.Value = 2;
			ct.Commit ();
			/* Cannot accept any new work now, so TransactionException */
			ct.Rollback ();
		}

		[Test]
		public void ExplicitTransaction8 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			using ( TransactionScope scope = new TransactionScope (ct) ) {
				irm.Value = 2;
				/* FIXME: Why TransactionAbortedException ?? */
				try {
					ct.Commit ();
				} catch ( TransactionAbortedException) {
					irm.Check ( 0, 0, 1, 0, "irm" );
					return;
				}
				Assert.Fail ( "Should not be reached" );
			}
		}

		[Test]
		public void ExplicitTransaction8a ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			using ( TransactionScope scope = new TransactionScope ( ct ) ) {
				irm.Value = 2;
				scope.Complete ();
				/* FIXME: Why TransactionAbortedException ?? */
				try {
					ct.Commit ();
				}
				catch ( TransactionAbortedException) {
					irm.Check ( 0, 0, 1, 0, "irm" );
					return;
				}
				Assert.Fail ( "Should not be reached" );
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ExplicitTransaction9 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			ct.BeginCommit ( null, null );
			ct.BeginCommit ( null, null );
		}

		[Test]
		public void ExplicitTransaction10 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			Transaction.Current = ct;
			irm.Value = 2;

			TransactionScope scope = new TransactionScope ( ct );
			Assert.AreEqual ( ct, Transaction.Current, "ambient transaction" );
			//scope.Complete ();
			//scope.Dispose ();
			try {
				ct.Commit ();
			} catch ( TransactionAbortedException) {
				irm.Check ( 0, 0, 1, 0, "irm" );
				Transaction.Current = null;
				return;
			}
			
			Transaction.Current = null;
			Assert.Fail ("Expected TransactionAbortedException");
		}

		[Test]
		public void ExplicitTransaction10a ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			Transaction.Current = ct;
			irm.Value = 2;
			Transaction.Current = null;

			TransactionScope scope = new TransactionScope ( ct );
			Assert.AreEqual ( ct, Transaction.Current, "ambient transaction" );
			Transaction.Current = null;
			//scope2.Complete ();
			//scope2.Dispose ();
			try {
				ct.Commit ();
			}
			catch ( TransactionAbortedException) {
				irm.Check ( 0, 0, 1, 0, "irm" );
				Transaction.Current = null;
				return;
			}

			Transaction.Current = null;
			Assert.Fail ();
		}

		[Test]
		public void ExplicitTransaction10b ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			Transaction.Current = ct;
			irm.Value = 2;
			Transaction.Current = null;

			TransactionScope scope = new TransactionScope ( ct );
			Assert.AreEqual ( ct, Transaction.Current, "ambient transaction" );
			//scope2.Complete ();
			//scope2.Dispose ();
			IAsyncResult ar = ct.BeginCommit ( null, null );
			try {
				ct.EndCommit (ar);
			}
			catch ( TransactionAbortedException) {
				irm.Check ( 0, 0, 1, 0, "irm" );
				Transaction.Current = null;
				return;
			}

			Transaction.Current = null;
			Assert.Fail ();
		}

		[Test]
		[ExpectedException ( typeof (ArgumentException))]
		public void ExplicitTransaction12 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();

			IntResourceManager irm = new IntResourceManager ( 1 );
			irm.FailPrepare = true;
			ct.BeginCommit ( null, null );
			ct.EndCommit ( null );
		}

		[Test]
		public void ExplicitTransaction13 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();
			IntResourceManager irm = new IntResourceManager ( 1 );

			Assert.IsNull ( Transaction.Current );
			Transaction.Current = ct;
			irm.Value = 2;
			irm.FailPrepare = true;

			try {
				ct.Commit ();
			} catch ( TransactionAbortedException ) {
				Assert.AreEqual ( TransactionStatus.Aborted, ct.TransactionInformation.Status );
				try {
					ct.BeginCommit ( null, null );
				} catch (Exception) {
					Transaction.Current = null;
					return;
				}
				Assert.Fail ( "Should not be reached(2)" );
			}
			Assert.Fail ("Should not be reached");
		}

		[Test]
		public void ExplicitTransaction14 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();
			IntResourceManager irm = new IntResourceManager ( 1 );

			Assert.IsNull ( Transaction.Current );
			Transaction.Current = ct;
			irm.Value = 2;

			ct.Commit ();

			Assert.AreEqual ( TransactionStatus.Committed, ct.TransactionInformation.Status );
			try {
				ct.BeginCommit ( null, null );
			}
			catch ( Exception) {
				Transaction.Current = null;
				return;
			}
			Assert.Fail ( "Should not be reached" );
		}

		[Test]
		public void ExplicitTransaction15 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();
			IntResourceManager irm = new IntResourceManager ( 1 );
			IntResourceManager irm2 = new IntResourceManager ( 3 );

			Assert.IsNull ( Transaction.Current );
			Transaction.Current = ct;

			try {
				using (TransactionScope scope = new TransactionScope ()) {
					irm.Value = 2;
					Transaction.Current = new CommittableTransaction ();
					irm2.Value = 6;
				}
			} catch (InvalidOperationException) {
				irm.Check ( 0, 0, 1, 0, "irm" );
				irm2.Check ( 0, 0, 1, 0, "irm2" );
				Transaction.Current = null;
				return;
			}

			Assert.Fail ( "Should not be reached" );
		}

		[Test]
		public void ExplicitTransaction16 ()
		{
			CommittableTransaction ct = new CommittableTransaction ();
			IntResourceManager irm0 = new IntResourceManager ( 3 );
			IntResourceManager irm = new IntResourceManager ( 1 );

			Assert.IsNull ( Transaction.Current );

			Transaction.Current = ct;

			irm.FailPrepare = true;
			irm.FailWithException = true;
			irm.Value = 2;
			irm0.Value = 6;

			try {
				ct.Commit ();
			} catch (TransactionAbortedException e) {
				Assert.IsNotNull ( e.InnerException, "Expected an InnerException of type NotSupportedException" );
				Assert.AreEqual ( typeof (NotSupportedException), e.InnerException.GetType (), "Inner exception should be NotSupportedException" );
				irm.Check ( 1, 0, 0, 0, "irm" );
				irm0.Check ( 0, 0, 1, 0, "irm0" );
				Transaction.Current = null;
				return;
			}
			 
			Assert.Fail ( "Should not be reached" );
		}

		#endregion

		[Test]
		public void DefaultIsolationLevel()
		{
			using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required))
			{
				Assert.AreEqual(IsolationLevel.Serializable, Transaction.Current.IsolationLevel);
			}
		}
		
		[Test]
		public void ExplicitIsolationLevel()
		{
			TransactionOptions transactionOptions = new TransactionOptions();
			transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;
			using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
			{
				Assert.AreEqual(IsolationLevel.ReadCommitted, Transaction.Current.IsolationLevel);
			}
		}
	}

}

