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
[TestFixture] public class DataRow_GetParentRows_SD : GHTBase
{
	[Test] public void Main()
	{
		DataRow_GetParentRows_SD tc = new DataRow_GetParentRows_SD();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRow_GetParentRows_SD");
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
	
		DataRow drParent,drChild;
		DataRow[] drArrExcepted,drArrResult;
		DataTable dtChild,dtParent;
		DataSet ds = new DataSet();
		//Create tables
		dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		dtParent= GHTUtils.DataProvider.CreateParentDataTable(); 
		//Add tables to dataset
		ds.Tables.Add(dtChild);
		ds.Tables.Add(dtParent);
		//Add Relation
		DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
		ds.Relations.Add(dRel);
				
		//Create several copies of the first row
		drParent = dtParent.Rows[0];	//row[0] has versions: Default,Current,Original
		dtParent.ImportRow(drParent);	//row[1] has versions: Default,Current,Original
		dtParent.ImportRow(drParent);	//row[2] has versions: Default,Current,Original
		dtParent.ImportRow(drParent);	//row[3] has versions: Default,Current,Original
		dtParent.ImportRow(drParent);	//row[4] has versions: Default,Current,Original
		dtParent.ImportRow(drParent);	//row[5] has versions: Default,Current,Original
		dtParent.AcceptChanges();
				
		//Get the first child row for drParent
		drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];
				

		DataRow[] drTemp = dtParent.Select("ParentId=" + drParent["ParentId"]);
		//				Console.WriteLine("********");
		//				foreach (DataRow d in drTemp)
		//				{
		//					CheckRowVersion(d);
		//				}
		drTemp[0].BeginEdit();
		drTemp[0]["String1"] = "NewValue"; //row now has versions: Proposed,Current,Original,Default
		drTemp[1].BeginEdit();
		drTemp[1]["String1"] = "NewValue"; //row now has versions: Proposed,Current,Original,Default

		//		Console.WriteLine("********");
		//		foreach (DataRow d in drTemp)
		//		{
		//			CheckRowVersion(d);
		//		}
		//		Console.WriteLine("********");


		try
		{
			base.BeginCase("Check DataRowVersion.Current");
			//Check DataRowVersion.Current 
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Current);
			base.Compare( drArrResult, drArrExcepted);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{base.EndCase(exp); exp = null;}
		

		try
		{
			//Check DataRowVersion.Current 
			base.BeginCase("Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Original");
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Original );
			base.Compare( drArrResult, drArrExcepted);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		

		try
		{
			//Check DataRowVersion.Default
			base.BeginCase("Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Default");
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Default  );
			base.Compare( drArrResult, drArrExcepted);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		

/* .Net don't work as expected
		try
		{
			//Check DataRowVersion.Proposed
			base.BeginCase("Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Proposed");
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.ModifiedCurrent);
			//drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.ModifiedOriginal );

			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Proposed  );
			base.Compare( drArrResult, drArrExcepted);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
*/		
		
			
	}

	private void CheckRowVersion(DataRow dr)
	{
		Console.WriteLine("");
		if (dr.HasVersion(DataRowVersion.Current)) Console.WriteLine("Has " + DataRowVersion.Current.ToString());
		if (dr.HasVersion(DataRowVersion.Default)) Console.WriteLine("Has " + DataRowVersion.Default.ToString());
		if (dr.HasVersion(DataRowVersion.Original)) Console.WriteLine("Has " + DataRowVersion.Original.ToString());
		if (dr.HasVersion(DataRowVersion.Proposed)) Console.WriteLine("Has " + DataRowVersion.Proposed.ToString());
	}

}
}