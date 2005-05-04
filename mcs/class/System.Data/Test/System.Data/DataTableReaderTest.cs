
// DataTableReaderTest.cs - NUnit Test Cases for testing the DataTableReader
//
// Authors:
//   Sureshkumar T <tsureshkumar@novell.com>
// 
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
        [TestFixture]
        public class DataTableReaderTest
        {
                DataTable dt;

                [SetUp]
                public void Setup ()
                {
                        dt = new DataTable ("test");
                        dt.Columns.Add ("id", typeof (int));
                        dt.Columns.Add ("name", typeof (string));
                        dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };

                        dt.Rows.Add (new object [] { 1, "mono 1" });
                        dt.Rows.Add (new object [] { 2, "mono 2" });
                        dt.Rows.Add (new object [] { 3, "mono 3" });

                        dt.AcceptChanges ();

                }

                #region Positive Tests
                [Test]
                public void CtorTest ()
                {
                        dt.Rows [1].Delete ();
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                
                                int i = 0;
                                while (reader.Read ())
                                        i++;
                                reader.Close ();

                                Assert.AreEqual (2, i, "no. of rows iterated is wrong");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
                public void RowInAccessibleTest ()
                {

                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                                reader.Read (); // 2nd row
                                dt.Rows [1].Delete ();
                                string value = reader [1].ToString ();
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void IgnoreDeletedRowsDynamicTest ()
                {

                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                dt.Rows [1].Delete ();
                                reader.Read (); // it should be 3rd row
                                string value = reader [0].ToString ();
                                Assert.AreEqual ("3", value, "#1 reader should have moved to 3rd row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void SeeTheModifiedTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                dt.Rows [1] ["name"] = "mono changed";
                                reader.Read ();
                                string value = reader [1].ToString ();
                                Assert.AreEqual ("mono changed", value, "#2 reader should have moved to 3rd row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void SchemaTest ()
                {
                        DataTable another = new DataTable ("another");
                        another.Columns.Add ("x", typeof (string));

                        another.Rows.Add (new object [] {"test 1" });
                        another.Rows.Add (new object [] {"test 2" });
                        another.Rows.Add (new object [] {"test 3" });

                        DataTableReader reader = new DataTableReader (new DataTable [] { dt, another });
                        try {
                                DataTable schema = reader.GetSchemaTable ();

                                Assert.AreEqual (dt.Columns.Count, schema.Rows.Count, "#1 should be same");
                                Assert.AreEqual (dt.Columns [1].DataType.ToString (), schema.Rows [1] ["DataType"].ToString (), "#2 data type should match");

                                reader.NextResult (); //schema should change here
                                schema = reader.GetSchemaTable ();

                                Assert.AreEqual (another.Columns.Count, schema.Rows.Count, "#3 should be same");
                                Assert.AreEqual (another.Columns [0].DataType.ToString (), schema.Rows [0] ["DataType"].ToString (), "#4 data type should match");
                        
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void MultipleResultSetsTest ()
                {
                        DataTable dt1 = new DataTable ("test2");
                        dt1.Columns.Add ("x", typeof (string));
                        dt1.Rows.Add (new object [] {"test"} );
                        dt1.Rows.Add (new object [] {"test1"} );
                        dt1.AcceptChanges ();
                        
                        DataTable [] collection = new DataTable [] { dt, dt1 } ; 
                        
                        DataTableReader reader = new DataTableReader (collection);
                        try {
                                int i = 0;
                                do {
                                        while (reader.Read ())
                                                i++;
                                } while (reader.NextResult ());
                                                
                                Assert.AreEqual (5, i, "#1 rows should be of both the tables");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void GetTest ()
                {
                        dt.Columns.Add ("nullint", typeof (int));
                        dt.Rows [0] ["nullint"] = 333;

                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                        
                                int ordinal = reader.GetOrdinal ("nullint");
                                // Get by name
                                Assert.AreEqual (1, (int) reader ["id"], "#1 should be able to get by name");
                                Assert.AreEqual (333, reader.GetInt32 (ordinal), "#2 should get int32");
                                Assert.AreEqual ("System.Int32", reader.GetDataTypeName (ordinal), "#3 data type should match");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
                public void CloseTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                int i = 0;
                                while (reader.Read () && i < 1)
                                        i++;
                                reader.Close ();
                                reader.Read ();
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void GetOrdinalTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                Assert.AreEqual (1, reader.GetOrdinal ("name"), "#1 get ordinal should work even" +
                                                 " without calling Read");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }
                #endregion // Positive Tests

                
                #region Negative Tests
                [Test]
                public void NoRowsTest ()
                {
                        dt.Rows.Clear ();
                        dt.AcceptChanges ();
                        
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                        
                                Assert.AreEqual (false, reader.Read (), "#1 there are no rows");
                                Assert.AreEqual (false, reader.NextResult (), "#2 there are no further resultsets");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }
                
                [Test]
                [ExpectedException (typeof (ArgumentException))]
                public void NoTablesTest ()
                {
                        DataTableReader reader = new DataTableReader (new DataTable [] {});
                        try {
                                reader.Read ();
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

		[Test]
                [ExpectedException (typeof (InvalidOperationException))]
		public void ReadAfterClosedTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                                reader.Close ();
                                reader.Read ();
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}	

		[Test]
                [ExpectedException (typeof (InvalidOperationException))]
		public void AccessAfterClosedTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                                reader.Close ();
                                int i = (int) reader [0];
                                i++; // to supress warning
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
		public void AccessBeforeReadTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                int i = (int) reader [0];
                                i++; // to supress warning
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}

                [Test]
                [ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InvalidIndexTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                                int i = (int) reader [90]; // kidding, ;-)
                                i++; // to supress warning
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}

                [Test]
		public void DontSeeTheEarlierRowsTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row

                                // insert a row at position 0
                                DataRow r = dt.NewRow ();
                                r [0] = 0;
                                r [1] = "adhi bagavan";
                                dt.Rows.InsertAt (r, 0);
                        
                                Assert.AreEqual (2, (int) reader.GetInt32 (0), "#1 should not alter the position");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}

                [Test]
                public void AddBeforePointTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                DataRow r = dt.NewRow ();
                                r [0] = 0;
                                r [1] = "adhi bagavan";
                                dt.Rows.InsertAt (r, 0);
                                dt.Rows.Add (new object [] { 4, "mono 4"}); // should not affect the counter
                                Assert.AreEqual (2, (int) reader [0], "#1 should not affect the current position");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void AddAtPointTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                DataRow r = dt.NewRow ();
                                r [0] = 0;
                                r [1] = "same point";
                                dt.Rows.InsertAt (r, 1);
                                dt.Rows.Add (new object [] { 4, "mono 4"}); // should not affect the counter
                                Assert.AreEqual (2, (int) reader [0], "#1 should not affect the current position");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void DeletePreviousAndAcceptChangesTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                dt.Rows [0].Delete ();
                                dt.AcceptChanges ();
                                Assert.AreEqual (2, (int) reader [0], "#1 should not affect the current position");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }

                }

                [Test]
                public void DeleteCurrentAndAcceptChangesTest2 ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                dt.Rows [1].Delete (); // delete row, where reader points to
                                dt.AcceptChanges (); // accept the action
                                Assert.AreEqual (1, (int) reader [0], "#1 should point to the first row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void DeleteLastAndAcceptChangesTest2 ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                reader.Read (); // third row
                                dt.Rows [2].Delete (); // delete row, where reader points to
                                dt.AcceptChanges (); // accept the action
                                Assert.AreEqual (2, (int) reader [0], "#1 should point to the first row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void ClearTest ()
                {
                        DataTableReader reader = null;
                        try {
                                reader = new DataTableReader (dt);
                                reader.Read (); // first row
                                reader.Read (); // second row
                                dt.Clear ();
                                try {
                                        int i = (int) reader [0];
                                        i++; // supress warning
                                        Assert.Fail("#1 should have thrown RowNotInTableException");
                                } catch (RowNotInTableException) {}

                                // clear and add test
                                reader.Close ();
                                reader = new DataTableReader (dt);
                                reader.Read (); // first row
                                reader.Read (); // second row
                                dt.Clear ();
                                dt.Rows.Add (new object [] {8, "mono 8"});
                                dt.AcceptChanges ();
                                bool success = reader.Read ();
                                Assert.AreEqual (false, success, "#2 is always invalid");

                                // clear when reader is not read yet
                                reader.Close ();
                                reader = new DataTableReader (dt);
                                dt.Clear ();
                                dt.Rows.Add (new object [] {8, "mono 8"});
                                dt.AcceptChanges ();
                                success = reader.Read ();
                                Assert.AreEqual (true, success, "#3 should add");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                        
                }

                [Test]
                public void MultipleDeleteTest ()
                {
                        dt.Rows.Add (new object [] {4, "mono 4"});
                        dt.Rows.Add (new object [] {5, "mono 5"});
                        dt.Rows.Add (new object [] {6, "mono 6"});
                        dt.Rows.Add (new object [] {7, "mono 7"});
                        dt.Rows.Add (new object [] {8, "mono 8"});
                        dt.AcceptChanges ();
                        
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read ();
                                reader.Read ();
                                reader.Read ();
                                reader.Read ();

                                dt.Rows [3].Delete ();
                                dt.Rows [1].Delete ();
                                dt.Rows [2].Delete ();
                                dt.Rows [0].Delete ();
                                dt.Rows [6].Delete ();
                                dt.AcceptChanges ();

                                Assert.AreEqual (5, (int) reader [0], "#1 should keep pointing to 5");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }
                #endregion // Negative Tests

        }
}

#endif // NET_2_0
