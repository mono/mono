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

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture]
        public class DataRelationTest
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
                	
                	Assertion.AssertEquals ("test#01", 2, Child.Rows.Count);
                	
                	Row = Mom.Rows [0];
                	Row.Delete ();
                	
                	Assertion.AssertEquals ("test#02", 1, Child.Rows.Count);
                	
                	Row = Mom.NewRow ();
                	Row [0] = "Teresa";
                	Row [1] = "Dick";
                	
                	try {
                		Mom.Rows.Add (Row);
                		Assertion.Fail ("test#03");
                	} catch (Exception e) {
                		Assertion.AssertEquals ("test#04", typeof (ConstraintException), e.GetType ());
                		Assertion.AssertEquals ("test#05", "Column 'ChildName' is constrained to be unique.  Value 'Dick' is already present.", e.Message);
                	}                	

			Row = Mom.NewRow ();                                 
                        Row [0] = "Teresa";                                  
                        Row [1] = "Mich";                                    
                        Mom.Rows.Add (Row);                                  
                        Assertion.AssertEquals ("test#06", 1, Child.Rows.Count);       
			
                        Row = Child.NewRow ();                               
                        Row [0] = "Jack";                                    
                        Row [1] = 16;                                        
			
                        try {                                                
                                Child.Rows.Add (Row);                               
                                Assertion.Fail ("test#07");                                   
                        } catch (Exception e) {                              
                                Assertion.AssertEquals ("test#08", typeof (InvalidConstraintException), e.GetType ());
                                Assertion.AssertEquals ("test#09", "ForeignKeyConstraint Rel requires the child key values (Jack) to exist in the parent table.", e.Message);                                                                      
                        }                                                    

                }

		[Test]
		public void InvalidConstraintException ()
		{
			
			DataRelation Relation = null;
			try {
				Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [1], true);
				Assertion.Fail ("test#01");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#02", typeof (InvalidConstraintException), e.GetType ());
				Assertion.AssertEquals ("test#03", "Parent Columns and Child Columns don't have type-matching columns.", e.Message);
			}
			
			Child.Columns [1].DataType = Mom.Columns [1].DataType;
			
			Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [1], true);
			Set.Relations.Add (Relation);
			
			try {
				Child.Columns [1].DataType = Type.GetType ("System.Double");
				Assertion.Fail ("test#04");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#05", typeof (InvalidConstraintException), e.GetType ());
				Assertion.AssertEquals ("test#06", "Parent Columns and Child Columns don't have type-matching columns.", e.Message);
			}									
		}
		
		[Test]
		public void DataSetRelations ()
		{
			DataRelation Relation;
			Assertion.AssertEquals ("test#01", 0, Set.Relations.Count);
			Assertion.AssertEquals ("test#02", 0, Mom.ParentRelations.Count);
			Assertion.AssertEquals ("test#03", 0, Mom.ChildRelations.Count);
			Assertion.AssertEquals ("test#04", 0, Child.ParentRelations.Count);
			Assertion.AssertEquals ("test#05", 0, Child.ChildRelations.Count);
			
			Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			Assertion.AssertEquals ("test#06", 1, Set.Relations.Count);
			Assertion.AssertEquals ("test#07", 0, Mom.ParentRelations.Count);
			Assertion.AssertEquals ("test#08", 1, Mom.ChildRelations.Count);
			Assertion.AssertEquals ("test#09", 1, Child.ParentRelations.Count);
			Assertion.AssertEquals ("test#10", 0, Child.ChildRelations.Count);
						
			Relation = Set.Relations [0];
			Assertion.AssertEquals ("test#11", 1, Relation.ParentColumns.Length);
			Assertion.AssertEquals ("test#12", 1, Relation.ChildColumns.Length);
			Assertion.AssertEquals ("test#13", "Rel", Relation.ChildKeyConstraint.ConstraintName);
			Assertion.AssertEquals ("test#14", "Constraint1", Relation.ParentKeyConstraint.ConstraintName);
		}
		
		[Test]
		public void Constraints ()
		{
				
			Assertion.AssertEquals ("test#01", 0, Mom.Constraints.Count);
			Assertion.AssertEquals ("test#02", 0, Child.Constraints.Count);

			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			
			Assertion.AssertEquals ("test#03", 1, Mom.Constraints.Count);
			Assertion.AssertEquals ("test#04", 1, Child.Constraints.Count);
			Assertion.AssertEquals ("test#05", typeof (ForeignKeyConstraint), Child.Constraints [0].GetType ());
			Assertion.AssertEquals ("test#05", typeof (UniqueConstraint), Mom.Constraints [0].GetType ());
			
		}

		[Test]
		public void Creation ()
		{
			
			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);
			DataRelation Test = null;
			Assertion.AssertEquals ("test#01", 1, Mom.ChildRelations.Count);
			Assertion.AssertEquals ("test#02", 0, Child.ChildRelations.Count);
			Assertion.AssertEquals ("test#03", 0, Mom.ParentRelations.Count);
			Assertion.AssertEquals ("test#04", 1, Child.ParentRelations.Count);
				
			Test = Child.ParentRelations [0];
			Assertion.AssertEquals ("test#05", "Rel", Test.ToString ());
			Assertion.AssertEquals ("test#06", "Rel", Test.RelationName);
			Assertion.AssertEquals ("test#07", "Mom", Test.ParentTable.TableName);
			Assertion.AssertEquals ("test#08", 1, Test.ParentKeyConstraint.Columns.Length);
			Assertion.AssertEquals ("test#09", false, Test.ParentKeyConstraint.IsPrimaryKey);
			Assertion.AssertEquals ("test#10", 1, Test.ParentColumns.Length);
			Assertion.AssertEquals ("test#11", false, Test.Nested);
			Assertion.AssertEquals ("test#12", 0, Test.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#13", "Child", Test.ChildTable.TableName);
			Assertion.AssertEquals ("test#14", "Rel", Test.ChildKeyConstraint.ConstraintName);
			Assertion.AssertEquals ("test#15", 1, Test.ChildColumns.Length);
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
				Assertion.Fail ("test#01");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#02", typeof (InvalidConstraintException), e.GetType ());				
				Assertion.AssertEquals ("test#03", "Cannot create a Key from Columns that belong to different tables.", e.Message);
			}
			
			Childs [1] = Col6;
			Relation = new DataRelation ("Rel", Parents, Childs);
			
			Set.Relations.Add (Relation);
			
			DataRelation Test = null;
			Assertion.AssertEquals ("test#01", 1, Mom2.ChildRelations.Count);
			Assertion.AssertEquals ("test#02", 0, Child2.ChildRelations.Count);
			Assertion.AssertEquals ("test#03", 0, Mom2.ParentRelations.Count);
			Assertion.AssertEquals ("test#04", 1, Child2.ParentRelations.Count);
				
			Test = Child2.ParentRelations [0];
			Assertion.AssertEquals ("test#05", "Rel", Test.ToString ());
			Assertion.AssertEquals ("test#06", "Rel", Test.RelationName);
			Assertion.AssertEquals ("test#07", "Mom", Test.ParentTable.TableName);
			Assertion.AssertEquals ("test#08", 2, Test.ParentKeyConstraint.Columns.Length);
			Assertion.AssertEquals ("test#09", false, Test.ParentKeyConstraint.IsPrimaryKey);
			Assertion.AssertEquals ("test#10", 2, Test.ParentColumns.Length);
			Assertion.AssertEquals ("test#11", false, Test.Nested);
			Assertion.AssertEquals ("test#12", 0, Test.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#13", "Child", Test.ChildTable.TableName);
			Assertion.AssertEquals ("test#14", "Rel", Test.ChildKeyConstraint.ConstraintName);
			Assertion.AssertEquals ("test#15", 2, Test.ChildColumns.Length);
			Assertion.AssertEquals ("test#16", 1, Mom2.Constraints.Count);
			Assertion.AssertEquals ("test#17", "Constraint1", Mom2.Constraints [0].ToString ());
			Assertion.AssertEquals ("test#18", 1, Child2.Constraints.Count);			
			Assertion.AssertEquals ("test#19", 0, Hubby.Constraints.Count);
		}
		
		[Test]
		public void Creation3 ()
		{

			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0], false);
			Set.Relations.Add (Relation);
			DataRelation Test = null;
	
			Assertion.AssertEquals ("test#01", 1, Mom.ChildRelations.Count);
			Assertion.AssertEquals ("test#02", 0, Child.ChildRelations.Count);
			Assertion.AssertEquals ("test#03", 0, Mom.ParentRelations.Count);
			Assertion.AssertEquals ("test#04", 1, Child.ParentRelations.Count);
				
			Test = Child.ParentRelations [0];
			
			Assertion.AssertEquals ("test#05", "Rel", Test.ToString ());
			
			Assertion.AssertEquals ("test#06", "Rel", Test.RelationName);
			Assertion.AssertEquals ("test#07", "Mom", Test.ParentTable.TableName);
			
			Assertion.Assert ("test#08", Test.ParentKeyConstraint == null);
			
			Assertion.Assert ("test#09", Test.ParentKeyConstraint == null);
			
			Assertion.AssertEquals ("test#10", 1, Test.ParentColumns.Length);
			Assertion.AssertEquals ("test#11", false, Test.Nested);
			Assertion.AssertEquals ("test#12", 0, Test.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#13", "Child", Test.ChildTable.TableName);
			
			Assertion.Assert ("test#14", Test.ChildKeyConstraint == null);
			Assertion.AssertEquals ("test#15", 1, Test.ChildColumns.Length);
			Assertion.AssertEquals ("test#16", 0, Mom.Constraints.Count);			
			Assertion.AssertEquals ("test#17", 0, Child.Constraints.Count);			

		}

		[Test]
		public void Creation4 ()
		{
			
			DataRelation Relation = new DataRelation ("Rel", "Mom", "Child", 
			                                          new string [] {"ChildName"},
			                                          new string [] {"Name"}, true);
			
			try {
				Set.Relations.Add (Relation);
				Assertion.Fail ("test#01");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#02", typeof (NullReferenceException), e.GetType ());
			}
			
			try {
				Set.Relations.AddRange (new DataRelation [] {Relation});
				Assertion.Fail ("test#03");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#04", typeof (NullReferenceException), e.GetType ());
			}
			
			//Set.BeginInit ();
			Set.Relations.AddRange (new DataRelation [] {Relation});
			//Set.EndInit ();
			
			DataRelation Test = null;
			Assertion.AssertEquals ("test#01", 1, Mom.ChildRelations.Count);
			Assertion.AssertEquals ("test#02", 0, Child.ChildRelations.Count);
			Assertion.AssertEquals ("test#03", 0, Mom.ParentRelations.Count);
			Assertion.AssertEquals ("test#04", 1, Child.ParentRelations.Count);
				
			Test = Child.ParentRelations [0];
			Assertion.AssertEquals ("test#05", "Rel", Test.ToString ());
			Assertion.AssertEquals ("test#06", "Rel", Test.RelationName);
			Assertion.AssertEquals ("test#07", "Mom", Test.ParentTable.TableName);
			
			Assertion.AssertEquals ("test#08", true, Test.ParentKeyConstraint == null);
						
			Assertion.AssertEquals ("test#10", 1, Test.ParentColumns.Length);
			Assertion.AssertEquals ("test#11", true, Test.Nested);
			Assertion.AssertEquals ("test#12", 0, Test.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#13", "Child", Test.ChildTable.TableName);
			Assertion.AssertEquals ("test#14", true, Test.ChildKeyConstraint == null);
			Assertion.AssertEquals ("test#15", 1, Test.ChildColumns.Length);
			
		}

		[Test]
		public void RelationFromSchema ()
		{
			DataSet Set = new DataSet ();
			Set.ReadXmlSchema ("System.Data/store.xsd");
			DataTable Table = Set.Tables [0];
			
			Assertion.AssertEquals ("test#01", false, Table.CaseSensitive);
			Assertion.AssertEquals ("test#02", 1, Table.ChildRelations.Count);
			Assertion.AssertEquals ("test#03", 0, Table.ParentRelations.Count);
			Assertion.AssertEquals ("test#04", 1, Table.Constraints.Count);
			Assertion.AssertEquals ("test#05", 1, Table.PrimaryKey.Length);
			Assertion.AssertEquals ("test#06", 0, Table.Rows.Count);
			Assertion.AssertEquals ("test#07", "bookstore", Table.TableName);
			Assertion.AssertEquals ("test#08", 1, Table.Columns.Count);
						
			DataRelation Relation = Table.ChildRelations [0];
			Assertion.AssertEquals ("test#09", 1, Relation.ChildColumns.Length);
			Assertion.AssertEquals ("test#10", "bookstore_book", Relation.ChildKeyConstraint.ConstraintName);
			Assertion.AssertEquals ("test#11", 1, Relation.ChildKeyConstraint.Columns.Length);
			Assertion.AssertEquals ("test#12", "book", Relation.ChildTable.TableName);
			Assertion.AssertEquals ("test#13", "NewDataSet", Relation.DataSet.DataSetName);
			Assertion.AssertEquals ("test#14", 0, Relation.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#15", true, Relation.Nested);
			Assertion.AssertEquals ("test#16", 1, Relation.ParentColumns.Length);
			Assertion.AssertEquals ("test#17", "Constraint1", Relation.ParentKeyConstraint.ConstraintName);
			Assertion.AssertEquals ("test#18", "bookstore", Relation.ParentTable.TableName);
			Assertion.AssertEquals ("test#19", "bookstore_book", Relation.RelationName);

			Table = Set.Tables [1];
			
			Assertion.AssertEquals ("test#20", false, Table.CaseSensitive);
			Assertion.AssertEquals ("test#21", 1, Table.ChildRelations.Count);
			Assertion.AssertEquals ("test#22", 1, Table.ParentRelations.Count);
			Assertion.AssertEquals ("test#23", 2, Table.Constraints.Count);
			Assertion.AssertEquals ("test#24", 1, Table.PrimaryKey.Length);
			Assertion.AssertEquals ("test#25", 0, Table.Rows.Count);
			Assertion.AssertEquals ("test#26", "book", Table.TableName);
			Assertion.AssertEquals ("test#27", 5, Table.Columns.Count);
		
			Relation = Table.ChildRelations [0];
			Assertion.AssertEquals ("test#28", 1, Relation.ChildColumns.Length);
			Assertion.AssertEquals ("test#29", "book_author", Relation.ChildKeyConstraint.ConstraintName);
			Assertion.AssertEquals ("test#30", 1, Relation.ChildKeyConstraint.Columns.Length);
			Assertion.AssertEquals ("test#31", "author", Relation.ChildTable.TableName);
			Assertion.AssertEquals ("test#32", "NewDataSet", Relation.DataSet.DataSetName);
			Assertion.AssertEquals ("test#33", 0, Relation.ExtendedProperties.Count);
			Assertion.AssertEquals ("test#34", true, Relation.Nested);
			Assertion.AssertEquals ("test#35", 1, Relation.ParentColumns.Length);
			Assertion.AssertEquals ("test#36", "Constraint1", Relation.ParentKeyConstraint.ConstraintName);
			Assertion.AssertEquals ("test#37", "book", Relation.ParentTable.TableName);
			Assertion.AssertEquals ("test#38", "book_author", Relation.RelationName);
			
			Table = Set.Tables [2];
			Assertion.AssertEquals ("test#39", false, Table.CaseSensitive);
			Assertion.AssertEquals ("test#40", 0, Table.ChildRelations.Count);
			Assertion.AssertEquals ("test#41", 1, Table.ParentRelations.Count);
			Assertion.AssertEquals ("test#42", 1, Table.Constraints.Count);
			Assertion.AssertEquals ("test#43", 0, Table.PrimaryKey.Length);
			Assertion.AssertEquals ("test#44", 0, Table.Rows.Count);
			Assertion.AssertEquals ("test#45", "author", Table.TableName);
			Assertion.AssertEquals ("test#46", 3, Table.Columns.Count);
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
			Assertion.AssertEquals ("test#01", "Dick", TempRow [0]);
			Assertion.AssertEquals ("test#02", "10", TempRow [1].ToString ());
			TempRow = TempRow.GetParentRow ("Rel");
			Assertion.AssertEquals ("test#03", "teresa", TempRow [0]);
			Assertion.AssertEquals ("test#04", "Dick", TempRow [1]);
			
			Row = Child.Rows [0];
			TempRow = Row.GetParentRows ("Rel") [0];
			Assertion.AssertEquals ("test#05", "teresa", TempRow [0]);
			Assertion.AssertEquals ("test#06", "john", TempRow [1]);						
		}

        }
}
