// SqliteExceptionUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteExceptions
//
// Author(s):	Thomas Zoechling <thomas.zoechling@gmx.at>

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
	[TestFixture]
	public class SqliteExceptionUnitTests : SqliteUnitTestsBaseWithT1 {
		[Test]
		public void WrongSyntax ()
		{
			SqliteCommand insertCmd = new SqliteCommand ("INSERT INTO t1 VALUES (,')", _conn);
			using (_conn) {
				_conn.Open ();
				try { 
					int res = insertCmd.ExecuteNonQuery ();
					insertCmd.Dispose ();
					Assert.Fail ("Expected an exception due to incorrect syntax");
				} catch (SqliteException) {
				}
			}
		}
	}
}
