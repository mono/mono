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
using NUnit.Framework;
using System;
using System.Data;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif

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
                	
			Assert.That (Child.Rows.Count, Is.EqualTo (2), "test#01");
                	
			Row = Mom.Rows [0];
			Row.Delete ();
                	
			Assert.That (Child.Rows.Count, Is.EqualTo (1), "test#02");
                	
			Row = Mom.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Dick";
                	
			try {
				Mom.Rows.Add (Row);
				Assert.Fail ("test#03");
			} catch (Exception e) {
				Assert.That (e, Is.TypeOf (typeof(ConstraintException)), "test#04");
				// Never premise English.
				//Assert.That (e.Message, Is.EqualTo("Column 'ChildName' is constrained to be unique.  Value 'Dick' is already present."), "test#05");
			}                	

			Row = Mom.NewRow ();                                 
			Row [0] = "Teresa";                                  
			Row [1] = "Mich";                                    
			Mom.Rows.Add (Row);                                  
			Assert.That (Child.Rows.Count, Is.EqualTo (1), "test#06");
			
			Row = Child.NewRow ();                               
			Row [0] = "Jack";                                    
			Row [1] = 16;                                        
			
			try {                                                
				Child.Rows.Add (Row);                               
				Assert.Fail ("test#07");
			} catch (Exception e) {                              
				Assert.That (e, Is.TypeOf (typeof(InvalidConstraintException)), "test#08");
			}                                                    
		}

		[Test]
		[ExpectedException (typeof(InvalidConstraintException))]
		public void InvalidConstraintException ()
		{
			// Parent Columns and Child Columns don't have type-matching columns.
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [1], true);
		}

		[Test]
		[ExpectedException (typeof (InvalidConstraintException))]
		public void InvalidConstraintException2 ()
		{
			// Parent Columns and Child Columns don't have type-matching columns.
			Child.Columns [1].DataType = Mom.Columns [1].DataType;
			
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [1], true);
			Set.Relations.Add (Relation);
			Assert.That (Set.Relations.Count, Is.EqualTo (1), "test#01");
			
			Child.Columns [1].DataType = Type.GetType ("System.Double");
		}

		[Test]
		public void DataSetRelations ()
		{
			DataRelation Relation;
			Assert.That (Set.Relations.Count, Is.EqualTo (0), "test#01");
			Assert.That (Mom.ParentRelations.Count, Is.EqualTo (0), "test#02");
			Assert.That (Mom.ChildRelations.Count, Is.EqualTo (0), "test#03");
			Assert.That (Child.ParentRelations.Count, Is.EqualTo (0), "test#04");
			Assert.That (Child.ChildRelations.Count, Is.EqualTo (0), "test#05");
			
			Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			Assert.That (Set.Relations.Count, Is.EqualTo (1), "test#06");
			Assert.That (Mom.ParentRelations.Count, Is.EqualTo (0), "test#07");
			Assert.That (Mom.ChildRelations.Count, Is.EqualTo (1), "test#08");
			Assert.That (Child.ParentRelations.Count, Is.EqualTo (1), "test#09");
			Assert.That (Child.ChildRelations.Count, Is.EqualTo (0), "test#10");
						
			Relation = Set.Relations [0];
			Assert.That (Relation.ParentColumns.Length, Is.EqualTo (1), "test#11");
			Assert.That (Relation.ChildColumns.Length, Is.EqualTo (1), "test#12");
			Assert.That (Relation.ChildKeyConstraint.ConstraintName, Is.EqualTo ("Rel"), "test#13");
			Assert.That (Relation.ParentKeyConstraint.ConstraintName, Is.EqualTo ("Constraint1"), "test#14");
		}

		[Test]
		public void Constraints ()
		{
			Assert.That (Mom.Constraints.Count, Is.EqualTo (0), "test#01");
			Assert.That (Child.Constraints.Count, Is.EqualTo (0), "test#02");

			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			Assert.That (Mom.Constraints.Count, Is.EqualTo (1), "test#03");
			Assert.That (Child.Constraints.Count, Is.EqualTo (1), "test#04");
			Assert.That (Child.Constraints [0], Is.TypeOf (typeof(ForeignKeyConstraint)), "test#05");
			Assert.That (Mom.Constraints [0], Is.TypeOf (typeof(UniqueConstraint)), "test#06");
		}

		[Test]
		public void Creation ()
		{
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			DataRelation Test = null;
			Assert.That (Mom.ChildRelations.Count, Is.EqualTo (1), "test#01");
			Assert.That (Child.ChildRelations.Count, Is.EqualTo (0), "test#02");
			Assert.That (Mom.ParentRelations.Count, Is.EqualTo (0), "test#03");
			Assert.That (Child.ParentRelations.Count, Is.EqualTo (1), "test#04");
				
			Test = Child.ParentRelations [0];
			Assert.That (Test.ToString (), Is.EqualTo ("Rel"), "test#05");
			Assert.That (Test.RelationName, Is.EqualTo ("Rel"), "test#06");
			Assert.That (Test.ParentTable.TableName, Is.EqualTo ("Mom"), "test#07");
			Assert.That (Test.ParentKeyConstraint.Columns.Length, Is.EqualTo (1), "test#08");
			Assert.That (Test.ParentKeyConstraint.IsPrimaryKey, Is.False, "test#09");
			Assert.That (Test.ParentColumns.Length, Is.EqualTo (1), "test#10");
			Assert.That (Test.Nested, Is.False, "test#11");
			Assert.That (Test.ExtendedProperties.Count, Is.EqualTo (0), "test#12");
			Assert.That (Test.ChildTable.TableName, Is.EqualTo ("Child"), "test#13");
			Assert.That (Test.ChildKeyConstraint.ConstraintName, Is.EqualTo ("Rel"), "test#14");
			Assert.That (Test.ChildColumns.Length, Is.EqualTo (1), "test#15");
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
			Assert.That (Mom2.ChildRelations.Count, Is.EqualTo (1), "test#01");
			Assert.That (Child2.ChildRelations.Count, Is.EqualTo (0), "test#02");
			Assert.That (Mom2.ParentRelations.Count, Is.EqualTo (0), "test#03");
			Assert.That (Child2.ParentRelations.Count, Is.EqualTo (1), "test#04");
				
			Test = Child2.ParentRelations [0];
			Assert.That (Test.ToString (), Is.EqualTo ("Rel"), "test#05");
			Assert.That (Test.RelationName, Is.EqualTo ("Rel"), "test#06");
			Assert.That (Test.ParentTable.TableName, Is.EqualTo ("Mom"), "test#07");
			Assert.That (Test.ParentKeyConstraint.Columns.Length, Is.EqualTo (2), "test#08");
			Assert.That (Test.ParentKeyConstraint.IsPrimaryKey, Is.False, "test#09");
			Assert.That (Test.ParentColumns.Length, Is.EqualTo (2), "test#10");
			Assert.That (Test.Nested, Is.False, "test#11");
			Assert.That (Test.ExtendedProperties.Count, Is.EqualTo (0), "test#12");
			Assert.That (Test.ChildTable.TableName, Is.EqualTo ("Child"), "test#13");
			Assert.That (Test.ChildKeyConstraint.ConstraintName, Is.EqualTo ("Rel"), "test#14");
			Assert.That (Test.ChildColumns.Length, Is.EqualTo (2), "test#15");
			Assert.That (Mom2.Constraints.Count, Is.EqualTo (1), "test#16");
			Assert.That (Mom2.Constraints [0].ToString (), Is.EqualTo ("Constraint1"), "test#17");
			Assert.That (Child2.Constraints.Count, Is.EqualTo (1), "test#18");
			Assert.That (Hubby.Constraints.Count, Is.EqualTo (0), "test#19");
		}

		[Test]
		public void Creation3 ()
		{
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0], false);
			Set.Relations.Add (Relation);
			DataRelation Test = null;
	
			Assert.That (Mom.ChildRelations.Count, Is.EqualTo (1), "test#01");
			Assert.That (Child.ChildRelations.Count, Is.EqualTo (0), "test#02");
			Assert.That (Mom.ParentRelations.Count, Is.EqualTo (0), "test#03");
			Assert.That (Child.ParentRelations.Count, Is.EqualTo (1), "test#04");

			Test = Child.ParentRelations [0];
			
			Assert.That (Test.ToString (), Is.EqualTo ("Rel"), "test#05");

			Assert.That (Test.RelationName, Is.EqualTo ("Rel"), "test#06");
			Assert.That (Test.ParentTable.TableName, Is.EqualTo ("Mom"), "test#07");

			Assert.That (Test.ParentKeyConstraint, Is.Null, "test#08");

			Assert.That (Test.ParentKeyConstraint, Is.Null, "test#09");

			Assert.That (Test.ParentColumns.Length, Is.EqualTo (1), "test#10");
			Assert.That (Test.Nested, Is.False, "test#11");
			Assert.That (Test.ExtendedProperties.Count, Is.EqualTo (0), "test#12");
			Assert.That (Test.ChildTable.TableName, Is.EqualTo ("Child"), "test#13");

			Assert.That (Test.ChildKeyConstraint, Is.Null, "test#14");
			Assert.That (Test.ChildColumns.Length, Is.EqualTo (1), "test#15");
			Assert.That (Mom.Constraints.Count, Is.EqualTo (0), "test#16");
			Assert.That (Child.Constraints.Count, Is.EqualTo (0), "test#17");
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
				Assert.That (e, Is.TypeOf (typeof(NullReferenceException)), "test#02");
			}
			
			try {
				Set.Relations.AddRange (new DataRelation [] {Relation});
				Assert.Fail ("test#03");
			} catch (Exception e) {
				Assert.That (e, Is.TypeOf (typeof(NullReferenceException)), "test#04");
			}
			
			Set.BeginInit ();
			Set.Relations.AddRange (new DataRelation [] {Relation});
			Set.EndInit ();
			
			DataRelation Test = null;
			Assert.That (Mom.ChildRelations.Count, Is.EqualTo (1), "test#01");
			Assert.That (Child.ChildRelations.Count, Is.EqualTo (0), "test#02");
			Assert.That (Mom.ParentRelations.Count, Is.EqualTo (0), "test#03");
			Assert.That (Child.ParentRelations.Count, Is.EqualTo (1), "test#04");
				
			Test = Child.ParentRelations [0];
			Assert.That (Test.ToString (), Is.EqualTo ("Rel"), "test#05");
			Assert.That (Test.RelationName, Is.EqualTo ("Rel"), "test#06");
			Assert.That (Test.ParentTable.TableName, Is.EqualTo ("Mom"), "test#07");
			
			Assert.That (Test.ParentKeyConstraint, Is.Null, "test#08");
						
			Assert.That (Test.ParentColumns.Length, Is.EqualTo (1), "test#10");
			Assert.That (Test.Nested, Is.True, "test#11");
			Assert.That (Test.ExtendedProperties.Count, Is.EqualTo (0), "test#12");
			Assert.That (Test.ChildTable.TableName, Is.EqualTo ("Child"), "test#13");
			Assert.That (Test.ChildKeyConstraint, Is.Null, "test#14");
			Assert.That (Test.ChildColumns.Length, Is.EqualTo (1), "test#15");
			
		}

		[Test]
		public void RelationFromSchema ()
		{
			DataSet Set = new DataSet ();
			Set.ReadXmlSchema ("Test/System.Data/store.xsd");
			DataTable Table = Set.Tables [0];
			
			Assert.That (Table.CaseSensitive, Is.False, "test#01");
			Assert.That (Table.ChildRelations.Count, Is.EqualTo (1), "test#02");
			Assert.That (Table.ParentRelations.Count, Is.EqualTo (0), "test#03");
			Assert.That (Table.Constraints.Count, Is.EqualTo (1), "test#04");
			Assert.That (Table.PrimaryKey.Length, Is.EqualTo (1), "test#05");
			Assert.That (Table.Rows.Count, Is.EqualTo (0), "test#06");
			Assert.That (Table.TableName, Is.EqualTo ("bookstore"), "test#07");
			Assert.That (Table.Columns.Count, Is.EqualTo (1), "test#08");
						
			DataRelation Relation = Table.ChildRelations [0];
			Assert.That (Relation.ChildColumns.Length, Is.EqualTo (1), "test#09");
			Assert.That (Relation.ChildKeyConstraint.ConstraintName, Is.EqualTo ("bookstore_book"), "test#10");
			Assert.That (Relation.ChildKeyConstraint.Columns.Length, Is.EqualTo (1), "test#11");
			Assert.That (Relation.ChildTable.TableName, Is.EqualTo ("book"), "test#12");
			Assert.That (Relation.DataSet.DataSetName, Is.EqualTo ("NewDataSet"), "test#13");
			Assert.That (Relation.ExtendedProperties.Count, Is.EqualTo (0), "test#14");
			Assert.That (Relation.Nested, Is.True, "test#15");
			Assert.That (Relation.ParentColumns.Length, Is.EqualTo (1), "test#16");
			Assert.That (Relation.ParentKeyConstraint.ConstraintName, Is.EqualTo ("Constraint1"), "test#17");
			Assert.That (Relation.ParentTable.TableName, Is.EqualTo ("bookstore"), "test#18");
			Assert.That (Relation.RelationName, Is.EqualTo ("bookstore_book"), "test#19");

			Table = Set.Tables [1];
			
			Assert.That (Table.CaseSensitive, Is.False, "test#20");
			Assert.That (Table.ChildRelations.Count, Is.EqualTo (1), "test#21");
			Assert.That (Table.ParentRelations.Count, Is.EqualTo (1), "test#22");
			Assert.That (Table.Constraints.Count, Is.EqualTo (2), "test#23");
			Assert.That (Table.PrimaryKey.Length, Is.EqualTo (1), "test#24");
			Assert.That (Table.Rows.Count, Is.EqualTo (0), "test#25");
			Assert.That (Table.TableName, Is.EqualTo ("book"), "test#26");
			Assert.That (Table.Columns.Count, Is.EqualTo (5), "test#27");
		
			Relation = Table.ChildRelations [0];
			Assert.That (Relation.ChildColumns.Length, Is.EqualTo (1), "test#28");
			Assert.That (Relation.ChildKeyConstraint.ConstraintName, Is.EqualTo ("book_author"), "test#29");
			Assert.That (Relation.ChildKeyConstraint.Columns.Length, Is.EqualTo (1), "test#30");
			Assert.That (Relation.ChildTable.TableName, Is.EqualTo ("author"), "test#31");
			Assert.That (Relation.DataSet.DataSetName, Is.EqualTo ("NewDataSet"), "test#32");
			Assert.That (Relation.ExtendedProperties.Count, Is.EqualTo (0), "test#33");
			Assert.That (Relation.Nested, Is.True, "test#34");
			Assert.That (Relation.ParentColumns.Length, Is.EqualTo (1), "test#35");
			Assert.That (Relation.ParentKeyConstraint.ConstraintName, Is.EqualTo ("Constraint1"), "test#36");
			Assert.That (Relation.ParentTable.TableName, Is.EqualTo ("book"), "test#37");
			Assert.That (Relation.RelationName, Is.EqualTo ("book_author"), "test#38");
			
			Table = Set.Tables [2];
			Assert.That (Table.CaseSensitive, Is.False, "test#39");
			Assert.That (Table.ChildRelations.Count, Is.EqualTo (0), "test#40");
			Assert.That (Table.ParentRelations.Count, Is.EqualTo (1), "test#41");
			Assert.That (Table.Constraints.Count, Is.EqualTo (1), "test#42");
			Assert.That (Table.PrimaryKey.Length, Is.EqualTo (0), "test#43");
			Assert.That (Table.Rows.Count, Is.EqualTo (0), "test#44");
			Assert.That (Table.TableName, Is.EqualTo ("author"), "test#45");
			Assert.That (Table.Columns.Count, Is.EqualTo (3), "test#46");
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
			Assert.That (TempRow [0], Is.EqualTo ("Dick"), "test#01");
			Assert.That (TempRow [1].ToString (), Is.EqualTo ("10"), "test#02");
			TempRow = TempRow.GetParentRow ("Rel");
			Assert.That (TempRow [0], Is.EqualTo ("teresa"), "test#03");
			Assert.That (TempRow [1], Is.EqualTo ("Dick"), "test#04");
			
			Row = Child.Rows [0];
			TempRow = Row.GetParentRows ("Rel") [0];
			Assert.That (TempRow [0], Is.EqualTo ("teresa"), "test#05");
			Assert.That (TempRow [1], Is.EqualTo ("john"), "test#06");						
		}
	}
}
