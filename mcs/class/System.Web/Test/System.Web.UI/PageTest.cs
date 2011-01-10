
//
// Tests for System.Web.UI.Page
//
// Authors:
//	Peter Dennis Bartok (pbartok@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//      Yoni Klain         <yonik@mainsoft.com>
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
using System.Threading;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
#if NET_2_0
using System.Web.UI.Adapters;
#endif
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Collections.Specialized;
using System.Net;

namespace MonoTests.System.Web.UI {
	class TestPage : Page {

		private HttpContext ctx;

		// don't call base class (so _context is never set to a non-null value)
		protected internal override HttpContext Context {
			get {
				if (ctx == null) {
					ctx = new HttpContext (null);
					ctx.User = new GenericPrincipal (new GenericIdentity ("me"), null);
				}
				return ctx;
			}
		}

		#if NET_2_0 
		public new bool AsyncMode {
			get { return base.AsyncMode; }
			set { base.AsyncMode = value; }
		}

		public new object GetWrappedFileDependencies(string[] virtualFileDependencies)
		{
			return base.GetWrappedFileDependencies(virtualFileDependencies);
		}

		public new void InitOutputCache (OutputCacheParameters cacheSettings)
		{
			base.InitOutputCache (cacheSettings);
		}

		public new string UniqueFilePathSuffix {
			get { return base.UniqueFilePathSuffix; }
		}

		public new char IdSeparator {
			get {
				return base.IdSeparator;
			}
		}
		#endif
	}

	class TestPage2 : Page {

		private HttpContext ctx;

		// don't call base class (so _context is never set to a non-null value)
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

		public HttpContext HttpContext {
			get { return Context; }
		}
	}

	[TestFixture]
	public class PageTest {

		[TestFixtureSetUp]
		public void SetUpTest ()
		{
			WebTest.CopyResource (GetType (), "PageCultureTest.aspx", "PageCultureTest.aspx");
			WebTest.CopyResource (GetType (), "PageLifecycleTest.aspx", "PageLifecycleTest.aspx");
			WebTest.CopyResource (GetType (), "PageValidationTest.aspx", "PageValidationTest.aspx");
			WebTest.CopyResource (GetType (), "AsyncPage.aspx", "AsyncPage.aspx");
			WebTest.CopyResource (GetType (), "PageWithAdapter.aspx", "PageWithAdapter.aspx");
			WebTest.CopyResource (GetType (), "RedirectOnError.aspx", "RedirectOnError.aspx");
			WebTest.CopyResource (GetType (), "ClearErrorOnError.aspx", "ClearErrorOnError.aspx");
		}

		[Test]
		[ExpectedException (typeof(HttpException))]
		public void RequestExceptionTest ()
		{
			Page p;
			HttpRequest r;

			p = new Page ();
			r = p.Request;
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // page.User throw NRE in 2.0 RC
#endif
		public void User_OverridenContext ()
		{
			TestPage page = new TestPage ();
			Assert.AreEqual ("me", page.User.Identity.Name, "User");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Request_OverridenContext ()
		{
			TestPage2 page = new TestPage2 ();
			Assert.IsNotNull (page.Request, "Request");
			// it doesn't seems to access the context via the virtual property
		}

		[Test]
		public void Request_OverridenContext_Indirect ()
		{
			TestPage2 page = new TestPage2 ();
			Assert.IsNotNull (page.HttpContext.Request, "Request");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Response_OverridenContext ()
		{
			TestPage2 page = new TestPage2 ();
			Assert.IsNotNull (page.Response, "Response");
		}
		
		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Cache_OverridenContext ()
		{
			TestPage2 page = new TestPage2 ();
			Assert.IsNotNull (page.Cache, "Cache");
		}
		
		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Session_OverridenContext ()
		{
			TestPage2 page = new TestPage2 ();
			Assert.IsNotNull (page.Session, "Session");
		}

		[Test]
		public void Application_OverridenContext ()
		{
			TestPage2 page = new TestPage2 ();
			Assert.IsNull (page.Application, "Application");
		}

#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderOnPreInit ()
		{
			PageDelegate pd = new PageDelegate (Page_OnPreInit);
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                                PreInit
                                    </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderInit");
		}

		public static void Page_OnPreInit (Page p)
		{
			Assert.AreEqual (null, p.Header, "HeaderOnPreInit");
			p.Title = "PreInit";
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderInit ()
		{
			PageDelegate pd = new PageDelegate (CheckHeader);
			WebTest t = new WebTest (PageInvoker.CreateOnInit (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderInit");

		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderInitComplete ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.InitComplete = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderInitComplete");
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreLoad ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreLoad = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderPreLoad");
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderLoad ()
		{
			PageDelegate pd = new PageDelegate (CheckHeader);
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderLoad");
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderLoadComplete ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.LoadComplete = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderLoadComplete");
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreRender ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreRender = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderPreRender");
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreRenderComplete ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreRenderComplete = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderPreRenderComplete");
		}

		public static void CheckHeader (Page p)
		{
			Assert.AreEqual ("Untitled Page", p.Title, "CheckHeader#1");
			Assert.AreEqual ("Untitled Page", p.Header.Title, "CheckHeader#2");
			p.Title = "Test0";
			Assert.AreEqual ("Test0", p.Header.Title, "CheckHeader#3");
			p.Header.Title = "Test";
			Assert.AreEqual ("Test", p.Title, "CheckHeader#4");
		}
#endif

#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidationGroup ()
		{
			new WebTest (PageInvoker.CreateOnLoad (Page_ValidationGroup_Load)).Run ();
		}

		public static void Page_ValidationGroup_Load (Page page)
		{
			TextBox textbox;
			BaseValidator val;

			textbox = new TextBox ();
			textbox.ID = "T1";
			textbox.ValidationGroup = "VG1";
			page.Form.Controls.Add (textbox);
			val = new RequiredFieldValidator ();
			val.ControlToValidate = "T1";
			val.ValidationGroup = "VG1";
			page.Form.Controls.Add (val);

			textbox = new TextBox ();
			textbox.ID = "T2";
			textbox.ValidationGroup = "VG2";
			page.Form.Controls.Add (textbox);
			val = new RequiredFieldValidator ();
			val.ControlToValidate = "T2";
			val.ValidationGroup = "VG2";
			page.Form.Controls.Add (val);

			textbox = new TextBox ();
			textbox.ID = "T3";
			page.Form.Controls.Add (textbox);
			val = new RequiredFieldValidator ();
			val.ControlToValidate = "T3";
			page.Form.Controls.Add (val);

			Assert.AreEqual (3, page.Validators.Count, "Page_ValidationGroup#1");
			Assert.AreEqual (1, page.GetValidators ("").Count, "Page_ValidationGroup#2");
			Assert.AreEqual (1, page.GetValidators (null).Count, "Page_ValidationGroup#3");
			Assert.AreEqual (0, page.GetValidators ("Fake").Count, "Page_ValidationGroup#4");
			Assert.AreEqual (1, page.GetValidators ("VG1").Count, "Page_ValidationGroup#5");
			Assert.AreEqual (1, page.GetValidators ("VG2").Count, "Page_ValidationGroup#6");
		}

		[Test]
		[Category("NunitWeb")]
		public void InitOutputCache_UsesAdapter ()
		{
			WebTest t = new WebTest ("PageWithAdapter.aspx");
			
			t.Invoker = PageInvoker.CreateOnLoad (InitOutputCache_UsesAdapter_OnLoad);
			t.Run ();
		}

		public static void InitOutputCache_UsesAdapter_OnLoad (Page p)
		{
			Assert.IsTrue (p.Response.Cache.VaryByHeaders ["header-from-aspx"], 
				"InitOutputCache_UsesAdapter #1");
			Assert.IsTrue (p.Response.Cache.VaryByParams ["param-from-aspx"], 
				"InitOutputCache_UsesAdapter #2");
			Assert.IsTrue (p.Response.Cache.VaryByHeaders ["header-from-adapter"], 
				"InitOutputCache_UsesAdapter #3");
			Assert.IsTrue (p.Response.Cache.VaryByParams ["param-from-adapter"], 
				"InitOutputCache_UsesAdapter #4");
		}
		
		[Test]
		[Category("NunitWeb")]
		public void PageStatePersister_UsesAdapter ()
		{
			WebTest t = new WebTest ("PageWithAdapter.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (PageStatePersister_UsesAdapter_OnLoad);
			t.Run ();
		}
		
		public static void PageStatePersister_UsesAdapter_OnLoad (Page p)
		{
			TestPageWithAdapter pageWithAdapter = (TestPageWithAdapter) p;
			Assert.IsTrue (pageWithAdapter.PageStatePersister is TestPersister, 
				"PageStatePersister_UsesAdapter #1");
		}
		
		[Test]
		[Category("NunitWeb")]
		public void ScriptUsesAdapter ()
		{
			WebTest t = new WebTest ("PageWithAdapter.aspx");
			string html = t.Run ();
			Assert.IsTrue(html.IndexOf("var theForm = /* testFormReference */document.forms[") != -1, "ScriptUsesAdapter #1");
		}

		[Test]
		[Category("NunitWeb")]
		public void DeterminePostBackMode_UsesAdapter ()
		{
			WebTest t = new WebTest ("PageWithAdapter.aspx");
			t.Run ();
			t.Request = new FormRequest (t.Response, "form1");
			t.Invoker = PageInvoker.CreateOnInit(DeterminePostBackMode_UsesAdapter_OnInit);
			t.Run ();
		}
		
		public static void DeterminePostBackMode_UsesAdapter_OnInit (Page p)
		{
			// We need to use this special version of HtmlInputHidden because the
			// original class registers itself for event validation in RenderAttributes
			// which is not called by WebTest after this OnInit handler returns (WebTest
			// performs a fake postback which bypasses the rendering phase) and it would
			// result in an event validation failure.
			HtmlInputHidden h = new TestHtmlInputHidden();
			h.ID = "DeterminePostBackModeTestField";
			p.Controls.Add(h);
			p.Load += new EventHandler(DeterminePostBackMode_UsesAdapter_OnLoad);
		}
		
		public static void DeterminePostBackMode_UsesAdapter_OnLoad(object source, EventArgs args)
		{
			Page p = (Page)source;
			HtmlInputHidden h = (HtmlInputHidden)p.FindControl("DeterminePostBackModeTestField");
			Assert.AreEqual("DeterminePostBackModeTestValue", h.Value, 
				"DeterminePostBackMode #1");
		}
#endif

#if NET_2_0
		// This test are testing validation fixture using RequiredFieldValidator for example
		
		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidationCollection () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ValidationCollectionload));
			string html = t.Run ();
		}

		public static void ValidationCollectionload (Page p)
		{
			TextBox txt = new TextBox ();
			txt.ID = "txt";
			RequiredFieldValidator validator = new RequiredFieldValidator ();
			validator.ID = "v";
			validator.ControlToValidate = "txt";
			RequiredFieldValidator validator1 = new RequiredFieldValidator ();
			validator1.ID = "v1";
			validator1.ControlToValidate = "txt";
			p.Form.Controls.Add (txt);
			p.Form.Controls.Add (validator);
			p.Form.Controls.Add (validator1);
			Assert.AreEqual (2, p.Validators.Count, "Validators collection count fail");
			Assert.AreEqual (true, p.Validators[0].IsValid, "Validators collection value#1 fail");
			Assert.AreEqual (true, p.Validators[1].IsValid, "Validators collection value#2 fail");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidatorTest1 ()
		{

			WebTest t = new WebTest ("PageValidationTest.aspx");
			string PageRenderHtml = t.Run ();

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("TextBox1");
			PageDelegates pd = new PageDelegates ();
			pd.PreRender = ValidatorTest1PreRender;
			t.Invoker = new PageInvoker (pd);
			fr.Controls["TextBox1"].Value = "";
			t.Request = fr;

			PageRenderHtml = t.Run ();
			Assert.IsNotNull(t.UserData, "Validate server side method not raised fail");
			ArrayList list = t.UserData as ArrayList;
			if (list == null)
				Assert.Fail ("User data not created fail#1");
			Assert.AreEqual (1, list.Count, "Just validate with no validation group must be called fail#1");
			Assert.AreEqual ("Validate", list[0].ToString (), "Validate with no validation group must be called fail#1");
		}

		public static void ValidatorTest1PreRender (Page p)
		{
			Assert.AreEqual (1, p.Validators.Count, "Validators count fail#1");
			Assert.AreEqual (false, p.Validators[0].IsValid, "Specific validator value filed#1");
			Assert.AreEqual (false, p.IsValid, "Page validation Failed#1");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidatorTest2 ()
		{

			WebTest t = new WebTest ("PageValidationTest.aspx");
			string PageRenderHtml = t.Run ();

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("TextBox1");
			PageDelegates pd = new PageDelegates ();
			pd.PreRender = ValidatorTest2PreRender;
			t.Invoker = new PageInvoker (pd);
			fr.Controls["TextBox1"].Value = "test";
			t.Request = fr;

			PageRenderHtml = t.Run ();
			Assert.IsNotNull ( t.UserData, "Validate server side method not raised fail#2");
			ArrayList list = t.UserData as ArrayList;
			if (list == null)
			Assert.Fail ("User data not created fail#2");
			Assert.AreEqual (1, list.Count, "Just validate with no validation group must be called fail#2");
			Assert.AreEqual ("Validate", list[0].ToString (), "Validate with no validation group must be called fail#2");
		}

		public static void ValidatorTest2PreRender (Page p)
		{
			Assert.AreEqual (1, p.Validators.Count, "Validators count fail#2");
			Assert.AreEqual (true, p.Validators[0].IsValid, "Specific validator value fail#2");
			Assert.AreEqual (true, p.IsValid, "Page validation Fail#2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidatorTest3 ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ValidatorTest3Load));
			t.Run ();
		}

		public static void ValidatorTest3Load (Page p)
		{
			TextBox tbx = new TextBox ();
			tbx.ID = "tbx";
			RequiredFieldValidator vld = new RequiredFieldValidator ();
			vld.ID = "vld";
			vld.ControlToValidate = "tbx";
			p.Controls.Add (tbx);
			p.Controls.Add (vld);
			vld.Validate ();
			Assert.AreEqual (false, p.Validators[0].IsValid, "RequiredField result fail #1");
			tbx.Text = "test";
			vld.Validate ();
			Assert.AreEqual (true, p.Validators[0].IsValid, "RequiredField result fail #2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidatorTest4 ()
		{

			WebTest t = new WebTest ("PageValidationTest.aspx");
			string PageRenderHtml = t.Run ();

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("TextBox1");
			fr.Controls.Add ("Button1");

			PageDelegates pd = new PageDelegates ();
			pd.PreRender = ValidatorTest4PreRender;
			t.Invoker = new PageInvoker (pd);
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["TextBox1"].Value = "";
			fr.Controls["Button1"].Value = "Button";
			t.Request = fr;

			PageRenderHtml = t.Run ();
			Assert.IsNotNull (t.UserData, "Validate server side method not raised fail#3");
			ArrayList list = t.UserData as ArrayList;
			if (list == null)
				Assert.Fail ("User data not created fail#3");
			Assert.AreEqual (1, list.Count, "Just validate with validation group must be called fail#3");
			Assert.AreEqual ("Validate_WithGroup", list[0].ToString (), "Validate with validation group must be called fail#3");
		}

		public static void ValidatorTest4PreRender (Page p)
		{
			Assert.AreEqual (1, p.Validators.Count, "Validators count fail#3");
			Assert.AreEqual (false, p.Validators[0].IsValid, "Specific validator value filed#3");
			Assert.AreEqual (false, p.IsValid, "Page validation Failed#3");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidatorTest5 ()
		{

			WebTest t = new WebTest ("PageValidationTest.aspx");
			string PageRenderHtml = t.Run ();

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("TextBox1");
			fr.Controls.Add ("Button1");

			PageDelegates pd = new PageDelegates ();
			pd.PreRender = ValidatorTest5PreRender;
			t.Invoker = new PageInvoker (pd);
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["TextBox1"].Value = "Test";
			fr.Controls["Button1"].Value = "Button";
			t.Request = fr;

			PageRenderHtml = t.Run ();
			Assert.IsNotNull ( t.UserData, "Validate server side method not raised fail#3");
			ArrayList list = t.UserData as ArrayList;
			if (list == null)
				Assert.Fail ("User data not created fail#3");
			Assert.AreEqual (1, list.Count, "Just validate with validation group must be called fail#3");
			Assert.AreEqual ("Validate_WithGroup", list[0].ToString (), "Validate with validation group must be called fail#3");
		}

		public static void ValidatorTest5PreRender (Page p)
		{
			Assert.AreEqual (1, p.Validators.Count, "Validators count fail#3");
			Assert.AreEqual (true, p.Validators[0].IsValid, "Specific validator value filed#3");
			Assert.AreEqual (true, p.IsValid, "Page validation Failed#3");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidatorTest6 ()
		{

			WebTest t = new WebTest ("PageValidationTest.aspx");
			string PageRenderHtml = t.Run ();

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("TextBox1");
			fr.Controls.Add ("Button1");

			PageDelegates pd = new PageDelegates ();
			pd.PreRender = ValidatorTest6PreRender;
			pd.Load = ValidatorTest6Load;
			t.Invoker = new PageInvoker (pd);
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["TextBox1"].Value = "Test";
			fr.Controls["Button1"].Value = "Button";
			t.Request = fr;

			PageRenderHtml = t.Run ();
			Assert.IsNotNull (t.UserData, "Validate server side method not raised fail#3");
			ArrayList list = t.UserData as ArrayList;
			if (list == null)
				Assert.Fail ("User data not created fail#3");
			Assert.AreEqual (1, list.Count, "Just validate with validation group must be called fail#3");
			Assert.AreEqual ("Validate_WithGroup", list[0].ToString (), "Validate with validation group must be called fail#3");
		}

		public static void ValidatorTest6PreRender (Page p)
		{
			Assert.AreEqual (1, p.Validators.Count, "Validators count fail#3");
			Assert.AreEqual (false, p.Validators[0].IsValid, "Specific validator value filed#3");
			Assert.AreEqual (false, p.IsValid, "Page validation Failed#3");
		}

		public static void ValidatorTest6Load (Page p)
		{
			if (p.IsPostBack) {
				RequiredFieldValidator rfv = p.FindControl ("RequiredFieldValidator1") as RequiredFieldValidator;
				if (rfv == null)
					Assert.Fail ("RequiredFieldValidator does not created fail");
				rfv.InitialValue = "Test";
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidatorTest7 ()
		{

			WebTest t = new WebTest ("PageValidationTest.aspx");
			string PageRenderHtml = t.Run ();

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("TextBox1");
			fr.Controls.Add ("Button1");

			PageDelegates pd = new PageDelegates ();
			pd.PreRender = ValidatorTest7PreRender;
			pd.Load = ValidatorTest7Load;
			t.Invoker = new PageInvoker (pd);
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["TextBox1"].Value = "Test";
			fr.Controls["Button1"].Value = "Button";
			t.Request = fr;

			PageRenderHtml = t.Run ();
			Assert.IsNotNull (t.UserData, "Validate server side method not raised fail#4");
			ArrayList list = t.UserData as ArrayList;
			if (list == null)
				Assert.Fail ("User data not created fail#4");
			Assert.AreEqual (1, list.Count, "Just validate with validation group must be called fail#4");
			Assert.AreEqual ("Validate_WithGroup", list[0].ToString (), "Validate with validation group must be called fail#4");
		}

		public static void ValidatorTest7PreRender (Page p)
		{
			Assert.AreEqual (2, p.Validators.Count, "Validators count fail#4");
			Assert.AreEqual (true, p.Validators[0].IsValid, "Specific validator value filed_1#4");
			Assert.AreEqual (true, p.Validators[1].IsValid, "Specific validator value filed#4_2#4");
			Assert.AreEqual (true, p.IsValid, "Page validation Failed#4");
		}

		public static void ValidatorTest7Load (Page p)
		{
			RequiredFieldValidator validator = new RequiredFieldValidator ();
			validator.ID = "validator";
			validator.ControlToValidate = "TextBox1";
			validator.ValidationGroup = "fake";
			validator.InitialValue = "Test";
			p.Form.Controls.Add (validator);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Page_Lifecycle ()
		{

			WebTest t = new WebTest ("PageLifecycleTest.aspx");
			string PageRenderHtml = t.Run ();
			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("OnPreInit", eventlist[0], "Live Cycle Flow #1");
			Assert.AreEqual ("OnInit", eventlist[1], "Live Cycle Flow #2");
			Assert.AreEqual ("OnInitComplete", eventlist[2], "Live Cycle Flow #3");
			Assert.AreEqual ("OnPreLoad", eventlist[3], "Live Cycle Flow #4");
			Assert.AreEqual ("OnLoad", eventlist[4], "Live Cycle Flow #5");
			Assert.AreEqual ("OnLoadComplete", eventlist[5], "Live Cycle Flow #6");
			Assert.AreEqual ("OnPreRender", eventlist[6], "Live Cycle Flow #7");
			Assert.AreEqual ("OnPreRenderComplete", eventlist[7], "Live Cycle Flow #8");
			Assert.AreEqual ("OnSaveStateComplete", eventlist[8], "Live Cycle Flow #9");
			Assert.AreEqual ("OnUnload", eventlist[9], "Live Cycle Flow #10");
		}

		[Test]
		[Category ("NunitWeb")]
#if !TARGET_JVM
		[Category ("NotWorking")] // Mono PageParser does not handle @Page Async=true
#endif
		public void AddOnPreRenderCompleteAsync ()
		{
			WebTest t = new WebTest ("AsyncPage.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (AddOnPreRenderCompleteAsync_Load);
			string str = t.Run ();
			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("BeginGetAsyncData", eventlist[0], "BeginGetAsyncData Failed");
			Assert.AreEqual ("EndGetAsyncData", eventlist[1], "EndGetAsyncData Failed");
		}

		[Test]
#if !TARGET_JVM
		[Category ("NotWorking")] // Mono PageParser does not handle @Page Async=true
#endif
		[Category ("NunitWeb")]
		public void ExecuteRegisteredAsyncTasks ()
		{
			WebTest t = new WebTest ("AsyncPage.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (ExecuteRegisteredAsyncTasks_Load);
			string str = t.Run ();
			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("BeginGetAsyncData", eventlist[0], "BeginGetAsyncData Failed");
			Assert.AreEqual ("EndGetAsyncData", eventlist[1], "EndGetAsyncData Failed");
		}

		[Test]
		[Category ("NunitWeb")]
#if !TARGET_JVM
		[Category ("NotWorking")] // Mono PageParser does not handle @Page Async=true
#endif
		[ExpectedException (typeof (Exception))]
		public void AddOnPreRenderCompleteAsyncBeginThrows () 
		{
			WebTest t = new WebTest ("AsyncPage.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (AddOnPreRenderCompleteAsyncBeginThrows_Load);
			string str = t.Run ();
		}

		[Test]
		[Category ("NunitWeb")]
#if !TARGET_JVM
		[Category ("NotWorking")] // Mono PageParser does not handle @Page Async=true
#endif
		[ExpectedException (typeof (Exception))]
		public void AddOnPreRenderCompleteAsyncEndThrows () 
		{
			WebTest t = new WebTest ("AsyncPage.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (AddOnPreRenderCompleteAsyncEndThrows_Load);
			string str = t.Run ();
		}

		public static void ExecuteRegisteredAsyncTasks_Load (Page p)
		{
			BeginEventHandler bh = new BeginEventHandler (BeginGetAsyncData);
			EndEventHandler eh = new EndEventHandler (EndGetAsyncData);
			p.AddOnPreRenderCompleteAsync (bh, eh);
			p.ExecuteRegisteredAsyncTasks ();
		}

		static WebRequest myRequest;
		public static void AddOnPreRenderCompleteAsync_Load (Page p)
		{
			BeginEventHandler bh = new BeginEventHandler(BeginGetAsyncData);
			EndEventHandler eh = new EndEventHandler(EndGetAsyncData);
			p.AddOnPreRenderCompleteAsync(bh, eh);

			// Initialize the WebRequest.
			string address = "http://MyPage.aspx";
			myRequest = WebRequest.Create(address);
		}

		public static void AddOnPreRenderCompleteAsyncBeginThrows_Load (Page p) 
		{
			BeginEventHandler bh = new BeginEventHandler (BeginGetAsyncDataThrows);
			EndEventHandler eh = new EndEventHandler (EndGetAsyncData);
			p.AddOnPreRenderCompleteAsync (bh, eh);

			// Initialize the WebRequest.
			string address = "http://MyPage.aspx";
			myRequest = WebRequest.Create (address);
		}

		public static void AddOnPreRenderCompleteAsyncEndThrows_Load (Page p) 
		{
			BeginEventHandler bh = new BeginEventHandler (BeginGetAsyncData);
			EndEventHandler eh = new EndEventHandler (EndGetAsyncDataThrows);
			p.AddOnPreRenderCompleteAsync (bh, eh);

			// Initialize the WebRequest.
			string address = "http://MyPage.aspx";
			myRequest = WebRequest.Create (address);
		}

		static IAsyncResult BeginGetAsyncData (Object src, EventArgs args, AsyncCallback cb, Object state)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("BeginGetAsyncData");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("BeginGetAsyncData");
				WebTest.CurrentTest.UserData = list;
			}
			return new Customresult(); // myRequest.BeginGetResponse (cb, state);
		}

		static IAsyncResult BeginGetAsyncDataThrows (Object src, EventArgs args, AsyncCallback cb, Object state) 
		{
			ArrayList list = null;
			if (WebTest.CurrentTest.UserData == null) {
				list = new ArrayList ();
			}
			else {
				list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
			}
			list.Add ("BeginGetAsyncData");
			WebTest.CurrentTest.UserData = list;

			throw new Exception ("BeginGetAsyncDataThrows");
		}

		static void EndGetAsyncData (IAsyncResult ar)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("EndGetAsyncData");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("EndGetAsyncData");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void EndGetAsyncDataThrows (IAsyncResult ar) 
		{
			ArrayList list = null;
			if (WebTest.CurrentTest.UserData == null) {
				list = new ArrayList ();
			}
			else {
				list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
			}
			list.Add ("EndGetAsyncData");
			WebTest.CurrentTest.UserData = list;

			throw new Exception ("EndGetAsyncDataThrows");
		}

		[Test]
		public void AsyncMode ()
		{
			TestPage p = new TestPage ();
			Assert.AreEqual (false, p.AsyncMode, "AsyncMode#1");
			p.AsyncMode = true;
			Assert.AreEqual (true, p.AsyncMode, "AsyncMode#2");
		}

		[Test]
		public void AsyncTimeout ()
		{
			Page p = new Page ();
			Assert.AreEqual (45, ((TimeSpan) p.AsyncTimeout).Seconds, "AsyncTimeout#1");
			p.AsyncTimeout = new TimeSpan (0, 0, 50);
			Assert.AreEqual (50, ((TimeSpan) p.AsyncTimeout).Seconds, "AsyncTimeout#2");
		}

		[Test]
		public void ClientQueryString ()
		{
			// httpContext URL cannot be set.
		}

		[Test]
		public void ClientScript ()
		{
			Page p = new Page ();
			Assert.AreEqual (typeof(ClientScriptManager), p.ClientScript.GetType(), "ClientScriptManager");
		}

		[Test]
		public void CreateHtmlTextWriterFromType ()
		{
			HtmlTextWriter writer = Page.CreateHtmlTextWriterFromType (null, typeof (HtmlTextWriter));
			Assert.IsNotNull (writer, "CreateHtmlTextWriterFromType Failed");
		}

		[Test]
		public void EnableEventValidation ()
		{
			Page p = new Page ();
			Assert.AreEqual (true, p.EnableEventValidation, "EnableEventValidation#1");
			p.EnableEventValidation = false;
			Assert.AreEqual (false, p.EnableEventValidation, "EnableEventValidation#2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Form ()
		{
			Page p = new Page ();
			Assert.AreEqual (null, p.Form, "Form#1");
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (Form_Load));
			t.Run ();
		}

		public static void Form_Load (Page p)
		{
			Assert.IsNotNull (p.Form, "Form#2");
			Assert.AreEqual ("form1", p.Form.ID, "Form#3");
			Assert.AreEqual (typeof (HtmlForm), p.Form.GetType (), "Form#4");
		}

		[Test]
		public void GetWrappedFileDependencies ()
		{
			TestPage p = new TestPage ();
			string []s = { "test.aspx","fake.aspx" };
			object list = p.GetWrappedFileDependencies (s);
			Assert.AreEqual (typeof(String[]), list.GetType (), "GetWrappedFileDependencie#1");
			Assert.AreEqual (2, ((String[]) list).Length, "GetWrappedFileDependencie#2");
			Assert.AreEqual ("test.aspx", ((String[]) list)[0], "GetWrappedFileDependencie#3");
			Assert.AreEqual ("fake.aspx", ((String[]) list)[1], "GetWrappedFileDependencie#4");
		}

		[Test]
		public void IdSeparator () 
		{
			TestPage p = new TestPage ();
			Assert.AreEqual ('$', p.IdSeparator, "IdSeparator");
		}

		[Test]
		[Category ("NunitWeb")]
		public void InitializeCulture ()
		{
			WebTest t = new WebTest ("PageCultureTest.aspx");
			string PageRenderHtml = t.Run ();
			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("InitializeCulture:0", eventlist[0], "Live Cycle Flow #0");
			Assert.AreEqual ("OnPreInit", eventlist[1], "Live Cycle Flow #1");
			Assert.AreEqual ("OnInit", eventlist[2], "Live Cycle Flow #2");
			Assert.AreEqual ("OnInitComplete", eventlist[3], "Live Cycle Flow #3");
			Assert.AreEqual ("OnPreLoad", eventlist[4], "Live Cycle Flow #4");
			Assert.AreEqual ("OnLoad", eventlist[5], "Live Cycle Flow #5");
			Assert.AreEqual ("OnLoadComplete", eventlist[6], "Live Cycle Flow #6");
			Assert.AreEqual ("OnPreRender", eventlist[7], "Live Cycle Flow #7");
			Assert.AreEqual ("OnPreRenderComplete", eventlist[8], "Live Cycle Flow #8");
			Assert.AreEqual ("OnSaveStateComplete", eventlist[9], "Live Cycle Flow #9");
			Assert.AreEqual ("OnUnload", eventlist[10], "Live Cycle Flow #10");
		}

		[Test]
		public void IsAsync ()
		{
			Page p = new Page ();
			Assert.AreEqual (false, p.IsAsync, "IsAsync");
		}

		[Test]
		public void IsCallback ()
		{
			Page p = new Page ();
			Assert.AreEqual (false, p.IsCallback, "IsCallback");
		}

		[Test]
		public void IsCrossPagePostBack ()
		{
			Page p = new Page ();
			Assert.AreEqual (false, p.IsCrossPagePostBack, "IsCrossPagePostBack");
		}

		[Test]
		public void Items ()
		{
			Page p = new Page ();
			IDictionary d = p.Items;
			d.Add ("key", "test");
			Assert.AreEqual (1, p.Items.Count, "Items#1");
			Assert.AreEqual ("test", p.Items["key"].ToString(), "Items#2");
		}

		[Test]
		public void MaintainScrollPositionOnPostBack ()
		{
			Page p = new Page ();
			Assert.AreEqual (false, p.MaintainScrollPositionOnPostBack, "MaintainScrollPositionOnPostBack#1");
			p.MaintainScrollPositionOnPostBack = true;
			Assert.AreEqual (true, p.MaintainScrollPositionOnPostBack, "MaintainScrollPositionOnPostBack#2");
		}

		[Test]
		[Category("NunitWeb")]
		public void Master ()
		{
			Page p = new Page ();
			Assert.AreEqual (null, p.Master, "Master#1");
			WebTest t = new WebTest ("MyPageWithMaster.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (Master_Load);
			t.Run ();
			Assert.AreEqual ("asp.my_master", t.UserData.ToString ().ToLower(), "Master#2");
		}

		public static void Master_Load (Page p)
		{
			WebTest.CurrentTest.UserData = p.Master.GetType().ToString();
		}

		[Test]
		public void MasterPageFile ()
		{
			Page p = new Page ();
			Assert.AreEqual (null, p.MasterPageFile, "MasterPageFile#1");
			p.MasterPageFile = "test";
			Assert.AreEqual ("test", p.MasterPageFile, "MasterPageFile#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void MaxPageStateFieldLength ()
		{
			Page p = new Page ();
			Assert.AreEqual (-1, p.MaxPageStateFieldLength, "MaxPageStateFieldLength#1");
			p.MaxPageStateFieldLength = 10;
			Assert.AreEqual (10, p.MaxPageStateFieldLength, "MaxPageStateFieldLength#2");
		}

		[Test]
		public void PageAdapterWithNoAdapter ()
		{
			Page p = new Page ();
			Assert.AreEqual (null, p.PageAdapter, "PageAdapter");
		}

		[Test]
		public void PageAdapterWithPageAdapter ()
		{
			TestPageWithAdapter p = new TestPageWithAdapter ();
			Assert.AreEqual (p.page_adapter, p.PageAdapter, "PageAdapter");
		}

		[Test]
		public void PageAdapterWithControlAdapter ()
		{
			TestPageWithControlAdapter p = new TestPageWithControlAdapter ();
			Assert.AreEqual (null, p.PageAdapter, "PageAdapter");
		}

		[Test]
		public void PreviousPage ()
		{
			// NUnit.Framework limitation for server.transfer	
		}

		[Test]
		public void RegisterRequiresViewStateEncryption ()
		{
			Page p = new Page ();
			p.ViewStateEncryptionMode = ViewStateEncryptionMode.Always;
			p.RegisterRequiresViewStateEncryption ();
			// No changes after the Encryption 
		}

		[Test]
		public void Theme ()
		{
			Page p = new Page ();
			Assert.AreEqual (null, p.Theme, "Theme#1");
			p.Theme = "Theme.skin";
			Assert.AreEqual ("Theme.skin",p.Theme, "Theme#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void UniqueFilePathSuffix ()
		{
			TestPage p = new TestPage ();
			if (!p.UniqueFilePathSuffix.StartsWith ("__ufps=")) {
				Assert.Fail ("UniqueFilePathSuffix");
			}
		}

		[Test]
		public void ViewStateEncryptionModeTest ()
		{
			Page p = new Page ();
			Assert.AreEqual (ViewStateEncryptionMode.Auto, p.ViewStateEncryptionMode, "ViewStateEncryptionMode#1");
			p.ViewStateEncryptionMode = ViewStateEncryptionMode.Never;
			Assert.AreEqual (ViewStateEncryptionMode.Never, p.ViewStateEncryptionMode, "ViewStateEncryptionMode#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetDataItem_Exception ()
		{
			Page p = new Page ();
			p.GetDataItem ();
		}

		#region help_classes
		class Customresult : IAsyncResult
		{

			#region IAsyncResult Members

			public object AsyncState
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public WaitHandle AsyncWaitHandle
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public bool CompletedSynchronously
			{
				get { return true; }
			}

			public bool IsCompleted
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			#endregion
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void ProcessPostData_Second_Try ()  //Just flow and not implementation detail
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ProcessPostData_Second_Try_Load));
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = "__Page";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();

			Assert.AreEqual ("CustomPostBackDataHandler_LoadPostData", t.UserData, "User data does not been created fail");
		}

		public static void ProcessPostData_Second_Try_Load (Page p)
		{
			CustomPostBackDataHandler c = new CustomPostBackDataHandler ();
			c.ID = "CustomPostBackDataHandler1";
			p.Form.Controls.Add (c);
		}

		class CustomPostBackDataHandler : WebControl, IPostBackDataHandler
		{
			protected internal override void OnInit (EventArgs e)
			{
				base.OnInit (e);
				Page.RegisterRequiresPostBack (this);
			}

			#region IPostBackDataHandler Members

			public bool LoadPostData (string postDataKey, global::System.Collections.Specialized.NameValueCollection postCollection)
			{
				WebTest.CurrentTest.UserData = "CustomPostBackDataHandler_LoadPostData";
				return false;
			}

			public void RaisePostDataChangedEvent ()
			{
			}

			#endregion
		}

		[Test]
		[Category ("NunitWeb")]
		public void RegisterRequiresPostBack ()  //Just flow and not implementation detail
		{
			PageDelegates delegates = new PageDelegates ();
			delegates.Init = RegisterRequiresPostBack_Init;
			delegates.Load = RegisterRequiresPostBack_Load;
			WebTest t = new WebTest (new PageInvoker (delegates));
			string html = t.Run ();
			
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = "__Page";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			html = t.Run ();

			Assert.AreEqual ("CustomPostBackDataHandler2_LoadPostData", t.UserData, "RegisterRequiresPostBack#1");
			t.UserData = null;

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = "__Page";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			html = t.Run ();

			Assert.AreEqual (null, t.UserData, "RegisterRequiresPostBack#2");
		}

		public static void RegisterRequiresPostBack_Init (Page p)
		{
			CustomPostBackDataHandler2 c = new CustomPostBackDataHandler2 ();
			c.ID = "CustomPostBackDataHandler2";
			p.Form.Controls.Add (c);
		}

		public static void RegisterRequiresPostBack_Load (Page p)
		{
			if (!p.IsPostBack)
				p.RegisterRequiresPostBack (p.Form.FindControl ("CustomPostBackDataHandler2"));
		}

		class CustomPostBackDataHandler2 : WebControl, IPostBackDataHandler
		{
			#region IPostBackDataHandler Members

			public bool LoadPostData (string postDataKey, global::System.Collections.Specialized.NameValueCollection postCollection) {
				WebTest.CurrentTest.UserData = "CustomPostBackDataHandler2_LoadPostData";
				return false;
			}

			public void RaisePostDataChangedEvent () {
			}

			#endregion
		}

		[Test]
		[Category ("NunitWeb")]
		public void ClearErrorOnErrorTest ()
		{
			WebTest t = new WebTest ("ClearErrorOnError.aspx");
			string html = t.Run ();
			Assert.AreEqual (HttpStatusCode.OK, t.Response.StatusCode);
		}

		[Test]
		[Category ("NunitWeb")]
		public void RedirectOnErrorTest ()
		{
			WebTest t = new WebTest ("RedirectOnError.aspx");
			string html = t.Run ();
			Assert.AreEqual (HttpStatusCode.Found, t.Response.StatusCode);
		}
#endif

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}

#if NET_2_0
	class TestHtmlInputHidden : global::System.Web.UI.HtmlControls.HtmlInputHidden
	{
		protected override bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			string data = postCollection [postDataKey];
			if (data != null && data != Value) {
				Value = data;
				return true;
			}
			return false;
		}
	}
	
	class TestAdapter : global::System.Web.UI.Adapters.PageAdapter
	{
		public override StringCollection CacheVaryByParams {
			get {
				StringCollection paramNames = new StringCollection();
				paramNames.AddRange (new string[] {"param-from-adapter"});
				return paramNames;
			}
		}

		public override StringCollection CacheVaryByHeaders {
			get {
				StringCollection headerNames = new StringCollection();
				headerNames.AddRange (new string[] {"header-from-adapter"});
				return headerNames;
			}
		}
		
		PageStatePersister persister;
		public override PageStatePersister GetStatePersister ()
		{
			if (persister == null)
				persister = new TestPersister(Page);
			return persister;
		}

		protected internal override string GetPostBackFormReference (string formId)
		{
			return String.Format("/* testFormReference */{0}", 
				base.GetPostBackFormReference (formId));
		}
		
		public override NameValueCollection DeterminePostBackMode ()
		{
			NameValueCollection origRequestValues = base.DeterminePostBackMode ();
			if (origRequestValues == null)
				return null;
			NameValueCollection requestValues = new NameValueCollection ();
			requestValues.Add (origRequestValues);
			requestValues ["DeterminePostBackModeTestField"] 
				= "DeterminePostBackModeTestValue";
			return requestValues;
		}
		
		internal new void RenderPostBackEvent (HtmlTextWriter w,
		                                       string target,
		                                       string argument,
		                                       string softKeyLabel,
		                                       string text,
		                                       string postUrl,
		                                       string accessKey,
		                                       bool encode)
		{
			base.RenderPostBackEvent (w, target, argument, softKeyLabel, text, postUrl,
				accessKey, encode);
		}
		
	}
	
	class TestPersister : HiddenFieldPageStatePersister
	{
		public TestPersister (Page p) : base (p)
		{
		}
	}

	public class TestPageWithAdapter : Page
	{
		public global::System.Web.UI.Adapters.PageAdapter page_adapter;
		
		public TestPageWithAdapter () : base ()
		{
			page_adapter = new TestAdapter ();
			WebTest t = WebTest.CurrentTest;
			if (t != null)
				t.Invoke (this);
		}
		
		protected override global::System.Web.UI.Adapters.ControlAdapter ResolveAdapter ()
		{
			return page_adapter;
		}
		
		public new PageStatePersister PageStatePersister {
			get { return base.PageStatePersister; }
		}
	}
	
	public class TestControlAdapter : ControlAdapter
	{
	}

	public class TestPageWithControlAdapter : Page
	{
		private global::System.Web.UI.Adapters.ControlAdapter control_adapter;
		
		public TestPageWithControlAdapter () : base ()
		{
			control_adapter = new TestControlAdapter ();
			WebTest t = WebTest.CurrentTest;
			if (t != null)
				t.Invoke (this);
		}
		
		protected override global::System.Web.UI.Adapters.ControlAdapter ResolveAdapter ()
		{
			return control_adapter;
		}
	}
#endif
}
