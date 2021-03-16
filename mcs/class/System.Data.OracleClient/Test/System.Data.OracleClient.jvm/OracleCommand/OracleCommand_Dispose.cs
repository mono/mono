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
public class OracleCommand_Dispose : GHTBase
{
	public static void Main()
	{
		OracleCommand_Dispose tc = new OracleCommand_Dispose();
		Exception exp = null;
		try
		{
			tc.BeginTest("OracleCommand_Dispose");
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

	//problems from Synergy

	//Ofer, 

	//Take a look at #1. Test this to see if we have a problem in dispose. If we do, add a test for this to make sure we catch it in next rounds. 

	//

	//-----Original Message-----
	//From: David Teplitchi [mailto:davidt@mainsoft.com] 
	//Sent: Sunday, March 21, 2004 9:31 AM
	//To: //Oved Yavine//
	//Subject: 2 problems from Synergy


	//Please check those 2 problems reported by Synergy and tell me what do you think.

	//1) The following code works in dotnet but doesn//t work in j2ee.
	//// Oracle Drivers
	//OracleConnection Connect;
	//OracleDataReader DbReader;
	//OracleCommand DbCommand;
	//IDataReader DR;
	//string sSQL;
	//int iField;
	//bool bFound;
	//try
	//{
	//Connect = new OracleConnection("[PLACEHOLDER]");
	//Connect.Open();
	//sSQL = "SELECT * FROM PRODUCT WHERE PRO_SKU = //SKU_208//";
	//DbCommand = new OracleCommand(sSQL, Connect);
	//DbReader = DbCommand.ExecuteReader(CommandBehavior.SingleRow);
	//DR = DbReader;
	//DbCommand.Dispose(); // comment out
	////DbCommand = null;
	//bFound = DbReader.HasRows;
	//DR.Read();
	//// Get Column Ordinal
	//iField = DR.GetOrdinal("PRO_DESCRIPTION");

	//string sDesc = DR.GetString(iField);
	//}
	//catch(Exception e)
	//{
	//this.WriteErrorLog(e.Message);
	//}
	//i have identified the problem as being "DbCommand.Dispose()". If you comment this line out or make it 
	//"DbCommand = null", then its okay. So this instruction doesnt work the same in both in .net and j2ee.


	[Test]
	public void run()
	{
		Exception exp = null;

		//OracleConnection  con = null;

		//this test was added due to a request from Oved:
		//this bug occur on all databases (SQL,Oracle,DB2)
		OracleCommand DbCommand = null;
		OracleConnection Connect = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
		Connect.Open();
		DbCommand = new OracleCommand("SELECT * FROM Customers", Connect);
		OracleDataReader  DbReader  = DbCommand.ExecuteReader();

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

		if (Connect.State != ConnectionState.Closed)
			Connect.Close();


	}
}



}