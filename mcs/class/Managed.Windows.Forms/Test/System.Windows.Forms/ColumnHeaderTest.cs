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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ColumnHeaderTest
	{
		[Test]
		public void DefaultValuesTest ()
		{
			ColumnHeader col = new ColumnHeader ();

			Assert.IsNull (col.ListView, "1");
			Assert.AreEqual (-1, col.Index, "2");
			Assert.AreEqual ("ColumnHeader", col.Text, "3");
			Assert.AreEqual (HorizontalAlignment.Left, col.TextAlign, "4");
#if NET_2_0
			Assert.AreEqual (-1, col.DisplayIndex, "5");
			Assert.AreEqual (-1, col.ImageIndex, "6");
			Assert.AreEqual (String.Empty, col.ImageKey, "7");
			Assert.IsNull (col.ImageList, "8");
			Assert.AreEqual (String.Empty, col.Name, "9");
			Assert.IsNull (col.Tag, "10");
#endif
		}

#if NET_2_0
		[Test]
		public void DisplayIndexTest ()
		{
			ColumnHeader col = new ColumnHeader ();
			col.DisplayIndex = -66;
			col.DisplayIndex = 66;

			ListView lv = new ListView ();
			lv.Columns.Add (col);

			try {
				col.DisplayIndex = -1;
				Assert.Fail ("1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				col.DisplayIndex = lv.Columns.Count;
				Assert.Fail ("2");
			} catch (ArgumentOutOfRangeException) {
			}

			Assert.AreEqual (0, col.DisplayIndex, "3");
		}
#endif

		[Test]
		public void WidthTest ()
		{
			ColumnHeader col = new ColumnHeader ();
			col.Text = "Column text";

			ListView lv = new ListView ();
			lv.Items.Add ("Item text");
			lv.View = View.Details;
			lv.Columns.Add (col);
			lv.CreateControl ();

			col.Width = -1;
			Assert.IsTrue (col.Width > 0, "1");

			col.Width = -2;
			Assert.IsTrue (col.Width > 0, "2");
		}
	}
}

