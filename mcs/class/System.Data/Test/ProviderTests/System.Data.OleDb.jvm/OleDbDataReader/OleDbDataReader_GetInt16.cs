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
public class OleDbDataReader_GetInt16 : ADONetTesterClass 
{
	private Exception exp = null;
	private int testTypesInvocations;

	public static void Main()
	{
		OleDbDataReader_GetInt16 tc = new OleDbDataReader_GetInt16();
		Exception exp = null;
		try
		{
			tc.BeginTest("OleDbDataReader_GetInt16_I");
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
		string rowId = "43971_" + this.testTypesInvocations.ToString();
		OleDbDataReader rdr = null;
		OleDbConnection con = null;
		try
		{
			row.ExecuteInsert(rowId);
			row.ExecuteSelectReader(rowId, out rdr, out con);
			while (rdr.Read())
			{
				//Run over all the columns in the result set row.
				//For each column, try to read it as a Int16.
				for (int i=0; i<row.Count; i++)
				{
					if (row[i].Value.GetType() == typeof(Int16)) //The value in the result set should be a Int16.
					{
						try
						{
							BeginCase(string.Format("Calling GetInt16() on a field of dbtype {0}", row[i].DbTypeName));
							Int16 retInt16 = rdr.GetInt16(i);
							Compare(row[i].Value, retInt16);
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
					else //The value in the result set should NOT be Int16. In this case an Invalid case exception should be thrown.
					{
						try
						{
							BeginCase(string.Format("Calling GetInt16() on a field of dbtype {0}", row[i].DbTypeName));
							Int16 retInt16 = rdr.GetInt16(i);
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