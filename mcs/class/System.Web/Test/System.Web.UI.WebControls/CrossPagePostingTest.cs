//
// Tests for System.Web.UI.WebControls.CrossPagePostingTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using System.Web.UI;
using System.Threading;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls
{


	[TestFixture]
	public class CrossPagePosting
	{
		[TestFixtureSetUp]
		public void Set_Up ()
		{
#if DOT_NET
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.CrossPagePosting1.aspx", "CrossPagePosting1.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.CrossPagePosting2.aspx", "CrossPagePosting2.aspx");
#else
			WebTest.CopyResource (GetType (), "CrossPagePosting1.aspx", "CrossPagePosting1.aspx");
			WebTest.CopyResource (GetType (), "CrossPagePosting2.aspx", "CrossPagePosting2.aspx");
#endif
		}

		[SetUp]
		public void SetupTestCase ()
		{
			Thread.Sleep (100);
		}

		[Test]
		[Category ("NunitWeb")]
		public void CrossPagePosting_BaseFixture ()
		{
			WebTest t = new WebTest ("CrossPagePosting1.aspx");
			string html = t.Run ();

			if (html.IndexOf ("LinkButtonText") < 0)
				Assert.Fail ("Link button not created fail");

			PageDelegates pd = new PageDelegates ();
			pd.Load = Load;
			t.Invoker = new PageInvoker (pd);

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			
			fr.Controls["__EVENTTARGET"].Value = "LinkButton1";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			
			t.Request = fr;
			fr.Url = "CrossPagePosting2.aspx";
			html = t.Run ();
			if (html.IndexOf ("CrossedPostbackPage") < 0)
				Assert.Fail ("CrossPagePosting removeing to target page fail");
		}

		public static void Load (Page p)
		{
			if (p.PreviousPage == null)
				Assert.Fail ("Post back page does not exist fail");

		}

		[Test]
		[Category ("NunitWeb")]
		public void CrossPagePosting_BaseFlow ()
		{
			// NOTE!!! Test user data assigned stright on aspx pages
			WebTest t = new WebTest ("CrossPagePosting1.aspx");
			string html = t.Run ();

			if (html.IndexOf ("LinkButtonText") < 0)
				Assert.Fail ("Link button not created fail");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls["__EVENTTARGET"].Value = "LinkButton1";
			fr.Controls["__EVENTARGUMENT"].Value = "";

			t.Request = fr;
			fr.Url = "CrossPagePosting2.aspx";
			html = t.Run ();
			ExpectTestResult (t);	
		}

		private void ExpectTestResult (WebTest t)
		{
			// NOTE!!! Test user data assigned stright on aspx pages
			if (t.UserData == null)
				Assert.Fail ("User data not created fail");

			ArrayList list = t.UserData as ArrayList;
		        if (list == null)
				Assert.Fail ("User data type failed");
			
			Assert.AreEqual ("Page1 - Load", list[0].ToString (), "Application flow #1");
			Assert.AreEqual ("Page1 - LoadComplete", list[1].ToString (), "Application flow #2");
			Assert.AreEqual ("Page1 - PreRender", list[2].ToString (), "Application flow #3");
			Assert.AreEqual ("Page1 - SaveStateComplete", list[3].ToString (), "Application flow #4");
			Assert.AreEqual ("Page1 - Unload", list[4].ToString (), "Application flow #5");
			Assert.AreEqual ("Page2 - OnInit", list[5].ToString (), "Application flow #6");
			Assert.AreEqual ("Page2 - Load", list[6].ToString (), "Application flow #7");
			Assert.AreEqual ("Page2 - LoadComplete", list[7].ToString (), "Application flow #8");
			Assert.AreEqual ("Page2 - PreRender", list[8].ToString (), "Application flow #9");
			Assert.AreEqual ("Page2 - SaveStateComplete", list[9].ToString (), "Application flow #10");
			Assert.AreEqual ("Page2 - Unload", list[10].ToString (), "Application flow #11");
		}

		[TestFixtureTearDown]
		public void Unload ()
		{
			WebTest.Unload ();
		}
	}
}
#endif
