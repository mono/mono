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
	public class OleDbCommand_ExecuteNonQuery : GHTBase
	{
		OleDbConnection con;
		OleDbCommand cmd;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				//prepare Data
				OleDbCommand cmdPrepare = new OleDbCommand("", new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString));
				cmdPrepare.Connection.Open();
				cmdPrepare.CommandText = "DELETE FROM Employees WHERE EmployeeID = 99999";
				cmdPrepare.ExecuteScalar();
				cmdPrepare.Connection.Close();
				cmdPrepare.Dispose();

				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				cmd = new OleDbCommand("", con);
				con.Open();
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
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
			OleDbCommand_ExecuteNonQuery tc = new OleDbCommand_ExecuteNonQuery();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbCommand_ExecuteNonQuery");
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
	
			int intRecordsAffected = 0;

			try
			{
				BeginCase("Execute Insert");
				cmd.CommandText = "INSERT INTO Employees (EmployeeID,FirstName, LastName) VALUES (99999,'OferXYZ', 'Testing')";
				intRecordsAffected = cmd.ExecuteNonQuery();
				Compare(intRecordsAffected, 1);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check insert operation");
				cmd.CommandText = "SELECT FirstName FROM Employees WHERE (EmployeeID = 99999)";
				string  strFirstName = cmd.ExecuteScalar().ToString();
				Compare(strFirstName, "OferXYZ");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Execute Select");
				cmd.CommandText = "SELECT EmployeeID FROM Employees WHERE (EmployeeID = 99999)";
				intRecordsAffected = cmd.ExecuteNonQuery();
				
				switch (ConnectedDataProvider.GetDbType())
				{
					case DataBaseServer.PostgreSQL:
						// postgres odbc returns 1
#if !JAVA
						{
							Compare(intRecordsAffected, 1);
						}
#else
						{
							Compare(intRecordsAffected, -1);
						}
#endif
						break;
					default:
						Compare(intRecordsAffected, -1);
						break;
				}
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Execute UPDATE");
				cmd.CommandText = "UPDATE Employees SET FirstName = 'OferABC', LastName = 'TestXYZ' WHERE (EmployeeID = 99999)";
				intRecordsAffected = cmd.ExecuteNonQuery();
				Compare(intRecordsAffected, 1);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			try
			{
				BeginCase("Check Update operation");
				cmd.CommandText = "SELECT FirstName FROM Employees WHERE (EmployeeID = 99999)";
				string  strFirstName = cmd.ExecuteScalar().ToString();
				Compare(strFirstName, "OferABC");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Execute UPDATE");
				cmd.CommandText = "DELETE FROM Employees WHERE (EmployeeID = 99999)";
				intRecordsAffected = cmd.ExecuteNonQuery();
				Compare(intRecordsAffected, 1);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check Delete operation");
				cmd.CommandText = "SELECT FirstName FROM Employees WHERE (EmployeeID = 99999)";
				object obj = cmd.ExecuteScalar();
				Compare(obj==null, true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check OleDBException - update with bad value");
				cmd.CommandText = "UPDATE Employees SET BirthDate = 'bad value' WHERE (EmployeeID = 1)";
				try
				{
					cmd.ExecuteNonQuery(); 
				}
				catch (OleDbException ex)
				{
					exp = ex;
				}
				Compare(exp.GetType().FullName, typeof(OleDbException).FullName);
				exp=null;
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check OleDBException - missing EmployeeID");
				cmd.CommandText = "INSERT INTO Employees (FirstName, BirthDate) VALUES ('Dado', 'Ben David')";
				try
				{
					cmd.ExecuteNonQuery(); 
				}
				catch (OleDbException ex)
				{
					exp = ex;
				}
				Compare(exp.GetType().FullName, typeof(OleDbException).FullName);
				exp=null;
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

		}
	}
}