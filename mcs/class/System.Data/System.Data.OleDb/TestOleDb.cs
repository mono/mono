using System;
using System.Data.OleDb;

namespace System.Data.OleDb.Test
{
	public class TestOleDb
	{
		private OleDbConnection m_cnc;
		
		private TestOleDb ()
		{
			m_cnc = new OleDbConnection ("PostgreSQL");
			m_cnc.Open ();
		}

		void TestDataReader ()
		{
			string sql = "SELECT * FROM pg_tables";
			OleDbCommand cmd = new OleDbCommand (sql, m_cnc);
		}

		static void Main (string[] args)
		{
			try {
				TestOleDb test = new TestOleDb ();
				test.TestDataReader ();
			} catch (Exception e) {
				Console.Write ("An error has occured");
			}
		}
	}
}
