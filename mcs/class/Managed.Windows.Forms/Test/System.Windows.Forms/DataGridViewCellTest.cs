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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;
using System.Threading;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridViewCellTest : TestHelper
	{
		[Test]
		public void GetClipboardContentTest ()
		{
			DataGridViewClipboardCell cell = new DataGridViewClipboardCell ();
			
			cell.Value = "abc";
			Assert.IsNull (cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Text), "#01");
			
			using (DataGridView dgv = new DataGridView ()) {
				dgv.Columns.Add ("A", "A");
				DataGridViewRow row = new DataGridViewRow ();
				row.Cells.Add (cell);
				dgv.Rows.Add (row);
				cell.Selected = true;

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD>", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TR><TD>abc</TD>", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual (string.Format("abc{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual (string.Format ("abc{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD></TR>", cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual (string.Format ("abc{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD>", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD>", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>abc</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>abc</TD>", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TR><TD>abc</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.CommaSeparatedValue), "#A4");
				
				Assert.AreEqual ("abc" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>abc</TD></TR>", cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.CommaSeparatedValue), "#A4");
				
				cell.Selected = false;

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TR><TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual (string.Format ("{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual (string.Format ("{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD></TR>", cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual (string.Format ("{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>&nbsp;</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TR><TD>&nbsp;</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>&nbsp;</TD></TR>", cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.CommaSeparatedValue), "#A4");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetClipboardContentTestException ()
		{
			DataGridViewClipboardCell cell = new DataGridViewClipboardCell ();

			cell.Value = "abc";
			Assert.IsNull (cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Text), "#01");

			using (DataGridView dgv = new DataGridView ()) {
				dgv.Columns.Add ("A", "A");
				DataGridViewRow row = new DataGridViewRow ();
				row.Cells.Add (cell);
				dgv.Rows.Add (row);
				cell.Selected = true;

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (123, false, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (123, false, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD>", cell.GetClipboardContentPublic (123, false, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (123, false, false, false, false, DataFormats.CommaSeparatedValue), "#A4");
			}
		}

		[Test]
		public void ReadOnlyTest ()
		{
			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				Assert.IsFalse (dgv.ReadOnly, "1-DGV");
				Assert.IsFalse (dgv.Rows [0].ReadOnly, "1-R0");
				Assert.IsFalse (dgv.Rows [1].ReadOnly, "1-R1");
				Assert.IsFalse (dgv.Columns [0].ReadOnly, "1-C1");
				Assert.IsFalse (dgv.Columns [1].ReadOnly, "1-C1");
				Assert.IsFalse (dgv [0, 0].ReadOnly, "1-C00");
				Assert.IsFalse (dgv [1, 0].ReadOnly, "1-C10");
				Assert.IsFalse (dgv [0, 1].ReadOnly, "1-C01");
				Assert.IsFalse (dgv [1, 1].ReadOnly, "1-C11");
			}


			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				dgv.ReadOnly = true;
				Assert.IsTrue (dgv.ReadOnly, "2-DGV");
				Assert.IsTrue (dgv.Rows [0].ReadOnly, "2-R0");
				Assert.IsTrue (dgv.Rows [1].ReadOnly, "2-R1");
				Assert.IsTrue (dgv.Columns [0].ReadOnly, "2-C1");
				Assert.IsTrue (dgv.Columns [1].ReadOnly, "2-C1");
				Assert.IsTrue (dgv [0, 0].ReadOnly, "2-C00");
				Assert.IsTrue (dgv [1, 0].ReadOnly, "2-C10");
				Assert.IsTrue (dgv [0, 1].ReadOnly, "2-C01");
				Assert.IsTrue (dgv [1, 1].ReadOnly, "2-C11");
			}

			// If the DGV is ReadOnly, everything is ReadOnly.
			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				dgv.ReadOnly = true;
				Assert.IsTrue (dgv.ReadOnly, "3-DGV");
				dgv.Rows [0].ReadOnly = false;
				Assert.IsTrue (dgv.Rows [0].ReadOnly, "3-R0");
				Assert.IsTrue (dgv.Rows [1].ReadOnly, "3-R1");
				dgv.Columns [0].ReadOnly = false;
				Assert.IsTrue (dgv.Columns [0].ReadOnly, "3-C1");
				Assert.IsTrue (dgv.Columns [1].ReadOnly, "3-C1");
				dgv [0, 0].ReadOnly = false;
				Assert.IsTrue (dgv [0, 0].ReadOnly, "3-C00");
				Assert.IsTrue (dgv [1, 0].ReadOnly, "3-C10");
				Assert.IsTrue (dgv [0, 1].ReadOnly, "3-C01");
				Assert.IsTrue (dgv [1, 1].ReadOnly, "3-C11");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				Assert.IsFalse (dgv.ReadOnly, "4-DGV");
				dgv.Rows [0].ReadOnly = true;
				Assert.IsTrue (dgv.Rows [0].ReadOnly, "4-R0");
				Assert.IsFalse (dgv.Rows [1].ReadOnly, "4-R1");
				dgv.Columns [0].ReadOnly = true;
				Assert.IsTrue (dgv.Columns [0].ReadOnly, "4-C1");
				Assert.IsFalse (dgv.Columns [1].ReadOnly, "4-C1");
				Assert.IsTrue (dgv [0, 0].ReadOnly, "4-C00");
				Assert.IsTrue (dgv [1, 0].ReadOnly, "4-C10");
				Assert.IsTrue (dgv [0, 1].ReadOnly, "4-C01");
				Assert.IsFalse (dgv [1, 1].ReadOnly, "4-C11");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				Assert.IsFalse (dgv.ReadOnly, "5-DGV");
				dgv.Rows [0].ReadOnly = true;
				Assert.IsTrue (dgv.Rows [0].ReadOnly, "5-R0");
				Assert.IsFalse (dgv.Rows [1].ReadOnly, "5-R1");
				dgv.Columns [0].ReadOnly = true;
				Assert.IsTrue (dgv.Columns [0].ReadOnly, "5-C1");
				Assert.IsFalse (dgv.Columns [1].ReadOnly, "5-C1");
				dgv [0, 0].ReadOnly = false; // Cell override
				Assert.IsFalse (dgv [0, 0].ReadOnly, "5-C00");
				Assert.IsTrue (dgv [1, 0].ReadOnly, "5-C10");
				Assert.IsTrue (dgv [0, 1].ReadOnly, "5-C01");
				Assert.IsFalse (dgv [1, 1].ReadOnly, "5-C11");
			}
		}

		[Test]
		public void EditTypeTest ()
		{
			DataGridViewCell cell =new DataGridViewCellMockObject ();
			Assert.AreEqual ("DataGridViewTextBoxEditingControl", cell.EditType.Name, "#01");
		}

		[Test]
		public void TestDefaultValues ()
		{
			DataGridViewCell cell = new DataGridViewCellMockObject ();

			Assert.IsNotNull (cell.AccessibilityObject, "#cell.AccessibilityObject");
			Assert.AreEqual (-1, cell.ColumnIndex, "#cell.ColumnIndex");
			Assert.IsNotNull (cell.ContentBounds, "#cell.ContentBounds");
			Assert.IsNull (cell.ContextMenuStrip, "#cell.ContextMenuStrip");
			Assert.IsNull (cell.DataGridView, "#cell.DataGridView");
			Assert.IsNull (cell.DefaultNewRowValue, "#cell.DefaultNewRowValue");
			Assert.AreEqual (false, cell.Displayed, "#cell.Displayed");
			Assert.IsNull (cell.EditedFormattedValue, "#cell.EditedFormattedValue");
			Assert.IsNotNull (cell.EditType, "#cell.EditType");
			try {
				object zxf = cell.ErrorIconBounds;
				TestHelper.RemoveWarning (zxf);
				Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#cell.ErrorIconBounds");
			} catch (InvalidOperationException ex) {
				// We don't need to check the specific message
				//Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
			} catch (Exception ex) {
				Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#cell.ErrorIconBounds");
			}
			Assert.AreEqual (@"", cell.ErrorText, "#cell.ErrorText");
			Assert.IsNull (cell.FormattedValue, "#cell.FormattedValue");
			Assert.IsNull (cell.FormattedValueType, "#cell.FormattedValueType");
			Assert.AreEqual (false, cell.Frozen, "#cell.Frozen");
			Assert.AreEqual (false, cell.HasStyle, "#cell.HasStyle");
			Assert.AreEqual (DataGridViewElementStates.ResizableSet, cell.InheritedState, "#cell.InheritedState");
			try {
				object zxf = cell.InheritedStyle;
				TestHelper.RemoveWarning (zxf);
				Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#cell.InheritedStyle");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
			} catch (Exception ex) {
				Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#cell.InheritedStyle");
			}
			Assert.AreEqual (false, cell.IsInEditMode, "#cell.IsInEditMode");
			Assert.IsNull (cell.OwningColumn, "#cell.OwningColumn");
			Assert.IsNull (cell.OwningRow, "#cell.OwningRow");
			Assert.IsNotNull (cell.PreferredSize, "#cell.PreferredSize");
			Assert.AreEqual (false, cell.ReadOnly, "#cell.ReadOnly");
			Assert.AreEqual (false, cell.Resizable, "#cell.Resizable");
			Assert.AreEqual (-1, cell.RowIndex, "#cell.RowIndex");
			Assert.AreEqual (false, cell.Selected, "#cell.Selected");
			Assert.IsNotNull (cell.Size, "#cell.Size");
			Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#cell.State");
			Assert.IsNotNull (cell.Style, "#cell.Style");
			Assert.IsNull (cell.Tag, "#cell.Tag");
			Assert.AreEqual (@"", cell.ToolTipText, "#cell.ToolTipText");
			Assert.IsNull (cell.Value, "#cell.Value");
			Assert.IsNull (cell.ValueType, "#cell.ValueType");
			Assert.AreEqual (false, cell.Visible, "#cell.Visible");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]	// DGVComboBox not implemented
		public void AddRow_Changes ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewColumn col = new DataGridViewComboBoxColumn ();
				DataGridViewRow row = new DataGridViewRow ();
				DataGridViewCell cell = new DataGridViewComboBoxCell ();

				Assert.IsNotNull (cell.AccessibilityObject, "#A cell.AccessibilityObject");
				Assert.AreEqual (-1, cell.ColumnIndex, "#A cell.ColumnIndex");
				Assert.IsNotNull (cell.ContentBounds, "#A cell.ContentBounds");
				Assert.IsNull (cell.ContextMenuStrip, "#A cell.ContextMenuStrip");
				Assert.IsNull (cell.DataGridView, "#A cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#A cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#A cell.Displayed");
				Assert.IsNull (cell.EditedFormattedValue, "#A cell.EditedFormattedValue");
				Assert.IsNotNull (cell.EditType, "#A cell.EditType");
				try {
					object zxf = cell.ErrorIconBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#A cell.ErrorIconBounds");
				} catch (InvalidOperationException ex) {
					//Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#A cell.ErrorIconBounds");
				}
				Assert.AreEqual (@"", cell.ErrorText, "#A cell.ErrorText");
				Assert.IsNull (cell.FormattedValue, "#A cell.FormattedValue");
				Assert.IsNotNull (cell.FormattedValueType, "#A cell.FormattedValueType");
				Assert.AreEqual (false, cell.Frozen, "#A cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#A cell.HasStyle");
				Assert.AreEqual (DataGridViewElementStates.ResizableSet, cell.InheritedState, "#A cell.InheritedState");
				try {
					object zxf = cell.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#A cell.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#A cell.InheritedStyle");
				}
				Assert.AreEqual (false, cell.IsInEditMode, "#A cell.IsInEditMode");
				Assert.IsNull (cell.OwningColumn, "#A cell.OwningColumn");
				Assert.IsNull (cell.OwningRow, "#A cell.OwningRow");
				Assert.IsNotNull (cell.PreferredSize, "#A cell.PreferredSize");
				Assert.AreEqual (false, cell.ReadOnly, "#A cell.ReadOnly");
				Assert.AreEqual (false, cell.Resizable, "#A cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#A cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#A cell.Selected");
				Assert.IsNotNull (cell.Size, "#A cell.Size");
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#A cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#A cell.Style");
				Assert.IsNull (cell.Tag, "#A cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#A cell.ToolTipText");
				Assert.IsNull (cell.Value, "#A cell.Value");
				Assert.IsNotNull (cell.ValueType, "#A cell.ValueType");
				Assert.AreEqual (false, cell.Visible, "#A cell.Visible");
				
				row.Cells.Add (cell);

				Assert.IsNotNull (cell.AccessibilityObject, "#B cell.AccessibilityObject");
				Assert.AreEqual (-1, cell.ColumnIndex, "#B cell.ColumnIndex");
				Assert.IsNotNull (cell.ContentBounds, "#B cell.ContentBounds");
				Assert.IsNull (cell.ContextMenuStrip, "#B cell.ContextMenuStrip");
				Assert.IsNull (cell.DataGridView, "#B cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#B cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#B cell.Displayed");
				Assert.IsNull (cell.EditedFormattedValue, "#B cell.EditedFormattedValue");
				Assert.IsNotNull (cell.EditType, "#B cell.EditType");
				try {
					object zxf = cell.ErrorIconBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#B cell.ErrorIconBounds");
				} catch (InvalidOperationException ex) {
					//Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#B cell.ErrorIconBounds");
				}
				Assert.AreEqual (@"", cell.ErrorText, "#B cell.ErrorText");
				Assert.IsNull (cell.FormattedValue, "#B cell.FormattedValue");
				Assert.IsNotNull (cell.FormattedValueType, "#B cell.FormattedValueType");
				Assert.AreEqual (false, cell.Frozen, "#B cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#B cell.HasStyle");
				Assert.AreEqual (DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Visible, cell.InheritedState, "#B cell.InheritedState");
				try {
					object zxf = cell.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#B cell.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#B cell.InheritedStyle");
				}
				Assert.AreEqual (false, cell.IsInEditMode, "#B cell.IsInEditMode");
				Assert.IsNull (cell.OwningColumn, "#B cell.OwningColumn");
				Assert.IsNotNull (cell.OwningRow, "#B cell.OwningRow");
				Assert.IsNotNull (cell.PreferredSize, "#B cell.PreferredSize");
				Assert.AreEqual (false, cell.ReadOnly, "#B cell.ReadOnly");
				Assert.AreEqual (false, cell.Resizable, "#B cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#B cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#B cell.Selected");
				Assert.IsNotNull (cell.Size, "#B cell.Size");
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#B cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#B cell.Style");
				Assert.IsNull (cell.Tag, "#B cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#B cell.ToolTipText");
				Assert.IsNull (cell.Value, "#B cell.Value");
				Assert.IsNotNull (cell.ValueType, "#B cell.ValueType");
				Assert.AreEqual (true, cell.Visible, "#B cell.Visible");
				
				dgv.Columns.Add (col);

				Assert.IsNotNull (cell.AccessibilityObject, "#C cell.AccessibilityObject");
				Assert.AreEqual (-1, cell.ColumnIndex, "#C cell.ColumnIndex");
				Assert.IsNotNull (cell.ContentBounds, "#C cell.ContentBounds");
				Assert.IsNull (cell.ContextMenuStrip, "#C cell.ContextMenuStrip");
				Assert.IsNull (cell.DataGridView, "#C cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#C cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#C cell.Displayed");
				Assert.IsNull (cell.EditedFormattedValue, "#C cell.EditedFormattedValue");
				Assert.IsNotNull (cell.EditType, "#C cell.EditType");
				try {
					object zxf = cell.ErrorIconBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#C cell.ErrorIconBounds");
				} catch (InvalidOperationException ex) {
					//Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#C cell.ErrorIconBounds");
				}
				Assert.AreEqual (@"", cell.ErrorText, "#C cell.ErrorText");
				Assert.IsNull (cell.FormattedValue, "#C cell.FormattedValue");
				Assert.IsNotNull (cell.FormattedValueType, "#C cell.FormattedValueType");
				Assert.AreEqual (false, cell.Frozen, "#C cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#C cell.HasStyle");
				Assert.AreEqual (DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Visible, cell.InheritedState, "#C cell.InheritedState");
				try {
					object zxf = cell.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#C cell.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#C cell.InheritedStyle");
				}
				Assert.AreEqual (false, cell.IsInEditMode, "#C cell.IsInEditMode");
				Assert.IsNull (cell.OwningColumn, "#C cell.OwningColumn");
				Assert.IsNotNull (cell.OwningRow, "#C cell.OwningRow");
				Assert.IsNotNull (cell.PreferredSize, "#C cell.PreferredSize");
				Assert.AreEqual (false, cell.ReadOnly, "#C cell.ReadOnly");
				Assert.AreEqual (false, cell.Resizable, "#C cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#C cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#C cell.Selected");
				Assert.IsNotNull (cell.Size, "#C cell.Size");
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#C cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#C cell.Style");
				Assert.IsNull (cell.Tag, "#C cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#C cell.ToolTipText");
				Assert.IsNull (cell.Value, "#C cell.Value");
				Assert.IsNotNull (cell.ValueType, "#C cell.ValueType");
				Assert.AreEqual (true, cell.Visible, "#C cell.Visible");
				
				dgv.Rows.Add (row);

				Assert.IsNotNull (cell.AccessibilityObject, "#D cell.AccessibilityObject");
				Assert.AreEqual (0, cell.ColumnIndex, "#D cell.ColumnIndex");
				try {
					object zxf = cell.ContentBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#D cell.ContentBounds");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#D cell.ContentBounds");
				}
				Assert.IsNull (cell.ContextMenuStrip, "#D cell.ContextMenuStrip");
				Assert.IsNotNull (cell.DataGridView, "#D cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#D cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#D cell.Displayed");
				try {
					object zxf = cell.EditedFormattedValue;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#D cell.EditedFormattedValue");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#D cell.EditedFormattedValue");
				}
				Assert.IsNotNull (cell.EditType, "#D cell.EditType");
				try {
					object zxf = cell.ErrorIconBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#D cell.ErrorIconBounds");
				} catch (ArgumentOutOfRangeException ex) {
					//Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#D cell.ErrorIconBounds");
				}
				Assert.AreEqual (@"", cell.ErrorText, "#D cell.ErrorText");
				try {
					object zxf = cell.FormattedValue;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#D cell.FormattedValue");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#D cell.FormattedValue");
				}
				Assert.IsNotNull (cell.FormattedValueType, "#D cell.FormattedValueType");
				Assert.AreEqual (false, cell.Frozen, "#D cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#D cell.HasStyle");
				try {
					object zxf = cell.InheritedState;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#D cell.InheritedState");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#D cell.InheritedState");
				}
				try {
					object zxf = cell.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#D cell.InheritedStyle");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#D cell.InheritedStyle");
				}
				try {
					object zxf = cell.IsInEditMode;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D cell.IsInEditMode");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Operation cannot be performed on a cell of a shared row.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D cell.IsInEditMode");
				}
				Assert.IsNotNull (cell.OwningColumn, "#D cell.OwningColumn");
				Assert.IsNotNull (cell.OwningRow, "#D cell.OwningRow");
				try {
					object zxf = cell.PreferredSize;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#D cell.PreferredSize");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#D cell.PreferredSize");
				}
				Assert.AreEqual (false, cell.ReadOnly, "#D cell.ReadOnly");
				Assert.AreEqual (false, cell.Resizable, "#D cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#D cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#D cell.Selected");
				try {
					object zxf = cell.Size;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#D cell.Size");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the Size property of a cell in a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#D cell.Size");
				}
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#D cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#D cell.Style");
				Assert.IsNull (cell.Tag, "#D cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#D cell.ToolTipText");
				try {
					object zxf = cell.Value;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#D cell.Value");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#D cell.Value");
				}
				Assert.IsNotNull (cell.ValueType, "#D cell.ValueType");
				Assert.AreEqual (false, cell.Visible, "#D cell.Visible");
			}
		}

		/*
		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestException () {
			ConcreteCollection myCollection;
			myCollection = new ConcreteCollection();
			....
			Assert.AreEqual (expected, actual, "#UniqueID");
			....
			Assert.Fail ("Message");
		}
		*/

		[Test]
		public void ColumnIndex ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (-1, c.ColumnIndex, "A1");
			
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (0, dgv.Rows[0].Cells[0].ColumnIndex, "A2");
		}

		[Test]
		public void ContentBounds ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (Rectangle.Empty, c.ContentBounds, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (Rectangle.Empty, dgv.Rows[0].Cells[0].ContentBounds, "A2");
		}

		[Test]
		public void ContextMenuStrip ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.ContextMenuStrip, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			ContextMenuStrip cms1 = new ContextMenuStrip ();
			cms1.Items.Add ("hey");

			ContextMenuStrip cms2 = new ContextMenuStrip ();
			cms2.Items.Add ("yo");
			
			dgv.Rows[0].ContextMenuStrip = cms1;
			Assert.AreEqual (null, dgv.Rows[0].Cells[0].ContextMenuStrip, "A2");
			
			dgv.Rows[0].Cells[0].ContextMenuStrip = cms2;
			Assert.AreSame (cms2, dgv.Rows[0].Cells[0].ContextMenuStrip, "A3");
		}

		[Test]
		public void DataGridView ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.DataGridView, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreSame (dgv, dgv.Rows[0].Cells[0].DataGridView, "A2");
		}

		[Test]
		public void DefaultNewRowValue ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.DefaultNewRowValue, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (null, dgv.Rows[0].Cells[0].DefaultNewRowValue, "A2");
		}

		[Test]
		public void Displayed ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (false, c.Displayed, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Displayed, "A2");
		}

		[Test]
		public void EditedFormattedValue ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.EditedFormattedValue, "A1");
		}

		[Test]
		public void FormattedValueType ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.FormattedValueType, "A1");
		}

		[Test]
		public void Frozen ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (false, c.Frozen, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Frozen, "A2");

			dgv.Columns[0].Frozen = true;
			dgv.Rows[0].Frozen = true;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Frozen, "A3");
		}

		[Test]
		public void HasStyle ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (false, c.HasStyle, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (false, dgv.Rows[0].Cells[0].HasStyle, "A2");
			
			dgv.Rows[0].Cells[0].Style.BackColor = Color.Orange;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].HasStyle, "A3");
		}

		[Test]
		public void InheritedState ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (DataGridViewElementStates.ResizableSet, c.InheritedState, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Resizable | DataGridViewElementStates.Visible, dgv.Rows[0].Cells[0].InheritedState, "A2");

			dgv.Rows[0].Selected = true;
			Assert.AreEqual (DataGridViewElementStates.Selected | DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Resizable | DataGridViewElementStates.Visible, dgv.Rows[0].Cells[0].InheritedState, "A3");

			dgv.Rows[0].Selected = false;
			dgv.Columns[0].Selected = true;
			Assert.AreEqual (DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Resizable | DataGridViewElementStates.Visible, dgv.Rows[0].Cells[0].InheritedState, "A4");
		}

		[Test]
		public void InheritedStyle ()
		{
			DataGridViewCell c = new BaseCell ();
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (SystemColors.Window, dgv.Rows[0].Cells[0].InheritedStyle.BackColor, "A1");

			dgv.DefaultCellStyle.BackColor = Color.Firebrick;
			Assert.AreEqual (Color.Firebrick, dgv.Rows[0].Cells[0].InheritedStyle.BackColor, "A2");

			dgv.Columns[0].DefaultCellStyle.BackColor = Color.FloralWhite;
			Assert.AreEqual (Color.FloralWhite, dgv.Rows[0].Cells[0].InheritedStyle.BackColor, "A3");

			dgv.RowsDefaultCellStyle.BackColor = Color.DeepPink;
			Assert.AreEqual (Color.DeepPink, dgv.Rows[0].Cells[0].InheritedStyle.BackColor, "A4");

			dgv.Rows[0].DefaultCellStyle.BackColor = Color.DeepSkyBlue;
			Assert.AreEqual (Color.DeepSkyBlue, dgv.Rows[0].Cells[0].InheritedStyle.BackColor, "A5");

			dgv.Rows[0].Cells[0].Style.BackColor = Color.DodgerBlue;
			Assert.AreEqual (Color.DodgerBlue, dgv.Rows[0].Cells[0].InheritedStyle.BackColor, "A6");
		}

		[Test]
		public void IsInEditMode ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (false, c.IsInEditMode, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (false, dgv.Rows[0].Cells[0].IsInEditMode, "A2");
		}

		[Test]
		public void OwningColumn ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.OwningColumn, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreSame (dgv.Columns[0], dgv.Rows[0].Cells[0].OwningColumn, "A2");
		}

		[Test]
		public void OwningRow ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.OwningRow, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreSame (dgv.Rows[0], dgv.Rows[0].Cells[0].OwningRow, "A2");
		}

		[Test]
		public void PreferredSize ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (new Size (-1, -1), c.PreferredSize, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (new Size (-1, -1), dgv.Rows[0].Cells[0].PreferredSize, "A2");
			
			// Always returns (-1, -1)
			dgv.Rows[0].Cells[0].Value = "bob";
			Assert.AreEqual (new Size (-1, -1), dgv.Rows[0].Cells[0].PreferredSize, "A3");
		}

		[Test]
		public void ReadOnly ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (false, c.ReadOnly, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (false, dgv.Rows[0].Cells[0].ReadOnly, "A2");

			dgv.Rows[0].ReadOnly = true;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].ReadOnly, "A3");

			dgv.Rows[0].Cells[0].ReadOnly = false;
			Assert.AreEqual (false, dgv.Rows[0].Cells[0].ReadOnly, "A4");
		}

		[Test]
		public void Resizable ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (false, c.Resizable, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Resizable, "A2");

			dgv.Rows[0].Resizable = DataGridViewTriState.False;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Resizable, "A3");

			dgv.Columns[0].Resizable = DataGridViewTriState.False;
			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Resizable, "A4");

			dgv.Columns[0].Resizable = DataGridViewTriState.True;
			dgv.Rows[0].Resizable = DataGridViewTriState.True;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Resizable, "A5");

			dgv.AllowUserToResizeColumns = false;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Resizable, "A6");

			dgv.AllowUserToResizeRows = false;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Resizable, "A7");

			dgv.Columns[0].Resizable = DataGridViewTriState.NotSet;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Resizable, "A8");

			dgv.Rows[0].Resizable = DataGridViewTriState.NotSet;
			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Resizable, "A9");
		}

		[Test]
		public void RowIndex ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (-1, c.RowIndex, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (0, dgv.Rows[0].Cells[0].RowIndex, "A2");
		}

		[Test]
		public void Selected ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (false, c.Selected, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Selected, "A2");

			dgv.Columns[0].Selected = true;
			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Selected, "A3");

			dgv.Rows[0].Selected = true;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Selected, "A4");

			dgv.Rows[0].Cells[0].Selected = false;
			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Selected, "A5");

			dgv.Rows[0].Selected = false;
			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Selected, "A6");

			dgv.Rows[0].Cells[0].Selected = true;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Selected, "A7");
		}

		/* The height of a cell (row) is based on Font
		[Test]
		public void Size ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (new Size (-1, -1), c.Size, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (new Size (100, 22), dgv.Rows[0].Cells[0].Size, "A2");
			
			// Does not resize to content
			dgv.Rows[0].Cells[0].Value = "bob";
			Assert.AreEqual (new Size (100, 22), dgv.Rows[0].Cells[0].Size, "A3");
		}
		*/

		[Test]
		public void Style ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (DataGridViewContentAlignment.NotSet, c.Style.Alignment, "A1");
			Assert.AreEqual (Color.Empty, c.Style.BackColor, "A2");
			Assert.AreEqual (DBNull.Value, c.Style.DataSourceNullValue, "A3");
			Assert.AreEqual (null, c.Style.Font, "A4");
			Assert.AreEqual (Color.Empty, c.Style.ForeColor, "A5");
			Assert.AreEqual (string.Empty, c.Style.Format, "A6");
			Assert.AreEqual (Thread.CurrentThread.CurrentCulture, c.Style.FormatProvider, "A7");
			Assert.AreEqual (true, c.Style.IsDataSourceNullValueDefault, "A8");
			Assert.AreEqual (true, c.Style.IsFormatProviderDefault, "A9");
			Assert.AreEqual (true, c.Style.IsNullValueDefault, "A10");
			Assert.AreEqual (string.Empty, c.Style.NullValue, "A11");
			Assert.AreEqual (Padding.Empty, c.Style.Padding, "A12");
			Assert.AreEqual (Color.Empty, c.Style.SelectionBackColor, "A13");
			Assert.AreEqual (Color.Empty, c.Style.SelectionForeColor, "A14");
			Assert.AreEqual (null, c.Style.Tag, "A15");
			Assert.AreEqual (DataGridViewTriState.NotSet, c.Style.WrapMode, "A16");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			// Style does not change based on parent
			// (InheritedStyle does)
			Assert.AreEqual (DataGridViewContentAlignment.NotSet, c.Style.Alignment, "A17");
			Assert.AreEqual (Color.Empty, c.Style.BackColor, "A18");
			Assert.AreEqual (DBNull.Value, c.Style.DataSourceNullValue, "A19");
			Assert.AreEqual (null, c.Style.Font, "A20");
			Assert.AreEqual (Color.Empty, c.Style.ForeColor, "A21");
			Assert.AreEqual (string.Empty, c.Style.Format, "A22");
			Assert.AreEqual (Thread.CurrentThread.CurrentCulture, c.Style.FormatProvider, "A23");
			Assert.AreEqual (true, c.Style.IsDataSourceNullValueDefault, "A24");
			Assert.AreEqual (true, c.Style.IsFormatProviderDefault, "A25");
			Assert.AreEqual (true, c.Style.IsNullValueDefault, "A26");
			Assert.AreEqual (string.Empty, c.Style.NullValue, "A27");
			Assert.AreEqual (Padding.Empty, c.Style.Padding, "A28");
			Assert.AreEqual (Color.Empty, c.Style.SelectionBackColor, "A29");
			Assert.AreEqual (Color.Empty, c.Style.SelectionForeColor, "A30");
			Assert.AreEqual (null, c.Style.Tag, "A31");
			Assert.AreEqual (DataGridViewTriState.NotSet, c.Style.WrapMode, "A32");
		}

		[Test]
		public void Tag ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.Tag, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (null, dgv.Rows[0].Cells[0].Tag, "A2");

			dgv.Rows[0].Cells[0].Tag = "bob";
			Assert.AreEqual ("bob", dgv.Rows[0].Cells[0].Tag, "A3");
		}

		[Test]
		public void ToolTipText ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (string.Empty, c.ToolTipText, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (string.Empty, dgv.Rows[0].Cells[0].ToolTipText, "A2");

			dgv.Rows[0].Cells[0].ToolTipText = "bob";
			Assert.AreEqual ("bob", dgv.Rows[0].Cells[0].ToolTipText, "A3");
		}

		[Test]
		public void Value ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.Value, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (null, dgv.Rows[0].Cells[0].Value, "A2");

			dgv.Rows[0].Cells[0].Value = "bob";
			Assert.AreEqual ("bob", dgv.Rows[0].Cells[0].Value, "A3");
		}

		[Test]
		public void ValueType ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.ValueType, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (null, dgv.Rows[0].Cells[0].ValueType, "A2");

			dgv.Rows[0].Cells[0].ValueType = typeof (bool);
			Assert.AreEqual (typeof (bool), dgv.Rows[0].Cells[0].ValueType, "A3");
		}

		[Test]
		public void Visible ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (false, c.Visible, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Visible, "A2");

			dgv.Columns[0].Visible = false;
			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Visible, "A3");

			dgv.Columns[0].Visible = true;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Visible, "A4");

			dgv.Rows[0].Visible = false;
			Assert.AreEqual (false, dgv.Rows[0].Cells[0].Visible, "A5");

			dgv.Rows[0].Visible = true;
			Assert.AreEqual (true, dgv.Rows[0].Cells[0].Visible, "A6");
		}

		/* These can be used on windows to ensure the implementation
		 * is correct, but are font-dependent
		[Test]
		public void MeasureTextHeight ()
		{
			Bitmap b = new Bitmap (1, 1);
			Graphics g = Graphics.FromImage (b);

			bool trunc = false;
			int s = DataGridViewCell.MeasureTextHeight (g, "Text", Control.DefaultFont, 100, TextFormatFlags.Default, out trunc);

			//Assert.AreEqual (false, trunc, "A1");
			Assert.AreEqual (13, s, "A2");

			trunc = false;
			s = DataGridViewCell.MeasureTextHeight (g, "Text Longer than the width", Control.DefaultFont, 100, TextFormatFlags.Default, out trunc);

			//Assert.AreEqual (true, trunc, "A3");
			Assert.AreEqual (13, s, "A4");

			trunc = false;
			s = DataGridViewCell.MeasureTextHeight (g, "Text Longer than the width", Control.DefaultFont, 100, TextFormatFlags.HorizontalCenter, out trunc);

			//Assert.AreEqual (true, trunc, "A5");
			Assert.AreEqual (13, s, "A6");

			g.Dispose ();
			b.Dispose ();
		}

		[Test]
		public void MeasureTextWidth ()
		{
			Bitmap b = new Bitmap (1, 1);
			Graphics g = Graphics.FromImage (b);

			int s = DataGridViewCell.MeasureTextWidth (g, "Text", Control.DefaultFont, 100, TextFormatFlags.Default);

			Assert.AreEqual (28, s, "A1");

			s = DataGridViewCell.MeasureTextWidth (g, "Text Longer than the width", Control.DefaultFont, 100, TextFormatFlags.Default);

			Assert.AreEqual (134, s, "A2");

			s = DataGridViewCell.MeasureTextWidth (g, "Text Longer than the width", Control.DefaultFont, 100, TextFormatFlags.HorizontalCenter);

			Assert.AreEqual (134, s, "A3");

			g.Dispose ();
			b.Dispose ();
		}

		[Test]
		public void MeasureTextSize ()
		{
			Bitmap b = new Bitmap (1, 1);
			Graphics g = Graphics.FromImage (b);

			Size s = DataGridViewCell.MeasureTextSize (g, "Text", Control.DefaultFont, TextFormatFlags.Default);

			Assert.AreEqual (new Size (28, 13), s, "A1");

			s = DataGridViewCell.MeasureTextSize (g, "Text Longer than the width", Control.DefaultFont, TextFormatFlags.Default);

			Assert.AreEqual (new Size (134, 13), s, "A2");

			s = DataGridViewCell.MeasureTextSize (g, "Text Longer than the width", Control.DefaultFont, TextFormatFlags.HorizontalCenter);

			Assert.AreEqual (new Size (134, 13), s, "A3");
			
			s = DataGridViewCell.MeasureTextSize (g, "Text Longer \nthan the width", Control.DefaultFont, TextFormatFlags.HorizontalCenter);

			Assert.AreEqual (new Size (74, 26), s, "A4");

			g.Dispose ();
			b.Dispose ();
		}

		[Test]
		public void MeasureTextPreferredSize ()
		{
			Bitmap b = new Bitmap (1, 1);
			Graphics g = Graphics.FromImage (b);

			Size s = DataGridViewCell.MeasureTextPreferredSize (g, "Text", Control.DefaultFont, 1.5f, TextFormatFlags.Default);

			Assert.AreEqual (new Size (28, 13), s, "A1");

			s = DataGridViewCell.MeasureTextPreferredSize (g, "Text Longer than the width", Control.DefaultFont, 1.5f, TextFormatFlags.Default);

			Assert.AreEqual (new Size (134, 13), s, "A2");

			s = DataGridViewCell.MeasureTextPreferredSize (g, "Text Longer than the width", Control.DefaultFont, 2.0f, TextFormatFlags.Default);

			Assert.AreEqual (new Size (134, 13), s, "A3");

			s = DataGridViewCell.MeasureTextPreferredSize (g, "Text Longer \nthan the width", Control.DefaultFont, 0.5f, TextFormatFlags.Default);

			Assert.AreEqual (new Size (74, 26), s, "A4");

			g.Dispose ();
			b.Dispose ();
		}
	*/

		[Test]
		public void MethodBorderWidths ()
		{
			BaseCell c = new BaseCell ();
			
			DataGridViewAdvancedBorderStyle style = new DataGridViewAdvancedBorderStyle ();
			style.Bottom = DataGridViewAdvancedCellBorderStyle.Inset;
			style.Left = DataGridViewAdvancedCellBorderStyle.InsetDouble;
			style.Top = DataGridViewAdvancedCellBorderStyle.None;
			//style.Right = DataGridViewAdvancedCellBorderStyle.NotSet;
			
			Assert.AreEqual (new Rectangle (2, 0, 0, 1), c.PublicBorderWidths (style), "A1");

			style.Bottom = DataGridViewAdvancedCellBorderStyle.Outset;
			style.Left = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
			style.Right = DataGridViewAdvancedCellBorderStyle.OutsetPartial;
			style.Top = DataGridViewAdvancedCellBorderStyle.Single;

			Assert.AreEqual (new Rectangle (2, 1, 1, 1), c.PublicBorderWidths (style), "A2");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			dgv.Rows[0].DividerHeight = 3;
			dgv.Columns[0].DividerWidth = 5;

			Assert.AreEqual (new Rectangle (2, 1, 6, 4), (dgv.Rows[0].Cells[0] as BaseCell).PublicBorderWidths (style), "A3");
		}

		[Test]
		public void MethodGetContentBounds ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (Rectangle.Empty, c.GetContentBounds (c.RowIndex), "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (Rectangle.Empty, dgv.Rows[0].Cells[0].GetContentBounds (dgv.Rows[0].Cells[0].RowIndex), "A2");
		}

		[Test]
		public void MethodGetContentBoundsOverload ()
		{
			Bitmap b = new Bitmap (1, 1);
			Graphics g = Graphics.FromImage (b);

			BaseCell c = new BaseCell ();
			Assert.AreEqual (Rectangle.Empty, c.PublicGetContentBounds (g, c.Style, c.RowIndex), "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (Rectangle.Empty, (dgv.Rows[0].Cells[0] as BaseCell).PublicGetContentBounds (g, dgv.Rows[0].Cells[0].InheritedStyle, dgv.Rows[0].Cells[0].RowIndex), "A2");
			g.Dispose ();
			b.Dispose ();
		}

		[Test]
		public void MethodGetErrorIconBounds ()
		{
			Bitmap b = new Bitmap (1, 1);
			Graphics g = Graphics.FromImage (b);

			BaseCell c = new BaseCell ();
			Assert.AreEqual (Rectangle.Empty, c.PublicGetErrorIconBounds (g, c.Style, c.RowIndex), "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (Rectangle.Empty, (dgv.Rows[0].Cells[0] as BaseCell).PublicGetErrorIconBounds (g, dgv.Rows[0].Cells[0].InheritedStyle, dgv.Rows[0].Cells[0].RowIndex), "A2");
			g.Dispose ();
			b.Dispose ();
		}

		[Test]
		public void MethodGetErrorText ()
		{
			Bitmap b = new Bitmap (1, 1);
			Graphics g = Graphics.FromImage (b);

			BaseCell c = new BaseCell ();
			Assert.AreEqual (string.Empty, c.PublicGetErrorText (c.RowIndex), "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (string.Empty, (dgv.Rows[0].Cells[0] as BaseCell).PublicGetErrorText (dgv.Rows[0].Cells[0].RowIndex), "A2");
			g.Dispose ();
			b.Dispose ();
		}

		[Test]
		public void MethodKeyEntersEditMode ()
		{
			bool result = false;
			DataGridViewCell c = new BaseCell ();
			
			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				result |= c.KeyEntersEditMode (new KeyEventArgs (k));
			
			Assert.AreEqual (false, result, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			result = false;

			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				result |= dgv.Rows[0].Cells[0].KeyEntersEditMode (new KeyEventArgs (k));

			Assert.AreEqual (false, result, "A2");
		}
		
		[Test]
		public void MethodParseFormattedValue ()
		{
			DataGridViewCell c = new FormattedBaseCell ();
			c.ValueType = typeof (bool);
			
			BooleanConverter bc = new BooleanConverter ();
			StringConverter sc = new StringConverter ();
			
			object o = c.ParseFormattedValue ("true", c.Style, sc, bc);
			Assert.AreEqual (true, (bool)o, "A1");
		}

		[Test]
		public void MethodGetInheritedContextMenuStrip ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (null, c.GetInheritedContextMenuStrip (c.RowIndex), "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (null, dgv.Rows[0].Cells[0].GetInheritedContextMenuStrip (dgv.Rows[0].Cells[0].RowIndex), "A2");

			ContextMenuStrip cms1 = new ContextMenuStrip ();
			cms1.Items.Add ("Moose");
			dgv.ContextMenuStrip = cms1;

			Assert.AreSame (cms1, dgv.Rows[0].Cells[0].GetInheritedContextMenuStrip (dgv.Rows[0].Cells[0].RowIndex), "A3");

			ContextMenuStrip cms2 = new ContextMenuStrip ();
			cms2.Items.Add ("Moose");
			dgv.Columns[0].ContextMenuStrip = cms2;

			Assert.AreSame (cms2, dgv.Rows[0].Cells[0].GetInheritedContextMenuStrip (dgv.Rows[0].Cells[0].RowIndex), "A4");

			dgv.Rows[0].ContextMenuStrip = cms1;
			Assert.AreSame (cms1, dgv.Rows[0].Cells[0].GetInheritedContextMenuStrip (dgv.Rows[0].Cells[0].RowIndex), "A5");

			dgv.Rows[0].Cells[0].ContextMenuStrip = cms2;
			Assert.AreSame (cms2, dgv.Rows[0].Cells[0].GetInheritedContextMenuStrip (dgv.Rows[0].Cells[0].RowIndex), "A6");
		}


		[Test]
		public void TestOnDataGridChangedMethod ()
		{
			DataGridView dataGridView1 = new DataGridView ();
			DataGridView dataGridView2 = new DataGridView ();

			TestHeaderCell testCell = new TestHeaderCell ();
			Assert.AreEqual (0, testCell.OnDataGridViewChangedInvokeCount, "#1");

			testCell.OnDataGridViewChangedInvokeCount = 0;
			dataGridView1.TopLeftHeaderCell = testCell;
			Assert.AreEqual (1, testCell.OnDataGridViewChangedInvokeCount, "#2");

			testCell.OnDataGridViewChangedInvokeCount = 0;
			dataGridView1.TopLeftHeaderCell = null;
			Assert.AreEqual (1, testCell.OnDataGridViewChangedInvokeCount, "#3");

			testCell.OnDataGridViewChangedInvokeCount = 0;
			dataGridView2.TopLeftHeaderCell = testCell;
			Assert.AreEqual (1, testCell.OnDataGridViewChangedInvokeCount, "#4");

			testCell.OnDataGridViewChangedInvokeCount = 0;
			dataGridView1.TopLeftHeaderCell = testCell;
			Assert.AreEqual (1, testCell.OnDataGridViewChangedInvokeCount, "#5");

			testCell.OnDataGridViewChangedInvokeCount = 0;
			dataGridView1.TopLeftHeaderCell = testCell;
			Assert.AreEqual (0, testCell.OnDataGridViewChangedInvokeCount, "#6");
			
		}

		private class TestHeaderCell : DataGridViewHeaderCell
		{
			public int OnDataGridViewChangedInvokeCount = 0;

			protected override void OnDataGridViewChanged ()
			{
				OnDataGridViewChangedInvokeCount++;
				base.OnDataGridViewChanged ();
			}
		}

		private class FormattedBaseCell : DataGridViewCell
		{
			public override Type FormattedValueType { get { return typeof (string); } }
		}
		
		private class BaseCell : DataGridViewCell
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
		
		class DataGridViewCellMockObject : DataGridViewCell
		{
			public DataGridViewCellMockObject ()
			{
			}
		}

		public class DataGridViewClipboardCell : DataGridViewCell
		{
			public DataGridViewClipboardCell ()
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
