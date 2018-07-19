// SqliteDataAdapterUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteDataAdapter
//
// Author(s):	Thomas Zoechling <thomas.zoechling@gmx.at>


using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite
{
	
	[TestFixture]
	public class SqliteCommandUnitTests
	{
		string _uri;
		string _connectionString;
		SqliteConnection _conn;
		readonly static string stringvalue = "my keyboard is better than yours : äöüß";

		public SqliteCommandUnitTests()
		{
		}

		[SetUp]
		public void Create()
		{
			_uri = Path.GetTempFileName ();
		  	_connectionString = "URI=file://" + _uri + ", version=3";
			_conn = new SqliteConnection (_connectionString);

			try
			{
				if(File.Exists(_uri))
				{
					_conn.Dispose();
					// We want to start with a fresh db for each full run
					// The database is created on the first open()
					File.Delete(_uri);

				}
			}
			catch(Exception e)
			{
				throw e;
			}

			try
			{
				using (SqliteCommand createCommand = new SqliteCommand("CREATE TABLE t1(t  TEXT,  f FLOAT, i INTEGER, b TEXT);", _conn))
				using (SqliteCommand insertCommand = new SqliteCommand("INSERT INTO t1  (t, f, i, b ) VALUES('" + stringvalue + "',123,123,'123')", _conn))
				{
					_conn.Open();
					createCommand.ExecuteNonQuery();
					insertCommand.ExecuteNonQuery();
				}
			}
			catch(Exception e)
			{
				Console.WriteLine (e);
				throw new AssertionException("Create table failed",e);
			}
			finally
			{
				_conn.Close();  
			}
		}
		
		[TearDown]
 		public void TearDown ()
 		{
 			if (File.Exists (_uri))
 				File.Delete (_uri);
 		}

		[Test]	
		public void Select()
		{
			using (_conn)
			using (SqliteCommand simpleSelect = new SqliteCommand("SELECT * FROM t1;  ", _conn)) // check trailing spaces
			{
				_conn.Open();
				using (SqliteDataReader dr = simpleSelect.ExecuteReader())
				{
					while (dr.Read())
					{
						string test = dr[0].ToString();
						Assert.AreEqual(dr["T"], stringvalue); // also checks case-insensitive column
						Assert.AreEqual(dr["F"], 123);
						Assert.AreEqual(dr["I"], 123);
						Assert.AreEqual(dr["B"], "123");
					}
					Assert.IsTrue(dr.FieldCount>0);
				}
			}
		}

		[Test]
		public void Delete()
		{
			using (_conn)
			using (SqliteCommand insCmd = new SqliteCommand("INSERT INTO t1 VALUES ('todelete',0.1,0,'')", _conn))
			using (SqliteCommand delCmd = new SqliteCommand("DELETE FROM t1 WHERE t = 'todelete'", _conn))
			{
				_conn.Open();
				int insReturn = insCmd.ExecuteNonQuery();
				int delReturn = delCmd.ExecuteNonQuery();
			
				Assert.IsTrue(insReturn == delReturn);
			}
		}
		
		[Test]
		public void Insert()
		{
			using (_conn)
			using (SqliteCommand insCmd = new SqliteCommand("INSERT INTO t1 VALUES ('inserted',0.1,0,'')", _conn))
			{
				_conn.Open();
				int insReturn = insCmd.ExecuteNonQuery();
				Assert.IsTrue(insReturn == 1);
			}
		}
		
		[Test]
		public void Update()
		{
			using (_conn)
			using (SqliteCommand insCmd = new SqliteCommand("INSERT INTO t1 VALUES ('toupdate',0.1,0,'')", _conn))
			using (SqliteCommand updCmd = new SqliteCommand("UPDATE t1 SET t = 'updated' ,f = 2.0, i = 2, b = '' WHERE t = 'toupdate'", _conn))
			{
				_conn.Open();
				insCmd.ExecuteNonQuery();
				Assert.IsTrue(updCmd.ExecuteNonQuery() == 1);
			}
		}

		
		[Test]
		public void ScalarReturn()
		{
			// This should return the 1 line that got inserted in CreateTable() Test
			using (_conn)
			using (SqliteCommand cmd = new SqliteCommand("SELECT COUNT(*) FROM t1 WHERE  t LIKE '%äöüß'", _conn))
			{
				_conn.Open();
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar()));
			}
		}
		
		[Test]
		public void InsertWithTransaction()
		{
			_conn.Open();
			using (_conn)
			using (SqliteTransaction t = _conn.BeginTransaction() as SqliteTransaction)
			using (SqliteCommand c1 = new SqliteCommand("INSERT INTO t1 VALUES ('a',0.1,0,'0')", _conn, t))
			using (SqliteCommand c2 = new SqliteCommand("INSERT INTO t1 VALUES ('b',1.2,0,'0')", _conn, t))
			using (SqliteCommand c3 = new SqliteCommand("INSERT INTO t1 VALUES ('c',0.3,1,'0')", _conn, t))
			using (SqliteCommand c4 = new SqliteCommand("INSERT INTO t1 VALUES ('d',0.4,0,'1')", _conn, t))
			{
				try
				{
					c1.ExecuteNonQuery();
					c2.ExecuteNonQuery();
					c3.ExecuteNonQuery();
					c4.ExecuteNonQuery();
					t.Commit();
				}
				catch(Exception e)
				{
					t.Rollback();
					throw new AssertionException("Sqlite Commands failed", e);
				}
			}
		}
		
		[Test]
		[ExpectedException(typeof(SqliteException))]
		public void InsertWithFailingTransaction()
		{
			_conn.Open();
			using (_conn)
			using (SqliteTransaction t = _conn.BeginTransaction() as SqliteTransaction)
			using (SqliteCommand c1 = new SqliteCommand("INSERT INTO t1 VALUES ('1','0','0','0')", _conn, t))
			using (SqliteCommand c2 = new SqliteCommand("INSERT INTO t1 VALUES ('0','1','0','0')", _conn, t))
			using (SqliteCommand c3 = new SqliteCommand("INSERT INTO t1 VALUES ('x',?,'x',?,'x',?,'x')", _conn, t))
			using (SqliteCommand c4 = new SqliteCommand("INSERT INTO t1 VALUES ('0','0','0','1')", _conn, t))
			{
				try
				{
					c1.ExecuteNonQuery();
					c2.ExecuteNonQuery();
					c3.ExecuteNonQuery();
					c4.ExecuteNonQuery();
					t.Commit();
				}
				catch(Exception e)
				{
					t.Rollback();
					throw e;
				}
			}
		}
	}
}
