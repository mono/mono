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
	public class DataGridViewRowCollectionTest : TestHelper
	{
		private DataGridView CreateAndFill ()
		{
			DataGridView dgv = DataGridViewCommon.CreateAndFill ();
			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (new DataGridViewComboBoxCell ());
			row.Cells.Add (new DataGridViewComboBoxCell ());
			dgv.Rows.Add (row);
			return dgv;
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("System.Windows.Forms.DataGridViewRowCollection", new DataGridViewRowCollection (null).ToString (), "A");

			using (DataGridView dgv = CreateAndFill ()) {
				Assert.AreEqual ("System.Windows.Forms.DataGridViewRowCollection", dgv.Rows.ToString (), "B");
			}

		}
		
		[Test]
		public void CtorTest ()
		{
			DataGridViewRowCollection rc;
			
			rc = new DataGridViewRowCollection (null);
			Assert.AreEqual (0, rc.Count, "#01");
			
			using (DataGridView dgv = new DataGridView ()) {
				rc = new DataGridViewRowCollection (dgv);
				Assert.AreEqual (0, rc.Count, "#02");
				Assert.IsTrue (rc != dgv.Rows, "#03");
			}
		}
		
		[Test]
		[NUnit.Framework.Category ("NotWorking")]	// Don't currently support shared rows
		public void AddTest ()
		{
			DataGridViewRow row;
			DataGridViewCell cell;
			
			using (DataGridView dgv = new DataGridView ()) {
				dgv.Columns.Add ("a", "A");
				row = new DataGridViewRow ();
				dgv.Rows.Add (row);
				Assert.AreEqual (-1, row.Index, "#01");
			}

			using (DataGridView dgv = new DataGridView ()) {
				dgv.Columns.Add ("a", "A");
				row = new DataGridViewRow ();
				cell = new DataGridViewTextBoxCell ();
				cell.Value = "abc";
				row.Cells.Add (cell);
				dgv.Rows.Add (row);
				Assert.AreEqual (0, row.Index, "#02");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RemoveAtNewRowException ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("A", "A");
			dgv.Rows.RemoveAt (0);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RemoveNewRowException ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("A", "A");
			dgv.Rows.Remove (dgv.Rows[0]);
		}


		[Test]	// bug #442181
		public void RemoveNewRowClear ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("A", "A");
			dgv.Rows.Clear ();
			
			Assert.AreEqual (1, dgv.Rows.Count, "A1");

			// This was crashing in the bug
			dgv.Sort (dgv.Columns[0], ListSortDirection.Ascending);
		}
		
		[Test]  // bug #448005
		public void ClearRows ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("A", "A");
			dgv.Columns.Add ("A2", "A2");

			dgv.Rows.Add (1, 2);
			dgv.Rows.Add (1, 2);
			dgv.Rows.Add (1, 2);
			dgv.Rows.Add (1, 2);
			dgv.Rows.Add (1, 2);

			dgv.Rows.Clear ();

			Assert.AreEqual (1, dgv.Rows.Count, "A1");
		}

		[Test]  // bug xamarin #821
		public void TestAddedRowType ()
		{
			DataGridView dgv = new DataGridView ();
			DataGridViewCheckBoxColumn col1 = new DataGridViewCheckBoxColumn ();
			DataGridViewTextBoxColumn col2 = new DataGridViewTextBoxColumn ();
			DataGridViewTextBoxColumn col3 = new DataGridViewTextBoxColumn ();
			dgv.Columns.AddRange (new DataGridViewColumn[] { col1, col2, col3 });

			dgv.Rows.Add (new object[] { false, "one", 12 });
			dgv.Rows.Insert (0, new object[] { false, "zero", 18 });

			foreach (DataGridViewRow row in dgv.Rows)
			{
				int index = row.Index;
				Assert.IsInstanceOfType (typeof (DataGridViewCheckBoxCell), row.Cells[0], "#" + index + "A");
				Assert.IsInstanceOfType (typeof (DataGridViewTextBoxCell), row.Cells[1], "#" + index + "B");
				Assert.IsInstanceOfType (typeof (DataGridViewTextBoxCell), row.Cells[2], "#" + index + "C");
			}
		}
	}
}
#endif