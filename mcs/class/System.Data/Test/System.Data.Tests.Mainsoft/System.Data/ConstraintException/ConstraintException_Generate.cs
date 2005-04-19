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
[TestFixture] public class ConstraintException_Generate : GHTBase
{
	[Test] public void Main()
	{
		ConstraintException_Generate tc = new ConstraintException_Generate();
		Exception exp = null;
		try
		{
			tc.BeginTest("ConstraintException");
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
		Exception tmpEx = new Exception(); 

		DataTable dtParent= GHTUtils.DataProvider.CreateParentDataTable(); 
		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable(); 

		DataSet ds = new DataSet();
		ds.Tables.Add(dtChild);
		ds.Tables.Add(dtParent);
		


		//------ check UniqueConstraint ---------

		//create unique constraint
		UniqueConstraint uc; 
		
		//Column type = int
		uc = new UniqueConstraint(dtParent.Columns[0]); 
		dtParent.Constraints.Add(uc);
		try
		{
			BeginCase("UniqueConstraint Exception - Column type = int");
			try
			{
				//add exisiting value - will raise exception
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray);
			}
			catch (ConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(ConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Column type = DateTime
		dtParent.Constraints.Clear();
		uc = new UniqueConstraint(dtParent.Columns["ParentDateTime"]); 
		dtParent.Constraints.Add(uc);
		try
		{
			BeginCase("UniqueConstraint Exception - Column type = DateTime");
			try
			{
				//add exisiting value - will raise exception
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray);
			}
			catch (ConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(ConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Column type = double
		dtParent.Constraints.Clear();
		uc = new UniqueConstraint(dtParent.Columns["ParentDouble"]); 
		dtParent.Constraints.Add(uc);
		try
		{
			BeginCase("UniqueConstraint Exception - Column type = double");
			try
			{
				//add exisiting value - will raise exception
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray);
			}
			catch (ConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(ConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Column type = string
		dtParent.Constraints.Clear();
		uc = new UniqueConstraint(dtParent.Columns["String1"]); 
		dtParent.Constraints.Add(uc);
		try
		{
			BeginCase("UniqueConstraint Exception - Column type = String");
			try
			{
				//add exisiting value - will raise exception
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray);
			}
			catch (ConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(ConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Column type = string, ds.CaseSensitive = false;
		ds.CaseSensitive = false;

		dtParent.Constraints.Clear();
		uc = new UniqueConstraint(dtParent.Columns["String1"]); 
		dtParent.Constraints.Add(uc);
		DataRow dr = dtParent.NewRow();
		dr.ItemArray = dtParent.Rows[0].ItemArray ;
		dr["String1"] = dr["String1"].ToString().ToUpper();

		try
		{
			BeginCase("UniqueConstraint Exception - Column type = String, CaseSensitive = false;");
			try
			{
				dtParent.Rows.Add(dr);
			}
			catch (ConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(ConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Column type = string, ds.CaseSensitive = true;
		ds.CaseSensitive = true;

		dtParent.Constraints.Clear();
		uc = new UniqueConstraint(dtParent.Columns["String1"]); 
		dtParent.Constraints.Add(uc);

		try
		{
			BeginCase("UniqueConstraint Exception - Column type = String, CaseSensitive = true;");
			try
			{
				dtParent.Rows.Add(dr);
			}
			catch (ConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(Exception).FullName ); //no exception will raise
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Column type = string, ds.CaseSensitive = false;

		try
		{
			BeginCase("UniqueConstraint Exception - Column type = String, Enable CaseSensitive = true;");
			try
			{
				ds.CaseSensitive = false;
			}
			catch (ConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(ConstraintException).FullName ); //no exception will raise
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}		


		dtChild.Constraints.Add(new UniqueConstraint(new DataColumn[] {dtChild.Columns[0],dtChild.Columns[1]}));
		ds.EnforceConstraints = false;
		dtChild.Rows.Add(dtChild.Rows[0].ItemArray);

		try
		{
			BeginCase("UniqueConstraint Exception - ds.EnforceConstraints ");
			try
			{
				ds.EnforceConstraints = true;
			}
			catch (ConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(ConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	}

	}

}