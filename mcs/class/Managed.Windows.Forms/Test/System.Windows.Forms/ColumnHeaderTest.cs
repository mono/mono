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

		[Test] 
		public void DisplayIndex_ListView_Disposed ()
		{
			ListView lv = new ListView ();
			lv.View = View.Details;
			ColumnHeader colA = new ColumnHeader ();
			lv.Columns.Add (colA);
			ColumnHeader colB = new ColumnHeader ();
			lv.Columns.Add (colB);
			lv.Dispose ();
			colA.DisplayIndex = 1;
			Assert.AreEqual (1, colA.DisplayIndex, "#1");
			Assert.AreEqual (1, colB.DisplayIndex, "#2");
		}

		[Test]
		public void ImageIndex_Invalid ()
		{
			ColumnHeader col = new ColumnHeader ();
			col.ImageIndex = 2;
			try {
				col.ImageIndex = -2;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("ImageIndex", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
			Assert.AreEqual (2, col.ImageIndex, "#7");
		}

		[Test]
		public void ImageKey ()
		{
			ColumnHeader col = new ColumnHeader ();
			Assert.AreEqual (string.Empty, col.ImageKey, "#1");
			col.ImageKey = "test";
			Assert.AreEqual ("test", col.ImageKey, "#2");
			col.ImageKey = null;
			Assert.AreEqual (string.Empty, col.ImageKey, "#3");
		}

		[Test]
		public void ImageKeyAndImageIndexInteraction ()
		{
			ColumnHeader col = new ColumnHeader ();
			col.ImageIndex = 1;
			Assert.AreEqual (1, col.ImageIndex, "#A1");
			Assert.AreEqual (string.Empty, col.ImageKey, "#A2");
			col.ImageKey = "test";
			Assert.AreEqual (-1, col.ImageIndex, "#B1");
			Assert.AreEqual ("test", col.ImageKey, "#B2");
			col.ImageIndex = 2;
			Assert.AreEqual (2, col.ImageIndex, "#C1");
			Assert.AreEqual (string.Empty, col.ImageKey, "#C2");
			col.ImageKey = null;
			Assert.AreEqual (-1, col.ImageIndex, "#D1");
			Assert.AreEqual (string.Empty, col.ImageKey, "#D2");
		}

		[Test]
		public void ImageList ()
		{
			ColumnHeader col = new ColumnHeader ();
			Assert.IsNull (col.ImageList, "#1");

			ListView lv = new ListView ();
			lv.View = View.Details;
			ImageList small = new ImageList ();
			lv.SmallImageList = small;
			ImageList large = new ImageList ();
			lv.LargeImageList = large;
			lv.Columns.Add (col);
			Assert.IsNotNull (col.ImageList, "#2");
			Assert.AreSame (small, col.ImageList, "#3");
		}

		[Test]
		public void ImageList_ListView_Disposed ()
		{
			ListView lv = new ListView ();
			lv.View = View.Details;
			ImageList small = new ImageList ();
			lv.SmallImageList = small;
			ImageList large = new ImageList ();
			lv.LargeImageList = large;
			ColumnHeader col = new ColumnHeader ();
			lv.Columns.Add (col);
			lv.Dispose ();
			Assert.IsNull (col.ImageList);
		}
#endif

		[Test]
		public void Index_ListView_Disposed ()
		{
			ListView lv = new ListView ();
			lv.View = View.Details;
			ColumnHeader colA = new ColumnHeader ();
			lv.Columns.Add (colA);
			ColumnHeader colB = new ColumnHeader ();
			lv.Columns.Add (colB);
			lv.Dispose ();
			Assert.AreEqual (-1, colA.Index, "#1");
			Assert.AreEqual (-1, colB.Index, "#2");
		}

#if NET_2_0
		[Test]
		public void Name ()
		{
			ColumnHeader col = new ColumnHeader ();
			Assert.AreEqual (string.Empty, col.Name, "#1");
			col.Name = "Address";
			Assert.AreEqual ("Address", col.Name, "#2");
			col.Name = null;
			Assert.AreEqual (string.Empty, col.Name, "#3");
		}

		[Test]
		public void Tag ()
		{
			ColumnHeader col = new ColumnHeader ();
			Assert.IsNull (col.Tag, "#1");
			col.Tag = "whatever";
			Assert.AreEqual ("whatever", col.Tag, "#2");
			col.Tag = null;
			Assert.IsNull (col.Tag, "#3");
		}
#endif

		[Test]
		public void Text_ListView_Disposed ()
		{
			ListView lv = new ListView ();
			lv.View = View.Details;
			ColumnHeader col = new ColumnHeader ();
			lv.Columns.Add (col);
			lv.Dispose ();
			col.Text = "whatever";
			Assert.AreEqual ("whatever", col.Text);
		}

		[Test]
		public void TextAlign_ListView_Disposed ()
		{
			ListView lv = new ListView ();
			lv.View = View.Details;
			ColumnHeader col = new ColumnHeader ();
			lv.Columns.Add (col);
			lv.Dispose ();
			col.TextAlign = HorizontalAlignment.Right;
			Assert.AreEqual (HorizontalAlignment.Right, col.TextAlign);
		}

		[Test]
		public void ToStringTest ()
		{
			ListView lv = new ListView ();
			lv.SmallImageList = new ImageList ();
			ColumnHeader col = new ColumnHeader ();
#if NET_2_0
			col.DisplayIndex = 3;
			col.ImageIndex = 2;
			col.Name = "address_col";
			col.Tag = DateTime.Now;
#endif
			col.Text = "Address";
			col.TextAlign = HorizontalAlignment.Right;
			col.Width = 30;
			lv.Columns.Add (col);
			Assert.AreEqual ("ColumnHeader: Text: Address", col.ToString ());
		}

		[Test]
		[Category ("NotWorking")]
		public void WidthDefault ()
		{
			ColumnHeader col = new ColumnHeader ();
			Assert.AreEqual (60, col.Width);
		}

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
			Assert.IsTrue (col.Width > 0, "#1");

			col.Width = -2;
			Assert.IsTrue (col.Width > 0, "#2");
		}

		[Test]
		public void Width_ListView_Disposed ()
		{
			ListView lv = new ListView ();
			lv.View = View.Details;
			ColumnHeader col = new ColumnHeader ();
			lv.Columns.Add (col);
			lv.Dispose ();
			col.Width = 10;
			Assert.AreEqual (10, col.Width);
		}
	}
}
