//
// Tests for System.Web.UI.WebControls.CheckBoxList.cs 
//
// Author:
//	Jackson Harper (jackson@ximian.com)
//

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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Collections;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Collections.Specialized;

namespace MonoTests.System.Web.UI.WebControls {

	public class CheckBoxListPoker : CheckBoxList {

		public Style CreateStyle ()
		{
			return CreateControlStyle ();
		}

		public Control FindControlPoke (string name, int offset)
		{
			return FindControl (name, offset);
		}

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		
#if NET_2_0
		public new bool HasFooter
		{
			get
			{
				return base.HasFooter;
			}
		}

		public new bool HasHeader
		{
			get
			{
				return base.HasHeader;
			}
		}

		public new bool HasSeparators
		{
			get
			{
				return base.HasSeparators;
			}
		}

		public new int RepeatedItemCount
		{
			get
			{
				return base.RepeatedItemCount;
			}
		}

		public new void RaisePostDataChangedEvent ()
		{
			base.RaisePostDataChangedEvent ();
		}

		protected override Style GetItemStyle (ListItemType itemType, int repeatIndex)
		{
			Style s = new Style();
			s.BackColor = Color.Red;
			s.BorderStyle = BorderStyle.Solid;
			WebTest.CurrentTest.UserData = "GetItemStyle";
			return s;
		}

		public Style DoGetItemStyle (ListItemType itemType, int repeatIndex)
		{
			return base.GetItemStyle (itemType, repeatIndex);
		}

		public new string RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo)
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.RenderItem(itemType,repeatIndex,repeatInfo,writer);
			return writer.InnerWriter.ToString ();
		}
#endif
	}

	[TestFixture]
	public class CheckBoxListTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type t = GetType ();
			WebTest.CopyResource (t, "CheckBoxList_Bug377703_1.aspx", "CheckBoxList_Bug377703_1.aspx");
			WebTest.CopyResource (t, "CheckBoxList_Bug377703_2.aspx", "CheckBoxList_Bug377703_2.aspx");
			WebTest.CopyResource (t, "CheckBoxList_Bug578770.aspx", "CheckBoxList_Bug578770.aspx");
			WebTest.CopyResource (t, "CheckBoxList_Bug600415.aspx", "CheckBoxList_Bug600415.aspx");
		}
		
		[Test]
		public void Defaults ()
		{
			CheckBoxListPoker c = new CheckBoxListPoker ();

			Assert.AreEqual (c.CellPadding, -1, "A1");
			Assert.AreEqual (c.CellSpacing, -1, "A2");
			Assert.AreEqual (c.RepeatColumns, 0, "A3");
			Assert.AreEqual (c.RepeatDirection,
					RepeatDirection.Vertical, "A4");
			Assert.AreEqual (c.RepeatLayout,
					RepeatLayout.Table, "A5");
			Assert.AreEqual (c.TextAlign, TextAlign.Right, "A6");
#if NET_2_0
			Assert.AreEqual (false, c.HasFooter, "HasFooter");
			Assert.AreEqual (false, c.HasHeader, "HasHeader");
			Assert.AreEqual (false, c.HasSeparators, "HasSeparators");
			Assert.AreEqual (0, c.RepeatedItemCount, "RepeatedItemCount");
			Assert.AreEqual (null, c.DoGetItemStyle (ListItemType.Item, 0), "GetItemStyle");
#endif
		}

#if NET_2_0
		[Test]
		public void CheckBoxList_Bug377703_1 ()
		{
			WebTest t = new WebTest ("CheckBoxList_Bug377703_1.aspx");
			t.Invoker = PageInvoker.CreateOnInit (CheckBoxList_Bug377703_1_OnInit);
			string origHtmlFirst = @"<table id=""cbxl1"" border=""0"">
	<tr>
		<td><input id=""cbxl1_0"" type=""checkbox"" name=""cbxl1$0"" /><label for=""cbxl1_0"">x</label></td>

	</tr><tr>
		<td><input id=""cbxl1_1"" type=""checkbox"" name=""cbxl1$1"" /><label for=""cbxl1_1"">y</label></td>
	</tr><tr>
		<td><input id=""cbxl1_2"" type=""checkbox"" name=""cbxl1$2"" /><label for=""cbxl1_2"">z</label></td>
	</tr>
</table>";
			string origHtmlSecond = @"<table id=""cbxl1"" border=""0"">
	<tr>
		<td><input id=""cbxl1_0"" type=""checkbox"" name=""cbxl1$0"" checked=""checked"" /><label for=""cbxl1_0"">x</label></td>

	</tr><tr>
		<td><input id=""cbxl1_1"" type=""checkbox"" name=""cbxl1$1"" /><label for=""cbxl1_1"">y</label></td>
	</tr><tr>
		<td><input id=""cbxl1_2"" type=""checkbox"" name=""cbxl1$2"" /><label for=""cbxl1_2"">z</label></td>
	</tr>
</table>";
			string html = t.Run ();
			string listHtml = HtmlDiff.GetControlFromPageHtml (html);

			HtmlDiff.AssertAreEqual (origHtmlFirst, listHtml, "#A1");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("cbxl1$0");
			fr.Controls ["cbxl1$0"].Value = "on";

			fr.Controls.Add ("ctl01");
			fr.Controls ["ctl01"].Value = "Click me twice to have the first Item become empty";

			t.Request = fr;
			html = t.Run ();

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("cbxl1$0");
			fr.Controls ["cbxl1$0"].Value = "on";

			fr.Controls.Add ("ctl01");
			fr.Controls ["ctl01"].Value = "Click me twice to have the first Item become empty";

			t.Request = fr;
			html = t.Run ();

			listHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origHtmlSecond, listHtml, "#A2");
		}

		public static void CheckBoxList_Bug377703_1_OnInit (Page p)
		{
			CheckBoxList cbxl1 = p.FindControl ("cbxl1") as CheckBoxList;
			
			cbxl1.DataSource = new[] {
				new { ID = "x", Text = "X" },
				new { ID = "y", Text = "Y" },
				new { ID = "z", Text = "Z" },
			};
			cbxl1.DataValueField = "ID";
			cbxl1.DataTextField = "ID";
			cbxl1.DataBind();
		}

		[Test]
		public void CheckBoxList_Bug377703_2 ()
		{
			WebTest t = new WebTest ("CheckBoxList_Bug377703_2.aspx");
			t.Invoker = PageInvoker.CreateOnInit (CheckBoxList_Bug377703_2_OnInit);
			string origHtmlFirst = @"<table id=""cbxl2"" border=""0"">
	<tr>
		<td><input id=""cbxl2_0"" type=""checkbox"" name=""cbxl2$0"" /><label for=""cbxl2_0"">x</label></td>

	</tr><tr>
		<td><input id=""cbxl2_1"" type=""checkbox"" name=""cbxl2$1"" /><label for=""cbxl2_1"">y</label></td>
	</tr><tr>
		<td><input id=""cbxl2_2"" type=""checkbox"" name=""cbxl2$2"" /><label for=""cbxl2_2"">z</label></td>
	</tr>
</table>";
			string origHtmlSecond = @"<table id=""cbxl2"" disabled=""disabled"" border=""0"">
	<tr>
		<td><span disabled=""disabled""><input id=""cbxl2_0"" type=""checkbox"" name=""cbxl2$0"" disabled=""disabled"" checked=""checked"" /><label for=""cbxl2_0"">x</label></span></td>

	</tr><tr>
		<td><span disabled=""disabled""><input id=""cbxl2_1"" type=""checkbox"" name=""cbxl2$1"" disabled=""disabled"" /><label for=""cbxl2_1"">y</label></span></td>
	</tr><tr>
		<td><span disabled=""disabled""><input id=""cbxl2_2"" type=""checkbox"" name=""cbxl2$2"" disabled=""disabled"" checked=""checked"" /><label for=""cbxl2_2"">z</label></span></td>
	</tr>
</table>";
			string origHtmlThird = @"<table id=""cbxl2"" disabled=""disabled"" border=""0"">
	<tr>
		<td><span disabled=""disabled""><input id=""cbxl2_0"" type=""checkbox"" name=""cbxl2$0"" disabled=""disabled"" checked=""checked"" /><label for=""cbxl2_0"">x</label></span></td>

	</tr><tr>
		<td><span disabled=""disabled""><input id=""cbxl2_1"" type=""checkbox"" name=""cbxl2$1"" disabled=""disabled"" /><label for=""cbxl2_1"">y</label></span></td>
	</tr><tr>
		<td><span disabled=""disabled""><input id=""cbxl2_2"" type=""checkbox"" name=""cbxl2$2"" disabled=""disabled"" checked=""checked"" /><label for=""cbxl2_2"">z</label></span></td>
	</tr>
</table>";
			string html = t.Run ();
			string listHtml = HtmlDiff.GetControlFromPageHtml (html);

			HtmlDiff.AssertAreEqual (origHtmlFirst, listHtml, "#A1");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("cbxl2$0");
			fr.Controls ["cbxl2$0"].Value = "on";

			fr.Controls.Add ("cbxl2$2");
			fr.Controls ["cbxl2$2"].Value = "on";
			
			fr.Controls.Add ("ctl01");
			fr.Controls ["ctl01"].Value = "Click to toggle enable status above";

			t.Request = fr;
			html = t.Run ();

			listHtml = HtmlDiff.GetControlFromPageHtml (html);

			HtmlDiff.AssertAreEqual (origHtmlSecond, listHtml, "#A2");
			
			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("ctl02");
			fr.Controls ["ctl02"].Value = "Click to refresh page";

			t.Request = fr;
			html = t.Run ();

			listHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origHtmlThird, listHtml, "#A3");
		}

		public static void CheckBoxList_Bug377703_2_OnInit (Page p)
		{
			CheckBoxList cbxl2 = p.FindControl ("cbxl2") as CheckBoxList;
			
			cbxl2.DataSource = new[] {
				new { ID = "x", Text = "X" },
				new { ID = "y", Text = "Y" },
				new { ID = "z", Text = "Z" },
			};
			cbxl2.DataValueField = "ID";
			cbxl2.DataTextField = "ID";
			cbxl2.DataBind();
		}

		[Test]
		public void CheckBoxList_Bug578770 ()
		{
			WebTest t = new WebTest ("CheckBoxList_Bug578770.aspx");
			t.Invoker = PageInvoker.CreateOnInit (CheckBoxList_Bug578770_OnInit);
			string origHtml = @"<table id=""test"" border=""0"">

	<tr>
		<td><span disabled=""disabled""><input id=""test_0"" type=""checkbox"" name=""test$0"" disabled=""disabled"" /><label for=""test_0"">Sun</label></span></td>
	</tr><tr>
		<td><input id=""test_1"" type=""checkbox"" name=""test$1"" /><label for=""test_1"">Mon</label></td>
	</tr><tr>
		<td><input id=""test_2"" type=""checkbox"" name=""test$2"" /><label for=""test_2"">Tue</label></td>
	</tr><tr>

		<td><input id=""test_3"" type=""checkbox"" name=""test$3"" /><label for=""test_3"">Wed</label></td>
	</tr><tr>
		<td><input id=""test_4"" type=""checkbox"" name=""test$4"" /><label for=""test_4"">Thu</label></td>
	</tr><tr>
		<td><input id=""test_5"" type=""checkbox"" name=""test$5"" /><label for=""test_5"">Fri</label></td>
	</tr><tr>
		<td><input id=""test_6"" type=""checkbox"" name=""test$6"" /><label for=""test_6"">Sat</label></td>

	</tr>
</table>";
			string html = t.Run ();
			string listHtml = HtmlDiff.GetControlFromPageHtml (html);

			HtmlDiff.AssertAreEqual (origHtml, listHtml, "#A1");
		}

		public static void CheckBoxList_Bug578770_OnInit (Page p)
		{
			CheckBoxList test = p.FindControl ("test") as CheckBoxList;
			string[] weekDays = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
			test.DataSource = weekDays;
			test.DataBind();
			test.Items[0].Enabled = false;
		}

		[Test]
		public void CheckBoxList_Bug600415 ()
		{
			WebTest t = new WebTest ("CheckBoxList_Bug600415.aspx");
			string origHtmlFirst = @"<table id=""checkBoxList"" border=""0"">
	<tr>

		<td><input id=""checkBoxList_0"" type=""checkbox"" name=""checkBoxList$0"" checked=""checked"" /><label for=""checkBoxList_0"">Item 1</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_1"" type=""checkbox"" name=""checkBoxList$1"" /><label for=""checkBoxList_1"">Item 2</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_2"" type=""checkbox"" name=""checkBoxList$2"" checked=""checked"" /><label for=""checkBoxList_2"">Item 3</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_3"" type=""checkbox"" name=""checkBoxList$3"" /><label for=""checkBoxList_3"">Item 4</label></td>

	</tr>
</table>";
			string origHtmlSecond = @"<table id=""checkBoxList"" border=""0"">
	<tr>

		<td><input id=""checkBoxList_0"" type=""checkbox"" name=""checkBoxList$0"" /><label for=""checkBoxList_0"">Item 1</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_1"" type=""checkbox"" name=""checkBoxList$1"" /><label for=""checkBoxList_1"">Item 2</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_2"" type=""checkbox"" name=""checkBoxList$2"" /><label for=""checkBoxList_2"">Item 3</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_3"" type=""checkbox"" name=""checkBoxList$3"" /><label for=""checkBoxList_3"">Item 4</label></td>

	</tr>
</table>";
			string origHtmlThird = @"<table id=""checkBoxList"" border=""0"">
	<tr>

		<td><input id=""checkBoxList_0"" type=""checkbox"" name=""checkBoxList$0"" checked=""checked"" /><label for=""checkBoxList_0"">Item 1</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_1"" type=""checkbox"" name=""checkBoxList$1"" checked=""checked"" /><label for=""checkBoxList_1"">Item 2</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_2"" type=""checkbox"" name=""checkBoxList$2"" checked=""checked"" /><label for=""checkBoxList_2"">Item 3</label></td>
	</tr><tr>
		<td><input id=""checkBoxList_3"" type=""checkbox"" name=""checkBoxList$3"" checked=""checked"" /><label for=""checkBoxList_3"">Item 4</label></td>

	</tr>
</table>";
			string html = t.Run ();
			string listHtml = HtmlDiff.GetControlFromPageHtml (html);

			HtmlDiff.AssertAreEqual (origHtmlFirst, listHtml, "#A1");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("cmdClick");
			fr.Controls ["cmdClick"].Value = "Ok";

			t.Request = fr;
			html = t.Run ();
			
			listHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origHtmlSecond, listHtml, "#A2");

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("checkBoxList$0");
			fr.Controls ["checkBoxList$0"].Value = "on";
			fr.Controls.Add ("checkBoxList$1");
			fr.Controls ["checkBoxList$1"].Value = "on";
			fr.Controls.Add ("checkBoxList$2");
			fr.Controls ["checkBoxList$2"].Value = "on";
			fr.Controls.Add ("checkBoxList$3");
			fr.Controls ["checkBoxList$3"].Value = "on";

			t.Request = fr;
			html = t.Run ();
			listHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origHtmlThird, listHtml, "#A3");
		}
		
		[Test]
		public void RaisePostDataChangedEvent ()
		{
			CheckBoxListPoker c = new CheckBoxListPoker ();
			c.SelectedIndexChanged += new EventHandler (c_SelectedIndexChanged);
			Assert.AreEqual (false, eventSelectedIndexChanged, "RaisePostDataChangedEvent#1");
			c.RaisePostDataChangedEvent ();
			Assert.AreEqual (true, eventSelectedIndexChanged, "RaisePostDataChangedEvent#2");
		}

		bool eventSelectedIndexChanged;
		void c_SelectedIndexChanged (object sender, EventArgs e)
		{
			eventSelectedIndexChanged = true;
		}

		[Test]
		[Category("NunitWeb")]
		public void GetItemStyle ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (GetItemStyle_Load));
			string html = t.Run ();
			string ctrl = HtmlDiff.GetControlFromPageHtml (html);
			if (ctrl == string.Empty)
				Assert.Fail ("CheckBoxList not created fail");
			Assert.AreEqual ("GetItemStyle", (string) t.UserData, "GetItemStyle not done");
			if ( ctrl.IndexOf("<td style=\"background-color:Red;border-style:Solid;\">") == -1)
				Assert.Fail ("CheckBoxList style not rendered");
		}

		public static void GetItemStyle_Load (Page p)
		{
			CheckBoxListPoker c = new CheckBoxListPoker ();
			ListItem l1 = new ListItem ("item1", "value1");
			ListItem l2 = new ListItem ("item2", "value2");

			c.Items.Add (l1);
			c.Items.Add (l2);
			p.Form.Controls.Add(new LiteralControl(HtmlDiff.BEGIN_TAG));
			p.Form.Controls.Add (c);
			p.Form.Controls.Add(new LiteralControl(HtmlDiff.END_TAG));
		}

		[Test]
		public void RenderItem ()
		{
			CheckBoxListPoker c = new CheckBoxListPoker ();
			ListItem l1 = new ListItem ("item1", "value1");
			ListItem l2 = new ListItem ("item2", "value2");

			c.Items.Add (l1);
			c.Items.Add (l2);
			string html = c.RenderItem (ListItemType.Item, 0, null);
			HtmlDiff.AssertAreEqual ("<input id=\"0\" type=\"checkbox\" name=\"0\" /><label for=\"0\">item1</label>", html, "RenderItem#1");
			html = c.RenderItem (ListItemType.Item, 1, null);
			HtmlDiff.AssertAreEqual ("<input id=\"1\" type=\"checkbox\" name=\"1\" /><label for=\"1\">item2</label>", html, "RenderItem#2");
		}

		[Test]
		public void RepeatedItemCount ()
		{
			CheckBoxListPoker c = new CheckBoxListPoker ();
			ListItem l1 = new ListItem ("item1", "value1");
			ListItem l2 = new ListItem ("item2", "value2");
			Assert.AreEqual (0, c.RepeatedItemCount, "RepeatedItemCount#1");
			c.Items.Add (l1);
			c.Items.Add (l2);
			Assert.AreEqual (2, c.RepeatedItemCount, "RepeatedItemCount#2");
		}

#endif


		[Test]
		public void CleanProperties ()
		{
			CheckBoxList c = new CheckBoxList ();

			c.CellPadding = Int32.MaxValue;
			Assert.AreEqual (c.CellPadding, Int32.MaxValue, "A1");

			c.CellSpacing = Int32.MaxValue;
			Assert.AreEqual (c.CellSpacing, Int32.MaxValue, "A2");

			c.RepeatColumns = Int32.MaxValue;
			Assert.AreEqual (c.RepeatColumns, Int32.MaxValue, "A3");

			foreach (RepeatDirection d in
					Enum.GetValues (typeof (RepeatDirection))) {
				c.RepeatDirection = d;
				Assert.AreEqual (c.RepeatDirection, d, "A4-" + d);
			}

			foreach (RepeatLayout l in
					Enum.GetValues (typeof (RepeatLayout))) {
				c.RepeatLayout = l;
				Assert.AreEqual (c.RepeatLayout, l, "A5-" + l);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellPaddingTooLow ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.CellPadding = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellSpacingTooLow ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.CellSpacing = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatColumsTooLow ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.RepeatColumns = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatDirection_Invalid ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.RepeatDirection = (RepeatDirection) Int32.MaxValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatLayout_Invalid ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.RepeatLayout = (RepeatLayout) Int32.MaxValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TextAlign_Invalid ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.TextAlign = (TextAlign) Int32.MaxValue;
		}

		[Test]
		public void ChildCheckBoxControl ()
		{
			CheckBoxList c = new CheckBoxList ();
			Assert.AreEqual (c.Controls.Count, 1, "A1");
			Assert.AreEqual (c.Controls [0].GetType (), typeof (CheckBox), "A2");
		}

		[Test]
		public void CreateStyle ()
		{
			CheckBoxListPoker c = new CheckBoxListPoker ();
			Assert.AreEqual (c.CreateStyle ().GetType (), typeof (TableStyle), "A1");
		}

		[Test]
		public void RepeatInfoProperties ()
		{
			IRepeatInfoUser ri = new CheckBoxList ();

			Assert.IsFalse (ri.HasFooter, "A1");
			Assert.IsFalse (ri.HasHeader, "A2");
			Assert.IsFalse (ri.HasSeparators, "A3");
			Assert.AreEqual (ri.RepeatedItemCount, 0, "A4");
		}

		[Test]
		public void RepeatInfoCount ()
		{
			CheckBoxList c = new CheckBoxList ();
			IRepeatInfoUser ri = (IRepeatInfoUser) c;

			Assert.AreEqual (ri.RepeatedItemCount, 0, "A1");

			c.Items.Add ("one");
			c.Items.Add ("two");
			c.Items.Add ("three");
			Assert.AreEqual (ri.RepeatedItemCount, 3, "A2");
		}

		[Test]
		public void RepeatInfoStyle ()
		{
			IRepeatInfoUser ri = new CheckBoxList ();

			foreach (ListItemType t in Enum.GetValues (typeof (ListItemType))) {
				Assert.AreEqual (ri.GetItemStyle (t, 0), null, "A1-" + t);
				Assert.AreEqual (ri.GetItemStyle (t, 1), null, "A2-" + t);
				Assert.AreEqual (ri.GetItemStyle (t, 2), null, "A3-" + t);
				Assert.AreEqual (ri.GetItemStyle (t, 3), null, "A4-" + t);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatInfoRenderOutOfRange ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			IRepeatInfoUser ri = new CheckBoxList ();

			ri.RenderItem (ListItemType.Item, -1, new RepeatInfo (), tw); 
		}

		[Test]
		public void RepeatInfoRenderItem ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			CheckBoxList c = new CheckBoxList ();
			IRepeatInfoUser ri = (IRepeatInfoUser) c;
			RepeatInfo r = new RepeatInfo ();

			c.Items.Add ("one");
			c.Items.Add ("two");

			ri.RenderItem (ListItemType.Item, 0, r, tw); 
			Assert.AreEqual ("<input id=\"0\" type=\"checkbox\" name=\"0\" />" +
					"<label for=\"0\">one</label>", sw.ToString (), "A1");
		}

		[Test]
		public void FindControl ()
		{
			CheckBoxListPoker p = new CheckBoxListPoker ();

			p.ID = "id";
			p.Items.Add ("one");
			p.Items.Add ("two");
			p.Items.Add ("three");

			// Everything seems to return this.
			Assert.AreEqual (p.FindControlPoke (String.Empty, 0), p, "A1");
			Assert.AreEqual (p.FindControlPoke ("id", 0), p, "A2");
			Assert.AreEqual (p.FindControlPoke ("id_0", 0), p, "A3");
			Assert.AreEqual (p.FindControlPoke ("id_1", 0), p, "A4");
			Assert.AreEqual (p.FindControlPoke ("id_2", 0), p, "A5");
			Assert.AreEqual (p.FindControlPoke ("id_3", 0), p, "A6");
			Assert.AreEqual (p.FindControlPoke ("0", 0), p, "A7");

			Assert.AreEqual (p.FindControlPoke (String.Empty, 10), p, "A1");
			Assert.AreEqual (p.FindControlPoke ("id", 10), p, "A2");
			Assert.AreEqual (p.FindControlPoke ("id_0", 10), p, "A3");
			Assert.AreEqual (p.FindControlPoke ("id_1", 10), p, "A4");
			Assert.AreEqual (p.FindControlPoke ("id_2", 10), p, "A5");
			Assert.AreEqual (p.FindControlPoke ("id_3", 10), p, "A6");
			Assert.AreEqual (p.FindControlPoke ("0", 10), p, "A7");
		}

		private void Render (CheckBoxList list, string expected, string test)
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new CleanHtmlTextWriter (sw);
			sw.NewLine = "\n";

			list.RenderControl (tw);
			HtmlDiff.AssertAreEqual (expected, sw.ToString (), test);
		}

		[Test]
		public void RenderEmpty ()
		{
			CheckBoxList c = new CheckBoxList ();

#if NET_2_0
			Render (c, "", "A1");
#else
			Render (c, "<table border=\"0\">\n\n</table>", "A1");
#endif
			c.CellPadding = 1;
#if NET_2_0
			Render (c, "", "A2");
#else
			Render (c, "<table border=\"0\" cellpadding=\"1\">\n\n</table>", "A2");
#endif

			c = new CheckBoxList ();
			c.CellPadding = 1;
#if NET_2_0
			Render (c, "", "A3");
#else
			Render (c, "<table border=\"0\" cellpadding=\"1\">\n\n</table>", "A3");
#endif

			c = new CheckBoxList ();
			c.TextAlign = TextAlign.Left;
#if NET_2_0
			Render (c, "", "A4");
#else
			Render (c, "<table border=\"0\">\n\n</table>", "A4");
#endif
		}

		[Test]
#if NET_2_0
		[Category("NotDotNet")] // MS's implementation throws NRE's from these
#endif
		public void Render ()
		{
			CheckBoxList c;
			c = new CheckBoxList ();
			c.Items.Add ("foo");
			Render (c, "<table border=\"0\">\n\t<tr>\n\t\t<td><input id=\"0\" " +
					"name=\"0\" type=\"checkbox\" />" +
					"<label for=\"0\">foo</label>" +
					"</td>\n\t</tr>\n</table>", "A5");

			c = new CheckBoxList ();
			c.Items.Add ("foo");
			Render (c, "<table border=\"0\">\n\t<tr>\n\t\t<td><input id=\"0\" " +
					"name=\"0\" type=\"checkbox\" />" +
					"<label for=\"0\">foo</label>" +
					"</td>\n\t</tr>\n</table>", "A6");
		}

		// bug 51648
		[Test]
#if NET_2_0
		[Category("NotDotNet")] // MS's implementation throws NRE's from these
#endif
		public void TestTabIndex ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.TabIndex = 5;
			c.Items.Add ("Item1");
			string exp = @"<table border=""0"">
	<tr>
		<td><input id=""0"" name=""0"" tabindex=""5"" type=""checkbox"" /><label for=""0"">Item1</label></td>
	</tr>
</table>";
			Render (c, exp, "B1");
		}

		// bug 48802
		[Test]
#if NET_2_0
		[Category("NotDotNet")] // MS's implementation throws NRE's from these
#endif
		public void TestDisabled ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.Enabled = false;
			c.Items.Add ("Item1");
			string exp = @"<table border=""0"" disabled=""disabled"">
	<tr>
		<td><span disabled=""disabled""><input disabled=""disabled"" id=""0"" name=""0"" type=""checkbox"" /><label for=""0"">Item1</label></span></td>
	</tr>
</table>";
			Render (c, exp, "C1");
		}	
#if NET_2_0
        class TestCheckBoxList : CheckBoxList
        {
            public new virtual void VerifyMultiSelect()
            {
                base.VerifyMultiSelect();
            }
        }
        [Test]
        public void VerifyMultiSelectTest()
        {
            TestCheckBoxList list = new TestCheckBoxList();
            list.VerifyMultiSelect();
        }
        [TestFixtureTearDown]
		public void teardown ()
		{
			WebTest.Unload ();
		}
#endif
	}
}


