//
// LoginNameTest.cs - Unit tests for System.Web.UI.WebControls.LoginName
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
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestLoginName : LoginName {

		public HttpContext HttpContext {
			get { return base.Context; }
		}

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
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public string RenderContents ()
		{
			HtmlTextWriter writer = GetWriter ();
			base.RenderContents (writer);
			return writer.InnerWriter.ToString ();
		}

		public string RenderBeginTag ()
		{
			HtmlTextWriter writer = GetWriter ();
			base.RenderBeginTag (writer);
			return writer.InnerWriter.ToString ();
		}

		public string RenderEndTag (bool includeBeginTag)
		{
			HtmlTextWriter writer = GetWriter ();
			if (includeBeginTag) {
				// required before calling RenderEndTag
				base.RenderBeginTag (writer); 
				// unless we're not really calling our base...
			}
			base.RenderEndTag (writer);
			return writer.InnerWriter.ToString ();
		}
	}

	public class ContextPage : Page {

		HttpContext ctx;

		public ContextPage ()
		{
		}

		public ContextPage (IPrincipal principal)
		{
			Context.User = principal;
		}

		protected internal override HttpContext Context {
			get {
				if (ctx == null) {
					ctx = new HttpContext (
						new HttpRequest (String.Empty, "http://www.mono-project.com", String.Empty),
						new HttpResponse (new StringWriter ())
						);
				}
				return ctx;
			}
		}
	}

	public class UnauthenticatedIdentity : GenericIdentity	{

		public UnauthenticatedIdentity (string name)
			: base (name)
		{
		}

		public override bool IsAuthenticated {
			get { return false; }
		}
	}

	[TestFixture]
	public class LoginNameTest {

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
			TestLoginName ln = new TestLoginName ();
			Assert.AreEqual (0, ln.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, ln.StateBag.Count, "ViewState.Count");

			Assert.AreEqual ("{0}", ln.FormatString, "FormatString");

			Assert.AreEqual ("span", ln.Tag, "span");
			Assert.AreEqual (0, ln.Attributes.Count, "Attributes.Count-1");
			Assert.AreEqual (0, ln.StateBag.Count, "ViewState.Count-1");
		}

		[Test]
		public void SetOriginalProperties ()
		{
			TestLoginName ln = new TestLoginName ();
			ln.FormatString = "{0}";
			Assert.AreEqual (1, ln.StateBag.Count, "ViewState.Count-1");
		}

		[Test]
		public void CleanProperties ()
		{
			TestLoginName ln = new TestLoginName ();
			ln.FormatString = "Hola {0}!";
			Assert.AreEqual ("Hola {0}!", ln.FormatString, "FormatString-1");
			Assert.AreEqual (1, ln.StateBag.Count, "ViewState.Count-1");
			ln.FormatString = "{0}";
			Assert.AreEqual ("{0}", ln.FormatString, "FormatString-2");
			Assert.AreEqual (1, ln.StateBag.Count, "ViewState.Count-2");
			ln.FormatString = String.Empty;
			Assert.AreEqual (String.Empty, ln.FormatString, "FormatString-3");
			Assert.AreEqual (1, ln.StateBag.Count, "ViewState.Count-3");
			ln.FormatString = null;
			Assert.AreEqual ("{0}", ln.FormatString, "FormatString-4");
			Assert.AreEqual (0, ln.StateBag.Count, "ViewState.Count-4");
		}

		[Test]
		public void CacheIdentity ()
		{
			TestLoginName ln = new TestLoginName ();
			Assert.AreEqual (String.Empty, ln.RenderContents (), "Anonymous");
			ln.Page = new ContextPage (GetPrincipal ("me"));
			Assert.AreEqual ("me", ln.RenderContents (), "me");
			ln.Page = new ContextPage (GetPrincipal ("you"));
			Assert.AreEqual ("you", ln.RenderContents (), "you");
		}

		[Test]
		public void Render_NoPage ()
		{
			TestLoginName ln = new TestLoginName ();
			Assert.AreEqual (String.Empty, ln.Render (), "Render");
			Assert.AreEqual (String.Empty, ln.RenderContents (), "RenderContents");
			Assert.AreEqual (String.Empty, ln.RenderBeginTag (), "RenderBeginTag");
			Assert.AreEqual (String.Empty, ln.RenderEndTag (false), "RenderEndTag");
		}

		[Test]
		public void Render_Anonymous_NoPrincipal ()
		{
			ContextPage page = new ContextPage ();
			TestLoginName ln = new TestLoginName ();
			ln.Page = page;
			Assert.AreEqual (String.Empty, ln.Render (), "Render");
			Assert.AreEqual (String.Empty, ln.RenderContents (), "RenderContents");
			Assert.AreEqual (String.Empty, ln.RenderBeginTag (), "RenderBeginTag");
			Assert.AreEqual (String.Empty, ln.RenderEndTag (false), "RenderEndTag");
		}

		[Test]
		public void Render_Anonymous_IPrincipal ()
		{
			ContextPage page = new ContextPage (GetPrincipal (String.Empty));
			TestLoginName ln = new TestLoginName ();
			ln.Page = page;
			Assert.AreEqual (String.Empty, ln.Render (), "Render");
			Assert.AreEqual (String.Empty, ln.RenderContents (), "RenderContents");
			Assert.AreEqual (String.Empty, ln.RenderBeginTag (), "RenderBeginTag");
			Assert.AreEqual (String.Empty, ln.RenderEndTag (false), "RenderEndTag");
		}

		[Test]
		public void Render_User ()
		{
			ContextPage page = new ContextPage (GetPrincipal ("me"));
			TestLoginName ln = new TestLoginName ();
			ln.Page = page;
			Assert.IsTrue (page.User.Identity.IsAuthenticated, "IsAuthenticated");
			Assert.AreEqual ("<span>me</span>", ln.Render (), "Render");
			Assert.AreEqual ("me", ln.RenderContents (), "RenderContents");
			Assert.AreEqual ("<span>", ln.RenderBeginTag (), "RenderBeginTag");
			Assert.AreEqual ("<span></span>", ln.RenderEndTag (true), "RenderEndTag");
		}

		[Test]
		public void Render_UnauthenticatedUser ()
		{
			ContextPage page = new ContextPage (GetUnauthenticatedPrincipal ("me"));
			TestLoginName ln = new TestLoginName ();
			ln.Page = page;
			Assert.IsFalse (page.User.Identity.IsAuthenticated, "IsAuthenticated");
			Assert.AreEqual ("<span>me</span>", ln.Render (), "Render");
			Assert.AreEqual ("me", ln.RenderContents (), "RenderContents");
			Assert.AreEqual ("<span>", ln.RenderBeginTag (), "RenderBeginTag");
			Assert.AreEqual ("<span></span>", ln.RenderEndTag (true), "RenderEndTag");
		}

		[Test]
		public void Render_StringFormat ()
		{
			ContextPage page = new ContextPage (GetPrincipal ("me"));
			TestLoginName ln = new TestLoginName ();
			ln.Page = page;
			ln.FormatString = "Hola {0}!";
			Assert.AreEqual ("Hola me!", ln.RenderContents (), "RenderContents");
		}

		[Test]
		public void Render_StringFormat_Empty ()
		{
			ContextPage page = new ContextPage (GetPrincipal ("me"));
			TestLoginName ln = new TestLoginName ();
			ln.Page = page;
			ln.FormatString = String.Empty;
			Assert.AreEqual ("me", ln.RenderContents (), "RenderContents");
		}

		[Test]
		public void Render_StringFormat_NoVar ()
		{
			ContextPage page = new ContextPage (GetPrincipal ("me"));
			TestLoginName ln = new TestLoginName ();
			ln.Page = page;
			ln.FormatString = "Hola!";
			Assert.AreEqual ("Hola!", ln.RenderContents (), "RenderContents");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Render_StringFormat_TwoVars ()
		{
			ContextPage page = new ContextPage (GetPrincipal ("me"));
			TestLoginName ln = new TestLoginName ();
			ln.Page = page;
			ln.FormatString = "Hola {0} {1}!";
			Assert.AreEqual ("Hola me!", ln.RenderContents (), "RenderContents");
		}
	}
}

#endif
