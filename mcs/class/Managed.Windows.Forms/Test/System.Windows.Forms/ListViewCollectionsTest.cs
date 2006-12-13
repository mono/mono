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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListViewCollectionsTest
	{
		/*
			ColumnHeaderCollection
		*/
		[Test]
		public void ColumnHeaderCollectionTest_PropertiesTest ()
		{
			ListView listview = new ListView ();

			// Properties
			Assert.AreEqual (false, listview.Columns.IsReadOnly, "ColumnHeaderCollectionTest_PropertiesTest#1");
			Assert.AreEqual (true, ((ICollection)listview.Columns).IsSynchronized, "ColumnHeaderCollectionTest_PropertiesTest#2");
			Assert.AreEqual (listview.Columns, ((ICollection)listview.Columns).SyncRoot, "ColumnHeaderCollectionTest_PropertiesTest#3");
			Assert.AreEqual (false, ((IList)listview.Columns).IsFixedSize, "ColumnHeaderCollectionTest_PropertiesTest#4");
			Assert.AreEqual (0, listview.Columns.Count, "ColumnHeaderCollectionTest_PropertiesTest#5");
		}

		[Test]
		public void ColumnHeaderCollectionTest_AddTest ()
		{
			ListView listview = new ListView ();

			// Duplicated elements with same text added
			listview.Columns.Add (new ColumnHeader ());
			listview.Columns.Add (new ColumnHeader ());
			Assert.AreEqual (2, listview.Columns.Count, "ColumnHeaderCollectionTest_AddTest#1");
			Assert.AreEqual ("ColumnHeader", listview.Columns[0].Text, "ColumnHeaderCollectionTest_AddTest#2");
		}

		[Test]
		public void ColumnHeaderCollectionTest_ClearTest ()
		{
			ListView listview = new ListView ();

			// Duplicated elements with same text added
			listview.Columns.Add (new ColumnHeader ());
			listview.Columns.Clear ();
			Assert.AreEqual (0, listview.Columns.Count, "ColumnHeaderCollectionTest_ClearTest#1");
		}

		// Exceptions
		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ColumnHeaderCollectionTest_GetItem_ExceptionTest ()
		{
			// Duplicated elements not added
			ListView listview = new ListView ();
			ColumnHeader item = listview.Columns[5];
			Assert.Fail ("#1: " + item.Text); // avoid CS0219 warning
		}

		/*
			CheckedIndexCollection
		*/
		[Test]
		public void CheckedIndexCollectionTest_PropertiesTest ()
		{
			ListView listview = new ListView ();

			// Properties
			Assert.AreEqual (true, listview.CheckedIndices.IsReadOnly, "CheckedIndexCollectionTest_PropertiesTest#1");
			Assert.AreEqual (false, ((ICollection)listview.CheckedIndices).IsSynchronized, "CheckedIndexCollectionTest_PropertiesTest#2");
			Assert.AreEqual (listview.CheckedIndices, ((ICollection)listview.CheckedIndices).SyncRoot, "CheckedIndexCollectionTest_PropertiesTest#3");
			Assert.AreEqual (true, ((IList)listview.CheckedIndices).IsFixedSize, "CheckedIndexCollectionTest_PropertiesTest#4");
			Assert.AreEqual (0, listview.CheckedIndices.Count, "CheckedIndexCollectionTest_PropertiesTest#5");
		}


		// Exceptions
		[Test, ExpectedException (typeof (NotSupportedException))]
		public void CheckedIndexCollectionTest_Add_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.CheckedIndices).Add (5);
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void CheckedIndexCollectionTest_Remove_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.CheckedIndices).Remove (5);
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void CheckedIndexCollectionTest_RemoveAt_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.CheckedIndices).RemoveAt (5);
		}

		/*
			CheckedItemCollection
		*/
		[Test]
		public void CheckedItemCollectionTest_PropertiesTest ()
		{
			ListView listview = new ListView ();

			// Properties
			Assert.AreEqual (true, listview.CheckedItems.IsReadOnly, "CheckedItemCollectionTest_PropertiesTest#1");
			Assert.AreEqual (false, ((ICollection)listview.CheckedItems).IsSynchronized, "CheckedItemCollectionTest_PropertiesTest#2");
			Assert.AreEqual (listview.CheckedItems, ((ICollection)listview.CheckedItems).SyncRoot, "CheckedItemCollectionTest_PropertiesTest#3");
			Assert.AreEqual (true, ((IList)listview.CheckedItems).IsFixedSize, "CheckedItemCollectionTest_PropertiesTest#4");
			Assert.AreEqual (0, listview.CheckedItems.Count, "CheckedItemCollectionTest_PropertiesTest#5");
		}


		// Exceptions
		[Test, ExpectedException (typeof (NotSupportedException))]
		public void CheckedItemCollectionTest_PropertiesTest_Add_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.CheckedItems).Add (5);
		}

		[Test]
		public void CheckedItemCollectionTest_Order ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			lvw.CheckBoxes = true;
			form.Controls.Add (lvw);
			ListViewItem itemA = lvw.Items.Add ("A");
			itemA.Checked = true;
			ListViewItem itemB = lvw.Items.Add ("B");
			itemB.Checked = true;
			ListViewItem itemC = lvw.Items.Add ("C");
			itemC.Checked = true;

			Assert.AreEqual (3, lvw.CheckedItems.Count, "#A1");
			Assert.AreSame (itemA, lvw.CheckedItems [0], "#A2");
			Assert.AreSame (itemB, lvw.CheckedItems [1], "#A3");
			Assert.AreSame (itemC, lvw.CheckedItems [2], "#A3");

			itemB.Checked = false;

			Assert.AreEqual (2, lvw.CheckedItems.Count, "#B1");
			Assert.AreSame (itemA, lvw.CheckedItems [0], "#B2");
			Assert.AreSame (itemC, lvw.CheckedItems [1], "#B3");

			itemB.Checked = true;

			Assert.AreEqual (3, lvw.CheckedItems.Count, "#C1");
			Assert.AreSame (itemA, lvw.CheckedItems [0], "#C2");
			Assert.AreSame (itemB, lvw.CheckedItems [1], "#C3");
			Assert.AreSame (itemC, lvw.CheckedItems [2], "#C4");

			lvw.Sorting = SortOrder.Descending;

			Assert.AreEqual (3, lvw.CheckedItems.Count, "#D1");
			Assert.AreSame (itemA, lvw.CheckedItems [0], "#D2");
			Assert.AreSame (itemB, lvw.CheckedItems [1], "#D3");
			Assert.AreSame (itemC, lvw.CheckedItems [2], "#D4");

			// sorting only takes effect when listview is created
			form.Show ();

			Assert.AreEqual (3, lvw.CheckedItems.Count, "#E1");
			Assert.AreSame (itemC, lvw.CheckedItems [0], "#E2");
			Assert.AreSame (itemB, lvw.CheckedItems [1], "#E3");
			Assert.AreSame (itemA, lvw.CheckedItems [2], "#E4");
			form.Dispose ();
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void CheckedItemCollectionTest_PropertiesTest_Remove_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.CheckedItems).Remove (5);
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void CheckedItemCollectionTest_PropertiesTest_RemoveAt_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.CheckedItems).RemoveAt (5);
		}

		/*
			SelectedIndexCollection
		*/
		[Test]
		public void SelectedIndexCollectionTest_PropertiesTest ()
		{
			ListView listview = new ListView ();

			// Properties
#if !NET_2_0
			Assert.AreEqual (true, listview.SelectedIndices.IsReadOnly, "SelectedIndexCollectionTest_PropertiesTest#1");
			Assert.AreEqual (true, ((IList)listview.SelectedIndices).IsFixedSize, "SelectedIndexCollectionTest_PropertiesTest#4");
#else
			Assert.AreEqual (false, listview.SelectedIndices.IsReadOnly, "SelectedIndexCollectionTest_PropertiesTest#1");
			Assert.AreEqual (false, ((IList)listview.SelectedIndices).IsFixedSize, "SelectedIndexCollectionTest_PropertiesTest#4");
#endif
			Assert.AreEqual (false, ((ICollection)listview.SelectedIndices).IsSynchronized, "SelectedIndexCollectionTest_PropertiesTest#2");
			Assert.AreEqual (listview.SelectedIndices, ((ICollection)listview.SelectedIndices).SyncRoot, "SelectedIndexCollectionTest_PropertiesTest#3");
			Assert.AreEqual (0, listview.SelectedIndices.Count, "SelectedIndexCollectionTest_PropertiesTest#5");
		}


		// Exceptions
#if !NET_2_0
		[Test, ExpectedException (typeof (NotSupportedException))]
		public void SelectedIndexCollectionTest_Add_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.SelectedIndices).Add (5);
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void SelectedIndexCollectionTest_Remove_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.SelectedIndices).Remove (5);
		}
#endif

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void SelectedIndexCollectionTest_RemoveAt_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.SelectedIndices).RemoveAt (5);
		}

		/*
			SelectedItemCollection
		*/
		[Test]
		public void SelectedItemCollectionTest_PropertiesTest ()
		{
			ListView listview = new ListView ();

			// Properties
			Assert.AreEqual (true, listview.SelectedItems.IsReadOnly, "SelectedItemCollectionTest_PropertiesTest#1");
			Assert.AreEqual (false, ((ICollection)listview.SelectedItems).IsSynchronized, "SelectedItemCollectionTest_PropertiesTest#2");
			Assert.AreEqual (listview.SelectedItems, ((ICollection)listview.SelectedItems).SyncRoot, "SelectedItemCollectionTest_PropertiesTest#3");
			Assert.AreEqual (true, ((IList)listview.SelectedItems).IsFixedSize, "SelectedItemCollectionTest_PropertiesTest#4");
			Assert.AreEqual (0, listview.SelectedItems.Count, "SelectedItemCollectionTest_PropertiesTest#5");
		}


		// Exceptions
		[Test, ExpectedException (typeof (NotSupportedException))]
		public void SelectedItemCollectionTest_PropertiesTest_Add_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.SelectedItems).Add (5);
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void SelectedItemCollectionTest_PropertiesTest_Remove_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.SelectedItems).Remove (5);
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void SelectedItemCollectionTest_PropertiesTest_RemoveAt_ExceptionTest ()
		{
			ListView listview = new ListView ();
			((IList)listview.SelectedItems).RemoveAt (5);
		}

		[Test]
		public void SelectedItemCollectionTest_Clear ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			ListViewItem item = lvw.Items.Add ("Title");
			item.Selected = true;

			lvw.SelectedItems.Clear ();

			Assert.IsTrue (item.Selected, "#A1");
			Assert.AreEqual (0, lvw.SelectedItems.Count, "#A2");
			Assert.IsFalse (lvw.SelectedItems.Contains (item), "#A3");

			form.Show ();

			Assert.IsTrue (item.Selected, "#B1");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#B2");
			Assert.IsTrue (lvw.SelectedItems.Contains (item), "#B3");

			// once listview is created, clear DOES have effect
			lvw.SelectedItems.Clear ();

			Assert.IsFalse (item.Selected, "#C1");
			Assert.AreEqual (0, lvw.SelectedItems.Count, "#C2");
			Assert.IsFalse (lvw.SelectedItems.Contains (item), "#C3");
			form.Dispose ();
		}

		[Test]
		public void SelectedItemCollectionTest_Contains ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			ListViewItem item = lvw.Items.Add ("Title");
			item.Selected = true;
			IList list = (IList) lvw.SelectedItems;

			Assert.IsFalse (lvw.SelectedItems.Contains (item), "#A1");
			Assert.IsFalse (lvw.SelectedItems.Contains (new ListViewItem ()), "#A2");
			Assert.IsFalse (list.Contains (item), "#A3");
			Assert.IsFalse (list.Contains (new ListViewItem ()), "#A4");

			form.Show ();

			Assert.IsTrue (lvw.SelectedItems.Contains (item), "#B1");
			Assert.IsFalse (lvw.SelectedItems.Contains (new ListViewItem ()), "#B2");
			Assert.IsTrue (list.Contains (item), "#B3");
			Assert.IsFalse (list.Contains (new ListViewItem ()), "#B4");
			form.Dispose ();
		}

		[Test]
		public void SelectedItemCollectionTest_CopyTo ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			ListViewItem item = lvw.Items.Add ("Title");
			item.Selected = true;
			IList list = (IList) lvw.SelectedItems;
			Assert.IsNotNull (list, "#A1");
			ListViewItem [] items = new ListViewItem [1];

			lvw.SelectedItems.CopyTo (items, 0);
			Assert.IsNull (items [0], "#A2");
			lvw.SelectedItems.CopyTo (items, 455);

			form.Show ();

			lvw.SelectedItems.CopyTo (items, 0);
			Assert.AreSame (item, items [0], "#B1");
			try {
				lvw.SelectedItems.CopyTo (items, 455);
				Assert.Fail ("#B2");
			} catch (ArgumentException) {
			}
			form.Dispose ();
		}

		[Test]
		public void SelectedItemCollectionTest_Count ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			ListViewItem item = lvw.Items.Add ("Title");
			item.Selected = true;

			Assert.AreEqual (0, lvw.SelectedItems.Count, "#1");
			form.Show ();
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#2");
			form.Dispose ();
		}

		[Test]
		public void SelectedItemCollectionTest_GetEnumerator ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			ListViewItem item = lvw.Items.Add ("Title");
			item.Selected = true;

			Assert.IsFalse (lvw.SelectedItems.GetEnumerator ().MoveNext (), "#A1");

			form.Show ();

			Assert.IsTrue (lvw.SelectedItems.GetEnumerator ().MoveNext (), "#B1");

			form.Dispose ();
		}

		[Test]
		public void SelectedItemCollectionTest_Indexer ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			ListViewItem item = lvw.Items.Add ("Title");
			item.Selected = true;
			IList list = (IList) lvw.SelectedItems;

			try  {
				ListViewItem x = lvw.SelectedItems [0];
				Assert.Fail ("#A1: " + x.ToString ());
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				ListViewItem x = list [0] as ListViewItem;
				Assert.Fail ("#A2: " + x.ToString ());
			} catch (ArgumentOutOfRangeException) {
			}

			form.Show ();

			Assert.AreSame (item, lvw.SelectedItems [0], "#B1");
			Assert.AreSame (item, list [0], "#B2");

			form.Dispose ();
		}

		[Test]
		public void SelectedItemCollectionTest_IndexOf ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			ListViewItem item = lvw.Items.Add ("Title");
			item.Selected = true;
			IList list = (IList) lvw.SelectedItems;

			Assert.AreEqual (-1, lvw.SelectedItems.IndexOf (item), "#A1");
			Assert.AreEqual (-1, lvw.SelectedItems.IndexOf (new ListViewItem ()), "#A2");
			Assert.AreEqual (-1, list.IndexOf (item), "#A3");
			Assert.AreEqual (-1, list.IndexOf (new ListViewItem ()), "#A4");

			form.Show ();

			Assert.AreEqual (0, lvw.SelectedItems.IndexOf (item), "#B1");
			Assert.AreEqual (-1, lvw.SelectedItems.IndexOf (new ListViewItem ()), "#B2");
			Assert.AreEqual (0, list.IndexOf (item), "#B3");
			Assert.AreEqual (-1, list.IndexOf (new ListViewItem ()), "#B4");

			form.Dispose ();
		}

		[Test]
		public void SelectedItemCollectionTest_Order ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			lvw.MultiSelect = true;
			form.Controls.Add (lvw);
			ListViewItem itemA = lvw.Items.Add ("A");
			itemA.Selected = true;
			ListViewItem itemB = lvw.Items.Add ("B");
			itemB.Selected = true;
			ListViewItem itemC = lvw.Items.Add ("C");
			itemC.Selected = true;

			form.Show ();

			Assert.AreEqual (3, lvw.SelectedItems.Count, "#A1");
			Assert.AreSame (itemA, lvw.SelectedItems [0], "#A2");
			Assert.AreSame (itemB, lvw.SelectedItems [1], "#A3");
			Assert.AreSame (itemC, lvw.SelectedItems [2], "#A3");

			itemB.Selected = false;

			Assert.AreEqual (2, lvw.SelectedItems.Count, "#B1");
			Assert.AreSame (itemA, lvw.SelectedItems [0], "#B2");
			Assert.AreSame (itemC, lvw.SelectedItems [1], "#B3");

			itemB.Selected = true;

			Assert.AreEqual (3, lvw.SelectedItems.Count, "#C1");
			Assert.AreSame (itemA, lvw.SelectedItems [0], "#C2");
			Assert.AreSame (itemB, lvw.SelectedItems [1], "#C3");
			Assert.AreSame (itemC, lvw.SelectedItems [2], "#C4");

			lvw.Sorting = SortOrder.Descending;

			Assert.AreEqual (3, lvw.SelectedItems.Count, "#D1");
			Assert.AreSame (itemC, lvw.SelectedItems [0], "#D2");
			Assert.AreSame (itemB, lvw.SelectedItems [1], "#D3");
			Assert.AreSame (itemA, lvw.SelectedItems [2], "#D4");

			form.Dispose ();
		}

		/*
			ListViewItemCollection
		*/

		[Test]
		public void ListViewItemCollectionTest_Add ()
		{
			ListView lvw = new ListView ();
			ListViewItem item = new ListViewItem ("Title");
			ListViewItem newItem = lvw.Items.Add (item);
			Assert.AreSame (newItem, item, "#A1");
			Assert.AreEqual (0, item.Index, "#A2");
			Assert.AreSame (item, lvw.Items [0], "#A3");
			Assert.AreSame (lvw, item.ListView, "#A4");

			newItem = lvw.Items.Add ("A title");
			Assert.AreEqual ("A title", newItem.Text, "#B1");
			Assert.AreEqual (-1, newItem.ImageIndex, "#B2");
			Assert.AreEqual (1, newItem.Index, "#B3");
			Assert.AreSame (newItem, lvw.Items [1], "#B4");
			Assert.AreSame (lvw, newItem.ListView, "#B5");
			Assert.AreEqual (0, item.Index, "#B6");
			Assert.AreSame (item, lvw.Items [0], "#B7");

			newItem = lvw.Items.Add ("A title", 4);
			Assert.AreEqual ("A title", newItem.Text, "#C1");
			Assert.AreEqual (4, newItem.ImageIndex, "#C2");
			Assert.AreEqual (2, newItem.Index, "#C3");
			Assert.AreSame (newItem, lvw.Items [2], "#C4");
			Assert.AreSame (lvw, newItem.ListView, "#C5");
			Assert.AreEqual (0, item.Index, "#C6");
			Assert.AreSame (item, lvw.Items [0], "#C7");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // An item cannot be added more than once. To add an item again, you need to clone it
		public void ListViewItemCollectionTest_Add_ExistingItem ()
		{
			ListView lvw = new ListView ();
			ListViewItem itemA = lvw.Items.Add ("A");
			lvw.Items.Add (itemA);
		}

		[Test]
		public void ListViewItemCollectionTest_Insert ()
		{
			ListView lvw = new ListView ();
			ListViewItem item = new ListViewItem ("Title");
			ListViewItem newItem = lvw.Items.Insert (0, item);
			Assert.AreSame (newItem, item, "#A1");
			Assert.AreEqual (-1, newItem.ImageIndex, "#A2");
			Assert.AreSame (newItem, lvw.Items [0], "#A3");
			Assert.AreSame (lvw, newItem.ListView, "#A4");

			newItem = lvw.Items.Insert (1, "A title");
			Assert.AreEqual ("A title", newItem.Text, "#B1");
			Assert.AreEqual (-1, newItem.ImageIndex, "#B2");
			Assert.AreEqual (1, newItem.Index, "#B3");
			Assert.AreSame (newItem, lvw.Items [1], "#B4");
			Assert.AreSame (lvw, newItem.ListView, "#B5");
			Assert.AreEqual (0, item.Index, "#B6");
			Assert.AreSame (item, lvw.Items [0], "#B7");

			newItem = lvw.Items.Insert (0, "Other title");
			Assert.AreEqual ("Other title", newItem.Text, "#C1");
			Assert.AreEqual (-1, newItem.ImageIndex, "#C2");
			Assert.AreEqual (0, newItem.Index, "#C3");
			Assert.AreSame (newItem, lvw.Items [0], "#C4");
			Assert.AreSame (lvw, newItem.ListView, "#C5");
			Assert.AreEqual (1, item.Index, "#C6");
			Assert.AreSame (item, lvw.Items [1], "#C7");

			newItem = lvw.Items.Insert (3, "Some title", 4);
			Assert.AreEqual ("Some title", newItem.Text, "#D1");
			Assert.AreEqual (4, newItem.ImageIndex, "#D2");
			Assert.AreEqual (3, newItem.Index, "#D3");
			Assert.AreSame (newItem, lvw.Items [3], "#D4");
			Assert.AreSame (lvw, newItem.ListView, "#D5");
			Assert.AreEqual (1, item.Index, "#D6");
			Assert.AreSame (item, lvw.Items [1], "#D7");

			newItem = lvw.Items.Insert (0, "No title", 4);
			Assert.AreEqual ("No title", newItem.Text, "#E1");
			Assert.AreEqual (4, newItem.ImageIndex, "#E2");
			Assert.AreEqual (0, newItem.Index, "#E3");
			Assert.AreSame (newItem, lvw.Items [0], "#E4");
			Assert.AreSame (lvw, newItem.ListView, "#E5");
			Assert.AreEqual (2, item.Index, "#E6");
			Assert.AreSame (item, lvw.Items [2], "#E7");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // An item cannot be added more than once. To add an item again, you need to clone it
		public void ListViewItemCollectionTest_Insert_ExistingItem ()
		{
			ListView lvw = new ListView ();
			ListViewItem itemA = lvw.Items.Add ("A");
			lvw.Items.Insert (0, itemA);
		}

		[Test]
		public void ListViewItemCollectionTest_Remove ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			lvw.MultiSelect = true;
			lvw.CheckBoxes = true;

			form.Show ();

			ListViewItem itemA = lvw.Items.Add ("A");
			ListViewItem itemB = lvw.Items.Add ("B");
			lvw.Items.Add ("C");
			ListViewItem itemD = lvw.Items.Add ("D");

			Assert.AreEqual (4, lvw.Items.Count, "#A1");
			Assert.AreEqual (0, lvw.SelectedItems.Count, "#A2");
			Assert.AreEqual (0, lvw.CheckedItems.Count, "#A3");

			itemB.Checked = true;
			itemD.Checked = true;

			Assert.AreEqual (4, lvw.Items.Count, "#B1");
			Assert.AreEqual (0, lvw.SelectedItems.Count, "#B2");
			Assert.AreEqual (2, lvw.CheckedItems.Count, "#B3");
			Assert.AreSame (itemB, lvw.CheckedItems [0], "#B4");
			Assert.AreSame (itemD, lvw.CheckedItems [1], "#B5");

			itemD.Selected = true;

			Assert.AreEqual (4, lvw.Items.Count, "#C1");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#C2");
			Assert.AreSame (itemD, lvw.SelectedItems [0], "#C3");
			Assert.AreEqual (2, lvw.CheckedItems.Count, "#C4");
			Assert.AreSame (itemB, lvw.CheckedItems [0], "#C5");
			Assert.AreSame (itemD, lvw.CheckedItems [1], "#C6");

			lvw.Items.Remove (itemB);

			Assert.AreEqual (3, lvw.Items.Count, "#D1");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#D2");
			Assert.AreSame (itemD, lvw.SelectedItems [0], "#D3");
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#D4");
			Assert.AreSame (itemD, lvw.CheckedItems [0], "#D5");

			lvw.Items.Remove (itemA);

			Assert.AreEqual (2, lvw.Items.Count, "#E1");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#E2");
			Assert.AreEqual (itemD, lvw.SelectedItems [0], "#E3");
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#E4");
			Assert.AreEqual (itemD, lvw.CheckedItems [0], "#E5");

			form.Dispose ();
		}

		[Test]
		public void ListViewItemCollectionTest_RemoveAt ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			ListView lvw = new ListView ();
			form.Controls.Add (lvw);
			lvw.MultiSelect = true;
			lvw.CheckBoxes = true;
			lvw.Items.Add ("A");
			lvw.Items.Add ("B");
			lvw.Items.Add ("C");
			lvw.Items.Add ("D");

			form.Show ();

			Assert.AreEqual (4, lvw.Items.Count, "#A1");
			Assert.AreEqual (0, lvw.SelectedItems.Count, "#A2");
			Assert.AreEqual (0, lvw.CheckedItems.Count, "#A3");

			lvw.Items [1].Checked = true;
			lvw.Items [3].Checked = true;

			Assert.AreEqual (4, lvw.Items.Count, "#B1");
			Assert.AreEqual (0, lvw.SelectedItems.Count, "#B2");
			Assert.AreEqual (2, lvw.CheckedItems.Count, "#B3");
			Assert.AreEqual ("B", lvw.CheckedItems [0].Text, "#B4");
			Assert.AreEqual ("D", lvw.CheckedItems [1].Text, "#B5");

			lvw.Items [3].Selected = true;

			Assert.AreEqual (4, lvw.Items.Count, "#C1");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#C2");
			Assert.AreEqual ("D", lvw.SelectedItems [0].Text, "#C3");
			Assert.AreEqual (2, lvw.CheckedItems.Count, "#C4");
			Assert.AreEqual ("B", lvw.CheckedItems [0].Text, "#C5");
			Assert.AreEqual ("D", lvw.CheckedItems [1].Text, "#C6");

			lvw.Items.RemoveAt (1);

			Assert.AreEqual (3, lvw.Items.Count, "#D1");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#D2");
			Assert.AreEqual ("D", lvw.SelectedItems [0].Text, "#D3");
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#D4");
			Assert.AreEqual ("D", lvw.CheckedItems [0].Text, "#D5");

			lvw.Items.RemoveAt (0);

			Assert.AreEqual (2, lvw.Items.Count, "#E1");
			Assert.AreEqual (1, lvw.SelectedItems.Count, "#E2");
			Assert.AreEqual ("D", lvw.SelectedItems [0].Text, "#E3");
			Assert.AreEqual (1, lvw.CheckedItems.Count, "#E4");
			Assert.AreEqual ("D", lvw.CheckedItems [0].Text, "#E5");

			form.Dispose ();
		}

#if NET_2_0
		[Test]
		public void ListViewItemCollectionTest_RemoveByKey ()
		{
			ListView lvw = new ListView ();
			ListViewItem lvi1 = new ListViewItem ("A");
			lvi1.Name = "A name";
			ListViewItem lvi2 = new ListViewItem ("B");
			lvi2.Name = "B name";
			ListViewItem lvi3 = new ListViewItem ("C");
			lvi3.Name = "Same name";
			ListViewItem lvi4 = new ListViewItem ("D");
			lvi4.Name = "Same name";
			ListViewItem lvi5 = new ListViewItem ("E");
			lvi5.Name = String.Empty;
			lvw.Items.AddRange (new ListViewItem [] { lvi1, lvi2, lvi3, lvi4, lvi5 });

			Assert.AreEqual (5, lvw.Items.Count, "#A1");

			lvw.Items.RemoveByKey ("B name");
			Assert.AreEqual (4, lvw.Items.Count, "#B1");
			Assert.AreSame (lvi1, lvw.Items [0], "#B2");
			Assert.AreSame (lvi3, lvw.Items [1], "#B3");
			Assert.AreSame (lvi4, lvw.Items [2], "#B4");
			Assert.AreSame (lvi5, lvw.Items [3], "#B5");

			lvw.Items.RemoveByKey ("Same name");
			Assert.AreEqual (3, lvw.Items.Count, "#C1");
			Assert.AreSame (lvi1, lvw.Items [0], "#C2");
			Assert.AreSame (lvi4, lvw.Items [1], "#C3");
			Assert.AreSame (lvi5, lvw.Items [2], "#C4");

			lvw.Items.RemoveByKey ("a NAME");
			Assert.AreEqual (2, lvw.Items.Count, "#D1");
			Assert.AreSame (lvi4, lvw.Items [0], "#D2");
			Assert.AreSame (lvi5, lvw.Items [1], "#D3");

			lvw.Items.RemoveByKey (String.Empty);
			Assert.AreEqual (2, lvw.Items.Count, "#E1");
			Assert.AreSame (lvi4, lvw.Items [0], "#E2");
			Assert.AreSame (lvi5, lvw.Items [1], "#E3");
		}

		[Test]
		public void ListViewItemCollectionTest_IndexOfKey ()
		{
			ListView lvw = new ListView ();
			ListViewItem lvi1 = new ListViewItem ("A");
			lvi1.Name = "A name";
			ListViewItem lvi2 = new ListViewItem ("B");
			lvi2.Name = "Same name";
			ListViewItem lvi3 = new ListViewItem ("C");
			lvi3.Name = "Same name";
			ListViewItem lvi4 = new ListViewItem ("D");
			lvi4.Name = String.Empty;
			lvw.Items.AddRange (new ListViewItem [] { lvi1, lvi2, lvi3, lvi4 });

			Assert.AreEqual (4, lvw.Items.Count, "#A1");
			Assert.AreEqual (-1, lvw.Items.IndexOfKey (String.Empty), "#A2");
			Assert.AreEqual (-1, lvw.Items.IndexOfKey (null), "#A3");
			Assert.AreEqual (0, lvw.Items.IndexOfKey ("A name"), "#A4");
			Assert.AreEqual (0, lvw.Items.IndexOfKey ("a NAME"), "#A5");
			Assert.AreEqual (1, lvw.Items.IndexOfKey ("Same name"), "#A6");

			ListViewItem lvi5 = new ListViewItem ("E");
			lvw.Items.Add (lvi5);
			lvi5.Name = "E name";

			Assert.AreEqual (4, lvw.Items.IndexOfKey ("E name"), "#B1");
		}

		[Test]
		public void ListViewItemCollectionTest_Indexer ()
		{
			ListView lvw = new ListView ();
			ListViewItem lvi1 = new ListViewItem ("A");
			lvi1.Name = "A name";
			ListViewItem lvi2 = new ListViewItem ("B");
			lvi2.Name = "Same name";
			ListViewItem lvi3 = new ListViewItem ("C");
			lvi3.Name = "Same name";
			ListViewItem lvi4 = new ListViewItem ("D");
			lvi4.Name = String.Empty;
			lvw.Items.AddRange (new ListViewItem [] { lvi1, lvi2, lvi3, lvi4 });

			Assert.AreEqual (4, lvw.Items.Count, "#A1");
			Assert.AreEqual (null, lvw.Items [String.Empty], "#A2");
			Assert.AreEqual (null, lvw.Items [null], "#A3");
			Assert.AreSame (lvi1, lvw.Items ["A name"], "#A4");
			Assert.AreSame (lvi1, lvw.Items ["a NAME"], "#A5");
			Assert.AreSame (lvi2, lvw.Items ["Same name"], "#A6");

			ListViewItem lvi5 = new ListViewItem ("E");
			lvw.Items.Add (lvi5);
			lvi5.Name = "E name";

			Assert.AreSame (lvi5, lvw.Items ["E name"], "#B1");
		}

		[Test]
		public void ListViewItemCollectionTest_ContainsKey ()
		{
			ListView lvw = new ListView();
			ListViewItem lvi1 = new ListViewItem("A");
			lvi1.Name = "A name";
			ListViewItem lvi2 = new ListViewItem("B");
			lvi2.Name = "B name";
			ListViewItem lvi3 = new ListViewItem("D");
			lvi3.Name = String.Empty;
			lvw.Items.AddRange(new ListViewItem[] { lvi1, lvi2, lvi3 });

			Assert.AreEqual(3, lvw.Items.Count, "#A1");
			Assert.AreEqual(false, lvw.Items.ContainsKey (String.Empty), "#A2");
			Assert.AreEqual(false, lvw.Items.ContainsKey (null), "#A3");
			Assert.AreEqual(true, lvw.Items.ContainsKey ("A name"), "#A4");
			Assert.AreEqual(true, lvw.Items.ContainsKey ("a NAME"), "#A5");
			Assert.AreEqual(true, lvw.Items.ContainsKey ("B name"), "#A6");

			ListViewItem lvi5 = new ListViewItem("E");
			lvw.Items.Add(lvi5);
			lvi5.Name = "E name";

			Assert.AreEqual(true, lvw.Items.ContainsKey ("E name"), "#B1");
		}

		[Test]
		public void ListViewItemCollectionTest_Find ()
		{
			ListView lvw = new ListView ();
			ListViewItem lvi1 = new ListViewItem ("A");
			lvi1.Name = "A name";
			ListViewItem lvi2 = new ListViewItem ("B");
			lvi2.Name = "a NAME";
			ListViewItem lvi3 = new ListViewItem ("C");
			lvi3.Name = "a NAME";
			ListViewItem lvi4 = new ListViewItem ("D");
			lvi4.Name = String.Empty;
			ListViewItem lvi5 = new ListViewItem ("F");
			lvi5.Name = String.Empty;
			lvw.Items.AddRange (new ListViewItem [] { lvi1, lvi2, lvi3, lvi4, lvi5 });

			Assert.AreEqual (5, lvw.Items.Count, "#A1");

			ListViewItem [] items = lvw.Items.Find ("A name", false);
			Assert.AreEqual (3, items.Length, "#B11");
			Assert.AreSame (lvi1, items [0], "#B2");
			Assert.AreSame (lvi2, items [1], "#B3");
			Assert.AreSame (lvi3, items [2], "#B4");

			items = lvw.Items.Find (String.Empty, false);
			Assert.AreEqual (2, items.Length, "#B1");
			Assert.AreSame (lvi4, items [0], "#B2");
			Assert.AreSame (lvi5, items [1], "#B3");

			Assert.AreEqual (0, lvw.Items.Find (null, false).Length, "#C1");
		}

#endif

	}
}
