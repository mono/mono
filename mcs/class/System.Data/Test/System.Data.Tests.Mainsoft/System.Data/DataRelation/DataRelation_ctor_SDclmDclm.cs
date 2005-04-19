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
[TestFixture] public class DataRelation_ctor_SDclmDclm : GHTBase
{
	[Test] public void Main()
	{
		DataRelation_ctor_SDclmDclm tc = new DataRelation_ctor_SDclmDclm();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRelation_ctor_SDclmDclm");
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
		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		DataTable dtParent = GHTUtils.DataProvider.CreateParentDataTable();
		ds.Tables.Add(dtParent);
		ds.Tables.Add(dtChild);

		DataRelation dRel;
		dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
		ds.Relations.Add(dRel);
		
        
		try
		{
			BeginCase("DataRelation - CTor");
			Compare(dRel == null ,false );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("DataRelation - parent Constraints");
			Compare(dtParent.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("DataRelation - child Constraints");
			Compare(dtChild.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("DataRelation - child relations");
			Compare(dtParent.ChildRelations[0] ,dRel);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("DataRelation - parent relations");
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
		
		try
		{
			BeginCase("DataRelation - parent UniqueConstraint");
			Compare(dtParent.Constraints[0].GetType() ,typeof(UniqueConstraint));
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("DataRelation - Child ForeignKeyConstraint");
			Compare(dtChild.Constraints[0].GetType() ,typeof(ForeignKeyConstraint));
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		
		ds.Relations.Clear();
		try
		{
			BeginCase("Remove DataRelation - Parent Constraints");
			Compare(dtParent.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Remove DataRelation - Child Constraints");
			Compare(dtChild.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Remove DataRelation - child relations");
			Compare(dtParent.ChildRelations.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Remove DataRelation - parent relations");
			Compare(dtChild.ParentRelations.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;} 

		//add relation which will create invalid constraint
		dtChild.Constraints.Clear();
		dtParent.Constraints.Clear();
		//add duplicated row
		dtParent.Rows.Add(dtParent.Rows[0].ItemArray); 
		dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);


		try
		{
			BeginCase("Add relation which will create invalid constraint");
			exp = new Exception();
			try
			{
					ds.Relations.Add(dRel);
			}
			catch (ArgumentException ex) {exp=ex;}
			Compare(exp.GetType().FullName ,typeof(ArgumentException).FullName );
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;} 

		



	
	}
}
}