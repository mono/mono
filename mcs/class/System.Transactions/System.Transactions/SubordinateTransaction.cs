//
// SubordinateTransaction.cs
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
#if !WINDOWS_STORE_APP
	[Serializable]
#endif
	public sealed class SubordinateTransaction : Transaction
	{
		public SubordinateTransaction (IsolationLevel level,
			ISimpleTransactionSuperior superior)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
