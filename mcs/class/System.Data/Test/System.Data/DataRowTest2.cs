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

using NUnit.Framework;
using System;
using System.IO;
using System.ComponentModel;
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataRowTest2
	{
		[Test] public void AcceptChanges()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataRow myRow;
			myRow = myTable.NewRow();
			myTable.Rows.Add(myRow);

			// DataRow AcceptChanges
			// DataRowState.Added -> DataRowState.Unchanged 
			myTable.AcceptChanges();
			Assert.AreEqual(DataRowState.Unchanged , myRow.RowState , "DRW1");
		}

		[Test] public void CancelEdit()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Id",typeof(int));
			dc.Unique=true;
			myTable.Columns.Add(dc);
			myTable.Rows.Add(new object[] {1});
			myTable.Rows.Add(new object[] {2});
			myTable.Rows.Add(new object[] {3});

			DataRow myRow = myTable.Rows[0];
			myRow.BeginEdit();
			myRow[0] = 7;
			myRow.CancelEdit();

			// DataRow CancelEdit
			Assert.AreEqual(true ,  (int)myRow[0] == 1, "DRW2");
		}

		[Test] public void ClearErrors()
		{
			DataTable dt = new DataTable("myTable"); 
			DataRow dr = dt.NewRow();
			dr.RowError = "err";

			// DataRow ClearErrors
			Assert.AreEqual(true ,  dr.HasErrors , "DRW3");

			// DataRow ClearErrors
			dr.ClearErrors();
			Assert.AreEqual(false ,  dr.HasErrors , "DRW4");
		}

		[Test] public void Delete()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Id",typeof(int));
			dc.Unique=true;
			myTable.Columns.Add(dc);
			myTable.Rows.Add(new object[] {1});
			myTable.Rows.Add(new object[] {2});
			myTable.Rows.Add(new object[] {3});
			myTable.AcceptChanges();

			DataRow myRow = myTable.Rows[0];
			myRow.Delete();

			// Delete1
			Assert.AreEqual(DataRowState.Deleted  ,  myRow.RowState , "DRW5");

			// Delete2
			myTable.AcceptChanges();
			Assert.AreEqual(DataRowState.Detached  ,  myRow.RowState , "DRW6");
		}

		[Test] public void EndEdit()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Id",typeof(int));
			dc.Unique=true;
			myTable.Columns.Add(dc);
			myTable.Rows.Add(new object[] {1});
			myTable.Rows.Add(new object[] {2});
			myTable.Rows.Add(new object[] {3});

			DataRow myRow = myTable.Rows[0];

			int iProposed;
			//After calling the DataRow object's BeginEdit method, if you change the value, the Current and Proposed values become available
			myRow.BeginEdit();
			myRow[0] = 7;
			iProposed = (int)myRow[0,DataRowVersion.Proposed];
			myRow.EndEdit();

			// EndEdit
			Assert.AreEqual(iProposed ,  (int)myRow[0,DataRowVersion.Current] , "DRW7");
		}

		[Test] public void Equals()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataRow dr1,dr2;
			dr1 = myTable.NewRow();
			dr2 = myTable.NewRow();

			// not equals
			Assert.AreEqual(false  , dr1.Equals(dr2), "DRW8");

        	dr1=dr2;
			// equals
			Assert.AreEqual(true , dr1.Equals(dr2), "DRW9");
		}

		[Test] public void GetChildRows_ByDataRealtion()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + dr["ParentId"]);
			//Get Result
			drArrResult = dr.GetChildRows(dRel);

			// GetChildRows_D
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW10");
		}

		[Test] public void GetChildRows_ByDataRealtionDataRowVersion()
		{
			DataRow drParent;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];

			// Teting: DateTime.Now.ToShortTimeString()
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW11");

			// Teting: DataRow.GetParentRows_D_D
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW12");

			// Teting: DataRow.GetParentRows_D_D
			//Get Excepted result, in this case Current = Default
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW13");

			// Teting: DataRow.GetParentRows_D_D
			drParent.BeginEdit();
			drParent["String1"] = "Value";
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW14");
		}

		[Test] public void GetChildRows_ByName()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + dr["ParentId"]);
			//Get Result
			drArrResult = dr.GetChildRows("Parent-Child");

			// GetChildRows_S
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW15");
		}

		[Test] public void GetChildRows_ByNameDataRowVersion()
		{
			DataRow drParent;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];

			// GetChildRows_SD 1
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows("Parent-Child",DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW16");

			// GetChildRows_SD 2
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows("Parent-Child",DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW17");

			// GetParentRows_SD 3
			//Get Excepted result, in this case Current = Default
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows("Parent-Child",DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW18");

			// GetParentRows_SD 4
			drParent.BeginEdit();
			drParent["String1"] = "Value";
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows("Parent-Child",DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW19");
		}

		[Test] public void GetColumnError_ByIndex()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// GetColumnError 1
			Assert.AreEqual(String.Empty ,  dr.GetColumnError(0) , "DRW20");

			dr.SetColumnError(0,sColErr );

			// GetColumnError 2
			Assert.AreEqual(sColErr ,  dr.GetColumnError(0) , "DRW21");
		}

		[Test] public void GetColumnError_ByName()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// GetColumnError 1
			Assert.AreEqual(String.Empty ,  dr.GetColumnError("Column1") , "DRW22");

			dr.SetColumnError("Column1",sColErr );

			// GetColumnError 2
			Assert.AreEqual(sColErr ,  dr.GetColumnError("Column1") , "DRW23");
		}

		[Test] public void GetColumnsInError()
		{
			string sColErr = "Error!";
			DataColumn[] dcArr;
			DataTable dt = new DataTable("myTable"); 
			//init some columns
			dt.Columns.Add(new DataColumn());
			dt.Columns.Add(new DataColumn());
			dt.Columns.Add(new DataColumn());
			dt.Columns.Add(new DataColumn());
			dt.Columns.Add(new DataColumn());

			//init some rows
			dt.Rows.Add(new object[] {});
			dt.Rows.Add(new object[] {});
			dt.Rows.Add(new object[] {});

			DataRow dr = dt.Rows[1];

			dcArr = dr.GetColumnsInError();

			// GetColumnsInError 1
			Assert.AreEqual(0,  dcArr.Length , "DRW24");

			dr.SetColumnError(0,sColErr);
			dr.SetColumnError(2,sColErr);
			dr.SetColumnError(4,sColErr);

			dcArr = dr.GetColumnsInError();

			// GetColumnsInError 2
			Assert.AreEqual(3, dcArr.Length , "DRW25");

			//check that the right columns taken
			// GetColumnsInError 3
			Assert.AreEqual(dt.Columns[0], dcArr[0], "DRW26");

			// GetColumnsInError 4
			Assert.AreEqual(dt.Columns[2], dcArr[1], "DRW27");

			// GetColumnsInError 5
			Assert.AreEqual(dt.Columns[4], dcArr[2], "DRW28");
		}

		[Test] public new void GetHashCode()
		{
			int iHashCode;
			DataRow dr;
			DataTable dt = new DataTable();
			dr = dt.NewRow();

			iHashCode = dr.GetHashCode();
			for (int i=0; i<10; i++)
			{	//must return the same value each time
				// GetHashCode #" + i
				Assert.AreEqual(dr.GetHashCode() ,  iHashCode , "DRW29");
			}
		}

		[Test] public void GetParentRow_ByDataRelation()
		{
			DataRow drExcepted,drResult,drChild;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent = DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			//Excepted result
			drExcepted = dtParent.Rows[0];

			//Get Result
			drChild = dtChild.Select("ParentId=" + drExcepted["ParentId"])[0]; 
			drResult = drChild.GetParentRow(dRel);

			// GetParentRow_D
			Assert.AreEqual(drExcepted.ItemArray,  drResult.ItemArray , "DRW30");
		}

		[Test] public void GetParentRow_ByDataRelationDataRowVersion()
		{
			DataRow drParent,drChild;
			DataRow drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			// GetParentRow_DD 1
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow(dRel,DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW31");

			// GetParentRow_DD 2
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow(dRel,DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW32");

			// GetParentRow_DD 3
			//Get Excepted result, in this case Current = Default
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow(dRel,DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW33");

			// GetParentRow_DD 4
			drChild.BeginEdit();
			drChild["String1"] = "Value";
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow(dRel,DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW34");
		}

		[Test] public void GetParentRow_ByName()
		{
			DataRow drExcepted,drResult,drChild;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent = DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			//Excepted result
			drExcepted = dtParent.Rows[0];

			//Get Result
			drChild = dtChild.Select("ParentId=" + drExcepted["ParentId"])[0]; 
			drResult = drChild.GetParentRow("Parent-Child");

			// GetParentRow_S
			Assert.AreEqual(drExcepted.ItemArray,  drResult.ItemArray , "DRW35");
		}

		[Test] public void GetParentRow_ByNameDataRowVersion()
		{
			DataRow drParent,drChild;
			DataRow drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			// GetParentRow_SD 1
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW36");

			// GetParentRow_SD 2
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW37");

			// GetParentRow_SD 3
			//Get Excepted result, in this case Current = Default
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW38");

			// GetParentRow_SD 4
			drChild.BeginEdit();
			drChild["String1"] = "Value";
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW39");
		}

		[Test] public void GetParentRows_ByDataRelation()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent = DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Duplicate several rows in order to create Many to Many relation
			dtParent.ImportRow(dr); 
			dtParent.ImportRow(dr); 
			dtParent.ImportRow(dr); 

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtParent.Select("ParentId=" + dr["ParentId"]);
			dr = dtChild.Select("ParentId=" + dr["ParentId"])[0];
			//Get Result
			drArrResult = dr.GetParentRows(dRel);

			// GetParentRows_D
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW40");
		}

		[Test] public void GetParentRows_ByName()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent = DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Duplicate several rows in order to create Many to Many relation
			dtParent.ImportRow(dr); 
			dtParent.ImportRow(dr); 
			dtParent.ImportRow(dr); 

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtParent.Select("ParentId=" + dr["ParentId"]);
			dr = dtChild.Select("ParentId=" + dr["ParentId"])[0];
			//Get Result
			drArrResult = dr.GetParentRows("Parent-Child");

			// GetParentRows_S
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW41");
		}

		[Test] public void GetParentRows_ByNameDataRowVersion()
		{
			DataRow drParent,drChild;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
			ds.Relations.Add(dRel);

			//Create several copies of the first row
			drParent = dtParent.Rows[0];	//row[0] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[1] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[2] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[3] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[4] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[5] has versions: Default,Current,Original
			dtParent.AcceptChanges();

			//Get the first child row for drParent
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			DataRow[] drTemp = dtParent.Select("ParentId=" + drParent["ParentId"]);
			//				Console.WriteLine("********");
			//				foreach (DataRow d in drTemp)
			//				{
			//					CheckRowVersion(d);
			//				}
			drTemp[0].BeginEdit();
			drTemp[0]["String1"] = "NewValue"; //row now has versions: Proposed,Current,Original,Default
			drTemp[1].BeginEdit();
			drTemp[1]["String1"] = "NewValue"; //row now has versions: Proposed,Current,Original,Default

			//		Console.WriteLine("********");
			//		foreach (DataRow d in drTemp)
			//		{
			//			CheckRowVersion(d);
			//		}
			//		Console.WriteLine("********");

			// Check DataRowVersion.Current
			//Check DataRowVersion.Current 
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW42");

			//Check DataRowVersion.Current 
			// Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Original
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW43");

			//Check DataRowVersion.Default
			// Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Default
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW44");

		/* .Net don't work as expected
			//Check DataRowVersion.Proposed
			// Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Proposed
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.ModifiedCurrent);
			//drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.ModifiedOriginal );

			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW45");
		*/		
		}

		private void CheckRowVersion(DataRow dr)
		{
			Console.WriteLine("");
			if (dr.HasVersion(DataRowVersion.Current)) Console.WriteLine("Has " + DataRowVersion.Current.ToString());
			if (dr.HasVersion(DataRowVersion.Default)) Console.WriteLine("Has " + DataRowVersion.Default.ToString());
			if (dr.HasVersion(DataRowVersion.Original)) Console.WriteLine("Has " + DataRowVersion.Original.ToString());
			if (dr.HasVersion(DataRowVersion.Proposed)) Console.WriteLine("Has " + DataRowVersion.Proposed.ToString());
		}

		[Test] public new void GetType()
		{
			Type myType;	
			DataTable dt = new DataTable(); 
			DataRow dr = dt.NewRow();
			myType = typeof(DataRow);

			// GetType
			Assert.AreEqual(typeof(DataRow), myType , "DRW46");
		}

		[Test] public void HasErrors()
		{
			DataTable dt = new DataTable("myTable"); 
			DataRow dr = dt.NewRow();

			// HasErrors (default)
			Assert.AreEqual(false, dr.HasErrors, "DRW47");

			dr.RowError = "Err";

			// HasErrors (set/get)
			Assert.AreEqual(true , dr.HasErrors , "DRW48");
		}

		[Test] public void HasVersion_ByDataRowVersion()
		{
			DataTable t = new DataTable("atable");
			t.Columns.Add("id", typeof(int));
			t.Columns.Add("name", typeof(string));
			t.Columns[0].DefaultValue = 1;
			t.Columns[1].DefaultValue = "something";

			// row r is detached
			DataRow r = t.NewRow();

			// HasVersion Test #10
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Current) , "DRW49");

			// HasVersion Test #11
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Original) , "DRW50");

			// HasVersion Test #12
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW51");

			// HasVersion Test #13
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Proposed) , "DRW52");

			r[0] = 4; 
			r[1] = "four";

			// HasVersion Test #20
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Current) , "DRW53");

			// HasVersion Test #21
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Original) , "DRW54");

			// HasVersion Test #22
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW55");

			// HasVersion Test #23
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Proposed) , "DRW56");

			t.Rows.Add(r);
			// now it is "added"

			// HasVersion Test #30
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW57");

			// HasVersion Test #31
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Original) , "DRW58");

			// HasVersion Test #32
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW59");

			// HasVersion Test #33
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW60");

			t.AcceptChanges();
			// now it is "unchanged"

			// HasVersion Test #40
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW61");

			// HasVersion Test #41
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW62");

			// HasVersion Test #42
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW63");

			// HasVersion Test #43
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW64");

			r.BeginEdit();
			r[1] = "newvalue";

			// HasVersion Test #50
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW65");

			// HasVersion Test #51
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW66");

			// HasVersion Test #52
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW67");

			// HasVersion Test #53
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Proposed) , "DRW68");

			r.EndEdit();
			// now it is "modified"
			// HasVersion Test #60
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW69");

			// HasVersion Test #61
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW70");

			// HasVersion Test #62
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW71");

			// HasVersion Test #63
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW72");

			// this or t.AcceptChanges
			r.AcceptChanges(); 
			// now it is "unchanged" again
			// HasVersion Test #70
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW73");

			// HasVersion Test #71
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW74");

			// HasVersion Test #72
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW75");

			// HasVersion Test #73
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW76");

			r.Delete();
			// now it is "deleted"

			// HasVersion Test #80
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Current) , "DRW77");

			// HasVersion Test #81
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW78");

			// HasVersion Test #82
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Default) , "DRW79");

			// HasVersion Test #83
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW80");

			r.AcceptChanges();
			// back to detached
			// HasVersion Test #90
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Current) , "DRW81");

			// HasVersion Test #91
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Original) , "DRW82");

			// HasVersion Test #92
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Default) , "DRW83");

			// HasVersion Test #93
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW84");
		}

		[Test] public void IsNull_ByDataColumn()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

			// IsNull_I 2
			Assert.AreEqual(false , dr.IsNull(dc0) , "DRW85");

			// IsNull_I 2
			Assert.AreEqual(true , dr.IsNull(dc1) , "DRW86");
		}

		[Test] public void IsNull_ByDataColumnDataRowVersion()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

			// IsNull - col0 Current
			Assert.AreEqual(false, dr.IsNull(dc0,DataRowVersion.Current) , "DRW87");

			// IsNull - col1 Current
			Assert.AreEqual(true, dr.IsNull(dc1,DataRowVersion.Current) , "DRW88");

			// IsNull - col0 Default
			Assert.AreEqual(false, dr.IsNull(dc0,DataRowVersion.Default) , "DRW89");
			// IsNull - col1 Default
			Assert.AreEqual(true, dr.IsNull(dc1,DataRowVersion.Default) , "DRW90");

			dr.BeginEdit();
			dr[0] = 9; //Change value, Create RowVersion Proposed

			// IsNull - col0 Proposed
			Assert.AreEqual(false, dr.IsNull(dc0,DataRowVersion.Proposed) , "DRW91");
			// IsNull - col1 Proposed
			Assert.AreEqual(true, dr.IsNull(dc1,DataRowVersion.Proposed) , "DRW92");

			dr.AcceptChanges();
			dr.Delete();

			// IsNull - col0 Original
			Assert.AreEqual(false, dr.IsNull(dc0,DataRowVersion.Original) , "DRW93");
		}

		[Test] public void IsNull_ByIndex()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

			// IsNull_I 2
			Assert.AreEqual(false , dr.IsNull(0) , "DRW94");

			// IsNull_I 2
			Assert.AreEqual(true , dr.IsNull(1) , "DRW95");
		}

		[Test] public void IsNull_ByName()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

#region --- assignment  ----
			// IsNull_S 1
			Assert.AreEqual(false, dr.IsNull("Col0"), "DRW96");

			// IsNull_S 2
			Assert.AreEqual(true, dr.IsNull("Col1"), "DRW97");
#endregion

#region --- bug 3124 ---

			// IsNull_S 1
			MemoryStream st = new MemoryStream();
			StreamWriter sw = new StreamWriter(st);
			sw.Write("<?xml version=\"1.0\" standalone=\"yes\"?><NewDataSet>");
			sw.Write("<Table><EmployeeNo>9</EmployeeNo></Table>");
			sw.Write("</NewDataSet>");
			sw.Flush();
			st.Position=0;
			DataSet ds = new DataSet();
			ds.ReadXml(st);
			//  Here we add the expression column
			ds.Tables[0].Columns.Add("ValueListValueMember", typeof(object), "EmployeeNo");

			foreach( DataRow row in ds.Tables[0].Rows )
			{
				// Console.WriteLine(row["ValueListValueMember"].ToString() + " " );
				if( row.IsNull("ValueListValueMember") == true )
					Assert.AreEqual("Failed", "SubTest", "DRW98");
				else
					Assert.AreEqual("Passed", "Passed", "DRW99");
			}

#endregion
		}

		[Test] public void Item()
		{
			// init table with columns
			DataTable myTable = new DataTable("myTable"); 

			myTable.Columns.Add(new DataColumn("Id",typeof(int)));
			myTable.Columns.Add(new DataColumn("Name",typeof(string)));
			DataColumn dc = myTable.Columns[0];

			myTable.Rows.Add(new object[] {1,"Ofer"});
			myTable.Rows.Add(new object[] {2,"Ofer"});

			myTable.AcceptChanges();

			DataRow myRow = myTable.Rows[0];

			//Start checking

			// Item - index
			Assert.AreEqual(1 , (int)myRow[0] , "DRW100");

			// Item - string
			Assert.AreEqual(1 ,  (int)myRow["Id"] , "DRW101");

			// Item - Column
			Assert.AreEqual(1 ,  (int)myRow[dc] , "DRW102");

			// Item - index,Current
			Assert.AreEqual(1 ,  (int)myRow[0,DataRowVersion.Current ] , "DRW103");

			// Item - string,Current
			Assert.AreEqual(1 ,  (int)myRow["Id",DataRowVersion.Current] , "DRW104");

			// Item - columnn,Current
			Assert.AreEqual(1 ,  (int)myRow[dc,DataRowVersion.Current] , "DRW105");

			//	testMore();
		}

		/*public void testMore()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Rows[0].BeginEdit();
			dt.Rows[0][0] = 10;
			dt.Rows[0].EndEdit();
			dt.AcceptChanges();
		}*/

		[Test] public void RejectChanges()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			dt.AcceptChanges();
			DataRow dr = dt.Rows[0];

			dr[0] = 567;
			dr[1] = 789;
			dr.RejectChanges();

			// RejectChanges - row 0
			Assert.AreEqual(1234  ,  (int)dr[0], "DRW106");

			// RejectChanges - row 1
			Assert.AreEqual(DBNull.Value  ,  dr[1] , "DRW107");

			dr.Delete();
			dr.RejectChanges();

			// RejectChanges - count
			Assert.AreEqual(1 ,  dt.Rows.Count , "DRW108");
		}

		[Test] public void RowState()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Name",typeof(string));
			myTable.Columns.Add(dc);
			DataRow myRow;

			// Create a new DataRow.
			myRow = myTable.NewRow();

			// Detached row.

			// Detached
			Assert.AreEqual(DataRowState.Detached ,  myRow.RowState , "DRW109");

			myTable.Rows.Add(myRow);
			// New row.

			// Added
			Assert.AreEqual(DataRowState.Added ,  myRow.RowState , "DRW110");

			myTable.AcceptChanges();
			// Unchanged row.

			// Unchanged
			Assert.AreEqual(DataRowState.Unchanged ,  myRow.RowState , "DRW111");

			myRow["Name"] = "Scott";
			// Modified row.

			// Modified
			Assert.AreEqual(DataRowState.Modified ,  myRow.RowState , "DRW112");

			myRow.Delete();
			// Deleted row.

			// Deleted
			Assert.AreEqual(DataRowState.Deleted ,  myRow.RowState , "DRW113");
		}

		[Test] public void SetColumnError_ByDataColumnError()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// empty string
			Assert.AreEqual(String.Empty,  dr.GetColumnError(dc) , "DRW114");

			dr.SetColumnError(dc,sColErr );

			// error string
			Assert.AreEqual(sColErr, dr.GetColumnError(dc) , "DRW115");
		}

		[Test] public void SetColumnError_ByIndexError()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// empty string
			Assert.AreEqual(String.Empty ,  dr.GetColumnError(0) , "DRW116");

			dr.SetColumnError(0,sColErr );

			// error string
			Assert.AreEqual(sColErr  ,  dr.GetColumnError(0) , "DRW117");
		}

		[Test] public void SetColumnError_ByColumnNameError()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// empty string
			Assert.AreEqual(String.Empty,  dr.GetColumnError("Column1") , "DRW118");

			dr.SetColumnError("Column1",sColErr );

			// error string
			Assert.AreEqual(sColErr,  dr.GetColumnError("Column1") , "DRW119");
		}

		[Test] public void SetParentRow_ByDataRow()
		{
			DataRow drParent,drChild;
			DataRow drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			drChild.SetParentRow(drParent);

			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Current);

			// SetParentRow
			Assert.AreEqual(drArrExcepted ,  drArrResult, "DRW120");
		}

		public void testMore()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			DataRow drParent = ds.Tables[0].Rows[0];
			//DataRow[] drArray =  ds.Tables[1].Rows[0].GetParentRows(ds.Tables[1].ParentRelations[0]);
			ds.Tables[1].Rows[0].SetParentRow(drParent);
		}

		public void test()
		{
			// test SetParentRow
			DataTable parent = DataProvider.CreateParentDataTable();
			DataTable child = DataProvider.CreateChildDataTable();
			DataRow dr = parent.Rows[0];
			dr.Delete();
			parent.AcceptChanges();

			child.Rows[0].SetParentRow(dr);
		}

		public void checkForLoops()
		{
			DataSet ds = new DataSet();
			//Create tables
			DataTable  dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			dtChild.Rows.Clear();
			dtParent.Rows.Clear();

			dtParent.ChildRelations.Add(dtParent.Columns[0],dtChild.Columns[0]);
			dtChild.ChildRelations.Add(dtChild.Columns[0],dtParent.Columns[0]);

			dtChild.Rows[0].SetParentRow(dtParent.Rows[0]);
			dtParent.Rows[0].SetParentRow(dtChild.Rows[0]);
		}

		public void checkForLoopsAdvenced()
		{
			//Create tables
			DataTable  dtChild = new DataTable();
			dtChild.Columns.Add("Col1",typeof(int));
			dtChild.Columns.Add("Col2",typeof(int));

			DataRelation drl = new DataRelation("drl1",dtChild.Columns[0],dtChild.Columns[1]);
			dtChild.ChildRelations.Add(drl);
			dtChild.Rows[0].SetParentRow(dtChild.Rows[1]);
			dtChild.Rows[1].SetParentRow(dtChild.Rows[0]);
		}

		[Test] public void SetParentRow_ByDataRowDataRelation()
		{
			DataRow drParent,drChild;
			DataRow drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			drChild.SetParentRow(drParent ,dRel);

			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Current);

			// SetParentRow
			Assert.AreEqual(drArrExcepted ,  drArrResult, "DRW121");
		}

		[Test] public void Table()
		{
			DataTable dt1,dt2;
			dt2 = new DataTable("myTable"); 
			DataRow dr = dt2.NewRow();
			dt1 = dr.Table;

			// ctor
			Assert.AreEqual(dt2, dt1 , "DRW122");
		}

		[Test] public new  void ToString()
		{
			DataRow dr;
			DataTable dtParent;
			dtParent= DataProvider.CreateParentDataTable(); 
			dr = dtParent.Rows[0];

			// ToString
			Assert.AreEqual(true, dr.ToString().ToLower().StartsWith("system.data.datarow") , "DRW123");
		}
		
		[Test] public void DataRow_RowError()
		{
			DataTable dt = new DataTable ("myTable"); 
			DataRow dr = dt.NewRow ();
	
			Assert.AreEqual ( dr.RowError, string.Empty );
						
			dr.RowError = "Err";
			Assert.AreEqual ( dr.RowError , "Err" );
		}
		
		[Test] 
		[ExpectedException (typeof (ConstraintException))]
		public void DataRow_RowError2()
		{
			DataTable dt1 = DataProvider.CreateUniqueConstraint();

			dt1.BeginLoadData();

			DataRow  dr = dt1.NewRow();
			dr[0] = 3;
			dt1.Rows.Add(dr);
			dt1.EndLoadData();
		}
		
		[Test] 
		[ExpectedException (typeof (ConstraintException))]
		public void DataRow_RowError3()
		{
			DataSet ds= DataProvider.CreateForigenConstraint();
			ds.Tables[0].BeginLoadData();
			ds.Tables[0].Rows[0][0] = 10; 
			ds.Tables[0].EndLoadData(); //Foreign constraint violation
		}

		[Test]
		public void BeginEdit()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Id",typeof(int));
			dc.Unique=true;
			myTable.Columns.Add(dc);
			myTable.Rows.Add(new object[] {1});
			myTable.Rows.Add(new object[] {2});
			myTable.Rows.Add(new object[] {3});
			
			DataRow myRow = myTable.Rows[0];
			
			try	
			{ 
				myRow[0] = 2; //row[0] now conflict with row[1] 
				Assert.Fail("DRW121: failed to throw ConstraintException");
			}
			catch (ConstraintException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW122: Add. Wrong exception type. Got:" + exc);
			}

			//Will NOT! throw exception
			myRow.BeginEdit();
			myRow[0] = 2; //row[0] now conflict with row[1] 
					
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow dr = dt.Rows[0];
			dr.Delete();
			try
			{
				dr.BeginEdit();				
				Assert.Fail("DRW123: failed to throw DeletedRowInaccessibleException");
			}
			catch (DeletedRowInaccessibleException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW124: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void GetChildRows_DataRelation()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + dr["ParentId"]);
			//Get Result
			drArrResult = dr.GetChildRows(dRel);
			
			Assert.AreEqual(drArrExcepted, drArrResult, "DRW125");
		}

		[Test]
		public void GetParentRows_DataRelation_DataRowVersion()
		{
			DataRow drParent,drChild;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
					
			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			//Duplicate several rows in order to create Many to Many relation
			dtParent.ImportRow(drParent); 
			dtParent.ImportRow(drParent); 
			dtParent.ImportRow(drParent); 				
					
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
			ds.Relations.Add(dRel);

			//Get Excepted result
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRows(dRel,DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted, drArrResult, "DRW126");

			//Get Excepted result
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRows(dRel,DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted, drArrResult, "DRW127");

			//Get Excepted result, in this case Current = Default
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRows(dRel,DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted, drArrResult, "DRW128");
			
			try
			{
				DataTable dtOtherParent = DataProvider.CreateParentDataTable();
				DataTable dtOtherChild = DataProvider.CreateChildDataTable();

				DataRelation drl = new DataRelation("newRelation",dtOtherParent.Columns[0],dtOtherChild.Columns[0]);
				drChild.GetParentRows(drl,DataRowVersion.Current); 
				Assert.Fail("DRW129: failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW130: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void ItemArray()
		{
			DataTable dt = GetDataTable();				
			DataRow dr = dt.Rows[0];
	        
			Assert.AreEqual(1, (int)dr.ItemArray[0] , "DRW131" );

			Assert.AreEqual("Ofer", (string)dr.ItemArray[1] , "DRW132" );

			dt = GetDataTable();

			dr = dt.Rows[0];
			
			//Changing row via itemArray

			dt.Rows[0].ItemArray = new object[] {2,"Oren"};

			Assert.AreEqual(2, (Int32)dr.ItemArray[0] , "DRW133" );
			Assert.AreEqual("Oren", (string)dr.ItemArray[1] , "DRW134" );

			try
			{
				dt.Rows[0].ItemArray = new object[] {2,"Oren","some1else"};
				Assert.Fail("DRW135: failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW136: Add. Wrong exception type. Got:" + exc);
			}
		}

		private DataTable GetDataTable()
		{
			DataTable dt = new DataTable("myTable"); 
			dt.Columns.Add("Id",typeof(int));
			dt.Columns.Add("Name",typeof(string));

			DataRow dr = dt.NewRow();
			dr.ItemArray = new object[] {1,"Ofer"};

			dt.Rows.Add(dr);

			return dt;
		}

		public void RowError()
		{
			DataTable dt = new DataTable("myTable"); 
			DataRow dr = dt.NewRow();

			Assert.AreEqual(string.Empty , dr.RowError, "DRW137");
						
			dr.RowError = "Err";	        

			Assert.AreEqual("Err", dr.RowError , "DRW138" );

			DataTable dt1 = DataProvider.CreateUniqueConstraint();

			try
			{
				dt1.BeginLoadData();

				dr = dt1.NewRow();
				dr[0] = 3;
				dt1.Rows.Add(dr);
				dt1.EndLoadData();
				Assert.Fail("DRW139: failed to throw ConstraintException");
			}
			catch (ConstraintException) 
			{
				Assert.AreEqual(2,dt1.GetErrors().Length,"DRW141");
				Assert.AreEqual(true,dt1.GetErrors()[0].RowError.Length > 10,"DRW142");
				Assert.AreEqual(true,dt1.GetErrors()[1].RowError.Length > 10,"DRW143");
			}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW144: Wrong exception type. Got:" + exc);
			}


			DataSet ds=null;
			try
			{
				ds= DataProvider.CreateForigenConstraint();
				ds.Tables[0].BeginLoadData();
				ds.Tables[0].Rows[0][0] = 10; //Forigen constraint violation
				//ds.Tables[0].AcceptChanges();
				ds.Tables[0].EndLoadData();
				Assert.Fail("DRW139: failed to throw ConstraintException");
			}
			catch (ConstraintException) 
			{
				Assert.AreEqual(3,ds.Tables[1].GetErrors().Length,"DRW145");
				for(int index=0;index<3;index++)
				{
					Assert.AreEqual(true,ds.Tables[1].GetErrors()[index].RowError.Length > 10,"RDW146");
				}
			}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW147: Wrong exception type. Got:" + exc);
			}
		}
	}
}
