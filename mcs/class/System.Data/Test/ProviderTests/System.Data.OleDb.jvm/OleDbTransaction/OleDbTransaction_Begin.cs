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
	public class OleDbTransaction_Begin : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbTransaction_Begin tc = new OleDbTransaction_Begin();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbTransaction_Begin");
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

			/*********************************************************
			 * OLEDB Provider for SQL Server does not allow nested transactions
			 * http://support.microsoft.com/kb/177138/EN-US/
			 * http://support.microsoft.com/default.aspx?scid=KB;EN-US;Q316872&
			*/
			if ((ConnectedDataProvider.GetDbType(con) == DataBaseServer.SQLServer) ||
				(ConnectedDataProvider.GetDbType(con) == DataBaseServer.Oracle) ||
				(ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL) ||
				(ConnectedDataProvider.GetDbType(con) == DataBaseServer.DB2))
			{
				Log(string.Format("Test skipped, nested transactions are not supported in {0}", ConnectedDataProvider.GetDbType(con)));
				return;
			}

			// How To Implement Nested Transactions with Oracle
			// http://support.microsoft.com/kb/187289/EN-US/

			OleDbTransaction txnOuter = null;
			OleDbTransaction txnInner = null;
		
			try
			{
				BeginCase("Check Outer Transaction Isoloation Level");
			
				txnOuter = con.BeginTransaction();
				txnInner = txnOuter.Begin();

				Compare(txnOuter.IsolationLevel,IsolationLevel.ReadCommitted);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check Inner Transaction Isoloation Level");
				Compare(txnOuter.IsolationLevel,IsolationLevel.RepeatableRead);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			if (con.State == ConnectionState.Open) con.Close();
		}
	}
}