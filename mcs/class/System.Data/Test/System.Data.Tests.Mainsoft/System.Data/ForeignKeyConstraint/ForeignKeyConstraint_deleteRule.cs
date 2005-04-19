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
[TestFixture] public class ForeignKeyConstraint_deleteRule : GHTBase
{
	[Test] public void Main()
	{
		ForeignKeyConstraint_deleteRule tc = new ForeignKeyConstraint_deleteRule();
		Exception exp = null;
		try
		{
			tc.BeginTest("ForeignKeyConstraint_deleteRule");
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
		DataSet ds = new DataSet();
		DataTable dtParent = GHTUtils.DataProvider.CreateParentDataTable();
		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		ds.Tables.Add(dtParent);
		ds.Tables.Add(dtChild);
		dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
		ds.EnforceConstraints = true;

		ForeignKeyConstraint fc = null;
		fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);


		//checking default
		try
		{
			BeginCase("Default");
			Compare(fc.DeleteRule ,Rule.Cascade );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		//checking set/get
		foreach (Rule rule in Enum.GetValues(typeof(Rule)))
		{
			try
			{
				BeginCase("Set/Get - " + rule.ToString());
				fc.DeleteRule = rule;
				Compare(fc.DeleteRule ,rule);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		dtChild.Constraints.Add(fc);

		//checking delete rule

		try
		{
			BeginCase("Rule = None, Delete Exception");
			fc.DeleteRule = Rule.None;
			//Exception = "Cannot delete this row because constraints are enforced on relation Constraint1, and deleting this row will strand child rows."
			try
			{
				dtParent.Rows.Find(1).Delete();
			}
			catch (Exception e)
			{
				Compare(e.GetType(),typeof(System.Data.InvalidConstraintException));
			}
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Rule = None, Delete succeed");
			fc.DeleteRule = Rule.None;
			foreach (DataRow dr in dtChild.Select("ParentId = 1"))
				dr.Delete();
			dtParent.Rows.Find(1).Delete();
			Compare(dtParent.Select("ParentId=1").Length ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Rule = Cascade");
			fc.DeleteRule = Rule.Cascade;
			dtParent.Rows.Find(2).Delete();
			Compare(dtChild.Select("ParentId=2").Length ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase("Rule = SetNull");
					DataSet ds1 = new DataSet();
					ds1.Tables.Add(DataProvider.CreateParentDataTable());
					ds1.Tables.Add(DataProvider.CreateChildDataTable());

					ForeignKeyConstraint fc1 = new ForeignKeyConstraint(ds1.Tables[0].Columns[0],ds1.Tables[1].Columns[1]); 
					fc1.DeleteRule = Rule.SetNull;
					ds1.Tables[1].Constraints.Add(fc1);

					
					Compare(ds1.Tables[1].Select("ChildId is null").Length,0);

					ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};
					ds1.Tables[0].Rows.Find(3).Delete();

					ds1.Tables[0].AcceptChanges();
					ds1.Tables[1].AcceptChanges();	
					
					DataRow[] arr =  ds1.Tables[1].Select("ChildId is null");

					/*foreach (DataRow dr in arr)
					{
						Compare(dr["ChildId"],null);
					}*/

					Compare(arr.Length ,4);

				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
		
				try
				{
					BeginCase("Rule = SetDefault");
					//fc.DeleteRule = Rule.SetDefault;
					DataSet ds1 = new DataSet();
					ds1.Tables.Add(DataProvider.CreateParentDataTable());
					ds1.Tables.Add(DataProvider.CreateChildDataTable());

					ForeignKeyConstraint fc1 = new ForeignKeyConstraint(ds1.Tables[0].Columns[0],ds1.Tables[1].Columns[1]); 
					fc1.DeleteRule = Rule.SetDefault;
					ds1.Tables[1].Constraints.Add(fc1);
					ds1.Tables[1].Columns[1].DefaultValue="777";

					//Add new row  --> in order to apply the forigen key rules
					DataRow dr = ds1.Tables[0].NewRow();
					dr["ParentId"] = 777;
					ds1.Tables[0].Rows.Add(dr);
					
					ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};
					ds1.Tables[0].Rows.Find(3).Delete();
					Compare(ds1.Tables[1].Select("ChildId=777").Length  ,4);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

	}
}
}