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
	[TestFixture] public class DataRow_GetChildRows_DD : GHTBase
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
			DataRow_GetChildRows_DD tc = new DataRow_GetChildRows_DD();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataRow_GetChildRows_DD");
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
				
			DataRow drParent;
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
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];

		
			try
			{
				base.BeginCase("Teting: " + DateTime.Now.ToShortTimeString());
				//Get Excepted result
				drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
				//Get Result DataRowVersion.Current
				drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Current);
				base.Compare( drArrResult, drArrExcepted);
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
				base.BeginCase("Teting: DataRow.GetParentRows_D_D");
				//Get Excepted result
				drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
				//Get Result DataRowVersion.Current
				drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Original );
				base.Compare( drArrResult, drArrExcepted);
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
				base.BeginCase("Teting: DataRow.GetParentRows_D_D");
				//Get Excepted result, in this case Current = Default
				drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
				//Get Result DataRowVersion.Current
				drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Default  );
				base.Compare( drArrResult, drArrExcepted);
			} 
			catch(Exception ex)	{	exp = ex;	}
			finally {	EndCase(exp);	exp = null;	}

			try
			{
				base.BeginCase("Teting: DataRow.GetParentRows_D_D");
				drParent.BeginEdit();
				drParent["String1"] = "Value";
				//Get Excepted result
				drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
				//Get Result DataRowVersion.Current
				drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Proposed  );
				base.Compare( drArrResult, drArrExcepted);
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
