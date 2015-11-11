// ForeignKeyConstraintTest.cs - NUnit Test Cases for [explain here]
//
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Franklin Wise
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
	public class ForeignKeyConstraintTest : Assertion
	{
		private DataSet _ds;

		//NOTE: fk constraints only work when the table is part of a DataSet

		[SetUp]
		public void GetReady() 
		{
			_ds = new DataSet();

			//Setup DataTable
			DataTable table;
			table = new DataTable("TestTable");
			table.Columns.Add("Col1",typeof(int));
			table.Columns.Add("Col2",typeof(int));
			table.Columns.Add("Col3",typeof(int));

			_ds.Tables.Add(table);

			table = new DataTable("TestTable2");
			table.Columns.Add("Col1",typeof(int));
			table.Columns.Add("Col2",typeof(int));
			table.Columns.Add("Col3",typeof(int));

			_ds.Tables.Add(table);

		}

		// Tests ctor (string, DataColumn, DataColumn)
		[Test]
		public void Ctor1 ()
		{
			DataTable Table =  _ds.Tables [0];
			
			AssertEquals ("test#01", 0, Table.Constraints.Count);
			Table =  _ds.Tables [1];
			AssertEquals ("test#02", 0, Table.Constraints.Count);
			
			// ctor (string, DataColumn, DataColumn
			ForeignKeyConstraint Constraint = new ForeignKeyConstraint ("test", _ds.Tables [0].Columns [2], _ds.Tables [1].Columns [0]);
			Table = _ds.Tables [1];
			Table.Constraints.Add (Constraint);
			
			AssertEquals ("test#03", 1, Table.Constraints.Count);
			AssertEquals ("test#04", "test", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#05", typeof (ForeignKeyConstraint), Table.Constraints [0].GetType ());

			Table = _ds.Tables [0];
			AssertEquals ("test#06", 1, Table.Constraints.Count);
			AssertEquals ("test#07", "Constraint1", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#08", typeof (UniqueConstraint), Table.Constraints [0].GetType ());
		}
		
		// Tests ctor (DataColumn, DataColumn)
		[Test]
		public void Ctor2 ()
		{
			DataTable Table =  _ds.Tables [0];
			
			AssertEquals ("test#01", 0, Table.Constraints.Count);
			Table =  _ds.Tables [1];
			AssertEquals ("test#02", 0, Table.Constraints.Count);
			
			// ctor (string, DataColumn, DataColumn
			ForeignKeyConstraint Constraint = new ForeignKeyConstraint (_ds.Tables [0].Columns [2], _ds.Tables [1].Columns [0]);
			Table = _ds.Tables [1];
			Table.Constraints.Add (Constraint);
			
			AssertEquals ("test#03", 1, Table.Constraints.Count);
			AssertEquals ("test#04", "Constraint1", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#05", typeof (ForeignKeyConstraint), Table.Constraints [0].GetType ());

			Table = _ds.Tables [0];
			AssertEquals ("test#06", 1, Table.Constraints.Count);
			AssertEquals ("test#07", "Constraint1", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#08", typeof (UniqueConstraint), Table.Constraints [0].GetType ());
		}
		
		// Test ctor (DataColumn [], DataColumn [])
		[Test]
		public void Ctor3 ()
		{
			DataTable Table =  _ds.Tables [0];
			
			AssertEquals ("test#01", 0, Table.Constraints.Count);
			Table =  _ds.Tables [1];
			AssertEquals ("test#02", 0, Table.Constraints.Count);
						
			DataColumn [] Cols1 = new DataColumn [2];
			Cols1 [0] = _ds.Tables [0].Columns [1];
			Cols1 [1] = _ds.Tables [0].Columns [2];
			
			DataColumn [] Cols2 = new DataColumn [2];
			Cols2 [0] = _ds.Tables [1].Columns [0];
			Cols2 [1] = _ds.Tables [1].Columns [1];
			
			ForeignKeyConstraint Constraint = new ForeignKeyConstraint (Cols1, Cols2);
			Table = _ds.Tables [1];
			Table.Constraints.Add (Constraint);
			
			AssertEquals ("test#03", 1, Table.Constraints.Count);
			AssertEquals ("test#04", "Constraint1", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#05", typeof (ForeignKeyConstraint), Table.Constraints [0].GetType ());

			Table = _ds.Tables [0];
			AssertEquals ("test#06", 1, Table.Constraints.Count);
			AssertEquals ("test#07", "Constraint1", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#08", typeof (UniqueConstraint), Table.Constraints [0].GetType ());

		}
	
		// Tests ctor (string, DataColumn [], DataColumn [])	
		[Test]
		public void Ctor4 ()
		{
			DataTable Table =  _ds.Tables [0];
			
			AssertEquals ("test#01", 0, Table.Constraints.Count);
			Table =  _ds.Tables [1];
			AssertEquals ("test#02", 0, Table.Constraints.Count);
						
			DataColumn [] Cols1 = new DataColumn [2];
			Cols1 [0] = _ds.Tables [0].Columns [1];
			Cols1 [1] = _ds.Tables [0].Columns [2];
			
			DataColumn [] Cols2 = new DataColumn [2];
			Cols2 [0] = _ds.Tables [1].Columns [0];
			Cols2 [1] = _ds.Tables [1].Columns [1];
			
			ForeignKeyConstraint Constraint = new ForeignKeyConstraint ("Test", Cols1, Cols2);
			Table = _ds.Tables [1];
			Table.Constraints.Add (Constraint);
			
			AssertEquals ("test#03", 1, Table.Constraints.Count);
			AssertEquals ("test#04", "Test", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#05", typeof (ForeignKeyConstraint), Table.Constraints [0].GetType ());

			Table = _ds.Tables [0];
			AssertEquals ("test#06", 1, Table.Constraints.Count);
			AssertEquals ("test#07", "Constraint1", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#08", typeof (UniqueConstraint), Table.Constraints [0].GetType ());			
		}
		
		[Test]
                public void TestCtor5()
                {
                        DataTable table1 = new DataTable ("Table1");
                        DataTable table2 = new DataTable ("Table2");
                        DataSet dataSet = new DataSet();
                        dataSet.Tables.Add (table1);
                        dataSet.Tables.Add (table2);
                        DataColumn column1 = new DataColumn ("col1");
                        DataColumn column2 = new DataColumn ("col2");
                        DataColumn column3 = new DataColumn ("col3");
                        table1.Columns.Add (column1);
                        table1.Columns.Add (column2);
                        table1.Columns.Add (column3);
                        DataColumn column4 = new DataColumn ("col4");
                        DataColumn column5 = new DataColumn ("col5");
                        DataColumn column6 = new DataColumn ("col6");
                        table2.Columns.Add (column4);
                        table2.Columns.Add (column5);                         
			table2.Columns.Add (column6);
                        string []parentColumnNames = {"col1", "col2", "col3"};
                        string []childColumnNames = {"col4", "col5", "col6"};
                        string parentTableName = "table1";
		
			// Create a ForeingKeyConstraint Object using the constructor
                        // ForeignKeyConstraint (string, string, string[], string[], AcceptRejectRule, Rule, Rule);
			 ForeignKeyConstraint fkc = new ForeignKeyConstraint ("hello world", parentTableName, parentColumnNames, childColumnNames, AcceptRejectRule.Cascade, Rule.Cascade, Rule.Cascade);                                                                                                                            // Assert that the Constraint object does not belong to any table yet
			try {
				DataTable tmp = fkc.Table;
				Fail ("When table is null, get_Table causes an InvalidOperationException.");
			} catch (NullReferenceException) { // actually .NET throws this (bug)
			} catch (InvalidOperationException) {
			}
                                                                                                    
                        Constraint []constraints = new Constraint[3];
                        constraints [0] = new UniqueConstraint (column1);
                        constraints [1] = new UniqueConstraint (column2);
                        constraints [2] = fkc;
                                                                                                    
                        // Try to add the constraint to ConstraintCollection of the DataTable through Add()
                        try{
                                table2.Constraints.Add (fkc);
                                throw new ApplicationException ("An Exception was expected");
                        }
                        // LAMESPEC : spec says InvalidConstraintException but throws this
                        catch (NullReferenceException) {
                        }

#if false // FIXME: Here this test crashes under MS.NET.
                        // OK - So AddRange() is the only way!
                        table2.Constraints.AddRange (constraints);
			   // After AddRange(), Check the properties of ForeignKeyConstraint object
                        Assertion.Assert("#A04", fkc.RelatedColumns [0].ColumnName.Equals ("col1"));                        Assertion.Assert("#A05", fkc.RelatedColumns [1].ColumnName.Equals ("col2"));                        Assertion.Assert("#A06", fkc.RelatedColumns [2].ColumnName.Equals ("col3"));                        Assertion.Assert("#A07", fkc.Columns [0].ColumnName.Equals ("col4"));
                        Assertion.Assert("#A08", fkc.Columns [1].ColumnName.Equals ("col5"));
                        Assertion.Assert("#A09", fkc.Columns [2].ColumnName.Equals ("col6"));
#endif
                        // Try to add columns with names which do not exist in the table
                        parentColumnNames [2] = "noColumn";
                        ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint ("hello world", parentTableName, parentColumnNames, childColumnNames, AcceptRejectRule.Cascade, Rule.Cascade, Rule.Cascade);
                        constraints [0] = new UniqueConstraint (column1);
                        constraints [1] = new UniqueConstraint (column2);
                        constraints [2] = foreignKeyConstraint;
                        try{
                                table2.Constraints.AddRange (constraints);
                                throw new ApplicationException ("An Exception was expected");
                        }
                        catch (ArgumentException e) {
                        }
                        catch (InvalidConstraintException e){ // Could not test on ms.net, as ms.net does not reach here so far.        
                        }
                        
#if false // FIXME: Here this test crashes under MS.NET.
                        // Check whether the child table really contains the foreign key constraint named "hello world"
                        Assertion.Assert("#A11 ", table2.Constraints.Contains ("hello world"));
#endif
                }



		//  If Childs and parents are in same table
		[Test]
		public void KeyBetweenColumns ()
		{
			DataTable Table =  _ds.Tables [0];
			
			AssertEquals ("test#01", 0, Table.Constraints.Count);
			Table =  _ds.Tables [1];
			AssertEquals ("test#02", 0, Table.Constraints.Count);
						
			
			ForeignKeyConstraint Constraint = new ForeignKeyConstraint ("Test", _ds.Tables [0].Columns [0], _ds.Tables [0].Columns [2]);
			Table = _ds.Tables [0];
			Table.Constraints.Add (Constraint);
			
			AssertEquals ("test#03", 2, Table.Constraints.Count);
			AssertEquals ("test#04", "Constraint1", Table.Constraints [0].ConstraintName);
			AssertEquals ("test#05", typeof (UniqueConstraint), Table.Constraints [0].GetType ());
			AssertEquals ("test#04", "Test", Table.Constraints [1].ConstraintName);
			AssertEquals ("test#05", typeof (ForeignKeyConstraint), Table.Constraints [1].GetType ());

		}

		[Test]
		public void CtorExceptions ()
		{
			ForeignKeyConstraint fkc;

			DataTable localTable = new DataTable();
			localTable.Columns.Add("Col1",typeof(int));
			localTable.Columns.Add("Col2",typeof(bool));

			//Null
			try
			{
				fkc = new ForeignKeyConstraint((DataColumn)null,(DataColumn)null);
				Fail("Failed to throw ArgumentNullException.");
			}
			catch (NullReferenceException) {}
			catch (AssertionException exc) {throw exc;}
			catch (Exception exc)
			{
				Fail("A1: Wrong Exception type. " + exc.ToString());
			}

			//zero length collection
			try
			{
				fkc = new ForeignKeyConstraint(new DataColumn[]{},new DataColumn[]{});
				Fail("B1: Failed to throw ArgumentException.");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw exc;}
			catch (Exception exc)
			{
				Fail("A2: Wrong Exception type. " + exc.ToString());
			}

			//different datasets
			try
			{
				fkc = new ForeignKeyConstraint(_ds.Tables[0].Columns[0], localTable.Columns[0]);
				Fail("Failed to throw InvalidOperationException.");
			}
			catch (InvalidOperationException) {}
			catch (AssertionException exc) {throw exc;}
			catch (Exception exc)
			{
				Fail("A3: Wrong Exception type. " + exc.ToString());
			}

			try
			{
				fkc = new ForeignKeyConstraint(_ds.Tables[0].Columns[0], localTable.Columns[1]);
				Fail("Failed to throw InvalidConstraintException.");
			}
			// tables in different datasets
			catch (InvalidOperationException) {}

			// Cannot create a Key from Columns that belong to
			// different tables.
                        try                                           
                        {                                             
                                fkc = new ForeignKeyConstraint(new DataColumn [] {_ds.Tables[0].Columns[0], _ds.Tables[0].Columns[1]}, new DataColumn [] {localTable.Columns[1], _ds.Tables[1].Columns [0]});    
                                Fail("Failed to throw InvalidOperationException.");                                         
                        }                                             
                        catch (InvalidConstraintException) {}         
                        catch (AssertionException exc) {throw exc;} 
		}

		[Test]
		public void CtorExceptions2 () 
		{
			DataColumn col = new DataColumn("MyCol1",typeof(int));

			ForeignKeyConstraint fkc;
			
			//Columns must belong to a Table
			try 
			{
				fkc = new ForeignKeyConstraint(col, _ds.Tables[0].Columns[0]);
				Fail("FTT1: Failed to throw ArgumentException.");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw exc;}
//			catch (Exception exc)
//			{
//				Fail("WET1: Wrong Exception type. " + exc.ToString());
//			}

			//Columns must belong to the same table
			//InvalidConstraintException
			
			DataColumn [] difTable = new DataColumn [] {_ds.Tables[0].Columns[2],
									   _ds.Tables[1].Columns[0]};
			try 
			{
				fkc = new ForeignKeyConstraint(difTable,new DataColumn[] {
								 _ds.Tables[0].Columns[1],
								_ds.Tables[0].Columns[0]});
					
				Fail("FTT2: Failed to throw InvalidConstraintException.");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionException exc) {throw exc;}
			catch (Exception exc)
			{
				Fail("WET2: Wrong Exception type. " + exc.ToString());
			}


			//parent columns and child columns should be the same length
			//ArgumentException
			DataColumn [] twoCol = 
				new DataColumn [] {_ds.Tables[0].Columns[0],_ds.Tables[0].Columns[1]};
							  

			try 
			{
				fkc = new ForeignKeyConstraint(twoCol, 
					new DataColumn[] { _ds.Tables[0].Columns[0]});
					
				Fail("FTT3: Failed to throw ArgumentException.");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw exc;}
			catch (Exception exc)
			{
				Fail("WET3: Wrong Exception type. " + exc.ToString());
			}

			//InvalidOperation: Parent and child are the same column.
			try 
			{
				fkc = new ForeignKeyConstraint( _ds.Tables[0].Columns[0],
					_ds.Tables[0].Columns[0] );
					
				Fail("FTT4: Failed to throw InvalidOperationException.");
			}
			catch (InvalidOperationException) {}
			catch (AssertionException exc) {throw exc;}
			catch (Exception exc)
			{
				Fail("WET4: Wrong Exception type. " + exc.ToString());
			}

		}

		[Test]
		public void EqualsAndHashCode()
		{
			DataTable tbl = _ds.Tables[0];
			DataTable tbl2 = _ds.Tables[1];

			ForeignKeyConstraint fkc = new ForeignKeyConstraint( 
				new DataColumn[] {tbl.Columns[0], tbl.Columns[1]} ,
				new DataColumn[] {tbl2.Columns[0], tbl2.Columns[1]} );

			ForeignKeyConstraint fkc2 = new ForeignKeyConstraint( 
				new DataColumn[] {tbl.Columns[0], tbl.Columns[1]} ,
				new DataColumn[] {tbl2.Columns[0], tbl2.Columns[1]} );

			ForeignKeyConstraint fkcDiff = 
				new ForeignKeyConstraint( tbl.Columns[1], tbl.Columns[2]);
		
			Assert( "Equals failed. 1" , fkc.Equals(fkc2));
			Assert( "Equals failed. 2" , fkc2.Equals(fkc));
			Assert( "Equals failed. 3" , fkc.Equals(fkc));

			Assert( "Equals failed diff. 1" , fkc.Equals(fkcDiff) == false);

			//Assert( "Hash Code Failed. 1", fkc.GetHashCode() == fkc2.GetHashCode() );
			Assert( "Hash Code Failed. 2", fkc.GetHashCode() != fkcDiff.GetHashCode() );
	
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ViolationTest ()
		{
			DataTable parent = _ds.Tables [0];
			DataTable child  = _ds.Tables [1];
			
			parent.Rows.Add (new object [] {1, 1, 1});
			child.Rows.Add (new object [] {2, 2, 2});

			try {
				child.Constraints.Add (new ForeignKeyConstraint ( parent.Columns [0],
										  child.Columns [0])
						       );
			} finally {
				// clear the rows for further testing
				_ds.Clear ();
			}
		}

		[Test]
		public void NoViolationTest ()
		{
			DataTable parent = _ds.Tables [0];
			DataTable child  = _ds.Tables [1];
			
			parent.Rows.Add (new object [] {1, 1, 1});
			child.Rows.Add (new object [] {2, 2, 2});

			try {
				_ds.EnforceConstraints = false;
				child.Constraints.Add (new ForeignKeyConstraint ( parent.Columns [0],
										  child.Columns [0])
						       );
			} finally {
				// clear the rows for further testing
				_ds.Clear ();
				_ds.EnforceConstraints = true;
			}
		}

		[Test]
		public void ModifyParentKeyBeforeAcceptChanges ()
		{
			DataSet ds1 = new DataSet();
			DataTable t1= ds1.Tables.Add ("t1");
			DataTable t2= ds1.Tables.Add ("t2");
			t1.Columns.Add ("col1", typeof (int));
			t2.Columns.Add ("col2", typeof (int));
			ds1.Relations.Add ("fk", t1.Columns [0], t2.Columns [0]);

			t1.Rows.Add (new object[] {10});
			t2.Rows.Add (new object [] {10});

			t1.Rows [0][0]=20;
			Assert("#1", (int)t2.Rows [0][0] == 20);
		}

		[Test]
		// https://bugzilla.novell.com/show_bug.cgi?id=650402
		public void ForeignKey_650402 ()
		{
			DataSet data = new DataSet ();
			DataTable parent = new DataTable ("parent");
			DataColumn pk = parent.Columns.Add ("PK");
			DataTable child = new DataTable ("child");
			DataColumn fk = child.Columns.Add ("FK");
			
			data.Tables.Add (parent);
			data.Tables.Add (child);
			data.Relations.Add (pk, fk);
			
			parent.Rows.Add ("value");
			child.Rows.Add ("value");
			data.AcceptChanges ();
			child.Rows[0].Delete ();
			parent.Rows[0][0] = "value2";
			
			data.EnforceConstraints = false;
			data.EnforceConstraints = true;
		}
	}
}
