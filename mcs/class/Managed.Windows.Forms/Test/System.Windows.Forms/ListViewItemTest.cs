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
		}

		[Test]
		public void ListViewItemDefaultValues ()
		{
			ListViewItem item = new ListViewItem ();

			Assert.AreEqual (false, item.Focused, "DefaultValues#3");
			Assert.AreEqual (false, item.Checked, "DefaultValues#4");
			Assert.AreEqual (string.Empty, item.Text, "DefaultValues#5");
			Assert.AreEqual (true, item.UseItemStyleForSubItems, "DefaultValues#6");
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
	}
}
