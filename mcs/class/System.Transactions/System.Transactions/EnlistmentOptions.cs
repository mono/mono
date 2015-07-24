//
// EnlistmentOptions.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


namespace System.Transactions
{
	[Flags]
	public enum EnlistmentOptions {
		None,
		EnlistDuringPrepareRequired,
	}
}

