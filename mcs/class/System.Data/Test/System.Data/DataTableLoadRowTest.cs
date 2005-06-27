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

                        dt.LoadDataRow (new object [] { 2, "mono test" }, LoadOption.Upsert);
                        Assert.AreEqual (3, dt.Rows.Count, "#1 should not add a row");
                        Assert.AreEqual ("mono test", dt.Rows [1] [1], "#2 should change the current");
                        Assert.AreEqual ("mono 2", dt.Rows [1] [1, DataRowVersion.Original], "#3 should not change original");
                        Assert.AreEqual (DataRowState.Modified, dt.Rows [1].RowState, "#4 should change state");


                        // Row State tests
                        // current - modified ; result - modified
                        dt.LoadDataRow (new object [] { 2, "mono test 2" }, LoadOption.Upsert);
                        Assert.AreEqual ("mono test 2", dt.Rows [1] [1], "#c1 should change the current");
                        Assert.AreEqual ("mono 2", dt.Rows [1] [1, DataRowVersion.Original], "#c2 should not change original");
                        Assert.AreEqual (DataRowState.Modified, dt.Rows [1].RowState, "#c3 should not change state");

                        // current - Unchanged; result - Unchanged if no new value
                        dt.AcceptChanges ();
                        dt.LoadDataRow (new object [] { 2, "mono test 2" }, LoadOption.Upsert);
                        Assert.AreEqual ("mono test 2", dt.Rows [1] [1], "#c4 should change the current");
                        Assert.AreEqual ("mono test 2", dt.Rows [1] [1, DataRowVersion.Original], "#c5 should not change original");
                        Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#c6 should not change state");
                        // not the same value again
                        dt.RejectChanges ();
                        dt.LoadDataRow (new object [] { 2, "mono test 3" }, LoadOption.Upsert);
                        Assert.AreEqual (DataRowState.Modified, dt.Rows [1].RowState, "#c7 should not change state");

                        // current - added; result - added
                        dt.Rows.Add (new object [] { 4, "mono 4" });
                        dt.LoadDataRow (new object [] { 4, "mono 4" }, LoadOption.Upsert);
                        Assert.AreEqual ("mono 4", dt.Rows [3] [1], "#c8 should change the current");
                        try {
                                object o = dt.Rows [3] [1, DataRowVersion.Original];
                                Assert.Fail ("#c9 should have thrown version not found exception");
                        } catch (VersionNotFoundException) { }
                        Assert.AreEqual (DataRowState.Added, dt.Rows [3].RowState, "#c10 should not change state");

                        // current - new; result - added
                        dt.LoadDataRow (new object [] { 5, "mono 5" }, LoadOption.Upsert);
                        Assert.AreEqual ("mono 5", dt.Rows [4] [1], "#c11 should change the current");
                        try {
                                object o = dt.Rows [4] [1, DataRowVersion.Original];
                                Assert.Fail ("#c12 should have thrown version not found exception");
                        } catch (VersionNotFoundException) { }
                        Assert.AreEqual (DataRowState.Added, dt.Rows [4].RowState, "#c13 should change state");

                        // current - deleted; result - added a new row
                        dt.AcceptChanges ();
                        dt.Rows [4].Delete ();
                        dt.LoadDataRow (new object [] { 5, "mono 5" }, LoadOption.Upsert);
                        Assert.AreEqual (6, dt.Rows.Count, "#c14 should not add a row");
                        Assert.AreEqual ("mono 5", dt.Rows [5] [1], "#c15 should change the current");
                        try {
                                object o = dt.Rows [5] [1, DataRowVersion.Original];
                                Assert.Fail ("#c16 expected version not found exception ");
                        } catch (VersionNotFoundException) {}
                        Assert.AreEqual (DataRowState.Added, dt.Rows [5].RowState, "#c17 should change state");
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

                        dt.LoadDataRow (new object [] { 2, "mono test" }, LoadOption.OverwriteChanges);
                        Assert.AreEqual (3, dt.Rows.Count, "#2 has not added a new row");
                        Assert.AreEqual ("mono test", dt.Rows [1] [1], "#3 should change the current");
                        Assert.AreEqual ("mono test", dt.Rows [1] [1, DataRowVersion.Original], "#4 should change the original");
                        Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#5 has not changed the row state");

                        DataRow r = dt.Rows [1];
                        r [1] = "test";
                        Assert.AreEqual ("test", dt.Rows [1] [1], "#6 should change the current");
                        Assert.AreEqual ("mono test", dt.Rows [1] [1, DataRowVersion.Original], "#7 should change the original");
                        //Assert.AreEqual ("ramesh", dt.Rows [1] [1, DataRowVersion.Proposed], "#8 should change the original");

                        // Row State tests
                        // current - modified ; result - modified
                        dt.LoadDataRow (new object [] { 2, "mono test 2" }, LoadOption.OverwriteChanges);
                        Assert.AreEqual ("mono test 2", dt.Rows [1] [1], "#c1 should change the current");
                        Assert.AreEqual ("mono test 2", dt.Rows [1] [1, DataRowVersion.Original], "#c2 should change original");
                        Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#c3 should not change state");

                        // current - Unchanged; result - Unchanged if no new value
                        dt.AcceptChanges ();
                        dt.LoadDataRow (new object [] { 2, "mono test 2" }, LoadOption.OverwriteChanges);
                        Assert.AreEqual ("mono test 2", dt.Rows [1] [1], "#c4 should change the current");
                        Assert.AreEqual ("mono test 2", dt.Rows [1] [1, DataRowVersion.Original], "#c5 should change original");
                        Assert.AreEqual (DataRowState.Unchanged, dt.Rows [1].RowState, "#c6 should not change state");
                        
                        // current - added; result - added
                        dt.Rows.Add (new object [] { 4, "mono 4" });
                        dt.LoadDataRow (new object [] { 4, "mono 4" }, LoadOption.OverwriteChanges);
                        Assert.AreEqual ("mono 4", dt.Rows [3] [1], "#c8 should change the current");
                        Assert.AreEqual ("mono 4", dt.Rows [3] [1, DataRowVersion.Original], "#c9 should change the original");
                        Assert.AreEqual (DataRowState.Unchanged, dt.Rows [3].RowState, "#c10 should not change state");

                        // current - new; result - added
                        dt.LoadDataRow (new object [] { 5, "mono 5" }, LoadOption.OverwriteChanges);
                        Assert.AreEqual ("mono 5", dt.Rows [4] [1], "#c11 should change the current");
                        Assert.AreEqual ("mono 5", dt.Rows [4] [1, DataRowVersion.Original], "#c12 should change original");
                        Assert.AreEqual (DataRowState.Unchanged, dt.Rows [4].RowState, "#c13 should change state");

                        // current - deleted; result - added a new row
                        dt.AcceptChanges ();
                        dt.Rows [4].Delete ();
                        dt.LoadDataRow (new object [] { 5, "mono 51" }, LoadOption.OverwriteChanges);
                        Assert.AreEqual (5, dt.Rows.Count, "#c14 should not add a row");
                        Assert.AreEqual ("mono 51", dt.Rows [4] [1], "#c15 should change the current");
                        Assert.AreEqual ("mono 51", dt.Rows [4] [1, DataRowVersion.Original], "#c16 should change the current");
                        Assert.AreEqual (DataRowState.Unchanged, dt.Rows [4].RowState, "#c17 should change state");
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
         
                        dt.LoadDataRow (new object [] { 2, "mono test" }, LoadOption.PreserveChanges);
                        Assert.AreEqual (3, dt.Rows.Count, "#1 should not add a new row");
                        Assert.AreEqual ("mono 2", dt.Rows [1] [1], "#2 should not change the current");
                        Assert.AreEqual ("mono test", dt.Rows [1] [1, DataRowVersion.Original], "#3 should change the original");

                        dt.LoadDataRow (new object [] { 4, "mono 4" }, LoadOption.PreserveChanges);
                        Assert.AreEqual (4, dt.Rows.Count, "#5 should add a new row");
                        Assert.AreEqual ("mono 4", dt.Rows [3] [1], "#6 should change the current");
                        Assert.AreEqual ("mono 4", dt.Rows [3] [1, DataRowVersion.Original], "#7 should change the original");
                }

                [Test]
                public void LoadRowDefaultValueTest ()
                {
                        DataTable dt = new DataTable ();
                        dt.Columns.Add ("id", typeof (int));
                        dt.Columns.Add ("age", typeof (int));
                        dt.Columns.Add ("name", typeof (string));

                        dt.Columns [1].DefaultValue = 20;

                        dt.Rows.Add (new object [] {1, 15, "mono 1"});
                        dt.Rows.Add (new object [] { 2, 25, "mono 2" });
                        dt.Rows.Add (new object [] { 3, 35, "mono 3" });

                        dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };

                        dt.AcceptChanges ();

                        dt.LoadDataRow (new object [] { 2, null, "mono test" }, LoadOption.OverwriteChanges);
                        Assert.AreEqual ( 3, dt.Rows.Count, "#1 should not have added a row");
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

			dt.LoadDataRow (new object [] {25, 20, "mono test"}, LoadOption.Upsert);
			dt.LoadDataRow (new object [] {25, 20, "mono test 2"}, LoadOption.Upsert);
			dt.LoadDataRow (new object [] {null, 20, "mono test aaa"}, LoadOption.Upsert);
			
			Assert.AreEqual (5, dt.Rows.Count, "#4 has not added a new row");
			Assert.AreEqual (25, dt.Rows [3] [0], "#5 current should be ai");
			Assert.AreEqual (25, dt.Rows [3] [0, DataRowVersion.Original], "#6 original should be ai");

			Assert.AreEqual (30, dt.Rows [4] [0], "#7 current should be ai");

                }
        }
}

#endif // NET_2_0
