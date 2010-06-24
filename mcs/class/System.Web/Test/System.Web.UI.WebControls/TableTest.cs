//
// TableTest.cs
//	- Unit tests for System.Web.UI.WebControls.Table
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
using MonoTests.SystemWeb.Framework;

using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestTable : Table {

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
#if NET_2_0
		protected override void RaisePostBackEvent (string argument)
		{
			WebTest.CurrentTest.UserData = "RaisePostBackEvent";
			base.RaisePostBackEvent (argument);
		}
#endif

	}

	[TestFixture]
	public class TableTest {

		private const string imageUrl = "http://www.mono-project.com/stylesheets/images.wiki.png";
		private const string localImageUrl = "foo.jpg";

		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		[Test]
		public void DefaultProperties ()
		{
			TestTable t = new TestTable ();
			Assert.AreEqual (0, t.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, t.StateBag.Count, "ViewState.Count");

			Assert.AreEqual (String.Empty, t.BackImageUrl, "BackImageUrl");
			Assert.AreEqual (String.Empty, t.Caption, "Caption");
			Assert.AreEqual (TableCaptionAlign.NotSet, t.CaptionAlign, "CaptionAlign");
			Assert.AreEqual (-1, t.CellPadding, "CellPadding");
			Assert.AreEqual (-1, t.CellSpacing, "CellSpacing");
			Assert.AreEqual (GridLines.None, t.GridLines, "GridLines");
			Assert.AreEqual (HorizontalAlign.NotSet, t.HorizontalAlign, "HorizontalAlign");
			Assert.AreEqual (0, t.Rows.Count, "Rows.Count");

			Assert.AreEqual ("table", t.Tag, "TagName");
			Assert.AreEqual (0, t.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, t.StateBag.Count, "ViewState.Count-2");
#if NET_2_0
			Assert.AreEqual (String.Empty, t.Caption, "Caption");
			Assert.AreEqual (TableCaptionAlign.NotSet, t.CaptionAlign, "CaptionAlign");
#endif

		}
#if NET_2_0
		[Test]
		public void Caption ()
		{
			TestTable t = new TestTable ();
			t.Caption = "CaptionText";
			string html = t.Render ();
			string orig = "<table border=\"0\">\n\t<caption>\n\t\tCaptionText\n\t</caption>\n</table>";
			HtmlDiff.AssertAreEqual (orig, html, "Caption");
		}

		[Test]
		public void CaptionAlign ()
		{
			TestTable t = new TestTable ();
			t.Caption = "CaptionText";
			t.CaptionAlign = TableCaptionAlign.Left; 
			string html = t.Render ();
			string orig = "<table border=\"0\">\n\t<caption align=\"Left\">\n\t\tCaptionText\n\t</caption>\n</table>";
			HtmlDiff.AssertAreEqual (orig, html, "CaptionAlign");
		}
#endif
		[Test]
		public void NullProperties ()
		{
			TestTable t = new TestTable ();
			t.BackImageUrl = String.Empty; // doesn't accept null, see specific test
			Assert.AreEqual (String.Empty, t.BackImageUrl, "BackImageUrl");
			t.Caption = null; // doesn't get added to ViewState
			Assert.AreEqual (String.Empty, t.Caption, "Caption");
			t.CaptionAlign = TableCaptionAlign.NotSet;
			Assert.AreEqual (TableCaptionAlign.NotSet, t.CaptionAlign, "CaptionAlign");
			t.CellPadding = -1;
			Assert.AreEqual (-1, t.CellPadding, "CellPadding");
			t.CellSpacing = -1;
			Assert.AreEqual (-1, t.CellSpacing, "CellSpacing");
			t.GridLines = GridLines.None;
			Assert.AreEqual (GridLines.None, t.GridLines, "GridLines");
			t.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, t.HorizontalAlign, "HorizontalAlign");

			Assert.AreEqual (0, t.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (6, t.StateBag.Count, "ViewState.Count-1");
		}

		[Test]
		public void CleanProperties ()
		{
			TestTable t = new TestTable ();
			t.BackImageUrl = imageUrl;
			Assert.AreEqual (imageUrl, t.BackImageUrl, "BackImageUrl");
			t.Caption = "Mono";
			Assert.AreEqual ("Mono", t.Caption, "Caption");
			t.CaptionAlign = TableCaptionAlign.Top;
			Assert.AreEqual (TableCaptionAlign.Top, t.CaptionAlign, "CaptionAlign");
			t.CellPadding = 1;
			Assert.AreEqual (1, t.CellPadding, "CellPadding");
			t.CellSpacing = 2;
			Assert.AreEqual (2, t.CellSpacing, "CellSpacing");
			t.GridLines = GridLines.Both;
			Assert.AreEqual (GridLines.Both, t.GridLines, "GridLines");
			t.HorizontalAlign = HorizontalAlign.Justify;
			Assert.AreEqual (HorizontalAlign.Justify, t.HorizontalAlign, "HorizontalAlign");
			Assert.AreEqual (0, t.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (7, t.StateBag.Count, "ViewState.Count");

			t.BackImageUrl = String.Empty; // doesn't accept null, see specific test
			Assert.AreEqual (String.Empty, t.BackImageUrl, "-BackImageUrl");
			t.Caption = null; // removed
			Assert.AreEqual (String.Empty, t.Caption, "-Caption");
			t.CaptionAlign = TableCaptionAlign.NotSet;
			Assert.AreEqual (TableCaptionAlign.NotSet, t.CaptionAlign, "-CaptionAlign");
			t.CellPadding = -1;
			Assert.AreEqual (-1, t.CellPadding, "-CellPadding");
			t.CellSpacing = -1;
			Assert.AreEqual (-1, t.CellSpacing, "-CellSpacing");
			t.GridLines = GridLines.None;
			Assert.AreEqual (GridLines.None, t.GridLines, "-GridLines");
			t.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, t.HorizontalAlign, "-HorizontalAlign");

			Assert.AreEqual (0, t.Attributes.Count, "Attributes.Count-1");
			Assert.AreEqual (6, t.StateBag.Count, "ViewState.Count-1");
		}

		[Test]
		// LAMESPEC: undocumented (all others property I've seen takes null as the default value)
		[ExpectedException (typeof (ArgumentNullException))]
		public void BackImageUrl_Null ()
		{
			Table t = new Table ();
			t.BackImageUrl = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CaptionAlign_Invalid ()
		{
			Table t = new Table ();
			t.CaptionAlign = (TableCaptionAlign)Int32.MinValue;
		}

		[Test]
		// LAMESPEC: undocumented exception but similar to Image
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GridLines_Invalid ()
		{
			Table t = new Table ();
			t.GridLines = (GridLines)Int32.MinValue;
		}

		[Test]
		// LAMESPEC: undocumented exception but similar to Image
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void HorizontalAlign_Invalid ()
		{
			Table t = new Table ();
			t.HorizontalAlign = (HorizontalAlign)Int32.MinValue;
		}

		[Test]
		public void BorderWidth ()
		{
			Table t = new Table ();
			Assert.AreEqual (0, t.BorderWidth.Value, "GridLines.None");
			t.GridLines = GridLines.Horizontal;
			Assert.AreEqual (0, t.BorderWidth.Value, "GridLines.Horizontal");
			t.GridLines = GridLines.Vertical;
			Assert.AreEqual (0, t.BorderWidth.Value, "GridLines.Vertical");
			t.GridLines = GridLines.Both;
			Assert.AreEqual (0, t.BorderWidth.Value, "GridLines.Both");
			// note: but border="1" when rendered
		}

		[Test]
		public void Render ()
		{
			TestTable t = new TestTable ();
			string s = t.Render ();
			Assert.AreEqual ("<table border=\"0\">\n\n</table>", s, "empty/default");

			t.CellPadding = 1;
			s = t.Render ();
			Assert.AreEqual ("<table cellpadding=\"1\" border=\"0\">\n\n</table>", s, "CellPadding");
			t.CellPadding = -1;

			t.CellSpacing = 2;
			s = t.Render ();
			Assert.AreEqual ("<table cellspacing=\"2\" border=\"0\">\n\n</table>", s, "CellSpacing");
			t.CellSpacing = -1;

			t.GridLines = GridLines.Horizontal;
			s = t.Render ();
			Assert.AreEqual ("<table rules=\"rows\" border=\"1\">\n\n</table>", s, "GridLines.Horizontal");
			t.GridLines = GridLines.Vertical;
			s = t.Render ();
			Assert.AreEqual ("<table rules=\"cols\" border=\"1\">\n\n</table>", s, "GridLines.Vertical");
			t.GridLines = GridLines.Both;
			s = t.Render ();
			Assert.AreEqual ("<table rules=\"all\" border=\"1\">\n\n</table>", s, "GridLines.Both");
			t.GridLines = GridLines.None;

			t.BorderWidth = new Unit (2);
			s = t.Render ();
			Assert.IsTrue ((s.IndexOf ("border=\"0\"") > 0), "border=0/2");
			t.GridLines = GridLines.Horizontal;
			s = t.Render ();
			Assert.IsTrue ((s.IndexOf ("rules=\"rows\" border=\"2\"") > 0), "2/GridLines.Horizontal");
			t.GridLines = GridLines.Vertical;
			s = t.Render ();
			Assert.IsTrue ((s.IndexOf ("rules=\"cols\" border=\"2\"") > 0), "2/GridLines.Vertical");
			t.GridLines = GridLines.Both;
			s = t.Render ();
			Assert.IsTrue ((s.IndexOf ("rules=\"all\" border=\"2\"") > 0), "2/GridLines.Both");
			t.GridLines = GridLines.None;
			t.BorderWidth = new Unit ();

			t.HorizontalAlign = HorizontalAlign.Left;
			s = t.Render ();
			Assert.AreEqual ("<table align=\"left\" border=\"0\">\n\n</table>", s.ToLower (), "HorizontalAlign.Left");
			t.HorizontalAlign = HorizontalAlign.Center;
			s = t.Render ();
			Assert.AreEqual ("<table align=\"center\" border=\"0\">\n\n</table>", s.ToLower (), "HorizontalAlign.Center");
			t.HorizontalAlign = HorizontalAlign.Right;
			s = t.Render ();
			Assert.AreEqual ("<table align=\"right\" border=\"0\">\n\n</table>", s.ToLower (), "HorizontalAlign.Right");
			t.HorizontalAlign = HorizontalAlign.Justify;
			s = t.Render ();
			Assert.AreEqual ("<table align=\"justify\" border=\"0\">\n\n</table>", s.ToLower (), "HorizontalAlign.Justify");
			t.HorizontalAlign = HorizontalAlign.NotSet;

			t.Caption = "mono";
			s = t.Render ();
			Assert.AreEqual ("<table border=\"0\">\n\t<caption>\n\t\tmono\n\t</caption>\n</table>", s.ToLower (), "Caption");

			t.CaptionAlign = TableCaptionAlign.Top;
			s = t.Render ();
			Assert.AreEqual ("<table border=\"0\">\n\t<caption align=\"top\">\n\t\tmono\n\t</caption>\n</table>", s.ToLower (), "Caption/Top");
			t.CaptionAlign = TableCaptionAlign.Bottom;
			s = t.Render ();
			Assert.AreEqual ("<table border=\"0\">\n\t<caption align=\"bottom\">\n\t\tmono\n\t</caption>\n</table>", s.ToLower (), "Caption/Bottom");
			t.CaptionAlign = TableCaptionAlign.Right;
			s = t.Render ();
			Assert.AreEqual ("<table border=\"0\">\n\t<caption align=\"right\">\n\t\tmono\n\t</caption>\n</table>", s.ToLower (), "Caption/Right");
			t.CaptionAlign = TableCaptionAlign.Left;
			s = t.Render ();
			Assert.AreEqual ("<table border=\"0\">\n\t<caption align=\"left\">\n\t\tmono\n\t</caption>\n</table>", s.ToLower (), "Caption/Left");
			t.Caption = null;
			s = t.Render ();
			Assert.AreEqual ("<table border=\"0\">\n\n</table>", s, "CaptionAlign without Caption");
			t.CaptionAlign = TableCaptionAlign.NotSet;

			t.BackImageUrl = imageUrl;
			s = t.Render ();
			Assert.AreEqual ("<table border=\"0\" style=\"background-image:url(http://www.mono-project.com/stylesheets/images.wiki.png);\">\n\n</table>", s, "BackImageUrl");
			t.BackImageUrl = localImageUrl;
			s = t.Render ();
			Assert.AreEqual ("<table border=\"0\" style=\"background-image:url(foo.jpg);\">\n\n</table>", s, "BackImageUrl");
			t.BackImageUrl = String.Empty;
		}

#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void RenderInAspxPage ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (RenderInAspxPage_OnLoad));
			string res = t.Run ();
			Assert.IsTrue (res.IndexOf ("<table id=\"MagicID_A1C3\" border=\"0\" style=\"background-image:url(foo.jpg);\"")!= -1, res);
		}

		public static void RenderInAspxPage_OnLoad (Page p)
		{
			Table t = new Table ();
			t.BackImageUrl = "foo.jpg";
			t.ID = "MagicID_A1C3";
			p.Form.Controls.Add (t);
			p.Controls.Add (t);
		}
#endif

		[Test]
		public void CreateControlStyle ()
		{
			TestTable t = new TestTable ();
			t.BackImageUrl = imageUrl;
			t.CellPadding = 1;
			t.CellSpacing = 2;
			t.GridLines = GridLines.Horizontal;
			t.HorizontalAlign = HorizontalAlign.Left;

			TableStyle ts = (TableStyle)t.GetStyle ();
			// is it live ?
			ts.BackImageUrl = "mono";
			Assert.AreEqual ("mono", t.BackImageUrl, "BackImageUrl-2");
			ts.CellPadding = Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, t.CellPadding, "CellPadding-2");
			ts.CellSpacing = 0;
			Assert.AreEqual (0, t.CellSpacing, "CellSpacing-2");
			ts.GridLines = GridLines.Vertical;
			Assert.AreEqual (GridLines.Vertical, t.GridLines, "GridLines-2");
			ts.HorizontalAlign = HorizontalAlign.Right;
			Assert.AreEqual (HorizontalAlign.Right, t.HorizontalAlign, "HorizontalAlign-2");
		}

		private string Adjust (string s)
		{
			// right now Mono doesn't generate the exact same indentation/lines as MS implementation
			// and different fx versions have different casing for enums
			return s.Replace ("\n", "").Replace ("\t", "").ToLower ();
		}

		[Test]
		public void Rows ()
		{
			TestTable t = new TestTable ();
			Assert.AreEqual (0, t.Rows.Count, "0");
			TableRow tr = new TableRow ();

			t.Rows.Add (tr);
			Assert.AreEqual (1, t.Rows.Count, "r1");
			Assert.AreEqual (1, t.Controls.Count, "c1");
			string s = t.Render ();
			Assert.AreEqual (Adjust ("<table border=\"0\">\n\t<tr>\n\n\t</tr>\n</table>"), Adjust (s), "tr-1");

			// change instance properties
			tr.HorizontalAlign = HorizontalAlign.Justify;
			s = t.Render ();
			Assert.AreEqual (Adjust ("<table border=\"0\">\n\t<tr align=\"justify\">\n\n\t</tr>\n</table>"), Adjust (s), "tr-1j");

			// add it again (same instance)
			t.Rows.Add (tr);
			Assert.AreEqual (1, t.Rows.Count, "t1bis");
			Assert.AreEqual (1, t.Controls.Count, "c1bis");
			s = t.Render ();
			Assert.AreEqual (Adjust ("<table border=\"0\">\n\t<tr align=\"justify\">\n\n\t</tr>\n</table>"), Adjust (s), "tr-1bis");
			tr.HorizontalAlign = HorizontalAlign.NotSet;

			tr = new TableRow ();
			tr.HorizontalAlign = HorizontalAlign.Justify;
			t.Rows.Add (tr);
			Assert.AreEqual (2, t.Rows.Count, "r2");
			Assert.AreEqual (2, t.Controls.Count, "c2");
			s = t.Render ();
			Assert.AreEqual (Adjust ("<table border=\"0\">\n\t<tr>\n\n\t</tr><tr align=\"justify\">\n\n\t</tr>\n</table>"), Adjust (s), "tr-2");

			tr = new TableRow ();
			tr.VerticalAlign = VerticalAlign.Bottom;
			t.Controls.Add (tr);
			Assert.AreEqual (3, t.Rows.Count, "r3");
			Assert.AreEqual (3, t.Controls.Count, "c3");
			s = t.Render ();
			Assert.AreEqual (Adjust ("<table border=\"0\">\n\t<tr>\n\n\t</tr><tr align=\"justify\">\n\n\t</tr><tr valign=\"bottom\">\n\n\t</tr>\n</table>"), Adjust (s), "tr-3");

			t.Caption = "caption";
			s = t.Render ();
			Assert.AreEqual (Adjust ("<table border=\"0\">\n\t<caption>\n\t\tcaption\n\t</caption><tr>\n\n\t</tr><tr align=\"justify\">\n\n\t</tr><tr valign=\"bottom\">\n\n\t</tr>\n</table>"), Adjust (s), "tr-2c");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ControlsAdd_Null ()
		{
			new Table ().Controls.Add (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ControlsAdd_LiteralControl ()
		{
			new Table ().Controls.Add (new LiteralControl ("mono"));
		}

		[Test]
		public void ControlsAdd_TableRow ()
		{
			Table t = new Table ();
			t.Controls.Add (new TableRow ());
			Assert.AreEqual (1, t.Controls.Count, "Controls");
			Assert.AreEqual (1, t.Rows.Count, "Rows");
		}

		[Test]
		public void ControlsAdd_TestTableRow ()
		{
			Table t = new Table ();
			t.Controls.Add (new TestTableRow ());
			Assert.AreEqual (1, t.Controls.Count, "Controls");
			Assert.AreEqual (1, t.Rows.Count, "Rows");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ControlsAddAt_Null ()
		{
			new Table ().Controls.AddAt (0, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ControlsAddAt_Negative ()
		{
			new Table ().Controls.AddAt (Int32.MinValue, new TableRow ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ControlsAddAt_LiteralControl ()
		{
			new Table ().Controls.AddAt (0, new LiteralControl ("mono"));
		}

		[Test]
		public void ControlsAddAt_TableRow ()
		{
			Table t = new Table ();
			t.Controls.AddAt (0, new TableRow ());
			Assert.AreEqual (1, t.Controls.Count, "Controls");
			Assert.AreEqual (1, t.Rows.Count, "Rows");
		}

		[Test]
		public void ControlsAddAt_TestTableRow ()
		{
			Table t = new Table ();
			t.Controls.AddAt (0, new TestTableRow ());
			Assert.AreEqual (1, t.Controls.Count, "Controls");
			Assert.AreEqual (1, t.Rows.Count, "Rows");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void RenderBeginTag_Null ()
		{
			Table t = new Table ();
			t.RenderBeginTag (null);
		}

		[Test]
		public void RenderBeginTag_Empty ()
		{
			HtmlTextWriter writer = GetWriter ();
			Table t = new Table ();
			t.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table border=\"0\">\n", s, "empty");
		}

		[Test]
		public void RenderBeginTag_Attributes ()
		{
			HtmlTextWriter writer = GetWriter ();
			Table t = new Table ();
			t.CellPadding = 1;
			t.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table cellpadding=\"1\" border=\"0\">\n", s, "CellPadding");
		}

		[Test]
		public void RenderBeginTag_Caption ()
		{
			HtmlTextWriter writer = GetWriter ();
			Table t = new Table ();
			t.Caption = "caption";
			t.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table border=\"0\">\n\t<caption>\n\t\tcaption\n\t</caption>", s, "caption");
		}

		[Test]
		public void RenderBeginTag_Caption_Align ()
		{
			HtmlTextWriter writer = GetWriter ();
			Table t = new Table ();
			t.Caption = "caption";
			t.CaptionAlign = TableCaptionAlign.Top;
			t.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table border=\"0\">\n\t<caption align=\"top\">\n\t\tcaption\n\t</caption>", s.ToLower (), "caption");
		}

		[Test]
		public void RenderBeginTag_Row ()
		{
			HtmlTextWriter writer = GetWriter ();
			Table t = new Table ();
			t.Rows.Add (new TableRow ());
			t.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table border=\"0\">\n", s, "tr");
		}

#if NET_2_0
		[Test]
		[Category("NunitWeb")] // Note: No event fired , only flow been checked.
		public void RaisePostBackEvent ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (RaisePostBackEvent__Init));
			string str = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "Table";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
			Assert.AreEqual ("RaisePostBackEvent", (String) t.UserData, "RaisePostBackEvent");
		}

		public static void RaisePostBackEvent__Init (Page page)
		{
			TestTable t = new TestTable ();
			t.ID = "Table";
			page.Form.Controls.Add (t);
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
#endif
	}
}
