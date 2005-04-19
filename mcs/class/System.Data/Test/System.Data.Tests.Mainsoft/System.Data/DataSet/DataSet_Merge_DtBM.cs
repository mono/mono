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
[TestFixture] public class DataSet_Merge_DtBM : GHTBase
{
	[Test] public void Main()
	{
		DataSet_Merge_DtBM tc = new DataSet_Merge_DtBM();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_Merge_DtBM");
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

		DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();
		dt.TableName = "Table1";
		dt.PrimaryKey = new DataColumn[] {dt.Columns[0]};

		//create target dataset (copy of source dataset)
		DataSet dsTarget = new DataSet();
		dsTarget.Tables.Add(dt.Copy());

		//add new column (for checking MissingSchemaAction)
		DataColumn dc = new DataColumn("NewColumn",typeof(float));
		dt.Columns.Add(dc);

		//Update row
		string OldValue = dt.Select("ParentId=1")[0][1].ToString();
		dt.Select("ParentId=1")[0][1] = "NewValue";
		//delete rows
		dt.Select("ParentId=2")[0].Delete();
		//add row
		object[] arrAddedRow = new object[] {99,"NewRowValue1","NewRowValue2",new DateTime(0),0.5,true};
		dt.Rows.Add(arrAddedRow);
		
		#region "Merge(dt,true,MissingSchemaAction.Ignore )"
		DataSet dsTarget1 = dsTarget.Copy();
		dsTarget1.Merge(dt,true,MissingSchemaAction.Ignore );
		try
		{
			BeginCase("Merge true,Ignore - Column");
			Compare(dsTarget1.Tables["Table1"].Columns.Contains("NewColumn"),false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Ignore - changed values");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=1")[0][1] , OldValue);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Ignore - added values");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=99")[0].ItemArray  , arrAddedRow);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Ignore - deleted row");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=2").Length > 0,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		#endregion

		#region "Merge(dt,false,MissingSchemaAction.Ignore )"

		dsTarget1 = dsTarget.Copy();
		dsTarget1.Merge(dt,false,MissingSchemaAction.Ignore );
		try
		{
			BeginCase("Merge true,Ignore - Column");
			Compare(dsTarget1.Tables["Table1"].Columns.Contains("NewColumn"),false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Ignore - changed values");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=1")[0][1] , "NewValue");
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Ignore - added values");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=99")[0].ItemArray  , arrAddedRow);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Ignore - deleted row");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=2").Length ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		#endregion

		#region "Merge(dt,true,MissingSchemaAction.Add  )"
		dsTarget1 = dsTarget.Copy();
		dsTarget1.Merge(dt,true,MissingSchemaAction.Add  );
		try
		{
			BeginCase("Merge true,Add - Column");
			Compare(dsTarget1.Tables["Table1"].Columns.Contains("NewColumn"),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Add - changed values");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=1")[0][1] , OldValue);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Add - added values");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=99").Length , 1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Add - deleted row");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=2").Length > 0,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		#endregion

		#region "Merge(dt,false,MissingSchemaAction.Add  )"
		dsTarget1 = dsTarget.Copy();
		dsTarget1.Merge(dt,false,MissingSchemaAction.Add  );
		try
		{
			BeginCase("Merge true,Add - Column");
			Compare(dsTarget1.Tables["Table1"].Columns.Contains("NewColumn"),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Add - changed values");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=1")[0][1] , "NewValue");
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Add - added values");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=99").Length , 1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge true,Add - deleted row");
			Compare(dsTarget1.Tables["Table1"].Select("ParentId=2").Length ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		#endregion

		#region "Merge(dt,false/true,MissingSchemaAction.Error  )"
//		dsTarget1 = dsTarget.Copy();
//		Exception expMerge = null;
//		try
//		{
//			BeginCase("Merge true,Error - Column");
//			try { dsTarget1.Merge(dt,true,MissingSchemaAction.Error ); }
//			catch (Exception e) {expMerge = e;}
//			Compare(expMerge.GetType() , typeof(InvalidOperationException) );
//		}
//		catch(Exception ex)	{exp = ex;}
//		finally	{EndCase(exp); exp = null;}
//
//		expMerge = null;
//		try
//		{
//			BeginCase("Merge false,Error - Column");
//			try { dsTarget1.Merge(dt,false,MissingSchemaAction.Error ); }
//			catch (Exception e) {expMerge = e;}
//			Compare(expMerge.GetType() , typeof(InvalidOperationException) );
//		}
//		catch(Exception ex)	{exp = ex;}
//		finally	{EndCase(exp); exp = null;}
		#endregion
	}
}
}