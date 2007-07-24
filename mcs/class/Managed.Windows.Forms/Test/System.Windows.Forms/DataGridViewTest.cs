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
// Copyright (c) 2005, 2006, 2007 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//	Daniel Nauck    (dna(at)mono-project(dot)de)


#if NET_2_0

using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewTest {
		
		private DataGridView grid = null;

		[SetUp]
		public void GetReady() 
		{
			grid = new DataGridView();
		}

		[TearDown]
		public void Clean()
		{
			grid.Dispose ();
		}

		[Test]
		public void bug_81918 ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewColumn col = new DataGridViewComboBoxColumn ();
				
				dgv.Columns.Add (col);
				
				dgv.Rows.Add ("a");
				
				DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell) dgv [0, 0];
			}
		}

		[Test]
		public void TestDefaultValues () {
			DataGridView grid = new DataGridView ();
			Assert.AreEqual (true, grid.AllowUserToAddRows, "#A1");
			Assert.AreEqual (true, grid.AllowUserToDeleteRows, "#A2");
			Assert.AreEqual (false, grid.AllowUserToOrderColumns, "#A3");
			Assert.AreEqual (true, grid.AllowUserToResizeColumns, "#A4");
			Assert.AreEqual (true, grid.AllowUserToResizeRows, "#A5");
			Assert.AreEqual (new DataGridViewCellStyle(), grid.AlternatingRowsDefaultCellStyle, "#A6");
			Assert.AreEqual (true, grid.AutoGenerateColumns, "#A7");
			Assert.AreEqual (DataGridViewAutoSizeRowsMode.None, grid.AutoSizeRowsMode, "#A8");
			Assert.AreEqual (Control.DefaultBackColor, grid.BackColor, "#A9");
			Assert.AreEqual (SystemColors.AppWorkspace, grid.BackgroundColor, "#A10");
			Assert.AreEqual (BorderStyle.FixedSingle, grid.BorderStyle, "#A11");
			Assert.AreEqual (DataGridViewClipboardCopyMode.EnableWithAutoHeaderText, grid.ClipboardCopyMode, "#A12");
			Assert.AreEqual (DataGridViewColumnHeadersHeightSizeMode.EnableResizing, grid.ColumnHeadersHeightSizeMode, "#A21");
			Assert.AreEqual (true, grid.ColumnHeadersVisible, "#A22");
			Assert.AreEqual (String.Empty, grid.DataMember, "#A23");
			Assert.AreEqual (DataGridViewEditMode.EditOnKeystrokeOrF2, grid.EditMode, "#A31");
			Assert.AreEqual (Control.DefaultFont, grid.Font, "#A32");
			Assert.AreEqual (Control.DefaultForeColor, grid.ForeColor, "#A33");
			Assert.AreEqual (Color.FromKnownColor(KnownColor.ControlDark), grid.GridColor, "#A34");
			Assert.AreEqual (true, grid.MultiSelect, "#A35");
			Assert.AreEqual (grid.Rows.Count - 1, grid.NewRowIndex, "#A36");
			Assert.AreEqual (Padding.Empty, grid.Padding, "#A37");
			Assert.AreEqual (false, grid.ReadOnly, "#A38");
			Assert.AreEqual (true, grid.RowHeadersVisible, "#A39");
			Assert.AreEqual (41, grid.RowHeadersWidth, "#A40");
			Assert.AreEqual (DataGridViewSelectionMode.RowHeaderSelect, grid.SelectionMode, "#A41");
			Assert.AreEqual (true, grid.ShowCellErrors, "#A42");
			Assert.AreEqual (true, grid.ShowEditingIcon, "#A43");
			Assert.AreEqual (Cursors.Default, grid.UserSetCursor, "#A44");
			Assert.AreEqual (false, grid.VirtualMode, "#A45");
		}

		#region AutoSizeColumnsModeExceptions

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestAutoSizeColumnsModeInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill | DataGridViewAutoSizeColumnsMode.None;
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestAutoSizeColumnsModeInvalidOperationException1 () {
			DataGridView grid = new DataGridView();
			grid.ColumnHeadersVisible = false;
			DataGridViewColumn col = new DataGridViewColumn();
			col.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
			grid.Columns.Add(col);
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestAutoSizeColumnsModeInvalidOperationException2 () {
			DataGridView grid = new DataGridView();
			DataGridViewColumn col = new DataGridViewColumn();
			col.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
			col.Frozen = true;
			grid.Columns.Add(col);
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		}

		#endregion

		#region AutoSizeRowsModeExceptions

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestAutoSizeRowsModeInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.AutoSizeRowsMode = (DataGridViewAutoSizeRowsMode) 4;
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestAutoSizeRowsModeInvalidOperationException1 () {
			DataGridView grid = new DataGridView();
			grid.RowHeadersVisible = false;
			grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders;
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestAutoSizeRowsModeInvalidOperationException2 () {
			DataGridView grid = new DataGridView();
			grid.RowHeadersVisible = false;
			grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedHeaders;
		}

		#endregion

		[Test]
		public void ControlsTest ()
		{
			using (DataGridView grid = new DataGridView ()) {
				Assert.AreEqual ("DataGridViewControlCollection", grid.Controls.GetType ().Name, "#01");
				Assert.AreEqual (2, grid.Controls.Count, "#02");
			}
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestBackgroundColorArgumentException () {
			DataGridView grid = new DataGridView();
			grid.BackgroundColor = Color.Empty;
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestBorderStyleInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.BorderStyle = BorderStyle.FixedSingle | BorderStyle.Fixed3D;
		}

		[Test]
		public void ColumnCount () {
			DataGridView grid = new DataGridView();
			Assert.AreEqual (0, grid.ColumnCount, "#A1");

			try {
				grid.ColumnCount = -1;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("ColumnCount", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestColumnCountInvalidOperationException () {
			DataGridView grid = new DataGridView();
			grid.DataSource = new ArrayList();
			grid.ColumnCount = 0;
		}

		[Test]
		public void ColumnHeadersHeight () {
			DataGridView grid = new DataGridView();
			Assert.AreEqual (23, grid.ColumnHeadersHeight, "#A1");
			grid.ColumnHeadersHeight = 4;
			Assert.AreEqual (4, grid.ColumnHeadersHeight, "#A2");
			grid.ColumnHeadersHeight = 32768;
			Assert.AreEqual (32768, grid.ColumnHeadersHeight, "#A3");

			try {
				grid.ColumnHeadersHeight = 3;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("ColumnHeadersHeight", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			try {
				grid.ColumnHeadersHeight = 32769;
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNotNull (ex.ParamName, "#C4");
				Assert.AreEqual ("ColumnHeadersHeight", ex.ParamName, "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
			}
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestColumnHeadersHeightSizeModeInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.ColumnHeadersHeightSizeMode = (DataGridViewColumnHeadersHeightSizeMode) 3;
		}

		[Test]
		public void RowHeadersWidth () {
			DataGridView grid = new DataGridView();
			Assert.AreEqual (41, grid.RowHeadersWidth, "#A1");
			grid.RowHeadersWidth = 4;
			Assert.AreEqual (4, grid.RowHeadersWidth, "#A2");
			grid.RowHeadersWidth = 32768;
			Assert.AreEqual (32768, grid.RowHeadersWidth, "#A3");

			try {
				grid.RowHeadersWidth = 3;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("RowHeadersWidth", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			try {
				grid.RowHeadersWidth = 32769;
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNotNull (ex.ParamName, "#C4");
				Assert.AreEqual ("RowHeadersWidth", ex.ParamName, "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
			}
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestDataGridViewRowHeadersWidthSizeModeInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.RowHeadersWidthSizeMode = (DataGridViewRowHeadersWidthSizeMode) 5;
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestScrollBarsInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.ScrollBars = (ScrollBars) 4;
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestSelectionModeInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.SelectionMode = (DataGridViewSelectionMode) 5;
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestAutoResizeRowsInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.AutoResizeRows((DataGridViewAutoSizeRowsMode) 4);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestAutoResizeRowsInvalidOperationException1 () {
			DataGridView grid = new DataGridView();
			grid.RowHeadersVisible = false;
			grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllHeaders);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestAutoResizeRowsInvalidOperationException2 () {
			DataGridView grid = new DataGridView();
			grid.RowHeadersVisible = false;
			grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.DisplayedHeaders);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestAutoResizeRowsArgumentException () {
			DataGridView grid = new DataGridView();
			grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.None);
		}

		[Test]
		public void DefaultSize ()
		{
			MockDataGridView grid = new MockDataGridView ();
			Assert.AreEqual (new Size (240, 150), grid.default_size, "#1");
			Assert.AreEqual (new Size (240, 150), grid.Size, "#2");
		}

		[Test]
		public void ColumnHeadersDefaultCellStyle ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (SystemColors.Control, grid.ColumnHeadersDefaultCellStyle.BackColor, "#A1");
			Assert.AreEqual (SystemColors.WindowText,  grid.ColumnHeadersDefaultCellStyle.ForeColor, "#A2");
			Assert.AreEqual (SystemColors.Highlight, grid.ColumnHeadersDefaultCellStyle.SelectionBackColor, "#A3");
			Assert.AreEqual (SystemColors.HighlightText, grid.ColumnHeadersDefaultCellStyle.SelectionForeColor, "#A4");
			Assert.AreSame (grid.Font, grid.ColumnHeadersDefaultCellStyle.Font, "#A5");
			Assert.AreEqual (DataGridViewContentAlignment.MiddleLeft, grid.ColumnHeadersDefaultCellStyle.Alignment, "#A6");
			Assert.AreEqual (DataGridViewTriState.True, grid.ColumnHeadersDefaultCellStyle.WrapMode, "#A7");
		}

		[Test]
		public void DefaultCellStyle ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (SystemColors.Window, grid.DefaultCellStyle.BackColor, "#A1");
			Assert.AreEqual (SystemColors.ControlText,  grid.DefaultCellStyle.ForeColor, "#A2");
			Assert.AreEqual (SystemColors.Highlight, grid.DefaultCellStyle.SelectionBackColor, "#A3");
			Assert.AreEqual (SystemColors.HighlightText, grid.DefaultCellStyle.SelectionForeColor, "#A4");
			Assert.AreSame (grid.Font, grid.DefaultCellStyle.Font, "#A5");
			Assert.AreEqual (DataGridViewContentAlignment.MiddleLeft, grid.DefaultCellStyle.Alignment, "#A6");
			Assert.AreEqual (DataGridViewTriState.False, grid.DefaultCellStyle.WrapMode, "#A7");
		}

		[Test]
		public void RowsDefaultCellStyle ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (Color.Empty, grid.RowsDefaultCellStyle.BackColor, "#A1");
			Assert.AreEqual (Color.Empty, grid.RowsDefaultCellStyle.ForeColor, "#A2");
			Assert.AreEqual (Color.Empty, grid.RowsDefaultCellStyle.SelectionBackColor, "#A3");
			Assert.AreEqual (Color.Empty, grid.RowsDefaultCellStyle.SelectionForeColor, "#A4");
			Assert.IsNull(grid.RowsDefaultCellStyle.Font, "#A5");
			Assert.AreEqual (DataGridViewContentAlignment.NotSet, grid.RowsDefaultCellStyle.Alignment, "#A6");
			Assert.AreEqual (DataGridViewTriState.NotSet, grid.RowsDefaultCellStyle.WrapMode, "#A7");
		}

		[Test]
		public void RowHeadersDefaultCellStyle ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (SystemColors.Control, grid.RowHeadersDefaultCellStyle.BackColor, "#A1");
			Assert.AreEqual (SystemColors.WindowText, grid.RowHeadersDefaultCellStyle.ForeColor, "#A2");
			Assert.AreEqual (SystemColors.Highlight, grid.RowHeadersDefaultCellStyle.SelectionBackColor, "#A3");
			Assert.AreEqual (SystemColors.HighlightText, grid.RowHeadersDefaultCellStyle.SelectionForeColor, "#A4");
			Assert.AreSame (grid.Font, grid.RowHeadersDefaultCellStyle.Font, "#A5");
			Assert.AreEqual (DataGridViewContentAlignment.MiddleLeft, grid.RowHeadersDefaultCellStyle.Alignment, "#A6");
			Assert.AreEqual (DataGridViewTriState.True, grid.RowHeadersDefaultCellStyle.WrapMode, "#A7");
		}

		private class MockDataGridView : DataGridView
		{
			public Size default_size {
				get { return base.DefaultSize; }
			}
		}
	}
	
	[TestFixture]
	public class DataGridViewControlCollectionTest
	{
		
		public void TestClear ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection) dgv.Controls;
				Control c1 = new Control ();
				Control c2 = new Control ();
				Control c3 = new Control ();
				Assert.AreEqual (2, controls.Count, "#02");
				controls.Add (c1);
				controls.Add (c2);
				controls.Add (c3);
				Assert.AreEqual (5, controls.Count, "#02");
				controls.Clear ();
				Assert.AreEqual (3, controls.Count, "#03");
				Assert.AreSame (c2, controls [2], "#04");
			}

			// Maybe MS should start writing unit-tests?

			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection)dgv.Controls;
				Control [] c = new Control [20];
				for (int i = 0; i < c.Length; i++) {
					c [i] = new Control ();
					c [i].Text = "#" + i.ToString ();
				}
				
				Assert.AreEqual (2, controls.Count, "#02");
				controls.AddRange (c);
				Assert.AreEqual (22, controls.Count, "#02");
				controls.Clear ();
				Assert.AreEqual (12, controls.Count, "#03");
				
				for (int i = 0; i < c.Length; i += 2) {
					Assert.AreSame (c [i+1], controls [ (i / 2) + 2], "#A" + i.ToString ());
				}
			}
		}


		public void TestCopyTo ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection)dgv.Controls;
				Control c1 = new Control ();
				Control c2 = new Control ();
				Control c3 = new Control ();
				Control [] copy = new Control [10];
				Assert.AreEqual (2, controls.Count, "#01");
				controls.AddRange (new Control [] { c1, c2, c3 });
				Assert.AreEqual (5, controls.Count, "#01-b");
				controls.CopyTo (copy, 0);
				Assert.AreEqual (5, controls.Count, "#02");
				Assert.AreEqual (10, copy.Length, "#03");
				for (int i = 0; i < copy.Length; i++) {
					if (i >= 5)
						Assert.IsNull (copy [i], "#A" + i.ToString ());
					else
						Assert.IsNotNull (copy [i], "#B" + i.ToString ());
				}
			
			}
		}

		[ExpectedException (typeof (NotSupportedException))]
		public void TestInsert ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection)dgv.Controls;
				controls.Insert (1, new Control ());
			}
		}


		public void TestRemove ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection)dgv.Controls;
				Control c1 = new Control ();
				Control c2 = new Control ();
				Control c3 = new Control ();
				Control [] copy = new Control [10];
				
				controls.AddRange (new Control [] {c1, c2, c3});
				
				controls.Remove (c2);
				Assert.AreEqual (4, controls.Count, "#01");
				controls.Remove (c2);
				Assert.AreEqual (4, controls.Count, "#02");
				controls.Remove (c1);
				Assert.AreEqual (3, controls.Count, "#03");
				controls.Remove (c3);
				Assert.AreEqual (2, controls.Count, "#04");
				
				controls.Remove (controls [0]);
				controls.Remove (controls [1]);
				Assert.AreEqual (2, controls.Count, "#05");
			}
		}
	}
		
}

#endif
