//
// Delegates.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public delegate Transaction HostCurrentTransactionCallback ();
	public delegate void TransactionCompletedEventHandler (object o,
		TransactionEventArgs e);
	public delegate void TransactionStartedEventHandler (object o,
		TransactionEventArgs e);
}

#endif
