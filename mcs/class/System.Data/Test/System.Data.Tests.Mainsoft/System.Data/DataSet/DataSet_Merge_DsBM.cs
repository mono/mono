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
[TestFixture] public class DataSet_Merge_DsBM : GHTBase
{
	[Test] public void Main()
	{
		DataSet_Merge_DsBM tc = new DataSet_Merge_DsBM();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_Merge_DsBM");
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
	

		//create source dataset
		DataSet ds = new DataSet();

		DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();
		dt.TableName = "Table1";

		dt.PrimaryKey = new DataColumn[] {dt.Columns[0]};

		//add table to dataset
		ds.Tables.Add(dt.Copy());

		dt = ds.Tables[0];

		//create target dataset (copy of source dataset)
		DataSet dsTarget = ds.Copy();

		//add new column (for checking MissingSchemaAction)
		DataColumn dc = new DataColumn("NewColumn",typeof(float));
		//make the column to be primary key
		dt.Columns.Add(dc);

				
		
		//add new table (for checking MissingSchemaAction)
		ds.Tables.Add(new DataTable("NewTable"));
		ds.Tables["NewTable"].Columns.Add("NewColumn1",typeof(int));
		ds.Tables["NewTable"].Columns.Add("NewColumn2",typeof(long));
		ds.Tables["NewTable"].Rows.Add(new object[] {1,2});
		ds.Tables["NewTable"].Rows.Add(new object[] {3,4});
		ds.Tables["NewTable"].PrimaryKey = new DataColumn[] {ds.Tables["NewTable"].Columns["NewColumn1"]};


		#region "ds,false,MissingSchemaAction.Add)"
		DataSet dsTarget1 = dsTarget.Copy();
		dsTarget1.Merge(ds,false,MissingSchemaAction.Add);
		try
		{
			BeginCase("Merge MissingSchemaAction.Add - Column");
			Compare(dsTarget1.Tables["Table1"].Columns.Contains("NewColumn"),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge MissingSchemaAction.Add - Table");
			Compare(dsTarget1.Tables.Contains("NewTable"),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		//failed, should be success by MSDN Library documentation
//		try
//		{
//			BeginCase("Merge MissingSchemaAction.Add - PrimaryKey");
//			Compare(dsTarget1.Tables["NewTable"].PrimaryKey.Length,0);
//		}
//		catch(Exception ex)	{exp = ex;}
//		finally	{EndCase(exp); exp = null;}
		#endregion



		#region "ds,false,MissingSchemaAction.AddWithKey)"
		//MissingSchemaAction.Add,MissingSchemaAction.AddWithKey - behave the same, checked only Add

//		DataSet dsTarget2 = dsTarget.Copy();
//		dsTarget2.Merge(ds,false,MissingSchemaAction.AddWithKey);
//		try
//		{
//			BeginCase("Merge MissingSchemaAction.AddWithKey - Column");
//			Compare(dsTarget2.Tables["Table1"].Columns.Contains("NewColumn"),true);
//		}
//		catch(Exception ex)	{exp = ex;}
//		finally	{EndCase(exp); exp = null;}
//
//		try
//		{
//			BeginCase("Merge MissingSchemaAction.AddWithKey - Table");
//			Compare(dsTarget2.Tables.Contains("NewTable"),true);
//		}
//		catch(Exception ex)	{exp = ex;}
//		finally	{EndCase(exp); exp = null;}
//
//		try
//		{
//			BeginCase("Merge MissingSchemaAction.AddWithKey - PrimaryKey");
//			Compare(dsTarget2.Tables["NewTable"].PrimaryKey[0],dsTarget2.Tables["NewTable"].Columns["NewColumn1"]);
//		}
//		catch(Exception ex)	{exp = ex;}
//		finally	{EndCase(exp); exp = null;}
		#endregion

		#region "ds,false,MissingSchemaAction.Error)"
		//Error - throw System.Data.DataException, should throw InvalidOperationException
//		DataSet dsTarget3 ;
//		Exception expMerge = null;
//		try
//		{
//			BeginCase("Merge MissingSchemaAction.Error ");
//			dsTarget3 = dsTarget.Copy();
//			try {dsTarget3.Merge(ds,false,MissingSchemaAction.Error);}
//			catch (Exception e) {expMerge = e;}
//			Compare(expMerge.GetType() , typeof(InvalidOperationException));
//		}
//		catch(Exception ex)	{exp = ex;}
//		finally	{EndCase(exp); exp = null;}
		#endregion

		

		#region "ds,false,MissingSchemaAction.Ignore )"
		DataSet dsTarget4 = dsTarget.Copy();
		dsTarget4.Merge(ds,false,MissingSchemaAction.Ignore );
		try
		{
			BeginCase("Merge MissingSchemaAction.Ignore - Column");
			Compare(dsTarget4.Tables["Table1"].Columns.Contains("NewColumn"),false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge MissingSchemaAction.Ignore - Table");
			Compare(dsTarget4.Tables.Contains("NewTable"),false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		#endregion
	}
}
}