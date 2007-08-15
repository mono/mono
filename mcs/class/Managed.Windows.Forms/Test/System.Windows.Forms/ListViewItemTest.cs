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
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListViewItemTest
	{
		[Test]
		public void ListViewItemConstructors ()
		{
			Font fnt = new Font ("Arial", 12);
			ListViewItem item1 = new ListViewItem ("Hello folks");
			Assert.AreEqual ("Hello folks", item1.Text, "Constructor#1");

			ListViewItem item2 = new ListViewItem (new string [] {"Element1", "Element2"},
				-1, Color.Blue, Color.Red, fnt);

			Assert.AreEqual (item2.ForeColor, Color.Blue, "Constructor#2");
			Assert.AreEqual (item2.BackColor, Color.Red, "Constructor#3");

			Assert.AreEqual (2, item2.SubItems.Count,"Constructor#4");
			Assert.AreEqual (Color.Blue, item2.SubItems[0].ForeColor,"Constructor#5");
			Assert.AreEqual (Color.Red, item2.SubItems[0].BackColor, "Constructor#6");
			Assert.AreEqual (fnt, item2.SubItems[0].Font, "Constructor#7");
			Assert.AreEqual ("Element1", item2.SubItems[0].Text, "Constructor#8");
			Assert.AreEqual ("Element2", item2.SubItems[1].Text, "Constructor#12");

			ListViewItem item3 = new ListViewItem ((string)null);
			Assert.AreEqual (String.Empty, item3.Text, "Constructor#13");

			ListViewItem item4 = new ListViewItem ((string)null, -99);
			Assert.AreEqual (String.Empty, item4.Text, "Constructor#14");
			Assert.AreEqual (-99, item4.ImageIndex, "Constructor#15");

			ListViewItem item5 = new ListViewItem (new string [2]);
			Assert.AreEqual (2, item5.SubItems.Count, "Constructor#16");
			Assert.IsNotNull (item5.SubItems [0], "Constructor#17");
			Assert.IsNotNull (item5.SubItems [1], "Constructor#18");

			ListViewItem item6 = new ListViewItem (new string [2], -1, Color.Blue, Color.Red,
				fnt);
			Assert.AreEqual (2, item6.SubItems.Count, "Constructor#19");
			Assert.IsNotNull (item6.SubItems [0], "Constructor#20");
			Assert.IsNotNull (item6.SubItems [1], "Constructor#21");
			Assert.AreEqual (Color.Blue, item6.ForeColor, "Constructor#22");
			Assert.AreEqual (Color.Blue, item6.SubItems [0].ForeColor, "Constructor#23");
			Assert.AreEqual (Color.Red, item6.BackColor, "Constructor#24");
			Assert.AreEqual (Color.Red, item6.SubItems [0].BackColor, "Constructor#25");
			Assert.AreEqual (fnt, item6.Font, "Constructor#26");
			Assert.AreEqual (fnt, item6.SubItems [0].Font, "Constructor#27");
		}

		[Test]
		public void Constructor2_Text_Null ()
		{
			ListViewItem item = new ListViewItem ((string) null);
			Assert.AreEqual (-1, item.ImageIndex, "#1");
			Assert.AreEqual (1, item.SubItems.Count, "#2");
			Assert.IsNotNull (item.SubItems [0].Text, "#3");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#4");
		}

		[Test]
		public void Constructor3_Items_Empty ()
		{
			ListViewItem item = new ListViewItem (new string [3]);
			Assert.AreEqual (-1, item.ImageIndex, "#1");
			Assert.AreEqual (3, item.SubItems.Count, "#2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#3");
			Assert.AreEqual (string.Empty, item.SubItems [1].Text, "#4");
			Assert.AreEqual (string.Empty, item.SubItems [2].Text, "#5");
		}

		[Test]
		public void Constructor3_Items_Null ()
		{
			ListViewItem item = new ListViewItem ((string []) null);
			Assert.AreEqual (-1, item.ImageIndex, "#1");
			Assert.AreEqual (1, item.SubItems.Count, "#2");
			Assert.IsNotNull (item.SubItems [0].Text, "#3");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#4");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor4_SubItems_Empty ()
		{
			new ListViewItem (new ListViewItem.ListViewSubItem [2], 3);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor4_SubItems_Null ()
		{
			new ListViewItem ((ListViewItem.ListViewSubItem []) null, 3);
		}

		[Test]
		public void Constructor5_Text_Null ()
		{
			ListViewItem item = new ListViewItem ((string) null, 2);
			Assert.AreEqual (2, item.ImageIndex, "#1");
			Assert.AreEqual (1, item.SubItems.Count, "#2");
			Assert.IsNotNull (item.SubItems [0].Text, "#3");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#4");
		}

		[Test]
		public void Constructor6_Items_Empty ()
		{
			ListViewItem item = new ListViewItem (new string [3], 5);
			Assert.AreEqual (5, item.ImageIndex, "#1");
			Assert.AreEqual (3, item.SubItems.Count, "#2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#3");
			Assert.AreEqual (string.Empty, item.SubItems [1].Text, "#4");
			Assert.AreEqual (string.Empty, item.SubItems [2].Text, "#5");
		}

		[Test]
		public void Constructor6_Items_Null ()
		{
			ListViewItem item = new ListViewItem ((string []) null, 3);
			Assert.AreEqual (3, item.ImageIndex, "#1");
			Assert.AreEqual (1, item.SubItems.Count, "#2");
			Assert.IsNotNull (item.SubItems [0].Text, "#3");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#4");
		}

		[Test]
		public void Constructor7_Items_Empty ()
		{
			Font font = new Font (FontFamily.GenericSansSerif, 6);

			ListViewItem item = new ListViewItem (new string [2], 3, Color.Red,
				Color.Blue, font);
			Assert.AreEqual (Color.Blue, item.BackColor, "#1");
			Assert.AreEqual (Color.Red, item.ForeColor, "#2");
			Assert.AreSame (font, item.Font, "#3");
			Assert.AreEqual (3, item.ImageIndex, "#4");
			Assert.AreEqual (2, item.SubItems.Count, "#5");
			Assert.IsNotNull (item.SubItems [0].Text, "#6");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#7");
			Assert.IsNotNull (item.SubItems [1].Text, "#8");
			Assert.AreEqual (string.Empty, item.SubItems [1].Text, "#9");
		}

		[Test]
		public void Constructor7_Items_Null ()
		{
			Font font = new Font (FontFamily.GenericSansSerif, 6);

			ListViewItem item = new ListViewItem ((string []) null, 3, Color.Red,
				Color.Blue, font);
			Assert.AreEqual (Color.Blue, item.BackColor, "#1");
			Assert.AreEqual (Color.Red, item.ForeColor, "#2");
			Assert.AreSame (font, item.Font, "#3");
			Assert.AreEqual (3, item.ImageIndex, "#4");
			Assert.AreEqual (1, item.SubItems.Count, "#5");
			Assert.IsNotNull (item.SubItems [0].Text, "#6");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#7");
		}

#if NET_2_0
		[Test]
		public void Constructor9_Text_Null ()
		{
			ListViewItem item = new ListViewItem ((string) null, "key");
			Assert.AreEqual (-1, item.ImageIndex, "#1");
			Assert.IsNotNull (item.ImageKey, "#2");
			Assert.AreEqual ("key", item.ImageKey, "#3");
			Assert.AreEqual (1, item.SubItems.Count, "#4");
			Assert.IsNotNull (item.SubItems [0].Text, "#5");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#6");
		}

		[Test]
		public void Constructor9_ImageKey_Null ()
		{
			ListViewItem item = new ListViewItem ("name", (string) null);
			Assert.AreEqual (-1, item.ImageIndex, "#1");
			Assert.IsNotNull (item.ImageKey, "#2");
			Assert.AreEqual (string.Empty, item.ImageKey, "#3");
			Assert.AreEqual (1, item.SubItems.Count, "#4");
			Assert.IsNotNull (item.SubItems [0].Text, "#5");
			Assert.AreEqual ("name", item.SubItems [0].Text, "#6");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor10_SubItems_Null ()
		{
			new ListViewItem ((ListViewItem.ListViewSubItem []) null, "key");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor10_SubItems_Empty ()
		{
			new ListViewItem (new ListViewItem.ListViewSubItem [2], "key");
		}

		[Test]
		public void Constructor10_ImageKey_Null ()
		{
			ListViewItem.ListViewSubItem subItemA = new ListViewItem.ListViewSubItem ();
			subItemA.Text = "A";
			ListViewItem.ListViewSubItem subItemB = new ListViewItem.ListViewSubItem ();
			subItemB.Text = "B";

			ListViewItem item = new ListViewItem (new ListViewItem.ListViewSubItem [] {
				subItemA, subItemB }, (string) null);
			Assert.AreEqual (-1, item.ImageIndex, "#1");
			Assert.IsNotNull (item.ImageKey, "#2");
			Assert.AreEqual (string.Empty, item.ImageKey, "#3");
			Assert.AreEqual (2, item.SubItems.Count, "#4");
			Assert.IsNotNull (item.SubItems [0].Text, "#5");
			Assert.AreEqual ("A", item.SubItems [0].Text, "#6");
			Assert.IsNotNull (item.SubItems [1].Text, "#7");
			Assert.AreEqual ("B", item.SubItems [1].Text, "#8");
		}
#endif

		[Test]
		public void ListViewItemDefaultValues ()
		{
			ListViewItem item = new ListViewItem ();

			Assert.AreEqual (false, item.Focused, "DefaultValues#3");
			Assert.AreEqual (false, item.Checked, "DefaultValues#4");
			Assert.AreEqual (string.Empty, item.Text, "DefaultValues#5");
			Assert.AreEqual (true, item.UseItemStyleForSubItems, "DefaultValues#6");
			Assert.AreEqual (-1, item.ImageIndex, "DefaultValues#7");
#if NET_2_0
			Assert.AreEqual (String.Empty, item.Name, "DefaultValues#8");
			Assert.AreEqual(String.Empty, item.ImageKey, "DefaultValues#9");
			Assert.AreEqual (String.Empty, item.ToolTipText, "DefaultValues#10");
#endif
		}

		[Test]
		public void ListViewItemBackColor ()
		{
			ListViewItem item = new ListViewItem ();

			ListView lv = new ListView ();
			lv.Items.Add (item);
			lv.BackColor = Color.Orange;
			Assert.AreEqual (lv.BackColor, item.BackColor, "BackColor#1");
			Assert.AreEqual (lv.BackColor, item.SubItems [0].BackColor, "BackColor#2");

			item.BackColor = Color.Navy;
			Assert.AreEqual (Color.Navy, item.BackColor, "BackColor#3");
			Assert.AreEqual (Color.Navy, item.SubItems [0].BackColor, "BackColor#4");

			item.SubItems [0].BackColor = Color.Green;
			Assert.AreEqual (Color.Green, item.BackColor, "BackColor#5");
			Assert.AreEqual (Color.Green, item.SubItems [0].BackColor, "BackColor#6");
		}

		[Test]
		public void ListViewItemForeColor ()
		{
			ListViewItem item = new ListViewItem ();

			ListView lv = new ListView ();
			lv.Items.Add (item);
			lv.ForeColor = Color.Orange;
			Assert.AreEqual (lv.ForeColor, item.ForeColor, "ForeColor#1");
			Assert.AreEqual (lv.ForeColor, item.SubItems [0].ForeColor, "ForeColor#2");

			item.ForeColor = Color.Navy;
			Assert.AreEqual (Color.Navy, item.ForeColor, "ForeColor#3");
			Assert.AreEqual (Color.Navy, item.SubItems [0].ForeColor, "ForeColor#4");

			item.SubItems [0].ForeColor = Color.Green;
			Assert.AreEqual (Color.Green, item.ForeColor, "ForeColor#5");
			Assert.AreEqual (Color.Green, item.SubItems [0].ForeColor, "ForeColor#6");
		}

		[Test]
		public void ListViewItemFocused ()
		{
			ListView lv = new ListView ();
			ListViewItem item1 = lv.Items.Add ("A");
			ListViewItem item2 = lv.Items.Add ("B");
			ListViewItem item3 = lv.Items.Add ("C");

			// Need to show form
			Form form = new Form ();
			lv.Parent = form;
			form.Show ();

			item1.Focused = true;
			Assert.AreEqual (true, item1.Focused, "#A1");
			Assert.AreEqual (false, item2.Focused, "#A2");
			Assert.AreEqual (false, item3.Focused, "#A3");
			Assert.AreEqual (item1, lv.FocusedItem, "#A4");

			item2.Focused = true;
			Assert.AreEqual (false, item1.Focused, "#B1");
			Assert.AreEqual (true, item2.Focused, "#B2");
			Assert.AreEqual (false, item3.Focused, "#B3");
			Assert.AreEqual (item2, lv.FocusedItem, "#B4");

			item3.Focused = true;
			Assert.AreEqual (false, item1.Focused, "#C1");
			Assert.AreEqual (false, item2.Focused, "#C2");
			Assert.AreEqual (true, item3.Focused, "#C3");
			Assert.AreEqual (item3, lv.FocusedItem, "#C4");

			item3.Focused = false;
			Assert.AreEqual (false, item1.Focused, "#D1");
			Assert.AreEqual (false, item2.Focused, "#D2");
			Assert.AreEqual (false, item3.Focused, "#D3");
			Assert.AreEqual (null, lv.FocusedItem, "#D4");

			form.Dispose ();
		}
	
#if NET_2_0
		[Test]
		public void ListViewItemGroup ()
		{
			ListViewGroup lvg1 = new ListViewGroup ();
			ListViewGroup lvg2 = new ListViewGroup ();
			ListViewItem lvi = new ListViewItem ();
		
			lvg1.Items.Add (lvi);
		
			Assert.AreEqual (1, lvg1.Items.Count, "#A1");
			Assert.AreEqual (lvg1, lvi.Group, "#A2");
			lvi.Group = lvg2;
		
			Assert.AreEqual (0, lvg1.Items.Count, "#B1");
			Assert.AreEqual (1, lvg2.Items.Count, "#B2");
			Assert.AreEqual (lvg2, lvi.Group, "#B3");
		}
#endif

		[Test]
		public void ListViewItemUseItemStyleForSubItems ()
		{
			ListViewItem item = new ListViewItem ();
			Assert.AreEqual (1, item.SubItems.Count);

			// UseitemStyleForSubItems works at draw level
			item.UseItemStyleForSubItems = true;

			ListViewItem.ListViewSubItem subitem0 = item.SubItems [0];
			Color subitem0_back_color = subitem0.BackColor = Color.Black;
			Color subitem0_fore_color = subitem0.ForeColor = Color.White;

			Assert.AreEqual (subitem0_back_color, item.SubItems [0].BackColor, "UseItemStyleForSubItems#1");
			Assert.AreEqual (subitem0_fore_color, item.SubItems [0].ForeColor, "UseItemStyleForSubItems#2");
			Assert.AreEqual (item.BackColor, item.SubItems [0].BackColor, "UseItemStyleForSubItems#3");
			Assert.AreEqual (item.ForeColor, item.SubItems [0].ForeColor, "UseItemStyleForSubItems#4");

			ListViewItem.ListViewSubItem subitem1 = item.SubItems.Add ("SubItem");
			Color subitem1_back_color = subitem1.BackColor = Color.Blue;
			Color subitem1_fore_color = subitem1.ForeColor = Color.Gray;

			Assert.AreEqual (subitem1_back_color, subitem1.BackColor, "UseItemStyleForSubItem#5");
			Assert.AreEqual (subitem1_fore_color, subitem1.ForeColor, "UseItemStyleForSubItem#6");
		}

		[Test]
		public void ListViewItemTestClone ()
		{
			ListViewItem item1 = new ListViewItem ("Hello");
			item1.ForeColor = Color.Blue;
			item1.BackColor = Color.Red;
			item1.Font = new Font ("Arial", 14);
			item1.SubItems.Add ("Element2");
#if NET_2_0
			item1.ToolTipText = item1.Text;
#endif

			ListViewItem item2 =  (ListViewItem) item1.Clone ();
			Assert.AreEqual (Color.Blue, item2.ForeColor, "#1");
			Assert.AreEqual (Color.Red, item2.BackColor, "#2");
			Assert.AreEqual ("Hello", item2.Text, "#3");
			Assert.AreEqual (item1.Font, item2.Font, "#4");
			Assert.AreEqual (2, item2.SubItems.Count, "#5");
			Assert.AreEqual ("Hello", item2.SubItems [0].Text, "#6");
			Assert.AreEqual ("Element2", item2.SubItems[1].Text, "#7");
#if NET_2_0
			Assert.AreEqual (item1.ToolTipText, item2.ToolTipText, "#8");
#endif
		}

#if NET_2_0
		[Test]
		public void ListViewItemTestImageIndex()
		{
			ListViewItem item1 = new ListViewItem();

			item1.ImageKey = "Key1";
			item1.ImageIndex = 0;
			Assert.AreEqual(String.Empty, item1.ImageKey, "ImageIndex#1");

			item1.ImageIndex = 0;
			item1.ImageKey = "Key1";
			Assert.AreEqual (-1, item1.ImageIndex, "ImageIndex#2");

			item1.ImageKey = "Key1";
			item1.ImageIndex = -1;
			Assert.AreEqual (String.Empty, item1.ImageKey, "ImageIndex#3");

			item1.ImageIndex = 0;
			item1.ImageKey = String.Empty;
			Assert.AreEqual (-1, item1.ImageIndex, "ImageIndex#4");
		}

		[Test]
		public void ListViewItemToolTipText ()
		{
			ListViewItem item1 = new ListViewItem();
			item1.ToolTipText = null;
			Assert.AreEqual (String.Empty, item1.ToolTipText, "ToolTipText#1");
		}
#endif
	}

	[TestFixture]
	public class ListViewSubItemTest
	{
		[Test]
		public void ListViewSubItemConstructorTest ()
		{
			ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem (null, null);
			Assert.AreEqual (String.Empty, subItem.Text, "#A1");

			ListViewItem.ListViewSubItem subItem2 = new ListViewItem.ListViewSubItem (null, "SubItem2");
			Assert.AreEqual ("SubItem2", subItem2.Text, "#A2");

			Font fnt = new Font ("Arial", 12);
			ListViewItem.ListViewSubItem subItem3 = new ListViewItem.ListViewSubItem (null, "SubItem3", 
				Color.Blue, Color.Green, fnt);
			Assert.AreEqual ("SubItem3", subItem3.Text, "#A3");
			Assert.AreEqual (Color.Green, subItem3.BackColor, "#A4");
			Assert.AreEqual (Color.Blue, subItem3.ForeColor, "#A5");
			Assert.AreEqual (fnt, subItem3.Font, "#A6");
		}

		[Test]
		public void ListViewSubItemPropertiesTest ()
		{
			ListViewItem.ListViewSubItem subItem1 = new ListViewItem.ListViewSubItem ();
			Assert.AreEqual (string.Empty, subItem1.Text, "#A1");
			
			subItem1.Text = null;
			Assert.AreEqual (string.Empty, subItem1.Text, "#B1");
			subItem1.Text = "test";
			Assert.AreEqual ("test", subItem1.Text, "#B2");
		}
	}

	[TestFixture]
	public class ListViewSubItemCollectionTest
	{
		[Test]
		public void AddRange1 ()
		{
			ListViewItem item = new ListViewItem ();
			ListViewItem.ListViewSubItem subItemA = item.SubItems.Add ("A");

			Assert.AreEqual (2, item.SubItems.Count, "#A1");
			Assert.IsNotNull (item.SubItems [0], "#A2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#A3");

			ListViewItem.ListViewSubItem subItemB = new ListViewItem.ListViewSubItem ();
			subItemB.Text = "B";
			ListViewItem.ListViewSubItem subItemC = new ListViewItem.ListViewSubItem ();
			subItemB.Text = "C";

			item.SubItems.AddRange (new ListViewItem.ListViewSubItem [] {
				subItemB, null, subItemC });
			Assert.AreEqual (4, item.SubItems.Count, "#B1");
			Assert.IsNotNull (item.SubItems [0], "#B2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#B3");
			Assert.IsNotNull (item.SubItems [1], "#B3");
			Assert.AreSame (subItemA, item.SubItems [1], "#B4");
			Assert.IsNotNull (item.SubItems [2], "#B5");
			Assert.AreSame (subItemB, item.SubItems [2], "#B6");
			Assert.IsNotNull (item.SubItems [3], "#B7");
			Assert.AreSame (subItemC, item.SubItems [3], "#B8");
		}

		[Test]
		public void AddRange1_Null ()
		{
			ListViewItem item = new ListViewItem ();
			try {
				item.SubItems.AddRange ((ListViewItem.ListViewSubItem []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("items", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		public void AddRange2 ()
		{
			string subItemAText = "A";
			string subItemBText = "B";
			string subItemCText = "B";

			ListViewItem item = new ListViewItem ();
			item.SubItems.Add (subItemAText);

			Assert.AreEqual (2, item.SubItems.Count, "#A1");
			Assert.IsNotNull (item.SubItems [0], "#A2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#A3");
			Assert.IsNotNull (item.SubItems [1], "#A4");
			Assert.AreEqual (subItemAText, item.SubItems [1].Text, "#A5");

			item.SubItems.AddRange (new string [] { subItemBText, null, subItemCText });
			Assert.AreEqual (4, item.SubItems.Count, "#B1");
			Assert.IsNotNull (item.SubItems [0], "#B2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#B3");
			Assert.IsNotNull (item.SubItems [1], "#B4");
			Assert.AreSame (subItemAText, item.SubItems [1].Text, "#B5");
			Assert.IsNotNull (item.SubItems [2], "#B6");
			Assert.AreSame (subItemBText, item.SubItems [2].Text, "#B7");
			Assert.IsNotNull (item.SubItems [3], "#B8");
			Assert.AreSame (subItemCText, item.SubItems [3].Text, "#B9");
		}

		[Test]
		public void AddRange2_Null ()
		{
			ListViewItem item = new ListViewItem ();
			try {
				item.SubItems.AddRange ((string []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("items", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		public void AddRange3 ()
		{
			string subItemAText = "A";
			string subItemBText = "B";
			string subItemCText = "B";
			Font font = new Font ("Arial", 14);

			ListViewItem item = new ListViewItem ();
			item.SubItems.Add (subItemAText);

			Assert.AreEqual (2, item.SubItems.Count, "#A1");
			Assert.IsNotNull (item.SubItems [0], "#A2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#A3");
			Assert.IsNotNull (item.SubItems [1], "#A4");
			Assert.AreEqual (subItemAText, item.SubItems [1].Text, "#A5");

			item.SubItems.AddRange (new string [] { subItemBText, null, subItemCText },
				Color.Blue, Color.Red, font);
			Assert.AreEqual (4, item.SubItems.Count, "#B1");
			Assert.IsNotNull (item.SubItems [0], "#B2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#B3");
			Assert.IsNotNull (item.SubItems [1], "#C1");
			Assert.AreSame (subItemAText, item.SubItems [1].Text, "#C2");
			Assert.IsNotNull (item.SubItems [2], "#D1");
			Assert.AreSame (subItemBText, item.SubItems [2].Text, "#D2");
			Assert.AreEqual (Color.Blue, item.SubItems [2].ForeColor, "#D3");
			Assert.AreEqual (Color.Red, item.SubItems [2].BackColor, "#D4");
			Assert.AreSame (font, item.SubItems [2].Font, "#D5");
			Assert.IsNotNull (item.SubItems [3], "#E1");
			Assert.AreSame (subItemCText, item.SubItems [3].Text, "#E2");
			Assert.AreEqual (Color.Blue, item.SubItems [3].ForeColor, "#E3");
			Assert.AreEqual (Color.Red, item.SubItems [3].BackColor, "#E4");
			Assert.AreSame (font, item.SubItems [3].Font, "#E6");
		}

		[Test]
		public void AddRange3_Items_Null ()
		{
			ListViewItem item = new ListViewItem ();
			try {
				item.SubItems.AddRange ((string []) null, Color.Blue, Color.Red,
					new Font ("Arial", 14));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("items", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		public void AddRange4 ()
		{
			ListViewItem item = new ListViewItem ();
			Assert.AreEqual (1, item.SubItems.Count, "#1");

			item.SubItems.AddRange (new string [3]);
			Assert.AreEqual (1, item.SubItems.Count, "#2");

			item.SubItems.AddRange (new ListViewItem.ListViewSubItem [3]);
			Assert.AreEqual (1, item.SubItems.Count, "#3");
		}

		[Test]
		public void Clear ()
		{
			ListViewItem item = new ListViewItem ();
			item.SubItems.AddRange (new string [] { "A", "B", "C" });
			item.SubItems.Clear ();
			Assert.AreEqual (1, item.SubItems.Count, "#1");
			Assert.IsNotNull (item.SubItems [0].Text, "#2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#3");
		}

		[Test]
		public void RemoveAt ()
		{
			ListViewItem item = new ListViewItem ();
			item.SubItems.AddRange (new string [] { "A", "B" });
			Assert.AreEqual (3, item.SubItems.Count, "#A1");
			Assert.IsNotNull (item.SubItems [0].Text, "#A2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#A3");
			Assert.IsNotNull (item.SubItems [1].Text, "#A4");
			Assert.AreEqual ("A", item.SubItems [1].Text, "#A5");
			Assert.IsNotNull (item.SubItems [2].Text, "#A6");
			Assert.AreEqual ("B", item.SubItems [2].Text, "#A7");

			item.SubItems.RemoveAt (1);

			Assert.AreEqual (2, item.SubItems.Count, "#B1");
			Assert.IsNotNull (item.SubItems [0].Text, "#B2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#B3");
			Assert.IsNotNull (item.SubItems [1].Text, "#B4");
			Assert.AreEqual ("B", item.SubItems [1].Text, "#B5");

			item.SubItems.RemoveAt (0);

			Assert.AreEqual (1, item.SubItems.Count, "#C1");
			Assert.IsNotNull (item.SubItems [0].Text, "#C2");
			Assert.AreEqual ("B", item.SubItems [0].Text, "#C3");

			item.SubItems.RemoveAt (0);

			Assert.AreEqual (1, item.SubItems.Count, "#D1");
			Assert.IsNotNull (item.SubItems [0].Text, "#D2");
			Assert.AreEqual (string.Empty, item.SubItems [0].Text, "#D3");
		}
	}
}
