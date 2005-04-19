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
[TestFixture] public class NoNullAllowedException_Generate : GHTBase
{
	[Test] public void Main()
	{
		NoNullAllowedException_Generate tc = new NoNullAllowedException_Generate();
		Exception exp = null;
		try
		{
			tc.BeginTest("NoNullAllowedException");
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
		Exception tmpEx = new Exception() ;

		DataTable tbl = GHTUtils.DataProvider.CreateParentDataTable();

		// ----------- check with columnn type int -----------------------
		tbl.Columns[0].AllowDBNull = false;

		//add new row with null value
		try
		{
			BeginCase("NoNullAllowedException - Add Row");
			try
			{
				tbl.Rows.Add(new object[] {null,"value","value",new DateTime(0),0.5,true});
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//add new row with DBNull value
		try
		{
			BeginCase("NoNullAllowedException - Add Row");
			try
			{
				tbl.Rows.Add(new object[] {DBNull.Value,"value","value",new DateTime(0),0.5,true});
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("NoNullAllowedException - ItemArray");
			try
			{
				tbl.Rows[0].ItemArray = new object[] {DBNull.Value,"value","value",new DateTime(0),0.5,true};
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("NoNullAllowedException - Add Row - LoadDataRow");
			try
			{
				tbl.LoadDataRow(new object[] {DBNull.Value,"value","value",new DateTime(0),0.5,true},true);
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("NoNullAllowedException - EndEdit");
			tbl.Rows[0].BeginEdit();
			tbl.Rows[0][0] = DBNull.Value ;
			try
			{
				tbl.Rows[0].EndEdit();
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}




		// ----------- add new column -----------------------
		tbl.Columns[0].AllowDBNull = true;
		tbl.Columns.Add(new DataColumn("bolCol",typeof(bool)));

		try
		{
			BeginCase("add new column");
			try
			{
				tbl.Columns[tbl.Columns.Count-1].AllowDBNull = false;
			}
			catch (DataException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(DataException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//clear table data in order to add the new column
		tbl.Rows.Clear();
		tbl.Columns[tbl.Columns.Count-1].AllowDBNull = false;
		tbl.Rows.Add(new object[] {99,"value","value",new DateTime(0),0.5,true,false}); //missing last value - will be null


		//add new row with null value
		try
		{
			BeginCase("NoNullAllowedException - Add Row");
			try
			{
				tbl.Rows.Add(new object[] {99,"value","value",new DateTime(0),0.5,true}); //missing last value - will be null
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//add new row with DBNull value
		try
		{
			BeginCase("NoNullAllowedException - Add Row");
			try
			{
				tbl.Rows.Add(new object[] {1,"value","value",new DateTime(0),0.5,true,DBNull.Value});
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("NoNullAllowedException - ItemArray");
			try
			{
				tbl.Rows[0].ItemArray = new object[] {77,"value","value",new DateTime(0),0.5,true,DBNull.Value };
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("NoNullAllowedException - Add Row - LoadDataRow");
			try
			{
				tbl.LoadDataRow(new object[] {66,"value","value",new DateTime(0),0.5,true},true);
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("NoNullAllowedException - EndEdit");
			tbl.Rows[0].BeginEdit();
			tbl.Rows[0][tbl.Columns.Count-1] = DBNull.Value ;
			try
			{
				tbl.Rows[0].EndEdit();
			}
			catch (NoNullAllowedException  ex)
			{
				tmpEx = ex;
			}
			base.Compare(tmpEx.GetType(),typeof(NoNullAllowedException));
			tmpEx = new  Exception();
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


	}
}
}