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
using System.Data.OleDb ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
[TestFixture]
public class OleDbConnection_ChangeDatabase : ADONetTesterClass 
{
	public static void Main()
	{
		OleDbConnection_ChangeDatabase tc = new OleDbConnection_ChangeDatabase();
		Exception exp = null;
		try
		{
			tc.BeginTest("NoName");
			tc.run();
		}
		catch(Exception ex){exp = ex;}
		finally	{tc.EndTest(exp);}
	}

	[Test]
	public void run()
	{
		Exception exp = null;

		OleDbConnection con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
		con.Open();

		//Currently not running on Oracle/DB2: .Net-Failed, GH:Pass
		//not runnig on postgres because "The 'current catalog' property is not supported by the 'Mainsoft.JDBC.OLEDB.1' provider."
		if (ConnectedDataProvider.GetDbType(con) != DataBaseServer.Oracle  &&
			ConnectedDataProvider.GetDbType(con) != DataBaseServer.DB2 && 
			ConnectedDataProvider.GetDbType(con) != DataBaseServer.Sybase &&
			ConnectedDataProvider.GetDbType(con) != DataBaseServer.PostgreSQL)
		{
			try
			{
				BeginCase("Change DataBase");
				con.ChangeDatabase("GHTDB_EX");
				Compare(con.Database , "GHTDB_EX");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check DataBase Changed ");
				OleDbCommand cmd = new OleDbCommand("select count(*) from Customers", con);
				object obj = cmd.ExecuteScalar();
				Compare(obj != null, true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			if (con.State == ConnectionState.Open) con.Close();
		}

	}


	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}

	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

}
}