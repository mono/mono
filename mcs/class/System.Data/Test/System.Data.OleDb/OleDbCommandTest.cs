//
// OleDbCommandTest.cs - NUnit Test Cases for testing
// System.Data.OleDb.OleDbCommand
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

#if !NO_OLEDB

using System.Data;
using System.Data.OleDb;

using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbCommandTest
	{
		const string COMMAND_TEXT = "SELECT * FROM Authors";

		[Test] // OleDbCommand ()
		public void Constructor1 ()
		{
			OleDbCommand cmd = new OleDbCommand ();
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

		[Test] // OleDbCommand (string)
		public void Constructor2 ()
		{
			OleDbCommand cmd = new OleDbCommand (COMMAND_TEXT);
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

			cmd = new OleDbCommand ((string) null);
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

		[Test] // OleDbCommand (string, OleDbConnection)
		public void Constructor3 ()
		{
			OleDbConnection conn = new OleDbConnection ();
			OleDbCommand cmd;

			cmd = new OleDbCommand (COMMAND_TEXT, conn);
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

			cmd = new OleDbCommand ((string) null, conn);
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

			cmd = new OleDbCommand (COMMAND_TEXT, (OleDbConnection) null);
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

		[Test] // OleDbCommand (string, OleDbConnection, OleDbTransaction)
		public void Constructor4 ()
		{
			OleDbConnection conn = new OleDbConnection ();
			OleDbCommand cmd;

			cmd = new OleDbCommand (COMMAND_TEXT, conn, (OleDbTransaction) null);
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

			cmd = new OleDbCommand ((string) null, conn, (OleDbTransaction) null);
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

			cmd = new OleDbCommand (COMMAND_TEXT, (OleDbConnection) null, (OleDbTransaction) null);
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
			OleDbCommand cmd = new OleDbCommand ();
			cmd.CommandText = COMMAND_TEXT;
			Assert.AreSame (COMMAND_TEXT, cmd.CommandText, "#1");
			cmd.CommandText = null;
			Assert.AreEqual (string.Empty, cmd.CommandText, "#2");
			cmd.CommandText = COMMAND_TEXT;
			Assert.AreSame (COMMAND_TEXT, cmd.CommandText, "#3");
			cmd.CommandText = string.Empty;
			Assert.AreEqual (string.Empty, cmd.CommandText, "#4");
		}
	}
}

#endif