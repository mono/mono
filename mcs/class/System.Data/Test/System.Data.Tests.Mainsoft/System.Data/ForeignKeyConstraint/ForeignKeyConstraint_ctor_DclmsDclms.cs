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
[TestFixture] public class ForeignKeyConstraint_ctor_DclmsDclms : GHTBase
{
	[Test] public void Main()
	{
		ForeignKeyConstraint_ctor_DclmsDclms tc = new ForeignKeyConstraint_ctor_DclmsDclms();
		Exception exp = null;
		try
		{
			tc.BeginTest("ForeignKeyConstraint_ctor_DclmsDclms");
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


		DataTable dtParent = GHTUtils.DataProvider.CreateParentDataTable();
		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		DataSet ds = new DataSet();
		ds.Tables.Add(dtChild);
		ds.Tables.Add(dtParent);
		
		ForeignKeyConstraint fc = null;

		try
		{
			BeginCase("Ctor ArgumentException");
			try
			{
				fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0]} ,new DataColumn[] {dtChild.Columns[0],dtChild.Columns[1]});				
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			Compare(exp.GetType().FullName , typeof(ArgumentException).FullName );
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	

		fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0],dtParent.Columns[1]} ,new DataColumn[] {dtChild.Columns[0],dtChild.Columns[2]});				

		try
		{
			BeginCase("Add constraint to table - ArgumentException");
			try
			{
				dtChild.Constraints.Add(fc);
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			Compare(exp.GetType().FullName , typeof(ArgumentException).FullName );
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Child Table Constraints Count - two columnns");
			Compare(dtChild.Constraints.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Parent Table Constraints Count - two columnns");
			Compare(dtParent.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			BeginCase("DataSet relations Count");
			Compare(ds.Relations.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dtParent.Constraints.Clear();
		dtChild.Constraints.Clear();

		fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0]} ,new DataColumn[] {dtChild.Columns[0]});
		try
		{
			BeginCase("Ctor");
			Compare(fc == null ,false );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Child Table Constraints Count");
			Compare(dtChild.Constraints.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Parent Table Constraints Count");
			Compare(dtParent.Constraints.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			BeginCase("DataSet relations Count");
			Compare(ds.Relations.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dtChild.Constraints.Add(fc);

		try
		{
			BeginCase("Child Table Constraints Count, Add");
			Compare(dtChild.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Parent Table Constraints Count, Add");
			Compare(dtParent.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("DataSet relations Count, Add");
			Compare(ds.Relations.Count ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Parent Table Constraints type");
			Compare(dtParent.Constraints[0].GetType() ,typeof(UniqueConstraint));
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Parent Table Constraints type");
			Compare(dtChild.Constraints[0].GetType() ,typeof(ForeignKeyConstraint));
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Parent Table Primary key");
			Compare(dtParent.PrimaryKey.Length ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		
		dtChild.Constraints.Clear();
		dtParent.Constraints.Clear();
		ds.Relations.Add(new DataRelation("myRelation",dtParent.Columns[0],dtChild.Columns[0]));

		try
		{
			BeginCase("Relation - Child Table Constraints Count");
			Compare(dtChild.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Relation - Parent Table Constraints Count");
			Compare(dtParent.Constraints.Count ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Relation - Parent Table Constraints type");
			Compare(dtParent.Constraints[0].GetType() ,typeof(UniqueConstraint));
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Relation - Parent Table Constraints type");
			Compare(dtChild.Constraints[0].GetType() ,typeof(ForeignKeyConstraint));
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Relation - Parent Table Primary key");
			Compare(dtParent.PrimaryKey.Length ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}




	}
}
}