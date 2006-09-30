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
using System.Data.OracleClient ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleCommand_ExecuteScalar : GHTBase
	{
		OracleConnection	con;
		OracleCommand cmd;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				con.Open();

				//prepare Data
				OracleCommand cmdPrepare = new OracleCommand("", con);
				cmdPrepare.CommandText = "DELETE FROM Employees WHERE EmployeeID = 99999";
				cmdPrepare.ExecuteNonQuery();
				cmdPrepare.CommandText = "INSERT INTO Employees (EmployeeID, FirstName, LastName) VALUES (99999,'OferXYZ', 'Testing')";
				cmdPrepare.ExecuteNonQuery();
				cmdPrepare.Dispose();

				cmd = new OracleCommand("", con);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		[TearDown]
		public void TearDown()
		{
			if (con != null)
			{
				if (con.State == ConnectionState.Open) con.Close();
			}
		}

		public static void Main()
		{
			OracleCommand_ExecuteScalar tc = new OracleCommand_ExecuteScalar();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleCommand_ExecuteScalar");
				tc.SetUp();
				tc.run();
				tc.TearDown();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;
			object objResult = null;

			cmd.CommandText="Select FirstName,City From Employees Where EmployeeID=-1";

			try
			{
				BeginCase("Execute Scalar");
				objResult = cmd.ExecuteScalar();
				Compare(objResult==null,true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			cmd.CommandText="Select FirstName,City From Employees Where EmployeeID=99999 ";
		
			try
			{
				BeginCase("Execute Scalar");
				objResult = cmd.ExecuteScalar();
				Compare(objResult.ToString(), "OferXYZ");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			try
			{
				BeginCase("Execute Scalar again");
				objResult = cmd.ExecuteScalar();
				Compare(objResult.ToString(), "OferXYZ");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

		}
	}
}