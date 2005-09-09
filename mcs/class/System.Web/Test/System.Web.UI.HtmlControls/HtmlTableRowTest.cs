//
// HtmlTableRowTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlTableRow
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

	public class TestHtmlTableRow : HtmlTableRow {

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public ControlCollection GetCollection ()
		{
			return base.CreateControlCollection ();
		}
	}

	public class InheritedHtmlTableCell : HtmlTableCell {
	}

	[TestFixture]
	public class HtmlTableRowTest {

		[Test]
		public void DefaultProperties ()
		{
			HtmlTableRow r = new HtmlTableRow ();
			Assert.AreEqual (0, r.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, r.Align, "Align");
			Assert.AreEqual (String.Empty, r.BgColor, "BgColor");
			Assert.AreEqual (String.Empty, r.BorderColor, "BorderColor");
			Assert.AreEqual (0, r.Cells.Count, "Cells");
			Assert.AreEqual (String.Empty, r.Height, "Height");
			Assert.AreEqual (String.Empty, r.VAlign, "VAlign");

			Assert.AreEqual ("tr", r.TagName, "TagName");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlTableRow r = new HtmlTableRow ();
			r.Align = null;
			Assert.AreEqual (String.Empty, r.Align, "Align");
			r.BgColor = null;
			Assert.AreEqual (String.Empty, r.BgColor, "BgColor");
			r.BorderColor = null;
			Assert.AreEqual (String.Empty, r.BorderColor, "BorderColor");
			r.Height = null;
			Assert.AreEqual (String.Empty, r.Height, "Height");
			r.VAlign = null;
			Assert.AreEqual (String.Empty, r.VAlign, "VAlign");

			Assert.AreEqual (0, r.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlTableRow r = new HtmlTableRow ();
			r.Align = "center";
			Assert.AreEqual ("center", r.Align, "Align");
			Assert.AreEqual (1, r.Attributes.Count, "1");

			r.Align = null;
			Assert.AreEqual (String.Empty, r.Align, "-Align");
			Assert.AreEqual (0, r.Attributes.Count, "0");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerHtml_Get ()
		{
			HtmlTableRow r = new HtmlTableRow ();
			Assert.IsNotNull (r.InnerHtml);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerHtml_Set ()
		{
			HtmlTableRow r = new HtmlTableRow ();
			r.InnerHtml = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerText_Get ()
		{
			HtmlTableRow r = new HtmlTableRow ();
			Assert.IsNotNull (r.InnerText);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerText_Set ()
		{
			HtmlTableRow r = new HtmlTableRow ();
			r.InnerText = String.Empty;
		}

		private string AdjustLineEndings (string s)
		{
			return s.Replace ("\r\n", Environment.NewLine);
		}

		[Test]
		public void Render ()
		{
			TestHtmlTableRow r = new TestHtmlTableRow ();
			r.Align = "*1*";
			r.BgColor = "*2*";
			r.BorderColor = "*3*";
			r.Height = "*4*";
			r.VAlign = "*5*";
			Assert.AreEqual (AdjustLineEndings ("<tr align=\"*1*\" bgcolor=\"*2*\" bordercolor=\"*3*\" height=\"*4*\" valign=\"*5*\">\r\n</tr>\r\n"), r.Render ());
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void HtmlTableCellControlCollectionAdd_Null ()
		{
			TestHtmlTableRow t = new TestHtmlTableRow ();
			ControlCollection c = t.GetCollection ();
			c.Add (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HtmlTableCellControlCollectionAdd_WrongType ()
		{
			TestHtmlTableRow t = new TestHtmlTableRow ();
			ControlCollection c = t.GetCollection ();
			c.Add (new HtmlTable ());
		}

		[Test]
		public void HtmlTableCellControlCollectionAdd ()
		{
			TestHtmlTableRow t = new TestHtmlTableRow ();
			ControlCollection c = t.GetCollection ();
			c.Add (new HtmlTableCell ());
			c.Add (new InheritedHtmlTableCell ());
			Assert.AreEqual (2, c.Count, "Cells");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void HtmlTableCellControlCollectionAddAt_Null ()
		{
			TestHtmlTableRow t = new TestHtmlTableRow ();
			ControlCollection c = t.GetCollection ();
			c.AddAt (0, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HtmlTableCellControlCollectionAddAt_WrongType ()
		{
			TestHtmlTableRow t = new TestHtmlTableRow ();
			ControlCollection c = t.GetCollection ();
			c.AddAt (0, new HtmlTable ());
		}

		[Test]
		public void HtmlTableCellControlCollectionAddAt ()
		{
			TestHtmlTableRow t = new TestHtmlTableRow ();
			ControlCollection c = t.GetCollection ();
			c.AddAt (0, new HtmlTableCell ());
			c.AddAt (0, new InheritedHtmlTableCell ());
			Assert.AreEqual (2, c.Count, "Cells");
		}
	}
}
