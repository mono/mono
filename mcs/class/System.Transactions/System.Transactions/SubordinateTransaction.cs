//
// SubordinateTransaction.cs
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
	public sealed class SubordinateTransaction : Transaction
	{
		public SubordinateTransaction (IsolationLevel level,
			ISimpleTransactionSuperior superior)
		{
			throw new NotImplementedException ();
		}
	}
}

