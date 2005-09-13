//
// ProcessModelInfoCas.cs - CAS unit tests for System.Web.ProcessModelInfo
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
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class ProcessModelInfoCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0 ()
		{
			new ProcessModelInfo ();
			// everything else is static so...
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.High)]
		[ExpectedException (typeof (SecurityException))]
		public void GetCurrentProcessInfo_Deny_High ()
		{
			ProcessModelInfo.GetCurrentProcessInfo ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.High)]
		public void GetCurrentProcessInfo_PermitOnly_High ()
		{
			try {
				ProcessModelInfo.GetCurrentProcessInfo ();
			}
			catch (HttpException) {
				// ms 2.x - kind of expected (as we're not running ASP.NET)
			}
			catch (TypeInitializationException) {
				// ms 1.x - fails initializing HttpRuntime
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.High)]
		[ExpectedException (typeof (SecurityException))]
		public void GetHistory_Deny_High ()
		{
			ProcessModelInfo.GetHistory (0);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.High)]
		public void GetHistory_PermitOnly_High ()
		{
			try {
				ProcessModelInfo.GetHistory (0);
			}
			catch (HttpException) {
				// ms 2.x - kind of expected (as we're not running ASP.NET)
			}
			catch (NotImplementedException) {
				// mono
			}
			catch (TypeInitializationException) {
				// ms 1.x - fails initializing HttpRuntime
			}
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (ProcessModelInfo); }
		}
	}
}
