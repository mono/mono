//
// TableCellTest.cs
//	- Unit tests for System.Web.UI.WebControls.TableCell
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

	public class TestTableCell : TableCell {

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

		public Style GetStyle ()
		{
			return base.CreateControlStyle ();
		}

		public void Add (object o)
		{
			base.AddParsedSubObject (o);
		}
	}

	[TestFixture]
	public class TableCellTest {

		[Test]
		public void DefaultProperties ()
		{
			TestTableCell td = new TestTableCell ();
			Assert.AreEqual (0, td.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, td.StateBag.Count, "ViewState.Count");

			Assert.AreEqual (0, td.ColumnSpan, "ColumnSpan");
			Assert.AreEqual (HorizontalAlign.NotSet, td.HorizontalAlign, "HorizontalAlign");
			Assert.AreEqual (0, td.RowSpan, "RowSpan");
			Assert.AreEqual (String.Empty, td.Text, "Text");
			Assert.AreEqual (VerticalAlign.NotSet, td.VerticalAlign, "VerticalAlign");
			Assert.IsTrue (td.Wrap, "Wrap");
#if NET_2_0
			Assert.AreEqual (0, td.AssociatedHeaderCellID.Length, "AssociatedHeaderCellID");
#endif
			Assert.AreEqual ("td", td.Tag, "TagName");
			Assert.AreEqual (0, td.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, td.StateBag.Count, "ViewState.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			TestTableCell td = new TestTableCell ();
			td.ColumnSpan = 0;
			Assert.AreEqual (0, td.ColumnSpan, "ColumnSpan");
			td.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, td.HorizontalAlign, "HorizontalAlign");
			td.RowSpan = 0;
			Assert.AreEqual (0, td.RowSpan, "RowSpan");
			td.Text = null;
			Assert.AreEqual (String.Empty, td.Text, "Text");
			td.VerticalAlign = VerticalAlign.NotSet;
			Assert.AreEqual (VerticalAlign.NotSet, td.VerticalAlign, "VerticalAlign");
			td.Wrap = true;
			Assert.IsTrue (td.Wrap, "Wrap");
#if NET_2_0
			td.AssociatedHeaderCellID = new string[0];
			Assert.AreEqual (0, td.AssociatedHeaderCellID.Length, "AssociatedHeaderCellID");
			Assert.AreEqual (6, td.StateBag.Count, "ViewState.Count-1");
#else
			Assert.AreEqual (5, td.StateBag.Count, "ViewState.Count-1");
#endif
			Assert.AreEqual (0, td.Attributes.Count, "Attributes.Count");
			// note: nothing is removed (no need for CleanProperties test)
		}
#if NET_2_0
		[Test]
		public void AssociatedHeaderCellID ()
		{
			TableCell td = new TableCell ();
			td.AssociatedHeaderCellID = null;
			Assert.AreEqual (0, td.AssociatedHeaderCellID.Length, "0");
			// no NRE

			td.AssociatedHeaderCellID = new string[1] { "mono" };
			Assert.AreEqual (1, td.AssociatedHeaderCellID.Length, "1");

			td.AssociatedHeaderCellID = null;
			Assert.AreEqual (0, td.AssociatedHeaderCellID.Length, "2");
		}
#endif
		[Test]
		// LAMESPEC: undocumented exception but similar to integer properties of Table
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ColumnSpan_Negative ()
		{
			TableCell td = new TableCell ();
			td.ColumnSpan = -1;
		}

		[Test]
		// LAMESPEC: undocumented exception but similar to integer properties of Table
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RowSpan_Negative ()
		{
			TableCell td = new TableCell ();
			td.RowSpan = -1;
		}

		[Test]
		public void Render ()
		{
			TestTableCell td = new TestTableCell ();
			string s = td.Render ();
			Assert.AreEqual ("<td></td>", s, "empty/default");

			// case varies with fx versions
			td.HorizontalAlign = HorizontalAlign.Left;
			s = td.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" align=\"left\"") > 0), "HorizontalAlign.Left");
			td.HorizontalAlign = HorizontalAlign.Center;
			s = td.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" align=\"center\"") > 0), "HorizontalAlign.Center");
			td.HorizontalAlign = HorizontalAlign.Right;
			s = td.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" align=\"right\"") > 0), "HorizontalAlign.Justify");
			td.HorizontalAlign = HorizontalAlign.Justify;
			s = td.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" align=\"justify\"") > 0), "HorizontalAlign.Justify");
			td.HorizontalAlign = HorizontalAlign.NotSet;

			td.VerticalAlign = VerticalAlign.Top;
			s = td.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" valign=\"top\"") > 0), "VerticalAlign.Top");
			td.VerticalAlign = VerticalAlign.Middle;
			s = td.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" valign=\"middle\"") > 0), "VerticalAlign.Middle");
			td.VerticalAlign = VerticalAlign.Bottom;
			s = td.Render ();
			Assert.IsTrue ((s.ToLower ().IndexOf (" valign=\"bottom\"") > 0), "VerticalAlign.Bottom");
			td.VerticalAlign = VerticalAlign.NotSet;

			td.ColumnSpan = 1;
			s = td.Render ();
			Assert.AreEqual ("<td colspan=\"1\"></td>", s, "ColumnSpan");
			td.ColumnSpan = 0;

			td.RowSpan = 1;
			s = td.Render ();
			Assert.AreEqual ("<td rowspan=\"1\"></td>", s, "RowSpan");
			td.RowSpan = 0;

			td.Text = "text";
			s = td.Render ();
			Assert.AreEqual ("<td>text</td>", s, "Text");
			td.Text = null;

			td.Wrap = false;
			s = td.Render ();
#if NET_2_0
			Assert.AreEqual ("<td style=\"white-space:nowrap;\"></td>", s, "Wrap");

			// it seems that rendering with AssociatedHeaderCellID property set
			// isn't (at least easyly) possible even if we build a whole table
			// with a page... it keeps throwing NullReferenceException. Even in a
			// web page using that property makes it easy to throw exceptions :(
#else
			Assert.AreEqual ("<td nowrap=\"nowrap\"></td>", s, "Wrap");
#endif
			td.Wrap = true;
		}

		[Test]
		public void CreateControlStyle ()
		{
			TestTableCell td = new TestTableCell ();
			td.HorizontalAlign = HorizontalAlign.Left;
			td.VerticalAlign = VerticalAlign.Bottom;
			td.Wrap = false;

			TableItemStyle tis = (TableItemStyle)td.GetStyle ();
			// is it live ?
			tis.HorizontalAlign = HorizontalAlign.Right;
			Assert.AreEqual (HorizontalAlign.Right, td.HorizontalAlign, "HorizontalAlign-2");
			tis.VerticalAlign = VerticalAlign.Top;
			Assert.AreEqual (VerticalAlign.Top, td.VerticalAlign, "VerticalAlign-2");
			tis.Wrap = false;
			Assert.IsFalse (tis.Wrap, "Wrap-2");
		}

		[Test]
        [Category ("NotWorking")]
		public void Add_LiteralControl_NoText ()
		{
			TestTableCell td = new TestTableCell ();
			// this is moved into the (empty) Text property
			td.Add (new LiteralControl ("Mono"));
			Assert.IsFalse (td.HasControls (), "!HasControls");
			Assert.AreEqual ("Mono", td.Text, "Text");
			// this replace the current Text property
			td.Add (new LiteralControl ("Go Mono"));
			Assert.IsFalse (td.HasControls (), "!HasControls-2");
#if NET_2_0
			Assert.AreEqual ("MonoGo Mono", td.Text, "Text-2");
#else
			Assert.AreEqual ("Go Mono", td.Text, "Text-2");
#endif
		}

		[Test]
		public void Text_Add_LiteralControl ()
		{
			TestTableCell td = new TestTableCell ();
			td.Text = "Mono";
			Assert.AreEqual ("Mono", td.Text, "Text-1");
			Assert.IsFalse (td.HasControls (), "!HasControls");
			// this replace the current Text property
			td.Add (new LiteralControl ("Go Mono"));
			Assert.IsFalse (td.HasControls (), "!HasControls-2");
			Assert.AreEqual ("Go Mono", td.Text, "Text-2");
		}

		[Test]
		public void Add_LiteralControl_Text ()
		{
			TestTableCell td = new TestTableCell ();
			// this is moved into the (empty) Text property
			td.Add (new LiteralControl ("Mono"));
			Assert.IsFalse (td.HasControls (), "!HasControls");
			Assert.AreEqual ("Mono", td.Text, "Text");
			// this replace the current Text property
			td.Text = "Go Mono";
			Assert.IsFalse (td.HasControls (), "!HasControls-2");
			Assert.AreEqual ("Go Mono", td.Text, "Text-2");
		}

		[Test]
        [Category ("NotWorking")]
		public void Add_LiteralControl_Literal_And_Literal ()
		{
			TestTableCell td = new TestTableCell ();
			// this is moved into the (empty) Text property
			td.Add (new LiteralControl ("Mono"));
			Assert.IsFalse (td.HasControls (), "!HasControls");
			Assert.AreEqual ("Mono", td.Text, "Text");
			td.Add (new LiteralControl ("Mono2"));
			Assert.IsFalse (td.HasControls (), "HasControls-2");
#if NET_2_0
			Assert.AreEqual ("MonoMono2", td.Text, "Text");
#else
			Assert.AreEqual ("Mono2", td.Text, "Text");
#endif
			Assert.AreEqual (0, td.Controls.Count, "NControls");
		}

		[Test]
		public void Add_LiteralControl_Control_And_Literal ()
		{
			TestTableCell td = new TestTableCell ();
			// this is moved into the (empty) Text property
			td.Add (new TableCell ());
			Assert.IsTrue (td.HasControls (), "HasControls");
			td.Add (new LiteralControl ("Mono2"));
			Assert.AreEqual (2, td.Controls.Count, "NControls");
			Assert.AreEqual (typeof (TableCell), td.Controls [0].GetType (), "type 1");
			Assert.AreEqual (typeof (LiteralControl), td.Controls [1].GetType (), "type 2");
		}

		[Test]
		public void Add_LiteralControl_Literal_And_Control ()
		{
			TestTableCell td = new TestTableCell ();
			// this is moved into the (empty) Text property
			td.Add (new LiteralControl ("Mono2"));
			Assert.IsFalse (td.HasControls (), "HasControls");
			td.Add (new TableCell ());
			Assert.AreEqual (2, td.Controls.Count, "NControls");
			Assert.AreEqual (typeof (LiteralControl), td.Controls [0].GetType (), "type 1");
			Assert.AreEqual (typeof (TableCell), td.Controls [1].GetType (), "type 2");
		}

		[Test]
		public void HasControls_Text ()
		{
			TestTableCell td = new TestTableCell ();
			for (int i = 0; i < 10; i++)
				td.Add (new Table ());
			Assert.AreEqual (10, td.Controls.Count, "10");
			// this removes all existing controls and set the Text property
			td.Text = "Mono";
			Assert.AreEqual ("Mono", td.Text, "Text");
			Assert.AreEqual (0, td.Controls.Count, "0");
		}

		[Test]
		public void Text_Add_Controls ()
		{
			TestTableCell td = new TestTableCell ();
			td.Text = "Mono";
			Assert.AreEqual ("Mono", td.Text, "Text");
			Assert.IsFalse (td.HasControls (), "!HasControls");
			// then add 10 more controls
			for (int i = 0; i < 10; i++)
				td.Add (new Table ());
			Assert.AreEqual (11, td.Controls.Count, "11");
			// Text was moved into a LiteralControl
			Assert.IsTrue ((td.Controls[0] is LiteralControl), "LiteralControl");
			// and removed from property
			Assert.AreEqual (String.Empty, td.Text, "Test-2");
		}

		[Test]
		public void NoDefaultID ()
		{
			Page page = new Page ();
			TableCell tc = new TableCell ();
			Assert.AreEqual (null, tc.ID, "#01");
			page.Controls.Add (tc);
			Assert.AreEqual (null, tc.ID, "#02");
			Assert.IsNotNull (tc.UniqueID, "#03");
			Assert.IsNull (tc.ID, "#04");
		}

		[Test]
		public void PropertyOrControls ()
		{
			TestTableCell tc = new TestTableCell ();
			tc.Controls.Add (new LiteralControl ("hola"));
			tc.StateBag ["Text"] = "adios";
			string str = tc.Render ();
			Assert.AreEqual (1, tc.Controls.Count, "#01");
			Assert.IsTrue (-1 != str.IndexOf ("hola"), "#02");
			Assert.IsTrue (-1 == str.IndexOf ("adios"), "#03");

			tc = new TestTableCell ();
			tc.StateBag ["Text"] = "adios";
			str = tc.Render ();
			Assert.AreEqual (0, tc.Controls.Count, "#04");
			Assert.IsTrue (-1 == str.IndexOf ("hola"), "#05");
			Assert.IsTrue (-1 != str.IndexOf ("adios"), "#06");
		}
	}
}

