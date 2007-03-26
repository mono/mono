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

			ListViewItem item2 =  (ListViewItem) item1.Clone ();
			Assert.AreEqual (item2.ForeColor, Color.Blue, "Clone#1");
			Assert.AreEqual (item2.BackColor, Color.Red, "Clone#2");
			Assert.AreEqual (item2.Text, "Hello", "Clone#3");
			Assert.AreEqual (item2.Font, item1.Font, "Clone#4");
			Assert.AreEqual (2, item2.SubItems.Count, "Clone#5");
			Assert.AreEqual (item2.SubItems[1].Text, "Element2", "Clone#6");
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
	}
}
