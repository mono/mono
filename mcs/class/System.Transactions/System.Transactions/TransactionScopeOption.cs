//
// TransactionScopeOption.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public enum TransactionScopeOption {
		Required,
		RequiresNew,
		Suppress,
	}
}

#endif
