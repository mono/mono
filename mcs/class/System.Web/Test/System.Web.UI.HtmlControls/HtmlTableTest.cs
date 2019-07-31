//
// HtmlTableTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlTable
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

	public class TestHtmlTable : HtmlTable {

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

	public class InheritedHtmlTableRow : HtmlTableRow {
	}

	[TestFixture]
	public class HtmlTableTest {

		[Test]
		public void DefaultProperties ()
		{
			HtmlTable t = new HtmlTable ();
			Assert.AreEqual (0, t.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, t.Align, "Align");
			Assert.AreEqual (String.Empty, t.BgColor, "BgColor");
			Assert.AreEqual (-1, t.Border, "Border");
			Assert.AreEqual (String.Empty, t.BorderColor, "BorderColor");
			Assert.AreEqual (-1, t.CellPadding, "CellPadding");
			Assert.AreEqual (-1, t.CellSpacing, "CellSpacing");
			Assert.AreEqual (String.Empty, t.Height, "Height");
			Assert.AreEqual (0, t.Rows.Count, "Rows");
			Assert.AreEqual (String.Empty, t.Width, "Width");

			Assert.AreEqual ("table", t.TagName, "TagName");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlTable t = new HtmlTable ();
			t.Align = null;
			Assert.AreEqual (String.Empty, t.Align, "Align");
			t.BgColor = null;
			Assert.AreEqual (String.Empty, t.BgColor, "BgColor");
			t.BorderColor = null;
			Assert.AreEqual (String.Empty, t.BorderColor, "BorderColor");
			t.Height = null;
			Assert.AreEqual (String.Empty, t.Height, "Height");
			t.Width = null;
			Assert.AreEqual (String.Empty, t.Width, "Width");

			Assert.AreEqual (0, t.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void EmptyProperties ()
		{
			HtmlTable t = new HtmlTable ();
			t.Border = -1;
			Assert.AreEqual (-1, t.Border, "Border");
			t.CellPadding = -1;
			Assert.AreEqual (-1, t.CellPadding, "CellPadding");
			t.CellSpacing = -1;
			Assert.AreEqual (-1, t.CellSpacing, "CellSpacing");

			Assert.AreEqual (0, t.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlTable t = new HtmlTable ();
			t.Align = "center";
			Assert.AreEqual ("center", t.Align, "Align");
			t.Border = 1;
			Assert.AreEqual (1, t.Border, "Border");
			Assert.AreEqual (2, t.Attributes.Count, "2");

			t.Border = -1;
			Assert.AreEqual (-1, t.Border, "-Border");
			t.Align = null;
			Assert.AreEqual (String.Empty, t.Align, "-Align");
			Assert.AreEqual (0, t.Attributes.Count, "0");
		}

		[Test]
		public void MaxInt32 ()
		{
			HtmlTable t = new HtmlTable ();
			t.Border = Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, t.Border, "Border");
			t.CellPadding = Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, t.CellPadding, "CellPadding");
			t.CellSpacing = Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, t.CellSpacing, "CellSpacing");

			Assert.AreEqual (3, t.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void MinInt32 ()
		{
			HtmlTable t = new HtmlTable ();
			t.Border = Int32.MinValue;
			Assert.AreEqual (Int32.MinValue, t.Border, "Border");
			t.CellPadding = Int32.MinValue;
			Assert.AreEqual (Int32.MinValue, t.CellPadding, "CellPadding");
			t.CellSpacing = Int32.MinValue;
			Assert.AreEqual (Int32.MinValue, t.CellSpacing, "CellSpacing");

			Assert.AreEqual (3, t.Attributes.Count, "Attributes.Count");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void WrongTypeString ()
		{
			HtmlTable t = new HtmlTable ();
			t.Attributes.Add ("Border", "yes");
			Assert.AreEqual (-1, t.Border, "Border");
		}

		[Test]
		public void WrongTypeInt ()
		{
			HtmlTable t = new HtmlTable ();
			t.Border = 42;
			Assert.AreEqual ("42", t.Attributes ["border"], "Border");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerHtml_Get ()
		{
			HtmlTable t = new HtmlTable ();
			Assert.IsNotNull (t.InnerHtml);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerHtml_Set ()
		{
			HtmlTable t = new HtmlTable ();
			t.InnerHtml = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerText_Get ()
		{
			HtmlTable t = new HtmlTable ();
			Assert.IsNotNull (t.InnerText);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerText_Set ()
		{
			HtmlTable t = new HtmlTable ();
			t.InnerText = String.Empty;
		}

		private string RemoveWS (string s)
		{
			s = s.Replace ("\t", "");
			s = s.Replace ("\r", "");
			return s.Replace ("\n", "");
		}

		[Test]
		public void Render_Table_Simple ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			Assert.AreEqual (RemoveWS ("<table>\r\n</table>\r\n"), RemoveWS (t.Render ()));
		}

		[Test]
		public void Render_Table ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			t.Align = "*1*";
			t.BgColor = "*2*";
			t.Border = 3;
			t.BorderColor = "*4*";
			t.CellPadding = 5;
			t.CellSpacing = 6;
			t.Height = "*7*";
			t.Width = "*8*";
			Assert.AreEqual (RemoveWS ("<table align=\"*1*\" bgcolor=\"*2*\" border=\"3\" bordercolor=\"*4*\" cellpadding=\"5\" cellspacing=\"6\" height=\"*7*\" width=\"*8*\">\r\n</table>\r\n"), RemoveWS (t.Render ()));
		}

		[Test]
		public void Render_TableRow_Simple ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			t.Rows.Add (new HtmlTableRow ());
			Assert.AreEqual (RemoveWS ("<table>\r\n\t<tr>\r\n\t</tr>\r\n</table>\r\n"), RemoveWS (t.Render ()));
		}

		[Test]
		public void Render_TableRow ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			t.Border = 0;
			HtmlTableRow r1 = new HtmlTableRow ();
			r1.Align = "right";
			t.Rows.Add (r1);
			HtmlTableRow r2 = new HtmlTableRow ();
			r2.Align = "left";
			t.Rows.Add (r2);
			Assert.AreEqual (RemoveWS ("<table border=\"0\">\r\n\t<tr align=\"right\">\r\n\t</tr>\r\n\t<tr align=\"left\">\r\n\t</tr>\r\n</table>\r\n"), RemoveWS (t.Render ()));
		}

		[Test]
		public void Render_TableRowCell_Simple ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			HtmlTableRow r = new HtmlTableRow ();
			r.Cells.Add (new HtmlTableCell ());
			t.Rows.Add (r);
			Assert.AreEqual (RemoveWS ("<table>\r\n\t<tr>\r\n\t\t<td></td>\r\n\t</tr>\r\n</table>\r\n"),
					RemoveWS (t.Render ()));
		}

		[Test]
		public void Render_TableRowCell ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			t.Align = "center";
			HtmlTableRow r = new HtmlTableRow ();
			r.VAlign = "top";
			t.Rows.Add (r);
			HtmlTableCell c1 = new HtmlTableCell ();
			c1.Align = "right";
			c1.InnerText = "Go";
			r.Cells.Add (c1);
			HtmlTableCell c2 = new HtmlTableCell ();
			c2.Align = "left";
			c2.InnerHtml = "<a href=\"http://www.example.com\">Example</a>";
			r.Cells.Add (c2);
			Assert.AreEqual (RemoveWS ("<table align=\"center\">\r\n\t<tr valign=\"top\">\r\n\t\t<td align=\"right\">Go</td>\r\n\t\t<td align=\"left\"><a href=\"http://www.example.com\">Example</a></td>\r\n\t</tr>\r\n</table>\r\n"), RemoveWS (t.Render ()));
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void HtmlTableRowControlCollectionAdd_Null ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			ControlCollection c = t.GetCollection ();
			c.Add (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HtmlTableRowControlCollectionAdd_WrongType ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			ControlCollection c = t.GetCollection ();
			c.Add (new HtmlTable ());
		}

		[Test]
		public void HtmlTableRowControlCollectionAdd ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			ControlCollection c = t.GetCollection ();
			c.Add (new HtmlTableRow ());
			c.Add (new InheritedHtmlTableRow ());
			Assert.AreEqual (2, c.Count, "Rows");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void HtmlTableRowControlCollectionAddAt_Null ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			ControlCollection c = t.GetCollection ();
			c.AddAt (0, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HtmlTableRowControlCollectionAddAt_WrongType ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			ControlCollection c = t.GetCollection ();
			c.AddAt (0, new HtmlTable ());
		}

		[Test]
		public void HtmlTableRowControlCollectionAddAt ()
		{
			TestHtmlTable t = new TestHtmlTable ();
			ControlCollection c = t.GetCollection ();
			c.AddAt (0, new HtmlTableRow ());
			c.AddAt (0, new InheritedHtmlTableRow ());
			Assert.AreEqual (2, c.Count, "Rows");
		}
	}
}
