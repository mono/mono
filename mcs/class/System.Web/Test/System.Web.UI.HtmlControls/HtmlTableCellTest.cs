//
// HtmlTableCellTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlTableCell
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
using System.Web.UI;
using System.Web.UI.HtmlControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlTableCell : HtmlTableCell {

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
	}

	[TestFixture]
	public class HtmlTableCellTest {

		[Test]
		public void DefaultProperties ()
		{
			HtmlTableCell c = new HtmlTableCell ();
			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, c.Align, "Align");
			Assert.AreEqual (String.Empty, c.BgColor, "BgColor");
			Assert.AreEqual (String.Empty, c.BorderColor, "BorderColor");
			Assert.AreEqual (-1, c.ColSpan, "ColSpan");
			Assert.AreEqual (String.Empty, c.Height, "Height");
			Assert.IsFalse (c.NoWrap, "NoWrap");
			Assert.AreEqual (-1, c.RowSpan, "RowSpan");
			Assert.AreEqual (String.Empty, c.VAlign, "VAlign");
			Assert.AreEqual (String.Empty, c.Width, "Width");

			Assert.AreEqual ("td", c.TagName, "TagName");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlTableCell c = new HtmlTableCell ();
			c.Align = null;
			Assert.AreEqual (String.Empty, c.Align, "Align");
			c.BgColor = null;
			Assert.AreEqual (String.Empty, c.BgColor, "BgColor");
			c.BorderColor = null;
			Assert.AreEqual (String.Empty, c.BorderColor, "BorderColor");
			c.Height = null;
			Assert.AreEqual (String.Empty, c.Height, "Height");
			c.VAlign = null;
			Assert.AreEqual (String.Empty, c.VAlign, "VAlign");
			c.Width = null;
			Assert.AreEqual (String.Empty, c.Width, "Width");

			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void EmptyProperties ()
		{
			HtmlTableCell c = new HtmlTableCell ();
			c.ColSpan = -1;
			Assert.AreEqual (-1, c.ColSpan, "ColSpan");
			c.RowSpan = -1;
			Assert.AreEqual (-1, c.RowSpan, "RowSpan");
			c.NoWrap = false;
			Assert.IsFalse (c.NoWrap, "NoWrap");

			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlTableCell c = new HtmlTableCell ();
			c.Align = "center";
			Assert.AreEqual ("center", c.Align, "Align");
			c.ColSpan = 1;
			Assert.AreEqual (1, c.ColSpan, "Align");
			c.NoWrap = true;
			Assert.IsTrue (c.NoWrap, "NoWrap");
			Assert.AreEqual (3, c.Attributes.Count, "3");

			c.Align = null;
			Assert.AreEqual (String.Empty, c.Align, "-Align");
			c.ColSpan = -1;
			Assert.AreEqual (-1, c.ColSpan, "-ColSpan");
			c.NoWrap = false;
			Assert.IsFalse (c.NoWrap, "-NoWrap");
			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count");
		}

		private string AdjustLineEndings (string s)
		{
			return s.Replace ("\r\n", Environment.NewLine);
		}

		[Test]
		public void Render ()
		{
			TestHtmlTableCell c = new TestHtmlTableCell ();
			c.Align = "*1*";
			c.BgColor = "*2*";
			c.BorderColor = "*3*";
			c.ColSpan = 4;
			c.Width = "*5*";

			Assert.AreEqual (AdjustLineEndings ("<td align=\"*1*\" bgcolor=\"*2*\" bordercolor=\"*3*\" colspan=\"4\" width=\"*5*\"></td>\r\n"), c.Render ());
		}
	}
}
