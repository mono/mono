//
// IsolationLevel.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


namespace System.Transactions
{
	public enum IsolationLevel {
		Serializable,
		RepeatableRead,
		ReadCommitted,
		ReadUncommitted,
		Snapshot,
		Chaos,
		Unspecified
	}
}

