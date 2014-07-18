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

using System;
using System.Data;
using MonoTests.System.Data.Utils;
#if WINDOWS_STORE_APP
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
using AssertionException = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.UnitTestAssertException;
#else
using NUnit.Framework;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif
#endif

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
				AssertHelpers.AssertIsInstanceOfType (e, typeof(ArgumentException), "test#02");
			}        

			//Null exception
			try {
				//Should throw argument null exception
				cst = new UniqueConstraint ((DataColumn)null);
			} catch (Exception e) {
				AssertHelpers.AssertIsInstanceOfType (e, typeof(NullReferenceException), "test#05");
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
			
			Assert.AreSame (cst.Table, _table, "B1");

			//table is set on ctor
			cst = new UniqueConstraint (new DataColumn [] {
				      _table.Columns[0], _table.Columns[1]});
			Assert.AreSame (cst.Table, _table, "B2");

			cst = new UniqueConstraint ("MyName", _table.Columns [0], true);

			//Test ctor parm set for ConstraintName & IsPrimaryKey
			Assert.AreEqual ("MyName", cst.ConstraintName, "ConstraintName not set in ctor.");
			Assert.IsFalse (cst.IsPrimaryKey, "IsPrimaryKey already set.");
                
			_table.Constraints.Add (cst);

			Assert.IsTrue (cst.IsPrimaryKey, "IsPrimaryKey not set set.");
                	
			Assert.AreEqual (1, _table.PrimaryKey.Length, "PrimaryKey not set.");
			Assert.IsTrue (_table.PrimaryKey [0].Unique, "Not unigue.");
		}

		[Test]
		public void Unique ()
		{                                                     
			UniqueConstraint U = new UniqueConstraint (_table.Columns [0]);
			Assert.IsFalse (_table.Columns [0].Unique, "test#01"); 
			
			U = new UniqueConstraint (new DataColumn [] {_table.Columns [0],_table.Columns [1]});     
			
			Assert.IsFalse (_table.Columns [0].Unique, "test#02");
			Assert.IsFalse (_table.Columns [1].Unique, "test#03");
			Assert.IsFalse (_table.Columns [2].Unique, "test#04");
			
			_table.Constraints.Add (U);
			Assert.IsFalse (_table.Columns [0].Unique, "test#05");
			Assert.IsFalse (_table.Columns [1].Unique, "test#06");
			Assert.IsFalse (_table.Columns [2].Unique, "test#07");
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
			Assert.IsTrue (cst.Equals (cst2), "A0");
			
			//false
			Assert.IsFalse (cst.Equals (23), "A1");
			Assert.IsFalse (cst.Equals (cst3), "A2");
			Assert.IsFalse (cst3.Equals (cst), "A3");
			Assert.IsFalse (cst.Equals (cst4), "A4");

			//true
			Assert.AreEqual (cst2.GetHashCode (), cst.GetHashCode (), "HashEquals");

			//false
			Assert.AreNotEqual (cst.GetHashCode (), cst3.GetHashCode (), "Hash Not Equals");
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
