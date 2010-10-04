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
<<<<<<< HEAD
	{
		private Transaction transaction;

		public TransactionEventArgs()
		{
		}

		internal TransactionEventArgs(Transaction transaction)
			: this()
		{
			this.transaction = transaction;
=======
	{
		private Transaction transaction;

		public TransactionEventArgs()
		{
		}

		internal TransactionEventArgs(Transaction transaction)
			: this()
		{
			this.transaction = transaction;
>>>>>>> 3d577e4060dccd67d1450b790ef12bc0781198be
		}

		public Transaction Transaction {
			get { return transaction; }
		}
	}
}

#endif
