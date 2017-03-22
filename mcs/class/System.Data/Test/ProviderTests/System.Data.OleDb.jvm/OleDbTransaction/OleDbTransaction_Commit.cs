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
	public class OleDbTransaction_Commit : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbTransaction_Commit tc = new OleDbTransaction_Commit();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbTransaction_Commit");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;

			// ORACLE does not support transactions with savepoints 
			// http://support.microsoft.com/kb/187289/EN-US/
			if (ConnectedDataProvider.GetDbType(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString) == DataBaseServer.Oracle) return;
			if (ConnectedDataProvider.GetDbType(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString) == DataBaseServer.PostgreSQL) return;

			//prepare data
			base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			OleDbConnection con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			con.Open();

			//prepare command for checking database
			OleDbConnection conSelect = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			conSelect.Open();
			OleDbCommand cmdSelect = new OleDbCommand("Select Title From Employees where EmployeeID in (100,200)", conSelect);

			OleDbTransaction txn = con.BeginTransaction();

			//prepare first transaction
			OleDbCommand cmd1 = new OleDbCommand("Update Employees Set Title = 'New Value1' Where EmployeeID = 100",con);
			cmd1.Transaction = txn;
		
			//prepare a second transaction
			OleDbCommand cmd2 = new OleDbCommand("Update Employees Set Title = 'New Value2' Where EmployeeID = 200",con);
			cmd2.Transaction = txn;

			try
			{
				BeginCase("one transaction - After commiting transaction");
				cmd1.ExecuteNonQuery();
				txn.Commit();
				string Result = cmdSelect.ExecuteScalar().ToString();
				Compare(Result,"New Value1" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			//prepare data
			base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			txn = con.BeginTransaction();
			cmd1.Transaction = txn;
			cmd2.Transaction = txn;
			cmd1.ExecuteNonQuery();
			cmd2.ExecuteNonQuery();
			txn.Commit();
			OleDbDataReader rdr = cmdSelect.ExecuteReader();

			try
			{
				BeginCase("two transaction - check first transaction");
				rdr.Read();
				Compare(rdr.GetString(0),"New Value1" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("two transaction - check second transaction");
				rdr.Read();
				Compare(rdr.GetString(0),"New Value2" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			if (con.State == ConnectionState.Open) con.Close();
			if (conSelect.State == ConnectionState.Open) conSelect.Close();


		}
	}
}