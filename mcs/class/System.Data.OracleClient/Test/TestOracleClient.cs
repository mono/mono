// 
// TestOracleClient.cs - Tests Sytem.Data.OracleClient
//                       data provider in Mono.
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.OCI
//
// Tests:
//     Assembly: System.Data.OracleClient.dll
//     Namespace: System.Data.OracleClient
// 
// Author: 
//     Daniel Morgan <danmorg@sc.rr.com>
//         
// Copyright (C) Daniel Morgan, 2002
// 

// Expected Results:
//    3 new rows where ENAME being: 'conn3', 'conn9', and 'conn1'

using System;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.OracleClient;

namespace Test.OracleClient
{
	public class OracleTest
	{
		public OracleTest() 
		{

		}

		static void DoTest1(OracleConnection con, int conn) 
		{
			string inst = conn.ToString();

			string insertSql =
				"insert into scott.emp " +
				"(empno, ename, job, sal, deptno) " +
				"values(123" + inst + "," +
				"'conn" + inst + "'," +
				"'homy" + inst + "'," +
				"321" + inst + ",20)";
			
			Console.WriteLine("insertSql: " + insertSql);
			OracleCommand cmd = new OracleCommand();
			cmd.Connection = con;

			cmd.CommandText = insertSql;
			cmd.ExecuteNonQuery();

			if(conn == 2)
				cmd.CommandText = "rollback";
			else
				cmd.CommandText = "commit";
			cmd.ExecuteNonQuery();
		}

		static void DoTest9(OracleConnection con) {
			string inst = "9";

			string insertSql =
				"insert into scott.emp " +
				"(empno, ename, job, sal, deptno) " +
				"values(123" + inst + "," +
				"'conn" + inst + "'," +
				"'homy" + inst + "'," +
				"321" + inst + ",20)";
			
			Console.WriteLine("insertSql: " + insertSql);
			OracleCommand cmd = new OracleCommand();
			cmd.Connection = con;

			cmd.CommandText = insertSql;
			cmd.ExecuteNonQuery();

			cmd.CommandText = "commit";
			cmd.ExecuteNonQuery();
		}

		static void ReadSimpleTest(OracleConnection con) 
		{
			string selectSql = 
				"SELECT ename, job FROM scott.emp";
			OracleCommand cmd = new OracleCommand();
			cmd.Connection = con;
			cmd.CommandText = selectSql;
			OracleDataReader reader = cmd.ExecuteReader();
			Console.WriteLine("Results...");
			Console.WriteLine("Schema");
			DataTable table;
			table = reader.GetSchemaTable();
			for(int c = 0; c < reader.FieldCount; c++) {
				Console.WriteLine("  Column " + c.ToString());
				DataRow row = table.Rows[c];
			
				string ColumnName = (string) row["ColumnName"];
				string BaseColumnName = (string) row["BaseColumnName"];
				int ColumnSize = (int) row["ColumnSize"];
				int NumericScale = Convert.ToInt32( row["NumericScale"]);
				int NumericPrecision = Convert.ToInt32(row["NumericPrecision"]);
				Type DataType = (Type) row["DataType"];

				Console.WriteLine("    ColumnName: " + ColumnName);
				Console.WriteLine("    BaseColumnName: " + BaseColumnName);
				Console.WriteLine("    ColumnSize: " + ColumnSize.ToString());
				Console.WriteLine("    NumericScale: " + NumericScale.ToString());
				Console.WriteLine("    NumericPrecision: " + NumericPrecision.ToString());
				Console.WriteLine("    DataType: " + DataType.ToString());
			}

			int row = 0;
			Console.WriteLine("Data");
			while(reader.Read()) {
				row++;
				Console.WriteLine("  Row: " + row.ToString());
				for(int f = 0; f < reader.FieldCount; f++) {
					object ovalue;
					string svalue;
					ovalue = reader.GetValue(0);
					svalue = ovalue.ToString();
					Console.WriteLine("     Field: " + f.ToString());
					Console.WriteLine("         Value: " + svalue);
				}
			}
			if(row == 0)
				Console.WriteLine("No data returned.");
		}
		
		static void DataAdapterTest (OracleConnection connection)
		{
			OracleCommand command = connection.CreateCommand ();
			command.CommandText = "SELECT * FROM EMP";
			OracleDataAdapter adapter = new OracleDataAdapter (command);

			DataSet dataSet = new DataSet ("EMP");

			adapter.Fill (dataSet);

			DataTable table = dataSet.Tables [0];
			int rowCount = 0;
			foreach (DataRow row in table.Rows) {
				Console.WriteLine ("row {0}", rowCount + 1);
				for (int i = 0; i < table.Columns.Count; i += 1) {
					Console.WriteLine ("{0}:{1}", table.Columns [i].ColumnName, row [i]);
				}
				Console.WriteLine ();
				rowCount += 1;
			}
		}

		static void RollbackTest (OracleConnection connection)
		{
			OracleTransaction transaction = connection.BeginTransaction ();

			OracleCommand insert = connection.CreateCommand ();
			insert.Transaction = transaction;
			insert.CommandText = "INSERT INTO EMP (EMPNO, ENAME, JOB) VALUES (8787, 'T Coleman', 'Monoist')";

			Console.WriteLine ("Inserting record ...");

			insert.ExecuteNonQuery ();

			OracleCommand select = connection.CreateCommand ();
			select.CommandText = "SELECT COUNT(*) FROM EMP WHERE EMPNO = 8787";
			select.Transaction = transaction;
			OracleDataReader reader = select.ExecuteReader ();
			reader.Read ();

			Console.WriteLine ("Row count SHOULD BE 1, VALUE IS {0}", reader.GetValue (0));
			reader.Close ();

			Console.WriteLine ("Rolling back transaction ...");

			transaction.Rollback ();

			select = connection.CreateCommand ();
			select.CommandText = "SELECT COUNT(*) FROM EMP WHERE EMPNO = 8787";

			reader = select.ExecuteReader ();
			reader.Read ();
			Console.WriteLine ("Row count SHOULD BE 0, VALUE IS {0}", reader.GetValue (0));
			reader.Close ();
		}
		
		static void CommitTest (OracleConnection connection)
		{
			OracleTransaction transaction = connection.BeginTransaction ();

			OracleCommand insert = connection.CreateCommand ();
			insert.Transaction = transaction;
			insert.CommandText = "INSERT INTO EMP (EMPNO, ENAME, JOB) VALUES (8787, 'T Coleman', 'Monoist')";

			Console.WriteLine ("Inserting record ...");

			insert.ExecuteNonQuery ();

			OracleCommand select = connection.CreateCommand ();
			select.CommandText = "SELECT COUNT(*) FROM EMP WHERE EMPNO = 8787";
			select.Transaction = transaction;

			Console.WriteLine ("Row count SHOULD BE 1, VALUE IS {0}", select.ExecuteScalar ());

			Console.WriteLine ("Committing transaction ...");

			transaction.Commit ();

			select = connection.CreateCommand ();
			select.CommandText = "SELECT COUNT(*) FROM EMP WHERE EMPNO = 8787";

			Console.WriteLine ("Row count SHOULD BE 1, VALUE IS {0}", select.ExecuteScalar ());
			transaction = connection.BeginTransaction ();
			OracleCommand delete = connection.CreateCommand ();
			delete.Transaction = transaction;
			delete.CommandText = "DELETE FROM EMP WHERE EMPNO = 8787";
			delete.ExecuteNonQuery ();
			transaction.Commit ();
		}

		public static void ParameterTest (OracleConnection connection)
		{
			OracleTransaction transaction = connection.BeginTransaction ();
			OracleCommand insert = connection.CreateCommand ();
			insert.Transaction = transaction;

			insert.CommandText = "INSERT INTO EMP (EMPNO, ENAME, JOB) VALUES (:P1, :P2, :P3)";
			insert.Parameters.Add (":P1", 8888);
			insert.Parameters.Add (":P2", "danmorg");
			insert.Parameters.Add (":P3", "Monoist");

			Console.WriteLine ("INSERTING DATA WITH PARAMETERS...");
			Console.WriteLine (insert.CommandText);
			insert.Prepare ();
			insert.ExecuteNonQuery ();

			OracleCommand select = connection.CreateCommand ();
			select.Transaction = transaction;

			select.CommandText = "SELECT ENAME, JOB FROM EMP WHERE EMPNO=:P1";
			select.Parameters.Add (":P1", 8888);

			Console.WriteLine ("VERIFYING RESULTS ...");

			OracleDataReader reader = select.ExecuteReader ();
			if (!reader.Read ())
				Console.WriteLine ("ERROR: RECORD NOT FOUND");

			Console.WriteLine ("ENAME - SHOULD BE danmorg, is {0}", reader.GetValue (0));
			Console.WriteLine ("JOB - SHOULD BE Monoist, is {0}", reader.GetValue (1));

			reader.Close ();

			Console.WriteLine ("ROLLBACK TRANSACTION...");

			transaction.Rollback ();
		}


		static void Wait(string msg) 
		{
			//Console.WriteLine(msg);
			//Console.WriteLine("Waiting...  Presee Enter to continue...");
			//string nothing = Console.ReadLine();
		}

		[STAThread]
		static void Main(string[] args)
		{	
			if(args.Length != 3) {
				Console.WriteLine("Usage: mono TestOracleClient database userid password");
				return;
			}

			string connectionString = String.Format(
				"Data Source={0};" +
				"User ID={1};" +
				"Password={2}",
				args[0], args[1], args[2]);

			Wait("Verify database.");

			OracleConnection con1 = new OracleConnection();
			con1.ConnectionString = connectionString;
			con1.Open();

			Wait("Verify 1 connection.");
			
			OracleConnection con2 = new OracleConnection();
			con2.ConnectionString = connectionString;
			con2.Open();
			
			Wait("Verify 2 connections.");

			OracleConnection con3 = new OracleConnection();
			con3.ConnectionString = connectionString;
			con3.Open();
			
			Wait("Verify 3 connections.");
					
			//DoTest1(con1, 1);
			//DoTest1(con2, 2);
			//DoTest1(con3, 3);
			
			//DoTest9(con1);
			
			Console.WriteLine ("Read Simple Test BEGIN...");
                        ReadSimpleTest(con1);
			Console.WriteLine ("Read Simple Test END.");

			Wait ("Press enter to continue ...");

			Console.WriteLine ("DataAdapter Test BEGIN...");
                        DataAdapterTest(con1);
			Console.WriteLine ("DataAdapter Test END.");

			Console.WriteLine ("Rollback Test BEGIN...");
                        RollbackTest(con1);
			Console.WriteLine ("Rollback Test END.");

			Console.WriteLine ("Commit Test BEGIN...");
                        CommitTest(con1);
			Console.WriteLine ("Commit Test END.");

			Console.WriteLine ("Parameter Test BEGIN...");
                        ParameterTest(con1);
			Console.WriteLine ("Parameter Test END.");

			Wait("Verify Proper Results.");
						
			con1.Close();		

			Wait("Verify 2 connections left.");

			con2.Close();

			Wait("Verify 1 connection left.");

			con3.Close();
			
			Wait("Verify all disconnected.");
		}
	}
}

