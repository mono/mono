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
// Authors:
//	Gert Driesen <drieseng@users.sourceforge.net>
// 
// Copyright (c) 2007 Gert Driesen


using System;
using System.Data;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridViewCellCollectionTest : TestHelper
	{
		private DataGridView _dataGridView;

		[SetUp]
		protected override void SetUp () {
			DataTable dt = new DataTable ();
			dt.Columns.Add ("Date", typeof (DateTime));
			dt.Columns.Add ("Registered", typeof (bool));
			dt.Columns.Add ("Event", typeof (string));

			DataRow row = dt.NewRow ();
			row ["Date"] = new DateTime (2007, 2, 3);
			row ["Event"] = "one";
			row ["Registered"] = false;
			dt.Rows.Add (row);

			row = dt.NewRow ();
			row ["Date"] = new DateTime (2008, 3, 4);
			row ["Event"] = "two";
			row ["Registered"] = true;
			dt.Rows.Add (row);

			_dataGridView = new DataGridView ();
			_dataGridView.DataSource = dt;
			base.SetUp ();
		}

		[Test]
		[Category ("NotWorking")]
		public void Indexer_ColumnName ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (_dataGridView);
			form.Show ();

			DataGridViewCellCollection cells = _dataGridView.Rows [0].Cells;

			DataGridViewCell dateCell = cells ["Date"];
			Assert.IsNotNull (dateCell, "#A1");
			Assert.IsNotNull (dateCell.OwningColumn, "#A2");
			Assert.AreEqual ("Date", dateCell.OwningColumn.Name, "#A3");
			Assert.IsNotNull (dateCell.Value, "#A4");
			Assert.AreEqual (new DateTime (2007, 2, 3), dateCell.Value, "#A5");

			DataGridViewCell eventCell = cells ["eVeNT"];
			Assert.IsNotNull (eventCell, "#B1");
			Assert.IsNotNull (eventCell.OwningColumn, "#B2");
			Assert.AreEqual ("Event", eventCell.OwningColumn.Name, "#B3");
			Assert.IsNotNull (eventCell.Value, "#B4");
			Assert.AreEqual ("one", eventCell.Value, "#B5");

			DataGridViewCell registeredCell = cells ["Registered"];
			Assert.IsNotNull (registeredCell, "#C1");
			Assert.IsNotNull (registeredCell.OwningColumn, "#C2");
			Assert.AreEqual ("Registered", registeredCell.OwningColumn.Name, "#C3");
			Assert.IsNotNull (registeredCell.Value, "#C4");
			Assert.AreEqual (false, registeredCell.Value, "#C5");

			form.Dispose ();
		}

		[Test]
		public void Indexer_ColumnName_NotFound ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (_dataGridView);
			form.Show ();

			DataGridViewCellCollection cells = _dataGridView.Rows [0].Cells;

			try {
				DataGridViewCell cell = cells ["DoesNotExist"];
				Assert.Fail ("#A1: " + cell);
			} catch (ArgumentException ex) {
				// Column named DoesNotExist cannot be found
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("DoesNotExist") != -1, "#A5");
				Assert.IsNotNull (ex.ParamName, "#A6");
				Assert.AreEqual ("columnName", ex.ParamName, "#A7");
			}

			try {
				DataGridViewCell cell = cells [string.Empty];
				Assert.Fail ("#B1: " + cell);
			} catch (ArgumentException ex) {
				// Column named DoesNotExist cannot be found
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#B5");
				Assert.IsNotNull (ex.ParamName, "#B6");
				Assert.AreEqual ("columnName", ex.ParamName, "#B7");
			}

			form.Dispose ();
		}

		[Test]
		public void Indexer_ColumnName_Null ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (_dataGridView);
			form.Show ();

			DataGridViewCellCollection cells = _dataGridView.Rows [0].Cells;

			try {
				DataGridViewCell cell = cells [(string) null];
				Assert.Fail ("#A1: " + cell);
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("columnName", ex.ParamName, "#A6");
			}

			try {
				cells [(string) null] = cells [0];
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("columnName", ex.ParamName, "#B6");
			}

			form.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void Indexer_Index ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (_dataGridView);
			form.Show ();

			DataGridViewCellCollection cells = _dataGridView.Rows [0].Cells;

			DataGridViewCell dateCell = cells [0];
			Assert.IsNotNull (dateCell, "#A1");
			Assert.IsNotNull (dateCell.OwningColumn, "#A2");
			Assert.AreEqual ("Date", dateCell.OwningColumn.Name, "#A3");
			Assert.IsNotNull (dateCell.Value, "#A4");
			Assert.AreEqual (new DateTime (2007, 2, 3), dateCell.Value, "#A5");

			DataGridViewCell eventCell = cells [2];
			Assert.IsNotNull (eventCell, "#B1");
			Assert.IsNotNull (eventCell.OwningColumn, "#B2");
			Assert.AreEqual ("Event", eventCell.OwningColumn.Name, "#B3");
			Assert.IsNotNull (eventCell.Value, "#B4");
			Assert.AreEqual ("one", eventCell.Value, "#B5");

			DataGridViewCell registeredCell = cells [1];
			Assert.IsNotNull (registeredCell, "#C1");
			Assert.IsNotNull (registeredCell.OwningColumn, "#C2");
			Assert.AreEqual ("Registered", registeredCell.OwningColumn.Name, "#C3");
			Assert.IsNotNull (registeredCell.Value, "#C4");
			Assert.AreEqual (false, registeredCell.Value, "#C5");

			form.Dispose ();
		}

		[Test]
		public void Indexer_Index_Negative ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (_dataGridView);
			form.Show ();

			DataGridViewCellCollection cells = _dataGridView.Rows [0].Cells;

			try {
				DataGridViewCell cell = cells [-1];
				Assert.Fail ("#A1:" + cell);
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative
				// and less than the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("index", ex.ParamName, "#A6");
			}

			try {
				cells [-1] = new MockDataGridViewCell ();
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative
				// and less than the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("index", ex.ParamName, "#B6");
			}

			form.Close();
		}

		[Test]
		public void Indexer_Index_Overflow ()
		{
			DataGridViewRow row = new DataGridViewRow ();
			DataGridViewCellCollection cells = row.Cells;
			try {
				DataGridViewCell cell = cells [0];
				Assert.Fail ("#1:" + cell);
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative
				// and less than the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("index", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Indexer_Value_Null ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (_dataGridView);
			form.Show ();

			DataGridViewCellCollection cells = _dataGridView.Rows [0].Cells;

			try {
				cells ["Date"] = null;
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("value", ex.ParamName, "#A6");
			}

			try {
				cells [0] = null;
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("value", ex.ParamName, "#B6");
			}

			form.Dispose ();
		}

		class MockDataGridViewCell : DataGridViewCell
		{
		}
	}
}

