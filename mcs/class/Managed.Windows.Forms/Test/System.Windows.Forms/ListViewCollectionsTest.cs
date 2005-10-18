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
			Assert.AreEqual (true, listview.SelectedIndices.IsReadOnly, "SelectedIndexCollectionTest_PropertiesTest#1");
			Assert.AreEqual (false, ((ICollection)listview.SelectedIndices).IsSynchronized, "SelectedIndexCollectionTest_PropertiesTest#2");
			Assert.AreEqual (listview.SelectedIndices, ((ICollection)listview.SelectedIndices).SyncRoot, "SelectedIndexCollectionTest_PropertiesTest#3");
			Assert.AreEqual (true, ((IList)listview.SelectedIndices).IsFixedSize, "SelectedIndexCollectionTest_PropertiesTest#4");
			Assert.AreEqual (0, listview.SelectedIndices.Count, "SelectedIndexCollectionTest_PropertiesTest#5");
		}


		// Exceptions
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
	}
}
