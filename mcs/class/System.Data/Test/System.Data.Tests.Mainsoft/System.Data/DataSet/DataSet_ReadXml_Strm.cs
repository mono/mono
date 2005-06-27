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
public class DataSet_ReadXml_Strm : GHTBase
{
	[Test]
	[Category ("NotWorking")]
	public void Main()
	{
		DataSet_ReadXml_Strm tc = new DataSet_ReadXml_Strm();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_ReadXml_Strm");
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

		//add data to check GH bug of DataSet.ReadXml of empty strings
		ds1.Tables[1].Rows.Add(new object[] {7,1,string.Empty,string.Empty,new DateTime(2000,1,1,0,0,0,0),35});
		ds1.Tables[1].Rows.Add(new object[] {7,2," ","		",new DateTime(2000,1,1,0,0,0,0),35});
		ds1.Tables[1].Rows.Add(new object[] {7,3,"","",new DateTime(2000,1,1,0,0,0,0),35});


		System.IO.MemoryStream ms = new System.IO.MemoryStream();
		//write xml file, data only
		ds1.WriteXml(ms);

		//copy both data and schema
		DataSet ds2 = ds1.Copy();
		//clear the data
		ds2.Clear();

		ms.Position=0;
		ds2.ReadXml(ms);
		
	
		//check xml data
		try
		{
			BeginCase("ReadXml - Tables count");
			Compare(ds1.Tables.Count ,ds2.Tables.Count );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ReadXml - Table 1 row count");
			Compare(ds1.Tables[0].Rows.Count ,ds2.Tables[0].Rows.Count);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ReadXml - Table 2 row count");
			Compare(ds1.Tables[1].Rows.Count ,ds2.Tables[1].Rows.Count);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		ms.Close();
		

	}
}
}
