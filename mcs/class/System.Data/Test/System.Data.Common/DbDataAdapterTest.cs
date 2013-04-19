//
// DbDataAdapterTest.cs - NUnit Test Cases for testing the DbDataAdapter class
//
// Author:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2007 Gert Driesen
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
using System.Data.Common;
using System.Data.SqlClient;

/*--For Bug 853 Test Begin--*/
#if !MOBILE
using Mono.Data.Sqlite;
#endif
/*--For Bug 853 Test End--*/

using NUnit.Framework;

namespace MonoTests.System.Data.Common
{
	[TestFixture]
	public class DbDataAdapterTest
	{
#if NET_2_0
		[Test]
		public void UpdateBatchSize ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.UpdateBatchSize = 0;
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}
			Assert.AreEqual (1, da.UpdateBatchSize, "#A5");

			try {
				da.UpdateBatchSize = int.MaxValue;
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
			Assert.AreEqual (1, da.UpdateBatchSize, "#B5");

			da.UpdateBatchSize = 1;
			Assert.AreEqual (1, da.UpdateBatchSize, "#C");
		}

		[Test]
		public void UpdateBatchSize_Negative ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.UpdateBatchSize = -1;
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void AddToBatch ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.AddToBatch (new SqlCommand ());
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void ClearBatch ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.ClearBatch ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void ExecuteBatch ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.ExecuteBatch ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void GetBatchedParameter ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.GetBatchedParameter (1, 1);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void GetBatchedRecordsAffected ()
		{
			MyAdapter da = new MyAdapter ();
			int recordsAffected = 0;
			Exception error = null;

			Assert.IsTrue (da. GetBatchedRecordsAffected (int.MinValue,
				out recordsAffected, out error), "#1");
			Assert.AreEqual (1, recordsAffected, "#2");
			Assert.IsNull (error, "#3");
		}

		[Test]
		public void InitializeBatching ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.InitializeBatching ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TerminateBatching ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.TerminateBatching ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}
#if !MOBILE	
		[Test]
		[Category ("NotWorking")] // Requires newer sqlite than is on wrench
		public void XimarinBugzillaBug853Test()
                {
                        const string connectionString = "URI = file:./SqliteTest.db; Version = 3";//will be in System.Data directory
                        SqliteConnection dbConnection = new SqliteConnection(connectionString);
                        dbConnection.Open();
			SqliteCommand ClearTableEntry=new SqliteCommand("DELETE FROM Primus;",dbConnection);
			ClearTableEntry.ExecuteNonQuery();

                        SqliteDataAdapter sqliteDataAdapter = new SqliteDataAdapter("SELECT * FROM primus", dbConnection);
                        SqliteCommandBuilder builder = new SqliteCommandBuilder(sqliteDataAdapter);
			sqliteDataAdapter.InsertCommand = builder.GetInsertCommand();
                        sqliteDataAdapter.DeleteCommand = builder.GetDeleteCommand();
			
                        DataSet dataSet = new DataSet();

                        sqliteDataAdapter.Fill(dataSet, "Primus");//reset

                        DataRow rowToBeAdded = dataSet.Tables["Primus"].NewRow();
                        rowToBeAdded["id"] = 123;
                        rowToBeAdded["name"] = "Name";//not null primary key
                        rowToBeAdded["value"] = 777;

                        dataSet.Tables["Primus"].Rows.Add(rowToBeAdded);
sqliteDataAdapter.Update (dataSet, "Primus");

			//This would fail with NULL constraint violation in bug
			//report.  Because before the patch, it would create
			//a new record with all fields being null-- if the
			//exception rises, test fails
                        sqliteDataAdapter.Update (dataSet, "Primus");

                        dbConnection.Close();
                        dbConnection = null;
		}
#endif

#endif

		class MyAdapter : DbDataAdapter
		{
#if ONLY_1_1
			protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command,
										     StatementType statementType,
										     DataTableMapping tableMapping)
			{
				throw new NotImplementedException ();
			}

			protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command,
										       StatementType statementType,
										       DataTableMapping tableMapping)
			{
				throw new NotImplementedException ();
			}

			protected override void OnRowUpdated (RowUpdatedEventArgs value)
			{
				throw new NotImplementedException ();
			}

			protected override void OnRowUpdating (RowUpdatingEventArgs value)
			{
				throw new NotImplementedException ();
			}
#endif

#if NET_2_0
			public new int AddToBatch (IDbCommand command)
			{
				return base.AddToBatch (command);
			}

			public new void ClearBatch ()
			{
				base.ClearBatch ();
			}

			public new void ExecuteBatch ()
			{
				base.ClearBatch ();
			}

			public new IDataParameter GetBatchedParameter (int commandIdentifier, int parameterIndex)
			{
				return base.GetBatchedParameter (commandIdentifier, parameterIndex);
			}

			public new bool GetBatchedRecordsAffected (int commandIdentifier, out int recordsAffected, out Exception error)
			{
				return base.GetBatchedRecordsAffected (commandIdentifier, out recordsAffected, out error);
			}

			public new void InitializeBatching ()
			{
				base.InitializeBatching ();
			}

			public new void TerminateBatching ()
			{
				base.TerminateBatching ();
			}
#endif
		}
	}
}
