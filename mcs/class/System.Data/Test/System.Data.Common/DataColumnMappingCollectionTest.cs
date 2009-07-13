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
	public class DataColumnMappingCollectionTest
	{
		//DataTableMapping tableMap;
		DataColumnMappingCollection columnMapCollection;
		DataColumnMapping [] cols;
		
		[SetUp]
		public void GetReady()
		{
			cols=new DataColumnMapping[5];
			cols[0]=new DataColumnMapping("sourceName", "dataSetName");
			cols[1]=new DataColumnMapping("sourceID", "dataSetID");
			cols[2]=new DataColumnMapping("sourceAddress", "dataSetAddress");
			cols[3]=new DataColumnMapping("sourcePhone", "dataSetPhone");
			cols[4]=new DataColumnMapping("sourcePIN", "dataSetPIN");
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
			DataColumnMapping col1=new DataColumnMapping("sourceName", "dataSetName");
			int t=columnMapCollection.Add((Object)col1);
			Assert.AreEqual (0, t, "test1");
			bool eq1=col1.Equals(columnMapCollection[0]);
			Assert.AreEqual (true, eq1, "test2");
			Assert.AreEqual (1, columnMapCollection.Count, "test3");
			DataColumnMapping col2;
			col2=columnMapCollection.Add("sourceID", "dataSetID");
			bool eq2=col2.Equals(columnMapCollection[1]);
			Assert.AreEqual (true, eq2, "test4");
			Assert.AreEqual (2, columnMapCollection.Count, "test5");
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
			columnMapCollection.Add(new DataColumnMapping("sourceAge", "dataSetAge"));
			Assert.AreEqual (1, columnMapCollection.Count, "test1");
			columnMapCollection.AddRange(cols);
			Assert.AreEqual (6, columnMapCollection.Count, "test2");
			bool eq;
			eq=cols[0].Equals(columnMapCollection[1]);
			Assert.AreEqual (true, eq, "test3");
			eq=cols[1].Equals(columnMapCollection[2]);
			Assert.AreEqual (true, eq, "test4");
			
			eq=cols[0].Equals(columnMapCollection[0]);
			Assert.AreEqual (false, eq, "test5");
			eq=cols[1].Equals(columnMapCollection[0]);
			Assert.AreEqual (false, eq, "test6");
		}
		
		[Test]
		public void Clear()
		{
			DataColumnMapping col1=new DataColumnMapping("sourceName", "dataSetName");
			columnMapCollection.Add((Object)col1);
			Assert.AreEqual (1, columnMapCollection.Count, "test1");
			columnMapCollection.Clear();
			Assert.AreEqual (0, columnMapCollection.Count, "test2");
			columnMapCollection.AddRange(cols);
			Assert.AreEqual (5, columnMapCollection.Count, "test3");
			columnMapCollection.Clear();
			Assert.AreEqual (0, columnMapCollection.Count, "test4");
		}
		
		[Test]
		public void Contains()
		{
			DataColumnMapping col1=new DataColumnMapping("sourceName", "dataSetName");
			columnMapCollection.AddRange(cols);
			bool eq;
			eq=columnMapCollection.Contains((Object)cols[0]);
			Assert.AreEqual (true, eq, "test1");
			eq=columnMapCollection.Contains((Object)cols[1]);
			Assert.AreEqual (true, eq, "test2");
			
			eq=columnMapCollection.Contains((Object)col1);
			Assert.AreEqual (false, eq, "test3");
			
			eq=columnMapCollection.Contains(cols[0].SourceColumn);
			Assert.AreEqual (true, eq, "test4");
			eq=columnMapCollection.Contains(cols[1].SourceColumn);
			Assert.AreEqual (true, eq, "test5");
			
			eq=columnMapCollection.Contains(col1.SourceColumn);
			Assert.AreEqual (true, eq, "test6");
			
			eq=columnMapCollection.Contains(cols[0].DataSetColumn);
			Assert.AreEqual (false, eq, "test7");
			eq=columnMapCollection.Contains(cols[1].DataSetColumn);
			Assert.AreEqual (false, eq, "test8");
			
			eq=columnMapCollection.Contains(col1.DataSetColumn);
			Assert.AreEqual (false, eq, "test9");
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
			columnMapCollection.CopyTo(colcops, 0);
			bool eq;
			for (int i=0;i<5;i++)
			{
				eq=columnMapCollection[i].Equals(colcops[i]);
				Assert.AreEqual (true, eq, "test1" + i);
			}
			colcops=null;
			colcops=new DataColumnMapping[7];
			columnMapCollection.CopyTo(colcops, 2);
			for (int i=0;i<5;i++)
			{
				eq=columnMapCollection[i].Equals(colcops[i+2]);
				Assert.AreEqual (true, eq, "test2" + i);
			}
			eq=columnMapCollection[0].Equals(colcops[0]);
			Assert.AreEqual (false, eq, "test31");
			eq=columnMapCollection[0].Equals(colcops[1]);
			Assert.AreEqual (false, eq, "test32");
		}
		
		[Test]
		public void Equals()
		{
//			DataColumnMappingCollection collect2=new DataColumnMappingCollection();
			columnMapCollection.AddRange(cols);
//			collect2.AddRange(cols);
			DataColumnMappingCollection copy1;
			copy1=columnMapCollection;
			
//			Assert.AreEqual (false, columnMapCollection.Equals(collect2), "test1");
			Assert.AreEqual (true, columnMapCollection.Equals(copy1), "test2");
//			Assert.AreEqual (false, collect2.Equals(columnMapCollection), "test3");
			Assert.AreEqual (true, copy1.Equals(columnMapCollection), "test4");
//			Assert.AreEqual (false, collect2.Equals(copy1), "test5");
			Assert.AreEqual (true, copy1.Equals(columnMapCollection), "test6");
			Assert.AreEqual (true, columnMapCollection.Equals(columnMapCollection), "test7");
//			Assert.AreEqual (true, collect2.Equals(collect2), "test8");
			Assert.AreEqual (true, copy1.Equals(copy1), "test9");
			
//			Assert.AreEqual (false, Object.Equals(collect2, columnMapCollection), "test10");
			Assert.AreEqual (true, Object.Equals(copy1, columnMapCollection), "test11");
//			Assert.AreEqual (false, Object.Equals(columnMapCollection, collect2), "test12");
			Assert.AreEqual (true, Object.Equals(columnMapCollection, copy1), "test13");
//			Assert.AreEqual (false, Object.Equals(copy1, collect2), "test14");
			Assert.AreEqual (true, Object.Equals(columnMapCollection, copy1), "test15");
			Assert.AreEqual (true, Object.Equals(columnMapCollection, columnMapCollection), "test16");
//			Assert.AreEqual (true, Object.Equals(collect2, collect2), "test17");
			Assert.AreEqual (true, Object.Equals(copy1, copy1), "test18");
//			Assert.AreEqual (false, Object.Equals(columnMapCollection, collect2), "test10");
			Assert.AreEqual (true, Object.Equals(columnMapCollection, copy1), "test11");
//			Assert.AreEqual (false, Object.Equals(collect2, columnMapCollection), "test12");
			Assert.AreEqual (true, Object.Equals(copy1, columnMapCollection), "test13");
//			Assert.AreEqual (false, Object.Equals(collect2, copy1), "test14");
			Assert.AreEqual (true, Object.Equals(copy1, columnMapCollection), "test15");
		}
		
		[Test]
		public void GetByDataSetColumn()
		{
			columnMapCollection.AddRange(cols);
			bool eq;
			DataColumnMapping col1;
			col1=columnMapCollection.GetByDataSetColumn("dataSetName");
			eq=(col1.DataSetColumn.Equals("dataSetName") && col1.SourceColumn.Equals("sourceName"));
			Assert.AreEqual (true, eq, "test1");
			col1=columnMapCollection.GetByDataSetColumn("dataSetID");
			eq=(col1.DataSetColumn.Equals("dataSetID") && col1.SourceColumn.Equals("sourceID"));
			Assert.AreEqual (true, eq, "test2");
						
			col1=columnMapCollection.GetByDataSetColumn("datasetname");
			eq=(col1.DataSetColumn.Equals("dataSetName") && col1.SourceColumn.Equals("sourceName"));
			Assert.AreEqual (true, eq, "test3");
			col1=columnMapCollection.GetByDataSetColumn("datasetid");
			eq=(col1.DataSetColumn.Equals("dataSetID") && col1.SourceColumn.Equals("sourceID"));
			Assert.AreEqual (true, eq, "test4");
		}
		
		[Test]
		public void GetColumnMappingBySchemaAction()
		{
			columnMapCollection.AddRange(cols);
			bool eq;
			DataColumnMapping col1;
			col1=DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection, "sourceName", MissingMappingAction.Passthrough);
			eq=(col1.DataSetColumn.Equals("dataSetName") && col1.SourceColumn.Equals("sourceName"));
			Assert.AreEqual (true, eq, "test1");
			col1=DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection, "sourceID", MissingMappingAction.Passthrough);
			eq=(col1.DataSetColumn.Equals("dataSetID") && col1.SourceColumn.Equals("sourceID"));
			Assert.AreEqual (true, eq, "test2");
			
			col1=DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection, "sourceData", MissingMappingAction.Passthrough);
			eq=(col1.DataSetColumn.Equals("sourceData") && col1.SourceColumn.Equals("sourceData"));
			Assert.AreEqual (true, eq, "test3");
			eq=columnMapCollection.Contains(col1);
			Assert.AreEqual (false, eq, "test4");
			col1=DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection, "sourceData", MissingMappingAction.Ignore);
			Assert.AreEqual (null, col1, "test5");
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetColumnMappingBySchemaActionException1()
		{
			DataColumnMappingCollection.GetColumnMappingBySchemaAction(columnMapCollection, "sourceName", MissingMappingAction.Error);
		}
		
		[Test]
		public void IndexOf()
		{
			columnMapCollection.AddRange(cols);
			int ind;
			ind=columnMapCollection.IndexOf(cols[0]);
			Assert.AreEqual (0, ind, "test1");
			ind=columnMapCollection.IndexOf(cols[1]);
			Assert.AreEqual (1, ind, "test2");
					
			ind=columnMapCollection.IndexOf(cols[0].SourceColumn);
			Assert.AreEqual (0, ind, "test3");
			ind=columnMapCollection.IndexOf(cols[1].SourceColumn);
			Assert.AreEqual (1, ind, "test4");
		}
		
		[Test]
		public void IndexOfDataSetColumn()
		{
			columnMapCollection.AddRange(cols);
			int ind;
			ind=columnMapCollection.IndexOfDataSetColumn(cols[0].DataSetColumn);
			Assert.AreEqual (0, ind, "test1");
			ind=columnMapCollection.IndexOfDataSetColumn(cols[1].DataSetColumn);
			Assert.AreEqual (1, ind, "test2");
			
			ind=columnMapCollection.IndexOfDataSetColumn("datasetname");
			Assert.AreEqual (0, ind, "test3");
			ind=columnMapCollection.IndexOfDataSetColumn("datasetid");
			Assert.AreEqual (1, ind, "test4");
						
			ind=columnMapCollection.IndexOfDataSetColumn("sourcedeter");
			Assert.AreEqual (-1, ind, "test5");
		}
		
		[Test]
		public void Insert()
		{
			columnMapCollection.AddRange(cols);
			DataColumnMapping mymap=new DataColumnMapping("sourceAge", "dataSetAge");
			columnMapCollection.Insert(3, mymap);
			int ind=columnMapCollection.IndexOfDataSetColumn("dataSetAge");
			Assert.AreEqual (3, ind, "test1");
		}
		
		[Test]
		[Ignore ("This test is wrong. A mapping in a DataColumnMappingCollection must be identical.")]
		public void Remove()
		{
			columnMapCollection.AddRange(cols);
			DataColumnMapping mymap=new DataColumnMapping("sourceName", "dataSetName");
			columnMapCollection.Add(mymap);
			columnMapCollection.Remove((Object)mymap);
			bool eq=columnMapCollection.Contains((Object)mymap);
			Assert.AreEqual (false, eq, "test1");
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
			DataColumnMapping mymap=new DataColumnMapping("sourceAge", "dataSetAge");
			columnMapCollection.Remove(mymap);
		}
		
		[Test]
		public void RemoveAt()
		{
			columnMapCollection.AddRange(cols);
			bool eq;
			columnMapCollection.RemoveAt(0);
			eq=columnMapCollection.Contains(cols[0]);
			Assert.AreEqual (false, eq, "test1");
			eq=columnMapCollection.Contains(cols[1]);
			Assert.AreEqual (true, eq, "test2");
						
			columnMapCollection.RemoveAt("sourceID");
			eq=columnMapCollection.Contains(cols[1]);
			Assert.AreEqual (false, eq, "test3");
			eq=columnMapCollection.Contains(cols[2]);
			Assert.AreEqual (true, eq, "test4");
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
			Assert.AreEqual ("System.Data.Common.DataColumnMappingCollection", columnMapCollection.ToString(), "test1");
		}
	}
}
