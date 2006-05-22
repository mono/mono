//
// Tests for System.Web.UI.WebControls.MasterPageTest.cs
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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using MyWebControl = System.Web.UI.WebControls;
using System.Collections;
using NunitWeb;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{
	class PokerMasterPage : MasterPage
	{
		public PokerMasterPage ()
		{
			TrackViewState ();
		}
		public StateBag StateBag
		{
			get { return base.ViewState; }
		}
		public new IDictionary ContentTemplates ()
		{
			return base.ContentTemplates;
		}
		public new void AddContentTemplate (string templateName, ITemplate template)
		{
			base.AddContentTemplate (templateName, template);
		}
	}

	
	[TestFixture]
	public class MasterPageTest
	{

		[Test]
		public void MasterPage_DefaultProperties ()
		{
			PokerMasterPage pmp = new PokerMasterPage ();
			Assert.AreEqual (null, pmp.Master, "Master Property");
			Assert.AreEqual (null, pmp.MasterPageFile, "MasterPageFile Property");
			IDictionary i = pmp.ContentTemplates ();
			Assert.AreEqual (null,i,"ContentTemplates");
		}

		[Test]
		public void MasterPage_Render()
		{
			string PageRenderHtml = Helper.Instance.RunInPageWithMaster (TestRenderDefault, null);
			Assert.AreEqual (-1, PageRenderHtml.IndexOf ("Master header text"), "Master#1");
			
			if (PageRenderHtml.IndexOf ("Page main text") < 0) {
				Assert.Fail ("Master#2");
			}
			
			Assert.AreEqual (-1, PageRenderHtml.IndexOf ("Master main text"), "Master#3");
			Assert.AreEqual (-1, PageRenderHtml.IndexOf ("Master dynamic text"), "Master#4");

			if (PageRenderHtml.IndexOf ("Page dynamic text") < 0) {
				Assert.Fail ("Master#5");
			}

			if (PageRenderHtml.IndexOf ("My master page footer") < 0) {
				Assert.Fail ("Master#6");
			}

			if (PageRenderHtml.IndexOf ("Master page content text") < 0) {
				Assert.Fail ("Master#7");
			}

		}

		static void TestRenderDefault (HttpContext c, Page p, object param)
		{
			p.Form.Controls.Add(new LiteralControl("Page dynamic text"));
		}

	[	Test]
	 	[ExpectedException (typeof(HttpException))]
		public void MasterPage_AddContentTemplate ()
		{
			PokerMasterPage pmp = new PokerMasterPage();
			ITemplate it = null;
			pmp.AddContentTemplate ("myTemplate", it);
			pmp.AddContentTemplate ("myTemplate", it);
		}
		
		[TestFixtureTearDown]
		public void TearDown ()
		{
			Helper.Unload ();
		}
	}
}
#endif