// ConstraintCollection.cs - NUnit Test Cases for testing the ConstraintCollection 
//	class.
//	
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Franklin Wise
// (C) 2003 Martin Willemoes Hansen
// 


using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class ConstraintCollectionTest {
		private DataTable _table;
		private DataTable _table2;
		private Constraint _constraint1;
		private Constraint _constraint2;

		[SetUp]
		public void GetReady() 
		{
			//Setup DataTable
			_table = new DataTable("TestTable");
			_table.Columns.Add("Col1",typeof(int));
			_table.Columns.Add("Col2",typeof(int));
			_table.Columns.Add("Col3",typeof(int));

			_table2 = new DataTable("TestTable");
			_table2.Columns.Add("Col1",typeof(int));
			_table2.Columns.Add("Col2",typeof(int));

			//Use UniqueConstraint to test Constraint Base Class
			_constraint1 = new UniqueConstraint(_table.Columns[0],false); 
			_constraint2 = new UniqueConstraint(_table.Columns[1],false); 

			// not sure why this is needed since a new _table was just created
			// for us, but this Clear() keeps the tests from throwing
			// an exception when the Add() is called.
			_table.Constraints.Clear();
		}

		[Test]
		public void Add()
		{
			ConstraintCollection col = _table.Constraints;
			col.Add(_constraint1);
			col.Add(_constraint2);

			Assertion.AssertEquals("Count doesn't equal added.",2, col.Count);
		}

		[Test]
		public void AddExceptions()
		{
			ConstraintCollection col = _table.Constraints;
			
			//null
			try 
			{
				col.Add(null);
				Assertion.Fail("B1: Failed to throw ArgumentNullException.");
			}
			catch (ArgumentNullException) {}
			catch (AssertionException exc) {throw exc;}
			catch 
			{
				Assertion.Fail("A1: Wrong exception type");
			}

			//duplicate name
			try 
			{
				_constraint1.ConstraintName = "Dog";
				_constraint2.ConstraintName = "dog"; //case insensitive
				col.Add(_constraint1);
				col.Add(_constraint2);
				Assertion.Fail("Failed to throw Duplicate name exception.");
			}
			catch (DuplicateNameException) {}
			catch (AssertionException exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("A2: Wrong exception type. " + exc.ToString());
			}

			//Constraint Already exists
			try 
			{
				col.Add(_constraint1);
				Assertion.Fail("B2: Failed to throw ArgumentException.");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw exc;}
			catch 
			{
				Assertion.Fail("A3: Wrong exception type");
			}
		}

		[Test]
		public void Indexer()
		{
			Constraint c1 = new UniqueConstraint(_table.Columns[0]);
			Constraint c2 = new UniqueConstraint(_table.Columns[1]);

			c1.ConstraintName = "first";
			c2.ConstraintName = "second";


			_table.Constraints.Add(c1);
			_table.Constraints.Add(c2);

			Assertion.AssertSame("A1", c1, _table.Constraints[0]); 
			Assertion.AssertSame("A2", c2, _table.Constraints[1]);

			Assertion.AssertSame("A3", c1, _table.Constraints["first"]);
			Assertion.AssertSame("A4", c2, _table.Constraints["sEcond"]); //case insensitive

		}

		[Test]
		public void IndexOf()
		{
			Constraint c1 = new UniqueConstraint(_table.Columns[0]);
			Constraint c2 = new UniqueConstraint(_table.Columns[1]);

			c1.ConstraintName = "first";
			c2.ConstraintName = "second";

			_table.Constraints.Add(c1);
			_table.Constraints.Add(c2);

			Assertion.AssertEquals("A1", 0, _table.Constraints.IndexOf(c1));
			Assertion.AssertEquals("A2", 1, _table.Constraints.IndexOf(c2));
			Assertion.AssertEquals("A3", 0, _table.Constraints.IndexOf("first"));
			Assertion.AssertEquals("A4", 1, _table.Constraints.IndexOf("second"));
		}

		[Test]
		public void Contains()
		{
			Constraint c1 = new UniqueConstraint(_table.Columns[0]);
			Constraint c2 = new UniqueConstraint(_table.Columns[1]);

			c1.ConstraintName = "first";
			c2.ConstraintName = "second";

			_table.Constraints.Add(c1);

			Assertion.Assert("A1", _table.Constraints.Contains(c1.ConstraintName)); //true
			Assertion.Assert("A2", _table.Constraints.Contains(c2.ConstraintName) == false); //doesn't contain
		}

		[Test]
		public void IndexerFailures()
		{
			_table.Constraints.Add(new UniqueConstraint(_table.Columns[0]));

			//This doesn't throw
			Assertion.AssertNull(_table.Constraints["notInCollection"]);
			
			//Index too high
			try 
			{
				Constraint c = _table.Constraints[_table.Constraints.Count];
				Assertion.Fail("B1: Failed to throw IndexOutOfRangeException.");
			}
			catch (IndexOutOfRangeException) {}
			catch (AssertionException exc) {throw exc;}
			catch 
			{
				Assertion.Fail("A1: Wrong exception type");
			}

			//Index too low
			try 
			{
				Constraint c = _table.Constraints[-1];
				Assertion.Fail("B2: Failed to throw IndexOutOfRangeException.");
			}
			catch (IndexOutOfRangeException) {}
			catch (AssertionException exc) {throw exc;}
			catch 
			{
				Assertion.Fail("A2: Wrong exception type");
			}	

		}

		//TODO: Implementation not ready for this test yet
		//[Test]
//		public void AddFkException1()
//		{
//			DataSet ds = new DataSet();
//			ds.Tables.Add(_table);
//			ds.Tables.Add(_table2);
//
//			_table.Rows.Add(new object [] {1});
//			_table.Rows.Add(new object [] {1});
//
//			//FKC: can't create unique constraint because duplicate values already exist
//			try
//			{
//				ForeignKeyConstraint fkc = new ForeignKeyConstraint( _table.Columns[0],
//											_table2.Columns[0]);
//				
//				_table2.Constraints.Add(fkc);	//should throw			
//				Assertion.Fail("B1: Failed to throw ArgumentException.");
//			}
//			catch (ArgumentException) {}
//			catch (AssertionException exc) {throw exc;}
//			catch (Exception exc)
//			{
//				Assertion.Fail("A1: Wrong Exception type. " + exc.ToString());
//			}
//
//
//		}


		//TODO: Implementation not ready for this test yet
		//[Test]
//		public void AddFkException2()
//		{
//			//Foreign key rules only work when the tables
//			//are apart of the dataset
//			DataSet ds = new DataSet();
//			ds.Tables.Add(_table);
//			ds.Tables.Add(_table2);
//
//			_table.Rows.Add(new object [] {1});
//			
//			// will need a matching parent value in 
//			// _table
//			_table2.Rows.Add(new object [] {3}); 
//								
//
//			//FKC: no matching parent value
//			try
//			{
//				ForeignKeyConstraint fkc = new ForeignKeyConstraint( _table.Columns[0],
//					_table2.Columns[0]);
//				
//				_table2.Constraints.Add(fkc);	//should throw			
//				Assertion.Fail("B1: Failed to throw ArgumentException.");
//			}
//			catch (ArgumentException) {}
//			catch (AssertionException exc) {throw exc;}
//			catch (Exception exc)
//			{
//				Assertion.Fail("A1: Wrong Exception type. " + exc.ToString());
//			}
//
//
//		}


		//TODO: Implementation not ready for this test yet
		//[Test]
//		public void AddUniqueExceptions()
//		{
//			
//
//			//UC: can't create unique constraint because duplicate values already exist
//			try
//			{
//				_table.Rows.Add(new object [] {1});
//				_table.Rows.Add(new object [] {1});
//				UniqueConstraint uc = new UniqueConstraint( _table.Columns[0]);
//				
//				_table.Constraints.Add(uc);	//should throw			
//				Assertion.Fail("B1: Failed to throw ArgumentException.");
//			}
//			catch (ArgumentException) {}
//			catch (AssertionException exc) {throw exc;}
//			catch (Exception exc)
//			{
//				Assertion.Fail("A1: Wrong Exception type. " + exc.ToString());
//			}
//		}

		[Test]
		public void AddRange()
		{
		}

		[Test]
		public void Clear()
		{
		}

		[Test]
		public void CanRemove()
		{
		}

		[Test]
		public void CollectionChanged()
		{
		}

		[Test]
		public void RemoveAt()
		{
		}

		[Test]
		public void Remove()
		{
		}

		[Test]
		public void RemoveExceptions()
		{
		}
	}
}
