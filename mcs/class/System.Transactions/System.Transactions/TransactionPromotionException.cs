//
// TransactionPromotionException.cs
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
	public class TransactionPromotionException : TransactionException
	{
		protected TransactionPromotionException ()
		{
		}

		public TransactionPromotionException (string message)
			: base (message)
		{
		}

		public TransactionPromotionException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected TransactionPromotionException (SerializationInfo info,
			StreamingContext context)
			: base (info, context)
		{
		}
	}
}

#endif
