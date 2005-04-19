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
using System.Data;

using GHTUtils;
using GHTUtils.Base;

using NUnit.Framework;

namespace tests.system_data_dll.System_Data
{
	[TestFixture] public class DataTable_Select_SSD : GHTBase
	{
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		public void TearDown()
		{
		}

		[Test] public void Main()
		{
			DataTable_Select_SSD tc = new DataTable_Select_SSD();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataTable_Select_SSD");
				tc.SetUp();
				tc.run();
				tc.TearDown();
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

		public void run()
		{
			Exception exp = null;
			DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();
			DataRow[] drSelect, drResult;

			dt.Rows[0].Delete();
			dt.Rows[1]["ParentId"] = 1;
			dt.Rows[2]["ParentId"] = 1;
			dt.Rows[3].Delete();
			dt.Rows.Add(new object[] {1,"A","B"});
			dt.Rows.Add(new object[] {1,"C","D"});
			dt.Rows.Add(new object[] {1,"E","F"});
			
			drSelect = dt.Select("ParentId=1","",DataViewRowState.Added );
			drResult = GetResultRows(dt,DataRowState.Added);
			try
			{
				BeginCase("Select_SSD DataViewRowState.Added");
				Compare(drSelect ,drResult);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			drSelect = dt.Select("ParentId=1","",DataViewRowState.CurrentRows  );
			drResult = GetResultRows(dt,DataRowState.Unchanged | DataRowState.Added  | DataRowState.Modified );
			try
			{
				BeginCase("Select_SSD DataViewRowState.CurrentRows");
				Compare(drSelect ,drResult);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			drSelect = dt.Select("ParentId=1","",DataViewRowState.Deleted  );
			drResult = GetResultRows(dt,DataRowState.Deleted );
			try
			{
				BeginCase("Select_SSD DataViewRowState.Deleted");
				Compare(drSelect ,drResult);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			drSelect = dt.Select("ParentId=1","",DataViewRowState.ModifiedCurrent | DataViewRowState.ModifiedOriginal  );
			drResult = GetResultRows(dt,DataRowState.Modified );
			try
			{
				BeginCase("Select_SSD ModifiedCurrent or ModifiedOriginal");
				Compare(drSelect ,drResult);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}
	
		private DataRow[] GetResultRows(DataTable dt,DataRowState State)
		{
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
					&& ((int)dr["ParentId", drVer] == 1) 
					&& ((dr.RowState & State) > 0 ) 
					)
					al.Add(dr);
			}
			DataRow[] result = (DataRow[])al.ToArray((typeof(DataRow)));
			return result; 
		}	
	}
}
