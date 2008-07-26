//
// System.Web.HttpBrowserCapabilitiesTest.cs - Unit tests for System.Web.HttpBrowserCapabilities
//
// Author:
//	Adar Wesley <adarw@mainsoft.com>
//
// Copyright (C) 2007 Mainsoft, Inc (http://www.mainsoft.com)
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
using System.Web;
using System.Web.UI;
using System.IO;
using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.System.Web.UI;
using System.Text;
using System.Web.Configuration;
using System.Threading;

namespace MonoTests.System.Web
{
	[TestFixture]
	[Ignore ("Pending fix for bug 351878")]
	public class AppBrowsersTest
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp ()
		{
			WebTest.CleanApp(); 
			WebTest.CopyResource(typeof(HttpBrowserCapabilitiesTest), "TestCapability.browser", 
				Path.Combine("App_Browsers", "TestCapability.browser"));
			WebTest.CopyResource (GetType (), "adapters.browser",
	        	Path.Combine("App_Browsers", "adapters.browser"));
		}
		
		[TestFixtureTearDown]
		public void TestFixtureTearDown ()
		{
			WebTest.CleanApp();
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void AppBrowsersCapabilities () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (AppBrowsersCapabilities_OnLoad));
			t.Request.UserAgent = "testUserAgent";
			t.Run ();
		}
				
		public static void AppBrowsersCapabilities_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;

			Assert.IsFalse (String.IsNullOrEmpty(caps.Browser), "Browser");
			Assert.AreEqual ("testUserAgent", request.UserAgent, "AppBrowsersCapabilities #1");
			Assert.AreEqual ("testUserAgent", caps[""], "AppBrowsersCapabilities #2");
			Assert.AreEqual ("default", caps["notChanged"], "AppBrowsersCapabilities #3");
			Assert.AreEqual ("uaInOrig:testUserAgent", caps["capturedInOrigNode"], "AppBrowsersCapabilities #4");
			Assert.AreEqual ("added", caps["addedInRefNode"], "AppBrowsersCapabilities #5");
			Assert.AreEqual ("changed", caps["changedInRefNode"], "AppBrowsersCapabilities #6");
			Assert.AreEqual ("uaInRef:testUserAgent", caps["capturedInRefNode"], "AppBrowsersCapabilities #7");
			// This property is inherited from browscap.ini
			Assert.AreEqual ("0", caps["majorver"], "AppBrowsersCapabilities #8");
			// This capability uses multiple substitutions
			Assert.AreEqual ("uaInOrig:testUserAgent uaInRef:testUserAgent", caps["multipleSubstitutions"],
			                 "AppBrowsersCapabilities #9");
			Assert.AreEqual ("10%*$100=$10", caps["specialCharsInValue"],
			                 "AppBrowsersCapabilities #10");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void CompatBrowserIE7 () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (CompatBrowserIE7_OnLoad));
			t.Request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";
			t.Run ();
		}
				
		public static void CompatBrowserIE7_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;

			Assert.AreEqual ("added", caps["addedInIE6to9RefNode"], "CompatBrowserIE7 #1");
		}

		[Test]
		[Category ("NunitWeb")]
		public void TagWriter() 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (TagWriter_OnLoad));
			t.Request.UserAgent = "testUserAgent";
			t.Run ();
		}

		public static void TagWriter_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;

			Assert.AreEqual (typeof(CustomHtmlTextWriter), caps.TagWriter, "TagWriter #1");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void CreateHtmlTextWriter() 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (CreateHtmlTextWriter_OnLoad));
			t.Request.UserAgent = "testUserAgent";
			t.Run ();
			Assert.IsTrue(t.Response.Body.Contains(@"renderedby=""CustomHtmlTextWriter"""), 
				"CreateHtmlTextWriter #2");
		}

		public static void CreateHtmlTextWriter_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;

			Assert.AreEqual (typeof(CustomHtmlTextWriter),
				caps.CreateHtmlTextWriter(new StringWriter()).GetType(),
				"CreateHtmlTextWriter #1");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void Adapter ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnInit (Adapter_Init));
			t.Request.UserAgent = "testUserAgent";
			string html = t.Run ();
		}
		
		public static void Adapter_Init (Page p)
		{
		        Customadaptercontrol ctrl = new Customadaptercontrol ();
		        p.Controls.Add (ctrl);
		        ctrl.Load += new EventHandler (Adapter_ctrl_Load);
		}
		
		static void Adapter_ctrl_Load (object sender, EventArgs e)
		{
		        Assert.IsNotNull (((Customadaptercontrol) sender).Adapter, "Adapter Failed#1");
		        Assert.AreEqual ("MonoTests.System.Web.UI.Customadapter", ((Customadaptercontrol) sender).Adapter.ToString (),
		        	"Adapter Failed#2");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void ResolveAdapter_1 ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnInit (ResolveAdapter_Init));
			t.Request.UserAgent = "testUserAgent";
			string html = t.Run ();
		}
		
		public static void ResolveAdapter_Init (Page p)
		{
		        Customadaptercontrol ctrl = new Customadaptercontrol ();
		        p.Controls.Add (ctrl);
		        ctrl.Load += new EventHandler (ResolveAdapter_ctrl_Load);

		        Customadaptercontrol derivedCtrl = new DerivedCustomadaptercontrol ();
		        p.Controls.Add (derivedCtrl);
		        derivedCtrl.Load += new EventHandler (ResolveAdapter_derivedCtrl_Load);
		}
		
		static void ResolveAdapter_ctrl_Load (object sender, EventArgs e)
		{
		        Assert.IsNotNull (((Customadaptercontrol) sender).ResolveAdapter (), "ResolveAdapter Failed#1");
		        Assert.AreEqual ("MonoTests.System.Web.UI.Customadapter", ((Customadaptercontrol) sender).ResolveAdapter ().ToString (),
		        	"ResolveAdapter Failed#2");
		}
		
		static void ResolveAdapter_derivedCtrl_Load (object sender, EventArgs e)
		{
		        Assert.IsNotNull (((Customadaptercontrol) sender).ResolveAdapter (), "ResolveAdapter Failed#2");
		        Assert.AreEqual ("MonoTests.System.Web.UI.Customadapter", ((Customadaptercontrol) sender).ResolveAdapter ().ToString (),
		        	"ResolveAdapter Failed#2");
		}
	}
	
	public class CustomHtmlTextWriter : HtmlTextWriter
	{
		public CustomHtmlTextWriter (TextWriter tw)
			: base (tw)
		{
		}
		
		public override void WriteBeginTag(string s)
		{
			AddAttribute("renderedby", "CustomHtmlTextWriter");
			base.WriteBeginTag(s);
		}
	}
	
	class DerivedCustomadaptercontrol : Customadaptercontrol
	{
		internal  DerivedCustomadaptercontrol () : base ()
		{
		}
	}


}
#endif
