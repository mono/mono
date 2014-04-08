//
// TransactionInDoubtException.cs
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
#if !WINDOWS_PHONE && !NETFX_CORE
	[Serializable]
#endif
	public class TransactionInDoubtException : TransactionException
	{
		protected TransactionInDoubtException ()
		{
		}

		public TransactionInDoubtException (string message)
			: base (message)
		{
		}

		public TransactionInDoubtException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

#if !WINDOWS_PHONE && !NETFX_CORE
		protected TransactionInDoubtException (SerializationInfo info,
			StreamingContext context)
			: base (info, context)
		{
		}
#endif
	}
}

#endif
