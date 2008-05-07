//
// OracleCommandTest.cs -
//      NUnit Test Cases for OraclePermissionAttribute
//
// Author:
//      Leszek Ciesielski  <skolima@gmail.com>
//
// Copyright (C) 2006 Forcom (http://www.forcom.com.pl/)
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
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleCommandTest
	{
		const string COMMAND_TEXT = "SELECT * FROM dual";

		OracleCommand command;
		IDbCommand interface_command;

		[SetUp]
		public void SetUp ()
		{
			command = new OracleCommand ();
			interface_command = command;
		}

		[TearDown]
		public void TearDown ()
		{
			command.Dispose ();
		}

		[Test] // ctor ()
		public void Constructor1 ()
		{
			OracleCommand cmd = new OracleCommand ();
			Assert.AreEqual (string.Empty, cmd.CommandText, "#1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#3");
			Assert.IsNull (cmd.Connection, "#4");
			Assert.IsNull (cmd.Container, "#5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#6");
			Assert.IsNotNull (cmd.Parameters, "#7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#8");
			Assert.IsNull (cmd.Site, "#9");
			Assert.IsNull (cmd.Transaction, "#10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#11");
		}

		[Test] // ctor (String)
		public void Constructor2 ()
		{
			OracleCommand cmd = new OracleCommand (COMMAND_TEXT);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#A2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.IsNull (cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNotNull (cmd.Parameters, "#A7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A8");
			Assert.IsNull (cmd.Site, "#A9");
			Assert.IsNull (cmd.Transaction, "#A10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A11");

			cmd = new OracleCommand ((string) null);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#B2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#B3");
			Assert.IsNull (cmd.Connection, "#B4");
			Assert.IsNull (cmd.Container, "#B5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#B6");
			Assert.IsNotNull (cmd.Parameters, "#B7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B8");
			Assert.IsNull (cmd.Site, "#B9");
			Assert.IsNull (cmd.Transaction, "#B10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B11");
		}

		[Test] // ctor (String, OracleConnection)
		public void Constructor3 ()
		{
			OracleConnection conn = new OracleConnection ();
			OracleCommand cmd;

			cmd = new OracleCommand (COMMAND_TEXT, conn);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#A2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.AreSame (conn, cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNotNull (cmd.Parameters, "#A7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A8");
			Assert.IsNull (cmd.Site, "#A9");
			Assert.IsNull (cmd.Transaction, "#A10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A11");

			cmd = new OracleCommand ((string) null, conn);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#B2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#B3");
			Assert.AreSame (conn, cmd.Connection, "#B4");
			Assert.IsNull (cmd.Container, "#B5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#B6");
			Assert.IsNotNull (cmd.Parameters, "#B7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B8");
			Assert.IsNull (cmd.Site, "#B9");
			Assert.IsNull (cmd.Transaction, "#B10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B11");

			cmd = new OracleCommand (COMMAND_TEXT, (OracleConnection) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#C1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#C2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#C3");
			Assert.IsNull (cmd.Connection, "#C4");
			Assert.IsNull (cmd.Container, "#C5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#C6");
			Assert.IsNotNull (cmd.Parameters, "#C7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#C8");
			Assert.IsNull (cmd.Site, "#C9");
			Assert.IsNull (cmd.Transaction, "#C10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#C11");
		}

		[Test] // ctor (String, OracleConnection, OracleTransaction)
		public void Constructor4 ()
		{
			OracleConnection conn = new OracleConnection ();
			OracleCommand cmd;

			cmd = new OracleCommand (COMMAND_TEXT, conn, (OracleTransaction) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#A2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.AreSame (conn, cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNotNull (cmd.Parameters, "#A7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A8");
			Assert.IsNull (cmd.Site, "#A9");
			Assert.IsNull (cmd.Transaction, "#A10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A11");

			cmd = new OracleCommand ((string) null, conn, (OracleTransaction) null);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#B2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#B3");
			Assert.AreSame (conn, cmd.Connection, "#B4");
			Assert.IsNull (cmd.Container, "#B5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#B6");
			Assert.IsNotNull (cmd.Parameters, "#B7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B8");
			Assert.IsNull (cmd.Site, "#B9");
			Assert.IsNull (cmd.Transaction, "#B10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B11");

			cmd = new OracleCommand (COMMAND_TEXT, (OracleConnection) null, (OracleTransaction) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#C1");
#if NET_2_0
			Assert.AreEqual (0, cmd.CommandTimeout, "#C2");
#endif
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#C3");
			Assert.IsNull (cmd.Connection, "#C4");
			Assert.IsNull (cmd.Container, "#C5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#C6");
			Assert.IsNotNull (cmd.Parameters, "#C7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#C8");
			Assert.IsNull (cmd.Site, "#C9");
			Assert.IsNull (cmd.Transaction, "#C10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#C11");
		}

		[Test] // bug #78765
		public void AllowNullTransactionTest ()
		{
			command.Transaction = null;
			interface_command.Transaction = null;
		}

		[Test]
		public void CommandText ()
		{
			OracleCommand cmd = new OracleCommand ();
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
		[Test]
		public void CommandTimeout ()
		{
			Assert.AreEqual (0, command.CommandTimeout, "#1");
			command.CommandTimeout = 10;
			Assert.AreEqual (0, command.CommandTimeout, "#2");
			command.CommandTimeout = int.MaxValue;
			Assert.AreEqual (0, command.CommandTimeout, "#3");
			command.CommandTimeout = int.MinValue;
			Assert.AreEqual (0, command.CommandTimeout, "#4");
		}
#endif

		[Test]
		public void ConnectionTimeout_IDbConnection ()
		{
			Assert.AreEqual (0, interface_command.CommandTimeout, "#1");
			interface_command.CommandTimeout = 10;
			Assert.AreEqual (0, interface_command.CommandTimeout, "#2");
			interface_command.CommandTimeout = int.MaxValue;
			Assert.AreEqual (0, interface_command.CommandTimeout, "#3");
			interface_command.CommandTimeout = int.MinValue;
			Assert.AreEqual (0, interface_command.CommandTimeout, "#4");
		}

		[Test]
		public void Connection ()
		{
			OracleConnection connection = new OracleConnection ();

			Assert.IsNull (command.Connection, "#1");
			command.Connection = connection;
			Assert.AreSame (connection, command.Connection, "#2");
			Assert.AreSame (connection, interface_command.Connection, "#3");
			command.Connection = null;
			Assert.IsNull (command.Connection, "#4");
			Assert.IsNull (interface_command.Connection, "#5");
		}

		[Test]
		public void Connection_IDbConnection ()
		{
			OracleConnection connection = new OracleConnection ();

			Assert.IsNull (interface_command.Connection, "#A1");
			interface_command.Connection = connection;
			Assert.AreSame (connection, interface_command.Connection, "#A2");
			Assert.AreSame (connection, command.Connection, "#A3");
			interface_command.Connection = null;
			Assert.IsNull (interface_command.Connection, "#A4");
			Assert.IsNull (command.Connection, "#A5");

			try {
				interface_command.Connection = new SqlConnection ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}
	}
}
