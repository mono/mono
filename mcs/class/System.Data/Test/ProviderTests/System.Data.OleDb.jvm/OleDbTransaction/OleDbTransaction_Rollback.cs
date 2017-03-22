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
	public class OleDbTransaction_Rollback : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbTransaction_Rollback tc = new OleDbTransaction_Rollback();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbTransaction_Rollback");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			OleDbConnection con = null;
			OleDbTransaction txn;
			Exception exp = null;
			try
			{
				BeginCase("OleDbTransaction Rollback");

				//
				//prepare data
				base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

				string Result = "";
				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				con.Open();
				txn = con.BeginTransaction();
				OleDbCommand cmd = new OleDbCommand("Update Employees Set LastName = 'StamLastName' Where EmployeeID = 100", con, txn);
				cmd.ExecuteNonQuery();
				txn.Rollback();

				//
				//
				cmd = new OleDbCommand("Select LastName From Employees Where EmployeeID = 100", con);
				Result = cmd.ExecuteScalar().ToString();
				Compare(Result,"Last100" );
				this.Log(Result);

			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			if (con.State == ConnectionState.Open) con.Close();
		}
	}
}