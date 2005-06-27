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
public class DataSet_ReadXml_Strm5 : GHTBase
{
		Exception m_exp = null;
	[Test]
	[Category ("NotWorking")]
	public void Main()
	{
		DataSet_ReadXml_Strm5 tc = new DataSet_ReadXml_Strm5();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_ReadXml_Strm5");
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
		string xmlData;
		string name;
		string expected;
		#region "TestCase 1 - Empty string"
		try 
		{
			BeginCase("Empty string");
			DataSet ds = new DataSet();
			System.IO.StringReader sr = new System.IO.StringReader (string.Empty);
			System.Xml.XmlTextReader xReader = new System.Xml.XmlTextReader(sr);
			try
			{
				ds.ReadXml (xReader);
			}
			catch (System.Xml.XmlException ex)
			{
				Compare(ex.GetType(), typeof(System.Xml.XmlException));
			}
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
		#endregion
		#region "TestCase 2 - Single element"
		name = "Single element";
		expected = "DataSet Name=a Tables count=0";
		xmlData = "<a>1</a>";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 3 - Nesting one level single element."
		name = "Nesting one level single element.";
		expected = "DataSet Name=NewDataSet Tables count=1 Table Name=a Rows count=1 Items count=1 1";
		xmlData = "<a><b>1</b></a>";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 4 - Nesting one level multiple elements."
		name = "Nesting one level multiple elements.";
		expected = "DataSet Name=NewDataSet Tables count=1 Table Name=a Rows count=1 Items count=3 bb cc dd";
		xmlData = "<a><b>bb</b><c>cc</c><d>dd</d></a>";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 5 - Nesting two levels single elements."
		name = "Nesting two levels single elements.";
		expected = "DataSet Name=a Tables count=1 Table Name=b Rows count=1 Items count=1 cc";
		xmlData = "<a><b><c>cc</c></b></a>";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 6 - Nesting two levels multiple elements."
		name = "Nesting two levels multiple elements.";
		expected = "DataSet Name=a Tables count=1 Table Name=b Rows count=1 Items count=2 cc dd";
		xmlData = string.Empty;
		xmlData += "<a>";
		xmlData += 		"<b>";
		xmlData += 			"<c>cc</c>";
		xmlData += 			"<d>dd</d>";
		xmlData += 		"</b>";
		xmlData += 	"</a>";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 7 - Nesting two levels multiple elements."
		name = "Nesting two levels multiple elements.";
		expected = "DataSet Name=a Tables count=2 Table Name=b Rows count=1 Items count=2 cc dd Table Name=e Rows count=1 Items count=2 cc dd";
		xmlData = string.Empty;
		xmlData += "<a>";
		xmlData += 		"<b>";
		xmlData += 			"<c>cc</c>";
		xmlData += 			"<d>dd</d>";
		xmlData += 		"</b>";
		xmlData += 		"<e>";
		xmlData += 			"<c>cc</c>";
		xmlData += 			"<d>dd</d>";
		xmlData += 		"</e>";
		xmlData += 	"</a>";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 8 - Nesting three levels single element."
		name = "Nesting three levels single element.";
		xmlData = string.Empty;
		xmlData += "<a>";
		xmlData += 		"<b>";
		xmlData += 			"<c>";
		xmlData += 				"<d>dd</d>";
		xmlData += 			"</c>";
		xmlData += 		"</b>";
		xmlData += 	"</a>";
		expected = "DataSet Name=a Tables count=2 Table Name=b Rows count=1 Items count=1 0 Table Name=c Rows count=1 Items count=2 0 dd";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 9 - Nesting three levels multiple elements."
		name = "Nesting three levels multiple elements.";
		xmlData = string.Empty;
		xmlData += "<a>";
		xmlData += 		"<b>";
		xmlData += 			"<c>";
		xmlData += 				"<d>dd</d>";
		xmlData += 				"<e>ee</e>";
		xmlData += 			"</c>";
		xmlData += 		"</b>";
		xmlData += 	"</a>";
		expected = "DataSet Name=a Tables count=2 Table Name=b Rows count=1 Items count=1 0 Table Name=c Rows count=1 Items count=3 0 dd ee";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 10 - Nesting three levels multiple elements."
		name = "Nesting three levels multiple elements.";
		xmlData = string.Empty;
		xmlData += "<a>";
		xmlData += 		"<b>";
		xmlData += 			"<c>";
		xmlData += 				"<d>dd</d>";
		xmlData += 				"<e>ee</e>";
		xmlData += 			"</c>";
		xmlData +=			"<f>ff</f>";
		xmlData += 		"</b>";
		xmlData += 	"</a>";
		expected = "DataSet Name=a Tables count=2 Table Name=b Rows count=1 Items count=2 0 ff Table Name=c Rows count=1 Items count=3 0 dd ee";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 11 - Nesting three levels multiple elements."
		name = "Nesting three levels multiple elements.";
		xmlData = string.Empty;
		xmlData += "<a>";
		xmlData += 		"<b>";
		xmlData += 			"<c>";
		xmlData += 				"<d>dd</d>";
		xmlData += 				"<e>ee</e>";
		xmlData += 			"</c>";
		xmlData +=			"<f>ff</f>";
		xmlData += 			"<g>";
		xmlData += 				"<h>hh</h>";
		xmlData += 				"<i>ii</i>";
		xmlData += 			"</g>";
		xmlData +=			"<j>jj</j>";
		xmlData += 		"</b>";
		xmlData += 	"</a>";
		expected = "DataSet Name=a Tables count=3 Table Name=b Rows count=1 Items count=3 0 ff jj Table Name=c Rows count=1 Items count=3 0 dd ee Table Name=g Rows count=1 Items count=3 0 hh ii";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 12 - Nesting three levels multiple elements."
		name = "Nesting three levels multiple elements.";
		xmlData = string.Empty;
		xmlData += "<a>";
		xmlData += 		"<b>";
		xmlData += 			"<c>";
		xmlData += 				"<d>dd</d>";
		xmlData += 				"<e>ee</e>";
		xmlData += 			"</c>";
		xmlData +=			"<f>ff</f>";
		xmlData += 		"</b>";
		xmlData += 		"<g>";
		xmlData += 			"<h>";
		xmlData += 				"<i>ii</i>";
		xmlData += 				"<j>jj</j>";
		xmlData += 			"</h>";
		xmlData +=			"<f>ff</f>";
		xmlData += 		"</g>";
		xmlData += 	"</a>";
		expected = "DataSet Name=a Tables count=4 Table Name=b Rows count=1 Items count=2 0 ff Table Name=c Rows count=1 Items count=3 0 dd ee Table Name=g Rows count=1 Items count=2 ff 0 Table Name=h Rows count=1 Items count=3 0 ii jj";
		PrivateTestCase(name, expected, xmlData);
		#endregion
		#region "TestCase 13 - Nesting three levels multiple elements."
		name = "Nesting three levels multiple elements.";
		xmlData = string.Empty;
		xmlData += "<a>";
		xmlData += 		"<b>";
		xmlData += 			"<c>";
		xmlData += 				"<d>dd</d>";
		xmlData += 				"<e>ee</e>";
		xmlData += 			"</c>";
		xmlData +=			"<f>ff</f>";
		xmlData += 			"<k>";
		xmlData += 				"<l>ll</l>";
		xmlData += 				"<m>mm</m>";
		xmlData += 			"</k>";
		xmlData +=			"<n>nn</n>";
		xmlData += 		"</b>";
		xmlData += 		"<g>";
		xmlData += 			"<h>";
		xmlData += 				"<i>ii</i>";
		xmlData += 				"<j>jj</j>";
		xmlData += 			"</h>";
		xmlData +=			"<o>oo</o>";
		xmlData += 		"</g>";
		xmlData += 	"</a>";
		expected = "DataSet Name=a Tables count=5 Table Name=b Rows count=1 Items count=3 0 ff nn Table Name=c Rows count=1 Items count=3 0 dd ee Table Name=k Rows count=1 Items count=3 0 ll mm Table Name=g Rows count=1 Items count=2 0 oo Table Name=h Rows count=1 Items count=3 0 ii jj";
		PrivateTestCase(name, expected, xmlData);
		#endregion

		#region "TestCase 14 - for Bug 2387 (System.Data.DataSet.ReadXml(..) - ArgumentException while reading specific XML)"

		name = "Specific XML - for Bug 2387";
		expected = "DataSet Name=PKRoot Tables count=2 Table Name=Content Rows count=4 Items count=2 0  Items count=2 1 103 Items count=2 2 123 Items count=2 3 252 Table Name=Cont Rows count=3 Items count=3 1 103 0 Items count=3 2 123 0 Items count=3 3 252 -4";
		xmlData = "<PKRoot><Content /><Content><ContentId>103</ContentId><Cont><ContentId>103</ContentId><ContentStatusId>0</ContentStatusId></Cont></Content><Content><ContentId>123</ContentId><Cont><ContentId>123</ContentId><ContentStatusId>0</ContentStatusId></Cont></Content><Content><ContentId>252</ContentId><Cont><ContentId>252</ContentId><ContentStatusId>-4</ContentStatusId></Cont></Content></PKRoot>";
		PrivateTestCase(name, expected, xmlData);

		#endregion
	}

	private void PrivateTestCase(string a_name, string a_expected, string a_xmlData)
	{
		string file_name = null;
		try {
			BeginCase(a_name);
			DataSet ds = new DataSet();
			System.IO.StringReader sr = new System.IO.StringReader(a_xmlData) ;
			System.Xml.XmlTextReader xReader = new System.Xml.XmlTextReader(sr) ;
			ds.ReadXml (xReader);
			file_name = System.IO.Path.GetTempFileName ();
			ds.WriteXml (file_name);
			Compare(this.dataSetDescription(ds), a_expected);
		} catch (Exception ex) {
			m_exp = ex;
		} finally {
			EndCase(m_exp);
			m_exp = null;
			if (file_name != null)
				System.IO.File.Delete (file_name);
		}
	}


	private string dataSetDescription(DataSet ds)
	{
		string desc = string.Empty;
		desc += "DataSet Name=" + ds.DataSetName;
		desc += " Tables count=" + ds.Tables.Count;
		foreach (DataTable dt in ds.Tables)
		{
			desc += " Table Name=" + dt.TableName;
			desc += " Rows count=" + dt.Rows.Count;

			string[] colNames = new string[dt.Columns.Count];
			for(int i = 0; i < dt.Columns.Count; i++)
				colNames[i] = dt.Columns[i].ColumnName;

			Array.Sort(colNames);

			foreach (DataRow dr in dt.Rows)
			{
				desc += " Items count=" + dr.ItemArray.Length;
				foreach (string name in colNames)
				{
					desc += " " + dr[name].ToString();
				}
			}
		}
		return desc;
	}

}

}
