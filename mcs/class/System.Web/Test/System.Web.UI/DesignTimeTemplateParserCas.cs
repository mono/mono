//
// DesignTimeTemplateParserCas.cs 
//	- CAS unit tests for System.Web.UI.DesignTimeTemplateParser
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
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class DesignTimeTemplateParserCas : AspNetHostingMinimal {

		private DesignTimeParseData dtpd;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			dtpd = new DesignTimeParseData (null, "parseText");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ParseControl_Deny_ControlThread ()
		{
			DesignTimeTemplateParser.ParseControl (dtpd);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ParseControl_Deny_UnmanagedCode ()
		{
			DesignTimeTemplateParser.ParseControl (dtpd);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlThread = true, UnmanagedCode = true)]
#if ONLY_1_1
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void ParseControl_PermitOnly_ControlThreadUnmanagedCode ()
		{
			DesignTimeTemplateParser.ParseControl (dtpd);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ParseTemplate_Deny_ControlThread ()
		{
			DesignTimeTemplateParser.ParseTemplate (dtpd);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ParseTemplate_Deny_UnmanagedCode ()
		{
			DesignTimeTemplateParser.ParseTemplate (dtpd);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlThread = true, UnmanagedCode = true)]
#if ONLY_1_1
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void ParseTemplate_PermitOnly_ControlThreadUnmanagedCode ()
		{
			DesignTimeTemplateParser.ParseTemplate (dtpd);
		}
#if NET_2_0
		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ParseControls_Deny_ControlThread ()
		{
			DesignTimeTemplateParser.ParseControls (dtpd);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ParseControls_Deny_UnmanagedCode ()
		{
			DesignTimeTemplateParser.ParseControls (dtpd);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlThread = true, UnmanagedCode = true)]
		public void ParseControls_PermitOnly_ControlThreadUnmanagedCode ()
		{
			DesignTimeTemplateParser.ParseControls (dtpd);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ParseTheme_Deny_ControlThread ()
		{
			try {
				DesignTimeTemplateParser.ParseTheme (null, "theme", "path");
			}
			catch (Exception) {
				// security exception gets hidden :-(
				Assert.Ignore ("security exception gets hidden :-(");
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlThread = true)]
		public void ParseTheme_PermitOnly_ControlThread ()
		{
			DesignTimeTemplateParser.ParseTheme (null, "theme", "path");
		}
#endif

		// LinkDemand

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true, ControlThread = true)]
		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			try {
				// static class 2.0 / no public ctor before (1.x)
				MethodInfo mi = this.Type.GetMethod ("ParseTemplate");
				Assert.IsNotNull (mi, "ParseTemplate");
				return mi.Invoke (null, new object[1] { dtpd });
			}
			catch (TargetInvocationException tie) {
#if ONLY_1_1
				if (tie.InnerException is NullReferenceException)
					return String.Empty;
#endif
				throw tie;
			}
		}

		public override Type Type {
			get { return typeof (DesignTimeTemplateParser); }
		}
	}
}
