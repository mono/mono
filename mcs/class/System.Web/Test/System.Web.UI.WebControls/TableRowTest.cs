//
// TableRowTest.cs
//	- Unit tests for System.Web.UI.WebControls.TableRow
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
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

#if TARGET_JVM
    [vmw.common.ChangeInterfaceMethodNames]
#endif
	public interface ITableRowTest {

		// testing
		string Tag { get; }
		StateBag StateBag { get; }
		string Render ();
		Style GetStyle ();

		// TableRow
		AttributeCollection Attributes { get; }
		Color BackColor { get; set; }
		ControlCollection Controls { get; }
		TableCellCollection Cells { get; }
		Style ControlStyle { get; }
		HorizontalAlign HorizontalAlign { get; set; }
		VerticalAlign VerticalAlign { get; set; }
#if NET_2_0
		TableRowSection TableSection { get; set; }
#endif
	}

	public class TestTableRow : TableRow, ITableRowTest {

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
	public class TableRowTest {

		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		public virtual TableRow GetNewTableRow ()
		{
			return new TableRow ();
		}

		public virtual ITableRowTest GetNewTestTableRow ()
		{
			return new TestTableRow ();
		}

		[Test]
		public void DefaultProperties ()
		{
			ITableRowTest tr = GetNewTestTableRow ();
			Assert.AreEqual (0, tr.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, tr.StateBag.Count, "ViewState.Count");

			Assert.AreEqual (0, tr.Cells.Count, "Cells.Count");
			Assert.AreEqual (HorizontalAlign.NotSet, tr.HorizontalAlign, "HorizontalAlign");
			Assert.AreEqual (VerticalAlign.NotSet, tr.VerticalAlign, "VerticalAlign");
#if NET_2_0
			Assert.AreEqual (TableRowSection.TableBody, tr.TableSection, "TableSection");
#endif
			Assert.AreEqual ("tr", tr.Tag, "TagName");
			Assert.AreEqual (0, tr.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, tr.StateBag.Count, "ViewState.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			ITableRowTest tr = GetNewTestTableRow ();
			tr.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, tr.HorizontalAlign, "HorizontalAlign");
			tr.VerticalAlign = VerticalAlign.NotSet;
			Assert.AreEqual (VerticalAlign.NotSet, tr.VerticalAlign, "VerticalAlign");
#if NET_2_0
			tr.TableSection = TableRowSection.TableBody;
			Assert.AreEqual (TableRowSection.TableBody, tr.TableSection, "TableSection");
			Assert.AreEqual (3, tr.StateBag.Count, "ViewState.Count-1");
#else
			Assert.AreEqual (2, tr.StateBag.Count, "ViewState.Count-1");
#endif
			Assert.AreEqual (0, tr.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			ITableRowTest tr = GetNewTestTableRow ();
			tr.HorizontalAlign = HorizontalAlign.Justify;
			Assert.AreEqual (HorizontalAlign.Justify, tr.HorizontalAlign, "HorizontalAlign");
			tr.VerticalAlign = VerticalAlign.Bottom;
			Assert.AreEqual (VerticalAlign.Bottom, tr.VerticalAlign, "VerticalAlign");
			Assert.AreEqual (0, tr.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (2, tr.StateBag.Count, "ViewState.Count");

			tr.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, tr.HorizontalAlign, "HorizontalAlign");
			tr.VerticalAlign = VerticalAlign.NotSet;
			Assert.AreEqual (VerticalAlign.NotSet, tr.VerticalAlign, "VerticalAlign");
#if NET_2_0
			tr.TableSection = TableRowSection.TableFooter;
			Assert.AreEqual (TableRowSection.TableFooter, tr.TableSection, "TableFooter");
			tr.TableSection = TableRowSection.TableHeader;
			Assert.AreEqual (TableRowSection.TableHeader, tr.TableSection, "TableHeader");
			tr.TableSection = TableRowSection.TableBody;
			Assert.AreEqual (TableRowSection.TableBody, tr.TableSection, "TableBody");
			Assert.AreEqual (3, tr.StateBag.Count, "ViewState.Count-1");
#else
			Assert.AreEqual (2, tr.StateBag.Count, "ViewState.Count-1");
#endif
			Assert.AreEqual (0, tr.Attributes.Count, "Attributes.Count");
		}

		[Test]
		// LAMESPEC: undocumented exception but similar to Image
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void HorizontalAlign_Invalid ()
		{
			TableRow tr = GetNewTableRow ();
			tr.HorizontalAlign = (HorizontalAlign)Int32.MinValue;
		}

		[Test]
		// LAMESPEC: undocumented exception but similar to Image
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void VerticalAlign_Invalid ()
		{
			TableRow tr = GetNewTableRow ();
			tr.VerticalAlign = (VerticalAlign)Int32.MinValue;
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TableSection_Invalid ()
		{
			TableRow tr = GetNewTableRow ();
			tr.TableSection = (TableRowSection)Int32.MinValue;
		}
#endif
		[Test]
		public void Render ()
		{
			ITableRowTest tr = GetNewTestTableRow ();
			string s = tr.Render ();
			Assert.AreEqual ("<tr>\n\n</tr>", s, "empty/default");

			// case varies with fx versions
			tr.HorizontalAlign = HorizontalAlign.Left;
			s = tr.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" align=\"left\"") > 0), "HorizontalAlign.Left");
			tr.HorizontalAlign = HorizontalAlign.Center;
			s = tr.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" align=\"center\"") > 0), "HorizontalAlign.Center");
			tr.HorizontalAlign = HorizontalAlign.Right;
			s = tr.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" align=\"right\"") > 0), "HorizontalAlign.Justify");
			tr.HorizontalAlign = HorizontalAlign.Justify;
			s = tr.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" align=\"justify\"") > 0), "HorizontalAlign.Justify");
			tr.HorizontalAlign = HorizontalAlign.NotSet;

			tr.VerticalAlign = VerticalAlign.Top;
			s = tr.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" valign=\"top\"") > 0), "VerticalAlign.Top");
			tr.VerticalAlign = VerticalAlign.Middle;
			s = tr.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" valign=\"middle\"") > 0), "VerticalAlign.Middle");
			tr.VerticalAlign = VerticalAlign.Bottom;
			s = tr.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" valign=\"bottom\"") > 0), "VerticalAlign.Bottom");
			tr.VerticalAlign = VerticalAlign.NotSet;
#if NET_2_0
			// TableSection has no influence over the "row" rendering
			tr.TableSection = TableRowSection.TableFooter;
			s = tr.Render ();
			Assert.AreEqual ("<tr>\n\n</tr>", s, "TableRowSection.TableFooter");
			tr.TableSection = TableRowSection.TableHeader;
			s = tr.Render ();
			Assert.AreEqual ("<tr>\n\n</tr>", s, "TableRowSection.TableHeader");
			tr.TableSection = TableRowSection.TableBody;
			s = tr.Render ();
			Assert.AreEqual ("<tr>\n\n</tr>", s, "TableRowSection.TableBody");
#endif
		}

		[Test]
		public void Render_Style ()
		{
			ITableRowTest tr = GetNewTestTableRow ();
			tr.BackColor = Color.Aqua;
			string s = tr.Render ();
			Assert.AreEqual ("<tr style=\"background-color:Aqua;\">\n\n</tr>", s, "direct");

			TableItemStyle tis = new TableItemStyle ();
			tis.BackColor = Color.Red;
			tr.ControlStyle.CopyFrom (tis);
			s = tr.Render ();
			Assert.AreEqual ("<tr style=\"background-color:Red;\">\n\n</tr>", s, "CopyFrom");
		}

		[Test]
		public void CreateControlStyle ()
		{
			ITableRowTest tr = GetNewTestTableRow ();
			tr.HorizontalAlign = HorizontalAlign.Left;
			tr.VerticalAlign = VerticalAlign.Bottom;

			TableItemStyle tis = (TableItemStyle)tr.GetStyle ();
			// is it live ?
			tis.HorizontalAlign = HorizontalAlign.Right;
			Assert.AreEqual (HorizontalAlign.Right, tr.HorizontalAlign, "HorizontalAlign-2");
			tis.VerticalAlign = VerticalAlign.Top;
			Assert.AreEqual (VerticalAlign.Top, tr.VerticalAlign, "VerticalAlign-2");
		}

		private string Adjust (string s)
		{
			// right now Mono doesn't generate the exact same indentation/lines as MS implementation
			// and different fx versions have different casing for enums
			return s.Replace ("\n", "").Replace ("\t", "").ToLower ();
		}

		[Test]
		public void Cells ()
		{
			ITableRowTest tr = GetNewTestTableRow ();
			Assert.AreEqual (0, tr.Cells.Count, "0");
			TableCell td = new TableCell ();

			tr.Cells.Add (td);
			Assert.AreEqual (1, tr.Cells.Count, "c1");
			Assert.AreEqual (1, tr.Controls.Count, "k1");
			string s = tr.Render ();
			Assert.AreEqual (Adjust ("<tr>\n\t<td></td>\n</tr>"), Adjust (s), "td-1");

			// change instance properties
			td.RowSpan = 1;
			s = tr.Render ();
			Assert.AreEqual (Adjust ("<tr>\n\t<td rowspan=\"1\"></td>\n</tr>"), Adjust (s), "td-1r");

			// add it again (same instance)
			tr.Cells.Add (td);
			Assert.AreEqual (1, tr.Cells.Count, "c1bis");
			Assert.AreEqual (1, tr.Controls.Count, "k1bis");
			s = tr.Render ();
			Assert.AreEqual (Adjust ("<tr>\n\t<td rowspan=\"1\"></td>\n</tr>"), Adjust (s), "tr-1bis");

			td = new TableCell ();
			td.VerticalAlign = VerticalAlign.Top;
			tr.Cells.Add (td);
			Assert.AreEqual (2, tr.Cells.Count, "c2");
			Assert.AreEqual (2, tr.Controls.Count, "k2");
			s = tr.Render ();
			Assert.AreEqual (Adjust ("<tr>\n\t<td rowspan=\"1\"></td><td valign=\"top\"></td>\n</tr>"), Adjust (s), "tr-2");

			td = new TableCell ();
			td.HorizontalAlign = HorizontalAlign.Center;
			tr.Cells.Add (td);
			Assert.AreEqual (3, tr.Cells.Count, "c3");
			Assert.AreEqual (3, tr.Controls.Count, "k3");
			s = tr.Render ();
			Assert.AreEqual (Adjust ("<tr>\n\t<td rowspan=\"1\"></td><td valign=\"top\"></td><td align=\"center\"></td>\n</tr>"), Adjust (s), "tr-3");

			tr.HorizontalAlign = HorizontalAlign.Right;
			s = tr.Render ();
			Assert.AreEqual (Adjust ("<tr align=\"right\">\n\t<td rowspan=\"1\"></td><td valign=\"top\"></td><td align=\"center\"></td>\n</tr>"), Adjust (s), "tr-3a");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ControlsAdd_Null ()
		{
			GetNewTableRow ().Controls.Add (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ControlsAdd_LiteralControl ()
		{
			GetNewTableRow ().Controls.Add (new LiteralControl ("mono"));
		}

		[Test]
		public void ControlsAdd_TableCell ()
		{
			TableRow tr = GetNewTableRow ();
			tr.Controls.Add (new TableCell ());
			Assert.AreEqual (1, tr.Controls.Count, "Controls");
			Assert.AreEqual (1, tr.Cells.Count, "Cells");
		}

		[Test]
		public void ControlsAdd_TestTableRow ()
		{
			TableRow tr = GetNewTableRow ();
			tr.Controls.Add (new TestTableCell ());
			Assert.AreEqual (1, tr.Controls.Count, "Controls");
			Assert.AreEqual (1, tr.Cells.Count, "Cells");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ControlsAddAt_Null ()
		{
			GetNewTableRow ().Controls.AddAt (0, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		// note: for Table it's ArgumentOutOfRangeException
		public void ControlsAddAt_Negative ()
		{
			GetNewTableRow ().Controls.AddAt (Int32.MinValue, new TableRow ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ControlsAddAt_LiteralControl ()
		{
			GetNewTableRow ().Controls.AddAt (0, new LiteralControl ("mono"));
		}

		[Test]
		public void ControlsAddAt_TableRow ()
		{
			TableRow tr = GetNewTableRow ();
			tr.Controls.AddAt (0, new TableCell ());
			Assert.AreEqual (1, tr.Controls.Count, "Controls");
			Assert.AreEqual (1, tr.Cells.Count, "Cells");
		}

		[Test]
		public void ControlsAddAt_TestTableRow ()
		{
			TableRow tr = GetNewTableRow ();
			tr.Controls.AddAt (0, new TestTableCell ());
			Assert.AreEqual (1, tr.Controls.Count, "Controls");
			Assert.AreEqual (1, tr.Cells.Count, "Cells");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void RenderBeginTag_Null ()
		{
			TableRow tr = GetNewTableRow ();
			tr.RenderBeginTag (null);
		}

		[Test]
		public void RenderBeginTag_Empty ()
		{
			HtmlTextWriter writer = GetWriter ();
			TableRow tr = GetNewTableRow ();
			tr.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<tr>\n", s, "empty");
		}

		[Test]
		public void RenderBeginTag_HorizontalAlign ()
		{
			HtmlTextWriter writer = GetWriter ();
			TableRow tr = GetNewTableRow ();
			tr.HorizontalAlign = HorizontalAlign.Center;
			tr.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.IsTrue (s.ToLower ().StartsWith ("<tr align=\"center\">"), "HorizontalAlign");
		}

		[Test]
		public void RenderBeginTag_Cells ()
		{
			HtmlTextWriter writer = GetWriter ();
			TableRow tr = GetNewTableRow ();
			tr.Cells.Add (new TableCell ());
			tr.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<tr>\n", s, "td");
		}
	}
}
