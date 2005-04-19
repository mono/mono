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
[TestFixture] public class DataSet_GetXml : GHTBase
{
	[Test] public void Main()
	{
		DataSet_GetXml tc = new DataSet_GetXml();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_GetXml");
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
	
		DataSet ds = new DataSet();
		ds.Namespace = "namespace"; //if we don't add namespace the test will fail because GH (by design) always add namespace
		DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();
		dt.Clear();
		dt.Rows.Add(new object[] {1,"Value1","Value2"});
		dt.Rows.Add(new object[] {2,"Value3","Value4"});
		dt.Rows.Add(new object[] {3,"Value5","Value5"});

		System.Text.StringBuilder resultXML = new System.Text.StringBuilder();

		resultXML.Append("<" + ds.DataSetName  + "xmlns=\"namespace\">");
				 
		resultXML.Append("<Parent>");
		resultXML.Append("<ParentId>1</ParentId>");
		resultXML.Append("<String1>Value1</String1>");
		resultXML.Append("<String2>Value2</String2>");
		resultXML.Append("</Parent>");
				 
		resultXML.Append("<Parent>");
		resultXML.Append("<ParentId>2</ParentId>");
		resultXML.Append("<String1>Value3</String1>");
		resultXML.Append("<String2>Value4</String2>");
		resultXML.Append("</Parent>");
				 
		resultXML.Append("<Parent>");
		resultXML.Append("<ParentId>3</ParentId>");
		resultXML.Append("<String1>Value5</String1>");
		resultXML.Append("<String2>Value5</String2>");
		resultXML.Append("</Parent>");
				 
		resultXML.Append("</" + ds.DataSetName  + ">");
		
		ds.Tables.Add(dt);
		string strXML = ds.GetXml();
		strXML = strXML.Replace(" ","");
		strXML = strXML.Replace("\t","");
		strXML = strXML.Replace("\n","");
		strXML = strXML.Replace("\r","");
		        
		try
		{
			BeginCase("GetXml");
			Compare(strXML ,resultXML.ToString() );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	}
}
}