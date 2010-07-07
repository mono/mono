//
// DataListItemTest.cs
//	- Unit tests for System.Web.UI.WebControls.DataListItem
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
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestDataListItem : DataListItem {

		public TestDataListItem (int index, ListItemType type)
			: base (index, type)
		{
		}


		public string Render (bool extractRows, bool tableLayout)
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.RenderItem (writer, extractRows, tableLayout);
			return writer.InnerWriter.ToString ();
		}

		public void SetType (ListItemType type)
		{
			base.SetItemType (type);
		}
	}

	[TestFixture]
	public class DataListItemTest {

		private void BaseTests (TestDataListItem dli)
		{
			Assert.IsNull (dli.DataItem, "DataItem");
			Assert.AreEqual (String.Empty, dli.Render (true, true), "Render-Empty-T-T");
			Assert.AreEqual (String.Empty, dli.Render (true, false), "Render-Empty-T-F");
			Assert.AreEqual (String.Empty, dli.Render (false, true), "Render-Empty-F-T");
			Assert.AreEqual ("<span></span>", dli.Render (false, false), "Render-Empty-F-F");

			dli.DataItem = (object)Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, dli.DataItem, "DataItem-Int32");
			Assert.AreEqual (String.Empty, dli.Render (true, true), "Render-Int32-T-T");
			Assert.AreEqual (String.Empty, dli.Render (true, false), "Render-Int32-T-F");
			Assert.AreEqual (String.Empty, dli.Render (false, true), "Render-Int32-F-T");
			Assert.AreEqual ("<span></span>", dli.Render (false, false), "Render-Int32-F-F");

			dli.DataItem = (object)"mono";
			Assert.AreEqual ("mono", dli.DataItem, "DataItem-String");
			Assert.AreEqual (String.Empty, dli.Render (true, true), "Render-String-T-T");
			Assert.AreEqual (String.Empty, dli.Render (true, false), "Render-String-T-F");
			Assert.AreEqual (String.Empty, dli.Render (false, true), "Render-String-F-T");
			Assert.AreEqual ("<span></span>", dli.Render (false, false), "Render-String-F-F");
		}

		private void DataItemContainer (TestDataListItem dli, int index)
		{
#if NET_2_0
			IDataItemContainer dic = (dli as IDataItemContainer);
			Assert.IsNull (dic.DataItem, "IDataItemContainer-DataItem");
			Assert.AreEqual (index, dic.DataItemIndex, "IDataItemContainer-DataItemIndex");
			Assert.AreEqual (index, dic.DisplayIndex, "IDataItemContainer-DisplayIndex");
#endif
		}

		[Test]
		public void AlternatingItem ()
		{
			TestDataListItem dli = new TestDataListItem (0, ListItemType.AlternatingItem);
			Assert.AreEqual (0, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual (ListItemType.AlternatingItem, dli.ItemType, "ItemType");

			DataItemContainer (dli, 0);
			BaseTests (dli);

			dli.SetType (ListItemType.EditItem);
			Assert.AreEqual (ListItemType.EditItem, dli.ItemType, "SetItemType");
		}

		[Test]
		public void EditItem ()
		{
			TestDataListItem dli = new TestDataListItem (Int32.MaxValue, ListItemType.EditItem);
			Assert.AreEqual (Int32.MaxValue, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual (ListItemType.EditItem, dli.ItemType, "ItemType");

			DataItemContainer (dli, Int32.MaxValue);
			BaseTests (dli);

			dli.SetType (ListItemType.Footer);
			Assert.AreEqual (ListItemType.Footer, dli.ItemType, "SetItemType");
		}

		[Test]
		public void Footer ()
		{
			TestDataListItem dli = new TestDataListItem (Int32.MinValue, ListItemType.Footer);
			Assert.AreEqual (Int32.MinValue, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual (ListItemType.Footer, dli.ItemType, "ItemType");

			DataItemContainer (dli, Int32.MinValue);
			BaseTests (dli);

			dli.SetType (ListItemType.Header);
			Assert.AreEqual (ListItemType.Header, dli.ItemType, "SetItemType");
		}

		[Test]
		public void Header ()
		{
			TestDataListItem dli = new TestDataListItem (0, ListItemType.Header);
			Assert.AreEqual (0, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual (ListItemType.Header, dli.ItemType, "ItemType");

			DataItemContainer (dli, 0);
			BaseTests (dli);

			dli.SetType (ListItemType.Item);
			Assert.AreEqual (ListItemType.Item, dli.ItemType, "SetItemType");
		}

		[Test]
		public void Item ()
		{
			TestDataListItem dli = new TestDataListItem (0, ListItemType.Item);
			Assert.AreEqual (0, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual (ListItemType.Item, dli.ItemType, "ItemType");

			DataItemContainer (dli, 0);
			BaseTests (dli);

			dli.SetType (ListItemType.Pager);
			Assert.AreEqual (ListItemType.Pager, dli.ItemType, "SetItemType");
		}

		[Test]
		public void Pager ()
		{
			TestDataListItem dli = new TestDataListItem (0, ListItemType.Pager);
			Assert.AreEqual (0, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual (ListItemType.Pager, dli.ItemType, "ItemType");

			DataItemContainer (dli, 0);
			BaseTests (dli);

			dli.SetType (ListItemType.SelectedItem);
			Assert.AreEqual (ListItemType.SelectedItem, dli.ItemType, "SetItemType");
		}

		[Test]
		public void SelectedItem ()
		{
			TestDataListItem dli = new TestDataListItem (1, ListItemType.SelectedItem);
			Assert.AreEqual (1, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual (ListItemType.SelectedItem, dli.ItemType, "ItemType");

			DataItemContainer (dli, 1);
			BaseTests (dli);

			dli.SetType (ListItemType.Separator);
			Assert.AreEqual (ListItemType.Separator, dli.ItemType, "SetItemType");
		}

		[Test]
		public void Separator ()
		{
			TestDataListItem dli = new TestDataListItem (-1, ListItemType.Separator);
			Assert.AreEqual (-1, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual (ListItemType.Separator, dli.ItemType, "ItemType");

			DataItemContainer (dli, -1);
			BaseTests (dli);

			dli.SetType (ListItemType.AlternatingItem);
			Assert.AreEqual (ListItemType.AlternatingItem, dli.ItemType, "SetItemType");
		}

		[Test]
		public void Bad_ListItemType ()
		{
			TestDataListItem dli = new TestDataListItem (0, (ListItemType)Int32.MinValue);
			Assert.AreEqual (0, dli.ItemIndex, "ItemIndex");
			Assert.AreEqual ((ListItemType)Int32.MinValue, dli.ItemType, "ItemType");

			DataItemContainer (dli, 0);
			BaseTests (dli);
		}

		private Table GetTable (string s)
		{
			LiteralControl lc = new LiteralControl (s);
			TableCell td = new TableCell ();
			td.Controls.Add (lc);
			TableRow tr = new TableRow ();
			tr.Cells.Add (td);
			Table t = new Table ();
			t.Rows.Add (tr);
			return t;
		}

		private string Adjust (string s)
		{
			// right now Mono doesn't generate the exact same indentation/lines as MS implementation
			// and different fx versions have different casing for enums
			return s.Replace ("\n", "").Replace ("\t", "").ToLower ();
		}

		[Test]
		public void Controls_Table ()
		{
#if NET_4_0
			string origHtml1 = "<tr>\n\t<td>mono</td>\n</tr>";
			string origHtml2 = "<tr>\n\t<td>mono</td>\n</tr>";
			string origHtml3 = "<table>\n\t<tr>\n\t\t<td>mono</td>\n\t</tr>\n</table>";
			string origHtml4 = "<span><table>\n\t<tr>\n\t\t<td>mono</td>\n\t</tr>\n</table></span>";
#else
			string origHtml1 = "<tr>\n\t<td>mono</td>\n</tr>";
			string origHtml2 = "<tr>\n\t<td>mono</td>\n</tr>";
			string origHtml3 = "<table border=\"0\">\n\t<tr>\n\t\t<td>mono</td>\n\t</tr>\n</table>";
			string origHtml4 = "<span><table border=\"0\">\n\t<tr>\n\t\t<td>mono</td>\n\t</tr>\n</table></span>";
#endif
			TestDataListItem dli = new TestDataListItem (0, ListItemType.Item);
			dli.Controls.Add (GetTable ("mono"));

			string renderedHtml = dli.Render (true, true);
			Assert.AreEqual (origHtml1, renderedHtml, "Render-Empty-T-T");

			renderedHtml = dli.Render (true, false);
			Assert.AreEqual (origHtml2, renderedHtml, "Render-Empty-T-F");

			renderedHtml = dli.Render (false, true);
			Assert.AreEqual (Adjust (origHtml3), Adjust (renderedHtml), "Render-Empty-F-T");

			renderedHtml = dli.Render (false, false);
			Assert.AreEqual (Adjust (origHtml4), Adjust (renderedHtml), "Render-Empty-F-F");
		}

		[Test]
		public void Controls_Table_Dual ()
		{
#if NET_4_0
			string origHtml1 = "<tr>\n\t<td>mono</td>\n</tr>";
			string origHtml2 = "<tr>\n\t<td>mono</td>\n</tr>";
			string origHtml3 = "<table>\n\t<tr>\n\t\t<td>mono</td>\n\t</tr>\n</table><table>\n\t<tr>\n\t\t<td>monkey</td>\n\t</tr>\n</table>";
			string origHtml4 = "<span><table>\n\t<tr>\n\t\t<td>mono</td>\n\t</tr>\n</table><table>\n\t<tr>\n\t\t<td>monkey</td>\n\t</tr>\n</table></span>";
#else
			string origHtml1 = "<tr>\n\t<td>mono</td>\n</tr>";
			string origHtml2 = "<tr>\n\t<td>mono</td>\n</tr>";
			string origHtml3 = "<table border=\"0\">\n\t<tr>\n\t\t<td>mono</td>\n\t</tr>\n</table><table border=\"0\">\n\t<tr>\n\t\t<td>monkey</td>\n\t</tr>\n</table>";
			string origHtml4 = "<span><table border=\"0\">\n\t<tr>\n\t\t<td>mono</td>\n\t</tr>\n</table><table border=\"0\">\n\t<tr>\n\t\t<td>monkey</td>\n\t</tr>\n</table></span>";
#endif
			TestDataListItem dli = new TestDataListItem (0, ListItemType.Item);
			dli.Controls.Add (GetTable ("mono"));
			dli.Controls.Add (GetTable ("monkey"));

			// the second table is ignored if extractRows is true
			string renderedHtml = dli.Render (true, true);
			Assert.AreEqual (origHtml1, renderedHtml, "Render-Empty-T-T");

			renderedHtml = dli.Render (true, false);
			Assert.AreEqual (origHtml2, renderedHtml, "Render-Empty-T-F");

			// but not if extractRows is false
			renderedHtml = dli.Render (false, true);
			Assert.AreEqual (Adjust (origHtml3), Adjust (renderedHtml), "Render-Empty-F-T");

			renderedHtml = dli.Render (false, false);
			Assert.AreEqual (Adjust (origHtml4), Adjust (renderedHtml), "Render-Empty-F-F");
		}

		[Test]
		public void Controls_LiteralControl ()
		{
			TestDataListItem dli = new TestDataListItem (0, ListItemType.Item);
			LiteralControl lc = new LiteralControl ("mono");
			dli.Controls.Add (lc);

			// there's no table here (but there are controls), so calling Render with true for 
			// extractRows cause a NullReferenceException on MS implementation
			//Assert.AreEqual ("<tr>\n\t<td></td>\n<\tr>", dli.Render (true, true), "Render-Empty-T-T");
			//Assert.AreEqual ("<tr>\n\t<td></td>\n<\tr>", dli.Render (true, false), "Render-Empty-T-F");
			Assert.AreEqual ("mono", dli.Render (false, true), "Render-Empty-F-T");
			Assert.AreEqual ("<span>mono</span>", dli.Render (false, false), "Render-Empty-F-F");
		}
	}
}
