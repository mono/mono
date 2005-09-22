//
// TransactionManager.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//
using System.Collections;
using System.Collections.Specialized;

#if NET_2_0

namespace System.Transactions
{
	public static class TransactionManager
	{
		// it could contain both Transaction and non-tx (Suppress).
		static Stack tx_states = new Stack ();

		[MonoTODO]
		public static TimeSpan DefaultTimeout {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static HostCurrentTransactionCallback HostCurrentCallback {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static TimeSpan MaximumTimeout {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static void RecoveryComplete (Guid manager)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Enlistment Reenlist (Guid manager,
			byte[] recoveryInfo,
			IEnlistmentNotification notification)
		{
			throw new NotImplementedException ();
		}

		public static event TransactionStartedEventHandler
			DistributedTransactionStarted;

		// internals
		internal static Transaction Current {
			get {
				if (tx_states.Count == 0)
					return null;
				return tx_states.Peek () as Transaction;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		internal static void BeginScope (TransactionScope scope)
		{
			throw new NotImplementedException ();
		}

		internal static void EndScope (TransactionScope scope)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
