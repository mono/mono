//
// TransactionScope.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
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
		TimeSpan timeout;
		bool disposed;

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

		[MonoTODO]
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

		[MonoTODO]
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

		[MonoTODO]
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
			this.transaction = transaction;
			this.timeout = timeout;
			TransactionManager.BeginScope (this);
		}

		[MonoTODO]
		public void Complete ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose ()
		{
			if (!disposed) {
				disposed = true;
				TransactionManager.EndScope (this);
			}
		}
	}
}

#endif
