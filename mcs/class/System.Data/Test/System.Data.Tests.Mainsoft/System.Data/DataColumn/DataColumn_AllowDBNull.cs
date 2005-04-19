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
[TestFixture] public class DataColumn_AllowDBNull : GHTBase
{
	[Test] public void Main()
	{
		DataColumn_AllowDBNull tc = new DataColumn_AllowDBNull();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataColumn_AllowDBNull");
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
		DataTable dt = new DataTable();
		DataColumn dc;
		dc = new DataColumn("ColName",typeof(int));
		dc.DefaultValue = DBNull.Value;
		dt.Columns.Add(dc);
		dc.AutoIncrement=false;
                
		try
		{
			BeginCase("Checking default value (True)");
			Compare( dc.AllowDBNull,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		//adding new row with null value
		try
		{
			BeginCase("AllowDBNull=true - adding new row with null value");
			dt.Rows.Add(dt.NewRow());
			Compare(dt.Rows[0][0],DBNull.Value );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			BeginCase("set AllowDBNull=false ");
			try
			{
				dc.AllowDBNull=false; //the exisiting row contains null value
			}
			catch (Exception ex) {exp = ex;}
			Compare(exp.GetType().FullName,typeof(DataException).FullName);
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dt.Rows.Clear();
		dc.AllowDBNull=false;

		//adding new row with null value
		try
		{
			BeginCase("AllowDBNull=false - adding new row with null value");
			try
			{
				dt.Rows.Add(dt.NewRow());
			}
			catch (Exception ex) {exp = ex;}
			Compare(exp.GetType().FullName,typeof(NoNullAllowedException).FullName);
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		

		dc.AutoIncrement=true;
		int iRowCount = dt.Rows.Count;
		//adding new row with null value,AutoIncrement=true
		try
		{
			BeginCase("AllowDBNull=false,AutoIncrement=true - adding new row with null value");
			dt.Rows.Add(dt.NewRow());
			Compare(iRowCount+1,dt.Rows.Count);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		

	}
}
}