//
// HyperLinkTest.cs
//	- Unit tests for (Ben's) System.Web.UI.WebControls.HyperLink
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
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestHyperLink : HyperLink {

		public string Tag {
			get { return base.TagName; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public Style GetStyle ()
		{
			return base.CreateControlStyle ();
		}
	}

	[TestFixture]
	public class HyperLinkTest {

		private const string imageUrl = "http://www.mono-project.com/stylesheets/images.wiki.png";

		[Test]
		public void Empty ()
		{
			TestHyperLink hl = new TestHyperLink ();
			Assert.AreEqual (String.Empty, hl.ImageUrl, "ImageUrl");
			Assert.AreEqual (String.Empty, hl.NavigateUrl, "NavigateUrl");
			Assert.AreEqual (String.Empty, hl.Target, "Target");
			Assert.AreEqual (String.Empty, hl.Text, "Text");
			Assert.AreEqual ("<a></a>", hl.Render (), "Empty");
		}

		[Test]
		public void ImageUrlWithoutText ()
		{
#if NET_4_0
			string origHtml = "<a><img src=\"http://www.mono-project.com/stylesheets/images.wiki.png\" alt=\"\" /></a>";
#else
			string origHtml = "<a><img src=\"http://www.mono-project.com/stylesheets/images.wiki.png\" style=\"border-width:0px;\" /></a>";
#endif
			TestHyperLink hl = new TestHyperLink ();
			hl.ImageUrl = imageUrl;
			Assert.AreEqual (imageUrl, hl.ImageUrl, "ImageUrl");
			Assert.AreEqual (String.Empty, hl.NavigateUrl, "NavigateUrl");
			Assert.AreEqual (String.Empty, hl.Target, "Target");
			Assert.AreEqual (String.Empty, hl.Text, "Text");
			// an empty alt attribute is begin added
			
			string renderedHtml = hl.Render ();
			Assert.AreEqual (origHtml, renderedHtml, "Empty");
		}

		[Test]
		public void ImageUrlWithoutText_ToolTip ()
		{
#if NET_4_0
			string origHtml = "<a title=\"Some message\"><img title=\"Some message\" src=\"http://www.mono-project.com/stylesheets/images.wiki.png\" alt=\"\" /></a>";
#else
			string origHtml = "<a title=\"Some message\"><img title=\"Some message\" src=\"http://www.mono-project.com/stylesheets/images.wiki.png\" style=\"border-width:0px;\" /></a>";
#endif
			TestHyperLink hl = new TestHyperLink ();
			hl.ImageUrl = imageUrl;
			hl.ToolTip = "Some message";

			Assert.AreEqual (imageUrl, hl.ImageUrl, "ImageUrl");
			Assert.AreEqual (String.Empty, hl.NavigateUrl, "NavigateUrl");
			Assert.AreEqual (String.Empty, hl.Target, "Target");
			Assert.AreEqual (String.Empty, hl.Text, "Text");
			// an empty alt attribute is begin added

			string renderedHtml = hl.Render ();
			Assert.AreEqual (origHtml, renderedHtml, "Empty");
		}

		[Test]
		public void NavigateUrl_NO_ResolveUrl ()
		{
			TestHyperLink hl = new TestHyperLink ();
			hl.NavigateUrl = "~/index.html";
			Assert.AreEqual (String.Empty, hl.ImageUrl, "ImageUrl");
			Assert.AreEqual ("~/index.html", hl.NavigateUrl, "NavigateUrl");
			Assert.AreEqual (String.Empty, hl.Target, "Target");
			Assert.AreEqual (String.Empty, hl.Text, "Text");
			// Note: resolve only occurs inside a Page
			Assert.AreEqual ("<a href=\"~/index.html\"></a>", hl.Render (), "Resolve");
		}

		[Test]
		public void ImageUrl_NO_ResolveUrl ()
		{
#if NET_4_0
			string origHtml = "<a><img src=\"~/ben.jpeg\" alt=\"\" /></a>";
#else
			string origHtml = "<a><img src=\"~/ben.jpeg\" style=\"border-width:0px;\" /></a>";
#endif
			TestHyperLink hl = new TestHyperLink ();
			hl.ImageUrl = "~/ben.jpeg";
			Assert.AreEqual ("~/ben.jpeg", hl.ImageUrl, "ImageUrl");
			Assert.AreEqual (String.Empty, hl.NavigateUrl, "NavigateUrl");
			Assert.AreEqual (String.Empty, hl.Target, "Target");
			Assert.AreEqual (String.Empty, hl.Text, "Text");
			// Note: resolve only occurs inside a Page

			string renderedHtml = hl.Render ();
			Assert.AreEqual (origHtml, renderedHtml, "Resolve");
		}
	}
}
