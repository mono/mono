//
// IDtcTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

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
		void Abort (IntPtr manager, int whatever, int whatever2);

		void Commit (int whatever, int whatever2, int whatever3);

		void GetTransactionInfo (IntPtr whatever);
	}
}

#endif
