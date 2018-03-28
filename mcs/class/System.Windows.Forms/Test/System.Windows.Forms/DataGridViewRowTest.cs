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



using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewRowTest : TestHelper {

		[Test]
		public void TestDefaultValues () {
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestVisibleInvalidOperationException () {
			DataGridView grid = new DataGridView();
			DataGridViewRow row = new DataGridViewRow();
			grid.Rows.Add(row);
			row.Visible = false;
		}

		[Test]
		public void Height ()
		{
			DataGridViewRow row = new DataGridViewRow();
			Assert.IsTrue (row.Height > 5, "#1");
			row.Height = 70;
			Assert.AreEqual (70, row.Height, "#2");
			row.Height = 40;
			Assert.AreEqual (40, row.Height, "#3");
		}

		[Test]
		public void Height_SetHeightLessThanMinHeightSilentlySetsToMinHeight()
		{
			using (var row = new DataGridViewRow ()) {
				// Setup
				row.MinimumHeight = 5;

				// Execute
				row.Height = 2;

				// Verify
				Assert.AreEqual (5, row.Height, "Height didn't get set to MinimumHeight");
			}
		}

		[Test]
		public void MinimumHeight_DefaultValues ()
		{
			using (DataGridViewRow row = new DataGridViewRow ()) {
				Assert.IsTrue (row.MinimumHeight > 0, "#A1");
				Assert.IsTrue (row.Height >= row.MinimumHeight, "#A2");
			}
		}

		[Test]
		public void MinimumHeight_SetValues ()
		{
			using (DataGridViewRow row = new DataGridViewRow ()) {
				row.MinimumHeight = 40;
				row.Height = 50;
				Assert.AreEqual (40, row.MinimumHeight, "#B1");
				Assert.AreEqual (50, row.Height, "#B2");
			}
		}

		[Test]
		public void MinimumHeight_IncreaseMinHeightChangesHeight ()
		{
			using (DataGridViewRow row = new DataGridViewRow ()) {
				row.MinimumHeight = 20;
				row.Height = 20;
				Assert.AreEqual (20, row.MinimumHeight, "#C1");
				Assert.AreEqual (20, row.Height, "#C2");
				row.MinimumHeight = 40;
				Assert.AreEqual (40, row.MinimumHeight, "#D1");
				Assert.AreEqual (40, row.Height, "#D2");
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MinimumHeight_SettingToLessThan2ThrowsException ()
		{
			using (DataGridViewRow row = new DataGridViewRow ()) {
				// We expect the next line to throw an ArgumentOutOfRangeException
				row.MinimumHeight = 1;
			}
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]	// DGVComboBox not implemented
		public void AddRow_Changes ()
		{

			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewColumn col = new DataGridViewComboBoxColumn ();
				DataGridViewRow row = new DataGridViewRow ();
				DataGridViewCell cell = new DataGridViewComboBoxCell ();
				
				Assert.IsNotNull (row.AccessibilityObject, "#A row.AccessibilityObject");
				Assert.IsNotNull (row.Cells, "#A row.Cells");
				Assert.IsNull (row.ContextMenuStrip, "#A row.ContextMenuStrip");
				Assert.IsNull (row.DataBoundItem, "#A row.DataBoundItem");
				Assert.IsNull (row.DataGridView, "#A row.DataGridView");
				Assert.IsNotNull (row.DefaultCellStyle, "#A row.DefaultCellStyle");
				Assert.IsNotNull (row.DefaultHeaderCellType, "#A row.DefaultHeaderCellType");
				Assert.AreEqual (false, row.Displayed, "#A row.Displayed");
				Assert.AreEqual (0, row.DividerHeight, "#A row.DividerHeight");
				Assert.AreEqual (@"", row.ErrorText, "#A row.ErrorText");
				Assert.AreEqual (false, row.Frozen, "#A row.Frozen");
				Assert.AreEqual (true, row.HasDefaultCellStyle, "#A row.HasDefaultCellStyle");
				Assert.IsNotNull (row.HeaderCell, "#A row.HeaderCell");
				// DPI Dependent? // Assert.AreEqual (22, row.Height, "#A row.Height");
				Assert.AreEqual (-1, row.Index, "#A row.Index");
				try {
					object zxf = row.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#A row.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the InheritedStyle property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#A row.InheritedStyle");
				}
				Assert.AreEqual (false, row.IsNewRow, "#A row.IsNewRow");
				Assert.AreEqual (3, row.MinimumHeight, "#A row.MinimumHeight");
				Assert.AreEqual (false, row.ReadOnly, "#A row.ReadOnly");
				Assert.AreEqual (DataGridViewTriState.NotSet, row.Resizable, "#A row.Resizable");
				Assert.AreEqual (false, row.Selected, "#A row.Selected");
				Assert.AreEqual (DataGridViewElementStates.Visible, row.State, "#A row.State");
				Assert.IsNull (row.Tag, "#A row.Tag");
				Assert.AreEqual (true, row.Visible, "#A row.Visible");

				row.Cells.Add (cell);

				Assert.IsNotNull (row.AccessibilityObject, "#B row.AccessibilityObject");
				Assert.IsNotNull (row.Cells, "#B row.Cells");
				Assert.IsNull (row.ContextMenuStrip, "#B row.ContextMenuStrip");
				Assert.IsNull (row.DataBoundItem, "#B row.DataBoundItem");
				Assert.IsNull (row.DataGridView, "#B row.DataGridView");
				Assert.IsNotNull (row.DefaultCellStyle, "#B row.DefaultCellStyle");
				Assert.IsNotNull (row.DefaultHeaderCellType, "#B row.DefaultHeaderCellType");
				Assert.AreEqual (false, row.Displayed, "#B row.Displayed");
				Assert.AreEqual (0, row.DividerHeight, "#B row.DividerHeight");
				Assert.AreEqual (@"", row.ErrorText, "#B row.ErrorText");
				Assert.AreEqual (false, row.Frozen, "#B row.Frozen");
				Assert.AreEqual (true, row.HasDefaultCellStyle, "#B row.HasDefaultCellStyle");
				Assert.IsNotNull (row.HeaderCell, "#B row.HeaderCell");
				// DPI Dependent? // Assert.AreEqual (22, row.Height, "#B row.Height");
				Assert.AreEqual (-1, row.Index, "#B row.Index");
				try {
					object zxf = row.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#B row.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the InheritedStyle property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#B row.InheritedStyle");
				}
				Assert.AreEqual (false, row.IsNewRow, "#B row.IsNewRow");
				Assert.AreEqual (3, row.MinimumHeight, "#B row.MinimumHeight");
				Assert.AreEqual (false, row.ReadOnly, "#B row.ReadOnly");
				Assert.AreEqual (DataGridViewTriState.NotSet, row.Resizable, "#B row.Resizable");
				Assert.AreEqual (false, row.Selected, "#B row.Selected");
				Assert.AreEqual (DataGridViewElementStates.Visible, row.State, "#B row.State");
				Assert.IsNull (row.Tag, "#B row.Tag");
				Assert.AreEqual (true, row.Visible, "#B row.Visible");
				
				dgv.Columns.Add (col);
				
				Assert.IsNotNull (row.AccessibilityObject, "#C row.AccessibilityObject");
				Assert.IsNotNull (row.Cells, "#C row.Cells");
				Assert.IsNull (row.ContextMenuStrip, "#C row.ContextMenuStrip");
				Assert.IsNull (row.DataBoundItem, "#C row.DataBoundItem");
				Assert.IsNull (row.DataGridView, "#C row.DataGridView");
				Assert.IsNotNull (row.DefaultCellStyle, "#C row.DefaultCellStyle");
				Assert.IsNotNull (row.DefaultHeaderCellType, "#C row.DefaultHeaderCellType");
				Assert.AreEqual (false, row.Displayed, "#C row.Displayed");
				Assert.AreEqual (0, row.DividerHeight, "#C row.DividerHeight");
				Assert.AreEqual (@"", row.ErrorText, "#C row.ErrorText");
				Assert.AreEqual (false, row.Frozen, "#C row.Frozen");
				Assert.AreEqual (true, row.HasDefaultCellStyle, "#C row.HasDefaultCellStyle");
				Assert.IsNotNull (row.HeaderCell, "#C row.HeaderCell");
				// DPI Dependent? // Assert.AreEqual (22, row.Height, "#C row.Height");
				Assert.AreEqual (-1, row.Index, "#C row.Index");
				try {
					object zxf = row.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#C row.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the InheritedStyle property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#C row.InheritedStyle");
				}
				Assert.AreEqual (false, row.IsNewRow, "#C row.IsNewRow");
				Assert.AreEqual (3, row.MinimumHeight, "#C row.MinimumHeight");
				Assert.AreEqual (false, row.ReadOnly, "#C row.ReadOnly");
				Assert.AreEqual (DataGridViewTriState.NotSet, row.Resizable, "#C row.Resizable");
				Assert.AreEqual (false, row.Selected, "#C row.Selected");
				Assert.AreEqual (DataGridViewElementStates.Visible, row.State, "#C row.State");
				Assert.IsNull (row.Tag, "#C row.Tag");
				Assert.AreEqual (true, row.Visible, "#C row.Visible");
				
				dgv.Rows.Add (row);

				Assert.IsNotNull (row.AccessibilityObject, "#D row.AccessibilityObject");
				Assert.IsNotNull (row.Cells, "#D row.Cells");
				try {
					object zxf = row.ContextMenuStrip;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.ContextMenuStrip");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Operation cannot be performed on a shared row.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.ContextMenuStrip");
				}
				Assert.IsNull (row.DataBoundItem, "#D row.DataBoundItem");
				Assert.IsNotNull (row.DataGridView, "#D row.DataGridView");
				Assert.IsNotNull (row.DefaultCellStyle, "#D row.DefaultCellStyle");
				Assert.IsNotNull (row.DefaultHeaderCellType, "#D row.DefaultHeaderCellType");
				try {
					object zxf = row.Displayed;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.Displayed");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the Displayed property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.Displayed");
				}
				Assert.AreEqual (0, row.DividerHeight, "#D row.DividerHeight");
				try {
					object zxf = row.ErrorText;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.ErrorText");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Operation cannot be performed on a shared row.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.ErrorText");
				}
				try {
					object zxf = row.Frozen;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.Frozen");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the Frozen property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.Frozen");
				}
				Assert.AreEqual (true, row.HasDefaultCellStyle, "#D row.HasDefaultCellStyle");
				Assert.IsNotNull (row.HeaderCell, "#D row.HeaderCell");
				// DPI Dependent? // Assert.AreEqual (22, row.Height, "#D row.Height");
				Assert.AreEqual (-1, row.Index, "#D row.Index");
				try {
					object zxf = row.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the InheritedStyle property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.InheritedStyle");
				}
				Assert.AreEqual (false, row.IsNewRow, "#D row.IsNewRow");
				Assert.AreEqual (3, row.MinimumHeight, "#D row.MinimumHeight");
				try {
					object zxf = row.ReadOnly;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.ReadOnly");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the ReadOnly property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.ReadOnly");
				}
				try {
					object zxf = row.Resizable;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.Resizable");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the Resizable property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.Resizable");
				}
				try {
					object zxf = row.Selected;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.Selected");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the Selected property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.Selected");
				}
				try {
					object zxf = row.State;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.State");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the State property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.State");
				}
				Assert.IsNull (row.Tag, "#D row.Tag");
				try {
					object zxf = row.Visible;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D row.Visible");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the Visible property of a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D row.Visible");
				}
			}
		}
		
		[Test]
		public void InitialValues ()
		{
			DataGridViewRow row = new DataGridViewRow ();

			Assert.IsNotNull (row.AccessibilityObject, "#A row.AccessibilityObject");
			Assert.IsNotNull (row.Cells, "#A row.Cells");
			Assert.IsNull (row.ContextMenuStrip, "#A row.ContextMenuStrip");
			Assert.IsNull (row.DataBoundItem, "#A row.DataBoundItem");
			Assert.IsNull (row.DataGridView, "#A row.DataGridView");
			Assert.IsNotNull (row.DefaultCellStyle, "#A row.DefaultCellStyle");
			Assert.IsNotNull (row.DefaultHeaderCellType, "#A row.DefaultHeaderCellType");
			Assert.AreEqual (false, row.Displayed, "#A row.Displayed");
			Assert.AreEqual (0, row.DividerHeight, "#A row.DividerHeight");
			Assert.AreEqual (@"", row.ErrorText, "#A row.ErrorText");
			Assert.AreEqual (false, row.Frozen, "#A row.Frozen");
			Assert.AreEqual (true, row.HasDefaultCellStyle, "#A row.HasDefaultCellStyle");
			Assert.IsNotNull (row.HeaderCell, "#A row.HeaderCell");
			// DPI Dependent? // Assert.AreEqual (22, row.Height, "#A row.Height");
			Assert.AreEqual (-1, row.Index, "#A row.Index");
			try {
				object zxf = row.InheritedStyle;
				TestHelper.RemoveWarning (zxf);
				Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#A row.InheritedStyle");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (@"Getting the InheritedStyle property of a shared row is not a valid operation.", ex.Message);
			} catch (Exception ex) {
				Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#A row.InheritedStyle");
			}
			Assert.AreEqual (false, row.IsNewRow, "#A row.IsNewRow");
			Assert.AreEqual (3, row.MinimumHeight, "#A row.MinimumHeight");
			Assert.AreEqual (false, row.ReadOnly, "#A row.ReadOnly");
			Assert.AreEqual (DataGridViewTriState.NotSet, row.Resizable, "#A row.Resizable");
			Assert.AreEqual (false, row.Selected, "#A row.Selected");
			Assert.AreEqual (DataGridViewElementStates.Visible, row.State, "#A row.State");
			Assert.IsNull (row.Tag, "#A row.Tag");
			Assert.AreEqual (true, row.Visible, "#A row.Visible");
		}
		
		[Test]
		public void Clone ()
		{
			DataGridView dgv = new DataGridView ();

			dgv.Columns.Add ("Column 1", "Column 1");
			dgv.Columns.Add ("Column 2", "Column 2");
			
			dgv.Rows.Add ("Cell 1", "Cell 2");
			
			DataGridViewRow row1 = dgv.Rows[0];
			
			row1.ErrorText = "Yikes!";
			row1.Tag = "Helo";
			row1.ReadOnly = true;
			row1.Visible = false;
			
			DataGridViewRow row2 = (DataGridViewRow)row1.Clone ();

			Assert.AreEqual (2, row2.Cells.Count, "A1");
			Assert.AreEqual (null, row2.DataGridView, "A3");
			Assert.AreEqual ("Yikes!", row2.ErrorText, "A4");
			Assert.AreEqual (-1, row2.HeaderCell.RowIndex, "A5");
			Assert.AreEqual (-1, row2.Index, "A6");
			Assert.AreEqual (true, row2.ReadOnly, "A7");
			Assert.AreEqual ("Helo", row2.Tag, "A8");
			Assert.AreEqual (false, row2.Visible, "A9");
		}


		private class TestDataGridViewRow : DataGridViewRow
		{
			protected override DataGridViewCellCollection CreateCellsInstance ()
			{
				return new MockDataGridViewCellCollection (this);
			}
		}

		private class MockDataGridViewCellCollection : DataGridViewCellCollection
		{
			public MockDataGridViewCellCollection(DataGridViewRow dataGridViewRow) : base(dataGridViewRow)
			{
			}
		}

		[Test]
		public void CreateCellsInstance ()
		{
			var row = new TestDataGridViewRow ();
			Assert.That (row.Cells, Is.TypeOf<MockDataGridViewCellCollection> (), "#A row.CreateCellsInstance");
		}
	}
}
