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
[TestFixture] public class DataSet_ReadXml_Strm3 : GHTBase
{
	[Test] public void Main()
	{
		DataSet_ReadXml_Strm3 tc = new DataSet_ReadXml_Strm3();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_ReadXml_Strm3");
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
		DataSet ds = new DataSet("TestDataSet");
		string input = string.Empty;
		System.IO.StringReader sr;

		input += "<?xml version=\"1.0\" standalone=\"yes\"?>";
		input += "<Stocks><Stock name=\"MSFT\"><Company name=\"Microsoft Corp.\" /><Price type=\"high\"><Value>10.0</Value>";
		input += "<Date>01/20/2000</Date></Price><Price type=\"low\"><Value>10</Value><Date>03/21/2002</Date></Price>";
		input += "<Price type=\"current\"><Value>3.0</Value><Date>TODAY</Date></Price></Stock><Stock name=\"GE\">";
		input += "<Company name=\"General Electric\" /><Price type=\"high\"><Value>22.23</Value><Date>02/12/2001</Date></Price>";
		input += "<Price type=\"low\"><Value>1.97</Value><Date>04/20/2003</Date></Price><Price type=\"current\"><Value>3.0</Value>";
		input += "<Date>TODAY</Date></Price></Stock></Stocks>";
		sr = new System.IO.StringReader(input);
		ds.EnforceConstraints = false;
		ds.ReadXml(sr);

		//Test that all added columns have "Hidden" mapping type.
		try
		{
			BeginCase("StockTable.Stock_IdCol.ColumnMapping");
			Compare(ds.Tables["Stock"].Columns["Stock_Id"].ColumnMapping, MappingType.Hidden);
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

		try
		{
			BeginCase("CompanyTable.Stock_IdCol.ColumnMapping");
			Compare(ds.Tables["Company"].Columns["Stock_Id"].ColumnMapping, MappingType.Hidden);
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

		try
		{
			BeginCase("PriceTable.Stock_IdCol.ColumnMapping");
			Compare(ds.Tables["Price"].Columns["Stock_Id"].ColumnMapping, MappingType.Hidden);
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