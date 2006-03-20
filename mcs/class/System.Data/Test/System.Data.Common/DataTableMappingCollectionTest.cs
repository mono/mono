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
	public class DataTableMappingCollectionTest : Assertion
	{
		DataTableMappingCollection tableMapCollection;
		DataTableMapping [] tabs;
		[SetUp]
		public void GetReady()
		{
			tabs=new DataTableMapping[5];
			tabs[0]=new DataTableMapping("sourceCustomers","dataSetCustomers");
			tabs[1]=new DataTableMapping("sourceEmployees","dataSetEmployees");
			tabs[2]=new DataTableMapping("sourceBooks","dataSetBooks");
			tabs[3]=new DataTableMapping("sourceStore","dataSetStore");
			tabs[4]=new DataTableMapping("sourceInventory","dataSetInventory");
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
			AssertEquals("test1",0,t);
			bool eq1=tabs[0].Equals(tableMapCollection[0]);
			AssertEquals("test2",true,eq1);
			AssertEquals("test3",1,tableMapCollection.Count);
			DataTableMapping tab2;
			tab2=tableMapCollection.Add("sourceEmployees","dataSetEmployees");
			bool eq2=tab2.Equals(tableMapCollection[1]);
			AssertEquals("test4",true,eq2);
			AssertEquals("test5",2,tableMapCollection.Count);
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
			tableMapCollection.Add(new DataTableMapping("sourceFactory","dataSetFactory"));
			AssertEquals("test1",1,tableMapCollection.Count);
			tableMapCollection.AddRange(tabs);
			AssertEquals("test2",6,tableMapCollection.Count);
			bool eq;
			eq=tabs[0].Equals(tableMapCollection[1]);
			AssertEquals("test3",true,eq);
			eq=tabs[1].Equals(tableMapCollection[2]);
			AssertEquals("test4",true,eq);
			eq=tabs[0].Equals(tableMapCollection[0]);
			AssertEquals("test5",false,eq);
			eq=tabs[1].Equals(tableMapCollection[0]);
			AssertEquals("test6",false,eq);			
		}
		
		[Test]
		public void Clear()
		{
			DataTableMapping tab1=new DataTableMapping("sourceSuppliers","dataSetSuppliers");
			tableMapCollection.Add((Object)tab1);
			AssertEquals("test1",1,tableMapCollection.Count);
			tableMapCollection.Clear();
			AssertEquals("test2",0,tableMapCollection.Count);
			tableMapCollection.AddRange(tabs);
			AssertEquals("test3",5,tableMapCollection.Count);
			tableMapCollection.Clear();
			AssertEquals("test4",0,tableMapCollection.Count);
		}
		
		[Test]
		public void Contains()
		{
			DataTableMapping tab1=new DataTableMapping("sourceCustomers","dataSetCustomers");
			tableMapCollection.AddRange(tabs);
			bool eq;
			eq=tableMapCollection.Contains((Object)tabs[0]);
			AssertEquals("test1",true,eq);
			eq=tableMapCollection.Contains((Object)tabs[1]);
			AssertEquals("test2",true,eq);
			eq=tableMapCollection.Contains((Object)tab1);
			AssertEquals("test3",false,eq);
			eq=tableMapCollection.Contains(tabs[0].SourceTable);
			AssertEquals("test4",true,eq);
			eq=tableMapCollection.Contains(tabs[1].SourceTable);
			AssertEquals("test5",true,eq);
			eq=tableMapCollection.Contains(tab1.SourceTable);
			AssertEquals("test6",true,eq);
			eq=tableMapCollection.Contains(tabs[0].DataSetTable);
			AssertEquals("test7",false,eq);
			eq=tableMapCollection.Contains(tabs[1].DataSetTable);
			AssertEquals("test8",false,eq);
			eq=tableMapCollection.Contains(tab1.DataSetTable);
			AssertEquals("test9",false,eq);
		}
		
		[Test]
		public void CopyTo()
		{
			DataTableMapping [] tabcops=new DataTableMapping[5];
			tableMapCollection.AddRange(tabs);
			tableMapCollection.CopyTo(tabcops,0);
			bool eq;
			for (int i=0;i<5;i++)
			{
				eq=tableMapCollection[i].Equals(tabcops[i]);
				AssertEquals("test1"+i,true,eq);
			}
			tabcops=null;
			tabcops=new DataTableMapping[7];
			tableMapCollection.CopyTo(tabcops,2);
			for (int i=0;i<5;i++)
			{
				eq=tableMapCollection[i].Equals(tabcops[i+2]);
				AssertEquals("test2"+i,true,eq);
			}
			eq=tableMapCollection[0].Equals(tabcops[0]);
			AssertEquals("test31",false,eq);
			eq=tableMapCollection[0].Equals(tabcops[1]);
			AssertEquals("test32",false,eq);
		}
		
		[Test]
		public void Equals()
		{
//			DataTableMappingCollection collect2=new DataTableMappingCollection();
			tableMapCollection.AddRange(tabs);
//			collect2.AddRange(tabs);
			DataTableMappingCollection copy1;
			copy1=tableMapCollection;
			
//			AssertEquals("test1",false,tableMapCollection.Equals(collect2));
			AssertEquals("test2",true,tableMapCollection.Equals(copy1));
//			AssertEquals("test3",false,collect2.Equals(tableMapCollection));
			AssertEquals("test4",true,copy1.Equals(tableMapCollection));
//			AssertEquals("test5",false,collect2.Equals(copy1));
			AssertEquals("test6",true,copy1.Equals(tableMapCollection));
			AssertEquals("test7",true,tableMapCollection.Equals(tableMapCollection));
//			AssertEquals("test8",true,collect2.Equals(collect2));
			AssertEquals("test9",true,copy1.Equals(copy1));
			
//			AssertEquals("test10",false,Object.Equals(collect2,tableMapCollection));
			AssertEquals("test11",true,Object.Equals(copy1,tableMapCollection));
//			AssertEquals("test12",false,Object.Equals(tableMapCollection,collect2));
			AssertEquals("test13",true,Object.Equals(tableMapCollection,copy1));
//			AssertEquals("test14",false,Object.Equals(copy1,collect2));
			AssertEquals("test15",true,Object.Equals(tableMapCollection,copy1));
			AssertEquals("test16",true,Object.Equals(tableMapCollection,tableMapCollection));
//			AssertEquals("test17",true,Object.Equals(collect2,collect2));
			AssertEquals("test18",true,Object.Equals(copy1,copy1));
//			AssertEquals("test10",false,Object.Equals(tableMapCollection,collect2));
			AssertEquals("test11",true,Object.Equals(tableMapCollection,copy1));
//			AssertEquals("test12",false,Object.Equals(collect2,tableMapCollection));
			AssertEquals("test13",true,Object.Equals(copy1,tableMapCollection));
//			AssertEquals("test14",false,Object.Equals(collect2,copy1));
			AssertEquals("test15",true,Object.Equals(copy1,tableMapCollection));
		}
		
		[Test]
		public void GetByDataSetTable()
		{
			tableMapCollection.AddRange(tabs);
			bool eq;
			DataTableMapping tab1;
			tab1=tableMapCollection.GetByDataSetTable("dataSetCustomers");
			eq=(tab1.DataSetTable.Equals("dataSetCustomers") && tab1.SourceTable.Equals("sourceCustomers"));
			AssertEquals("test1",true,eq);
			tab1=tableMapCollection.GetByDataSetTable("dataSetEmployees");
			eq=(tab1.DataSetTable.Equals("dataSetEmployees") && tab1.SourceTable.Equals("sourceEmployees"));
			AssertEquals("test2",true,eq);
						
			tab1=tableMapCollection.GetByDataSetTable("datasetcustomers");
			eq=(tab1.DataSetTable.Equals("dataSetCustomers") && tab1.SourceTable.Equals("sourceCustomers"));
			AssertEquals("test3",true,eq);
			tab1=tableMapCollection.GetByDataSetTable("datasetemployees");
			eq=(tab1.DataSetTable.Equals("dataSetEmployees") && tab1.SourceTable.Equals("sourceEmployees"));
			AssertEquals("test4",true,eq);
			
		}
		
		[Test]
		public void GetTableMappingBySchemaAction()
		{
			tableMapCollection.AddRange(tabs);
			bool eq;
			DataTableMapping tab1;
			tab1=DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection,"sourceCustomers","dataSetCustomers",MissingMappingAction.Passthrough);
			eq=(tab1.DataSetTable.Equals("dataSetCustomers") && tab1.SourceTable.Equals("sourceCustomers"));
			AssertEquals("test1",true,eq);
			tab1=DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection,"sourceEmployees","dataSetEmployees",MissingMappingAction.Passthrough);
			eq=(tab1.DataSetTable.Equals("dataSetEmployees") && tab1.SourceTable.Equals("sourceEmployees"));
			AssertEquals("test2",true,eq);
			
			tab1=DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection,"sourceData","dataSetData",MissingMappingAction.Passthrough);
			eq=(tab1.DataSetTable.Equals("sourceData") && tab1.SourceTable.Equals("dataSetData"));
			AssertEquals("test3",false, eq);
			eq=tableMapCollection.Contains(tab1);
			AssertEquals("test4",false,eq);
			tab1=DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection,"sourceData","dataSetData",MissingMappingAction.Ignore);
			AssertEquals("test5",null,tab1);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetTableMappingBySchemaActionException1()
		{
			DataTableMappingCollection.GetTableMappingBySchemaAction(tableMapCollection,"sourceCustomers","dataSetCustomers",MissingMappingAction.Error);
		}
		
		[Test]
		public void IndexOf()
		{
			tableMapCollection.AddRange(tabs);
			int ind;
			ind=tableMapCollection.IndexOf(tabs[0]);
			AssertEquals("test1",0,ind);
			ind=tableMapCollection.IndexOf(tabs[1]);
			AssertEquals("test2",1,ind);
					
			ind=tableMapCollection.IndexOf(tabs[0].SourceTable);
			AssertEquals("test3",0,ind);
			ind=tableMapCollection.IndexOf(tabs[1].SourceTable);
			AssertEquals("test4",1,ind);			
		}
		
		[Test]
		public void IndexOfDataSetTable()
		{
			tableMapCollection.AddRange(tabs);
			int ind;
			ind=tableMapCollection.IndexOfDataSetTable(tabs[0].DataSetTable);
			AssertEquals("test1",0,ind);
			ind=tableMapCollection.IndexOfDataSetTable(tabs[1].DataSetTable);
			AssertEquals("test2",1,ind);
						
			ind=tableMapCollection.IndexOfDataSetTable("datasetcustomers");
			AssertEquals("test3",0,ind);
			ind=tableMapCollection.IndexOfDataSetTable("datasetemployees");
			AssertEquals("test4",1,ind);
						
			ind=tableMapCollection.IndexOfDataSetTable("sourcedeter");
			AssertEquals("test5",-1,ind);
		}
		
		[Test]
		public void Insert()
		{
			tableMapCollection.AddRange(tabs);
			DataTableMapping mymap=new DataTableMapping("sourceTestAge","datatestSetAge");
			tableMapCollection.Insert(3,mymap);
			int ind=tableMapCollection.IndexOfDataSetTable("datatestSetAge");
			AssertEquals("test1",3,ind);
		}
		
		[Test]
		[Ignore ("This test is invalid; a mapping in a mapcollection must be identical.")]
		public void Remove()
		{
			tableMapCollection.AddRange(tabs);
			DataTableMapping mymap=new DataTableMapping("sourceCustomers","dataSetCustomers");
			tableMapCollection.Add(mymap);
			tableMapCollection.Remove((Object)mymap);
			bool eq=tableMapCollection.Contains((Object)mymap);
			AssertEquals("test1",false,eq);
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
			DataTableMapping mymap=new DataTableMapping("sourceAge","dataSetAge");
			tableMapCollection.Remove(mymap);
		}
		
		[Test]
		public void RemoveAt()
		{
			tableMapCollection.AddRange(tabs);
			bool eq;
			tableMapCollection.RemoveAt(0);
			eq=tableMapCollection.Contains(tabs[0]);
			AssertEquals("test1",false,eq);
			eq=tableMapCollection.Contains(tabs[1]);
			AssertEquals("test2",true,eq);
						
			tableMapCollection.RemoveAt("sourceEmployees");
			eq=tableMapCollection.Contains(tabs[1]);
			AssertEquals("test3",false,eq);
			eq=tableMapCollection.Contains(tabs[2]);
			AssertEquals("test4",true,eq);			
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
			AssertEquals("test1","System.Data.Common.DataTableMappingCollection",tableMapCollection.ToString());
		}
	}
}
