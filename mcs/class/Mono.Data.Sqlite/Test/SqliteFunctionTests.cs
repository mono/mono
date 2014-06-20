// SqliteFunctionTests.cs - NUnit Test Cases for SqliteFunction
//
// Authors:
//   Rolf Bjarne Kvinge <rolf@xamarin.com>
// 

//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
//

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
	public class SqliteFunctionTest : SqliteUnitTestsBase {
		[Test]
		public void CollationTest ()
		{
			var builder = new SqliteConnectionStringBuilder ();
			builder.DataSource = _uri;

			var connectionString = builder.ToString ();
			using (var connection = new SqliteConnection (connectionString)) {
				connection.Open ();
				connection.Close ();
			}
		}

		[SqliteFunction (Name = "TestCollation", FuncType = FunctionType.Collation)]
		public class TestCollation : SqliteFunction
		{
			public override int Compare (string param1, string param2)
			{
				return string.Compare (param1, param2);
			}
		}

		[Test]
		public void ScalarFunctionTest ()
		{
			var builder = new SqliteConnectionStringBuilder ();
			builder.DataSource = _uri;

			var connectionString = builder.ToString ();
			using (var connection = new SqliteConnection (connectionString)) {
				connection.Open ();
				using (var cmd = connection.CreateCommand ()) {
					cmd.CommandText = "SELECT TestFunction(12, 21);";
					Assert.AreEqual (33L, cmd.ExecuteScalar ());
				}
				connection.Close ();
			}
		}

		[SqliteFunction (Name = "TestFunction", FuncType = FunctionType.Scalar, Arguments = 2)]
		public class TestFunction : SqliteFunction
		{
			public override object Invoke (object[] args)
			{
				return Convert.ToInt32 (args [0]) + Convert.ToInt32 (args [1]);
			}
		}
	}
}
