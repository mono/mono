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
[TestFixture] public class ReadOnlyException_Generate : GHTBase
{
	[Test] public void Main()
	{
		ReadOnlyException_Generate tc = new ReadOnlyException_Generate();
		Exception exp = null;
		try
		{
			tc.BeginTest("ReadOnlyException");
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
		Exception tmpEx = new Exception() ;

		DataTable tbl = GHTUtils.DataProvider.CreateParentDataTable();
		tbl.Columns[0].ReadOnly = true;

		//chaeck for int column
		try
		{
			BeginCase("ReadOnlyException - EndEdit");
			//tbl.Rows[0].BeginEdit();   // this throw an exception but according to MSDN it shouldn't !!!
			//tbl.Rows[0][0] = 99 ;
			try
			{
				tbl.Rows[0][0] = 99 ;
				//tbl.Rows[0].EndEdit();
			}
			catch (ReadOnlyException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(ReadOnlyException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			BeginCase("ReadOnlyException - ItemArray");
			try
			{
				tbl.Rows[0].ItemArray = new object[] {99,"value","value"};
			}
			catch (ReadOnlyException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(ReadOnlyException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		//chaeck for string column
		tbl.Columns[0].ReadOnly = false;
		tbl.Columns[1].ReadOnly = true;
		
		try
		{
			BeginCase("ReadOnlyException - EndEdit");
			//tbl.Rows[0].BeginEdit();   // this throw an exception but according to MSDN it shouldn't !!!
			//tbl.Rows[0][0] = 99 ;
			try
			{
				tbl.Rows[0][1] = "NewValue" ;
				//tbl.Rows[0].EndEdit();
			}
			catch (ReadOnlyException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(ReadOnlyException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ReadOnlyException - ItemArray");
			try
			{
				tbl.Rows[0].ItemArray = new object[] {99,"value","value"};
			}
			catch (ReadOnlyException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(ReadOnlyException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
	}
}
}