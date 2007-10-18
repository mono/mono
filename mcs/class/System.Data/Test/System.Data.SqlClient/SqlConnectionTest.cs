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
		[Test]
		public void DefaultConnectionValues()
		{
			SqlConnection cn = new SqlConnection ();

			Assert.AreEqual (15, cn.ConnectionTimeout, 
				"Default connection timeout should be 15 seconds");
			Assert.AreEqual (string.Empty, cn.Database, 
				"Default database name should be empty string");
			Assert.AreEqual (string.Empty, cn.DataSource,
				"Default data source should be empty string");
			Assert.AreEqual (8192, cn.PacketSize,
				"Default packet size should be 8192 bytes");
			Assert.AreEqual (ConnectionState.Closed, cn.State,
				"Default connection state should be closed");
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
	}
}
