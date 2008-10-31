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
//	Rolf Bjarne Kvinge  (RKvinge@novell.com)
//


#if NET_2_0

using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridViewRowHeaderTest : TestHelper
	{

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetClipboardContentTestException ()
		{
			DataGridViewRowHeaderClipboardCell cell = new DataGridViewRowHeaderClipboardCell ();
			
			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewColumn col = new DataGridViewColumn ();
				col.CellTemplate = new DataGridViewTextBoxCell ();
				dgv.Columns.Add (col);
				DataGridViewRow row = new DataGridViewRow ();
				row.HeaderCell = cell;
				dgv.Rows.Add (row);
				dgv.Rows [0].SetValues ("abc");
				dgv.Rows [0].Cells [0].Selected = true;

				cell = dgv.Rows [0].HeaderCell as DataGridViewRowHeaderClipboardCell;
				cell.GetClipboardContentPublic (-1, false, false, false, false, "Text");

			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetClipboardContentTestException2 ()
		{
			DataGridViewRowHeaderClipboardCell cell = new DataGridViewRowHeaderClipboardCell ();

			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewColumn col = new DataGridViewColumn ();
				col.CellTemplate = new DataGridViewTextBoxCell ();
				dgv.Columns.Add (col);
				DataGridViewRow row = new DataGridViewRow ();
				row.HeaderCell = cell;
				dgv.Rows.Add (row);
				dgv.Rows [0].SetValues ("abc");
				dgv.Rows [0].Cells [0].Selected = true;

				cell = dgv.Rows [0].HeaderCell as DataGridViewRowHeaderClipboardCell;
				cell.GetClipboardContentPublic (2, false, false, false, false, "Text");

			}
		}
		
		
		public class DataGridViewRowHeaderClipboardCell : DataGridViewRowHeaderCell
		{
			public DataGridViewRowHeaderClipboardCell ()
			{
			}

			public object GetClipboardContentPublic (int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format)
			{
				return GetClipboardContent (rowIndex, firstCell, lastCell, inFirstRow, inLastRow, format);
			}

			public override Type FormattedValueType
			{
				get
				{
					return typeof (string);
				}
			}
		}

		/* font dependent
		[Test]
		public void MethodGetContentBounds ()
		{
			BaseCell c = new BaseCell ();
			Assert.AreEqual (Rectangle.Empty, c.GetContentBounds (c.RowIndex), "A1");
			c.Value = "hello there";
			
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.HeaderCell = c;
			dgv.Rows.Add (row);

			Assert.AreEqual (new Size (41, 22), dgv.Rows[0].HeaderCell.Size, "A1-1");
			Assert.AreEqual (new Rectangle (24, 2, 11, 18), dgv.Rows[0].HeaderCell.GetContentBounds (dgv.Rows[0].HeaderCell.RowIndex), "A2");

			dgv.Rows[0].HeaderCell.Value = "whoa whoa whoa whoa";
			Assert.AreEqual (new Rectangle (24, 2, 11, 18), dgv.Rows[0].HeaderCell.GetContentBounds (dgv.Rows[0].HeaderCell.RowIndex), "A3");
		}
		*/
		
		[Test]
		public void MethodGetErrorIconBounds ()
		{
			Bitmap b = new Bitmap (1, 1);
			Graphics g = Graphics.FromImage (b);

			BaseCell c = new BaseCell ();
			c.ErrorText = "Yo!";
			Assert.AreEqual (Rectangle.Empty, c.PublicGetErrorIconBounds (g, c.Style, c.RowIndex), "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.HeaderCell = c;
			dgv.Rows.Add (row);

			Assert.AreEqual (Rectangle.Empty, (dgv.Rows[0].HeaderCell as BaseCell).PublicGetErrorIconBounds (g, dgv.Rows[0].HeaderCell.InheritedStyle, dgv.Rows[0].HeaderCell.RowIndex), "A2");

			dgv.Rows[0].ErrorText = "Danger!";
			Assert.AreEqual (new Rectangle (24, 5, 12, 11), (dgv.Rows[0].HeaderCell as BaseCell).PublicGetErrorIconBounds (g, dgv.Rows[0].HeaderCell.InheritedStyle, dgv.Rows[0].HeaderCell.RowIndex), "A3");
			Assert.AreEqual ("Danger!", (dgv.Rows[0].HeaderCell as BaseCell).PublicGetErrorText (dgv.Rows[0].HeaderCell.RowIndex), "A4");

			g.Dispose ();
			b.Dispose ();
		}
		
		[Test]
		public void MethodGetInheritedContextMenuStrip ()
		{
			BaseCell c = new BaseCell ();
			Assert.AreEqual (null, c.GetInheritedContextMenuStrip (c.RowIndex), "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.HeaderCell = c;
			dgv.Rows.Add (row);

			Assert.AreEqual (null, dgv.Rows[0].HeaderCell.GetInheritedContextMenuStrip (dgv.Rows[0].HeaderCell.RowIndex), "A2");

			ContextMenuStrip cms1 = new ContextMenuStrip ();
			cms1.Items.Add ("Moose");
			dgv.ContextMenuStrip = cms1;

			Assert.AreSame (cms1, dgv.Rows[0].HeaderCell.GetInheritedContextMenuStrip (dgv.Rows[0].HeaderCell.RowIndex), "A3");
			
			ContextMenuStrip cms2 = new ContextMenuStrip ();
			cms2.Items.Add ("Moose");

			dgv.Rows[0].ContextMenuStrip = cms2;
			Assert.AreSame (cms1, dgv.Rows[0].HeaderCell.GetInheritedContextMenuStrip (dgv.Rows[0].HeaderCell.RowIndex), "A4");

			dgv.Rows[0].HeaderCell.ContextMenuStrip = cms2;
			Assert.AreSame (cms2, dgv.Rows[0].HeaderCell.GetInheritedContextMenuStrip (dgv.Rows[0].HeaderCell.RowIndex), "A5");
		}

		/* font dependent
		[Test]
		public void PreferredSize ()
		{
			BaseCell c = new BaseCell ();
			Assert.AreEqual (new Size (-1, -1), c.PreferredSize, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.HeaderCell = c;
			dgv.Rows.Add (row);

			Assert.AreEqual (new Size (39, 17), dgv.Rows[0].HeaderCell.PreferredSize, "A2");

			dgv.Rows[0].HeaderCell.Value = "bob";
			Assert.AreEqual (new Size (73, 17), dgv.Rows[0].HeaderCell.PreferredSize, "A3");

			dgv.Rows[0].HeaderCell.Value = "roasted quail";
			Assert.AreEqual (new Size (115, 17), dgv.Rows[0].HeaderCell.PreferredSize, "A4");
		}
		*/
		
		private class BaseCell : DataGridViewRowHeaderCell
		{
			public Rectangle PublicGetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
			{ return GetContentBounds (graphics, cellStyle, rowIndex); }
			public Rectangle PublicGetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
			{ return GetErrorIconBounds (graphics, cellStyle, rowIndex); }
			public string PublicGetErrorText (int rowIndex)
			{ return GetErrorText (rowIndex); }
			public Rectangle PublicBorderWidths (DataGridViewAdvancedBorderStyle advancedBorderStyle)
			{ return BorderWidths (advancedBorderStyle); }
		}
	}
}
#endif