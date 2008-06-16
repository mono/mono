// DataRowCollectionTest.cs - NUnit Test Cases for System.DataRowCollection
//
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2003 Martin Willemoes Hansen
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


using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataRowCollectionTest
	{
		private DataTable _tbl;	

		[SetUp]
		public void GetReady()
		{
			_tbl = new DataTable();
		}

                [Test]
                public void AutoIncrement()
                {
                        DataColumn col = new DataColumn("Auto");
                        col.AutoIncrement = true;
                        col.AutoIncrementSeed = 0;
                        col.AutoIncrementStep = 1;
                        
                        _tbl.Columns.Add(col);
                        _tbl.Rows.Add(_tbl.NewRow());

                        Assert.AreEqual (0, Convert.ToInt32(_tbl.Rows[0]["Auto"] ), "test#01" );
                                
                        _tbl.Rows.Add(_tbl.NewRow());
                       	Assert.AreEqual (1, Convert.ToInt32(_tbl.Rows[1]["Auto"] ), "test#02" );
                	
                	col.AutoIncrement = false;
                	Assert.AreEqual (1, Convert.ToInt32(_tbl.Rows[1]["Auto"] ), "test#03" );

                        _tbl.Rows.Add(_tbl.NewRow());
                       	Assert.AreEqual (DBNull.Value, _tbl.Rows[2]["Auto"], "test#04" );

                	col.AutoIncrement = true;
			col.AutoIncrementSeed = 10;
                	col.AutoIncrementStep = 2;
                	
                        _tbl.Rows.Add(_tbl.NewRow());
                       	Assert.AreEqual (10, Convert.ToInt32(_tbl.Rows[3]["Auto"] ), "test#05" );
                        _tbl.Rows.Add(_tbl.NewRow());
                       	Assert.AreEqual (12, Convert.ToInt32(_tbl.Rows[4]["Auto"] ), "test#06" );

			col = new DataColumn ("Auto2");
                	col.DataType = typeof(string);
                	col.AutoIncrement = true;
                	col.AutoIncrementSeed = 0;
                	col.AutoIncrementStep = 1;
                	_tbl.Columns.Add (col);
                	
                	_tbl.Rows.Add(_tbl.NewRow());
                	Assert.AreEqual (typeof (int), _tbl.Columns [1].DataType, "test#07");
                       	Assert.AreEqual (typeof (int), _tbl.Rows[5]["Auto2"].GetType (), "test#08" );

			col = new DataColumn ("Auto3");
                	col.AutoIncrement = true;
                	col.AutoIncrementSeed = 0;
                	col.AutoIncrementStep = 1;
	               	col.DataType = typeof(string);
                	Assert.AreEqual (typeof (string), col.DataType, "test#09");
                	Assert.IsFalse (col.AutoIncrement, "test#10");
                }

		[Test]
		public void Add ()
		{
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			DataRow Row = _tbl.NewRow ();
			DataRowCollection Rows = _tbl.Rows;
			
			Rows.Add (Row);
			Assert.AreEqual (1, Rows.Count, "test#01");
			Assert.IsFalse (Rows.IsReadOnly, "test#02");
			Assert.IsFalse (Rows.IsSynchronized, "test#03");
#if !TARGET_JVM
			Assert.AreEqual ("System.Data.DataRowCollection", Rows.ToString (), "test#04");
#endif
			
			string [] cols = new string [2];
			cols [0] = "first";
			cols [1] = "second";
			
			Rows.Add (cols);
			cols [0] = "something";
			cols [1] = "else";
			Rows.Add (cols);
			
			Assert.AreEqual (3, Rows.Count, "test#05");
#if !TARGET_JVM
			Assert.AreEqual ("System.Data.DataRow",  Rows [0].ToString (), "test#06");
#endif
			Assert.AreEqual (DBNull.Value, Rows [0] [0], "test#07");
			Assert.AreEqual (DBNull.Value, Rows [0] [1], "test#08");
			Assert.AreEqual ("first", Rows [1] [0], "test#09");
			Assert.AreEqual ("something", Rows [2] [0], "test#10");
			Assert.AreEqual ("second", Rows [1] [1], "test#11");
			Assert.AreEqual ("else", Rows [2] [1], "test#12");
			
			try {
				Rows.Add (Row);
				Assert.Fail ("test#13");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#14");
				// Never premise English.
				//Assert.AreEqual ("This row already belongs to this table.", e.Message, "test#15");
			}
			
			try {
				Row = null;
				Rows.Add (Row);
				Assert.Fail ("test#16");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "test#17");
				//Assert.AreEqual ("'row' argument cannot be null.\r\nParameter name: row", e.Message, "test#18");
			}
			
			DataColumn Column = new DataColumn ("not_null");
			Column.AllowDBNull = false;
			_tbl.Columns.Add (Column);
			
			cols = new string [3];
			cols [0] = "first";
			cols [1] = "second";
			cols [2] = null;
			
			try {
				Rows.Add (cols);
				Assert.Fail ("test#19");
			} catch (Exception e) {
				Assert.AreEqual (typeof (NoNullAllowedException), e.GetType (), "test#20");
				//Assert.AreEqual ("Column 'not_null' does not allow nulls.", e.Message, "test#21");
			}
			
			Column = _tbl.Columns [0];			
			Column.Unique = true;

			cols = new string [3];
			cols [0] = "first";
			cols [1] = "second";
			cols [2] = "blabal";
			
			try {
				Rows.Add (cols);
				Assert.Fail ("test#22");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ConstraintException), e.GetType (), "test#23");
				// Never premise English.
				//Assert.AreEqual ("Column 'Column1' is constrained to be unique.  Value 'first' is already present.", e.Message, "test#24");
			}
		       
			Column = new DataColumn ("integer");
			Column.DataType = typeof (short);
			_tbl.Columns.Add (Column);
			
			object [] obs = new object [4];
			obs [0] = "_first";
			obs [1] = "second";
			obs [2] = "blabal";
			obs [3] = "ads";
			
			try {
				Rows.Add (obs);
				Assert.Fail ("test#25");
			} catch (ArgumentException e) {
				// LAMESPEC: MSDN says this exception is InvalidCastException
//				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#26");
			}

			object [] obs1 = new object [5];
			obs1 [0] = "A";
			obs1 [1] = "B";
			obs1 [2] = "C";
			obs1 [3] = 38;
			obs1 [4] = "Extra";
			try {
				Rows.Add (obs1);
				Assert.Fail ("test#27");
			} catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType (), "test#28");
			}
		}

                [Test]
                public void Add_ByValuesNullTest ()
                {
                        DataTable t = new DataTable ("test");
                        t.Columns.Add ("id", typeof (int));
                        t.Columns.Add ("name", typeof (string));
                        t.Columns.Add ("nullable", typeof (string));

                        t.Columns [0].AutoIncrement = true;
                        t.Columns [0].AutoIncrementSeed = 10;
                        t.Columns [0].AutoIncrementStep = 5;

                        t.Columns [1].DefaultValue = "testme";
                        

                        // null test & missing columns
                        DataRow r = t.Rows.Add (new object [] { null, null});
                        Assert.AreEqual (10, (int) r [0], "#ABV1");
                        Assert.AreEqual ("testme", (string) r [1], "#ABV2");
                        Assert.AreEqual (DBNull.Value, r [2], "#ABV3");

                        // dbNull test
                        r = t.Rows.Add (new object [] { DBNull.Value, DBNull.Value, DBNull.Value});
                        Assert.AreEqual (DBNull.Value, r [0], "#ABV4");
                        Assert.AreEqual (DBNull.Value, r [1], "#ABV5");
                        Assert.AreEqual (DBNull.Value, r [2], "#ABV6");

                        // ai test & no default value test
                        r = t.Rows.Add (new object [] { null, null, null});
                        Assert.AreEqual (15, (int) r [0], "#ABV7");
                        Assert.AreEqual ("testme", (string) r [1], "#ABV8");
                        Assert.AreEqual (DBNull.Value, r [2], "#ABV9");
                }
		
		[Test]
		public void Clear ()
		{
			DataRowCollection Rows = _tbl.Rows;
			DataTable Table = new DataTable ("child");
			Table.Columns.Add ("first", typeof (int));
			Table.Columns.Add ("second", typeof (string));
			
			_tbl.Columns.Add ("first", typeof (int));
			_tbl.Columns.Add ("second", typeof (float));

			string [] cols = new string [2];
			cols [0] = "1";
			cols [1] = "1,1";
			Rows.Add (cols);
			
			cols [0] = "2";
			cols [1] = "2,1";
			Rows.Add (cols);
			
			cols [0] = "3";
			cols [1] = "3,1";
			Rows.Add (cols);
			
			Assert.AreEqual (3, Rows.Count, "test#01");
			Rows.Clear ();
			
			// hmm... TODO: better tests
			Assert.AreEqual (0, Rows.Count, "test#02");
			
			cols [0] = "1";
			cols [1] = "1,1";
			Rows.Add (cols);
			
			cols [0] = "2";
			cols [1] = "2,1";
			Rows.Add (cols);
			
			cols [0] = "3";
			cols [1] = "3,1";
			Rows.Add (cols);

			cols [0] = "1";
			cols [1] = "test";
			Table.Rows.Add (cols);
			
			cols [0] = "2";
			cols [1] = "test2";
			Table.Rows.Add (cols);
			
			cols [0] = "3";
			cols [1] = "test3";
			Table.Rows.Add (cols);			
			
			DataRelation Rel = new DataRelation ("REL", _tbl.Columns [0], Table.Columns [0]);
			DataSet Set = new DataSet ();
			Set.Tables.Add (_tbl);
			Set.Tables.Add (Table);
			Set.Relations.Add (Rel);
			
			try {
				Rows.Clear ();
				Assert.Fail ("test#03");
			} catch (InvalidConstraintException) {
			}
			
			Assert.AreEqual (3, Table.Rows.Count, "test#06");
			Table.Rows.Clear ();
			Assert.AreEqual (0, Table.Rows.Count, "test#07");
		}
		
		[Test]
		public void Contains ()
		{
			DataColumn C = new DataColumn ("key");
			C.Unique = true;			
			C.DataType = typeof (int);
			C.AutoIncrement = true;
			C.AutoIncrementSeed = 0;
			C.AutoIncrementStep = 1;
			_tbl.Columns.Add (C);
			_tbl.Columns.Add ("first", typeof (string));
			_tbl.Columns.Add ("second", typeof (decimal));
			
			DataRowCollection Rows = _tbl.Rows;
			
			DataRow Row = _tbl.NewRow ();
			_tbl.Rows.Add (Row);
			Row = _tbl.NewRow ();
			_tbl.Rows.Add (Row);
			Row = _tbl.NewRow ();
			_tbl.Rows.Add (Row);
			Row = _tbl.NewRow ();
			_tbl.Rows.Add (Row);
			
			Rows [0] [1] = "test0";
			Rows [0] [2] = 0;
			Rows [1] [1] = "test1";
			Rows [1] [2] = 1;
			Rows [2] [1] = "test2";
			Rows [2] [2] = 2;
			Rows [3] [1] = "test3";
			Rows [3] [2] = 3;
			
			Assert.AreEqual (3, _tbl.Columns.Count, "test#01");
			Assert.AreEqual (4, _tbl.Rows.Count, "test#02");
			Assert.AreEqual (0, _tbl.Rows [0] [0], "test#03");
			Assert.AreEqual (1, _tbl.Rows [1] [0], "test#04");
			Assert.AreEqual (2, _tbl.Rows [2] [0], "test#05");
			Assert.AreEqual (3, _tbl.Rows [3] [0], "test#06");
			
			try {
				Rows.Contains (1);
				Assert.Fail ("test#07");
			} catch (Exception e) {
				Assert.AreEqual (typeof (MissingPrimaryKeyException), e.GetType (), "test#08");
				// Never premise English.
				//Assert.AreEqual ("Table doesn't have a primary key.", e.Message, "test#09");			
			}
			
			_tbl.PrimaryKey = new DataColumn [] {_tbl.Columns [0]};
			Assert.IsTrue (Rows.Contains (1), "test#10");
			Assert.IsTrue (Rows.Contains (2), "test#11");
			Assert.IsFalse (Rows.Contains (4), "test#12");
			
			try {
				Rows.Contains (new object [] {64, "test0"});
				Assert.Fail ("test#13");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#14");
				// Never premise English.
				//Assert.AreEqual ("Expecting 1 value(s) for the key being indexed, but received 2 value(s).", e.Message, "test#15");
			}
			
			_tbl.PrimaryKey = new DataColumn [] {_tbl.Columns [0], _tbl.Columns [1]};
			Assert.IsFalse (Rows.Contains (new object [] {64, "test0"}), "test#16");
			Assert.IsFalse (Rows.Contains (new object [] {0, "test1"}), "test#17");
			Assert.IsTrue (Rows.Contains (new object [] {1, "test1"}), "test#18");
			Assert.IsTrue (Rows.Contains (new object [] {2, "test2"}), "test#19");
			
			try {
				Rows.Contains (new object [] {2});
				Assert.Fail ("test#20");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#21");
				// Never premise English.
				//Assert.AreEqual ("Expecting 2 value(s) for the key being indexed, but received 1 value(s).", e.Message, "test#22");
			}
		}
		
		[Test]
		public void CopyTo ()
		{
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			
			DataRowCollection Rows = _tbl.Rows;
			
			Rows.Add (new object [] {"1", "1", "1"});
			Rows.Add (new object [] {"2", "2", "2"});
			Rows.Add (new object [] {"3", "3", "3"});
			Rows.Add (new object [] {"4", "4", "4"});
			Rows.Add (new object [] {"5", "5", "5"});
			Rows.Add (new object [] {"6", "6", "6"});
			Rows.Add (new object [] {"7", "7", "7"});
			
			DataRow [] dr = new DataRow [10];
			
			try {
				Rows.CopyTo (dr, 4);
				Assert.Fail ("test#01");
			} catch (Exception e) {			
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#02");
				//Assert.AreEqual ("Destination array was not long enough.  Check destIndex and length, and the array's lower bounds.", e.Message, "test#03");
			}
			
			dr = new DataRow [11];
			Rows.CopyTo (dr, 4);
			
			Assert.IsNull (dr [0], "test#04");
			Assert.IsNull (dr [1], "test#05");
			Assert.IsNull (dr [2], "test#06");
			Assert.IsNull (dr [3], "test#07");
			Assert.AreEqual ("1", dr [4] [0], "test#08");
			Assert.AreEqual ("2", dr [5] [0], "test#09");
			Assert.AreEqual ("3", dr [6] [0], "test#10");
			Assert.AreEqual ("4", dr [7] [0], "test#11");
			Assert.AreEqual ("5", dr [8] [0], "test#12");
			Assert.AreEqual ("6", dr [9] [0], "test#13");
		}
		
		[Test]
		public void Equals ()
		{
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			
			DataRowCollection Rows1 = _tbl.Rows;
			
			Rows1.Add (new object [] {"1", "1", "1"});
			Rows1.Add (new object [] {"2", "2", "2"});
			Rows1.Add (new object [] {"3", "3", "3"});
			Rows1.Add (new object [] {"4", "4", "4"});
			Rows1.Add (new object [] {"5", "5", "5"});
			Rows1.Add (new object [] {"6", "6", "6"});
			Rows1.Add (new object [] {"7", "7", "7"});
			
			DataRowCollection Rows2 = _tbl.Rows;
			
			Assert.IsTrue (Rows2.Equals (Rows1), "test#01");
			Assert.IsTrue (Rows1.Equals (Rows2), "test#02");
			Assert.IsTrue (Rows1.Equals (Rows1), "test#03");
			
			DataTable Table = new DataTable ();
			Table.Columns.Add ();
			Table.Columns.Add ();
			Table.Columns.Add ();
			DataRowCollection Rows3 = Table.Rows;

			Rows3.Add (new object [] {"1", "1", "1"});
			Rows3.Add (new object [] {"2", "2", "2"});
			Rows3.Add (new object [] {"3", "3", "3"});
			Rows3.Add (new object [] {"4", "4", "4"});
			Rows3.Add (new object [] {"5", "5", "5"});
			Rows3.Add (new object [] {"6", "6", "6"});
			Rows3.Add (new object [] {"7", "7", "7"});
			
			Assert.IsFalse (Rows3.Equals (Rows1), "test#04");
			Assert.IsFalse (Rows3.Equals (Rows2), "test#05");
			Assert.IsFalse (Rows1.Equals (Rows3), "test#06");
			Assert.IsFalse (Rows2.Equals (Rows3), "test#07");
		}
		
		[Test]
		public void Find ()
		{
			DataColumn Col = new DataColumn ("test_1");
			Col.AllowDBNull = false;
			Col.Unique = true;
			Col.DataType = typeof (long);
			_tbl.Columns.Add (Col);
			
			Col = new DataColumn ("test_2");
			Col.DataType = typeof (string);
			_tbl.Columns.Add (Col);
			
			DataRowCollection Rows = _tbl.Rows;
			
			Rows.Add (new object [] {1, "first"});
			Rows.Add (new object [] {2, "second"});
			Rows.Add (new object [] {3, "third"});
			Rows.Add (new object [] {4, "fourth"});
			Rows.Add (new object [] {5, "fifth"});
			
			try {
				Rows.Find (1);
				Assert.Fail ("test#01");
			} catch (Exception e) {
				Assert.AreEqual (typeof (MissingPrimaryKeyException), e.GetType (), "test#02");
				// Never premise English.
				//Assert.AreEqual ("Table doesn't have a primary key.", e.Message, "test#03");              
			}
			
			_tbl.PrimaryKey = new DataColumn [] {_tbl.Columns [0]};
			DataRow row = Rows.Find (1);
			Assert.AreEqual (1L, row [0], "test#04");
			row = Rows.Find (2);			
			Assert.AreEqual (2L, row [0], "test#05");
			row = Rows.Find ("2");
			Assert.AreEqual (2L, row [0], "test#06");
			
			try {
				row = Rows.Find ("test");
				Assert.Fail ("test#07");
			} catch (Exception e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "test#08");
				//Assert.AreEqual ("Input string was not in a correct format.", e.Message, "test#09");
			}
			
			String tes = null;			
			row = Rows.Find (tes);			
			Assert.IsNull (row, "test#10");
			_tbl.PrimaryKey = null;
			
			try {
				Rows.Find (new object [] {1, "fir"});
				Assert.Fail ("test#11");
			} catch (Exception e) {
				Assert.AreEqual (typeof (MissingPrimaryKeyException), e.GetType (), "test#12");
				// Never premise English.
				//Assert.AreEqual ("Table doesn't have a primary key.", e.Message, "tets#13");
			}
			
			_tbl.PrimaryKey = new DataColumn [] {_tbl.Columns [0], _tbl.Columns [1]};
			
			try {
				Rows.Find (1);
				Assert.Fail ("test#14");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#15");
				// Never premise English.
				//Assert.AreEqual ("Expecting 2 value(s) for the key being indexed, but received 1 value(s).", e.Message, "test#16");
			}
			
			row = Rows.Find (new object [] {1, "fir"});
			Assert.IsNull (row, "test#16");
			row = Rows.Find (new object [] {1, "first"});
			Assert.AreEqual (1L, row [0], "test#17");
		}
		
		[Test]
		public void Find2 ()
		{
			DataSet ds = new DataSet ();
			ds.EnforceConstraints = false;

			DataTable dt = new DataTable ();
			ds.Tables.Add (dt);

			DataColumn dc = new DataColumn ("Column A");
			dt.Columns.Add (dc);

			dt.PrimaryKey = new DataColumn [] {dc};

			DataRow dr = dt.NewRow ();
			dr [0] = "a";
			dt.Rows.Add (dr);

			dr = dt.NewRow ();
			dr [0] = "b";
			dt.Rows.Add (dr);

			dr = dt.NewRow ();
			dr [0] = "c";
			dt.Rows.Add (dr);

			DataRow row = (DataRow) ds.Tables [0].Rows.Find (new object [] {"a"});
			
			Assert.IsNotNull (row);
		}
		
		[Test]
		public void InsertAt ()
		{
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			DataRowCollection Rows = _tbl.Rows;
			
			Rows.Add (new object [] {"a", "aa", "aaa"});
			Rows.Add (new object [] {"b", "bb", "bbb"});
			Rows.Add (new object [] {"c", "cc", "ccc"});
			Rows.Add (new object [] {"d", "dd", "ddd"});
			
			DataRow Row = _tbl.NewRow ();
			Row [0] = "e";
			Row [1] = "ee";
			Row [2] = "eee";
			
			try {
				Rows.InsertAt (Row, -1);
				Assert.Fail ("test#01");
			} catch (Exception e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "test#02");
				// Never premise English.
				//Assert.AreEqual ("The row insert position -1 is invalid.", e.Message, "test#03");
			}
			
			Rows.InsertAt (Row, 0);
			Assert.AreEqual ("e", Rows [0][0], "test#04");
			Assert.AreEqual ("a", Rows [1][0], "test#05");
			
			Row = _tbl.NewRow ();
			Row [0] = "f";
			Row [1] = "ff";
			Row [2] = "fff";
			
			Rows.InsertAt (Row, 5);
			Assert.AreEqual ("f", Rows [5][0], "test#06");
			
			Row = _tbl.NewRow ();
			Row [0] = "g";
			Row [1] = "gg";
			Row [2] = "ggg";

			Rows.InsertAt (Row, 500);
			Assert.AreEqual ("g", Rows [6][0], "test#07");

			try {
                                Rows.InsertAt (Row, 6);	//Row already belongs to the table
                                Assert.Fail ("test#08");
                        }
                        catch (Exception e) {
                                Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#09");
				// Never premise English.
                                //Assert.AreEqual ("This row already belongs to this table.", e.Message, "test#10");
                        }

			DataTable table = new DataTable ();
			DataColumn col = new DataColumn ("Name");
			table.Columns.Add (col);
			Row = table.NewRow ();
			Row ["Name"] = "Abc";
			table.Rows.Add (Row);
			try {
				Rows.InsertAt (Row, 6);
				Assert.Fail ("test#11");
			}
			catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#12");
				// Never premise English.
				//Assert.AreEqual ("This row already belongs to another table.", e.Message, "test#13");
			}

			table = new DataTable ();
			col = new DataColumn ("Name");
			col.DataType = typeof (string);
			table.Columns.Add (col);
			UniqueConstraint uk = new UniqueConstraint (col);
			table.Constraints.Add (uk);
			
			Row = table.NewRow ();
			Row ["Name"] = "aaa";
			table.Rows.InsertAt (Row, 0);
	
			Row = table.NewRow ();
                        Row ["Name"] = "aaa";
			try {
				table.Rows.InsertAt (Row, 1);
				Assert.Fail ("test#14");
			}
			catch (Exception e) {
				Assert.AreEqual (typeof (ConstraintException), e.GetType (), "test#15");
			}
			try {
				table.Rows.InsertAt (null, 1);
			}
			catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "test#16");
			}
		}
		
		[Test]
		public void Remove ()
		{
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			DataRowCollection Rows = _tbl.Rows;
			
			Rows.Add (new object [] {"a", "aa", "aaa"});
			Rows.Add (new object [] {"b", "bb", "bbb"});
			Rows.Add (new object [] {"c", "cc", "ccc"});
			Rows.Add (new object [] {"d", "dd", "ddd"});
			
			Assert.AreEqual (4, _tbl.Rows.Count, "test#01");
			
			Rows.Remove (_tbl.Rows [1]);
			Assert.AreEqual (3, _tbl.Rows.Count, "test#02");
			Assert.AreEqual ("a", _tbl.Rows [0] [0], "test#03");
			Assert.AreEqual ("c", _tbl.Rows [1] [0], "test#04");
			Assert.AreEqual ("d", _tbl.Rows [2] [0], "test#05");
			
			try {
				Rows.Remove (null);
				Assert.Fail ("test#06");
			} catch (Exception e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "test#07");
				// Never premise English.
				//Assert.AreEqual ("The given datarow is not in the current DataRowCollection.", e.Message, "test#08");
			}
			
			DataRow Row = new DataTable ().NewRow ();
			
			try {
				Rows.Remove (Row);
				Assert.Fail ("test#09");
			} catch (Exception e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "test#10");
				// Never premise English.
				//Assert.AreEqual ("The given datarow is not in the current DataRowCollection.", e.Message, "test#11");
			}
			
			try {
				Rows.RemoveAt (-1);
				Assert.Fail ("test#12");
			} catch (Exception e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "test#13");
				// Never premise English.
				//Assert.AreEqual ("There is no row at position -1.", e.Message, "test#14");
			}
			
			try { 
				Rows.RemoveAt (64);
				Assert.Fail ("test#15");
			} catch (Exception e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "test#16");
				// Never premise English.
				//Assert.AreEqual ("There is no row at position 64.", e.Message, "test#17");
			}
			
			Rows.RemoveAt (0);
			Rows.RemoveAt (1);
			Assert.AreEqual (1, Rows.Count, "test#18");
			Assert.AreEqual ("c", Rows [0] [0], "test#19");
		}

#if NET_2_0
		[Test]
		public void IndexOf () {
			DataSet ds = new DataSet ();

			DataTable dt = new DataTable ();
			ds.Tables.Add (dt);

			DataColumn dc = new DataColumn ("Column A");
			dt.Columns.Add (dc);

			dt.PrimaryKey = new DataColumn[] { dc };

			DataRow dr1 = dt.NewRow ();
			dr1[0] = "a";
			dt.Rows.Add (dr1);

			DataRow dr2 = dt.NewRow ();
			dr2[0] = "b";
			dt.Rows.Add (dr2);

			DataRow dr3 = dt.NewRow ();
			dr3[0] = "c";
			dt.Rows.Add (dr3);

			DataRow dr4 = dt.NewRow ();
			dr4[0] = "d";
			dt.Rows.Add (dr4);

			DataRow dr5 = dt.NewRow ();
			dr5[0] = "e";

			int index = ds.Tables[0].Rows.IndexOf (dr3);
			Assert.AreEqual (2, index, "IndexOf-Yes");
			
			index = ds.Tables[0].Rows.IndexOf (dr5);
			Assert.AreEqual (-1, index, "IndexOf-No");
		}
	        [Test]
		public void IndexOfTest()
		{
			DataTable dt = new DataTable("TestWriteXmlSchema");
			dt.Columns.Add("Col1", typeof(int));
			dt.Columns.Add("Col2", typeof(int));
			DataRow dr = dt.NewRow();
			dr[0] = 10;
			dr[1] = 20;
			dt.Rows.Add(dr);
			DataRow dr1 = dt.NewRow();
			dr1[0] = 10;
			dr1[1] = 20;
			dt.Rows.Add(dr1);
			DataRow dr2 = dt.NewRow();
			dr2[0] = 10;
			dr2[1] = 20;
			dt.Rows.Add(dr2);
			Assert.AreEqual (1, dt.Rows.IndexOf (dr1));
			DataTable dt1 = new DataTable("HelloWorld");
			dt1.Columns.Add("T1", typeof(int));
			dt1.Columns.Add("T2", typeof(int));
			DataRow dr3 = dt1.NewRow();
			dr3[0] = 10;
			dr3[1] = 20;
			dt1.Rows.Add(dr3);
			Assert.AreEqual (-1, dt.Rows.IndexOf (dr3));
			Assert.AreEqual (-1, dt.Rows.IndexOf (null));
		}
#endif
	}
}
