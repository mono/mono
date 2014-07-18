//
// TransactionManagerCommunicationException.cs
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
	public class TransactionManagerCommunicationException : TransactionException
	{
		protected TransactionManagerCommunicationException ()
		{
		}

		public TransactionManagerCommunicationException (string message)
			: base (message)
		{
		}

		public TransactionManagerCommunicationException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

#if !WINDOWS_PHONE && !NETFX_CORE
		protected TransactionManagerCommunicationException (SerializationInfo info,
			StreamingContext context)
			: base (info, context)
		{
		}
#endif
	}
}

#endif
