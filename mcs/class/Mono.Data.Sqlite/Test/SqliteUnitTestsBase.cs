// SqliteUnitTestsBase.cs - Base logic for most tests
//
// Author(s):	Matthew Leibowitz <matthew.leibowitz@xamarin.com>

using System;
using System.Data;
using System.IO;
using System.Text;
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
			if (File.Exists (_uri)) {
				// We want to start with a fresh db for each full run
				// The database is created on the first open()
				File.Delete (_uri);
			}
		}

		[SetUp]
		public virtual void Create ()
		{
			Drop ();

			_conn = new SqliteConnection (_connectionString);
		}
	}

	public class SqliteUnitTestsBaseWithT1 : SqliteUnitTestsBase {
		[SetUp]
		public override void Create ()
		{
			base.Create ();

			try {
				using (SqliteCommand createCommand = new SqliteCommand ("CREATE TABLE t1(t  TEXT,  f FLOAT, i INTEGER, b TEXT);", _conn))
				using (SqliteCommand insertCommand = new SqliteCommand ("INSERT INTO t1  (t, f, i, b ) VALUES('" + stringvalue + "',123,123,'123')", _conn)) {
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