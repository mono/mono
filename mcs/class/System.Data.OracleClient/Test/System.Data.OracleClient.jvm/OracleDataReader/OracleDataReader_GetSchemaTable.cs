// 
// Copyright (c) 2006 Mainsoft Co.
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
using System.Data.OracleClient;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
[TestFixture]
class TestId13294 : GHTBase
{
	public static void Main()
	{
		TestId13294 tc = new TestId13294();
		Exception exp = null;
		try
		{
			tc.BeginTest("OracleDataReader_GetSchemaTable");
			tc.run();
		}
		catch(Exception ex){exp = ex;}
		finally	{tc.EndTest(exp);}
	}

	[Test]
	public void run()
	{
		Exception exp = null;

		
		OracleConnection con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
		con.Open();
		OracleCommand cmd = new OracleCommand("Select * From Orders", con);
		OracleDataReader rdr = cmd.ExecuteReader();
		DataTable tbl = rdr.GetSchemaTable();

		//check that all the columns properties (according to .Net) exists (GH give more properties)

		try
		{
			BeginCase("ColumnName");
				Compare(tbl.Columns.Contains("ColumnName"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ColumnOrdinal");
			Compare(tbl.Columns.Contains("ColumnOrdinal"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ColumnSize");
			Compare(tbl.Columns.Contains("ColumnSize"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("NumericPrecision");
			Compare(tbl.Columns.Contains("NumericPrecision"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("NumericScale");
			Compare(tbl.Columns.Contains("NumericScale"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("DataType");
			Compare(tbl.Columns.Contains("DataType"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ProviderType");
			Compare(tbl.Columns.Contains("ProviderType"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IsLong");
			Compare(tbl.Columns.Contains("IsLong"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("AllowDBNull");
			Compare(tbl.Columns.Contains("AllowDBNull"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IsReadOnly");
			Compare(tbl.Columns.Contains("IsReadOnly"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IsRowVersion");
			Compare(tbl.Columns.Contains("IsRowVersion"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IsUnique");
			Compare(tbl.Columns.Contains("IsUnique"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IsKey");
			Compare(tbl.Columns.Contains("IsKey"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IsAutoIncrement");
			Compare(tbl.Columns.Contains("IsAutoIncrement"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("BaseSchemaName");
			Compare(tbl.Columns.Contains("BaseSchemaName"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("BaseTableName");
			Compare(tbl.Columns.Contains("BaseTableName"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("BaseColumnName");
			Compare(tbl.Columns.Contains("BaseColumnName"),true );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}


		if (con.State == ConnectionState.Open) con.Close();
	
		}
	}


}