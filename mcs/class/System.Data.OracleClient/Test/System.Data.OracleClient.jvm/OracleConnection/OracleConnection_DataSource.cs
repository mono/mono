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
public class OracleConnection_DataSource : ADONetTesterClass 
{
	public static void Main()
	{
		OracleConnection_DataSource tc = new OracleConnection_DataSource();
		Exception exp = null;
		try
		{
			tc.BeginTest("OracleConnection_DataSource");
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

		//test does not apply to ORACLE,DB2,Postgres
		if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.Oracle) return;
		if (ConnectedDataProvider.GetDbType(con) ==  DataBaseServer.DB2) return;
		if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL) return;

		//get the expected result from the connection string
		string[] arrCon = con.ConnectionString.Split(';');
		string result = null;
		for (int i=0; i < arrCon.Length; i++)
			if (arrCon[i].IndexOf("Data Source") >= 0) 
			{
				result = arrCon[i];
				break;
			}
		result = result.Substring(result.IndexOf('=')+1).Trim();
		try
		{
			BeginCase("check DataSource");
			Compare(con.DataSource  , result);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
	}


	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}

	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

}
}