// UniqueConstraintTest.cs - NUnit Test Cases for testing the class System.Data.UniqueConstraint
//
// Authors:
//   Franklin Wise <gracenote@earthlink.net>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Franklin Wise
// (C) 2003 Martin Willemoes Hansen
// 

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class UniqueConstraintTest
	{
		private DataTable _table;

		[SetUp]
		public void GetReady() {

			//Setup DataTable
			_table = new DataTable("TestTable");
			_table.Columns.Add("Col1",typeof(int));
			_table.Columns.Add("Col2",typeof(int));
			_table.Columns.Add("Col3",typeof(int));

		}  

		[Test]
		public void CtorExceptions() {
			//UniqueConstraint(string name, DataColumn column, bool isPrimaryKey)

			UniqueConstraint cst;
			
			//must have DataTable exception
			try{
				//Should throw an ArgumentException
				//Can only add DataColumns that are attached
				//to a DataTable
				cst = new UniqueConstraint(new DataColumn(""));

				Assertion.Fail("Failed to throw ArgumentException.");
			} 
			catch (Exception e) {
				Assertion.AssertEquals ("test#02", typeof (ArgumentException), e.GetType ());
                        	Assertion.AssertEquals ("test#03", "Column must belong to a table.", e.Message);
                        }        

			//Null exception
			try {
				//Should throw argument null exception
				cst = new UniqueConstraint((DataColumn)null);
			}
                        catch (Exception e) {
                        	Assertion.AssertEquals ("test#05", typeof (NullReferenceException), e.GetType ());
                                Assertion.AssertEquals ("test#06", "Object reference not set to an instance of an object.", e.Message);
                        }
			
			try {
				//Should throw exception
				//must have at least one valid column
				//InvalidConstraintException is thrown by msft ver
				cst = new UniqueConstraint(new DataColumn [] {});

				Assertion.Fail("B1: Failed to throw InvalidConstraintException.");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionException exc) {throw exc;}
			catch {
				Assertion.Fail("A3: Wrong Exception type.");
			}

			DataTable dt = new DataTable("Table1");
			dt.Columns.Add("Col1",typeof(int));
			DataTable dt2 = new DataTable("Table2");
			dt2.Columns.Add("Col1",typeof(int));

			DataSet ds = new DataSet();
			ds.Tables.Add(dt);
			ds.Tables.Add(dt2);

			//columns from two different tables.
			try {
				//next line should throw
				//can't have columns from two different tables
				cst = new UniqueConstraint(new DataColumn [] { 
						 dt.Columns[0], dt2.Columns[0]});

				Assertion.Fail("B2: Failed to throw InvalidConstraintException");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionException exc) {throw exc;}
			catch {
				Assertion.Fail("A4: Wrong Exception type.");
			}
			

			
		}

		[Test]
		public void Ctor() {
			
			UniqueConstraint cst;
		
			//Success case
			try {
				cst = new UniqueConstraint(_table.Columns[0]);
			}
			catch (Exception exc) {
				Assertion.Fail("A1: Failed to ctor. " + exc.ToString());
			}

			
			try {
				cst = new UniqueConstraint( new DataColumn [] {
						_table.Columns[0], _table.Columns[1]});
			}
			catch (Exception exc) {
				Assertion.Fail("A2: Failed to ctor. " + exc.ToString());
			}

			
			//table is set on ctor
			cst = new UniqueConstraint(_table.Columns[0]);
			
			Assertion.AssertSame("B1", cst.Table, _table);

			//table is set on ctor
			cst = new UniqueConstraint( new DataColumn [] {
				      _table.Columns[0], _table.Columns[1]});
			Assertion.AssertSame ("B2", cst.Table, _table);

			cst = new UniqueConstraint("MyName",_table.Columns[0],true);

			//Test ctor parm set for ConstraintName & IsPrimaryKey
			Assertion.AssertEquals("ConstraintName not set in ctor.", 
				"MyName", cst.ConstraintName);
                        Assertion.AssertEquals("IsPrimaryKey already set.",
                                false, cst.IsPrimaryKey);
                
			_table.Constraints.Add (cst);

                        Assertion.AssertEquals("IsPrimaryKey not set set.",
                                true, cst.IsPrimaryKey);
                	
                	Assertion.AssertEquals("PrimaryKey not set.", 1, _table.PrimaryKey.Length);
                	Assertion.AssertEquals("Not unigue.", true, _table.PrimaryKey [0].Unique);

		}

		[Test]
		public void Unique ()                             
		{                                                     
			UniqueConstraint U = new UniqueConstraint (_table.Columns [0]);
			Assertion.AssertEquals ("test#01", false, _table.Columns [0].Unique); 
			
                        U = new UniqueConstraint (new DataColumn [] {_table.Columns [0],_table.Columns [1]});     
			
                        Assertion.AssertEquals ("test#02", false, _table.Columns [0].Unique);
                        Assertion.AssertEquals ("test#03", false, _table.Columns [1].Unique);
                        Assertion.AssertEquals ("test#04", false, _table.Columns [2].Unique);
			
                        _table.Constraints.Add (U);
                        Assertion.AssertEquals ("test#05", false, _table.Columns [0].Unique);
                        Assertion.AssertEquals ("test#06", false, _table.Columns [1].Unique);
                        Assertion.AssertEquals ("test#07", false, _table.Columns [2].Unique);
                }                                                     
		
		[Test]
		public void EqualsAndHashCode() {
			UniqueConstraint cst = new UniqueConstraint( new DataColumn [] {
					_table.Columns[0], _table.Columns[1]});
			UniqueConstraint cst2 = new UniqueConstraint( new DataColumn [] {
					 _table.Columns[1], _table.Columns[0]});

			UniqueConstraint cst3 = new UniqueConstraint(_table.Columns[0]);
			UniqueConstraint cst4 = new UniqueConstraint(_table.Columns[2]);
			
			//true
			Assertion.Assert(cst.Equals(cst2) == true);
			
			//false
			Assertion.Assert("A1", cst.Equals(23) == false);
			Assertion.Assert("A2", cst.Equals(cst3) == false);
			Assertion.Assert("A3", cst3.Equals(cst) == false);
			Assertion.Assert("A4", cst.Equals(cst4) == false);

			//true
			Assertion.Assert("HashEquals", cst.GetHashCode() == cst2.GetHashCode());

			//false
			Assertion.Assert("Hash Not Equals", (cst.GetHashCode() == cst3.GetHashCode()) == false);
		}
	}
}
