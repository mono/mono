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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jonathan Pobst  (monkey@jpobst.com)
//

#if NET_2_0

using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Data;
using System.Collections.Generic;

namespace MonoTests.System.Windows.Forms.DataGridViewBindingTest
{

	[TestFixture]
	public class DataSetBindingTest : TestHelper
	{
		[Test]
		public void TestDataSet ()
		{
			// Binding to a DataSet doesn't work unless you specify DataMember
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			DataSet ds = new DataSet ();
			
			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");
			dt.Columns.Add ("Sex");

			dt.Rows.Add (1, "Kermit", "Male");
			dt.Rows.Add (2, "Miss Piggy", "Female");
			dt.Rows.Add (3, "Gonzo", "Male");
			
			DataGridView dgv = new DataGridView ();
			dgv.DataSource = ds;
	
			f.Controls.Add (dgv);
			f.Show ();
			
			Assert.AreEqual (0, dgv.Columns.Count, "A1");
			Assert.AreEqual (0, dgv.Rows.Count, "A2");
			
			dgv.DataMember = "Muppets";

			Assert.AreEqual (3, dgv.Columns.Count, "A3");
			Assert.AreEqual (4, dgv.Rows.Count, "A4");			
			
			f.Dispose ();
		}

		[Test]
		public void TestBasic ()
		{
			// Binding to a basic DataTable
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			DataSet ds = new DataSet ();

			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");
			dt.Columns.Add ("Sex");

			dt.Rows.Add (1, "Kermit", "Male");
			dt.Rows.Add (2, "Miss Piggy", "Female");
			dt.Rows.Add (3, "Gonzo", "Male");

			DataGridView dgv = new DataGridView ();
			dgv.DataSource = dt;

			f.Controls.Add (dgv);
			f.Show ();

			Assert.AreEqual (3, dgv.ColumnCount, "A1");
			Assert.AreEqual (4, dgv.RowCount, "A2");

			Assert.AreEqual ("ID", dgv.Columns[0].Name, "A3");
			Assert.AreEqual ("ID", dgv.Columns[0].DataPropertyName, "A4");
			Assert.AreEqual (0, dgv.Columns[0].DisplayIndex, "A5");
			Assert.AreEqual ("ID", dgv.Columns[0].HeaderText, "A6");
			Assert.AreEqual (0, dgv.Columns[0].Index, "A7");
			Assert.AreEqual (true, dgv.Columns[0].IsDataBound, "A8");
			Assert.AreEqual (false, dgv.Columns[0].ReadOnly, "A9");
			Assert.AreEqual (true, dgv.Columns[0].Visible, "A10");
			Assert.AreEqual ("System.Windows.Forms.DataGridViewTextBoxCell", dgv.Columns[0].CellType.ToString (), "A11");
			Assert.AreEqual ("System.String", dgv.Columns[0].ValueType.ToString (), "A11-B");
			Assert.AreEqual ("System.Windows.Forms.DataGridViewTextBoxColumn", dgv.Columns[0].GetType ().ToString (), "A11-C");

			Assert.AreEqual ("Name", dgv.Columns[1].Name, "A12");
			Assert.AreEqual ("Name", dgv.Columns[1].DataPropertyName, "A13");
			Assert.AreEqual (1, dgv.Columns[1].DisplayIndex, "A14");
			Assert.AreEqual ("Name", dgv.Columns[1].HeaderText, "A15");
			Assert.AreEqual (1, dgv.Columns[1].Index, "A16");
			Assert.AreEqual (true, dgv.Columns[1].IsDataBound, "A17");
			Assert.AreEqual (false, dgv.Columns[1].ReadOnly, "A18");
			Assert.AreEqual (true, dgv.Columns[1].Visible, "A19");
			Assert.AreEqual ("System.Windows.Forms.DataGridViewTextBoxCell", dgv.Columns[1].CellType.ToString (), "A20");
			Assert.AreEqual ("System.String", dgv.Columns[1].ValueType.ToString (), "A20-B");

			Assert.AreEqual ("Sex", dgv.Columns[2].Name, "A21");
			Assert.AreEqual ("Sex", dgv.Columns[2].DataPropertyName, "A22");
			Assert.AreEqual (2, dgv.Columns[2].DisplayIndex, "A23");
			Assert.AreEqual ("Sex", dgv.Columns[2].HeaderText, "A24");
			Assert.AreEqual (2, dgv.Columns[2].Index, "A25");
			Assert.AreEqual (true, dgv.Columns[2].IsDataBound, "A26");
			Assert.AreEqual (false, dgv.Columns[2].ReadOnly, "A27");
			Assert.AreEqual (true, dgv.Columns[2].Visible, "A28");
			Assert.AreEqual ("System.Windows.Forms.DataGridViewTextBoxCell", dgv.Columns[2].CellType.ToString (), "A29");
			Assert.AreEqual ("System.String", dgv.Columns[2].ValueType.ToString (), "A29-B");

			Assert.AreEqual ("1", dgv.Rows[0].Cells[0].Value, "A30");
			Assert.AreEqual ("Kermit", dgv.Rows[0].Cells[1].Value, "A31");
			Assert.AreEqual ("Male", dgv.Rows[0].Cells[2].Value, "A32");
			Assert.AreEqual ("2", dgv.Rows[1].Cells[0].Value, "A33");
			Assert.AreEqual ("Miss Piggy", dgv.Rows[1].Cells[1].Value, "A34");
			Assert.AreEqual ("Female", dgv.Rows[1].Cells[2].Value, "A35");
			Assert.AreEqual ("3", dgv.Rows[2].Cells[0].Value, "A36");
			Assert.AreEqual ("Gonzo", dgv.Rows[2].Cells[1].Value, "A37");
			Assert.AreEqual ("Male", dgv.Rows[2].Cells[2].Value, "A38");
			
			f.Dispose ();
		}

		[Test]
		public void TestCheckBoxColumn ()
		{
			// Binding to a basic DataTable with a boolean value
			Form f = new Form ();
			f.ShowInTaskbar = false;

			DataSet ds = new DataSet ();

			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");
			dt.Columns.Add ("IsFunny", typeof (bool));

			dt.Rows.Add (1, "Kermit", "true");
			dt.Rows.Add (2, "Miss Piggy", "false");
			dt.Rows.Add (3, "Gonzo", DBNull.Value);
			dt.Rows.Add (4, "Animal", true);
			dt.Rows.Add (5, "Fozzy", false);
			dt.Rows.Add (6, "Beaker", "TRUE");
			dt.Rows.Add (7, "Bunsen", "fALSe");
			dt.Rows.Add (8, "Sweedish Chef", 1);
			dt.Rows.Add (9, "Rolf", 0);

			DataGridView dgv = new DataGridView ();
			dgv.DataSource = dt;

			f.Controls.Add (dgv);
			f.Show ();

			Assert.AreEqual (3, dgv.ColumnCount, "A1");
			Assert.AreEqual (10, dgv.RowCount, "A2");

			Assert.AreEqual ("IsFunny", dgv.Columns[2].Name, "A3");
			Assert.AreEqual ("IsFunny", dgv.Columns[2].DataPropertyName, "A4");
			Assert.AreEqual (2, dgv.Columns[2].DisplayIndex, "A5");
			Assert.AreEqual ("IsFunny", dgv.Columns[2].HeaderText, "A6");
			Assert.AreEqual (2, dgv.Columns[2].Index, "A7");
			Assert.AreEqual (true, dgv.Columns[2].IsDataBound, "A8");
			Assert.AreEqual (false, dgv.Columns[2].ReadOnly, "A9");
			Assert.AreEqual (true, dgv.Columns[2].Visible, "A10");
			Assert.AreEqual ("System.Windows.Forms.DataGridViewCheckBoxCell", dgv.Columns[2].CellType.ToString (), "A11");
			Assert.AreEqual ("System.Boolean", dgv.Columns[2].ValueType.ToString (), "A12");
			Assert.AreEqual ("System.Windows.Forms.DataGridViewCheckBoxColumn", dgv.Columns[2].GetType ().ToString (), "A12-B");

			Assert.AreEqual (true, dgv.Rows[0].Cells[2].Value, "A13");
			Assert.AreEqual (false, dgv.Rows[1].Cells[2].Value, "A14");
			Assert.AreEqual (DBNull.Value, dgv.Rows[2].Cells[2].Value, "A15");
			Assert.AreEqual (true, dgv.Rows[3].Cells[2].Value, "A16");
			Assert.AreEqual (false, dgv.Rows[4].Cells[2].Value, "A17");
			Assert.AreEqual (true, dgv.Rows[5].Cells[2].Value, "A18");
			Assert.AreEqual (false, dgv.Rows[6].Cells[2].Value, "A19");
			Assert.AreEqual (true, dgv.Rows[7].Cells[2].Value, "A20");
			Assert.AreEqual (false, dgv.Rows[8].Cells[2].Value, "A21");

			Assert.AreEqual ("System.Windows.Forms.DataGridViewCheckBoxCell", dgv.Rows[8].Cells[2].GetType ().ToString (), "A22");
			
			f.Dispose ();
		}

		[Test]
		public void TestAutoGenerateColumns ()
		{
			// Binding when AutoGenerateColumns is false
			Form f = new Form ();
			f.ShowInTaskbar = false;

			DataSet ds = new DataSet ();

			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");

			dt.Rows.Add (1, "Kermit");
			dt.Rows.Add (2, "Miss Piggy");
			dt.Rows.Add (3, "Gonzo");

			DataGridView dgv = new DataGridView ();
			dgv.AutoGenerateColumns = false;
			dgv.DataSource = dt;

			f.Controls.Add (dgv);
			f.Show ();

			Assert.AreEqual (0, dgv.ColumnCount, "A1");
			Assert.AreEqual (0, dgv.RowCount, "A2");

			dgv.DataSource = null;
			
			DataGridViewTextBoxColumn col1 = new DataGridViewTextBoxColumn ();
			col1.DataPropertyName = "Name";
			dgv.Columns.Add (col1);

			dgv.DataSource = dt;

			Assert.AreEqual (1, dgv.ColumnCount, "A3");
			Assert.AreEqual (4, dgv.RowCount, "A4");

			Assert.AreEqual ("Kermit", dgv.Rows[0].Cells[0].Value, "A5");

			dgv.DataSource = null;

			DataGridViewTextBoxColumn col2 = new DataGridViewTextBoxColumn ();
			col2.DataPropertyName = "id";
			dgv.Columns.Add (col2);

			dgv.DataSource = dt;

			Assert.AreEqual (2, dgv.ColumnCount, "A6");
			Assert.AreEqual (4, dgv.RowCount, "A7");

			Assert.AreEqual ("Kermit", dgv.Rows[0].Cells[0].Value, "A8");
			Assert.AreEqual ("1", dgv.Rows[0].Cells[1].Value, "A9");

			f.Dispose ();
		}
		
		[Test]	// Bug #399601
		public void TestAddingWithoutAutoGenerate ()
		{
			// Binding when AutoGenerateColumns is false
			// and adding rows to the dataset
			Form f = new Form ();
			f.ShowInTaskbar = false;

			DataSet ds = new DataSet ();

			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");

			DataGridView dgv = new DataGridView ();
			dgv.AutoGenerateColumns = false;
			dgv.AllowUserToAddRows = false;
			
			DataGridViewTextBoxColumn col1 = new DataGridViewTextBoxColumn ();
			col1.DataPropertyName = "Name";
			dgv.Columns.Add (col1);

			dgv.DataSource = dt;

			f.Controls.Add (dgv);
			f.Show ();

			dt.Rows.Add (1, "Kermit");
			dt.Rows.Add (2, "Miss Piggy");
			dt.Rows.Add (3, "Gonzo");

			Assert.AreEqual (1, dgv.ColumnCount, "A1");
			Assert.AreEqual (3, dgv.RowCount, "A2");

			f.Dispose ();
		}

		[Test]
		public void TestDeleting ()
		{
			// Binding when AutoGenerateColumns is false
			// and deleting rows from the dataset and DGV
			Form f = new Form ();
			f.ShowInTaskbar = false;

			DataSet ds = new DataSet ();

			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");

			DataGridView dgv = new DataGridView ();
			dgv.AutoGenerateColumns = false;
			dgv.AllowUserToAddRows = false;

			DataGridViewTextBoxColumn col1 = new DataGridViewTextBoxColumn ();
			col1.DataPropertyName = "Name";
			dgv.Columns.Add (col1);

			dgv.DataSource = dt;

			f.Controls.Add (dgv);
			f.Show ();

			dt.Rows.Add (1, "Kermit");
			dt.Rows.Add (2, "Miss Piggy");
			dt.Rows.Add (3, "Gonzo");

			Assert.AreEqual (1, dgv.ColumnCount, "A1");
			Assert.AreEqual (3, dgv.RowCount, "A2");

			dt.Rows[2].Delete ();
			Assert.AreEqual (2, dgv.RowCount, "A3");

			dgv.Rows.RemoveAt (0);
			Assert.AreEqual (1, dgv.RowCount, "A4");
			
			f.Dispose();
		}

		[Test]
		public void TestChangingDataSetAfterSettingDataSource ()
		{
			// Binding when AutoGenerateColumns is false
			// and deleting rows from the dataset and DGV
			Form f = new Form ();
			f.ShowInTaskbar = false;

			DataSet ds = new DataSet ();

			DataGridView dgv = new DataGridView ();
			dgv.AutoGenerateColumns = false;
			dgv.AllowUserToAddRows = false;

			DataGridViewTextBoxColumn col1 = new DataGridViewTextBoxColumn ();
			col1.DataPropertyName = "Name";
			dgv.Columns.Add (col1);

			dgv.DataSource = ds;
			dgv.DataMember = "Muppets";

			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");

			f.Controls.Add (dgv);
			f.Show ();

			dt.Rows.Add (1, "Kermit");
			dt.Rows.Add (2, "Miss Piggy");
			dt.Rows.Add (3, "Gonzo");

			Assert.AreEqual (1, dgv.ColumnCount, "A1");
			Assert.AreEqual (3, dgv.RowCount, "A2");

			dt.Rows[2].Delete ();
			Assert.AreEqual (2, dgv.RowCount, "A3");

			dgv.Rows.RemoveAt (0);
			Assert.AreEqual (1, dgv.RowCount, "A4");

			f.Dispose ();
		}

		[Test]  // bug #448005
		public void TestClearing ()
		{
			// Binding to a DataSet doesn't work unless you specify DataMember
			Form f = new Form ();
			f.ShowInTaskbar = false;

			DataSet ds = new DataSet ();

			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");
			dt.Columns.Add ("Sex");

			dt.Rows.Add (1, "Kermit", "Male");
			dt.Rows.Add (2, "Miss Piggy", "Female");
			dt.Rows.Add (3, "Gonzo", "Male");

			DataGridView dgv = new DataGridView ();
			dgv.DataSource = ds;

			f.Controls.Add (dgv);
			f.Show ();

			dgv.DataMember = "Muppets";

			Assert.AreEqual (3, dgv.Columns.Count, "A1");
			Assert.AreEqual (4, dgv.Rows.Count, "A2");
			
			ds.Tables[0].Clear ();
			
			Assert.AreEqual (3, dgv.Columns.Count, "A3");
			Assert.AreEqual (1, dgv.Rows.Count, "A4");

			f.Dispose ();
		}

		[Test]  // bug #462019
		public void TestCreatingColumnsAfterBind ()
		{
			// When columns are added, we need to rebind.
			Form f = new Form ();
			f.ShowInTaskbar = false;

			DataSet ds = new DataSet ();

			DataTable dt = ds.Tables.Add ("Muppets");

			dt.Columns.Add ("ID");
			dt.Columns.Add ("Name");
			dt.Columns.Add ("Sex");

			dt.Rows.Add (1, "Kermit", "Male");
			dt.Rows.Add (2, "Miss Piggy", "Female");
			dt.Rows.Add (3, "Gonzo", "Male");

			DataGridView dgv = new DataGridView ();
			dgv.AutoGenerateColumns = false;
			dgv.AllowUserToAddRows = false;
			dgv.DataSource = ds;
			dgv.DataMember = "Muppets";

			f.Controls.Add (dgv);
			f.Show ();

			Assert.AreEqual (0, dgv.Rows.Count, "A1");
			
			DataGridViewColumn col = new DataGridViewTextBoxColumn ();
			col.DataPropertyName = "ID";
			dgv.Columns.Add (col);

			Assert.AreEqual (3, dgv.Rows.Count, "A1");

			f.Dispose ();
		}
	}
	
	[TestFixture]
	public class BindingListTest : TestHelper
	{
		[Test]	// bug #325239
		public void TestNullItemInList ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			// The list contains one object, but the object is null
			IList<Customer> list = new Customer[1];

			DataGridView dgv = new DataGridView ();
			dgv.DataSource = new BindingList<Customer> (list);

			f.Controls.Add (dgv);
			f.Show ();

			Assert.AreEqual (1, dgv.ColumnCount, "A1");
			Assert.AreEqual (2, dgv.RowCount, "A2");

			f.Dispose ();
		}

		private class Customer
		{
			string name;
			
			public string Name {
				get { return name; }
				set { name = value; }
			}
		}
	}

	[TestFixture]
	public class ArrayTest : TestHelper
	{
		[Test]	// bug #337470
		public void TestNestedCollections ()
		{
			// The grid should not accept collection properties, like Names
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Array customers = new Customer[1];
			customers.SetValue (new Customer (), 0);
			
			DataGridView dgv = new DataGridView ();
			dgv.DataSource = customers;

			f.Controls.Add (dgv);
			f.Show ();

			Assert.AreEqual (1, dgv.ColumnCount, "A1");
			Assert.AreEqual ("Name", dgv.Columns[0].Name, "A2");

			f.Dispose ();
		}

		private class Customer
		{
			public string Name { get { return "Kermit"; } }
			public string[] Names { get { return new string[] { "Kermit", "Gonzo" }; } }
		}
	}

	[TestFixture]
	public class BindingSourceTest : TestHelper
	{
		[Test]	// bug #345483
		public void TestBindingSource ()
		{
			// The grid has to extract the List from the BindingSource
			Form f = new Form ();
			f.ShowInTaskbar = false;

			BindingSource BindingSource = new BindingSource ();

			DataSet dataSet1 = new DataSet ();

			dataSet1.Tables.Add ();
			dataSet1.Tables[0].Columns.Add ();
			dataSet1.Tables[0].Columns.Add ();
			dataSet1.Tables[0].Columns.Add ();
			dataSet1.Tables[0].Columns.Add ();
			dataSet1.Tables[0].Columns.Add ();
			dataSet1.Tables[0].Rows.Add ("111111", "222222", "333333", "444444", "555555");

			BindingSource.DataSource = dataSet1.Tables[0];

			DataGridView dgv = new DataGridView ();
			dgv.DataSource = BindingSource;

			f.Controls.Add (dgv);
			f.Show ();

			Assert.AreEqual (5, dgv.ColumnCount, "A1");
			Assert.AreEqual (2, dgv.RowCount, "A2");

			Assert.AreEqual ("Column1", dgv.Columns[0].Name, "A3");
			Assert.AreEqual ("111111", dgv.Rows[0].Cells[0].Value, "A4");

			f.Dispose ();
		}

		private class Customer
		{
			public string Name { get { return "Kermit"; } }
			public string[] Names { get { return new string[] { "Kermit", "Gonzo" }; } }
		}
	}
}
#endif