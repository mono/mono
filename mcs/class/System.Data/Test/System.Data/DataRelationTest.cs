//
// DataRelationTest.cs - NUnit Test Cases for  DataRelation
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Ville Palo
// (C) 2003 Martin Willemoes Hansen
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
#if USE_MSUNITTEST
#if WINDOWS_PHONE || NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
#else // !WINDOWS_PHONE && !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute;
#endif // WINDOWS_PHONE || NETFX_CORE
#else // !USE_MSUNITTEST
using NUnit.Framework;
#endif // USE_MSUNITTEST
using System;
using System.Data;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataRelationTest
	{
		private DataSet Set = null;
		private DataTable Mom = null;
		private DataTable Child = null;        	

		[SetUp]
		public void GetReady ()
		{
			Set = new DataSet ();
			Mom = new DataTable ("Mom");
			Child = new DataTable ("Child");
			Set.Tables.Add (Mom);
			Set.Tables.Add (Child);
			
			DataColumn Col = new DataColumn ("Name");
			DataColumn Col2 = new DataColumn ("ChildName");
			Mom.Columns.Add (Col);
			Mom.Columns.Add (Col2);
			
			DataColumn Col3 = new DataColumn ("Name");
			DataColumn Col4 = new DataColumn ("Age");
			Col4.DataType = Type.GetType ("System.Int16");
			Child.Columns.Add (Col3);
			Child.Columns.Add (Col4);
		}  

		[Test]
		public void Foreign ()
		{
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);

			DataRow Row = Mom.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Jack";
			Mom.Rows.Add (Row);
                	
			Row = Mom.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Dick";
			Mom.Rows.Add (Row);
                	
			Row = Mom.NewRow ();
			Row [0] = "Mary";
			Row [1] = "Harry";
                	
			Row = Child.NewRow ();
			Row [0] = "Jack";
			Row [1] = 16;
			Child.Rows.Add (Row);
                	
			Row = Child.NewRow ();
			Row [0] = "Dick";
			Row [1] = 56;
			Child.Rows.Add (Row);
                	
			Assert.AreEqual (2, Child.Rows.Count, "test#01");
                	
			Row = Mom.Rows [0];
			Row.Delete ();
                	
			Assert.AreEqual (1, Child.Rows.Count, "test#02");
                	
			Row = Mom.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Dick";
                	
			try {
				Mom.Rows.Add (Row);
				Assert.Fail ("test#03");
			} catch (Exception e) {
				AssertHelpers.AssertIsInstanceOfType<ConstraintException> (e, "test#04");
				// Never premise English.
				//Assert.That (e.Message, Is.EqualTo("Column 'ChildName' is constrained to be unique.  Value 'Dick' is already present."), "test#05");
			}                	

			Row = Mom.NewRow ();                                 
			Row [0] = "Teresa";                                  
			Row [1] = "Mich";                                    
			Mom.Rows.Add (Row);                                  
			Assert.AreEqual (1, Child.Rows.Count, "test#06");
			
			Row = Child.NewRow ();                               
			Row [0] = "Jack";                                    
			Row [1] = 16;                                        
			
			try {                                                
				Child.Rows.Add (Row);                               
				Assert.Fail ("test#07");
			} catch (Exception e) {                              
				AssertHelpers.AssertIsInstanceOfType<InvalidConstraintException> (e, "test#08");
			}                                                    
		}

		[Test]
		public void InvalidConstraintException ()
		{
			// Parent Columns and Child Columns don't have type-matching columns.
			AssertHelpers.AssertThrowsException<InvalidConstraintException>(() => {
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [1], true);
			});
		}

		[Test]
		public void InvalidConstraintException2 ()
		{
			// Parent Columns and Child Columns don't have type-matching columns.
			Child.Columns [1].DataType = Mom.Columns [1].DataType;
			
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [1], true);
			Set.Relations.Add (Relation);
			Assert.AreEqual (1, Set.Relations.Count, "test#01");
			
			AssertHelpers.AssertThrowsException<InvalidConstraintException>(() => {
			Child.Columns [1].DataType = Type.GetType ("System.Double");
			});
		}

		[Test]
		public void DataSetRelations ()
		{
			DataRelation Relation;
			Assert.AreEqual (0, Set.Relations.Count, "test#01");
			Assert.AreEqual (0, Mom.ParentRelations.Count, "test#02");
			Assert.AreEqual (0, Mom.ChildRelations.Count, "test#03");
			Assert.AreEqual (0, Child.ParentRelations.Count, "test#04");
			Assert.AreEqual (0, Child.ChildRelations.Count, "test#05");
			
			Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			Assert.AreEqual (1, Set.Relations.Count, "test#06");
			Assert.AreEqual (0, Mom.ParentRelations.Count, "test#07");
			Assert.AreEqual (1, Mom.ChildRelations.Count, "test#08");
			Assert.AreEqual (1, Child.ParentRelations.Count, "test#09");
			Assert.AreEqual (0, Child.ChildRelations.Count, "test#10");
						
			Relation = Set.Relations [0];
			Assert.AreEqual (1, Relation.ParentColumns.Length, "test#11");
			Assert.AreEqual (1, Relation.ChildColumns.Length, "test#12");
			Assert.AreEqual ("Rel", Relation.ChildKeyConstraint.ConstraintName, "test#13");
			Assert.AreEqual ("Constraint1", Relation.ParentKeyConstraint.ConstraintName, "test#14");
		}

		[Test]
		public void Constraints ()
		{
			Assert.AreEqual (0, Mom.Constraints.Count, "test#01");
			Assert.AreEqual (0, Child.Constraints.Count, "test#02");

			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			Assert.AreEqual (1, Mom.Constraints.Count, "test#03");
			Assert.AreEqual (1, Child.Constraints.Count, "test#04");
			AssertHelpers.AssertIsInstanceOfType (Child.Constraints [0], typeof(ForeignKeyConstraint), "test#05");
			AssertHelpers.AssertIsInstanceOfType (Mom.Constraints [0], typeof(UniqueConstraint), "test#06");
		}

		[Test]
		public void Creation ()
		{
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			DataRelation Test = null;
			Assert.AreEqual (1, Mom.ChildRelations.Count, "test#01");
			Assert.AreEqual (0, Child.ChildRelations.Count, "test#02");
			Assert.AreEqual (0, Mom.ParentRelations.Count, "test#03");
			Assert.AreEqual (1, Child.ParentRelations.Count, "test#04");
				
			Test = Child.ParentRelations [0];
			Assert.AreEqual ("Rel", Test.ToString (), "test#05");
			Assert.AreEqual ("Rel", Test.RelationName, "test#06");
			Assert.AreEqual ("Mom", Test.ParentTable.TableName, "test#07");
			Assert.AreEqual (1, Test.ParentKeyConstraint.Columns.Length, "test#08");
			Assert.IsFalse (Test.ParentKeyConstraint.IsPrimaryKey, "test#09");
			Assert.AreEqual (1, Test.ParentColumns.Length, "test#10");
			Assert.IsFalse (Test.Nested, "test#11");
			Assert.AreEqual (0, Test.ExtendedProperties.Count, "test#12");
			Assert.AreEqual ("Child", Test.ChildTable.TableName, "test#13");
			Assert.AreEqual ("Rel", Test.ChildKeyConstraint.ConstraintName, "test#14");
			Assert.AreEqual (1, Test.ChildColumns.Length, "test#15");
		}

		[Test]
		public void Creation2 ()
		{
			DataSet Set = new DataSet ();
			DataTable Mom2 = new DataTable ("Mom");
			DataTable Child2 = new DataTable ("Child");
			DataTable Hubby = new DataTable ("Hubby");
			Set.Tables.Add (Mom2);
			Set.Tables.Add (Child2);
			Set.Tables.Add (Hubby);
						
			DataColumn Col = new DataColumn ("Name");
			DataColumn Col2 = new DataColumn ("ChildName");
			DataColumn Col3 = new DataColumn ("hubby");
			Mom2.Columns.Add (Col);
			Mom2.Columns.Add (Col2);
			Mom2.Columns.Add (Col3);
			
			DataColumn Col4 = new DataColumn ("Name");
			DataColumn Col5 = new DataColumn ("Age");
			DataColumn Col6 = new DataColumn ("father");
			Child2.Columns.Add (Col4);
			Child2.Columns.Add (Col5);
			Child2.Columns.Add (Col6);
			
			
			DataColumn Col7 = new DataColumn ("Name");
			DataColumn Col8 = new DataColumn ("Age");
			Hubby.Columns.Add (Col7);
			Hubby.Columns.Add (Col8);
			
			DataColumn [] Parents = new DataColumn [2];
			Parents [0] = Col2;
			Parents [1] = Col3;
			DataColumn [] Childs = new DataColumn [2];
			Childs [0] = Col4;
			Childs [1] = Col7;
			
			DataRelation Relation = null;
			try {
				Relation = new DataRelation ("Rel", Parents, Childs);
				Assert.Fail ("test#01");
			} catch (InvalidConstraintException e) {
//				Assert.That (e.GetType (), Is.EqualTo(typeof (InvalidConstraintException)), "test#02");				
//				Assert.That (e.Message, Is.EqualTo("Cannot create a Key from Columns that belong to different tables."), "test#03");
			}
			
			Childs [1] = Col6;
			Relation = new DataRelation ("Rel", Parents, Childs);
			
			Set.Relations.Add (Relation);
			
			DataRelation Test = null;
			Assert.AreEqual (1, Mom2.ChildRelations.Count, "test#01");
			Assert.AreEqual (0, Child2.ChildRelations.Count, "test#02");
			Assert.AreEqual (0, Mom2.ParentRelations.Count, "test#03");
			Assert.AreEqual (1, Child2.ParentRelations.Count, "test#04");
				
			Test = Child2.ParentRelations [0];
			Assert.AreEqual ("Rel", Test.ToString (), "test#05");
			Assert.AreEqual ("Rel", Test.RelationName, "test#06");
			Assert.AreEqual ("Mom", Test.ParentTable.TableName, "test#07");
			Assert.AreEqual (2, Test.ParentKeyConstraint.Columns.Length, "test#08");
			Assert.IsFalse (Test.ParentKeyConstraint.IsPrimaryKey, "test#09");
			Assert.AreEqual (2, Test.ParentColumns.Length, "test#10");
			Assert.IsFalse (Test.Nested, "test#11");
			Assert.AreEqual (0, Test.ExtendedProperties.Count, "test#12");
			Assert.AreEqual ("Child", Test.ChildTable.TableName, "test#13");
			Assert.AreEqual ("Rel", Test.ChildKeyConstraint.ConstraintName, "test#14");
			Assert.AreEqual (2, Test.ChildColumns.Length, "test#15");
			Assert.AreEqual (1, Mom2.Constraints.Count, "test#16");
			Assert.AreEqual ("Constraint1", Mom2.Constraints [0].ToString (), "test#17");
			Assert.AreEqual (1, Child2.Constraints.Count, "test#18");
			Assert.AreEqual (0, Hubby.Constraints.Count, "test#19");
		}

		[Test]
		public void Creation3 ()
		{
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0], false);
			Set.Relations.Add (Relation);
			DataRelation Test = null;
	
			Assert.AreEqual (1, Mom.ChildRelations.Count, "test#01");
			Assert.AreEqual (0, Child.ChildRelations.Count, "test#02");
			Assert.AreEqual (0, Mom.ParentRelations.Count, "test#03");
			Assert.AreEqual (1, Child.ParentRelations.Count, "test#04");

			Test = Child.ParentRelations [0];
			
			Assert.AreEqual ("Rel", Test.ToString (), "test#05");

			Assert.AreEqual ("Rel", Test.RelationName, "test#06");
			Assert.AreEqual ("Mom", Test.ParentTable.TableName, "test#07");

			Assert.IsNull (Test.ParentKeyConstraint, "test#08");

			Assert.IsNull (Test.ParentKeyConstraint, "test#09");

			Assert.AreEqual (1, Test.ParentColumns.Length, "test#10");
			Assert.IsFalse (Test.Nested, "test#11");
			Assert.AreEqual (0, Test.ExtendedProperties.Count, "test#12");
			Assert.AreEqual ("Child", Test.ChildTable.TableName, "test#13");

			Assert.IsNull (Test.ChildKeyConstraint, "test#14");
			Assert.AreEqual (1, Test.ChildColumns.Length, "test#15");
			Assert.AreEqual (0, Mom.Constraints.Count, "test#16");
			Assert.AreEqual (0, Child.Constraints.Count, "test#17");
		}

		[Test]
		public void Creation4 ()
		{
			DataRelation Relation = new DataRelation ("Rel", "Mom", "Child", 
			                                          new string [] {"ChildName"},
			                                          new string [] {"Name"}, true);
			
			try {
				Set.Relations.Add (Relation);
				Assert.Fail ("test#01");
			} catch (Exception e) {
				AssertHelpers.AssertIsInstanceOfType (e, typeof(NullReferenceException), "test#02");
			}
			
			try {
				Set.Relations.AddRange (new DataRelation [] {Relation});
				Assert.Fail ("test#03");
			} catch (Exception e) {
				AssertHelpers.AssertIsInstanceOfType (e, typeof(NullReferenceException), "test#04");
			}
			
			Set.BeginInit ();
			Set.Relations.AddRange (new DataRelation [] {Relation});
			Set.EndInit ();
			
			DataRelation Test = null;
			Assert.AreEqual (1, Mom.ChildRelations.Count, "test#01");
			Assert.AreEqual (0, Child.ChildRelations.Count, "test#02");
			Assert.AreEqual (0, Mom.ParentRelations.Count, "test#03");
			Assert.AreEqual (1, Child.ParentRelations.Count, "test#04");
				
			Test = Child.ParentRelations [0];
			Assert.AreEqual ("Rel", Test.ToString (), "test#05");
			Assert.AreEqual ("Rel", Test.RelationName, "test#06");
			Assert.AreEqual ("Mom", Test.ParentTable.TableName, "test#07");
			
			Assert.IsNull (Test.ParentKeyConstraint, "test#08");
						
			Assert.AreEqual (1, Test.ParentColumns.Length, "test#10");
			Assert.IsTrue (Test.Nested, "test#11");
			Assert.AreEqual (0, Test.ExtendedProperties.Count, "test#12");
			Assert.AreEqual ("Child", Test.ChildTable.TableName, "test#13");
			Assert.IsNull (Test.ChildKeyConstraint, "test#14");
			Assert.AreEqual (1, Test.ChildColumns.Length, "test#15");
			
		}

		[Test]
		public void RelationFromSchema ()
		{
			DataSet Set = new DataSet ();
			Set.ReadXmlSchema ("Test/System.Data/store.xsd");
			DataTable Table = Set.Tables [0];
			
			Assert.IsFalse (Table.CaseSensitive, "test#01");
			Assert.AreEqual (1, Table.ChildRelations.Count, "test#02");
			Assert.AreEqual (0, Table.ParentRelations.Count, "test#03");
			Assert.AreEqual (1, Table.Constraints.Count, "test#04");
			Assert.AreEqual (1, Table.PrimaryKey.Length, "test#05");
			Assert.AreEqual (0, Table.Rows.Count, "test#06");
			Assert.AreEqual ("bookstore", Table.TableName, "test#07");
			Assert.AreEqual (1, Table.Columns.Count, "test#08");
						
			DataRelation Relation = Table.ChildRelations [0];
			Assert.AreEqual (1, Relation.ChildColumns.Length, "test#09");
			Assert.AreEqual ("bookstore_book", Relation.ChildKeyConstraint.ConstraintName, "test#10");
			Assert.AreEqual (1, Relation.ChildKeyConstraint.Columns.Length, "test#11");
			Assert.AreEqual ("book", Relation.ChildTable.TableName, "test#12");
			Assert.AreEqual ("NewDataSet", Relation.DataSet.DataSetName, "test#13");
			Assert.AreEqual (0, Relation.ExtendedProperties.Count, "test#14");
			Assert.IsTrue (Relation.Nested, "test#15");
			Assert.AreEqual (1, Relation.ParentColumns.Length, "test#16");
			Assert.AreEqual ("Constraint1", Relation.ParentKeyConstraint.ConstraintName, "test#17");
			Assert.AreEqual ("bookstore", Relation.ParentTable.TableName, "test#18");
			Assert.AreEqual ("bookstore_book", Relation.RelationName, "test#19");

			Table = Set.Tables [1];
			
			Assert.IsFalse (Table.CaseSensitive, "test#20");
			Assert.AreEqual (1, Table.ChildRelations.Count, "test#21");
			Assert.AreEqual (1, Table.ParentRelations.Count, "test#22");
			Assert.AreEqual (2, Table.Constraints.Count, "test#23");
			Assert.AreEqual (1, Table.PrimaryKey.Length, "test#24");
			Assert.AreEqual (0, Table.Rows.Count, "test#25");
			Assert.AreEqual ("book", Table.TableName, "test#26");
			Assert.AreEqual (5, Table.Columns.Count, "test#27");
		
			Relation = Table.ChildRelations [0];
			Assert.AreEqual (1, Relation.ChildColumns.Length, "test#28");
			Assert.AreEqual ("book_author", Relation.ChildKeyConstraint.ConstraintName, "test#29");
			Assert.AreEqual (1, Relation.ChildKeyConstraint.Columns.Length, "test#30");
			Assert.AreEqual ("author", Relation.ChildTable.TableName, "test#31");
			Assert.AreEqual ("NewDataSet", Relation.DataSet.DataSetName, "test#32");
			Assert.AreEqual (0, Relation.ExtendedProperties.Count, "test#33");
			Assert.IsTrue (Relation.Nested, "test#34");
			Assert.AreEqual (1, Relation.ParentColumns.Length, "test#35");
			Assert.AreEqual ("Constraint1", Relation.ParentKeyConstraint.ConstraintName, "test#36");
			Assert.AreEqual ("book", Relation.ParentTable.TableName, "test#37");
			Assert.AreEqual ("book_author", Relation.RelationName, "test#38");
			
			Table = Set.Tables [2];
			Assert.IsFalse (Table.CaseSensitive, "test#39");
			Assert.AreEqual (0, Table.ChildRelations.Count, "test#40");
			Assert.AreEqual (1, Table.ParentRelations.Count, "test#41");
			Assert.AreEqual (1, Table.Constraints.Count, "test#42");
			Assert.AreEqual (0, Table.PrimaryKey.Length, "test#43");
			Assert.AreEqual (0, Table.Rows.Count, "test#44");
			Assert.AreEqual ("author", Table.TableName, "test#45");
			Assert.AreEqual (3, Table.Columns.Count, "test#46");
		}

		[Test]
		public void ChildRows ()
		{
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			DataRow TempRow = Mom.NewRow ();
			TempRow [0] = "teresa";
			TempRow [1] = "john";
			Mom.Rows.Add (TempRow);
			
			TempRow = Mom.NewRow ();
			TempRow [0] = "teresa";
			TempRow [1] = "Dick";
			Mom.Rows.Add (TempRow);
						
			TempRow = Child.NewRow ();
			TempRow [0] = "john";
			TempRow [1] = "15";
			Child.Rows.Add (TempRow);
			
			TempRow = Child.NewRow ();
			TempRow [0] = "Dick";
			TempRow [1] = "10";
			Child.Rows.Add (TempRow);
			
			DataRow Row = Mom.Rows [1];			
			TempRow = Row.GetChildRows ("Rel") [0];
			Assert.AreEqual ("Dick", TempRow [0], "test#01");
			Assert.AreEqual ("10", TempRow [1].ToString (), "test#02");
			TempRow = TempRow.GetParentRow ("Rel");
			Assert.AreEqual ("teresa", TempRow [0], "test#03");
			Assert.AreEqual ("Dick", TempRow [1], "test#04");
			
			Row = Child.Rows [0];
			TempRow = Row.GetParentRows ("Rel") [0];
			Assert.AreEqual ("teresa", TempRow [0], "test#05");
			Assert.AreEqual ("john", TempRow [1], "test#06");						
		}
	}
}
