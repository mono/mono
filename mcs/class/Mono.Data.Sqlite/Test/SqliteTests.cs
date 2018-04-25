//
// SqliteTests.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite
{
	[TestFixture]
	public class SqliteTests
	{
		string _databasePath;

		[SetUp]
		public void Setup ()
		{

			_databasePath =  Path.GetTempFileName ();
			File.Delete (_databasePath);
		}

		[TearDown]
		public void TearDown ()
		{
			if (File.Exists (_databasePath))
				File.Delete (_databasePath);
		}

		[Test]
		public void DateTimeConvert_UTC ()
		{
			using (var connection = new SqliteConnection ($"Data Source={_databasePath};DateTimeKind=Utc")) {
				connection.Open ();

				using (var cmd = connection.CreateCommand ()) {
					cmd.CommandText = "CREATE TABLE OnlyDates (Date1 DATETIME)";
					cmd.CommandType = CommandType.Text;
					cmd.ExecuteNonQuery();
				}

				var datetest = DateTime.UtcNow;

				var sqlInsert = "INSERT INTO TestTable (ID, Modified) VALUES (@id, @mod)";
				using (var cmd = connection.CreateCommand ()) {
					cmd.CommandText = $"INSERT INTO OnlyDates (Date1) VALUES (@param1);";
					cmd.CommandType = CommandType.Text;
					cmd.Parameters.AddWithValue ("@param1", datetest);
					cmd.ExecuteNonQuery();
				}

				using (var cmd = connection.CreateCommand ()) {
					cmd.CommandText = $"SELECT Date1 FROM OnlyDates;";
					cmd.CommandType = CommandType.Text;
					object objRetrieved = cmd.ExecuteScalar ();
					var dateRetrieved = Convert.ToDateTime (objRetrieved);
					Assert.AreEqual (DateTimeKind.Unspecified, dateRetrieved.Kind);
				}
			}
		}

		[Test]
		public void DateTimeConvert ()
		{
			var dateTime = new DateTime (2016, 9, 15, 12, 1, 53);
			var guid = Guid.NewGuid ();

			using (var connection = new SqliteConnection ("Data Source=" + _databasePath)) {
				connection.Open ();

				var sqlCreate = "CREATE TABLE TestTable (ID uniqueidentifier PRIMARY KEY, Modified datetime)";
				using (var cmd = new SqliteCommand (sqlCreate, connection)) {
					cmd.ExecuteNonQuery ();
				}

				var sqlInsert = "INSERT INTO TestTable (ID, Modified) VALUES (@id, @mod)";
				using (var cmd = new SqliteCommand (sqlInsert, connection)) {
					cmd.Parameters.Add (new SqliteParameter ("@id", guid));
					cmd.Parameters.Add (new SqliteParameter ("@mod", dateTime));
					cmd.ExecuteNonQuery ();
				}
			}

			using (var connection = new SqliteConnection ("Data Source=" + _databasePath)) {
				connection.Open ();

				var sqlSelect = "SELECT * from TestTable";
				using (var cmd = new SqliteCommand (sqlSelect, connection))
				using (var reader = cmd.ExecuteReader ()) {
					while (reader.Read ()) {
						Assert.AreEqual (guid, reader.GetGuid (0), "#1");
						Assert.AreEqual (dateTime, reader.GetDateTime (1), "#2");
					}
				}
			}
		}
	}
}
