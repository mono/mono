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

namespace MonoTests.System.Data
{
	[TestFixture]
        public class DataRelationTest : Assertion
        {
		private DataSet Set = null;
        	private DataTable Mom = null;
        	private DataTable Child = null;        	

		[SetUp]
                public void GetReady() 
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
                	
                	AssertEquals ("test#01", 2, Child.Rows.Count);
                	
                	Row = Mom.Rows [0];
                	Row.Delete ();
                	
                	AssertEquals ("test#02", 1, Child.Rows.Count);
                	
                	Row = Mom.NewRow ();
                	Row [0] = "Teresa";
                	Row [1] = "Dick";
                	
                	try {
                		Mom.Rows.Add (Row);
                		Fail ("test#03");
                	} catch (Exception e) {
                		AssertEquals ("test#04", typeof (ConstraintException), e.GetType ());
				// Never premise English.
                		//AssertEquals ("test#05", "Column 'ChildName' is constrained to be unique.  Value 'Dick' is already present.", e.Message);
                	}                	

			Row = Mom.NewRow ();                                 
                        Row [0] = "Teresa";                                  
                        Row [1] = "Mich";                                    
                        Mom.Rows.Add (Row);                                  
                        AssertEquals ("test#06", 1, Child.Rows.Count);       
			
                        Row = Child.NewRow ();                               
                        Row [0] = "Jack";                                    
                        Row [1] = 16;                                        
			
                        try {                                                
                                Child.Rows.Add (Row);                               
                                Fail ("test#07");                                   
                        } catch (Exception e) {                              
                                AssertEquals ("test#08", typeof (InvalidConstraintException), e.GetType ());
				// Never premise English.
                                //AssertEquals ("test#09", "ForeignKeyConstraint Rel requires the child key values (Jack) to exist in the parent table.", e.Message);                                                                      
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
			AssertEquals("test#01", 1, Set.Relations.Count);
			
			Child.Columns [1].DataType = Type.GetType ("System.Double");
		}
		
		[Test]
		public void DataSetRelations ()
		{
			DataRelation Relation;
			AssertEquals ("test#01", 0, Set.Relations.Count);
			AssertEquals ("test#02", 0, Mom.ParentRelations.Count);
			AssertEquals ("test#03", 0, Mom.ChildRelations.Count);
			AssertEquals ("test#04", 0, Child.ParentRelations.Count);
			AssertEquals ("test#05", 0, Child.ChildRelations.Count);
			
			Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			AssertEquals ("test#06", 1, Set.Relations.Count);
			AssertEquals ("test#07", 0, Mom.ParentRelations.Count);
			AssertEquals ("test#08", 1, Mom.ChildRelations.Count);
			AssertEquals ("test#09", 1, Child.ParentRelations.Count);
			AssertEquals ("test#10", 0, Child.ChildRelations.Count);
						
			Relation = Set.Relations [0];
			AssertEquals ("test#11", 1, Relation.ParentColumns.Length);
			AssertEquals ("test#12", 1, Relation.ChildColumns.Length);
			AssertEquals ("test#13", "Rel", Relation.ChildKeyConstraint.ConstraintName);
			AssertEquals ("test#14", "Constraint1", Relation.ParentKeyConstraint.ConstraintName);
		}
		
		[Test]
		public void Constraints ()
		{
				
			AssertEquals ("test#01", 0, Mom.Constraints.Count);
			AssertEquals ("test#02", 0, Child.Constraints.Count);

			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			AssertEquals ("test#03", 1, Mom.Constraints.Count);
			AssertEquals ("test#04", 1, Child.Constraints.Count);
			AssertEquals ("test#05", typeof (ForeignKeyConstraint), Child.Constraints [0].GetType ());
			AssertEquals ("test#05", typeof (UniqueConstraint), Mom.Constraints [0].GetType ());
			
		}

		[Test]
		public void Creation ()
		{
			
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			DataRelation Test = null;
			AssertEquals ("test#01", 1, Mom.ChildRelations.Count);
			AssertEquals ("test#02", 0, Child.ChildRelations.Count);
			AssertEquals ("test#03", 0, Mom.ParentRelations.Count);
			AssertEquals ("test#04", 1, Child.ParentRelations.Count);
				
			Test = Child.ParentRelations [0];
			AssertEquals ("test#05", "Rel", Test.ToString ());
			AssertEquals ("test#06", "Rel", Test.RelationName);
			AssertEquals ("test#07", "Mom", Test.ParentTable.TableName);
			AssertEquals ("test#08", 1, Test.ParentKeyConstraint.Columns.Length);
			AssertEquals ("test#09", false, Test.ParentKeyConstraint.IsPrimaryKey);
			AssertEquals ("test#10", 1, Test.ParentColumns.Length);
			AssertEquals ("test#11", false, Test.Nested);
			AssertEquals ("test#12", 0, Test.ExtendedProperties.Count);
			AssertEquals ("test#13", "Child", Test.ChildTable.TableName);
			AssertEquals ("test#14", "Rel", Test.ChildKeyConstraint.ConstraintName);
			AssertEquals ("test#15", 1, Test.ChildColumns.Length);
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
				Fail ("test#01");
			} catch (InvalidConstraintException e) {
//				AssertEquals ("test#02", typeof (InvalidConstraintException), e.GetType ());				
//				AssertEquals ("test#03", "Cannot create a Key from Columns that belong to different tables.", e.Message);
			}
			
			Childs [1] = Col6;
			Relation = new DataRelation ("Rel", Parents, Childs);
			
			Set.Relations.Add (Relation);
			
			DataRelation Test = null;
			AssertEquals ("test#01", 1, Mom2.ChildRelations.Count);
			AssertEquals ("test#02", 0, Child2.ChildRelations.Count);
			AssertEquals ("test#03", 0, Mom2.ParentRelations.Count);
			AssertEquals ("test#04", 1, Child2.ParentRelations.Count);
				
			Test = Child2.ParentRelations [0];
			AssertEquals ("test#05", "Rel", Test.ToString ());
			AssertEquals ("test#06", "Rel", Test.RelationName);
			AssertEquals ("test#07", "Mom", Test.ParentTable.TableName);
			AssertEquals ("test#08", 2, Test.ParentKeyConstraint.Columns.Length);
			AssertEquals ("test#09", false, Test.ParentKeyConstraint.IsPrimaryKey);
			AssertEquals ("test#10", 2, Test.ParentColumns.Length);
			AssertEquals ("test#11", false, Test.Nested);
			AssertEquals ("test#12", 0, Test.ExtendedProperties.Count);
			AssertEquals ("test#13", "Child", Test.ChildTable.TableName);
			AssertEquals ("test#14", "Rel", Test.ChildKeyConstraint.ConstraintName);
			AssertEquals ("test#15", 2, Test.ChildColumns.Length);
			AssertEquals ("test#16", 1, Mom2.Constraints.Count);
			AssertEquals ("test#17", "Constraint1", Mom2.Constraints [0].ToString ());
			AssertEquals ("test#18", 1, Child2.Constraints.Count);			
			AssertEquals ("test#19", 0, Hubby.Constraints.Count);
		}
		
		[Test]
		public void Creation3 ()
		{

			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0], false);
			Set.Relations.Add (Relation);
			DataRelation Test = null;
	
			AssertEquals ("test#01", 1, Mom.ChildRelations.Count);
			AssertEquals ("test#02", 0, Child.ChildRelations.Count);
			AssertEquals ("test#03", 0, Mom.ParentRelations.Count);
			AssertEquals ("test#04", 1, Child.ParentRelations.Count);
				
			Test = Child.ParentRelations [0];
			
			AssertEquals ("test#05", "Rel", Test.ToString ());
			
			AssertEquals ("test#06", "Rel", Test.RelationName);
			AssertEquals ("test#07", "Mom", Test.ParentTable.TableName);
			
			Assert ("test#08", Test.ParentKeyConstraint == null);
			
			Assert ("test#09", Test.ParentKeyConstraint == null);
			
			AssertEquals ("test#10", 1, Test.ParentColumns.Length);
			AssertEquals ("test#11", false, Test.Nested);
			AssertEquals ("test#12", 0, Test.ExtendedProperties.Count);
			AssertEquals ("test#13", "Child", Test.ChildTable.TableName);
			
			Assert ("test#14", Test.ChildKeyConstraint == null);
			AssertEquals ("test#15", 1, Test.ChildColumns.Length);
			AssertEquals ("test#16", 0, Mom.Constraints.Count);			
			AssertEquals ("test#17", 0, Child.Constraints.Count);			

		}

		[Test]
		public void Creation4 ()
		{
			
			DataRelation Relation = new DataRelation ("Rel", "Mom", "Child", 
			                                          new string [] {"ChildName"},
			                                          new string [] {"Name"}, true);
			
			try {
				Set.Relations.Add (Relation);
				Fail ("test#01");
			} catch (Exception e) {
				AssertEquals ("test#02", typeof (NullReferenceException), e.GetType ());
			}
			
			try {
				Set.Relations.AddRange (new DataRelation [] {Relation});
				Fail ("test#03");
			} catch (Exception e) {
				AssertEquals ("test#04", typeof (NullReferenceException), e.GetType ());
			}
			
			Set.BeginInit ();
			Set.Relations.AddRange (new DataRelation [] {Relation});
			Set.EndInit ();
			
			DataRelation Test = null;
			AssertEquals ("test#01", 1, Mom.ChildRelations.Count);
			AssertEquals ("test#02", 0, Child.ChildRelations.Count);
			AssertEquals ("test#03", 0, Mom.ParentRelations.Count);
			AssertEquals ("test#04", 1, Child.ParentRelations.Count);
				
			Test = Child.ParentRelations [0];
			AssertEquals ("test#05", "Rel", Test.ToString ());
			AssertEquals ("test#06", "Rel", Test.RelationName);
			AssertEquals ("test#07", "Mom", Test.ParentTable.TableName);
			
			AssertEquals ("test#08", true, Test.ParentKeyConstraint == null);
						
			AssertEquals ("test#10", 1, Test.ParentColumns.Length);
			AssertEquals ("test#11", true, Test.Nested);
			AssertEquals ("test#12", 0, Test.ExtendedProperties.Count);
			AssertEquals ("test#13", "Child", Test.ChildTable.TableName);
			AssertEquals ("test#14", true, Test.ChildKeyConstraint == null);
			AssertEquals ("test#15", 1, Test.ChildColumns.Length);
			
		}

		[Test]
		public void RelationFromSchema ()
		{
			DataSet Set = new DataSet ();
			Set.ReadXmlSchema ("Test/System.Data/store.xsd");
			DataTable Table = Set.Tables [0];
			
			AssertEquals ("test#01", false, Table.CaseSensitive);
			AssertEquals ("test#02", 1, Table.ChildRelations.Count);
			AssertEquals ("test#03", 0, Table.ParentRelations.Count);
			AssertEquals ("test#04", 1, Table.Constraints.Count);
			AssertEquals ("test#05", 1, Table.PrimaryKey.Length);
			AssertEquals ("test#06", 0, Table.Rows.Count);
			AssertEquals ("test#07", "bookstore", Table.TableName);
			AssertEquals ("test#08", 1, Table.Columns.Count);
						
			DataRelation Relation = Table.ChildRelations [0];
			AssertEquals ("test#09", 1, Relation.ChildColumns.Length);
			AssertEquals ("test#10", "bookstore_book", Relation.ChildKeyConstraint.ConstraintName);
			AssertEquals ("test#11", 1, Relation.ChildKeyConstraint.Columns.Length);
			AssertEquals ("test#12", "book", Relation.ChildTable.TableName);
			AssertEquals ("test#13", "NewDataSet", Relation.DataSet.DataSetName);
			AssertEquals ("test#14", 0, Relation.ExtendedProperties.Count);
			AssertEquals ("test#15", true, Relation.Nested);
			AssertEquals ("test#16", 1, Relation.ParentColumns.Length);
			AssertEquals ("test#17", "Constraint1", Relation.ParentKeyConstraint.ConstraintName);
			AssertEquals ("test#18", "bookstore", Relation.ParentTable.TableName);
			AssertEquals ("test#19", "bookstore_book", Relation.RelationName);

			Table = Set.Tables [1];
			
			AssertEquals ("test#20", false, Table.CaseSensitive);
			AssertEquals ("test#21", 1, Table.ChildRelations.Count);
			AssertEquals ("test#22", 1, Table.ParentRelations.Count);
			AssertEquals ("test#23", 2, Table.Constraints.Count);
			AssertEquals ("test#24", 1, Table.PrimaryKey.Length);
			AssertEquals ("test#25", 0, Table.Rows.Count);
			AssertEquals ("test#26", "book", Table.TableName);
			AssertEquals ("test#27", 5, Table.Columns.Count);
		
			Relation = Table.ChildRelations [0];
			AssertEquals ("test#28", 1, Relation.ChildColumns.Length);
			AssertEquals ("test#29", "book_author", Relation.ChildKeyConstraint.ConstraintName);
			AssertEquals ("test#30", 1, Relation.ChildKeyConstraint.Columns.Length);
			AssertEquals ("test#31", "author", Relation.ChildTable.TableName);
			AssertEquals ("test#32", "NewDataSet", Relation.DataSet.DataSetName);
			AssertEquals ("test#33", 0, Relation.ExtendedProperties.Count);
			AssertEquals ("test#34", true, Relation.Nested);
			AssertEquals ("test#35", 1, Relation.ParentColumns.Length);
			AssertEquals ("test#36", "Constraint1", Relation.ParentKeyConstraint.ConstraintName);
			AssertEquals ("test#37", "book", Relation.ParentTable.TableName);
			AssertEquals ("test#38", "book_author", Relation.RelationName);
			
			Table = Set.Tables [2];
			AssertEquals ("test#39", false, Table.CaseSensitive);
			AssertEquals ("test#40", 0, Table.ChildRelations.Count);
			AssertEquals ("test#41", 1, Table.ParentRelations.Count);
			AssertEquals ("test#42", 1, Table.Constraints.Count);
			AssertEquals ("test#43", 0, Table.PrimaryKey.Length);
			AssertEquals ("test#44", 0, Table.Rows.Count);
			AssertEquals ("test#45", "author", Table.TableName);
			AssertEquals ("test#46", 3, Table.Columns.Count);
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
			AssertEquals ("test#01", "Dick", TempRow [0]);
			AssertEquals ("test#02", "10", TempRow [1].ToString ());
			TempRow = TempRow.GetParentRow ("Rel");
			AssertEquals ("test#03", "teresa", TempRow [0]);
			AssertEquals ("test#04", "Dick", TempRow [1]);
			
			Row = Child.Rows [0];
			TempRow = Row.GetParentRows ("Rel") [0];
			AssertEquals ("test#05", "teresa", TempRow [0]);
			AssertEquals ("test#06", "john", TempRow [1]);						
		}

        }
}
