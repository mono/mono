// SqliteConvertUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteConvert
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

	[TestFixture]
	public class SqliteConvertUnitTests : SqliteUnitTestsBase {

		[Test]
		public void DateTimeToJulianDayTest ()
		{
			const double janJulian = 2455220.1635897337;
			const double novJulian = 2455523.7885897337;
			
			DateTime janDate = new DateTime (2010, 01, 23, 15, 55, 34, 153);
			DateTime novDate = new DateTime (2010, 11, 23, 06, 55, 34, 153);

			using (_conn)
			using (SqliteCommand janCmd = new SqliteCommand ("SELECT julianday ('2010-01-23 15:55:34.153')", _conn))
			using (SqliteCommand novCmd = new SqliteCommand ("SELECT julianday ('2010-11-23 06:55:34.153')", _conn)) {
				_conn.Open ();

				// get sqlite values
				double janRes = Convert.ToDouble (janCmd.ExecuteScalar ());
				double novRes = Convert.ToDouble (novCmd.ExecuteScalar ());
				
				// make sure all is as planned
				Assert.AreEqual (janJulian, janRes);
				Assert.AreEqual (novJulian, novRes);

				double janConv = SqliteConvert.ToJulianDay (janDate);
				double novConv = SqliteConvert.ToJulianDay (novDate);

				// now check our code
				Assert.AreEqual (janJulian, janConv);
				Assert.AreEqual (novJulian, novConv);
			}
		}

		[Test]
		public void JulianDayToDateTimeTest ()
		{
			const double janJulian = 2455220.1635897337;
			const double novJulian = 2455523.7885897337;
			
			DateTime janDate = new DateTime (2010, 01, 23, 15, 55, 34, 153);
			DateTime novDate = new DateTime (2010, 11, 23, 06, 55, 34, 153);

			using (_conn)
			using (SqliteCommand janCmd = new SqliteCommand ("SELECT strftime ('%Y-%m-%d %H:%M:%f', '2455220.1635897337')", _conn))
			using (SqliteCommand novCmd = new SqliteCommand ("SELECT strftime ('%Y-%m-%d %H:%M:%f', '2455523.7885897337')", _conn)) {
				_conn.Open ();

				// get sqlite values
				DateTime janRes = Convert.ToDateTime (janCmd.ExecuteScalar ());
				DateTime novRes = Convert.ToDateTime (novCmd.ExecuteScalar ());
				
				// make sure all is as planned
				Assert.AreEqual (janDate, janRes);
				Assert.AreEqual (novDate, novRes);

				DateTime janConv = SqliteConvert.ToDateTime (janJulian);
				DateTime novConv = SqliteConvert.ToDateTime (novJulian);

				// now check our code
				Assert.AreEqual (janDate, janConv);
				Assert.AreEqual (novDate, novConv);
			}
		}
	}
}
