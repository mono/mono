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

using System.Data;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlCommandTest
	{
		const string COMMAND_TEXT = "SELECT * FROM Authors";

		[Test] // SqlCommand ()
		public void Constructor1 ()
		{
			SqlCommand cmd = new SqlCommand ();
			Assert.AreEqual (string.Empty, cmd.CommandText, "#1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#3");
			Assert.IsNull (cmd.Connection, "#4");
			Assert.IsNull (cmd.Container, "#5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#6");
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#8");
#endif
			Assert.IsNotNull (cmd.Parameters, "#9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#10");
			Assert.IsNull (cmd.Site, "#11");
			Assert.IsNull (cmd.Transaction, "#11");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#12");
		}

		[Test] // SqlCommand (string)
		public void Constructor2 ()
		{
			SqlCommand cmd = new SqlCommand (COMMAND_TEXT);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#A2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.IsNull (cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#A7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#A8");
#endif
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
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#B7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#B8");
#endif
			Assert.IsNotNull (cmd.Parameters, "#B9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B10");
			Assert.IsNull (cmd.Site, "#B11");
			Assert.IsNull (cmd.Transaction, "#B12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B13");
		}

		[Test] // SqlCommand (string, SqlConnection)
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
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#A7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#A8");
#endif
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
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#B7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#B8");
#endif
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
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#C7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#C8");
#endif
			Assert.IsNotNull (cmd.Parameters, "#C9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#C10");
			Assert.IsNull (cmd.Site, "#C11");
			Assert.IsNull (cmd.Transaction, "#C12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#C13");
		}

		[Test] // SqlCommand (string, SqlConnection, SqlTransaction)
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
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#A7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#A8");
#endif
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
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#B7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#B8");
#endif
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
#if NET_2_0
			Assert.IsNull (cmd.Notification, "#C7");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#C8");
#endif
			Assert.IsNotNull (cmd.Parameters, "#C9");
			Assert.AreEqual (0, cmd.Parameters.Count, "#C10");
			Assert.IsNull (cmd.Site, "#C11");
			Assert.IsNull (cmd.Transaction, "#C12");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#C13");
		}

		[Test] // bug #324386
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

#if NET_2_0
		[Test] // bug #381100
		public void ParameterCollectionTest ()
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Parameters.AddRange(new SqlParameter[] { });
		}
#endif
	}
}
