//
// TransactionsCas.cs - CAS unit tests for System.Web.Util.Transactions
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using NUnit.Framework;

using System;
using System.EnterpriseServices;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.Util;

namespace MonoCasTests.System.Web.Util {

	[TestFixture]
	[Category ("CAS")]
	public class TransactionsCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor_Deny_Unrestricted ()
		{
			new Transactions ();
		}

		private void Callback ()
		{
		}

		[Test]
		// LAMESPEC - documented as AspNetHostingPermission, Level Medium
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void InvokeTransacted2_Deny_Unrestricted ()
		{
			try {
				Transactions.InvokeTransacted (new TransactedCallback (Callback), TransactionOption.Required);
			}
#if ONLY_1_1
			catch (TypeInitializationException) {
				// under 1.x, this _can_ trigger initialization of HttpRuntime
			}
#endif
			catch (PlatformNotSupportedException) {
				// Mono and Windows prior to NT
			}
		}

		[Test]
		// LAMESPEC - documented as AspNetHostingPermission, Level Medium
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void InvokeTransacted3_Deny_Unrestricted ()
		{
			try {
				bool aborted = false;
				Transactions.InvokeTransacted (new TransactedCallback (Callback), TransactionOption.Required, ref aborted);
			}
#if ONLY_1_1
			catch (TypeInitializationException) {
				// under 1.x, this _can_ trigger initialization of HttpRuntime
			}
#endif
			catch (PlatformNotSupportedException) {
				// Mono and Windows prior to NT
			}
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (Transactions); }
		}
	}
}
