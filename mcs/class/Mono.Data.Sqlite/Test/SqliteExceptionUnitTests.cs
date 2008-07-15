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
		readonly static string _uri = "SqliteTest.db";
		readonly static string _connectionString = "URI=file://" + _uri + ", version=3";
		static SqliteConnection _conn = new SqliteConnection (_connectionString);

		public SqliteExceptionUnitTests()
		{
		}
		
		[Test]
#if NET_2_0
		[ExpectedException(typeof(SqliteException))]
#else
		[ExpectedException(typeof(SqliteSyntaxException))]
#endif
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
