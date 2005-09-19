//
// UserControlCas.cs 
//	- CAS unit tests for System.Web.UI.UserControlCas
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
using System.Collections;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class UserControlCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			UserControl uc = new UserControl ();
			try {
				Assert.IsNull (uc.Application, "Application");
			}
			catch (NullReferenceException) {
				// ms 2.0 rc
			}
			Assert.IsNotNull (uc.Attributes, "Attributes");
			try {
				Assert.IsNull (uc.Cache, "Cache");
			}
			catch (NullReferenceException) {
				// ms 2.0 rc
			}
			try {
				Assert.IsFalse (uc.IsPostBack, "IsPostBack");
			}
			catch (NullReferenceException) {
				// ms 2.0 rc
			}
			try {
				Assert.IsNull (uc.Request, "Request");
			}
			catch (NullReferenceException) {
				// ms 2.0 rc
			}
			try {
				Assert.IsNull (uc.Response, "Response");
			}
			catch (NullReferenceException) {
				// ms 2.0 rc
			}
			try {
				Assert.IsNull (uc.Server, "Server");
			}
			catch (NullReferenceException) {
				// ms 2.0 rc
			}
			try {
				Assert.IsNull (uc.Session, "Session");
			}
			catch (NullReferenceException) {
				// ms 2.0 rc
			}
			try {
				Assert.IsNull (uc.Trace, "Trace");
			}
			catch (NullReferenceException) {
				// ms 2.0 rc
			}
#if NET_2_0
			try {
				Assert.IsNotNull (uc.CachePolicy, "CachePolicy");
			}
			catch (NotImplementedException) {
				// mono
			}
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void IUserControlDesignerAccessor_Deny_Unrestricted ()
		{
			IUserControlDesignerAccessor ucda = new UserControl ();
			ucda.InnerText = "mono";
			Assert.AreEqual ("mono", ucda.InnerText, "InnerText");
			ucda.TagName = "monkey";
			Assert.AreEqual ("monkey", ucda.TagName, "TagName");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void InitializeAsUserControl_Deny_Unrestricted ()
		{
			UserControl uc = new UserControl ();
			try {
				uc.InitializeAsUserControl (new Page ());
			}
			catch (TypeInitializationException tie) {
				// 2.0 - error initializing HttpRuntime
				Console.WriteLine (tie.InnerException);
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void MapPath_Deny_Unrestricted ()
		{
			new UserControl ().MapPath ("/");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void IAttributeAccessor_Deny_Unrestricted ()
		{
			IAttributeAccessor aa = new UserControl ();
			Assert.IsNull (aa.GetAttribute (null));
			aa.SetAttribute ("name", "value");
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (UserControl); }
		}
	}
}
