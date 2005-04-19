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
[TestFixture] public class DataSet_ReadXml_Strm4 : GHTBase
{

	private DataSet m_ds;
	private Exception m_exp;

	[Test] public void Main()
	{
		DataSet_ReadXml_Strm4 tc = new DataSet_ReadXml_Strm4();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_ReadXml_Strm4");
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
		m_exp = null;
		m_ds = new DataSet("Stocks");
		string input = string.Empty;
		System.IO.StringReader sr;

		input += "<?xml version=\"1.0\"?>";
		input += "<Stocks>";
		input += "		<Stock name=\"MSFT\">";
		input += "			<Company name=\"Microsoft Corp.\" />";
		input += "			<Company name=\"General Electric\"/>";
		input += "			<Price type=\"high\">";
		input += "				<Value>10.0</Value>";
		input += "				<Date>01/20/2000</Date>";
		input += "			</Price>";
		input += "			<Price type=\"low\">";
		input += "				<Value>1.0</Value>";
		input += "				<Date>03/21/2002</Date>";
		input += "			</Price>";
		input += "			<Price type=\"current\">";
		input += "				<Value>3.0</Value>";
		input += "				<Date>TODAY</Date>";
		input += "			</Price>";
		input += "		</Stock>";
		input += "		<Stock name=\"GE\">";
		input += "			<Company name=\"GE company\"/>";
		input += "			<Price type=\"high\">";
		input += "				<Value>22.23</Value>";
		input += "				<Date>02/12/2001</Date>";
		input += "			</Price>";
		input += "			<Price type=\"low\">";
		input += "				<Value>1.97</Value>";
		input += "				<Date>04/20/2003</Date>";
		input += "			</Price>";
		input += "			<Price type=\"current\">";
		input += "				<Value>3.0</Value>";
		input += "				<Date>TODAY</Date>";
		input += "			</Price>";
		input += "		</Stock>";
		input += "		<Stock name=\"Intel\">";
		input += "			<Company name=\"Intel Corp.\"/>";
		input += "			<Company name=\"Test1\" />";
		input += "			<Company name=\"Test2\"/>";
		input += "			<Price type=\"high\">";
		input += "				<Value>15.0</Value>";
		input += "				<Date>01/25/2000</Date>";
		input += "			</Price>";
		input += "			<Price type=\"low\">";
		input += "				<Value>1.0</Value>";
		input += "				<Date>03/23/2002</Date>";
		input += "			</Price>";
		input += "			<Price type=\"current\">";
		input += "				<Value>3.0</Value>";
		input += "				<Date>TODAY</Date>";
		input += "			</Price>";
		input += "		</Stock>";
		input += "		<Stock name=\"Mainsoft\">";
		input += "			<Company name=\"Mainsoft Corp.\"/>";
		input += "			<Price type=\"high\">";
		input += "				<Value>30.0</Value>";
		input += "				<Date>01/26/2000</Date>";
		input += "			</Price>";
		input += "			<Price type=\"low\">";
		input += "				<Value>1.0</Value>";
		input += "				<Date>03/26/2002</Date>";
		input += "			</Price>";
		input += "			<Price type=\"current\">";
		input += "				<Value>27.0</Value>";
		input += "				<Date>TODAY</Date>";
		input += "			</Price>";
		input += "		</Stock>";
		input += "</Stocks>";
		
		sr = new System.IO.StringReader(input);
		m_ds.EnforceConstraints = true;
		m_ds.ReadXml(sr);
		this.privateTestCase("TestCase 1", "Company", "name='Microsoft Corp.'", "Stock", "name='MSFT'");
		this.privateTestCase("TestCase 2", "Company", "name='General Electric'", "Stock", "name='MSFT'");
		this.privateTestCase("TestCase 3", "Price", "Date='01/20/2000'", "Stock", "name='MSFT'");
		this.privateTestCase("TestCase 4", "Price", "Date='03/21/2002'", "Stock", "name='MSFT'");
		this.privateTestCase("TestCase 5", "Company", "name='GE company'", "Stock", "name='GE'");
		this.privateTestCase("TestCase 6", "Price", "Date='02/12/2001'", "Stock", "name='GE'");
		this.privateTestCase("TestCase 7", "Price", "Date='04/20/2003'", "Stock", "name='GE'");
		this.privateTestCase("TestCase 8", "Company", "name='Intel Corp.'", "Stock", "name='Intel'");
		this.privateTestCase("TestCase 9", "Company", "name='Test1'", "Stock", "name='Intel'");
		this.privateTestCase("TestCase 10", "Company", "name='Test2'", "Stock", "name='Intel'");
		this.privateTestCase("TestCase 11", "Price", "Date='01/25/2000'", "Stock", "name='Intel'");
		this.privateTestCase("TestCase 12", "Price", "Date='03/23/2002'", "Stock", "name='Intel'");
		this.privateTestCase("TestCase 13", "Company", "name='Mainsoft Corp.'", "Stock", "name='Mainsoft'");
		this.privateTestCase("TestCase 12", "Price", "Date='01/26/2000'", "Stock", "name='Mainsoft'");
		this.privateTestCase("TestCase 12", "Price", "Date='03/26/2002'", "Stock", "name='Mainsoft'");
	}

	private void privateTestCase(string name, string toTable, string toTestSelect, string toCompareTable, string toCompareSelect)
	{
		try
		{
			BeginCase(name);
			DataRow drToTest = m_ds.Tables[toTable].Select(toTestSelect)[0];
			DataRow drToCompare = m_ds.Tables[toCompareTable].Select(toCompareSelect)[0];
			Compare(m_ds.Tables[toCompareTable].Select(toCompareSelect)[0]["Stock_Id"], m_ds.Tables[toTable].Select(toTestSelect)[0]["Stock_Id"]);
		} 
		catch(Exception ex)
		{
			m_exp = ex;
		}
		finally
		{
			EndCase(m_exp);
			m_exp = null;
		}
	}
}
}
