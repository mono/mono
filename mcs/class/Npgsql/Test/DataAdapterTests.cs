// created on 3/5/2003 at 14:29

using System;
using System.Data;
using System.Web.UI.WebControls;
using Npgsql;

using NpgsqlTypes;

using NUnit.Framework;
using NUnit.Core;

namespace NpgsqlTests
{
	
	[TestFixture]
	public class DataAdapterTests
	{
		
		private NpgsqlConnection 	_conn = null;
		private String 						_connString = "Server=localhost;User ID=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests";
		
		[SetUp]
		protected void SetUp()
		{
			NpgsqlEventLog.Level = LogLevel.Debug;	
			NpgsqlEventLog.LogName = "NpgsqlTests.LogFile";
			_conn = new NpgsqlConnection(_connString);
		}
		
		[TearDown]
		protected void TearDown()
		{
			if (_conn.State != ConnectionState.Closed)
				_conn.Close();
		}
		
		[Test]
		public void InsertWithDataSet()
		{
			
			_conn.Open();
			
			DataSet ds = new DataSet();

			NpgsqlDataAdapter da = new NpgsqlDataAdapter("select * from tableb", _conn);
	
			da.InsertCommand = new NpgsqlCommand("insert into tableb(field_int2, field_timestamp, field_numeric) values (:a, :b, :c)", _conn);
			
			da.InsertCommand.Parameters.Add(new NpgsqlParameter("a", DbType.Int16));
	
			da.InsertCommand.Parameters.Add(new NpgsqlParameter("b", DbType.DateTime));
			
			da.InsertCommand.Parameters.Add(new NpgsqlParameter("c", DbType.Decimal));
	
			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
			da.InsertCommand.Parameters[1].Direction = ParameterDirection.Input;
			da.InsertCommand.Parameters[2].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			da.InsertCommand.Parameters[1].SourceColumn = "field_timestamp";
			da.InsertCommand.Parameters[2].SourceColumn = "field_numeric";
	
	
			da.Fill(ds);
	
			
			DataTable dt = ds.Tables[0];
			
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			dr["field_timestamp"] = new DateTime(2003, 03, 03, 14, 0, 0);
			dr["field_numeric"] = 7.3M;
			
			dt.Rows.Add(dr);
	
			
			DataSet ds2 = ds.GetChanges();
			
			da.Update(ds2);
			
			ds.Merge(ds2);
			ds.AcceptChanges();
			
			
			NpgsqlDataReader dr2 = new NpgsqlCommand("select * from tableb where field_serial > 4", _conn).ExecuteReader();
			dr2.Read();
			
			Assertion.AssertEquals(4, dr2[1]);
			Assertion.AssertEquals(7.3M, dr2[3]);
			
			
			new NpgsqlCommand("delete from tableb where field_serial > 4", _conn).ExecuteNonQuery();
			
						
		}
	}
}
