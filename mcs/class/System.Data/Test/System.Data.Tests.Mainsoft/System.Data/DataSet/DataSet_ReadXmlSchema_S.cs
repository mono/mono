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
[TestFixture] public class DataSet_ReadXmlSchema_S : GHTBase
{
	[Test] public void Main()
	{
		DataSet_ReadXmlSchema_S tc = new DataSet_ReadXmlSchema_S();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_ReadXmlSchema_S");
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
	
		DataSet ds1 = new DataSet();
		ds1.Tables.Add(GHTUtils.DataProvider.CreateParentDataTable());
		ds1.Tables.Add(GHTUtils.DataProvider.CreateChildDataTable());
		
		System.IO.MemoryStream ms = new System.IO.MemoryStream();
		//write xml  schema only
		ds1.WriteXmlSchema(ms);

		System.IO.MemoryStream ms1 = new System.IO.MemoryStream(ms.GetBuffer());  
		//copy schema
		DataSet ds2 = new DataSet();
		ds2.ReadXmlSchema(ms1);
	
		//check xml schema
		try
		{
			BeginCase("ReadXmlSchema - Tables count");
			Compare(ds1.Tables.Count ,ds2.Tables.Count );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ReadXmlSchema - Tables 0 Col count");
			Compare(ds2.Tables[0].Columns.Count ,ds1.Tables[0].Columns.Count  );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ReadXmlSchema - Tables 1 Col count");
			Compare(ds2.Tables[1].Columns.Count  ,ds1.Tables[1].Columns.Count  );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//check some colummns types
		try
		{
			BeginCase("ReadXmlSchema - Tables 0 Col type");
			Compare(ds2.Tables[0].Columns[0].GetType() ,ds1.Tables[0].Columns[0].GetType() );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ReadXmlSchema - Tables 1 Col type");
			Compare(ds2.Tables[1].Columns[3].GetType() ,ds1.Tables[1].Columns[3].GetType() );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		//check that no data exists
		try
		{
			BeginCase("ReadXmlSchema - Table 1 row count");
			Compare(ds2.Tables[0].Rows.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ReadXmlSchema - Table 2 row count");
			Compare(ds2.Tables[1].Rows.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


	}
}
}