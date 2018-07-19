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


using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;

namespace MonoTests.System.Windows.Forms
{

	[TestFixture]
	public class DataGridViewColumnCollectionTest : TestHelper 
	{
		[Test]
		[ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Column's SortMode cannot be set to Automatic while the DataGridView control's SelectionMode is set to FullColumnSelect.")]
		public void AddFullColumnSelect ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
			dgv.Columns.Add ("A", "A");
		}
		
		[Test]
		public void Add ()
		{
			DataGridViewColumnCollection c;
			
			c = (new DataGridView ()).Columns;
			c.Add ("A", "B");
			
			DataGridViewColumn col = c [0];
			
			Assert.AreEqual ("DataGridViewTextBoxColumn { Name=A, Index=0 }", col.ToString (), "T3");
			Assert.AreEqual ("DataGridViewTextBoxColumn", col.GetType ().Name, "G2");
			
			Assert.AreEqual (DataGridViewAutoSizeColumnMode.NotSet, col.AutoSizeMode, "#A col.AutoSizeMode");
			Assert.IsNotNull (col.CellTemplate, "#A col.CellTemplate");
			Assert.IsNotNull (col.CellType, "#A col.CellType");
			Assert.IsNull (col.ContextMenuStrip, "#A col.ContextMenuStrip");
			Assert.IsNotNull (col.DataGridView, "#A col.DataGridView");
			Assert.AreEqual (@"", col.DataPropertyName, "#A col.DataPropertyName");
			Assert.IsNotNull (col.DefaultCellStyle, "#A col.DefaultCellStyle");
			Assert.IsNotNull (col.DefaultHeaderCellType, "#A col.DefaultHeaderCellType");
			Assert.AreEqual (false, col.Displayed, "#A col.Displayed");
			Assert.AreEqual (0, col.DisplayIndex, "#A col.DisplayIndex");
			Assert.AreEqual (0, col.DividerWidth, "#A col.DividerWidth");
			Assert.AreEqual (100, col.FillWeight, "#A col.FillWeight");
			Assert.AreEqual (false, col.Frozen, "#A col.Frozen");
			Assert.AreEqual (true, col.HasDefaultCellStyle, "#A col.HasDefaultCellStyle");
			Assert.IsNotNull (col.HeaderCell, "#A col.HeaderCell");
			Assert.AreEqual (@"B", col.HeaderText, "#A col.HeaderText");
			Assert.AreEqual (0, col.Index, "#A col.Index");
			Assert.AreEqual (DataGridViewAutoSizeColumnMode.None, col.InheritedAutoSizeMode, "#A col.InheritedAutoSizeMode");
			Assert.IsNotNull (col.InheritedStyle, "#A col.InheritedStyle");
			Assert.AreEqual (false, col.IsDataBound, "#A col.IsDataBound");
			Assert.AreEqual (5, col.MinimumWidth, "#A col.MinimumWidth");
			Assert.AreEqual (@"A", col.Name, "#A col.Name");
			Assert.AreEqual (false, col.ReadOnly, "#A col.ReadOnly");
			Assert.AreEqual (DataGridViewTriState.True, col.Resizable, "#A col.Resizable");
			Assert.AreEqual (false, col.Selected, "#A col.Selected");
			Assert.IsNull (col.Site, "#A col.Site");
			Assert.AreEqual (DataGridViewColumnSortMode.Automatic, col.SortMode, "#A col.SortMode");
			Assert.AreEqual (DataGridViewElementStates.Visible, col.State, "#A col.State");
			Assert.IsNull (col.Tag, "#A col.Tag");
			Assert.AreEqual (@"", col.ToolTipText, "#A col.ToolTipText");
			Assert.IsNull (col.ValueType, "#A col.ValueType");
			Assert.AreEqual (true, col.Visible, "#A col.Visible");
			Assert.AreEqual (100, col.Width, "#A col.Width");
			
		}

		[Test]
		public void IndexUpdatedOnColumnCollectionChange ()
		{
			DataGridView dgv = new DataGridView ();

			Form f = new Form ();
			f.Controls.Add (dgv);
			f.Show ();

			dgv.Columns.Add ("A1", "A1");
			Assert.AreEqual (0, dgv.Columns[0].Index, "#1");
			Assert.AreEqual (0, dgv.Columns[0].DisplayIndex, "#2");
			Assert.AreEqual ("A1", dgv.Columns[0].Name, "#3");


			dgv.Columns.Add ("A2", "A2");
			Assert.AreEqual (0, dgv.Columns[0].Index, "#10");
			Assert.AreEqual (0, dgv.Columns[0].DisplayIndex, "#11");
			Assert.AreEqual ("A1", dgv.Columns[0].Name, "#12");
			Assert.AreEqual (1, dgv.Columns[1].Index, "#13");
			Assert.AreEqual (1, dgv.Columns[1].DisplayIndex, "#14");
			Assert.AreEqual ("A2", dgv.Columns[1].Name, "#15");

			dgv.Columns.Insert (0, new DataGridViewTextBoxColumn ());
			Assert.AreEqual (0, dgv.Columns[0].Index, "#20");
			Assert.AreEqual (0, dgv.Columns[0].DisplayIndex, "#21");
			Assert.AreEqual ("", dgv.Columns[0].Name, "#22");

			Assert.AreEqual (1, dgv.Columns[1].Index, "#23");
			Assert.AreEqual (1, dgv.Columns[1].DisplayIndex, "#24");
			Assert.AreEqual ("A1", dgv.Columns[1].Name, "#25");
			Assert.AreEqual (2, dgv.Columns[2].Index, "#26");
			Assert.AreEqual (2, dgv.Columns[2].DisplayIndex, "#27");
			Assert.AreEqual ("A2", dgv.Columns[2].Name, "#28");

			dgv.Columns.RemoveAt (1);
			Assert.AreEqual (0, dgv.Columns[0].Index, "A7");
			Assert.AreEqual (0, dgv.Columns[0].DisplayIndex, "B7");
			Assert.AreEqual (1, dgv.Columns[1].Index, "A8");
			Assert.AreEqual (1, dgv.Columns[1].DisplayIndex, "B8");

			dgv.Columns.RemoveAt (0);
			Assert.AreEqual (0, dgv.Columns[0].Index, "A9");
			Assert.AreEqual (0, dgv.Columns[0].DisplayIndex, "B9");

			f.Close ();
			f.Dispose ();
		}
	}
}
