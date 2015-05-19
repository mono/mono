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

#if MONOTOUCH
using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite {
	
	[TestFixture]
	public class SqliteiOS82BugTests {
		readonly static string dbPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "adodemo.db3");
		readonly static string connectionString = "Data Source=" + dbPath;
		static SqliteConnection cnn = new SqliteConnection (connectionString);
		
		[SetUp]
		public void Create()
		{
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
						Assert.IsTrue(dr.FieldCount>0);
					}
					if (BCL.Tests.TestRuntime.CheckSystemVersion (8, 2))
						Assert.IsTrue (false, "Apple fixed bug 27864, this check can now be removed");
				}
			} catch (SqliteException ex) {

				if (BCL.Tests.TestRuntime.CheckSystemVersion (8, 2)) // Expected Exception on iOS 8.2+, if this does not happen anymore it means apple fixed it
					Assert.That (ex.Message.Contains ("no such column: com.Name"));
				else
					throw new AssertionException ("Unexpected Sqlite Error", ex); // This should not happen
			}
		}
	}
}
#endif