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
public class OleDbDataReader_HasRows : GHTBase
{
	Exception exp;

	public static void Main()
	{
		OleDbDataReader_HasRows tc = new OleDbDataReader_HasRows();
		tc.exp = null;
		try
		{
			tc.BeginTest("OleDbDataReader_HasRows");
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

	public void run()
	{
		TestHasRowsTrue();
		TestHasRowsFalse();
	}

	[Test]
	public void TestHasRowsTrue()
	{
		BeginCase("Test HasRows = True");
		exp = null;
		string rowId = string.Format("43977_{0}", TestCaseNumber);
		OleDbConnection con = null;
		OleDbDataReader rdr = null;
		try
		{
			DbTypeParametersCollection row = ConnectedDataProvider.GetSimpleDbTypesParameters();
			row.ExecuteInsert(rowId);
			row.ExecuteSelectReader(rowId, out rdr, out con);
			Compare(rdr.HasRows, true);
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

	[Test]
	public void TestHasRowsFalse()
	{
		BeginCase("Test HasRows = False");
		exp = null;
		string rowId = string.Format("43977_{0}", TestCaseNumber);
		OleDbConnection con = null;
		OleDbDataReader rdr = null;
		try
		{
			DbTypeParametersCollection row = ConnectedDataProvider.GetSimpleDbTypesParameters();
			row.ExecuteDelete(rowId);	//Make sure that a row with such ID does not exist.
			row.ExecuteSelectReader(rowId, out rdr, out con);
			Compare(rdr.HasRows, false);
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