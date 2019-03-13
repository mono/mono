//
// TemplateControlCas.cs - CAS unit tests for System.Web.UI.TemplateControl
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
using System.IO;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	class NonAbstractTemplateControl : TemplateControl {

		public NonAbstractTemplateControl ()
		{
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class TemplateControlCas {

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LoadControl_Deny_Unrestricted ()
		{
			NonAbstractTemplateControl tc = new NonAbstractTemplateControl ();
			tc.LoadControl ((string)null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LoadTemplate_Deny_Unrestricted ()
		{
			NonAbstractTemplateControl tc = new NonAbstractTemplateControl ();
			tc.LoadTemplate ((string)null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ParseControl_Deny_Unrestricted ()
		{
			NonAbstractTemplateControl tc = new NonAbstractTemplateControl ();
			try {
				tc.ParseControl (null);
			}
			catch (NullReferenceException) {
				throw;
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReadStringResource_Deny_Unrestricted ()
		{
			try {
				TemplateControl.ReadStringResource (null);
			}
			catch (TypeInitializationException) {
				Assert.Ignore ("exception during initialization");
			}
		}

		private void Handler (object sender, EventArgs e)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Events_Deny_Unrestricted ()
		{
			NonAbstractTemplateControl tc = new NonAbstractTemplateControl ();
			tc.AbortTransaction += new EventHandler (Handler);
			tc.CommitTransaction += new EventHandler (Handler);
			tc.Error += new EventHandler (Handler);

			tc.AbortTransaction -= new EventHandler (Handler);
			tc.CommitTransaction -= new EventHandler (Handler);
			tc.Error -= new EventHandler (Handler);
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (TypeInitializationException))]
		public void IFilterResolutionService_Deny_Unrestricted ()
		{
			IFilterResolutionService frs = new NonAbstractTemplateControl ();
			try {
				Assert.AreEqual (0, frs.CompareFilters (String.Empty, String.Empty), "CompareFilters");
			}
			catch (NotImplementedException) {
				// mono
			}
			try {
				Assert.IsFalse (frs.EvaluateFilter (String.Empty), "EvaluateFilter");
			}
			catch (NotImplementedException) {
				// mono
			}
		}
	}
}
