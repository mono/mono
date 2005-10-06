//
// DataListTest.cs - Unit tests for System.Web.UI.WebControls.DataList
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
using System.Data;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestDataList : DataList {

		public string Tag {
			get { return base.TagName; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}


		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		public string Render ()
		{
			HtmlTextWriter writer = GetWriter ();
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public string RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo)
		{
			HtmlTextWriter writer = GetWriter ();
			(this as IRepeatInfoUser).RenderItem (itemType, repeatIndex, repeatInfo, writer);
			return writer.InnerWriter.ToString ();
		}

		public void TrackState ()
		{
			TrackViewState ();
		}

		public void LoadState (object state)
		{
			LoadViewState (state);
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}
#if NET_2_0
		public void LoadControl (object state)
		{
			LoadControlState (state);
		}

		public object SaveControl ()
		{
			return SaveControlState ();
		}
#endif
		public void DoCancelCommand (DataListCommandEventArgs e)
		{
			OnCancelCommand (e);
		}

		public void DoDeleteCommand (DataListCommandEventArgs e)
		{
			OnDeleteCommand (e);
		}

		public void DoEditCommand (DataListCommandEventArgs e)
		{
			OnEditCommand (e);
		}

		public void DoItemCommand (DataListCommandEventArgs e)
		{
			OnItemCommand (e);
		}

		public void DoUpdateCommand (DataListCommandEventArgs e)
		{
			OnUpdateCommand (e);
		}

		public void DoItemCreated (DataListItemEventArgs e)
		{
			OnItemCreated (e);
		}

		public void DoItemDataBound (DataListItemEventArgs e)
		{
			OnItemDataBound (e);
		}

		public void DoSelectedIndexChanged (EventArgs e)
		{
			OnSelectedIndexChanged (e);
		}

		public void DoBubbleEvent (object source, EventArgs e)
		{
			OnBubbleEvent (source, e);
		}
	}

	public class TestTemplate : ITemplate {

		public void InstantiateIn (Control container)
		{
		}
	}

	[TestFixture]
	public class DataListTest {

		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		// IList based
		private ArrayList GetData (int n)
		{
			ArrayList al = new ArrayList ();
			for (int i = 0; i < n; i++) {
				al.Add (i.ToString ());
			}
			return al;
		}

		// IListSource based
		private TestDataSource GetDataSource (int n)
		{
			return new TestDataSource (GetData (n));
		}

		public void CheckIRepeatInfoUser (IRepeatInfoUser riu)
		{
			Assert.IsFalse (riu.HasFooter, "HasFooter");
			Assert.IsFalse (riu.HasHeader, "HasHeader");
			Assert.IsFalse (riu.HasSeparators, "HasSeparators");
			Assert.AreEqual (0, riu.RepeatedItemCount, "RepeatedItemCount");
		}
		
		[Test]
		public void ConstantStrings ()
		{
			Assert.AreEqual ("Cancel", DataList.CancelCommandName, "CancelCommandName");
			Assert.AreEqual ("Delete", DataList.DeleteCommandName, "DeleteCommandName");
			Assert.AreEqual ("Edit", DataList.EditCommandName, "EditCommandName");
			Assert.AreEqual ("Select", DataList.SelectCommandName, "SelectCommandName");
			Assert.AreEqual ("Update", DataList.UpdateCommandName, "UpdateCommandName");
		}

		[Test]
		public void DefaultProperties ()
		{
			TestDataList dl = new TestDataList ();
			CheckIRepeatInfoUser (dl);
#if NET_2_0
			Assert.AreEqual ("table", dl.Tag, "TagName");
#else
			Assert.AreEqual ("span", dl.Tag, "TagName");
#endif
			Assert.AreEqual (0, dl.Attributes.Count, "Attributes.Count-1");
			Assert.AreEqual (0, dl.StateBag.Count, "ViewState.Count-1");

			// Styles
			Assert.IsNotNull (dl.AlternatingItemStyle, "AlternatingItemStyle");
			Assert.IsNotNull (dl.EditItemStyle, "EditItemStyle");
			Assert.IsNotNull (dl.FooterStyle, "FooterStyle");
			Assert.IsNotNull (dl.HeaderStyle, "HeaderStyle");
			Assert.IsNotNull (dl.ItemStyle, "ItemStyle");
			Assert.IsNotNull (dl.SelectedItemStyle, "SelectedItemStyle");
			Assert.IsNotNull (dl.SeparatorStyle, "SeparatorStyle");

			// Templates
			Assert.IsNull (dl.AlternatingItemTemplate, "AlternatingItemTemplate");
			Assert.IsNull (dl.EditItemTemplate, "EditItemTemplate");
			Assert.IsNull (dl.FooterTemplate, "FooterTemplate");
			Assert.IsNull (dl.HeaderTemplate, "HeaderTemplate");
			Assert.IsNull (dl.ItemTemplate, "ItemTemplate");
			Assert.IsNull (dl.SelectedItemTemplate, "SelectedItemTemplate");
			Assert.IsNull (dl.SeparatorTemplate, "SeparatorTemplate");

			// Indexes
			Assert.AreEqual (-1, dl.EditItemIndex, "EditItemIndex");
			Assert.AreEqual (-1, dl.SelectedIndex, "SelectedIndex");

			// others (from DataList)
			Assert.IsFalse (dl.ExtractTemplateRows, "ExtractTemplateRows");
			Assert.AreEqual (0, dl.Items.Count, "Items.Count");
			Assert.AreEqual (0, dl.RepeatColumns, "RepeatColumns");
			Assert.AreEqual (RepeatDirection.Vertical, dl.RepeatDirection, "RepeatDirection");
			Assert.AreEqual (RepeatLayout.Table, dl.RepeatLayout, "RepeatLayout");
			Assert.IsNull (dl.SelectedItem, "SelectedItem");
			Assert.IsTrue (dl.ShowFooter, "ShowFooter");
			Assert.IsTrue (dl.ShowHeader, "ShowHeader");

			// the CellPadding, CellSpacing, GridLines and HorizontalAlign are defined
			// in BaseDataList but couldn't be totally tested from there
			Assert.AreEqual (-1, dl.CellPadding, "CellPadding");
			Assert.AreEqual (0, dl.CellSpacing, "CellSpacing");
#if NET_2_0
			Assert.AreEqual (GridLines.None, dl.GridLines, "GridLines");
#else			
			Assert.AreEqual (GridLines.Both, dl.GridLines, "GridLines");
#endif
			Assert.AreEqual (HorizontalAlign.NotSet, dl.HorizontalAlign, "HorizontalAlign");

			Assert.AreEqual (0, dl.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, dl.StateBag.Count, "ViewState.Count-2");
#if NET_2_0
			Assert.IsNull (dl.SaveControl (), "SaveControl");
#endif
		}

		[Test]
		public void NullProperties ()
		{
			TestDataList dl = new TestDataList ();
			Assert.AreEqual (0, dl.Attributes.Count, "Attributes.Count-1");
			Assert.AreEqual (0, dl.StateBag.Count, "ViewState.Count-1");

			// some properties couldn't be set in BaseDataList without causing a
			// InvalidCastException, so they get test here first...
			dl.CellPadding = -1;
			Assert.AreEqual (-1, dl.CellPadding, "CellPadding");
			dl.CellSpacing = 0;
			Assert.AreEqual (0, dl.CellSpacing, "CellSpacing");
			dl.GridLines = GridLines.None;
			Assert.AreEqual (GridLines.None, dl.GridLines, "GridLines");
			dl.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, dl.HorizontalAlign, "HorizontalAlign");
#if NET_2_0
			int sc = 0;
			// so the TableStyle isn't kept directly in the ViewState
#else
			int sc = 4;
#endif
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-2");

			// now for the DataList properties
			// touching all styles
			Assert.IsTrue (dl.AlternatingItemStyle.BackColor.IsEmpty, "AlternatingItemStyle");
			Assert.IsTrue (dl.EditItemStyle.BackColor.IsEmpty, "EditItemStyle");
			Assert.IsTrue (dl.FooterStyle.BackColor.IsEmpty, "FooterStyle");
			Assert.IsTrue (dl.HeaderStyle.BackColor.IsEmpty, "HeaderStyle");
			Assert.IsTrue (dl.ItemStyle.BackColor.IsEmpty, "ItemStyle");
			Assert.IsTrue (dl.SelectedItemStyle.BackColor.IsEmpty, "SelectedItemStyle");
			Assert.IsTrue (dl.SeparatorStyle.BackColor.IsEmpty, "SeparatorStyle");
			dl.AlternatingItemTemplate = null;
			Assert.IsNull (dl.AlternatingItemTemplate, "AlternatingItemTemplate");
			dl.EditItemTemplate = null;
			Assert.IsNull (dl.EditItemTemplate, "EditItemTemplate");
			dl.FooterTemplate = null;
			Assert.IsNull (dl.FooterTemplate, "FooterTemplate");
			dl.HeaderTemplate = null;
			Assert.IsNull (dl.HeaderTemplate, "HeaderTemplate");
			dl.ItemTemplate = null;
			Assert.IsNull (dl.ItemTemplate, "ItemTemplate");
			dl.SelectedItemTemplate = null;
			Assert.IsNull (dl.SelectedItemTemplate, "SelectedItemTemplate");
			dl.SeparatorTemplate = null;
			Assert.IsNull (dl.SeparatorTemplate, "SeparatorTemplate");
			dl.EditItemIndex = -1;
			Assert.AreEqual (-1, dl.EditItemIndex, "EditItemIndex");
#if ONLY_1_1
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-2b");
#endif
			dl.SelectedIndex = -1;
			Assert.AreEqual (-1, dl.SelectedIndex, "SelectedIndex");
#if ONLY_1_1
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-2c");
#endif
			dl.ExtractTemplateRows = false;
			Assert.IsFalse (dl.ExtractTemplateRows, "ExtractTemplateRows");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-3");
			dl.RepeatColumns = 0;
			Assert.AreEqual (0, dl.RepeatColumns, "RepeatColumns");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-4");
			dl.RepeatDirection = RepeatDirection.Vertical;
			Assert.AreEqual (RepeatDirection.Vertical, dl.RepeatDirection, "RepeatDirection");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-5");
			dl.RepeatLayout = RepeatLayout.Table;
			Assert.AreEqual (RepeatLayout.Table, dl.RepeatLayout, "RepeatLayout");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-6");
			dl.ShowFooter = true;
			Assert.IsTrue (dl.ShowFooter, "ShowFooter");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-7");
			dl.ShowHeader = true;
			Assert.IsTrue (dl.ShowHeader, "ShowHeader");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-8");

			// and all this didn't affect IRepeatInfoUser
			CheckIRepeatInfoUser (dl);

			Assert.AreEqual (0, dl.Attributes.Count, "Attributes.Count-2");
#if NET_2_0
			Assert.IsNull (dl.SaveControl (), "SaveControl");
#endif
		}

		[Test]
		public void CleanProperties ()
		{
			TestDataList dl = new TestDataList ();
			IRepeatInfoUser riu = (dl as IRepeatInfoUser);
			Assert.AreEqual (0, dl.Attributes.Count, "Attributes.Count-1");
			Assert.AreEqual (0, dl.StateBag.Count, "ViewState.Count-1");

			dl.CellPadding = 0;
			Assert.AreEqual (0, dl.CellPadding, "CellPadding");
			dl.CellSpacing = 1;
			Assert.AreEqual (1, dl.CellSpacing, "CellSpacing");
			dl.GridLines = GridLines.Vertical;
			Assert.AreEqual (GridLines.Vertical, dl.GridLines, "GridLines");
			dl.HorizontalAlign = HorizontalAlign.Center;
			Assert.AreEqual (HorizontalAlign.Center, dl.HorizontalAlign, "HorizontalAlign");
#if NET_2_0
			int sc = 0;
			// so the TableStyle isn't kept directly in the ViewState
#else
			int sc = 4;
#endif
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-2");

			// now for the DataList properties
			// touching all styles
			dl.AlternatingItemStyle.BackColor = Color.AliceBlue;
			Assert.IsFalse (dl.AlternatingItemStyle.BackColor.IsEmpty, "AlternatingItemStyle");
			dl.EditItemStyle.BackColor = Color.AntiqueWhite;
			Assert.IsFalse (dl.EditItemStyle.BackColor.IsEmpty, "EditItemStyle");
			dl.FooterStyle.BackColor = Color.Aqua;
			Assert.IsFalse (dl.FooterStyle.BackColor.IsEmpty, "FooterStyle");
			dl.HeaderStyle.BackColor = Color.Aquamarine;
			Assert.IsFalse (dl.HeaderStyle.BackColor.IsEmpty, "HeaderStyle");
			dl.ItemStyle.BackColor = Color.Azure;
			Assert.IsFalse (dl.ItemStyle.BackColor.IsEmpty, "ItemStyle");
			dl.SelectedItemStyle.BackColor = Color.Beige;
			Assert.IsFalse (dl.SelectedItemStyle.BackColor.IsEmpty, "SelectedItemStyle");
			dl.SeparatorStyle.BackColor = Color.Bisque;
			Assert.IsFalse (dl.SeparatorStyle.BackColor.IsEmpty, "SeparatorStyle");
			dl.AlternatingItemTemplate = new TestTemplate ();
			Assert.IsNotNull (dl.AlternatingItemTemplate, "AlternatingItemTemplate");
			dl.EditItemTemplate = new TestTemplate ();
			Assert.IsNotNull (dl.EditItemTemplate, "EditItemTemplate");
			dl.FooterTemplate = new TestTemplate ();
			Assert.IsTrue (riu.HasFooter, "HasFooter");
			Assert.IsNotNull (dl.FooterTemplate, "FooterTemplate");
			dl.HeaderTemplate = new TestTemplate ();
			Assert.IsTrue (riu.HasHeader, "HasHeader");
			Assert.IsNotNull (dl.HeaderTemplate, "HeaderTemplate");
			dl.ItemTemplate = new TestTemplate ();
			Assert.IsNotNull (dl.ItemTemplate, "ItemTemplate");
			dl.SelectedItemTemplate = new TestTemplate ();
			Assert.IsNotNull (dl.SelectedItemTemplate, "SelectedItemTemplate");
			dl.SeparatorTemplate = new TestTemplate ();
			Assert.IsTrue (riu.HasSeparators, "HasSeparators");
			Assert.IsNotNull (dl.SeparatorTemplate, "SeparatorTemplate");
			dl.EditItemIndex = 0;
			Assert.AreEqual (0, dl.EditItemIndex, "EditItemIndex");
#if NET_2_0
			Assert.IsNotNull (dl.SaveControl (), "SaveControl-EditItemIndex");
			dl.EditItemIndex = -1;
#else
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-2b");
#endif
			dl.SelectedIndex = 0;
			Assert.AreEqual (0, dl.SelectedIndex, "SelectedIndex");
#if NET_2_0
			Assert.IsNotNull (dl.SaveControl (), "SaveControl-SelectedIndex");
			dl.SelectedIndex = -1;
#else
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-2c");
#endif
			dl.ExtractTemplateRows = true;
			Assert.IsTrue (dl.ExtractTemplateRows, "ExtractTemplateRows");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-3");
			dl.RepeatColumns = 1;
			Assert.AreEqual (1, dl.RepeatColumns, "RepeatColumns");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-4");
			dl.RepeatDirection = RepeatDirection.Horizontal;
			Assert.AreEqual (RepeatDirection.Horizontal, dl.RepeatDirection, "RepeatDirection");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-5");
			dl.RepeatLayout = RepeatLayout.Flow;
			Assert.AreEqual (RepeatLayout.Flow, dl.RepeatLayout, "RepeatLayout");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-6");
			dl.ShowFooter = false;
			Assert.IsFalse (riu.HasFooter, "HasFooter(lost)");
			Assert.IsFalse (dl.ShowFooter, "ShowFooter");
			Assert.AreEqual (sc++, dl.StateBag.Count, "ViewState.Count-7");
			dl.ShowHeader = false;
			Assert.IsFalse (riu.HasHeader, "HasHeader(lost)");
			Assert.IsFalse (dl.ShowHeader, "ShowHeader");
			Assert.AreEqual (sc, dl.StateBag.Count, "ViewState.Count-8");

			// reverting back changes to default...

			dl.CellPadding = -1;
			Assert.AreEqual (-1, dl.CellPadding, "-CellPadding");
			dl.CellSpacing = 0;
			Assert.AreEqual (0, dl.CellSpacing, "-CellSpacing");
			dl.GridLines = GridLines.None;
			Assert.AreEqual (GridLines.None, dl.GridLines, "-GridLines");
			dl.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, dl.HorizontalAlign, "-HorizontalAlign");
			Assert.AreEqual (sc, dl.StateBag.Count, "ViewState.Count-9");
			dl.AlternatingItemStyle.Reset ();
			Assert.IsTrue (dl.AlternatingItemStyle.BackColor.IsEmpty, "-AlternatingItemStyle");
			dl.EditItemStyle.Reset ();
			Assert.IsTrue (dl.EditItemStyle.BackColor.IsEmpty, "-EditItemStyle");
			dl.FooterStyle.Reset ();
			Assert.IsTrue (dl.FooterStyle.BackColor.IsEmpty, "-FooterStyle");
			dl.HeaderStyle.Reset ();
			Assert.IsTrue (dl.HeaderStyle.BackColor.IsEmpty, "-HeaderStyle");
			dl.ItemStyle.Reset ();
			Assert.IsTrue (dl.ItemStyle.BackColor.IsEmpty, "-ItemStyle");
			dl.SelectedItemStyle.Reset ();
			Assert.IsTrue (dl.SelectedItemStyle.BackColor.IsEmpty, "-SelectedItemStyle");
			dl.SeparatorStyle.Reset ();
			Assert.IsTrue (dl.SeparatorStyle.BackColor.IsEmpty, "-SeparatorStyle");
			dl.AlternatingItemTemplate = null;
			Assert.IsNull (dl.AlternatingItemTemplate, "-AlternatingItemTemplate");
			dl.EditItemTemplate = null;
			Assert.IsNull (dl.EditItemTemplate, "-EditItemTemplate");
			dl.FooterTemplate = null;
			Assert.IsNull (dl.FooterTemplate, "-FooterTemplate");
			dl.HeaderTemplate = null;
			Assert.IsNull (dl.HeaderTemplate, "-HeaderTemplate");
			dl.ItemTemplate = null;
			Assert.IsNull (dl.ItemTemplate, "-ItemTemplate");
			dl.SelectedItemTemplate = null;
			Assert.IsNull (dl.SelectedItemTemplate, "-SelectedItemTemplate");
			dl.SeparatorTemplate = null;
			Assert.IsNull (dl.SeparatorTemplate, "-SeparatorTemplate");
			dl.EditItemIndex = -1;
			Assert.AreEqual (-1, dl.EditItemIndex, "-EditItemIndex");
			dl.SelectedIndex = -1;
			Assert.AreEqual (-1, dl.SelectedIndex, "-SelectedIndex");
			dl.ExtractTemplateRows = false;
			Assert.IsFalse (dl.ExtractTemplateRows, "-ExtractTemplateRows");
			dl.RepeatColumns = 0;
			Assert.AreEqual (0, dl.RepeatColumns, "-RepeatColumns");
			dl.RepeatDirection = RepeatDirection.Vertical;
			Assert.AreEqual (RepeatDirection.Vertical, dl.RepeatDirection, "-RepeatDirection");
			dl.RepeatLayout = RepeatLayout.Table;
			Assert.AreEqual (RepeatLayout.Table, dl.RepeatLayout, "-RepeatLayout");
			dl.ShowFooter = true;
			Assert.IsTrue (dl.ShowFooter, "-ShowFooter");
			dl.ShowHeader = true;
			Assert.IsTrue (dl.ShowHeader, "-ShowHeader");
			Assert.AreEqual (sc, dl.StateBag.Count, "ViewState.Count-10");

			// and all this didn't affect IRepeatInfoUser
			CheckIRepeatInfoUser (dl);

			Assert.AreEqual (0, dl.Attributes.Count, "Attributes.Count-2");
#if NET_2_0
			Assert.IsNull (dl.SaveControl (), "SaveControl");
#endif
		}

		[Test]
		public void RepeatedItemCount ()
		{
			TestDataList dl = new TestDataList ();
			IRepeatInfoUser riu = (dl as IRepeatInfoUser);
			dl.DataSource = GetData (10);
			Assert.AreEqual (0, riu.RepeatedItemCount, "before Bind");
			dl.DataBind ();
			Assert.AreEqual (10, riu.RepeatedItemCount, "after Bind");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetItemStyle_Header_Empty ()
		{
			TestDataList dl = new TestDataList ();
			IRepeatInfoUser riu = (dl as IRepeatInfoUser);
			// empty list/controls
			riu.GetItemStyle (ListItemType.Header, 0);
		}

		[Test]
		public void GetItemStyle_Header ()
		{
			TestDataList dl = new TestDataList ();
			dl.DataSource = GetData (6);
			dl.DataBind ();
			IRepeatInfoUser riu = (dl as IRepeatInfoUser);
			Assert.IsNull (riu.GetItemStyle (ListItemType.Header, 0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetItemStyle_Separator_Empty ()
		{
			TestDataList dl = new TestDataList ();
			IRepeatInfoUser riu = (dl as IRepeatInfoUser);
			// empty list/controls
			riu.GetItemStyle (ListItemType.Separator, 0);
		}

		[Test]
		public void GetItemStyle_Separator ()
		{
			TestDataList dl = new TestDataList ();
			dl.DataSource = GetData (6);
			dl.DataBind ();
			IRepeatInfoUser riu = (dl as IRepeatInfoUser);
			Assert.IsNull (riu.GetItemStyle (ListItemType.Separator, 0));
		}

		[Test]
		public void GetItemStyle_Pager_Empty ()
		{
			TestDataList dl = new TestDataList ();
			IRepeatInfoUser riu = (dl as IRepeatInfoUser);
			// Pager isn't supported in DataList
			Assert.IsNull (riu.GetItemStyle (ListItemType.Pager, 0), "Pager-0");
		}

		[Test]
		public void Controls ()
		{
			TestDataList dl = new TestDataList ();
			Assert.AreEqual (0, dl.Controls.Count, "Controls-1");
			Assert.AreEqual (0, dl.Items.Count, "Items-1");
			dl.DataSource = GetDataSource (3);
			Assert.AreEqual (0, dl.Controls.Count, "Controls-2");
			Assert.AreEqual (0, dl.Items.Count, "Items-2");
			dl.DataBind ();
			Assert.AreEqual (0, dl.Controls.Count, "Controls-3");
			Assert.AreEqual (0, dl.Items.Count, "Items-3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void EditItemIndexTooLow ()
		{
			TestDataList dl = new TestDataList ();
			dl.EditItemIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexTooLow ()
		{
			TestDataList dl = new TestDataList ();
			dl.SelectedIndex = -2;
		}

		[Test]
		public void SelectIndexOutOfRange ()
		{
			TestDataList dl = new TestDataList ();
			// No exception is thrown
			dl.SelectedIndex = 25;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectItemOutOfRange ()
		{
			TestDataList dl = new TestDataList ();
			dl.SelectedIndex = 25;
			DataListItem dli = dl.SelectedItem;
		}

		[Test]
		public void SaveViewState ()
		{
			TestDataList dl = new TestDataList ();
			dl.TrackState ();

			object[] vs = (object[]) dl.SaveState ();
#if NET_2_0
			Assert.AreEqual (9, vs.Length, "Size");
#else
			Assert.AreEqual (8, vs.Length, "Size");
#endif
			// By default the viewstate is all null
			int i = 0;
			for (; i < vs.Length; i++)
				Assert.IsNull (vs [i], "Empty-" + i);

			i = 0;
#if NET_2_0
			i++;
#else
			dl.GridLines = GridLines.Vertical;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "GridLines");
#endif
			dl.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "ItemStyle");

			dl.SelectedItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "SelectedItemStyle");

			dl.AlternatingItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "AlternatingItemStyle");

			dl.EditItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "EditItemStyle");

			dl.SeparatorStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "SeparatorStyle");

			dl.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "HeaderStyle");

			dl.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "FooterStyle");
#if NET_2_0
			// GridLines was moved last
			dl.GridLines = GridLines.Vertical;
			vs = (object []) dl.SaveState ();
			Assert.IsNotNull (vs[i++], "GridLines");
#endif
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SelectedValue_WithoutDataKeyField ()
		{
			TestDataList dl = new TestDataList ();
			Assert.IsNull (dl.SelectedValue, "SelectedValue");
		}

		[Test]
		public void SelectedValue_WithUnexistingDataKeyField ()
		{
			TestDataList dl = new TestDataList ();
			dl.DataKeyField = "mono";
			Assert.IsNull (dl.SelectedValue, "SelectedValue");
		}

		[Test]
		public void LoadControlState ()
		{
			TestDataList dl = new TestDataList ();
			dl.LoadControl (null);
			Assert.AreEqual (-1, dl.EditItemIndex, "null-EditItemIndex");
			Assert.AreEqual (-1, dl.SelectedIndex, "null-SelectedIndex");
			dl.LoadControl (new Triplet (1, 2, 3));
			Assert.AreEqual (2, dl.EditItemIndex, "Triplet-EditItemIndex");
			Assert.AreEqual (3, dl.SelectedIndex, "Triplet-SelectedIndex");
			dl.LoadControl (null);
			Assert.AreEqual (-1, dl.EditItemIndex, "null-EditItemIndex-2");
			Assert.AreEqual (-1, dl.SelectedIndex, "null-SelectedIndex-2");
		}

		[Test]
		public void LoadControlState_FirstTripletObject ()
		{
			TestDataList dl = new TestDataList ();
			// first item seems ignored
			dl.LoadControl (new Triplet (this, 2, 3));
			Assert.AreEqual (2, dl.EditItemIndex, "Triplet-EditItemIndex");
			Assert.AreEqual (3, dl.SelectedIndex, "Triplet-SelectedIndex");

			dl.LoadControl (new Triplet (dl, null, 3));
			Assert.AreEqual (-1, dl.EditItemIndex, "Triplet-EditItemIndex-2");
			Assert.AreEqual (3, dl.SelectedIndex, "Triplet-SelectedIndex-2");

			dl.LoadControl (new Triplet (null, 2, null));
			Assert.AreEqual (2, dl.EditItemIndex, "Triplet-EditItemIndex-3");
			Assert.AreEqual (-1, dl.SelectedIndex, "Triplet-SelectedIndex-3");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void LoadControlState_SecondTripletObject ()
		{
			TestDataList dl = new TestDataList ();
			dl.LoadControl (new Triplet (1, this, 3));
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void LoadControlState_ThirdTripletObject ()
		{
			TestDataList dl = new TestDataList ();
			dl.LoadControl (new Triplet (1, 2, this));
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void LoadControlState_BadType ()
		{
			TestDataList dl = new TestDataList ();
			dl.LoadControl (this);
		}

		[Test]
		public void SaveControlState ()
		{
			TestDataList dl = new TestDataList ();
			Assert.IsNull (dl.SaveControl (), "Empty");
			dl.EditItemIndex = 0;
			Triplet state = (Triplet) dl.SaveControl ();
			Assert.IsNull (state.First, "EditItemIndex-First");
			Assert.AreEqual (0, (int)state.Second, "EditItemIndex-Second");
			Assert.IsNull (state.Third, "EditItemIndex-Third");

			dl.SelectedIndex = 1;
			state = (Triplet) dl.SaveControl ();
			Assert.IsNull (state.First, "SelectedIndex-First");
			Assert.AreEqual (0, (int)state.Second, "SelectedIndex-Second");
			Assert.AreEqual (1, (int)state.Third, "SelectedIndex-Third");

			dl.EditItemIndex = -1;
			state = (Triplet) dl.SaveControl ();
			dl.SelectedIndex = -1;
			Assert.IsNull (dl.SaveControl (), "Cleaned");
		}
#endif
		private bool cancelCommandEvent;
		private bool deleteCommandEvent;
		private bool editCommandEvent;
		private bool itemCommandEvent;
		private bool itemCreatedEvent;
		private bool itemDataBoundEvent;
		private bool updateCommandEvent;
		private bool selectedIndexChangedEvent;

		private void ResetEvents ()
		{
			cancelCommandEvent = false;
			deleteCommandEvent = false;
			editCommandEvent = false;
			itemCommandEvent = false;
			itemCreatedEvent = false;
			itemDataBoundEvent = false;
			updateCommandEvent = false;
			selectedIndexChangedEvent = false;
		}
				
		private void CancelCommandHandler (object sender, DataListCommandEventArgs e)
		{
			cancelCommandEvent = true;
		}

		private void DeleteCommandHandler (object sender, DataListCommandEventArgs e)
		{
			deleteCommandEvent = true;
		}
		
		private void EditCommandHandler (object sender, DataListCommandEventArgs e)
		{
			editCommandEvent = true;
		}

		private void ItemCommandHandler (object sender, DataListCommandEventArgs e)
		{
			itemCommandEvent = true;
		}

		private void ItemCreatedHandler (object sender, DataListItemEventArgs e)
		{
			itemCreatedEvent = true;
		}

		private void ItemDataBoundHandler (object sender, DataListItemEventArgs e)
		{
			itemDataBoundEvent = true;
		}

		private void SelectedIndexChangedHandler (object sender, EventArgs e)
		{
			selectedIndexChangedEvent = true;
		}
		
		private void UpdateCommandHandler (object sender, DataListCommandEventArgs e)
		{
			updateCommandEvent = true;
		}

		[Test]
		public void Events ()
		{
			TestDataList dl = new TestDataList ();
			DataListCommandEventArgs command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs (String.Empty, String.Empty));
			DataListItemEventArgs item_args = new DataListItemEventArgs (null);

			ResetEvents ();
			dl.CancelCommand += new DataListCommandEventHandler (CancelCommandHandler);
			dl.DoCancelCommand (command_args);
			Assert.IsTrue (cancelCommandEvent, "cancelCommandEvent");

			ResetEvents ();
			dl.DeleteCommand += new DataListCommandEventHandler (DeleteCommandHandler);
			dl.DoDeleteCommand (command_args);
			Assert.IsTrue (deleteCommandEvent, "deleteCommandEvent");

			ResetEvents ();
			dl.EditCommand += new DataListCommandEventHandler (EditCommandHandler);
			dl.DoEditCommand (command_args);
			Assert.IsTrue (editCommandEvent, "editCommandEvent");

			ResetEvents ();
			dl.ItemCommand += new DataListCommandEventHandler (ItemCommandHandler);
			dl.DoItemCommand (command_args);
			Assert.IsTrue (itemCommandEvent, "itemCommandEvent");

			ResetEvents ();
			dl.ItemCreated += new DataListItemEventHandler (ItemCreatedHandler);
			dl.DoItemCreated (item_args);
			Assert.IsTrue (itemCreatedEvent, "itemCreatedEvent");

			ResetEvents ();
			dl.ItemDataBound += new DataListItemEventHandler (ItemDataBoundHandler);
			dl.DoItemDataBound (item_args);
			Assert.IsTrue (itemDataBoundEvent, "itemDataBoundEvent");

			ResetEvents ();
			dl.UpdateCommand += new DataListCommandEventHandler (UpdateCommandHandler);
			dl.DoUpdateCommand (command_args);
			Assert.IsTrue (updateCommandEvent, "updateCommandEvent");

			ResetEvents ();
			dl.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			dl.DoSelectedIndexChanged (new EventArgs ());
			Assert.IsTrue (selectedIndexChangedEvent, "selectedIndexChangedEvent");
		}

		[Test]
		public void BubbleEvent ()
		{
			TestDataList dl = new TestDataList ();
			DataListCommandEventArgs command_args;

			//
			// Cancel
			//
			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("Cancel", String.Empty));
			dl.CancelCommand += new DataListCommandEventHandler (CancelCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancelCommandEvent, "cancelCommandEvent-1");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("cancel", String.Empty));
			dl.CancelCommand += new DataListCommandEventHandler (CancelCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancelCommandEvent, "cancelCommandEvent-2");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("CANCEL", String.Empty));
			dl.CancelCommand += new DataListCommandEventHandler (CancelCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancelCommandEvent, "cancelCommandEvent-3");

			//
			// Delete
			//
			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("Delete", String.Empty));
			dl.DeleteCommand += new DataListCommandEventHandler (DeleteCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (deleteCommandEvent, "deleteCommandEvent-1");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("delete", String.Empty));
			dl.DeleteCommand += new DataListCommandEventHandler (DeleteCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (deleteCommandEvent, "deleteCommandEvent-2");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("DELETE", String.Empty));
			dl.DeleteCommand += new DataListCommandEventHandler (DeleteCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (deleteCommandEvent, "deleteCommandEvent-3");

			//
			// Edit
			//
			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("Edit", String.Empty));
			dl.EditCommand += new DataListCommandEventHandler (EditCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (editCommandEvent, "editCommandEvent-1");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("edit", String.Empty));
			dl.EditCommand += new DataListCommandEventHandler (EditCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (editCommandEvent, "editCommandEvent-2");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("EDIT", String.Empty));
			dl.EditCommand += new DataListCommandEventHandler (EditCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (editCommandEvent, "editCommandEvent-3");

			//
			// Item
			//
			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("Item", String.Empty));
			dl.ItemCommand += new DataListCommandEventHandler (ItemCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (itemCommandEvent, "itemCommandEvent-1");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("item", String.Empty));
			dl.ItemCommand += new DataListCommandEventHandler (ItemCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (itemCommandEvent, "itemCommandEvent-2");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("ITEM", String.Empty));
			dl.ItemCommand += new DataListCommandEventHandler (ItemCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (itemCommandEvent, "itemCommandEvent-3");

			//
			// Update
			//
			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("Update", String.Empty));
			dl.UpdateCommand += new DataListCommandEventHandler (UpdateCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (updateCommandEvent, "updateCommandEvent-1");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("update", String.Empty));
			dl.UpdateCommand += new DataListCommandEventHandler (UpdateCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (updateCommandEvent, "updateCommandEvent-2");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (null,
					null, new CommandEventArgs ("UPDATE", String.Empty));
			dl.UpdateCommand += new DataListCommandEventHandler (UpdateCommandHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (updateCommandEvent, "updateCommandEvent-3");

			//
			// Select
			//
			DataListItem item = new DataListItem (0, ListItemType.Item);
			
			ResetEvents ();
			command_args = new DataListCommandEventArgs (item, null,
					new CommandEventArgs ("Select", String.Empty));
			dl.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selectedIndexChangedEvent, "selectedIndexChangedEvent-1");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (item, null,
					new CommandEventArgs ("select", String.Empty));
			dl.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selectedIndexChangedEvent, "selectedIndexChangedEvent-2");

			ResetEvents ();
			command_args = new DataListCommandEventArgs (item, null,
					new CommandEventArgs ("SELECT", String.Empty));
			dl.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			dl.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selectedIndexChangedEvent, "selectedIndexChangedEvent-3");
		}
	}
}
