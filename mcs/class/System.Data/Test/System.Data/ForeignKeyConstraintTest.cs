// ForeignKeyConstraintTest.cs - NUnit Test Cases for [explain here]
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

	public class ForeignKeyConstraintTest : TestCase 
	{
		private DataSet _ds;

		//NOTE: fk constraints only work when the table is part of a DataSet

		public ForeignKeyConstraintTest() : base ("MonoTests.System.Data.ForeignKeyConstraintTest") {}
		public ForeignKeyConstraintTest(string name) : base(name) {}

		protected override void SetUp() 
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

		protected override void TearDown() {}

		public static ITest Suite 
		{
			get 
			{ 
				return new TestSuite(typeof(ForeignKeyConstraintTest)); 
			}
		}


		public void TestCtorExceptions ()
		{
			ForeignKeyConstraint fkc;

			DataTable localTable = new DataTable();
			localTable.Columns.Add("Col1",typeof(int));
			localTable.Columns.Add("Col2",typeof(bool));

			//Null
			try
			{
				fkc = new ForeignKeyConstraint((DataColumn)null,(DataColumn)null);
				Assertion.Fail("Failed to throw ArgumentNullException.");
			}
			catch (ArgumentNullException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("A1: Wrong Exception type. " + exc.ToString());
			}

			//zero length collection
			try
			{
				fkc = new ForeignKeyConstraint(new DataColumn[]{},new DataColumn[]{});
				Assertion.Fail("B1: Failed to throw ArgumentException.");
			}
			catch (ArgumentException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("A2: Wrong Exception type. " + exc.ToString());
			}

			//different datasets
			try
			{
				fkc = new ForeignKeyConstraint(_ds.Tables[0].Columns[0], localTable.Columns[0]);
				Assertion.Fail("Failed to throw InvalidOperationException.");
			}
			catch (InvalidOperationException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("A3: Wrong Exception type. " + exc.ToString());
			}

			//different dataTypes
			try
			{
				fkc = new ForeignKeyConstraint(_ds.Tables[0].Columns[0], localTable.Columns[1]);
				Assertion.Fail("Failed to throw InvalidOperationException.");
			}
			catch (InvalidOperationException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("A4: Wrong Exception type. " + exc.ToString());
			}


		}
		public void TestCtorExceptions2 () 
		{
			DataColumn col = new DataColumn("MyCol1",typeof(int));

			ForeignKeyConstraint fkc;
			
			//Columns must belong to a Table
			try 
			{
				fkc = new ForeignKeyConstraint(col, _ds.Tables[0].Columns[0]);
				Assertion.Fail("FTT1: Failed to throw ArgumentException.");
			}
			catch (ArgumentException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("WET1: Wrong Exception type. " + exc.ToString());
			}

			//Columns must belong to the same table
			//InvalidConstraintException
			
			DataColumn [] difTable = new DataColumn [] {_ds.Tables[0].Columns[2],
									   _ds.Tables[1].Columns[0]};
			try 
			{
				fkc = new ForeignKeyConstraint(difTable,new DataColumn[] {
								 _ds.Tables[0].Columns[1],
								_ds.Tables[0].Columns[0]});
					
				Assertion.Fail("FTT2: Failed to throw InvalidConstraintException.");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("WET2: Wrong Exception type. " + exc.ToString());
			}


			//parent columns and child columns should be the same length
			//ArgumentException
			DataColumn [] twoCol = 
				new DataColumn [] {_ds.Tables[0].Columns[0],_ds.Tables[0].Columns[1]};
							  

			try 
			{
				fkc = new ForeignKeyConstraint(twoCol, 
					new DataColumn[] { _ds.Tables[0].Columns[0]});
					
				Assertion.Fail("FTT3: Failed to throw ArgumentException.");
			}
			catch (ArgumentException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("WET3: Wrong Exception type. " + exc.ToString());
			}

			//InvalidOperation: Parent and child are the same column.
			try 
			{
				fkc = new ForeignKeyConstraint( _ds.Tables[0].Columns[0],
					_ds.Tables[0].Columns[0] );
					
				Assertion.Fail("FTT4: Failed to throw InvalidOperationException.");
			}
			catch (InvalidOperationException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch (Exception exc)
			{
				Assertion.Fail("WET4: Wrong Exception type. " + exc.ToString());
			}

		}

		public void TestEquals()
		{
			//TODO:
		}
	}
}
