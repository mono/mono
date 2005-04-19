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
	[TestFixture] public class DataSet_AcceptChanges : GHTBase
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
			DataSet_AcceptChanges tc = new DataSet_AcceptChanges();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataSet_AcceptChanges");
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
	
			DataSet ds = new DataSet();
			DataTable dtP = GHTUtils.DataProvider.CreateParentDataTable();
			DataTable dtC = GHTUtils.DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtP);
			ds.Tables.Add(dtC);
			ds.Relations.Add(new DataRelation("myRelation",dtP.Columns[0],dtC.Columns[0]));
	
			//create changes
			dtP.Rows[0][0] = "70"; 
			dtP.Rows[1].Delete();
			dtP.Rows.Add(new object[] {9,"string1","string2"});
        		
			try
			{
				BeginCase("AcceptChanges");
				ds.AcceptChanges();
				Compare(dtP.GetChanges(),null);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

		
			//read only exception
			dtP.Columns[0].ReadOnly = true;
			try
			{
				BeginCase("check ReadOnlyException ");
				try
				{
					dtP.Rows[0][0] = 99;
				}
				catch (ReadOnlyException ex) {exp=ex;}
				Compare(exp.GetType() ,typeof(ReadOnlyException));
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check invoke AcceptChanges ");
				try
				{
					ds.AcceptChanges();
				}
				catch (Exception ex) {exp=ex;}
				Compare(exp==null,true);
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}		
		}
	}
}
