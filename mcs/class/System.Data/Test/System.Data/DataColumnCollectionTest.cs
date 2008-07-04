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
using System.Xml;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataColumnCollectionTest
	{
		//private DataTable _tbl;

		[SetUp]
		public void GetReady () 
		{
			//_tbl = new DataTable();
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
			Assert.IsTrue (C.AllowDBNull, "test#01");
			Assert.IsFalse (C.AutoIncrement, "test#02");
			Assert.AreEqual (0L, C.AutoIncrementSeed, "test#03");
			Assert.AreEqual (1L, C.AutoIncrementStep, "test#04");
			Assert.AreEqual ("Column1", C.Caption, "test#05");
			Assert.AreEqual ("Element", C.ColumnMapping.ToString (), "test#06");
			Assert.AreEqual ("Column1", C.ColumnName, "test#07");
			Assert.IsNull (C.Container, "test#08");
			Assert.AreEqual (typeof (string), C.DataType, "test#09");
			Assert.AreEqual (DBNull.Value, C.DefaultValue, "test#10");
			Assert.IsFalse (C.DesignMode, "test#11");
			Assert.AreEqual ("", C.Expression, "test#12");
			Assert.AreEqual (0, C.ExtendedProperties.Count, "test#13");
			Assert.AreEqual (-1, C.MaxLength, "test#14");
			Assert.AreEqual ("", C.Namespace, "test#15");
			Assert.AreEqual (0, C.Ordinal, "test#16");
			Assert.AreEqual ("", C.Prefix, "test#17");
			Assert.IsFalse (C.ReadOnly, "test#18");
			Assert.IsNull (C.Site, "test#19");
			Assert.AreEqual ("test_table", C.Table.TableName, "test#20");
			Assert.AreEqual ("Column1", C.ToString (), "test#21");
			Assert.IsFalse (C.Unique, "test#22");

			C = Cols [1];
			Assert.IsTrue (C.AllowDBNull, "test#23");
			Assert.IsFalse (C.AutoIncrement, "test#24");
			Assert.AreEqual (0L, C.AutoIncrementSeed, "test#25");
			Assert.AreEqual (1L, C.AutoIncrementStep, "test#26");
			Assert.AreEqual ("Column2", C.Caption, "test#27");
			Assert.AreEqual ("Element", C.ColumnMapping.ToString (), "test#28");
			Assert.AreEqual ("Column2", C.ColumnName, "test#29");
			Assert.IsNull (C.Container, "test#30");
			Assert.AreEqual (typeof (string), C.DataType, "test#31");
			Assert.AreEqual (DBNull.Value, C.DefaultValue, "test#32");
			Assert.IsFalse (C.DesignMode, "test#33");
			Assert.AreEqual ("", C.Expression, "test#34");
			Assert.AreEqual (0, C.ExtendedProperties.Count, "test#35");
			Assert.AreEqual (-1, C.MaxLength, "test#36");
			Assert.AreEqual ("", C.Namespace, "test#37");
			Assert.AreEqual (1, C.Ordinal, "test#38");
			Assert.AreEqual ("", C.Prefix, "test#39");
			Assert.IsFalse (C.ReadOnly, "test#40");
			Assert.IsNull (C.Site, "test#41");
			Assert.AreEqual ("test_table", C.Table.TableName, "test#42");
			Assert.AreEqual ("Column2", C.ToString (), "test#43");
			Assert.IsFalse (C.Unique, "test#44");

			Cols.Add ("test1", typeof (int), "");
			Cols.Add ("test2", typeof (string), "Column1 + Column2");

			C = Cols [2];
			Assert.IsTrue (C.AllowDBNull, "test#45");
			Assert.IsFalse (C.AutoIncrement, "test#46");
			Assert.AreEqual (0L, C.AutoIncrementSeed, "test#47");
			Assert.AreEqual (1L, C.AutoIncrementStep, "test#48");
			Assert.AreEqual ("test1", C.Caption, "test#49");
			Assert.AreEqual ("Element", C.ColumnMapping.ToString (), "test#50");
			Assert.AreEqual ("test1", C.ColumnName, "test#51");
			Assert.IsNull (C.Container, "test#52");
			Assert.AreEqual (typeof (int), C.DataType, "test#53");
			Assert.AreEqual (DBNull.Value, C.DefaultValue, "test#54");
			Assert.IsFalse (C.DesignMode, "test#55");
			Assert.AreEqual ("", C.Expression, "test#56");
			Assert.AreEqual (0, C.ExtendedProperties.Count, "test#57");
			Assert.AreEqual (-1, C.MaxLength, "test#58");
			Assert.AreEqual ("", C.Namespace, "test#59");
			Assert.AreEqual (2, C.Ordinal, "test#60");
			Assert.AreEqual ("", C.Prefix, "test#61");
			Assert.IsFalse (C.ReadOnly, "test#62");
			Assert.IsNull (C.Site, "test#63");
			Assert.AreEqual ("test_table", C.Table.TableName, "test#64");
			Assert.AreEqual ("test1", C.ToString (), "test#65");
			Assert.IsFalse (C.Unique, "test#66");

			C = Cols [3];
			Assert.IsTrue (C.AllowDBNull, "test#67");
			Assert.IsFalse (C.AutoIncrement, "test#68");
			Assert.AreEqual (0L, C.AutoIncrementSeed, "test#69");
			Assert.AreEqual (1L, C.AutoIncrementStep, "test#70");
			Assert.AreEqual ("test2", C.Caption, "test#71");
			Assert.AreEqual ("Element", C.ColumnMapping.ToString (), "test#72");
			Assert.AreEqual ("test2", C.ColumnName, "test#73");
			Assert.IsNull (C.Container, "test#74");
			Assert.AreEqual (typeof (string), C.DataType, "test#75");
			Assert.AreEqual (DBNull.Value, C.DefaultValue, "test#76");
			Assert.IsFalse (C.DesignMode, "test#77");
			Assert.AreEqual ("Column1 + Column2", C.Expression, "test#78");
			Assert.AreEqual (0, C.ExtendedProperties.Count, "test#79");
			Assert.AreEqual (-1, C.MaxLength, "test#80");
			Assert.AreEqual ("", C.Namespace, "test#81");
			Assert.AreEqual (3, C.Ordinal, "test#82");
			Assert.AreEqual ("", C.Prefix, "test#83");
			Assert.IsTrue (C.ReadOnly, "test#84");
			Assert.IsNull (C.Site, "test#85");
			Assert.AreEqual ("test_table", C.Table.TableName, "test#86");
			Assert.AreEqual ("test2 + Column1 + Column2", C.ToString (), "test#87");
			Assert.IsFalse (C.Unique, "test#88"); 

			C = new DataColumn ("test3", typeof (int));
			Cols.Add (C);

			C = Cols [4];
			Assert.IsTrue (C.AllowDBNull, "test#89");
			Assert.IsFalse (C.AutoIncrement, "test#90");
			Assert.AreEqual (0L, C.AutoIncrementSeed, "test#91");
			Assert.AreEqual (1L, C.AutoIncrementStep, "test#92");
			Assert.AreEqual ("test3", C.Caption, "test#93");
			Assert.AreEqual ("Element", C.ColumnMapping.ToString (), "test#94");
			Assert.AreEqual ("test3", C.ColumnName, "test#95");
			Assert.IsNull (C.Container, "test#96");
			Assert.AreEqual (typeof (int), C.DataType, "test#97");
			Assert.AreEqual (DBNull.Value, C.DefaultValue, "test#98");
			Assert.IsFalse (C.DesignMode, "test#99");
			Assert.AreEqual ("", C.Expression, "test#100");
			Assert.AreEqual (0, C.ExtendedProperties.Count, "test#101");
			Assert.AreEqual (-1, C.MaxLength, "test#102");
			Assert.AreEqual ("", C.Namespace, "test#103");
			Assert.AreEqual (4, C.Ordinal, "test#104");
			Assert.AreEqual ("", C.Prefix, "test#105");
			Assert.IsFalse (C.ReadOnly, "test#106");
			Assert.IsNull (C.Site, "test#107");
			Assert.AreEqual ("test_table", C.Table.TableName, "test#108");
			Assert.AreEqual ("test3", C.ToString (), "test#109");
			Assert.IsFalse (C.Unique, "test#110"); 
		}

		[Test] // Add (String)
		public void Add3_ColumnName_Empty ()
		{
			DataTable table = new DataTable ();
			DataColumnCollection cols = table.Columns;
			DataColumn col;

			col = cols.Add (string.Empty);
			Assert.AreEqual (1, cols.Count, "#A1");
			Assert.AreEqual ("Column1", col.ColumnName, "#A2");
			Assert.AreSame (table, col.Table, "#A3");

			col = cols.Add (string.Empty);
			Assert.AreEqual (2, cols.Count, "#B1");
			Assert.AreEqual ("Column2", col.ColumnName, "#B2");
			Assert.AreSame (table, col.Table, "#B3");

			cols.RemoveAt (1);

			col = cols.Add (string.Empty);
			Assert.AreEqual (2, cols.Count, "#C1");
			Assert.AreEqual ("Column2", col.ColumnName, "#C2");
			Assert.AreSame (table, col.Table, "#C3");

			cols.Clear ();

			col = cols.Add (string.Empty);
			Assert.AreEqual (1, cols.Count, "#D1");
			Assert.AreEqual ("Column1", col.ColumnName, "#D2");
			Assert.AreSame (table, col.Table, "#D3");
		}

		[Test] // Add (String)
		public void Add3_ColumnName_Null ()
		{
			DataTable table = new DataTable ();
			DataColumnCollection cols = table.Columns;
			DataColumn col;
			
			col = cols.Add ((string) null);
			Assert.AreEqual (1, cols.Count, "#A1");
			Assert.AreEqual ("Column1", col.ColumnName, "#A2");
			Assert.AreSame (table, col.Table, "#A3");

			col = cols.Add ((string) null);
			Assert.AreEqual (2, cols.Count, "#B1");
			Assert.AreEqual ("Column2", col.ColumnName, "#B2");
			Assert.AreSame (table, col.Table, "#B3");

			cols.RemoveAt (1);

			col = cols.Add ((string) null);
			Assert.AreEqual (2, cols.Count, "#C1");
			Assert.AreEqual ("Column2", col.ColumnName, "#C2");
			Assert.AreSame (table, col.Table, "#C3");

			cols.Clear ();

			col = cols.Add ((string) null);
			Assert.AreEqual (1, cols.Count, "#D1");
			Assert.AreEqual ("Column1", col.ColumnName, "#D2");
			Assert.AreSame (table, col.Table, "#D3");
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
				Assert.Fail ("test#01");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "test#02");
			}

			C = new DataColumn ("test");
			Cols.Add (C);

			try {
				Cols.Add (C);
				Assert.Fail ("test#04");
			} catch (ArgumentException e) {
//				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#05");
//				Assert.AreEqual ("Column 'test' already belongs to this or another DataTable.", e.Message, "test#06");
			}

			try {
				Table2.Columns.Add (C);
				Assert.Fail ("test#07");
			} catch (ArgumentException e) {
//				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#08");
//				Assert.AreEqual ("Column 'test' already belongs to this or another DataTable.", e.Message, "test#09");
			}

			DataColumn C2 = new DataColumn ("test");

			try {
				Cols.Add (C2);
				Assert.Fail ("test#10");
			} catch (DuplicateNameException e) {
//				Assert.AreEqual (typeof (DuplicateNameException), e.GetType (), "test#11");
//				Assert.AreEqual ("A DataColumn named 'test' already belongs to this DataTable.", e.Message, "test#12");
			}

			try {
				Cols.Add ("test2", typeof (string), "substring ('fdsafewq', 2)");
				Assert.Fail ("test#13");
			} catch (InvalidExpressionException e) {
//				Assert.IsTrue (e is InvalidExpressionException, "test#14");
//				Assert.AreEqual ("Expression 'substring ('fdsafewq', 2)' is invalid.", e.Message, "test#15");
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
			Assert.IsTrue (C.AllowDBNull, "test#01");
			Assert.IsFalse (C.AutoIncrement, "test#02");
			Assert.AreEqual (0L, C.AutoIncrementSeed, "test#03");
			Assert.AreEqual (1L, C.AutoIncrementStep, "test#04");
			Assert.AreEqual ("test1", C.Caption, "test#05");
			Assert.AreEqual ("Element", C.ColumnMapping.ToString (), "test#06");
			Assert.AreEqual ("test1", C.ColumnName, "test#07");
			Assert.IsNull (C.Container, "test#08");
			Assert.AreEqual (typeof (string), C.DataType, "test#09");
			Assert.AreEqual (DBNull.Value, C.DefaultValue, "test#10");
			Assert.IsFalse (C.DesignMode, "test#11");
			Assert.AreEqual ("", C.Expression, "test#12");
			Assert.AreEqual (0, C.ExtendedProperties.Count, "test#13");
			Assert.AreEqual (-1, C.MaxLength, "test#14");
			Assert.AreEqual ("", C.Namespace, "test#15");
			Assert.AreEqual (0, C.Ordinal, "test#16");
			Assert.AreEqual ("", C.Prefix, "test#17");
			Assert.IsFalse (C.ReadOnly, "test#18");
			Assert.IsNull (C.Site, "test#19");
			Assert.AreEqual ("test_table", C.Table.TableName, "test#20");
			Assert.AreEqual ("test1", C.ToString (), "test#21");
			Assert.IsFalse (C.Unique, "test#22");

			C = Cols [1];
			Assert.IsFalse (C.AllowDBNull, "test#01");
			Assert.IsFalse (C.AutoIncrement, "test#02");
			Assert.AreEqual (0L, C.AutoIncrementSeed, "test#03");
			Assert.AreEqual (1L, C.AutoIncrementStep, "test#04");
			Assert.AreEqual ("Test_caption", C.Caption, "test#05");
			Assert.AreEqual ("Element", C.ColumnMapping.ToString (), "test#06");
			Assert.AreEqual ("test2", C.ColumnName, "test#07");
			Assert.IsNull (C.Container, "test#08");
			Assert.AreEqual (typeof (XmlReader), C.DataType, "test#09");
			Assert.AreEqual (DBNull.Value, C.DefaultValue, "test#10");
			Assert.IsFalse (C.DesignMode, "test#11");
			Assert.AreEqual ("", C.Expression, "test#12");
			Assert.AreEqual (0, C.ExtendedProperties.Count, "test#13");
			Assert.AreEqual (-1, C.MaxLength, "test#14");
			Assert.AreEqual ("", C.Namespace, "test#15");
			Assert.AreEqual (1, C.Ordinal, "test#16");
			Assert.AreEqual ("", C.Prefix, "test#17");
			Assert.IsFalse (C.ReadOnly, "test#18");
			Assert.IsNull (C.Site, "test#19");
			Assert.AreEqual ("test_table", C.Table.TableName, "test#20");
			Assert.AreEqual ("test2", C.ToString (), "test#21");
			Assert.IsFalse (C.Unique, "test#22");
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
			Assert.IsFalse (Cols.CanRemove (C), "test#01");

			Cols.Add (C);
			Assert.IsTrue (Cols.CanRemove (C), "test#02");

			C = new DataColumn ();
			C.Expression = "test1 + 2";
			Cols.Add (C);

			C = Cols ["test2"];
			Assert.IsFalse (Cols.CanRemove (C), "test#03");

			C = new DataColumn ("t");
			Table2.Columns.Add (C);
			DataColumnCollection Cols2 = Table2.Columns;
			Assert.IsTrue (Cols2.CanRemove (C), "test#04");

			DataRelation Rel = new DataRelation ("Rel", Table.Columns [0], Table2.Columns [0]);
			DataSet Set = new DataSet ();
			Set.Tables.Add (Table);
			Set.Tables.Add (Table2);
			Set.Relations.Add (Rel);

			Assert.IsFalse (Cols2.CanRemove (C), "test#05");
			Assert.IsFalse (Cols.CanRemove (null), "test#06");
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
			Assert.AreEqual (0, Cols.Count, "test#01");

			Cols.Add ();
			Cols.Add ("testi");
			Cols2.Add ();
			Cols2.Add ();

			DataRelation Rel = new DataRelation ("Rel", Cols [0], Cols2 [0]);
			Set.Relations.Add (Rel);
			try {
				Cols.Clear ();
				Assert.Fail ("test#02");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#03");
				// Never premise English.
				//Assert.AreEqual ("Cannot remove this column, because it is part of the parent key for relationship Rel.", e.Message, "test#04");
			}
		}

		[Test]
		public void Clear_ExpressionColumn ()
		{
			DataTable table = new DataTable ("test");
			table.Columns.Add ("col1", typeof(int));
			table.Columns.Add ("col2", typeof (int), "sum(col1)");

			//shudnt throw an exception.
			table.Columns.Clear ();
			Assert.AreEqual (0, table.Columns.Count, "#1");
		}

		[Test]
		public void Contains ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("tesT2");

			Assert.IsTrue (Cols.Contains ("test"), "test#01");
			Assert.IsFalse (Cols.Contains ("_test"), "test#02");
			Assert.IsTrue (Cols.Contains ("TEST"), "test#03");
			Table.CaseSensitive = true;
			Assert.IsTrue (Cols.Contains ("TEST"), "test#04");
			Assert.IsTrue (Cols.Contains ("test2"), "test#05");
			Assert.IsFalse (Cols.Contains ("_test2"), "test#06");
			Assert.IsTrue (Cols.Contains ("TEST2"), "test#07");
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
			Assert.AreEqual (4, array.Length, "test#01");
			Assert.AreEqual ("test", array [0].ColumnName, "test#02");
			Assert.AreEqual ("test2", array [1].ColumnName, "test#03");
			Assert.AreEqual ("test3", array [2].ColumnName, "test#04");
			Assert.AreEqual ("test4", array [3].ColumnName, "test#05");

			array = new DataColumn [6];
			Cols.CopyTo (array, 2);
			Assert.AreEqual (6, array.Length, "test#06");
			Assert.AreEqual ("test", array [2].ColumnName, "test#07");
			Assert.AreEqual ("test2", array [3].ColumnName, "test#08");
			Assert.AreEqual ("test3", array [4].ColumnName, "test#09");
			Assert.AreEqual ("test4", array [5].ColumnName, "test#10");
			Assert.IsNull (array [0], "test#11");
			Assert.IsNull (array [1], "test#12");
		}

		[Test]
		public void Equals ()
		{
			DataTable Table = new DataTable ("test_table");
			DataTable Table2 = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;
			DataColumnCollection Cols2 = Table2.Columns;

			Assert.IsFalse (Cols.Equals (Cols2), "test#01");
			Assert.IsFalse (Cols2.Equals (Cols), "test#02");
			Assert.IsFalse (Object.Equals (Cols, Cols2), "test#03");
			Assert.IsTrue (Cols.Equals (Cols), "test#04");
			Assert.IsTrue (Cols2.Equals (Cols2), "test#05");
			Assert.IsTrue (Object.Equals (Cols2, Cols2), "test#06");
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

			Assert.AreEqual (0, Cols.IndexOf ("test"), "test#01");
			Assert.AreEqual (1, Cols.IndexOf ("TEST2"), "test#02");
			Table.CaseSensitive = true;
			Assert.AreEqual (1, Cols.IndexOf ("TEST2"), "test#03");

			Assert.AreEqual (3, Cols.IndexOf (Cols [3]), "test#04");
			DataColumn C = new DataColumn ("error");
			Assert.AreEqual (-1, Cols.IndexOf (C), "test#05");
			Assert.AreEqual (-1, Cols.IndexOf ("_error_"), "test#06");
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

			Assert.AreEqual (4, Cols.Count, "test#01");
			Cols.Remove ("test2");
			Assert.AreEqual (3, Cols.Count, "test#02");
			Cols.Remove ("TEST3");
			Assert.AreEqual (2, Cols.Count, "test#03");

			try {
				Cols.Remove ("_test_");
				Assert.Fail ("test#04");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#05");
				// Never premise English.
				//Assert.AreEqual ("Column '_test_' does not belong to table test_table.", e.Message, "test#06");
			}

			Cols.Add ();
			Cols.Add ();
			Cols.Add ();
			Cols.Add ();

			Assert.AreEqual (6, Cols.Count, "test#07");
			Cols.Remove (Cols [0]);
			Cols.Remove (Cols [0]);
			Assert.AreEqual (4, Cols.Count, "test#08");
			Assert.AreEqual ("Column1", Cols [0].ColumnName, "test#09");

			try {
				Cols.Remove (new DataColumn ("Column10"));
				Assert.Fail ("test#10");
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#11");
				// Never premise English.
				//Assert.AreEqual ("Cannot remove a column that doesn't belong to this table.", e.Message, "test#12");
			}

			Cols.Add ();
			Cols.Add ();
			Cols.Add ();
			Cols.Add ();

			Assert.AreEqual (8, Cols.Count, "test#13");
			Cols.RemoveAt (7);
			Cols.RemoveAt (1);
			Cols.RemoveAt (0);
			Cols.RemoveAt (0);
			Assert.AreEqual (4, Cols.Count, "test#14");
			Assert.AreEqual ("Column4", Cols [0].ColumnName, "test#15");
			Assert.AreEqual ("Column5", Cols [1].ColumnName, "test#16");

			try {
				Cols.RemoveAt (10);
				Assert.Fail ("test#17");
			} catch (Exception e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "test#18");
				// Never premise English.
				//Assert.AreEqual ("Cannot find column 10.", e.Message, "test#19");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_Dep_Rel_Col ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add ("test");
			ds.Tables.Add ("test1");
			ds.Tables[0].Columns.Add ("col1", typeof(int));
			ds.Tables[1].Columns.Add ("col2", typeof(int));

			ds.Relations.Add ("rel1",  ds.Tables[0].Columns[0], ds.Tables[1].Columns[0]);
			ds.Tables[0].Columns.RemoveAt (0);
		}	

		[Test]
#if TARGET_JVM
		[Ignore ("Does not work with TARGET_JVM")]
#endif
		public void ToStringTest ()
		{
			DataTable Table = new DataTable ("test_table");
			DataColumnCollection Cols = Table.Columns;

			Cols.Add ("test");
			Cols.Add ("test2");
			Cols.Add ("test3");
			Assert.AreEqual ("System.Data.DataColumnCollection", Cols.ToString (), "test#01");
		}
		
		[Test]
		public void CaseSensitiveIndexOfTest ()
		{
			DataTable dt = new DataTable ("TestCaseSensitiveIndexOf");
			dt.Columns.Add ("nom_colonne1", typeof (string));
			dt.Columns.Add ("NOM_COLONNE1", typeof (string));
			dt.Columns.Remove ("nom_colonne1");
			int i=dt.Columns.IndexOf ("nom_colonne1"); 
			Assert.AreEqual (0, dt.Columns.IndexOf ("nom_colonne1"));
		}
	}
}
