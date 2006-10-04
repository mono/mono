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
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;

namespace MonoTests.System.Web.UI {

	class TestPage : Page {

		private HttpContext ctx;

		// don't call base class (so _context is never set to a non-null value)
		protected override HttpContext Context {
			get {
				if (ctx == null) {
					ctx = new HttpContext (null);
					ctx.User = new GenericPrincipal (new GenericIdentity ("me"), null);
				}
				return ctx;
			}
		}
	}

	class TestPage2 : Page {

		private HttpContext ctx;

		// don't call base class (so _context is never set to a non-null value)
		protected override HttpContext Context {
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
		public void CopyTestResources ()
		{
#if DOT_NET
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.PageValidationTest.aspx", "PageValidationTest.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.PageLifecycleTest.aspx", "PageLifecycleTest.aspx");
#else
			WebTest.CopyResource (GetType (), "PageValidationTest.aspx", "PageValidationTest.aspx");
			WebTest.CopyResource (GetType (), "PageLifecycleTest.aspx", "PageLifecycleTest.aspx");
#endif
		}

		[SetUp]
		public void SetUpTest ()
		{
			Thread.Sleep (100);
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

#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderOnPreInit ()
		{
			Thread.Sleep (200); 
			PageDelegate pd = new PageDelegate (Page_OnPreInit);
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                                PreInit
                                    </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderInit");
			Thread.Sleep (200); 
			WebTest.Unload ();
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
			Thread.Sleep (200); 
			PageDelegate pd = new PageDelegate (CheckHeader);
			WebTest t = new WebTest (PageInvoker.CreateOnInit (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderInit");
			Thread.Sleep (200); 
			WebTest.Unload ();

		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderInitComplete ()
		{
			Thread.Sleep (200); 
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
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreLoad ()
		{
			Thread.Sleep (200); 
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
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderLoad ()
		{
			Thread.Sleep (200); 
			PageDelegate pd = new PageDelegate (CheckHeader);
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderLoad");
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderLoadComplete ()
		{
			Thread.Sleep (200); 
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
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreRender ()
		{
			Thread.Sleep (200);
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
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreRenderComplete ()
		{
			Thread.Sleep (200); 
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
			Thread.Sleep (200); 
			WebTest.Unload ();
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
		public void Page_ValidationGroup () {
			new WebTest (PageInvoker.CreateOnLoad (Page_ValidationGroup_Load)).Run ();
		}

		public static void Page_ValidationGroup_Load (Page page) {
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
#endif
#if NET_2_0

		// This test are testing validation fixture using RequiredFieldValidator for example
		
		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidationCollection () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ValidationCollectionload));
			t.Run ();
		}

		public static void ValidationCollectionload (Page p)
		{
			RequiredFieldValidator validator = new RequiredFieldValidator ();
			validator.ID = "v";
			RequiredFieldValidator validator1 = new RequiredFieldValidator ();
			validator.ID = "v1";
			p.Controls.Add (validator);
			p.Controls.Add (validator1);
			Assert.AreEqual (2, p.Validators.Count, "Validators collection count fail");
			Assert.AreEqual (true, p.Validators[0].IsValid, "Validators collection value#1 fail");
			Assert.AreEqual (true, p.Validators[1].IsValid, "Validators collection value#2 fail");
		}

		[Test]
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
#endif

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}
}
