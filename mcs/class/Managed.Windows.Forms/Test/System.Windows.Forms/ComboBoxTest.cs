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
			Assert.AreEqual (false, mycmbbox.DroppedDown, "#4");
			Assert.AreEqual (true, mycmbbox.IntegralHeight, "#5");
			Assert.AreEqual (0, mycmbbox.Items.Count, "#6");
			//Assert.AreEqual (15, mycmbbox.ItemHeight, "#7"); 	// Note: Item height depends on the current font.
			Assert.AreEqual (8, mycmbbox.MaxDropDownItems, "#8");
			Assert.AreEqual (0, mycmbbox.MaxLength, "#9");
			//Assert.AreEqual (20, mycmbbox.PreferredHeight, "#10");
			// Note: Item height depends on the current font.
			Assert.AreEqual (-1, mycmbbox.SelectedIndex, "#11");
			Assert.AreEqual (null, mycmbbox.SelectedItem, "#12");
			Assert.AreEqual ("", mycmbbox.SelectedText, "#13");
			Assert.AreEqual (0, mycmbbox.SelectionLength, "#14");
			Assert.AreEqual (0, mycmbbox.SelectionStart, "#15");
			Assert.AreEqual (false, mycmbbox.Sorted, "#16");
			Assert.AreEqual ("", mycmbbox.Text, "#17");
#if NET_2_0
			Assert.AreEqual (true, mycmbbox.AutoCompleteCustomSource != null, "#18");
			Assert.AreEqual (AutoCompleteMode.None, mycmbbox.AutoCompleteMode, "#19");
			Assert.AreEqual (AutoCompleteSource.None, mycmbbox.AutoCompleteSource, "#20");

			mycmbbox.AutoCompleteCustomSource = null;
			Assert.AreEqual (true, mycmbbox.AutoCompleteCustomSource != null, "#21");
#endif
		}

		[Test]
		public void BeginEndUpdateTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
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
			myform.Dispose ();
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
		public void DropDownWidth ()
		{
			ComboBox cmbbox = new ComboBox ();
			Assert.AreEqual (121, cmbbox.DropDownWidth, "#A1");
			cmbbox.DropDownWidth = 1;
			Assert.AreEqual (1, cmbbox.DropDownWidth, "#A2");

			try {
				cmbbox.DropDownWidth = 0;
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("DropDownWidth", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}
#endif
		}

		[Test]
		public void ItemHeight ()
		{
			ComboBox cmbbox = new ComboBox ();
			Assert.IsTrue (cmbbox.ItemHeight >= 1, "#A1");
			cmbbox.ItemHeight = 1;
			Assert.AreEqual (1, cmbbox.ItemHeight, "#A2");

			try {
				cmbbox.ItemHeight = 0;
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("ItemHeight", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}
#endif
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
#if ONLY_1_1
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#endif
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
			myform.ShowInTaskbar = false;
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
			myform.Dispose ();
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

		[Test]
		public void SelectionWithAdd()
		{
			ComboBox cb = new ComboBox();
			cb.SelectedIndexChanged += new EventHandler(GenericHandler);
			cb.Items.Add("Item 1");
			cb.Items.Add("Item 3");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Items.Add("Item 4");
			Assert.AreEqual(1, cb.SelectedIndex, "SWA1");
			Assert.AreEqual(false, eventFired, "SWA2");
			cb.Sorted = true;
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Items.Add("Item 5");
			Assert.AreEqual(1, cb.SelectedIndex, "SWA3");
			Assert.AreEqual("Item 3", cb.SelectedItem, "SWA4");
			Assert.AreEqual(false, eventFired, "SWA5");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Items.Add("Item 2");
			Assert.AreEqual(1, cb.SelectedIndex, "SWA6");
			Assert.AreEqual("Item 2", cb.SelectedItem, "SWA7");
			Assert.AreEqual(false, eventFired, "SWA8");
		}

		[Test]
		public void SelectionWithInsert()
		{
			ComboBox cb = new ComboBox();
			cb.SelectedIndexChanged += new EventHandler(GenericHandler);
			cb.Items.Add("Item 1");
			cb.SelectedIndex = 0;
			eventFired = false;
			cb.Items.Insert(0, "Item 2");
			Assert.AreEqual(0, cb.SelectedIndex, "SWI1");
			Assert.AreEqual(false, eventFired, "SWI2");
		}

		[Test]
		public void SelectionWithClear()
		{
			ComboBox cb = new ComboBox();
			cb.SelectedIndexChanged += new EventHandler(GenericHandler);
			cb.Items.Add("Item 1");
			cb.SelectedIndex = 0;
			eventFired = false;
			cb.Items.Clear();
			Assert.AreEqual(-1, cb.SelectedIndex, "SWC1");
			Assert.AreEqual(false, eventFired, "SWC2");
		}

		[Test]
		public void SortedTest()
		{
			ComboBox mycb = new ComboBox();
			Assert.AreEqual(false, mycb.Sorted, "#1");
			mycb.Items.Add("Item 2");
			mycb.Items.Add("Item 1");
			Assert.AreEqual("Item 2", mycb.Items[0], "#2");
			Assert.AreEqual("Item 1", mycb.Items[1], "#3");
			mycb.Sorted = true;
			Assert.AreEqual(true, mycb.Sorted, "#4");
			Assert.AreEqual("Item 1", mycb.Items[0], "#5");
			Assert.AreEqual("Item 2", mycb.Items[1], "#6");
			mycb.Sorted = false;
			Assert.AreEqual(false, mycb.Sorted, "#7");
			Assert.AreEqual("Item 1", mycb.Items[0], "#8");
			Assert.AreEqual("Item 2", mycb.Items[1], "#9");
		}

		[Test]
		public void SortedAddTest()
		{
			ComboBox mycb = new ComboBox();
			mycb.Items.Add("Item 2");
			mycb.Items.Add("Item 1");
			mycb.Sorted = true;
			Assert.AreEqual("Item 1", mycb.Items[0], "#I1");
			Assert.AreEqual("Item 2", mycb.Items[1], "#I2");
		}

		[Test]
		public void SortedInsertTest()
		{
			ComboBox mycb = new ComboBox();
			mycb.Items.Add("Item 2");
			mycb.Items.Add("Item 1");
			mycb.Sorted = true;
			mycb.Items.Insert (0, "Item 3");
			Assert.AreEqual("Item 1", mycb.Items[0], "#J1");
			Assert.AreEqual("Item 2", mycb.Items[1], "#J2");
			Assert.AreEqual("Item 3", mycb.Items[2], "#J3");
		}

		[Test]
		public void SortedSelectionInteractions()
		{
			ComboBox cb = new ComboBox();
			cb.SelectedIndexChanged += new EventHandler(GenericHandler);
			cb.Items.Add("Item 1");
			cb.Items.Add("Item 2");
			cb.Items.Add("Item 3");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Sorted = true;
			Assert.AreEqual(-1, cb.SelectedIndex, "#SSI1");
			Assert.AreEqual(true, eventFired, "#SSI2");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Sorted = true;
			Assert.AreEqual(1, cb.SelectedIndex, "#SSI3");
			Assert.AreEqual(false, eventFired, "#SSI4");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Sorted = false;
			Assert.AreEqual(-1, cb.SelectedIndex, "#SSI5");
			Assert.AreEqual(true, eventFired, "#SSI6");
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

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNullTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRangeNullTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.AddRange (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ContainsNullTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Contains (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexOfNullTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.IndexOf (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InsertNullTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Insert (0, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexerNullTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col [0] = null;
		}
	}

}
