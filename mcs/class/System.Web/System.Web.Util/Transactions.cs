//
// System.Web.Util.Transactions.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.EnterpriseServices;

namespace System.Web.Util
{
	public class Transactions
	{
		public Transactions ()
		{
		}

		public static void InvokeTransacted (TransactedCallback callback, TransactionOption mode)
		{
			bool abortedTransaction = false;
			InvokeTransacted (callback, mode, ref abortedTransaction);
		}

		public static void InvokeTransacted (TransactedCallback callback, 
							TransactionOption mode, 
							ref bool transactionAborted)
		{
			throw new PlatformNotSupportedException ("Not supported on mono");
		}
	}
}
