// created on 3/5/2003 at 14:29
// 
// Author:
// 	Francisco Figueiredo Jr. <fxjrlists@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


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
		private String 						_connString = "Server=localhost;User ID=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests;maxpoolsize=2;";
		
		[SetUp]
		protected void SetUp()
		{
			//NpgsqlEventLog.Level = LogLevel.None;
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
			
			
			Assert.AreEqual(4, dr2[1]);
			Assert.AreEqual(7.3000000M, dr2[3]);
			
			new NpgsqlCommand("delete from tableb where field_serial > 4", _conn).ExecuteNonQuery();
			
			
						
		}
		
		[Test]
		public void FillWithEmptyResultset()
		{
		  
		  _conn.Open();
			
			DataSet ds = new DataSet();

			NpgsqlDataAdapter da = new NpgsqlDataAdapter("select * from tableb where field_serial = -1", _conn);
		  
		  
		  da.Fill(ds);
		  
		  Assert.AreEqual(1, ds.Tables.Count);
		  Assert.AreEqual(4, ds.Tables[0].Columns.Count);
		  Assert.AreEqual("field_serial", ds.Tables[0].Columns[0].ColumnName);
		  Assert.AreEqual("field_int2", ds.Tables[0].Columns[1].ColumnName);
		  Assert.AreEqual("field_timestamp", ds.Tables[0].Columns[2].ColumnName);
		  Assert.AreEqual("field_numeric", ds.Tables[0].Columns[3].ColumnName);
		  
		}
        
        [Test]
        public void FillWithDuplicateColumnName()
        {
            _conn.Open();
            DataSet ds = new DataSet();

			NpgsqlDataAdapter da = new NpgsqlDataAdapter("select field_serial, field_serial from tableb", _conn);
		  
		    da.Fill(ds);
            
        }
	}
}
