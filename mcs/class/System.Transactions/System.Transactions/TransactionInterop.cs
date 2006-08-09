//
// TransactionInterop.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

// OK, everyone knows that am not interested in DTC support ;-)

namespace System.Transactions
{
	[MonoTODO]
	public static class TransactionInterop
	{
		[MonoTODO]
		public static IDtcTransaction GetDtcTransaction (Transaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte [] GetExportCookie (Transaction transaction,
			byte [] exportCookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Transaction GetTransactionFromDtcTransaction (
			IDtcTransaction dtc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Transaction GetTransactionFromExportCookie (
			byte [] exportCookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Transaction GetTransactionFromTransmitterPropagationToken (byte [] token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte [] GetTransmitterPropagationToken (
			Transaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte [] GetWhereabouts ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
