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
	public class DataGridViewColumnHeaderTest : TestHelper
	{
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetClipboardContentTestException ()
		{
			DataGridViewColumnHeaderClipboardCell cell = new DataGridViewColumnHeaderClipboardCell ();
			
			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewColumn col = new DataGridViewColumn ();
				col.CellTemplate = new DataGridViewTextBoxCell ();
				col.HeaderCell = cell;
				dgv.Columns.Add (col);
				DataGridViewRow row = new DataGridViewRow ();
				dgv.Rows.Add (row);
				dgv.Rows [0].SetValues ("abc");
				dgv.Rows [0].Cells [0].Selected = true;

				cell = dgv.Columns [0].HeaderCell as DataGridViewColumnHeaderClipboardCell;
				cell.GetClipboardContentPublic (0, false, false, false, false, "Text");

			}
		}
		
		public class DataGridViewColumnHeaderClipboardCell : DataGridViewColumnHeaderCell
		{
			public DataGridViewColumnHeaderClipboardCell ()
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
	}
}
#endif