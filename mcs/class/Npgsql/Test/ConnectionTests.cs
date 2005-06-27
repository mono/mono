// project created on 30/11/2002 at 22:00
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
using Npgsql;
using System.Data;

using NUnit.Framework;

namespace NpgsqlTests
{
	
	
		
	[TestFixture]
	public class ConnectionTests
	{
		NpgsqlConnection _conn;
		
		[SetUp]
		protected void SetUp()
		{
			//NpgsqlEventLog.Level = LogLevel.None;
			//NpgsqlEventLog.Level = LogLevel.Debug;
			//NpgsqlEventLog.LogName = "NpgsqlTests.LogFile";
			_conn = new NpgsqlConnection (TestConfiguration.NpgsqlConnectionString);
		}
		
		[TearDown]
		protected void TearDown()
		{
			if (_conn != null)
				_conn.Close();
		}
		
		
		[Test]
		public void Open()
		{
			try{
				_conn.Open();
				//Assert.AreEqual("ConnectionOpen", ConnectionState.Open, _conn.State);
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
			
			Assert.AreEqual("template1", result);
				
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void NestedTransaction()
		{
			_conn.Open();
			
            NpgsqlTransaction t = null;
            try
            {
			    t = _conn.BeginTransaction();
			
			    t = _conn.BeginTransaction();
            }
            catch(Exception e)
            {
                // Catch exception so we call rollback the transaction initiated.
                // This way, the connection pool doesn't get a connection with a transaction
                // started.
                t.Rollback();
                throw e;
            }            
			
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
