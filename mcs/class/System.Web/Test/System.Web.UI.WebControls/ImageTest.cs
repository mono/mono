//
// ImageTest.cs
//	- Unit tests for System.Web.UI.WebControls.Image
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

	public class TestImage : Image {

		public string Tag {
			get { return base.TagName; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
	}

	public class PokerImage : Image
	{
		public PokerImage () {
			TrackViewState ();
		}

		public object SaveState () {
			return SaveViewState ();
		}

		public void LoadState (object state) {
			LoadViewState (state);
		}
	}

	[TestFixture]
	public class ImageTest {

		private const string imageUrl = "http://www.mono-project.com/stylesheets/images.wiki.png";

		[Test]
		public void DefaultProperties ()
		{
			TestImage i = new TestImage ();
			Assert.AreEqual (0, i.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, i.AlternateText, "AlternateText");
			Assert.IsTrue (i.Enabled, "Enabled");
			Assert.IsNotNull (i.Font, "Font");
			Assert.AreEqual (ImageAlign.NotSet, i.ImageAlign, "ImageAlign");
			Assert.AreEqual (String.Empty, i.ImageUrl, "ImageUrl");
			// this was added in Fx 1.1 SP1
			Assert.AreEqual (String.Empty, i.DescriptionUrl, "DescriptionUrl");
#if NET_2_0
			Assert.IsFalse (i.GenerateEmptyAlternateText, "GenerateEmptyAlternateText");
#endif
			Assert.AreEqual ("img", i.Tag, "TagName");
			Assert.AreEqual (0, i.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void ViewStateTest () {

			PokerImage src = new PokerImage ();
			src.Enabled = false;

			PokerImage dest = new PokerImage ();
			dest.LoadState (src.SaveState ());

			Assert.AreEqual (false, dest.Enabled, "Enabled");
		}

		[Test]
		public void NullProperties ()
		{
			TestImage i = new TestImage ();
			i.AlternateText = null;
			Assert.AreEqual (String.Empty, i.AlternateText, "AlternateText");
			i.Enabled = true;
			Assert.IsTrue (i.Enabled, "Enabled");
			i.ImageAlign = ImageAlign.NotSet;
			Assert.AreEqual (ImageAlign.NotSet, i.ImageAlign, "ImageAlign");
			i.ImageUrl = null;
			Assert.AreEqual (String.Empty, i.ImageUrl, "ImageUrl");
			i.DescriptionUrl = null;

			Assert.AreEqual (0, i.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (1, i.StateBag.Count, "ViewState.Count-1");
#if NET_2_0
			i.GenerateEmptyAlternateText = false;
			Assert.AreEqual (2, i.StateBag.Count, "ViewState.Count-2");
#endif
		}

		[Test]
		public void CleanProperties ()
		{
			TestImage i = new TestImage ();
			i.AlternateText = "alt";
			Assert.AreEqual ("alt", i.AlternateText, "AlternateText");
			i.Enabled = false;
			i.ImageAlign = ImageAlign.Top;
			i.ImageUrl = imageUrl;
			i.DescriptionUrl = "http://www.mono-project.com/";
#if NET_2_0
			i.GenerateEmptyAlternateText = true;
			Assert.AreEqual (5, i.StateBag.Count, "ViewState.Count");
#else
			Assert.AreEqual (4, i.StateBag.Count, "ViewState.Count");
#endif
			Assert.AreEqual (0, i.Attributes.Count, "Attributes.Count");

			i.AlternateText = null;
			i.Enabled = true;
			i.ImageAlign = ImageAlign.NotSet;
			i.ImageUrl = null;
			i.DescriptionUrl = null;
#if NET_2_0
			i.GenerateEmptyAlternateText = false;
			// ImageAlign and GenerateEmptyAlternateText can't be removed by returning to default value
			Assert.AreEqual (2, i.StateBag.Count, "ViewState.Count-2");
			Assert.AreEqual (ImageAlign.NotSet, i.StateBag["ImageAlign"], "ImageAlign");
			Assert.IsFalse ((bool)i.StateBag["GenerateEmptyAlternateText"], "GenerateEmptyAlternateText");
#else
			// ImageAlign can't be removed by returning to default value
			Assert.AreEqual (1, i.StateBag.Count, "ViewState.Count-2");
			Assert.AreEqual (ImageAlign.NotSet, i.StateBag["ImageAlign"], "ImageAlign");
#endif
			Assert.AreEqual (0, i.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		// LAMESPEC: 2.0 beta2 documents this as an ArgumentException
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ImageAlign_Invalid ()
		{
			Image i = new Image ();
			i.ImageAlign = (ImageAlign)Int32.MinValue;
		}

		[Test]
		public void RenderEnabled () {
			TestImage img = new TestImage ();
			img.Enabled = false;

			string html = img.Render ();
#if NET_4_0
			Assert.IsTrue (html.IndexOf (" class=\"aspNetDisabled\"") > 0, "#");
#else
			Assert.IsTrue (html.IndexOf (" disabled=\"") > 0, "#");
#endif
		}

		[Test]
		public void Render ()
		{
			TestImage i = new TestImage ();
			// fx 2.0 (beta2) like to add: style="border-width:0px;", 
			// while 1.x adds: border="0". Both aren't coming from Image
			// so we're testing subparts of the string here
			string s = i.Render ();
			Assert.IsTrue (i.Render ().IndexOf (" src=\"\"") > 0, "src");

			i.GenerateEmptyAlternateText = true;
			s = i.Render ();
			Assert.IsTrue (i.Render ().IndexOf (" alt=\"\"") > 0, "alt/GenerateEmptyAlternateText-true");

			i.GenerateEmptyAlternateText = false;
			s = i.Render ();
			Assert.IsTrue (i.Render ().IndexOf (" alt=\"\"") < 0, "alt/GenerateEmptyAlternateText-false");

			i.AlternateText = "alt";
			s = i.Render ();
			Assert.IsTrue (i.Render ().IndexOf (" alt=\"alt\"") > 0, "alt");
			i.AlternateText = String.Empty;
#if NET_4_0
			s = i.Render ();
			Assert.IsTrue (s.IndexOf (" class=\"aspNetDisabled\"") < 0, "enabled");
			i.Enabled = false;
			s = i.Render ();
			Assert.IsTrue (s.IndexOf (" class=\"aspNetDisabled\"") > 0, "disabled");
			i.Enabled = true;
#else
			s = i.Render ();
			Assert.IsTrue (i.Render ().IndexOf (" disabled=\"disabled\"") < 0, "enabled");
			i.Enabled = false;
			s = i.Render ();
			Assert.IsTrue (i.Render ().IndexOf (" disabled=\"disabled\"") > 0, "disabled");
			i.Enabled = true;
#endif

			// note: align is in mixed-case in 1.x so we lower everything to test
			i.ImageAlign = ImageAlign.AbsBottom;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"absbottom\"") > 0, "absbottom");
			i.ImageAlign = ImageAlign.AbsMiddle;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"absmiddle\"") > 0, "absmiddle");
			i.ImageAlign = ImageAlign.Baseline;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"baseline\"") > 0, "baseline");
			i.ImageAlign = ImageAlign.Bottom;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"bottom\"") > 0, "bottom");
			i.ImageAlign = ImageAlign.Left;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"left\"") > 0, "left");
			i.ImageAlign = ImageAlign.Middle;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"middle\"") > 0, "middle");
			i.ImageAlign = ImageAlign.Right;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"right\"") > 0, "right");
			i.ImageAlign = ImageAlign.TextTop;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"texttop\"") > 0, "texttop");
			i.ImageAlign = ImageAlign.Top;
			s = i.Render ();
			Assert.IsTrue (i.Render ().ToLower ().IndexOf (" align=\"top\"") > 0, "top");

			i.ImageAlign = ImageAlign.NotSet;
			s = i.Render ();
			Assert.IsTrue (i.Render ().IndexOf (" align=\"") < 0, "align/none");

			i.ImageUrl = imageUrl;
			s = i.Render ();
			Assert.IsTrue (i.Render ().IndexOf (" src=\"" + imageUrl + "\"") > 0, "ImageUrl");
		}
	}
}
