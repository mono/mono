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

		static void ConnectionCount() {
			uint count = OracleConnection.ConnectionCount;
			string msg = "Connection Count: " + count.ToString();
			Console.WriteLine(msg);
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
			string connectionString;
			connectionString = 
				"Data Source=dansdb;" +
				"User ID=scott;" +
				"Password=tiger";

			Wait("Verify database.");

			ConnectionCount(); // should be 0

			OracleConnection con1 = new OracleConnection();
			con1.ConnectionString = connectionString;
			con1.Open();
			ConnectionCount(); // should be 1

			OracleConnection con2 = new OracleConnection();
			con2.ConnectionString = connectionString;
			con2.Open();
			ConnectionCount(); // should be 2

			OracleConnection con3 = new OracleConnection();
			con3.ConnectionString = connectionString;
			con3.Open();
			ConnectionCount(); // should be 3

			Wait("Verify Connected.");
					
			DoTest1(con1, 1);
			DoTest1(con2, 2);
			DoTest1(con3, 3);
			
			DoTest9(con1);

			Wait("Verify Proper Results.");
			
			ConnectionCount(); // should be 3

			con1.Close();
			ConnectionCount(); // should be 2

			con2.Close();
			ConnectionCount(); // should be 1

			con3.Close();
			ConnectionCount(); // should be 0

			Wait("Verify Disconnected");
		}
	}
}
