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
		ListBox listBox;
		Form form;

		[SetUp]
		public void SetUp()
		{
			listBox = new ListBox();
			form = new Form();
			form.ShowInTaskbar = false;
		}

		[TearDown]
		public void TearDown()
		{
			form.Dispose ();
		}

		[Test]
		public void ListBoxPropertyTest ()
		{
			Assert.AreEqual (0, listBox.ColumnWidth, "#1");
			Assert.AreEqual (DrawMode.Normal, listBox.DrawMode, "#2");
			Assert.AreEqual (0, listBox.HorizontalExtent, "#3");
			Assert.AreEqual (false, listBox.HorizontalScrollbar, "#4");
			Assert.AreEqual (true, listBox.IntegralHeight, "#5");
			//Assert.AreEqual (13, listBox.ItemHeight, "#6"); // Note: Item height depends on the current font.
			listBox.Items.Add ("a");
			listBox.Items.Add ("b");
			listBox.Items.Add ("c");
			Assert.AreEqual (3,  listBox.Items.Count, "#7");
			Assert.AreEqual (false, listBox.MultiColumn, "#8");
			//Assert.AreEqual (46, listBox.PreferredHeight, "#9"); // Note: Item height depends on the current font.
			//Assert.AreEqual (RightToLeft.No , listBox.RightToLeft, "#10"); // Depends on Windows version
			Assert.AreEqual (false, listBox.ScrollAlwaysVisible, "#11");
			Assert.AreEqual (-1, listBox.SelectedIndex, "#12");
			listBox.SetSelected (2,true);
			Assert.AreEqual (2, listBox.SelectedIndices[0], "#13");
			Assert.AreEqual ("c", listBox.SelectedItem, "#14");
			Assert.AreEqual ("c", listBox.SelectedItems[0], "#15");
			Assert.AreEqual (SelectionMode.One, listBox.SelectionMode, "#16");
			listBox.SetSelected (2,false);
			Assert.AreEqual (false, listBox.Sorted, "#17");
			Assert.AreEqual ("", listBox.Text, "#18");
			Assert.AreEqual (0, listBox.TopIndex, "#19");
			Assert.AreEqual (true, listBox.UseTabStops, "#20");
		}

		[Test]
		public void BeginEndUpdateTest ()
		{
			form.Visible = true;
			listBox.Items.Add ("A");
			listBox.Visible = true;
			form.Controls.Add (listBox);
			listBox.BeginUpdate ();
			for (int x = 1; x < 5000; x++)
			{
				listBox.Items.Add ("Item " + x.ToString ());
			}
			listBox.EndUpdate ();
			listBox.SetSelected (1, true);
			listBox.SetSelected (3, true);
			Assert.AreEqual (true, listBox.SelectedItems.Contains ("Item 3"), "#21");
		}

		[Test]
		public void ClearSelectedTest ()
		{
			form.Visible = true;
			listBox.Items.Add ("A");
			listBox.Visible = true;
			form.Controls.Add (listBox);
			listBox.SetSelected (0, true);
			Assert.AreEqual ("A", listBox.SelectedItems [0].ToString (),"#22");
			listBox.ClearSelected ();
			Assert.AreEqual (0, listBox.SelectedItems.Count,"#23");
		}

		[Test] // bug #80620
		[NUnit.Framework.Category ("NotWorking")]
		public void ClientRectangle_Borders ()
		{
			// This test is invalid because createcontrol forces .net to resize
			// the listbox using integralheight, which defaults to true.  This
			// will only hold for most font sizes.
			listBox.CreateControl ();
			Assert.AreEqual (listBox.ClientRectangle, new ListBox ().ClientRectangle);
		}

		[Ignore ("It depends on user system settings")]
		public void GetItemHeightTest ()
		{
			listBox.Visible = true;
			form.Controls.Add (listBox);
			listBox.Items.Add ("A");
			Assert.AreEqual (13, listBox.GetItemHeight (0) , "#28");
		}

		[Ignore ("It depends on user system settings")]
		public void GetItemRectangleTest ()
		{
			form.Visible = true;
			listBox.Visible = true;
			form.Controls.Add (listBox);
			listBox.Items.Add ("A");
			Assert.AreEqual (new Rectangle(0,0,116,13), listBox.GetItemRectangle (0), "#29");
		}

		[Test]
		public void GetSelectedTest ()
		{
			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			listBox.Items.Add ("C");
			listBox.Items.Add ("D");
			listBox.SelectionMode = SelectionMode.MultiSimple;
			listBox.Sorted = true;
			listBox.SetSelected (0,true);
			listBox.SetSelected (2,true);
			listBox.TopIndex=0;
			Assert.AreEqual (true, listBox.GetSelected (0), "#30");
			listBox.SetSelected (2,false);
			Assert.AreEqual (false, listBox.GetSelected (2), "#31");
		}

		[Test]
		public void IndexFromPointTest ()
		{
			listBox.Items.Add ("A");
			Point pt = new Point (100,100);
				listBox.IndexFromPoint (pt);
			Assert.AreEqual (-1, listBox.IndexFromPoint (100,100), "#32");
		}

		[Test]
		public void FindStringTest ()
		{
			listBox.FindString ("Hola", -5); // No exception, it's empty
			int x = listBox.FindString ("Hello");
			Assert.AreEqual (-1, x, "#19");
			listBox.Items.AddRange(new object[] {"ACBD", "ABDC", "ACBD", "ABCD"});
			String myString = "ABC";
			x = listBox.FindString (myString);
			Assert.AreEqual (3, x, "#191");
			x = listBox.FindString (string.Empty);
			Assert.AreEqual (0, x, "#192");
			x = listBox.FindString ("NonExistant");
			Assert.AreEqual (-1, x, "#193");

			x = listBox.FindString ("A", -1);
			Assert.AreEqual (0, x, "#194");
			x = listBox.FindString ("A", 0);
			Assert.AreEqual (1, x, "#195");
			x = listBox.FindString ("A", listBox.Items.Count - 1);
			Assert.AreEqual (0, x, "#196");
			x = listBox.FindString ("a", listBox.Items.Count - 1);
			Assert.AreEqual (0, x, "#197");
		}

		[Test]
		public void FindStringExactTest ()
		{
			listBox.FindStringExact ("Hola", -5); // No exception, it's empty
			int x = listBox.FindStringExact ("Hello");
			Assert.AreEqual (-1, x, "#20");
			listBox.Items.AddRange (new object[] {"ABCD","ABC","ABDC"});
			String myString = "ABC";
			x = listBox.FindStringExact (myString);
			Assert.AreEqual (1, x, "#201");
			x = listBox.FindStringExact (string.Empty);
			Assert.AreEqual (-1, x, "#202");
			x = listBox.FindStringExact ("NonExistant");
			Assert.AreEqual (-1, x, "#203");

			x = listBox.FindStringExact ("ABCD", -1);
			Assert.AreEqual (0, x, "#204");
			x = listBox.FindStringExact ("ABC", 0);
			Assert.AreEqual (1, x, "#205");
			x = listBox.FindStringExact ("ABC", listBox.Items.Count - 1);
			Assert.AreEqual (1, x, "#206");
			x = listBox.FindStringExact ("abcd", listBox.Items.Count - 1);
			Assert.AreEqual (0, x, "#207");
		}

#if NET_2_0
		[Test]
		public void AllowSelection ()
		{
			MockListBox lb = new MockListBox ();
			lb.SelectionMode = SelectionMode.None;
			Assert.IsFalse (lb.allow_selection, "#1");
			lb.SelectionMode = SelectionMode.One;
			Assert.IsTrue (lb.allow_selection, "#2");
		}
#endif

		//
		// Exceptions
		//

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void BorderStyleException ()
		{
			listBox.BorderStyle = (BorderStyle) 10;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ColumnWidthException ()
		{
			listBox.ColumnWidth = -1;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void DrawModeException ()
		{
			listBox.DrawMode = (DrawMode) 10;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawModeAndMultiColumnException ()
		{
			listBox.MultiColumn = true;
			listBox.DrawMode = DrawMode.OwnerDrawVariable;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ItemHeightException ()
		{
			listBox.ItemHeight = 256;
		}

		[Test] // bug #80696
		public void SelectedIndex_Created ()
		{
			Form form = new Form ();
			ListBox listBox = new ListBox ();
			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			form.Controls.Add (listBox);
			form.Show ();

			Assert.AreEqual (-1, listBox.SelectedIndex, "#1");
			listBox.SelectedIndex = 0;
			Assert.AreEqual (0, listBox.SelectedIndex, "#2");
			listBox.SelectedIndex = -1;
			Assert.AreEqual (-1, listBox.SelectedIndex, "#3");
			listBox.SelectedIndex = 1;
			Assert.AreEqual (1, listBox.SelectedIndex, "#4");
			
			form.Close ();
		}

		[Test] // bug #80753
		public void SelectedIndex_NotCreated ()
		{
			ListBox listBox = new ListBox ();
			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			Assert.AreEqual (-1, listBox.SelectedIndex, "#1");
			listBox.SelectedIndex = 0;
			Assert.AreEqual (0, listBox.SelectedIndex, "#2");
			listBox.SelectedIndex = -1;
			Assert.AreEqual (-1, listBox.SelectedIndex, "#3");
			listBox.SelectedIndex = 1;
			Assert.AreEqual (1, listBox.SelectedIndex, "#4");
		}

#if NET_2_0
		[Test]
		public void SelectedIndex_Removed ()
		{
			ListBox listBox = new ListBox ();
			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			listBox.Items.Add ("C");

			Assert.AreEqual (-1, listBox.SelectedIndex, "#1");
			listBox.SelectedIndex = 2;
			Assert.AreEqual (2, listBox.SelectedIndex, "#2");
			listBox.Items.RemoveAt (2);
			Assert.AreEqual (-1, listBox.SelectedIndex, "#3");

			listBox.SelectedIndex = 0;
			Assert.AreEqual (0, listBox.SelectedIndex, "#4");
			listBox.Items.RemoveAt (0);
			Assert.AreEqual (-1, listBox.SelectedIndex, "#5");
		}

		// This should also apply to MultiSimple selection mode
		[Test]
		public void Selection_MultiExtended ()
		{
			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			listBox.Items.Add ("C");
			listBox.Items.Add ("D");
			listBox.SelectionMode = SelectionMode.MultiExtended;

			//
			// First part: test the order of SelectedItems as well
			// as SelectedIndex when more than one item is selected
			//
			listBox.SelectedItems.Add ("D");
			listBox.SelectedItems.Add ("B");
			Assert.AreEqual (1, listBox.SelectedIndex, "#A1");
			Assert.AreEqual (2, listBox.SelectedItems.Count, "#A2");
			Assert.AreEqual ("B", listBox.SelectedItems [0], "#A3");
			Assert.AreEqual ("D", listBox.SelectedItems [1], "#A4");

			listBox.SelectedItems.Add ("C");
			Assert.AreEqual (1, listBox.SelectedIndex, "#B1");
			Assert.AreEqual (3, listBox.SelectedItems.Count, "#B2");
			Assert.AreEqual ("B", listBox.SelectedItems [0], "#B3");
			Assert.AreEqual ("C", listBox.SelectedItems [1], "#B4");
			Assert.AreEqual ("D", listBox.SelectedItems [2], "#B5");

			listBox.SelectedItems.Add ("A");
			Assert.AreEqual (0, listBox.SelectedIndex, "#C1");
			Assert.AreEqual (4, listBox.SelectedItems.Count, "#C2");
			Assert.AreEqual ("A", listBox.SelectedItems [0], "#C3");
			Assert.AreEqual ("B", listBox.SelectedItems [1], "#C4");
			Assert.AreEqual ("C", listBox.SelectedItems [2], "#C5");
			Assert.AreEqual ("D", listBox.SelectedItems [3], "#C6");

			// 
			// Second part: how does SelectedIndex setter work related
			// to SelectedItems
			//
			listBox.SelectedIndex = -1;
			Assert.AreEqual (-1, listBox.SelectedIndex, "#D1");
			Assert.AreEqual (0, listBox.SelectedItems.Count, "#D2");

			listBox.SelectedIndex = 3; // "D"
			Assert.AreEqual (3, listBox.SelectedIndex, "#E1");
			Assert.AreEqual (1, listBox.SelectedItems.Count, "#E2");
			Assert.AreEqual ("D", listBox.SelectedItems [0], "#E3");

			listBox.SelectedItems.Add ("B"); // index = 1
			Assert.AreEqual (1, listBox.SelectedIndex, "#F1");
			Assert.AreEqual (2, listBox.SelectedItems.Count, "#E3");
			Assert.AreEqual ("B", listBox.SelectedItems [0], "#E4");
			Assert.AreEqual ("D", listBox.SelectedItems [1], "#E5");

			listBox.SelectedIndex = 2;
			Assert.AreEqual (1, listBox.SelectedIndex, "#G1");
			Assert.AreEqual (3, listBox.SelectedItems.Count, "#G2");
			Assert.AreEqual ("B", listBox.SelectedItems [0], "#G3");
			Assert.AreEqual ("C", listBox.SelectedItems [1], "#G4");
			Assert.AreEqual ("D", listBox.SelectedItems [2], "#G5");

			listBox.SelectedIndex = 1; // already selected
			Assert.AreEqual (1, listBox.SelectedIndex, "#H1");
			Assert.AreEqual (3, listBox.SelectedItems.Count, "#H2");

			// NOTE: It seems that passing -1 does not affect the collection
			// in anyway (other wrong values generate an exception, however)
			listBox.SelectedIndices.Add (-1);
			Assert.AreEqual (3, listBox.SelectedItems.Count, "#J1");
		}

		[Test]
		public void Selection_One ()
		{
			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			listBox.Items.Add ("C");
			listBox.SelectionMode = SelectionMode.One;

			listBox.SelectedItems.Add ("B");
			Assert.AreEqual (1, listBox.SelectedIndex, "#A1");
			Assert.AreEqual (1, listBox.SelectedItems.Count, "#A2");
			Assert.AreEqual ("B", listBox.SelectedItems [0], "#A3");

			listBox.SelectedIndex = 2;
			Assert.AreEqual (2, listBox.SelectedIndex, "#B1");
			Assert.AreEqual (1, listBox.SelectedItems.Count, "#B2");
			Assert.AreEqual ("C", listBox.SelectedItems [0], "#B3");

			listBox.SelectedItems.Add ("A");
			Assert.AreEqual (0, listBox.SelectedIndex, "#C1");
			Assert.AreEqual (1, listBox.SelectedItems.Count, "#C2");
			Assert.AreEqual ("A", listBox.SelectedItems [0], "#C3");
		}

		[Test]
		public void Selection_None ()
		{
			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			listBox.SelectionMode = SelectionMode.None;

			try {
				listBox.SelectedIndex = 0;
				Assert.Fail ("#A");
			} catch (ArgumentException) {
			}

			try {
				listBox.SelectedIndices.Add (0);
				Assert.Fail ("#B");
			} catch (InvalidOperationException e) {
				Console.WriteLine (e.Message);
			}

			try {
				listBox.SelectedItems.Add ("A");
				Assert.Fail ("#C");
			} catch (ArgumentException) {
			}
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexException ()
		{
			listBox.SelectedIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexException2 ()
		{
			listBox.SelectedIndex = listBox.Items.Count;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectedIndexModeNoneException ()
		{
			listBox.SelectionMode = SelectionMode.None;
			listBox.SelectedIndex = -1;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void SelectionModeException ()
		{
			listBox.SelectionMode = (SelectionMode) 10;
		}

		[Test]
		public void SelectedValueNull()
		{
			listBox.Items.Clear ();

			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			listBox.Items.Add ("C");
			listBox.Items.Add ("D");

			listBox.SelectedIndex = 2;
			listBox.SelectedValue = null;
			Assert.AreEqual (listBox.SelectedIndex, 2);
		}

		[Test]
		public void SelectedValueEmptyString()
		{
			listBox.Items.Clear ();

			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			listBox.Items.Add ("C");
			listBox.Items.Add ("D");

			listBox.SelectedIndex = 2;
			listBox.SelectedValue = null;
			Assert.AreEqual (listBox.SelectedIndex, 2);
		}
		
		[Test]	// Bug #80466
		public void ListBoxHeight ()
		{
			ListBox l = new ListBox ();
			
			for (int h = 0; h < 100; h++) {
				l.Height = h;
				
				if (l.Height != h)
					Assert.Fail ("Set ListBox height of {0}, got back {1}.  Should be the same.", h, l.Height);
			}
		}

#if NET_2_0
		[Test]
		public void GetScaledBoundsTest ()
		{
			ScaleListBox c = new ScaleListBox ();

			Rectangle r = new Rectangle (100, 200, 300, 400);

			Assert.AreEqual (new Rectangle (200, 100, 596, 50), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.All), "A1");
			Assert.AreEqual (new Rectangle (200, 100, 300, 96), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Location), "A2");
			Assert.AreEqual (new Rectangle (100, 200, 596, 50), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Size), "A3");
			Assert.AreEqual (new Rectangle (100, 200, 300, 50), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Height), "A4");
			Assert.AreEqual (new Rectangle (200, 200, 300, 96), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.X), "A5");
			Assert.AreEqual (new Rectangle (100, 200, 300, 96), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.None), "A6");

			Assert.AreEqual (new Rectangle (100, 200, 300, 96), c.PublicGetScaledBounds (r, new SizeF (1f, 1f), BoundsSpecified.All), "A6-2");
			Assert.AreEqual (new Rectangle (200, 400, 596, 188), c.PublicGetScaledBounds (r, new SizeF (2f, 2f), BoundsSpecified.All), "A7");
			Assert.AreEqual (new Rectangle (300, 600, 892, 280), c.PublicGetScaledBounds (r, new SizeF (3f, 3f), BoundsSpecified.All), "A8");
			Assert.AreEqual (new Rectangle (400, 800, 1188, 372), c.PublicGetScaledBounds (r, new SizeF (4f, 4f), BoundsSpecified.All), "A9");
			Assert.AreEqual (new Rectangle (50, 100, 152, 50), c.PublicGetScaledBounds (r, new SizeF (.5f, .5f), BoundsSpecified.All), "A10");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void MethodScaleControl ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Show ();

			ScaleListBox gb = new ScaleListBox ();
			gb.Location = new Point (5, 10);
			f.Controls.Add (gb);

			Assert.AreEqual (new Rectangle (5, 10, 120, 95), gb.Bounds, "A1");

			gb.PublicScaleControl (new SizeF (2.0f, 2.0f), BoundsSpecified.All);
			Assert.AreEqual (new Rectangle (10, 20, 236, 186), gb.Bounds, "A2");

			gb.PublicScaleControl (new SizeF (.5f, .5f), BoundsSpecified.Location);
			Assert.AreEqual (new Rectangle (5, 10, 236, 186), gb.Bounds, "A3");

			gb.PublicScaleControl (new SizeF (.5f, .5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 120, 95), gb.Bounds, "A4");

			gb.PublicScaleControl (new SizeF (3.5f, 3.5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 410, 316), gb.Bounds, "A5");

			gb.PublicScaleControl (new SizeF (2.5f, 2.5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 1019, 797), gb.Bounds, "A6");

			gb.PublicScaleControl (new SizeF (.2f, .2f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 207, 160), gb.Bounds, "A7");

			f.Dispose ();
		}

		private class ScaleListBox : ListBox
		{
			public Rectangle PublicGetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
			{
				return base.GetScaledBounds (bounds, factor, specified);
			}

			public void PublicScaleControl (SizeF factor, BoundsSpecified specified)
			{
				base.ScaleControl (factor, specified);
			}
		}
#endif

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

		private class InputCharControl : ListBox
		{
			public bool PublicIsInputChar (char charCode)
			{
				return base.IsInputChar (charCode);
			}
		}

		[Test]
		public void HeightAndIntegralHeight ()
		{
			ListBox a = new ListBox();
			Size defaultSize = new Size(120, 96);
			Assert.AreEqual (defaultSize, a.Size, "A1");
			a.CreateControl();
			Assert.AreEqual (0, (a.ClientSize.Height % a.ItemHeight), "A2");
			a.IntegralHeight = false;
			Assert.AreEqual (a.Size, defaultSize, "A3");
			a.IntegralHeight = true;
			Assert.AreEqual (0, (a.ClientSize.Height % a.ItemHeight), "A4");

			Size clientSizeI = new Size(200, a.ItemHeight * 5);
			Size clientSize = clientSizeI + new Size(0, a.ItemHeight / 2);
			Size borderSize = new Size(a.Width - a.ClientSize.Width, a.Height - a.ClientSize.Height);
			Size totalSizeI = clientSizeI + borderSize;
			Size totalSize = clientSize + borderSize;

			a = new ListBox();
			a.ClientSize = clientSize;
			Assert.AreEqual (clientSize, a.ClientSize, "A5");
			Assert.AreEqual (totalSize, a.Size, "A6");
			a.IntegralHeight = false;
			a.IntegralHeight = true;
			Assert.AreEqual (clientSize, a.ClientSize, "A7");
			a.CreateControl();
			Assert.AreEqual (clientSizeI, a.ClientSize, "A8");
			Assert.AreEqual (totalSizeI, a.Size, "A9");
			a.IntegralHeight = false;
			Assert.AreEqual (clientSize, a.ClientSize, "A10");
			a.IntegralHeight = true;
			Assert.AreEqual (totalSizeI, a.Size, "A11");

			a = new ListBox();
			a.CreateControl();
			a.Size = totalSize;
			Assert.AreEqual (totalSizeI, a.Size, "A12");
			Assert.AreEqual (clientSizeI, a.ClientSize, "A13");
			a.IntegralHeight = false;
			Assert.AreEqual (totalSize, a.Size, "A14");
			Assert.AreEqual (clientSize, a.ClientSize, "A15");

			a = new ListBox();
			a.IntegralHeight = false;
			Assert.AreEqual (defaultSize, a.Size, "A16");
			a.CreateControl();
			Assert.AreEqual (defaultSize, a.Size, "A17");

			a = new ListBox();
			a.ClientSize = clientSize;
			a.IntegralHeight = false;
			Assert.AreEqual (clientSize, a.ClientSize, "A18");
			a.CreateControl();
			Assert.AreEqual (clientSize, a.ClientSize, "A19");
		}

		[Test]
		public void PropertyTopIndex ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Show ();
			
			ListBox l = new ListBox ();
			l.Height = 100;
			f.Controls.Add (l);
			
			l.Items.AddRange (new string[] { "A", "B", "C"});
			
			Assert.AreEqual (0, l.TopIndex, "A1");
			
			l.TopIndex = 2;
			Assert.AreEqual (0, l.TopIndex, "A2");

			l.Items.AddRange (new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M" });
			Assert.AreEqual (0, l.TopIndex, "A3");

			l.TopIndex = 2;
			Assert.AreEqual (2, l.TopIndex, "A4");

			// There aren't items enough for 12 to be the top index, but
			// the actual value is font height dependent.
			l.TopIndex = 12;
			Assert.IsTrue (l.TopIndex < 12, "A5");
			
			f.Close ();
			f.Dispose ();
		}
		
		//
		// Events
		//
		//private bool eventFired;

		//private void GenericHandler (object sender,  EventArgs e)
		//{
		//        eventFired = true;
		//}

		public class MockListBox : ListBox
		{
#if NET_2_0
			public bool allow_selection {
				get { return base.AllowSelection; }
			}
#endif
		}
	}

	[TestFixture]
	public class ListBoxObjectCollectionTest
	{
		ListBox.ObjectCollection col;

		[SetUp]
		public void SetUp()
		{
			col = new ListBox.ObjectCollection (new ListBox ());
		}

		[Test]
		public void DefaultProperties ()
		{
			Assert.AreEqual (false, col.IsReadOnly, "#B1");
			Assert.AreEqual (false, ((ICollection)col).IsSynchronized, "#B2");
			Assert.AreEqual (col, ((ICollection)col).SyncRoot, "#B3");
			Assert.AreEqual (false, ((IList)col).IsFixedSize, "#B4");
			Assert.AreEqual (0, col.Count);
		}

		[Test]
		public void AddTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (2, col.Count, "#C1");
		}

		[Test]
		public void ClearTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			col.Clear ();
			Assert.AreEqual (0, col.Count, "#D1");
		}

		[Test]
		public void ContainsTest ()
		{
			object obj = "Item1";
			col.Add (obj);
			Assert.AreEqual (true, col.Contains ("Item1"), "#E1");
			Assert.AreEqual (false, col.Contains ("Item2"), "#E2");
		}

		[Test]
		public void IndexOfTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (1, col.IndexOf ("Item2"), "#F1");
		}

		[Test]
		public void RemoveTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			col.Remove ("Item1");
			Assert.AreEqual (1, col.Count, "#G1");
		}

		[Test]
		public void RemoveAtTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			col.RemoveAt (0);
			Assert.AreEqual (1, col.Count, "#H1");
			Assert.AreEqual (true, col.Contains ("Item2"), "#H1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRangeNullTest ()
		{
			col.AddRange ((object []) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRangeNullTest2 ()
		{
			col.AddRange ((ListBox.ObjectCollection) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ContainsNullTest ()
		{
			col.Contains (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexOfNullTest ()
		{
			col.IndexOf (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InsertNullTest ()
		{
			col.Add ("Item1");
			col.Insert (0, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexerNullTest ()
		{
			col.Add ("Item1");
			col [0] = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNullTest ()
		{
			col.Add (null);
		}
	}
}
