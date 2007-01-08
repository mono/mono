//
// DataTableLoadRowTest.cs - NUnit Test Cases for testing the
//                          DataTable's LoadRow method
// Author:
//      Sureshkumar T (tsureshkumar@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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

#if NET_2_0

using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class DataTableLoadRowTest
	{
		bool rowChanging;
		bool rowChanged;
		bool rowDeleting;
		bool rowDeleted;

		DataRow rowInAction_Changing;
		DataRowAction rowAction_Changing;
		DataRow rowInAction_Changed;
		DataRowAction rowAction_Changed;
		DataRow rowInAction_Deleting;
		DataRowAction rowAction_Deleting;
		DataRow rowInAction_Deleted;
		DataRowAction rowAction_Deleted;

		[Test]
		public void LoadRowTest ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("id", typeof (int));
			dt.Columns.Add ("name", typeof (string));

			dt.Rows.Add (new object [] { 1, "mono 1" });
			dt.Rows.Add (new object [] { 2, "mono 2" });
			dt.Rows.Add (new object [] { 3, "mono 3" });

			dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };
			dt.AcceptChanges ();

			dt.LoadDataRow (new object [] { 4, "mono 4" }, LoadOption.Upsert);
			Assert.AreEqual (4, dt.Rows.Count, "#1 has not added a new row");
		}

		[Test]
		public void LoadRowTestUpsert ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("id", typeof (int));
			dt.Columns.Add ("name", typeof (string));

			dt.Rows.Add (new object [] { 1, "mono 1" });
			dt.Rows.Add (new object [] { 2, "mono 2" });
			dt.Rows.Add (new object [] { 3, "mono 3" });

			dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };

			dt.AcceptChanges ();
			try {
				SubscribeEvents (dt);

				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 2, "mono test" }, LoadOption.Upsert);
				Assert.AreEqual (3, dt.Rows.Count, "#1 should not add a row");
				Assert.AreEqual ("mono test", dt.Rows [1] [1], "#2 should change the current");
				Assert.AreEqual ("mono 2", dt.Rows [1] [1, DataRowVersion.Original], "#3 should not change original");
				Assert.AreEqual (DataRowState.Modified, dt.Rows [1].RowState, "#4 should change state");
				Assert.IsTrue (rowChanging, "#ev1 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ev2 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Change, rowAction_Changing, "#ev3 row action is not Change");
				Assert.IsTrue (rowChanged, "#ev4 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ev5 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Change, rowAction_Changed, "#ev6 row action is not Change");
                

				// Row State tests
				// current - modified ; result - modified
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 2, "mono test 2" }, LoadOption.Upsert);
				Assert.AreEqual ("mono test 2", dt.Rows [1] [1], "#c1 should change the current");
				Assert.AreEqual ("mono 2", dt.Rows [1] [1, DataRowVersion.Original], "#c2 should not change original");
				Assert.AreEqual (DataRowState.Modified, dt.Rows [1].RowState, "#c3 should not change state");
				Assert.IsTrue (rowChanging, "#ev11 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ev12 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Change, rowAction_Changing, "#ev13 row action is not Change");
				Assert.IsTrue (rowChanged, "#ev14 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ev15 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Change, rowAction_Changed, "#ev16 row action is not Change");
                

				// current - Unchanged; result - Unchanged if no new value
				dt.AcceptChanges ();
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 2, "mono test 2" }, LoadOption.Upsert);
				Assert.AreEqual ("mono test 2", dt.Rows [1] [1], "#c4 should not change the current");
				Assert.AreEqual ("mono test 2", dt.Rows [1] [1, DataRowVersion.Original], "#c5 should not change original");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#c6 should not change state");
				Assert.IsTrue (rowChanging, "#ev21 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ev22 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Nothing, rowAction_Changing, "#ev13 row action is not Nothing");
				Assert.IsTrue (rowChanged, "#ev24 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ev25 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Nothing, rowAction_Changed, "#ev26 row action is not Nothing");
                
				// not the same value again
				dt.RejectChanges (); 
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 2, "mono test 3" }, LoadOption.Upsert);
				Assert.AreEqual (DataRowState.Modified, dt.Rows [1].RowState, "#c7 should not change state");
				Assert.IsTrue (rowChanging, "#ev31 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ev32 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Change, rowAction_Changing, "#ev33 row action is not Change");
				Assert.IsTrue (rowChanged, "#ev34 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ev35 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Change, rowAction_Changed, "#ev36 row action is not Change");
                

				// current - added; result - added
				dt.Rows.Add (new object [] { 4, "mono 4" });
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 4, "mono 4" }, LoadOption.Upsert);
				Assert.AreEqual ("mono 4", dt.Rows [3] [1], "#c8 should change the current");
				try {
					object o = dt.Rows [3] [1, DataRowVersion.Original];
					Assert.Fail ("#c9 should have thrown version not found exception");
				} catch (VersionNotFoundException) { }
				Assert.AreEqual (DataRowState.Added, dt.Rows [3].RowState, "#c10 should not change state");
				Assert.IsTrue (rowChanging, "#ev41 row changing not called");
				Assert.AreEqual (dt.Rows [3], rowInAction_Changing, "#ev42 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Change, rowAction_Changing, "#ev43 row action is not Change");
				Assert.IsTrue (rowChanged, "#ev44 row changed not called");
				Assert.AreEqual (dt.Rows [3], rowInAction_Changed, "#ev45 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Change, rowAction_Changed, "#ev46 row action is not Change");
                

				// current - none; result - added
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 5, "mono 5" }, LoadOption.Upsert);
				Assert.AreEqual ("mono 5", dt.Rows [4] [1], "#c11 should change the current");
				try {
					object o = dt.Rows [4] [1, DataRowVersion.Original];
					Assert.Fail ("#c12 should have thrown version not found exception");
				} catch (VersionNotFoundException) { }
				Assert.AreEqual (DataRowState.Added, dt.Rows [4].RowState, "#c13 should change state");
				Assert.IsTrue (rowChanging, "#ev51 row changing not called");
				Assert.AreEqual (dt.Rows [4], rowInAction_Changing, "#ev52 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Add, rowAction_Changing, "#ev53 row action is not Change");
				Assert.IsTrue (rowChanged, "#ev54 row changed not called");
				Assert.AreEqual (dt.Rows [4], rowInAction_Changed, "#ev55 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Add, rowAction_Changed, "#ev56 row action is not Change");
                

				// current - deleted; result - added a new row
				ResetEventFlags ();
				dt.AcceptChanges ();
				dt.Rows [4].Delete ();
				Assert.IsTrue (rowDeleting, "#ev57 row deleting");
				Assert.IsTrue (rowDeleted, "#ev58 row deleted");
				Assert.AreEqual (rowInAction_Deleting, dt.Rows[4], "#ev59 rowInAction_Deleting");
				Assert.AreEqual (rowInAction_Deleted, dt.Rows[4], "#ev59 rowInAction_Deleted");
				Assert.AreEqual (rowAction_Deleting, DataRowAction.Delete, "#ev60 rowInAction_Deleting");
				Assert.AreEqual (rowAction_Deleted, DataRowAction.Delete, "#ev61 rowInAction_Deleted");
				dt.LoadDataRow (new object [] { 5, "mono 5" }, LoadOption.Upsert);
				Assert.AreEqual (6, dt.Rows.Count, "#c14 should not add a row");
				Assert.AreEqual ("mono 5", dt.Rows [5] [1], "#c15 should change the current");
				try {
					object o = dt.Rows [5] [1, DataRowVersion.Original];
					Assert.Fail ("#c16 expected version not found exception ");
				} catch (VersionNotFoundException) { }
				Assert.AreEqual (DataRowState.Added, dt.Rows [5].RowState, "#c17 should change state");
				Assert.IsTrue (rowChanging, "#ev61 row changing not called");
				Assert.AreEqual (dt.Rows [5], rowInAction_Changing, "#ev62 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Add, rowAction_Changing, "#ev63 row action is not Change");
				Assert.IsTrue (rowChanged, "#ev64 row changed not called");
				Assert.AreEqual (dt.Rows [5], rowInAction_Changed, "#ev65 this row is not intended to change");
				Assert.AreEqual (DataRowAction.Add, rowAction_Changed, "#ev66 row action is not Change");
                
			} finally {
				UnsubscribeEvents (dt);
			}
		}

		[Test]
		public void LoadRowTestOverwriteChanges ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("id", typeof (int));
			dt.Columns.Add ("name", typeof (string));

			dt.Rows.Add (new object [] { 1, "mono 1" });
			dt.Rows.Add (new object [] { 2, "mono 2" });
			dt.Rows.Add (new object [] { 3, "mono 3" });

			dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };
			dt.AcceptChanges ();

			dt.Rows [1] [1] = "overwrite";
			Assert.AreEqual (DataRowState.Modified, dt.Rows [1].RowState, "#1 has not changed the row state");

			try {
				SubscribeEvents (dt);
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 2, "mono test" }, LoadOption.OverwriteChanges);
				Assert.AreEqual (3, dt.Rows.Count, "#2 has not added a new row");
				Assert.AreEqual ("mono test", dt.Rows [1] [1], "#3 should change the current");
				Assert.AreEqual ("mono test", dt.Rows [1] [1, DataRowVersion.Original], "#4 should change the original");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#5 has not changed the row state");
				Assert.IsTrue (rowChanging, "#ltoc11 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ltoc12 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changing, "#ltoc13 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltoc14 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ltoc15 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changed, "#ltoc16 row action is not Change");

				DataRow r = dt.Rows [1];
				r [1] = "test";
				Assert.AreEqual ("test", dt.Rows [1] [1], "#6 should change the current");
				Assert.AreEqual ("mono test", dt.Rows [1] [1, DataRowVersion.Original], "#7 should change the original");
				//Assert.AreEqual ("ramesh", dt.Rows [1] [1, DataRowVersion.Proposed], "#8 should change the original");

				// Row State tests
				// current - modified ; result - modified
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 2, "mono test 2" }, LoadOption.OverwriteChanges);
				Assert.AreEqual ("mono test 2", dt.Rows [1] [1], "#c1 should change the current");
				Assert.AreEqual ("mono test 2", dt.Rows [1] [1, DataRowVersion.Original], "#c2 should change original");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#c3 should not change state");
				Assert.IsTrue (rowChanging, "#ltoc21 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ltoc22 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changing, "#ltoc23 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltoc24 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ltoc25 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changed, "#ltoc26 row action is not Change");


				// current - Unchanged; result - Unchanged
				dt.AcceptChanges ();
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 2, "mono test 2" }, LoadOption.OverwriteChanges);
				Assert.AreEqual ("mono test 2", dt.Rows [1] [1], "#c4 should change the current");
				Assert.AreEqual ("mono test 2", dt.Rows [1] [1, DataRowVersion.Original], "#c5 should change original");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#c6 should not change state");
				Assert.IsTrue (rowChanging, "#ltoc31 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ltoc32 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changing, "#ltoc33 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltoc34 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ltoc35 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changed, "#ltoc36 row action is not Change");

				// current - added; result - added
				dt.Rows.Add (new object [] { 4, "mono 4" });
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 4, "mono 4" }, LoadOption.OverwriteChanges);
				Assert.AreEqual ("mono 4", dt.Rows [3] [1], "#c8 should change the current");
				Assert.AreEqual ("mono 4", dt.Rows [3] [1, DataRowVersion.Original], "#c9 should change the original");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [3].RowState, "#c10 should not change state");
				Assert.IsTrue (rowChanging, "#ltoc41 row changing not called");
				Assert.AreEqual (dt.Rows [3], rowInAction_Changing, "#ltoc42 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changing, "#ltoc43 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltoc44 row changed not called");
				Assert.AreEqual (dt.Rows [3], rowInAction_Changed, "#ltoc45 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changed, "#ltoc46 row action is not Change");


				// current - new; result - added
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 5, "mono 5" }, LoadOption.OverwriteChanges);
				Assert.AreEqual ("mono 5", dt.Rows [4] [1], "#c11 should change the current");
				Assert.AreEqual ("mono 5", dt.Rows [4] [1, DataRowVersion.Original], "#c12 should change original");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [4].RowState, "#c13 should change state");
				Assert.IsTrue (rowChanging, "#ltoc51 row changing not called");
				Assert.AreEqual (dt.Rows [4], rowInAction_Changing, "#ltoc52 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changing, "#ltoc53 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltoc54 row changed not called");
				Assert.AreEqual (dt.Rows [4], rowInAction_Changed, "#ltoc55 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changed, "#ltoc56 row action is not Change");


				// current - deleted; result - added a new row
				ResetEventFlags ();
				dt.AcceptChanges ();
				dt.Rows [4].Delete ();
				Assert.IsTrue (rowDeleting, "#ltoc57 row deleting");
				Assert.IsTrue (rowDeleted, "#ltoc58 row deleted");
				Assert.AreEqual (rowInAction_Deleting, dt.Rows[4], "#ltoc59 rowInAction_Deleting");
				Assert.AreEqual (rowInAction_Deleted, dt.Rows[4], "#ltoc60 rowInAction_Deleted");
				Assert.AreEqual (rowAction_Deleting, DataRowAction.Delete, "#ltoc60 rowInAction_Deleting");
				Assert.AreEqual (rowAction_Deleted, DataRowAction.Delete, "#ltoc61 rowInAction_Deleted");
				dt.LoadDataRow (new object [] { 5, "mono 51" }, LoadOption.OverwriteChanges);
				Assert.AreEqual (5, dt.Rows.Count, "#c14 should not add a row");
				Assert.AreEqual ("mono 51", dt.Rows [4] [1], "#c15 should change the current");
				Assert.AreEqual ("mono 51", dt.Rows [4] [1, DataRowVersion.Original], "#c16 should change the current");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [4].RowState, "#c17 should change state");
				Assert.IsTrue (rowChanging, "#ltoc61 row changing not called");
				Assert.AreEqual (dt.Rows [4], rowInAction_Changing, "#ltoc62 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changing, "#ltoc63 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltoc64 row changed not called");
				Assert.AreEqual (dt.Rows [4], rowInAction_Changed, "#ltoc65 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changed, "#ltoc66 row action is not Change");

			} finally {
				UnsubscribeEvents (dt);
			}
		}

		[Test]
		public void LoadRowTestPreserveChanges ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("id", typeof (int));
			dt.Columns.Add ("name", typeof (string));

			dt.Rows.Add (new object [] { 1, "mono 1" });
			dt.Rows.Add (new object [] { 2, "mono 2" });
			dt.Rows.Add (new object [] { 3, "mono 3" });

			dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };
			dt.AcceptChanges ();
			try {
				SubscribeEvents (dt);

				// current - modified; new - modified
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 2, "mono test" }, LoadOption.PreserveChanges);
				Assert.AreEqual (3, dt.Rows.Count, "#1 should not add a new row");
				Assert.AreEqual ("mono test", dt.Rows [1] [1], "#2 should change the current");
				Assert.AreEqual ("mono test", dt.Rows [1] [1, DataRowVersion.Original], "#3 should change the original");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#4 has not changed the row state");
				Assert.IsTrue (rowChanging, "#ltpc11 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ltpc12 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changing, "#ltpc13 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltpc14 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ltpc15 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changed, "#ltpc16 row action is not Change");

				// current - none; new - unchanged
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 4, "mono 4" }, LoadOption.PreserveChanges);
				Assert.AreEqual (4, dt.Rows.Count,"#5 should add a new row");
				Assert.AreEqual ("mono 4", dt.Rows [3] [1], "#6 should change the current");
				Assert.AreEqual ("mono 4", dt.Rows [3] [1, DataRowVersion.Original], "#7 should change the original");
				Assert.AreEqual (DataRowState.Unchanged, dt.Rows [3].RowState, "#8 has not changed the row state");
				Assert.IsTrue (rowChanging, "#ltpc21 row changing not called");
				Assert.AreEqual (dt.Rows [3], rowInAction_Changing, "#ltpc22 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changing, "#ltpc23 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltpc24 row changed not called");
				Assert.AreEqual (dt.Rows [3], rowInAction_Changed, "#ltpc25 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeCurrentAndOriginal, rowAction_Changed, "#ltpc16 row action is not Change");


				dt.RejectChanges ();
                
				// current - added; new - modified
				dt.Rows.Add (new object [] { 5, "mono 5" });
				ResetEventFlags ();
				dt.LoadDataRow (new object [] { 5, "mono test" }, LoadOption.PreserveChanges);
				Assert.AreEqual (5, dt.Rows.Count, "#9 should not add a new row");
				Assert.AreEqual ("mono 5", dt.Rows [4] [1], "#10 should not change the current");
				Assert.AreEqual ("mono test", dt.Rows [4] [1, DataRowVersion.Original], "#11 should change the original");
				Assert.AreEqual (DataRowState.Modified, dt.Rows [4].RowState, "#12 has not changed the row state");
				Assert.IsTrue (rowChanging, "#ltpc31 row changing not called");
				Assert.AreEqual (dt.Rows [4], rowInAction_Changing, "#ltpc32 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeOriginal, rowAction_Changing, "#ltpc33 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltpc34 row changed not called");
				Assert.AreEqual (dt.Rows [4], rowInAction_Changed, "#ltpc35 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeOriginal, rowAction_Changed, "#ltpc36 row action is not Change");


				dt.RejectChanges ();

				// current - deleted ; new - deleted ChangeOriginal
				ResetEventFlags ();
				dt.Rows [1].Delete ();
				Assert.IsTrue (rowDeleting, "#ltpc37 row deleting");
				Assert.IsTrue (rowDeleted, "#ltpc38 row deleted");
				Assert.AreEqual (rowInAction_Deleting, dt.Rows[1], "#ltpc39 rowInAction_Deleting");
				Assert.AreEqual (rowInAction_Deleted, dt.Rows[1], "#ltpc40 rowInAction_Deleted");
				Assert.AreEqual (rowAction_Deleting, DataRowAction.Delete, "#ltpc60 rowInAction_Deleting");
				Assert.AreEqual (rowAction_Deleted, DataRowAction.Delete, "#ltpc61 rowInAction_Deleted");
				dt.LoadDataRow (new object [] { 2, "mono deleted" }, LoadOption.PreserveChanges);
				Assert.AreEqual (5, dt.Rows.Count, "#13 should not add a new row");
				Assert.AreEqual ("mono deleted", dt.Rows [1] [1, DataRowVersion.Original], "#14 should change the original");
				Assert.AreEqual (DataRowState.Deleted, dt.Rows [1].RowState, "#15 has not changed the row state");
				Assert.IsTrue (rowChanging, "#ltpc41 row changing not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changing, "#ltpc42 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeOriginal, rowAction_Changing, "#ltoc43 row action is not Change");
				Assert.IsTrue (rowChanged, "#ltpc44 row changed not called");
				Assert.AreEqual (dt.Rows [1], rowInAction_Changed, "#ltpc45 this row is not intended to change");
				Assert.AreEqual (DataRowAction.ChangeOriginal, rowAction_Changed, "#ltpc46 row action is not Change");


			} finally {
				UnsubscribeEvents (dt);
			}
		}

		[Test]
		public void LoadRowDefaultValueTest ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("id", typeof (int));
			dt.Columns.Add ("age", typeof (int));
			dt.Columns.Add ("name", typeof (string));

			dt.Columns [1].DefaultValue = 20;

			dt.Rows.Add (new object [] { 1, 15, "mono 1" });
			dt.Rows.Add (new object [] { 2, 25, "mono 2" });
			dt.Rows.Add (new object [] { 3, 35, "mono 3" });

			dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };

			dt.AcceptChanges ();

			dt.LoadDataRow (new object [] { 2, null, "mono test" }, LoadOption.OverwriteChanges);
			Assert.AreEqual (3, dt.Rows.Count, "#1 should not have added a row");
			Assert.AreEqual (20, dt.Rows [1] [1], "#2 should be default value");
			Assert.AreEqual (20, dt.Rows [1] [1, DataRowVersion.Original], "#3 should be default value");

		}

		[Test]
		public void LoadRowAutoIncrementTest ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("id", typeof (int));
			dt.Columns.Add ("age", typeof (int));
			dt.Columns.Add ("name", typeof (string));

			dt.Columns [0].AutoIncrementSeed = 10;
			dt.Columns [0].AutoIncrementStep = 5;
			dt.Columns [0].AutoIncrement = true;

			dt.Rows.Add (new object [] { null, 15, "mono 1" });
			dt.Rows.Add (new object [] { null, 25, "mono 2" });
			dt.Rows.Add (new object [] { null, 35, "mono 3" });

			dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };

			dt.AcceptChanges ();

			dt.LoadDataRow (new object [] { null, 20, "mono test" }, LoadOption.OverwriteChanges);
			Assert.AreEqual (4, dt.Rows.Count, "#1 has not added a new row");
			Assert.AreEqual (25, dt.Rows [3] [0], "#2 current should be ai");
			Assert.AreEqual (25, dt.Rows [3] [0, DataRowVersion.Original], "#3 original should be ai");

			dt.LoadDataRow (new object [] { 25, 20, "mono test" }, LoadOption.Upsert);
			dt.LoadDataRow (new object [] { 25, 20, "mono test 2" }, LoadOption.Upsert);
			dt.LoadDataRow (new object [] { null, 20, "mono test aaa" }, LoadOption.Upsert);

			Assert.AreEqual (5, dt.Rows.Count, "#4 has not added a new row");
			Assert.AreEqual (25, dt.Rows [3] [0], "#5 current should be ai");
			Assert.AreEqual (25, dt.Rows [3] [0, DataRowVersion.Original], "#6 original should be ai");

			Assert.AreEqual (30, dt.Rows [4] [0], "#7 current should be ai");

		}

		public void SubscribeEvents (DataTable dt)
		{
			dt.RowChanging += new DataRowChangeEventHandler (dt_RowChanging);
			dt.RowChanged += new DataRowChangeEventHandler (dt_RowChanged);
			dt.RowDeleted += new DataRowChangeEventHandler (dt_RowDeleted);
			dt.RowDeleting += new DataRowChangeEventHandler (dt_RowDeleting);
			//dt.TableNewRow += new DataTableNewRowEventHandler (dt_TableNewRow);
		}

        
		public void UnsubscribeEvents (DataTable dt)
		{
			dt.RowChanging -= new DataRowChangeEventHandler (dt_RowChanging);
			dt.RowChanged -= new DataRowChangeEventHandler (dt_RowChanged);
			dt.RowDeleted -= new DataRowChangeEventHandler (dt_RowDeleted);
			dt.RowDeleting -= new DataRowChangeEventHandler (dt_RowDeleting);
			//dt.TableNewRow -= new DataTableNewRowEventHandler (dt_TableNewRow);
		}

		public void ResetEventFlags ()
		{
			rowChanging = false;
			rowChanged = false;
			rowDeleting = false;
			rowDeleted = false;
			rowInAction_Changing = null;
			rowAction_Changing = DataRowAction.Nothing;
			rowInAction_Changed = null;
			rowAction_Changed = DataRowAction.Nothing;
			rowInAction_Deleting = null;
			rowAction_Deleting = DataRowAction.Nothing;
			rowInAction_Deleted = null;
			rowAction_Deleted = DataRowAction.Nothing;
		}

		void dt_RowDeleting (object sender, DataRowChangeEventArgs e)
		{
			rowDeleting = true;
			rowInAction_Deleting = e.Row;
			rowAction_Deleting = e.Action;
		}

		void dt_RowDeleted (object sender, DataRowChangeEventArgs e)
		{
			rowDeleted = true;
			rowInAction_Deleted = e.Row;
			rowAction_Deleted = e.Action;
		}

		void dt_RowChanged (object sender, DataRowChangeEventArgs e)
		{
			rowChanged = true;
			rowInAction_Changed = e.Row;
			rowAction_Changed = e.Action;
		}

		void dt_RowChanging (object sender, DataRowChangeEventArgs e)
		{
			rowChanging = true;
			rowInAction_Changing = e.Row;
			rowAction_Changing = e.Action;
		}

	}
}

#endif // NET_2_0
