// DataView.cs - Nunit Test Cases for for testing the DataView
// class
// Authors:
//	Punit Kumar Todi ( punit_todi@da-iict.org )
// 	Patrick Kalkman  kalkman@cistron.nl
//      Umadevi S (sumadevi@novell.com)
//
// (C) 2003 Patrick Kalkman
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
using System.ComponentModel;

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
			[SetUp]
			public void GetReady ()
			{
				dataTable = new DataTable ("itemTable");
				DataColumn dc1 = new DataColumn ("itemId");
				DataColumn dc2 = new DataColumn ("itemName");
				DataColumn dc3 = new DataColumn ("itemPrice");
				DataColumn dc4 = new DataColumn ("itemCategory");
				
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
			[Ignore("Test code not implemented")]
			public void RowStateFilter ()
			{
				// TODO 			
				AssertEquals ("test#01",ListChangedType.Reset,listChangedArgs.ListChangedType);
			}
			[Test]
			public void Sort ()
			{
				dataView.Sort = "itemName DESC";
				AssertEquals ("test#01",ListChangedType.Reset,listChangedArgs.ListChangedType);
				// UnComment the line below to see if dataView is sorted
				// PrintTableOrView (dataView);
			}

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
				AssertEquals ("test#02",drv["itemName"],dataView [dataView.Count - 1]["itemName"]);
				drv["itemId"] = "item " + 1001;
				drv["itemName"] = "name " + rndm.Next();
				drv["itemPrice"] = "Rs. " + (rndm.Next() % 1000);
				drv["itemCategory"] = "Cat " + ((rndm.Next() % 10) + 1);
				AssertEquals ("test#01",ListChangedType.ItemChanged,listChangedArgs.ListChangedType);				
			}
			
			[Test]
			[Ignore("Test code not implemented")]
			public void BeginInit ()
			{
				//TODO
			}
			
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
				
				dr ["itemId"] = "aitem 0";
				// PrintTableOrView(dataView, "ItemChanged");
				AssertEquals ("test#02",ListChangedType.ItemChanged,listChangedArgs.ListChangedType);
				
				dr ["itemId"] = "zitem 0";
				// PrintTableOrView(dataView, "ItemMoved");
				AssertEquals ("test#03",ListChangedType.ItemMoved,listChangedArgs.ListChangedType);
				
				dataTable.Rows.Remove (dr);
				// PrintTableOrView(dataView, "ItemDeleted");
				AssertEquals ("test#04",ListChangedType.ItemDeleted,listChangedArgs.ListChangedType);
				
				DataColumn dc5 = new DataColumn ("itemDesc");
				dataTable.Columns.Add (dc5);
				// PrintTableOrView(dataView, "PropertyDescriptorAdded");
				AssertEquals ("test#05",ListChangedType.PropertyDescriptorAdded,listChangedArgs.ListChangedType);
				
				dc5.ColumnName = "itemDescription";
				// PrintTableOrView(dataView, "PropertyDescriptorChanged");
				// AssertEquals ("test#06",ListChangedType.PropertyDescriptorChanged,listChangedArgs.ListChangedType);
				
				dataTable.Columns.Remove (dc5);
				// PrintTableOrView(dataView, "PropertyDescriptorDeleted");
				AssertEquals ("test#07",ListChangedType.PropertyDescriptorDeleted,listChangedArgs.ListChangedType);
			}
	
			[Test]
                public void TestFindRows ()
                {
                        DataView TestView = new DataView (dataTable);
                        TestView.Sort = "itemId";
                        DataRowView[] Result = TestView.FindRows ("itemId");
                        Assertion.AssertEquals ("Dv #1", 1, Result.Length);
                        Assertion.AssertEquals ("Dv #2", "item 3", Result [0]["itemId"]);
                }
                                                                                                    
                [Test]
		[ExpectedException (typeof (DeletedRowInaccessibleException))]
                public void TestDelete ()
                {
                        DataView TestView = new DataView (dataTable);
                        TestView.Delete (0);
                        DataRow r = TestView.Table.Rows [0];
                        Assertion.Assert ("Dv #1", !(r ["itemId"] == "item 1"));
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

		}
}
