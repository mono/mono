//
// DataRelationTest.cs - NUnit Test Cases for  DataRelation
//
// Ville Palo (vi64pa@koti.soon.fi)
//
// (C) Ville Palo 2003
// 

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{

        public class DataRelationTest : TestCase 
        {
                public DataRelationTest() : base ("MonoTests.System.Data.DataRelation") {}
                public DataRelationTest(string name) : base(name) {}

                protected override void SetUp() {}  
                
                protected override void TearDown() {}

                public static ITest Suite {
                        get { 
                                return new TestSuite(typeof(DataRelationTest)); 
                        }
                }

		public void TestCreation ()
		{
			DataSet Set = new DataSet ();
			DataTable Mom = new DataTable ("Mom");
			DataTable Child = new DataTable ("Child");
			Set.Tables.Add (Mom);
			Set.Tables.Add (Child);
			
			DataColumn Col = new DataColumn ("Name");
			DataColumn Col2 = new DataColumn ("ChildName");
			Mom.Columns.Add (Col);
			Mom.Columns.Add (Col2);
			
			DataColumn Col3 = new DataColumn ("Name");
			DataColumn Col4 = new DataColumn ("Age");
			Child.Columns.Add (Col3);
			Child.Columns.Add (Col4);
			
			DataRelation Relation = new DataRelation ("Rel", Col2, Col3);
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
		
		public void TestRelationFromSchema ()
		{
			DataSet Set = new DataSet ();
			Set.ReadXmlSchema ("System.Data/store.xsd");
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
		
		public void TestChildRows ()
		{
			DataSet Set = new DataSet ();
			DataTable Mom = new DataTable ("Mom");
			DataTable Child = new DataTable ("Child");
			Set.Tables.Add (Mom);
			Set.Tables.Add (Child);
			
			DataColumn Col = new DataColumn ("Name");
			DataColumn Col2 = new DataColumn ("ChildName");
			Mom.Columns.Add (Col);
			Mom.Columns.Add (Col2);
			
			DataColumn Col3 = new DataColumn ("Name");
			DataColumn Col4 = new DataColumn ("Age");
			Child.Columns.Add (Col3);
			Child.Columns.Add (Col4);
			
			DataRelation Relation = new DataRelation ("Rel", Col2, Col3);
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
			AssertEquals ("test#02", "10", TempRow [1]);
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
