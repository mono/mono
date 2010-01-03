//
// TransactionAbortedException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0
using System.Runtime.Serialization;

namespace System.Transactions
{
	[Serializable]
	public class TransactionAbortedException : TransactionException
	{
		public TransactionAbortedException ()
		{
		}

		public TransactionAbortedException (string message)
			: base (message)
		{
		}

		public TransactionAbortedException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected TransactionAbortedException (SerializationInfo info,
			StreamingContext context)
			: base (info, context)
		{
		}
	}
}

#endif
