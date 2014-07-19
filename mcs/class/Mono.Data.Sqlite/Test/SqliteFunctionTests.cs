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

		[SqliteFunction(Name = "TestScalar", FuncType = FunctionType.Scalar)]
		public class TestScalar : SqliteFunction
		{
			public override object Invoke (object[] args)
			{
				return null;
			}
		}

		[SqliteFunction(Name = "TestAggregate", FuncType = FunctionType.Aggregate)]
		public class TestAggregate : SqliteFunction
		{
			public override void Step(object[] args, int stepNumber, ref object contextData)
			{
			}

			public override object Final (object contextData)
			{
				return null;
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
