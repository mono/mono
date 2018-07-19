//
// SqlCommandTest.cs - NUnit Test Cases for testing
// System.Data.SqlClient.SqlCommand
// 
// Author:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2007 Gert Driesen
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
using System.Data.Sql;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlCommandTest
	{
		const string COMMAND_TEXT = "SELECT * FROM Authors";

		[Test] // SqlCommand ()
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Constructor1 ()
		{
			SqlCommand cmd = new SqlCommand ();
			Assert.AreEqual (string.Empty, cmd.CommandText, "#1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#3");
			Assert.IsNull (cmd.Connection, "#4");
			Assert.IsNull (cmd.Container, "#5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#6");
			Assert.IsNull (cmd.Notification, "#7");
			// Not implemented in corefx:
			//Assert.IsTrue (cmd.NotificationAutoEnlist, "#8");
			Assert.IsNotNull (cmd.Parameters, "#9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#10");
			Assert.IsNull (cmd.Site, "#11");
			Assert.IsNull (cmd.Transaction, "#11");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#12");
		}

		[Test] // SqlCommand (string)
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Constructor2 ()
		{
			SqlCommand cmd = new SqlCommand (COMMAND_TEXT);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#A2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.IsNull (cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNull (cmd.Notification, "#A7");
			// Not implemented in corefx:
			// Assert.IsTrue (cmd.NotificationAutoEnlist, "#A8");
			Assert.IsNotNull (cmd.Parameters, "#A9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A10");
			Assert.IsNull (cmd.Site, "#A11");
			Assert.IsNull (cmd.Transaction, "#A12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A13");

			cmd = new SqlCommand ((string) null);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#B2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#B3");
			Assert.IsNull (cmd.Connection, "#B4");
			Assert.IsNull (cmd.Container, "#B5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#B6");
			Assert.IsNull (cmd.Notification, "#B7");
			// Not implemented in corefx:
			// Assert.IsTrue (cmd.NotificationAutoEnlist, "#B8");
			Assert.IsNotNull (cmd.Parameters, "#B9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B10");
			Assert.IsNull (cmd.Site, "#B11");
			Assert.IsNull (cmd.Transaction, "#B12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B13");
		}

		[Test] // SqlCommand (string, SqlConnection)
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Constructor3 ()
		{
			SqlConnection conn = new SqlConnection ();
			SqlCommand cmd;

			cmd = new SqlCommand (COMMAND_TEXT, conn);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#A2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.AreSame (conn, cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNull (cmd.Notification, "#A7");
			// Not implemented in corefx:
			// Assert.IsTrue (cmd.NotificationAutoEnlist, "#A8");
			Assert.IsNotNull (cmd.Parameters, "#A9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A10");
			Assert.IsNull (cmd.Site, "#A11");
			Assert.IsNull (cmd.Transaction, "#A12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A13");

			cmd = new SqlCommand ((string) null, conn);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#B2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#B3");
			Assert.AreSame (conn, cmd.Connection, "#B4");
			Assert.IsNull (cmd.Container, "#B5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#B6");
			Assert.IsNull (cmd.Notification, "#B7");
			// Not implemented in corefx:
			//Assert.IsTrue (cmd.NotificationAutoEnlist, "#B8");
			Assert.IsNotNull (cmd.Parameters, "#B9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B10");
			Assert.IsNull (cmd.Site, "#B11");
			Assert.IsNull (cmd.Transaction, "#B12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B13");

			cmd = new SqlCommand (COMMAND_TEXT, (SqlConnection) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#C1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#C2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#C3");
			Assert.IsNull (cmd.Connection, "#C4");
			Assert.IsNull (cmd.Container, "#C5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#C6");
			Assert.IsNull (cmd.Notification, "#C7");
			// Not implemented in corefx:
			//Assert.IsTrue (cmd.NotificationAutoEnlist, "#C8");
			Assert.IsNotNull (cmd.Parameters, "#C9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#C10");
			Assert.IsNull (cmd.Site, "#C11");
			Assert.IsNull (cmd.Transaction, "#C12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#C13");
		}

		[Test] // SqlCommand (string, SqlConnection, SqlTransaction)
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Constructor4 ()
		{
			SqlConnection conn = new SqlConnection ();
			SqlCommand cmd;

			cmd = new SqlCommand (COMMAND_TEXT, conn, (SqlTransaction) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#A2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.AreSame (conn, cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNull (cmd.Notification, "#A7");
			// Not implemented in corefx:
			// Assert.IsTrue (cmd.NotificationAutoEnlist, "#A8");
			Assert.IsNotNull (cmd.Parameters, "#A9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A10");
			Assert.IsNull (cmd.Site, "#A11");
			Assert.IsNull (cmd.Transaction, "#A12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A13");

			cmd = new SqlCommand ((string) null, conn, (SqlTransaction) null);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#B2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#B3");
			Assert.AreSame (conn, cmd.Connection, "#B4");
			Assert.IsNull (cmd.Container, "#B5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#B6");
			Assert.IsNull (cmd.Notification, "#B7");
			// Not implemented in corefx:
			// Assert.IsTrue (cmd.NotificationAutoEnlist, "#B8");
			Assert.IsNotNull (cmd.Parameters, "#B9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B10");
			Assert.IsNull (cmd.Site, "#B11");
			Assert.IsNull (cmd.Transaction, "#B12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B13");

			cmd = new SqlCommand (COMMAND_TEXT, (SqlConnection) null, (SqlTransaction) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#C1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#C2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#C3");
			Assert.IsNull (cmd.Connection, "#C4");
			Assert.IsNull (cmd.Container, "#C5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#C6");
			Assert.IsNull (cmd.Notification, "#C7");
			// Not implemented in corefx:
			// Assert.IsTrue (cmd.NotificationAutoEnlist, "#C8");
			Assert.IsNotNull (cmd.Parameters, "#C9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#C10");
			Assert.IsNull (cmd.Site, "#C11");
			Assert.IsNull (cmd.Transaction, "#C12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#C13");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Clone ()
		{
			SqlNotificationRequest notificationReq = new SqlNotificationRequest ();

			SqlCommand cmd = new SqlCommand ();
			cmd.CommandText = "sp_insert";
			cmd.CommandTimeout = 100;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.DesignTimeVisible = false;
			cmd.Notification = notificationReq;
			// not implemented in corefx
			//cmd.NotificationAutoEnlist = false;
			cmd.Parameters.Add ("@TestPar1", SqlDbType.Int);
			cmd.Parameters ["@TestPar1"].Value = DBNull.Value;
			cmd.Parameters.AddWithValue ("@BirthDate", DateTime.Now);
			cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;

			SqlCommand clone = (((ICloneable) (cmd)).Clone ()) as SqlCommand;
			Assert.AreEqual ("sp_insert", clone.CommandText, "#1");
			Assert.AreEqual (100, clone.CommandTimeout, "#2");
			Assert.AreEqual (CommandType.StoredProcedure, clone.CommandType, "#3");
			Assert.IsNull (cmd.Connection, "#4");
			Assert.IsFalse (cmd.DesignTimeVisible, "#5");
			Assert.AreSame (notificationReq, cmd.Notification, "#6");
			// not implemented in corefx
			//Assert.IsFalse (cmd.NotificationAutoEnlist, "#7");
			Assert.AreEqual (2, clone.Parameters.Count, "#8");
			Assert.AreEqual (100, clone.CommandTimeout, "#9");
			clone.Parameters.AddWithValue ("@test", DateTime.Now);
			clone.Parameters [0].ParameterName = "@ClonePar1";
			Assert.AreEqual (3, clone.Parameters.Count, "#10");
			Assert.AreEqual (2, cmd.Parameters.Count, "#11");
			Assert.AreEqual ("@ClonePar1", clone.Parameters [0].ParameterName, "#12");
			Assert.AreEqual ("@TestPar1", cmd.Parameters [0].ParameterName, "#13");
			Assert.AreEqual ("@BirthDate", clone.Parameters [1].ParameterName, "#14");
			Assert.AreEqual ("@BirthDate", cmd.Parameters [1].ParameterName, "#15");
			Assert.IsNull (clone.Transaction, "#16");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CommandText ()
		{
			SqlCommand cmd = new SqlCommand ();
			cmd.CommandText = COMMAND_TEXT;
			Assert.AreSame (COMMAND_TEXT, cmd.CommandText, "#1");
			cmd.CommandText = null;
			Assert.AreEqual (string.Empty, cmd.CommandText, "#2");
			cmd.CommandText = COMMAND_TEXT;
			Assert.AreSame (COMMAND_TEXT, cmd.CommandText, "#3");
			cmd.CommandText = string.Empty;
			Assert.AreEqual (string.Empty, cmd.CommandText, "#4");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CommandTimeout ()
		{
			SqlCommand cmd = new SqlCommand ();
			cmd.CommandTimeout = 10;
			Assert.AreEqual (10, cmd.CommandTimeout, "#1");
			cmd.CommandTimeout = 25;
			Assert.AreEqual (25, cmd.CommandTimeout, "#2");
			cmd.CommandTimeout = 0;
			Assert.AreEqual (0, cmd.CommandTimeout, "#3");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CommandTimeout_Value_Negative ()
		{
			SqlCommand cmd = new SqlCommand ();
			try {
				cmd.CommandTimeout = -1;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid CommandTimeout value -1; the value must be >= 0
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("CommandTimeout", ex.ParamName, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CommandType_Value_Invalid ()
		{
			SqlCommand cmd = new SqlCommand ();
			try {
				cmd.CommandType = (CommandType) (666);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// The CommandType enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#5");
				Assert.AreEqual ("CommandType", ex.ParamName, "#6");
			}
		}

		[Test] // bug #324386
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Dispose ()
		{
			string connectionString = "Initial Catalog=a;Server=b;User ID=c;"
				+ "Password=d";
			SqlConnection connection = new SqlConnection (connectionString);
			SqlCommand command = connection.CreateCommand ();
			command.Dispose ();
			Assert.AreEqual (connectionString, connection.ConnectionString);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ExecuteNonQuery_Connection_Closed ()
		{
			string connectionString = "Initial Catalog=a;Server=b;User ID=c;"
				+ "Password=d";
			SqlConnection cn = new SqlConnection (connectionString);

			SqlCommand cmd = new SqlCommand ("delete from whatever", cn);
			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery requires an open and available
				// Connection. The connection's current state is
				// closed.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteNonQuery") != -1, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ExecuteNonQuery_Connection_Null ()
		{
			SqlCommand cmd = new SqlCommand ("delete from whatever");
			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery: Connection property has not
				// been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteNonQuery:"), "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ExecuteReader_Connection_Closed ()
		{
			string connectionString = "Initial Catalog=a;Server=b;User ID=c;"
				+ "Password=d";
			SqlConnection cn = new SqlConnection (connectionString);

			SqlCommand cmd = new SqlCommand ("Select count(*) from whatever", cn);
			try {
				cmd.ExecuteReader ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteReader requires an open and available
				// Connection. The connection's current state is
				// closed.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteReader") != -1, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ExecuteReader_Connection_Null ()
		{
			SqlCommand cmd = new SqlCommand ("select * from whatever");
			try {
				cmd.ExecuteReader ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteReader: Connection property has not
				// been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteReader:"), "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ExecuteScalar_Connection_Closed ()
		{
			string connectionString = "Initial Catalog=a;Server=b;User ID=c;"
				+ "Password=d";
			SqlConnection cn = new SqlConnection (connectionString);

			SqlCommand cmd = new SqlCommand ("Select count(*) from whatever", cn);
			try {
				cmd.ExecuteScalar ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteScalar requires an open and available
				// Connection. The connection's current state is
				// closed.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteScalar") != -1, "#5");
			}
		}

		[Test] // bug #412584
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ExecuteScalar_Connection_Null ()
		{
			SqlCommand cmd = new SqlCommand ("select count(*) from whatever");
			try {
				cmd.ExecuteScalar ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteScalar: Connection property has not
				// been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteScalar:"), "#5");
			}
		}

		// FIXME: this actually doesn't match .NET behavior. It shouldn't throw NRE.
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Prepare_Connection_Null ()
		{
			SqlCommand cmd;

			// Text, with parameters
			cmd = new SqlCommand ("select count(*) from whatever");
			cmd.Parameters.Add ("@TestPar1", SqlDbType.Int);
			try {
				cmd.Prepare ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException) {
			}
		}
		
		[Test] // bug #412586
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Prepare_Connection_Closed ()
		{
			string connectionString = "Initial Catalog=a;Server=b;User ID=c;"
				+ "Password=d";
			SqlConnection cn = new SqlConnection (connectionString);

			SqlCommand cmd;

			// Text, without parameters
			cmd = new SqlCommand ("select count(*) from whatever", cn);
			cmd.Prepare ();

			// Text, with parameters
			cmd = new SqlCommand ("select count(*) from whatever", cn);
			cmd.Parameters.Add ("@TestPar1", SqlDbType.Int);
			try {
				cmd.Prepare ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Prepare requires an open and available
				// Connection. The connection's current state
				// is Closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Prepare") != -1, "#A5");
			}

			// Text, parameters cleared
			cmd = new SqlCommand ("select count(*) from whatever", cn);
			cmd.Parameters.Add ("@TestPar1", SqlDbType.Int);
			cmd.Parameters.Clear ();
			cmd.Prepare ();

			// StoredProcedure, without parameters
			cmd = new SqlCommand ("FindCustomer", cn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Prepare ();

			// StoredProcedure, with parameters
			cmd = new SqlCommand ("FindCustomer", cn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add ("@TestPar1", SqlDbType.Int);
			cmd.Prepare ();

			// ensure connection was not implictly opened
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#B");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ResetCommandTimeout ()
		{
			SqlCommand cmd = new SqlCommand ();
			cmd.CommandTimeout = 50;
			Assert.AreEqual (cmd.CommandTimeout, 50, "#1");
			cmd.ResetCommandTimeout ();
			Assert.AreEqual (cmd.CommandTimeout, 30, "#2");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UpdatedRowSource ()
		{
			SqlCommand cmd = new SqlCommand ();
			cmd.UpdatedRowSource = UpdateRowSource.None;
			Assert.AreEqual (UpdateRowSource.None, cmd.UpdatedRowSource, "#1");
			cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;
			Assert.AreEqual (UpdateRowSource.OutputParameters, cmd.UpdatedRowSource, "#2");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UpdatedRowSource_Value_Invalid ()
		{
			SqlCommand cmd = new SqlCommand ();
			try {
				cmd.UpdatedRowSource = (UpdateRowSource) 666;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// The UpdateRowSource enumeration value,666,
				// is invalid
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("UpdateRowSource", ex.ParamName, "#5");
			}
		}


		[Test] // bug #381100
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ParameterCollectionTest ()
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Parameters.AddRange(new SqlParameter[] { });
		}
	}
}

