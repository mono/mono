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
	public class UniqueConstraintTest
	{
		private DataTable _table;

		[SetUp]
		public void GetReady ()
		{

			//Setup DataTable
			_table = new DataTable ("TestTable");
			_table.Columns.Add ("Col1", typeof(int));
			_table.Columns.Add ("Col2", typeof(int));
			_table.Columns.Add ("Col3", typeof(int));

		}  

		[Test]
		public void CtorExceptions ()
		{
			//UniqueConstraint(string name, DataColumn column, bool isPrimaryKey)

			UniqueConstraint cst;
			
			//must have DataTable exception
			try {
				//Should throw an ArgumentException
				//Can only add DataColumns that are attached
				//to a DataTable
				cst = new UniqueConstraint (new DataColumn (""));

				Assert.Fail ("Failed to throw ArgumentException.");
			} catch (Exception e) {
				Assert.That (e, Is.TypeOf (typeof(ArgumentException)), "test#02");
			}        

			//Null exception
			try {
				//Should throw argument null exception
				cst = new UniqueConstraint ((DataColumn)null);
			} catch (Exception e) {
				Assert.That (e, Is.TypeOf (typeof(NullReferenceException)), "test#05");
			}
			
			try {
				//Should throw exception
				//must have at least one valid column
				//InvalidConstraintException is thrown by msft ver
				cst = new UniqueConstraint (new DataColumn [] {});

				Assert.Fail ("B1: Failed to throw InvalidConstraintException.");
			} catch (InvalidConstraintException) {
			} catch (AssertionException exc) {
				throw exc;
			} catch {
				Assert.Fail ("A3: Wrong Exception type.");
			}

			DataTable dt = new DataTable ("Table1");
			dt.Columns.Add ("Col1", typeof(int));
			DataTable dt2 = new DataTable ("Table2");
			dt2.Columns.Add ("Col1", typeof(int));

			DataSet ds = new DataSet ();
			ds.Tables.Add (dt);
			ds.Tables.Add (dt2);

			//columns from two different tables.
			try {
				//next line should throw
				//can't have columns from two different tables
				cst = new UniqueConstraint (new DataColumn [] { 
						 dt.Columns[0], dt2.Columns[0]});

				Assert.Fail ("B2: Failed to throw InvalidConstraintException");
			} catch (InvalidConstraintException) {
			} catch (AssertionException exc) {
				throw exc;
			} catch {
				Assert.Fail ("A4: Wrong Exception type.");
			}
		}

		[Test]
		public void Ctor ()
		{
			UniqueConstraint cst;
		
			//Success case
			try {
				cst = new UniqueConstraint (_table.Columns [0]);
			} catch (Exception exc) {
				Assert.Fail ("A1: Failed to ctor. " + exc.ToString ());
			}

			try {
				cst = new UniqueConstraint (new DataColumn [] {
						_table.Columns[0], _table.Columns[1]});
			} catch (Exception exc) {
				Assert.Fail ("A2: Failed to ctor. " + exc.ToString ());
			}

			//table is set on ctor
			cst = new UniqueConstraint (_table.Columns [0]);
			
			Assert.That (cst.Table, Is.SameAs (_table), "B1");

			//table is set on ctor
			cst = new UniqueConstraint (new DataColumn [] {
				      _table.Columns[0], _table.Columns[1]});
			Assert.That (cst.Table, Is.SameAs (_table), "B2");

			cst = new UniqueConstraint ("MyName", _table.Columns [0], true);

			//Test ctor parm set for ConstraintName & IsPrimaryKey
			Assert.That (cst.ConstraintName, Is.EqualTo ("MyName"), "ConstraintName not set in ctor.");
			Assert.That (cst.IsPrimaryKey, Is.False, "IsPrimaryKey already set.");
                
			_table.Constraints.Add (cst);

			Assert.That (cst.IsPrimaryKey, Is.True, "IsPrimaryKey not set set.");
                	
			Assert.That (_table.PrimaryKey.Length, Is.EqualTo (1), "PrimaryKey not set.");
			Assert.That (_table.PrimaryKey [0].Unique, Is.True, "Not unigue.");
		}

		[Test]
		public void Unique ()
		{                                                     
			UniqueConstraint U = new UniqueConstraint (_table.Columns [0]);
			Assert.That (_table.Columns [0].Unique, Is.False, "test#01"); 
			
			U = new UniqueConstraint (new DataColumn [] {_table.Columns [0],_table.Columns [1]});     
			
			Assert.That (_table.Columns [0].Unique, Is.False, "test#02");
			Assert.That (_table.Columns [1].Unique, Is.False, "test#03");
			Assert.That (_table.Columns [2].Unique, Is.False, "test#04");
			
			_table.Constraints.Add (U);
			Assert.That (_table.Columns [0].Unique, Is.False, "test#05");
			Assert.That (_table.Columns [1].Unique, Is.False, "test#06");
			Assert.That (_table.Columns [2].Unique, Is.False, "test#07");
		}                                                     

		[Test]
		public void EqualsAndHashCode ()
		{
			UniqueConstraint cst = new UniqueConstraint (new DataColumn [] {
					_table.Columns[0], _table.Columns[1]});
			UniqueConstraint cst2 = new UniqueConstraint (new DataColumn [] {
					 _table.Columns[1], _table.Columns[0]});

			UniqueConstraint cst3 = new UniqueConstraint (_table.Columns [0]);
			UniqueConstraint cst4 = new UniqueConstraint (_table.Columns [2]);
			
			//true
			Assert.That (cst.Equals (cst2), Is.True, "A0");
			
			//false
			Assert.That (cst.Equals (23), Is.False, "A1");
			Assert.That (cst.Equals (cst3), Is.False, "A2");
			Assert.That (cst3.Equals (cst), Is.False, "A3");
			Assert.That (cst.Equals (cst4), Is.False, "A4");

			//false... but it should be true (FXDG violation)
			//Assert.That (cst.GetHashCode (), Is.Not.EqualTo (cst2.GetHashCode ()), "HashEquals");

			//false
			Assert.That (cst.GetHashCode (), Is.Not.EqualTo (cst3.GetHashCode ()), "Hash Not Equals");
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
