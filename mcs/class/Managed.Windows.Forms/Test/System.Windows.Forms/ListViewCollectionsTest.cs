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
			ColumnHeader colA = new ColumnHeader ();
			ColumnHeader colB = new ColumnHeader ();

			// Duplicated elements with same text added
			listview.Columns.Add (colA);
			listview.Columns.Add (colB);
			Assert.AreEqual (2, listview.Columns.Count, "#1");
			Assert.AreEqual ("ColumnHeader", listview.Columns[0].Text, "#2");
			Assert.AreSame (listview, colA.ListView, "#3");
			Assert.AreSame (listview, colB.ListView, "#4");
		}

		[Test]
		public void ColumnHeaderCollectionTest_ClearTest ()
		{
			ListView listview = new ListView ();
			ColumnHeader colA = new ColumnHeader ();
			ColumnHeader colB = new ColumnHeader ();
			listview.Columns.Add (colA);
			listview.Columns.Add (colB);
			listview.Columns.Clear ();
			Assert.AreEqual (0, listview.Columns.Count, "#1");
			Assert.IsNull (colA.ListView, "#2");
			Assert.IsNull (colB.ListView, "#3");
		}

		[Test]
		public void ColumnHeaderCollectionTest_Remove ()
		{
			ListView listview = new ListView ();
			ColumnHeader colA = new ColumnHeader ();
			ColumnHeader colB = new ColumnHeader ();
			ColumnHeader colC = new ColumnHeader ();
			listview.Columns.Add (colA);
			listview.Columns.Add (colB);
			listview.Columns.Add (colC);

			listview.Columns.Remove (colB);
			Assert.AreEqual (2, listview.Columns.Count, "#A1");
			Assert.AreSame (colA, listview.Columns [0], "#A2");
			Assert.AreSame (colC, listview.Columns [1], "#A3");
			Assert.AreSame (listview, colA.ListView, "#A4");
			Assert.IsNull (colB.ListView, "#A5");
			Assert.AreSame (listview, colC.ListView, "#A6");

			listview.Columns.Remove (colC);
			Assert.AreEqual (1, listview.Columns.Count, "#B1");
			Assert.AreSame (colA, listview.Columns [0], "#B2");
			Assert.AreSame (listview, colA.ListView, "#B3");
			Assert.IsNull (colB.ListView, "#B4");
			Assert.IsNull (colC.ListView, "#B5");

			listview.Columns.Remove (colA);
			Assert.AreEqual (0, listview.Columns.Count, "#C1");
			Assert.IsNull (colA.ListView, "#C2");
			Assert.IsNull (colB.ListView, "#C3");
			Assert.IsNull (colC.ListView, "#C4");
		}

		[Test]
		public void ColumnHeaderCollectionTest_RemoveAt ()
		{
			ListView listview = new ListView ();
			ColumnHeader colA = new ColumnHeader ();
			ColumnHeader colB = new ColumnHeader ();
			ColumnHeader colC = new ColumnHeader ();
			listview.Columns.Add (colA);
			listview.Columns.Add (colB);
			listview.Columns.Add (colC);

			listview.Columns.RemoveAt (1);
			Assert.AreEqual (2, listview.Columns.Count, "#A1");
			Assert.AreSame (colA, listview.Columns [0], "#A2");
			Assert.AreSame (colC, listview.Columns [1], "#A3");
			Assert.AreSame (listview, colA.ListView, "#A4");
			Assert.IsNull (colB.ListView, "#A5");
			Assert.AreSame (listview, colC.ListView, "#A6");

			listview.Columns.RemoveAt (0);
			Assert.AreEqual (1, listview.Columns.Count, "#B1");
			Assert.AreSame (colC, listview.Columns [0], "#B2");
			Assert.IsNull (colA.ListView, "#B3");
			Assert.IsNull (colB.ListView, "#B4");
			Assert.AreSame (listview, colC.ListView, "#B5");

			listview.Columns.RemoveAt (0);
			Assert.AreEqual (0, listview.Columns.Count, "#C1");
			Assert.IsNull (colA.ListView, "#C2");
			Assert.IsNull (colB.ListView, "#C3");
			Assert.IsNull (colC.ListView, "#C4");
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

#if NET_2_0
		[Test]
		public void SelectedIndexCollectionTest_AddTest ()
		{
			ListView listview = new ListView ();
			listview.Items.Add ("A");

			int n = listview.SelectedIndices.Add (0);
			Assert.AreEqual (0, n, "SelectedIndexCollectionTest_AddTest#1");
			Assert.AreEqual (true, listview.Items [0].Selected, "SelectedIndexCollectionTest_AddTest#2");

			// Force to create the handle
			listview.CreateControl ();
			Assert.AreEqual (1, listview.SelectedIndices.Count, "SelectedIndexCollectionTest_AddTest#4");

			n = listview.SelectedIndices.Add (0);
			Assert.AreEqual (1, n, "SelectedIndexCollectionTest_AddTest#5");
			Assert.AreEqual (1, listview.SelectedIndices.Count, "SelectedIndexCollectionTest_AddTest#6");
			Assert.AreEqual (true, listview.Items [0].Selected, "SelectedIndexCollectionTest_AddTest#7");
		}

		[Test]
		public void SelectedIndexCollectionTest_ClearTest ()
		{
			ListView listview = new ListView ();
			listview.Items.Add ("A");
			listview.Items.Add ("B");
			listview.Items.Add ("C");

			listview.SelectedIndices.Add (0);
			listview.SelectedIndices.Add (2);

			// Nothing if handle hasn't been created
			listview.SelectedIndices.Clear (); 
			Assert.AreEqual (true, listview.Items [0].Selected, "SelectedIndexCollectionTest_ClearTest#2");
			Assert.AreEqual (false, listview.Items [1].Selected, "SelectedIndexCollectionTest_ClearTest#3");
			Assert.AreEqual (true, listview.Items [2].Selected, "SelectedIndexCollectionTest_ClearTest#4");

			// Force to create the handle
			listview.CreateControl ();

			listview.SelectedIndices.Add (0);
			listview.SelectedIndices.Add (2);

			listview.SelectedIndices.Clear ();
			Assert.AreEqual (0, listview.SelectedIndices.Count, "SelectedIndexCollectionTest_ClearTest#5");
			Assert.AreEqual (false, listview.Items [0].Selected, "SelectedIndexCollectionTest_ClearTest#6");
			Assert.AreEqual (false, listview.Items [1].Selected, "SelectedIndexCollectionTest_ClearTest#7");
			Assert.AreEqual (false, listview.Items [2].Selected, "SelectedIndexCollectionTest_ClearTest#8");
		}

		[Test]
		public void SelectedIndexCollectionTest_RemoveTest ()
		{
			ListView listview = new ListView ();
			listview.Items.Add ("A");

			listview.SelectedIndices.Add (0);
			listview.SelectedIndices.Remove (0);
			Assert.AreEqual (0, listview.SelectedIndices.Count, "SelectedIndexCollectionTest_RemoveTest#1");
			Assert.AreEqual (false, listview.Items [0].Selected, "SelectedIndexCollectionTest_RemoveTest#2");

			// Force to create the handle
			listview.CreateControl ();

			listview.SelectedIndices.Add (0);
			listview.SelectedIndices.Remove (0);
			Assert.AreEqual (0, listview.SelectedIndices.Count, "SelectedIndexCollectionTest_RemoveTest#3");
			Assert.AreEqual (false, listview.Items [0].Selected, "SelectedIndexCollectionTest_RemoveTest#4");
		}
#endif

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

#if NET_2_0
		[Test]
		public void SelectedIndexCollectionTest_Remove_ExceptionTest ()
		{
			ListView listview = new ListView ();
			try {
				listview.SelectedIndices.Remove (-1);
				Assert.Fail ("SelectedIndexCollectionTest_Remove_ExceptionTest#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				listview.SelectedIndices.Remove (listview.Items.Count);
				Assert.Fail ("SelectedIndexCollectionTest_Remove_ExceptionTest#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void SelectedIndexCollectionTest_Add_ExceptionTest ()
		{
			ListView listview = new ListView ();
			try {
				listview.SelectedIndices.Add (-1);
				Assert.Fail ("SelectedIndexCollectionTest_Add_ExceptionTest#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				listview.SelectedIndices.Add (listview.Items.Count);
				Assert.Fail ("SelectedIndexCollectionTest_Add_ExceptionTest#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}
#endif

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

#if NET_2_0
		[Test]
		public void SelectedItemCollectionTest_IndexOfKey ()
		{
			ListView lvw = new ListView ();
			ListViewItem lvi1 = new ListViewItem ("A");
			lvi1.Name = "A name";
			lvi1.Selected = true;
			ListViewItem lvi2 = new ListViewItem ("B");
			lvi2.Name = "Same name";
			lvi2.Selected = false;
			ListViewItem lvi3 = new ListViewItem ("C");
			lvi3.Name = "Same name";
			lvi3.Selected = true;
			ListViewItem lvi4 = new ListViewItem ("D");
			lvi4.Name = String.Empty;
			lvi4.Selected = true;
			ListViewItem lvi5 = new ListViewItem ("E");
			lvi5.Name = "E name";
			lvi5.Selected = false;
			lvw.Items.AddRange (new ListViewItem [] { lvi1, lvi2, lvi3, lvi4, lvi5 });

			// Force to create the control
			lvw.CreateControl ();

			Assert.AreEqual (3, lvw.SelectedItems.Count, "#A1");
			Assert.AreEqual (-1, lvw.SelectedItems.IndexOfKey (String.Empty), "#A2");
			Assert.AreEqual (-1, lvw.SelectedItems.IndexOfKey (null), "#A3");
			Assert.AreEqual (0, lvw.SelectedItems.IndexOfKey ("A name"), "#A4");
			Assert.AreEqual (0, lvw.SelectedItems.IndexOfKey ("a NAME"), "#A5");
			Assert.AreEqual (1, lvw.SelectedItems.IndexOfKey ("Same name"), "#A6");
			Assert.AreEqual (-1, lvw.SelectedItems.IndexOfKey ("E name"), "#A7");

			ListViewItem lvi6 = new ListViewItem ("F");
			lvw.Items.Add (lvi6);
			lvi6.Selected = true;
			lvi6.Name = "F name";

			Assert.AreEqual (4, lvw.SelectedItems.Count, "#B1");
			Assert.AreEqual (3, lvw.SelectedItems.IndexOfKey ("F name"), "#B2");
		}

		[Test]
		public void SelectedItemCollectionTest_Indexer2 ()
		{
			ListView lvw = new ListView ();
			ListViewItem lvi1 = new ListViewItem ("A");
			lvi1.Name = "A name";
			lvi1.Selected = true;
			ListViewItem lvi2 = new ListViewItem ("B");
			lvi2.Name = "Same name";
			lvi2.Selected = false;
			ListViewItem lvi3 = new ListViewItem ("C");
			lvi3.Name = "Same name";
			lvi3.Selected = true;
			ListViewItem lvi4 = new ListViewItem ("D");
			lvi4.Name = String.Empty;
			lvi4.Selected = true;
			ListViewItem lvi5 = new ListViewItem ("E");
			lvi5.Name = "E name";
			lvi5.Selected = false;
			lvw.Items.AddRange (new ListViewItem [] { lvi1, lvi2, lvi3, lvi4, lvi5 });

			// Force to create the control
			lvw.CreateControl ();

			Assert.AreEqual (3, lvw.SelectedItems.Count, "#A1");
			Assert.AreEqual (null, lvw.SelectedItems [String.Empty], "#A2");
			Assert.AreEqual (null, lvw.SelectedItems [null], "#A3");
			Assert.AreEqual (lvi1, lvw.SelectedItems ["A name"], "#A4");
			Assert.AreEqual (lvi1, lvw.SelectedItems ["a NAME"], "#A5");
			Assert.AreEqual (lvi3, lvw.SelectedItems ["Same name"], "#A6");
			Assert.AreEqual (null, lvw.SelectedItems ["E name"], "#A7");

			ListViewItem lvi6 = new ListViewItem ("F");
			lvw.Items.Add (lvi6);
			lvi6.Selected = true;
			lvi6.Name = "F name";

			Assert.AreEqual (4, lvw.SelectedItems.Count, "#B1");
			Assert.AreEqual (lvi6, lvw.SelectedItems ["F name"], "#B2");
		}
#endif

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
		[ExpectedException (typeof (ArgumentException))] // An item cannot be added to more than one ListView. To add an item again, you need to clone it
		public void ListViewItemCollectionTest_Add_OwnedItem ()
		{
			ListView lv1 = new ListView ();
			ListView lv2 = new ListView ();
			ListViewItem item = lv1.Items.Add ("A");
			lv2.Items.Add (item);
		}

		[Test]
		public void ListViewItemCollectionTest_Add_Junk ()
		{
			ListView lv1 = new ListView ();

			ListViewItem item4 = lv1.Items.Add("Item4", 4);
			Assert.AreEqual(item4, lv1.Items[0], "#D1");
#if NET_2_0
			Assert.AreEqual(string.Empty, lv1.Items[0].Name, "#D2");
#endif
			Assert.AreEqual("Item4", lv1.Items[0].Text, "#D3");
			Assert.AreEqual(4, lv1.Items[0].ImageIndex, "#D4");

			string text = null;
			ListViewItem item5 = lv1.Items.Add(text);
			Assert.AreEqual(item5, lv1.Items[1], "#E1");
#if NET_2_0
			Assert.AreEqual(string.Empty, lv1.Items[1].Name, "#E2");
#endif
			Assert.AreEqual(string.Empty, lv1.Items[1].Text, "#E3");

			ListViewItem item6 = lv1.Items.Add(null, 5);
			Assert.AreEqual(item6, lv1.Items[2], "#F1");
#if NET_2_0
			Assert.AreEqual(string.Empty, lv1.Items[2].Name, "#F2");
#endif
			Assert.AreEqual(string.Empty, lv1.Items[2].Text, "#F3");
			Assert.AreEqual(5, lv1.Items[2].ImageIndex, "#F4");
#if NET_2_0
			ListViewItem item1 = lv1.Items.Add("ItemKey1", "Item1", 1);
			Assert.AreEqual(item1, lv1.Items[3], "#A1");
			Assert.AreEqual("ItemKey1", lv1.Items[3].Name, "#A2");
			Assert.AreEqual("Item1", lv1.Items[3].Text, "#A3");
			Assert.AreEqual(1, lv1.Items[3].ImageIndex, "#A4");

			ListViewItem item2 = lv1.Items.Add("ItemKey2", "Item2", "Image2");
			Assert.AreEqual(item2, lv1.Items[4], "#B1");
			Assert.AreEqual("ItemKey2", lv1.Items[4].Name, "#B2");
			Assert.AreEqual("Item2", lv1.Items[4].Text, "#B3");
			Assert.AreEqual("Image2", lv1.Items[4].ImageKey, "#B4");

			ListViewItem item3 = lv1.Items.Add("Item3", "Image3");
			Assert.AreEqual(item3, lv1.Items[5], "#C1");
			Assert.AreEqual(string.Empty, lv1.Items[5].Name, "#C2");
			Assert.AreEqual("Item3", lv1.Items[5].Text, "#C3");
			Assert.AreEqual("Image3", lv1.Items[5].ImageKey, "#C4");

			ListViewItem item7 = lv1.Items.Add(null, "Item6", 6);
			Assert.AreEqual(item7, lv1.Items[6], "#G1");
			Assert.AreEqual(string.Empty, lv1.Items[6].Name, "#G2");
			Assert.AreEqual("Item6", lv1.Items[6].Text, "#G3");
			Assert.AreEqual(6, lv1.Items[6].ImageIndex, "#G4");

			ListViewItem item8 = lv1.Items.Add("ItemKey7", null, 7);
			Assert.AreEqual(item8, lv1.Items[7], "#H1");
			Assert.AreEqual("ItemKey7", lv1.Items[7].Name, "#H2");
			Assert.AreEqual(string.Empty, lv1.Items[7].Text, "#H3");
			Assert.AreEqual(7, lv1.Items[7].ImageIndex, "#H4");

			ListViewItem item9 = lv1.Items.Add("ItemKey8", "Item8", null);
			Assert.AreEqual(item9, lv1.Items[8], "#I1");
			Assert.AreEqual("ItemKey8", lv1.Items[8].Name, "#I2");
			Assert.AreEqual("Item8", lv1.Items[8].Text, "#I3");
			Assert.AreEqual(string.Empty, lv1.Items[8].ImageKey, "#I4");
#endif
		}

		[Test]
		public void ListViewItemCollectionTest_AddRange ()
		{
			ListView lv1 = new ListView ();
			ListViewItem item1 = new ListViewItem ("Item1");
			ListViewItem item2 = new ListViewItem ("Item2");
			ListViewItem item3 = new ListViewItem ("Item3");
			lv1.Items.AddRange (new ListViewItem[] { item1, item2, item3 });

			Assert.AreSame (item1, lv1.Items[0], "#A1");
			Assert.AreEqual (0, item1.Index, "#A2");
			Assert.AreSame (lv1, item1.ListView, "#A3");

			Assert.AreSame (item2, lv1.Items[1], "#B1");
			Assert.AreEqual (1, item2.Index, "#B2");
			Assert.AreSame (lv1, item2.ListView, "#B3");

			Assert.AreSame (item3, lv1.Items[2], "#C1");
			Assert.AreEqual (2, item3.Index, "#C2");
			Assert.AreSame (lv1, item3.ListView, "#C3");
		}

		[Test]
		public void ListViewItemCollectionTest_AddRange_Count ()
		{
			ListView lv1 = new ListView ();
			ListViewItem item1 = new ListViewItem ("Item1");
			ListViewItem item2 = new ListViewItem ("Item2");
			ListViewItem item3 = new ListViewItem ("Item3");

			lv1.Items.Add ("Item4");
			Assert.AreEqual (1, lv1.Items.Count, "#A1");
			lv1.Items.AddRange (new ListViewItem[] { item1, item2, item3 });
			Assert.AreEqual (4, lv1.Items.Count, "#A1");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ListViewItemCollectionTest_AddRange_NullException ()
		{
			ListView lv1 = new ListView ();
			ListViewItem[] value = null;
			lv1.Items.AddRange (value);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))] // An item cannot be added to more than one ListView. To add an item again, you need to clone it
		public void ListViewItemCollectionTest_AddRange_OwnedItem ()
		{
			//MSDN told us, we can use this method to reuse items from a different ListView control. That is not true.
			ListView lv1 = new ListView ();
			ListView lv2 = new ListView ();
			ListViewItem a = lv1.Items.Add ("Item1");
			ListViewItem b = lv1.Items.Add ("Item2");

			lv2.Items.AddRange (new ListViewItem[] { a, b });
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))] // An item cannot be added more than once. To add an item again, you need to clone it
		public void ListViewItemCollectionTest_AddRange_ExistingItem ()
		{
			ListView lv1 = new ListView ();
			ListViewItem item1 = lv1.Items.Add ("Item1");
			lv1.Items.Add (item1);
		}

		[Test]
		public void ListViewItemCollectionTest_Clear ()
		{
			ListView lvw = new ListView ();
			ListViewItem itemA = lvw.Items.Add ("A");
			ListViewItem itemB = lvw.Items.Add ("B");

			Assert.AreEqual (2, lvw.Items.Count, "#A1");
			Assert.AreSame (lvw, itemA.ListView, "#A2");
			Assert.AreSame (lvw, itemB.ListView, "#A3");

			lvw.Items.Clear ();

			Assert.AreEqual (0, lvw.Items.Count, "#B1");
			Assert.IsNull (itemA.ListView, "#B2");
			Assert.IsNull (itemB.ListView, "#B3");
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
		[ExpectedException (typeof (ArgumentException))] // An item cannot be added to more than one ListView. To add an item again, you need to clone it
		public void ListViewItemCollectionTest_Insert_OwnedItem ()
		{
			ListView lv1 = new ListView ();
			ListView lv2 = new ListView ();
			ListViewItem item = lv1.Items.Add ("A");
			lv2.Items.Insert (0, item);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))] // An item cannot be added to more than one ListView. To add an item again, you need to clone it
		public void ListViewItemCollectionTest_Indexer_OwnedItem ()
		{
			ListView lv1 = new ListView ();
			ListView lv2 = new ListView ();
			ListViewItem item = lv1.Items.Add ("A");

			lv2.Items.Add ("Dummy");
			lv2.Items[0] = item;
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

			Assert.IsNull (itemA.ListView, "#F1");
			Assert.IsNull (itemB.ListView, "#F2");
			Assert.AreSame (lvw, itemD.ListView, "#F3");

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

			ListViewItem itemA = lvw.Items.Add ("A");
			ListViewItem itemB = lvw.Items.Add ("B");
			ListViewItem itemC = lvw.Items.Add ("C");
			ListViewItem itemD = lvw.Items.Add ("D");

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

			Assert.IsNull (itemA.ListView, "#F1");
			Assert.IsNull (itemB.ListView, "#F2");
			Assert.AreSame (lvw, itemC.ListView, "#F3");
			Assert.AreSame (lvw, itemD.ListView, "#F4");

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

			Assert.IsNull (lvi1.ListView, "#F1");
			Assert.IsNull (lvi2.ListView, "#F2");
			Assert.IsNull (lvi3.ListView, "#F3");
			Assert.AreSame (lvw, lvi4.ListView, "#F4");
			Assert.AreSame (lvw, lvi5.ListView, "#F5");
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

		[Test]
		public void ListViewSubItemCollectionTest_ContainsKey ()
		{
			ListViewItem lvi = new ListViewItem ("A");
			ListViewItem.ListViewSubItem si1 = new ListViewItem.ListViewSubItem ();
			si1.Name = "A name";
			ListViewItem.ListViewSubItem si2 = new ListViewItem.ListViewSubItem ();
			si2.Name = "B name";
			ListViewItem.ListViewSubItem si3 = new ListViewItem.ListViewSubItem ();
			si3.Name = String.Empty;
			lvi.SubItems.AddRange (new ListViewItem.ListViewSubItem [] { si1, si2, si3 });

			Assert.AreEqual (4, lvi.SubItems.Count, "#A1");
			Assert.AreEqual (false, lvi.SubItems.ContainsKey (String.Empty), "#A2");
			Assert.AreEqual (false, lvi.SubItems.ContainsKey (null), "#A3");
			Assert.AreEqual (true, lvi.SubItems.ContainsKey ("A name"), "#A4");
			Assert.AreEqual (true, lvi.SubItems.ContainsKey ("a NAME"), "#A5");
			Assert.AreEqual (true, lvi.SubItems.ContainsKey ("B name"), "#A6");

			ListViewItem.ListViewSubItem si5 = new ListViewItem.ListViewSubItem ();
			lvi.SubItems.Add (si5);
			si5.Name = "E name";

			Assert.AreEqual (true, lvi.SubItems.ContainsKey ("E name"), "#B1");
		}

		[Test]
		public void ListViewSubItemCollectionTest_IndexOfKey ()
		{
			ListViewItem lvi = new ListViewItem ();
			ListViewItem.ListViewSubItem si1 = new ListViewItem.ListViewSubItem ();
			si1.Name = "A name";
			ListViewItem.ListViewSubItem si2 = new ListViewItem.ListViewSubItem ();
			si2.Name = "Same name";
			ListViewItem.ListViewSubItem si3 = new ListViewItem.ListViewSubItem ();
			si3.Name = "Same name";
			ListViewItem.ListViewSubItem si4 = new ListViewItem.ListViewSubItem ();
			si4.Name = String.Empty;
			lvi.SubItems.AddRange (new ListViewItem.ListViewSubItem [] { si1, si2, si3, si4 });

			Assert.AreEqual (5, lvi.SubItems.Count, "#A1");
			Assert.AreEqual (-1, lvi.SubItems.IndexOfKey (String.Empty), "#A2");
			Assert.AreEqual (-1, lvi.SubItems.IndexOfKey (null), "#A3");
			Assert.AreEqual (1, lvi.SubItems.IndexOfKey ("A name"), "#A4");
			Assert.AreEqual (1, lvi.SubItems.IndexOfKey ("a NAME"), "#A5");
			Assert.AreEqual (2, lvi.SubItems.IndexOfKey ("Same name"), "#A6");

			ListViewItem.ListViewSubItem si5 = new ListViewItem.ListViewSubItem ();
			lvi.SubItems.Add (si5);
			si5.Name = "E name";

			Assert.AreEqual (5, lvi.SubItems.IndexOfKey ("E name"), "#B1");
		}

		[Test]
		public void ListViewSubItemCollectionTest_RemoveByKey ()
		{
			ListViewItem lvi = new ListViewItem ();
			ListViewItem.ListViewSubItem si1 = new ListViewItem.ListViewSubItem ();
			si1.Name = "A name";
			ListViewItem.ListViewSubItem si2 = new ListViewItem.ListViewSubItem ();
			si2.Name = "B name";
			ListViewItem.ListViewSubItem si3 = new ListViewItem.ListViewSubItem ();
			si3.Name = "Same name";
			ListViewItem.ListViewSubItem si4 = new ListViewItem.ListViewSubItem ();
			si4.Name = "Same name";
			ListViewItem.ListViewSubItem si5 = new ListViewItem.ListViewSubItem ();
			si5.Name = String.Empty;
			lvi.SubItems.AddRange (new ListViewItem.ListViewSubItem [] { si1, si2, si3, si4, si5 });

			Assert.AreEqual (6, lvi.SubItems.Count, "#A1");

			lvi.SubItems.RemoveByKey ("B name");
			Assert.AreEqual (5, lvi.SubItems.Count, "#B1");
			Assert.AreSame (si1, lvi.SubItems [1], "#B2");
			Assert.AreSame (si3, lvi.SubItems [2], "#B3");
			Assert.AreSame (si4, lvi.SubItems [3], "#B4");
			Assert.AreSame (si5, lvi.SubItems [4], "#B5");

			lvi.SubItems.RemoveByKey ("Same name");
			Assert.AreEqual (4, lvi.SubItems.Count, "#C1");
			Assert.AreSame (si1, lvi.SubItems [1], "#C2");
			Assert.AreSame (si4, lvi.SubItems [2], "#C3");
			Assert.AreSame (si5, lvi.SubItems [3], "#C4");

			lvi.SubItems.RemoveByKey ("a NAME");
			Assert.AreEqual (3, lvi.SubItems.Count, "#D1");
			Assert.AreSame (si4, lvi.SubItems [1], "#D2");
			Assert.AreSame (si5, lvi.SubItems [2], "#D3");

			lvi.SubItems.RemoveByKey (String.Empty);
			Assert.AreEqual (3, lvi.SubItems.Count, "#E1");
			Assert.AreSame (si4, lvi.SubItems [1], "#E2");
			Assert.AreSame (si5, lvi.SubItems [2], "#E3");
		}

		[Test]
		public void ListViewSubItemCollectionTest_Indexer ()
		{
			ListViewItem lvi = new ListViewItem ();
			ListViewItem.ListViewSubItem si1 = new ListViewItem.ListViewSubItem ();
			si1.Name = "A name";
			ListViewItem.ListViewSubItem si2 = new ListViewItem.ListViewSubItem ();
			si2.Name = "Same name";
			ListViewItem.ListViewSubItem si3 = new ListViewItem.ListViewSubItem ();
			si3.Name = "Same name";
			ListViewItem.ListViewSubItem si4 = new ListViewItem.ListViewSubItem ();
			si4.Name = String.Empty;
			lvi.SubItems.AddRange (new ListViewItem.ListViewSubItem [] { si1, si2, si3, si4 });

			Assert.AreEqual (5, lvi.SubItems.Count, "#A1");
			Assert.AreEqual (null, lvi.SubItems [String.Empty], "#A2");
			Assert.AreEqual (null, lvi.SubItems [null], "#A3");
			Assert.AreEqual (si1, lvi.SubItems ["A name"], "#A4");
			Assert.AreEqual (si1, lvi.SubItems ["a NAME"], "#A5");
			Assert.AreEqual (si2, lvi.SubItems ["Same name"], "#A6");

			ListViewItem.ListViewSubItem si5 = new ListViewItem.ListViewSubItem ();
			lvi.SubItems.Add (si5);
			si5.Name = "E name";

			Assert.AreEqual (si5, lvi.SubItems ["E name"], "#B1");
		}

#endif

	}
}
