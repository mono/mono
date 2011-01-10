//
// Tests for System.Web.UI.Adapters.PageAdapter
//
// Author:
//	Dean Brettle (dean@brettle.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && !TARGET_DOTNET
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Adapters;
using System.Web.Configuration;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.Adapters
{
	[TestFixture]
	public class PageAdapterTest
	{
		private MyPageAdapter mpa;
		private MyPage page;

		[TestFixtureSetUp]
		public void SetUpTest ()
		{
			WebTest.CopyResource (GetType (), "PageWithAdapter.aspx", "PageWithAdapter.aspx");
		}
		
		[SetUp]
		public void SetUp()
		{
			page = new MyPage();
			mpa = new MyPageAdapter (page);
		}
		
		[Test]
		public void CacheVaryByHeaders ()
		{
			Assert.IsNull (mpa.CacheVaryByHeaders, "CacheVaryByHeaders #1");
		}
		
		[Test]
		public void CacheVaryByParams ()
		{
			Assert.IsNull (mpa.CacheVaryByParams, "CacheVaryByParams #1");
		}
		
		[Test]
		public void GetStatePersister ()
		{
			PageStatePersister persister = mpa.GetStatePersister ();
			Assert.AreEqual (typeof(HiddenFieldPageStatePersister), 
				persister.GetType (), "GetStatePersister #1");
		}
		
		[Test]
		public void GetPostBackFormReference ()
		{
			Assert.AreEqual("document.forms['test']", mpa.GetPostBackFormReference ("test"),
				"GetPostBackFormReference #1");
		}
		
		[Test]
		public void DeterminePostBackMode ()
		{
			Assert.AreEqual(page.MyDeterminePostBackMode (), mpa.DeterminePostBackMode (),
				"DeterminePostBackMode #1");
		}

		[Test]
		public void RenderBeginHyperlink_NoEncode ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			mpa.RenderBeginHyperlink (htw, "url with &, <, and \"", false, "softKeyLabel");
			Assert.AreEqual("<a href=\"url with &, <, and \"\">", sw.ToString(),
				"RenderBeginHyperlink_NoEncode #1");
		}

		[Test]
		public void RenderBeginHyperlink_Encode ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			mpa.RenderBeginHyperlink (htw, "url with &, <, and \"", true, "softKeyLabel");
			Assert.AreEqual("<a href=\"url with &amp;, &lt;, and &quot;\">", sw.ToString(),
				"RenderBeginHyperlink_Encode #1");
		}

		[Test]
		public void RenderBeginHyperlink_NoEncode_AccessKey ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			mpa.RenderBeginHyperlink (htw, "url with &, <, and \"", false, "softKeyLabel", "X");
			Assert.AreEqual("<a href=\"url with &, <, and \"\" accesskey=\"X\">",
				sw.ToString(), "RenderBeginHyperlink_NoEncode_AccessKey #1");
		}

		[Test]
		public void RenderBeginHyperlink_Encode_AccessKey ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			mpa.RenderBeginHyperlink (htw, "url with &, <, and \"", true, "softKeyLabel", "X");
			Assert.AreEqual("<a href=\"url with &amp;, &lt;, and &quot;\" accesskey=\"X\">",
				sw.ToString(), "RenderBeginHyperlink_Encode_AccessKey #1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RenderBeginHyperlink_LongAccessKey ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			mpa.RenderBeginHyperlink (htw, "url with &, <, and \"", true, "softKeyLabel", "accessKey");
		}

		[Test]
		public void EndHyperlink ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			mpa.RenderBeginHyperlink (htw, "url", false, null);
			mpa.RenderEndHyperlink (htw);
			Assert.AreEqual("<a href=\"url\"></a>",	sw.ToString(), "RenderEndHyperlink #1");
		}

		[Test]
		[Category ("NunitWeb")]
		public void RenderPostBackEvent ()
		{
			WebTest t = new WebTest ("PageWithAdapter.aspx");
			PageDelegates pd = new PageDelegates ();
			pd.SaveStateComplete = RenderPostBackEvent_OnSaveStateComplete;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			File.WriteAllText("response.html", html);
		}
		
		public static void RenderPostBackEvent_OnSaveStateComplete (Page p)
		{
			TestPageWithAdapter pageWithAdapter = (TestPageWithAdapter) p;
			TestAdapter testAdapter = (TestAdapter)pageWithAdapter.PageAdapter;
			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter htw = new HtmlTextWriter (sw);
				testAdapter.RenderPostBackEvent (htw, "target", "argument", "softKeyLabel", "text", "postUrl", "X", true);
				string origHtml = "<a href=\"postUrl?__VIEWSTATE=DAAAAA%3d%3d&amp;__EVENTTARGET=target&amp;__EVENTARGUMENT=argument&amp;__PREVIOUSPAGE=/NunitWeb/PageWithAdapter.aspx\" accesskey=\"X\">text</a>";
				string renderedHtml = sw.ToString ();
				HtmlDiff.AssertAreEqual(origHtml, renderedHtml, "RenderPostBackEvent #1");
			}
			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter htw = new HtmlTextWriter (sw);
				testAdapter.RenderPostBackEvent (htw, "target", "argument", "softKeyLabel", "text", "postUrl", "X", false);
				string origHtml = "<a href=\"postUrl?__VIEWSTATE=DAAAAA%3d%3d&__EVENTTARGET=target&__EVENTARGUMENT=argument&__PREVIOUSPAGE=/NunitWeb/PageWithAdapter.aspx\" accesskey=\"X\">text</a>";
				string renderedHtml = sw.ToString ();
				HtmlDiff.AssertAreEqual(origHtml, renderedHtml, "RenderPostBackEvent #2");
			}
			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter htw = new HtmlTextWriter (sw);
				string origHtml = "<a href=\"postUrl?__VIEWSTATE=DAAAAA%3d%3d&amp;__EVENTTARGET=target&amp;__EVENTARGUMENT=argument&amp;__PREVIOUSPAGE=/NunitWeb/PageWithAdapter.aspx\" accesskey=\"X\">text</a>";
				testAdapter.RenderPostBackEvent (htw, "target", "argument", "softKeyLabel", "text", "postUrl", "X");
				string renderedHtml = sw.ToString ();
				HtmlDiff.AssertAreEqual(origHtml, renderedHtml, "RenderPostBackEvent #3");
			}
			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter htw = new HtmlTextWriter (sw);
				string origHtml = "<a href=\"/NunitWeb/PageWithAdapter.aspx?__VIEWSTATE=DAAAAA%3d%3d&amp;__EVENTTARGET=target&amp;__EVENTARGUMENT=argument&amp;__PREVIOUSPAGE=/NunitWeb/PageWithAdapter.aspx\">text</a>";
				testAdapter.RenderPostBackEvent (htw, "target", "argument", "softKeyLabel", "text");
				string renderedHtml = sw.ToString ();
				HtmlDiff.AssertAreEqual(origHtml, renderedHtml, "RenderPostBackEvent #4");
			}

		}
		
		[Test]
		public void RadioButtons ()
		{
			ArrayList group = new ArrayList (mpa.GetRadioButtonsByGroup ("Group1"));
			Assert.AreEqual (0, group.Count, "RadioButtons #0");

			RadioButton g1b1 = new RadioButton ();
			g1b1.GroupName = "Group1";
			mpa.RegisterRadioButton(g1b1);
			RadioButton g1b2 = new RadioButton ();
			g1b2.GroupName = "Group1";
			mpa.RegisterRadioButton(g1b2);	
			RadioButton g2b1 = new RadioButton ();
			g2b1.GroupName = "Group2";
			mpa.RegisterRadioButton (g2b1);
			RadioButton noGroupB1 = new RadioButton ();
			mpa.RegisterRadioButton (noGroupB1);
			
			Assert.AreEqual (0, mpa.GetRadioButtonsByGroup ("Non-existent group").Count, "RadioButtons #1");

			ArrayList group1 = new ArrayList (mpa.GetRadioButtonsByGroup ("Group1"));			
			Assert.AreEqual (2, group1.Count, "RadioButtons #2");
			Assert.IsTrue (group1.Contains (g1b1), "RadioButtons #3");
			Assert.IsTrue (group1.Contains (g1b2), "RadioButtons #4");
			
			ArrayList group2 = new ArrayList (mpa.GetRadioButtonsByGroup ("Group2"));			
			Assert.AreEqual (1, group2.Count, "RadioButtons #5");
			Assert.IsTrue (group2.Contains (g2b1), "RadioButtons #6");
			
			ArrayList noGroup = new ArrayList (mpa.GetRadioButtonsByGroup (""));			
			Assert.AreEqual (1, noGroup.Count, "RadioButtons #7");
			Assert.IsTrue (noGroup.Contains (noGroupB1), "RadioButtons #8");
		}

		[Test]
		public void ClientState ()
		{
			page.RawViewState = "test";
			Assert.AreEqual ("test", mpa.ClientState, "ClientState #1");
		}
		
		[Test]
		public void TransformText ()
		{
			Assert.AreEqual ("test", mpa.TransformText("test"), "TransformText #1");
			Assert.IsNull (mpa.TransformText(null), "TransformText #2");
		}
		
		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

		class MyPageAdapter : SystemWebTestShim.PageAdapter
		{
			internal MyPageAdapter (MyPage p) : base (p)
			{
			}
			
			new internal string ClientState {
				get { return base.ClientState; }
			}

			new internal string GetPostBackFormReference (string s)
			{
				return base.GetPostBackFormReference (s);
			}
		}

		class MyPage : SystemWebTestShim.Page
		{
			NameValueCollection post_back_mode = new NameValueCollection ();
			
			override protected internal NameValueCollection DeterminePostBackMode ()
			{
				return post_back_mode;
			}

			internal NameValueCollection MyDeterminePostBackMode ()
			{
				return DeterminePostBackMode ();
			}
		}
	}
	
}
#endif
