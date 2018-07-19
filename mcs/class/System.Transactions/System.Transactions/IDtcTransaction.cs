//
// IDtcTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


// Well, am not interested in implementing DTC support...
using System;
using System.Runtime.InteropServices;

namespace System.Transactions
{
	//[ComImport]
	// [Guid (whatever)]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDtcTransaction
	{
		void Abort (IntPtr reason, int retaining, int async);

		void Commit (int retaining, int commitType, int reserved);

		void GetTransactionInfo (IntPtr transactionInformation);
	}
}

