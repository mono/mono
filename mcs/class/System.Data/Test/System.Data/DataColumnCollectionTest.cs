// DataColumnCollectionTest.cs - NUnit Test Cases for System.Data.DataColumnCollection
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Ville Palo <vi64pa@kolumbus.fi>
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2003 Ville Palo
//

using NUnit.Framework;
using System;
using System.Data;
using System.Xml;

namespace MonoTests.System.Data
{
	public class DataColumnCollectionTest : TestCase
	{
		public DataColumnCollectionTest () : base ("MonoTest.System.Data.DataColumnCollectionTest") {}
		public DataColumnCollectionTest (string name) : base (name) {}

		private DataTable _tbl;

		protected override void SetUp () 
		{
			_tbl = new DataTable();
		}

		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite (typeof (DataColumnCollectionTest));
			}
		}

		//TODO
		public void TestAddValidationExceptions()
		{
			
			//Set DefaultValue and AutoIncr == true
			//And get an exception
		}

	        public void TestAdd ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;
			DataColumn C = null;
			Cols.Add ();
			Cols.Add ();
	        	
	        	C = Cols [0];
			AssertEquals ("test#01", true, C.AllowDBNull);
			AssertEquals ("test#02", false, C.AutoIncrement);
			AssertEquals ("test#03", 0L, C.AutoIncrementSeed);
			AssertEquals ("test#04", 1L, C.AutoIncrementStep);
			AssertEquals ("test#05", "Column1", C.Caption);
			AssertEquals ("test#06", "Element", C.ColumnMapping.ToString ());
			AssertEquals ("test#07", "Column1", C.ColumnName);
			AssertEquals ("test#08", true, C.Container == null);
			AssertEquals ("test#09", typeof (string), C.DataType);
			AssertEquals ("test#10", DBNull.Value, C.DefaultValue);
			AssertEquals ("test#11", false, C.DesignMode);
			AssertEquals ("test#12", "", C.Expression);
			AssertEquals ("test#13", 0, C.ExtendedProperties.Count);
			AssertEquals ("test#14", -1, C.MaxLength);
			AssertEquals ("test#15", "", C.Namespace);
			AssertEquals ("test#16", 0, C.Ordinal);
			AssertEquals ("test#17", "", C.Prefix);
			AssertEquals ("test#18", false, C.ReadOnly);
			AssertEquals ("test#19", null, C.Site);
			AssertEquals ("test#20", "test_table", C.Table.TableName);
			AssertEquals ("test#21", "Column1", C.ToString ());
			AssertEquals ("test#22", false, C.Unique);

			C = Cols [1];
			AssertEquals ("test#23", true, C.AllowDBNull);
			AssertEquals ("test#24", false, C.AutoIncrement);
			AssertEquals ("test#25", 0L, C.AutoIncrementSeed);
			AssertEquals ("test#26", 1L, C.AutoIncrementStep);
			AssertEquals ("test#27", "Column2", C.Caption);
			AssertEquals ("test#28", "Element", C.ColumnMapping.ToString ());
			AssertEquals ("test#29", "Column2", C.ColumnName);
			AssertEquals ("test#30", true, C.Container == null);
			AssertEquals ("test#31", typeof (string), C.DataType);
			AssertEquals ("test#32", DBNull.Value, C.DefaultValue);
			AssertEquals ("test#33", false, C.DesignMode);
			AssertEquals ("test#34", "", C.Expression);
			AssertEquals ("test#35", 0, C.ExtendedProperties.Count);
			AssertEquals ("test#36", -1, C.MaxLength);
			AssertEquals ("test#37", "", C.Namespace);
			AssertEquals ("test#38", 1, C.Ordinal);
			AssertEquals ("test#39", "", C.Prefix);
			AssertEquals ("test#40", false, C.ReadOnly);
			AssertEquals ("test#41", null, C.Site);
			AssertEquals ("test#42", "test_table", C.Table.TableName);
			AssertEquals ("test#43", "Column2", C.ToString ());
			AssertEquals ("test#44", false, C.Unique);

			Cols.Add ("test1", typeof (int), "");
			Cols.Add ("test2", typeof (string), "Column1 +  Column2");

			C = Cols [2];
			AssertEquals ("test#45", true, C.AllowDBNull);
			AssertEquals ("test#46", false, C.AutoIncrement);
			AssertEquals ("test#47", 0L, C.AutoIncrementSeed);
			AssertEquals ("test#48", 1L, C.AutoIncrementStep);
			AssertEquals ("test#49", "test1", C.Caption);
			AssertEquals ("test#50", "Element", C.ColumnMapping.ToString ());
			AssertEquals ("test#51", "test1", C.ColumnName);
			AssertEquals ("test#52", true, C.Container == null);
			AssertEquals ("test#53", typeof (int), C.DataType);
			AssertEquals ("test#54", DBNull.Value, C.DefaultValue);
			AssertEquals ("test#55", false, C.DesignMode);
			AssertEquals ("test#56", "", C.Expression);
			AssertEquals ("test#57", 0, C.ExtendedProperties.Count);
			AssertEquals ("test#58", -1, C.MaxLength);
			AssertEquals ("test#59", "", C.Namespace);
			AssertEquals ("test#60", 2, C.Ordinal);
			AssertEquals ("test#61", "", C.Prefix);
			AssertEquals ("test#62", false, C.ReadOnly);
			AssertEquals ("test#63", null, C.Site);
			AssertEquals ("test#64", "test_table", C.Table.TableName);
			AssertEquals ("test#65", "test1", C.ToString ());
			AssertEquals ("test#66", false, C.Unique);

			C = Cols [3];
			AssertEquals ("test#67", true, C.AllowDBNull);
			AssertEquals ("test#68", false, C.AutoIncrement);
			AssertEquals ("test#69", 0L, C.AutoIncrementSeed);
			AssertEquals ("test#70", 1L, C.AutoIncrementStep);
			AssertEquals ("test#71", "test2", C.Caption);
			AssertEquals ("test#72", "Element", C.ColumnMapping.ToString ());
			AssertEquals ("test#73", "test2", C.ColumnName);
			AssertEquals ("test#74", true, C.Container == null);
			AssertEquals ("test#75", typeof (string), C.DataType);
			AssertEquals ("test#76", DBNull.Value, C.DefaultValue);
			AssertEquals ("test#77", false, C.DesignMode);
			AssertEquals ("test#78", "Column1 +  Column2", C.Expression);
			AssertEquals ("test#79", 0, C.ExtendedProperties.Count);
			AssertEquals ("test#80", -1, C.MaxLength);
			AssertEquals ("test#81", "", C.Namespace);
			AssertEquals ("test#82", 3, C.Ordinal);
			AssertEquals ("test#83", "", C.Prefix);
			AssertEquals ("test#84", true, C.ReadOnly);
			AssertEquals ("test#85", null, C.Site);
			AssertEquals ("test#86", "test_table", C.Table.TableName);
			AssertEquals ("test#87", "test2 + Column1 +  Column2", C.ToString ());
			AssertEquals ("test#88", false, C.Unique); 

			C = new DataColumn ("test3", typeof (int));
			Cols.Add (C);

			C = Cols [4];
			AssertEquals ("test#89", true, C.AllowDBNull);
			AssertEquals ("test#90", false, C.AutoIncrement);
			AssertEquals ("test#91", 0L, C.AutoIncrementSeed);
			AssertEquals ("test#92", 1L, C.AutoIncrementStep);
			AssertEquals ("test#93", "test3", C.Caption);
			AssertEquals ("test#94", "Element", C.ColumnMapping.ToString ());
			AssertEquals ("test#95", "test3", C.ColumnName);
			AssertEquals ("test#96", true, C.Container == null);
			AssertEquals ("test#97", typeof (int), C.DataType);
			AssertEquals ("test#98", DBNull.Value, C.DefaultValue);
			AssertEquals ("test#99", false, C.DesignMode);
			AssertEquals ("test#100", "", C.Expression);
			AssertEquals ("test#101", 0, C.ExtendedProperties.Count);
			AssertEquals ("test#102", -1, C.MaxLength);
			AssertEquals ("test#103", "", C.Namespace);
			AssertEquals ("test#104", 4, C.Ordinal);
			AssertEquals ("test#105", "", C.Prefix);
			AssertEquals ("test#106", false, C.ReadOnly);
			AssertEquals ("test#107", null, C.Site);
			AssertEquals ("test#108", "test_table", C.Table.TableName);
			AssertEquals ("test#109", "test3", C.ToString ());
			AssertEquals ("test#110", false, C.Unique); 
		}

		public void TestAddExceptions ()
		{
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table2");
			DataColumnCollection Cols = Table.Columns;
			DataColumn C = null;

			try {
				Cols.Add (C);
				Fail ("test#01");
			} catch (Exception e) {
				AssertEquals ("test#02", typeof (ArgumentNullException), e.GetType ());
				AssertEquals ("test#03", "'column' argument cannot be null.\r\nParameter name: column", e.Message);
			}

			C = new DataColumn ("test");
			Cols.Add (C);

			try {
				Cols.Add (C);
				Fail ("test#04");
			} catch (Exception e) {
				AssertEquals ("test#05", typeof (ArgumentException), e.GetType ());
				AssertEquals ("test#06", "Column 'test' already belongs to this DataTable.", e.Message);
			}

			try {
				Table2.Columns.Add (C);
				Fail ("test#07");
			} catch (Exception e) {
				AssertEquals ("test#08", typeof (ArgumentException), e.GetType ());
				AssertEquals ("test#09", "Column 'test' already belongs to another DataTable.", e.Message);
			}

			DataColumn C2 = new DataColumn ("test");

			try {
				Cols.Add (C2);
				Fail ("test#10");
			} catch (Exception e) {
				AssertEquals ("test#11", typeof (DuplicateNameException), e.GetType ());
				AssertEquals ("test#12", "A column named 'test' already belongs to this DataTable.", e.Message);
			}

			try {
				Cols.Add ("test2", typeof (string), "substring ('fdsafewq', 2)");
				Fail ("test#13");
			} catch (Exception e) {
				AssertEquals ("test#14", true, e is InvalidExpressionException);
				AssertEquals ("test#15", "Invalid number of arguments: function substring().", e.Message);
			}
		}

		public void TestAddRange ()
		{			
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table2");
			DataColumnCollection Cols = Table.Columns;
			DataColumn C = null;
			DataColumn [] ColArray = new DataColumn [2];

			C = new DataColumn ("test1");
			ColArray [0] = C;

			C = new DataColumn ("test2");
			C.AllowDBNull = false;
			C.Caption = "Test_caption";
			C.DataType = typeof (XmlReader);
			ColArray [1] = C;

			Cols.AddRange (ColArray);

			C = Cols [0];
			AssertEquals ("test#01", true, C.AllowDBNull);
			AssertEquals ("test#02", false, C.AutoIncrement);
			AssertEquals ("test#03", 0L, C.AutoIncrementSeed);
			AssertEquals ("test#04", 1L, C.AutoIncrementStep);
			AssertEquals ("test#05", "test1", C.Caption);
			AssertEquals ("test#06", "Element", C.ColumnMapping.ToString ());
			AssertEquals ("test#07", "test1", C.ColumnName);
			AssertEquals ("test#08", true, C.Container == null);
			AssertEquals ("test#09", typeof (string), C.DataType);
			AssertEquals ("test#10", DBNull.Value, C.DefaultValue);
			AssertEquals ("test#11", false, C.DesignMode);
			AssertEquals ("test#12", "", C.Expression);
			AssertEquals ("test#13", 0, C.ExtendedProperties.Count);
			AssertEquals ("test#14", -1, C.MaxLength);
			AssertEquals ("test#15", "", C.Namespace);
			AssertEquals ("test#16", 0, C.Ordinal);
			AssertEquals ("test#17", "", C.Prefix);
			AssertEquals ("test#18", false, C.ReadOnly);
			AssertEquals ("test#19", null, C.Site);
			AssertEquals ("test#20", "test_table", C.Table.TableName);
			AssertEquals ("test#21", "test1", C.ToString ());
			AssertEquals ("test#22", false, C.Unique);

			C = Cols [1];
			AssertEquals ("test#01", false, C.AllowDBNull);
			AssertEquals ("test#02", false, C.AutoIncrement);
			AssertEquals ("test#03", 0L, C.AutoIncrementSeed);
			AssertEquals ("test#04", 1L, C.AutoIncrementStep);
			AssertEquals ("test#05", "Test_caption", C.Caption);
			AssertEquals ("test#06", "Element", C.ColumnMapping.ToString ());
			AssertEquals ("test#07", "test2", C.ColumnName);
			AssertEquals ("test#08", true, C.Container == null);
			AssertEquals ("test#09", typeof (XmlReader), C.DataType);
			AssertEquals ("test#10", DBNull.Value, C.DefaultValue);
			AssertEquals ("test#11", false, C.DesignMode);
			AssertEquals ("test#12", "", C.Expression);
			AssertEquals ("test#13", 0, C.ExtendedProperties.Count);
			AssertEquals ("test#14", -1, C.MaxLength);
			AssertEquals ("test#15", "", C.Namespace);
			AssertEquals ("test#16", 1, C.Ordinal);
			AssertEquals ("test#17", "", C.Prefix);
			AssertEquals ("test#18", false, C.ReadOnly);
			AssertEquals ("test#19", null, C.Site);
			AssertEquals ("test#20", "test_table", C.Table.TableName);
			AssertEquals ("test#21", "test2", C.ToString ());
			AssertEquals ("test#22", false, C.Unique);
		}

		public void TestCanRemove ()
		{
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table_2");
			DataColumnCollection Cols = Table.Columns;
			DataColumn C = new DataColumn ("test1");
			Cols.Add ();

			// LAMESPEC: MSDN says that if C doesn't belong to Cols
			// Exception is thrown.
			AssertEquals ("test#01", false, Cols.CanRemove (C));

			Cols.Add (C);
			AssertEquals ("test#02", true, Cols.CanRemove (C));

			C = new DataColumn ();
			C.Expression = "test1 + 2";
			Cols.Add (C);

			C = Cols ["test2"];
			AssertEquals ("test#03", false, Cols.CanRemove (C));

			C = new DataColumn ("t");
			Table2.Columns.Add (C);
			DataColumnCollection Cols2 = Table2.Columns;
			AssertEquals ("test#04", true, Cols2.CanRemove (C));

			DataRelation Rel = new DataRelation ("Rel", Table.Columns [0], Table2.Columns [0]);
			DataSet Set = new DataSet ();
			Set.Tables.Add (Table);
			Set.Tables.Add (Table2);
			Set.Relations.Add (Rel);

			AssertEquals ("test#05", false, Cols2.CanRemove (C));
			AssertEquals ("test#06", false, Cols.CanRemove (null));
		}

		public void TestClear ()
		{
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table2");
			DataSet Set = new DataSet ();
			Set.Tables.Add (Table);
			Set.Tables.Add (Table2);
			DataColumnCollection Cols = Table.Columns;
			DataColumnCollection Cols2 = Table2.Columns;

			Cols.Add ();
			Cols.Add ("testi");

			Cols.Clear ();
			AssertEquals ("test#01", 0, Cols.Count);

			Cols.Add ();
			Cols.Add ("testi");
			Cols2.Add ();
			Cols2.Add ();

			DataRelation Rel = new DataRelation ("Rel", Cols [0], Cols2 [0]);
			Set.Relations.Add (Rel);
			try {
				Cols.Clear ();
				Fail ("test#02");
			} catch (Exception e) {
				AssertEquals ("test#03", typeof (ArgumentException), e.GetType ());
				AssertEquals ("test#04", "Cannot remove this column, because it is part of the parent key for relationship Rel.", e.Message);
			}
		}

		public void TestContains ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("tesT2");

			AssertEquals ("test#01", true, Cols.Contains ("test"));
			AssertEquals ("test#02", false, Cols.Contains ("_test"));
			AssertEquals ("test#03", true, Cols.Contains ("TEST"));
			Table.CaseSensitive = true;
			AssertEquals ("test#04", true, Cols.Contains ("TEST"));
			AssertEquals ("test#05", true, Cols.Contains ("test2"));
			AssertEquals ("test#06", false, Cols.Contains ("_test2"));
			AssertEquals ("test#07", true, Cols.Contains ("TEST2"));
		}

		public void TestCopyTo ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			Cols.Add ("test4");

			DataColumn [] array = new DataColumn [4];
			Cols.CopyTo (array, 0);
			AssertEquals ("test#01", 4, array.Length);
			AssertEquals ("test#02", "test", array [0].ColumnName);
			AssertEquals ("test#03", "test2", array [1].ColumnName);
			AssertEquals ("test#04", "test3", array [2].ColumnName);
			AssertEquals ("test#05", "test4", array [3].ColumnName);

			array = new DataColumn [6];
			Cols.CopyTo (array, 2);
			AssertEquals ("test#06", 6, array.Length);
			AssertEquals ("test#07", "test", array [2].ColumnName);
			AssertEquals ("test#08", "test2", array [3].ColumnName);
			AssertEquals ("test#09", "test3", array [4].ColumnName);
			AssertEquals ("test#10", "test4", array [5].ColumnName);
			AssertEquals ("test#11", null, array [0]);
			AssertEquals ("test#12", null, array [1]);
		}

		public void TestEquals ()
		{
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;
			DataColumnCollection Cols2 = Table2.Columns;

			AssertEquals ("test#01", false, Cols.Equals (Cols2));
			AssertEquals ("test#02", false, Cols2.Equals (Cols));
			AssertEquals ("test#03", false, Object.Equals (Cols, Cols2));
			AssertEquals ("test#04", true, Cols.Equals (Cols));
			AssertEquals ("test#05", true, Cols2.Equals (Cols2));
			AssertEquals ("test#06", true, Object.Equals (Cols2, Cols2));
		}

		public void TestIndexOf ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			Cols.Add ("test4");

			AssertEquals ("test#01", 0, Cols.IndexOf ("test"));
			AssertEquals ("test#02", 1, Cols.IndexOf ("TEST2"));
			Table.CaseSensitive = true;
			AssertEquals ("test#03", 1, Cols.IndexOf ("TEST2"));

			AssertEquals ("test#04", 3, Cols.IndexOf (Cols [3]));
			DataColumn C = new DataColumn ("error");
			AssertEquals ("test#05", -1, Cols.IndexOf (C));
			AssertEquals ("test#06", -1, Cols.IndexOf ("_error_"));
		}

		public void TestRemove ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			Cols.Add ("test4");

			AssertEquals ("test#01", 4, Cols.Count);
			Cols.Remove ("test2");
			AssertEquals ("test#02", 3, Cols.Count);
			Cols.Remove ("TEST3");
			AssertEquals ("test#03", 2, Cols.Count);

			try {
				Cols.Remove ("_test_");
				Fail ("test#04");
			} catch (Exception e) {
				AssertEquals ("test#05", typeof (ArgumentException), e.GetType ());
				AssertEquals ("test#06", "Column '_test_' does not belong to table test_table.", e.Message);
			}

			Cols.Add ();
			Cols.Add ();
			Cols.Add ();
			Cols.Add ();

			AssertEquals ("test#07", 6, Cols.Count);
			Cols.Remove (Cols [0]);
			Cols.Remove (Cols [0]);
			AssertEquals ("test#08", 4, Cols.Count);
			AssertEquals ("test#09", "Column1", Cols [0].ColumnName);

			try {
				Cols.Remove (new DataColumn ("Column1"));
				Fail ("test#10");
			} catch (Exception e) {
				AssertEquals ("test#11", typeof (ArgumentException), e.GetType ());
				AssertEquals ("test#12", "Cannot remove a column that doesn't belong to this table.", e.Message);
			}

			Cols.Add ();
			Cols.Add ();
			Cols.Add ();
			Cols.Add ();

			AssertEquals ("test#13", 8, Cols.Count);
			Cols.RemoveAt (7);
			Cols.RemoveAt (1);
			Cols.RemoveAt (0);
			Cols.RemoveAt (0);
			AssertEquals ("test#14", 4, Cols.Count);
			AssertEquals ("test#15", "Column4", Cols [0].ColumnName);
			AssertEquals ("test#16", "Column5", Cols [1].ColumnName);

			try {
				Cols.RemoveAt (10);
				Fail ("test#17");
			} catch (Exception e) {
				AssertEquals ("test#18", typeof (IndexOutOfRangeException), e.GetType ());
				AssertEquals ("test#19", "Cannot find column 10.", e.Message);
			}
		}

		public void TestToString ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			AssertEquals ("test#01", "System.Data.DataColumnCollection", Cols.ToString ());
		}
	}
}
