// SqliteExceptionUnitTests.cs - NUnit Test Cases for Mono.Data.SqliteClient.SqliteExceptions
//
// Author(s):	Thomas Zoechling <thomas.zoechling@gmx.at>


using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.SqliteClient;
using NUnit.Framework;

namespace MonoTests.Mono.Data.SqliteClient
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
		[ExpectedException(typeof(SqliteSyntaxException))]
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
