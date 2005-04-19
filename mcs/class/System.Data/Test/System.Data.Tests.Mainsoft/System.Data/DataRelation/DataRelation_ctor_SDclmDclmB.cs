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
	[TestFixture] public class DataRelation_ctor_SDclmDclmB : GHTBase
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

		[SetUp]
		public void S()
		{
		}

		[TearDown]
		public void T()
		{
		}

		[Test] public void Main()
		{
			DataRelation_ctor_SDclmDclmB tc = new DataRelation_ctor_SDclmDclmB();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataRelation_ctor_SDclmDclmB");
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
			DataRelation dRel;		
			DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable();
			DataTable dtParent = GHTUtils.DataProvider.CreateParentDataTable();

			DataSet ds = new DataSet();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			//parameter createConstraints = true

			bool createConstraints = true;
			for (int i=0; i<=1; i++)
			{
				if (i==0)
					createConstraints = false;
				else 
					createConstraints = true;

				ds.Relations.Clear();
				dtParent.Constraints.Clear();
				dtChild.Constraints.Clear();

				//add duplicated row
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray); 
				dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0],createConstraints);
				try
				{
					BeginCase("Add relation which will create invalid constraint,createConstraints=" + createConstraints.ToString());
					exp = new Exception();
					try
					{
						ds.Relations.Add(dRel);
					}
					catch (ArgumentException ex) {exp=ex;}
					if (createConstraints==true)
						Compare(exp.GetType().FullName ,typeof(ArgumentException).FullName);
					else
						Compare(exp.GetType().FullName ,typeof(Exception).FullName );
					exp=null;
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;} 

				dtParent.Rows.Remove(dtParent.Rows[dtParent.Rows.Count-1]);
				ds.Relations.Clear();
				dtParent.Constraints.Clear();
				dtChild.Constraints.Clear();
				dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0],createConstraints);
				ds.Relations.Add(dRel);
		        
				try
				{
					BeginCase("DataRelation - CTor,createConstraints=" + createConstraints.ToString());
					Compare(dRel == null ,false );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase("DataRelation - parent Constraints,createConstraints=" + createConstraints.ToString());
					Compare(dtParent.Constraints.Count ,i);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase("DataRelation - child Constraints,createConstraints=" + createConstraints.ToString());
					Compare(dtChild.Constraints.Count ,i);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase("DataRelation - child relations,createConstraints=" + createConstraints.ToString());
					Compare(dtParent.ChildRelations[0] ,dRel);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase("DataRelation - parent relations,createConstraints=" + createConstraints.ToString());
					Compare(dtChild.ParentRelations[0],dRel );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;} 

				try
				{
					BeginCase("DataRelation - name");
					Compare(dRel.RelationName ,"MyRelation" );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;} 
			}

		}
	}
}
