// created on 30/11/2002 at 22:35
using System;
using Npgsql;
using NUnit.Framework;
using NUnit.Core;
using System.Data;
using NpgsqlTypes;

namespace NpgsqlTests
{
	
	[TestFixture]
	public class CommandTests
	{
		private NpgsqlConnection 	_conn = null;
		private String 						_connString = "Server=localhost;User ID=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests";
		
		[SetUp]
		protected void SetUp()
		{
			NpgsqlEventLog.Level = LogLevel.None;
			//NpgsqlEventLog.Level = LogLevel.Debug;
			//NpgsqlEventLog.LogName = "NpgsqlTests.LogFile";
			_conn = new NpgsqlConnection(_connString);
		}
		
		[TearDown]
		protected void TearDown()
		{
			if (_conn.State != ConnectionState.Closed)
				_conn.Close();
		}
		
		
		[Test]
		public void ParametersGetName()
		{
			NpgsqlCommand command = new NpgsqlCommand();
			
			// Add parameters.
			command.Parameters.Add(new NpgsqlParameter("Parameter1", DbType.Boolean));
			command.Parameters.Add(new NpgsqlParameter("Parameter2", DbType.Int32));
			command.Parameters.Add(new NpgsqlParameter("Parameter3", DbType.DateTime));
			
			
			// Get by indexers.
			
			Assertion.AssertEquals("ParametersGetName", "Parameter1", command.Parameters["Parameter1"].ParameterName);
			Assertion.AssertEquals("ParametersGetName", "Parameter2", command.Parameters["Parameter2"].ParameterName);
			Assertion.AssertEquals("ParametersGetName", "Parameter3", command.Parameters["Parameter3"].ParameterName);
						                 

			Assertion.AssertEquals("ParametersGetName", "Parameter1", command.Parameters[0].ParameterName);
			Assertion.AssertEquals("ParametersGetName", "Parameter2", command.Parameters[1].ParameterName);
			Assertion.AssertEquals("ParametersGetName", "Parameter3", command.Parameters[2].ParameterName);						             
			
			
			
		}
		
		[Test]
		[ExpectedException(typeof(NpgsqlException))]
		public void NoNameParameterAdd()
		{
			NpgsqlCommand command = new NpgsqlCommand();
			
			command.Parameters.Add(new NpgsqlParameter());
			
		}
		
		[Test]
		public void FunctionCallFromSelect()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select * from funcB()", _conn);
			
			NpgsqlDataReader reader = command.ExecuteReader();
			
			Assertion.AssertNotNull(reader);
			//reader.FieldCount
			
		}
		
		[Test]
		public void ExecuteScalar()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select count(*) from tablea", _conn);
			
			Object result = command.ExecuteScalar();
			
			Assertion.AssertEquals(5, result);
			//reader.FieldCount
			
		}
	
		[Test]
		public void FunctionCallReturnSingleValue()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("funcC()", _conn);
			command.CommandType = CommandType.StoredProcedure;
						
			Object result = command.ExecuteScalar();
			
			Assertion.AssertEquals(5, result);
			//reader.FieldCount
			
		}
		
		
		[Test]
		public void FunctionCallReturnSingleValueWithPrepare()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("funcC()", _conn);
			command.CommandType = CommandType.StoredProcedure;
			
			command.Prepare();
			Object result = command.ExecuteScalar();
			
			Assertion.AssertEquals(5, result);
			//reader.FieldCount
			
		}
		
		[Test]
		public void FunctionCallWithParametersReturnSingleValue()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("funcC(:a)", _conn);
			command.CommandType = CommandType.StoredProcedure;
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));
						
			command.Parameters[0].Value = 4;
						
			Int64 result = (Int64) command.ExecuteScalar();
			
			Assertion.AssertEquals(1, result);
			
			
		}
		
		
		[Test]
		public void FunctionCallWithParametersPrepareReturnSingleValue()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("funcC(:a)", _conn);
			command.CommandType = CommandType.StoredProcedure;
			
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));
			
			Assertion.AssertEquals(1, command.Parameters.Count);
			command.Prepare();
			
			
			command.Parameters[0].Value = 4;
						
			Int64 result = (Int64) command.ExecuteScalar();
			
			Assertion.AssertEquals(1, result);
			
			
		}
		
		[Test]
		public void FunctionCallReturnResultSet()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("funcB()", _conn);
			command.CommandType = CommandType.StoredProcedure;
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			
			
			
		}
		
		
		[Test]
		public void CursorStatement()
		{
			
			_conn.Open();
			
			Int32 i = 0;
			
			NpgsqlTransaction t = _conn.BeginTransaction();
			
			NpgsqlCommand command = new NpgsqlCommand("declare te cursor for select * from tablea;", _conn);
			
			command.ExecuteNonQuery();
			
			command.CommandText = "fetch forward 3 in te;";
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			
			while (dr.Read())
			{
				i++;
			}
			
			Assertion.AssertEquals(3, i);
			
			
			i = 0;
			
			command.CommandText = "fetch backward 1 in te;";
			
			NpgsqlDataReader dr2 = command.ExecuteReader();
			
			while (dr2.Read())
			{
				i++;
			}
			
			Assertion.AssertEquals(1, i);
			
			command.CommandText = "close te;";
			
			command.ExecuteNonQuery();
			
			t.Commit();
			
			
			
		}
		
		[Test]
		public void PreparedStatementNoParameters()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea;", _conn);
			
			command.Prepare();
			
			command.Prepare();
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
						
		}
		
		[Test]
		public void PreparedStatementWithParameters()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_int4 = :a and field_int8 = :b;", _conn);
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));
			command.Parameters.Add(new NpgsqlParameter("b", DbType.Int64));
			
			Assertion.AssertEquals(2, command.Parameters.Count);
			
			Assertion.AssertEquals(DbType.Int32, command.Parameters[0].DbType);
			
			command.Prepare();
			
			command.Parameters[0].Value = 3;
			command.Parameters[1].Value = 5;
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			
			
			
		}
		
		[Test]
		public void DateTimeSupport()
		{
			_conn.Open();
			
			
			NpgsqlCommand command = new NpgsqlCommand("select field_timestamp from tableb where field_serial = 2;", _conn);
			
			DateTime d = (DateTime)command.ExecuteScalar();
			
			
			Assertion.AssertEquals("2002-02-02 09:00:23Z", d.ToString("u"));
			
			
		}
		
		[Test]
		public void NumericSupport()
		{
			_conn.Open();
			
			
			NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_numeric) values (:a)", _conn);
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Decimal));
			
			command.Parameters[0].Value = 7.4M;
			
			Int32 rowsAdded = command.ExecuteNonQuery();
			
			Assertion.AssertEquals(1, rowsAdded);
			
			command.CommandText = "select * from tableb where field_numeric = :a";
			
			
			NpgsqlDataReader dr = command.ExecuteReader();
			dr.Read();
			
			Decimal result = dr.GetDecimal(3);
			
			
			command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
			command.Parameters.Clear();
			command.ExecuteNonQuery();
			
			
			Assertion.AssertEquals(7.4M, result);
			
			
			
			
		}
		
		[Test]
		public void NegativeNumericSupport()
		{
			_conn.Open();
			
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tableb where field_serial = 4", _conn);
			
						
			NpgsqlDataReader dr = command.ExecuteReader();
			dr.Read();
			
			Decimal result = dr.GetDecimal(3);
			
			Assertion.AssertEquals(-4.3M, result);
			
		}
		
		[Test]
		public void PrecisionScaleNumericSupport()
		{
			_conn.Open();
			
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tableb where field_serial = 4", _conn);
			
						
			NpgsqlDataReader dr = command.ExecuteReader();
			dr.Read();
			
			Decimal result = dr.GetDecimal(3);
			
			Assertion.AssertEquals(-4.3M, (Decimal)result);
			//Assertion.AssertEquals(11, result.Precision);
			//Assertion.AssertEquals(7, result.Scale);
			
		}
		
		[Test]
		public void InsertNullString()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values (:a)", _conn);
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.String));
			
			command.Parameters[0].Value = DBNull.Value;
			
			Int32 rowsAdded = command.ExecuteNonQuery();
			
			Assertion.AssertEquals(1, rowsAdded);
			
			command.CommandText = "select count(*) from tablea where field_text is null";
			command.Parameters.Clear();
			
			Int64 result = (Int64)command.ExecuteScalar();
			
			command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea) and field_serial != 4;";
			command.ExecuteNonQuery();
			
			Assertion.AssertEquals(4, result);
			
			
			
		}
	
		[Test]
		public void InsertNullDateTime()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_timestamp) values (:a)", _conn);
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.DateTime));
			
			command.Parameters[0].Value = DBNull.Value;
			
			Int32 rowsAdded = command.ExecuteNonQuery();
			
			Assertion.AssertEquals(1, rowsAdded);
			
			command.CommandText = "select count(*) from tableb where field_timestamp is null";
			command.Parameters.Clear();
			
			Object result = command.ExecuteScalar();
			
			command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
			command.ExecuteNonQuery();
			
			Assertion.AssertEquals(4, result);
			
			
			
		}
		
		
		[Test]
		public void InsertNullInt16()
		{
			_conn.Open();
			
			
			NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_int2) values (:a)", _conn);
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Int16));
			
			command.Parameters[0].Value = DBNull.Value;
			
			Int32 rowsAdded = command.ExecuteNonQuery();
			
			Assertion.AssertEquals(1, rowsAdded);
			
			command.CommandText = "select count(*) from tableb where field_int2 is null";
			command.Parameters.Clear();
			
			Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64
			
			command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
			command.ExecuteNonQuery();
			
			Assertion.AssertEquals(4, result);
			
			
		}
		
		
		[Test]
		public void InsertNullInt32()
		{
			_conn.Open();
			
			
			NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_int4) values (:a)", _conn);
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));
			
			command.Parameters[0].Value = DBNull.Value;
			
			Int32 rowsAdded = command.ExecuteNonQuery();
			
			Assertion.AssertEquals(1, rowsAdded);
			
			command.CommandText = "select count(*) from tablea where field_int4 is null";
			command.Parameters.Clear();
			
			Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64
			
			command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
			command.ExecuteNonQuery();
			
			Assertion.AssertEquals(5, result);
			
		}
		
		
		[Test]
		public void InsertNullNumeric()
		{
			_conn.Open();
			
			
			NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_numeric) values (:a)", _conn);
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Decimal));
			
			command.Parameters[0].Value = DBNull.Value;
			
			Int32 rowsAdded = command.ExecuteNonQuery();
			
			Assertion.AssertEquals(1, rowsAdded);
			
			command.CommandText = "select count(*) from tableb where field_numeric is null";
			command.Parameters.Clear();
			
			Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64
			
			command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
			command.ExecuteNonQuery();
			
			Assertion.AssertEquals(3, result);
			
		}
		
		[Test]
		public void InsertNullBoolean()
		{
			_conn.Open();
			
			
			NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_bool) values (:a)", _conn);
			
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Boolean));
			
			command.Parameters[0].Value = DBNull.Value;
			
			Int32 rowsAdded = command.ExecuteNonQuery();
			
			Assertion.AssertEquals(1, rowsAdded);
			
			command.CommandText = "select count(*) from tablea where field_bool is null";
			command.Parameters.Clear();
			
			Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64
			
			command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
			command.ExecuteNonQuery();
			
			Assertion.AssertEquals(5, result);
			
		}
		
		
		
		
	}
}
