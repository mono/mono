// SqliteExceptionUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteExceptions
//
// Author(s):	Thomas Zoechling <thomas.zoechling@gmx.at>


using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite
{
	[TestFixture]
	public class SqliteExceptionUnitTests
	{
		static string _uri;
		static string _connectionString;
		SqliteConnection _conn;

		[SetUp]
		public void SetUp ()
		{
			_uri = Path.GetTempFileName ();
			_connectionString = "URI=file://" + _uri + ", version=3";
			_conn = new SqliteConnection (_connectionString);
		}

		[TearDown]
		public void TearDown ()
		{
			if (File.Exists (_uri))
				File.Delete (_uri);
		}
		
		[Test]
		[ExpectedException(typeof(SqliteException))]
		public void WrongSyntax()
		{
			SqliteCommand insertCmd = new SqliteCommand("INSERT INTO t1 VALUES (,')",_conn);
			using(_conn)
			{
				_conn.Open();
				int res = insertCmd.ExecuteNonQuery();
				Assert.AreEqual(res,1);
			}
		}
	}
}
