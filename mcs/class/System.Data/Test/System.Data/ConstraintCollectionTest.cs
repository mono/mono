// ConstraintCollection.cs - NUnit Test Cases for testing the ConstraintCollection 
//	class.
//	
//
// Franklin Wise (gracenote@earthlink.net)
//
// (C) Franklin Wise
// 


using NUnit.Framework;
using System;
using System.Data;


namespace MonoTests.System.Data
{


	public class ConstraintCollectionTest : TestCase 
	{
		private DataTable _table;
		private Constraint _constraint1;
		private Constraint _constraint2;

		public ConstraintCollectionTest() : base ("MonoTests.System.Data.ConstraintCollectionTest") {}
		public ConstraintCollectionTest(string name) : base(name) {}

		public void PublicSetup(){SetUp();}
		protected override void SetUp() 
		{
			//Setup DataTable
			_table = new DataTable("TestTable");
			_table.Columns.Add("Col1",typeof(int));
			_table.Columns.Add("Col2",typeof(int));

			//Use UniqueConstraint to test Constraint Base Class
			_constraint1 = new UniqueConstraint(_table.Columns[0],false); 
			_constraint2 = new UniqueConstraint(_table.Columns[1],false); 
		}

		protected override void TearDown() {}

		public static ITest Suite 
		{
			get 
			{ 
				return new TestSuite(typeof(ConstraintCollectionTest)); 
			}
		}

		public void TestAdd()
		{
			ConstraintCollection col = _table.Constraints;
			col.Add(_constraint1);
			col.Add(_constraint2);

			Assertion.AssertEquals("Count doesn't equal added.",2, col.Count);
		}

		public void TestAddExceptions()
		{
			ConstraintCollection col = _table.Constraints;
			try 
			{
				col.Add(null);
				Assertion.Fail("B1: Failed to throw ArgumentNullException.");
			}
			catch (ArgumentNullException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch 
			{
				Assertion.Fail("A1: Wrong exception type");
			}

			try 
			{
				_constraint1.ConstraintName = "Dog";
				_constraint2.ConstraintName = "dog"; //case insensitive
				col.Add(_constraint1);
				col.Add(_constraint2);
				Assertion.Fail("Failed to throw Duplicate name exception.");
			}
			catch (DuplicateNameException) {}
			catch (AssertionFailedError exc) {throw exc;}
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
			catch (AssertionFailedError exc) {throw exc;}
			catch 
			{
				Assertion.Fail("A3: Wrong exception type");
			}
		}

		public void TestRemoveExceptions()
		{


		}
	}
}
