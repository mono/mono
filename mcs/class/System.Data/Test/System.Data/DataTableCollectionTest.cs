// DataTableCollectionTest.cs - NUnit Test Cases for for testing the DataTableCollection
// class
// Author:
// 	Punit Kumar Todi ( punit_todi@da-iict.org )
//
// (C) Punit Todi

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
	public class DataTableCollectionTest {
		// common variables here
		private DataSet [] _dataset;
		private DataTable [] _tables;
		
		[SetUp]
		public void GetReady() 
		{
			// setting up dataset && tables
			_dataset = new DataSet[2];	
			_tables = new DataTable[2];
			_dataset[0] = new DataSet();
			_dataset[1] = new DataSet();
			_tables[0] = new DataTable("Books");
			_tables[0].Columns.Add("id",typeof(int));
			_tables[0].Columns.Add("name",typeof(String));
			_tables[0].Columns.Add("author",typeof(String));
			
			_tables[1] = new DataTable("Category");
			_tables[1].Columns.Add("id",typeof(int));
			_tables[1].Columns.Add("desc",typeof(String));
		}
		// clean up code here
		[TearDown]
		public void Clean() 
		{
			_dataset[0].Tables.Clear();
			_dataset[1].Tables.Clear();
		}
		[Test]
		public void Add()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(_tables[0]);
			int i,j;
			i=0;
			
			foreach( DataTable table in tbcol )
   			{
				Assert.AreEqual(_tables[i].TableName,table.TableName,"test#1");
				j=0;
       			foreach( DataColumn column in table.Columns )
       			{
					Assert.AreEqual(_tables[i].Columns[j].ColumnName,column.ColumnName,"test#2");
					j++;
       			}
				i++;
   			}
			
			tbcol.Add(_tables[1]);
			i=0;
			foreach( DataTable table in tbcol )
   			{
				Assert.AreEqual(_tables[i].TableName,table.TableName,"test#3");
				j=0;
       			foreach( DataColumn column in table.Columns )
       			{
					Assert.AreEqual(_tables[i].Columns[j].ColumnName,column.ColumnName,"test#4");
					j++;
       			}
				i++;
   			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddException1()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			DataTable tb = null;
			tbcol.Add(tb);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddException2()
		{
			/* table already exist in the collection */
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(_tables[0]);
			tbcol.Add(_tables[0]);
		}
		
		[Test]
		[ExpectedException(typeof(DuplicateNameException))]
		public void AddException3()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(new DataTable("SameTableName"));
			tbcol.Add(new DataTable("SameTableName"));
		}
		
		[Test]
		[ExpectedException(typeof(DuplicateNameException))]
		public void AddException4()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add("SameTableName");
			tbcol.Add("SameTableName");
		}

		[Test]
		public void Count() 
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(_tables[0]);
			Assert.AreEqual(1,tbcol.Count, "test#1");
			tbcol.Add(_tables[1]);
			Assert.AreEqual(2,tbcol.Count, "test#2");
		}
		
		[Test]
		public void AddRange()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Clear();
			/* _tables is array of type DataTable defined in Setup */
			tbcol.AddRange(_tables);
			int i,j;
			i=0;
			foreach( DataTable table in tbcol )
   			{
				Assert.AreEqual(_tables[i].TableName,table.TableName,"test#1");
				j=0;
       			foreach( DataColumn column in table.Columns )
       			{
					Assert.AreEqual(_tables[i].Columns[j].ColumnName,column.ColumnName,"test#2");
					j++;
       			}
				i++;
   			}
		}
		
		[Test]
		public void CanRemove()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Clear();
			/* _tables is array of DataTables defined in Setup */
			tbcol.AddRange(_tables);
			DataTable tbl = null;
			/* checking for a recently input table, expecting true */
			Assert.AreEqual(true,tbcol.CanRemove(_tables[0]),"test#1");
			/* trying to check with a null reference, expecting false */
			Assert.AreEqual(false,tbcol.CanRemove(tbl),"test#2");
			/* trying to check with a table that does not exist in collection, expecting false */
			Assert.AreEqual(false,tbcol.CanRemove(new DataTable("newTable")),"test#3");
		}
		
		[Test]
		public void Remove()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Clear();
			/* _tables is array of DataTables defined in Setup */
			tbcol.AddRange(_tables);
			
			/* removing a recently added table */
			int count = tbcol.Count;
			tbcol.Remove(_tables[0]);
			Assert.AreEqual(count-1,tbcol.Count,"test#1");
			DataTable tbl = null;
			/* removing a null reference. must generate an Exception */
			try
			{
				tbcol.Remove(tbl);
				Assert.Fail("Err:: tbcol.Rmove(null) must fail");
			}
			catch(Exception e)
			{
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType(), "test#2");
			}
			/* removing a table that is not there in collection */
			try
			{
				tbcol.Remove(new DataTable("newTable"));
				Assert.Fail("Err:: cannot remove a table that is not there in collection");
			}
			catch(Exception e)
			{
				Assert.AreEqual (typeof (ArgumentException), e.GetType(), "test#3");
			}
			
		}
		[Test]
		public void Clear()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(_tables[0]);
			tbcol.Clear();
			Assert.AreEqual(0,tbcol.Count,"Test#1");
			
			tbcol.AddRange(new DataTable[] {_tables[0],_tables[1]});
			tbcol.Clear();
			Assert.AreEqual(0,tbcol.Count,"Test#2");
		}
		[Test]
		public void Contains()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Clear();
			/* _tables is array of DataTables defined in Setup */
			tbcol.AddRange(_tables);
			string tblname = "";
			/* checking for a recently input table, expecting true */
			Assert.AreEqual(true,tbcol.Contains(_tables[0].TableName),"test#1");
			/* trying to check with a empty string, expecting false */
			Assert.AreEqual(false,tbcol.Contains(tblname),"test#2");
			/* trying to check for a table that donot exist, expecting false */
			Assert.AreEqual(false,tbcol.Contains("InvalidTableName"),"test#3");
		}
		
		[Test]
		public void CopyTo()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add("Table1");			
			tbcol.Add("Table2");			
			tbcol.Add("Table3");			
			tbcol.Add("Table4");

			DataTable [] array = new DataTable[4];
			/* copying to the beginning of the array */
			tbcol.CopyTo(array,0);
			Assert.AreEqual (4, array.Length, "test#01");
			Assert.AreEqual ("Table1", array[0].TableName, "test#02");
			Assert.AreEqual ("Table2", array[1].TableName, "test#03");
			Assert.AreEqual ("Table3", array[2].TableName, "test#04");
			Assert.AreEqual ("Table4", array[3].TableName, "test#05");

			/* copying with in a array */
			DataTable [] array1 = new DataTable[6];
			tbcol.CopyTo(array1,2);
			Assert.AreEqual(null,array1[0],"test#06");
			Assert.AreEqual(null,array1[1],"test#07");
			Assert.AreEqual("Table1",array1[2].TableName,"test#08");
			Assert.AreEqual("Table2",array1[3].TableName,"test#09");
			Assert.AreEqual("Table3",array1[4].TableName,"test#10");
			Assert.AreEqual("Table4",array1[5].TableName,"test#11");			
		}
		[Test]
		public void Equals()
		{
			DataTableCollection tbcol1 = _dataset[0].Tables;
			DataTableCollection tbcol2 = _dataset[1].Tables;
			DataTableCollection tbcol3;
			tbcol1.Add(_tables[0]);
			tbcol2.Add(_tables[1]);
			tbcol3 = tbcol1;
			
			Assert.AreEqual(true,tbcol1.Equals(tbcol1),"test#1");
			Assert.AreEqual(true,tbcol1.Equals(tbcol3),"test#2");
			Assert.AreEqual(true,tbcol3.Equals(tbcol1),"test#3");
			
			Assert.AreEqual(false,tbcol1.Equals(tbcol2),"test#4");
			Assert.AreEqual(false,tbcol2.Equals(tbcol1),"test#5");
			
			Assert.AreEqual(true,Object.Equals(tbcol1,tbcol3),"test#6");
			Assert.AreEqual(true,Object.Equals(tbcol1,tbcol1),"test#7");
			Assert.AreEqual(false,Object.Equals(tbcol1,tbcol2),"test#8");
		}
		[Test]
		public void IndexOf()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(_tables[0]);
			tbcol.Add("table1");
			tbcol.Add("table2");
			
			Assert.AreEqual(0,tbcol.IndexOf(_tables[0]),"test#1");
			Assert.AreEqual(-1,tbcol.IndexOf(_tables[1]),"test#2");
			Assert.AreEqual(1,tbcol.IndexOf("table1"),"test#3");
			Assert.AreEqual(2,tbcol.IndexOf("table2"),"test#4");
			
			Assert.AreEqual(0,tbcol.IndexOf(tbcol[0]),"test#5");
			Assert.AreEqual(1,tbcol.IndexOf(tbcol[1]),"test#6");
			Assert.AreEqual(-1,tbcol.IndexOf("_noTable_"),"test#7");
			DataTable tb = new DataTable("new_table");
			Assert.AreEqual(-1,tbcol.IndexOf(tb),"test#8");
			
		}
		[Test]
		public void RemoveAt()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(_tables[0]);
			tbcol.Add("table1");
			
			try
			{
				tbcol.RemoveAt(-1);
				Assert.Fail("the index was out of bound: must have failed");
			}
			catch(IndexOutOfRangeException e)
			{
			}
			try
			{
				tbcol.RemoveAt(101);
				Assert.Fail("the index was out of bound: must have failed");
			}
			catch(IndexOutOfRangeException e)
			{
			}
			tbcol.RemoveAt (1);
			Assert.AreEqual (1, tbcol.Count, "test#5");
			tbcol.RemoveAt (0);
			Assert.AreEqual (0, tbcol.Count, "test#6");
		}

		[Test]
		public void ToStringTest()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add("Table1");
			tbcol.Add("Table2");
			tbcol.Add("Table3");
			Assert.AreEqual("System.Data.DataTableCollection",tbcol.ToString(),"test#1");
		}

		[Test]
		public void TableDataSetNamespaces ()
		{
			DataTable dt = new DataTable ("dt1");
			Assert.AreEqual (String.Empty, dt.Namespace, "#1-1");
			Assert.IsNull (dt.DataSet, "#1-2");

			DataSet ds1 = new DataSet ("ds1");
			ds1.Tables.Add (dt);
			Assert.AreEqual (String.Empty, dt.Namespace, "#2-1");
			Assert.AreEqual (ds1, dt.DataSet, "#2-2");

			ds1.Namespace = "ns1";
			Assert.AreEqual ("ns1", dt.Namespace, "#3");

			// back to null again
			ds1.Tables.Remove (dt);
			Assert.AreEqual (String.Empty, dt.Namespace, "#4-1");
			Assert.IsNull (dt.DataSet, "#4-2");

			// This table is being added to _already namespaced_
			// dataset.
			dt = new DataTable ("dt2");

			ds1.Tables.Add (dt);
			Assert.AreEqual ("ns1", dt.Namespace, "#5-1");
			Assert.AreEqual (ds1, dt.DataSet, "#5-2");

			ds1.Tables.Remove (dt);
			Assert.AreEqual (String.Empty, dt.Namespace, "#6-1");
			Assert.IsNull (dt.DataSet, "#6-2");

			DataSet ds2 = new DataSet ("ds2");
			ds2.Namespace = "ns2";
			ds2.Tables.Add (dt);
			Assert.AreEqual ("ns2", dt.Namespace, "#7-1");
			Assert.AreEqual (ds2, dt.DataSet, "#7-2");
		}
	}
}
