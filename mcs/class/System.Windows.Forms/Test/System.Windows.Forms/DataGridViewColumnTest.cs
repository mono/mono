//
// DataGridViewColumnTest.cs - Unit tests for 
// System.Windows.Forms.DataGridViewColumn
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

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridViewColumnTest : TestHelper
	{
		[SetUp]
		protected override void SetUp () {
			columnChanged = 0;
			base.SetUp ();
		}

		[Test]
		public void InitialValues ()
		{
			DataGridViewColumn dvc = new DataGridViewColumn  ();
			Assert.AreEqual (DataGridViewAutoSizeColumnMode.NotSet, dvc.AutoSizeMode, "#A dvc.AutoSizeMode");
			Assert.IsNull (dvc.CellTemplate, "#A dvc.CellTemplate");
			Assert.IsNull (dvc.CellType, "#A dvc.CellType");
			Assert.IsNull (dvc.ContextMenuStrip, "#A dvc.ContextMenuStrip");
			Assert.IsNull (dvc.DataGridView, "#A dvc.DataGridView");
			Assert.AreEqual (@"", dvc.DataPropertyName, "#A dvc.DataPropertyName");
			Assert.IsNotNull (dvc.DefaultCellStyle, "#A dvc.DefaultCellStyle");
			Assert.IsNotNull (dvc.DefaultHeaderCellType, "#A dvc.DefaultHeaderCellType");
			Assert.AreEqual (false, dvc.Displayed, "#A dvc.Displayed");
			Assert.AreEqual (-1, dvc.DisplayIndex, "#A dvc.DisplayIndex");
			Assert.AreEqual (0, dvc.DividerWidth, "#A dvc.DividerWidth");
			Assert.AreEqual (100, dvc.FillWeight, "#A dvc.FillWeight");
			Assert.AreEqual (false, dvc.Frozen, "#A dvc.Frozen");
			Assert.AreEqual (true, dvc.HasDefaultCellStyle, "#A dvc.HasDefaultCellStyle");
			Assert.IsNotNull (dvc.HeaderCell, "#A dvc.HeaderCell");
			Assert.AreEqual (@"", dvc.HeaderText, "#A dvc.HeaderText");
			Assert.AreEqual (-1, dvc.Index, "#A dvc.Index");
			Assert.AreEqual (DataGridViewAutoSizeColumnMode.NotSet, dvc.InheritedAutoSizeMode, "#A dvc.InheritedAutoSizeMode");
			Assert.IsNotNull (dvc.InheritedStyle, "#A dvc.InheritedStyle");
			Assert.AreEqual (false, dvc.IsDataBound, "#A dvc.IsDataBound");
			Assert.AreEqual (5, dvc.MinimumWidth, "#A dvc.MinimumWidth");
			Assert.AreEqual (@"", dvc.Name, "#A dvc.Name");
			Assert.AreEqual (false, dvc.ReadOnly, "#A dvc.ReadOnly");
			Assert.AreEqual (DataGridViewTriState.NotSet, dvc.Resizable, "#A dvc.Resizable");
			Assert.AreEqual (false, dvc.Selected, "#A dvc.Selected");
			Assert.IsNull (dvc.Site, "#A dvc.Site");
			Assert.AreEqual (DataGridViewColumnSortMode.NotSortable, dvc.SortMode, "#A dvc.SortMode");
			Assert.AreEqual (DataGridViewElementStates.Visible, dvc.State, "#A dvc.State");
			Assert.IsNull (dvc.Tag, "#A dvc.Tag");
			Assert.AreEqual (@"", dvc.ToolTipText, "#A dvc.ToolTipText");
			Assert.IsNull (dvc.ValueType, "#A dvc.ValueType");
			Assert.AreEqual (true, dvc.Visible, "#A dvc.Visible");
			Assert.AreEqual (100, dvc.Width, "#A dvc.Width");
		}

		[Test] // bug #80746
		public void HeaderText_NotBound ()
		{
			DataGridViewColumn dvc = new DataGridViewColumn ();
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#A1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#A2");
			dvc.Name = "A";
			dvc.HeaderText = "B";
			Assert.AreEqual ("B", dvc.HeaderText, "#B1");
			Assert.AreEqual ("B", dvc.HeaderCell.Value, "#B2");
			Assert.AreEqual ("A", dvc.Name, "#B3");
			dvc.HeaderText = "C";
			Assert.AreEqual ("C", dvc.HeaderText, "#C1");
			Assert.AreEqual ("C", dvc.HeaderCell.Value, "#C2");
			Assert.AreEqual ("A", dvc.Name, "#C3");
			dvc.HeaderText = string.Empty;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#D1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#D2");
			Assert.AreEqual ("A", dvc.Name, "#D3");
			dvc.HeaderText = "E";
			Assert.AreEqual ("E", dvc.HeaderText, "#E1");
			Assert.AreEqual ("E", dvc.HeaderCell.Value, "#E2");
			Assert.AreEqual ("A", dvc.Name, "#E3");
			dvc.HeaderText = null;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#F1");
			Assert.IsNull (dvc.HeaderCell.Value, "#F2");
			Assert.AreEqual ("A", dvc.Name, "#F3");
		}

		[Test]
		public void HeaderText_Bound ()
		{
			DataGridView dataGrid = new DataGridView ();
			DataGridViewColumn dvc = new DataGridViewTextBoxColumn ();
			dataGrid.ColumnNameChanged += new DataGridViewColumnEventHandler (
				DataGridView_ColumnNameChanged);
			dataGrid.Columns.Add (dvc);
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#A1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#A2");
			Assert.AreEqual (string.Empty, dvc.Name, "#A3");
			Assert.AreEqual (0, columnChanged, "#A4");
			dvc.HeaderText = "A";
			Assert.AreEqual ("A", dvc.HeaderText, "#B1");
			Assert.AreEqual ("A", dvc.HeaderCell.Value, "#B2");
			Assert.AreEqual (string.Empty, dvc.Name, "#B3");
			Assert.AreEqual (0, columnChanged, "#B4");
			dvc.Name = "B";
			Assert.AreEqual ("A", dvc.HeaderText, "#C1");
			Assert.AreEqual ("A", dvc.HeaderCell.Value, "#C2");
			Assert.AreEqual ("B", dvc.Name, "#C3");
			Assert.AreEqual (1, columnChanged, "#C4");
			dvc.HeaderText = "C";
			Assert.AreEqual ("C", dvc.HeaderText, "#D1");
			Assert.AreEqual ("C", dvc.HeaderCell.Value, "#D2");
			Assert.AreEqual ("B", dvc.Name, "#D3");
			Assert.AreEqual (1, columnChanged, "#D4");
			dvc.HeaderText = string.Empty;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#E1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#E2");
			Assert.AreEqual ("B", dvc.Name, "#E3");
			Assert.AreEqual (1, columnChanged, "#E4");
		}

		[Test]
		public void Name_Bound ()
		{
			DataGridView dataGrid = new DataGridView ();
			DataGridViewColumn dvc = new DataGridViewTextBoxColumn ();
			dataGrid.ColumnNameChanged += new DataGridViewColumnEventHandler (
				DataGridView_ColumnNameChanged);
			dataGrid.Columns.Add (dvc);
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#A1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#A2");
			Assert.AreEqual (string.Empty, dvc.Name, "#A3");
			Assert.AreEqual (0, columnChanged, "#A4");
			dvc.Name = "A";
			//Assert.AreEqual (string.Empty, dvc.HeaderText, "#B1");
			Assert.AreEqual ("A", dvc.HeaderCell.Value, "#B2");
			Assert.AreEqual ("A", dvc.Name, "#B3");
			Assert.AreEqual (1, columnChanged, "#B4");
			dvc.Name = "B";
			Assert.AreEqual ("B", dvc.HeaderText, "#C1");
			Assert.AreEqual ("B", dvc.HeaderCell.Value, "#C2");
			Assert.AreEqual ("B", dvc.Name, "#C3");
			Assert.AreEqual (2, columnChanged, "#C4");
			dvc.Name = null;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#D1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#D2");
			Assert.AreEqual (string.Empty, dvc.Name, "#D3");
			Assert.AreEqual (3, columnChanged, "#D4");
			dvc.HeaderText = "C";
			Assert.AreEqual ("C", dvc.HeaderText, "#E1");
			Assert.AreEqual ("C", dvc.HeaderCell.Value, "#E2");
			Assert.AreEqual (string.Empty, dvc.Name, "#E3");
			Assert.AreEqual (3, columnChanged, "#E4");
			dvc.Name = "D";
			Assert.AreEqual ("C", dvc.HeaderText, "#F1");
			Assert.AreEqual ("C", dvc.HeaderCell.Value, "#F2");
			Assert.AreEqual ("D", dvc.Name, "#F3");
			Assert.AreEqual (4, columnChanged, "#F4");
			dvc.HeaderText = null;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#G1");
			Assert.IsNull (dvc.HeaderCell.Value, "#G2");
			Assert.AreEqual ("D", dvc.Name, "#G3");
			Assert.AreEqual (4, columnChanged, "#G4");
			dvc.Name = "E";
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#H1");
			Assert.IsNull (dvc.HeaderCell.Value, "#H2");
			Assert.AreEqual ("E", dvc.Name, "#H3");
			Assert.AreEqual (5, columnChanged, "#H4");
			dvc.Name = null;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#I1");
			Assert.IsNull (dvc.HeaderCell.Value, "#I2");
			Assert.AreEqual (string.Empty, dvc.Name, "#I3");
			Assert.AreEqual (6, columnChanged, "#I4");
			dvc.Name = "F";
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#J1");
			Assert.IsNull (dvc.HeaderCell.Value, "#J2");
			Assert.AreEqual ("F", dvc.Name, "#J3");
			Assert.AreEqual (7, columnChanged, "#J4");
			dvc.Name = "G";
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#K1");
			Assert.IsNull (dvc.HeaderCell.Value, "#K2");
			Assert.AreEqual ("G", dvc.Name, "#K3");
			Assert.AreEqual (8, columnChanged, "#K4");
		}

		[Test]
		public void Name_NotBound ()
		{
			DataGridViewColumn dvc = new DataGridViewTextBoxColumn ();
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#A1");
			Assert.AreEqual (string.Empty, dvc.Name, "#A2");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#A3");
			dvc.Name = "A";
			Assert.AreEqual ("A", dvc.HeaderText, "#B1");
			Assert.AreEqual ("A", dvc.HeaderCell.Value, "#B2");
			Assert.AreEqual ("A", dvc.Name, "#B3");
			dvc.Name = "B";
			Assert.AreEqual ("B", dvc.HeaderText, "#C1");
			Assert.AreEqual ("B", dvc.HeaderCell.Value, "#C2");
			Assert.AreEqual ("B", dvc.Name, "#C3");
			dvc.Name = null;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#D1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#D2");
			Assert.AreEqual (string.Empty, dvc.Name, "#D3");
			dvc.HeaderText = "C";
			Assert.AreEqual ("C", dvc.HeaderText, "#E1");
			Assert.AreEqual ("C", dvc.HeaderCell.Value, "#E2");
			Assert.AreEqual (string.Empty, dvc.Name, "#E3");
			dvc.Name = "D";
			Assert.AreEqual ("C", dvc.HeaderText, "#F1");
			Assert.AreEqual ("C", dvc.HeaderCell.Value, "#F2");
			Assert.AreEqual ("D", dvc.Name, "#F3");
			dvc.HeaderText = null;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#G1");
			Assert.IsNull (dvc.HeaderCell.Value, "#G2");
			Assert.AreEqual ("D", dvc.Name, "#G3");
			dvc.Name = "E";
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#H1");
			Assert.IsNull (dvc.HeaderCell.Value, "#H2");
			Assert.AreEqual ("E", dvc.Name, "#H3");
		}

		void DataGridView_ColumnNameChanged (object sender, DataGridViewColumnEventArgs e)
		{
			columnChanged++;
		}

		private int columnChanged;

		[Test]
		public void CellTemplateDataGridView ()
		{
			DataGridView dgv = new DataGridView ();
			DataGridViewColumn dvc = new DataGridViewTextBoxColumn ();
			dgv.Columns.Add (dvc);
			Assert.IsNull (dvc.CellTemplate.DataGridView, "#1");
		}

		[Test]
		public void SetNewHeaderCell ()
		{
			var dgv = new DataGridView ();
			var dvc = new DataGridViewTextBoxColumn ();
			var dch = new DataGridViewColumnHeaderCell ();
			dgv.Columns.Add (dvc);
			dvc.HeaderCell = dch;
			Assert.IsNotNull (dch.DataGridView, "#1");
			Assert.True (dch.ColumnIndex >= 0, "#2");
		}
	}
}
