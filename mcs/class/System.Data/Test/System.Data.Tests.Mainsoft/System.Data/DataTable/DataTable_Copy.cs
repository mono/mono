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
	[TestFixture] public class DataTable_Copy : GHTBase
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
			DataTable_Copy tc = new DataTable_Copy();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataTable_Copy");
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
		
			DataTable dt1,dt2 = GHTUtils.DataProvider.CreateParentDataTable();
			dt2.Constraints.Add("Unique",dt2.Columns[0],true);
			dt2.Columns[0].DefaultValue=7;

			dt1 = dt2.Copy();

			for (int i=0; i<dt2.Constraints.Count; i++)
			{
				try
				{
					BeginCase(String.Format("Copy - Constraints[{0}]",i));
					Compare( dt1.Constraints[i].ConstraintName ,dt2.Constraints[i].ConstraintName );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
			}
		
			for (int i=0; i<dt2.Columns.Count; i++)
			{
				try
				{
					BeginCase(String.Format("Copy - Columns[{0}].ColumnName",i));
					Compare( dt1.Columns[i].ColumnName  ,dt2.Columns[i].ColumnName );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase(String.Format("Copy - Columns[{0}].DataType",i));
					Compare( dt1.Columns[i].DataType ,dt2.Columns[i].DataType );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
			}
        
			
			DataRow[] drArr1,drArr2;
			drArr1 = dt1.Select("");
			drArr2 = dt2.Select("");
			for (int i=0; i<drArr1.Length ; i++)
			{
				try
				{
					BeginCase(String.Format("Copy - Data [ParentId]{0}" ,i));
					Compare(drArr1[i]["ParentId"],drArr2[i]["ParentId"]);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
				try
				{
					BeginCase(String.Format("Copy - Data [String1]{0}" ,i));
					Compare(drArr1[i]["String1"],drArr2[i]["String1"]);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
				try
				{
					BeginCase(String.Format("Copy - Data [String2]{0}" ,i));
					Compare(drArr1[i]["String2"],drArr2[i]["String2"]);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
			}
						

		}
	}
}
