//
// OdbcCommandTest.cs - NUnit Test Cases for testing
// System.Data.Odbc.OdbcCommand
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

#if !NO_ODBC

using System;
using System.Data;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{
	[TestFixture]
	public class OdbcCommandTest
	{
		const string COMMAND_TEXT = "SELECT * FROM Authors";

		[Test] // OdbcCommand ()
		public void Constructor1 ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			Assert.AreEqual (string.Empty, cmd.CommandText, "#1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#2");
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

		[Test] // OdbcCommand (String)
		public void Constructor2 ()
		{
			OdbcCommand cmd = new OdbcCommand (COMMAND_TEXT);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#A2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.IsNull (cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNotNull (cmd.Parameters, "#A7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A8");
			Assert.IsNull (cmd.Site, "#A9");
			Assert.IsNull (cmd.Transaction, "#A10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A11");

			cmd = new OdbcCommand ((string) null);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#B2");
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

		[Test] // OdbcCommand (String, OdbcConnection)
		public void Constructor3 ()
		{
			OdbcConnection conn = new OdbcConnection ();
			OdbcCommand cmd;

			cmd = new OdbcCommand (COMMAND_TEXT, conn);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#A2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.AreSame (conn, cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNotNull (cmd.Parameters, "#A7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A8");
			Assert.IsNull (cmd.Site, "#A9");
			Assert.IsNull (cmd.Transaction, "#A10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A11");

			cmd = new OdbcCommand ((string) null, conn);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#B2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#B3");
			Assert.AreSame (conn, cmd.Connection, "#B4");
			Assert.IsNull (cmd.Container, "#B5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#B6");
			Assert.IsNotNull (cmd.Parameters, "#B7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B8");
			Assert.IsNull (cmd.Site, "#B9");
			Assert.IsNull (cmd.Transaction, "#B10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B11");

			cmd = new OdbcCommand (COMMAND_TEXT, (OdbcConnection) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#C1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#C2");
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

		[Test] // OdbcCommand (String, OdbcConnection, OdbcTransaction)
		public void Constructor4 ()
		{
			OdbcConnection conn = new OdbcConnection ();
			OdbcCommand cmd;

			cmd = new OdbcCommand (COMMAND_TEXT, conn, (OdbcTransaction) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#A1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#A2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#A3");
			Assert.AreSame (conn, cmd.Connection, "#A4");
			Assert.IsNull (cmd.Container, "#A5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#A6");
			Assert.IsNotNull (cmd.Parameters, "#A7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#A8");
			Assert.IsNull (cmd.Site, "#A9");
			Assert.IsNull (cmd.Transaction, "#A10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#A11");

			cmd = new OdbcCommand ((string) null, conn, (OdbcTransaction) null);
			Assert.AreEqual (string.Empty, cmd.CommandText, "#B1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#B2");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#B3");
			Assert.AreSame (conn, cmd.Connection, "#B4");
			Assert.IsNull (cmd.Container, "#B5");
			Assert.IsTrue (cmd.DesignTimeVisible, "#B6");
			Assert.IsNotNull (cmd.Parameters, "#B7");
			Assert.AreEqual (0, cmd.Parameters.Count, "#B8");
			Assert.IsNull (cmd.Site, "#B9");
			Assert.IsNull (cmd.Transaction, "#B10");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#B11");

			cmd = new OdbcCommand (COMMAND_TEXT, (OdbcConnection) null, (OdbcTransaction) null);
			Assert.AreEqual (COMMAND_TEXT, cmd.CommandText, "#C1");
			Assert.AreEqual (30, cmd.CommandTimeout, "#C2");
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

		[Test]
		public void CommandText ()
		{
			OdbcCommand cmd = new OdbcCommand ();
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
		public void CommandTimeout ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			cmd.CommandTimeout = 10;
			Assert.AreEqual (10, cmd.CommandTimeout, "#1");
			cmd.CommandTimeout = 25;
			Assert.AreEqual (25, cmd.CommandTimeout, "#2");
			cmd.CommandTimeout = 0;
			Assert.AreEqual (0, cmd.CommandTimeout, "#3");
		}

		[Test]
		public void CommandTimeout_Value_Negative ()
		{
			OdbcCommand cmd = new OdbcCommand ();
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
		public void CommandType_Value_Invalid ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			try {
				cmd.CommandType = (CommandType) (666);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// The CommandType enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#5:" + ex.Message);
				Assert.AreEqual ("CommandType", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Dispose ()
		{
			OdbcConnection conn = new OdbcConnection ();
			OdbcCommand cmd = null;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT 'a'";
				cmd.CommandTimeout = 67;
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.DesignTimeVisible = false;
				cmd.Parameters.Add (new OdbcParameter ());
				cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;

				cmd.Dispose ();

				Assert.AreEqual (string.Empty, cmd.CommandText, "CommandText");
				Assert.AreEqual (67, cmd.CommandTimeout, "CommandTimeout");
				Assert.AreEqual (CommandType.StoredProcedure, cmd.CommandType, "CommandType");
				Assert.IsNull (cmd.Connection, "Connection");
				Assert.IsFalse (cmd.DesignTimeVisible, "DesignTimeVisible");
				Assert.IsNotNull (cmd.Parameters, "Parameters#1");
				Assert.AreEqual (0, cmd.Parameters.Count, "Parameters#2");
				Assert.IsNull (cmd.Transaction, "Transaction");
				Assert.AreEqual (UpdateRowSource.OutputParameters, cmd.UpdatedRowSource, "UpdatedRowSource");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				conn.Dispose ();
			}
		}

		[Test]
		public void ExecuteNonQuery_Connection_Closed ()
		{
			OdbcConnection cn = new OdbcConnection ();
			OdbcCommand cmd = new OdbcCommand (COMMAND_TEXT, cn);
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
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteNonQuery") != -1, "#5:" + ex.Message);
			}
		}

		[Test]
		public void ExecuteNonQuery_Connection_Null ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			cmd.CommandText = COMMAND_TEXT;

			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery: Connection property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteNonQuery") != -1, "#5:" + ex.Message);
			}
		}

		[Test]
		public void ExecuteReader_Connection_Closed ()
		{
			OdbcConnection cn = new OdbcConnection ();
			OdbcCommand cmd = new OdbcCommand (COMMAND_TEXT, cn);
			try {
				cmd.ExecuteReader ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery requires an open and available
				// Connection. The connection's current state is
				// closed.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteReader") != -1, "#5:" + ex.Message);
			}
		}

		[Test]
		public void ExecuteReader_Connection_Null ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			cmd.CommandText = COMMAND_TEXT;

			try {
				cmd.ExecuteReader ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery: Connection property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteReader") != -1, "#5:" + ex.Message);
			}
		}

		[Test]
		public void ExecuteScalar_Connection_Closed ()
		{
			OdbcConnection cn = new OdbcConnection ();
			OdbcCommand cmd = new OdbcCommand (COMMAND_TEXT, cn);
			try {
				cmd.ExecuteScalar ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery requires an open and available
				// Connection. The connection's current state is
				// closed.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteScalar") != -1, "#5:" + ex.Message);
			}
		}

		[Test]
		public void ExecuteScalar_Connection_Null ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			cmd.CommandText = COMMAND_TEXT;

			try {
				cmd.ExecuteScalar ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery: Connection property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ExecuteScalar") != -1, "#5:" + ex.Message);
			}
		}

		[Test]
		public void ResetCommandTimeout ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			cmd.CommandTimeout = 50;
			Assert.AreEqual (cmd.CommandTimeout, 50, "#1");
			cmd.ResetCommandTimeout ();
			Assert.AreEqual (cmd.CommandTimeout, 30, "#2");
		}

		[Test]
		public void UpdatedRowSource ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			cmd.UpdatedRowSource = UpdateRowSource.None;
			Assert.AreEqual (UpdateRowSource.None, cmd.UpdatedRowSource, "#1");
			cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;
			Assert.AreEqual (UpdateRowSource.OutputParameters, cmd.UpdatedRowSource, "#2");
		}

		[Test]
		public void UpdatedRowSource_Value_Invalid ()
		{
			OdbcCommand cmd = new OdbcCommand ();
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
	}
}

#endif