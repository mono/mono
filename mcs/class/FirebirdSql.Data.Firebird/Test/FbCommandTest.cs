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
using System.Collections;
using System.Data;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbCommandTest : BaseTest 
	{	
		public FbCommandTest() : base(false)
		{		
		}

		[Test]
		public void ExecuteNonQueryTest()
		{							
			Transaction = Connection.BeginTransaction();

			FbCommand command = Connection.CreateCommand();
			
			command.Transaction = Transaction;
			command.CommandText = "insert into TEST (INT_FIELD) values (?) ";
									
			command.Parameters.Add("@INT_FIELD", 100);
									
			int affectedRows = command.ExecuteNonQuery();
									
			Assert.AreEqual(affectedRows, 1);
								
			Transaction.Rollback();

			command.Dispose();
		}
		
		[Test]
		public void ExecuteReaderTest()
		{							
			FbCommand command = Connection.CreateCommand();
			
			command.CommandText = "select * from TEST";
			
			FbDataReader reader = command.ExecuteReader();
			reader.Close();

			command.Dispose();
		}

        [Test]
        public void ExecuteMultipleReaderTest()
        {
            FbCommand command1 = Connection.CreateCommand();
            FbCommand command2 = Connection.CreateCommand();

            command1.CommandText = "select * from test where int_field = 1";
            command2.CommandText = "select * from test where int_field = 2";

            FbDataReader r1 = command1.ExecuteReader();
            FbDataReader r2 = command2.ExecuteReader();

            r2.Close();

            try
            {
                // Try to call ExecuteReader in command1
                // it should throw an exception
                r2 = command1.ExecuteReader();

                throw new InvalidProgramException();
            }
            catch
            {
                r1.Close();
            }
        }

        [Test]
		public void ExecuteReaderWithBehaviorTest()
		{							
			FbCommand command = new FbCommand("select * from TEST", Connection);
			
			FbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);								
			reader.Close();

			command.Dispose();
		}
		
		[Test]
		public void ExecuteScalarTest()
		{							
			FbCommand command = Connection.CreateCommand();
			
			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = ?";									
			command.Parameters.Add("@INT_FIELD", 2);
						
			string charFieldValue = command.ExecuteScalar().ToString();
			
			Console.WriteLine("Scalar value: {0}", charFieldValue);

			command.Dispose();
		}
		
		[Test]
		public void PrepareTest()
		{					
			// Create a new test table
			FbCommand create = new FbCommand("create table PrepareTest(test_field varchar(20));", Connection);
			create.ExecuteNonQuery();
			create.Dispose();
		
			// Insert data using a prepared statement
			FbCommand command = new FbCommand(
				"insert into PrepareTest(test_field) values(@test_field);",
				Connection);
			
			command.Parameters.Add("@test_field", FbDbType.VarChar).Value = DBNull.Value;
			command.Prepare();

			for (int i = 0; i < 5; i++) 
			{
				if (i < 1)
				{
					command.Parameters[0].Value = DBNull.Value;
				}
				else
				{
					command.Parameters[0].Value = i.ToString();
				}
				command.ExecuteNonQuery();
			}

			command.Dispose();

			try
			{
				// Check that data is correct
				FbCommand select = new FbCommand("select * from PrepareTest", Connection);
				FbDataReader reader = select.ExecuteReader();
				int count = 0;
				while (reader.Read())
				{
					if (count == 0)
					{
						Assert.AreEqual(DBNull.Value, reader[0], "Invalid value.");
					}
					else
					{
						Assert.AreEqual(count, reader.GetInt32(0), "Invalid value.");
					}

					count++;
				}
				reader.Close();
				select.Dispose();
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				// Drop table
				FbCommand drop = new FbCommand("drop table PrepareTest", Connection);
				drop.ExecuteNonQuery();
				drop.Dispose();
			}			
		}

		[Test]
		public void NamedParametersTest()
		{
			FbCommand command = Connection.CreateCommand();
			
			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = @int_field or CHAR_FIELD = @char_field";
									
			command.Parameters.Add("@int_field", 2);
			command.Parameters.Add("@char_field", "TWO");
						
			FbDataReader reader = command.ExecuteReader();
			
			int count = 0;

			while (reader.Read())
			{
				count++;
			}

			Assert.AreEqual(1, count, "Invalid number of records fetched.");

			reader.Close();
			command.Dispose();
		}

		[Test]
		public void NamedParametersAndLiterals()
		{
			string sql = "update test set char_field = 'carlos@firebird.org', bigint_field = @bigint, varchar_field = 'carlos@ado.net' where int_field = @integer";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@bigint", FbDbType.BigInt).Value = 200;
			command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;

			int recordsAffected = command.ExecuteNonQuery();

			command.Dispose();

			Assert.AreEqual(recordsAffected, 1, "Invalid number of records affected.");
		}

		[Test]
		public void NamedParametersReuseTest()
		{
			string sql = "select * from test where int_field >= @lang and int_field <= @lang";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@lang", FbDbType.Integer).Value = 10;
						
			FbDataReader reader = command.ExecuteReader();
			
			int count		= 0;
			int intValue	= 0;

			while (reader.Read())
			{
				if (count == 0)
				{
					intValue = reader.GetInt32(0);
				}
				count++;
			}

			Assert.AreEqual(1, count, "Invalid number of records fetched.");
			Assert.AreEqual(10, intValue, "Invalid record fetched.");

			reader.Close();
			command.Dispose();
		}

		[Test]
		public void ExecuteStoredProcTest()
		{			
			FbCommand command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", Connection);
				
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This will fill output parameters values
			command.ExecuteNonQuery();

			Console.WriteLine("Output Parameters");
			Console.WriteLine(command.Parameters[1].Value);
		}

		[Test]
		public void RecordsAffectedTest()
		{
			FbCommand selectCommand = new FbCommand("SELECT * FROM TEST WHERE INT_FIELD = -1", Connection);
			int recordsAffected = selectCommand.ExecuteNonQuery();
			Console.WriteLine("\r\nRecords Affected: {0}", recordsAffected);
			Assert.IsTrue(recordsAffected == -1);
			selectCommand.Dispose();

			FbCommand deleteCommand = new FbCommand("DELETE FROM TEST WHERE INT_FIELD = -1", Connection);	
			recordsAffected = deleteCommand.ExecuteNonQuery();
			Console.WriteLine("\r\nRecords Affected: {0}", recordsAffected);
			Assert.IsTrue(recordsAffected == 0);
			deleteCommand.Dispose();
		}

		[Test]
		public void ExecuteNonQueryWithOutputParameters()
		{
			FbCommand command = new FbCommand("EXECUTE PROCEDURE GETASCIIBLOB(?)", Connection);
				
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@CLOB_FIELD", FbDbType.Text).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This will fill output parameters values
			command.ExecuteNonQuery();

			// Check that the output parameter has a correct value
			Assert.AreEqual("IRow Number 1", command.Parameters[1].Value, "Output parameter value is not valid");

			// Dispose command - this will do a transaction commit
			command.Dispose();
		}

		[Test]
		public void InvalidParameterFormat()
		{
			string sql = "update test set timestamp_field = @timestamp where int_field = @integer";

			FbTransaction transaction = this.Connection.BeginTransaction();
			try
			{
				FbCommand command = new FbCommand(sql, this.Connection, transaction);
				command.Parameters.Add("@timestamp", FbDbType.TimeStamp).Value = 1;
				command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;

				command.ExecuteNonQuery();

				command.Dispose();

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
			}
		}

        [Test]
        public void UnicodeTest()
        {
            string createTable = "CREATE TABLE VARCHARTEST (VARCHAR_FIELD  VARCHAR(10));";

            FbCommand ct = new FbCommand(createTable, this.Connection);
            ct.ExecuteNonQuery();
            ct.Dispose();

            ArrayList l = new ArrayList();

            l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('1');");
            l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('11');");
            l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('111');");
            l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('1111');");

            foreach (string statement in l)
            {
                FbCommand insert = new FbCommand(statement, this.Connection);
                insert.ExecuteNonQuery();
                insert.Dispose();
            }

            string sql = "select * from varchartest";

            FbCommand cmd = new FbCommand(sql, this.Connection);
            FbDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                Console.WriteLine("{0} :: {1}", r[0], r[0].ToString().Length);
            }

            r.Close();
        }

        [Test]
        public void SimplifiedChineseTest()
        {
            string createTable = "CREATE TABLE TABLE1 (FIELD1 varchar(20))";
            FbCommand create = new FbCommand(createTable, this.Connection);
            create.ExecuteNonQuery();
            create.Dispose();

            // insert using parametrized SQL
            string sql = "INSERT INTO Table1 VALUES (@value)";
            FbCommand command = new FbCommand(sql, this.Connection);
            command.Parameters.Add("@value", FbDbType.VarChar).Value = "中文";
            command.ExecuteNonQuery();
            command.Dispose();

            sql = "SELECT * FROM TABLE1";
            FbCommand select = new FbCommand(sql, this.Connection);
            string result = select.ExecuteScalar().ToString();
            select.Dispose();

            Assert.AreEqual("中文", result, "Incorrect results in parametrized insert");

            sql = "DELETE FROM TABLE1";
            FbCommand delete = new FbCommand(sql, this.Connection);
            delete.ExecuteNonQuery();
            delete.Dispose();

            // insert using plain SQL
            sql = "INSERT INTO Table1 VALUES ('中文')";
            FbCommand plainCommand = new FbCommand(sql, this.Connection);
            plainCommand.ExecuteNonQuery();
            plainCommand.Dispose();

            sql = "SELECT * FROM TABLE1";
            select = new FbCommand(sql, this.Connection);
            result = select.ExecuteScalar().ToString();
            select.Dispose();

            Assert.AreEqual("中文", result, "Incorrect results in plain insert");
        }

		[Test]
		public void InsertDateTest()
		{
			string sql = "insert into TEST (int_field, date_field) values (1002, @date)";

			FbCommand command = new FbCommand(sql, this.Connection);

			command.Parameters.Add("@date", FbDbType.Date).Value = DateTime.Now.ToString();

			int ra = command.ExecuteNonQuery();

			Assert.AreEqual(ra, 1);
		}

		[Test]
		public void InsertNullTest()
		{
			string sql = "insert into TEST (int_field) values (@value)";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@value", FbDbType.Integer).Value = null;

			try
			{
				command.ExecuteNonQuery();

				throw new Exception("The command was executed without throw an exception");
			}
			catch
			{
			}
		}

		[Test]
		public void ParameterDescribeTest()
		{
			string sql = "insert into TEST (int_field) values (@value)";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Prepare();
			command.Parameters.Add("@value", FbDbType.Integer).Value = 100000;

			command.ExecuteNonQuery();

			command.Dispose();
		}

		[Test]
		public void ReadOnlyTransactionTest()
		{
			using (IDbCommand command = this.Connection.CreateCommand())
			{
				using (IDbTransaction transaction = this.Connection.BeginTransaction(FbTransactionOptions.Read))
				{
					try
					{
						command.Transaction = transaction;
						command.CommandType = System.Data.CommandType.Text;
						command.CommandText = "CREATE TABLE X_TABLE_1(FIELD VARCHAR(50));";
						command.ExecuteNonQuery();
						transaction.Commit();
					}
					catch (FbException)
					{
					}
				}
			}
		}
	}
}
