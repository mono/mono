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
[TestFixture] public class DataView_RowFilter : GHTBase
{
	[Test] public void Main()
	{
		DataView_RowFilter tc = new DataView_RowFilter();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataView_RowFilter");
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

		//note: this test does not check all the possible row filter expression. this is done in DataTable.Select method.
		// this test also check DataView.Count property

		Exception exp = null;
		
		DataRowView[] drvResult = null;
		System.Collections.ArrayList al = new System.Collections.ArrayList();

		//create the source datatable
		DataTable dt = GHTUtils.DataProvider.CreateChildDataTable();

		//create the dataview for the table
		DataView dv = new DataView(dt);


		//-------------------------------------------------------------
		//Get excpected result 
		al.Clear();
		foreach (DataRow dr in dt.Rows ) 
		{
			if ((int)dr["ChildId"] == 1)
			{
				al.Add(dr);
			}
		}

		try
		{
			BeginCase("RowFilter = 'ChildId=1', check count");
			dv.RowFilter = "ChildId=1";
			Compare(dv.Count ,al.Count );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("RowFilter = 'ChildId=1', check rows");
			drvResult = new DataRowView[dv.Count];
			dv.CopyTo(drvResult,0);
			//check that the filterd rows exists
			bool Succeed = true;
			for (int i=0; i<drvResult.Length ; i++)
			{
				Succeed = al.Contains(drvResult[i].Row);
				if (!Succeed) break;
			}
			Compare(Succeed ,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		//-------------------------------------------------------------


		//-------------------------------------------------------------
		//Get excpected result 
		al.Clear();
		foreach (DataRow dr in dt.Rows ) 
			if ((int)dr["ChildId"] == 1 && dr["String1"].ToString() == "1-String1" ) 
				al.Add(dr);
		
		try
		{
			BeginCase("RowFilter - ChildId=1 and String1='1-String1'");
			dv.RowFilter = "ChildId=1 and String1='1-String1'";
			Compare(dv.Count ,al.Count );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("RowFilter = ChildId=1 and String1='1-String1', check rows");
			drvResult = new DataRowView[dv.Count];
			dv.CopyTo(drvResult,0);
			//check that the filterd rows exists
			bool Succeed = true;
			for (int i=0; i<drvResult.Length ; i++)
			{
				Succeed = al.Contains(drvResult[i].Row);
				if (!Succeed) break;
			}
			Compare(Succeed ,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		//-------------------------------------------------------------

		//EvaluateException
		try
		{
			BeginCase("RowFilter - check EvaluateException");
			try
			{
				dv.RowFilter = "Col=1";
			}
			catch (EvaluateException ex)
			{
				exp=ex;
			}
			Compare(exp.GetType().FullName  ,typeof(EvaluateException).FullName);
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//SyntaxErrorException 1
		try
		{
			BeginCase("RowFilter - check SyntaxErrorException 1");
			try
			{
				dv.RowFilter = "sum('something')";
			}
			catch (SyntaxErrorException ex)
			{
				exp=ex;
			}
			Compare(exp.GetType().FullName  ,typeof(SyntaxErrorException).FullName);
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//SyntaxErrorException 2
		try
		{
			BeginCase("RowFilter - check SyntaxErrorException 2");
			try
			{
				dv.RowFilter = "HH**!";
			}
			catch (SyntaxErrorException ex)
			{
				exp=ex;
			}
			Compare(exp.GetType().FullName  ,typeof(SyntaxErrorException).FullName);
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

	}
}
}