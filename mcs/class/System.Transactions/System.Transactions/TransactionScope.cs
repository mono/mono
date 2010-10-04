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

#if NET_2_0

using DTCOption = System.Transactions.EnterpriseServicesInteropOption;

namespace System.Transactions
{
	public sealed class TransactionScope : IDisposable
	{
		static TransactionOptions defaultOptions =
			new TransactionOptions (0, TransactionManager.DefaultTimeout);

		Transaction transaction;
		Transaction oldTransaction;
		TransactionScope parentScope;
		TimeSpan timeout;

		/* Num of non-disposed nested scopes */
		int nested;

		bool disposed;
		bool completed;
		bool isRoot;

		public TransactionScope ()
			: this (TransactionScopeOption.Required,
				TransactionManager.DefaultTimeout)
		{
		}

		public TransactionScope (Transaction transaction)
			: this (transaction, TransactionManager.DefaultTimeout)
		{
		}

		public TransactionScope (Transaction transaction,
			TimeSpan timeout)
			: this (transaction, timeout, DTCOption.None)
		{
		}

		[MonoTODO ("EnterpriseServicesInteropOption not supported.")]
		public TransactionScope (Transaction transaction,
			TimeSpan timeout, DTCOption opt)
		{
			Initialize (TransactionScopeOption.Required,
				transaction, defaultOptions, opt, timeout);
		}

		public TransactionScope (TransactionScopeOption option)
			: this (option, TransactionManager.DefaultTimeout)
		{
		}

		[MonoTODO ("No TimeoutException is thrown")]
		public TransactionScope (TransactionScopeOption option,
			TimeSpan timeout)
		{
			Initialize (option, null, defaultOptions,
				DTCOption.None, timeout);
		}

		public TransactionScope (TransactionScopeOption scopeOption,
			TransactionOptions options)
			: this (scopeOption, options, DTCOption.None)
		{
		}

		[MonoTODO ("EnterpriseServicesInteropOption not supported")]
		public TransactionScope (TransactionScopeOption scopeOption,
			TransactionOptions options,
			DTCOption opt)
		{
			Initialize (scopeOption, null, options, opt,
				TransactionManager.DefaultTimeout);
		}

		void Initialize (TransactionScopeOption scopeOption,
			Transaction tx, TransactionOptions options,
			DTCOption interop, TimeSpan timeout)
		{
			completed = false;
			isRoot = false;
			nested = 0;
			this.timeout = timeout;

			oldTransaction = Transaction.CurrentInternal;

			Transaction.CurrentInternal = transaction = InitTransaction (tx, scopeOption);
			if (transaction != null)
				transaction.InitScope (this);
			if (parentScope != null)
				parentScope.nested ++;
		}

		Transaction InitTransaction (Transaction tx, TransactionScopeOption scopeOption)
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
					return new Transaction ();
				}

				parentScope = Transaction.CurrentInternal.Scope;
				return Transaction.CurrentInternal;
			}

			/* RequiresNew */
			if (Transaction.CurrentInternal != null)
				parentScope = Transaction.CurrentInternal.Scope;
			isRoot = true;
			return new Transaction ();
		}

		public void Complete ()
		{
			if (completed)
				throw new InvalidOperationException ("The current TransactionScope is already complete. You should dispose the TransactionScope.");

			completed = true;
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

			if (Transaction.CurrentInternal != transaction) {
				if (transaction != null)
					transaction.Rollback ();
				if (Transaction.CurrentInternal != null)
					Transaction.CurrentInternal.Rollback ();

				throw new InvalidOperationException ("Transaction.Current has changed inside of the TransactionScope");
			}

			if (Transaction.CurrentInternal == oldTransaction && oldTransaction != null)
				oldTransaction.Scope = parentScope;

			Transaction.CurrentInternal = oldTransaction;

			if (transaction == null)
				/* scope was not in a transaction, (Suppress) */
				return;

			transaction.Scope = null;

			if (!IsComplete) {
				transaction.Rollback ();
				return;
			}

			if (!isRoot)
				/* Non-root scope has completed+ended */
				return;

			transaction.CommitInternal ();
		}


	}
}

#endif
