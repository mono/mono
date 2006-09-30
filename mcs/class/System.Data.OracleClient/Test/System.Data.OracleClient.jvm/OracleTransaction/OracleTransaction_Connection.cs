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
	public class OracleTransaction_Connection : ADONetTesterClass
	{
		public static void Main()
		{
			OracleTransaction_Connection tc = new OracleTransaction_Connection();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleTransaction_Connection");
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
			con.Open();
			OracleTransaction txn = con.BeginTransaction();

			try
			{
				BeginCase("check connection");
				Compare(txn.Connection,con);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			//check exception - using a command with the same connection as the transaction
			OracleCommand cmd = new OracleCommand("Select * from Employees",con);

			//Execute requires the command to have a transaction object when the connection assigned to the command is in a pending local transaction.  
			//The Transaction property of the command has not been initialized.
			try
			{
				BeginCase("Command that uses the transaction connection - exception");
				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (Exception ex){exp=ex;}
				Compare(exp.GetType().FullName ,typeof(InvalidOperationException).FullName);
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			if (con.State == ConnectionState.Open) con.Close();

		}
	}
}