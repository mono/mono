//
// ComboBoxTest.cs: Test cases for ComboBox.
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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//   	Ritvik Mayank <mritvik@novell.com>
//	Jordi Mas i Hernandez <jordi@ximian.com>
//


using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;
using System.Collections;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListBoxTest
	{
		[Test]
		public void ListBoxPropertyTest ()
		{
			ListBox lb1 = new ListBox ();
			Assert.AreEqual (0, lb1.ColumnWidth, "#1");
			Assert.AreEqual (DrawMode.Normal, lb1.DrawMode, "#2");
			Assert.AreEqual (0, lb1.HorizontalExtent, "#3");
			Assert.AreEqual (false, lb1.HorizontalScrollbar, "#4");
			Assert.AreEqual (true, lb1.IntegralHeight, "#5");
			//Assert.AreEqual (13, lb1.ItemHeight, "#6"); // Note: Item height depends on the current font.
			lb1.Items.Add ("a");
			lb1.Items.Add ("b");
			lb1.Items.Add ("c");
			Assert.AreEqual (3,  lb1.Items.Count, "#7");
			Assert.AreEqual (false, lb1.MultiColumn, "#8");
			//Assert.AreEqual (46, lb1.PreferredHeight, "#9"); // Note: Item height depends on the current font.
			//Assert.AreEqual (RightToLeft.No , lb1.RightToLeft, "#10"); // Depends on Windows version
			Assert.AreEqual (false, lb1.ScrollAlwaysVisible, "#11");
			Assert.AreEqual (-1, lb1.SelectedIndex, "#12");
			lb1.SetSelected (2,true);
			Assert.AreEqual (2, lb1.SelectedIndices[0], "#13");
			Assert.AreEqual ("c", lb1.SelectedItem, "#14");
			Assert.AreEqual ("c", lb1.SelectedItems[0], "#15");
			Assert.AreEqual (SelectionMode.One, lb1.SelectionMode, "#16");
			lb1.SetSelected (2,false);
			Assert.AreEqual (false, lb1.Sorted, "#17");
			Assert.AreEqual ("", lb1.Text, "#18");
			Assert.AreEqual (0, lb1.TopIndex, "#19");
			Assert.AreEqual (true, lb1.UseTabStops, "#20");
		}

		[Test]
		public void BeginEndUpdateTest ()
		{
			Form f = new Form ();
			f.Visible = true;
			ListBox lb1 = new ListBox ();
			lb1.Items.Add ("A");
			lb1.Visible = true;
			f.Controls.Add (lb1);
			lb1.BeginUpdate ();
			for (int x = 1; x < 5000; x++)
			{
				lb1.Items.Add ("Item " + x.ToString ());
			}
			lb1.EndUpdate ();
			lb1.SetSelected (1, true);
			lb1.SetSelected (3, true);
			Assert.AreEqual (true, lb1.SelectedItems.Contains ("Item 3"), "#21");
		}

		[Test]
		public void ClearSelectedTest ()
		{
			Form f = new Form ();
			f.Visible = true;
			ListBox lb1 = new ListBox ();
			lb1.Items.Add ("A");
			lb1.Visible = true;
			f.Controls.Add (lb1);
			lb1.SetSelected (0, true);
			Assert.AreEqual ("A", lb1.SelectedItems [0].ToString (),"#22");
			lb1.ClearSelected ();
			Assert.AreEqual (0, lb1.SelectedItems.Count,"#23");
		}

		[Ignore ("It depends on user system settings")]
		public void GetItemHeightTest ()
		{
			Form f = new Form ();
			ListBox lb1 = new ListBox ();
			lb1.Visible = true;
			f.Controls.Add (lb1);
			lb1.Items.Add ("A");
			Assert.AreEqual (13, lb1.GetItemHeight (0) , "#28");
		}

		[Ignore ("It depends on user system settings")]
		public void GetItemRectangleTest ()
		{
			Form f = new Form ();
			f.Visible = true;
			ListBox lb1 = new ListBox ();
			lb1.Visible = true;
			f.Controls.Add (lb1);
			lb1.Items.Add ("A");
			Assert.AreEqual (new Rectangle(0,0,116,13), lb1.GetItemRectangle (0), "#29");
		}

		[Test]
		public void GetSelectedTest ()
		{
			ListBox lb1 = new ListBox ();
			lb1.Items.Add ("A");
			lb1.Items.Add ("B");
			lb1.Items.Add ("C");
			lb1.Items.Add ("D");
			lb1.Sorted = true;
			lb1.SetSelected (0,true);
			lb1.SetSelected (2,true);
			lb1.TopIndex=0;
			Assert.AreEqual (true, lb1.GetSelected (0), "#30");
			lb1.SetSelected (2,false);
			Assert.AreEqual (false, lb1.GetSelected (2), "#31");
		}

		[Test]
		public void IndexFromPointTest ()
		{
			ListBox lb1 = new ListBox ();
			lb1.Items.Add ("A");
			Point pt = new Point (100,100);
				lb1.IndexFromPoint (pt);
			Assert.AreEqual (-1, lb1.IndexFromPoint (100,100), "#32");
		}

		[Test]
		public void FindStringTest ()
		{
			ListBox cmbbox = new ListBox ();
			cmbbox.FindString ("Hola", -5); // No exception, it's empty
			int x = cmbbox.FindString ("Hello");
			Assert.AreEqual (-1, x, "#19");
			cmbbox.Items.AddRange(new object[] {"ACBD", "ABDC", "ACBD", "ABCD"});
			String myString = "ABC";
			x = cmbbox.FindString (myString);
			Assert.AreEqual (3, x, "#191");
			x = cmbbox.FindString (string.Empty);
			Assert.AreEqual (0, x, "#192");
			x = cmbbox.FindString ("NonExistant");
			Assert.AreEqual (-1, x, "#193");
		}

		[Test]
		public void FindStringExactTest ()
		{
			ListBox cmbbox = new ListBox ();
			cmbbox.FindStringExact ("Hola", -5); // No exception, it's empty
			int x = cmbbox.FindStringExact ("Hello");
			Assert.AreEqual (-1, x, "#20");
			cmbbox.Items.AddRange (new object[] {"ABCD","ABC","ABDC"});
			String myString = "ABC";
			x = cmbbox.FindStringExact (myString);
			Assert.AreEqual (1, x, "#201");
			x = cmbbox.FindStringExact (string.Empty);
			Assert.AreEqual (-1, x, "#202");
			x = cmbbox.FindStringExact ("NonExistant");
			Assert.AreEqual (-1, x, "#203");
		}

		//
		// Exceptions
		//

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void BorderStyleException ()
		{
			ListBox lstbox = new ListBox ();
			lstbox.BorderStyle = (BorderStyle) 10;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ColumnWidthException ()
		{
			ListBox lstbox = new ListBox ();
			lstbox.ColumnWidth = -1;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void DrawModeException ()
		{
			ListBox lstbox = new ListBox ();
			lstbox.DrawMode = (DrawMode) 10;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawModeAndMultiColumnException ()
		{
			ListBox lstbox = new ListBox ();
			lstbox.MultiColumn = true;
			lstbox.DrawMode = DrawMode.OwnerDrawVariable;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ItemHeightException ()
		{
			ListBox lstbox = new ListBox ();
			lstbox.ItemHeight = 256;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexException ()
		{
			ListBox lstbox = new ListBox ();
			lstbox.SelectedIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectedIndexModeNoneException ()
		{
			ListBox lstbox = new ListBox ();
			lstbox.SelectionMode = SelectionMode.None;
			lstbox.SelectedIndex = -1;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void SelectionModeException ()
		{
			ListBox lstbox = new ListBox ();
			lstbox.SelectionMode = (SelectionMode) 10;
		}

		//
		// Events
		//
		private bool eventFired;

		private void GenericHandler (object sender,  EventArgs e)
		{
			eventFired = true;
		}


	}

	[TestFixture]
	public class ListBoxObjectCollectionTest
	{
		[Test]
		public void ComboBoxObjectCollectionPropertyTest ()
		{
			ListBox.ObjectCollection col = new ListBox.ObjectCollection (new ListBox ());
			Assert.AreEqual (false, col.IsReadOnly, "#B1");
			Assert.AreEqual (false, ((ICollection)col).IsSynchronized, "#B2");
			Assert.AreEqual (col, ((ICollection)col).SyncRoot, "#B3");
			Assert.AreEqual (false, ((IList)col).IsFixedSize, "#B4");
		}

		[Test]
		public void AddTest ()
		{
			ListBox.ObjectCollection col = new ListBox.ObjectCollection (new ListBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (2, col.Count, "#C1");
		}

		[Test]
		public void ClearTest ()
		{
			ListBox.ObjectCollection col = new ListBox.ObjectCollection (new ListBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.Clear ();
			Assert.AreEqual (0, col.Count, "#D1");
		}

		[Test]
		public void ContainsTest ()
		{
			ListBox.ObjectCollection col = new ListBox.ObjectCollection (new ListBox ());
			object obj = "Item1";
			col.Add (obj);
			Assert.AreEqual (true, col.Contains ("Item1"), "#E1");
			Assert.AreEqual (false, col.Contains ("Item2"), "#E2");
		}

		[Test]
		public void IndexOfTest ()
		{
			ListBox.ObjectCollection col = new ListBox.ObjectCollection (new ListBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (1, col.IndexOf ("Item2"), "#F1");
		}

		[Test]
		public void RemoveTest ()
		{
			ListBox.ObjectCollection col = new ListBox.ObjectCollection (new ListBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.Remove ("Item1");
			Assert.AreEqual (1, col.Count, "#G1");
		}

		[Test]
		public void RemoveAtTest ()
		{
			ListBox.ObjectCollection col = new ListBox.ObjectCollection (new ListBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.RemoveAt (0);
			Assert.AreEqual (1, col.Count, "#H1");
			Assert.AreEqual (true, col.Contains ("Item2"), "#H1");
		}


	}
}
