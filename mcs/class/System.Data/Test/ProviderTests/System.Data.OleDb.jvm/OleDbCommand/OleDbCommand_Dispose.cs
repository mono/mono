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


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
[TestFixture]
public class OleDbCommand_Dispose : GHTBase
{
	public static void Main()
	{
		OleDbCommand_Dispose tc = new OleDbCommand_Dispose();
		Exception exp = null;
		try
		{
			tc.BeginTest("OleDBCommand_Dispose");
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


	[Test]
	public void run()
	{
		Exception exp = null;

		//OleDbConnection  con = null;

		//this test was added due to a request from Oved:
		//this bug occur on all databases (SQL,Oracle,DB2)
		OleDbCommand DbCommand = null;
		OleDbConnection Connect = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
		Connect.Open();
		DbCommand = new OleDbCommand("SELECT * FROM Customers", Connect);
		OleDbDataReader  DbReader  = DbCommand.ExecuteReader();

		BeginCase("Check DataReader.IsClosed - before dispose");
		try
		{
			Compare(DbReader.IsClosed,false); //.Net=false, GH=false
		}
		catch (Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}


		BeginCase("Check DataReader.IsClosed - after dispose");
		try
		{
			DbCommand.Dispose();
			Compare(DbReader.IsClosed,false); //.Net=false, GH=true
		}
		catch (Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}


	}
}



}
