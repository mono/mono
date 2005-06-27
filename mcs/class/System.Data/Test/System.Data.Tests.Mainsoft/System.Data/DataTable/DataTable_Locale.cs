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
public class DataTable_Locale : GHTBase
{
	[Test]
	[Category ("NotWorking")]
	public void Main()
	{
		DataTable_Locale tc = new DataTable_Locale();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataTable_Locale");
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
		DataSet ds = new DataSet("MyDataSet");
				
		dtParent= GHTUtils.DataProvider.CreateParentDataTable(); 
		ds.Tables.Add(dtParent);
		System.Globalization.CultureInfo culInfo = System.Globalization.CultureInfo.CurrentCulture ;
        
		try
		{
			base.BeginCase("Checking Locale default from system ");
			base.Compare(dtParent.Locale  ,culInfo);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking Locale default from dataset ");
			culInfo = new System.Globalization.CultureInfo("az-AZ-Cyrl"); // = Azeri (Cyrillic) - Azerbaijan
			ds.Locale = culInfo;
			base.Compare(dtParent.Locale ,culInfo );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			base.BeginCase("Checking Locale get/set ");
			culInfo = new System.Globalization.CultureInfo("ar-IQ"); // = Arabic - Iraq
			dtParent.Locale = culInfo ;
			base.Compare(dtParent.Locale ,culInfo );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	}
}
}
