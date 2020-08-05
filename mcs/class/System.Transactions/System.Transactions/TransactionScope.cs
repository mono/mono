//
// TransactionScope.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

using System.Threading;

using DTCOption = System.Transactions.EnterpriseServicesInteropOption;

namespace System.Transactions
{
	public sealed class TransactionScope : IDisposable
	{
		static TransactionOptions defaultOptions =
			new TransactionOptions (0, TransactionManager.DefaultTimeout);

		Timer scopeTimer;
		Transaction transaction;
		Transaction oldTransaction;
		TransactionScope parentScope;
		TimeSpan timeout;

		/* Num of non-disposed nested scopes */
		int nested;

		bool disposed;
		bool completed;
		bool aborted;
		bool isRoot;

		bool asyncFlowEnabled;

		public TransactionScope ()
			: this (TransactionScopeOption.Required,
				TransactionManager.DefaultTimeout)
		{
		}

		public TransactionScope(TransactionScopeAsyncFlowOption asyncFlowOption)
			: this(TransactionScopeOption.Required,
				TransactionManager.DefaultTimeout, asyncFlowOption)
		{
		}

		public TransactionScope (Transaction transactionToUse)
			: this (transactionToUse, TransactionManager.DefaultTimeout)
		{
		}

		public TransactionScope (Transaction transactionToUse,
			TimeSpan scopeTimeout)
			: this (transactionToUse, scopeTimeout, DTCOption.None)
		{
		}

		[MonoTODO ("EnterpriseServicesInteropOption not supported.")]
		public TransactionScope (Transaction transactionToUse,
			TimeSpan scopeTimeout, DTCOption interopOption)
		{
			Initialize (TransactionScopeOption.Required,
				transactionToUse, defaultOptions, interopOption, scopeTimeout, TransactionScopeAsyncFlowOption.Suppress);
		}

		public TransactionScope (TransactionScopeOption scopeOption)
			: this (scopeOption, TransactionManager.DefaultTimeout)
		{
		}

		public TransactionScope (TransactionScopeOption scopeOption,
			TimeSpan scopeTimeout)
			: this (scopeOption, scopeTimeout, TransactionScopeAsyncFlowOption.Suppress)
		{
		}

		public TransactionScope(TransactionScopeOption option, TransactionScopeAsyncFlowOption asyncFlow)
			: this(option, TransactionManager.DefaultTimeout, asyncFlow)
		{
		}

		public TransactionScope (TransactionScopeOption scopeOption,
			TimeSpan scopeTimeout, TransactionScopeAsyncFlowOption asyncFlow)
		{
			Initialize (scopeOption, null, defaultOptions,
				DTCOption.None, scopeTimeout, asyncFlow);
		}

		public TransactionScope (TransactionScopeOption scopeOption,
			TransactionOptions transactionOptions)
			: this (scopeOption, transactionOptions, DTCOption.None)
		{
		}

		[MonoTODO ("EnterpriseServicesInteropOption not supported")]
		public TransactionScope (TransactionScopeOption scopeOption,
			TransactionOptions transactionOptions,
			DTCOption interopOption)
		{
			Initialize (scopeOption, null, transactionOptions, interopOption,
				transactionOptions.Timeout, TransactionScopeAsyncFlowOption.Suppress);
		}

		public TransactionScope (Transaction transactionToUse,
			TransactionScopeAsyncFlowOption asyncFlowOption)
		{
			throw new NotImplementedException ();
		}

		public TransactionScope (Transaction transactionToUse,
			TimeSpan scopeTimeout,
			TransactionScopeAsyncFlowOption asyncFlowOption)
		{
			throw new NotImplementedException ();
		}

		public TransactionScope (TransactionScopeOption scopeOption,
			TransactionOptions transactionOptions,
			TransactionScopeAsyncFlowOption asyncFlowOption)
		{
			throw new NotImplementedException ();
		}

		void Initialize (TransactionScopeOption scopeOption,
			Transaction tx, TransactionOptions options,
			DTCOption interop, TimeSpan scopeTimeout, TransactionScopeAsyncFlowOption asyncFlow)
		{
			completed = false;
			isRoot = false;
			nested = 0;
			asyncFlowEnabled = asyncFlow == TransactionScopeAsyncFlowOption.Enabled;

			if (scopeTimeout < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException ("scopeTimeout");

			this.timeout = scopeTimeout;

			oldTransaction = Transaction.CurrentInternal;

			Transaction.CurrentInternal = transaction = InitTransaction (tx, scopeOption, options);
			if (transaction != null)
				transaction.InitScope (this);
			if (parentScope != null)
				parentScope.nested ++;
			if (timeout != TimeSpan.Zero)
				scopeTimer = new Timer (TransactionScope.TimerCallback, this, scopeTimeout, TimeSpan.Zero);
		}

		private static void TimerCallback (object state)
		{
			TransactionScope scope = state as TransactionScope;
			if (null == scope)
			{
				throw new TransactionException ("TransactionScopeTimerObjectInvalid", null);
			}

			scope.TimeoutScope ();
		}

		private void TimeoutScope()
		{
			if (!completed && transaction != null)
			{
				try
				{
					this.transaction.Rollback ();
					this.aborted = true;
				}
				catch (ObjectDisposedException ex) { }
				catch (TransactionException txEx) { }
			}
 		}

		Transaction InitTransaction (Transaction tx, TransactionScopeOption scopeOption, TransactionOptions options)
		{
			if (tx != null)
				return tx;
				
			if (scopeOption == TransactionScopeOption.Suppress) {
				if (Transaction.CurrentInternal != null)
					parentScope = Transaction.CurrentInternal.Scope;
				return null;
			}

			if (scopeOption == TransactionScopeOption.Required) {
				if (Transaction.CurrentInternal == null) {
					isRoot = true;
					return new Transaction (options.IsolationLevel);
				}

				parentScope = Transaction.CurrentInternal.Scope;
				return Transaction.CurrentInternal;
			}

			/* RequiresNew */
			if (Transaction.CurrentInternal != null)
				parentScope = Transaction.CurrentInternal.Scope;
			isRoot = true;
			return new Transaction (options.IsolationLevel);
        }

		public void Complete ()
		{
			if (completed)
				throw new InvalidOperationException ("The current TransactionScope is already complete. You should dispose the TransactionScope.");

			completed = true;
		}

		internal bool IsAborted {
			get { return aborted; }
		}

		internal bool IsDisposed {
			get { return disposed; }
		}

		internal bool IsComplete {
			get { return completed; }
		}

		internal TimeSpan Timeout
		{
			get { return timeout; }
		}

		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;

			if (parentScope != null)
				parentScope.nested --;

			if (nested > 0) {
				transaction.Rollback ();
				throw new InvalidOperationException ("TransactionScope nested incorrectly");
			}

			if (Transaction.CurrentInternal != transaction && !asyncFlowEnabled) {
				if (transaction != null)
					transaction.Rollback ();
				if (Transaction.CurrentInternal != null)
					Transaction.CurrentInternal.Rollback ();

				throw new InvalidOperationException ("Transaction.Current has changed inside of the TransactionScope");
			} 

			if (scopeTimer != null)
				scopeTimer.Dispose();

			if (asyncFlowEnabled) {
				if (oldTransaction != null)
					oldTransaction.Scope = parentScope;

				var variedTransaction = Transaction.CurrentInternal;

				if (transaction == null && variedTransaction == null)
					/* scope was not in a transaction, (Suppress) */
					return;

				variedTransaction.Scope = parentScope;
				Transaction.CurrentInternal = oldTransaction;

				transaction.Scope = null;

				if (IsAborted) {
					throw new TransactionAbortedException("Transaction has aborted");
				}
				else if (!IsComplete) {
					transaction.Rollback ();
					variedTransaction.Rollback();
					return;
				}

				if (!isRoot)
					/* Non-root scope has completed+ended */
					return;

				variedTransaction.CommitInternal();
				transaction.CommitInternal();
			} else {
				if (Transaction.CurrentInternal == oldTransaction && oldTransaction != null)
					oldTransaction.Scope = parentScope;

				Transaction.CurrentInternal = oldTransaction;

				if (transaction == null)
					/* scope was not in a transaction, (Suppress) */
					return;

				if (IsAborted) {
					transaction.Scope = null;
					throw new TransactionAbortedException("Transaction has aborted");
				}
				else if (!IsComplete)
				{
					transaction.Rollback();
					return;
				}

				if (!isRoot)
					/* Non-root scope has completed+ended */
					return;

				transaction.CommitInternal();

				transaction.Scope = null;
			}
		}


	}
}
