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
[TestFixture] public class DataRowView_CreateChildView_D : GHTBase
{
	[Test] public void Main()
	{
		DataRowView_CreateChildView_D tc = new DataRowView_CreateChildView_D();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRowView_CreateChildView_D");
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

		//create a dataset with two tables, with a DataRelation between them
		DataTable dtParent = GHTUtils.DataProvider.CreateParentDataTable();
		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		DataSet ds = new DataSet();
		ds.Tables.Add(dtParent);
		ds.Tables.Add(dtChild);
		DataRelation drel = new DataRelation("ParentChild",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
		ds.Relations.Add(drel);

		//DataView dvChild = null;
		DataView dvParent = new DataView(dtParent);

		DataView dvTmp1 = dvParent[0].CreateChildView(drel);
		DataView dvTmp2 = dvParent[3].CreateChildView(drel);

		try
		{
			BeginCase("ChildView != null");
			Compare(dvTmp1!=null,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Child view table = ChildTable");
			Compare(dvTmp1.Table ,dtChild );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ChildView1.Table = ChildView2.Table");
			Compare(dvTmp1.Table ,dvTmp2.Table);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		//the child dataview are different
		try
		{
			BeginCase("Child DataViews different ");
			Compare(dvTmp1.Equals(dvTmp2),false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}



	}
}

}