//
// HtmlContainerControlTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlContainerControl
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

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlContainerControl : HtmlContainerControl {

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
	}

	[TestFixture]
	public class HtmlContainerControlTest {

		[Test]
		public void DefaultProperties ()
		{
			TestHtmlContainerControl cc = new TestHtmlContainerControl ();
			Assert.AreEqual (0, cc.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, cc.InnerHtml, "InnerHtml");
			Assert.AreEqual (String.Empty, cc.InnerText, "InnerText");

			Assert.AreEqual ("span", cc.TagName, "TagName");
			Assert.AreEqual (0, cc.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void InnerText () {
			HtmlButton c = new HtmlButton ();
			DataBoundLiteralControl db = new DataBoundLiteralControl (1, 0);
			db.SetStaticString (0, "FFF");
			c.Controls.Add (db);
			Assert.AreEqual ("FFF", c.InnerText);
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void InnerText2 () {
			HtmlButton c = new HtmlButton ();
			DesignerDataBoundLiteralControl x = new DesignerDataBoundLiteralControl ();
			x.Text = "FFF";
			c.Controls.Add (x);
			string s = c.InnerText;
		}

		[Test]
		public void InnerText3 () {
			HtmlButton c = new HtmlButton ();
			LiteralControl x = new LiteralControl ();
			x.Text = "FFF";
			c.Controls.Add (x);
			Assert.AreEqual("FFF", c.InnerText);
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void InnerText4 () {
			HtmlButton c = new HtmlButton ();
			LiteralControl x = new LiteralControl ();
			x.Text = "FFF";
			c.Controls.Add (x);
			LiteralControl x2 = new LiteralControl ();
			x2.Text = "BBB";
			c.Controls.Add (x2);

			string s = c.InnerText;
		}

		[Test]
		public void NullProperties ()
		{
			TestHtmlContainerControl cc = new TestHtmlContainerControl ();
			cc.InnerHtml = null;
			Assert.AreEqual (String.Empty, cc.InnerHtml, "InnerHtml");
			cc.InnerText = null;
			Assert.AreEqual (String.Empty, cc.InnerText, "InnerText");

			Assert.AreEqual (0, cc.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			TestHtmlContainerControl cc = new TestHtmlContainerControl ();
			cc.InnerHtml = "mono";
			Assert.AreEqual ("mono", cc.InnerHtml, "InnerHtml");
			Assert.AreEqual ("mono", cc.InnerText, "InnerText");
			cc.InnerText = "go mono";
			Assert.AreEqual ("go mono", cc.InnerHtml, "InnerHtml");
			Assert.AreEqual ("go mono", cc.InnerText, "InnerText");

			cc.InnerHtml = null;
			Assert.AreEqual (String.Empty, cc.InnerHtml, "InnerHtml");
			cc.InnerText = null;
			Assert.AreEqual (String.Empty, cc.InnerText, "InnerText");

			Assert.AreEqual (0, cc.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void Render ()
		{
			TestHtmlContainerControl cc = new TestHtmlContainerControl ();
			cc.InnerHtml = "mono";
			Assert.AreEqual ("<span>mono</span>", cc.Render ());
		}
	}
}
