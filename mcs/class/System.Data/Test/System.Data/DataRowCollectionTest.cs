// DataRowCollectionTest.cs - NUnit Test Cases for System.DataRowCollection
//
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2003 Martin Willemoes Hansen
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

		//FINISHME
		[Test]
		public void AutoIncrement()
		{
			DataColumn col = new DataColumn();
			col.AutoIncrement = true;
			col.AutoIncrementSeed = 0;
			col.AutoIncrementStep = 1;
			
			_tbl.Columns.Add(col);
			_tbl.Rows.Add(_tbl.NewRow());

			//Assertion.Assertion.Assertion.AssertEquals("Inc 0" , 0, Convert.ToInt32(_tbl.Rows[0]["Auto"] ));
				
			_tbl.Rows.Add(_tbl.NewRow());
			//Assertion.Assertion.Assertion.AssertEquals("Inc 1" , 1, Convert.ToInt32(_tbl.Rows[0]["Auto"] ));
		}

		[Test]
		public void Add ()
		{
			_tbl.Columns.Add ();
			_tbl.Columns.Add ();
			DataRow Row = _tbl.NewRow ();
			DataRowCollection Rows = _tbl.Rows;
			
			Rows.Add (Row);
			Assertion.AssertEquals ("test#01", 1, Rows.Count);
			Assertion.AssertEquals ("test#02", false, Rows.IsReadOnly);
			Assertion.AssertEquals ("test#03", false, Rows.IsSynchronized);
			Assertion.AssertEquals ("test#04", "System.Data.DataRowCollection", Rows.ToString ());
			
			string [] cols = new string [2];
			cols [0] = "first";
			cols [1] = "second";
			
			Rows.Add (cols);
			cols [0] = "something";
			cols [1] = "else";
			Rows.Add (cols);
			
			Assertion.AssertEquals ("test#05", 3, Rows.Count);
			Assertion.AssertEquals ("test#06", "System.Data.DataRow",  Rows [0].ToString ());
			Assertion.AssertEquals ("test#07", DBNull.Value, Rows [0] [0]);
			Assertion.AssertEquals ("test#08", DBNull.Value, Rows [0] [1]);
			Assertion.AssertEquals ("test#09", "first", Rows [1] [0]);
			Assertion.AssertEquals ("test#10", "something", Rows [2] [0]);
			Assertion.AssertEquals ("test#11", "second", Rows [1] [1]);
			Assertion.AssertEquals ("test#12", "else", Rows [2] [1]);
			
			try {
				Rows.Add (Row);
				Assertion.Fail ("test#13");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#14", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#15", "This row already belongs to this table.", e.Message);
			}
			
			try {
				Row = null;
				Rows.Add (Row);
				Assertion.Fail ("test#16");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#17", typeof (ArgumentNullException), e.GetType ());
				Assertion.AssertEquals ("test#18", "'row' argument cannot be null.\r\nParameter name: row", e.Message);
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
				Assertion.Fail ("test#19");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#20", typeof (NoNullAllowedException), e.GetType ());
				Assertion.AssertEquals ("test#21", "Column 'not_null' does not allow nulls.", e.Message);
			}
			
			Column = _tbl.Columns [0];
			Column.Unique = true;
			
			cols = new string [3];
			cols [0] = "first";
			cols [1] = "second";
			cols [2] = "blabal";
			
			try {
				Rows.Add (cols);
				Assertion.Fail ("test#22");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#23", typeof (ConstraintException), e.GetType ());
				Assertion.AssertEquals ("test#24", "Column 'Column1' is constrained to be unique.  Value 'first' is already present.", e.Message);
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
				Assertion.Fail ("test#25");
			} catch (Exception e) {
				// LAMESPEC: MSDN says this exception is InvalidCastException
				Assertion.AssertEquals ("test#26", typeof (ArgumentException), e.GetType ());
			}
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
			
			Assertion.AssertEquals ("test#01", 3, Rows.Count);
			Rows.Clear ();
			
			// hmm... TODO: better tests
			Assertion.AssertEquals ("test#02", 0, Rows.Count);
			
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
				Assertion.Fail ("test#03");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#04", typeof (InvalidConstraintException), e.GetType ());
				Assertion.AssertEquals ("test#05", "Cannot clear table Table1 because ForeignKeyConstraint REL enforces constraints and there are child rows in child.", e.Message);
			}
			
			Assertion.AssertEquals ("test#06", 3, Table.Rows.Count);
			Table.Rows.Clear ();
			Assertion.AssertEquals ("test#07", 0, Table.Rows.Count);
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
			
			Assertion.AssertEquals ("test#01", 3, _tbl.Columns.Count);
			Assertion.AssertEquals ("test#02", 4, _tbl.Rows.Count);
			Assertion.AssertEquals ("test#03", 0, _tbl.Rows [0] [0]);
			Assertion.AssertEquals ("test#04", 1, _tbl.Rows [1] [0]);
			Assertion.AssertEquals ("test#05", 2, _tbl.Rows [2] [0]);
			Assertion.AssertEquals ("test#06", 3, _tbl.Rows [3] [0]);
			
			try {
				Rows.Contains (1);
				Assertion.Fail ("test#07");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#08", typeof (MissingPrimaryKeyException), e.GetType ());
				Assertion.AssertEquals ("test#09", "Table doesn't have a primary key.", e.Message);			
			}
			
			_tbl.PrimaryKey = new DataColumn [] {_tbl.Columns [0]};
			Assertion.AssertEquals ("test#10", true, Rows.Contains (1));
			Assertion.AssertEquals ("test#11", true, Rows.Contains (2));
			Assertion.AssertEquals ("test#12", false, Rows.Contains (4));
			
			try {
				Rows.Contains (new object [] {64, "test0"});
				Assertion.Fail ("test#13");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#14", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#15", "Expecting 1 value(s) for the key being indexed, but received 2 value(s).", e.Message);
			}
			
			_tbl.PrimaryKey = new DataColumn [] {_tbl.Columns [0], _tbl.Columns [1]};
			Assertion.AssertEquals ("test#16", false, Rows.Contains (new object [] {64, "test0"}));
			Assertion.AssertEquals ("test#17", false, Rows.Contains (new object [] {0, "test1"}));
			Assertion.AssertEquals ("test#18", true, Rows.Contains (new object [] {1, "test1"}));
			Assertion.AssertEquals ("test#19", true, Rows.Contains (new object [] {2, "test2"}));
			
			try {
				Rows.Contains (new object [] {2});
				Assertion.Fail ("test#20");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#21", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#22", "Expecting 2 value(s) for the key being indexed, but received 1 value(s).", e.Message);
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
				Assertion.Fail ("test#01");
			} catch (Exception e) {			
				Assertion.AssertEquals ("test#02", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#03", "Destination array was not long enough.  Check destIndex and length, and the array's lower bounds.", e.Message);
			}
			
			dr = new DataRow [11];
			Rows.CopyTo (dr, 4);
			
			Assertion.AssertEquals ("test#04", null, dr [0]);
			Assertion.AssertEquals ("test#05", null, dr [1]);
			Assertion.AssertEquals ("test#06", null, dr [2]);
			Assertion.AssertEquals ("test#07", null, dr [3]);
			Assertion.AssertEquals ("test#08", "1", dr [4] [0]);
			Assertion.AssertEquals ("test#09", "2", dr [5] [0]);
			Assertion.AssertEquals ("test#10", "3", dr [6] [0]);
			Assertion.AssertEquals ("test#11", "4", dr [7] [0]);
			Assertion.AssertEquals ("test#12", "5", dr [8] [0]);
			Assertion.AssertEquals ("test#13", "6", dr [9] [0]);
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
			
			Assertion.AssertEquals ("test#01", true, Rows2.Equals (Rows1));
			Assertion.AssertEquals ("test#02", true, Rows1.Equals (Rows2));
			Assertion.AssertEquals ("test#03", true, Rows1.Equals (Rows1));
			
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
			
			Assertion.AssertEquals ("test#04", false, Rows3.Equals (Rows1));
			Assertion.AssertEquals ("test#05", false, Rows3.Equals (Rows2));
			Assertion.AssertEquals ("test#06", false, Rows1.Equals (Rows3));
			Assertion.AssertEquals ("test#07", false, Rows2.Equals (Rows3));
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
				Assertion.Fail ("test#01");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#02", typeof (MissingPrimaryKeyException), e.GetType ());
				Assertion.AssertEquals ("test#03", "Table doesn't have a primary key.", e.Message);              
			}
			
			_tbl.PrimaryKey = new DataColumn [] {_tbl.Columns [0]};
			
			DataRow row = Rows.Find (1);
			Assertion.AssertEquals ("test#04", 1L, row [0]);
			row = Rows.Find (2);
			Assertion.AssertEquals ("test#05", 2L, row [0]);
			row = Rows.Find ("2");
			Assertion.AssertEquals ("test#06", 2L, row [0]);
			
			try {
				row = Rows.Find ("test");
				Assertion.Fail ("test#07");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#08", typeof (FormatException), e.GetType ());
				Assertion.AssertEquals ("test#09", "Input string was not in a correct format.", e.Message);
			}
			
			String tes = null;
			row = Rows.Find (tes);
			Assertion.AssertEquals ("test#10", null, row);
			
			_tbl.PrimaryKey = null;
			
			try {
				Rows.Find (new object [] {1, "fir"});
				Assertion.Fail ("test#11");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#12", typeof (MissingPrimaryKeyException), e.GetType ());
				Assertion.AssertEquals ("tets#13", "Table doesn't have a primary key.", e.Message);
			}
			
			_tbl.PrimaryKey = new DataColumn [] {_tbl.Columns [0], _tbl.Columns [1]};
			
			try {
				Rows.Find (1);
				Assertion.Fail ("test#11");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#12", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#13", "Expecting 2 value(s) for the key being indexed, but received 1 value(s).", e.Message);
			}
			
			row = Rows.Find (new object [] {1, "fir"});
			Assertion.AssertEquals ("test#14", null, row);
			row = Rows.Find (new object [] {1, "first"});
			Assertion.AssertEquals ("test#15", 1L, row [0]);
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
				Assertion.Fail ("test#01");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#02", typeof (IndexOutOfRangeException), e.GetType ());
				Assertion.AssertEquals ("test#03", "The row insert position -1 is invalid.", e.Message);
			}
			
			Rows.InsertAt (Row, 0);
			Assertion.AssertEquals ("test#04", "e", Rows [0][0]);
			Assertion.AssertEquals ("test#05", "a", Rows [1][0]);
			
			Row = _tbl.NewRow ();
			Row [0] = "f";
			Row [1] = "ff";
			Row [2] = "fff";
			
			Rows.InsertAt (Row, 5);
			Assertion.AssertEquals ("test#06", "f", Rows [5][0]);
		
			Row = _tbl.NewRow ();
			Row [0] = "g";
			Row [1] = "gg";
			Row [2] = "ggg";
			
			Rows.InsertAt (Row, 500);
			Assertion.AssertEquals ("test#07", "g", Rows [6][0]);
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
			
			Assertion.AssertEquals ("test#01", 4, _tbl.Rows.Count);
			
			Rows.Remove (_tbl.Rows [1]);
			Assertion.AssertEquals ("test#02", 3, _tbl.Rows.Count);
			Assertion.AssertEquals ("test#03", "a", _tbl.Rows [0] [0]);
			Assertion.AssertEquals ("test#04", "c", _tbl.Rows [1] [0]);
			Assertion.AssertEquals ("test#05", "d", _tbl.Rows [2] [0]);
			
			try {
				Rows.Remove (null);
				Assertion.Fail ("test#06");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#07", typeof (IndexOutOfRangeException), e.GetType ());
				Assertion.AssertEquals ("test#08", "The given datarow is not in the current DataRowCollection.", e.Message);
			}
			
			DataRow Row = new DataTable ().NewRow ();
			
			try {
				Rows.Remove (Row);
				Assertion.Fail ("test#09");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#10", typeof (IndexOutOfRangeException), e.GetType ());
				Assertion.AssertEquals ("test#11", "The given datarow is not in the current DataRowCollection.", e.Message);
			}
			
			try {
				Rows.RemoveAt (-1);
				Assertion.Fail ("test#12");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#13", typeof (IndexOutOfRangeException), e.GetType ());
				Assertion.AssertEquals ("test#14", "There is no row at position -1.", e.Message);
			}
			
			try { 
				Rows.RemoveAt (64);
				Assertion.Fail ("test#15");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#16", typeof (IndexOutOfRangeException), e.GetType ());
				Assertion.AssertEquals ("test#17", "There is no row at position 64.", e.Message);
			}
			
			Rows.RemoveAt (0);
			Rows.RemoveAt (1);
			Assertion.AssertEquals ("test#18", 1, Rows.Count);
			Assertion.AssertEquals ("test#19", "c", Rows [0] [0]);
		}
	}
}
