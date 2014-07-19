// SqliteUnitTestsBase.cs - Base logic for most tests
//
// Author(s):	Matthew Leibowitz <matthew.leibowitz@xamarin.com>

using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
#if WINDOWS_STORE_APP
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
#else
using NUnit.Framework;
#endif

namespace MonoTests.Mono.Data.Sqlite {
	public class SqliteUnitTestsBase {
		protected readonly static string _uri = "test.sqlite";
		protected readonly static string _connectionString = "URI=file://" + _uri + ", version=3";
		protected static SqliteConnection _conn = null;
		protected readonly static string stringvalue = "my keyboard is better than yours : äöüß";

		[TearDown]
		public void Drop ()
		{
			if (_conn != null) {
				_conn.Dispose ();
				_conn = null;
			}
			if (SqliteConnection.FileExists (_uri)) {
				// We want to start with a fresh db for each full run
				// The database is created on the first open()
				SqliteConnection.DeleteFile (_uri);
			}
		}

		[SetUp]
		public virtual void Create ()
		{
			Drop ();

			SqliteConnection.CreateFile (_uri);
			_conn = new SqliteConnection (_connectionString);
		}
	}

	public class SqliteUnitTestsBaseWithT1 : SqliteUnitTestsBase {
		[SetUp]
		public override void Create ()
		{
			base.Create ();

			try {
				using (SqliteCommand createCommand = new SqliteCommand ("CREATE TABLE t1 (t TEXT, f FLOAT, i INTEGER, b TEXT);", _conn))
				using (SqliteCommand insertCommand = new SqliteCommand ("INSERT INTO t1 (t, f, i, b ) VALUES ('" + stringvalue + "', 123, 123, '123')", _conn)) {
					_conn.Open ();
					createCommand.ExecuteNonQuery ();
					insertCommand.ExecuteNonQuery ();
				}
			} finally {
				_conn.Close ();
			}
		}
	}
}