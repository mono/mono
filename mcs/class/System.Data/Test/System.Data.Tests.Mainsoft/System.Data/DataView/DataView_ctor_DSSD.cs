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
[TestFixture]
public class DataView_ctor_DSSD : GHTBase
{
	[Test]
	[Category ("NotWorking")]
	public void Main()
	{
		DataView_ctor_DSSD tc = new DataView_ctor_DSSD();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataView_ctor_DSSD");
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

	public void run()
	{
		Exception exp = null;
		DataView dv = null; 
		DataTable dt = new DataTable("myTable");

		bool ExceptionCaught = false;

		try
		{
			BeginCase("ctor - missing column CutomerID Exception");
			try
			{
				//exception: System.Data.EvaluateException: Cannot find column [CustomerId]
				dv = new DataView(dt,"CustomerId > 100","Age",DataViewRowState.Added );
			}
			catch (System.Data.EvaluateException ex)
			{
				exp = ex;
				ExceptionCaught = true;
			}
			Compare(ExceptionCaught,true);
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

		dt.Columns.Add(new DataColumn("CustomerId"));

		try
		{
			BeginCase("ctor - missing column Age Exception");
			try
			{
				//exception: System.Data.EvaluateException: Cannot find column [Age]
				dv = new DataView(dt,"CustomerId > 100","Age",DataViewRowState.Added );
			}
			catch (IndexOutOfRangeException ex)
			{
				exp = ex;
			}
			Compare(exp.GetType().FullName ,typeof(IndexOutOfRangeException).FullName);
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
		dt.Columns.Add(new DataColumn("Age"));

		try
		{
			BeginCase("ctor");
			dv = new DataView(dt,"CustomerId > 100","Age",DataViewRowState.Added );
			Compare(dv == null  ,false );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ctor - table");
			Compare(dv.Table  ,dt );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ctor - RowFilter");
			Compare(dv.RowFilter ,"CustomerId > 100" );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ctor - Sort");
			Compare(dv.Sort,"Age" );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ctor - RowStateFilter ");
			Compare(dv.RowStateFilter ,DataViewRowState.Added );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	}
}
}
