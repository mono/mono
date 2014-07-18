//
// TransactionException.cs
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
	public class TransactionException : 
#if !NETFX_CORE
		SystemException
#else
		Exception
#endif
	{
		protected TransactionException ()
		{
		}

		public TransactionException (string message)
			: base (message)
		{
		}

		public TransactionException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

#if !WINDOWS_PHONE && !NETFX_CORE
		protected TransactionException (SerializationInfo info,
			StreamingContext context)
			: base (info, context)
		{
		}
#endif
	}
}

#endif
