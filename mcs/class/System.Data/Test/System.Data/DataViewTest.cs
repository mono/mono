// DataViewTest.cs - Nunit Test Cases for for testing the DataView
// class
// Authors:
//	Punit Kumar Todi ( punit_todi@da-iict.org )
// 	Patrick Kalkman  kalkman@cistron.nl
//      Umadevi S (sumadevi@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//      Sureshkumar T <tsureshkumar@novell.com>
//
// (C) 2003 Patrick Kalkman
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.IO;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataViewTest : Assertion
	{
		DataTable dataTable;
		DataView  dataView;
		Random rndm;
		int seed, rowCount;
		ListChangedEventArgs listChangedArgs;
		TextWriter eventWriter;

		DataColumn dc1;
		DataColumn dc2;
		DataColumn dc3;
		DataColumn dc4;

		[SetUp]
		public void GetReady ()
		{
			dataTable = new DataTable ("itemTable");
			dc1 = new DataColumn ("itemId");
			dc2 = new DataColumn ("itemName");
			dc3 = new DataColumn ("itemPrice");
			dc4 = new DataColumn ("itemCategory");
			
			dataTable.Columns.Add (dc1);
			dataTable.Columns.Add (dc2);
			dataTable.Columns.Add (dc3);
			dataTable.Columns.Add (dc4);
			DataRow dr;
			seed = 123;
			rowCount = 5;
			rndm = new Random (seed);
			for (int i = 1; i <= rowCount; i++) {
				dr = dataTable.NewRow ();
				dr["itemId"] = "item " + i;
				dr["itemName"] = "name " + rndm.Next ();
				dr["itemPrice"] = "Rs. " + (rndm.Next () % 1000);
				dr["itemCategory"] = "Cat " + ((rndm.Next () % 10) + 1);
				dataTable.Rows.Add (dr);
			}
			dataTable.AcceptChanges ();
			dataView = new DataView (dataTable);
			dataView.ListChanged += new ListChangedEventHandler (OnListChanged);
			listChangedArgs = null;
		}
		
		protected void OnListChanged (object sender, ListChangedEventArgs args)
		{
			listChangedArgs = args;
			// for debugging
			/*Console.WriteLine("EventType :: " + listChangedArgs.ListChangedType + 
							  "  oldIndex  :: " + listChangedArgs.OldIndex + 
							  "  NewIndex  :: " + listChangedArgs.NewIndex);*/
			
		}

		private void PrintTableOrView (DataTable t, string label)
		{
			Console.WriteLine ("\n" + label);
			for (int i = 0; i<t.Rows.Count; i++){
				foreach (DataColumn dc in t.Columns)
					Console.Write (t.Rows [i][dc] + "\t");
				Console.WriteLine ("");
			}
			Console.WriteLine ();
		}

		private void PrintTableOrView (DataView dv, string label)
		{
			Console.WriteLine ("\n" + label);
			Console.WriteLine ("Sort Key :: " + dv.Sort);
			for (int i = 0; i < dv.Count; i++) {
				foreach (DataColumn dc in dv.Table.Columns)
					Console.Write (dv [i].Row [dc] + "\t");
				Console.WriteLine ("");
			}
			Console.WriteLine ();
		}

		[TearDown]
		public void Clean () 
		{
			dataTable = null;
			dataView = null;
		}

		[Test]
		public void TestSort ()
		{
			DataView dv = new DataView ();
			dv.Sort = "abc";
			dv.Sort = string.Empty;
			dv.Sort = "abc";
			AssertEquals ("test#01", "abc", dv.Sort);
		}

		[Test]
		public void DataView ()
		{
			DataView dv1,dv2,dv3;
			dv1 = new DataView ();
			// AssertEquals ("test#01",null,dv1.Table);
			AssertEquals ("test#02",true,dv1.AllowNew);
			AssertEquals ("test#03",true,dv1.AllowEdit);
			AssertEquals ("test#04",true,dv1.AllowDelete);
			AssertEquals ("test#05",false,dv1.ApplyDefaultSort);
			AssertEquals ("test#06",string.Empty,dv1.RowFilter);
			AssertEquals ("test#07",DataViewRowState.CurrentRows,dv1.RowStateFilter);
			AssertEquals ("test#08",string.Empty,dv1.Sort);
			
			dv2 = new DataView (dataTable);
			AssertEquals ("test#09","itemTable",dv2.Table.TableName);
			AssertEquals ("test#10",string.Empty,dv2.Sort);
			AssertEquals ("test#11",false,dv2.ApplyDefaultSort);
			AssertEquals ("test#12",dataTable.Rows[0],dv2[0].Row);
			
			dv3 = new DataView (dataTable,"","itemId DESC",DataViewRowState.CurrentRows);
			AssertEquals ("test#13","",dv3.RowFilter);
			AssertEquals ("test#14","itemId DESC",dv3.Sort);
			AssertEquals ("test#15",DataViewRowState.CurrentRows,dv3.RowStateFilter);
			//AssertEquals ("test#16",dataTable.Rows.[(dataTable.Rows.Count-1)],dv3[0]);
		}

		 [Test]
		 public void TestValue ()
		 {
			DataView TestView = new DataView (dataTable);
			Assertion.AssertEquals ("Dv #1", "item 1", TestView [0]["itemId"]);
		 }

		 [Test]
		 public void TestCount ()
		 {
			DataView TestView = new DataView (dataTable);
			Assertion.AssertEquals ("Dv #3", 5, TestView.Count);
		 }

		[Test]
		public void AllowNew ()
		{
			AssertEquals ("test#01",true,dataView.AllowNew);
		}

		[Test]
		public void ApplyDefaultSort ()
		{
			UniqueConstraint uc = new UniqueConstraint (dataTable.Columns["itemId"]);
			dataTable.Constraints.Add (uc);
			dataView.ApplyDefaultSort = true;
			// dataView.Sort = "itemName";
			// AssertEquals ("test#01","item 1",dataView[0]["itemId"]);
			AssertEquals ("test#02",ListChangedType.Reset,listChangedArgs.ListChangedType);
			// UnComment the line below to see if dataView is sorted
			//   PrintTableOrView (dataView,"* OnApplyDefaultSort");
		}

		[Test]
		public void RowStateFilter ()
		{
			dataView.RowStateFilter = DataViewRowState.Deleted;
			AssertEquals ("test#01",ListChangedType.Reset,listChangedArgs.ListChangedType);
		}

		[Test]
		public void RowStateFilter_2 ()
		{
			DataSet dataset = new DataSet ("new");
			DataTable dt = new DataTable ("table1");
			dataset.Tables.Add (dt);
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Rows.Add (new object [] {1,1});
			dt.Rows.Add (new object [] {1,2});
			dt.Rows.Add (new object [] {1,3});
			dataset.AcceptChanges ();

			DataView dataView = new DataView (dataset.Tables [0]);

			// 'new'  table in this sample contains 6 records
			dataView.AllowEdit = true;
			dataView.AllowDelete = true;
			string v;

			// Editing the row
			dataView [0] ["col1"] = -1;
			dataView.RowStateFilter = DataViewRowState.ModifiedOriginal;
			v = dataView [0] [0].ToString ();
			AssertEquals ("ModifiedOriginal.Count", 1, dataView.Count);
			AssertEquals ("ModifiedOriginal.Value", "1", v);

			// Deleting the row
			dataView.Delete (0);
			dataView.RowStateFilter = DataViewRowState.Deleted;

			v = dataView [0] [0].ToString ();
			AssertEquals ("Deleted.Count", 1, dataView.Count);
			AssertEquals ("Deleted.Value", "1", v);
		}

		[Test]
		public void NullTableGetItemPropertiesTest ()
		{
			DataView dataview = new DataView ();
			PropertyDescriptorCollection col = ((ITypedList)dataview).GetItemProperties (null);
			AssertEquals ("1", 0, col.Count);
		}

		#region Sort Tests
		[Test]
		public void SortListChangedTest ()
		{
			dataView.Sort = "itemName DESC";
			AssertEquals ("test#01",ListChangedType.Reset,listChangedArgs.ListChangedType);
			// UnComment the line below to see if dataView is sorted
			// PrintTableOrView (dataView);
		}


		[Test]
		public void SortTestWeirdColumnName ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("id]", typeof (int));
			dt.Columns.Add ("[id", typeof (int));

			DataView dv = dt.DefaultView;
			dv.Sort = "id]";
			//dv.Sort = "[id"; // this is not allowed
			dv.Sort = "[id]]";
			dv.Sort = "[[id]";
			dv.Sort = "id] ASC";
			dv.Sort = "[id]] DESC";
			dv.Sort = "[[id] ASC";
		}


		[Test]
		public void SortTests ()
		{
			DataTable dataTable = new DataTable ("itemTable");
			DataColumn dc1 = new DataColumn ("itemId", typeof(int));
			DataColumn dc2 = new DataColumn ("itemName", typeof(string));
			
			dataTable.Columns.Add (dc1);
			dataTable.Columns.Add (dc2);

			dataTable.Rows.Add (new object[2] { 1, "First entry" });
			dataTable.Rows.Add (new object[2] { 0, "Second entry" });
			dataTable.Rows.Add (new object[2] { 3, "Third entry" });
			dataTable.Rows.Add (new object[2] { 2, "Fourth entry" });
			
			DataView dataView = dataTable.DefaultView;

			string s = "Default sorting: ";
			AssertEquals (s + "First entry has wrong item", 1, dataView[0][0]);
			AssertEquals (s + "Second entry has wrong item", 0, dataView[1][0]);
			AssertEquals (s + "Third entry has wrong item", 3, dataView[2][0]);
			AssertEquals (s + "Fourth entry has wrong item", 2, dataView[3][0]);

			s = "Ascending sorting 1: ";
			dataView.Sort = "itemId ASC";
			AssertEquals (s + "First entry has wrong item", 0, dataView[0][0]);
			AssertEquals (s + "Second entry has wrong item", 1, dataView[1][0]);
			AssertEquals (s + "Third entry has wrong item", 2, dataView[2][0]);
			AssertEquals (s + "Fourth entry has wrong item", 3, dataView[3][0]);

			// bug #77104 (2-5)
			s = "Ascending sorting 2: ";
			dataView.Sort = "itemId     ASC";
			AssertEquals (s + "First entry has wrong item", 0, dataView[0][0]);
			AssertEquals (s + "Second entry has wrong item", 1, dataView[1][0]);
			AssertEquals (s + "Third entry has wrong item", 2, dataView[2][0]);
			AssertEquals (s + "Fourth entry has wrong item", 3, dataView[3][0]);

			s = "Ascending sorting 3: ";
			dataView.Sort = "[itemId] ASC";
			AssertEquals (s + "First entry has wrong item", 0, dataView[0][0]);
			AssertEquals (s + "Second entry has wrong item", 1, dataView[1][0]);
			AssertEquals (s + "Third entry has wrong item", 2, dataView[2][0]);
			AssertEquals (s + "Fourth entry has wrong item", 3, dataView[3][0]);

			s = "Ascending sorting 4: ";
			dataView.Sort = "[itemId]       ASC";
			AssertEquals (s + "First entry has wrong item", 0, dataView[0][0]);
			AssertEquals (s + "Second entry has wrong item", 1, dataView[1][0]);
			AssertEquals (s + "Third entry has wrong item", 2, dataView[2][0]);
			AssertEquals (s + "Fourth entry has wrong item", 3, dataView[3][0]);

			s = "Ascending sorting 5: ";
			try {
				dataView.Sort = "itemId \tASC";
				AssertEquals (s + "Tab cannot be a separator" , true, false);
			}catch (IndexOutOfRangeException e) {
			}

			s = "Descending sorting : ";
			dataView.Sort = "itemId DESC";
			AssertEquals (s + "First entry has wrong item", 3, dataView[0][0]);
			AssertEquals (s + "Second entry has wrong item", 2, dataView[1][0]);
			AssertEquals (s + "Third entry has wrong item", 1, dataView[2][0]);
			AssertEquals (s + "Fourth entry has wrong item", 0, dataView[3][0]);

			s = "Reverted to default sorting: ";
			dataView.Sort = null;
			AssertEquals (s + "First entry has wrong item", 1, dataView[0][0]);
			AssertEquals (s + "Second entry has wrong item", 0, dataView[1][0]);
			AssertEquals (s + "Third entry has wrong item", 3, dataView[2][0]);
			AssertEquals (s + "Fourth entry has wrong item", 2, dataView[3][0]);
		}
		
		#endregion // Sort Tests

		[Test]
		[ExpectedException(typeof(DataException))]
		public void AddNew_1 ()
		{
			dataView.AllowNew = false;
			DataRowView drv = dataView.AddNew ();
		}

		[Test]
		public void AddNew_2 ()
		{
			dataView.AllowNew = true;
			DataRowView drv = dataView.AddNew ();
			AssertEquals ("test#01",ListChangedType.ItemAdded,listChangedArgs.ListChangedType);
			AssertEquals ("test#02",-1,listChangedArgs.OldIndex);
			AssertEquals ("test#03",5,listChangedArgs.NewIndex);
			AssertEquals ("test#04",drv["itemName"],dataView [dataView.Count - 1]["itemName"]);
			listChangedArgs = null;
			drv["itemId"] = "item " + 1001;
			drv["itemName"] = "name " + rndm.Next();
			drv["itemPrice"] = "Rs. " + (rndm.Next() % 1000);
			drv["itemCategory"] = "Cat " + ((rndm.Next() % 10) + 1);
			// Actually no events are arisen when items are set.
			AssertNull ("test#05", listChangedArgs);
			drv.CancelEdit ();
			AssertEquals ("test#06",ListChangedType.ItemDeleted,listChangedArgs.ListChangedType);
			AssertEquals ("test#07",-1,listChangedArgs.OldIndex);
			AssertEquals ("test#08",5,listChangedArgs.NewIndex);
		}

		[Test]
		public void BeginInit ()
		{
			DataTable table = new DataTable ("table");
			DataView dv = new DataView ();
			DataColumn col1 = new DataColumn ("col1");
			DataColumn col2 = new DataColumn ("col2");
			
			dv.BeginInit ();
			table.BeginInit ();
			table.Columns.AddRange (new DataColumn[] {col1,col2});

			dv.Table = table;
			AssertNull ("#1", dv.Table);
			dv.EndInit ();

			AssertEquals ("#2", table, dv.Table);
			AssertEquals ("#3", 0, table.Columns.Count);

			table.EndInit ();
			AssertEquals ("#4", 2, table.Columns.Count);
		}

#if NET_2_0
		private bool dvInitialized;
                private void OnDataViewInitialized (object src, EventArgs args)
                {
			dvInitialized = true;
		}
		[Test]
		public void BeginInit2 ()
		{
			DataTable table = new DataTable ("table");
			DataView dv = new DataView ();
			DataColumn col1 = new DataColumn ("col1");
			DataColumn col2 = new DataColumn ("col2");

			dvInitialized = false;

			dv.Initialized += new EventHandler (OnDataViewInitialized);
			
			dv.BeginInit ();
			table.BeginInit ();
			table.Columns.AddRange (new DataColumn[] {col1,col2});

			dv.Table = table;
			AssertNull ("#1", dv.Table);
			dv.EndInit ();

			dv.Initialized -= new EventHandler (OnDataViewInitialized);
			
			AssertEquals ("#2", table, dv.Table);
			AssertEquals ("#3", 0, table.Columns.Count);

			table.EndInit ();
			AssertEquals ("#4", 2, table.Columns.Count);
			AssertEquals("DataViewInitialized #5", dvInitialized, true);
		}
#endif

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Find_1 ()
		{
			/* since the sort key is not specified. Must raise a ArgumentException */
			int sIndex = dataView.Find ("abc");
		}

		[Test]
		public void Find_2 ()
		{
			int randInt;
			DataRowView drv;
			randInt = rndm.Next () % rowCount;
			dataView.Sort = "itemId";
			drv = dataView [randInt];
			AssertEquals ("test#01",randInt,dataView.Find (drv ["itemId"]));
			
			dataView.Sort = "itemId DESC";
			drv = dataView [randInt];
			AssertEquals ("test#02",randInt,dataView.Find (drv ["itemId"]));
			
			dataView.Sort = "itemId, itemName";
			drv = dataView [randInt];
			object [] keys = new object [2];
			keys [0] = drv ["itemId"];
			keys [1] = drv ["itemName"];
			AssertEquals ("test#03",randInt,dataView.Find (keys));
			
			dataView.Sort = "itemId";
			AssertEquals ("test#04",-1,dataView.Find("no item"));

		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")] // defect 5446
#endif
		public void Find_3 ()
		{
			dataView.Sort = "itemID, itemName";
			/* expecting order key count mismatch */
			dataView.Find ("itemValue");
		}

		[Test]
		[Ignore("Test code not implemented")]
		public void GetEnumerator ()
		{
			//TODO
		}

		[Test]
		public void ToStringTest ()
		{
			AssertEquals ("test#01","System.Data.DataView",dataView.ToString());
		}

		[Test]
		public void TestingEventHandling ()
		{
			dataView.Sort = "itemId";
			DataRow dr;
			dr = dataTable.NewRow ();
			dr ["itemId"] = "item 0";
			dr ["itemName"] = "name " + rndm.Next ();
			dr ["itemPrice"] = "Rs. " + (rndm.Next () % 1000);
			dr ["itemCategory"] = "Cat " + ((rndm.Next () % 10) + 1);
			dataTable.Rows.Add(dr);

			//PrintTableOrView(dataView, "ItemAdded");
			AssertEquals ("test#01",ListChangedType.ItemAdded,listChangedArgs.ListChangedType);
			listChangedArgs = null;

			dr ["itemId"] = "aitem 0";
			// PrintTableOrView(dataView, "ItemChanged");
			AssertEquals ("test#02",ListChangedType.ItemChanged,listChangedArgs.ListChangedType);
			listChangedArgs = null;

			dr ["itemId"] = "zitem 0";
			// PrintTableOrView(dataView, "ItemMoved");
			AssertEquals ("test#03",ListChangedType.ItemMoved,listChangedArgs.ListChangedType);
			listChangedArgs = null;

			dataTable.Rows.Remove (dr);
			// PrintTableOrView(dataView, "ItemDeleted");
			AssertEquals ("test#04",ListChangedType.ItemDeleted,listChangedArgs.ListChangedType);
			
			listChangedArgs = null;
			DataColumn dc5 = new DataColumn ("itemDesc");
			dataTable.Columns.Add (dc5);
			// PrintTableOrView(dataView, "PropertyDescriptorAdded");
			AssertEquals ("test#05",ListChangedType.PropertyDescriptorAdded,listChangedArgs.ListChangedType);
			
			listChangedArgs = null;
			dc5.ColumnName = "itemDescription";
			// PrintTableOrView(dataView, "PropertyDescriptorChanged");
			// AssertEquals ("test#06",ListChangedType.PropertyDescriptorChanged,listChangedArgs.ListChangedType);
			
			listChangedArgs = null;
			dataTable.Columns.Remove (dc5);
			// PrintTableOrView(dataView, "PropertyDescriptorDeleted");
			AssertEquals ("test#07",ListChangedType.PropertyDescriptorDeleted,listChangedArgs.ListChangedType);
		}
	
		[Test]
		public void TestFindRows ()
		{
			DataView TestView = new DataView (dataTable);
			TestView.Sort = "itemId";
			DataRowView[] Result = TestView.FindRows ("item 3");
			Assertion.AssertEquals ("Dv #1", 1, Result.Length);
			Assertion.AssertEquals ("Dv #2", "item 3", Result [0]["itemId"]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FindRowsWithoutSort ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Columns.Add ("col3");
			dt.Rows.Add (new object [] {1,2,3});
			dt.Rows.Add (new object [] {4,5,6});
			dt.Rows.Add (new object [] {4,7,8});
			dt.Rows.Add (new object [] {5,7,8});
			dt.Rows.Add (new object [] {4,8,9});
			DataView dv = new DataView (dt);
			dv.Find (1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FindRowsInconsistentKeyLength ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Columns.Add ("col3");
			dt.Rows.Add (new object [] {1,2,3});
			dt.Rows.Add (new object [] {4,5,6});
			dt.Rows.Add (new object [] {4,7,8});
			dt.Rows.Add (new object [] {5,7,8});
			dt.Rows.Add (new object [] {4,8,9});
			DataView dv = new DataView (dt, null, "col1",
				DataViewRowState.CurrentRows);
			dv.FindRows (new object [] {1, 2, 3});
		}

		[Test]
		[ExpectedException (typeof (DeletedRowInaccessibleException))]
		public void TestDelete ()
		{
			DataView TestView = new DataView (dataTable);
			TestView.Delete (0);
			DataRow r = TestView.Table.Rows [0];
			Assertion.Assert ("Dv #1", !((string)r ["itemId"] == "item 1"));
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void TestDeleteOutOfBounds ()
		{
			DataView TestView = new DataView (dataTable);
			TestView.Delete (100);
		}
									    
		[Test]
		[ExpectedException (typeof (DataException))]
		public void TestDeleteNotAllowed ()
		 {
			DataView TestView = new DataView (dataTable);
			TestView.AllowDelete = false;
			TestView.Delete (0);
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void TestDeleteClosed ()
		{
			DataView TestView = new DataView (dataTable);
			TestView.Dispose (); // Close the table
			TestView.Delete (0);
		}

		[Test] // based on bug #74631
		public void TestDeleteAndCount ()
		{
			DataSet dataset = new DataSet ("new");
			DataTable dt = new DataTable ("table1");
			dataset.Tables.Add (dt);
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Rows.Add (new object []{1,1});
			dt.Rows.Add (new object []{1,2});
			dt.Rows.Add (new object []{1,3});

			DataView dataView = new DataView (dataset.Tables[0]);

			AssertEquals ("before delete", 3, dataView.Count);
			dataView.AllowDelete = true;

			// Deleting the first row
			dataView.Delete (0);

			AssertEquals ("before delete", 2, dataView.Count);
		}

		[Test]
		public void ListChangeOnSetItem ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Columns.Add ("col3");
			dt.Rows.Add (new object [] {1, 2, 3});
			dt.AcceptChanges ();
			DataView dv = new DataView (dt);
			dv.ListChanged += new ListChangedEventHandler (OnChange);
			dv [0] ["col1"] = 4;
		}

		ListChangedEventArgs ListChangeArgOnSetItem;

		void OnChange (object o, ListChangedEventArgs e)
		{
			if (ListChangeArgOnSetItem != null)
				throw new Exception ("The event is already fired.");
			ListChangeArgOnSetItem = e;
		}

		[Test]
		public void CancelEditAndEvents ()
		{
			string reference = " =====ItemAdded:3 ------4 =====ItemAdded:4 ------5 =====ItemAdded:5 ------6 =====ItemDeleted:5 ------5 =====ItemAdded:5";

			eventWriter = new StringWriter ();

			DataTable dt = new DataTable ();
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Columns.Add ("col3");
			dt.Rows.Add (new object [] {1,2,3});
			dt.Rows.Add (new object [] {1,2,3});
			dt.Rows.Add (new object [] {1,2,3});

			DataView dv = new DataView (dt);
			dv.ListChanged += new ListChangedEventHandler (ListChanged);
			DataRowView a1 = dv.AddNew ();
			eventWriter.Write (" ------" + dv.Count);
			// I wonder why but MS fires another event here.
			a1 = dv.AddNew ();
			eventWriter.Write (" ------" + dv.Count);
			// I wonder why but MS fires another event here.
			DataRowView a2 = dv.AddNew ();
			eventWriter.Write (" ------" + dv.Count);
			a2.CancelEdit ();
			eventWriter.Write (" ------" + dv.Count);
			DataRowView a3 = dv.AddNew ();

			AssertEquals (reference, eventWriter.ToString ());
		}

		[Test]
		public void ColumnChangeName ()
		{
			string result = @"setting table...
---- OnListChanged PropertyDescriptorChanged,0,0
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
table was set.
---- OnListChanged PropertyDescriptorChanged,0,0
";

			eventWriter = new StringWriter ();

			ComplexEventSequence1View dv =
				new ComplexEventSequence1View (dataTable, eventWriter);

			dc2.ColumnName = "new_column_name";

			AssertEquals (result.Replace ("\r\n", "\n"), eventWriter.ToString ().Replace ("\r\n", "\n"));
		}

		private void ListChanged (object o, ListChangedEventArgs e)
		{
			eventWriter.Write (" =====" + e.ListChangedType + ":" + e.NewIndex);
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void ComplexEventSequence2 ()
		{
			string result = @"setting table...
---- OnListChanged PropertyDescriptorChanged,0,0
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
table was set.
---- OnListChanged PropertyDescriptorAdded,0,0
 col1 added.
---- OnListChanged PropertyDescriptorAdded,0,0
 col2 added.
---- OnListChanged PropertyDescriptorAdded,0,0
 col3 added.
---- OnListChanged Reset,-1,-1
added tables to dataset
---- OnListChanged PropertyDescriptorAdded,0,0
added relation 1
---- OnListChanged PropertyDescriptorAdded,0,0
added relation 2
---- OnListChanged PropertyDescriptorDeleted,0,0
removed relation 2
";

			eventWriter = new StringWriter ();

			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ("table");
			DataTable dt2 = new DataTable ("table2");
			ComplexEventSequence1View dv =
				new ComplexEventSequence1View (dt, eventWriter);
			dt.Columns.Add ("col1");
			eventWriter.WriteLine (" col1 added.");
			dt.Columns.Add ("col2");
			eventWriter.WriteLine (" col2 added.");
			dt.Columns.Add ("col3");
			eventWriter.WriteLine (" col3 added.");

			dt2.Columns.Add ("col1");
			dt2.Columns.Add ("col2");
			dt2.Columns.Add ("col3");

			ds.Tables.Add (dt);
			ds.Tables.Add (dt2);

			eventWriter.WriteLine ("added tables to dataset");
			ds.Relations.Add ("Relation", dt.Columns ["col1"], dt2.Columns ["col1"]);
			eventWriter.WriteLine ("added relation 1");

			DataRelation dr = ds.Relations.Add ("Relation2", dt2.Columns ["col2"], dt.Columns ["col2"]);
			eventWriter.WriteLine ("added relation 2");

			ds.Relations.Remove (dr);
			eventWriter.WriteLine ("removed relation 2");

			AssertEquals (result.Replace ("\r\n", "\n"), eventWriter.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void ComplexEventSequence1 ()
		{
			string result = @"setting table...
---- OnListChanged PropertyDescriptorChanged,0,0
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
table was set.
---- OnListChanged PropertyDescriptorAdded,0,0
 col1 added.
---- OnListChanged PropertyDescriptorAdded,0,0
 col2 added.
---- OnListChanged PropertyDescriptorAdded,0,0
 col3 added.
 uniq added.
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
 sort changed.
---- OnListChanged PropertyDescriptorDeleted,0,0
 col3 removed.
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
 rowfilter changed.
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
 rowstatefilter changed.
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
 rowstatefilter changed.
---- OnListChanged ItemAdded,0,-1
 added row to Rows.
---- OnListChanged ItemAdded,0,-1
 added row to Rows.
---- OnListChanged ItemAdded,0,-1
 added row to Rows.
---- OnListChanged ItemAdded,3,-1
 AddNew() invoked.
4
---- OnListChanged ItemDeleted,3,-1
---- OnListChanged ItemMoved,-2147483648,3
 EndEdit() invoked.
3
---- OnListChanged ItemMoved,0,-2147483648
 value changed to appear.
4
---- OnListChanged ItemMoved,3,0
 value moved.
4
---- OnListChanged ItemMoved,1,3
 value moved again.
4
---- OnListChanged PropertyDescriptorChanged,0,0
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
table changed.
";

			eventWriter = new StringWriter ();

			DataTable dt = new DataTable ("table");
			ComplexEventSequence1View dv =
				new ComplexEventSequence1View (dt, eventWriter);
			dt.Columns.Add ("col1");
			eventWriter.WriteLine (" col1 added.");
			dt.Columns.Add ("col2");
			eventWriter.WriteLine (" col2 added.");
			dt.Columns.Add ("col3");
			eventWriter.WriteLine (" col3 added.");
			dt.Constraints.Add (new UniqueConstraint (dt.Columns [0]));
			eventWriter.WriteLine (" uniq added.");
			dv.Sort = "col2";
			eventWriter.WriteLine (" sort changed.");
			dt.Columns.Remove ("col3");
			eventWriter.WriteLine (" col3 removed.");
			dv.RowFilter = "col1 <> 0";
			eventWriter.WriteLine (" rowfilter changed.");
			dv.RowStateFilter = DataViewRowState.Deleted;
			eventWriter.WriteLine (" rowstatefilter changed.");
			// FIXME: should be also tested.
//			dv.ApplyDefaultSort = true;
//			eventWriter.WriteLine (" apply default sort changed.");
			dv.RowStateFilter = DataViewRowState.CurrentRows;
			eventWriter.WriteLine (" rowstatefilter changed.");
			dt.Rows.Add (new object [] {1, 3});
			eventWriter.WriteLine (" added row to Rows.");
			dt.Rows.Add (new object [] {2, 2});
			eventWriter.WriteLine (" added row to Rows.");
			dt.Rows.Add (new object [] {3, 1});
			eventWriter.WriteLine (" added row to Rows.");
			DataRowView drv = dv.AddNew ();
			eventWriter.WriteLine (" AddNew() invoked.");
			eventWriter.WriteLine (dv.Count);
			drv [0] = 0;
			drv.EndEdit ();
			eventWriter.WriteLine (" EndEdit() invoked.");
			eventWriter.WriteLine (dv.Count);
			dt.Rows [dt.Rows.Count - 1] [0] = 4;
			eventWriter.WriteLine (" value changed to appear.");
			eventWriter.WriteLine (dv.Count);
			dt.Rows [dt.Rows.Count - 1] [1] = 4;
			eventWriter.WriteLine (" value moved.");
			eventWriter.WriteLine (dv.Count);
			dt.Rows [dt.Rows.Count - 1] [1] = 1.5;
			eventWriter.WriteLine (" value moved again.");
			eventWriter.WriteLine (dv.Count);
			dv.Table = new DataTable ("table2");
			eventWriter.WriteLine ("table changed.");

			AssertEquals (result.Replace ("\r\n", "\n"), eventWriter.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void DefaultColumnNameAddListChangedTest ()
		{
#if NET_2_0
			string result = @"setting table...
---- OnListChanged PropertyDescriptorChanged,0,0
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
table was set.
---- OnListChanged PropertyDescriptorAdded,0,0
 default named column added.
---- OnListChanged PropertyDescriptorAdded,0,0
 non-default named column added.
---- OnListChanged PropertyDescriptorAdded,0,0
 another default named column added (Column2).
---- OnListChanged PropertyDescriptorAdded,0,0
 add a column with the same name as the default columnnames.
---- OnListChanged PropertyDescriptorAdded,0,0
 add a column with a null name.
---- OnListChanged PropertyDescriptorAdded,0,0
 add a column with an empty name.
";
#else
			string result = @"setting table...
---- OnListChanged PropertyDescriptorChanged,0,0
----- UpdateIndex : True
---- OnListChanged Reset,-1,-1
table was set.
---- OnListChanged PropertyDescriptorChanged,0,0
---- OnListChanged PropertyDescriptorAdded,0,0
 default named column added.
---- OnListChanged PropertyDescriptorAdded,0,0
 non-default named column added.
---- OnListChanged PropertyDescriptorChanged,0,0
---- OnListChanged PropertyDescriptorAdded,0,0
 another default named column added (Column2).
---- OnListChanged PropertyDescriptorAdded,0,0
 add a column with the same name as the default columnnames.
---- OnListChanged PropertyDescriptorChanged,0,0
---- OnListChanged PropertyDescriptorAdded,0,0
 add a column with a null name.
---- OnListChanged PropertyDescriptorChanged,0,0
---- OnListChanged PropertyDescriptorAdded,0,0
 add a column with an empty name.
";
#endif
			eventWriter = new StringWriter ();
			DataTable dt = new DataTable ("table");
			ComplexEventSequence1View dv =
				new ComplexEventSequence1View (dt, eventWriter);
			dt.Columns.Add ();
			eventWriter.WriteLine (" default named column added.");
			dt.Columns.Add ("non-defaultNamedColumn");
			eventWriter.WriteLine (" non-default named column added.");
			DataColumn c = dt.Columns.Add ();
			eventWriter.WriteLine (" another default named column added ({0}).", c.ColumnName);
			dt.Columns.Add ("Column3");
			eventWriter.WriteLine (" add a column with the same name as the default columnnames.");
			dt.Columns.Add ((string)null);
			eventWriter.WriteLine (" add a column with a null name.");
			dt.Columns.Add ("");
			eventWriter.WriteLine (" add a column with an empty name.");

			AssertEquals (result.Replace ("\r\n", "\n"), eventWriter.ToString ().Replace ("\r\n", "\n"));
		}

		public class ComplexEventSequence1View : DataView
		{
			TextWriter w;

			public ComplexEventSequence1View (DataTable dt, 
				TextWriter w) : base ()
			{
				this.w = w;
				w.WriteLine ("setting table...");
				Table = dt;
				w.WriteLine ("table was set.");
			}

			protected override void OnListChanged (ListChangedEventArgs e)
			{
				if (w != null)
					w.WriteLine ("---- OnListChanged " + e.ListChangedType + "," + e.NewIndex + "," + e.OldIndex);
				base.OnListChanged (e);
			}

			protected override void UpdateIndex (bool force)
			{
				if (w != null)
					w.WriteLine ("----- UpdateIndex : " + force);
				base.UpdateIndex (force);
			}
		}
	}
}
