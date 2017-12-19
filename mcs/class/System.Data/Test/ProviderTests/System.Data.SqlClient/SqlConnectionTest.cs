//
// SqlConnectionTest.cs - NUnit Test Cases for testing the
//                          SqlConnection class
// Author:
//      Senganal T (tsenganal@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Data.Connected.SqlClient
{
	[TestFixture]
	[Category ("sqlserver")]
	public class SqlConnectionTest
	{
		SqlConnection conn;
		String connectionString;
		ArrayList events;
		EngineConfig engine;

		[SetUp]
		public void SetUp ()
		{
			events = new ArrayList ();
			connectionString = ConnectionManager.Instance.Sql.ConnectionString;
			engine = ConnectionManager.Instance.Sql.EngineConfig;
		}

		[TearDown]
		public void TearDown ()
		{
			if (conn != null)
				conn.Dispose ();
			if (connectionString != null)
				SqlConnection.ClearAllPools ();
		}

		[Test]
		public void OverloadedConstructorTest ()
		{
			//check synonyms.
			//do i need to check for all the synonyms.. 
			conn = new SqlConnection ("Timeout=10;Connect Timeout=20;Connection Timeout=30");
			Assert.AreEqual (30, conn.ConnectionTimeout, "#A1");
			conn = new SqlConnection ("Connect Timeout=100;Connection Timeout=200;Timeout=300");
			Assert.AreEqual (300, conn.ConnectionTimeout, "#A2");
			conn = new SqlConnection ("Connection Timeout=1000;Timeout=2000;Connect Timeout=3000");
			Assert.AreEqual (3000, conn.ConnectionTimeout, "#A3");

			//'==' doesent work correctly in both msdotnet and mono
			/*
			conn = new SqlConnection ("server=local==host;database=tmp;");
			Assert.AreEqual ("local==host", conn.DataSource, 
				"# Datasource name is set incorrectly");
			*/
			string connStr = "Server='loca\"lhost';Database='''Db'; packet Size=\"512\";"
				+ "connect Timeout=20;Workstation Id=\"'\"\"desktop\";";
			conn = new SqlConnection (connStr);
			Assert.AreEqual (connStr , conn.ConnectionString , "#B1");
			Assert.AreEqual ("loca\"lhost" , conn.DataSource , "#B2");
			Assert.AreEqual ("'Db" , conn.Database , "#B3");
			Assert.AreEqual (512 , conn.PacketSize , "#B4");
			Assert.AreEqual (20 , conn.ConnectionTimeout , "#B5");
			Assert.AreEqual ("'\"desktop" , conn.WorkstationId , "#B6");
		}

		[Test]
		public void Open ()
		{
			conn = new SqlConnection (connectionString);
			conn.StateChange += new StateChangeEventHandler (Connection_StateChange);
			conn.Open ();

			Assert.AreEqual (ConnectionState.Open, conn.State, "#1");
			Assert.AreEqual (1, events.Count, "#2");
			StateChangeEventArgs args = events [0] as StateChangeEventArgs;
			Assert.IsNotNull (args, "#3");
			Assert.AreEqual (ConnectionState.Closed, args.OriginalState, "#4");
			Assert.AreEqual (ConnectionState.Open, args.CurrentState, "#5");

			conn.Close ();
		}

		[Test]
		public void Open_Connection_Open ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			try {
				conn.Open ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The connection was not closed. The connection's
				// current state is open
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			} finally {
				conn.Close ();
			}
		}

		[Test]
		public void Open_ConnectionString_LoginInvalid ()
		{
			// login invalid
			conn = new SqlConnection (connectionString + "user id=invalidLogin");
			try {
				conn.Open ();
				Assert.Fail ("#1");
			} catch (SqlException ex) {
				// Login failed for user 'invalidLogin'
				Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
				Assert.AreEqual ((byte) 14, ex.Class, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (18456, ex.Number, "#7");
				Assert.AreEqual ((byte) 1, ex.State, "#8");
			} finally {
				conn.Close ();
			}
		}

		[Test]
		public void Open_ConnectionString_DatabaseInvalid ()
		{
			conn = new SqlConnection (connectionString + "database=invalidDB");
			try {
				conn.Open ();
				Assert.Fail ("#1");
			} catch (SqlException ex) {
				// Cannot open database "invalidDB" requested
				// by the login. The login failed
				Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
				Assert.AreEqual ((byte) 11, ex.Class, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (4060, ex.Number, "#7");
				Assert.AreEqual ((byte) 1, ex.State, "#8");
			} finally {
				conn.Close ();
			}

		}

		[Test]
		public void Open_ConnectionString_PasswordInvalid ()
		{
			// password invalid
			conn = new SqlConnection (connectionString + ";password=invalidPassword");
			try {
				conn.Open ();
				Assert.Fail ("#1");
			} catch (SqlException ex) {
				// Login failed for user '...'
				Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
				Assert.AreEqual ((byte) 14, ex.Class, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (18456, ex.Number, "#6");
				Assert.AreEqual ((byte) 1, ex.State, "#7");
			} finally {
				conn.Close ();
			}
		}

		[Test]
		public void Open_ConnectionString_ServerInvalid ()
		{
			Assert.Ignore ("Long running");

			// server invalid
			conn = new SqlConnection (connectionString + ";server=invalidServerName");
			try {
				conn.Open ();
				Assert.Fail ("#1");
			} catch (SqlException ex) {
				// An error has occurred while establishing a
				// connection to the server...
				Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
				Assert.AreEqual ((byte) 20, ex.Class, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (53, ex.Number, "#6");
				Assert.AreEqual ((byte) 0, ex.State, "#7");
			} finally {
				conn.Close ();
			}
			}

		[Test] // bug #383061
		[Category("NotWorking")]
		public void Open_MaxPoolSize_Reached ()
		{
			connectionString += ";Pooling=true;Connection Lifetime=6;Connect Timeout=3;Max Pool Size=2";

			SqlConnection conn1 = new SqlConnection (connectionString);
			conn1.Open ();

			SqlConnection conn2 = new SqlConnection (connectionString);
			conn2.Open ();

			DateTime start = DateTime.Now;

			try {
				using (SqlConnection sqlConnection = new SqlConnection (connectionString)) {
					sqlConnection.Open ();
				}
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// System.InvalidOperationException: Timeout expired.
				// The timeout period elapsed prior to obtaining a
				// connection from the pool. This may have occurred
				// because all pooled connections were in use and max
				// pool size was reached.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			TimeSpan elapsed = DateTime.Now - start;

			Assert.IsTrue (elapsed.TotalSeconds >= 3, "#B1:" + elapsed.TotalSeconds);
			Assert.IsTrue (elapsed.TotalSeconds < 4, "#B2:" + elapsed.TotalSeconds);

			conn2.Close ();

			// as the second connection is closed, we should now be
			// able to open a new connection (which essentially
			// uses the pooled connection from conn2)
			SqlConnection conn3 = new SqlConnection (connectionString);
			conn3.Open ();
			conn3.Close ();

			conn1.Close ();
		}

		[Test] // bug #412574
		public void Close ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();
			conn.StateChange += new StateChangeEventHandler (Connection_StateChange);
			conn.Close ();

			Assert.AreEqual (ConnectionState.Closed, conn.State, "#1");
			Assert.AreEqual (1, events.Count, "#2");
			StateChangeEventArgs args = events [0] as StateChangeEventArgs;
			Assert.IsNotNull (args, "#3");
			Assert.AreEqual (ConnectionState.Open, args.OriginalState, "#4");
			Assert.AreEqual (ConnectionState.Closed, args.CurrentState, "5");

			conn.Close ();

			Assert.AreEqual (1, events.Count, "#6");
		}

		[Test]
		public void ChangeDatabase ()
		{
			conn = new SqlConnection(connectionString);
			conn.Open();

			if (ConnectionManager.Instance.Sql.IsAzure)
			{
				var exc = Assert.Throws<SqlException>(() => conn.ChangeDatabase("master"));
				Assert.Equals(40508, exc.Number); //USE statement is not supported to switch between databases (Azure).
			}
			else
			{
				conn.ChangeDatabase("master");
				Assert.AreEqual("master", conn.Database);
			}
		}

		[Test]
		public void ChangeDatabase_DatabaseName_DoesNotExist ()
		{
			if (ConnectionManager.Instance.Sql.IsAzure)
				Assert.Ignore("SQL Azure doesn't support 'ChangeDatabase'");

			conn = new SqlConnection (connectionString);
			conn.Open ();

			String database = conn.Database;

			try {
				conn.ChangeDatabase ("doesnotexist");
				Assert.Fail ("#1");
			} catch (SqlException ex) {
				// Could not locate entry in sysdatabases for
				// database 'doesnotexist'. No entry found with
				// that name. Make sure that the name is entered
				// correctly.
				Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
				Assert.AreEqual ((byte) 16, ex.Class, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#6");
				Assert.AreEqual (911, ex.Number, "#7");
				Assert.AreEqual ((byte) 1, ex.State, "#8");

				Assert.AreEqual (database, conn.Database, "#9");
			} finally {
				conn.Close ();
			}
		}

		[Test]
		public void ChangeDatabase_DatabaseName_Empty ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();
			try {
				conn.ChangeDatabase (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Database cannot be null, the empty string,
				// or string of only whitespace
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName);
			}
		}

		[Test]
		public void ChangeDatabase_DatabaseName_Null ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();
			try {
				conn.ChangeDatabase ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Database cannot be null, the empty string,
				// or string of only whitespace
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName);
			}
		}

		[Test] // bug #412581
		public void ChangeDatabase_DatabaseName_Whitespace ()
		{
			Assert.Ignore ("bug #412581");

			conn = new SqlConnection (connectionString);
			conn.Open ();
			try {
				conn.ChangeDatabase ("   ");
				Assert.Fail ("#1");
			} catch (SqlException ex) {
				// Could not locate entry in sysdatabases for
				// database '   '. No entry found with that name.
				// Make sure that the name is entered correctly
				Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
				Assert.AreEqual ((byte) 16, ex.Class, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("'   '") != -1, "#6");
				Assert.AreEqual (911, ex.Number, "#7");
				Assert.AreEqual ((byte) 1, ex.State, "#8");
			}
		}

		[Test]
		[Category("NotWorking")]
		public void ClearAllPools ()
		{
			SqlConnection conn1 = new SqlConnection (connectionString + ";Pooling=false");
			conn1.Open ();

			int initial_connection_count = GetConnectionCount (conn1);

			SqlConnection conn2 = new SqlConnection (connectionString + ";App=A");
			conn2.Open ();
			conn2.Close ();

			SqlConnection conn3 = new SqlConnection (connectionString + ";App=B");
			conn3.Open ();
			conn3.Close ();

			Assert.AreEqual (initial_connection_count + 2, GetConnectionCount (conn1), "#1");

			SqlConnection.ClearAllPools ();

			Assert.AreEqual (initial_connection_count, GetConnectionCount (conn1), "#2");
			conn1.Close ();
		}

		[Test] // bug #443131
		[Category("NotWorking")]
		public void ClearPool ()
		{
			SqlConnection conn1 = new SqlConnection (connectionString);
			conn1.Open ();

			int initial_connection_count = GetConnectionCount (conn1);

			SqlConnection conn2 = new SqlConnection (connectionString);
			conn2.Open ();

			SqlConnection conn3 = new SqlConnection (connectionString);
			conn3.Open ();
			conn3.Close ();

			Assert.AreEqual (initial_connection_count + 2, GetConnectionCount (conn1), "#1");

			SqlConnection.ClearPool (conn1);

			// check if pooled connections that were not in use are
			// actually closed
			Assert.AreEqual (initial_connection_count + 1, GetConnectionCount (conn1), "#2");

			conn2.Close ();

			// check if connections that were in use when the pool
			// was cleared will not be returned to the pool when
			// closed (and are closed instead)
			Assert.AreEqual (initial_connection_count, GetConnectionCount (conn1), "#3");

			SqlConnection conn4 = new SqlConnection (connectionString);
			conn4.Open ();

			SqlConnection conn5 = new SqlConnection (connectionString);
			conn5.Open ();

			SqlConnection conn6 = new SqlConnection (connectionString);
			conn6.Open ();

			Assert.AreEqual (initial_connection_count + 3, GetConnectionCount (conn1), "#4");

			conn5.Close ();
			conn6.Close ();

			// check if new connections are stored in the pool again
			Assert.AreEqual (initial_connection_count + 3, GetConnectionCount (conn1), "#5");

			conn1.Close ();

			Assert.AreEqual (initial_connection_count + 2, GetConnectionCount (conn4), "#6");

			SqlConnection.ClearPool (conn3);

			// the connection passed to ClearPool does not have to
			// be open
			Assert.AreEqual (initial_connection_count, GetConnectionCount (conn4), "#7");

			SqlConnection conn7 = new SqlConnection (connectionString);
			conn7.Open ();
			conn7.Close ();

			Assert.AreEqual (initial_connection_count + 1, GetConnectionCount (conn4), "#8");

			conn3.ConnectionString += ";App=B";
			SqlConnection.ClearPool (conn3);

			// check if a pool is identified by its connection string
			Assert.AreEqual (initial_connection_count + 1, GetConnectionCount (conn4), "#9");

			SqlConnection conn8 = new SqlConnection (connectionString);
			SqlConnection.ClearPool (conn8);

			// connection should not have been opened before to
			// clear the corresponding pool
			Assert.AreEqual (initial_connection_count, GetConnectionCount (conn4), "#10");

			SqlConnection conn9 = new SqlConnection (connectionString);
			conn9.Open ();
			conn9.Close ();

			Assert.AreEqual (initial_connection_count + 1, GetConnectionCount (conn4), "#11");

			conn3.ConnectionString = connectionString;
			SqlConnection.ClearPool (conn3);

			Assert.AreEqual (initial_connection_count, GetConnectionCount (conn4), "#12");

			SqlConnection conn10 = new SqlConnection (connectionString);
			conn10.Open ();

			SqlConnection conn11 = new SqlConnection (connectionString + ";App=B");
			conn11.Open ();

			SqlConnection conn12 = new SqlConnection (connectionString + ";App=B");
			conn12.Open ();

			SqlConnection conn13 = new SqlConnection (connectionString + ";App=B");
			conn13.Open ();

			conn10.Close ();
			conn11.Close ();
			conn12.Close ();
			conn13.Close ();

			Assert.AreEqual (initial_connection_count + 4, GetConnectionCount (conn4), "#13");

			// check that other connection pools are not affected
			SqlConnection.ClearPool (conn13);

			Assert.AreEqual (initial_connection_count + 1, GetConnectionCount (conn4), "#14");

			SqlConnection conn14 = new SqlConnection (connectionString);
			conn14.Open ();
			conn14.Dispose ();

			// a disposed connection cannot be used to clear a pool
			SqlConnection.ClearPool (conn14);

			Assert.AreEqual (initial_connection_count + 1, GetConnectionCount (conn4), "#15");

			SqlConnection.ClearPool (conn4);

			Assert.AreEqual (initial_connection_count, GetConnectionCount (conn4), "#16");

			conn4.Close ();
		}

		[Test]
		public void InterfaceTransactionTest ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();
			IDbCommand idbCommand = new SqlCommand ("use [mono-test]", conn);
			idbCommand.Connection = null;
			Assert.AreEqual (null, idbCommand.Connection, "Connection should be null");
			idbCommand.Transaction = null;
			Assert.AreEqual (null, idbCommand.Transaction, "Transaction should be null");

			conn.Close ();
		}

		[Test]
		public void BeginTransaction ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			SqlTransaction trans = conn.BeginTransaction ();
			Assert.AreSame (conn, trans.Connection, "#A1");
			Assert.AreEqual (IsolationLevel.ReadCommitted, trans.IsolationLevel, "#A2");
			trans.Rollback ();

			trans = conn.BeginTransaction ();

			try {
				conn.BeginTransaction ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// SqlConnection does not support parallel transactions
				Assert.AreEqual (typeof(InvalidOperationException), ex.GetType(), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			} finally {
				trans.Rollback();
			}

			try {
				trans = conn.BeginTransaction ();
				trans.Rollback ();
				trans = conn.BeginTransaction ();
				trans.Commit ();
				trans = conn.BeginTransaction ();
			} finally {
				trans.Rollback ();
			}
		}

		[Test]
		public void ConnectionString ()
		{
			conn = new SqlConnection (connectionString);
			// Test Repeated Keyoword should take the latest value
			conn.ConnectionString = conn.ConnectionString + ";server=RepeatedServer;";
			Assert.AreEqual ("RepeatedServer", ((SqlConnection)conn).DataSource, "#A1");
			conn.ConnectionString += ";database=gen;Initial Catalog=gen1";
			Assert.AreEqual ("gen1", conn.Database, "#A2");

			// Test if properties are set correctly
			string str = "server=localhost1;database=db;user id=user;";
			str += "password=pwd;Workstation ID=workstation;Packet Size=512;";
			str += "Connect Timeout=10";
			conn.ConnectionString = str;

			Assert.AreEqual ("localhost1", conn.DataSource, "#B1");
			Assert.AreEqual ("db", conn.Database, "#B2");
			Assert.AreEqual (ConnectionState.Closed, conn.State, "#B3");
			Assert.AreEqual ("workstation", conn.WorkstationId, "#B4");
			Assert.AreEqual (512, conn.PacketSize, "#B5");
			Assert.AreEqual (10, conn.ConnectionTimeout, "#B6");
			
			// Test if any leftover values exist from previous invocation
			conn.ConnectionString = connectionString;
			conn.ConnectionString = string.Empty;
			Assert.AreEqual (string.Empty, conn.DataSource, "#C1");
			Assert.AreEqual ("", conn.Database, "#C2");
			Assert.AreEqual (8000, conn.PacketSize, "#C3");
			Assert.AreEqual (15, conn.ConnectionTimeout, "#C4");
		}

		[Test]
		public void ConnectionString_Connection_Open ()
		{
			conn = new SqlConnection (connectionString);
			conn.ConnectionString = connectionString;
			conn.Open ();
			try {
				conn.ConnectionString = "server=localhost;database=tmp;";
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Not allowed to change the 'ConnectionString'
				// property. The connection's current state is open
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			} finally {
				conn.Close ();
			}
		}

		[Test]
		public void ServerVersionTest ()
		{
			conn = new SqlConnection (connectionString);

			// Test InvalidOperation Exception is thrown if Connection is CLOSED
			try {
				string s = conn.ServerVersion;
				Assert.Fail ("#A1:" + s);
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}
			
			// Test if Release Version is as per specification.
			conn.Open ();
			String [] version = conn.ServerVersion.Split ('.');
			Assert.AreEqual (2, version[0].Length,
				"#B1 The Major release shud be exactly 2 characters");
			Assert.AreEqual (2, version[1].Length,
				"#B2 The Minor release shud be exactly 2 characters");
			Assert.AreEqual (4, version[2].Length,
				"#B3 The Release version should be exactly 4 digits");
		}

		[Test]
		public void Database ()
		{
			if (ConnectionManager.Instance.Sql.IsAzure)
				Assert.Ignore("SQL Azure doesn't support 'use [db]'");

			conn = new SqlConnection (connectionString);
			string database = conn.Database;

			SqlCommand cmd;

			// Test if database property is updated when a query changes database
			conn.Open ();
			cmd = new SqlCommand ("use [master]" , conn);
			cmd.ExecuteNonQuery ();
			Assert.AreEqual ("master", conn.Database, "#1");

			// ensure we're really in the expected database
			if (ClientVersion == 7)
				cmd.CommandText = "SELECT name FROM sysdatabases WHERE name = 'master'";
			else
				cmd.CommandText = "SELECT name FROM sys.databases WHERE name = 'master'";
			using (SqlDataReader dr = cmd.ExecuteReader ()) {
				Assert.IsTrue (dr.Read (), "#2");
			}

			conn.Close ();
			Assert.AreEqual (database, conn.Database, "#3");

			// Test if the database property is reset on re-opening the connection
			conn.ConnectionString = connectionString;
			conn.Open ();
			Assert.AreEqual (database, conn.Database, "#4");

			// ensure we're really in the expected database
			cmd.CommandText = "SELECT fname FROM employee WHERE id = 2";
			using (SqlDataReader dr = cmd.ExecuteReader ()) {
				Assert.IsTrue (dr.Read (), "#5");
				Assert.AreEqual ("ramesh", dr.GetValue (0), "#6");
			}

			conn.Close ();
		}

		[Test]
		[Category("NotWorking")] //https://github.com/dotnet/corefx/issues/22871
		public void WorkstationId()
		{
			var connection1 = new SqlConnection (connectionString + ";Workstation Id=Desktop");
			var connection2 = new SqlConnection (connectionString);
			connection1.Dispose();
			Assert.AreEqual (Environment.MachineName, connection1.WorkstationId);
			Assert.AreEqual (Environment.MachineName, connection2.WorkstationId);
		}

		[Test] // bug #412571
		public void Dispose ()
		{
			StateChangeEventArgs stateChangeArgs;
			EventArgs disposedArgs;

			conn = new SqlConnection (connectionString + ";Connection Timeout=30;Packet Size=512;Workstation Id=Desktop");
			conn.Disposed += new EventHandler (Connection_Disposed);
			conn.Open ();
			conn.StateChange += new StateChangeEventHandler (Connection_StateChange);
			Assert.AreEqual (0, events.Count, "#A1");
			conn.Dispose ();
			Assert.AreEqual (string.Empty, conn.ConnectionString, "#A2");
			Assert.AreEqual (15, conn.ConnectionTimeout, "#A3");
			Assert.AreEqual (string.Empty, conn.Database, "#A4");
			Assert.AreEqual (string.Empty, conn.DataSource, "#A5");
			Assert.AreEqual (8000, conn.PacketSize, "#A6");
			Assert.AreEqual (ConnectionState.Closed, conn.State, "#A7");
			Assert.AreEqual (2, events.Count, "#A9");

			stateChangeArgs = events [0] as StateChangeEventArgs;
			Assert.IsNotNull (stateChangeArgs, "#B1");
			Assert.AreEqual (typeof (StateChangeEventArgs), stateChangeArgs.GetType (), "#B2");
			Assert.AreEqual (ConnectionState.Open, stateChangeArgs.OriginalState, "#B3");
			Assert.AreEqual (ConnectionState.Closed, stateChangeArgs.CurrentState, "B4");

			disposedArgs = events [1] as EventArgs;
			Assert.IsNotNull (disposedArgs, "#C1");
			Assert.AreEqual (typeof (EventArgs), disposedArgs.GetType (), "#C2");

			conn.Dispose ();

			Assert.AreEqual (ConnectionState.Closed, conn.State, "#D1");
			Assert.AreEqual (3, events.Count, "#D2");

			disposedArgs = events [2] as EventArgs;
			Assert.IsNotNull (disposedArgs, "#E1");
			Assert.AreEqual (typeof (EventArgs), disposedArgs.GetType (), "#E2");
		}

		void Connection_StateChange (object sender , StateChangeEventArgs e)
		{
			events.Add (e);
		}

		void Connection_Disposed (object sender , EventArgs e)
		{
			events.Add (e);
		}

		[Test]
		public void FireInfoMessageEventOnUserErrorsTest ()
		{
			conn = new SqlConnection ();
			Assert.AreEqual(false, conn.FireInfoMessageEventOnUserErrors, "#1 The default value should be false");
			conn.FireInfoMessageEventOnUserErrors = true;
			Assert.AreEqual(true, conn.FireInfoMessageEventOnUserErrors, "#1 The value should be true after setting the property to true");
		}

		[Test]
		public void StatisticsEnabledTest ()
		{
			conn = new SqlConnection (); 
			Assert.AreEqual(false, conn.StatisticsEnabled, "#1 The default value should be false");
			conn.StatisticsEnabled = true;
			Assert.AreEqual(true, conn.StatisticsEnabled, "#1 The value should be true after setting the property to true");
		}

		[Test]
		[Category("NotWorking")]
		public void ChangePasswordTest ()
		{
			string tmpPassword = "modifiedbymonosqlclient";
			SqlConnection.ChangePassword (connectionString, tmpPassword);
			SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder (connectionString);
			string oldPassword = connBuilder.Password;
			connBuilder.Password = tmpPassword;
			SqlConnection.ChangePassword (connBuilder.ConnectionString, oldPassword); // Modify to the original password
		}

		static int GetConnectionCount (SqlConnection conn)
		{
			Thread.Sleep (200);

			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "select count(*) from master..sysprocesses where db_name(dbid) = @dbname";
			cmd.Parameters.Add (new SqlParameter ("@dbname", conn.Database));
			int connection_count = (int) cmd.ExecuteScalar ();
			cmd.Dispose ();

			return connection_count;
		}

		int ClientVersion {
			get {
				return (engine.ClientVersion);
			}
		}
	}

	[TestFixture]
	[Category ("sqlserver")]
	public class GetSchemaTest
	{
		SqlConnection conn = null;
		String connectionString = ConnectionManager.Instance.Sql.ConnectionString;

		[SetUp]
		public void SetUp()
		{
			conn = new SqlConnection(connectionString);
			conn.Open();
		}

		[TearDown]
		public void TearDown()
		{
			conn?.Close();
		}

		[Test]
		public void GetSchemaTest1()
		{
			if (ConnectionManager.Instance.Sql.IsAzure)
				Assert.Ignore("SQL Azure - Not supported'");

			bool flag = false;
			DataTable tab1 = conn.GetSchema("databases");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					if (col.ColumnName.ToString() == "database_name" && row[col].ToString() == ConnectionManager.Instance.DatabaseName)
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS1 failed");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetSchemaTest2()
		{
			conn.GetSchema(null);
		}

		[Test]
		public void GetSchemaTest3 ()
		{
			Assert.Ignore ("We currently have no foreign keys defined in the test database");

			bool flag = false;
			DataTable tab1 = conn.GetSchema("ForeignKeys");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "TABLE_NAME" && row[col].ToString() == "tmptable1")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS3 failed");
		}

		[Test]
		public void GetSchemaTest4()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("Indexes");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "table_name" && row[col].ToString() == "binary_family")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS4 failed");
		}

		[Test]
		public void GetSchemaTest5()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("IndexColumns");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "table_name" && row[col].ToString() == "binary_family")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS5 failed");
		}

		[Test]
		public void GetSchemaTest6()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("Procedures");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "SPECIFIC_NAME" && row[col].ToString() == "sp_get_age")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS6 failed");
		}

		[Test]
		public void GetSchemaTest7()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("ProcedureParameters");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "SPECIFIC_NAME" && row[col].ToString() == "sp_get_age")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS7 failed");
		}

		[Test]
		public void GetSchemaTest8()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("Tables");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "TABLE_NAME" && row[col].ToString() == "binary_family")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS8 failed");
		}

		[Test]
		public void GetSchemaTest9()
		{
			if (ConnectionManager.Instance.Sql.IsAzure)
				Assert.Ignore("SQL Azure - Not supported'");

			bool flag = false;
			DataTable tab1 = conn.GetSchema("Columns");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "TABLE_NAME" && row[col].ToString() == "binary_family")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS9 failed");
		}

		[Test]
		public void GetSchemaTest10()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("Users");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "user_name" && row[col].ToString() == "public")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS10 failed");
		}

		[Test]
		public void GetSchemaTest11 ()
		{
			Assert.Ignore ("Incorrect syntax near 'TABLE_SCHEMA'");

			bool flag = false;
			DataTable tab1 = conn.GetSchema("Views");
			flag = true; // FIXME: Currently MS-SQL 2005 returns empty table. Remove this flag ASAP.
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values.
					 */
					if (col.ColumnName.ToString() == "user_name" && row[col].ToString() == "public")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS11 failed");
		}

		[Test]
		public void GetSchemaTest12 ()
		{
			Assert.Ignore ("Incorrect syntax near '('");

			bool flag = false;
			DataTable tab1 = conn.GetSchema("ViewColumns");
			flag = true; // FIXME: Currently MS-SQL 2005 returns empty table. Remove this flag ASAP.
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values.
					 */
					if (col.ColumnName.ToString() == "user_name" && row[col].ToString() == "public")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS12 failed");
		}

		[Test]
		public void GetSchemaTest13 ()
		{
			Assert.Ignore ("The multi-part identifier \"assportemblies.name\" could not be bound");

			bool flag = false;
			DataTable tab1 = conn.GetSchema("UserDefinedTypes");
			flag = true; // FIXME: Currently MS-SQL 2005 returns empty table. Remove this flag ASAP.
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values.
					 */
					if (col.ColumnName.ToString() == "user_name" && row[col].ToString() == "public")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS13 failed");
		}

		[Test]
		[Ignore("TODO: fix restrictions")]
		public void GetSchemaTest14()
		{
			bool flag = false;
			string [] restrictions = new string[4];

			restrictions[0] = ConnectionManager.Instance.DatabaseName;
			restrictions[1] = "dbo";
			restrictions[2] = null;
			restrictions[3] = "BASE TABLE";
			DataTable tab1 = conn.GetSchema("Tables", restrictions);
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "TABLE_NAME" && row[col].ToString() == "binary_family")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS14 failed");
		}

		[Test]
		[Ignore("TODO: fix restrictions")]
		public void GetSchemaTest15()
		{
			bool flag = false;
			string [] restrictions = new string[4];

			restrictions[0] = ConnectionManager.Instance.DatabaseName;
			restrictions[1] = null;
			restrictions[2] = "binary_family";
			restrictions[3] = null;
			DataTable tab1 = conn.GetSchema("IndexColumns", restrictions);
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "table_name" && row[col].ToString() == "binary_family")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS15 failed");
		}

		[Test]
		[Ignore("TODO: fix restrictions")]
		public void GetSchemaTest16()
		{
			bool flag = false;
			string [] restrictions = new string[4];

			restrictions[0] = ConnectionManager.Instance.DatabaseName;
			restrictions[1] = null;
			restrictions[2] = "sp_get_age";
			restrictions[3] = null;
			DataTable tab1 = conn.GetSchema("Procedures", restrictions);
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "ROUTINE_NAME" && row[col].ToString() == "sp_get_age")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS16 failed");
		}

		[Test]
		public void GetSchemaTest17()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema();
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "CollectionName" && row[col].ToString() == "UserDefinedTypes")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS17 failed");
		}

		[Test]
		public void GetSchemaTest18()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("RESTRICTIONS");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "RestrictionDefault" && row[col].ToString() == "CONSTRAINT_NAME")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS18 failed");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetSchemaTest19 ()
		{
			String [] restrictions = new String[1];
			conn.GetSchema("RESTRICTIONS", restrictions);
		}

		[Test]
		public void GetSchemaTest20 ()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("DataTypes");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "TypeName" && row[col].ToString() == "uniqueidentifier")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS20 failed");
		}

		[Test]
		public void GetSchemaTest21()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema();
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "CollectionName" && row[col].ToString() == "UserDefinedTypes")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS21 failed");
		}
		[Test]
		public void GetSchemaTest22()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("ReservedWords");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "ReservedWord" && row[col].ToString() == "UPPER")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(true, flag, "#GS22 failed");
		}

		[Test]
		public void GetSchemaTest23()
		{
			bool flag = false;
			DataTable tab1 = conn.GetSchema("ReservedWords");
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "ReservedWord" && row[col].ToString() == "upper")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(false, flag, "#GS23 failed");
		}

		[Test]
		public void GetSchemaTest24()
		{
			bool flag = false;
			string [] restrictions = new string[4];

			restrictions[0] = ConnectionManager.Instance.DatabaseName;
			restrictions[1] = null;
			restrictions[2] = "sp_get_age";
			restrictions[3] = null;
			DataTable tab1 = conn.GetSchema("Procedures", restrictions);
			foreach (DataRow row in tab1.Rows)
			{
				foreach (DataColumn col in tab1.Columns)
				{
					/*
					 * We need to consider multiple values
					 */
					if (col.ColumnName.ToString() == "ROUTINE_NAME" && row[col].ToString() == "mono")
					{
						flag = true;
						break;
					}
				}
				if (flag)
					break;
			}
			Assert.AreEqual(false, flag, "#GS24 failed");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetSchemaTest25 ()
		{
			String [] restrictions = new String [1];
			conn.GetSchema ("Mono", restrictions);
		}
	}
}
