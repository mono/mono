//
// ListViewTest.cs: Test cases for ListView.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListViewTest
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
		}

		[Test]
		public void ArrangeIconsTest ()
		{
			Form myform = new Form ();
			ListView mylistview = new ListView ();
			myform.Controls.Add (mylistview);
			mylistview.Items.Add ("Item 1");
			mylistview.Items.Add ("Item 2");
			mylistview.View = View.LargeIcon;
			mylistview.ArrangeIcons ();
		}

		[Test]
		public void BeginEndUpdateTest ()
		{
			Form myform = new Form ();
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
		}	

		[Test]
		public void ClearTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			ListView mylistview = new ListView ();
			mylistview.Items.Add ("A");
			mylistview.Columns.Add ("Item Column", -2, HorizontalAlignment.Left);
			mylistview.Visible = true;
			myform.Controls.Add (mylistview);
			Assert.AreEqual (1, mylistview.Columns.Count, "#31");
			Assert.AreEqual (1, mylistview.Items.Count, "#32");
			mylistview.Clear ();
			Assert.AreEqual (0, mylistview.Columns.Count, "#33");
			Assert.AreEqual (0, mylistview.Items.Count, "#34");
		}

		[Test]
		public void EnsureVisibleTest ()
		{
			Form myform = new Form ();
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
	}
}
