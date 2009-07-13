// DataTableMappingCollectionTest.cs - NUnit Test Cases for Testing the 
// DataTableMappingCollection class
//
// Author: Ameya Sailesh Gargesh (ameya_13@yahoo.com)
//
// (C) Ameya Gargesh

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
using System.Data.Common;
namespace MonoTests.System.Data.Common
{
	[TestFixture]
	public class DataTableMappingCollectionTest
	{
		DataTableMappingCollection tableMapCollection;
		DataTableMapping [] tabs;
		[SetUp]
		public void GetReady()
		{
			tabs=new DataTableMapping[5];
			tabs[0]=new DataTableMapping("sourceCustomers", "dataSetCustomers");
			tabs[1]=new DataTableMapping("sourceEmployees", "dataSetEmployees");
			tabs[2]=new DataTableMapping("sourceBooks", "dataSetBooks");
			tabs[3]=new DataTableMapping("sourceStore", "dataSetStore");
			tabs[4]=new DataTableMapping("sourceInventory", "dataSetInventory");
			tableMapCollection=new DataTableMappingCollection();
		}
		
		[TearDown]
		public void Clean()
		{
			tableMapCollection.Clear();
		}
		
		[Test]
		public void Add()
		{
			int t=tableMapCollection.Add((Object)tabs[0]);
			Assert.AreEqual(0, t, "test1");
			bool eq1=tabs[0].Equals(tableMapCollection[0]);
			Assert.AreEqual(true, eq1, "test2");
			Assert.AreEqual(1, tableMapCollection.Count, "test3");
			DataTableMapping tab2;
			tab2=tableMapCollection.Add("sourceEmployees", "dataSetEmployees");
			bool eq2=tab2.Equals(tableMapCollection[1]);
			Assert.AreEqual(true, eq2, "test4");
			Assert.AreEqual(2, tableMapCollection.Count, "test5");
		}
		
		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void AddException1()
		{
			DataTableMappingCollection c=new DataTableMappingCollection();
			tableMapCollection.Add((Object)c);
		}
		
		[Test]
		public void AddRange()
		{
			tableMapCollection.Add(new DataTableMapping("sourceFactory", "dataSetFactory"));
			Assert.AreEqual(1, tableMapCollection.Count, "test1");
			tableMapCollection.AddRange(tabs);
			Assert.AreEqual(6, tableMapCollection.Count, "test2");
			bool eq;
			eq=tabs[0].Equals(tableMapCollection[1]);
			Assert.AreEqual(true, eq, "test3");
			eq=tabs[1].Equals(tableMapCollection[2]);
			Assert.AreEqual(true, eq, "test4");
			eq=tabs[0].Equals(tableMapCollection[0]);
			Assert.AreEqual(false, eq, "test5");
			eq=tabs[1].Equals(tableMapCollection[0]);
			Assert.AreEqual(false, eq, "test6");
		}
		
		[Test]
		public void Clear()
		{
			DataTableMapping tab1=new DataTableMapping("sourceSuppliers", "dataSetSuppliers");
			tableMapCollection.Add((Object)tab1);
			Assert.AreEqual(1, tableMapCollection.Count, "test1");
			tableMapCollection.Clear();
			Assert.AreEqual(0, tableMapCollection.Count, "test2");
			tableMapCollection.AddRange(tabs);
			Assert.AreEqual(5, tableMapCollection.Count, "test3");
			tableMapCollection.Clear();
			Assert.AreEqual(0, tableMapCollection.Count, "test4");
		}
		
		[Test]
		public void Contains()
		{
			DataTableMapping tab1=new DataTableMapping("sourceCustomers", "dataSetCustomers");
			tableMapCollection.AddRange(tabs);
			bool eq;
			eq=tableMapCollection.Contains((Object)tabs[0]);
			Assert.AreEqual(true, eq, "test1");
			eq=tableMapCollection.Contains((Object)tabs[1]);
			Assert.AreEqual(true, eq, "test2");
			eq=tableMapCollection.Contains((Object)tab1);
			Assert.AreEqual(false, eq, "test3");
			eq=tableMapCollection.Contains(tabs[0].SourceTable);
			Assert.AreEqual(true, eq, "test4");
			eq=tableMapCollection.Contains(tabs[1].SourceTable);
			Assert.AreEqual(true, eq, "test5");
			eq=tableMapCollection.Contains(tab1.SourceTable);
			Assert.AreEqual(true, eq, "test6");
			eq=tableMapCollection.Contains(tabs[0].DataSetTable);
			Assert.AreEqual(false, eq, "test7");
			eq=tableMapCollection.Contains(tabs[1].DataSetTable);
			Assert.AreEqual(false, eq, "test8");
			eq=tableMapCollection.Contains(tab1.DataSetTable);
			Assert.AreEqual(false, eq, "test9");
		}
		
		[Test]
		public void CopyTo()
		{
			DataTableMapping [] tabcops=new DataTableMapping[5];
			tableMapCollection.AddRange(tabs);
			tableMapCollection.CopyTo(tabcops, 0);
			bool eq;
			for (int i=0;i<5;i++)
			{
				eq=tableMapCollection[i].Equals(tabcops[i]);
				Assert.AreEqual (true, eq, "test1" + i);
			}
			tabcops=null;
			tabcops=new DataTableMapping[7];
			tableMapCollection.CopyTo(tabcops, 2);
			for (int i=0;i<5;i++)
			{
				eq=tableMapCollection[i].Equals(tabcops[i+2]);
				Assert.AreEqual (true, eq, "test2" + i);
			}
			eq=tableMapCollection[0].Equals(tabcops[0]);
			Assert.AreEqual (false, eq, "test31");
			eq=tableMapCollection[0].Equals(tabcops[1]);
			Assert.AreEqual (false, eq, "test32");
		}
		
		[Test]
		public void Equals()
		{
//			DataTableMappingCollection collect2=new DataTableMappingCollection();
			tableMapCollection.AddRange(tabs);
//			collect2.AddRange(tabs);
			DataTableMappingCollection copy1;
			copy1=tableMapCollection;
			
//			Assert.AreEqual(false, tableMapCollection.Equals(collect2), "test1");
			Assert.AreEqual(true, tableMapCollection.Equals(copy1), "test2");
//			Assert.AreEqual(false, collect2.Equals(tableMapCollection), "test3");
			Assert.AreEqual(true, copy1.Equals(tableMapCollection), "test4");
//			Assert.AreEqual(false, collect2.Equals(copy1), "test5");
			Assert.AreEqual(true, copy1.Equals(tableMapCollection), "test6");
			Assert.AreEqual(true, tableMapCollection.Equals(tableMapCollection), "test7");
//			Assert.AreEqual(true, collect2.Equals(collect2), "test8");
			Assert.AreEqual(true, copy1.Equals(copy1), "test9");
			
//			Assert.AreEqual(false, Object.Equals(collect2, tableMapCollection), "test10");
			Assert.AreEqual(true, Object.Equals(copy1, tableMapCollection), "test11");
//			Assert.AreEqual(false, Object.Equals(tableMapCollection, collect2), "test12");
			Assert.AreEqual(true, Object.Equals(tableMapCollection, copy1), "test13");
//			Assert.AreEqual(false, Object.Equals(copy1, collect2), "test14");
			Assert.AreEqual(true, Object.Equals(tableMapCollection, copy1), "test15");
			Assert.AreEqual(true, Object.Equals(tableMapCollection, tableMapCollection), "test16");
//			Assert.AreEqual(true, Object.Equals(collect2, collect2), "test17");
			Assert.AreEqual(true, Object.Equals(copy1, copy1), "test18");
//			Assert.AreEqual(false, Object.Equals(tableMapCollection, collect2), "test10");
			Assert.AreEqual(true, Object.Equals(tableMapCollection, copy1), "test11");
//			Assert.AreEqual(false, Object.Equals(collect2, tableMapCollection), "test12");
			Assert.AreEqual(true, Object.Equals(copy1, tableMapCollection), "test13");
//			Assert.AreEqual(false, Object.Equals(collect2, copy1), "test14");
			Assert.AreEqual(true, Object.Equals(copy1, tableMapCollection), "test15");
		}
		
		[Test]
		public void GetByDataSetTable()
		{
			tableMapCollection.AddRange(tabs);
			bool eq;
			DataTableMapping tab1;
			tab1=tableMapCollection.GetByDataSetTable("dataSetCustomers");
			eq=(tab1.DataSetTable.Equals("dataSetCustomers") && tab1.SourceTable.Equals("sourceCustomers"));
			Assert.AreEqual(true, eq, "test1");
			tab1=tableMapCollection.GetByDataSetTable("dataSetEmployees");
			eq=(tab1.DataSetTable.Equals("dataSetEmployees") && tab1.SourceTable.Equals("sourceEmployees"));
			Assert.AreEqual(true, eq, "test2");
						
			tab1=tableMapCollection.GetByDataSetTable("datasetcustomers");
			eq=(tab1.DataSetTable.Equals("dataSetCustomers") && tab1.SourceTable.Equals("sourceCustomers"));
			Assert.AreEqual(true, eq, "test3");
			tab1=tableMapCollection.GetByDataSetTable("datasetemployees");
			eq=(tab1.DataSetTable.Equals("dataSetEmployees") && tab1.SourceTable.Equals("sourceEmployees"));
			Assert.AreEqual(true, eq, "test4");
			
		}
		
		[Test]
		public void GetTableMappingBySchemaAction()
		{
			tableMapCollection.AddRange(tabs);
			bool eq;
			DataTableMapping tab1;
			tab1=DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection, "sourceCustomers", "dataSetCustomers", MissingMappingAction.Passthrough);
			eq=(tab1.DataSetTable.Equals("dataSetCustomers") && tab1.SourceTable.Equals("sourceCustomers"));
			Assert.AreEqual(true, eq, "test1");
			tab1=DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection, "sourceEmployees", "dataSetEmployees", MissingMappingAction.Passthrough);
			eq=(tab1.DataSetTable.Equals("dataSetEmployees") && tab1.SourceTable.Equals("sourceEmployees"));
			Assert.AreEqual(true, eq, "test2");
			
			tab1=DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection, "sourceData", "dataSetData", MissingMappingAction.Passthrough);
			eq=(tab1.DataSetTable.Equals("sourceData") && tab1.SourceTable.Equals("dataSetData"));
			Assert.AreEqual(false, eq, "test3");
			eq=tableMapCollection.Contains(tab1);
			Assert.AreEqual(false, eq, "test4");
			tab1=DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection, "sourceData", "dataSetData", MissingMappingAction.Ignore);
			Assert.AreEqual(null, tab1, "test5");
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetTableMappingBySchemaActionException1()
		{
			DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection, "sourceCustomers", "dataSetCustomers", MissingMappingAction.Error);
		}
		
		[Test]
		public void IndexOf()
		{
			tableMapCollection.AddRange(tabs);
			int ind;
			ind=tableMapCollection.IndexOf(tabs[0]);
			Assert.AreEqual(0, ind, "test1");
			ind=tableMapCollection.IndexOf(tabs[1]);
			Assert.AreEqual(1, ind, "test2");
					
			ind=tableMapCollection.IndexOf(tabs[0].SourceTable);
			Assert.AreEqual(0, ind, "test3");
			ind=tableMapCollection.IndexOf(tabs[1].SourceTable);
			Assert.AreEqual(1, ind, "test4");
		}
		
		[Test]
		public void IndexOfDataSetTable()
		{
			tableMapCollection.AddRange(tabs);
			int ind;
			ind=tableMapCollection.IndexOfDataSetTable(tabs[0].DataSetTable);
			Assert.AreEqual(0, ind, "test1");
			ind=tableMapCollection.IndexOfDataSetTable(tabs[1].DataSetTable);
			Assert.AreEqual(1, ind, "test2");
						
			ind=tableMapCollection.IndexOfDataSetTable("datasetcustomers");
			Assert.AreEqual(0, ind, "test3");
			ind=tableMapCollection.IndexOfDataSetTable("datasetemployees");
			Assert.AreEqual(1, ind, "test4");
						
			ind=tableMapCollection.IndexOfDataSetTable("sourcedeter");
			Assert.AreEqual(-1, ind, "test5");
		}
		
		[Test]
		public void Insert()
		{
			tableMapCollection.AddRange(tabs);
			DataTableMapping mymap=new DataTableMapping("sourceTestAge", "datatestSetAge");
			tableMapCollection.Insert(3, mymap);
			int ind=tableMapCollection.IndexOfDataSetTable("datatestSetAge");
			Assert.AreEqual(3, ind, "test1");
		}
		
		[Test]
		[Ignore ("This test is invalid; a mapping in a mapcollection must be identical.")]
		public void Remove()
		{
			tableMapCollection.AddRange(tabs);
			DataTableMapping mymap=new DataTableMapping("sourceCustomers", "dataSetCustomers");
			tableMapCollection.Add(mymap);
			tableMapCollection.Remove((Object)mymap);
			bool eq=tableMapCollection.Contains((Object)mymap);
			Assert.AreEqual(false, eq, "test1");
		}
		
		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void RemoveException1()
		{
			String te="testingdata";
			tableMapCollection.AddRange(tabs);
			tableMapCollection.Remove(te);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RemoveException2()
		{
			tableMapCollection.AddRange(tabs);
			DataTableMapping mymap=new DataTableMapping("sourceAge", "dataSetAge");
			tableMapCollection.Remove(mymap);
		}
		
		[Test]
		public void RemoveAt()
		{
			tableMapCollection.AddRange(tabs);
			bool eq;
			tableMapCollection.RemoveAt(0);
			eq=tableMapCollection.Contains(tabs[0]);
			Assert.AreEqual(false, eq, "test1");
			eq=tableMapCollection.Contains(tabs[1]);
			Assert.AreEqual(true, eq, "test2");
						
			tableMapCollection.RemoveAt("sourceEmployees");
			eq=tableMapCollection.Contains(tabs[1]);
			Assert.AreEqual(false, eq, "test3");
			eq=tableMapCollection.Contains(tabs[2]);
			Assert.AreEqual(true, eq, "test4");
		}
		
		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void RemoveAtException1()
		{
			tableMapCollection.RemoveAt(3);
		}
		
		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void RemoveAtException2()
		{
			tableMapCollection.RemoveAt("sourceAge");
		}
		
		[Test]
#if TARGET_JVM
		[Ignore ("Does not work with TARGET_JVM")]
#endif
		public void ToStringTest()
		{
			Assert.AreEqual("System.Data.Common.DataTableMappingCollection", tableMapCollection.ToString(), "test1");
		}
	}
}
