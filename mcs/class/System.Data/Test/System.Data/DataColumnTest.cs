// DataColumnTest.cs - NUnit Test Cases for System.Data.DataColumn
//
// Authors:
//   Franklin Wise <gracenote@earthlink.net>
//   Rodrigo Moya <rodrigo@ximian.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2002 Rodrigo Moya
// (C) Copyright 2003 Daniel Morgan
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

using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;

using NUnit.Framework;

namespace MonoTests.System.Data 
{
	[TestFixture]
	public class DataColumnTest
	{
		private DataTable _tbl;

		[SetUp]
		public void GetReady () 
		{
			_tbl = new DataTable();
		}

		[Test]
		public void Ctor()
		{
			string colName = "ColName";
			DataColumn col = new DataColumn ();
			
			//These should all ctor without an exception
			col = new DataColumn (colName);
			col = new DataColumn (colName, typeof(int));
			col = new DataColumn (colName, typeof(int), null);
			col = new DataColumn (colName, typeof(int), null, MappingType.Attribute);
		}

		[Test]
		public void Constructor3_DataType_Null ()
		{
			try {
				new DataColumn ("ColName", (Type) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				// Never premise English.
//				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("dataType", ex.ParamName, "#6");
			}
		}

		[Test]
		public void AllowDBNull()
		{
			DataColumn col = new DataColumn("NullCheck",typeof(int));
			_tbl.Columns.Add(col);
			col.AllowDBNull = true;
			_tbl.Rows.Add(_tbl.NewRow());
			_tbl.Rows[0]["NullCheck"] = DBNull.Value;
			try {
				col.AllowDBNull = false;
				Assert.Fail ("DC8b: Failed to throw DataException.");
			} catch (DataException) {
			}
		}

		[Test]
		public void AllowDBNull1()
		{
			DataTable tbl = _tbl;
			tbl.Columns.Add ("id", typeof (int));
			tbl.Columns.Add ("name", typeof (string));
			tbl.PrimaryKey = new DataColumn [] { tbl.Columns ["id"] };
			tbl.Rows.Add (new object [] { 1, "RowState 1" });
			tbl.Rows.Add (new object [] { 2, "RowState 2" });
			tbl.Rows.Add (new object [] { 3, "RowState 3" });
			tbl.AcceptChanges ();
			// Update Table with following changes: Row0 unmodified, 
			// Row1 modified, Row2 deleted, Row3 added, Row4 not-present.
			tbl.Rows [1] ["name"] = "Modify 2";
			tbl.Rows [2].Delete ();

			DataColumn col = tbl.Columns ["name"];
			col.AllowDBNull = true;
			col.AllowDBNull = false;

			Assert.IsFalse (col.AllowDBNull);
		}

		[Test]
		public void AutoIncrement()
		{
			DataColumn col = new DataColumn("Auto",typeof (string));
			col.AutoIncrement = true;
			
			//Check for Correct Default Values
			Assert.AreEqual (0L, col.AutoIncrementSeed, "#1");
			Assert.AreEqual (1L, col.AutoIncrementStep, "#2");

			//Check for auto type convert
			Assert.AreEqual (typeof (int), col.DataType, "#3");
		}

		[Test]
		public void AutoIncrementExceptions()
		{
			DataColumn col = new DataColumn();
			col.Expression = "SomeExpression";

			//if computed column exception is thrown
			try {
				col.AutoIncrement = true;
				Assert.Fail ("DC12: Failed to throw ArgumentException");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Caption()
		{
			DataColumn col = new DataColumn("ColName");
			//Caption not set at this point
			Assert.AreEqual (col.ColumnName, col.Caption, "#1");

			//Set caption
			col.Caption = "MyCaption";
			Assert.AreEqual ("MyCaption", col.Caption, "#2");

			//Clear caption
			col.Caption = null;
			Assert.AreEqual (string.Empty, col.Caption, "#3");
		}

#if NET_2_0
		[Test]
		public void DateTimeMode_Valid ()
		{
			DataColumn col = new DataColumn ("birthdate", typeof (DateTime));
			col.DateTimeMode = DataSetDateTime.Local;
			Assert.AreEqual (DataSetDateTime.Local, col.DateTimeMode, "#1");
			col.DateTimeMode = DataSetDateTime.Unspecified;
			Assert.AreEqual (DataSetDateTime.Unspecified, col.DateTimeMode, "#2");
			col.DateTimeMode = DataSetDateTime.Utc;
			Assert.AreEqual (DataSetDateTime.Utc, col.DateTimeMode, "#3");
		}

		[Test]
		public void DateTime_DataType_Invalid ()
		{
			DataColumn col = new DataColumn ("birthdate", typeof (int));
			try {
				col.DateTimeMode = DataSetDateTime.Local;
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The DateTimeMode can be set only on DataColumns
				// of type DateTime
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("DateTimeMode") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("DateTime") != -1, "#6");
			}
		}

		[Test]
		public void DateTimeMode_Invalid ()
		{
			DataColumn col = new DataColumn ("birthdate", typeof (DateTime));
			try {
				col.DateTimeMode = (DataSetDateTime) 666;
				Assert.Fail ("#1");
			} catch (InvalidEnumArgumentException ex) {
				// The DataSetDateTime enumeration value, 666, is invalid
				Assert.AreEqual (typeof (InvalidEnumArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("DataSetDateTime") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#6");
				Assert.IsNull (ex.ParamName, "#7");
			}
		}
#endif

		[Test]
		public void ForColumnNameException()
		{
			DataColumn col = new DataColumn();
			DataColumn col2 = new DataColumn();
			DataColumn col3 = new DataColumn();
			DataColumn col4 = new DataColumn();
			
			col.ColumnName = "abc";
			Assert.AreEqual ("abc", col.ColumnName, "#1");

			_tbl.Columns.Add(col);
			
			//Duplicate name exception
			try {
				col2.ColumnName = "abc";
				_tbl.Columns.Add(col2);
				Assert.AreEqual ("abc", col2.ColumnName, "#2");
				Assert.Fail ("#3");
			} catch (DuplicateNameException) {
			}
			// Make sure case matters in duplicate checks
			col3.ColumnName = "ABC";
			_tbl.Columns.Add(col3);
		}

		[Test]
		public void DefaultValue()
		{
			DataTable tbl = new DataTable();
			tbl.Columns.Add("MyCol", typeof(int));
			
			//Set default Value if Autoincrement is true
			tbl.Columns[0].AutoIncrement = true;
			try {
				tbl.Columns[0].DefaultValue = 2;
				Assert.Fail ("DC19: Failed to throw ArgumentException.");
			} catch (ArgumentException) {
			}

			tbl.Columns[0].AutoIncrement = false;

			//Set default value to an incompatible datatype
			try {
				tbl.Columns[0].DefaultValue = "hello";
				Assert.Fail ("DC21: Failed to throw FormatException.");
			} catch (FormatException) {
			}

			//TODO: maybe add tests for setting default value for types that can implict
			//cast
		}

		[Test]
		public void SetDataType ()
		{
			//test for DataAlready exists and change the datatype
			//supported datatype
			//AutoInc column dataType supported
		}

		[Test]
		public void Defaults1 ()
		{
			//Check for defaults - ColumnName not set at the beginning
			DataTable table = new DataTable();
			DataColumn column = new DataColumn();

			Assert.AreEqual (String.Empty, column.ColumnName, "#A1");
			Assert.AreEqual (typeof (string), column.DataType, "#A2");
			
			table.Columns.Add(column);

			Assert.AreEqual ("Column1", table.Columns [0].ColumnName, "#B1");
			Assert.AreEqual (typeof (string), table.Columns [0].DataType, "#B2");
			
			DataRow row = table.NewRow();
			table.Rows.Add(row);
			DataRow dataRow = table.Rows[0];
			
			object v = dataRow.ItemArray [0];
			Assert.AreEqual (typeof (DBNull), v.GetType (), "#C1");
			Assert.AreEqual (DBNull.Value, v, "#C2");
		}

		[Test]
		public void Defaults2 ()
		{
			//Check for defaults - ColumnName set at the beginning
			string blah = "Blah";
			//Check for defaults - ColumnName not set at the beginning
			DataTable table = new DataTable();
			DataColumn column = new DataColumn(blah);

			Assert.AreEqual (blah, column.ColumnName, "#A1");
			Assert.AreEqual (typeof (string), column.DataType, "#A2");
			
			table.Columns.Add(column);
			
			Assert.AreEqual (blah, table.Columns[0].ColumnName, "#B1");
			Assert.AreEqual (typeof (string), table.Columns[0].DataType, "#B2");
			
			DataRow row = table.NewRow();
			table.Rows.Add(row);
			DataRow dataRow = table.Rows[0];

			object v = dataRow.ItemArray[0];
			Assert.AreEqual (typeof (DBNull), v.GetType (), "#C1");
			Assert.AreEqual (DBNull.Value, v, "#C2");
		}

		[Test]
		public void Defaults3 ()
		{
			DataColumn col = new DataColumn ("foo", typeof (SqlBoolean));
#if NET_2_0
			Assert.AreEqual (SqlBoolean.Null, col.DefaultValue, "#1");
			col.DefaultValue = SqlBoolean.True;
			// FIXME: not working yet
			//col.DefaultValue = true;
			//Assert.AreEqual (SqlBoolean.True, col.DefaultValue, "#2"); // not bool but SqlBoolean
			col.DefaultValue = DBNull.Value;
			Assert.AreEqual (SqlBoolean.Null, col.DefaultValue, "#3"); // not DBNull
#else
			Assert.AreEqual (DBNull.Value, col.DefaultValue, "#1");
			col.DefaultValue = SqlBoolean.True;
			col.DefaultValue = DBNull.Value;
#endif
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DataException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void ChangeTypeAfterSettingDefaultValue ()
		{
			DataColumn col = new DataColumn ("foo", typeof (SqlBoolean));
			col.DefaultValue = true;
			col.DataType = typeof (int);
		}

		[Test]
		public void ExpressionSubstringlimits () {
			DataTable t = new DataTable ();
			t.Columns.Add ("aaa");
			t.Rows.Add (new object [] {"xxx"});
			DataColumn c = t.Columns.Add ("bbb");
			try {
				c.Expression = "SUBSTRING(aaa, 6000000000000000, 2)";
				Assert.Fail ("#1");
			} catch (OverflowException) {
			}
		}

		[Test]
		public void ExpressionFunctions ()
		{
			DataTable T = new DataTable ("test");
			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("id");
			C.Expression = "substring (name, 1, 3) + len (name) + age";
			T.Columns.Add (C);
			
			DataSet Set = new DataSet ("TestSet");
			Set.Tables.Add (T);
			
			DataRow Row = null;
			for (int i = 0; i < 100; i++) {
				Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				T.Rows.Add (Row);
			}
			
			Row = T.NewRow ();
			Row [0] = "h*an";
			Row [1] = DBNull.Value;
			T.Rows.Add (Row);

			Assert.AreEqual ("hum710", T.Rows [10] [2], "#A1");
			Assert.AreEqual ("hum64", T.Rows [4] [2], "#A2");
			C = T.Columns [2];
			C.Expression = "isnull (age, 'succ[[]]ess')";
			Assert.AreEqual ("succ[[]]ess", T.Rows [100] [2], "#A3");

			C.Expression = "iif (age = 24, 'hurrey', 'boo')";
			Assert.AreEqual ("boo", T.Rows [50] [2], "#B1");
			Assert.AreEqual ("hurrey", T.Rows [24] [2], "#B2");

			C.Expression = "convert (age, 'System.Boolean')";
			Assert.AreEqual (Boolean.TrueString, T.Rows [50] [2], "#C1");
			Assert.AreEqual (Boolean.FalseString, T.Rows [0] [2], "#C2");

			//
			// Exceptions
			//

			try {
				// The expression contains undefined function call iff().
				C.Expression = "iff (age = 24, 'hurrey', 'boo')";
				Assert.Fail ("#D");
			} catch (EvaluateException) {
			} catch (SyntaxErrorException) {
			}
			
			//The following two cases fail on mono. MS.net evaluates the expression
			//immediatly upon assignment. We don't do this yet hence we don't throw
			//an exception at this point.
			try {
				C.Expression = "iif (nimi = 24, 'hurrey', 'boo')";
				Assert.Fail ("#E1");
			} catch (EvaluateException e) {
				Assert.AreEqual (typeof (EvaluateException), e.GetType (), "#E2");
				// Never premise English.
				//Assert.AreEqual ("Cannot find column [nimi].", e.Message, "#E3");
			}

			try {
				C.Expression = "iif (name = 24, 'hurrey', 'boo')";
				Assert.Fail ("#F1");
			} catch (EvaluateException e) {
				Assert.AreEqual (typeof (EvaluateException), e.GetType (), "#F2");
				//AssertEquals ("DC41", "Cannot perform '=' operation on System.String and System.Int32.", e.Message);
			}

			try {
				C.Expression = "convert (age, Boolean)";
				Assert.Fail ("#G1");
			} catch (EvaluateException e) {
				Assert.AreEqual (typeof (EvaluateException), e.GetType (), "#G2");
				// Never premise English.
				//Assert.AreEqual ("Invalid type name 'Boolean'.", e.Message, "#G3");
			}
		}

		[Test]
		public void ExpressionAggregates ()
		{
			DataTable T = new DataTable ("test");
			DataTable T2 = new DataTable ("test2");

			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("childname");
			T.Columns.Add (C);

			C = new DataColumn ("expression");
			T.Columns.Add (C);

			DataSet Set = new DataSet ("TestSet");
			Set.Tables.Add (T);
			Set.Tables.Add (T2);
			
			DataRow Row = null;
			for (int i = 0; i < 100; i++) {
				Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				Row [2] = "child" + i;
				T.Rows.Add (Row);
			}
			
			Row = T.NewRow ();
			Row [0] = "h*an";
			Row [1] = DBNull.Value;
			T.Rows.Add (Row);

			C = new DataColumn ("name");
			T2.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T2.Columns.Add (C);

			for (int i = 0; i < 100; i++) {
				Row = T2.NewRow ();
				Row [0] = "child" + i;
				Row [1] = i;
				T2.Rows.Add (Row);
				Row = T2.NewRow ();
				Row [0] = "child" + i;
				Row [1] = i - 2;
				T2.Rows.Add (Row);
			}

			DataRelation Rel = new DataRelation ("Rel", T.Columns [2], T2.Columns [0]);
			Set.Relations.Add (Rel);

			C = T.Columns [3];
			C.Expression = "Sum (Child.age)";
			Assert.AreEqual ("-2", T.Rows [0] [3], "#A1");
			Assert.AreEqual ("98", T.Rows [50] [3], "#A2");

			C.Expression = "Count (Child.age)";
			Assert.AreEqual ("2", T.Rows [0] [3], "#B1");
			Assert.AreEqual ("2", T.Rows [60] [3], "#B2");
		
			C.Expression = "Avg (Child.age)";
			Assert.AreEqual ("-1", T.Rows [0] [3], "#C1");
			Assert.AreEqual ("59", T.Rows [60] [3], "#C2");

			C.Expression = "Min (Child.age)";
			Assert.AreEqual ("-2", T.Rows [0] [3], "#D1");
			Assert.AreEqual ("58", T.Rows [60] [3], "#D2");

			C.Expression = "Max (Child.age)";
			Assert.AreEqual ("0", T.Rows [0] [3], "#E1");
			Assert.AreEqual ("60", T.Rows [60] [3], "#E2");

			C.Expression = "stdev (Child.age)";
			Assert.AreEqual ((1.4142135623731).ToString (T.Locale), T.Rows [0] [3], "#F1");
			Assert.AreEqual ((1.4142135623731).ToString (T.Locale), T.Rows [60] [3], "#F2");

			C.Expression = "var (Child.age)";
			Assert.AreEqual ("2", T.Rows [0] [3], "#G1");
			Assert.AreEqual ("2", T.Rows [60] [3], "#G2");
		}

		[Test]
		public void ExpressionOperator ()
		{
			DataTable T = new DataTable ("test");
			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("id");
			C.Expression = "substring (name, 1, 3) + len (name) + age";
			T.Columns.Add (C);
			
			DataSet Set = new DataSet ("TestSet");
			Set.Tables.Add (T);
			
			DataRow Row = null;
			for (int i = 0; i < 100; i++) {
				Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				T.Rows.Add (Row);
			}
			
			Row = T.NewRow ();
			Row [0] = "h*an";
			Row [1] = DBNull.Value;
			T.Rows.Add (Row);
			
			C = T.Columns [2];
			C.Expression = "age + 4";
			Assert.AreEqual ("68", T.Rows [64] [2], "#A");
			
			C.Expression = "age - 4";
			Assert.AreEqual ("60", T.Rows [64] [2], "#B");
			
			C.Expression = "age * 4";
			Assert.AreEqual ("256", T.Rows [64] [2], "#C");
			
			C.Expression = "age / 4";
			Assert.AreEqual ("16", T.Rows [64] [2], "#D");
			
			C.Expression = "age % 5";
			Assert.AreEqual ("4", T.Rows [64] [2], "#E");
			
			C.Expression = "age in (5, 10, 15, 20, 25)";
			Assert.AreEqual ("False", T.Rows [64] [2], "#F1");
			Assert.AreEqual ("True", T.Rows [25] [2], "#F2");
			
			C.Expression = "name like 'human1%'";
			Assert.AreEqual ("True", T.Rows [1] [2], "#G1");
			Assert.AreEqual ("False", T.Rows [25] [2], "#G2");

			C.Expression = "age < 4";
			Assert.AreEqual ("False", T.Rows [4] [2], "#H1");
			Assert.AreEqual ("True", T.Rows [3] [2], "#H2");

			C.Expression = "age <= 4";
			Assert.AreEqual ("True", T.Rows [4] [2], "#I1");
			Assert.AreEqual ("False", T.Rows [5] [2], "#I2");

			C.Expression = "age > 4";
			Assert.AreEqual ("False", T.Rows [4] [2], "#J1");
			Assert.AreEqual ("True", T.Rows [5] [2], "#J2");

			C.Expression = "age >= 4";
			Assert.AreEqual ("True", T.Rows [4] [2], "#K1");
			Assert.AreEqual ("False", T.Rows [1] [2], "#K2");

			C.Expression = "age = 4";
			Assert.AreEqual ("True", T.Rows [4] [2], "#L1");
			Assert.AreEqual ("False", T.Rows [1] [2], "#L2");

			C.Expression = "age <> 4";
			Assert.AreEqual ("False", T.Rows [4] [2], "#M1");
			Assert.AreEqual ("True", T.Rows [1] [2], "#M2");
		}

		[Test]
		public void SetMaxLengthException ()
		{
			// Setting MaxLength on SimpleContent -> exception
			DataSet ds = new DataSet("Example");
			ds.Tables.Add("MyType");
			ds.Tables["MyType"].Columns.Add(new DataColumn("Desc", 
				typeof (string), "", MappingType.SimpleContent));
			try {
				ds.Tables ["MyType"].Columns ["Desc"].MaxLength = 32;
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void SetMaxLengthNegativeValue ()
		{
			// however setting MaxLength on SimpleContent is OK
			DataSet ds = new DataSet ("Example");
			ds.Tables.Add ("MyType");
			ds.Tables ["MyType"].Columns.Add (
				new DataColumn ("Desc", typeof (string), "", MappingType.SimpleContent));
			ds.Tables ["MyType"].Columns ["Desc"].MaxLength = -1;
		}

		[Test]
		public void AdditionToConstraintCollectionTest()
		{
			DataTable myTable = new DataTable("myTable");
			DataColumn idCol = new DataColumn("id", typeof (int));
			idCol.Unique = true;
			myTable.Columns.Add(idCol);
			ConstraintCollection cc = myTable.Constraints;
			//cc just contains a single UniqueConstraint object.
			UniqueConstraint uc = cc[0] as UniqueConstraint;
			Assert.AreEqual ("id", uc.Columns[0].ColumnName);
		}

		[Test] // bug #77025
		public void CalcStatisticalFunction_SingleElement()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("test", typeof (int));

			table.Rows.Add (new object [] {0});
			table.Columns.Add ("result_var", typeof (double), "var(test)");
			table.Columns.Add ("result_stdev", typeof (double), "stdev(test)");

			// Check DBNull.Value is set as the result 
			Assert.AreEqual (typeof (DBNull), (table.Rows[0]["result_var"]).GetType (), "#1");
			Assert.AreEqual (typeof (DBNull), (table.Rows [0] ["result_stdev"]).GetType (), "#2");
		}

		[Test]
		public void Aggregation_CheckIfChangesDynamically()
		{
			DataTable table = new DataTable ();

			table.Columns.Add ("test", typeof (int));
			table.Columns.Add ("result_count", typeof (int), "count(test)");
			table.Columns.Add ("result_sum", typeof (int), "sum(test)");
			table.Columns.Add ("result_avg", typeof (int), "avg(test)");
			table.Columns.Add ("result_max", typeof (int), "max(test)");
			table.Columns.Add ("result_min", typeof (int), "min(test)");
			table.Columns.Add ("result_var", typeof (double), "var(test)");
			table.Columns.Add ("result_stdev", typeof (double), "stdev(test)");

			// Adding the rows after all the expression columns are added
			table.Rows.Add (new object[] {0});
			Assert.AreEqual (1, table.Rows [0] ["result_count"], "#A1");
			Assert.AreEqual (0, table.Rows [0] ["result_sum"], "#A2");
			Assert.AreEqual (0, table.Rows [0] ["result_avg"], "#A3");
			Assert.AreEqual (0, table.Rows [0] ["result_max"], "#A4");
			Assert.AreEqual (0, table.Rows [0] ["result_min"], "#A5");
			Assert.AreEqual (DBNull.Value, table.Rows [0] ["result_var"], "#A6");
			Assert.AreEqual (DBNull.Value, table.Rows [0] ["result_stdev"], "#A7");

			table.Rows.Add (new object[] {1});
			table.Rows.Add (new object[] {-2});

			// Check if the aggregate columns are updated correctly
			Assert.AreEqual (3, table.Rows [0] ["result_count"], "#B1");
			Assert.AreEqual (-1, table.Rows [0] ["result_sum"], "#B2");
			Assert.AreEqual (0, table.Rows [0] ["result_avg"], "#B3");
			Assert.AreEqual (1, table.Rows [0] ["result_max"], "#B4");
			Assert.AreEqual (-2, table.Rows [0] ["result_min"], "#B5");
			Assert.AreEqual ((7.0 / 3), table.Rows [0] ["result_var"], "#B6");
			Assert.AreEqual (Math.Sqrt (7.0 / 3), table.Rows [0] ["result_stdev"], "#B7");
		}
		
		[Test]
		public void Aggregation_CheckIfChangesDynamically_ChildTable ()
		{
			DataSet ds = new DataSet ();

			DataTable table = new DataTable ();
			DataTable table2 = new DataTable ();
			ds.Tables.Add (table);
			ds.Tables.Add (table2);

			table.Columns.Add ("test", typeof (int));
			table2.Columns.Add ("test", typeof (int));
			table2.Columns.Add ("val", typeof (int));
			DataRelation rel = new DataRelation ("rel", table.Columns[0], table2.Columns[0]);
			ds.Relations.Add (rel);

			table.Columns.Add ("result_count", typeof (int), "count(child.test)");
			table.Columns.Add ("result_sum", typeof (int), "sum(child.test)");
			table.Columns.Add ("result_avg", typeof (int), "avg(child.test)");
			table.Columns.Add ("result_max", typeof (int), "max(child.test)");
			table.Columns.Add ("result_min", typeof (int), "min(child.test)");
			table.Columns.Add ("result_var", typeof (double), "var(child.test)");
			table.Columns.Add ("result_stdev", typeof (double), "stdev(child.test)");

			table.Rows.Add (new object[] {1});
			table.Rows.Add (new object[] {2});
			// Add rows to the child table
			for (int j=0; j<10; j++)
				table2.Rows.Add (new object[] {1,j});
		
			// Check the values for the expression columns in parent table 
			Assert.AreEqual (10, table.Rows [0] ["result_count"], "#A1");
			Assert.AreEqual (0, table.Rows [1] ["result_count"], "#A2");

			Assert.AreEqual (10, table.Rows [0] ["result_sum"], "#B1");
			Assert.AreEqual (DBNull.Value, table.Rows [1] ["result_sum"], "#B2");

			Assert.AreEqual (1, table.Rows [0] ["result_avg"], "#C1");
			Assert.AreEqual (DBNull.Value, table.Rows [1] ["result_avg"], "#C2");

			Assert.AreEqual (1, table.Rows [0] ["result_max"], "#D1");
			Assert.AreEqual (DBNull.Value, table.Rows [1] ["result_max"], "#D2");

			Assert.AreEqual (1, table.Rows [0] ["result_min"], "#E1");
			Assert.AreEqual (DBNull.Value, table.Rows [1] ["result_min"], "#E2");

			Assert.AreEqual (0, table.Rows [0] ["result_var"], "#F1");
			Assert.AreEqual (DBNull.Value, table.Rows [1] ["result_var"], "#F2");

			Assert.AreEqual (0, table.Rows [0] ["result_stdev"], "#G1");
			Assert.AreEqual (DBNull.Value, table.Rows [1] ["result_stdev"], "#G2");
		}

		[Test]
		public void Aggregation_TestForSyntaxErrors ()
		{
			string error = "Aggregation functions cannot be called on Singular(Parent) Columns";
			DataSet ds = new DataSet ();
			DataTable table1 = new DataTable ();
			DataTable table2 = new DataTable ();
			DataTable table3 = new DataTable ();
			
			table1.Columns.Add ("test", typeof(int));
			table2.Columns.Add ("test", typeof(int));
			table3.Columns.Add ("test", typeof(int));

			DataRelation rel1 = new DataRelation ("rel1", table1.Columns[0], table2.Columns[0]);
			DataRelation rel2 = new DataRelation ("rel2", table2.Columns[0], table3.Columns[0]);

			ds.Tables.Add (table1);
			ds.Tables.Add (table2);
			ds.Tables.Add (table3);
			ds.Relations.Add (rel1);
			ds.Relations.Add (rel2);

			error = "Aggregation Functions cannot be called on Columns Returning Single Row (Parent Column)";
			try {
				table2.Columns.Add ("result", typeof (int), "count(parent.test)");
				Assert.Fail ("#1" + error);
			} catch (SyntaxErrorException) {
			}

			error = "Numerical or Functions cannot be called on Columns Returning Multiple Rows (Child Column)";
			// Check arithematic operator
			try {
				table2.Columns.Add ("result", typeof (int), "10*(child.test)");
				Assert.Fail ("#2" + error);
			} catch (SyntaxErrorException) {
			}

			// Check rel operator
			try {
				table2.Columns.Add ("result", typeof (int), "(child.test) > 10");
				Assert.Fail ("#3" + error);
			} catch (SyntaxErrorException) {
			}

			// Check predicates 
			try {
				table2.Columns.Add ("result", typeof (int), "(child.test) IN (1,2,3)");
				Assert.Fail ("#4" + error);
			} catch (SyntaxErrorException) {
			}

			try {
				table2.Columns.Add ("result", typeof (int), "(child.test) LIKE 1");
				Assert.Fail ("#5" + error);
			} catch (SyntaxErrorException) {
			}

			try {
				table2.Columns.Add ("result", typeof (int), "(child.test) IS null");
				Assert.Fail ("#6" + error);
			} catch (SyntaxErrorException) {
			}

			// Check Calc Functions
			try {
				table2.Columns.Add ("result", typeof (int), "isnull(child.test,10)");
				Assert.Fail ("#7" + error);
			} catch (SyntaxErrorException) {
			}
		}

		[Test]
		[Ignore ("passes in ms.net but looks more like a bug in ms.net")]
		public void ExpressionColumns_CheckConversions ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("result_int_div", typeof (int), "(5/10) + 0.5");
			table.Columns.Add ("result_float_div", typeof (int), "(5.0/10) + 0.5");
			table.Rows.Add (new object[] {});

			// ms.net behavior.. seems to covert all numbers to double
			Assert.AreEqual (1, table.Rows [0] [0], "#1");
			Assert.AreEqual (1, table.Rows [0] [1], "#2");
		}

		[Test]
		public void CheckValuesAfterRemovedFromCollection ()
		{
			DataTable table = new DataTable ("table1");
			DataColumn col1 = new DataColumn ("col1", typeof (int));
			DataColumn col2 = new DataColumn ("col2", typeof (int));

			Assert.AreEqual (-1, col1.Ordinal, "#A1");
			Assert.IsNull (col1.Table, "#A2");

			table.Columns.Add (col1);
			table.Columns.Add (col2);
			Assert.AreEqual (0, col1.Ordinal, "#B1");
			Assert.AreEqual (table, col1.Table, "#B2");

			table.Columns.RemoveAt(0);
			Assert.AreEqual (-1, col1.Ordinal, "#C1");
			Assert.IsNull (col1.Table, "#C2");

			table.Columns.Clear ();
			Assert.AreEqual (-1, col2.Ordinal, "#D1");
			Assert.IsNull (col2.Table, "#D2");
		}
		
		[Test]
		public void B565616_NonIConvertibleTypeTest ()
		{
			try {
				DataTable dt = new DataTable ();
				Guid id = Guid.NewGuid();
				dt.Columns.Add ("ID", typeof(string));
				DataRow row = dt.NewRow ();
				row["ID"]= id;
				Assert.AreEqual (id.ToString(), row["ID"], "#N1");
			} catch (InvalidCastException ex) {
				Assert.Fail ("#NonIConvertibleType Test");
			}
		}
		
		[Test]
		public void B623451_SetOrdinalTest ()
		{
			try {
				DataTable t = new DataTable();
				t.Columns.Add("one");
				t.Columns.Add("two");
				t.Columns.Add("three");
				Assert.AreEqual ("one", t.Columns[0].ColumnName, "#SO1-1");
				Assert.AreEqual ("two", t.Columns[1].ColumnName, "#SO1-2");
				Assert.AreEqual ("three", t.Columns[2].ColumnName, "#SO1-3");

				t.Columns["three"].SetOrdinal(0);
				Assert.AreEqual ("three", t.Columns[0].ColumnName, "S02-1");
				Assert.AreEqual ("one", t.Columns[1].ColumnName, "S02-2");
				Assert.AreEqual ("two", t.Columns[2].ColumnName, "S02-3");

				t.Columns["three"].SetOrdinal(1);
				Assert.AreEqual ("one", t.Columns[0].ColumnName, "S03-1");
				Assert.AreEqual ("three", t.Columns[1].ColumnName, "S03-2");
				Assert.AreEqual ("two", t.Columns[2].ColumnName, "S03-3");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.Fail ("SetOrdinalTest failed");
			}
		}
	}
}
