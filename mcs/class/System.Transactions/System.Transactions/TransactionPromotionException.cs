//
// TransactionPromotionException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

using System.Runtime.Serialization;

namespace System.Transactions
{
	[Serializable]
	public class TransactionPromotionException : TransactionException
	{
		public TransactionPromotionException ()
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

