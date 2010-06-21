//
// HtmlImageTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlImage
//
// Author:
//	Dick Porter  <dick@ximian.com>
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
using System.Web.UI;
using System.Web.UI.HtmlControls;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness; 

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlImage : HtmlImage {

		public HtmlTextWriter GetWriter ()
		{
			StringWriter text = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter (text);
			base.RenderAttributes (writer);
			return writer;
		}
	}

	[TestFixture]
	public class HtmlImageTest {

		[Test]
		public void DefaultProperties ()
		{
			HtmlImage img = new HtmlImage ();
			Assert.AreEqual (0, img.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, img.Align, "Align");
			Assert.AreEqual (String.Empty, img.Alt, "Alt");
			Assert.AreEqual (-1, img.Border, "Border");
			Assert.AreEqual (-1, img.Height, "Height");
			Assert.AreEqual (String.Empty, img.Src, "Src");
			Assert.AreEqual (-1, img.Width, "Width");

			Assert.AreEqual ("img", img.TagName, "TagName");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlImage img = new HtmlImage ();

			img.Align = null;
			Assert.AreEqual (String.Empty, img.Align, "Align");
			img.Alt = null;
			Assert.AreEqual (String.Empty, img.Alt, "Alt");
			img.Border = -1;
			Assert.AreEqual (-1, img.Border, "Border");
			img.Height = -1;
			Assert.AreEqual (-1, img.Height, "Height");
			img.Src = null;
			Assert.AreEqual (String.Empty, img.Src, "Src");
			img.Width = -1;
			Assert.AreEqual (-1, img.Width, "Width");

			Assert.AreEqual (0, img.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void Negative ()
		{
			HtmlImage img = new HtmlImage ();

			img.Border = 10;
			img.Height = 20;
			img.Width = 30;

			Assert.AreEqual (3, img.Attributes.Count, "First Attributes Count");

			img.Border = -10;
			img.Height = -20;
			img.Width = -30;

			Assert.AreEqual (-10, img.Border, "Border");
			Assert.AreEqual (-20, img.Height, "Height");
			Assert.AreEqual (-30, img.Width, "Width");

			Assert.AreEqual (3, img.Attributes.Count, "Second Attributes Count");
		}

		[Test]
		public void EmptySrc ()
		{
			TestHtmlImage img = new TestHtmlImage ();

			img.Src = String.Empty;

			HtmlTextWriter writer = img.GetWriter ();
			Assert.AreEqual (" /", writer.InnerWriter.ToString ());
		}

		[Test]
		[Category ("NunitWeb")]
		public void RenderAttributes ()
		{
			new WebTest (PageInvoker.CreateOnLoad (new PageDelegate (DoRenderAttributes))).Run ();
		}

		public static void DoRenderAttributes (Page p)
		{
			TestHtmlImage img = new TestHtmlImage ();

			img.Align = "*1*";
			img.Alt = "*2*";
			img.Border = 3;
			img.Height = 4;
			img.Src = "*5<&*";
			img.Width = 6;
			
			Assert.AreEqual (6, img.Attributes.Count, "Attributes.Count");

			HtmlTextWriter writer = img.GetWriter ();
			Assert.AreEqual (" src=\"*5<&*\" align=\"*1*\" alt=\"*2*\" border=\"3\" height=\"4\" width=\"6\" /", writer.InnerWriter.ToString ());
		}
	}
}
