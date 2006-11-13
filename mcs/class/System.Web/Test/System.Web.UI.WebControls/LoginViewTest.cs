//
// LoginViewTest.cs	- Unit tests for System.Web.UI.WebControls.LoginView
//
// Author:
//	Konstantin Triger <kostat@mainsoft.com>
//	Yoni Klain (yonik@mainsoft.com)
//
// Copyright (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls {
	
	public class LoginViewTemplate : WebControl, ITemplate {
		public LoginViewTemplate() {
			ID = "kuku";
		}
			
		void ITemplate.InstantiateIn(Control container) {
			container.Controls.Add(this);
		}
	}

	public class TestLoginView : LoginView {
		
		public void DoEnsureChildControls() {
			base.EnsureChildControls ();
		}

		public void DoOnInit (EventArgs e){
			base.OnInit (e);
		}

		public void DoOnPreRender (EventArgs e){
			base.OnPreRender (e);
		}

		public void DoOnViewChanged (EventArgs e){
			base.OnViewChanged (e);
		}

		public void DoOnViewChanging (EventArgs e){
			base.OnViewChanging (e);
		}

		public void DoSetDesignModeState (IDictionary data){
			base.SetDesignModeState (data);
		}

		public IDictionary DoGetDesignModeState (){
			return base.GetDesignModeState ();
		}

		public object DoSaveControlState (){
			return base.SaveControlState ();
		}

		public void DoLoadControlState (object savedState){
			base.LoadControlState (savedState);
		}

		
	}

	[TestFixture]
	public class LoginViewTest
	{

		[TestFixtureSetUp]
		public void CopyTestResources ()
		{
#if DOT_NET
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.LoginViewTest1.aspx", "LoginViewTest1.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.WebLogin.config", "Web.config");
#else
			WebTest.CopyResource (GetType (), "LoginViewTest1.aspx", "LoginViewTest1.aspx");
			WebTest.CopyResource (GetType (), "WebLogin.config", "Web.config");
#endif
		}

		[Test]
		public void LoginView_DefaultProperties ()
		{
			// Note : ALL PROTECTED PROPERTIES ARE INHERITED FROM BASE
			LoginView l = new LoginView ();
			Assert.AreEqual (null, l.AnonymousTemplate, "AnonymousTemplate");
			Assert.AreEqual (true, l.EnableTheming, "EnableTheming");
			Assert.AreEqual (null, l.LoggedInTemplate, "LoggedInTemplate");
			Assert.AreEqual ("System.Web.UI.WebControls.RoleGroupCollection", l.RoleGroups.GetType ().ToString (), "RoleGroups");
			Assert.AreEqual ("", l.SkinID, "SkinID");
		}

		[Test]
		public void LoginView_DefaultPropertiesNotWorking ()
		{
			LoginView l = new LoginView ();
			Assert.AreEqual ("System.Web.UI.ControlCollection", l.Controls.GetType ().ToString (), "Controls");
		}

		public void LoginView_AssignProperties ()
		{
			// Note : ALL PROTECTED PROPERTIES ARE INHERITED FROM BASE
			LoginView l = new LoginView ();
			l.EnableTheming = false;
			Assert.AreEqual (false, l.EnableTheming, "EnableTheming");
		}

		[Test]
		public void LoginView_Controls ()
		{
			TestLoginView l = new TestLoginView ();
			l.AnonymousTemplate = new LoginViewTemplate ();
			l.DoEnsureChildControls ();
			ControlCollection col = l.Controls as ControlCollection;
			Assert.IsNotNull (col, "ControlCollection");
			Assert.AreEqual (1, col.Count, "Controls");
		}

		[Test]
		public void LoginView_AnonymousTemplate ()
		{
			TestLoginView l = new TestLoginView ();
			l.AnonymousTemplate = new LoginViewTemplate ();
			l.DoEnsureChildControls ();
			Assert.IsNotNull (l.FindControl ("kuku"), "AnonymousTemplate");
		}

		[Test]
		public void LoginView_LoggedInTemplate ()
		{
			TestLoginView l = new TestLoginView ();
			ITemplate template = new LoginViewTemplate ();
			l.LoggedInTemplate = template;
			l.DoEnsureChildControls ();
			Assert.AreEqual (template, l.LoggedInTemplate, "LoggedInTemplate");
		}

		[Test]
		public void LoginView_DisignMode ()
		{
			// Not TODO
		}

		[Test]
		public void LoginView_State ()
		{
			// IT IS NOTHING SAVED IN  STATE OBJECT IN NON PAGE CONTEXT
			TestLoginView l = new TestLoginView ();
			l.AnonymousTemplate = new LoginViewTemplate ();
			l.DoEnsureChildControls ();
			object state = l.DoSaveControlState ();
			Assert.AreEqual (null, state, "StateObgect");
		}

		[Test]
		[Category ("NunitWeb")]
		// This test is not compleated , must be continued after adding 
		// possibilities to test framework (cookies and redirect)
		public void LoginView_Render ()
		{
			WebTest t = new WebTest ("LoginViewTest1.aspx");
			string result = t.Run ();
			if (result.IndexOf ("You are not logged in. Please Login.") < 0)
				Assert.Fail ("Not Logged In template fail");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("LoginView1$Login1$Password");
			fr.Controls.Add ("LoginView1$Login1$UserName");
			fr.Controls.Add ("LoginView1$Login1$LoginButton");

			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["LoginView1$Login1$Password"].Value = "TestPass";
			fr.Controls["LoginView1$Login1$UserName"].Value = "TestUser";
			fr.Controls["LoginView1$Login1$LoginButton"].Value = "Log In";
			t.Request = fr;
			result = t.Run ();
		}

		//events
		static bool checker;

		[Test]
		public void LoginView_EventInit ()
		{
			TestLoginView l = new TestLoginView ();
			l.Init += new EventHandler (event_handler);
			Assert.AreEqual (false, checker, "BeforeInit");
			l.DoOnInit (new EventArgs ());
			Assert.AreEqual (true, checker, "AfterInit");
		}

		[Test]
		public void LoginView_PreRender ()
		{
			TestLoginView l = new TestLoginView ();
			l.PreRender += new EventHandler (event_handler);
			l.DoOnPreRender (new EventArgs ());
			Eventassert ("AfterPreRender");
		}

		[Test]
		public void LoginView_ViewChanged ()
		{
			TestLoginView l = new TestLoginView ();
			l.ViewChanged += new EventHandler (event_handler);
			l.DoOnViewChanged (new EventArgs ());
			Eventassert ("AfterViewChanged");
		}

		[Test]
		public void LoginView_ViewChanging ()
		{
			TestLoginView l = new TestLoginView ();
			l.ViewChanging += new EventHandler (event_handler);
			l.DoOnViewChanging (new EventArgs ());
			Eventassert ("AfterViewChanging");
		}

		void event_handler (object sender, EventArgs e)
		{
			checker = true;
		}

		private static void Eventassert (string message)
		{
			Assert.IsTrue (checker, message);
			checker = false;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void LoginView_Focus ()
		{
			LoginView l = new LoginView ();
			l.Focus ();
		}
	}
}

#endif
