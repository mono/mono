//
// TransactionEventArgs.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public class TransactionEventArgs : EventArgs
	{
		private Transaction transaction;

		public TransactionEventArgs()
		{
		}

		internal TransactionEventArgs(Transaction transaction)
			: this()
		{
			this.transaction = transaction;
		}

		public Transaction Transaction {
			get { return transaction; }
		}
	}
}

#endif
