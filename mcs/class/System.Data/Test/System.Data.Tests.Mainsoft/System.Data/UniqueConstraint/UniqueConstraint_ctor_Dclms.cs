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
[TestFixture] public class UniqueConstraint_ctor_Dclms : GHTBase
{
	[Test] public void Main()
	{
		UniqueConstraint_ctor_Dclms tc = new UniqueConstraint_ctor_Dclms();
		Exception exp = null;
		try
		{
			tc.BeginTest("ConstraintException");
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
		Exception tmpEx = new Exception();
		DataTable dtParent = GHTUtils.DataProvider.CreateParentDataTable();
		
		UniqueConstraint uc = null;
		uc = new UniqueConstraint(new DataColumn[] {dtParent.Columns[0],dtParent.Columns[1]});

		try
		{
			BeginCase("Ctor - parent");
			Compare(uc == null ,false );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Ctor - add exisiting column");
			dtParent.Rows.Add(new object[] {99,"str1","str2"});
			dtParent.Constraints.Add(uc);
			try 
			{
				dtParent.Rows.Add(new object[] {99,"str1","str2"});
			}
			catch (Exception e)
			{
				tmpEx = e;
			}
			Compare(tmpEx.GetType().FullName  ,typeof(System.Data.ConstraintException).FullName  );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable(); 
		uc = new UniqueConstraint(new DataColumn[] {dtChild.Columns[0],dtChild.Columns[1]});
		dtChild.Constraints.Add(uc);

		try
		{
			BeginCase("Ctor - child");
			Compare(uc == null ,false );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		dtChild.Constraints.Clear();
		uc = new UniqueConstraint(new DataColumn[] {dtChild.Columns[1],dtChild.Columns[2]});
		
		//target columnn are not unnique, will throw an exception
		try
		{
			BeginCase("ArgumentException - child");
			try
			{
				dtChild.Constraints.Add(uc);        
			}
			catch (ArgumentException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(ArgumentException).FullName );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

	}
}
}