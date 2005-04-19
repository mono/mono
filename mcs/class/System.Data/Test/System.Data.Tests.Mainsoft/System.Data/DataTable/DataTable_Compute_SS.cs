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
[TestFixture] public class DataTable_Compute_SS : GHTBase
{
	[Test] public void Main()
	{
		DataTable_Compute_SS tc = new DataTable_Compute_SS();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataTable_Compute_SS");
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
		
		
		DataTable dt = GHTUtils.DataProvider.CreateChildDataTable();
		
		//Get expected
		DataRow[] drArr = dt.Select("ParentId=1");
		Int64 iExSum = 0;
		foreach (DataRow dr in drArr)
		{
			iExSum += (int)dr["ChildId"];
		}
		object objCompute=null;
		try
		{
			BeginCase("Compute - sum values");
			objCompute = dt.Compute("Sum(ChildId)","ParentId=1");
			Compare(Int64.Parse(iExSum.ToString()), Int64.Parse(objCompute.ToString()) );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Compute - sum type");
			Compare(objCompute.GetType().FullName, typeof(Int64).FullName );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//get expected
		double iExAvg = 0;
		drArr = dt.Select("ParentId=5");
		foreach (DataRow dr in drArr)
		{
			iExAvg += (double)dr["ChildDouble"];
		}
		iExAvg = iExAvg / drArr.Length;

		try
		{
			BeginCase("Compute - Avg value");
			objCompute = dt.Compute("Avg(ChildDouble)","ParentId=5");
			Compare(double.Parse(iExAvg.ToString()), double.Parse(objCompute.ToString()) );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Compute - Avg type");
			Compare(objCompute.GetType().FullName, typeof(double).FullName );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		

	}
}
}