// DataColumnCollectionTest.cs - NUnit Test Cases for System.Data.DataColumnCollection
//
// Authors:
//   Franklin Wise <gracenote@earthlink.net>
//   Ville Palo <vi64pa@kolumbus.fi>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2003 Ville Palo
// (C) Copyright 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Data;
using System.Xml;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataColumnCollectionTest
	{
		private DataTable _tbl;

		[SetUp]
		public void GetReady () 
		{
			_tbl = new DataTable();
		}

		//TODO
		[Test]
		public void AddValidationExceptions()
		{
			
			//Set DefaultValue and AutoIncr == true
			//And get an exception
		}

		[Test]
	        public void Add ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;
			DataColumn C = null;
			Cols.Add ();
			Cols.Add ();
	        	
	        	C = Cols [0];
			Assertion.AssertEquals ("test#01", true, C.AllowDBNull);
			Assertion.AssertEquals ("test#02", false, C.AutoIncrement);
			Assertion.AssertEquals ("test#03", 0L, C.AutoIncrementSeed);
			Assertion.AssertEquals ("test#04", 1L, C.AutoIncrementStep);
			Assertion.AssertEquals ("test#05", "Column1", C.Caption);
			Assertion.AssertEquals ("test#06", "Element", C.ColumnMapping.ToString ());
			Assertion.AssertEquals ("test#07", "Column1", C.ColumnName);
			Assertion.AssertEquals ("test#08", true, C.Container == null);
			Assertion.AssertEquals ("test#09", typeof (string), C.DataType);
			Assertion.AssertEquals ("test#10", DBNull.Value, C.DefaultValue);
			Assertion.AssertEquals ("test#11", false, C.DesignMode);
			Assertion.AssertEquals ("test#12", "", C.Expression);
			Assertion.AssertEquals ("test#13", 0, C.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#14", -1, C.MaxLength);
			Assertion.AssertEquals ("test#15", "", C.Namespace);
			Assertion.AssertEquals ("test#16", 0, C.Ordinal);
			Assertion.AssertEquals ("test#17", "", C.Prefix);
			Assertion.AssertEquals ("test#18", false, C.ReadOnly);
			Assertion.AssertEquals ("test#19", null, C.Site);
			Assertion.AssertEquals ("test#20", "test_table", C.Table.TableName);
			Assertion.AssertEquals ("test#21", "Column1", C.ToString ());
			Assertion.AssertEquals ("test#22", false, C.Unique);

			C = Cols [1];
			Assertion.AssertEquals ("test#23", true, C.AllowDBNull);
			Assertion.AssertEquals ("test#24", false, C.AutoIncrement);
			Assertion.AssertEquals ("test#25", 0L, C.AutoIncrementSeed);
			Assertion.AssertEquals ("test#26", 1L, C.AutoIncrementStep);
			Assertion.AssertEquals ("test#27", "Column2", C.Caption);
			Assertion.AssertEquals ("test#28", "Element", C.ColumnMapping.ToString ());
			Assertion.AssertEquals ("test#29", "Column2", C.ColumnName);
			Assertion.AssertEquals ("test#30", true, C.Container == null);
			Assertion.AssertEquals ("test#31", typeof (string), C.DataType);
			Assertion.AssertEquals ("test#32", DBNull.Value, C.DefaultValue);
			Assertion.AssertEquals ("test#33", false, C.DesignMode);
			Assertion.AssertEquals ("test#34", "", C.Expression);
			Assertion.AssertEquals ("test#35", 0, C.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#36", -1, C.MaxLength);
			Assertion.AssertEquals ("test#37", "", C.Namespace);
			Assertion.AssertEquals ("test#38", 1, C.Ordinal);
			Assertion.AssertEquals ("test#39", "", C.Prefix);
			Assertion.AssertEquals ("test#40", false, C.ReadOnly);
			Assertion.AssertEquals ("test#41", null, C.Site);
			Assertion.AssertEquals ("test#42", "test_table", C.Table.TableName);
			Assertion.AssertEquals ("test#43", "Column2", C.ToString ());
			Assertion.AssertEquals ("test#44", false, C.Unique);

			Cols.Add ("test1", typeof (int), "");
			Cols.Add ("test2", typeof (string), "Column1 +  Column2");

			C = Cols [2];
			Assertion.AssertEquals ("test#45", true, C.AllowDBNull);
			Assertion.AssertEquals ("test#46", false, C.AutoIncrement);
			Assertion.AssertEquals ("test#47", 0L, C.AutoIncrementSeed);
			Assertion.AssertEquals ("test#48", 1L, C.AutoIncrementStep);
			Assertion.AssertEquals ("test#49", "test1", C.Caption);
			Assertion.AssertEquals ("test#50", "Element", C.ColumnMapping.ToString ());
			Assertion.AssertEquals ("test#51", "test1", C.ColumnName);
			Assertion.AssertEquals ("test#52", true, C.Container == null);
			Assertion.AssertEquals ("test#53", typeof (int), C.DataType);
			Assertion.AssertEquals ("test#54", DBNull.Value, C.DefaultValue);
			Assertion.AssertEquals ("test#55", false, C.DesignMode);
			Assertion.AssertEquals ("test#56", "", C.Expression);
			Assertion.AssertEquals ("test#57", 0, C.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#58", -1, C.MaxLength);
			Assertion.AssertEquals ("test#59", "", C.Namespace);
			Assertion.AssertEquals ("test#60", 2, C.Ordinal);
			Assertion.AssertEquals ("test#61", "", C.Prefix);
			Assertion.AssertEquals ("test#62", false, C.ReadOnly);
			Assertion.AssertEquals ("test#63", null, C.Site);
			Assertion.AssertEquals ("test#64", "test_table", C.Table.TableName);
			Assertion.AssertEquals ("test#65", "test1", C.ToString ());
			Assertion.AssertEquals ("test#66", false, C.Unique);

			C = Cols [3];
			Assertion.AssertEquals ("test#67", true, C.AllowDBNull);
			Assertion.AssertEquals ("test#68", false, C.AutoIncrement);
			Assertion.AssertEquals ("test#69", 0L, C.AutoIncrementSeed);
			Assertion.AssertEquals ("test#70", 1L, C.AutoIncrementStep);
			Assertion.AssertEquals ("test#71", "test2", C.Caption);
			Assertion.AssertEquals ("test#72", "Element", C.ColumnMapping.ToString ());
			Assertion.AssertEquals ("test#73", "test2", C.ColumnName);
			Assertion.AssertEquals ("test#74", true, C.Container == null);
			Assertion.AssertEquals ("test#75", typeof (string), C.DataType);
			Assertion.AssertEquals ("test#76", DBNull.Value, C.DefaultValue);
			Assertion.AssertEquals ("test#77", false, C.DesignMode);
			Assertion.AssertEquals ("test#78", "Column1 +  Column2", C.Expression);
			Assertion.AssertEquals ("test#79", 0, C.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#80", -1, C.MaxLength);
			Assertion.AssertEquals ("test#81", "", C.Namespace);
			Assertion.AssertEquals ("test#82", 3, C.Ordinal);
			Assertion.AssertEquals ("test#83", "", C.Prefix);
			Assertion.AssertEquals ("test#84", true, C.ReadOnly);
			Assertion.AssertEquals ("test#85", null, C.Site);
			Assertion.AssertEquals ("test#86", "test_table", C.Table.TableName);
			Assertion.AssertEquals ("test#87", "test2 + Column1 +  Column2", C.ToString ());
			Assertion.AssertEquals ("test#88", false, C.Unique); 

			C = new DataColumn ("test3", typeof (int));
			Cols.Add (C);

			C = Cols [4];
			Assertion.AssertEquals ("test#89", true, C.AllowDBNull);
			Assertion.AssertEquals ("test#90", false, C.AutoIncrement);
			Assertion.AssertEquals ("test#91", 0L, C.AutoIncrementSeed);
			Assertion.AssertEquals ("test#92", 1L, C.AutoIncrementStep);
			Assertion.AssertEquals ("test#93", "test3", C.Caption);
			Assertion.AssertEquals ("test#94", "Element", C.ColumnMapping.ToString ());
			Assertion.AssertEquals ("test#95", "test3", C.ColumnName);
			Assertion.AssertEquals ("test#96", true, C.Container == null);
			Assertion.AssertEquals ("test#97", typeof (int), C.DataType);
			Assertion.AssertEquals ("test#98", DBNull.Value, C.DefaultValue);
			Assertion.AssertEquals ("test#99", false, C.DesignMode);
			Assertion.AssertEquals ("test#100", "", C.Expression);
			Assertion.AssertEquals ("test#101", 0, C.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#102", -1, C.MaxLength);
			Assertion.AssertEquals ("test#103", "", C.Namespace);
			Assertion.AssertEquals ("test#104", 4, C.Ordinal);
			Assertion.AssertEquals ("test#105", "", C.Prefix);
			Assertion.AssertEquals ("test#106", false, C.ReadOnly);
			Assertion.AssertEquals ("test#107", null, C.Site);
			Assertion.AssertEquals ("test#108", "test_table", C.Table.TableName);
			Assertion.AssertEquals ("test#109", "test3", C.ToString ());
			Assertion.AssertEquals ("test#110", false, C.Unique); 
		}

		[Test]
		public void AddExceptions ()
		{
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table2");
			DataColumnCollection Cols = Table.Columns;
			DataColumn C = null;

			try {
				Cols.Add (C);
				Assertion.Fail ("test#01");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#02", typeof (ArgumentNullException), e.GetType ());
			}

			C = new DataColumn ("test");
			Cols.Add (C);

			try {
				Cols.Add (C);
				Assertion.Fail ("test#04");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#05", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#06", "Column 'test' already belongs to this DataTable.", e.Message);
			}

			try {
				Table2.Columns.Add (C);
				Assertion.Fail ("test#07");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#08", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#09", "Column 'test' already belongs to another DataTable.", e.Message);
			}

			DataColumn C2 = new DataColumn ("test");

			try {
				Cols.Add (C2);
				Assertion.Fail ("test#10");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#11", typeof (DuplicateNameException), e.GetType ());
				Assertion.AssertEquals ("test#12", "A column named 'test' already belongs to this DataTable.", e.Message);
			}

			try {
				Cols.Add ("test2", typeof (string), "substring ('fdsafewq', 2)");
				Assertion.Fail ("test#13");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#14", true, e is InvalidExpressionException);
				Assertion.AssertEquals ("test#15", "Invalid number of arguments: function substring().", e.Message);
			}
		}

		[Test]
		public void AddRange ()
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
			Assertion.AssertEquals ("test#01", true, C.AllowDBNull);
			Assertion.AssertEquals ("test#02", false, C.AutoIncrement);
			Assertion.AssertEquals ("test#03", 0L, C.AutoIncrementSeed);
			Assertion.AssertEquals ("test#04", 1L, C.AutoIncrementStep);
			Assertion.AssertEquals ("test#05", "test1", C.Caption);
			Assertion.AssertEquals ("test#06", "Element", C.ColumnMapping.ToString ());
			Assertion.AssertEquals ("test#07", "test1", C.ColumnName);
			Assertion.AssertEquals ("test#08", true, C.Container == null);
			Assertion.AssertEquals ("test#09", typeof (string), C.DataType);
			Assertion.AssertEquals ("test#10", DBNull.Value, C.DefaultValue);
			Assertion.AssertEquals ("test#11", false, C.DesignMode);
			Assertion.AssertEquals ("test#12", "", C.Expression);
			Assertion.AssertEquals ("test#13", 0, C.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#14", -1, C.MaxLength);
			Assertion.AssertEquals ("test#15", "", C.Namespace);
			Assertion.AssertEquals ("test#16", 0, C.Ordinal);
			Assertion.AssertEquals ("test#17", "", C.Prefix);
			Assertion.AssertEquals ("test#18", false, C.ReadOnly);
			Assertion.AssertEquals ("test#19", null, C.Site);
			Assertion.AssertEquals ("test#20", "test_table", C.Table.TableName);
			Assertion.AssertEquals ("test#21", "test1", C.ToString ());
			Assertion.AssertEquals ("test#22", false, C.Unique);

			C = Cols [1];
			Assertion.AssertEquals ("test#01", false, C.AllowDBNull);
			Assertion.AssertEquals ("test#02", false, C.AutoIncrement);
			Assertion.AssertEquals ("test#03", 0L, C.AutoIncrementSeed);
			Assertion.AssertEquals ("test#04", 1L, C.AutoIncrementStep);
			Assertion.AssertEquals ("test#05", "Test_caption", C.Caption);
			Assertion.AssertEquals ("test#06", "Element", C.ColumnMapping.ToString ());
			Assertion.AssertEquals ("test#07", "test2", C.ColumnName);
			Assertion.AssertEquals ("test#08", true, C.Container == null);
			Assertion.AssertEquals ("test#09", typeof (XmlReader), C.DataType);
			Assertion.AssertEquals ("test#10", DBNull.Value, C.DefaultValue);
			Assertion.AssertEquals ("test#11", false, C.DesignMode);
			Assertion.AssertEquals ("test#12", "", C.Expression);
			Assertion.AssertEquals ("test#13", 0, C.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#14", -1, C.MaxLength);
			Assertion.AssertEquals ("test#15", "", C.Namespace);
			Assertion.AssertEquals ("test#16", 1, C.Ordinal);
			Assertion.AssertEquals ("test#17", "", C.Prefix);
			Assertion.AssertEquals ("test#18", false, C.ReadOnly);
			Assertion.AssertEquals ("test#19", null, C.Site);
			Assertion.AssertEquals ("test#20", "test_table", C.Table.TableName);
			Assertion.AssertEquals ("test#21", "test2", C.ToString ());
			Assertion.AssertEquals ("test#22", false, C.Unique);
		}

		[Test]
		public void CanRemove ()
		{
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table_2");
			DataColumnCollection Cols = Table.Columns;
			DataColumn C = new DataColumn ("test1");
			Cols.Add ();

			// LAMESPEC: MSDN says that if C doesn't belong to Cols
			// Exception is thrown.
			Assertion.AssertEquals ("test#01", false, Cols.CanRemove (C));

			Cols.Add (C);
			Assertion.AssertEquals ("test#02", true, Cols.CanRemove (C));

			C = new DataColumn ();
			C.Expression = "test1 + 2";
			Cols.Add (C);

			C = Cols ["test2"];
			Assertion.AssertEquals ("test#03", false, Cols.CanRemove (C));

			C = new DataColumn ("t");
			Table2.Columns.Add (C);
			DataColumnCollection Cols2 = Table2.Columns;
			Assertion.AssertEquals ("test#04", true, Cols2.CanRemove (C));

			DataRelation Rel = new DataRelation ("Rel", Table.Columns [0], Table2.Columns [0]);
			DataSet Set = new DataSet ();
			Set.Tables.Add (Table);
			Set.Tables.Add (Table2);
			Set.Relations.Add (Rel);

			Assertion.AssertEquals ("test#05", false, Cols2.CanRemove (C));
			Assertion.AssertEquals ("test#06", false, Cols.CanRemove (null));
		}

		[Test]
		public void Clear ()
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
			Assertion.AssertEquals ("test#01", 0, Cols.Count);

			Cols.Add ();
			Cols.Add ("testi");
			Cols2.Add ();
			Cols2.Add ();

			DataRelation Rel = new DataRelation ("Rel", Cols [0], Cols2 [0]);
			Set.Relations.Add (Rel);
			try {
				Cols.Clear ();
				Assertion.Fail ("test#02");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#03", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#04", "Cannot remove this column, because it is part of the parent key for relationship Rel.", e.Message);
			}
		}

		[Test]
		public void Contains ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("tesT2");

			Assertion.AssertEquals ("test#01", true, Cols.Contains ("test"));
			Assertion.AssertEquals ("test#02", false, Cols.Contains ("_test"));
			Assertion.AssertEquals ("test#03", true, Cols.Contains ("TEST"));
			Table.CaseSensitive = true;
			Assertion.AssertEquals ("test#04", true, Cols.Contains ("TEST"));
			Assertion.AssertEquals ("test#05", true, Cols.Contains ("test2"));
			Assertion.AssertEquals ("test#06", false, Cols.Contains ("_test2"));
			Assertion.AssertEquals ("test#07", true, Cols.Contains ("TEST2"));
		}

		[Test]
		public void CopyTo ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			Cols.Add ("test4");

			DataColumn [] array = new DataColumn [4];
			Cols.CopyTo (array, 0);
			Assertion.AssertEquals ("test#01", 4, array.Length);
			Assertion.AssertEquals ("test#02", "test", array [0].ColumnName);
			Assertion.AssertEquals ("test#03", "test2", array [1].ColumnName);
			Assertion.AssertEquals ("test#04", "test3", array [2].ColumnName);
			Assertion.AssertEquals ("test#05", "test4", array [3].ColumnName);

			array = new DataColumn [6];
			Cols.CopyTo (array, 2);
			Assertion.AssertEquals ("test#06", 6, array.Length);
			Assertion.AssertEquals ("test#07", "test", array [2].ColumnName);
			Assertion.AssertEquals ("test#08", "test2", array [3].ColumnName);
			Assertion.AssertEquals ("test#09", "test3", array [4].ColumnName);
			Assertion.AssertEquals ("test#10", "test4", array [5].ColumnName);
			Assertion.AssertEquals ("test#11", null, array [0]);
			Assertion.AssertEquals ("test#12", null, array [1]);
		}

		[Test]
		public void Equals ()
		{
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;
			DataColumnCollection Cols2 = Table2.Columns;

			Assertion.AssertEquals ("test#01", false, Cols.Equals (Cols2));
			Assertion.AssertEquals ("test#02", false, Cols2.Equals (Cols));
			Assertion.AssertEquals ("test#03", false, Object.Equals (Cols, Cols2));
			Assertion.AssertEquals ("test#04", true, Cols.Equals (Cols));
			Assertion.AssertEquals ("test#05", true, Cols2.Equals (Cols2));
			Assertion.AssertEquals ("test#06", true, Object.Equals (Cols2, Cols2));
		}

		[Test]
		public void IndexOf ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			Cols.Add ("test4");

			Assertion.AssertEquals ("test#01", 0, Cols.IndexOf ("test"));
			Assertion.AssertEquals ("test#02", 1, Cols.IndexOf ("TEST2"));
			Table.CaseSensitive = true;
			Assertion.AssertEquals ("test#03", 1, Cols.IndexOf ("TEST2"));

			Assertion.AssertEquals ("test#04", 3, Cols.IndexOf (Cols [3]));
			DataColumn C = new DataColumn ("error");
			Assertion.AssertEquals ("test#05", -1, Cols.IndexOf (C));
			Assertion.AssertEquals ("test#06", -1, Cols.IndexOf ("_error_"));
		}

		[Test]
		public void Remove ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			Cols.Add ("test4");

			Assertion.AssertEquals ("test#01", 4, Cols.Count);
			Cols.Remove ("test2");
			Assertion.AssertEquals ("test#02", 3, Cols.Count);
			Cols.Remove ("TEST3");
			Assertion.AssertEquals ("test#03", 2, Cols.Count);

			try {
				Cols.Remove ("_test_");
				Assertion.Fail ("test#04");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#05", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#06", "Column '_test_' does not belong to table test_table.", e.Message);
			}

			Cols.Add ();
			Cols.Add ();
			Cols.Add ();
			Cols.Add ();

			Assertion.AssertEquals ("test#07", 6, Cols.Count);
			Cols.Remove (Cols [0]);
			Cols.Remove (Cols [0]);
			Assertion.AssertEquals ("test#08", 4, Cols.Count);
			Assertion.AssertEquals ("test#09", "Column1", Cols [0].ColumnName);

			try {
				Cols.Remove (new DataColumn ("Column1"));
				Assertion.Fail ("test#10");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#11", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#12", "Cannot remove a column that doesn't belong to this table.", e.Message);
			}

			Cols.Add ();
			Cols.Add ();
			Cols.Add ();
			Cols.Add ();

			Assertion.AssertEquals ("test#13", 8, Cols.Count);
			Cols.RemoveAt (7);
			Cols.RemoveAt (1);
			Cols.RemoveAt (0);
			Cols.RemoveAt (0);
			Assertion.AssertEquals ("test#14", 4, Cols.Count);
			Assertion.AssertEquals ("test#15", "Column4", Cols [0].ColumnName);
			Assertion.AssertEquals ("test#16", "Column5", Cols [1].ColumnName);

			try {
				Cols.RemoveAt (10);
				Assertion.Fail ("test#17");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#18", typeof (IndexOutOfRangeException), e.GetType ());
				Assertion.AssertEquals ("test#19", "Cannot find column 10.", e.Message);
			}
		}

		[Test]
		public void ToStringTest ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			Assertion.AssertEquals ("test#01", "System.Data.DataColumnCollection", Cols.ToString ());
		}
	}
}
