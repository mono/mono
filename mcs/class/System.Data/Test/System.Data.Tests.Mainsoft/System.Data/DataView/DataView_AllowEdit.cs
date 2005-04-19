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
[TestFixture] public class DataView_AllowEdit : GHTBase
{
	[Test] public void Main()
	{
		DataView_AllowEdit tc = new DataView_AllowEdit();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataView_AllowEdit");
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
		DataView dv = new DataView(dt);
		
		try
		{
			BeginCase("AllowEdit - default value");
			Compare(dv.AllowEdit ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("AllowEdit - true");
			dv.AllowEdit = true;
			Compare(dv.AllowEdit , true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("AllowEdit - false");
			dv.AllowEdit = false;
			Compare(dv.AllowEdit , false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dv.AllowEdit=false;

		try
		{
			BeginCase("AllowEdit false - exception");
			try
			{
				dv[0][2] = "aaa";
			}
			catch(DataException ex)
			{
				exp=ex;
			}
			Compare(exp.GetType().FullName , typeof(DataException).FullName );
			exp=null;
			
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}	

		dv.AllowEdit=true;

		try
		{
			BeginCase("AllowEdit true- exception");
			dv[0][2] = "aaa";
			Compare(dv[0][2] ,"aaa");
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}	
	}
}
}