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
using System.Text;
using System.Data;
using System.Data.OracleClient ;
using System.Collections;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleConnection_ConnectionString : GHTBase
	{
		private Exception exp = null;
		private OracleConnection con = new OracleConnection();

		public static void Main()
		{
			OracleConnection_ConnectionString tc = new OracleConnection_ConnectionString();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleConnection_ConnectionString");

				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		public void run()
		{
			SetConnectionString();
			ConstractorWithConnectionString();
			DB2MissingProperties();
			TestCaseForBug3925();
		}

		#region Tests
		[Test]
		public void DB2MissingProperties()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.DB2)
			{
				Log("The 'DB2MissingProperties' subtest did not ran because it is relevant only for runs on DB2." );
				return;
			}
			string[] propsToUse;
			string description;

			//The complete list of properties is:
			//Provider, Password, User ID, Data Source, HostName, Port, Location, retrieveMessagesFromServerOnGetMessage
			
			description = "Test connection string without 'retrieveMessagesFromServerOnGetMessage'";
			propsToUse = new string[] {"Provider", "Password", "User ID", "Data Source", "HostName", "Port", "Location"};
			DoTestWithSpecificProperties(propsToUse, description);

			description = "Test connection string without 'HostName'";
			propsToUse = new string[] {"Provider", "Password", "User ID", "Data Source", "Port", "Location", "retrieveMessagesFromServerOnGetMessage"};
			DoTestWithSpecificProperties(propsToUse, description);
			
			description = "Test connection string without 'Port'";
			propsToUse = new string[] {"Provider", "Password", "User ID", "Data Source", "HostName", "Location", "retrieveMessagesFromServerOnGetMessage"};
			DoTestWithSpecificProperties(propsToUse, description);
			
			description = "Test connection string without 'Location'";
			propsToUse = new string[] {"Provider", "Password", "User ID", "Data Source", "HostName", "Port", "retrieveMessagesFromServerOnGetMessage"};
			DoTestWithSpecificProperties(propsToUse, description);
			
			description = "Test connection string without 'HostName' & 'Port'";
			propsToUse = new string[] {"Provider", "Password", "User ID", "Data Source", "Location", "retrieveMessagesFromServerOnGetMessage"};
			DoTestWithSpecificProperties(propsToUse, description);
		}

		void DoTestWithSpecificProperties(string[] propsToUse, string description)
		{
			try
			{
				BeginCase(description);
				exp = null;
				Hashtable connectionProps = GetConnectionStringProps(ConnectedDataProvider.ConnectionString);
				string actualConString = CreateConStringWithProps(propsToUse, connectionProps);
				con = new OracleConnection(actualConString);
				con.Open();
				Compare(con.State, ConnectionState.Open);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
				exp=null;
				if (con != null && con.State == ConnectionState.Open)
				{
					con.Close();
				}
			}
		}

		[Test]
		public void ConstractorWithConnectionString()
		{
			con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			try
			{
				BeginCase("Constructor with ConnectionString");
				Compare(con.ConnectionString, MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp); 
				exp = null;
			}
		}

		[Test]
		public void SetConnectionString()
		{
			con.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			try
			{
				BeginCase("Set ConnectionString");
				Compare(con.ConnectionString, MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp); 
				exp = null;
			}
		}
		[Test]
#if !JAVA
		[Category ("NotWorking")]
#endif
		public void TestCaseForBug3925()
		{
			exp=null;
			try
			{
				BeginCase("Test Case for bug #3925");

			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer)
			{
				Skip("This test currently runs only on SQLServer in java.");
				return;
			}
				Hashtable conProps = GetConnectionStringProps(ConnectedDataProvider.ConnectionString);
				string server = (string)conProps["Data Source"];
				string user = (string)conProps["User Id"];
				string password = (string)conProps["Password"];
				string database = (string)conProps["Initial Catalog"];
				string jdbcUrlTemplate = "JdbcDriverClassName=com.microsoft.jdbc.sqlserver.SQLServerDriver;JdbcURL=\"jdbc:microsoft:sqlserver://{0};User={1};Password={2};DatabaseName={3}\"";
				string conStr = string.Format(jdbcUrlTemplate, server, user, password, database);
				con = new OracleConnection(conStr);
				con.Open();
				Compare(ConnectionState.Open, con.State);
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
				exp = null;
				if (con != null && con.State != ConnectionState.Closed)
				{
					con.Close();
				}
			}
		}
		#endregion
		#region Utilities
		private string CreateConStringWithProps(string[] propsToUse, Hashtable connectionProps)
		{
			StringBuilder actualConStringBuilder = new StringBuilder();
			foreach(string prop in propsToUse)
			{
				string propString = string.Format("{0}={1}", prop, connectionProps[prop]);
				actualConStringBuilder.Append(propString);
				actualConStringBuilder.Append(";");
			}
			return actualConStringBuilder.ToString();
		}

		private Hashtable GetConnectionStringProps(string connectionString)
		{
			string[] connectionStringProps = connectionString.Split(';');
			Hashtable connectionProps = new Hashtable();
			foreach(string prop in connectionStringProps)
			{
				string[] keyValue = prop.Split('=');
				if (keyValue.Length == 2)
				{
					connectionProps.Add(keyValue[0], keyValue[1]);
				}
				else
				{
					connectionProps.Add(prop, prop);
				}
			}
			return connectionProps;
		}

		#endregion
	}
}