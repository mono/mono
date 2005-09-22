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
	public class ComboBoxTest
	{
		[Test]
		public void ComboBoxPropertyTest ()
		{
			ComboBox mycmbbox = new ComboBox ();
			Assert.AreEqual (DrawMode.Normal, mycmbbox.DrawMode, "#1");
			Assert.AreEqual (ComboBoxStyle.DropDown, mycmbbox.DropDownStyle, "#2");
			Assert.AreEqual (121, mycmbbox.DropDownWidth, "#3");
			Assert.AreEqual (false, mycmbbox.DroppedDown, "#4");
			Assert.AreEqual (true, mycmbbox.IntegralHeight, "#5");
			Assert.AreEqual (0, mycmbbox.Items.Count, "#6");
			//Assert.AreEqual (15, mycmbbox.ItemHeight, "#7"); 	// Note: Item height depends on the current font.
			Assert.AreEqual (8, mycmbbox.MaxDropDownItems, "#8");
			Assert.AreEqual (0, mycmbbox.MaxLength, "#9");
			Assert.AreEqual (20, mycmbbox.PreferredHeight, "#10");
			Assert.AreEqual (-1, mycmbbox.SelectedIndex, "#11");
			Assert.AreEqual (null, mycmbbox.SelectedItem, "#12");
			Assert.AreEqual ("", mycmbbox.SelectedText, "#13");
			Assert.AreEqual (0, mycmbbox.SelectionLength, "#14");
			Assert.AreEqual (0, mycmbbox.SelectionStart, "#15");
			Assert.AreEqual (false, mycmbbox.Sorted, "#16");
			Assert.AreEqual ("", mycmbbox.Text, "#17");
		}

		[Test]
		public void BeginEndUpdateTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.Add ("A");
			cmbbox.Visible = true;
			myform.Controls.Add (cmbbox);
			cmbbox.BeginUpdate ();
			for (int x = 1 ; x < 5000 ; x++) {
				cmbbox.Items.Add ("Item " + x.ToString ());
			}
			cmbbox.EndUpdate ();
		}

		[Test]
		public void FindStringTest ()
		{
			ComboBox cmbbox = new ComboBox ();
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
			ComboBox cmbbox = new ComboBox ();
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

		[Test]
		public void GetItemHeightTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.Add ("ABC");
			cmbbox.Items.Add ("BCD");
			cmbbox.Items.Add ("DEF");
			int x = -1;
			x = cmbbox.GetItemHeight (x);
			Assert.IsTrue (cmbbox.ItemHeight > 0, "#21");
		}


		//
		// Exceptions
		//

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void DropDownStyleException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownStyle = (ComboBoxStyle) 10;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void DrawModeException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DrawMode = (DrawMode) 10;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DropDownWidthException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownWidth = 0;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ItemHeightException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.ItemHeight = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.SelectedIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void FindStringExactMinException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange(new object[] {"ACBD", "ABDC", "ACBD", "ABCD"});
			cmbbox.FindStringExact ("Hola", -2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void FindStringExactMaxException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange(new object[] {"ACBD", "ABDC", "ACBD", "ABCD"});
			cmbbox.FindStringExact ("Hola", 3);
		}

		//
		// Events
		//
		private bool eventFired;
		private DrawItemEventArgs drawItemsArgs;
		private void DrawItemEventH (object sender,  DrawItemEventArgs e)
		{
			eventFired = true;
			drawItemsArgs = e;
		}

		private void GenericHandler (object sender,  EventArgs e)
		{
			eventFired = true;
		}

		[Ignore ("Bugs in X11 prevent this test to run properly")]
		public void DrawItemEventTest ()
		{
			eventFired = false;
			drawItemsArgs = null;
			Form myform = new Form ();
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownStyle = ComboBoxStyle.Simple;
			cmbbox.DrawMode = DrawMode.OwnerDrawFixed;
			cmbbox.DrawItem += new DrawItemEventHandler (DrawItemEventH);

			myform.Controls.Add (cmbbox);
			cmbbox.Items.AddRange(new object[] {"Item1"});

			myform.Visible = true;
			cmbbox.Visible = true;
			cmbbox.Refresh ();

			Assert.AreEqual (true, eventFired, "DW1");
			Assert.AreEqual (0, drawItemsArgs.Index, "DW2");
		}

		[Test]
		public void DropDownStyleEventTest ()
		{
			eventFired = false;
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownStyleChanged += new EventHandler (GenericHandler);
			cmbbox.DropDownStyle = ComboBoxStyle.Simple;

			Assert.AreEqual (true, eventFired, "DI1");
		}

		[Test]
		public void SelectedIndextTest ()
		{
			eventFired = false;
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange(new object[] {"Item1", "Item2"});
			cmbbox.SelectedIndexChanged += new EventHandler (GenericHandler);
			cmbbox.SelectedIndex = 1;
			Assert.AreEqual (true, eventFired, "SI1");
		}

	}

	[TestFixture]
	public class ComboBoxObjectCollectionTest
	{
		[Test]
		public void ComboBoxObjectCollectionPropertyTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			Assert.AreEqual (false, col.IsReadOnly, "#B1");
			Assert.AreEqual (false, ((ICollection)col).IsSynchronized, "#B2");
			Assert.AreEqual (col, ((ICollection)col).SyncRoot, "#B3");
			Assert.AreEqual (false, ((IList)col).IsFixedSize, "#B4");
		}

		[Test]
		public void AddTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (2, col.Count, "#C1");
		}

		[Test]
		public void ClearTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.Clear ();
			Assert.AreEqual (0, col.Count, "#D1");
		}

		[Test]
		public void ContainsTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			object obj = "Item1";
			col.Add (obj);
			Assert.AreEqual (true, col.Contains ("Item1"), "#E1");
			Assert.AreEqual (false, col.Contains ("Item2"), "#E2");
		}

		[Test]
		public void IndexOfTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (1, col.IndexOf ("Item2"), "#F1");
		}

		[Test]
		public void RemoveTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.Remove ("Item1");
			Assert.AreEqual (1, col.Count, "#G1");
		}

		[Test]
		public void RemoveAtTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.RemoveAt (0);
			Assert.AreEqual (1, col.Count, "#H1");
			Assert.AreEqual (true, col.Contains ("Item2"), "#H1");
		}
	}

}
