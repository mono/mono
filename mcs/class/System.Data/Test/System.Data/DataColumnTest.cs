// DataColumnTest.cs - NUnit Test Cases for System.Data.DataColumn
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Rodrigo Moya <rodrigo@ximian.com>
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2002 Rodrigo Moya
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
				Assertion.Fail("Failed to throw ArgumentNullException.");
			}
			catch (ArgumentNullException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("DataColumnNull. Wrong exception type. Got:" + exc);
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
			Assertion.AssertEquals("Seed default", (long)0, col.AutoIncrementSeed);
			Assertion.AssertEquals("Step default", (long)1, col.AutoIncrementStep);

			//Check for auto type convert
			Assertion.Assert("AutoInc type convert failed." ,col.DataType == typeof (int));

		}

		public void TestAutoIncrementExceptions()
		{
			DataColumn col = new DataColumn();

			col.Expression = "SomeExpression";

			//if computed column exception is thrown
			try 
			{
				col.AutoIncrement = true;
				Assertion.Fail("Failed to throw ArgumentException");
			}
			catch (ArgumentException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("ExprAutoInc. Wrong exception type. Got:" + exc);
			}


		}

		public void TestCaption()
		{
			DataColumn col = new DataColumn("ColName");
			//Caption not set at this point
			Assertion.AssertEquals("Caption Should Equal Col Name", col.ColumnName, col.Caption);

			//Set caption
			col.Caption = "MyCaption";
			Assertion.AssertEquals("Caption should equal caption.", "MyCaption", col.Caption);

			//Clear caption
			col.Caption = null;
			Assertion.AssertEquals("Caption Should Equal Col Name after clear", col.ColumnName, col.Caption);
			
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
				Assertion.Fail("Failed to throw duplicate name exception.");
			}
			catch (DuplicateNameException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("DNE: Wrong exception type. " + exc.ToString());
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
				Assertion.Fail("Failed to throw ArgumentException.");
			}
			catch (ArgumentException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("WET1: Wrong exception type. " + exc.ToString());
			}


			tbl.Columns[0].AutoIncrement = false;

			//Set default value to an incompatible datatype
			try
			{
				tbl.Columns[0].DefaultValue = "hello";
				Assertion.Fail("Failed to throw InvalidCastException.");
			}
			catch (InvalidCastException){}
			catch (AssertionFailedError exc) {throw  exc;}
			catch (Exception exc)
			{
				Assertion.Fail("WET2: Wrong exception type. " + exc.ToString());
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
	}
}
