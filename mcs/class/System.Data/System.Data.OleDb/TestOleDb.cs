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
		}

		void TestDataReader ()
		{
		}

		static void Main (string[] args)
		{
			TestOleDb test = new TestOleDb ();
			test.TestDataReader ();
		}
	}
}
