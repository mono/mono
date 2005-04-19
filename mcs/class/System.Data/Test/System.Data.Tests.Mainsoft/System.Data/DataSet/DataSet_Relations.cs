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
	[TestFixture] public class DataSet_Relations : GHTBase
	{
		[Test] public void Main()
		{
			DataSet_Relations tc = new DataSet_Relations();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataSet_Relations");
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
				
			DataTable dtChild1,dtChild2,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild1 = GHTUtils.DataProvider.CreateChildDataTable();
			dtChild1.TableName = "Child";
			dtChild2 = GHTUtils.DataProvider.CreateChildDataTable();
			dtChild2.TableName = "CHILD";
			dtParent= GHTUtils.DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild1);
			ds.Tables.Add(dtChild2);
		
			ds.Tables.Add(dtParent);

			DataRelation drl = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild1.Columns["ParentId"]);
			DataRelation drl1 = new DataRelation("Parent-CHILD",dtParent.Columns["ParentId"],dtChild2.Columns["ParentId"]);

			try
			{
				base.BeginCase("Checking Relations - default value");
				//Check default
				base.Compare(ds.Relations.Count  ,0);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			ds.Relations.Add(drl);
		        
			try
			{
				base.BeginCase("Checking Relations Count");
				base.Compare(ds.Relations.Count  ,1);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				base.BeginCase("Checking Relations Value");
				base.Compare(ds.Relations[0] ,drl);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				base.BeginCase("Checking Relations - get by name");
				base.Compare(ds.Relations["Parent-Child"] ,drl);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				base.BeginCase("Checking Relations - get by name case sensetive");
				base.Compare(ds.Relations["PARENT-CHILD"] ,drl);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}


			try
			{
				base.BeginCase("Checking Relations Count 2");
				ds.Relations.Add(drl1);
				base.Compare(ds.Relations.Count  ,2);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				base.BeginCase("Checking Relations - get by name case sensetive,ArgumentException");
				try
				{
					DataRelation tmp = ds.Relations["PARENT-CHILD"];
				}
				catch (ArgumentException ex) {exp = ex;}
				base.Compare(exp==null ,false);
				exp=null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
		}
	}

}