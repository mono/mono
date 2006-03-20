// DataColumnMappingCollectionTest.cs - NUnit Test Cases for Testing the 
// DataColumnMappingCollection class
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
	public class DataColumnMappingCollectionTest : Assertion
	{
		//DataTableMapping tableMap;
		DataColumnMappingCollection columnMapCollection;
		DataColumnMapping [] cols;
		
		[SetUp]
		public void GetReady()
		{
			cols=new DataColumnMapping[5];
			cols[0]=new DataColumnMapping("sourceName","dataSetName");
			cols[1]=new DataColumnMapping("sourceID","dataSetID");
			cols[2]=new DataColumnMapping("sourceAddress","dataSetAddress");
			cols[3]=new DataColumnMapping("sourcePhone","dataSetPhone");
			cols[4]=new DataColumnMapping("sourcePIN","dataSetPIN");
			columnMapCollection=new DataColumnMappingCollection();			
		}
		
		[TearDown]
		public void Clean()
		{
			columnMapCollection.Clear();
		}
		
		[Test]
		public void Add()
		{
			DataColumnMapping col1=new DataColumnMapping("sourceName","dataSetName");
			int t=columnMapCollection.Add((Object)col1);
			AssertEquals("test1",0,t);
			bool eq1=col1.Equals(columnMapCollection[0]);
			AssertEquals("test2",true,eq1);
			AssertEquals("test3",1,columnMapCollection.Count);
			DataColumnMapping col2;
			col2=columnMapCollection.Add("sourceID","dataSetID");
			bool eq2=col2.Equals(columnMapCollection[1]);
			AssertEquals("test4",true,eq2);
			AssertEquals("test5",2,columnMapCollection.Count);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void AddException1()
		{
			DataColumnMappingCollection c=new DataColumnMappingCollection();
			columnMapCollection.Add((Object)c);
		}
		
		[Test]
		public void AddRange()
		{
			columnMapCollection.Add(new DataColumnMapping("sourceAge","dataSetAge"));
			AssertEquals("test1",1,columnMapCollection.Count);
			columnMapCollection.AddRange(cols);
			AssertEquals("test2",6,columnMapCollection.Count);
			bool eq;
			eq=cols[0].Equals(columnMapCollection[1]);
			AssertEquals("test3",true,eq);
			eq=cols[1].Equals(columnMapCollection[2]);
			AssertEquals("test4",true,eq);
			
			eq=cols[0].Equals(columnMapCollection[0]);
			AssertEquals("test5",false,eq);
			eq=cols[1].Equals(columnMapCollection[0]);
			AssertEquals("test6",false,eq);			
		}
		
		[Test]
		public void Clear()
		{
			DataColumnMapping col1=new DataColumnMapping("sourceName","dataSetName");
			columnMapCollection.Add((Object)col1);
			AssertEquals("test1",1,columnMapCollection.Count);
			columnMapCollection.Clear();
			AssertEquals("test2",0,columnMapCollection.Count);
			columnMapCollection.AddRange(cols);
			AssertEquals("test3",5,columnMapCollection.Count);
			columnMapCollection.Clear();
			AssertEquals("test4",0,columnMapCollection.Count);
		}
		
		[Test]
		public void Contains()
		{
			DataColumnMapping col1=new DataColumnMapping("sourceName","dataSetName");
			columnMapCollection.AddRange(cols);
			bool eq;
			eq=columnMapCollection.Contains((Object)cols[0]);
			AssertEquals("test1",true,eq);
			eq=columnMapCollection.Contains((Object)cols[1]);
			AssertEquals("test2",true,eq);
			
			eq=columnMapCollection.Contains((Object)col1);
			AssertEquals("test3",false,eq);
			
			eq=columnMapCollection.Contains(cols[0].SourceColumn);
			AssertEquals("test4",true,eq);
			eq=columnMapCollection.Contains(cols[1].SourceColumn);
			AssertEquals("test5",true,eq);
			
			eq=columnMapCollection.Contains(col1.SourceColumn);
			AssertEquals("test6",true,eq);
			
			eq=columnMapCollection.Contains(cols[0].DataSetColumn);
			AssertEquals("test7",false,eq);
			eq=columnMapCollection.Contains(cols[1].DataSetColumn);
			AssertEquals("test8",false,eq);
			
			eq=columnMapCollection.Contains(col1.DataSetColumn);
			AssertEquals("test9",false,eq);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void ContainsException1()
		{
			Object o = new Object();
			bool a = columnMapCollection.Contains(o);
		}
		
		[Test]
		public void CopyTo()
		{
			DataColumnMapping [] colcops=new DataColumnMapping[5];
			columnMapCollection.AddRange(cols);
			columnMapCollection.CopyTo(colcops,0);
			bool eq;
			for (int i=0;i<5;i++)
			{
				eq=columnMapCollection[i].Equals(colcops[i]);
				AssertEquals("test1"+i,true,eq);
			}
			colcops=null;
			colcops=new DataColumnMapping[7];
			columnMapCollection.CopyTo(colcops,2);
			for (int i=0;i<5;i++)
			{
				eq=columnMapCollection[i].Equals(colcops[i+2]);
				AssertEquals("test2"+i,true,eq);
			}
			eq=columnMapCollection[0].Equals(colcops[0]);
			AssertEquals("test31",false,eq);
			eq=columnMapCollection[0].Equals(colcops[1]);
			AssertEquals("test32",false,eq);
		}
		
		[Test]
		public void Equals()
		{
//			DataColumnMappingCollection collect2=new DataColumnMappingCollection();
			columnMapCollection.AddRange(cols);
//			collect2.AddRange(cols);
			DataColumnMappingCollection copy1;
			copy1=columnMapCollection;
			
//			AssertEquals("test1",false,columnMapCollection.Equals(collect2));
			AssertEquals("test2",true,columnMapCollection.Equals(copy1));
//			AssertEquals("test3",false,collect2.Equals(columnMapCollection));
			AssertEquals("test4",true,copy1.Equals(columnMapCollection));
//			AssertEquals("test5",false,collect2.Equals(copy1));
			AssertEquals("test6",true,copy1.Equals(columnMapCollection));
			AssertEquals("test7",true,columnMapCollection.Equals(columnMapCollection));
//			AssertEquals("test8",true,collect2.Equals(collect2));
			AssertEquals("test9",true,copy1.Equals(copy1));
			
//			AssertEquals("test10",false,Object.Equals(collect2,columnMapCollection));
			AssertEquals("test11",true,Object.Equals(copy1,columnMapCollection));
//			AssertEquals("test12",false,Object.Equals(columnMapCollection,collect2));
			AssertEquals("test13",true,Object.Equals(columnMapCollection,copy1));
//			AssertEquals("test14",false,Object.Equals(copy1,collect2));
			AssertEquals("test15",true,Object.Equals(columnMapCollection,copy1));
			AssertEquals("test16",true,Object.Equals(columnMapCollection,columnMapCollection));
//			AssertEquals("test17",true,Object.Equals(collect2,collect2));
			AssertEquals("test18",true,Object.Equals(copy1,copy1));
//			AssertEquals("test10",false,Object.Equals(columnMapCollection,collect2));
			AssertEquals("test11",true,Object.Equals(columnMapCollection,copy1));
//			AssertEquals("test12",false,Object.Equals(collect2,columnMapCollection));
			AssertEquals("test13",true,Object.Equals(copy1,columnMapCollection));
//			AssertEquals("test14",false,Object.Equals(collect2,copy1));
			AssertEquals("test15",true,Object.Equals(copy1,columnMapCollection));
		}
		
		[Test]
		public void GetByDataSetColumn()
		{
			columnMapCollection.AddRange(cols);
			bool eq;
			DataColumnMapping col1;
			col1=columnMapCollection.GetByDataSetColumn("dataSetName");
			eq=(col1.DataSetColumn.Equals("dataSetName") && col1.SourceColumn.Equals("sourceName"));
			AssertEquals("test1",true,eq);
			col1=columnMapCollection.GetByDataSetColumn("dataSetID");
			eq=(col1.DataSetColumn.Equals("dataSetID") && col1.SourceColumn.Equals("sourceID"));
			AssertEquals("test2",true,eq);
						
			col1=columnMapCollection.GetByDataSetColumn("datasetname");
			eq=(col1.DataSetColumn.Equals("dataSetName") && col1.SourceColumn.Equals("sourceName"));
			AssertEquals("test3",true,eq);
			col1=columnMapCollection.GetByDataSetColumn("datasetid");
			eq=(col1.DataSetColumn.Equals("dataSetID") && col1.SourceColumn.Equals("sourceID"));
			AssertEquals("test4",true,eq);			
		}
		
		[Test]
		public void GetColumnMappingBySchemaAction()
		{
			columnMapCollection.AddRange(cols);
			bool eq;
			DataColumnMapping col1;
			col1=DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection,"sourceName",MissingMappingAction.Passthrough);
			eq=(col1.DataSetColumn.Equals("dataSetName") && col1.SourceColumn.Equals("sourceName"));
			AssertEquals("test1",true,eq);
			col1=DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection,"sourceID",MissingMappingAction.Passthrough);
			eq=(col1.DataSetColumn.Equals("dataSetID") && col1.SourceColumn.Equals("sourceID"));
			AssertEquals("test2",true,eq);
			
			col1=DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection,"sourceData",MissingMappingAction.Passthrough);
			eq=(col1.DataSetColumn.Equals("sourceData") && col1.SourceColumn.Equals("sourceData"));
			AssertEquals("test3",true,eq);
			eq=columnMapCollection.Contains(col1);
			AssertEquals("test4",false,eq);
			col1=DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection,"sourceData",MissingMappingAction.Ignore);
			AssertEquals("test5",null,col1);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetColumnMappingBySchemaActionException1()
		{
			DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection,"sourceName",MissingMappingAction.Error);
		}
		
		[Test]
		public void IndexOf()
		{
			columnMapCollection.AddRange(cols);
			int ind;
			ind=columnMapCollection.IndexOf(cols[0]);
			AssertEquals("test1",0,ind);
			ind=columnMapCollection.IndexOf(cols[1]);
			AssertEquals("test2",1,ind);
					
			ind=columnMapCollection.IndexOf(cols[0].SourceColumn);
			AssertEquals("test3",0,ind);
			ind=columnMapCollection.IndexOf(cols[1].SourceColumn);
			AssertEquals("test4",1,ind);			
		}
		
		[Test]
		public void IndexOfDataSetColumn()
		{
			columnMapCollection.AddRange(cols);
			int ind;
			ind=columnMapCollection.IndexOfDataSetColumn(cols[0].DataSetColumn);
			AssertEquals("test1",0,ind);
			ind=columnMapCollection.IndexOfDataSetColumn(cols[1].DataSetColumn);
			AssertEquals("test2",1,ind);			
			
			ind=columnMapCollection.IndexOfDataSetColumn("datasetname");
			AssertEquals("test3",0,ind);
			ind=columnMapCollection.IndexOfDataSetColumn("datasetid");
			AssertEquals("test4",1,ind);
						
			ind=columnMapCollection.IndexOfDataSetColumn("sourcedeter");
			AssertEquals("test5",-1,ind);
		}
		
		[Test]
		public void Insert()
		{
			columnMapCollection.AddRange(cols);
			DataColumnMapping mymap=new DataColumnMapping("sourceAge","dataSetAge");
			columnMapCollection.Insert(3,mymap);
			int ind=columnMapCollection.IndexOfDataSetColumn("dataSetAge");
			AssertEquals("test1",3,ind);			
		}
		
		[Test]
		[Ignore ("This test is wrong. A mapping in a DataColumnMappingCollection must be identical.")]
		public void Remove()
		{
			columnMapCollection.AddRange(cols);
			DataColumnMapping mymap=new DataColumnMapping("sourceName","dataSetName");
			columnMapCollection.Add(mymap);
			columnMapCollection.Remove((Object)mymap);
			bool eq=columnMapCollection.Contains((Object)mymap);
			AssertEquals("test1",false,eq);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void RemoveException1()
		{
			String te="testingdata";
			columnMapCollection.AddRange(cols);
			columnMapCollection.Remove(te);			
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RemoveException2()
		{
			columnMapCollection.AddRange(cols);
			DataColumnMapping mymap=new DataColumnMapping("sourceAge","dataSetAge");
			columnMapCollection.Remove(mymap);
		}
		
		[Test]
		public void RemoveAt()
		{
			columnMapCollection.AddRange(cols);
			bool eq;
			columnMapCollection.RemoveAt(0);
			eq=columnMapCollection.Contains(cols[0]);
			AssertEquals("test1",false,eq);
			eq=columnMapCollection.Contains(cols[1]);
			AssertEquals("test2",true,eq);
						
			columnMapCollection.RemoveAt("sourceID");
			eq=columnMapCollection.Contains(cols[1]);
			AssertEquals("test3",false,eq);
			eq=columnMapCollection.Contains(cols[2]);
			AssertEquals("test4",true,eq);			
		}
		
		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void RemoveAtException1()
		{
			columnMapCollection.RemoveAt(3);			
		}
		
		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void RemoveAtException2()
		{
			columnMapCollection.RemoveAt("sourceAge");			
		}
		
		[Test]
#if TARGET_JVM
		[Ignore ("Does not work with TARGET_JVM")]
#endif
		public void ToStringTest()
		{
			AssertEquals("test1","System.Data.Common.DataColumnMappingCollection",columnMapCollection.ToString());
		}
	}
}
