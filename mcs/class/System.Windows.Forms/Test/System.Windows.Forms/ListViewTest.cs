//
// ListViewTest.cs: Test cases for ListView.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListViewTest : TestHelper
	{
		[Test]
		public void ListViewPropertyTest ()
		{
			ListView mylistview = new ListView ();
			Assert.AreEqual (ItemActivation.Standard, mylistview.Activation, "#1");
			Assert.AreEqual (ListViewAlignment.Top, mylistview.Alignment, "#2");
			Assert.AreEqual (false, mylistview.AllowColumnReorder, "#3");
			Assert.AreEqual (true, mylistview.AutoArrange, "#4");
			Assert.AreEqual (BorderStyle.Fixed3D , mylistview.BorderStyle, "#5");
			Assert.AreEqual (false, mylistview.CheckBoxes, "#6");
			Assert.AreEqual (0, mylistview.CheckedIndices.Count, "#7");
			Assert.AreEqual (0, mylistview.CheckedItems.Count, "#8");
			Assert.AreEqual (0, mylistview.Columns.Count, "#9");
			Assert.AreEqual (null, mylistview.FocusedItem, "#10");
			Assert.AreEqual (false, mylistview.FullRowSelect, "#11");
			Assert.AreEqual (false, mylistview.GridLines, "#12");
			Assert.AreEqual (ColumnHeaderStyle.Clickable, mylistview.HeaderStyle, "#13");
			Assert.AreEqual (true, mylistview.HideSelection, "#14");
			Assert.AreEqual (false, mylistview.HoverSelection, "#15");
			ListViewItem item1 = new ListViewItem ("A", -1);
			mylistview.Items.Add (item1);
			Assert.AreEqual (1, mylistview.Items.Count, "#16");
			Assert.AreEqual (false, mylistview.LabelEdit, "#17");
			Assert.AreEqual (true, mylistview.LabelWrap, "#18");
			Assert.AreEqual (null, mylistview.LargeImageList, "#19");
			Assert.AreEqual (null, mylistview.ListViewItemSorter, "#20");
			Assert.AreEqual (true, mylistview.MultiSelect, "#21");
			Assert.AreEqual (true, mylistview.Scrollable, "#22");
			Assert.AreEqual (0, mylistview.SelectedIndices.Count, "#23");
			Assert.AreEqual (0, mylistview.SelectedItems.Count, "#24");
			Assert.AreEqual (null, mylistview.SmallImageList, "#25");
			Assert.AreEqual (null, mylistview.LargeImageList, "#26");
			Assert.AreEqual (SortOrder.None, mylistview.Sorting, "#27");
			Assert.AreEqual (null, mylistview.StateImageList, "#28");
			Assert.AreEqual (View.LargeIcon, mylistview.View, "#29");
			mylistview.View = View.List;
			Assert.AreEqual (false, mylistview.TopItem.Checked, "#30");
			Assert.AreEqual (false, mylistview.ShowItemToolTips, "#31");
			Assert.AreEqual (false, mylistview.HotTracking, "#31");
		}

		[Test]
		public void ArrangeIconsTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			ListView mylistview = new ListView ();
			myform.Controls.Add (mylistview);
			mylistview.Items.Add ("Item 1");
			mylistview.Items.Add ("Item 2");
			mylistview.View = View.LargeIcon;
			mylistview.ArrangeIcons ();
			myform.Dispose ();
		}

		// Hey
		[Test]
		public void BeginEndUpdateTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			ListView mylistview = new ListView();
			mylistview.Items.Add ("A");
			mylistview.Visible = true;
			myform.Controls.Add (mylistview);
			mylistview.BeginUpdate ();
			for(int x = 1 ; x < 5000 ; x++){
				mylistview.Items.Add ("Item " + x.ToString());   
			}
			mylistview.EndUpdate ();
			myform.Dispose ();
		}	

		[Test]
		public void CheckBoxes ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			lvw.Items.Add ("A");
			ListViewItem itemB = lvw.Items.Add ("B");
			lvw.Items.Add ("C");
			itemB.Checked = true;

			Assert.AreEqual (0, lvw.CheckedItems.Count, "#A1");
			Assert.AreEqual (0, lvw.CheckedIndices.Count, "#A2");

			lvw.CheckBoxes = true;

			Assert.AreEqual (1, lvw.CheckedItems.Count, "#B1");
			Assert.AreSame (itemB, lvw.CheckedItems [0], "#B2");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#B3");
			Assert.AreEqual (1, lvw.CheckedIndices [0], "#B4");

			form.Show ();

			Assert.AreEqual (1, lvw.CheckedItems.Count, "#C1");
			Assert.AreSame (itemB, lvw.CheckedItems [0], "#C2");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#C3");
			Assert.AreEqual (1, lvw.CheckedIndices [0], "#C4");

			lvw.CheckBoxes = false;

			Assert.AreEqual (0, lvw.CheckedItems.Count, "#D1");
			Assert.AreEqual (0, lvw.CheckedIndices.Count, "#D2");

			lvw.CheckBoxes = true;

			Assert.AreEqual (1, lvw.CheckedItems.Count, "#E1");
			Assert.AreSame (itemB, lvw.CheckedItems [0], "#E2");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#E3");
			Assert.AreEqual (1, lvw.CheckedIndices [0], "#E4");
			form.Dispose ();
		}

		[Test]
		public void ClearTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			ListView mylistview = new ListView ();
			ListViewItem itemA = mylistview.Items.Add ("A");
			ColumnHeader colA = mylistview.Columns.Add ("Item Column", -2, HorizontalAlignment.Left);
			Assert.AreSame (mylistview, itemA.ListView, "#1");
			Assert.AreSame (mylistview, colA.ListView, "#2");
			mylistview.Visible = true;
			myform.Controls.Add (mylistview);
			Assert.AreEqual (1, mylistview.Columns.Count, "#3");
			Assert.AreEqual (1, mylistview.Items.Count, "#4");
			mylistview.Clear ();
			Assert.AreEqual (0, mylistview.Columns.Count, "#5");
			Assert.AreEqual (0, mylistview.Items.Count, "#6");
			Assert.IsNull (itemA.ListView, "#7");
			Assert.IsNull (colA.ListView, "#8");
			myform.Dispose ();
		}

		[Test] // bug #80620
		public void ClientRectangle_Borders ()
		{
			ListView lv = new ListView ();
			lv.CreateControl ();
			Assert.AreEqual (lv.ClientRectangle, new ListView ().ClientRectangle);
		}

		[Test]
		public void DisposeTest ()
		{
			ListView lv = new ListView ();
			lv.View = View.Details;

			lv.LargeImageList = new ImageList ();
			lv.SmallImageList = new ImageList ();

			ListViewItem lvi = new ListViewItem ();
			lv.Items.Add (lvi);

			ColumnHeader col = new ColumnHeader ();
			lv.Columns.Add (col);

			lv.Dispose ();

			Assert.IsNull (lvi.ListView, "#A1");
			Assert.IsNull (col.ListView, "#A2");

			Assert.IsNull (lv.LargeImageList, "#B1");
			Assert.IsNull (lv.SmallImageList, "#B2");
			Assert.IsNull(lv.StateImageList, "#B3");
		}

		string dispose_log;

		[Test]
		public void DisposeLayoutTest ()
		{
			Form f = new Form ();
			ListView lv = new ListView ();
			f.Controls.Add (lv);
			f.Show ();

			dispose_log = String.Empty;
			lv.Layout += DisposeOnLayout;
			lv.Dispose (); // just to be sure.
			f.Dispose ();

			Assert.AreEqual (0, dispose_log.Length, "#A0");
		}

		void DisposeOnLayout (object o, LayoutEventArgs args)
		{
			dispose_log = "OnLayout";
		}

		// Hey
		//[Test]
		public void EnsureVisibleTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			ListView mylistview = new ListView ();
			mylistview.Items.Add ("A");
			myform.Controls.Add (mylistview);
			mylistview.BeginUpdate ();
			for(int x = 1 ; x < 5000 ; x++) {
				mylistview.Items.Add ("Item " + x.ToString());   
			}
			mylistview.EndUpdate ();
			mylistview.EnsureVisible (4999);
			myform.Dispose ();
		}

		[Test]
		public void GetItemRectTest ()
		{
			ListView mylistview = new ListView ();
			mylistview.Items.Add ("Item 1");
			mylistview.Items.Add ("Item 2");
			Rectangle r = mylistview.GetItemRect (1);
			Assert.AreEqual (0, r.Top, "#35a");
			Assert.IsTrue (r.Bottom > 0, "#35b");
			Assert.IsTrue (r.Right > 0, "#35c");
			Assert.IsTrue (r.Left > 0, "#35d");
			Assert.IsTrue (r.Height > 0, "#35e");
			Assert.IsTrue (r.Width > 0, "#35f");
		}

		[Test]
		public void bug79076 ()
		{
			ListView entryList = new ListView ();
			entryList.Sorting = SortOrder.Descending;

			entryList.BeginUpdate ();
			entryList.Columns.Add ("Type", 100, HorizontalAlignment.Left);

			ListViewItem item = new ListViewItem (new string [] { "A" });
			entryList.Items.Add (item);
			item = new ListViewItem (new string [] { "B" });
			entryList.Items.Add (item);
		}

		[Test] // bug #79416
		public void MultiSelect ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = CreateListView (View.Details);
			form.Controls.Add (lvw);
			lvw.MultiSelect = true;
			lvw.Items [0].Selected = true;
			lvw.Items [2].Selected = true;

			Assert.AreEqual (0, lvw.SelectedItems.Count, "#A1");
			Assert.AreEqual (0, lvw.SelectedIndices.Count, "#A2");

			lvw.Items [0].Selected = false;

			Assert.AreEqual (0, lvw.SelectedItems.Count, "#B1");
			Assert.AreEqual (0, lvw.SelectedIndices.Count, "#B2");

			lvw.Items [0].Selected = true;

			Assert.AreEqual (0, lvw.SelectedItems.Count, "#C1");
			Assert.AreEqual (0, lvw.SelectedIndices.Count, "#C2");

			form.Show ();

			Assert.AreEqual (2, lvw.SelectedItems.Count, "#D1");
			Assert.AreEqual ("B", lvw.SelectedItems [0].Text, "#D2");
			Assert.AreEqual ("C", lvw.SelectedItems [1].Text, "#D3");
			Assert.AreEqual (2, lvw.SelectedIndices.Count, "#D4");
			Assert.AreEqual (0, lvw.SelectedIndices [0], "#D5");
			Assert.AreEqual (2, lvw.SelectedIndices [1], "#D6");

			// de-select an item
			lvw.Items [2].Selected = false;

			Assert.AreEqual (1, lvw.SelectedItems.Count, "#E1");
			Assert.AreEqual ("B", lvw.SelectedItems [0].Text, "#E2");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#E3");
			Assert.AreEqual (0, lvw.SelectedIndices [0], "#E4");

			// re-select that item
			lvw.Items [2].Selected = true;

			Assert.AreEqual (2, lvw.SelectedItems.Count, "#F1");
			Assert.AreEqual ("B", lvw.SelectedItems [0].Text, "#F2");
			Assert.AreEqual ("C", lvw.SelectedItems [1].Text, "#F3");
			Assert.AreEqual (2, lvw.SelectedIndices.Count, "#F4");
			Assert.AreEqual (0, lvw.SelectedIndices [0], "#F5");
			Assert.AreEqual (2, lvw.SelectedIndices [1], "#F6");

			// dis-allow selection of multiple items
			lvw.MultiSelect = false;

			// setting MultiSelect to false when multiple items have been
			// selected does not deselect items
			Assert.AreEqual (2, lvw.SelectedItems.Count, "#G1");
			Assert.AreEqual ("B", lvw.SelectedItems [0].Text, "#G2");
			Assert.AreEqual ("C", lvw.SelectedItems [1].Text, "#G3");
			Assert.AreEqual (2, lvw.SelectedIndices.Count, "#G4");
			Assert.AreEqual (0, lvw.SelectedIndices [0], "#G5");
			Assert.AreEqual (2, lvw.SelectedIndices [1], "#G6");

			// de-select that item again
			lvw.Items [2].Selected = false;

			Assert.AreEqual (1, lvw.SelectedItems.Count, "#H1");
			Assert.AreEqual ("B", lvw.SelectedItems [0].Text, "#H2");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#H3");
			Assert.AreEqual (0, lvw.SelectedIndices [0], "#H4");

			// re-select that item again
			lvw.Items [2].Selected = true;

			// when MultiSelect is false, and you attempt to select more than
			// one item, then all items will first be de-selected and then
			// the item in question is selected
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#I1");
			Assert.AreEqual ("C", lvw.SelectedItems [0].Text, "#I2");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#I3");
			Assert.AreEqual (2, lvw.SelectedIndices [0], "#I4");
			form.Dispose ();
		}

		[Test]
		public void TopItem ()
		{
			ListView lvw = CreateListView (View.List);

			lvw.TopItem = null;
			Assert.AreEqual (lvw.Items [0], lvw.TopItem, "#A1");

			lvw.TopItem = new ListViewItem ();
			Assert.AreEqual (lvw.Items [0], lvw.TopItem, "#A2");
		}

		[Test]
		public void TopItem_Exceptions ()
		{
			ListView lvw = CreateListView (View.LargeIcon);
			ListViewItem item = null;

			try {
				lvw.TopItem = lvw.Items [2];
				Assert.Fail ("#A1");
			} catch (InvalidOperationException) {
			}

			try {
				item = lvw.TopItem;
				Assert.Fail ("#A2");
			} catch (InvalidOperationException) {
			}

			lvw.View = View.SmallIcon;
			try {
				lvw.TopItem = lvw.Items [2];
				Assert.Fail ("#A3");
			} catch (InvalidOperationException) {
			}

			try {
				item = lvw.TopItem;
				Assert.Fail ("#A4");
			} catch (InvalidOperationException) {
			}

			lvw.View = View.Tile;
			try {
				lvw.TopItem = lvw.Items [2];
				Assert.Fail ("#A5");
			} catch (InvalidOperationException) {
			}

			try {
				item = lvw.TopItem;
				Assert.Fail ("#A6");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void Selected ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = CreateListView (View.Details);
			form.Controls.Add (lvw);
			lvw.MultiSelect = true;
			lvw.Items [0].Selected = true;
			lvw.Items [2].Selected = true;

			Assert.AreEqual (0, lvw.SelectedItems.Count, "#A1");
			Assert.AreEqual (0, lvw.SelectedIndices.Count, "#A2");

			form.Show ();

			Assert.AreEqual (2, lvw.SelectedItems.Count, "#C1");
			Assert.AreEqual ("B", lvw.SelectedItems [0].Text, "#C2");
			Assert.AreEqual ("C", lvw.SelectedItems [1].Text, "#C3");
			Assert.AreEqual (2, lvw.SelectedIndices.Count, "#C4");
			Assert.AreEqual (0, lvw.SelectedIndices [0], "#C5");
			Assert.AreEqual (2, lvw.SelectedIndices [1], "#C6");
			form.Dispose ();
		}

		[Test]
		public void FindItemWithText ()
		{
			ListView lvw = new ListView();
			ListViewItem lvi1 = new ListViewItem (String.Empty);
			ListViewItem lvi2 = new ListViewItem ("angle bracket");
			ListViewItem lvi3 = new ListViewItem ("bracket holder");
			ListViewItem lvi4 = new ListViewItem ("bracket");
			lvw.Items.AddRange (new ListViewItem [] { lvi1, lvi2, lvi3, lvi4 });

			Assert.AreEqual (lvi1, lvw.FindItemWithText (String.Empty), "#A1");
			Assert.AreEqual (lvi3, lvw.FindItemWithText ("bracket"), "#A2");
			Assert.AreEqual (lvi3, lvw.FindItemWithText ("BrackeT"), "#A3");
			Assert.IsNull (lvw.FindItemWithText ("holder"), "#A5");

			Assert.AreEqual (lvw.Items [3], lvw.FindItemWithText ("bracket", true, 3), "#B1");

			Assert.AreEqual (lvw.Items [2], lvw.FindItemWithText ("bracket", true, 0, true), "#C1");
			Assert.AreEqual (lvw.Items [3], lvw.FindItemWithText ("bracket", true, 0, false), "#C2");
			Assert.AreEqual(lvw.Items [3], lvw.FindItemWithText("BrackeT", true, 0, false), "#C3");
			Assert.IsNull (lvw.FindItemWithText ("brack", true, 0, false), "#C4");

			// Sub item search tests
			lvw.Items.Clear ();

			lvi1.Text = "A";
			lvi1.SubItems.Add ("car bracket");
			lvi1.SubItems.Add ("C");

			lvi2.Text = "B";
			lvi2.SubItems.Add ("car");

			lvi3.Text = "C";

			lvw.Items.AddRange (new ListViewItem [] { lvi1, lvi2, lvi3 });

			Assert.AreEqual (lvi1, lvw.FindItemWithText ("car", true, 0), "#D1");
			Assert.AreEqual (lvi3, lvw.FindItemWithText ("C", true, 0), "#D2");
			Assert.AreEqual (lvi2, lvw.FindItemWithText ("car", true, 1), "#D3");
			Assert.IsNull (lvw.FindItemWithText ("car", false, 0), "#D4");

			Assert.AreEqual (lvi1, lvw.FindItemWithText ("car", true, 0, true), "#E1");
			Assert.AreEqual (lvi2, lvw.FindItemWithText ("car", true, 0, false), "#E2");
			Assert.AreEqual (lvi2, lvw.FindItemWithText ("CaR", true, 0, false), "#E3");
		}

		[Test]
		public void FindItemWithText_Exceptions ()
		{
			ListView lvw = new ListView ();

			// Shouldn't throw any exception
			lvw.FindItemWithText (null);

			try {
				lvw.FindItemWithText (null, false, 0);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				lvw.FindItemWithText (null, false, lvw.Items.Count);
				Assert.Fail ("#A2");
			} catch (ArgumentOutOfRangeException) {
			}

			// Add a single item
			lvw.Items.Add ("bracket");

			try {
				lvw.FindItemWithText (null);
				Assert.Fail ("#A3");
			} catch (ArgumentNullException) {
			}

			try {
				lvw.FindItemWithText ("bracket", false, -1);
				Assert.Fail ("#A4");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				lvw.FindItemWithText ("bracket", false, lvw.Items.Count);
				Assert.Fail ("#A5");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void FindNearestItem_Exceptions ()
		{
			ListView lvw = new ListView ();
			lvw.Items.Add ("A");
			lvw.Items.Add ("B");

			lvw.View = View.Details;
			try {
				lvw.FindNearestItem (SearchDirectionHint.Down, 0, 0);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException) {
			}

			lvw.View = View.List;
			try {
				lvw.FindNearestItem (SearchDirectionHint.Down, 0, 0);
				Assert.Fail ("#A2");
			} catch (InvalidOperationException) {
			}

			lvw.View = View.Tile;
			try {
				lvw.FindNearestItem (SearchDirectionHint.Down, 0, 0);
				Assert.Fail ("#A3");
			} catch (InvalidOperationException) {
			}

			lvw.View = View.LargeIcon;
			try {
				lvw.FindNearestItem ((SearchDirectionHint)666, 0, 0);
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void FocusedItem ()
		{
			ListView lvw = CreateListView (View.LargeIcon);
			Form form = new Form ();
			lvw.Parent = form;

			// FocusedItem setter ignores the value until form is shown
			lvw.FocusedItem = lvw.Items [2];
			Assert.AreEqual (null, lvw.FocusedItem, "#A1");

			// It's not enough to create the ListView control
			form.Show ();

			lvw.FocusedItem = lvw.Items [2];
			Assert.AreEqual (lvw.Items [2], lvw.FocusedItem, "#A2");

			lvw.FocusedItem = new ListViewItem ();
			Assert.AreEqual (lvw.Items [2], lvw.FocusedItem, "#A3");

			lvw.FocusedItem = null;
			Assert.AreEqual (lvw.Items [2], lvw.FocusedItem, "#A4");

			form.Dispose ();
		}

		[Test]
		public void HotTracking ()
		{
			ListView lvw = new ListView ();

			lvw.HotTracking = true;
			Assert.AreEqual (true, lvw.HotTracking, "#A1");
			Assert.AreEqual (true, lvw.HoverSelection, "#A2");
			Assert.AreEqual (ItemActivation.OneClick, lvw.Activation, "#A3");

			// HoverSelection and Activation keep the previous value
			lvw.HotTracking = false;
			Assert.AreEqual (false, lvw.HotTracking, "#B1");
			Assert.AreEqual (true, lvw.HoverSelection, "#B2");
			Assert.AreEqual (ItemActivation.OneClick, lvw.Activation, "#B3");

			lvw.HotTracking = true;
			try {
				lvw.HoverSelection = false;
				Assert.Fail ("#C1");
			} catch (ArgumentException) {
			}

			try {
				lvw.Activation = ItemActivation.Standard;
				Assert.Fail ("#C2");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void RedrawItems_Exceptions ()
		{
			ListView lvw = new ListView ();
			lvw.Items.Add ("A");
			lvw.Items.Add ("B");

			try {
				lvw.RedrawItems (-1, 1, true);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				lvw.RedrawItems (0, -1, true);
				Assert.Fail ("#A2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				lvw.RedrawItems (lvw.Items.Count, 1, true);
				Assert.Fail ("#A3");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				lvw.RedrawItems (0, lvw.Items.Count, true);
				Assert.Fail ("#A4");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				lvw.RedrawItems (1, 0, true);
				Assert.Fail ("#A5");
			} catch (ArgumentException) {
			}
		}

		const int item_count = 3;
		ListViewItem [] items = new ListViewItem [item_count];

		[Test]
		public void VirtualMode ()
		{
			ListView lvw = new ListView ();
			lvw.VirtualListSize = item_count;
			lvw.RetrieveVirtualItem += ListViewRetrieveVirtualItemHandler;
			lvw.VirtualMode = true;

			CreateListViewItems (item_count);

			Assert.AreEqual (item_count, lvw.Items.Count, "#A1");
			Assert.AreEqual (true, lvw.VirtualMode, "#A2");

			Assert.AreEqual (items [0], lvw.Items [0], "#B1");
			Assert.AreEqual (items [1], lvw.Items [1], "#B2");
			Assert.AreEqual (items [2], lvw.Items [2], "#B3");
			Assert.AreEqual (0, lvw.Items [0].Index, "#B4");
			Assert.AreEqual (1, lvw.Items [1].Index, "#B5");
			Assert.AreEqual (2, lvw.Items [2].Index, "#B6");
			Assert.AreEqual (lvw, lvw.Items [0].ListView, "#B7");
			Assert.AreEqual (lvw, lvw.Items [1].ListView, "#B8");
			Assert.AreEqual (lvw, lvw.Items [2].ListView, "#B9");

			// force to re-create the items, because we need a blank state
			// for our items
			CreateListViewItems (item_count);
			items [0].Name = "A";
			items [1].Name = "B";
			items [2].Name = "C";

			Assert.AreEqual (items [0], lvw.Items ["A"], "#C1");
			Assert.AreEqual (items [1], lvw.Items ["B"], "#C2");
			Assert.AreEqual (items [2], lvw.Items ["C"], "#C3");
			Assert.AreEqual (0, lvw.Items ["A"].Index, "#C4");
			Assert.AreEqual (1, lvw.Items ["B"].Index, "#C5");
			Assert.AreEqual (2, lvw.Items ["C"].Index, "#C6");
			Assert.AreEqual (lvw, lvw.Items ["A"].ListView, "#C7");
			Assert.AreEqual (lvw, lvw.Items ["B"].ListView, "#C8");
			Assert.AreEqual (lvw, lvw.Items ["C"].ListView, "#C9");
			Assert.IsNull (lvw.Items ["Invalid key"], "#C10");
			Assert.IsNull (lvw.Items [String.Empty], "#C11");

			Assert.AreEqual (false, lvw.Items.ContainsKey (String.Empty), "#D1");
			Assert.AreEqual (false, lvw.Items.ContainsKey (null), "#D2");
			Assert.AreEqual (true, lvw.Items.ContainsKey ("A"), "#D3");
			Assert.AreEqual (true, lvw.Items.ContainsKey ("a"), "#D4");
			Assert.AreEqual (true, lvw.Items.ContainsKey ("B"), "#D5");
		}

		void ListViewRetrieveVirtualItemHandler (object o, RetrieveVirtualItemEventArgs args)
		{
			args.Item = items [args.ItemIndex];
		}

		void CreateListViewItems(int count)
		{
			items = new ListViewItem [count];

			for (int i = 0; i < count; i++)
				items [i] = new ListViewItem (String.Empty);
		}

		[Test]
		public void VirtualMode_Exceptions()
		{
			ListView lvw = new ListView ();

			lvw.Items.Add ("Simple item");
			try {
				lvw.VirtualMode = true;
				Assert.Fail ("#A1");
			} catch (InvalidOperationException) {
			}

			lvw.Items.Clear();
			lvw.VirtualMode = true;
			lvw.VirtualListSize = 1;

			lvw.RetrieveVirtualItem += ListViewRetrieveVirtualItemHandler;
			CreateListViewItems (1);

			try {
				lvw.Sort ();
				Assert.Fail ("#A3");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void VirtualListSize ()
		{
			ListView lvw = new ListView ();

			lvw.VirtualListSize = item_count;
			Assert.AreEqual (item_count, lvw.VirtualListSize, "#A1");
			Assert.AreEqual (0, lvw.Items.Count, "#A2");

			lvw.VirtualMode = true;
			Assert.AreEqual (item_count, lvw.VirtualListSize, "#B1");
			Assert.AreEqual (item_count, lvw.Items.Count, "#B2");

			lvw.VirtualMode = false;
			Assert.AreEqual (item_count, lvw.VirtualListSize, "#C1");
			Assert.AreEqual (0, lvw.Items.Count, "#C2");
		}

		[Test]
		public void VirtualListSize_Exceptions ()
		{
			ListView lvw = new ListView ();
			try {
				lvw.VirtualListSize = -1;
				Assert.Fail ("#A1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Sort_Details_Checked ()
		{
			AssertSort_Checked (View.Details);
		}

		[Test]
		public void Sort_Details_Created ()
		{
			AssertSortNoIcon_Created (View.Details);
		}

		[Test]
		public void Sort_Details_NotCreated ()
		{
			AssertSortNoIcon_NotCreated (View.Details);
		}

		[Test]
		public void Sort_Details_Selected ()
		{
			AssertSort_Selected (View.Details);
		}

		[Test]
		public void Sort_LargeIcon_Checked ()
		{
			AssertSort_Checked (View.LargeIcon);
		}

		[Test]
		public void Sort_LargeIcon_Created ()
		{
			AssertSortIcon_Created (View.LargeIcon);
		}

		[Test]
		public void Sort_LargeIcon_NotCreated ()
		{
			AssertSortIcon_NotCreated (View.LargeIcon);
		}

		[Test]
		public void Sort_LargeIcon_Selected ()
		{
			AssertSort_Selected (View.LargeIcon);
		}

		[Test]
		public void Sort_List_Checked ()
		{
			AssertSort_Checked (View.List);
		}

		[Test]
		public void Sort_List_Created ()
		{
			AssertSortNoIcon_Created (View.List);
		}

		[Test]
		public void Sort_List_NotCreated ()
		{
			AssertSortNoIcon_NotCreated (View.List);
		}

		[Test]
		public void Sort_List_Selection ()
		{
			AssertSort_Selected (View.List);
		}

		[Test]
		public void Sort_SmallIcon_Checked ()
		{
			AssertSort_Checked (View.SmallIcon);
		}

		[Test]
		public void Sort_SmallIcon_Created ()
		{
			AssertSortIcon_Created (View.SmallIcon);
		}

		[Test]
		public void Sort_SmallIcon_NotCreated ()
		{
			AssertSortIcon_NotCreated (View.SmallIcon);
		}

		[Test]
		public void Sort_SmallIcon_Selection ()
		{
			AssertSort_Selected (View.SmallIcon);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Sort_Tile_Checked ()
		{
			AssertSort_Checked (View.Tile);
		}

		[Test]
		public void Sort_Tile_Created ()
		{
			AssertSortNoIcon_Created (View.Tile);
		}

		[Test]
		public void Sort_Tile_NotCreated ()
		{
			AssertSortNoIcon_NotCreated (View.Tile);
		}

		[Test]
		public void Sort_Tile_Selection ()
		{
			AssertSort_Selected (View.Tile);
		}

		private void AssertSortIcon_Created (View view)
		{
			int compareCount = 0;

			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = CreateListView (view);
			form.Controls.Add (lvw);
			Assert.IsNull (lvw.ListViewItemSorter, "#A");

			form.Show ();

			Assert.IsNull (lvw.ListViewItemSorter, "#B1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#B2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#B3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#B4");

			lvw.Sorting = SortOrder.None;
			Assert.IsNull (lvw.ListViewItemSorter, "#C1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#C2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#C3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#C4");

			lvw.Sorting = SortOrder.Descending;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#D1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#D2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#D3");
			Assert.AreEqual ("A", lvw.Items [2].Text, "#D4");

			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#E1");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#E2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#E3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#E4");

			lvw.Sorting = SortOrder.None;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#F1");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#F2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#F3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#F4");

			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#G1");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#G2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#G3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#G4");

			lvw.Sorting = SortOrder.Descending;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#G1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#G2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#G3");
			Assert.AreEqual ("A", lvw.Items [2].Text, "#G4");

			lvw.Sorting = SortOrder.None;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#H1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#H2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#H3");
			Assert.AreEqual ("A", lvw.Items [2].Text, "#H4");

			// when Sorting is None and a new item is added, the collection is
			// sorted using the previous Sorting value
			lvw.Items.Add ("BB");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#I1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#I2");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#I3");
			Assert.AreEqual ("B", lvw.Items [2].Text, "#I4");
			Assert.AreEqual ("A", lvw.Items [3].Text, "#I5");

			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#J1");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#J2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#J3");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#J4");
			Assert.AreEqual ("C", lvw.Items [3].Text, "#J5");

			// when Sorting is not None and a new item is added, the
			// collection is re-sorted automatically
			lvw.Items.Add ("BA");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#K1");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#K2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#K3");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#K4");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#K5");
			Assert.AreEqual ("C", lvw.Items [4].Text, "#K6");

			// assign a custom comparer
			MockComparer mc = new MockComparer (false);
			lvw.ListViewItemSorter = mc;

			// when a custom IComparer is assigned, the collection is immediately
			// re-sorted
			Assert.IsTrue (mc.CompareCount > compareCount, "#L1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#L2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#L3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#L4");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#L5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#L6");
			Assert.AreEqual ("B", lvw.Items [3].Text, "#L7");
			Assert.AreEqual ("A", lvw.Items [4].Text, "#L8");

			// record compare count
			compareCount = mc.CompareCount;

			// modifying Sorting results in re-sort
			lvw.Sorting = SortOrder.Descending;
			Assert.IsTrue (mc.CompareCount > compareCount, "#M1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#M2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#M3");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#M4");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#M5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#M6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#M7");
			Assert.AreEqual ("C", lvw.Items [4].Text, "#M8");

			// record compare count
			compareCount = mc.CompareCount;

			// setting Sorting to the same value does not result in a sort
			// operation
			lvw.Sorting = SortOrder.Descending;
			Assert.AreEqual (compareCount, mc.CompareCount, "#N1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#N2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#N3");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#N4");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#N5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#N6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#N7");
			Assert.AreEqual ("C", lvw.Items [4].Text, "#N8");

			// modifying Sorting results in re-sort
			lvw.Sorting = SortOrder.Ascending;
			Assert.IsTrue (mc.CompareCount > compareCount, "#O1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#O2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#O3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#O4");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#O5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#O6");
			Assert.AreEqual ("B", lvw.Items [3].Text, "#O7");
			Assert.AreEqual ("A", lvw.Items [4].Text, "#O8");

			// record compare count
			compareCount = mc.CompareCount;

			// adding an item when Sorting is not None causes re-sort
			lvw.Items.Add ("BC");
			Assert.IsTrue (mc.CompareCount > compareCount, "#P1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#P2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#P3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#P4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#P5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#P6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#P7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#P8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#P9");

			// record compare count
			compareCount = mc.CompareCount;

			// assigning the same custom IComparer again does not result in a
			// re-sort
			lvw.ListViewItemSorter = mc;
			Assert.AreEqual (compareCount, mc.CompareCount, "#Q1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#Q2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#Q3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#Q4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#Q5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#Q6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#Q7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#Q8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#Q9");

			// setting Sorting to None does not perform a sort
			lvw.Sorting = SortOrder.None;
			Assert.AreEqual (compareCount, mc.CompareCount, "#R1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#R2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#R3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#R4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#R5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#R6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#R7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#R8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#R9");

			// assigning the custom IComparer again does not result in a
			// re-sort
			lvw.ListViewItemSorter = mc;
			Assert.AreEqual (compareCount, mc.CompareCount, "#S1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#S2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#S3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#S4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#S5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#S6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#S7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#S8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#S9");

			// set Sorting to Ascending again
			lvw.Sorting = SortOrder.Ascending;
			Assert.IsTrue (mc.CompareCount > compareCount, "#T1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#T2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#T3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#T4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#T5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#T6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#T7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#T8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#T9");

			// record compare count
			compareCount = mc.CompareCount;

			// explicitly calling Sort results in a sort operation
			lvw.Sort ();
			Assert.IsTrue (mc.CompareCount > compareCount, "#U1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#U2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#U3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#U4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#U5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#U6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#U7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#U8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#U9");
			lvw.Sorting = SortOrder.None;

			// record compare count
			compareCount = mc.CompareCount;

			// adding an item when Sorting is None causes re-sort
			lvw.Items.Add ("BD");
			Assert.IsTrue (mc.CompareCount > compareCount, "#V1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#V2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#V3");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#V4");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#V5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#V6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#V7");
			Assert.AreEqual ("BC", lvw.Items [4].Text, "#V8");
			Assert.AreEqual ("BD", lvw.Items [5].Text, "#V9");
			Assert.AreEqual ("C", lvw.Items [6].Text, "#V10");

			// record compare count
			compareCount = mc.CompareCount;

			// explicitly calling Sort when Sorting is None causes a re-sort
			lvw.Sort ();
			Assert.IsTrue (mc.CompareCount > compareCount, "#W1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#W2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#W3");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#W4");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#W5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#W6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#W7");
			Assert.AreEqual ("BC", lvw.Items [4].Text, "#W8");
			Assert.AreEqual ("BD", lvw.Items [5].Text, "#W9");
			Assert.AreEqual ("C", lvw.Items [6].Text, "#W10");

			// record compare count
			compareCount = mc.CompareCount;
			form.Dispose ();
		}

		private void AssertSortIcon_NotCreated (View view)
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = CreateListView (view);
			form.Controls.Add (lvw);

			Assert.IsNull (lvw.ListViewItemSorter, "#A1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#A2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#A3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#A4");

			lvw.Sorting = SortOrder.None;
			Assert.IsNull (lvw.ListViewItemSorter, "#B1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#B2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#B3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#B4");

			lvw.Sorting = SortOrder.Descending;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#C1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#C2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#C3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#C4");

			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNotNull (lvw.ListViewItemSorter, "#D1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#D2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#D3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#D4");

			// when the handle is not created and a new item is added, the new
			// item is just appended to the collection
			lvw.Items.Add ("BB");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#E1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#E2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#E3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#E4");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#E5");

			// assign a custom comparer
			MockComparer mc = new MockComparer (false);
			lvw.ListViewItemSorter = mc;

			// assigning a custom IComparer has no effect
			Assert.AreEqual (0, mc.CompareCount, "#F1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#F2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#F3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#F4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#F5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#F6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#F7");

			// modifying Sorting does not result in sort operation
			lvw.Sorting = SortOrder.Descending;
			Assert.AreEqual (0, mc.CompareCount, "#G1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#G2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#G3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#G4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#G5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#G6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#G7");

			// setting Sorting to the same value does not result in a sort
			// operation
			lvw.Sorting = SortOrder.Descending;
			Assert.AreEqual (0, mc.CompareCount, "#H1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#H2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#H3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#H4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#H5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#H6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#H7");

			// setting Sorting to None does not result in a sort operation
			lvw.Sorting = SortOrder.None;
			Assert.AreEqual (0, mc.CompareCount, "#I1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#I2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#I3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#I4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#I5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#I6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#I7");

			// explicitly calling Sort when Sorting is None does not result
			// in a sort operation
			lvw.Sort ();
			Assert.AreEqual (0, mc.CompareCount, "#J1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#J2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#J3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#J4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#J5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#J6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#J7");

			// setting Sorting again does not result in a sort operation
			lvw.Sorting = SortOrder.Ascending;
			Assert.AreEqual (0, mc.CompareCount, "#K1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#K2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#K3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#K4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#K5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#K6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#K7");

			// explicitly calling Sort when Sorting is Ascending does not 
			// result in a sort operation
			lvw.Sort ();
			Assert.AreEqual (0, mc.CompareCount, "#L1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#L2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#L3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#L4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#L5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#L6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#L7");

			// show the form to create the handle
			form.Show ();

			// when the handle is created, the items are immediately sorted
			Assert.IsTrue (mc.CompareCount > 0, "#L1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#M2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#M3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#M4");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#M5");
			Assert.AreEqual ("B", lvw.Items [2].Text, "#M6");
			Assert.AreEqual ("A", lvw.Items [3].Text, "#M7");

			// setting ListViewItemSorter to null does not result in sort
			// operation
			lvw.ListViewItemSorter = null;
			Assert.IsNull (lvw.ListViewItemSorter, "#N1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#N2");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#N3");
			Assert.AreEqual ("B", lvw.Items [2].Text, "#N4");
			Assert.AreEqual ("A", lvw.Items [3].Text, "#N5");

			// explicitly calling sort does not result in sort operation
			lvw.Sort ();
			Assert.IsNull (lvw.ListViewItemSorter, "#O1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#O2");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#O3");
			Assert.AreEqual ("B", lvw.Items [2].Text, "#O4");
			Assert.AreEqual ("A", lvw.Items [3].Text, "#O5");

			form.Dispose ();
		}

		private void AssertSortNoIcon_Created (View view)
		{
			int compareCount = 0;

			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = CreateListView (view);
			form.Controls.Add (lvw);
			Assert.IsNull (lvw.ListViewItemSorter, "#A");

			form.Show ();

			Assert.IsNull (lvw.ListViewItemSorter, "#B1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#B2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#B3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#B4");

			lvw.Sorting = SortOrder.None;
			Assert.IsNull (lvw.ListViewItemSorter, "#C1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#C2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#C3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#C4");

			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNull (lvw.ListViewItemSorter, "#D1");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#D2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#D3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#D4");

			lvw.Sorting = SortOrder.Descending;
			Assert.IsNull (lvw.ListViewItemSorter, "#E1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#E2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#E3");
			Assert.AreEqual ("A", lvw.Items [2].Text, "#E4");

			lvw.Sorting = SortOrder.None;
			Assert.IsNull (lvw.ListViewItemSorter, "#F1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#F2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#F3");
			Assert.AreEqual ("A", lvw.Items [2].Text, "#F4");

			// when Sorting is None and a new item is added, the item is
			// appended to the collection
			lvw.Items.Add ("BB");
			Assert.IsNull (lvw.ListViewItemSorter, "#G1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#G2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#G3");
			Assert.AreEqual ("A", lvw.Items [2].Text, "#G4");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#G5");

			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNull (lvw.ListViewItemSorter, "#H1");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#H2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#H3");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#H4");
			Assert.AreEqual ("C", lvw.Items [3].Text, "#H5");

			// when Sorting is not None and a new item is added, the 
			// collection is re-sorted automatically
			lvw.Items.Add ("BA");
			Assert.IsNull (lvw.ListViewItemSorter, "#I1");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#I2");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#I3");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#I4");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#I5");
			Assert.AreEqual ("C", lvw.Items [4].Text, "#I6");

			// assign a custom comparer
			MockComparer mc = new MockComparer (false);
			lvw.ListViewItemSorter = mc;

			// when a custom IComparer is assigned, the collection is immediately
			// re-sorted
			Assert.IsTrue (mc.CompareCount > compareCount, "#J1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#J2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#J3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#J4");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#J5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#J6");
			Assert.AreEqual ("B", lvw.Items [3].Text, "#J7");
			Assert.AreEqual ("A", lvw.Items [4].Text, "#J8");

			// record compare count
			compareCount = mc.CompareCount;

			// modifying the sort order results in a sort
			lvw.Sorting = SortOrder.Descending;
			Assert.IsTrue (mc.CompareCount > compareCount, "#L1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#K2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#K3");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#K4");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#K5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#K6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#K7");
			Assert.AreEqual ("C", lvw.Items [4].Text, "#K8");

			// record compare count
			compareCount = mc.CompareCount;

			// set the sort order to the same value does not result in a sort
			// operation
			lvw.Sorting = SortOrder.Descending;
			Assert.AreEqual (compareCount, mc.CompareCount, "#L1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#L2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#L3");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#L4");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#L5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#L6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#L7");
			Assert.AreEqual ("C", lvw.Items [4].Text, "#L8");

			// modifying the sort order results in a sort
			lvw.Sorting = SortOrder.Ascending;
			Assert.IsTrue (mc.CompareCount > compareCount, "#M1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#M2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#M3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#M4");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#M5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#M6");
			Assert.AreEqual ("B", lvw.Items [3].Text, "#M7");
			Assert.AreEqual ("A", lvw.Items [4].Text, "#M8");

			// record compare count
			compareCount = mc.CompareCount;

			// adding an item when Sorting is not None caused a re-sort
			lvw.Items.Add ("BC");
			Assert.IsTrue (mc.CompareCount > compareCount, "#N1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#N2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#N3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#N4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#N5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#N6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#N7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#N8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#N9");

			// record compare count
			compareCount = mc.CompareCount;

			// assigning the same custom IComparer again does not result in a
			// re-sort
			lvw.ListViewItemSorter = mc;
			Assert.AreEqual (compareCount, mc.CompareCount, "#O1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#O2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#O3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#O4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#O5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#O6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#O7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#O8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#O9");

			// setting sort order to None does not perform a sort and resets
			// the ListViewItemSorter
			lvw.Sorting = SortOrder.None;
			Assert.AreEqual (compareCount, mc.CompareCount, "#P1");
			Assert.IsNull (lvw.ListViewItemSorter, "#P2");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#P3");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#P4");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#P5");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#P6");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#P7");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#P8");


			lvw.ListViewItemSorter = mc;
			// assigning the previous custom IComparer again results in a
			// re-sort
			Assert.IsTrue (mc.CompareCount > compareCount, "#Q1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#Q2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#Q3");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#Q4");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#Q5");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#Q6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#Q7");
			Assert.AreEqual ("BC", lvw.Items [4].Text, "#Q8");
			Assert.AreEqual ("C", lvw.Items [5].Text, "#Q9");

			// record compare count
			compareCount = mc.CompareCount;

			// set Sorting to Ascending again to verify that the internal
			// IComparer is not used when we reset Sorting to None
			// (as the items would then be sorted alfabetically)
			lvw.Sorting = SortOrder.Ascending;
			Assert.IsTrue (mc.CompareCount > compareCount, "#R1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#R2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#R3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#R4");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#R5");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#R6");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#R7");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#R8");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#R9");

			// record compare count
			compareCount = mc.CompareCount;

			lvw.Sorting = SortOrder.None;
			Assert.AreEqual (compareCount, mc.CompareCount, "#S1");
			Assert.IsNull (lvw.ListViewItemSorter, "#S2");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#S3");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#S4");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#S5");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#S6");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#S7");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#S8");

			// record compare count
			compareCount = mc.CompareCount;

			lvw.Items.Add ("BD");
			// adding an item when Sorting is None does not cause a re-sort
			Assert.AreEqual (compareCount, mc.CompareCount, "#T1");
			Assert.IsNull (lvw.ListViewItemSorter, "#T2");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#T3");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#T4");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#T5");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#T6");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#T7");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#T8");
			Assert.AreEqual ("BD", lvw.Items [6].Text, "#T9");

			// record compare count
			compareCount = mc.CompareCount;

			lvw.Sort ();
			// explicitly calling Sort when Sorting is None does nothing
			Assert.AreEqual (compareCount, mc.CompareCount, "#U1");
			Assert.IsNull (lvw.ListViewItemSorter, "#U2");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#U3");
			Assert.AreEqual ("BC", lvw.Items [1].Text, "#U4");
			Assert.AreEqual ("BB", lvw.Items [2].Text, "#U5");
			Assert.AreEqual ("BA", lvw.Items [3].Text, "#U6");
			Assert.AreEqual ("B", lvw.Items [4].Text, "#U7");
			Assert.AreEqual ("A", lvw.Items [5].Text, "#U8");
			Assert.AreEqual ("BD", lvw.Items [6].Text, "#U9");

			// record compare count
			compareCount = mc.CompareCount;

			lvw.Sorting = SortOrder.Ascending;
			// setting Sorting again, does not reinstate the custom IComparer
			// but sorting is actually performed using an internal non-visible
			// comparer
			Assert.AreEqual (compareCount, mc.CompareCount, "#V1");
			Assert.IsNull (lvw.ListViewItemSorter, "#V2");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#V3");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#V4");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#V5");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#V6");
			Assert.AreEqual ("BC", lvw.Items [4].Text, "#V7");
			Assert.AreEqual ("BD", lvw.Items [5].Text, "#V8");
			Assert.AreEqual ("C", lvw.Items [6].Text, "#V9");

			// record compare count
			compareCount = mc.CompareCount;

			lvw.Sort ();
			// explicitly calling Sort, does not reinstate the custom IComparer
			// but sorting is actually performed using an internal non-visible
			// comparer
			Assert.AreEqual (compareCount, mc.CompareCount, "#W1");
			Assert.IsNull (lvw.ListViewItemSorter, "#W2");
			Assert.AreEqual ("A", lvw.Items [0].Text, "#W3");
			Assert.AreEqual ("B", lvw.Items [1].Text, "#W4");
			Assert.AreEqual ("BA", lvw.Items [2].Text, "#W5");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#W6");
			Assert.AreEqual ("BC", lvw.Items [4].Text, "#W7");
			Assert.AreEqual ("BD", lvw.Items [5].Text, "#W8");
			Assert.AreEqual ("C", lvw.Items [6].Text, "#W9");

			// record compare count
			compareCount = mc.CompareCount;

			form.Dispose ();
		}

		private void AssertSortNoIcon_NotCreated (View view)
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = CreateListView (view);
			form.Controls.Add (lvw);

			Assert.IsNull (lvw.ListViewItemSorter, "#A1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#A2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#A3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#A4");

			lvw.Sorting = SortOrder.None;
			Assert.IsNull (lvw.ListViewItemSorter, "#B1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#B2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#B3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#B4");

			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNull (lvw.ListViewItemSorter, "#C1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#C2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#C3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#C4");

			lvw.Sorting = SortOrder.Descending;
			Assert.IsNull (lvw.ListViewItemSorter, "#D1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#D2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#D3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#D4");

			lvw.Sorting = SortOrder.None;
			Assert.IsNull (lvw.ListViewItemSorter, "#E1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#E2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#E3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#E4");

			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNull (lvw.ListViewItemSorter, "#F1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#F2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#F3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#F4");

			// when the handle is not created and a new item is added, the new
			// item is just appended to the collection
			lvw.Items.Add ("BB");
			Assert.IsNull (lvw.ListViewItemSorter, "#G1");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#G2");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#G3");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#G4");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#G5");

			// assign a custom comparer
			MockComparer mc = new MockComparer (false);
			lvw.ListViewItemSorter = mc;

			// assigning a custom IComparer has no effect
			Assert.AreEqual (0, mc.CompareCount, "#H1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#H2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#H3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#H4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#H5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#H6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#H7");

			// modifying Sorting has no effect
			lvw.Sorting = SortOrder.Descending;
			Assert.AreEqual (0, mc.CompareCount, "#I1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#I2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#I3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#I4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#I5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#I6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#I7");

			// setting Sorting to the same value does not result in a sort
			// operation
			lvw.Sorting = SortOrder.Descending;
			Assert.AreEqual (0, mc.CompareCount, "#J1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#J2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#J3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#J4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#J5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#J6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#J7");

			// setting Sorting to another value does not result in a sort
			// operation
			lvw.Sorting = SortOrder.Ascending;
			Assert.AreEqual (0, mc.CompareCount, "#K1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#K2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#K3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#K4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#K5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#K6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#K7");

			lvw.Sorting = SortOrder.None;
			Assert.AreEqual (0, mc.CompareCount, "#L1");
			// setting Sorting to None does not perform a sort and resets the
			// ListViewItemSorter
			Assert.IsNull (lvw.ListViewItemSorter, "#L2");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#L3");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#L4");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#L5");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#L6");

			// explicitly calling Sort when Sorting is None does nothing
			lvw.Sort ();
			Assert.AreEqual (0, mc.CompareCount, "#M1");
			Assert.IsNull (lvw.ListViewItemSorter, "#M2");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#M3");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#M4");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#M5");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#M6");

			lvw.Sorting = SortOrder.Ascending;
			Assert.AreEqual (0, mc.CompareCount, "#N1");
			Assert.IsNull (lvw.ListViewItemSorter, "#N2");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#N3");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#N4");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#N5");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#N6");

			// explicitly set the custom IComparer again
			lvw.ListViewItemSorter = mc;
			Assert.AreEqual (0, mc.CompareCount, "#O1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#O2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#O3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#O4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#O5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#O6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#O7");

			// explicitly calling Sort when handle is not created does not
			// result in sort operation
			Assert.AreEqual (0, mc.CompareCount, "#P1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#P2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#P3");
			Assert.AreEqual ("B", lvw.Items [0].Text, "#P4");
			Assert.AreEqual ("A", lvw.Items [1].Text, "#P5");
			Assert.AreEqual ("C", lvw.Items [2].Text, "#P6");
			Assert.AreEqual ("BB", lvw.Items [3].Text, "#P7");

			// show the form to create the handle
			form.Show ();

			// when the handle is created, the items are immediately sorted
			Assert.IsTrue (mc.CompareCount > 0, "#Q1");
			Assert.IsNotNull (lvw.ListViewItemSorter, "#Q2");
			Assert.AreSame (mc, lvw.ListViewItemSorter, "#Q3");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#Q4");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#Q5");
			Assert.AreEqual ("B", lvw.Items [2].Text, "#Q6");
			Assert.AreEqual ("A", lvw.Items [3].Text, "#Q7");

			// setting ListViewItemSorter to null does not result in sort
			// operation
			lvw.ListViewItemSorter = null;
			Assert.IsNull (lvw.ListViewItemSorter, "#R1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#R2");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#R3");
			Assert.AreEqual ("B", lvw.Items [2].Text, "#R4");
			Assert.AreEqual ("A", lvw.Items [3].Text, "#R5");

			// explicitly calling sort does not result in sort operation
			lvw.Sort ();
			Assert.IsNull (lvw.ListViewItemSorter, "#S1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#S2");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#S3");
			Assert.AreEqual ("B", lvw.Items [2].Text, "#S4");
			Assert.AreEqual ("A", lvw.Items [3].Text, "#S5");

			// modifying Sorting does not result in sort operation
			lvw.Sorting = SortOrder.Ascending;
			Assert.IsNull (lvw.ListViewItemSorter, "#T1");
			Assert.AreEqual ("C", lvw.Items [0].Text, "#T2");
			Assert.AreEqual ("BB", lvw.Items [1].Text, "#T3");
			Assert.AreEqual ("B", lvw.Items [2].Text, "#T4");
			Assert.AreEqual ("A", lvw.Items [3].Text, "#T5");

			form.Dispose ();
		}

		private void AssertSort_Checked (View view)
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = CreateListView (view);
			lvw.CheckBoxes = true;
			form.Controls.Add (lvw);

			form.Show ();

			Assert.AreEqual (0, lvw.CheckedItems.Count, "#A1");
			Assert.AreEqual (0, lvw.CheckedIndices.Count, "#A2");

			// select an item
			lvw.Items [2].Checked = true;
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#B1");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#B2");
			Assert.AreEqual ("C", lvw.CheckedItems [0].Text, "#B3");
			Assert.AreEqual (2, lvw.CheckedIndices [0], "#B4");

			// sort the items descending
			lvw.Sorting = SortOrder.Descending;
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#C1");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#C2");
			Assert.AreEqual ("C", lvw.CheckedItems [0].Text, "#C3");
			Assert.AreEqual (0, lvw.CheckedIndices [0], "#C4");

			// add an item, which ends up before the selected item after the
			// sort operation
			ListViewItem item = lvw.Items.Add ("D");
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#D1");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#D2");
			Assert.AreEqual ("C", lvw.CheckedItems [0].Text, "#D3");
			Assert.AreEqual (1, lvw.CheckedIndices [0], "#D4");

			// remove an item before the selected item
			lvw.Items.Remove (item);
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#E1");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#E2");
			Assert.AreEqual ("C", lvw.CheckedItems [0].Text, "#E3");
			Assert.AreEqual (0, lvw.CheckedIndices [0], "#E4");

			// insert an item before the selected item
			lvw.Items.Insert (0, "D");
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#F1");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#F2");
			Assert.AreEqual ("C", lvw.CheckedItems [0].Text, "#F3");
			Assert.AreEqual (1, lvw.CheckedIndices [0], "#F4");

			// assign a custom comparer
			MockComparer mc = new MockComparer (false);
			lvw.ListViewItemSorter = mc;

			// items are re-sorted automatically
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#G1");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#G2");
			Assert.AreEqual ("C", lvw.CheckedItems [0].Text, "#G3");
			Assert.AreEqual (2, lvw.CheckedIndices [0], "#G4");

			// modify sort order
			lvw.Sorting = SortOrder.Ascending;
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#H1");
			Assert.AreEqual (1, lvw.CheckedIndices.Count, "#H2");
			Assert.AreEqual ("C", lvw.CheckedItems [0].Text, "#H3");
			Assert.AreEqual (1, lvw.CheckedIndices [0], "#H4");

			form.Dispose ();
		}

		private void AssertSort_Selected (View view)
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = CreateListView (view);
			form.Controls.Add (lvw);

			form.Show ();

			Assert.AreEqual (0, lvw.SelectedItems.Count, "#A1");
			Assert.AreEqual (0, lvw.SelectedIndices.Count, "#A2");

			// select an item
			lvw.Items [2].Selected = true;
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#B1");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#B2");
			Assert.AreEqual ("C", lvw.SelectedItems [0].Text, "#B3");
			Assert.AreEqual (2, lvw.SelectedIndices [0], "#B4");

			// sort the items descending
			lvw.Sorting = SortOrder.Descending;
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#C1");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#C2");
			Assert.AreEqual ("C", lvw.SelectedItems [0].Text, "#C3");
			Assert.AreEqual (0, lvw.SelectedIndices [0], "#C4");

			// add an item, which ends up before the selected item after the
			// sort operation
			ListViewItem item = lvw.Items.Add ("D");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#D1");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#D2");
			Assert.AreEqual ("C", lvw.SelectedItems [0].Text, "#D3");
			Assert.AreEqual (1, lvw.SelectedIndices [0], "#D4");

			// remove an item before the selected item
			lvw.Items.Remove (item);
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#E1");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#E2");
			Assert.AreEqual ("C", lvw.SelectedItems [0].Text, "#E3");
			Assert.AreEqual (0, lvw.SelectedIndices [0], "#E4");

			// insert an item before the selected item
			lvw.Items.Insert (0, "D");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#F1");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#F2");
			Assert.AreEqual ("C", lvw.SelectedItems [0].Text, "#F3");
			Assert.AreEqual (1, lvw.SelectedIndices [0], "#F4");

			// assign a custom comparer
			MockComparer mc = new MockComparer (false);
			lvw.ListViewItemSorter = mc;

			// items are re-sorted automatically
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#G1");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#G2");
			Assert.AreEqual ("C", lvw.SelectedItems [0].Text, "#G3");
			Assert.AreEqual (2, lvw.SelectedIndices [0], "#G4");

			// modify sort order
			lvw.Sorting = SortOrder.Ascending;
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#H1");
			Assert.AreEqual (1, lvw.SelectedIndices.Count, "#H2");
			Assert.AreEqual ("C", lvw.SelectedItems [0].Text, "#H3");
			Assert.AreEqual (1, lvw.SelectedIndices [0], "#H4");

			form.Dispose ();
		}

		private ListView CreateListView (View view)
		{
			ListView lvw = new ListView ();
			lvw.View = view;
			lvw.Items.Add ("B");
			lvw.Items.Add ("A");
			lvw.Items.Add ("C");
			return lvw;
		}

		private class MockComparer : IComparer
		{
			int _compareCount;
			bool _throwException;

			public MockComparer (bool throwException)
			{
				_throwException = throwException;
			}

			public int CompareCount {
				get { return _compareCount; }
			}

			public int Compare (object x, object y)
			{
				_compareCount++;
				if (_throwException)
					throw new InvalidOperationException ();

				ListViewItem item_x = x as ListViewItem;
				ListViewItem item_y = y as ListViewItem;
				SortOrder sortOrder = item_x.ListView.Sorting;

				// we'll actually perform a reverse-sort
				if (sortOrder == SortOrder.Ascending)
					return String.Compare (item_y.Text, item_x.Text);
				else
					return String.Compare (item_x.Text, item_y.Text);
			}
		}

		[Test]
		public void MethodIsInputChar ()
		{
			// Basically, show that this method always returns true
			InputCharControl m = new InputCharControl ();
			bool result = true;

			for (int i = 0; i < 256; i++)
				result &= m.PublicIsInputChar ((char)i);

			Assert.AreEqual (true, result, "I1");
		}

		private class InputCharControl : ListView
		{
			public bool PublicIsInputChar (char charCode)
			{
				return base.IsInputChar (charCode);
			}
		}

		[Test]  // Should not throw IndexOutOfBoundsException
		public void ReaddingItem ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw1 = new ListView ();
			ListView lvw2 = new ListView ();
			lvw1.View = View.Details;
			lvw2.View = View.Details;
			lvw1.Columns.Add (new ColumnHeader ("1"));
			lvw2.Columns.Add (new ColumnHeader ("2"));
			form.Controls.Add (lvw1);
			form.Controls.Add (lvw2);
			form.Show ();

			for (int i = 0; i < 50; i++)
				lvw1.Items.Add ("A");
			lvw2.Items.Add ("B1");

			ListViewItem item = lvw1.Items [lvw1.Items.Count - 1];
			item.Selected = true;
			item.Remove ();
			lvw2.Items.Add (item);
			item.Selected = true;

			Assert.AreEqual (lvw1.Items.Count, 49, "#1");
			Assert.AreEqual (lvw2.Items.Count, 2, "#2");
			Assert.AreEqual (lvw2.Items [1].Selected, true, "#3");

			form.Dispose ();
		}

		[Test]  // Should not throw ArgumentOutOfRangeException
		public void DeleteNotFocusedItem ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			form.Show ();

			for (int i = 0; i < 3; i++)
				lvw.Items.Add ("A");

			lvw.Items [lvw.Items.Count - 1].Focused = true;
			lvw.Items [0].Remove ();
			lvw.Items [0].Remove ();

			form.Dispose ();
		}
	}
}
