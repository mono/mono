// SqliteConnectionTest.cs - NUnit Test Cases for SqliteConnection
//
// Authors:
//   Sureshkumar T <tsureshkumar@novell.com>
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.IO;
using Mono.Data.Sqlite;
#if USE_MSUNITTEST
#if WINDOWS_PHONE || NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
#else // !WINDOWS_PHONE && !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute;
#endif // WINDOWS_PHONE || NETFX_CORE
#else // !USE_MSUNITTEST
using NUnit.Framework;
#endif // USE_MSUNITTEST

namespace MonoTests.Mono.Data.Sqlite {
	[TestFixture]
	public class SqliteConnectionTest : SqliteUnitTestsBase {
		[Test]
		[Category ("NotWorking")]
		public void ReleaseDatabaseFileHandles ()
		{
			string filename = "test_" + Guid.NewGuid ().ToString ("N") + ".db";
			// open connection
			SqliteConnection conn = new SqliteConnection ("Data Source=" + filename);
			conn.Open ();
			// start command
			SqliteCommand cmd = (SqliteCommand)conn.CreateCommand ();
			cmd.CommandText = "PRAGMA legacy_file_format;";
			cmd.ExecuteScalar ();
			// close connection before
			conn.Dispose ();
			// close command after
			cmd.Dispose ();
			// ensure file is not locked
			SqliteConnection.DeleteFile (filename);
		}		

		[Test]
		public void ConnectionStringTest_Null ()
		{
			try {
				_conn.ConnectionString = null;
				Assert.Fail ("Expected exception of type ArgumentNullException");
			} catch (ArgumentNullException) {
			} catch (Exception ex) {
				Assert.Fail ("Expected exception of type ArgumentNullException, but was {0}", ex);
			}
		}

		[Test]
		public void ConnectionStringTest_MustBeClosed ()
		{
			_conn.ConnectionString = _connectionString;
			try {
				_conn.Open ();
				try {
					_conn.ConnectionString = _connectionString;
					Assert.Fail ("Expected exception of type InvalidOperationException");
				} catch (InvalidOperationException) {
				} catch (Exception ex) {
					Assert.Fail ("Expected exception of type InvalidOperationException, but was {0}", ex);
				}
			} finally {
				_conn.Close ();
			}
		}

		[Test]
		public void ConnectionStringTest_UsingDataDirectory ()
		{
			_conn.ConnectionString = "Data Source=|DataDirectory|/test.db, Version=3";
			try {
				_conn.Open ();
			} finally {
				_conn.Close ();
			}
		}

		[Test]
		public void ConnectionStringTest_UsingLocalDirectory ()
		{
			_conn.ConnectionString = "Data Source=./test.db, Version=3";
			try {
				_conn.Open ();
			} finally {
				_conn.Close ();
			}
		}

		[Test]
		public void OpenTest ()
		{
			try {
				_conn.ConnectionString = _connectionString;
				_conn.Open ();
				Assert.AreEqual (ConnectionState.Open, _conn.State, "#1 not opened");
				_conn.Close ();

				// negative test: try opening a non-existent file
				_conn.ConnectionString = "URI=file://abcdefgh.db, version=3, FailIfMissing=True";
				try {
					_conn.Open ();
					Assert.Fail ("#2 should have failed on opening a non-existent db");
				} catch (SqliteException e) {
					// expected
				} catch (Exception e) {
					Assert.Fail ("#3 should not have thrown this one: {0}", e);
				}
                                
			} finally {
				if (_conn != null && _conn.State != ConnectionState.Closed)
					_conn.Close ();
			}
		}

	}
}
