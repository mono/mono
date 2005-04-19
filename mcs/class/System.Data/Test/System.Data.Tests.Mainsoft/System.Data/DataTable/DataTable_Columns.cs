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
[TestFixture] public class DataTable_Columns : GHTBase
{
	[Test] public void Main()
	{
		DataTable_Columns tc = new DataTable_Columns();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataTable_Columns");
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
		DataTable dtParent;
		DataColumnCollection dcl;
		dtParent= GHTUtils.DataProvider.CreateParentDataTable(); 
				
		dcl = dtParent.Columns; 
        
		try
		{
			base.BeginCase("Checking ColumnsCollection != null ");
			base.Compare(dcl == null  ,false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		try
		{
			base.BeginCase("Checking ColumnCollection Count  1");
			base.Compare(dcl.Count  ,6);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			base.BeginCase("Checking ColumnCollection Count 2");
			dtParent.Columns.Add(new DataColumn("Test"));
			base.Compare(dcl.Count  ,7);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking ColumnCollection - get columnn by different case");
			DataColumn tmp = dtParent.Columns["TEST"];
			base.Compare(tmp  ,dtParent.Columns["Test"]);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking ColumnCollection colummn name case sensetive");
			dtParent.Columns.Add(new DataColumn("test"));
			base.Compare(dcl.Count  ,8);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking ColumnCollection - get columnn by different case,ArgumentException");
			try
			{
				DataColumn tmp = dtParent.Columns["TEST"];
			}
			catch (ArgumentException ex) {exp=ex;}
			base.Compare(exp==null,false);
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


	}
}
}