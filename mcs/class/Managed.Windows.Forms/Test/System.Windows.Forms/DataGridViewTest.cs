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
//	Pedro Martínez Juliá <pedromj@gmail.com>
//


#if NET_2_0

using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewTest : Assertion {
		
		[SetUp]
		public void GetReady() {}

		[TearDown]
		public void Clean() {}

		[Test]
		public void TestDefaultValues () {
			DataGridView grid = new DataGridView();
			AssertEquals("AllowUserToAddRows property", true, grid.AllowUserToAddRows);
			AssertEquals("AllowUserToDeleteRows property", true, grid.AllowUserToDeleteRows);
			AssertEquals("AllowUserToOrderColumns property", false, grid.AllowUserToOrderColumns);
			AssertEquals("AllowUserToResizeColumns property", true, grid.AllowUserToResizeColumns);
			AssertEquals("AllowUserToResizeRows property", true, grid.AllowUserToResizeRows);
			AssertEquals("AlternatingRowsDefaultCellStyle property", new DataGridViewCellStyle(), grid.AlternatingRowsDefaultCellStyle);
			AssertEquals("AutoGenerateColumns property", true, grid.AutoGenerateColumns);
			AssertEquals("AutoSizeRowsMode property", DataGridViewAutoSizeRowsMode.None, grid.AutoSizeRowsMode);
			AssertEquals("BackColor property", Control.DefaultBackColor, grid.BackColor);
			AssertEquals("BackgroundColor property", SystemColors.AppWorkspace, grid.BackgroundColor);
			AssertEquals("BorderStyle property", BorderStyle.FixedSingle, grid.BorderStyle);
			AssertEquals("ClipboardCopyMode property", DataGridViewClipboardCopyMode.EnableWithAutoHeaderText, grid.ClipboardCopyMode);
			AssertEquals("ColumnHeadersDefaultCellStyle.BackColor property", SystemColors.Control, grid.ColumnHeadersDefaultCellStyle.BackColor);
			AssertEquals("ColumnHeadersDefaultCellStyle.ForeColor property", SystemColors.WindowText,  grid.ColumnHeadersDefaultCellStyle.ForeColor);
			AssertEquals("ColumnHeadersDefaultCellStyle.SelectionBackColor property", SystemColors.Highlight, grid.ColumnHeadersDefaultCellStyle.SelectionBackColor);
			AssertEquals("ColumnHeadersDefaultCellStyle.SelectionForeColor property", SystemColors.HighlightText, grid.ColumnHeadersDefaultCellStyle.SelectionForeColor);
			AssertEquals("ColumnHeadersDefaultCellStyle.Font property", grid.Font, grid.ColumnHeadersDefaultCellStyle.Font);
			AssertEquals("ColumnHeadersDefaultCellStyle.Alignment property", DataGridViewContentAlignment.MiddleLeft, grid.ColumnHeadersDefaultCellStyle.Alignment);
			AssertEquals("ColumnHeadersDefaultCellStyle.WrapMode property", DataGridViewTriState.True, grid.ColumnHeadersDefaultCellStyle.WrapMode);
			AssertEquals("ColumnHeadersHeight property", 23, grid.ColumnHeadersHeight);
			AssertEquals("ColumnHeadersHeightSizeMode property", DataGridViewColumnHeadersHeightSizeMode.EnableResizing, grid.ColumnHeadersHeightSizeMode);
			AssertEquals("ColumnHeadersVisible property", true, grid.ColumnHeadersVisible);
			AssertEquals("DataMember property", String.Empty, grid.DataMember);
			AssertEquals("DefaultCellStyle.BackColor property", SystemColors.Control, grid.DefaultCellStyle.BackColor);
			AssertEquals("DefaultCellStyle.ForeColor property", SystemColors.WindowText,  grid.DefaultCellStyle.ForeColor);
			AssertEquals("DefaultCellStyle.SelectionBackColor property", SystemColors.Highlight, grid.DefaultCellStyle.SelectionBackColor);
			AssertEquals("DefaultCellStyle.SelectionForeColor property", SystemColors.HighlightText, grid.DefaultCellStyle.SelectionForeColor);
			AssertEquals("DefaultCellStyle.Font property", grid.Font, grid.DefaultCellStyle.Font);
			AssertEquals("DefaultCellStyle.Alignment property", DataGridViewContentAlignment.MiddleLeft, grid.DefaultCellStyle.Alignment);
			AssertEquals("DefaultCellStyle.WrapMode property", DataGridViewTriState.True, grid.DefaultCellStyle.WrapMode);
			AssertEquals("EditMode property", DataGridViewEditMode.EditOnKeystrokeOrF2, grid.EditMode);
			AssertEquals("Font property", Control.DefaultFont, grid.Font);
			AssertEquals("ForeColor property", Control.DefaultForeColor, grid.ForeColor);
			AssertEquals("GridColor property", Color.FromKnownColor(KnownColor.ControlDarkDark), grid.GridColor);
			AssertEquals("MultiSelect property", true, grid.MultiSelect);
			AssertEquals("NewRowIndex property", grid.Rows.Count - 1, grid.NewRowIndex);
			AssertEquals("Padding property", Padding.Empty, grid.Padding);
			AssertEquals("ReadOnly property", false, grid.ReadOnly);
			AssertEquals("RowHeadersVisible property", true, grid.RowHeadersVisible);
			AssertEquals("RowHeadersWidth property", 43, grid.RowHeadersWidth);
			AssertEquals("SelectionMode property", DataGridViewSelectionMode.RowHeaderSelect, grid.SelectionMode);
			AssertEquals("ShowCellErrors property", true, grid.ShowCellErrors);
			AssertEquals("ShowEditingIcon property", true, grid.ShowEditingIcon);
			AssertEquals("UserSetCursor property", Cursor.Current, grid.UserSetCursor);
			AssertEquals("VirtualMode property", false, grid.VirtualMode);
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
		[ExpectedException(typeof(ArgumentException))]
		public void TestColumnCountArgumentException () {
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
		[ExpectedException(typeof(ArgumentException))]
		public void TestColumnHeadersHeightArgumentException () {
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
		[ExpectedException(typeof(ArgumentException))]
		public void TestCurrentCellArgumentException () {
			DataGridView grid = new DataGridView();
			grid.CurrentCell = new DataGridViewTextBoxCell();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestRowHeadersWidthArgumentException () {
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

		/*
		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestException () {
			ConcreteCollection myCollection;
			myCollection = new ConcreteCollection();
			....
			AssertEquals ("#UniqueID", expected, actual);
			....
			Fail ("Message");
		}
		*/

	}

}

#endif
