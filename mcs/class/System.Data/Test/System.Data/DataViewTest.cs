//
// DataViewTest.cs
//
// Author:
//	Patrick Kalkman  kalkman@cistron.nl
//
// (C) 2003 Patrick Kalkman
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
		public void TestFindRows ()
		{
			DataView TestView = new DataView (TestTable);
			TestView.Sort = "ID";
			DataRowView[] Result = TestView.FindRows ("pk03");
			Assertion.AssertEquals ("Dv #1", 1, Result.Length);
			Assertion.AssertEquals ("Dv #2", "pk03", Result [0]["ID"]);
		}
	}
}

