// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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
using System.Collections;
using System.Data;
using System.Globalization;
using System.Collections.Generic;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests_System.Data
{
	[TestFixture]
	public class DataTableTest2
	{
		private bool _EventTriggered;
		private bool EventRaised;
		private bool EventValues;

		class ProtectedTestClass : DataTable
		{
			public ProtectedTestClass ()
			{
				this.Columns.Add ("Id", typeof (int));
				this.Columns.Add ("Value", typeof (string));
				this.Rows.Add (new object[] {1, "one"});
				this.Rows.Add (new object[] {2, "two"});
				this.AcceptChanges ();
			}

			public void OnColumnChanged_Test ()
			{
				OnColumnChanged (new DataColumnChangeEventArgs (
					this.Rows [0], this.Columns ["Value"],
					"NewValue"));
			}

			public void OnColumnChanging_Test ()
			{
				OnColumnChanging (new DataColumnChangeEventArgs (
					this.Rows [0], this.Columns ["Value"],
					"NewValue"));
			}

			public void OnRemoveColumn_Test ()
			{
				OnRemoveColumn (this.Columns [0]);
			}

			public DataTable CreateInstance_Test ()
			{
				return CreateInstance ();
			}

			public void OnRowChanged_Test (DataRowAction drAction)
			{
				base.OnRowChanged (new DataRowChangeEventArgs (this.Rows [0], drAction));
			}

			public void OnRowChanging_Test (DataRowAction drAction)
			{
				base.OnRowChanging (new DataRowChangeEventArgs (this.Rows [0], drAction));
			}

			public void OnRowDeleted_Test (DataRowAction drAction)
			{
				base.OnRowDeleted (new DataRowChangeEventArgs (this.Rows [0], drAction));
			}

			public void OnRowDeleting_Test (DataRowAction drAction)
			{
				base.OnRowDeleting (new DataRowChangeEventArgs (this.Rows [0], drAction));
			}
		}

		[Test]
		public void AcceptChanges ()
		{
			String sNewValue = "NewValue";
			DataRow drModified,drDeleted,drAdded;
			DataTable dt = DataProvider.CreateParentDataTable();

			drModified = dt.Rows[0];
			drModified[1] = sNewValue; //DataRowState = Modified, DataRowVersion = Proposed

			drDeleted = dt.Rows[1];
			drDeleted.Delete(); //DataRowState =  Deleted

			drAdded = dt.NewRow();
			dt.Rows.Add(drAdded); //DataRowState =  Added

			dt.AcceptChanges();

			// AcceptChanges - Unchanged1
			Assert.AreEqual(DataRowState.Unchanged, drModified.RowState, "DT1");

			// AcceptChanges - Current
			Assert.AreEqual(sNewValue, drModified[1,DataRowVersion.Current], "DT2");

			// AcceptChanges - Unchanged2
			Assert.AreEqual(DataRowState.Unchanged, drAdded.RowState, "DT3");

			// AcceptChanges - Detached
			Assert.AreEqual(DataRowState.Detached, drDeleted.RowState, "DT4");
		}

		[Test]
		public void ChildRelations ()
		{
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable();
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			DataRelationCollection drlCollection;
			DataRelation drl = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);

			// Checking ChildRelations - default value
			//Check default
			drlCollection = dtParent.ChildRelations;
			Assert.AreEqual(0, drlCollection.Count, "DT5");

			ds.Relations.Add(drl);
			drlCollection = dtParent.ChildRelations;

			// Checking ChildRelations Count
			Assert.AreEqual(1, drlCollection.Count, "DT6");

			// Checking ChildRelations Value
			Assert.AreEqual(drl, drlCollection [0], "DT7");
		}

		[Test]
		public void Clear ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Clear();
			Assert.AreEqual(0, dt.Rows.Count, "DT8");
		}

		[Test]
		public void Clone ()
		{
			DataTable dt1,dt2 = DataProvider.CreateParentDataTable();
			dt2.Constraints.Add ("Unique", dt2.Columns[0], true);
			dt2.Columns[0].DefaultValue = 7;

			dt1 = dt2.Clone ();

			for (int i = 0; i < dt2.Constraints.Count; i++) {
				// Clone - Constraints[{0}],i)
				Assert.AreEqual (dt2.Constraints [i].ConstraintName,
					dt1.Constraints[i].ConstraintName, "DT9");
			}

			for (int i = 0; i < dt2.Columns.Count; i++) {
				// Clone - Columns[{0}].ColumnName,i)
				Assert.AreEqual(dt2.Columns[i].ColumnName, dt1.Columns[i].ColumnName, "DT10");

				// Clone - Columns[{0}].DataType,i)
				Assert.AreEqual(dt2.Columns[i].DataType, dt1.Columns[i].DataType, "DT11");
			}
		}

		[Test]
		public void ColumnChanged ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.ColumnChanged += new DataColumnChangeEventHandler (Column_Changed);

			_EventTriggered = false;
			// ColumnChanged - EventTriggered
			dt.Rows[0][1] = "NewValue";
			Assert.IsTrue (_EventTriggered, "DT12");

			_EventTriggered = false;
			dt.ColumnChanged -= new DataColumnChangeEventHandler (Column_Changed);
			// ColumnChanged - NO EventTriggered
			dt.Rows[0][1] = "VeryNewValue";
			Assert.IsFalse (_EventTriggered, "DT13");
		}

		private void Column_Changed (object sender, DataColumnChangeEventArgs e)
		{
			_EventTriggered = true;
		}

		[Test]
		public void ColumnChanging ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.ColumnChanging += new DataColumnChangeEventHandler (Column_Changeding);

			_EventTriggered = false;
			// ColumnChanged - EventTriggered
			dt.Rows[0][1] = "NewValue";
			Assert.IsTrue (_EventTriggered, "DT14");

			_EventTriggered = false;
			dt.ColumnChanging  -= new DataColumnChangeEventHandler (Column_Changeding);
			// ColumnChanged - NO EventTriggered
			dt.Rows[0][1] = "VeryNewValue";
			Assert.IsFalse (_EventTriggered, "DT15");
		}

		private void Column_Changeding (object sender, DataColumnChangeEventArgs e)
		{
			_EventTriggered = true;
		}

		[Test]
		public void Columns ()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataColumnCollection dcl = dtParent.Columns;

			Assert.IsNotNull (dcl, "#A1");
			Assert.AreEqual(6, dcl.Count, "#A2");
			dtParent.Columns.Add(new DataColumn("Test"));
			Assert.AreEqual(7, dcl.Count, "#A3");
			DataColumn tmp = dtParent.Columns["TEST"];
			Assert.AreEqual(dtParent.Columns["Test"], tmp, "#A4");
			dtParent.Columns.Add(new DataColumn("test"));
			Assert.AreEqual(8, dcl.Count, "#A5");

			try {
				tmp = dtParent.Columns ["TEST"];
				Assert.Fail("#B1");
			} catch (ArgumentException ex) {
				// The given name 'TEST' matches at least two
				// names in the collection object with different
				// cases, but does not match either of them with
				// the same case
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'TEST'") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}
		}

		[Test]
		public void Compute ()
		{
			DataTable dt = DataProvider.CreateChildDataTable();

			//Get expected
			DataRow[] drArr = dt.Select("ParentId=1");
			Int64 iExSum = 0;
			foreach (DataRow dr in drArr)
				iExSum += (int)dr["ChildId"];
			object objCompute = null;
			// Compute - sum values
			objCompute = dt.Compute ("Sum(ChildId)", "ParentId=1");
			Assert.AreEqual(Int64.Parse (objCompute.ToString()), Int64.Parse(iExSum.ToString()), "DT23");

			// Compute - sum type
			Assert.AreEqual(typeof (Int64), objCompute.GetType (), "DT24");

			//get expected
			double iExAvg = 0;
			drArr = dt.Select("ParentId=5");
			foreach (DataRow dr in drArr)
				iExAvg += (double)dr["ChildDouble"];
			iExAvg = iExAvg / drArr.Length;

			// Compute - Avg value
			objCompute = dt.Compute("Avg(ChildDouble)", "ParentId=5");
			Assert.AreEqual(double.Parse(objCompute.ToString()), double.Parse(iExAvg.ToString()), "DT25");

			// Compute - Avg type
			Assert.AreEqual(typeof(double), objCompute.GetType(), "DT26");
		}

		[Test]
		public void Constraints ()
		{
			DataTable dtParent;
			ConstraintCollection consColl;
			dtParent= DataProvider.CreateParentDataTable();

			consColl = dtParent.Constraints;
			// Checking Constraints  != null 
			Assert.IsNotNull (consColl, "DT27");

			// Checking Constraints Count
			Assert.AreEqual (0, consColl.Count, "DT28");

			// Checking Constraints Count
			//Add primary key
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
			Assert.AreEqual (1, consColl.Count, "DT29");
		}

		[Test]
		public void Copy ()
		{
			DataTable dt1,dt2 = DataProvider.CreateParentDataTable();
			dt2.Constraints.Add("Unique",dt2.Columns[0],true);
			dt2.Columns[0].DefaultValue=7;

			dt1 = dt2.Copy();

			for (int i = 0; i < dt2.Constraints.Count; i++) {
				// Copy - Constraints[{0}],i)
				Assert.AreEqual (dt2.Constraints[i].ConstraintName,
					dt1.Constraints[i].ConstraintName, "DT30");
			}

			for (int i = 0; i < dt2.Columns.Count; i++) {
				// Copy - Columns[{0}].ColumnName,i)
				Assert.AreEqual (dt2.Columns [i].ColumnName, dt1.Columns [i].ColumnName, "DT31");

				// Copy - Columns[{0}].DataType,i)
				Assert.AreEqual (dt2.Columns [i].DataType, dt1.Columns[i].DataType, "DT32");
			}

			DataRow[] drArr1,drArr2;
			drArr1 = dt1.Select (string.Empty);
			drArr2 = dt2.Select (string.Empty);
			for (int i = 0; i < drArr1.Length; i++) {
				// Copy - Data [ParentId]{0} ,i)
				Assert.AreEqual (drArr2[i]["ParentId"], drArr1[i]["ParentId"], "DT33");
				// Copy - Data [String1]{0} ,i)
				Assert.AreEqual (drArr2[i]["String1"], drArr1[i]["String1"], "DT34");
				// Copy - Data [String2]{0} ,i)
				Assert.AreEqual (drArr2[i]["String2"], drArr1[i]["String2"], "DT35");
			}
		}

		[Test]
		public void CreateInstance ()
		{
			// CreateInstance
			ProtectedTestClass C = new ProtectedTestClass();
			DataTable dt = C.CreateInstance_Test();
			Assert.IsNotNull(dt, "DT36");
		}

		[Test]
		public void DataSet ()
		{
			DataTable dtParent;
			DataSet ds;
			dtParent= DataProvider.CreateParentDataTable();

			ds = dtParent.DataSet;

			// Checking DataSet == null
			Assert.IsNull(ds, "DT37");

			// Checking DataSet != null
			ds = new DataSet("MyDataSet");
			ds.Tables.Add(dtParent);
			Assert.IsNotNull(dtParent.DataSet, "DT38");

			// Checking DataSet Name
			Assert.AreEqual("MyDataSet", dtParent.DataSet.DataSetName, "DT39");
		}

		[Test]
		public void DefaultView ()
		{
			DataTable dtParent;
			DataView dv;
			dtParent= DataProvider.CreateParentDataTable();
			dv = dtParent.DefaultView;
			Assert.IsNotNull (dv, "DT40");
		}

		[Test]
		public void EndLoadData ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Columns[0].AllowDBNull = false;

			// EndLoadData
			dt.BeginLoadData();
			dt.LoadDataRow(new object [] { null, "A", "B" }, false);

			try {
				dt.EndLoadData ();
				Assert.Fail ("#1");
			} catch (ConstraintException ex) {
				// Failed to enable constraints. One or more rows
				// contain values violating non-null, unique, or
				// foreign-key constraints
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // LoadDataRow (Object [], Boolean)
		public void LoadDataRow1_Column_ReadOnly ()
		{
			DataTable dt = new DataTable ("myTable");
			DataColumn dcId = new DataColumn ("Id", typeof (int));
			dcId.ReadOnly = true;
			dt.Columns.Add (dcId);
			DataColumn dcName = new DataColumn ("Name", typeof (string));
			dcName.ReadOnly = true;
			dt.Columns.Add (dcName);
			DataColumn dcPassword = new DataColumn ("Password", typeof (string));
			dt.Columns.Add (dcPassword);
			dt.PrimaryKey = new DataColumn [] { dcId };

			dt.Rows.Add (new object [] { 5, "Mono", "guess" });
			dt.AcceptChanges ();
			dt.LoadDataRow (new object [] { 5, "SysData", "what" }, true);

			Assert.AreEqual (1, dt.Rows.Count, "#1");
			DataRow row = dt.Rows.Find (5);
			Assert.IsNotNull (row, "#2");
			Assert.AreEqual (5, row [dcId], "#3");
			Assert.AreEqual ("SysData", row [dcName], "#4");
			Assert.AreEqual ("what", row [dcPassword], "#5");
			Assert.AreEqual (DataRowState.Unchanged, row.RowState, "#6");
		}

		[Test]
		public void LoadDataRow_DuplicateValues ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.Columns.Add ("col2", typeof (int));

			table.PrimaryKey = new DataColumn[] {table.Columns [0]};

			table.BeginLoadData ();
			table.LoadDataRow (new object[] {1, 1}, false);
			table.LoadDataRow (new object[] {1, 10}, false);

			try {
				table.EndLoadData ();
				Assert.Fail ("#1");
			} catch (ConstraintException ex) {
				 // Failed to enable constraints. One or more rows
				// contain values violating non-null, unique, or
				// foreign-key constraints
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void LoadDataRow_WithoutBeginLoadData ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.Columns.Add ("col2", typeof (int));

			table.PrimaryKey = new DataColumn[] {table.Columns [0]};
			table.Rows.Add (new object[] {1,1});
			table.AcceptChanges ();

			table.LoadDataRow (new object[] {10,1}, false);
			DataRow row = table.Rows.Find (10);
			Assert.IsNotNull (row, "#1");
			Assert.AreEqual (1, row [1], "#2");
			Assert.AreEqual (DataRowState.Added, row.RowState, "#3");
			table.AcceptChanges ();

			table.LoadDataRow (new object[] {10,2}, true);
			row = table.Rows.Find (10);
			Assert.IsNotNull (row, "#4");
			Assert.AreEqual (2, row [1], "#5");
			Assert.AreEqual (DataRowState.Unchanged, row.RowState, "#6");

			table.LoadDataRow (new object[] {1,2}, false);
			row = table.Rows.Find (1);
			Assert.IsNotNull (row, "#7");
			Assert.AreEqual (2, row [1], "#8");
			Assert.AreEqual (DataRowState.Modified, table.Rows.Find (1).RowState, "#9");

			table.LoadDataRow (new object[] {1,3}, true);
			row = table.Rows.Find (1);
			Assert.IsNotNull (row, "#10");
			Assert.AreEqual (3, row [1], "#11");
			Assert.AreEqual (DataRowState.Unchanged, table.Rows.Find (1).RowState, "#12");
		}

		[Test]
		public void EndLoadData_MergeDuplcateValues ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.Columns.Add ("col2", typeof (int));

			table.PrimaryKey = new DataColumn[] {table.Columns [0]};
			table.Rows.Add (new object[] {1, 500});
			table.AcceptChanges ();

			table.BeginLoadData ();
			table.LoadDataRow (new object[] {1, 1}, false);
			table.LoadDataRow (new object[] {1, 10}, false);
			table.LoadDataRow (new object[] {1, 100}, false);
			table.EndLoadData ();

			Assert.AreEqual (1, table.Rows.Count, "#1");
			Assert.AreEqual (100, table.Rows [0][1], "#2");
		}

		[Test]
		public void GetChanges ()
		{
			DataTable dt1,dt2 = DataProvider.CreateParentDataTable();
			dt2.Constraints.Add("Unique",dt2.Columns[0],true);
			dt2.Columns[0].DefaultValue=7;

			//make some changes
			dt2.Rows[0].Delete();
			dt2.Rows[1].Delete();
			dt2.Rows[2].Delete();
			dt2.Rows[3].Delete();

			dt1 = dt2.GetChanges();

			for (int i = 0; i < dt2.Constraints.Count; i++) {
				// GetChanges - Constraints[{0}],i)
				Assert.AreEqual (dt2.Constraints[i].ConstraintName,
					dt1.Constraints[i].ConstraintName, "DT43");
			}

			for (int i = 0; i < dt2.Columns.Count; i++) {
				// GetChanges - Columns[{0}].ColumnName,i)
				Assert.AreEqual (dt2.Columns [i].ColumnName, dt1.Columns [i].ColumnName, "DT44");

				// GetChanges - Columns[{0}].DataType,i)
				Assert.AreEqual (dt2.Columns [i].DataType, dt1.Columns [i].DataType, "DT45");
			}

			DataRow [] drArr1,drArr2;

			drArr1 = dt1.Select (string.Empty, string.Empty,DataViewRowState.Deleted);
			drArr2 = dt2.Select (string.Empty, string.Empty,DataViewRowState.Deleted);

			for (int i = 0; i < drArr1.Length; i++) {
				// GetChanges - Data [ParentId]{0} ,i)
				Assert.AreEqual (drArr1 [i] ["ParentId",DataRowVersion.Original ],drArr2[i]["ParentId",DataRowVersion.Original], "DT46");
				// GetChanges - Data [String1]{0} ,i)
				Assert.AreEqual (drArr1 [i] ["String1", DataRowVersion.Original],drArr2[i]["String1",DataRowVersion.Original], "DT47");
				// GetChanges - Data [String2]{0} ,i)
				Assert.AreEqual (drArr1 [i] ["String2", DataRowVersion.Original],drArr2[i]["String2",DataRowVersion.Original], "DT48");
			}
		}

		[Test]
		public void GetChanges_ByDataRowState ()
		{
			DataTable dt1,dt2 = DataProvider.CreateParentDataTable();
			dt2.Constraints.Add("Unique",dt2.Columns[0],true);
			dt2.Columns[0].DefaultValue=7;

			//make some changes
			dt2.Rows[0].Delete(); //DataRowState.Deleted
			dt2.Rows[1].Delete(); //DataRowState.Deleted
			dt2.Rows[2].BeginEdit();
			dt2.Rows[2]["String1"] = "Changed"; //DataRowState.Modified
			dt2.Rows[2].EndEdit();

			dt2.Rows.Add(new object[] {"99", "Temp1", "Temp2"}); //DataRowState.Added

			// *********** Checking GetChanges - DataRowState.Deleted ************
			dt1 = null;
			dt1 = dt2.GetChanges(DataRowState.Deleted);
			CheckTableSchema (dt1,dt2,DataRowState.Deleted.ToString());
			DataRow[] drArr1,drArr2;
			drArr1 = dt1.Select (string.Empty, string.Empty, DataViewRowState.Deleted);
			drArr2 = dt2.Select (string.Empty, string.Empty, DataViewRowState.Deleted);

			for (int i = 0; i < drArr1.Length; i++) {
				// GetChanges(Deleted) - Data [ParentId]{0} ,i)
				Assert.AreEqual (drArr1 [i] ["ParentId", DataRowVersion.Original], drArr2 [i] ["ParentId", DataRowVersion.Original], "DT49");
				// GetChanges(Deleted) - Data [String1]{0} ,i)
				Assert.AreEqual (drArr1 [i] ["String1", DataRowVersion.Original], drArr2 [i] ["String1", DataRowVersion.Original], "DT50");
				// GetChanges(Deleted) - Data [String2]{0} ,i)
				Assert.AreEqual (drArr1 [i] ["String2", DataRowVersion.Original],drArr2 [i] ["String2", DataRowVersion.Original], "DT51");
			}

			// *********** Checking GetChanges - DataRowState.Modified ************
			dt1 = null;
			dt1 = dt2.GetChanges(DataRowState.Modified);
			CheckTableSchema (dt1,dt2,DataRowState.Modified.ToString());
			drArr1 = dt1.Select (string.Empty, string.Empty);
			drArr2 = dt2.Select (string.Empty, string.Empty, DataViewRowState.ModifiedCurrent);

			for (int i = 0; i < drArr1.Length; i++) {
				// GetChanges(Modified) - Data [ParentId]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["ParentId"], drArr1 [i] ["ParentId"], "DT52");
				// GetChanges(Modified) - Data [String1]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["String1"], drArr1 [i] ["String1"], "DT53");
				// GetChanges(Modified) - Data [String2]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["String2"], drArr1 [i] ["String2"], "DT54");
			}

			// *********** Checking GetChanges - DataRowState.Added ************
			dt1 = null;
			dt1 = dt2.GetChanges(DataRowState.Added);
			CheckTableSchema (dt1,dt2,DataRowState.Added.ToString());
			drArr1 = dt1.Select (string.Empty, string.Empty);
			drArr2 = dt2.Select (string.Empty, string.Empty, DataViewRowState.Added);

			for (int i = 0; i < drArr1.Length; i++) {
				// GetChanges(Added) - Data [ParentId]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["ParentId"], drArr1 [i] ["ParentId"], "DT55");
				// GetChanges(Added) - Data [String1]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["String1"], drArr1 [i] ["String1"], "DT56");
				// GetChanges(Added) - Data [String2]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["String2"], drArr1 [i] ["String2" ], "DT57");
			}

			// *********** Checking GetChanges - DataRowState.Unchanged  ************
			dt1 = null;
			dt1 = dt2.GetChanges(DataRowState.Unchanged);
			CheckTableSchema (dt1,dt2,DataRowState.Unchanged .ToString());
			drArr1 = dt1.Select (string.Empty, string.Empty);
			drArr2 = dt2.Select (string.Empty, string.Empty, DataViewRowState.Unchanged);

			for (int i = 0; i < drArr1.Length; i++) {
				// GetChanges(Unchanged) - Data [ParentId]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["ParentId"], drArr1 [i] ["ParentId"], "DT58");
				// GetChanges(Unchanged) - Data [String1]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["String1"], drArr1 [i] ["String1"], "DT59");
				// GetChanges(Unchanged) - Data [String2]{0} ,i)
				Assert.AreEqual (drArr2 [i] ["String2"], drArr1 [i] ["String2" ], "DT60");
			}
		}

		private void CheckTableSchema (DataTable dt1, DataTable dt2, string Description)
		{
			for (int i = 0; i < dt2.Constraints.Count; i++) {
				// GetChanges - Constraints[{0}] - {1},i,Description)
				Assert.AreEqual (dt2.Constraints [i].ConstraintName,
					dt1.Constraints [i].ConstraintName, "DT61");
			}

			for (int i = 0; i < dt2.Columns.Count; i++) {
				// GetChanges - Columns[{0}].ColumnName - {1},i,Description)
				Assert.AreEqual (dt2.Columns [i].ColumnName, dt1.Columns [i].ColumnName, "DT62");

				// GetChanges - Columns[{0}].DataType {1},i,Description)
				Assert.AreEqual (dt2.Columns [i].DataType, dt1.Columns [i].DataType, "DT63");
			}
		}

		[Test]
		public void GetErrors()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow[] drArr = new DataRow[3];
			drArr[0] = dt.Rows[0];
			drArr[1] = dt.Rows[2];
			drArr[2] = dt.Rows[5];

			drArr[0].RowError = "Error1";
			drArr[1].RowError = "Error2";
			drArr[2].RowError = "Error3";

			// GetErrors
			Assert.AreEqual(dt.GetErrors(), drArr, "DT64");
		}

		[Test]
		public new void GetHashCode ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			int iHashCode;
			iHashCode = dt.GetHashCode();

			for (int i = 0; i < 10; i++) {
				// HashCode - i= + i.ToString()
				Assert.AreEqual(dt.GetHashCode (), iHashCode, "DT65");
			}
		}

		[Test]
		public new void GetType ()
		{
			DataTable dt = DataProvider.CreateParentDataTable ();
			Type tmpType = typeof (DataTable);

			// GetType
			Assert.AreEqual (tmpType, dt.GetType(), "DT66");
		}

		[Test]
		public void HasErrors ()
		{
			DataTable dtParent;
			dtParent= DataProvider.CreateParentDataTable();

			// Checking HasErrors default 
			Assert.AreEqual(false, dtParent.HasErrors, "DT67");

			// Checking HasErrors Get 
			dtParent.Rows[0].RowError = "Error on row 0";
			dtParent.Rows[2].RowError = "Error on row 2";
			Assert.AreEqual(true, dtParent.HasErrors, "DT68");
		}

		[Test]
		public void ImportRow ()
		{
			DataTable dt1, dt2;
			dt1 = DataProvider.CreateParentDataTable ();
			dt2 = DataProvider.CreateParentDataTable ();
			DataRow dr = dt2.NewRow ();
			dr.ItemArray = new object [] { 99, string.Empty, string.Empty };
			dt2.Rows.Add (dr);

			// ImportRow - Values
			dt1.ImportRow (dr);
			Assert.AreEqual (dr.ItemArray, dt1.Rows [dt1.Rows.Count - 1].ItemArray, "DT69");

			// ImportRow - DataRowState
			Assert.AreEqual (dr.RowState, dt1.Rows [dt1.Rows.Count - 1].RowState, "DT70");
		}

		[Test]
		public void LoadDataRow ()
		{
			DataTable dt;
			DataRow dr;
			dt = DataProvider.CreateParentDataTable();
			dt.PrimaryKey= new DataColumn[] {dt.Columns[0]}; //add ParentId as Primary Key
			dt.Columns["String1"].DefaultValue = "Default";

			dr = dt.Select("ParentId=1")[0];

			//Update existing row without accept changes
			dt.BeginLoadData ();
			dt.LoadDataRow (new object [] { 1, null, "Changed" }, false);
			dt.EndLoadData ();

			// LoadDataRow(update1) - check column String1
			Assert.AreEqual (dr ["String1"], dt.Columns ["String1"].DefaultValue, "DT71");

			// LoadDataRow(update1) - check column String2
			Assert.AreEqual (dr ["String2"], "Changed", "DT72");

			// LoadDataRow(update1) - check row state
			Assert.AreEqual (DataRowState.Modified, dr.RowState, "DT73");

			//Update existing row with accept changes
			dr = dt.Select ("ParentId=2") [0];

			dt.BeginLoadData ();
			dt.LoadDataRow (new object [] { 2, null, "Changed"}, true);
			dt.EndLoadData ();

			// LoadDataRow(update2) - check row state
			Assert.AreEqual (DataRowState.Unchanged, dr.RowState, "DT74");

			//Add New row without accept changes
			dt.BeginLoadData();
			dt.LoadDataRow(new object [] { 99, null, "Changed" }, false);
			dt.EndLoadData();

			// LoadDataRow(insert1) - check column String2
			dr = dt.Select("ParentId=99")[0];
			Assert.AreEqual("Changed", dr ["String2"], "DT75");

			// LoadDataRow(insert1) - check row state
			Assert.AreEqual(DataRowState.Added, dr.RowState, "DT76");

			//Add New row with accept changes
			dt.BeginLoadData ();
			dt.LoadDataRow (new object [] { 100, null, "Changed" }, true);
			dt.EndLoadData ();

			// LoadDataRow(insert2) - check row and values
			dr = dt.Select ("ParentId=100") [0];
			Assert.AreEqual("Changed", dr ["String2"], "DT77");

			// LoadDataRow(insert2) - check row state
			Assert.AreEqual (DataRowState.Unchanged, dr.RowState, "DT78");
		}

		[Test]
		public void Locale ()
		{
			DataTable dtParent;
			DataSet ds = new DataSet ("MyDataSet");

			dtParent= DataProvider.CreateParentDataTable ();
			ds.Tables.Add (dtParent);
			CultureInfo culInfo = CultureInfo.CurrentCulture;

			// Checking Locale default from system
			Assert.AreEqual (culInfo, dtParent.Locale, "DT79");

			// Checking Locale default from dataset
			culInfo = new CultureInfo ("fr");
			ds.Locale = culInfo;
			Assert.AreEqual (culInfo, dtParent.Locale, "DT80");

			// Checking Locale get/set
			culInfo = new CultureInfo ("nl-BE");
			dtParent.Locale = culInfo;
			Assert.AreEqual(culInfo, dtParent.Locale, "DT81");
		}

		[Test]
		public void MinimumCapacity ()
		{
			//				i get default=50, according to MSDN the value should be 25 
			//				// Checking MinimumCapacity default = 25 
			//				Assert.AreEqual(25, dtParent.MinimumCapacity, "DT82");
			//				EndCase(null);
			DataTable dt = new DataTable ();

			// Checking MinimumCapacity get/set int.MaxValue 
			dt.MinimumCapacity = int.MaxValue;
			Assert.AreEqual (int.MaxValue, dt.MinimumCapacity, "DT83");

			// Checking MinimumCapacity get/set 0
			dt.MinimumCapacity = 0;
			Assert.AreEqual (0, dt.MinimumCapacity, "DT84");

			//				// Checking MinimumCapacity get/set int.MinValue 
			//				dtParent.MinimumCapacity = int.MinValue;
			//				Assert.AreEqual(int.MinValue, dtParent.MinimumCapacity, "DT85");
			//				EndCase(null);
		}

		[Test]
		public void Namespace ()
		{
			DataTable dtParent = new DataTable ();

			// Checking Namespace default
			Assert.AreEqual (String.Empty, dtParent.Namespace, "DT86");

			// Checking Namespace set/get
			String s = "MyNamespace";
			dtParent.Namespace = s;
			Assert.AreEqual (s, dtParent.Namespace, "DT87");
		}

		[Test]
		public void NewRow ()
		{
			DataTable dt;
			DataRow dr;
			dt = DataProvider.CreateParentDataTable();

			// NewRow
			dr = dt.NewRow();
			Assert.IsNotNull (dr, "DT88");
		}

		[Test]
		public void OnColumnChanged ()
		{
			ProtectedTestClass dt = new ProtectedTestClass();

			EventRaised = false;
			dt.OnColumnChanged_Test ();
			// OnColumnChanged Event 1
			Assert.AreEqual (false, EventRaised, "DT89");
			EventRaised = false;
			EventValues = false;
			dt.ColumnChanged += new DataColumnChangeEventHandler (OnColumnChanged_Handler);
			dt.OnColumnChanged_Test();
			// OnColumnChanged Event 2
			Assert.AreEqual (true, EventRaised, "DT90");
			// OnColumnChanged Values
			Assert.AreEqual (true, EventValues, "DT91");
			dt.ColumnChanged -= new DataColumnChangeEventHandler (OnColumnChanged_Handler);
		}

		private void OnColumnChanged_Handler(Object sender,DataColumnChangeEventArgs e)
		{
			DataTable dt = (DataTable)sender;
			if ((e.Column.Equals(dt.Columns["Value"])) && (e.Row.Equals(dt.Rows[0])) && (e.ProposedValue.Equals("NewValue"))) {
				EventValues = true;
			} else {
				EventValues = false;
			}
			EventRaised = true;
		}

		[Test]
		public void OnColumnChanging ()
		{
			ProtectedTestClass dt = new ProtectedTestClass ();

			EventRaised = false;
			dt.OnColumnChanging_Test ();
			// OnColumnChanging Event 1
			Assert.AreEqual (false, EventRaised, "DT92");
			EventRaised = false;
			EventValues = false;
			dt.ColumnChanging  += new DataColumnChangeEventHandler(OnColumnChanging_Handler);
			dt.OnColumnChanging_Test();
			// OnColumnChanging Event 2
			Assert.AreEqual (true, EventRaised, "DT93");
			// OnColumnChanging Values
			Assert.AreEqual (true, EventValues, "DT94");
			dt.ColumnChanging -= new DataColumnChangeEventHandler (OnColumnChanging_Handler);
		}

		private void OnColumnChanging_Handler (Object sender, DataColumnChangeEventArgs e)
		{
			DataTable dt = (DataTable)sender;
			if ((e.Column.Equals(dt.Columns["Value"])) && (e.Row.Equals(dt.Rows[0])) && (e.ProposedValue.Equals("NewValue"))) {
				EventValues = true;
			} else {
				EventValues = false;
			}
			EventRaised = true;
		}

		[Test]
		public void OnRemoveColumn ()
		{
			ProtectedTestClass dt = new ProtectedTestClass();
			dt.OnRemoveColumn_Test();
		}

		[Test]
		public void ParentRelations ()
		{
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable();
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			DataRelationCollection drlCollection;
			DataRelation drl = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);

			// Checking ParentRelations - default value
			//Check default
			drlCollection = dtChild.ParentRelations;
			Assert.AreEqual(0, drlCollection.Count, "DT96");

			ds.Relations.Add(drl);
			drlCollection = dtChild.ParentRelations;

			// Checking ParentRelations Count
			Assert.AreEqual(1, drlCollection.Count, "DT97");

			// Checking ParentRelations Value
			Assert.AreEqual(drl, drlCollection[0], "DT98");
		}

		[Test]
		public void Prefix ()
		{
			DataTable dtParent = new DataTable();

			// Checking Prefix default
			Assert.AreEqual(String.Empty, dtParent.Prefix, "DT99");

			// Checking Prefix set/get
			String s = "MyPrefix";
			dtParent.Prefix=s;
			Assert.AreEqual(s, dtParent.Prefix, "DT100");
		}

		[Test]
		public void RejectChanges ()
		{
			String sNewValue = "NewValue";
			DataRow drModified, drDeleted, drAdded;
			DataTable dt = DataProvider.CreateParentDataTable ();

			drModified = dt.Rows [0];
			drModified [1] = sNewValue; //DataRowState = Modified, DataRowVersion = Proposed

			drDeleted = dt.Rows [1];
			drDeleted.Delete (); //DataRowState =  Deleted

			drAdded = dt.NewRow ();
			dt.Rows.Add (drAdded); //DataRowState =  Added

			dt.RejectChanges ();

			// RejectChanges - Unchanged1
			Assert.AreEqual (DataRowState.Unchanged, drModified.RowState, "DT101");

			// RejectChanges - Unchanged2
			Assert.AreEqual (DataRowState.Detached, drAdded.RowState, "DT102");

			// RejectChanges - Detached
			Assert.AreEqual (DataRowState.Unchanged, drDeleted.RowState, "DT103");
		}

		[Test]
		public void Reset ()
		{
			DataTable dt1 = DataProvider.CreateParentDataTable ();
			DataTable dt2 = DataProvider.CreateChildDataTable ();
			dt1.PrimaryKey = new DataColumn [] { dt1.Columns [0] };
			dt2.PrimaryKey = new DataColumn [] {dt2.Columns [0], dt2.Columns [1]};
			DataRelation rel = new DataRelation ("Rel", dt1.Columns ["ParentId"], dt2.Columns ["ParentId"]);
			DataSet ds = new DataSet ();
			ds.Tables.AddRange (new DataTable [] { dt1, dt2 });
			ds.Relations.Add (rel);

			dt2.Reset ();

			// Reset - ParentRelations
			Assert.AreEqual (0, dt2.ParentRelations.Count, "DT104");
			// Reset - Constraints
			Assert.AreEqual (0, dt2.Constraints.Count, "DT105");
			// Reset - Rows
			Assert.AreEqual (0, dt2.Rows.Count, "DT106");
			// Reset - Columns
			Assert.AreEqual (0, dt2.Columns.Count, "DT107");
		}

		[Test]
		public void RowChanged ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.RowChanged += new DataRowChangeEventHandler (Row_Changed);

			_EventTriggered = false;
			// RowChanged - 1
			dt.Rows[0][1] = "NewValue";
			Assert.AreEqual (true, _EventTriggered, "DT108");

			_EventTriggered = false;
			// RowChanged - 2
			dt.Rows[0].BeginEdit ();
			dt.Rows[0][1] = "NewValue";
			Assert.AreEqual (false, _EventTriggered, "DT109");

			_EventTriggered = false;
			// RowChanged - 3
			dt.Rows[0].EndEdit ();
			Assert.AreEqual (true, _EventTriggered, "DT110");

			_EventTriggered = false;
			dt.RowChanged -= new DataRowChangeEventHandler (Row_Changed);
			// RowChanged - 4
			dt.Rows[0][1] = "NewValue A";
			Assert.AreEqual (false, _EventTriggered, "DT111");
		}

		private void Row_Changed (object sender, DataRowChangeEventArgs e)
		{
			_EventTriggered = true;
		}

		[Test]
		public void RowChanging ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.RowChanging  += new DataRowChangeEventHandler (Row_Changing);

			_EventTriggered = false;
			// RowChanging - 1
			dt.Rows[0][1] = "NewValue";
			Assert.AreEqual (true, _EventTriggered, "DT112");

			_EventTriggered = false;
			// RowChanging - 2
			dt.Rows[0].BeginEdit ();
			dt.Rows[0][1] = "NewValue";
			Assert.AreEqual(false, _EventTriggered, "DT113");

			_EventTriggered = false;
			// RowChanging - 3
			dt.Rows[0].EndEdit ();
			Assert.AreEqual (true, _EventTriggered, "DT114");

			_EventTriggered = false;
			dt.RowChanging  -= new DataRowChangeEventHandler (Row_Changing);
			// RowChanging - 4
			dt.Rows[0][1] = "NewValue A";
			Assert.AreEqual (false, _EventTriggered, "DT115");
		}

		private void Row_Changing (object sender, DataRowChangeEventArgs e)
		{
			_EventTriggered = true;
		}

		[Test]
		public void RowDeleted ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.RowDeleted += new DataRowChangeEventHandler (Row_Deleted);

			_EventTriggered = false;
			// RowDeleted - 1
			dt.Rows[0].Delete();
			Assert.IsTrue (_EventTriggered, "DT116");

			_EventTriggered = false;
			dt.RowDeleted -= new DataRowChangeEventHandler (Row_Deleted);
			// RowDeleted - 2
			dt.Rows[1].Delete ();
			Assert.IsFalse (_EventTriggered, "DT117");
		}

		private void Row_Deleted (object sender, DataRowChangeEventArgs e)
		{
			_EventTriggered = true;
		}

		[Test]
		public void RowDeleting ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.RowDeleting += new DataRowChangeEventHandler (Row_Deleting);

			_EventTriggered = false;
			// RowDeleting - 1
			dt.Rows [0].Delete ();
			Assert.IsTrue (_EventTriggered, "DT118");

			_EventTriggered = false;
			dt.RowDeleting  -= new DataRowChangeEventHandler (Row_Deleting);
			// RowDeleting - 2
			dt.Rows [1].Delete ();
			Assert.IsFalse (_EventTriggered, "DT119");
		}

		private void Row_Deleting (object sender, DataRowChangeEventArgs e)
		{
			_EventTriggered = true;
		}

		[Test]
		public void Rows ()
		{
			DataTable dtParent;
			dtParent = DataProvider.CreateParentDataTable();

			// Checking Rows
			Assert.IsNotNull (dtParent.Rows, "DT120");

			// Checking rows count
			Assert.IsTrue (dtParent.Rows.Count > 0, "DT121");
		}

		[Test]
		public void Select ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			DataRow[] drSelect = dt.Select ();
			DataRow[] drResult = new DataRow [dt.Rows.Count];
			dt.Rows.CopyTo (drResult, 0);

			// Select
			Assert.AreEqual(drResult, drSelect, "DT122");
		}

		[Test]
		public void Select_ByFilter ()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(DataProvider.CreateParentDataTable ());

			DataTable dt = DataProvider.CreateChildDataTable ();
			ds.Tables.Add (dt);
			DataRow[] drSelect = null;
			ArrayList al = new ArrayList ();

			//add column with special name
			DataColumn dc = new DataColumn("Column#",typeof(int));
			dc.DefaultValue=-1;
			dt.Columns.Add(dc);
			//put some values
			dt.Rows[0][dc] = 100;
			dt.Rows[1][dc] = 200;
			dt.Rows[2][dc] = 300;
			dt.Rows[4][dc] = -400;

			//for trim function
			dt.Rows[0]["String1"] = dt.Rows[0]["String1"] + "   \t\n ";
			dt.Rows[0]["String1"] = "   \t\n " + dt.Rows[0]["String1"];
			dt.Rows[0]["String1"] = dt.Rows[0]["String1"] + "    ";

			ds.Tables[0].Rows[0]["ParentBool"] = DBNull.Value;
			ds.Tables[0].Rows[2]["ParentBool"] = DBNull.Value;
			ds.Tables[0].Rows[3]["ParentBool"] = DBNull.Value;

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows) {
				if ((int)dr["ChildId"] == 1)
					al.Add(dr);
			}
			// Select_S - ChildId=1
			drSelect = dt.Select ("ChildId=1");
			Assert.AreEqual (al.ToArray(), drSelect, "DT123");

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows) {
				if ((int)dr["ChildId"] == 1)
					al.Add(dr);
			}
			// Select_S - ChildId='1'
			drSelect = dt.Select ("ChildId='1'");
			Assert.AreEqual (al.ToArray(), drSelect, "DT124");
			//-------------------------------------------------------------
			// Select_S - ChildId= '1'  (whitespace in filter string.
			drSelect = dt.Select("ChildId= '1'");
			Assert.AreEqual (al.ToArray(), drSelect, "DT125");
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if (dr["String1"].ToString() == "1-String1")
					al.Add(dr);
			// Select_S - String1='1-String1'
			drSelect = dt.Select ("String1='1-String1'");
			Assert.AreEqual (al.ToArray(), drSelect, "DT126");

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if ((int)dr["ChildId"] == 1 && dr["String1"].ToString() == "1-String1")
					al.Add(dr);
			// Select_S - ChildId=1 and String1='1-String1'
			drSelect = dt.Select ("ChildId=1 and String1='1-String1'");
			Assert.AreEqual (al.ToArray(), drSelect, "DT127");

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if ((int)dr["ChildId"] + (int)dr["ParentId"] >= 4)
					al.Add(dr);
			// Select_S - ChildId+ParentId >= 4
			drSelect = dt.Select ("ChildId+ParentId >= 4");
			CompareUnSorted(drSelect, al.ToArray());

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows) {
				if ((((int)dr["ChildId"] - (int)dr["ParentId"]) * -1) != 0)
					al.Add(dr);
			}
			// Select_S - ChildId-ParentId) * -1  <> 0
			drSelect = dt.Select ("(ChildId-ParentId) * -1  <> 0");
			CompareUnSorted(drSelect, al.ToArray());

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if ((double)dr["ChildDouble"] < ((int)dr["ParentId"]) % 4)
					al.Add(dr);
			// Select_S - ChildDouble < ParentId % 4
			drSelect = dt.Select ("ChildDouble < ParentId % 4");
			CompareUnSorted(drSelect, al.ToArray());

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if ((double)dr["ChildDouble"] == 10 || (double)dr["ChildDouble"] == 20 || (double)dr["ChildDouble"] == 25)
					al.Add (dr);
			// Select_S - ChildDouble in (10,20,25)
			drSelect = dt.Select("ChildDouble in (10,20,25)");
			CompareUnSorted(drSelect, al.ToArray());

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if (dr["String2"].ToString().IndexOf("1-S") >= 0)
					al.Add (dr);
			// Select_S - String2 like '%1-S%'
			drSelect = dt.Select ("String2 like '%1-S%'");
			Assert.AreEqual (al.ToArray (), drSelect, "DT128");

			//-------------------------------------------------------------
			//If a column name contains one of the above characters,(ex. #\/=><+-*%&|^'" and so on) the name must be wrapped in brackets. For example to use a column named "Column#" in an expression, you would write "[Column#]":
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if ((int)dr["Column#"] <= 0)
					al.Add (dr);
			// Select_S - [Column#] <= 0 
			drSelect = dt.Select ("[Column#] <= 0 ");
			CompareUnSorted (drSelect, al.ToArray());
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if ((int)dr["Column#"] <= 0)
					al.Add (dr);
			// Select_S - [Column#] <= 0
			drSelect = dt.Select ("[Column#] <= 0");
			CompareUnSorted (drSelect, al.ToArray());

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if (((DateTime)dr["ChildDateTime"]).CompareTo(new DateTime(2000,12,12)) > 0)
					al.Add(dr);
			// Select_S - ChildDateTime > #12/12/2000# 
			drSelect = dt.Select("ChildDateTime > #12/12/2000# ");
			CompareUnSorted (drSelect, al.ToArray ());

			//-------------------------------------------------------------

			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if (((DateTime)dr["ChildDateTime"]).CompareTo(new DateTime(1999,1,12,12,06,30)) > 0)
					al.Add(dr);
			// Select_S - ChildDateTime > #1/12/1999 12:06:30 PM#  
			drSelect = dt.Select ("ChildDateTime > #1/12/1999 12:06:30 PM#  ");
			CompareUnSorted (drSelect, al.ToArray ());

			//-------------------------------------------------------------

			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if (((DateTime) dr ["ChildDateTime"]).CompareTo (new DateTime (2005, 12, 03, 17, 06, 30)) >= 0  || ((DateTime) dr ["ChildDateTime"]).CompareTo (new DateTime (1980, 11, 03)) <= 0)
					al.Add (dr);
			// Select_S - ChildDateTime >= #12/3/2005 5:06:30 PM# or ChildDateTime <= #11/3/1980#  
			drSelect = dt.Select ("ChildDateTime >= #12/3/2005 5:06:30 PM# or ChildDateTime <= #11/3/1980#  ");
			CompareUnSorted (drSelect, al.ToArray());

#if LATER
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if (dr["ChildDouble"].ToString().Length > 10)
					al.Add(dr);
				// Select_S - Len(Convert(ChildDouble,'System.String')) > 10
				drSelect = dt.Select ("Len(Convert(ChildDouble,'System.String')) > 10");
				Assert.AreEqual (al.ToArray(), drSelect, "DT129");
#endif
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows)
				if (dr["String1"].ToString().Trim().Substring(0,2) == "1-")
					al.Add(dr);
			// Select_S - SubString(Trim(String1),1,2) = '1-'
			drSelect = dt.Select ("SubString(Trim(String1),1,2) = '1-'");
			Assert.AreEqual (al.ToArray(), drSelect, "DT130");
			//-------------------------------------------------------------
			/*
			al.Clear();
			foreach (DataRow dr in ds.Tables[0].Rows)
				if (dr.IsNull("ParentBool") || (bool)dr["ParentBool"])
					al.Add(dr);
				// Select_S - IsNull(ParentBool,true)
				drSelect = ds.Tables[0].Select("IsNull(ParentBool,true) ");
				Assert.AreEqual (al.ToArray(), drSelect, "DT131");
			*/
			//-------------------------------------------------------------
			al.Clear();
			// Select_S - Relation not exists, Exception
			try {
				drSelect = dt.Select ("Parent.ParentId = ChildId");
				Assert.Fail ("#A1");
			} catch (IndexOutOfRangeException ex) {
				// Cannot find relation 0
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}
			//-------------------------------------------------------------
			al.Clear();
			ds.Relations.Add(new DataRelation("ParentChild",ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]));
			foreach (DataRow dr in dt.Rows)
				if ((int)dr["ChildId"] == (int)dr.GetParentRow("ParentChild")["ParentId"])
					al.Add(dr);
			// Select_S - Parent.ParentId = ChildId
			drSelect = dt.Select ("Parent.ParentId = ChildId");
			Assert.AreEqual (al.ToArray(), drSelect, "DT134");
		}

		private void CompareUnSorted(Array a, Array b)
		{
			string msg = string.Format("Failed while comparing(Array a ={0} ({1}), Array b = {2} ({3}))]", a.ToString(), a.GetType().FullName, b.ToString(), b.GetType().FullName);
			foreach (object item in a) {
				if (Array.IndexOf(b, item) < 0) //b does not contain the current item.
					Assert.Fail(msg);
			}

			foreach (object item in b) {
				if (Array.IndexOf(a, item) < 0) //a does not contain the current item.
					Assert.Fail(msg);
			}
		}

		[Test]
		public void Select_ByFilterDataViewRowState ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow[] drSelect, drResult;

			dt.Rows[0].Delete();
			dt.Rows[1]["ParentId"] = 1;
			dt.Rows[2]["ParentId"] = 1;
			dt.Rows[3].Delete();
			dt.Rows.Add(new object[] {1, "A", "B"});
			dt.Rows.Add(new object[] {1, "C", "D"});
			dt.Rows.Add(new object[] {1, "E", "F"});

			drSelect = dt.Select ("ParentId=1", string.Empty, DataViewRowState.Added);
			drResult = GetResultRows(dt,DataRowState.Added);
			// Select_SSD DataViewRowState.Added
			Assert.AreEqual (drResult, drSelect, "DT135");

			drSelect = dt.Select ("ParentId=1", string.Empty, DataViewRowState.CurrentRows);
			drResult = GetResultRows (dt, DataRowState.Unchanged | DataRowState.Added  | DataRowState.Modified);
			// Select_SSD DataViewRowState.CurrentRows
			Assert.AreEqual (drResult, drSelect, "DT136");

			drSelect = dt.Select ("ParentId=1", string.Empty, DataViewRowState.Deleted);
			drResult = GetResultRows (dt, DataRowState.Deleted);
			// Select_SSD DataViewRowState.Deleted
			Assert.AreEqual (drResult, drSelect, "DT137");

			drSelect = dt.Select ("ParentId=1", string.Empty, DataViewRowState.ModifiedCurrent | DataViewRowState.ModifiedOriginal);
			drResult = GetResultRows (dt,DataRowState.Modified);
			// Select_SSD ModifiedCurrent or ModifiedOriginal
			Assert.AreEqual (drResult, drSelect, "DT138");
		}

		private DataRow [] GetResultRows (DataTable dt, DataRowState State)
		{
			ArrayList al = new ArrayList ();
			DataRowVersion drVer = DataRowVersion.Current;

			//From MSDN -	The row the default version for the current DataRowState.
			//				For a DataRowState value of Added, Modified or Current, 
			//				the default version is Current. 
			//				For a DataRowState of Deleted, the version is Original.
			//				For a DataRowState value of Detached, the version is Proposed.

			if (	((State & DataRowState.Added)		> 0)
				| ((State & DataRowState.Modified)	> 0)
				| ((State & DataRowState.Unchanged)	> 0))
				drVer = DataRowVersion.Current;
			if ( (State & DataRowState.Deleted)		> 0
				| (State & DataRowState.Detached)	> 0)
				drVer = DataRowVersion.Original;

			foreach (DataRow dr in dt.Rows) {
				if (dr.HasVersion(drVer) && ((int)dr["ParentId", drVer] == 1) && ((dr.RowState & State) > 0))
					al.Add(dr);
			}
			DataRow[] result = (DataRow[])al.ToArray((typeof(DataRow)));
			return result;
		}

		[Test]
		public void TableName ()
		{
			DataTable dtParent = new DataTable();

			// Checking TableName default
			Assert.AreEqual(String.Empty, dtParent.TableName, "DT139");

			// Checking TableName set/get
			String s = "MyTable";
			dtParent.TableName=s;
			Assert.AreEqual(s, dtParent.TableName, "DT140");
		}

		[Test]
		public new void ToString ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.DisplayExpression = dt.Columns[0].ColumnName;

			string sToString = dt.TableName + " + " + dt.DisplayExpression;
			Assert.AreEqual (sToString, dt.ToString (), "DT141");
		}

		[Test]
		public void CaseSensitive ()
		{
			DataTable dtParent = new DataTable();

			// Checking default
			Assert.IsFalse (dtParent.CaseSensitive, "DT142");

			// Checking set/get
			dtParent.CaseSensitive = true;
			Assert.IsTrue (dtParent.CaseSensitive, "DT143");
		}

		[Test]
		public void ctor ()
		{
			DataTable dt = new DataTable ();
			Assert.IsNotNull(dt, "DT144");
		}

		[Test]
		public void ctor_ByName ()
		{
			string sName = "MyName";

			DataTable dt = new DataTable (sName);

			Assert.IsNotNull (dt, "DT145");
			Assert.AreEqual(sName, dt.TableName, "DT146");
		}

		[Test]
		public void DisplayExpression ()
		{
			DataTable dtParent;
			dtParent= DataProvider.CreateParentDataTable ();

			// Checking DisplayExpression default 
			Assert.AreEqual (String.Empty, dtParent.DisplayExpression, "DT147");

			// Checking DisplayExpression Set/Get 
			dtParent.DisplayExpression = dtParent.Columns[0].ColumnName;
			Assert.AreEqual (dtParent.Columns[0].ColumnName, dtParent.DisplayExpression, "DT148");
		}

		[Test]
		public void ExtendedProperties ()
		{
			DataTable dtParent;
			PropertyCollection pc;
			dtParent= DataProvider.CreateParentDataTable ();

			pc = dtParent.ExtendedProperties;

			// Checking ExtendedProperties default
			Assert.IsNotNull (pc, "DT149");

			// Checking ExtendedProperties count
			Assert.AreEqual (0, pc.Count, "DT150");
		}

		[Test]
		[Category ("NotWorking")]
		public void PrimaryKey()
		{
			DataTable dtParent;
			dtParent = DataProvider.CreateParentDataTable();

			// Checking PrimaryKey default
			Assert.AreEqual (0, dtParent.PrimaryKey.Length, "DT151");

			// Checking PrimaryKey set/get
			DataColumn[] dcArr = new DataColumn[] {dtParent.Columns[0]};
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
			Assert.AreEqual (dcArr, dtParent.PrimaryKey, "DT152");

			dtParent.PrimaryKey=null;
			DataSet ds = new DataSet();
			DataRow dr = null;
			ds.Tables.Add(dtParent);

			//check primary key - ColumnType String, ds.CaseSensitive = false;
			ds.CaseSensitive = false;
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["String1"]};
			// check primary key - ColumnType String, ds.CaseSensitive = false;
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["String1"] = dr["String1"].ToString().ToUpper();
			try {
				dtParent.Rows.Add(dr);
				Assert.Fail("#A1");
			} catch (ConstraintException ex) {
				// Column 'String1' is constrained to be unique.
				// Value '1-STRING1' is already present
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'String1'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'1-STRING1'") != -1, "#A6");
			}
			if (dr.RowState != DataRowState.Detached)
				dtParent.Rows.Remove(dr);

			//check primary key - ColumnType String, ds.CaseSensitive = true;
			ds.CaseSensitive = true;
			// check primary key ConstraintException - ColumnType String, ds.CaseSensitive = true;
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["String1"] = dr["String1"].ToString();
			try {
				dtParent.Rows.Add(dr);
				Assert.Fail("#B1");
			} catch (ConstraintException ex) {
				// Column 'String1' is constrained to be unique.
				// Value '1-String1' is already present
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'String1'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'1-String1'") != -1, "#B6");
			}
			if (dr.RowState != DataRowState.Detached)
				dtParent.Rows.Remove(dr);

			//check primary key - ColumnType String, ds.CaseSensitive = true;
			ds.CaseSensitive = true;

			// check primary key - ColumnType String, ds.CaseSensitive = true;
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["String1"] = dr["String1"].ToString().ToUpper();
			dtParent.Rows.Add(dr);
			Assert.AreEqual(true, dtParent.Rows.Contains(dr["String1"]), "DT157");

			if (dr.RowState != DataRowState.Detached)
				dtParent.Rows.Remove(dr);

			dtParent.PrimaryKey=null;
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["ParentDateTime"]};
			// check primary key - ColumnType DateTime
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDateTime"] = DateTime.Now;
			dtParent.Rows.Add(dr);
			Assert.AreEqual(true, dtParent.Rows.Contains(dr["ParentDateTime"]), "DT158");
			if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

			// check primary key ConstraintException- ColumnType DateTime
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDateTime"] = dtParent.Rows[0]["ParentDateTime"];
			try {
				dtParent.Rows.Add(dr);
				Assert.Fail("#C1");
			} catch (ConstraintException ex) {
				// Column 'ParentDateTime' is constrained to be
				// unique.  Value '1/1/2005 12:00:00 AM' is
				// already present
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf ("'ParentDateTime'") != -1, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("'1/1/2005 12:00:00 AM'") != -1, "#C6");
			}
			if (dr.RowState != DataRowState.Detached)
				dtParent.Rows.Remove(dr);

			dtParent.PrimaryKey=null;
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["ParentDouble"]};
			// check primary key - ColumnType ParentDouble, value=Epsilon
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDouble"] = Double.Epsilon;
			dtParent.Rows.Add(dr);
			Assert.AreEqual(true, dtParent.Rows.Contains(dr["ParentDouble"]), "DT161");
			if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

			// check primary key ConstraintException - ColumnType ParentDouble
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDouble"] = dtParent.Rows[0]["ParentDouble"];
			try {
				dtParent.Rows.Add(dr);
				Assert.Fail("#D1");
			} catch (ConstraintException ex) {
				// Column 'ParentDouble' is constrained to be
				// unique.  Value '1.534' is already present
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'ParentDouble'") != -1, "#D5");
				Assert.IsTrue (ex.Message.IndexOf ("'1.534'") != -1, "#D6");
			}
			if (dr.RowState != DataRowState.Detached)
				dtParent.Rows.Remove(dr);

			//
			// SubTest
			//
			dtParent.PrimaryKey = null;
			// check primary key ConstraintException - ColumnType ParentBool 
			try {
				dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["ParentBool"]};
				Assert.Fail("#E1");
			} catch (ArgumentException ex) {
				// These columns don't currently have unique values
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}
			if (dr.RowState != DataRowState.Detached)
				dtParent.Rows.Remove(dr);

			//
			// SubTest
			//
			dtParent.PrimaryKey=null;
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["ParentDouble"],dtParent.Columns["ParentDateTime"]};
			// check primary key - ColumnType Double,DateTime test1
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDouble"] = dtParent.Rows[0]["ParentDouble"];
			dr["ParentDateTime"] = DateTime.Now;
			dtParent.Rows.Add(dr);
			Assert.AreEqual(true, dtParent.Rows.Contains(new object[] {dr["ParentDouble"],dr["ParentDateTime"]}), "DT166");
			if (dr.RowState != DataRowState.Detached)
				dtParent.Rows.Remove(dr);

			// check primary key - ColumnType Double,DateTime test2
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDateTime"] = dtParent.Rows[0]["ParentDateTime"];
			dr["ParentDouble"] = 99.399;
			dtParent.Rows.Add(dr);
			Assert.AreEqual(true, dtParent.Rows.Contains(new object[] {dr["ParentDouble"],dr["ParentDateTime"]}), "DT167");
			if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

			// check primary key ConstraintException - ColumnType Double,DateTime 
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDouble"] = dtParent.Rows[0]["ParentDouble"];
			dr["ParentDateTime"] = dtParent.Rows[0]["ParentDateTime"];
			try {
				dtParent.Rows.Add (dr);
				Assert.Fail("#F1");
			} catch (ConstraintException ex) {
				// Column 'ParentDouble, ParentDateTime' is
				// constrained to be unique.  Value '1.534,
				// 1/1/2005 12:00:00 AM' is already present
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsTrue (ex.Message.IndexOf ("'ParentDouble, ParentDateTime'") != -1, "#F5");
				Assert.IsTrue (ex.Message.IndexOf ("'1.534, 1/1/2005 12:00:00 AM'") != -1, "#F6");
			}
			if (dr.RowState != DataRowState.Detached)
				dtParent.Rows.Remove(dr);

			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtChild);
			dtParent.PrimaryKey = null;
			//this test was addedd to check java exception: 
			//System.ArgumentException: Cannot remove UniqueConstraint because the ForeignKeyConstraint myRelation exists.
			// check add primary key with relation 
			ds.Relations.Add(new DataRelation("myRelation",ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]));
			//the following line will cause java to fail
			ds.Tables[0].PrimaryKey = new DataColumn[] {ds.Tables[0].Columns[0],ds.Tables[0].Columns[1]};
			Assert.AreEqual (2, ds.Tables[0].PrimaryKey.Length, "DT170");
		}
	
		[Test] // bug #319089
		public void Compute_WithoutSchemaData_Test ()
		{
			DataSet ds = new DataSet ("TestData");
			DataTable table = ds.Tables.Add ("TestTable");

			table.Columns.Add ("Id");
			table.Columns.Add ("Value");

			table.Rows.Add (new object[] {"1", "4.5"});
			table.Rows.Add (new object[] {"2", "7.5"});
			table.Rows.Add (new object[] {"3", "2.5"});
			table.Rows.Add (new object[] {"4", "3.5"});
			
			Assert.AreEqual ("1",
				table.Compute ("Min(Id)", String.Empty), "#1");
			Assert.AreEqual ("4",
				table.Compute ("Max(Id)", String.Empty), "#2");
			Assert.AreEqual ("2.5",
				table.Compute ("Min(Value)", String.Empty), "#3");
			Assert.AreEqual ("7.5",
				table.Compute ("Max(Value)", String.Empty), "#4");
		}

		[Test]
		public void BeginLoadData ()
		{
			DataTable dt = DataProvider.CreateParentDataTable ();
			dt.Columns [0].AllowDBNull = false;

			try {
				//if BeginLoadData has not been called, an exception will be throw
				dt.LoadDataRow (new object [] {null, "A", "B"}, false);
				Assert.Fail ("#A1");
			} catch (NoNullAllowedException ex) {
				// Column 'ParentId' does not allow nulls
				Assert.AreEqual (typeof (NoNullAllowedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'ParentId'") != -1, "#A5");
			}

			DataTable dt1 = DataProvider.CreateUniqueConstraint();

			dt1.BeginLoadData ();

			DataRow dr = dt1.NewRow ();
			dr [0] = 3;
			dt1.Rows.Add (dr);

			try {
				dt1.EndLoadData ();
				Assert.Fail ("#B1");
			} catch (ConstraintException ex) {
				// Failed to enable constraints. One or more rows
				// contain values violating non-null, unique, or
				// foreign-key constraints
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");

				Assert.AreEqual (2, dt1.GetErrors().Length, "#B5");
				Assert.IsTrue (dt1.GetErrors () [0].RowError.Length > 10, "#B6");
				Assert.IsTrue (dt1.GetErrors () [1].RowError.Length > 10, "#B7");
			}

			DataSet ds = DataProvider.CreateForigenConstraint ();
			ds.Tables [0].BeginLoadData ();
			ds.Tables [0].Rows [0] [0] = 10; //Forigen constraint violation

			try {
				ds.Tables [0].EndLoadData ();
				Assert.Fail ("#C1");
			} catch (ConstraintException ex) {
				// Failed to enable constraints. One or more
				// rows contain values violating non-null,
				// unique, or foreign-key constraints
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");

				Assert.AreEqual(3,ds.Tables[1].GetErrors().Length, "#C5");
				for (int index =0; index < 3; index++)
					Assert.IsTrue (ds.Tables [1].GetErrors () [index].RowError.Length > 10, "#C6");
			}
		}

		private DataRowAction drExpectedAction;
		
		[Test]
		public void OnRowChanged ()
		{
			ProtectedTestClass dt = new ProtectedTestClass ();

			EventRaised = false;
			dt.OnRowChanged_Test (DataRowAction.Nothing);
			
			Assert.IsFalse (EventRaised, "DT181");

			dt.RowChanged += new DataRowChangeEventHandler(OnRowChanged_Handler);
			foreach (int i in Enum.GetValues (typeof (DataRowAction))) {
				EventRaised = false;
				EventValues = false;
				drExpectedAction = (DataRowAction) i;
				dt.OnRowChanged_Test (drExpectedAction);

				Assert.IsTrue (EventRaised, "DT182");
				Assert.IsTrue (EventValues, "DT183");
			}
			dt.RowChanged -= new DataRowChangeEventHandler(OnRowChanged_Handler);
		}

		private void OnRowChanged_Handler (Object sender, DataRowChangeEventArgs e)
		{
			DataTable dt = (DataTable)sender;
			if (dt.Rows [0].Equals (e.Row) && e.Action == drExpectedAction)
				EventValues = true;
			EventRaised = true;
		}

		[Test]
		public void OnRowChanging ()
		{
			ProtectedTestClass dt = new ProtectedTestClass();

			EventRaised = false;
			dt.OnRowChanging_Test (DataRowAction.Nothing);
			
			Assert.IsFalse (EventRaised, "DT184");

			dt.RowChanging += new DataRowChangeEventHandler(OnRowChanging_Handler);
			foreach (int i in Enum.GetValues (typeof (DataRowAction))) {
				EventRaised = false;
				EventValues = false;
				drExpectedAction = (DataRowAction) i;
				dt.OnRowChanging_Test (drExpectedAction);

				Assert.IsTrue (EventRaised, "DT185");
				Assert.IsTrue (EventValues, "DT186");
			}
			dt.RowChanging -= new DataRowChangeEventHandler(OnRowChanging_Handler);
		}

		private void OnRowChanging_Handler (Object sender,DataRowChangeEventArgs e)
		{
			DataTable dt = (DataTable) sender;
			if (dt.Rows [0].Equals (e.Row) && e.Action == drExpectedAction)
				EventValues = true;
			EventRaised = true;
		}

		[Test]
		public void OnRowDeleted ()
		{
			ProtectedTestClass dt = new ProtectedTestClass();

			EventRaised = false;
			dt.OnRowDeleted_Test (DataRowAction.Nothing);

			Assert.IsFalse (EventRaised, "DT187");

			dt.RowDeleted += new DataRowChangeEventHandler(OnRowDeleted_Handler);
			foreach (int i in Enum.GetValues (typeof (DataRowAction))) {
				EventRaised = false;
				EventValues = false;
				drExpectedAction = (DataRowAction) i;
				dt.OnRowDeleted_Test (drExpectedAction);

				Assert.IsTrue (EventRaised, "DT188");
				Assert.IsTrue (EventValues, "DT189");
			}
			dt.RowDeleted -= new DataRowChangeEventHandler (OnRowDeleted_Handler);
		}

		private void OnRowDeleted_Handler (Object sender,DataRowChangeEventArgs e)
		{
			DataTable dt = (DataTable)sender;
			if (dt.Rows [0].Equals (e.Row) && e.Action == drExpectedAction)
				EventValues = true;
			EventRaised = true;
		}

		[Test]
		public void OnRowDeleting ()
		{
			ProtectedTestClass dt = new ProtectedTestClass();

			EventRaised = false;
			dt.OnRowDeleting_Test (DataRowAction.Nothing);

			Assert.IsFalse (EventRaised, "DT190");

			dt.RowDeleting += new DataRowChangeEventHandler(OnRowDeleting_Handler);
			foreach (int i in Enum.GetValues (typeof(DataRowAction))) {
				EventRaised = false;
				EventValues = false;
				drExpectedAction = (DataRowAction) i;
				dt.OnRowDeleting_Test (drExpectedAction);

				Assert.IsTrue (EventRaised, "DT191");
				Assert.IsTrue (EventValues, "DT192");
			}
			dt.RowDeleting -= new DataRowChangeEventHandler(OnRowDeleting_Handler);
		}

		[Test]
		public void BeginInit_PrimaryKey_1 ()
		{
			DataTable table = new DataTable ();
			DataColumn col = table.Columns.Add ("col", typeof (int));
			table.PrimaryKey = new DataColumn[] {col};
			table.AcceptChanges ();
			Assert.AreEqual (1, table.PrimaryKey.Length, "#1");
		
			table.BeginInit ();
			DataColumn col2 = new DataColumn ("col2", typeof (int));
			table.Columns.AddRange (new DataColumn[] {col2});
			table.PrimaryKey = new DataColumn[] {col2};
			table.EndInit ();
			Assert.AreEqual (1, table.PrimaryKey.Length, "#2");
			Assert.AreEqual ("col2", table.PrimaryKey[0].ColumnName, "#3");
		}

		[Test]
		public void BeginInit_PrimaryKey_2()
		{
			DataTable table = new DataTable ();
			DataColumn col = table.Columns.Add ("col", typeof (int));
			table.PrimaryKey = new DataColumn[] {col};
			table.AcceptChanges ();
	
			// ms.net behavior.	
			table.BeginInit ();
			DataColumn col1 = new DataColumn ("col1", typeof (int));
			table.Columns.AddRange (new DataColumn[] {col1});
			UniqueConstraint uc = new UniqueConstraint (string.Empty, new String[] {"col1"}, true);
			table.Constraints.AddRange (new Constraint[] {uc});

			try {
				table.EndInit ();
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Cannot add primary key constraint since primary
				// key is already set for the table
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void BeginInit_PrimaryKey_3 ()
		{
			DataTable table = new DataTable ();
			DataColumn col1 = table.Columns.Add ("col1", typeof (int));
			DataColumn col2 = table.Columns.Add ("col2", typeof (int));
		
			// ms.net behavior
			table.BeginInit ();
			UniqueConstraint uc = new UniqueConstraint (string.Empty, new String[] {"col1"}, true);
			table.Constraints.AddRange (new Constraint[] {uc});
			table.PrimaryKey = new DataColumn [] {col2};
			table.EndInit ();

			Assert.AreEqual ("col1", table.PrimaryKey[0].ColumnName, "#1");
		}
		
		[Test]
		public void PrimaryKey_OnFailing ()
		{
			DataTable table = new DataTable ();
			DataColumn col1 = table.Columns.Add ("col1", typeof (int));
			table.PrimaryKey = new DataColumn[] {col1};

			try {
				table.PrimaryKey = new DataColumn [] { new DataColumn ("col2", typeof (int)) };
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				//  Column must belong to a table
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}

			Assert.AreEqual ("col1", table.PrimaryKey [0].ColumnName, "#6");
		}

		[Test]
		public void BeginInit_Cols_Constraints ()
		{
			DataTable table = new DataTable ();

			// if both cols and constraints are added after BeginInit, the cols
			// should be added, before the constraints are added/validated
			table.BeginInit ();
			DataColumn col1 = new DataColumn ("col1", typeof (int));
			table.Columns.AddRange (new DataColumn[] {col1});
			UniqueConstraint uc = new UniqueConstraint (string.Empty, new String[] {"col1"}, false);
			table.Constraints.AddRange (new Constraint[] {uc});
			// no exception shud be thrown
			table.EndInit ();

			Assert.AreEqual (1, table.Constraints.Count, "#1");
		}

		[Test]
		public void LoadDataRow_ExistingData ()
		{
			DataSet ds = new DataSet ();
			DataTable table = ds.Tables.Add ();
			
			table.Columns.Add ("col1", typeof (int));
			table.Columns.Add ("col2", typeof (int));
			table.PrimaryKey = new DataColumn[] {table.Columns [0]};

			table.BeginLoadData ();
			table.LoadDataRow (new object[] {1,10}, true);
			table.LoadDataRow (new object[] {2,10}, true);
			table.LoadDataRow (new object[] {3,10}, true);
			table.LoadDataRow (new object[] {4,10}, true);
			table.EndLoadData ();

			Assert.AreEqual (4, table.Rows.Count, "#1");
			Assert.AreEqual (10, table.Rows [0][1], "#2");
			Assert.AreEqual (10, table.Rows [1][1], "#3");
			Assert.AreEqual (10, table.Rows [2][1], "#4");
			Assert.AreEqual (10, table.Rows [3][1], "#5");

			table.BeginLoadData ();
			table.LoadDataRow (new object[] {1,100}, true);
			table.LoadDataRow (new object[] {2,100}, true);
			table.LoadDataRow (new object[] {3,100}, true);
			table.LoadDataRow (new object[] {4,100}, true);
			table.EndLoadData ();

			Assert.AreEqual (4, table.Rows.Count, "#6");
			Assert.AreEqual (100, table.Rows [0][1], "#7");
			Assert.AreEqual (100, table.Rows [1][1], "#8");
			Assert.AreEqual (100, table.Rows [2][1], "#7");
			Assert.AreEqual (100, table.Rows [3][1], "#10");
		}

		[Test]
		public void LoadDataRow_DefaultValueError ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.Columns.Add ("col2", typeof (int));
			table.PrimaryKey = new DataColumn[] {table.Columns [0]};

			table.BeginLoadData ();
			// should not throw exception
			table.LoadDataRow (new object[] {1}, true);
			table.EndLoadData ();

			Assert.AreEqual (1, table.Rows [0][0], "#1");
			Assert.AreEqual (DBNull.Value, table.Rows [0][1], "#2");
		}

		[Test]
		public void RejectChanges_CheckIndex ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.PrimaryKey = new DataColumn[] {table.Columns [0]};

			table.Rows.Add (new object[] {1});
			table.AcceptChanges ();

			table.Rows [0][0] = 10;
			table.RejectChanges ();

			Assert.IsNotNull (table.Rows.Find (1));
		}

		private void OnRowDeleting_Handler (Object sender,DataRowChangeEventArgs e)
		{
			DataTable dt = (DataTable)sender;
			if (dt.Rows [0].Equals (e.Row) && e.Action == drExpectedAction)
				EventValues = true;
			EventRaised = true;
		}

		[Test]
		public void Select_StringString ()
		{
			DataTable dt = DataProvider.CreateChildDataTable ();

			DataRow [] drSelect;
			List<DataRow> al;

			//add some rows
			dt.Rows.Add (new object[] {99,88, "bla", "wowww"});
			dt.Rows.Add (new object[] {999,888, string.Empty, "woowww"});

			//get excepted resault
			al = new List<DataRow> ();
			foreach (DataRow dr in dt.Rows) {
				if ((int) dr["ChildId"] == 1)
					al.Add (dr);
			}
			//al.Reverse();
			al.Sort (new DataRowsComparer ("ParentId", "Desc"));

			drSelect = dt.Select ("ChildId=1", "ParentId Desc");
			Assert.AreEqual (al.ToArray (), drSelect, "DT193");

			//get excepted resault
			al = new List<DataRow>();
			foreach (DataRow dr in dt.Rows) {
				if (dr ["String1"].ToString () == "1-String1")
					al.Add (dr);
			}
			//al.Reverse();
			al.Sort (new DataRowsComparer ("ParentId", "Desc"));

			drSelect = dt.Select("String1='1-String1'", "ParentId Desc");
			Assert.AreEqual (al.ToArray(),drSelect, "DT194");

			//get excepted resault
			al = new List<DataRow> ();
			foreach (DataRow dr in dt.Rows) {
				if ((int) dr["ChildId"] == 1 && dr["String1"].ToString() == "1-String1")
					al.Add (dr);
			}
			//al.Reverse();
			al.Sort (new DataRowsComparer ("ParentId", "Desc"));

			drSelect = dt.Select ("ChildId=1 and String1='1-String1'", "ParentId Desc");
			Assert.AreEqual(al.ToArray (), drSelect, "DT195");
			

			//get excepted resault
			al = new List<DataRow> ();
			foreach (DataRow dr in dt.Rows) {
				if (dr ["String1"].ToString ().Length < 4)
					al.Add (dr);
			}
			//al.Reverse();
			al.Sort(new DataRowsComparer("ParentId", "Desc"));

			drSelect = dt.Select("Len(String1) < 4 ", "ParentId Desc");
			Assert.AreEqual(al.ToArray(),drSelect, "DT196");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void Select_StringString_1 ()
		{
			DataTable dt = DataProvider.CreateChildDataTable ();

			DataRow [] drSelect;
			List<DataRow> al;

			//add some rows
			dt.Rows.Add (new object[] {99,88, "bla", "wowww"});
			dt.Rows.Add (new object[] {999,888, string.Empty, "woowww"});
		
			//get excepted resault
			al = new List<DataRow> ();
			foreach (DataRow dr in dt.Rows) {
				if (dr ["String1"].ToString ().IndexOf ("String") > 0)
					al.Add (dr);
			}
			//al.Reverse();
			al.Sort (new DataRowsComparer ("ParentId", "Desc"));

			drSelect = dt.Select ("String1 like '%%String*'  ", "ParentId Desc");
			Assert.AreEqual (al.ToArray (), drSelect, "DT197");
			
			//get excepted resault
			al = new List<DataRow> ();
			foreach (DataRow dr in dt.Rows) {
				if (((int) dr ["ChildId"] == 2) || ((int) dr ["ChildId"] == 3))
					al.Add (dr);
			}
			//al.Reverse();
			al.Sort (new DataRowsComparer ("ParentId", "Desc"));

			drSelect = dt.Select ("ChildId in (2,3)  ", "ParentId Desc");
			Assert.AreEqual (al.ToArray (), drSelect, "DT198");

			//get excepted resault
			al = new List<DataRow> ();
			foreach (DataRow dr in dt.Rows) {
				if ((((int) dr ["ChildId"] * (int) dr ["ParentId"]) > 5))
					al.Add (dr);
			}
			al.Sort (new DataRowsComparer ("ChildId", "Asc"));

			drSelect = dt.Select ("ChildId * ParentId > 5 ", "ChildId Asc");
			// Cannot check this way as ArrayList.Sort uses unstable sort and 
			// so the Order of the elements is not preserved when elements are equal
			//Assert.AreEqual (al.ToArray(),drSelect, "DT199");
			Assert.AreEqual (al.Count, drSelect.Length, "#DT199");
			for (int i = 0; i < al.Count; ++i){
				// check the datarow is present and that the sort order is correct
				Assert.IsTrue (Array.IndexOf (drSelect, al[i]) != -1, "#DT_199_2_"+i);
				Assert.AreEqual (((DataRow)al[i])["ChildId"], drSelect[i]["ChildId"], "#DT_199_1" +i);
			}

			//get excepted resault
			al = new List<DataRow> ();
			foreach (DataRow dr in dt.Rows) {
				if (dr ["String2"].ToString ().Substring (2, 3) == "Str")
					al.Add (dr);
			}
			al.Sort (new DataRowsComparer ("ParentId", "Desc"));

			drSelect = dt.Select ("SubString(String2,3,3) like 'Str' ", "ParentId Desc");
			Assert.AreEqual (al.ToArray (), drSelect, "DT200");
		}

		[Test]
		[Category ("NotWorking")]
		public void Select_StringString_2 ()
		{
			DataTable dt = DataProvider.CreateParentDataTable ();

			try {
				dt.Select (dt.Columns [0].ColumnName + "=1", "[x");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// [x isn't a valid Sort string entry
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("[x") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
		}

		[Test]
		public void Select_StringString_3 ()
		{	
			DataTable dt = DataProvider.CreateParentDataTable ();
			//Select - parse sort string checking 1");
			try {
				dt.Select (dt.Columns [0].ColumnName, dt.Columns [0].ColumnName + "1");
				Assert.Fail ("#1");
			} catch (IndexOutOfRangeException ex) {
				// Cannot find column ParentId1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ParentId1") != -1, "#5");
			}
		}

		[Test]
		public void Select_NonBooleanFilter ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			
			DataRow[] row = table.Select ("col1*10");
			Assert.AreEqual (0, row.Length, "#A");

			// No exception shud be thrown 
			// The filter created earlier (if cached), will raise an EvaluateException
			// and so shouldnt be cached
			for (int i=0; i < 5; ++i)
				table.Rows.Add (new object [] { i });

			try {
				table.Select ("col1*10");
				Assert.Fail ("#B1");
			} catch (EvaluateException ex) {
				// Filter expression 'col1*10' does not evaluate
				// to a Boolean term
				Assert.AreEqual (typeof (EvaluateException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				//Assert.IsTrue (ex.Message.IndexOf ("'col1*10'") != -1, "#B5");
				//Assert.IsTrue (ex.Message.IndexOf ("Boolean") != -1, "#B6");
			}
		}

		[Test]
		public void Select_BoolColumn ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.Columns.Add ("col2", typeof (bool));

			for (int i = 0; i < 5; i++)
				table.Rows.Add (new object [] { i });

			DataRow [] result;
			try {
				result = table.Select ("col1");
				Assert.Fail ("#1");
			} catch (EvaluateException ex) {
				// Filter expression 'col1' does not evaluate to
				// a Boolean term
				Assert.AreEqual (typeof (EvaluateException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				//Assert.IsTrue (ex.Message.IndexOf ("'col1'") != -1, "#5");
				//Assert.IsTrue (ex.Message.IndexOf ("Boolean") != -1, "#6");
			}

			//col2 is a boolean expression, and a null value translates to
			//false.
			result = table.Select ("col2");
			Assert.AreEqual (0, result.Length);
		}

		[Test]
		public void Select_OrderOfRows ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.Columns.Add ("col2", typeof (int));
		
			for (int i = 0; i < 10; i++)
				table.Rows.Add (new object [] { 10 - i, i });
			DataRow[] result = table.Select ("col1 > 5");

			Assert.AreEqual (6, result [0][0], "# incorrect sorting order");
		}

#if NET_2_0
		[Test]
		public void DataTable_Clone ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (DateTime));
			table.Columns [0].DateTimeMode = DataSetDateTime.Local;
			DataTable table1 = table.Clone ();
			Assert.AreEqual (DataSetDateTime.Local, table1.Columns [0].DateTimeMode, "#1");

			//Test any other new prop in 2.0
		}

		[Test]
		public void Merge_SchemaTest ()
		{
			DataTable table1 = new DataTable ("t1");
			table1.Columns.Add ("col1", typeof (int));

			DataTable table2 = new DataTable ("t2");
			table2.Columns.Add ("col2", typeof (int));

			DataTable table3;

			table3 = table1.Clone ();
			table3.Merge (table2);
			Assert.AreEqual (2, table3.Columns.Count, "#1");
			table3 = table1.Clone ();
			table3.Merge (table2, false, MissingSchemaAction.Ignore);
			Assert.AreEqual (1, table3.Columns.Count, "#2");

			// source constraints are ignored
			table2.Constraints.Add ("uc", table2.Columns [0], false);
			table3 = table1.Clone ();
			table3.Merge (table2);
			Assert.AreEqual (0, table3.Constraints.Count, "#3");

			// source PK is merged depending on MissingSchemaAction
			table2.PrimaryKey = new DataColumn[] {table2.Columns[0]};
			table3 = table1.Clone ();
			table3.Merge (table2);
			Assert.AreEqual (1, table3.Constraints.Count, "#4");
			table3 = table1.Clone ();
			table3.Merge (table2, false, MissingSchemaAction.Ignore);
			Assert.AreEqual (0, table3.Constraints.Count, "#5");

			//FIXME : If both source and target have PK, then 
			// shud be the exception raised when schema is merged? 
			// ms.net throws a nullreference exception.
			// If any data is merged, exception is anyways raised.
			/*
			table1.PrimaryKey = new DataColumn[] {table1.Columns[0]};
			table3 = table1.Clone ();
			try {
				table3.Merge(table2);
				Assert.Fail("#6");
			} catch (DataException e) {}
			*/

			table3.Merge (table2,false,MissingSchemaAction.Ignore);
			table1.PrimaryKey = null;

			// DateTime columns, DataType match only if DateTimeMode matches
			table1.Columns.Add ("col_datetime", typeof (DateTime));
			table2.Columns.Add ("col_datetime", typeof (DateTime));
			table1.Columns ["col_datetime"].DateTimeMode = DataSetDateTime.Local;
			table2.Columns ["col_datetime"].DateTimeMode = DataSetDateTime.Unspecified;

			table3 = table1.Clone ();
			try {
				table3.Merge (table2);
				Assert.Fail ("#7");
			} catch (DataException) {
				// <target>.col_datetime and <source>.col_datetime
				// have conflicting properties: DataType property mismatch.
			}

			table1.Columns ["col_datetime"].DateTimeMode = DataSetDateTime.Unspecified;
			table2.Columns ["col_datetime"].DateTimeMode = DataSetDateTime.UnspecifiedLocal;
			table3 = table1.Clone ();
			table3.Merge (table2);
			Assert.AreEqual (DataSetDateTime.Unspecified, table3.Columns ["col_datetime"].DateTimeMode, "#9");
		}

		[Test]
		public void Merge_TestData ()
		{
			DataTable t1 = new DataTable ("t1");
			DataTable t2 = new DataTable ("t2");

			t1.Columns.Add ("c1", typeof (int));
			t1.Columns.Add ("c2", typeof (int));
			t2.Columns.Add ("c1", typeof (int));
			t2.Columns.Add ("c2", typeof (int));

			t1.Rows.Add (new object[] {1, 1});
			t1.Rows.Add (new object[] {2, 2});

			t2.Rows.Add (new object[] {1, 5});
			t2.Rows.Add (new object[] {1, 10});

			DataTable t3 = t1.Copy ();
			// When primary key is not defined, rows are not merged.
			t3.Merge (t2);
			Assert.AreEqual (4, t3.Rows.Count, "#1");

			t1.PrimaryKey = new DataColumn[] {t1.Columns[0]};
			t3 = t1.Copy ();
			t3.Merge (t2);
			Assert.AreEqual (2, t3.Rows.Count, "#2");
			Assert.AreEqual (10, t3.Rows [0][1], "#3");

			t3 = t1.Copy ();
			t3.Merge (t2, true);
			Assert.AreEqual (2, t3.Rows.Count, "#4");
			Assert.AreEqual (1, t3.Rows [0][1], "#5");
		}
#endif

		internal class DataRowsComparer : IComparer<DataRow>
		{
			private string _columnName;
			private string _direction;

			public DataRowsComparer(string columnName, string direction)
			{
				_columnName = columnName;
				if (direction.ToLower() != "asc" && direction.ToLower() != "desc")
					throw new ArgumentException("Direction can only be one of: 'asc' or 'desc'");
				_direction = direction;
			}

			public int Compare(DataRow drX, DataRow drY)
			{
				object objX = drX[_columnName];
				object objY = drY[_columnName];

				int compareResult = Comparer.Default.Compare(objX, objY);

				//If we are comparing desc we need to reverse the result.
				if (_direction.ToLower() == "desc")
					compareResult = -compareResult;

				return compareResult;
			}
		}
	}
}
