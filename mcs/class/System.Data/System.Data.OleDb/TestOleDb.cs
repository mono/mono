using System;
using System.Data.OleDb;

namespace System.Data.OleDb.Test
{
	public class TestOleDb
	{
		private OleDbConnection m_cnc;

		private TestOleDb ()
		{
			OleDbCommand cmd;
			
			m_cnc = new OleDbConnection ("Provider=PostgreSQL;Addr=127.0.0.1;Database=rodrigo");
			m_cnc.Open ();

			Console.WriteLine ("Connected to:");
			Console.WriteLine (" Data Source: " + m_cnc.DataSource);
			Console.WriteLine (" Database: " + m_cnc.Database);
			Console.WriteLine (" Connection string: " + m_cnc.ConnectionString);
			Console.WriteLine (" Provider: " + m_cnc.Provider);
			Console.WriteLine (" Server version:" + m_cnc.ServerVersion);

			/* create temporary table */
			Console.WriteLine ("Creating temporary table...");
			cmd = new OleDbCommand ("CREATE TABLE mono_test_table ( " +
						" name varchar(25), email varchar(50), date_entered timestamp)",
						m_cnc);
			cmd.ExecuteNonQuery ();
			InsertRow ("Mike Smith", "mike@smiths.com");
			InsertRow ("Julie Andrews", "julie@hollywood.com");
			InsertRow ("Michael Jordan", "michael@bulls.com");
		}

		void InsertRow (string name, string email)
		{
			OleDbCommand cmd;

			cmd = new OleDbCommand ("INSERT INTO mono_test_table (name, email, date_entered) VALUES ('" +
						name + "', '" + email +"', date 'now')", m_cnc);
			Console.WriteLine ("Executing command '" + cmd.CommandText + "'");
			cmd.ExecuteNonQuery ();

		}
		
		void DisplayRow (OleDbDataReader reader)
		{
			for (int i = 0; i < reader.FieldCount; i++) {
				Console.WriteLine (" " + reader.GetDataTypeName (i) + ": " +
						   reader.GetValue (i).ToString ());
			}
		}
		
		void TestDataReader ()
		{
			int i = 0;
			string sql = "SELECT * FROM mono_test_table";
			
			Console.WriteLine ("Executing SELECT command...");
			OleDbCommand cmd = new OleDbCommand (sql, m_cnc);
			OleDbDataReader reader = cmd.ExecuteReader ();

			Console.WriteLine (" Recordset description:");
			for (i = 0; i < reader.FieldCount; i++) {
				Console.WriteLine ("  Field " + i + ": " +
						   reader.GetName (i) + " (" +
						   reader.GetDataTypeName (i) + ")");
			}

			Console.WriteLine ("Reading data...");
			i = 0;
			while (reader.Read ()) {
				Console.WriteLine ("Row " + i + ":");
				DisplayRow (reader);
				i++;
			}

			reader.Close ();
		}

		void TestTransaction ()
		{
			Console.WriteLine ("Starting transaction...");
			OleDbTransaction xaction = m_cnc.BeginTransaction ();

			Console.WriteLine ("Aborting transaction...");
			xaction.Rollback ();
		}
		
		void Close ()
		{
			OleDbCommand cmd = new OleDbCommand ("DROP TABLE mono_test_table", m_cnc);
			cmd.ExecuteNonQuery ();
			m_cnc.Close ();
		}

		static void Main (string[] args)
		{
			try {
				TestOleDb test = new TestOleDb ();
				test.TestDataReader ();
				test.TestTransaction ();
				test.Close ();
			} catch (Exception e) {
				Console.WriteLine ("An error has occured: {0}", e.ToString ());
			}
		}
	}
}
