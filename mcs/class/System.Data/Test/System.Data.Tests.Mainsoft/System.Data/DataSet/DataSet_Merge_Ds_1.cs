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
using System.Data;
using System.Data.OleDb;

using GHTUtils;
using GHTUtils.Base;

namespace tests.system_data_dll.System_Data
{
	[TestFixture] public class DataSet_Merge_Ds_1 : GHTBase
	{
		[Test] public void Main()
		{
			DataSet_Merge_Ds_1 tc = new DataSet_Merge_Ds_1();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataSet_Merge_Ds_1");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		public void run()
		{
			Exception exp = null;
	
			//create source dataset
			DataSet ds = new DataSet();

			ds.Tables.Add(GHTUtils.DataProvider.CreateParentDataTable());
			ds.Tables.Add(GHTUtils.DataProvider.CreateChildDataTable());
			ds.Tables["Child"].TableName = "Child2";
			ds.Tables.Add(GHTUtils.DataProvider.CreateChildDataTable());

			// Console.WriteLine(ds.Tables[0].TableName + ds.Tables[1].TableName + ds.Tables[2].TableName);
			// Console.WriteLine(ds.Tables[2].Rows.Count.ToString());

			//craete a target dataset to the merge operation
			DataSet dsTarget = ds.Copy();

			//craete a second target dataset to the merge operation 
			DataSet dsTarget1 = ds.Copy();
		
			//------------------ make some changes in the second target dataset schema --------------------
			//add primary key
			dsTarget1.Tables["Parent"].PrimaryKey = new DataColumn[] {dsTarget1.Tables["Parent"].Columns["ParentId"]};
			dsTarget1.Tables["Child"].PrimaryKey = new DataColumn[] {dsTarget1.Tables["Child"].Columns["ParentId"],dsTarget1.Tables["Child"].Columns["ChildId"]};

			//add Foreign Key (different name)
			dsTarget1.Tables["Child2"].Constraints.Add("Child2_FK_2",dsTarget1.Tables["Parent"].Columns["ParentId"],dsTarget1.Tables["Child2"].Columns["ParentId"]);

			//add relation (different name)
			//dsTarget1.Relations.Add("Parent_Child_1",dsTarget1.Tables["Parent"].Columns["ParentId"],dsTarget1.Tables["Child"].Columns["ParentId"]);


			//------------------ make some changes in the source dataset schema --------------------
			//add primary key
			ds.Tables["Parent"].PrimaryKey = new DataColumn[] {ds.Tables["Parent"].Columns["ParentId"]};
			ds.Tables["Child"].PrimaryKey = new DataColumn[] {ds.Tables["Child"].Columns["ParentId"],ds.Tables["Child"].Columns["ChildId"]};

			//unique column
			ds.Tables["Parent"].Columns["String2"].Unique = true; //will not be merged

			//add Foreign Key
			ds.Tables["Child2"].Constraints.Add("Child2_FK",ds.Tables["Parent"].Columns["ParentId"],ds.Tables["Child2"].Columns["ParentId"]);

			//add relation
			ds.Relations.Add("Parent_Child",ds.Tables["Parent"].Columns["ParentId"],ds.Tables["Child"].Columns["ParentId"]);

			//add allow null constraint
			ds.Tables["Parent"].Columns["ParentBool"].AllowDBNull = false; //will not be merged

			//add Indentity column
			ds.Tables["Parent"].Columns.Add("Indentity",typeof(int));
			ds.Tables["Parent"].Columns["Indentity"].AutoIncrement = true;
			ds.Tables["Parent"].Columns["Indentity"].AutoIncrementStep = 2;

			//modify default value
			ds.Tables["Child"].Columns["String1"].DefaultValue = "Default"; //will not be merged

			//remove column
			ds.Tables["Child"].Columns.Remove("String2"); //will not be merged


			//-------------------- begin to check ----------------------------------------------
			try
			{
				BeginCase("merge 1 - make sure the merge method invoked without exceptions");
				dsTarget.Merge(ds);
				Compare("Success","Success");
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			CompareResults_1("merge 1",ds,dsTarget);

			//merge again, 
			try
			{
				BeginCase("merge 2 - make sure the merge method invoked without exceptions");
				dsTarget.Merge(ds);
				Compare("Success","Success");
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			CompareResults_1("merge 2",ds,dsTarget);

			try
			{
				BeginCase("merge second dataset - make sure the merge method invoked without exceptions");
				dsTarget1.Merge(ds);
				Compare("Success","Success");
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			CompareResults_2("merge 3",ds,dsTarget1);

		}


		void CompareResults_1(string Msg,DataSet ds, DataSet dsTarget)
		{
			Exception exp = null;

			try
			{
				BeginCase(Msg + " check Parent Primary key length");
				Compare(ds.Tables["Parent"].PrimaryKey.Length ,dsTarget.Tables["Parent"].PrimaryKey.Length );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Child Primary key length");
				Compare(ds.Tables["Child"].PrimaryKey.Length ,dsTarget.Tables["Child"].PrimaryKey.Length );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Parent Primary key columns");
				Compare(ds.Tables["Parent"].PrimaryKey[0].ColumnName ,dsTarget.Tables["Parent"].PrimaryKey[0].ColumnName);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Child Primary key columns[0]");
				Compare(ds.Tables["Child"].PrimaryKey[0].ColumnName ,dsTarget.Tables["Child"].PrimaryKey[0].ColumnName);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase(Msg + " check Child Primary key columns[1]");
				Compare(ds.Tables["Child"].PrimaryKey[1].ColumnName ,dsTarget.Tables["Child"].PrimaryKey[1].ColumnName);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Parent Unique columns");
				Compare(ds.Tables["Parent"].Columns["String2"].Unique ,dsTarget.Tables["Parent"].Columns["String2"].Unique);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Child2 Foreign Key name");
				Compare(ds.Tables["Child2"].Constraints[0].ConstraintName ,dsTarget.Tables["Child2"].Constraints[0].ConstraintName );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check dataset relation count");
				Compare(ds.Relations.Count ,dsTarget.Relations.Count );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check dataset relation - Parent column");
				Compare(ds.Relations[0].ParentColumns[0].ColumnName ,dsTarget.Relations[0].ParentColumns[0].ColumnName );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check dataset relation - Child column ");
				Compare(ds.Relations[0].ChildColumns[0].ColumnName ,dsTarget.Relations[0].ChildColumns[0].ColumnName );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check allow null constraint");
				Compare(dsTarget.Tables["Parent"].Columns["ParentBool"].AllowDBNull,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Indentity column");
				Compare(ds.Tables["Parent"].Columns.Contains("Indentity"),dsTarget.Tables["Parent"].Columns.Contains("Indentity"));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Indentity column - AutoIncrementStep");
				Compare(ds.Tables["Parent"].Columns["Indentity"].AutoIncrementStep,dsTarget.Tables["Parent"].Columns["Indentity"].AutoIncrementStep);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Indentity column - AutoIncrement");
				Compare(ds.Tables["Parent"].Columns["Indentity"].AutoIncrement,dsTarget.Tables["Parent"].Columns["Indentity"].AutoIncrement);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Indentity column - DefaultValue");
				Compare(dsTarget.Tables["Child"].Columns["String1"].DefaultValue == DBNull.Value , true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check remove colum");
				Compare(dsTarget.Tables["Child"].Columns.Contains("String2"),true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		void CompareResults_2(string Msg,DataSet ds, DataSet dsTarget)
		{
			Exception exp = null;

			try
			{
				BeginCase(Msg + " check Parent Primary key length");
				Compare(ds.Tables["Parent"].PrimaryKey.Length ,dsTarget.Tables["Parent"].PrimaryKey.Length );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Child Primary key length");
				Compare(ds.Tables["Child"].PrimaryKey.Length ,dsTarget.Tables["Child"].PrimaryKey.Length );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Parent Primary key columns");
				Compare(ds.Tables["Parent"].PrimaryKey[0].ColumnName ,dsTarget.Tables["Parent"].PrimaryKey[0].ColumnName);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Child Primary key columns[0]");
				Compare(ds.Tables["Child"].PrimaryKey[0].ColumnName ,dsTarget.Tables["Child"].PrimaryKey[0].ColumnName);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase(Msg + " check Child Primary key columns[1]");
				Compare(ds.Tables["Child"].PrimaryKey[1].ColumnName ,dsTarget.Tables["Child"].PrimaryKey[1].ColumnName);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Parent Unique columns");
				Compare(ds.Tables["Parent"].Columns["String2"].Unique ,dsTarget.Tables["Parent"].Columns["String2"].Unique);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Child2 Foreign Key name");
				Compare(dsTarget.Tables["Child2"].Constraints[0].ConstraintName,"Child2_FK_2" );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check dataset relation count");
				Compare(ds.Relations.Count ,dsTarget.Relations.Count );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check dataset relation - Parent column");
				Compare(ds.Relations[0].ParentColumns[0].ColumnName ,dsTarget.Relations[0].ParentColumns[0].ColumnName );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check dataset relation - Child column ");
				Compare(ds.Relations[0].ChildColumns[0].ColumnName ,dsTarget.Relations[0].ChildColumns[0].ColumnName );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check allow null constraint");
				Compare(dsTarget.Tables["Parent"].Columns["ParentBool"].AllowDBNull,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Indentity column");
				Compare(ds.Tables["Parent"].Columns.Contains("Indentity"),dsTarget.Tables["Parent"].Columns.Contains("Indentity"));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Indentity column - AutoIncrementStep");
				Compare(ds.Tables["Parent"].Columns["Indentity"].AutoIncrementStep,dsTarget.Tables["Parent"].Columns["Indentity"].AutoIncrementStep);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Indentity column - AutoIncrement");
				Compare(ds.Tables["Parent"].Columns["Indentity"].AutoIncrement,dsTarget.Tables["Parent"].Columns["Indentity"].AutoIncrement);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check Indentity column - DefaultValue");
				Compare(dsTarget.Tables["Child"].Columns["String1"].DefaultValue == DBNull.Value , true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(Msg + " check remove colum");
				Compare(dsTarget.Tables["Child"].Columns.Contains("String2"),true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				//TestCase for bug #3168
				BeginCase("Check Relation.Nested value, TestCase for bug #3168");
				DataSet orig = new DataSet();

				DataTable parent = orig.Tables.Add("Parent");
				parent.Columns.Add("Id", typeof(int));
				parent.Columns.Add("col1", typeof(string));
				parent.Rows.Add(new object[] {0, "aaa"});

				DataTable child = orig.Tables.Add("Child");
				child.Columns.Add("ParentId", typeof(int));
				child.Columns.Add("col1", typeof(string));
				child.Rows.Add(new object[] {0, "bbb"});

				orig.Relations.Add("Parent_Child", parent.Columns["Id"], child.Columns["ParentId"]);
				orig.Relations["Parent_Child"].Nested = true;

				DataSet merged = new DataSet();
				merged.Merge(orig);
				Compare(merged.Relations["Parent_Child"].Nested, orig.Relations["Parent_Child"].Nested);
			}
			catch (Exception ex) {exp=ex;}
			finally{EndCase(exp); exp=null;}

		}


		//Activate This Construntor to log All To Standard output
		//public TestClass():base(true){}

		//Activate this constructor to log Failures to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, false){}

		//Activate this constructor to log All to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, true){}

		//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

	}
}
