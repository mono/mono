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

		static void Wait(string msg) 
		{
			Console.WriteLine(msg);
			Console.WriteLine("Waiting...  Presee Enter to continue...");
			string nothing = Console.ReadLine();
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
					
			DoTest1(con1, 1);
			DoTest1(con2, 2);
			DoTest1(con3, 3);
			
			DoTest9(con1);

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
