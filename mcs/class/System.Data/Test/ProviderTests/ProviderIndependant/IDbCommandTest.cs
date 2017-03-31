// IDbCommandTest.cs - NUnit Test Cases for testing the
// IDbCommand implemented classes.
//
// Authors:
//      Sureshkumar T (tsureshkumar@novell.com)
// 
// Copyright (c) 2004 Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.using System;

using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;

namespace MonoTests.System.Data.Connected
{
	[TestFixture]
	[Category ("odbc"), Category ("sqlserver")]
	public class CommandTest
	{
		IDbConnection conn;
		IDbCommand cmd;

		[SetUp]
		public void SetUp ()
		{
			conn = ConnectionManager.Instance.Sql.Connection;
			cmd = conn.CreateCommand ();
		}

		[TearDown]
		public void TearDown ()
		{
			if (cmd != null)
				cmd.Dispose ();
			ConnectionManager.Instance.Close ();
		}

		[Test]
		public void ExecuteNonQuery_CommandText_Empty ()
		{
			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery: CommandText property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteNonQuery"), "#A5:" + ex.Message);
			}

			cmd.CommandText = string.Empty;

			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery: CommandText property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteNonQuery"), "#B5:" + ex.Message);
			}

			cmd.CommandText = null;

			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// ExecuteNonQuery: CommandText property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteNonQuery"), "#C5:" + ex.Message);
			}
		}

		[Test]
		public void ExecuteReader_CommandText_Empty ()
		{
			try {
				cmd.ExecuteReader ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// ExecuteReader: CommandText property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteReader"), "#A5:" + ex.Message);
			}

			cmd.CommandText = string.Empty;

			try {
				cmd.ExecuteReader ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// ExecuteReader: CommandText property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteReader"), "#B5:" + ex.Message);
			}

			cmd.CommandText = null;

			try {
				cmd.ExecuteReader ();
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// ExecuteReader: CommandText property
				// has not been initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.StartsWith ("ExecuteReader"), "#C5:" + ex.Message);
			}
		}

		[Test] // bug #462947
		public void ExecuteReader_Connection_Reuse ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			CommandBehavior behavior = CommandBehavior.SequentialAccess |
				CommandBehavior.SingleResult;

			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#A1");

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize];
				long ret = reader.GetBytes (0, 0, val, 0, val.Length);
				Assert.AreEqual (5, ret, "#A2");
				Assert.AreEqual (new byte [] { 0x32, 0x56, 0x00, 0x44, 0x22 }, val, "#A3");
			}

			ConnectionManager.Instance.Sql.CloseConnection ();
			conn = ConnectionManager.Instance.Sql.Connection;

			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#B1");

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize];
				long ret = reader.GetBytes (0, 0, val, 0, val.Length);
				Assert.AreEqual (5, ret, "#B2");
				Assert.AreEqual (new byte [] { 0x32, 0x56, 0x00, 0x44, 0x22 }, val, "#B3");
			}

			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#C");
			}

			ConnectionManager.Instance.Sql.CloseConnection ();
			conn = ConnectionManager.Instance.Sql.Connection;


			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#D");
			}

			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#E");
			}
		}

		[Test]
		public void ExecuteScalar ()
		{
			cmd.CommandText = "select count(*) from employee where id < 3";
			Assert.AreEqual (2, (int) Convert.ChangeType (cmd.ExecuteScalar (),
								      typeof (int)),
					 "#1");
			cmd.Dispose ();

			cmd = conn.CreateCommand ();
			cmd.CommandText = "select id from employee where id = 666";
			Assert.IsNull (cmd.ExecuteScalar (), "#2");
		}
	}
}
