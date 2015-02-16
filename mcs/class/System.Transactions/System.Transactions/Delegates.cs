//
// Delegates.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


namespace System.Transactions
{
	public delegate Transaction HostCurrentTransactionCallback ();
	public delegate void TransactionCompletedEventHandler (object o,
		TransactionEventArgs e);
	public delegate void TransactionStartedEventHandler (object o,
		TransactionEventArgs e);
}

