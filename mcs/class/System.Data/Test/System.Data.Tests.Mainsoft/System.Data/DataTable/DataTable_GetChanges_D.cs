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

using GHTUtils;
using GHTUtils.Base;

namespace tests.system_data_dll.System_Data
{
[TestFixture] public class DataTable_GetChanges_D : GHTBase
{
	[Test] public void Main()
	{
		DataTable_GetChanges_D tc = new DataTable_GetChanges_D();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataTable_GetChanges_D");
			tc.run();
		}
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			tc.EndTest(exp);
		}
	}

	//Activate This Construntor to log All To Standard output
	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}


	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

	public void run()
	{
		Exception exp = null;
		
		
		DataTable dt1,dt2 = GHTUtils.DataProvider.CreateParentDataTable();
		dt2.Constraints.Add("Unique",dt2.Columns[0],true);
		dt2.Columns[0].DefaultValue=7;

		//make some changes
		dt2.Rows[0].Delete(); //DataRowState.Deleted 
		dt2.Rows[1].Delete(); //DataRowState.Deleted 
		dt2.Rows[2].BeginEdit();
		dt2.Rows[2]["String1"] = "Changed"; //DataRowState.Modified 
		dt2.Rows[2].EndEdit();

		dt2.Rows.Add(new object[] {"99","Temp1","Temp2"}); //DataRowState.Added 

		// *********** Checking GetChanges - DataRowState.Deleted ************
		dt1=null;
		dt1 = dt2.GetChanges(DataRowState.Deleted);
		CheckTableSchema (dt1,dt2,DataRowState.Deleted.ToString());
		DataRow[] drArr1,drArr2;
		drArr1 = dt1.Select("","",DataViewRowState.Deleted );
		drArr2 = dt2.Select("","",DataViewRowState.Deleted );

		for (int i=0; i<drArr1.Length ; i++)
		{
			try
			{
				BeginCase(String.Format("GetChanges(Deleted) - Data [ParentId]{0}" ,i));
				Compare(drArr1[i]["ParentId",DataRowVersion.Original ],drArr2[i]["ParentId", DataRowVersion.Original]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase(String.Format("GetChanges(Deleted) - Data [String1]{0}" ,i));
				Compare(drArr1[i]["String1", DataRowVersion.Original],drArr2[i]["String1", DataRowVersion.Original]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase(String.Format("GetChanges(Deleted) - Data [String2]{0}" ,i));
				Compare(drArr1[i]["String2", DataRowVersion.Original],drArr2[i]["String2", DataRowVersion.Original]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		// *********** Checking GetChanges - DataRowState.Modified ************
		dt1=null;
		dt1 = dt2.GetChanges(DataRowState.Modified);
		CheckTableSchema (dt1,dt2,DataRowState.Modified.ToString());
		drArr1 = dt1.Select("","");
		drArr2 = dt2.Select("","",DataViewRowState.ModifiedCurrent);

		for (int i=0; i<drArr1.Length ; i++)
		{
			try
			{
				BeginCase(String.Format("GetChanges(Modified) - Data [ParentId]{0}" ,i));
				Compare(drArr1[i]["ParentId"],drArr2[i]["ParentId"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase(String.Format("GetChanges(Modified) - Data [String1]{0}" ,i));
				Compare(drArr1[i]["String1"],drArr2[i]["String1"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase(String.Format("GetChanges(Modified) - Data [String2]{0}" ,i));
				Compare(drArr1[i]["String2" ],drArr2[i]["String2"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		// *********** Checking GetChanges - DataRowState.Added ************
		dt1=null;
		dt1 = dt2.GetChanges(DataRowState.Added);
		CheckTableSchema (dt1,dt2,DataRowState.Added.ToString());
		drArr1 = dt1.Select("","");
		drArr2 = dt2.Select("","",DataViewRowState.Added );

		for (int i=0; i<drArr1.Length ; i++)
		{
			try
			{
				BeginCase(String.Format("GetChanges(Added) - Data [ParentId]{0}" ,i));
				Compare(drArr1[i]["ParentId"],drArr2[i]["ParentId"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase(String.Format("GetChanges(Added) - Data [String1]{0}" ,i));
				Compare(drArr1[i]["String1"],drArr2[i]["String1"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase(String.Format("GetChanges(Added) - Data [String2]{0}" ,i));
				Compare(drArr1[i]["String2" ],drArr2[i]["String2"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		// *********** Checking GetChanges - DataRowState.Unchanged  ************
		dt1=null;
		dt1 = dt2.GetChanges(DataRowState.Unchanged);
		CheckTableSchema (dt1,dt2,DataRowState.Unchanged .ToString());
		drArr1 = dt1.Select("","");
		drArr2 = dt2.Select("","",DataViewRowState.Unchanged  );

		for (int i=0; i<drArr1.Length ; i++)
		{
			try
			{
				BeginCase(String.Format("GetChanges(Unchanged) - Data [ParentId]{0}" ,i));
				Compare(drArr1[i]["ParentId"],drArr2[i]["ParentId"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase(String.Format("GetChanges(Unchanged) - Data [String1]{0}" ,i));
				Compare(drArr1[i]["String1"],drArr2[i]["String1"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase(String.Format("GetChanges(Unchanged) - Data [String2]{0}" ,i));
				Compare(drArr1[i]["String2" ],drArr2[i]["String2"]);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}


	}

	private void CheckTableSchema(DataTable dt1, DataTable dt2,string Description)
	{
		Exception exp = null;
		for (int i=0; i<dt2.Constraints.Count; i++)
		{
			try
			{
				BeginCase(String.Format("GetChanges - Constraints[{0}] - {1}",i,Description));
				Compare( dt1.Constraints[i].ConstraintName ,dt2.Constraints[i].ConstraintName );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}
		
		for (int i=0; i<dt2.Columns.Count; i++)
		{
			try
			{
				BeginCase(String.Format("GetChanges - Columns[{0}].ColumnName - {1}",i,Description));
				Compare( dt1.Columns[i].ColumnName  ,dt2.Columns[i].ColumnName );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase(String.Format("GetChanges - Columns[{0}].DataType {1}",i,Description));
				Compare( dt1.Columns[i].DataType ,dt2.Columns[i].DataType );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}
	}

}
}