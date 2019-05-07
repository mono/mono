//
// DataGridViewCheckBoxCellTest.cs - Unit tests for
// System.Windows.Forms.DataGridViewCheckBoxCellTest
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2007 Gert Driesen
//
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


using System;
using System.Windows.Forms;

using NUnit.Framework;
using System.Drawing;
using System.Threading;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridViewCheckBoxCellTest : TestHelper
	{
		[Test]
		public void Defaults ()
		{
			DataGridViewCheckBoxCell cell = new DataGridViewCheckBoxCell ();

			Assert.AreEqual (null, cell.EditedFormattedValue, "A1");
			Assert.AreEqual (false, cell.EditingCellValueChanged, "A2");
			Assert.AreEqual (null, cell.EditType, "A3");
			Assert.AreEqual (null, cell.FalseValue, "A4");
			Assert.AreEqual (typeof (bool), cell.FormattedValueType, "A5");
			Assert.AreEqual (null, cell.IndeterminateValue, "A6");
			Assert.AreEqual (false, cell.ThreeState, "A7");
			Assert.AreEqual (null, cell.TrueValue, "A8");
			Assert.AreEqual (typeof (bool), cell.ValueType, "A9");

			Assert.AreEqual ("DataGridViewCheckBoxCell { ColumnIndex=-1, RowIndex=-1 }", cell.ToString (), "A10");
			
			cell.ThreeState = true;

			Assert.AreEqual (null, cell.EditedFormattedValue, "A11");
			Assert.AreEqual (false, cell.EditingCellValueChanged, "A12");
			Assert.AreEqual (null, cell.EditType, "A13");
			Assert.AreEqual (null, cell.FalseValue, "A14");
			Assert.AreEqual (typeof (CheckState), cell.FormattedValueType, "A15");
			Assert.AreEqual (null, cell.IndeterminateValue, "A16");
			Assert.AreEqual (true, cell.ThreeState, "A17");
			Assert.AreEqual (null, cell.TrueValue, "A18");
			Assert.AreEqual (typeof (CheckState), cell.ValueType, "A19");
		}

		[Test]
		public void Value ()
		{
			DataGridViewCheckBoxCell tbc = new DataGridViewCheckBoxCell ();
			Assert.IsNull (tbc.Value, "#1");
			tbc.Value = string.Empty;
			Assert.AreEqual (string.Empty, tbc.Value, "#2");
			tbc.Value = 5;
			Assert.AreEqual (5, tbc.Value, "#3");
			tbc.Value = null;
			Assert.IsNull (tbc.Value, "#4");
		}

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

		/* font measurement dependent
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

			Assert.AreEqual (new Rectangle (2, 4, 13, 13), dgv.Rows[0].Cells[0].ContentBounds, "A2");
		}
		*/

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
		public void EditingCellFormattedValue()
		{
			var boolCheckBoxCell = new DataGridViewCheckBoxCell();
			Assert.AreEqual(false, boolCheckBoxCell.EditingCellFormattedValue, "A1");
			boolCheckBoxCell.EditingCellFormattedValue = true;
			Assert.AreEqual(true, boolCheckBoxCell.EditingCellFormattedValue, "A2");

			var treeStateCheckBoxCell = new DataGridViewCheckBoxCell(true);
			Assert.AreEqual(CheckState.Unchecked, treeStateCheckBoxCell.EditingCellFormattedValue, "A3");
			treeStateCheckBoxCell.EditingCellFormattedValue = CheckState.Checked;
			Assert.AreEqual(CheckState.Checked, treeStateCheckBoxCell.EditingCellFormattedValue, "A4");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BoolEditingCellFormattedValueCheckStateSet()
		{
			var boolCheckBoxCell = new DataGridViewCheckBoxCell();
			boolCheckBoxCell.EditingCellFormattedValue = CheckState.Checked;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TreeStateEditingCellFormattedValueBoolSet()
		{
			var treeStateCheckBoxCell = new DataGridViewCheckBoxCell(true);
			treeStateCheckBoxCell.EditingCellFormattedValue = false;
		}

		[Test]
		public void FormattedValueType ()
		{
			BaseCell c = new BaseCell ();
			Assert.AreEqual (typeof (bool), c.FormattedValueType, "A1");
			
			c.ThreeState = true;
			Assert.AreEqual (typeof (CheckState), c.FormattedValueType, "A2");
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

		/* Font measurement dependent *
		[Test]
		public void PreferredSize ()
		{
			BaseCell c = new BaseCell ();
			Assert.AreEqual (new Size (-1, -1), c.PreferredSize, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (new Size (21, 20), dgv.Rows[0].Cells[0].PreferredSize, "A2");

			dgv.Rows[0].Cells[0].Value = "bob";
			Assert.AreEqual (new Size (21, 20), dgv.Rows[0].Cells[0].PreferredSize, "A3");

			dgv.Rows[0].Cells[0].Value = "roasted quail";
			Assert.AreEqual (new Size (21, 20), dgv.Rows[0].Cells[0].PreferredSize, "A3");
		}
		 */

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
		public void Value2 ()
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
			Assert.AreEqual (typeof (bool), c.ValueType, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (typeof (bool), dgv.Rows[0].Cells[0].ValueType, "A2");

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

		/* Font measurement dependent
		[Test]
		public void MethodGetContentBounds ()
		{
			DataGridViewCell c = new BaseCell ();
			Assert.AreEqual (Rectangle.Empty, c.GetContentBounds (c.RowIndex), "A1");
			c.Value = "hello there";
			
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			Assert.AreEqual (new Rectangle (2, 4, 13, 13), dgv.Rows[0].Cells[0].GetContentBounds (dgv.Rows[0].Cells[0].RowIndex), "A2");

			dgv.Rows[0].Cells[0].Value = "whoa whoa whoa whoa whoa whoa";
			Assert.AreEqual (new Rectangle (2, 4, 13, 13), dgv.Rows[0].Cells[0].GetContentBounds (dgv.Rows[0].Cells[0].RowIndex), "A3");
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

			Assert.AreEqual (new Rectangle (2, 4, 13, 13), (dgv.Rows[0].Cells[0] as BaseCell).PublicGetContentBounds (g, dgv.Rows[0].Cells[0].InheritedStyle, dgv.Rows[0].Cells[0].RowIndex), "A2");
			g.Dispose ();
			b.Dispose ();
		}
		*/
		
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
			string result = string.Empty;
			string expected = string.Empty;
			
			DataGridViewCell c = new BaseCell ();

			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				if (c.KeyEntersEditMode (new KeyEventArgs (k)))
					result += ((int)k).ToString () + ";";

			Assert.AreEqual (expected, result, "A1");

			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("hi", "there");

			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (c);
			dgv.Rows.Add (row);

			result = string.Empty;

			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				if (dgv.Rows[0].Cells[0].KeyEntersEditMode (new KeyEventArgs (k)))
					result += ((int)k).ToString () + ";";

			Assert.AreEqual (expected, result, "A2");

			result = string.Empty;
			dgv.EditMode = DataGridViewEditMode.EditOnEnter;

			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				if (dgv.Rows[0].Cells[0].KeyEntersEditMode (new KeyEventArgs (k)))
					result += ((int)k).ToString () + ";";

			Assert.AreEqual (expected, result, "A3");

			result = string.Empty;
			dgv.EditMode = DataGridViewEditMode.EditOnF2;

			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				if (dgv.Rows[0].Cells[0].KeyEntersEditMode (new KeyEventArgs (k)))
					result += ((int)k).ToString () + ";";

			Assert.AreEqual (expected, result, "A4");

			result = string.Empty;
			dgv.EditMode = DataGridViewEditMode.EditOnKeystroke;

			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				if (dgv.Rows[0].Cells[0].KeyEntersEditMode (new KeyEventArgs (k)))
					result += ((int)k).ToString () + ";";

			Assert.AreEqual (expected, result, "A5");

			result = string.Empty;
			dgv.EditMode = DataGridViewEditMode.EditProgrammatically;

			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				if (dgv.Rows[0].Cells[0].KeyEntersEditMode (new KeyEventArgs (k)))
					result += ((int)k).ToString () + ";";

			Assert.AreEqual (expected, result, "A6");
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

		private class FormattedBaseCell : DataGridViewCheckBoxCell
		{
			public override Type FormattedValueType { get { return typeof (string); } }
		}

		private class BaseCell : DataGridViewCheckBoxCell
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
