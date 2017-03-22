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
using System.Data.OleDb;

using MonoTests.System.Data.Utils;

using MonoTests.System.Data.Utils.Data;

using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
[TestFixture]
public class OleDbDataReader_GetInt64 : ADONetTesterClass 
{
	private Exception exp = null;
	private int testTypesInvocations;

	public static void Main()
	{
		OleDbDataReader_GetInt64 tc = new OleDbDataReader_GetInt64();
		Exception exp = null;
		try
		{
			tc.BeginTest("OleDbDataReader_GetInt64_I");
			tc.run();
		}
		catch(Exception ex)
		{
			tc.exp = ex;
		}
		finally	
		{
			tc.EndTest(tc.exp);
		}
	}

	[Test]
	public void run()
	{
		DoTestTypes(ConnectedDataProvider.GetSimpleDbTypesParameters());
		DoTestTypes(ConnectedDataProvider.GetExtendedDbTypesParameters());
	}
	public void DoTestTypes(DbTypeParametersCollection row)
	{
		testTypesInvocations++;
		exp = null;
		string rowId = "43972_" + this.testTypesInvocations.ToString();
		OleDbDataReader rdr = null;
		OleDbConnection con = null;
		try
		{
			row.ExecuteInsert(rowId);
			row.ExecuteSelectReader(rowId, out rdr, out con);
			while (rdr.Read())
			{
				//Run over all the columns in the result set row.
				//For each column, try to read it as a Int64.
				for (int i=0; i<row.Count; i++)
				{
					if (row[i].Value.GetType() == typeof(Int64)) //The value in the result set should be a Int64.
					{
						try
						{
							BeginCase(string.Format("Calling GetInt64() on a field of dbtype {0}", row[i].DbTypeName));
							Int64 retInt64 = rdr.GetInt64(i);
							Compare(row[i].Value, retInt64);
						}
						catch (Exception ex)
						{
							exp = ex;
						}
						finally
						{
							EndCase(exp);
							exp = null;
						}
					}
					else //The value in the result set should NOT be Int64. In this case an Invalid case exception should be thrown.
					{
						try
						{
							BeginCase(string.Format("Calling GetInt64() on a field of dbtype {0}", row[i].DbTypeName));
							Int64 retInt64 = rdr.GetInt64(i);
							ExpectedExceptionNotCaught("InvalidCastException");
						}
						catch (InvalidCastException ex)
						{
							ExpectedExceptionCaught(ex);
						}
						catch (Exception ex)
						{
							exp = ex;
						}
						finally
						{
							EndCase(exp);
							exp = null;
						}
					}
				}
			}
		}
		finally
		{
			row.ExecuteDelete(rowId);
			if ( (rdr != null) && (!rdr.IsClosed) )
			{
				rdr.Close();
			}
			if ( (con != null) && (con.State != ConnectionState.Closed) )
			{
				con.Close();
			}
		}
	}


}
	

}