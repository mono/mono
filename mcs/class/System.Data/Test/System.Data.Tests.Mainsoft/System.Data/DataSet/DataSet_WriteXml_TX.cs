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
[TestFixture] public class DataSet_WriteXml_TX : GHTBase
{
	[Test] public void Main()
	{
		DataSet_WriteXml_TX tc = new DataSet_WriteXml_TX();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_WriteXml_TX");
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
		System.IO.StringReader sr = null;
		System.IO.StringWriter sw = null;

		try
		{
			BeginCase("ReadXml - DataSetOut");

			DataSet oDataset = new DataSet("DataSetOut");
			sw = new System.IO.StringWriter();
			oDataset.WriteXml(sw,System.Data.XmlWriteMode.WriteSchema);
			
			sr = new System.IO.StringReader(sw.GetStringBuilder().ToString());
			oDataset = new DataSet("DataSetOut");

			oDataset.ReadXml(sr);
			Compare(oDataset.Tables.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	
		{
			EndCase(exp);
			exp = null;
			sw.Close();
		}

	
	}
}
}