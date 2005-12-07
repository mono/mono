//
// ControlCas.cs - CAS unit tests for System.Web.UI.Control
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class ControlCas : AspNetHostingMinimal {

		private Control control;
		private HtmlTextWriter writer;
		private Page page;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			control = new Control ();
			writer = new HtmlTextWriter (new StringWriter ());
			page = new Page ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			Control c = new Control ();
			Assert.IsNull (c.ClientID, "ClientID");
			Assert.IsNotNull (c.Controls, "Controls");
			c.EnableViewState = true;
			Assert.IsTrue (c.EnableViewState, "EnableViewState");
			c.ID = "mono";
			Assert.AreEqual ("mono", c.ID, "ID");
			Assert.IsNull (c.NamingContainer, "NamingContainer");
			Assert.IsNull (c.Page, "Page");
			Assert.IsNull (c.Parent, "Parent");
			Assert.IsNull (c.Site, "Site");
			Assert.AreEqual ("mono", c.UniqueID, "UniqueID");
			Assert.IsTrue (c.Visible, "Visible");
#if NET_2_0
			c.AppRelativeTemplateSourceDirectory = String.Empty;
			Assert.AreEqual (String.Empty, c.AppRelativeTemplateSourceDirectory, "AppRelativeTemplateSourceDirectory");
			c.EnableTheming = true;
			Assert.IsTrue (c.EnableTheming, "EnableTheming");
			c.SkinID = String.Empty;
			Assert.AreEqual (String.Empty, c.SkinID, "SkinID");
			c.TemplateControl = null;
			Assert.IsNull (c.TemplateControl, "TemplateControl");
			Assert.AreEqual (String.Empty, c.TemplateSourceDirectory, "TemplateSourceDirectory");
#endif
		}

		private void SetRenderMethodDelegate (HtmlTextWriter writer, Control control)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			Control c = new Control ();

			c.DataBind ();
			Assert.IsNull (c.FindControl ("mono"), "FindControl");

			Assert.IsFalse (c.HasControls (), "HasControls");
			c.RenderControl (writer);
			Assert.IsNotNull (c.ResolveUrl (String.Empty), "ResolveUrl");
			c.SetRenderMethodDelegate (new RenderMethod (SetRenderMethodDelegate));
#if NET_2_0
			c.ApplyStyleSheetSkin (page);
			Assert.IsNotNull (c.ResolveClientUrl (String.Empty), "ResolveClientUrl");
#endif
			c.Dispose ();
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Focus_Deny_Unrestricted ()
		{
			Control c = new Control ();
			page.Controls.Add (c);
			c.Focus ();
			// normal, no forms on page
		}
#endif

		private void Handler (object sender, EventArgs e)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Events_Deny_Unrestricted ()
		{
			Control c = new Control ();
			c.DataBinding += new EventHandler (Handler);
			c.Disposed += new EventHandler (Handler);
			c.Init += new EventHandler (Handler);
			c.Load += new EventHandler (Handler);
			c.PreRender += new EventHandler (Handler);
			c.Unload += new EventHandler (Handler);

			c.DataBinding -= new EventHandler (Handler);
			c.Disposed -= new EventHandler (Handler);
			c.Init -= new EventHandler (Handler);
			c.Load -= new EventHandler (Handler);
			c.PreRender -= new EventHandler (Handler);
			c.Unload -= new EventHandler (Handler);
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (Control); }
		}
	}
}
