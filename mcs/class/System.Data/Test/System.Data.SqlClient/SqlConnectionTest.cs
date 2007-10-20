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
#if NET_2_0
			Assert.IsFalse (cn.FireInfoMessageEventOnUserErrors, "#6");
			Assert.AreEqual (8000, cn.PacketSize, "#7");
#else
			Assert.AreEqual (8192, cn.PacketSize, "#7");
#endif
			Assert.IsNull (cn.Site, "#8");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#9");
#if NET_2_0
			Assert.IsFalse (cn.StatisticsEnabled, "#10");
#endif
			Assert.AreEqual (Environment.MachineName, cn.WorkstationId, "#11");
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
#if NET_2_0
			Assert.IsFalse (cn.FireInfoMessageEventOnUserErrors, "#A6");
			Assert.AreEqual (8000, cn.PacketSize, "#A7");
#else
			Assert.AreEqual (8192, cn.PacketSize, "#A7");
#endif
			Assert.IsNull (cn.Site, "#A8");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#A9");
#if NET_2_0
			Assert.IsFalse (cn.StatisticsEnabled, "#A10");
#endif
			Assert.AreEqual (Environment.MachineName, cn.WorkstationId, "#A11");

			cn = new SqlConnection ((string) null);
			Assert.AreEqual (string.Empty, cn.ConnectionString, "#B1");
			Assert.AreEqual (15, cn.ConnectionTimeout, "#B2");
			Assert.IsNull (cn.Container, "#B3");
			Assert.AreEqual (string.Empty, cn.Database, "#B4");
			Assert.AreEqual (string.Empty, cn.DataSource, "#B5");
#if NET_2_0
			Assert.IsFalse (cn.FireInfoMessageEventOnUserErrors, "#B6");
			Assert.AreEqual (8000, cn.PacketSize, "#B7");
#else
			Assert.AreEqual (8192, cn.PacketSize, "#B7");
#endif
			Assert.IsNull (cn.Site, "#B8");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#B9");
#if NET_2_0
			Assert.IsFalse (cn.StatisticsEnabled, "#B10");
#endif
			Assert.AreEqual (Environment.MachineName, cn.WorkstationId, "#B11");
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
		public void ConnectionTimeoutSynonyms()
		{
			SqlConnection cn = null;

			cn = new SqlConnection ();
			cn.ConnectionString = "Connection Timeout=25";
			Assert.AreEqual (25, cn.ConnectionTimeout);

			cn = new SqlConnection ();
			cn.ConnectionString = "Connect Timeout=25";
			Assert.AreEqual (25, cn.ConnectionTimeout);

			cn = new SqlConnection ();
			cn.ConnectionString = "Timeout=25";
			Assert.AreEqual (25, cn.ConnectionTimeout);
		}

#if NET_2_0
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
#endif

		[Test]
		public void NetworkLibrarySynonyms()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Net=DBMSSOCN";
			cn.ConnectionString = "Network=DBMSSOCN";
			cn.ConnectionString = "Network library=DBMSSOCN";
		}

		[Test]
		public void DatabaseSynonyms()
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
		public void DataSourceSynonyms()
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
		public void OtherConnectionStringKeywords()
		{
			SqlConnection cn = new SqlConnection ();
			cn.ConnectionString = "Application Name=test";
			cn.ConnectionString = "App=test";
			cn.ConnectionString = "Connection LifeTime=1000";
			cn.ConnectionString = "Connection Reset=true";
			cn.ConnectionString = "Current Language=test";
			cn.ConnectionString = "Language=test";
			cn.ConnectionString = "Encrypt=false";
			cn.ConnectionString = "Enlist=true";
			cn.ConnectionString = "Integrated Security=true";
			cn.ConnectionString = "Trusted_connection=true";
			cn.ConnectionString = "Max Pool Size=10";
			cn.ConnectionString = "Min Pool Size=10";
			cn.ConnectionString = "Password=scrambled";
			cn.ConnectionString = "Pwd=scrambled";
			cn.ConnectionString = "Pooling=true";
			cn.ConnectionString = "User Id=test";
			cn.ConnectionString = "User=test";
			cn.ConnectionString = "Uid=test";
			/*
			 * NOT IMPLEMENTED YET
			 */
			/*
			cn.ConnectionString = "Persist Security Info=true";
			cn.ConnectionString = "PersistSecurityInfo=true";
			cn.ConnectionString = "Encrypt=true";
			cn.ConnectionString = "Enlist=false";
			cn.ConnectionString = "attachdbfilename=dunno";
			cn.ConnectionString = "extended properties=dunno";
			cn.ConnectionString = "initial file name=dunno";
			*/
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
