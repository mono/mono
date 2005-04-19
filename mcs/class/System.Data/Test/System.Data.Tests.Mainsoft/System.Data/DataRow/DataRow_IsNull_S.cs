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

using System;
using System.Data;

using GHTUtils;
using GHTUtils.Base;

using NUnit.Framework;

namespace tests.system_data_dll.System_Data
{
	[TestFixture] public class DataRow_IsNull_S : GHTBase
	{
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		public void TearDown()
		{
		}

		[Test] public void Main()
		{
			DataRow_IsNull_S tc = new DataRow_IsNull_S();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataRow_IsNull_S");
				tc.SetUp();
				tc.run();
				tc.TearDown();
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
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

#region --- assignment  ----
			try
			{
				BeginCase("IsNull_S 1");
				Compare(dr.IsNull("Col0"), false);
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
				BeginCase("IsNull_S 2");
				Compare(dr.IsNull("Col1"), true);
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
#endregion

#region --- bug 3124 ---

			try
			{
				BeginCase("IsNull_S 1");
				System.IO.MemoryStream st = new System.IO.MemoryStream();
				System.IO.StreamWriter sw = new System.IO.StreamWriter(st);
				sw.Write("<?xml version=\"1.0\" standalone=\"yes\"?><NewDataSet>");
				sw.Write("<Table><EmployeeNo>9</EmployeeNo></Table>");
				sw.Write("</NewDataSet>");
				sw.Flush();
				st.Position=0;
				DataSet ds = new DataSet();
				ds.ReadXml(st);
				//  Here we add the expression column
				ds.Tables[0].Columns.Add("ValueListValueMember", typeof(object), "EmployeeNo");
		
				foreach( DataRow row in ds.Tables[0].Rows )
				{
					// Console.WriteLine(row["ValueListValueMember"].ToString() + " " );
					if( row.IsNull("ValueListValueMember") == true )
						Compare("SubTest","Failed");
					else
						Compare("Passed","Passed");
				}

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
#endregion

		}
	}
}
