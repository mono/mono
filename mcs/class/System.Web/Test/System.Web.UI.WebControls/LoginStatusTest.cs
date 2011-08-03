//
// LoginStatusTest.cs	- Unit tests for System.Web.UI.WebControls.LoginStatus
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

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestLoginStatus : LoginStatus {

		private bool cancel;
		private bool onLoggedOut;
		private bool onLoggingOut;
		private bool onPreRender;

		public string Tag {
			get { return base.TagName; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		public string Render ()
		{
			HtmlTextWriter writer = GetWriter ();
			Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public string RenderContents ()
		{
			HtmlTextWriter writer = GetWriter ();
			base.RenderContents (writer);
			return writer.InnerWriter.ToString ();
		}

		public void SetDesignMode (IDictionary dic)
		{
			base.SetDesignModeState (dic);
		}

		public bool Cancel {
			get { return cancel; }
			set { cancel = value; }
		}

		public bool OnLoggedOutCalled {
			get { return onLoggedOut; }
			set { onLoggedOut = value; }
		}

		protected override void OnLoggedOut (EventArgs e)
		{
			onLoggedOut = true;
			base.OnLoggedOut (e);
		}

		public void DoLoggedOut (EventArgs e)
		{
			base.OnLoggedOut (e);
		}

		public bool OnLoggingOutCalled {
			get { return onLoggingOut; }
			set { onLoggingOut = value; }
		}

		protected override void OnLoggingOut (LoginCancelEventArgs e)
		{
			onLoggingOut = true;
			e.Cancel = cancel;
			base.OnLoggingOut (e);
		}

		public void DoLoggingIn (LoginCancelEventArgs e)
		{
			base.OnLoggingOut (e);
		}

		public bool OnPreRenderCalled {
			get { return onPreRender; }
			set { onPreRender = value; }
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			onPreRender = true;
			base.OnPreRender (e);
		}
	}

	[TestFixture]
	public class LoginStatusTest {

		private IPrincipal GetPrincipal (string name)
		{
			return new GenericPrincipal (new GenericIdentity (name), null);
		}

		private IPrincipal GetUnauthenticatedPrincipal (string name)
		{
			return new GenericPrincipal (new UnauthenticatedIdentity (name), null);
		}

		[Test]
		public void DefaultProperties ()
		{
			TestLoginStatus ls = new TestLoginStatus ();
			Assert.AreEqual (0, ls.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, ls.StateBag.Count, "ViewState.Count");

			Assert.AreEqual (String.Empty, ls.LoginImageUrl, "LoginImageUrl");
			Assert.AreEqual ("Login", ls.LoginText, "LoginText");
			Assert.AreEqual (LogoutAction.Refresh, ls.LogoutAction, "LogoutAction");
			Assert.AreEqual (String.Empty, ls.LogoutImageUrl, "LogoutImageUrl");
			Assert.AreEqual (String.Empty, ls.LogoutPageUrl, "LogoutPageUrl");
			Assert.AreEqual ("Logout", ls.LogoutText, "LogoutText");

			Assert.AreEqual ("a", ls.Tag, "TagName");
			Assert.AreEqual (0, ls.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, ls.StateBag.Count, "ViewState.Count-2");
		}

		[Test]
		public void SetOriginalProperties ()
		{
			TestLoginStatus ls = new TestLoginStatus ();

			ls.LoginImageUrl = String.Empty;
			Assert.AreEqual (String.Empty, ls.LoginImageUrl, "LoginImageUrl");
			ls.LoginText = "Login";
			Assert.AreEqual ("Login", ls.LoginText, "LoginText");
			ls.LogoutAction = LogoutAction.Refresh;
			Assert.AreEqual (LogoutAction.Refresh, ls.LogoutAction, "LogoutAction");
			ls.LogoutImageUrl = String.Empty;
			Assert.AreEqual (String.Empty, ls.LogoutImageUrl, "LogoutImageUrl");
			ls.LogoutPageUrl = String.Empty;
			Assert.AreEqual (String.Empty, ls.LogoutPageUrl, "LogoutPageUrl");
			ls.LogoutText = "Logout";
			Assert.AreEqual ("Logout", ls.LogoutText, "LogoutText");

			Assert.AreEqual ("a", ls.Tag, "TagName");
			Assert.AreEqual (6, ls.StateBag.Count, "ViewState.Count-1");
		}

		[Test]
		public void CleanProperties ()
		{
			TestLoginStatus ls = new TestLoginStatus ();
			ls.LoginImageUrl = String.Empty;
			ls.LoginText = "Login";
			ls.LogoutAction = LogoutAction.Refresh;
			ls.LogoutImageUrl = String.Empty;
			ls.LogoutPageUrl = String.Empty;
			ls.LogoutText = "Logout";
			Assert.AreEqual (6, ls.StateBag.Count, "ViewState.Count-1");

			ls.LoginImageUrl = null;
			Assert.AreEqual (String.Empty, ls.LoginImageUrl, "LoginImageUrl");
			ls.LoginText = null;
			Assert.AreEqual ("Login", ls.LoginText, "LoginText");
			ls.LogoutImageUrl = null;
			Assert.AreEqual (String.Empty, ls.LogoutImageUrl, "LogoutImageUrl");
			ls.LogoutPageUrl = null;
			Assert.AreEqual (String.Empty, ls.LogoutPageUrl, "LogoutPageUrl");
			ls.LogoutText = null;
			Assert.AreEqual ("Logout", ls.LogoutText, "LogoutText");

			Assert.AreEqual ("a", ls.Tag, "TagName");
			Assert.AreEqual (1, ls.StateBag.Count, "ViewState.Count-2");
		}

		[Test]
		public void Tag ()
		{
			TestLoginStatus ls = new TestLoginStatus ();
			Assert.AreEqual ("a", ls.Tag, "TagName");

			ls.LoginImageUrl = "http://www.go-mono.com";
			ls.LogoutImageUrl = "http://www.mono-project.com";
			Assert.AreEqual ("a", ls.Tag, "TagName");
		}

		[Test]
		public void Controls ()
		{
			TestLoginStatus ls = new TestLoginStatus ();
			Assert.AreEqual (4, ls.Controls.Count, "Count");

			Assert.IsTrue (ls.Controls[0] is LinkButton, "Type-0");
			Assert.IsTrue (ls.Controls[1] is ImageButton, "Type-1");
			Assert.IsTrue (ls.Controls[2] is LinkButton, "Type-2");
			Assert.IsTrue (ls.Controls[3] is ImageButton, "Type-3");

			Assert.IsTrue (ls.Controls[0].Visible, "Visible-0");
			Assert.IsTrue (ls.Controls[1].Visible, "Visible-1");
			Assert.IsTrue (ls.Controls[2].Visible, "Visible-2");
			Assert.IsTrue (ls.Controls[3].Visible, "Visible-3");

			Assert.IsNotNull (ls.Controls[0].UniqueID, "UniqueID-0");
			Assert.IsNotNull (ls.Controls[1].UniqueID, "UniqueID-1");
			Assert.IsNotNull (ls.Controls[2].UniqueID, "UniqueID-2");
			Assert.IsNotNull (ls.Controls[3].UniqueID, "UniqueID-3");

			Assert.IsNull (ls.Controls[0].ID, "ID-0");
			Assert.IsNull (ls.Controls[1].ID, "ID-1");
			Assert.IsNull (ls.Controls[2].ID, "ID-2");
			Assert.IsNull (ls.Controls[3].ID, "ID-3");
			
			ls.CssClass = "loginClass";
			ls.LoginText = "LoginText";
			ls.LoginImageUrl = "LoginImageUrl";
			ls.LogoutText = "LogoutText";
			ls.LogoutImageUrl = "LogoutImageUrl";

			Assert.AreEqual (String.Empty, (ls.Controls[0] as LinkButton).Text, "Text-0");
			Assert.AreEqual (String.Empty, (ls.Controls[1] as ImageButton).ImageUrl, "ImageUrl-1");
			Assert.AreEqual (String.Empty, (ls.Controls[2] as LinkButton).Text, "Text-2");
			Assert.AreEqual (String.Empty, (ls.Controls[3] as ImageButton).ImageUrl, "ImageUrl-3");
			
			ls.Render ();
			Assert.IsFalse (ls.OnPreRenderCalled, "!OnPreRender");

			Assert.IsFalse (ls.Controls[0].Visible, "Render-Visible-0");
			Assert.IsFalse (ls.Controls[1].Visible, "Render-Visible-1");
			Assert.IsFalse (ls.Controls[2].Visible, "Render-Visible-2");
			Assert.IsTrue (ls.Controls[3].Visible, "Render-Visible-3");

			Assert.AreEqual (String.Empty, (ls.Controls[0] as LinkButton).Text, "Render-Text-0");
			Assert.AreEqual (String.Empty, (ls.Controls[1] as ImageButton).ImageUrl, "Render-ImageUrl-1");
			Assert.AreEqual (String.Empty, (ls.Controls[2] as LinkButton).Text, "Render-Text-2");
			Assert.AreEqual ("LoginImageUrl", (ls.Controls[3] as ImageButton).ImageUrl, "Render-ImageUrl-3");
			Assert.AreEqual ("loginClass", (ls.Controls[3] as ImageButton).CssClass, "Render-ImageUrl-4");
		}

		[Test]
		public void Render_NoPage ()
		{
			TestLoginStatus ls = new TestLoginStatus ();
			Assert.AreEqual ("<a>Login</a>", ls.Render (), "Render");
			Assert.AreEqual ("<a>Login</a>", ls.RenderContents (), "RenderContents");
			ls.LoginText = "Log In";
			Assert.AreEqual ("<a>Log In</a>", ls.Render (), "Render-2");
			Assert.AreEqual ("<a>Log In</a>", ls.RenderContents (), "RenderContents-2");
			ls.LoginImageUrl = "http://www.go-mono-com";

			string s = ls.Render ();
			Assert.IsTrue (s.StartsWith ("<input "), "Render-3a");
			Assert.IsTrue (s.IndexOf (" type=\"image\" ") > 0, "Render-3b");
			Assert.IsTrue (s.IndexOf (" name=\"" + ls.Controls [3].UniqueID + "\" ") > 0, "Render-3c");
			Assert.IsTrue (s.IndexOf (" src=\"http://www.go-mono-com\" ") > 0, "Render-3d");
			Assert.IsTrue (s.IndexOf (" alt=\"Log In\" ") > 0, "Render-3e");
			Assert.IsTrue (s.EndsWith (" />"), "Render-3z");
			Assert.AreEqual (s, ls.RenderContents (), "RenderContents-3");
			// rendering <input> but we're still report an A as tag
			Assert.AreEqual ("a", ls.Tag, "TagName");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Render_User_WithoutForm ()
		{
			ContextPage page = new ContextPage (GetPrincipal ("me"));
			TestLoginStatus ls = new TestLoginStatus ();
			ls.Page = page;
			ls.Render ();
			// must be in a server form
		}

		// all other rendering fails because I can't setup a proper environment
		// mostly because overriding Context doesn't make Page.Request to work :-(
	}
}

#endif

