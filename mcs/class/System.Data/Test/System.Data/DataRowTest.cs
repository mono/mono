// DataRowTest.cs - NUnit Test Cases for System.DataRow
//
// Franklin Wise (gracenote@earthlink.net)
// Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2003 Daniel Morgan
// 


using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{

	public class DataRowTest : TestCase {
	
		public DataRowTest() : base ("MonoTests.System.Data.DataRowTest") {}
		public DataRowTest(string name) : base(name) {}

		private DataTable _tbl;	

		protected override void SetUp() {
			_tbl = new DataTable();
		}

		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite(typeof(DataRowTest)); 
			}
		}

		// tests item at row, column in table to be DBNull.Value
		private void TestDBNull(string message, DataTable dt, int row, int column) 
		{
			object val = dt.Rows[row].ItemArray[column];
			Assertion.AssertEquals(message, DBNull.Value, val);
		}

		// tests item at row, column in table to be null
		private void TestNull(string message, DataTable dt, int row, int column) 
		{
			object val = dt.Rows[row].ItemArray[column];
			Assertion.AssertEquals(message, null, val);
		}

		// tests item at row, column in table to be 
		private void TestValue(string message, DataTable dt, int row, int column, object value) 
		{
			object val = dt.Rows[row].ItemArray[column];
			Assertion.AssertEquals(message, value, val);
		}

		// test set null, DBNull.Value, and ItemArray short count
		public void TestNullInItemArray () 
		{
			string zero = "zero";
			string one = "one";
			string two = "two";

			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn(zero, typeof(string)));
			table.Columns.Add(new DataColumn(one, typeof(string)));
			table.Columns.Add(new DataColumn(two, typeof(string)));

			object[] obj = new object[3];
			// -- normal -----------------
			obj[0] = zero;
			obj[1] = one;
			obj[2] = two;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = "one"
			//   table.Rows[0].ItemArray.ItemArray[2] = "two"
			
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Assertion.Fail("DR1: Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null ----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = DBNull.Value
			//   table.Rows[1].ItemArray.ItemArray[2] = "two"

			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Assertion.Fail("DR2: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//   table.Rows[2].ItemArray.ItemArray[2] = "two"

			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Assertion.Fail("DR3: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			obj[1] = def;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = "def"
			//   table.Rows[3].ItemArray.ItemArray[2] = DBNull.Value;
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Assertion.Fail("DR4: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			TestValue("DR5: normal value test", table, 0, 0, zero);
			TestValue("DR6: normal value test", table, 0, 1, one);
			TestValue("DR7: normal value test", table, 0, 2, two);

			// -- null ----------
			TestValue("DR8: null value test", table, 1, 0, zero);
			TestValue("DR9: null value test", table, 1, 1, DBNull.Value);
			TestValue("DR10: null value test", table, 1, 2, two);

			// -- DBNull.Value -------------
			TestValue("DR11: DBNull.Value value test", table, 2, 0, zero);
			TestValue("DR12: DBNull.Value value test", table, 2, 1, DBNull.Value);
			TestValue("DR13: DBNull.Value value test", table, 2, 2, two);

			// -- object array smaller than number of columns -----
			TestValue("DR14: array smaller value test", table, 3, 0, abc);
			TestValue("DR15: array smaller value test", table, 3, 1, def);
			TestValue("DR16: array smaller value test", table, 3, 2, DBNull.Value);
		}
	
		// test DefaultValue when setting ItemArray
		public void TestDefaultValueInItemArray () {		
			string zero = "zero";

			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn("zero", typeof(string)));		
			
			DataColumn column = new DataColumn("num", typeof(int));
			column.DefaultValue = 15;
			table.Columns.Add(column);
			
			object[] obj = new object[2];
			// -- normal -----------------
			obj[0] = "zero";
			obj[1] = 8;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = 8
						
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Assertion.Fail("DR17: Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null ----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 15
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Assertion.Fail("DR18: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//      even though internally, the v
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Assertion.Fail("DR19: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = DBNull.Value
						
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Assertion.Fail("DR20: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			TestValue("DR20: normal value test", table, 0, 0, zero);
			TestValue("DR21: normal value test", table, 0, 1, 8);
			
			// -- null ----------
			TestValue("DR22: null value test", table, 1, 0, zero);
			TestValue("DR23: null value test", table, 1, 1, 15);
			
			// -- DBNull.Value -------------
			TestValue("DR24: DBNull.Value value test", table, 2, 0, zero);
			TestDBNull("DR25: DBNull.Value value test", table, 2, 1);
			
			// -- object array smaller than number of columns -----
			TestValue("DR26: array smaller value test", table, 3, 0, abc);
			TestValue("DR27: array smaller value test", table, 3, 1, 15);
		}

		// test AutoIncrement when setting ItemArray
		public void TestAutoIncrementInItemArray () {
			string zero = "zero";
			string num = "num";
			
			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn(zero, typeof(string)));		
			
			DataColumn column = new DataColumn("num", typeof(int));
			column.AutoIncrement = true;
			table.Columns.Add(column);
			
			object[] obj = new object[2];
			// -- normal -----------------
			obj[0] = "zero";
			obj[1] = 8;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = 8
						
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Assertion.Fail("DR28:  Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null 1----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 9
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Assertion.Fail("DR29:  Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- null 2----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 10
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Assertion.Fail("DR30: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- null 3----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 11
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Assertion.Fail("DR31: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//      even though internally, the AutoIncrement value
			//      is incremented
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Assertion.Fail("DR32: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- null 4----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 13
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Assertion.Fail("DR48: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = 14
						
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Assertion.Fail("DR33: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			TestValue("DR34: normal value test", table, 0, 0, zero);
			TestValue("DR35: normal value test", table, 0, 1, 8);
			
			// -- null 1----------
			TestValue("DR36: null value test", table, 1, 0, zero);
			TestValue("DR37: null value test", table, 1, 1, 9);

			// -- null 2----------
			TestValue("DR38: null value test", table, 2, 0, zero);
			TestValue("DR39: null value test", table, 2, 1, 10);

			// -- null 3----------
			TestValue("DR40: null value test", table, 3, 0, zero);
			TestValue("DR41: null value test", table, 3, 1, 11);

			// -- DBNull.Value -------------
			TestValue("DR42: DBNull.Value value test", table, 4, 0, zero);
			TestValue("DR43: DBNull.Value value test", table, 4, 1, DBNull.Value);

			// -- null 4----------
			TestValue("DR44: null value test", table, 5, 0, zero);
			TestValue("DR45: null value test", table, 5, 1, 13);

			// -- object array smaller than number of columns -----
			TestValue("DR46: array smaller value test", table, 6, 0, abc);
			TestValue("DR47: array smaller value test", table, 6, 1, 14);
		}
	}
}
