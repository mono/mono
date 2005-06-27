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
			Assert.AreEqual ("", cn.Database, 
				"Default database name should be empty string");
			Assert.AreEqual ("", cn.DataSource,
				"Default data source should be empty string");
			Assert.AreEqual (8192, cn.PacketSize,
				"Default packet size should be 8192 bytes");
			Assert.AreEqual (ConnectionState.Closed, cn.State,
				"Default connection state should be closed");
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
			cn.ConnectionString = "Persist Security Info=true";
			cn.ConnectionString = "PersistSecurityInfo=true";
			cn.ConnectionString = "Pooling=true";
			cn.ConnectionString = "User Id=test";
			cn.ConnectionString = "User=test";
			cn.ConnectionString = "Uid=test";
			/*
			 * NOT IMPLEMENTED YET
			 */
			/*
			cn.ConnectionString = "Encrypt=true";
			cn.ConnectionString = "Enlist=false";
			cn.ConnectionString = "attachdbfilename=dunno";
			cn.ConnectionString = "extended properties=dunno";
			cn.ConnectionString = "initial file name=dunno";
			*/
		}
	}
}
