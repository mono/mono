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
[TestFixture] public class DataView_CopyTo_AI : GHTBase
{
	[Test] public void Main()
	{
		DataView_CopyTo_AI tc = new DataView_CopyTo_AI();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataView_CopyTo_AI");
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
	
		//create the source datatable
		DataTable dt = GHTUtils.DataProvider.CreateChildDataTable();

		//create the dataview for the table
		DataView dv = new DataView(dt);

		DataRowView[] drvExpected = null;
		DataRowView[] drvResult = null;

		// ------- Copy from Index=0
		drvExpected = new DataRowView[dv.Count];
		for (int i=0; i < dv.Count ;i++)
		{
			drvExpected[i] = dv[i];
		}

		drvResult = new DataRowView[dv.Count];
		try
		{
			BeginCase("CopyTo from index 0");
			dv.CopyTo(drvResult,0);
			Compare(drvExpected ,drvResult);
		} 
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
			exp = null;
		}

		// ------- Copy from Index=3
		drvExpected = new DataRowView[dv.Count+3];
		for (int i=0; i < dv.Count ;i++)
		{
			drvExpected[i+3] = dv[i];
		}

		drvResult = new DataRowView[dv.Count+3];
		try
		{
			BeginCase("CopyTo from index 3");
			dv.CopyTo(drvResult,3);
			Compare(drvExpected ,drvResult );
		} 
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
			exp = null;
		}

		// ------- Copy from Index=3,larger array
		drvExpected = new DataRowView[dv.Count+9];
		for (int i=0; i < dv.Count ;i++)
		{
			drvExpected[i+3] = dv[i];
		}

		drvResult = new DataRowView[dv.Count+9];
		try
		{
			BeginCase("CopyTo from index 3,larger array");
			dv.CopyTo(drvResult,3);
			Compare(drvExpected ,drvResult);
		} 
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
			exp = null;
		}

		// ------- CopyTo smaller array, check exception
		drvResult = new DataRowView[dv.Count-1];

		try
		{
			BeginCase("CopyTo smaller array, check exception");
			try
			{
				dv.CopyTo(drvResult,0);
			}
			catch (IndexOutOfRangeException ex)
			{
				exp=ex;
			}
			Compare(exp.GetType().FullName,typeof(IndexOutOfRangeException).FullName );
			exp=null;
		} 
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
			exp = null;
		}


	}
}
}