//
// OleDbConnectionTest.cs - NUnit Test Cases for testing the
//                          OleDbConnectionTest class
// Author:
//      Gert Driesen (drieseng@users.sourceforge.net)
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
using System;
using System.Data;
using System.Data.OleDb;

using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbConnectionTest
	{
		const string CONNECTION_STRING = "Provider=sqloledb;Data Source=SQLSRV;Initial Catalog=Mono;";

		[Test] // OleDbConnection ()
		public void Constructor1 ()
		{
			OleDbConnection cn = new OleDbConnection ();

			Assert.AreEqual (string.Empty, cn.ConnectionString, "#1");
			Assert.AreEqual (15, cn.ConnectionTimeout, "#2");
			Assert.IsNull (cn.Container, "#3");
			Assert.AreEqual (string.Empty, cn.Database, "#4");
			Assert.AreEqual (string.Empty, cn.DataSource, "#5");
			Assert.AreEqual (string.Empty, cn.Provider, "#6");
			Assert.IsNull (cn.Site, "#7");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#8");
		}

		[Test] // OleDbConnection (string)
		public void Constructor2 ()
		{
			OleDbConnection cn = new OleDbConnection (CONNECTION_STRING);
			Assert.AreEqual (CONNECTION_STRING, cn.ConnectionString, "#A1");
			Assert.AreEqual (15, cn.ConnectionTimeout, "#A2");
			Assert.IsNull (cn.Container, "#A3");
			//Assert.AreEqual ("Mono", cn.Database, "#A4");
			//Assert.AreEqual ("SQLSRV", cn.DataSource, "#A5");
			//Assert.AreEqual ("sqloledb", cn.Provider, "#A6");
			Assert.IsNull (cn.Site, "#A7");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#A8");

			cn = new OleDbConnection ((string) null);
			Assert.AreEqual (string.Empty, cn.ConnectionString, "#B1");
			Assert.AreEqual (15, cn.ConnectionTimeout, "#B2");
			Assert.IsNull (cn.Container, "#B3");
			Assert.AreEqual (string.Empty, cn.Database, "#B4");
			Assert.AreEqual (string.Empty, cn.DataSource, "#B5");
			Assert.AreEqual (string.Empty, cn.Provider, "#B6");
			Assert.IsNull (cn.Site, "#B7");
			Assert.AreEqual (ConnectionState.Closed, cn.State, "#B8");
		}

		[Test]
		public void BeginTransaction_Connection_Closed ()
		{
			OleDbConnection cn = new OleDbConnection ();

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
		}

		[Test]
		public void ConnectionString ()
		{
			OleDbConnection cn = new OleDbConnection ();
			cn.ConnectionString = CONNECTION_STRING;
			Assert.AreEqual (CONNECTION_STRING, cn.ConnectionString, "#1");
			cn.ConnectionString = null;
			Assert.AreEqual (string.Empty, cn.ConnectionString, "#2");
			cn.ConnectionString = CONNECTION_STRING;
			Assert.AreEqual (CONNECTION_STRING, cn.ConnectionString, "#3");
			cn.ConnectionString = string.Empty;
			Assert.AreEqual (string.Empty, cn.ConnectionString, "#4");
		}

		[Test]
		public void GetSchema_Connection_Closed ()
		{
			OleDbConnection cn = new OleDbConnection ();

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
		public void ServerVersion_Connection_Closed ()
		{
			OleDbConnection cn = new OleDbConnection ();
			try {
				Assert.Fail ("#A1:" + cn.ServerVersion);
			} catch (InvalidOperationException ex) {
				// Invalid operation. The connection is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			cn = new OleDbConnection (CONNECTION_STRING);
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

#endif