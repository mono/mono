//
// Unit test for bug https://bugzilla.xamarin.com/show_bug.cgi?id=27864
//
// Authors:
//	Thomas Zoechling <thomas.zoechling@gmx.at>
//	Alex Soto <alex.soto@xamarin.com>
//	
//
// Copyright 2015 Xamarin Inc.
//

using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite {
	
	[TestFixture]
	public class SqliteiOS82BugTests {
		string dbPath;
		string connectionString;
		SqliteConnection cnn;
		
		[SetUp]
		public void Create()
		{
			dbPath = Path.GetTempFileName ();

			// We want to start with a fresh db for each full run
			// The database is created on the first open()
			// but TempFile does create a file
			if (File.Exists (dbPath))
				File.Delete (dbPath);

			connectionString = "Data Source=" + dbPath;
			cnn = new SqliteConnection (connectionString);

			try {
				if(File.Exists(dbPath)) {
					cnn.Dispose();
					// We want to start with a fresh db for each full run
					// The database is created on the first open()
					File.Delete(dbPath);
				}
			}
			catch(Exception e) {
				throw e;
			}

			try {
				using (var createCommand = new SqliteCommand ("CREATE TABLE Company (RecordId int, Name text);", cnn))
				using (var insertCommand = new SqliteCommand ("INSERT INTO Company VALUES (1, 'Test CO')", cnn)) {
					cnn.Open();
					createCommand.ExecuteNonQuery();
					insertCommand.ExecuteNonQuery();
				}
			}
			catch(Exception e) {
				Console.WriteLine (e);
				throw new AssertionException ("Create table failed", e);
			}
			finally {
				cnn.Close();  
			}
		}

		[TearDown]
		public void TearDown ()
		{
			if (File.Exists (dbPath))
				File.Delete (dbPath);
		}

		// Ref: https://bugzilla.xamarin.com/show_bug.cgi?id=27864
		// As of iOS 8.2 Apple updated Sqlite and broke queries that used to work pre iOS 8.2 like the select command in this test
		// The pruppose of this test is to know when apple fixes this. Expected test behaivour is as follows.
		// If iOS 8.2+ Test will pass as long as apple does not fix the bug
		// If iOS < 8.2 Test will pass as it does work as expected
		// If iOS 8.2+ and apple fixed bug 27864 test will fail and we will need to remove the fail check on lines 105 and 106
		[Test]	
		public void SqliteSelectTestBug27864()
		{
			try {
				var cmdText = "SELECT " +
					"2 AS SortOrder, " +
					"com.Name AS 'Company.Name' " +
					"FROM " +
					"company com " +
					"UNION " +
					"SELECT " +
					"0 AS SortOrder, " +
					"com.Name AS 'Company.Name' " +
					"FROM " +
					"Company com " +
					"ORDER BY " +
					"com.Name, " +
					"SortOrder COLLATE NOCASE";

				using (cnn)
				using (var cmd = new SqliteCommand (cmdText, cnn)) {
					cnn.Open();
					using (SqliteDataReader dr = cmd.ExecuteReader()) {
						var i = 0;
						while (dr.Read()) {
							Assert.AreEqual(dr["SortOrder"], i);
							Assert.AreEqual(dr["Company.Name"], "Test CO");
							i += 2;
						}
						Assert.IsTrue(dr.FieldCount>0, i.ToString ());
					}
				}
			} catch (SqliteException ex) {
#if MONOTOUCH
				// Expected Exception from iOS 8.2 (broken) to 9.0 (fixed)
				if (BCL.Tests.TestRuntime.CheckSystemVersion (8,2) && !BCL.Tests.TestRuntime.CheckSystemVersion (9,0)) 
					Assert.That (ex.Message.Contains ("no such column: com.Name"));
				else
					throw;
#endif
			}
		}
	}
}
