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
[TestFixture] public class DataTable_OnColumnChanged_D : GHTBase
{
	[Test] public void Main()
	{
		DataTable_OnColumnChanged_D tc = new DataTable_OnColumnChanged_D();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataTable_OnColumnChanged_D");
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

	private bool EventRaised ;
	private bool EventValues ;
	public void run()
	{
		Exception exp = null;
		ProtectedTestClass dt = new ProtectedTestClass(); 

		EventRaised = false;
		dt.OnColumnChanged_Test();
		try
		{
			BeginCase("OnColumnChanged Event 1");
			Compare(EventRaised ,false );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		EventRaised = false;
		EventValues = false;
		dt.ColumnChanged += new DataColumnChangeEventHandler(OnColumnChanged_Handler);
		dt.OnColumnChanged_Test();
		try
		{
			BeginCase("OnColumnChanged Event 2");
			Compare(EventRaised ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("OnColumnChanged Values");
			Compare(EventValues ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		dt.ColumnChanged -= new DataColumnChangeEventHandler(OnColumnChanged_Handler);

	}

	private void OnColumnChanged_Handler(Object sender,DataColumnChangeEventArgs e)
	{
		DataTable dt = (DataTable)sender;
		if ( (e.Column.Equals(dt.Columns["Value"])	)	&&
			(e.Row.Equals(dt.Rows[0])				)	&&
			(e.ProposedValue.Equals("NewValue"))	)
		{
			EventValues = true;
		}
		else
		{
			EventValues = false;
		}
		EventRaised = true;
	}

	class ProtectedTestClass : DataTable
	{
		public ProtectedTestClass() : base()
		{
			this.Columns.Add("Id",typeof(int));
			this.Columns.Add("Value",typeof(string));
			this.Rows.Add(new object[] {1,"one"});
			this.Rows.Add(new object[] {2,"two"});
			this.AcceptChanges();
		}
		public void OnColumnChanged_Test()
		{
			base.OnColumnChanged(new DataColumnChangeEventArgs(this.Rows[0],this.Columns["Value"],"NewValue")); 
		}
	}


}
}