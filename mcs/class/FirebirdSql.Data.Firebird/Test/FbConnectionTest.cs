/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using NUnit.Framework;
using System;
using System.Configuration;
using System.Data;
using System.Reflection;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbConnectionTest : BaseTest 
	{
		public FbConnectionTest() : base(false)
		{
		}
				
		[Test]
		public void BeginTrasactionTest()
		{
			string connectionString = this.BuildConnectionString();

			FbConnection conn01 = new FbConnection(connectionString);
			conn01.Open();
			FbTransaction txn01 = conn01.BeginTransaction(IsolationLevel.Unspecified);
            txn01.Rollback();
			conn01.Close();
            
			FbConnection conn02 = new FbConnection(connectionString);
			conn02.Open();
			FbTransaction txn02 = conn02.BeginTransaction(IsolationLevel.ReadCommitted);
            txn02.Rollback();
			conn02.Close();
            
			FbConnection conn03 = new FbConnection(connectionString);
			conn03.Open();
			FbTransaction txn03 = conn03.BeginTransaction(IsolationLevel.ReadUncommitted);
            txn03.Rollback();
            conn03.Close();

			FbConnection conn04 = new FbConnection(connectionString);
			conn04.Open();
			FbTransaction txn04 = conn04.BeginTransaction(IsolationLevel.RepeatableRead);
            txn04.Rollback();
			conn04.Close();

			FbConnection conn05 = new FbConnection(connectionString);
			conn05.Open();
			FbTransaction txn05 = conn05.BeginTransaction(IsolationLevel.Serializable);
            txn05.Rollback();
			conn05.Close();			
		}
		
		[Test]
		public void CreateCommandTest()
		{
			FbCommand command = Connection.CreateCommand();

			Assert.AreEqual(command.Connection, Connection);
		}

		[Test]
		public void ConnectionPoolingTest()
		{
			string cs = this.BuildConnectionString(true);

			FbConnection myConnection1 = new FbConnection(cs);
			FbConnection myConnection2 = new FbConnection(cs);
			FbConnection myConnection3 = new FbConnection(cs);

			// Open two connections.
			Console.WriteLine ("Open two connections.");
			myConnection1.Open();
			myConnection2.Open();

			// Now there are two connections in the pool that matches the connection string.
			// Return the both connections to the pool. 
			Console.WriteLine ("Return both of the connections to the pool.");
			myConnection1.Close();
			myConnection2.Close();

			// Get a connection out of the pool.
			Console.WriteLine ("Open a connection from the pool.");
			myConnection1.Open();

			// Get a second connection out of the pool.
			Console.WriteLine ("Open a second connection from the pool.");
			myConnection2.Open();

			// Open a third connection.
			Console.WriteLine ("Open a third connection.");
			myConnection3.Open();

			// Return the all connections to the pool.  
			Console.WriteLine ("Return all three connections to the pool.");
			myConnection1.Close();
			myConnection2.Close();
			myConnection3.Close();

			// Clear pools
			FbConnection.ClearAllPools();
		}

        [Test]
        public void FbConnectionStringBuilderTest()
        {
            FbConnectionStringBuilder cs = new FbConnectionStringBuilder();

            cs.DataSource = ConfigurationSettings.AppSettings["DataSource"];
            cs.Database = ConfigurationSettings.AppSettings["Database"];
            cs.Port = Convert.ToInt32(ConfigurationSettings.AppSettings["Port"]);
            cs.UserID = ConfigurationSettings.AppSettings["User"];
            cs.Password = ConfigurationSettings.AppSettings["Password"];
            cs.ServerType = Convert.ToInt32(ConfigurationSettings.AppSettings["ServerType"]);
            cs.Charset = ConfigurationSettings.AppSettings["Charset"];
            cs.Pooling = Convert.ToBoolean(ConfigurationSettings.AppSettings["Pooling"]);

            using (FbConnection c = new FbConnection(cs.ToString()))
            {
                c.Open();
            }

        }

        public void OnStateChange(object sender, StateChangeEventArgs e)
		{		
			Console.WriteLine("OnStateChange");
			Console.WriteLine("  event args: ("+
				   "originalState=" + e.OriginalState +
				   " currentState=" + e.CurrentState +")");
		}
						
		public FbTransaction BeginTransaction(IsolationLevel level)
		{	
			switch(level)
			{
				case IsolationLevel.Unspecified:
					return Connection.BeginTransaction();
				
				default:
					return Connection.BeginTransaction(level);
			}
		}		
	}
}
