// project created on 30/11/2002 at 22:00
using System;
using Npgsql;
using System.Data;

using NUnit.Framework;

namespace NpgsqlTests
{
	
	
		
	[TestFixture]
	public class ConnectionTests
	{
		private NpgsqlConnection 	_conn = null;
		private String 						_connString = "Server=localhost;User ID=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests";
		
		[SetUp]
		protected void SetUp()
		{
			NpgsqlEventLog.Level = LogLevel.None;
			//NpgsqlEventLog.LogName = "NpgsqlTests.LogFile";
			_conn = new NpgsqlConnection(_connString);
		}
		
		[TearDown]
		protected void TearDown()
		{
			_conn.Close();
		}
		
		
		[Test]
		public void Open()
		{
			try{
				_conn.Open();
				//Assertion.AssertEquals("ConnectionOpen", ConnectionState.Open, _conn.State);
			} catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			
			
		}
		
		[Test]
		public void ChangeDatabase()
		{
			_conn.Open();
			
			_conn.ChangeDatabase("template1");
			
			NpgsqlCommand command = new NpgsqlCommand("select current_database()", _conn);
			
			String result = (String)command.ExecuteScalar();
			
			Assertion.AssertEquals("template1", result);
				
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void NestedTransaction()
		{
			_conn.Open();
			
			NpgsqlTransaction t = _conn.BeginTransaction();
			
			t = _conn.BeginTransaction();
			
		}
		
		[Test]
		public void SequencialTransaction()
		{
			_conn.Open();
			
			NpgsqlTransaction t = _conn.BeginTransaction();
			
			t.Rollback();
			
			t = _conn.BeginTransaction();
			
			t.Rollback();
			
			
		}
		
	}
}
