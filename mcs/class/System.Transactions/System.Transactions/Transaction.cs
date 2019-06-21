//
// Transaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Transactions
{
	[Serializable]
	public class Transaction : IDisposable, ISerializable
	{
		[ThreadStatic]
		static Transaction ambient;

		IsolationLevel level;
		TransactionInformation info;

		ArrayList dependents = new ArrayList ();

		/* Volatile enlistments */
		List <IEnlistmentNotification> volatiles;

		/* Durable enlistments 
		   Durable RMs can also have 2 Phase commit but
		   not in LTM, and that is what we are supporting
		   right now   
		 */
		List <ISinglePhaseNotification> durables;

		IPromotableSinglePhaseNotification pspe = null;

		delegate void AsyncCommit ();
		
		AsyncCommit asyncCommit = null;
		bool committing;
		bool committed = false;
		bool aborted = false;
		TransactionScope scope = null;

		Exception innerException;
		Guid tag = Guid.NewGuid ();

		internal List <IEnlistmentNotification> Volatiles {
			get {
				if (volatiles == null)
					volatiles = new List <IEnlistmentNotification> ();
				return volatiles;
			}
		}

		internal List <ISinglePhaseNotification> Durables {
			get {
				if (durables == null)
					durables = new List <ISinglePhaseNotification> ();
				return durables;
			}
		}

		internal IPromotableSinglePhaseNotification Pspe { get { return pspe; } }

		internal Transaction (IsolationLevel isolationLevel)
		{
			info = new TransactionInformation ();
			level = isolationLevel;
		}

		internal Transaction (Transaction other)
		{
			level = other.level;
			info = other.info;
			dependents = other.dependents;
			volatiles = other.Volatiles;
			durables = other.Durables;
			pspe = other.Pspe;
		}

		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info,
			StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public event TransactionCompletedEventHandler TransactionCompleted;

		public static Transaction Current {
			get { 
				EnsureIncompleteCurrentScope ();
				return CurrentInternal;
			}
			set { 
				EnsureIncompleteCurrentScope ();
				CurrentInternal = value;
			}
		}

		internal static Transaction CurrentInternal {
			get { return ambient; }
			set { ambient = value; }
		}

		public IsolationLevel IsolationLevel {
			get { 
				EnsureIncompleteCurrentScope ();
				return level; 
			}
		}

		public TransactionInformation TransactionInformation {
			get { 
				EnsureIncompleteCurrentScope ();
				return info; 
			}
		}

		public Transaction Clone ()
		{
			return new Transaction (this);
		}

		public void Dispose ()
		{
			if (TransactionInformation.Status == TransactionStatus.Active)
				Rollback();
		}

		[MonoTODO]
		public DependentTransaction DependentClone (
			DependentCloneOption cloneOption)
		{
			DependentTransaction d = 
				new DependentTransaction (this, cloneOption);
			dependents.Add (d);
			return d;
		}

		[MonoTODO ("Only SinglePhase commit supported for durable resource managers.")]
		[PermissionSetAttribute (SecurityAction.LinkDemand)]
		public Enlistment EnlistDurable (Guid resourceManagerIdentifier,
			IEnlistmentNotification enlistmentNotification,
			EnlistmentOptions enlistmentOptions)
		{
			throw new NotImplementedException ("DTC unsupported, only SinglePhase commit supported for durable resource managers.");
		}

		[MonoTODO ("Only Local Transaction Manager supported. Cannot have more than 1 durable resource per transaction. Only EnlistmentOptions.None supported yet.")]
		[PermissionSetAttribute (SecurityAction.LinkDemand)]
		public Enlistment EnlistDurable (Guid resourceManagerIdentifier,
			ISinglePhaseNotification singlePhaseNotification,
			EnlistmentOptions enlistmentOptions)
		{
			EnsureIncompleteCurrentScope ();
			if (pspe != null || Durables.Count > 0)
				throw new NotImplementedException ("DTC unsupported, multiple durable resource managers aren't supported.");

			if (enlistmentOptions != EnlistmentOptions.None)
				throw new NotImplementedException ("EnlistmentOptions other than None aren't supported");

			Durables.Add (singlePhaseNotification);

			/* FIXME: Enlistment ?? */
			return new Enlistment ();
		}

		public bool EnlistPromotableSinglePhase (
			IPromotableSinglePhaseNotification promotableSinglePhaseNotification)
		{
			EnsureIncompleteCurrentScope ();

			// The specs aren't entirely clear on whether we can have volatile RMs along with a PSPE, but
			// I'm assuming that yes based on: http://social.msdn.microsoft.com/Forums/br/windowstransactionsprogramming/thread/3df6d4d3-0d82-47c4-951a-cd31140950b3
			if (pspe != null || Durables.Count > 0)
				return false;

			pspe = promotableSinglePhaseNotification;
			pspe.Initialize();

			return true;
		}

		public void SetDistributedTransactionIdentifier (IPromotableSinglePhaseNotification promotableNotification, Guid distributedTransactionIdentifier)
		{
			throw new NotImplementedException ();
		}

 		public bool EnlistPromotableSinglePhase (IPromotableSinglePhaseNotification promotableSinglePhaseNotification, Guid promoterType)
		{
			throw new NotImplementedException ();
		}

		public byte[] GetPromotedToken ()
		{
			throw new NotImplementedException ();
		}

		public Guid PromoterType
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("EnlistmentOptions being ignored")]
		public Enlistment EnlistVolatile (
			IEnlistmentNotification enlistmentNotification,
			EnlistmentOptions enlistmentOptions)
		{
			return EnlistVolatileInternal (enlistmentNotification, enlistmentOptions);
		}

		[MonoTODO ("EnlistmentOptions being ignored")]
		public Enlistment EnlistVolatile (
			ISinglePhaseNotification singlePhaseNotification,
			EnlistmentOptions enlistmentOptions)
		{
			/* FIXME: Anything extra reqd for this? */
			return EnlistVolatileInternal (singlePhaseNotification, enlistmentOptions);
		}

		private Enlistment EnlistVolatileInternal (
			IEnlistmentNotification notification,
			EnlistmentOptions options)
		{
			EnsureIncompleteCurrentScope (); 
			/* FIXME: Handle options.EnlistDuringPrepareRequired */
			Volatiles.Add (notification);

			/* FIXME: Enlistment.. ? */
			return new Enlistment ();
		}

		[MonoTODO ("Only Local Transaction Manager supported. Cannot have more than 1 durable resource per transaction.")]
		[PermissionSetAttribute (SecurityAction.LinkDemand)]
		public Enlistment PromoteAndEnlistDurable (
			Guid manager,
			IPromotableSinglePhaseNotification promotableNotification,
			ISinglePhaseNotification notification,
			EnlistmentOptions options)
		{
			throw new NotImplementedException ("DTC unsupported, multiple durable resource managers aren't supported.");
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as Transaction);
		}

		// FIXME: Check whether this is correct (currently, GetHashCode() uses 'dependents' but this doesn't)
		private bool Equals (Transaction t)
		{
			if (ReferenceEquals (t, this))
				return true;
			if (ReferenceEquals (t, null))
				return false;
			return this.level == t.level &&
				this.info == t.info;
		}

		public static bool operator == (Transaction x, Transaction y)
		{
			if (ReferenceEquals (x, null))
				return ReferenceEquals (y, null);
			return x.Equals (y);
		}

		public static bool operator != (Transaction x, Transaction y)
		{
			return !(x == y);
		}

		public override int GetHashCode ()
		{
			return (int) level ^ info.GetHashCode () ^ dependents.GetHashCode ();
		}

		public void Rollback ()
		{
			Rollback (null);
		}

		public void Rollback (Exception e)
		{
			EnsureIncompleteCurrentScope ();
			Rollback (e, null);
		}

		internal void Rollback (Exception ex, object abortingEnlisted)
		{
			if (aborted)
			{
				FireCompleted ();
				return;
			}

			/* See test ExplicitTransaction7 */
			if (info.Status == TransactionStatus.Committed)
				throw new TransactionException ("Transaction has already been committed. Cannot accept any new work.");

			// Save thrown exception as 'reason' of transaction's abort.
			innerException = ex;

			SinglePhaseEnlistment e = new SinglePhaseEnlistment();
			foreach (IEnlistmentNotification prep in Volatiles)
				if (prep != abortingEnlisted)
					prep.Rollback (e);

			var durables = Durables;
			if (durables.Count > 0 && durables [0] != abortingEnlisted)
				durables [0].Rollback (e);

			if (pspe != null && pspe != abortingEnlisted)
				pspe.Rollback (e);

			Aborted = true;

			FireCompleted ();
		}

		bool Aborted {
			get { return aborted; }
			set {
				aborted = value;
				if (aborted)
					info.Status = TransactionStatus.Aborted;
			}
		}
		
		internal TransactionScope Scope {
			get { return scope; }
			set { scope = value; }
		}

		protected IAsyncResult BeginCommitInternal (AsyncCallback callback)
		{
			if (committed || committing)
				throw new InvalidOperationException ("Commit has already been called for this transaction.");

			this.committing = true;

			asyncCommit = new AsyncCommit (DoCommit);
			return asyncCommit.BeginInvoke (callback, null);
		}

		protected void EndCommitInternal (IAsyncResult ar)
		{
			asyncCommit.EndInvoke (ar);
		}

		internal void CommitInternal ()
		{
			if (committed || committing)
				throw new InvalidOperationException ("Commit has already been called for this transaction.");

			this.committing = true;

			try {
				DoCommit ();
			}
			catch (TransactionException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new TransactionAbortedException("Transaction failed", ex);
			}
		}
		
		private void DoCommit ()
		{
			/* Scope becomes null in TransactionScope.Dispose */
			if (Scope != null) {
				/* See test ExplicitTransaction8 */
				Rollback (null, null);
				CheckAborted ();
			}

			var volatiles = Volatiles;
			var durables = Durables;
			if (volatiles.Count == 1 && durables.Count == 0)
			{
				/* Special case */
				ISinglePhaseNotification single = volatiles[0] as ISinglePhaseNotification;
				if (single != null)
				{
					DoSingleCommit(single);
					Complete();
					return;
				}
			}

			if (volatiles.Count > 0)
				DoPreparePhase();

			if (durables.Count > 0)
				DoSingleCommit(durables[0]);

			if (pspe != null)
				DoSingleCommit(pspe);

			if (volatiles.Count > 0)
				DoCommitPhase();
			
			Complete();
		}

		private void Complete ()
		{
			committing = false;
			committed = true;

			if (!aborted)
				info.Status = TransactionStatus.Committed;

			FireCompleted ();
		}

		internal void InitScope (TransactionScope scope)
		{
			/* See test NestedTransactionScope10 */
			CheckAborted ();

			/* See test ExplicitTransaction6a */
			if (committed)
				throw new InvalidOperationException ("Commit has already been called on this transaction."); 

			Scope = scope;	
		}

		static void PrepareCallbackWrapper(object state)
		{
			PreparingEnlistment enlist = state as PreparingEnlistment;

			try
			{
				enlist.EnlistmentNotification.Prepare(enlist);
			}
			catch (Exception ex)
			{
				// Oops! Unhandled exception.. we should notify
				// to our caller thread that preparing has failed.
				// This usually happends when an exception is
				// thrown by code enlistment.Rollback() methods
				// executed inside prepare.ForceRollback(ex).
				enlist.Exception = ex;

				// Just in case enlistment did not call Prepared()
				// we need to manually set WH to avoid transaction
				// from failing due to transaction timeout.
				if (!enlist.IsPrepared)
					((ManualResetEvent)enlist.WaitHandle).Set();
			}
		}

		void DoPreparePhase ()
		{
			// Call prepare on all volatile managers.
			foreach (IEnlistmentNotification enlist in Volatiles)
			{
				PreparingEnlistment pe = new PreparingEnlistment (this, enlist);
				ThreadPool.QueueUserWorkItem (new WaitCallback(PrepareCallbackWrapper), pe);

				/* Wait (with timeout) for manager to prepare */
				TimeSpan timeout = Scope != null ? Scope.Timeout : TransactionManager.DefaultTimeout;

				// FIXME: Should we managers in parallel or on-by-one?
				if (!pe.WaitHandle.WaitOne(timeout, true))
				{
					this.Aborted = true;
					throw new TimeoutException("Transaction timedout");
				}

				if (pe.Exception != null)
				{
					innerException = pe.Exception;
					Aborted = true;
					break;
				}

				if (!pe.IsPrepared)
				{
					/* FIXME: if not prepared & !aborted as yet, then 
						this is inDoubt ? . For now, setting aborted = true */
					Aborted = true;
					break;
				}
			}			
			
			/* Either InDoubt(tmp) or Prepare failed and
			   Tx has rolledback */
			CheckAborted ();
		}

		void DoCommitPhase ()
		{
			foreach (IEnlistmentNotification enlisted in Volatiles) {
				Enlistment e = new Enlistment ();
				enlisted.Commit (e);
				/* Note: e.Done doesn't matter for volatile RMs */
			}
		}

		void DoSingleCommit (ISinglePhaseNotification single)
		{
			if (single == null)
				return;

			single.SinglePhaseCommit (new SinglePhaseEnlistment (this, single));
			CheckAborted ();
		}

		void DoSingleCommit (IPromotableSinglePhaseNotification single)
		{
			if (single == null)
				return;

			single.SinglePhaseCommit (new SinglePhaseEnlistment (this, single));
			CheckAborted ();
		}

		void CheckAborted ()
		{
			if (aborted)
				throw new TransactionAbortedException ("Transaction has aborted", innerException);
		}

		void FireCompleted ()
		{
			if (TransactionCompleted != null)
				TransactionCompleted (this, new TransactionEventArgs(this));
		}

		static void EnsureIncompleteCurrentScope ()
		{
			if (CurrentInternal == null)
				return;
			if (CurrentInternal.Scope != null && CurrentInternal.Scope.IsComplete)
				throw new InvalidOperationException ("The current TransactionScope is already complete");
		}
  }
}

