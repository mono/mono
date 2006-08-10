//
// Tests for System.Web.UI.WebControls.CallBackTest.cs
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



using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using System.Web.UI;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls
{


	[TestFixture]
	public class CallBackTest
	{
		[TestFixtureSetUp]
		public void Set_Up ()
		{
#if DOT_NET
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.CallbackTest1.aspx", "CallbackTest1.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.CallbackTest2.aspx", "CallbackTest2.aspx");
#else
			WebTest.CopyResource (GetType (), "CallbackTest1.aspx", "CallbackTest1.aspx");
			WebTest.CopyResource (GetType (), "CallbackTest2.aspx", "CallbackTest2.aspx");
#endif
		}

		[SetUp]
		public void SetupTestCase ()
		{
			Thread.Sleep (100);
		}

		[Test]
		[Category("NunitWeb")]
		[Category("NotWorking")]
		public void CallBackResulrValues ()
		{
			WebTest t = new WebTest ("CallbackTest1.aspx");
			string html = t.Run ();
			PageDelegates pd = new PageDelegates ();
			pd.Load = Load;
			t.Invoker = new PageInvoker (pd);

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("__CALLBACKID");
			fr.Controls.Add ("__CALLBACKPARAM");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["__CALLBACKID"].Value = "__Page";
			fr.Controls["__CALLBACKPARAM"].Value = "monitor";

			t.Request = fr;
			html = t.Run ();
			
			// Into result string the last 2 variables shows if events been done
			// first - RaiseCallbackEvent
			// second - GetCallbackResult

			Assert.AreEqual ("0|12|true|true", html, "CallBack#1");

			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["__CALLBACKID"].Value = "__Page";
			fr.Controls["__CALLBACKPARAM"].Value = "laptop";

			t.Request = fr;
			html = t.Run ();

			// Into result string the last 2 variables shows if events been done
			// first - RaiseCallbackEvent
			// second - GetCallbackResult

			Assert.AreEqual ("0|10|true|true", html, "CallBack#2");
		}

		public static void Load (Page p)
		{
			Assert.AreEqual (true, p.IsCallback, "IsCallbackDoneFail");
		}

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void CallBackFlow ()
		{
			WebTest t = new WebTest ("CallbackTest2.aspx");
			string html = t.Run ();

			PageDelegates pd = new PageDelegates ();
			pd.Load = callBackFlow_Load;
			t.Invoker = new PageInvoker (pd);

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("__CALLBACKID");
			fr.Controls.Add ("__CALLBACKPARAM");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["__CALLBACKID"].Value = "__Page";
			fr.Controls["__CALLBACKPARAM"].Value = "";
			t.Request = fr;
			
			html = t.Run ();

			// GetCallbackResult return string contained all flow functions name

			Assert.AreEqual ("0||PreInit|Init|InitComplete|PreLoad|Load|LoadComplete|RaiseCallbackEvent|GetCallbackResult", html, "CallBackPageFlow"); 
		}

		public static void callBackFlow_Load (Page p)
		{
			Assert.AreEqual (true, p.IsCallback, "IsCallbackDoneFail");
		}

		[TestFixtureTearDown]
		public void Unload ()
		{
			WebTest.Unload ();
		}
	}
}
