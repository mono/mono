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
[TestFixture] public class DataView_RowStateFilter : GHTBase
{
	[Test] public void Main()
	{
		DataView_RowStateFilter tc = new DataView_RowStateFilter();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataView_RowStateFilter");
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
		/*
			Added			A new row. 4 
			CurrentRows		Current rows including unchanged, new, and modified rows. 22 
			Deleted			A deleted row. 8 
			ModifiedCurrent A current version, which is a modified version of original data (see ModifiedOriginal). 16 
			ModifiedOriginal The original version (although it has since been modified and is available as ModifiedCurrent). 32 
			None			None. 0 
			OriginalRows	Original rows including unchanged and deleted rows. 42 
			Unchanged		An unchanged row. 2 
		 */

		//DataRowView[] drvResult = null;
		System.Collections.ArrayList al = new System.Collections.ArrayList();




		Exception exp = null;
		DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();

		//create the dataview for the table
		DataView dv = new DataView(dt);

		DataRow[]  drResult;

		dt.Rows[0].Delete();
		dt.Rows[1]["ParentId"] = 1;
		dt.Rows[2]["ParentId"] = 1;
		dt.Rows[3].Delete();
		dt.Rows.Add(new object[] {1,"A","B"});
		dt.Rows.Add(new object[] {1,"C","D"});
		dt.Rows.Add(new object[] {1,"E","F"});

			
		//---------- Added -------- 
		dv.RowStateFilter = DataViewRowState.Added ;
		drResult = GetResultRows(dt,DataRowState.Added);
		try
		{
			BeginCase("Added");
			Compare(CompareSortedRows(dv,drResult),true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		//---------- CurrentRows -------- 
		dv.RowStateFilter = DataViewRowState.CurrentRows ;
		drResult = GetResultRows(dt,DataRowState.Unchanged | DataRowState.Added  | DataRowState.Modified );
		try
		{
			BeginCase("CurrentRows");
			Compare(CompareSortedRows(dv,drResult),true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//---------- ModifiedCurrent -------- 
		dv.RowStateFilter = DataViewRowState.ModifiedCurrent  ;
		drResult = GetResultRows(dt,DataRowState.Modified );
		try
		{
			BeginCase("ModifiedCurrent");
			Compare(CompareSortedRows(dv,drResult) ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//---------- ModifiedOriginal -------- 
		dv.RowStateFilter = DataViewRowState.ModifiedOriginal   ;
		drResult = GetResultRows(dt,DataRowState.Modified );
		try
		{
			BeginCase("ModifiedOriginal");
			Compare(CompareSortedRows(dv,drResult) ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//---------- Deleted -------- 
		dv.RowStateFilter = DataViewRowState.Deleted ;
		drResult = GetResultRows(dt,DataRowState.Deleted );
		try
		{
			BeginCase("Deleted");
			Compare(CompareSortedRows(dv,drResult),true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		/*
				//---------- OriginalRows -------- 
				dv.RowStateFilter = DataViewRowState.OriginalRows ;
				drResult = GetResultRows(dt,DataRowState.Unchanged | DataRowState.Deleted );
				try
				{
					BeginCase("OriginalRows");
					Compare(CompareSortedRows(dv,drResult),true );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
		*/
	}
	
	private DataRow[] GetResultRows(DataTable dt,DataRowState State)
	{
		//get expected rows
		System.Collections.ArrayList al = new System.Collections.ArrayList();
		DataRowVersion drVer = DataRowVersion.Current;


		//From MSDN -	The row the default version for the current DataRowState.
		//				For a DataRowState value of Added, Modified or Current, 
		//				the default version is Current. 
		//				For a DataRowState of Deleted, the version is Original.
		//				For a DataRowState value of Detached, the version is Proposed.

		if (	((State & DataRowState.Added)		> 0)  
			| ((State & DataRowState.Modified)	> 0)  
			| ((State & DataRowState.Unchanged)	> 0) ) 
			drVer = DataRowVersion.Current;
		if ( (State & DataRowState.Deleted)		> 0
			| (State & DataRowState.Detached)	> 0 )  
			drVer = DataRowVersion.Original; 

		foreach (DataRow dr in dt.Rows )
		{
			if ( dr.HasVersion(drVer) 
				//&& ((int)dr["ParentId", drVer] == 1) 
				&& ((dr.RowState & State) > 0 ) 
				)
				al.Add(dr);
		}
		DataRow[] result = (DataRow[])al.ToArray((typeof(DataRow)));
		return result; 
	}

	private bool CompareSortedRows(DataView dv, DataRow[] drTable)
	{
		if (dv.Count != drTable.Length) throw new Exception("DataRows[] length are different");

		//comparing the rows by using columns ParentId and ChildId
		if ((dv.RowStateFilter & DataViewRowState.Deleted) > 0)
		{
			for (int i=0; i<dv.Count ; i++)
			{
				if (dv[i].Row["ParentId",DataRowVersion.Original ].ToString() != drTable[i]["ParentId",DataRowVersion.Original].ToString()) 
					return false;
			}
		}
		else
		{
			for (int i=0; i<dv.Count ; i++)
			{
				if (dv[i].Row["ParentId"].ToString() != drTable[i]["ParentId"].ToString()) 
					return false;
			}
		}
		return true;
	}
}




	



}