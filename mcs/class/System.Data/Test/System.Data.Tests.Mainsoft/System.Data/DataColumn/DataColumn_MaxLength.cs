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
	[TestFixture] public class DataColumn_MaxLength : GHTBase
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
			DataColumn_MaxLength tc = new DataColumn_MaxLength();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataColumn_MaxLength");
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
			DataColumn dc;
			dc = new DataColumn("ColName",typeof(string));
			//Checking default value (-1)
			try
			{
				BeginCase("MaxLength default");
				Compare(dc.MaxLength , (int)-1);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			//Cheking Set MaxValue
			dc.MaxLength = int.MaxValue ;
			//Checking Get MaxValue
			try
			{
				BeginCase("MaxLength MaxValue");
				Compare(dc.MaxLength ,int.MaxValue );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			//Cheking Set MinValue
			dc.MaxLength = int.MinValue  ;
			//Checking Get MinValue
			try
			{
				BeginCase("MaxLength MinValue");
				Compare(dc.MaxLength , int.MinValue);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}


			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("col",typeof(string)));
			dt.Columns[0].MaxLength = 5;
			dt.Rows.Add(new object[] {"a"});
        
			//MaxLength = 5
			try
			{
				BeginCase("MaxLength = 5");
				try
				{
					dt.Rows[0][0] = "123456";
				}
				catch (Exception ex)
				{
					exp = ex;
				}
				Compare(exp.GetType().FullName, typeof(System.ArgumentException).FullName);
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
		}
	}
}
