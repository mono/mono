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
[TestFixture] public class InvalidConstraintException_Generate : GHTBase
{
	[Test] public void Main()
	{
		InvalidConstraintException_Generate tc = new InvalidConstraintException_Generate();
		Exception exp = null;
		try
		{
			tc.BeginTest("InvalidConstraintException");
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
		DataTable dtParent;
		dtParent= GHTUtils.DataProvider.CreateParentDataTable(); 
		Exception tmpEx = new Exception();

		//------ check ForeignKeyConstraint  ---------
		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable(); 
		DataSet ds = new DataSet();
		ds.Tables.Add(dtChild);
		ds.Tables.Add(dtParent);

		ds.Relations.Add(new DataRelation("myRelation",dtParent.Columns[0],dtChild.Columns[0],true));
                
		//update to value which is not exists in Parent table
		try
		{
			BeginCase("InvalidConstraintException - update child row");
			try
			{
				dtChild.Rows[0]["ParentId"] = 99;
			}
			catch (InvalidConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(InvalidConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Add another relation to the same column of the existing relation in child table
		try
		{
			BeginCase("InvalidConstraintException - Add Relation Child");
			try
			{
				ds.Relations.Add(new DataRelation("test",dtParent.Columns[2],dtChild.Columns[0],true));
			}
			catch (InvalidConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(InvalidConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//          ?????????????????? should throw exception - according to MSDN 
		//		try
		//		{
		//			BeginCase("InvalidConstraintException - ");
		//			//ds.Relations.Clear();
		//			try
		//			{
		//				dtParent.Rows[0].GetParentRows("myRelation");
		//			}
		//			catch (InvalidConstraintException ex)
		//			{
		//				tmpEx = ex;
		//			}
		//			base.Compare(tmpEx.GetType(),typeof(InvalidConstraintException));
		//		}
		//		catch(Exception ex)	{exp = ex;}
		//		finally	{EndCase(exp); exp = null;}


		//Attempt to clear rows from parent table 
		try
		{
			BeginCase("InvalidConstraintException - RowsCollection.Clear");
			try
			{
				dtParent.Rows.Clear();
			}
			catch (InvalidConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(InvalidConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		//try to run commands on two different datasets
		DataSet ds1 = new DataSet();
		ds1.Tables.Add(dtParent.Copy());

		try
		{
			BeginCase("InvalidConstraintException - Add relation with two DataSets");
			try
			{
				ds.Relations.Add(new DataRelation("myRelation",ds1.Tables[0].Columns[0],dtChild.Columns[0],true));
			}
			catch (InvalidConstraintException ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType().FullName ,typeof(InvalidConstraintException).FullName );
			tmpEx = new Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}



	}		
	
	}

}