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

		void DisplayRow (IDataReader reader)
		{
			for (int i = 0; i < reader.FieldCount; i++) {
				Console.WriteLine (" " + reader.GetDataTypeName (i) + ": " +
						   reader.GetValue (i).ToString ());
			}
		}
		
		void TestDataReader ()
		{
			int i = 0;
			string sql = "SELECT * FROM pg_tables";
			
			Console.WriteLine ("Executing command...");
			OleDbCommand cmd = new OleDbCommand (sql, m_cnc);
			IDataReader reader = cmd.ExecuteReader ();

			Console.WriteLine (" Recordset description:");
			for (i = 0; i < reader.FieldCount; i++) {
				Console.WriteLine ("  Field " + i + ": " + reader.GetDataTypeName (i));
			}

			Console.WriteLine ("Reading data...");
			i = 0;
			while (reader.Read ()) {
				Console.WriteLine ("Row " + i + ":");
				DisplayRow (reader);
				i++;
			}
		}

		void Close ()
		{
			m_cnc.Close ();
		}

		static void Main (string[] args)
		{
			try {
				TestOleDb test = new TestOleDb ();
				test.TestDataReader ();
				test.Close ();
			} catch (Exception e) {
				Console.WriteLine ("An error has occured: {0}", e.ToString ());
			}
		}
	}
}
