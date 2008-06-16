// UniqueConstraintTest.cs - NUnit Test Cases for testing the class System.Data.UniqueConstraint
//
// Authors:
//   Franklin Wise <gracenote@earthlink.net>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Franklin Wise
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
	public class UniqueConstraintTest : Assertion
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

				Fail("Failed to throw ArgumentException.");
			} 
			catch (Exception e) {
				AssertEquals ("test#02", typeof (ArgumentException), e.GetType ());
				// Never premise English.
                        	// AssertEquals ("test#03", "Column must belong to a table.", e.Message);
                        }        

			//Null exception
			try {
				//Should throw argument null exception
				cst = new UniqueConstraint((DataColumn)null);
			}
                        catch (Exception e) {
                        	AssertEquals ("test#05", typeof (NullReferenceException), e.GetType ());
				// Never premise English.
                                //AssertEquals ("test#06", "Object reference not set to an instance of an object.", e.Message);
                        }
			
			try {
				//Should throw exception
				//must have at least one valid column
				//InvalidConstraintException is thrown by msft ver
				cst = new UniqueConstraint(new DataColumn [] {});

				Fail("B1: Failed to throw InvalidConstraintException.");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionException exc) {throw exc;}
			catch {
				Fail("A3: Wrong Exception type.");
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

				Fail("B2: Failed to throw InvalidConstraintException");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionException exc) {throw exc;}
			catch {
				Fail("A4: Wrong Exception type.");
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
				Fail("A1: Failed to ctor. " + exc.ToString());
			}

			
			try {
				cst = new UniqueConstraint( new DataColumn [] {
						_table.Columns[0], _table.Columns[1]});
			}
			catch (Exception exc) {
				Fail("A2: Failed to ctor. " + exc.ToString());
			}

			
			//table is set on ctor
			cst = new UniqueConstraint(_table.Columns[0]);
			
			AssertSame("B1", cst.Table, _table);

			//table is set on ctor
			cst = new UniqueConstraint( new DataColumn [] {
				      _table.Columns[0], _table.Columns[1]});
			AssertSame ("B2", cst.Table, _table);

			cst = new UniqueConstraint("MyName",_table.Columns[0],true);

			//Test ctor parm set for ConstraintName & IsPrimaryKey
			AssertEquals("ConstraintName not set in ctor.", 
				"MyName", cst.ConstraintName);
                        AssertEquals("IsPrimaryKey already set.",
                                false, cst.IsPrimaryKey);
                
			_table.Constraints.Add (cst);

                        AssertEquals("IsPrimaryKey not set set.",
                                true, cst.IsPrimaryKey);
                	
                	AssertEquals("PrimaryKey not set.", 1, _table.PrimaryKey.Length);
                	AssertEquals("Not unigue.", true, _table.PrimaryKey [0].Unique);

		}

		[Test]
		public void Unique ()                             
		{                                                     
			UniqueConstraint U = new UniqueConstraint (_table.Columns [0]);
			AssertEquals ("test#01", false, _table.Columns [0].Unique); 
			
                        U = new UniqueConstraint (new DataColumn [] {_table.Columns [0],_table.Columns [1]});     
			
                        AssertEquals ("test#02", false, _table.Columns [0].Unique);
                        AssertEquals ("test#03", false, _table.Columns [1].Unique);
                        AssertEquals ("test#04", false, _table.Columns [2].Unique);
			
                        _table.Constraints.Add (U);
                        AssertEquals ("test#05", false, _table.Columns [0].Unique);
                        AssertEquals ("test#06", false, _table.Columns [1].Unique);
                        AssertEquals ("test#07", false, _table.Columns [2].Unique);
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
			Assert(cst.Equals(cst2) == true);
			
			//false
			Assert("A1", cst.Equals(23) == false);
			Assert("A2", cst.Equals(cst3) == false);
			Assert("A3", cst3.Equals(cst) == false);
			Assert("A4", cst.Equals(cst4) == false);

			//true
			Assert("HashEquals", cst.GetHashCode() == cst2.GetHashCode());

			//false
			Assert("Hash Not Equals", (cst.GetHashCode() == cst3.GetHashCode()) == false);
		}

		[Test]
		public void DBNullAllowed ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Constraints.Add (new UniqueConstraint (dt.Columns [0]));
			dt.Rows.Add (new object [] {1, 3});
			dt.Rows.Add (new object [] {DBNull.Value, 3});
		}
	}
}
