//
// System.Web.Util.Transactions.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.EnterpriseServices;
using System.Security.Permissions;

namespace System.Web.Util
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
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

		[MonoTODO ("Not implemented, not supported by Mono")]
		public static void InvokeTransacted (TransactedCallback callback, 
							TransactionOption mode, 
							ref bool transactionAborted)
		{
			// note: this is the documented exception for (Windows) OS prior to NT
			// so in this case we won't throw a NotImplementedException
			throw new PlatformNotSupportedException ("Not supported on mono");
		}
	}
}
