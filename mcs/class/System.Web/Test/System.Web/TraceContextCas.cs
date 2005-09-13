//
// TraceContextCas.cs - CAS unit tests for System.Web.TraceContext
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class TraceContextCas : AspNetHostingMinimal {

		private HttpContext context;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			context = new HttpContext (null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor_Properties_Events ()
		{
			TraceContext tc = new TraceContext (context);

			tc.IsEnabled = false;
			Assert.IsFalse (tc.IsEnabled, "IsEnabled");

			Assert.AreEqual (TraceMode.SortByTime, tc.TraceMode, "TraceMode");
			tc.TraceMode = TraceMode.Default;
#if NET_2_0
			tc.TraceFinished += new TraceContextEventHandler (Handler);
			tc.TraceFinished -= new TraceContextEventHandler (Handler);
#endif
		}
#if NET_2_0
		private void Handler (object sender, TraceContextEventArgs e)
		{
		}
#endif
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
				// expected (as we're not running ASP.NET)
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
				// expected (as we're not running ASP.NET)
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (HttpContext) });
			Assert.IsNotNull (ci, ".ctor(HttpContext)");
			return ci.Invoke (new object[1] { context });
		}

		public override Type Type {
			get { return typeof (TraceContext); }
		}
	}
}
