//
// DataViewTest.cs
//
// Author:
//	Patrick Kalkman	 kalkman@cistron.nl
//
// (C) 2003 Patrick Kalkman
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
using NUnit.Framework;

namespace MonoTests.System.Data {
	
	[TestFixture]
	public class DataViewTest {
		DataTable TestTable;

		[SetUp]
		public void GetReady () 
		{
			TestTable = new DataTable ("TestTable");
			DataColumn ID = new DataColumn ("ID", Type.GetType ("System.String"));
			TestTable.Columns.Add (ID);
			DataRow NewRow;
			//Add 25 dummy rows to a datatable.
			for (int RowCnt = 0; RowCnt < 25; RowCnt++) {
				NewRow = TestTable.NewRow ();
				NewRow ["ID"] = "pk" + RowCnt.ToString ("D2");
				TestTable.Rows.Add (NewRow);
			}
		}
		
		[TearDown]
		public void Clean () {}

		[Test]
		public void TestValue ()
		{	
			DataView TestView = new DataView (TestTable);
			Assertion.AssertEquals ("Dv #1", "pk00", TestView [0]["ID"]);
			Assertion.AssertEquals ("Dv #2", "pk24", TestView [24]["ID"]);
		}

		[Test]
		public void TestCount ()
		{
			DataView TestView = new DataView (TestTable);
			Assertion.AssertEquals ("Dv #3", 25, TestView.Count);
		}

		[Test]
		public void TestSort ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.Sort = "ID Desc";
			Assertion.AssertEquals ("Dv #1", "pk24", TestView [0]["ID"]);
			TestView.Sort = "ID"; //Default is Ascending.
			Assertion.AssertEquals ("Dv #2", "pk00", TestView [0]["ID"]);
			TestView.Sort = "ID Asc";
			Assertion.AssertEquals ("Dv #3", "pk00", TestView [0]["ID"]);
		}

		[Test]
		public void TestRowFilter ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.RowFilter = "id = 'pk17'"; //Filter gives 1.
			Assertion.AssertEquals ("Dv #1", 1, TestView.Count);
			TestView.RowFilter = "id LIKE 'pk%'"; //Filter gives all.
			Assertion.AssertEquals ("Dv #2", 25, TestView.Count);
		}

		[Test]
		public void TestAddNew1 ()
		{
			DataView TestView = new DataView (TestTable);
			DataRowView TestDr = TestView.AddNew ();
			TestDr ["ID"] = "pk25";
			TestDr.EndEdit ();
			Assertion.AssertEquals ("Dv #1", 26, TestView.Count);
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void TestAddNew2 ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.AllowNew = false;
			DataRowView TestDr = TestView.AddNew ();
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void TestAddNew3 ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.Dispose (); // Only way I know of closing the table
			DataRowView row = TestView.AddNew ();
		}
		
		[Test]
		public void TestFindRows ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.Sort = "ID";
			DataRowView[] Result = TestView.FindRows ("pk03");
			Assertion.AssertEquals ("Dv #1", 1, Result.Length);
			Assertion.AssertEquals ("Dv #2", "pk03", Result [0]["ID"]);
		}

		[Test]
		public void TestDelete ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.Delete (0);
			DataRow r = TestView.Table.Rows [0];
			Assertion.Assert ("Dv #1", !(r ["ID"] == "pk00"));
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]		       
		public void TestDeleteOutOfBounds ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.Delete (100);
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void TestDeleteNotAllowed ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.AllowDelete = false;
			TestView.Delete (0);
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void TestDeleteClosed ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.Dispose (); // Close the table
			TestView.Delete (0);
		}
	}
}

