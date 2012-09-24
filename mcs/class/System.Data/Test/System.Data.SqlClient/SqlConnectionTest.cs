//
// SqlConnectionTest.cs - NUnit Test Cases for testing the
//                        SqlConnection class
// Author:
//      Gert Driesen (drieseng@users.sourceforge.net)
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
using System.Data;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlConnectionTest
	{
		[Test] // SqlConnection ()
		public void Constructor1 ()
		{
			SqlConnection cn = new SqlConnection ();

			Assert.AreEqual (string.Empty, cn.ConnectionString, "#1");
			Assert.AreEqual (15, cn.ConnectionTimeout, "#2");
			Assert.IsNull (cn.Container, "#3");
			Assert.AreEqual (string.Empty, cn.Database, "#4");
			Assert.AreEqual (string.Empty, cn.DataSource, "#5");
			Assert.IsFalse (cn.FireInfoMessageEventOnUserErrors, "#6");
			Assert.AreEqual (8000, cn.PacketSize, "#7");
			Assert.IsNull (cn.Site, "#8");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#9");
			Assert.IsFalse (cn.StatisticsEnabled, "#10");
			Assert.IsTrue (string.Compare (Environment.MachineName, cn.WorkstationId, true) == 0, "#11");
		}

		[Test] // SqlConnection (string)
		public void Constructor2 ()
		{
			string connectionString = "server=SQLSRV; database=Mono;";

			SqlConnection cn = new SqlConnection (connectionString);
			Assert.AreEqual (connectionString, cn.ConnectionString, "#A1");
			Assert.AreEqual (15, cn.ConnectionTimeout, "#A2");
			Assert.IsNull (cn.Container, "#A3");
			Assert.AreEqual ("Mono", cn.Database, "#A4");
			Assert.AreEqual ("SQLSRV", cn.DataSource, "#A5");
			Assert.IsFalse (cn.FireInfoMessageEventOnUserErrors, "#A6");
			Assert.AreEqual (8000, cn.PacketSize, "#A7");
			Assert.IsNull (cn.Site, "#A8");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#A9");
			Assert.IsFalse (cn.StatisticsEnabled, "#A10");
			Assert.IsTrue (string.Compare (Environment.MachineName, cn.WorkstationId, true) == 0, "#A11");

			cn = new SqlConnection ((string) null);
			Assert.AreEqual (string.Empty, cn.ConnectionString, "#B1");
			Assert.AreEqual (15, cn.ConnectionTimeout, "#B2");
			Assert.IsNull (cn.Container, "#B3");
			Assert.AreEqual (string.Empty, cn.Database, "#B4");
			Assert.AreEqual (string.Empty, cn.DataSource, "#B5");
			Assert.IsFalse (cn.FireInfoMessageEventOnUserErrors, "#B6");
			Assert.AreEqual (8000, cn.PacketSize, "#B7");
			Assert.IsNull (cn.Site, "#B8");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#B9");
			Assert.IsFalse (cn.StatisticsEnabled, "#B10");
			Assert.IsTrue (string.Compare (Environment.MachineName, cn.WorkstationId, true) == 0, "#B11");
		}

		[Test]
		public void Constructor2_ConnectionString_Invalid ()
		{
			try {
				new SqlConnection ("InvalidConnectionString");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Format of the initialization string does
				// not conform to specification starting at
				// index 0
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// invalid keyword
			try {
				new SqlConnection ("invalidKeyword=10");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Keyword not supported: 'invalidkeyword'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'invalidkeyword'") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}

			// invalid packet size (< minimum)
			try {
				new SqlConnection ("Packet Size=511");
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Invalid 'Packet Size'.  The value must be an
				// integer >= 512 and <= 32768
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}

			// invalid packet size (> maximum)
			try {
				new SqlConnection ("Packet Size=32769");
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Invalid 'Packet Size'.  The value must be an
				// integer >= 512 and <= 32768
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNull (ex.ParamName, "#D5");
			}

			// negative connect timeout
			try {
				new SqlConnection ("Connect Timeout=-1");
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'connect timeout'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsNull (ex.ParamName, "#E5");
			}

			// negative max pool size
			try {
				new SqlConnection ("Max Pool Size=-1");
				Assert.Fail ("#F1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'max pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsNull (ex.ParamName, "#F5");
			}

			// negative min pool size
			try {
				new SqlConnection ("Min Pool Size=-1");
				Assert.Fail ("#G1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'min pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
				Assert.IsNull (ex.ParamName, "#G5");
			}
		}

		[Test]
		public void BeginTransaction_Connection_Closed ()
		{
			SqlConnection cn = new SqlConnection ();

			try {
				cn.BeginTransaction ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				cn.BeginTransaction ((IsolationLevel) 666);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			try {
				cn.BeginTransaction (IsolationLevel.Serializable);
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			try {
				cn.BeginTransaction ("trans");
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
			}

			try {
				cn.BeginTransaction ((IsolationLevel) 666, "trans");
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
			}

			try {
				cn.BeginTransaction (IsolationLevel.Serializable, "trans");
				Assert.Fail ("#F1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
			}
		}

		[Test]
		public void ChangeDatabase_Connection_Closed ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "server=SQLSRV";

			try {
				cn.ChangeDatabase ("database");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void ChangePassword_ConnectionString_Empty ()
		{
			try {
				SqlConnection.ChangePassword (string.Empty, "mono");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.IsTrue (ex.ParamName.IndexOf ("'connectionString'") != -1, "#6");
			}
		}

		[Test]
		public void ChangePassword_ConnectionString_Null ()
		{
			try {
				SqlConnection.ChangePassword ((string) null, "mono");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.IsTrue (ex.ParamName.IndexOf ("'connectionString'") != -1, "#6");
			}
		}

		[Test]
		public void ChangePassword_NewPassword_Empty ()
		{
			try {
				SqlConnection.ChangePassword ("server=SQLSRV", string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.IsTrue (ex.ParamName.IndexOf ("'newPassword'") != -1, "#6");
			}
		}

		[Test]
		public void ChangePassword_NewPassword_ExceedMaxLength ()
		{
			try {
				SqlConnection.ChangePassword ("server=SQLSRV",
					new string ('d', 129));
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The length of argument 'newPassword' exceeds
				// it's limit of '128'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'newPassword'") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("128") != -1, "#6");
				Assert.IsNull (ex.ParamName, "#7");
			}
		}

		[Test]
		public void ChangePassword_NewPassword_Null ()
		{
			try {
				SqlConnection.ChangePassword ("server=SQLSRV", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.IsTrue (ex.ParamName.IndexOf ("'newPassword'") != -1, "#6");
			}
		}

		[Test]
		public void ClearPool_Connection_Null ()
		{
			try {
				SqlConnection.ClearPool ((SqlConnection) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("connection", ex.ParamName, "#5");
			}
		}

		[Test]
		public void ConnectionString ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "server=SQLSRV";
			Assert.AreEqual ("server=SQLSRV", cn.ConnectionString, "#1");
			cn.ConnectionString = null;
			Assert.AreEqual (string.Empty, cn.ConnectionString, "#2");
			cn.ConnectionString = "server=SQLSRV";
			Assert.AreEqual ("server=SQLSRV", cn.ConnectionString, "#3");
			cn.ConnectionString = string.Empty;
			Assert.AreEqual (string.Empty, cn.ConnectionString, "#4");
		}

		[Test]
		public void ConnectionString_Value_Invalid ()
		{
			SqlConnection cn = new SqlConnection ();

			try {
				cn.ConnectionString = "InvalidConnectionString";
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Format of the initialization string does
				// not conform to specification starting at
				// index 0
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// invalid keyword
			try {
				cn.ConnectionString = "invalidKeyword=10";
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Keyword not supported: 'invalidkeyword'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'invalidkeyword'") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}
		}

		[Test]
		public void CreateCommand ()
		{
			SqlConnection cn = new SqlConnection ();
			SqlCommand cmd = cn.CreateCommand ();
			Assert.IsNotNull (cmd, "#1");
			Assert.AreEqual (string.Empty, cmd.CommandText, "#2");
			Assert.AreEqual (30, cmd.CommandTimeout, "#3");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, "#4");
			Assert.AreSame (cn, cmd.Connection, "#5");
			Assert.IsNull (cmd.Container, "#6");
			Assert.IsTrue (cmd.DesignTimeVisible, "#7");
			Assert.IsNull (cmd.Notification, "#8");
			Assert.IsTrue (cmd.NotificationAutoEnlist, "#9");
			Assert.IsNotNull (cmd.Parameters, "#10");
			Assert.AreEqual (0, cmd.Parameters.Count, "#11");
			Assert.IsNull (cmd.Site, "#12");
			Assert.IsNull (cmd.Transaction, "#13");
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource, "#14");
		}

		[Test]
		public void Dispose ()
		{
			SqlConnection cn = new SqlConnection ("Server=SQLSRV;Database=master;Timeout=25;Packet Size=512;Workstation ID=DUMMY");
			cn.Dispose ();

			Assert.AreEqual (string.Empty, cn.ConnectionString, "#1");
			Assert.AreEqual (15, cn.ConnectionTimeout, "#2");
			Assert.AreEqual (string.Empty, cn.Database, "#3");
			Assert.AreEqual (string.Empty, cn.DataSource, "#4");
			Assert.AreEqual (8000, cn.PacketSize, "#5");
			Assert.IsTrue (string.Compare (Environment.MachineName, cn.WorkstationId, true) == 0, "#6");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#7");
			cn.Dispose ();

			cn = new SqlConnection ();
			cn.Dispose ();
		}

		[Test]
		public void GetSchema_Connection_Closed ()
		{
			SqlConnection cn = new SqlConnection ();

			try {
				cn.GetSchema ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			try {
				cn.GetSchema ("Tables");
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			try {
				cn.GetSchema ((string) null);
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			try {
				cn.GetSchema ("Tables", new string [] { "master" });
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
			}

			try {
				cn.GetSchema ((string) null, new string [] { "master" });
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
			}

			try {
				cn.GetSchema ("Tables", (string []) null);
				Assert.Fail ("#F1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
			}

			try {
				cn.GetSchema ((string) null, (string []) null);
				Assert.Fail ("#G1");
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
			}
		}

		[Test]
		public void ConnectionString_AsynchronousProcessing ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Asynchronous Processing=False";
			cn.ConnectionString = "Async=True";
		}

		[Test]
		public void ConnectionString_ConnectTimeout ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Connection Timeout=45";
			Assert.AreEqual (45, cn.ConnectionTimeout, "#1");
			cn.ConnectionString = "Connect Timeout=40";
			Assert.AreEqual (40, cn.ConnectionTimeout, "#2");
			cn.ConnectionString = "Timeout=";
			Assert.AreEqual (15, cn.ConnectionTimeout, "#3");
			cn.ConnectionString = "Timeout=2147483647";
			Assert.AreEqual (int.MaxValue, cn.ConnectionTimeout, "#4");
			cn.ConnectionString = "Timeout=0";
			Assert.AreEqual (0, cn.ConnectionTimeout, "#5");
		}

		[Test]
		public void ConnectionString_ConnectTimeout_Invalid ()
		{
			SqlConnection cn = new SqlConnection ();

			// negative number
			try {
				cn.ConnectionString = "Connection timeout=-1";
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'connect timeout'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'connect timeout'") != -1, "#A5:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#A6");
			}

			// invalid number
			try {
				cn.ConnectionString = "connect Timeout=BB";
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'connect timeout'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'connect timeout'") != -1, "#B6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#B7");

				// Input string was not in a correct format
				FormatException fe = (FormatException) ex.InnerException;
				Assert.IsNull (fe.InnerException, "#B8");
				Assert.IsNotNull (fe.Message, "#B9");
			}

			// overflow
			try {
				cn.ConnectionString = "timeout=2147483648";
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'connect timeout'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("'connect timeout'") != -1, "#C6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#C7");

				// Value was either too large or too small for an Int32
				OverflowException oe = (OverflowException) ex.InnerException;
				Assert.IsNull (oe.InnerException, "#C8");
				Assert.IsNotNull (oe.Message, "#C9");
			}
		}

		[Test]
		public void ConnectionString_Database_Synonyms ()
		{
			SqlConnection cn = null;

			cn = new SqlConnection ();
			cn.ConnectionString = "Initial Catalog=db";
			Assert.AreEqual ("db", cn.Database);

			cn = new SqlConnection ();
			cn.ConnectionString = "Database=db";
			Assert.AreEqual ("db", cn.Database);
		}

		[Test]
		public void ConnectionString_DataSource_Synonyms ()
		{
			SqlConnection cn = null;

			cn = new SqlConnection ();
			cn.ConnectionString = "Data Source=server";
			Assert.AreEqual ("server", cn.DataSource);

			cn = new SqlConnection ();
			cn.ConnectionString = "addr=server";
			Assert.AreEqual ("server", cn.DataSource);

			cn = new SqlConnection ();
			cn.ConnectionString = "address=server";
			Assert.AreEqual ("server", cn.DataSource);

			cn = new SqlConnection ();
			cn.ConnectionString = "network address=server";
			Assert.AreEqual ("server", cn.DataSource);

			cn = new SqlConnection ();
			cn.ConnectionString = "server=server";
			Assert.AreEqual ("server", cn.DataSource);
		}

		[Test]
		public void ConnectionString_MaxPoolSize ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Max Pool Size=2147483647";
			cn.ConnectionString = "Max Pool Size=1";
			cn.ConnectionString = "Max Pool Size=500";
		}

		[Test]
		public void ConnectionString_MaxPoolSize_Invalid ()
		{
			SqlConnection cn = new SqlConnection ();

			// negative number
			try {
				cn.ConnectionString = "Max Pool Size=-1";
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'max pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'max pool size'") != -1, "#A5:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#A6");
			}

			// invalid number
			try {
				cn.ConnectionString = "max Pool size=BB";
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'max pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'max pool size'") != -1, "#B6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#B7");

				// Input string was not in a correct format
				FormatException fe = (FormatException) ex.InnerException;
				Assert.IsNull (fe.InnerException, "#B8");
				Assert.IsNotNull (fe.Message, "#B9");
			}

			// overflow
			try {
				cn.ConnectionString = "max pool size=2147483648";
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'max pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("'max pool size'") != -1, "#C6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#C7");

				// Value was either too large or too small for an Int32
				OverflowException oe = (OverflowException) ex.InnerException;
				Assert.IsNull (oe.InnerException, "#C8");
				Assert.IsNotNull (oe.Message, "#C9");
			}

			// less than minimum (1)
			try {
				cn.ConnectionString = "Min Pool Size=0;Max Pool Size=0";
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'max pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'max pool size'") != -1, "#D5:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#D6");
			}

			// less than min pool size
			try {
				cn.ConnectionString = "Min Pool Size=5;Max Pool Size=4";
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// Invalid min or max pool size values, min
				// pool size cannot be greater than the max
				// pool size
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsNull (ex.ParamName, "#E5");
			}
		}

		[Test]
		public void ConnectionString_MinPoolSize ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "min Pool size=0";
			cn.ConnectionString = "Min Pool size=100";
			cn.ConnectionString = "Min Pool Size=2147483647;Max Pool Size=2147483647";
		}

		[Test]
		public void ConnectionString_MinPoolSize_Invalid ()
		{
			SqlConnection cn = new SqlConnection ();

			// negative number
			try {
				cn.ConnectionString = "Min Pool Size=-1";
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'min pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'min pool size'") != -1, "#A5:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#A6");
			}

			// invalid number
			try {
				cn.ConnectionString = "min Pool size=BB";
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'min pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'min pool size'") != -1, "#B6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#B7");

				// Input string was not in a correct format
				FormatException fe = (FormatException) ex.InnerException;
				Assert.IsNull (fe.InnerException, "#B8");
				Assert.IsNotNull (fe.Message, "#B9");
			}

			// overflow
			try {
				cn.ConnectionString = "min pool size=2147483648";
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'min pool size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("'min pool size'") != -1, "#C6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#C7");

				// Value was either too large or too small for an Int32
				OverflowException oe = (OverflowException) ex.InnerException;
				Assert.IsNull (oe.InnerException, "#C8");
				Assert.IsNotNull (oe.Message, "#C9");
			}
		}

		[Test]
		public void ConnectionString_MultipleActiveResultSets ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "MultipleActiveResultSets=true";
		}

		[Test]
		public void ConnectionString_MultipleActiveResultSets_Invalid ()
		{
			SqlConnection cn = new SqlConnection ();
			try {
				cn.ConnectionString = "MultipleActiveResultSets=1";
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'multipleactiveresultsets'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'multipleactiveresultsets'") != -1, "#5:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#6");
			}
		}

		[Test]
		public void ConnectionString_NetworkLibrary_Synonyms ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Net=DBMSSOCN";
			cn.ConnectionString = "Network=DBMSSOCN";
			cn.ConnectionString = "Network library=DBMSSOCN";
		}

		[Test]
		public void ConnectionString_PacketSize ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Packet Size=1024";
			Assert.AreEqual (1024, cn.PacketSize, "#1");
			cn.ConnectionString = "packet SizE=533";
			Assert.AreEqual (533, cn.PacketSize, "#2");
			cn.ConnectionString = "packet SizE=512";
			Assert.AreEqual (512, cn.PacketSize, "#3");
			cn.ConnectionString = "packet SizE=32768";
			Assert.AreEqual (32768, cn.PacketSize, "#4");
			cn.ConnectionString = "packet Size=";
			Assert.AreEqual (8000, cn.PacketSize, "#5");
		}

		[Test]
		public void ConnectionString_PacketSize_Invalid ()
		{
			SqlConnection cn = new SqlConnection ();

			// invalid packet size (< minimum)
			try {
				cn.ConnectionString = "Packet Size=511";
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid 'Packet Size'.  The value must be an
				// integer >= 512 and <= 32768
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'Packet Size'") != -1, "#A5:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#A6");
			}

			// invalid packet size (> maximum)
			try {
				cn.ConnectionString = "packet SIze=32769";
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid 'Packet Size'.  The value must be an
				// integer >= 512 and <= 32768
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'Packet Size'") != -1, "#B5:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#B6");
			}

			// overflow
			try {
				cn.ConnectionString = "packet SIze=2147483648";
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'packet size'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("'packet size'") != -1, "#C6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#C7");

				// Value was either too large or too small for an Int32
				OverflowException oe = (OverflowException) ex.InnerException;
				Assert.IsNull (oe.InnerException, "#C8");
				Assert.IsNotNull (oe.Message, "#C9");
			}
		}

		[Test]
		public void ConnectionString_Password_Synonyms ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Password=scrambled";
			cn.ConnectionString = "Pwd=scrambled";
		}

		[Test]
		public void ConnectionString_PersistSecurityInfo_Synonyms ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Persist Security Info=true";
			cn.ConnectionString = "PersistSecurityInfo=true";
		}

		[Test]
		public void ConnectionString_UserID_Synonyms ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "User Id=test";
			cn.ConnectionString = "User=test";
			cn.ConnectionString = "Uid=test";
		}

		[Test]
		public void ConnectionString_UserInstance ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "User Instance=true";
		}

		[Test]
		public void ConnectionString_UserInstance_Invalid ()
		{
			SqlConnection cn = new SqlConnection ();
			try {
				cn.ConnectionString = "User Instance=1";
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value for key 'user instance'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'user instance'") != -1, "#5:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#6");
			}
		}

		[Test]
		public void ConnectionString_OtherKeywords ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Application Name=test";
			cn.ConnectionString = "App=test";
			cn.ConnectionString = "Connection Reset=true";
			cn.ConnectionString = "Current Language=test";
			cn.ConnectionString = "Language=test";
			cn.ConnectionString = "Encrypt=false";
			//cn.ConnectionString = "Encrypt=true";
			//cn.ConnectionString = "Enlist=false";
			cn.ConnectionString = "Enlist=true";
			cn.ConnectionString = "Integrated Security=true";
			cn.ConnectionString = "Trusted_connection=true";
			cn.ConnectionString = "Max Pool Size=10";
			cn.ConnectionString = "Min Pool Size=10";
			cn.ConnectionString = "Pooling=true";
			cn.ConnectionString = "attachdbfilename=dunno";
			cn.ConnectionString = "extended properties=dunno";
			cn.ConnectionString = "initial file name=dunno";
		}

		[Test]
		public void Open_ConnectionString_Empty ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = string.Empty;

			try {
				cn.Open ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The ConnectionString property has not been
				// initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Open_ConnectionString_Null ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = null;

			try {
				cn.Open ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The ConnectionString property has not been
				// initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Open_ConnectionString_Whitespace ()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "    ";

			try {
				cn.Open ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The ConnectionString property has not been
				// initialized
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void ServerVersion_Connection_Closed ()
		{
			SqlConnection cn = new SqlConnection ();
			try {
				Assert.Fail ("#A1:" + cn.ServerVersion);
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			cn = new SqlConnection ("server=SQLSRV; database=Mono;");
			try {
				Assert.Fail ("#B1:" + cn.ServerVersion);
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}
	}
}
