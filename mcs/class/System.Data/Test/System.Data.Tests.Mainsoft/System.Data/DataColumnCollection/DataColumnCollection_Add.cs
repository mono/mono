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
[TestFixture] public class DataColumnCollection_Add : GHTBase
{
	[Test] public void Main()
	{
		DataColumnCollection_Add tc = new DataColumnCollection_Add();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataColumnCollection_Add");
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

		DataColumn dc = null;
		DataTable dt = new DataTable();


		//----------------------------- check default --------------------
		dc = dt.Columns.Add();
		try
		{
			BeginCase("Add column 1");
			Compare(dc.ColumnName, "Column1");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		dc = dt.Columns.Add();
		try
		{
			BeginCase("Add column 2");
			Compare(dc.ColumnName, "Column2");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		dc = dt.Columns.Add();
		try
		{
			BeginCase("Add column 3");
			Compare(dc.ColumnName, "Column3");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		dc = dt.Columns.Add();
		try
		{
			BeginCase("Add column 4");
			Compare(dc.ColumnName, "Column4");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		dc = dt.Columns.Add();
		try
		{
			BeginCase("Add column 5");
			Compare(dc.ColumnName, "Column5");
			Compare(dt.Columns.Count,5);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}


		//----------------------------- check Add/Remove from begining --------------------
		dt = initTable();

		dt.Columns.Remove(dt.Columns[0]);
		dt.Columns.Remove(dt.Columns[0]);
		dt.Columns.Remove(dt.Columns[0]);

		try
		{
			BeginCase("check column 4 - remove - from begining");
			Compare(dt.Columns[0].ColumnName, "Column4");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 5 - remove - from begining");
			Compare(dt.Columns[1].ColumnName , "Column5");
			Compare(dt.Columns.Count,2);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		dt.Columns.Add();
		dt.Columns.Add();
		dt.Columns.Add();
		dt.Columns.Add();

		try
		{
			BeginCase("check column 0 - Add new  - from begining");
			Compare(dt.Columns[0].ColumnName , "Column4");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 1 - Add new - from begining");
			Compare(dt.Columns[1].ColumnName , "Column5");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 2 - Add new - from begining");
			Compare(dt.Columns[2].ColumnName , "Column6");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 3 - Add new - from begining");
			Compare(dt.Columns[3].ColumnName , "Column7");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 4 - Add new - from begining");
			Compare(dt.Columns[4].ColumnName , "Column8");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 5 - Add new - from begining");
			Compare(dt.Columns[5].ColumnName , "Column9");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		//----------------------------- check Add/Remove from middle --------------------

		dt = initTable();

		dt.Columns.Remove(dt.Columns[2]);
		dt.Columns.Remove(dt.Columns[2]);
		dt.Columns.Remove(dt.Columns[2]);

		try
		{
			BeginCase("check column 0 - remove - from Middle");
			Compare(dt.Columns[0].ColumnName, "Column1");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 1 - remove - from Middle");
			Compare(dt.Columns[1].ColumnName , "Column2");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		dt.Columns.Add();
		dt.Columns.Add();
		dt.Columns.Add();
		dt.Columns.Add();


		try
		{
			BeginCase("check column 0 - Add new  - from Middle");
			Compare(dt.Columns[0].ColumnName , "Column1");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 1 - Add new - from Middle");
			Compare(dt.Columns[1].ColumnName , "Column2");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 2 - Add new - from Middle");
			Compare(dt.Columns[2].ColumnName , "Column3");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 3 - Add new - from Middle");
			Compare(dt.Columns[3].ColumnName , "Column4");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 4 - Add new - from Middle");
			Compare(dt.Columns[4].ColumnName , "Column5");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 5 - Add new - from Middle");
			Compare(dt.Columns[5].ColumnName , "Column6");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}




		//----------------------------- check Add/Remove from end --------------------

		dt = initTable();

		dt.Columns.Remove(dt.Columns[4]);
		dt.Columns.Remove(dt.Columns[3]);
		dt.Columns.Remove(dt.Columns[2]);

		try
		{
			BeginCase("check column 0 - remove - from end");
			Compare(dt.Columns[0].ColumnName, "Column1");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 1 - remove - from end");
			Compare(dt.Columns[1].ColumnName , "Column2");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		dt.Columns.Add();
		dt.Columns.Add();
		dt.Columns.Add();
		dt.Columns.Add();


		try
		{
			BeginCase("check column 0 - Add new  - from end");
			Compare(dt.Columns[0].ColumnName , "Column1");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 1 - Add new - from end");
			Compare(dt.Columns[1].ColumnName , "Column2");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 2 - Add new - from end");
			Compare(dt.Columns[2].ColumnName , "Column3");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 3 - Add new - from end");
			Compare(dt.Columns[3].ColumnName , "Column4");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 4 - Add new - from end");
			Compare(dt.Columns[4].ColumnName , "Column5");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("check column 5 - Add new - from end");
			Compare(dt.Columns[5].ColumnName , "Column6");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}















        
	}


	private void Print(DataTable dt)
	{
		Console.WriteLine();
		foreach(DataColumn dc in dt.Columns)
		{
			Console.WriteLine(dc.Ordinal.ToString() + " " + dc.ColumnName);
		}
	}
	private DataTable initTable()
	   {
		DataTable dt = new DataTable();
		for (int i=0; i<5; i++)
		{
			dt.Columns.Add();
		}
		return dt;
   }
}
}