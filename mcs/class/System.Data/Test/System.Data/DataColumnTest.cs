// DataColumnTest.cs - NUnit Test Cases for System.Data.DataColumn
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Rodrigo Moya <rodrigo@ximian.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2002 Rodrigo Moya
// (C) Copyright 2003 Daniel Morgan
//

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	public class DataColumnTest : TestCase
	{
		public DataColumnTest () : base ("System.Data.DataColumn") {}
		public DataColumnTest (string name) : base (name) {}

		private DataTable _tbl;

		protected override void SetUp () 
		{
			_tbl = new DataTable();
		}

		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite (typeof (DataColumnTest)); 
			}
		}

		public void TestCtor()	
		{
			string colName = "ColName";
			DataColumn col = new DataColumn();
			
			//These should all ctor without an exception
			col = new DataColumn(colName);
			col = new DataColumn(colName,typeof(int));
			col = new DataColumn(colName,typeof(int),null);
			col = new DataColumn(colName,typeof(int),null,MappingType.Attribute);

			//DataType Null
			try
			{
				col = new DataColumn(colName, null);
				Assertion.Fail("DC7: Failed to throw ArgumentNullException.");
			}
			catch (ArgumentNullException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("DC8: DataColumnNull. Wrong exception type. Got:" + exc);
			}

		}

		public void TestAllowDBNull()
		{
			DataColumn col = new DataColumn("NullCheck",typeof(int));
			_tbl.Columns.Add(col);
			col.AllowDBNull = true;
			_tbl.Rows.Add(_tbl.NewRow());
			_tbl.Rows[0]["NullCheck"] = DBNull.Value;
			col.AllowDBNull = false;
		}

		public void TestAutoIncrement()
		{
			DataColumn col = new DataColumn("Auto",typeof(string));
			col.AutoIncrement = true;
			
			//Check for Correct Default Values
			Assertion.AssertEquals("DC9: Seed default", (long)0, col.AutoIncrementSeed);
			Assertion.AssertEquals("DC10: Step default", (long)1, col.AutoIncrementStep);

			//Check for auto type convert
			Assertion.Assert("DC11: AutoInc type convert failed." ,col.DataType == typeof (int));
		}

		public void TestAutoIncrementExceptions()
		{
			DataColumn col = new DataColumn();

			col.Expression = "SomeExpression";

			//if computed column exception is thrown
			try 
			{
				col.AutoIncrement = true;
				Assertion.Fail("DC12: Failed to throw ArgumentException");
			}
			catch (ArgumentException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("DC13: ExprAutoInc. Wrong exception type. Got:" + exc);
			}


		}

		public void TestCaption()
		{
			DataColumn col = new DataColumn("ColName");
			//Caption not set at this point
			Assertion.AssertEquals("DC14: Caption Should Equal Col Name", col.ColumnName, col.Caption);

			//Set caption
			col.Caption = "MyCaption";
			Assertion.AssertEquals("DC15: Caption should equal caption.", "MyCaption", col.Caption);

			//Clear caption
			col.Caption = null;
			Assertion.AssertEquals("DC16: Caption Should Equal Col Name after clear", col.ColumnName, col.Caption);
			
		}

		public void TestForColumnNameException()
		{
			DataColumn col = new DataColumn();
			DataColumn col2 = new DataColumn();
			DataColumn col3 = new DataColumn();
			DataColumn col4 = new DataColumn();
			
			col.ColumnName = "abc";
			AssertEquals( "abc", col.ColumnName);

			_tbl.Columns.Add(col);
			
			//Duplicate name exception
			try
			{
				col2.ColumnName = "abc";
				_tbl.Columns.Add(col2);
				AssertEquals( "abc", col2.ColumnName);
				Assertion.Fail("DC17: Failed to throw duplicate name exception.");
			}
			catch (DuplicateNameException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("DC18: Wrong exception type. " + exc.ToString());
			}

			// Make sure case matters in duplicate checks
			col3.ColumnName = "ABC";
			_tbl.Columns.Add(col3);
		}

		public void TestDefaultValue()
		{
			DataTable tbl = new DataTable();
			tbl.Columns.Add("MyCol", typeof(int));
			
			//Set default Value if Autoincrement is true
			tbl.Columns[0].AutoIncrement = true;
			try
			{
				tbl.Columns[0].DefaultValue = 2;
				Assertion.Fail("DC19: Failed to throw ArgumentException.");
			}
			catch (ArgumentException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("DC20: Wrong exception type. " + exc.ToString());
			}


			tbl.Columns[0].AutoIncrement = false;

			//Set default value to an incompatible datatype
			try
			{
				tbl.Columns[0].DefaultValue = "hello";
				Assertion.Fail("DC21: Failed to throw InvalidCastException.");
			}
			catch (InvalidCastException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("DC22: Wrong exception type. " + exc.ToString());
			}

			//TODO: maybe add tests for setting default value for types that can implict
			//cast




		}

		public void TestSetDataType()
		{
			//test for DataAlready exists and change the datatype
			
			//supported datatype

			//AutoInc column dataType supported

		}

		public void TestDefaults1() 
		{
			//Check for defaults - ColumnName not set at the beginning
			DataTable table = new DataTable();		
			DataColumn column = new DataColumn();
			
			Assertion.AssertEquals("DC1: ColumnName default Before Add", column.ColumnName, String.Empty);
			Assertion.AssertEquals("DC2: DataType default Before Add", column.DataType.ToString(), typeof(string).ToString());
			
			table.Columns.Add(column);
			
			Assertion.AssertEquals("DC3: ColumnName default After Add", table.Columns[0].ColumnName, "Column1");
			Assertion.AssertEquals("DC4: DataType default After Add", table.Columns[0].DataType.ToString(), typeof(string).ToString());	
			
			DataRow row = table.NewRow();
			table.Rows.Add(row);
			DataRow dataRow = table.Rows[0];
			
			object v = null;
			try {
				v = dataRow.ItemArray[0];
			}
			catch(Exception e) {
				Assertion.Fail("DC5: getting item from dataRow.ItemArray[0] threw Exception: " + e);
			}
			
			Type vType = dataRow.ItemArray[0].GetType();
			Assertion.AssertEquals("DC6: Value from DataRow.Item", v, DBNull.Value);
		}

		public void TestDefaults2() 
		{
			//Check for defaults - ColumnName set at the beginning
			string blah = "Blah";
			//Check for defaults - ColumnName not set at the beginning
			DataTable table = new DataTable();		
			DataColumn column = new DataColumn(blah);
			
			Assertion.AssertEquals("DC23: ColumnName default Before Add", column.ColumnName,blah);
			Assertion.AssertEquals("DC24: DataType default Before Add", column.DataType.ToString(), typeof(string).ToString());
			
			table.Columns.Add(column);
			
			Assertion.AssertEquals("DC25: ColumnName default After Add", table.Columns[0].ColumnName, blah);
			Assertion.AssertEquals("DC26: DataType default After Add", table.Columns[0].DataType.ToString(), typeof(string).ToString());	
			
			DataRow row = table.NewRow();
			table.Rows.Add(row);
			DataRow dataRow = table.Rows[0];

			object v = null;
			try {
				v = dataRow.ItemArray[0];
			}
			catch(Exception e) {
				Assertion.Fail("DC27: getting item from dataRow.ItemArray[0] threw Exception: " + e);
			}
			
			Type vType = dataRow.ItemArray[0].GetType();
			Assertion.AssertEquals("DC28: Value from DataRow.Item", v, DBNull.Value);
		}
	}
}
