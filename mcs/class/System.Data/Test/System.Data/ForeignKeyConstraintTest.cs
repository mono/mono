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
				Assertion.Fail("A1: Wrong Exception type. " + exc.ToString());
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
				Assertion.Fail("A1: Wrong Exception type. " + exc.ToString());
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
				Assertion.Fail("A1: Wrong Exception type. " + exc.ToString());
			}

			



		}
	}
}
