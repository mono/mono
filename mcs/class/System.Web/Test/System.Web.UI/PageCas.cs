//
// PageCas.cs - CAS unit tests for System.Web.UI.Page
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
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class PageCas : AspNetHostingMinimal {

		private Control control;
		private Page page;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			control = new Control ();
			control.ID = "mono";
			page = new Page ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			Page p = new Page ();
			Assert.IsNull (p.Application, "Application");
			p.ClientTarget = "mono";
			Assert.AreEqual ("mono", p.ClientTarget, "ClientTarget");
			p.EnableViewState = true;
			Assert.IsTrue (p.EnableViewState, "EnableViewState");
			p.ErrorPage = "error.html";
			Assert.AreEqual ("error.html", p.ErrorPage, "ErrorPage");
			p.ID = "mono";
			Assert.AreEqual ("mono", p.ID, "ID");
			Assert.IsFalse (p.IsPostBack, "IsPostBack");
			Assert.IsFalse (p.IsReusable, "IsReusable");
			p.SmartNavigation = false;
			Assert.IsFalse (p.SmartNavigation, "SmartNavigation");
			Assert.IsNotNull (p.Validators, "Validators");
			p.ViewStateUserKey = "mono";
			Assert.AreEqual ("mono", p.ViewStateUserKey, "ViewStateUserKey");
			p.Visible = true;
			Assert.IsTrue (p.Visible, "Visible");
#if NET_2_0
			Assert.IsNotNull (p.ClientScript, "ClientScript");
//			p.CodePage = 0;
//			Assert.AreEqual (0, p.CodePage, "CodePage");
//			p.ContentType = "mono";
//			Assert.AreEqual ("mono", p.ContentType, "ContentType");
			Assert.IsNotNull (p.Culture, "Culture");
			Assert.IsNull (p.Form, "Form");
			Assert.IsNull (p.Header, "Header");
			Assert.IsFalse (p.IsCallback, "IsCallback");
			Assert.IsFalse (p.IsCrossPagePostBack, "IsCrossPagePostBack");
			Assert.IsTrue (p.LCID != 0, "LCID");
			p.MasterPageFile = String.Empty;
			Assert.AreEqual (String.Empty, p.MasterPageFile, "MasterPageFile");
			Assert.IsNull (p.Master, "Master");
			Assert.IsNull (p.PageAdapter, "PageAdapter");
			Assert.IsNull (p.PreviousPage, "PreviousPage");
//			p.ResponseEncoding = Encoding.UTF8.WebName;
//			Assert.AreEqual (Encoding.UTF8.WebName, p.ResponseEncoding, "ResponseEncoding");
			p.UICulture = "en-us";
			Assert.IsNotNull (p.UICulture, "UICulture");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (HttpException))]
		public void Cache_Deny_Unrestricted ()
		{
			Assert.IsNotNull (new Page ().Cache, "Cache");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (HttpException))]
		public void IsValid_Deny_Unrestricted ()
		{
			Assert.IsFalse (new Page ().IsValid, "IsValid");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (HttpException))]
		public void Request_Deny_Unrestricted ()
		{
			Assert.IsNotNull (new Page ().Request, "Request");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (HttpException))]
		public void Response_Deny_Unrestricted ()
		{
			Assert.IsNotNull (new Page ().Response, "Response");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void Server_Deny_Unrestricted ()
		{
			Assert.IsNotNull (new Page ().Server, "Server");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (HttpException))]
		public void Session_Deny_Unrestricted ()
		{
			Assert.IsNotNull (new Page ().Session, "Session");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void Trace_Deny_Unrestricted ()
		{
			Assert.IsNotNull (new Page ().Trace, "Trace");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void User_Deny_Unrestricted ()
		{
			Assert.IsNotNull (new Page ().User, "User");
		}
#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (HttpException))]
		public void Buffer_set_Deny_Unrestricted ()
		{
			page.Buffer = true;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (HttpException))]
		public void Buffer_get_Deny_Unrestricted ()
		{
			Assert.IsTrue (page.Buffer, "Buffer");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Culture_Deny_ControlThread ()
		{
			page.Culture = "fr-ca";
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlThread = true)]
		public void Culture_PermitOnly_ControlThread ()
		{
			page.Culture = "fr-ca";
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LCID_Deny_ControlThread ()
		{
			page.LCID = 0x409;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlThread = true)]
		public void LCID_PermitOnly_ControlThread ()
		{
			page.LCID = 0x409;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void TraceEnabled_set_Deny_Unrestricted ()
		{
			page.TraceEnabled = false;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void TraceEnabled_get_Deny_Unrestricted ()
		{
			Assert.IsFalse (page.TraceEnabled, "TraceEnabled");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void TraceModeValue_set_Deny_Unrestricted ()
		{
			page.TraceModeValue = TraceMode.Default;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void TraceModeValue_get_Deny_Unrestricted ()
		{
			Assert.AreEqual (TraceMode.Default, page.TraceModeValue, "TraceModeValue");
		}
#endif
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			Page p = new Page ();
			p.DesignerInitialize ();
			Assert.IsNotNull (p.GetPostBackClientEvent (control, "mono"), "GetPostBackClientEvent");
			Assert.IsNotNull (p.GetPostBackClientHyperlink (control, "mono"), "GetPostBackClientHyperlink");
			Assert.IsNotNull (p.GetPostBackEventReference (control), "GetPostBackEventReference(control)");
			Assert.IsNotNull (p.GetPostBackEventReference (control, "mono"), "GetPostBackEventReference(control,string)");
			Assert.AreEqual (0, p.GetTypeHashCode (), "GetTypeHashCode");
			Assert.IsFalse (p.IsClientScriptBlockRegistered ("mono"), "IsClientScriptBlockRegistered");
			Assert.IsFalse (p.IsStartupScriptRegistered ("mono"), "IsStartupScriptRegistered");
			p.RegisterArrayDeclaration ("arrayname", "value");
			p.RegisterClientScriptBlock ("key", "script");
			p.RegisterHiddenField ("name", "hidden");
			p.RegisterOnSubmitStatement ("key", "script");
			p.RegisterRequiresPostBack (new HtmlTextArea ());
			p.RegisterRequiresRaiseEvent (new HtmlAnchor ());
			p.RegisterStartupScript ("key", "script");
			p.Validate ();
			p.VerifyRenderingInServerForm (control);
#if NET_2_0
			p.Controls.Add (control);
			Assert.IsNotNull (p.FindControl ("mono"), "FindControl");
			p.RegisterRequiresControlState (control);
			Assert.IsTrue (p.RequiresControlState (control), "RequiresControlState");
			p.UnregisterRequiresControlState (control);
			Assert.IsNotNull (p.GetValidators (String.Empty), "GetValidators");
			p.Validate (String.Empty);
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void MapPath_Deny_Unrestricted ()
		{
			try {
				new Page ().MapPath ("/");
			}
			catch (NullReferenceException) {
				// ms 1.x + 2.0
			}
			catch (HttpException) {
				// mono
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
#if NET_2_0
		[ExpectedException (typeof (SecurityException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void ProcessRequest_Deny_Unrestricted ()
		{
			new Page ().ProcessRequest (new HttpContext (null));
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
#if NET_2_0
		[ExpectedException (typeof (HttpException))]
#else
		// indirect for HttpApplicationState | HttpStaticObjectsCollection
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void ProcessRequest_PermitOnly_FileIOPermission ()
		{
			new Page ().ProcessRequest (new HttpContext (null));
		}

#if NET_2_0
		private void Handler (object sender, EventArgs e)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Events_Deny_Unrestricted ()
		{
			Page p = new Page ();
			p.InitComplete += new EventHandler (Handler);
			p.LoadComplete += new EventHandler (Handler);
			p.PreInit += new EventHandler (Handler);
			p.PreLoad += new EventHandler (Handler);
			p.PreRenderComplete += new EventHandler (Handler);
			p.SaveStateComplete += new EventHandler (Handler);

			p.InitComplete -= new EventHandler (Handler);
			p.LoadComplete -= new EventHandler (Handler);
			p.PreInit -= new EventHandler (Handler);
			p.PreLoad -= new EventHandler (Handler);
			p.PreRenderComplete -= new EventHandler (Handler);
			p.SaveStateComplete -= new EventHandler (Handler);
		}
#endif

		// LinkDemand

		public override Type Type {
			get { return typeof (Page); }
		}
	}
}
