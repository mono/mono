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
using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite
{
	[TestFixture]
	public class SqliteFunctionTest
	{
		string uri;

		[SetUp]
		public void SetUp ()
		{
			uri = Path.GetTempFileName ();
		}

		[TearDown]
		public void TearDown ()
		{
			if (File.Exists (uri))
				File.Delete (uri);
		}

		[Test]
		public void CollationTest()
		{
			var builder = new SqliteConnectionStringBuilder();
			builder.DataSource = uri;

			var connectionString = builder.ToString();
			using (var connection = new SqliteConnection (connectionString)) {
				connection.Open ();
				connection.Close ();
			}
		}

		[SqliteFunction(Name = "TestCollation", FuncType = FunctionType.Collation)]
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
	}
}
