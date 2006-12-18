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
// Copyright (c) 2005,2006 Novell, Inc. (http://www.novell.com)
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
			Assert.AreEqual (SystemColors.Control, grid.ColumnHeadersDefaultCellStyle.BackColor, "#A13");
			Assert.AreEqual (SystemColors.WindowText,  grid.ColumnHeadersDefaultCellStyle.ForeColor, "#A14");
			Assert.AreEqual (SystemColors.Highlight, grid.ColumnHeadersDefaultCellStyle.SelectionBackColor, "#A15");
			Assert.AreEqual (SystemColors.HighlightText, grid.ColumnHeadersDefaultCellStyle.SelectionForeColor, "#A16");
			Assert.AreEqual (grid.Font, grid.ColumnHeadersDefaultCellStyle.Font, "#A17");
			Assert.AreEqual (DataGridViewContentAlignment.MiddleLeft, grid.ColumnHeadersDefaultCellStyle.Alignment, "#A18");
			Assert.AreEqual (DataGridViewTriState.True, grid.ColumnHeadersDefaultCellStyle.WrapMode, "#A19");
			Assert.AreEqual (23, grid.ColumnHeadersHeight, "#A20");
			Assert.AreEqual (DataGridViewColumnHeadersHeightSizeMode.EnableResizing, grid.ColumnHeadersHeightSizeMode, "#A21");
			Assert.AreEqual (true, grid.ColumnHeadersVisible, "#A22");
			Assert.AreEqual (String.Empty, grid.DataMember, "#A23");
			Assert.AreEqual (SystemColors.Window, grid.DefaultCellStyle.BackColor, "#A24");
			Assert.AreEqual (SystemColors.ControlText,  grid.DefaultCellStyle.ForeColor, "#A25");
			Assert.AreEqual (SystemColors.Highlight, grid.DefaultCellStyle.SelectionBackColor, "#A26");
			Assert.AreEqual (SystemColors.HighlightText, grid.DefaultCellStyle.SelectionForeColor, "#A27");
			Assert.AreEqual (grid.Font, grid.DefaultCellStyle.Font, "#A28");
			Assert.AreEqual (DataGridViewContentAlignment.MiddleLeft, grid.DefaultCellStyle.Alignment, "#A29");
			Assert.AreEqual (DataGridViewTriState.False, grid.DefaultCellStyle.WrapMode, "#A30");
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
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestColumnCountArgumentOutOfRangeException () {
			DataGridView grid = new DataGridView();
			grid.ColumnCount = -1;
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestColumnCountInvalidOperationException () {
			DataGridView grid = new DataGridView();
			grid.DataSource = new ArrayList();
			grid.ColumnCount = 0;
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestColumnHeadersHeightArgumentOutOfRangeException () {
			DataGridView grid = new DataGridView();
			grid.ColumnHeadersHeight = 3;
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestColumnHeadersHeightSizeModeInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView();
			grid.ColumnHeadersHeightSizeMode = (DataGridViewColumnHeadersHeightSizeMode) 3;
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestRowHeadersWidthArgumentOutOfRangeException () {
			DataGridView grid = new DataGridView();
			grid.RowHeadersWidth = 3;
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
	}
}

#endif
