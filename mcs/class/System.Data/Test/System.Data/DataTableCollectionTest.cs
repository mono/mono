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
	public class DataTableCollectionTest : Assertion {
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
				AssertEquals("test#1",_tables[i].TableName,table.TableName);
				j=0;
       			foreach( DataColumn column in table.Columns )
       			{
					AssertEquals("test#2",_tables[i].Columns[j].ColumnName,column.ColumnName);
					j++;
       			}
				i++;
   			}
			
			tbcol.Add(_tables[1]);
			i=0;
			foreach( DataTable table in tbcol )
   			{
				AssertEquals("test#3",_tables[i].TableName,table.TableName);
				j=0;
       			foreach( DataColumn column in table.Columns )
       			{
					AssertEquals("test#4",_tables[i].Columns[j].ColumnName,column.ColumnName);
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
			AssertEquals("test#1",1, tbcol.Count);
			tbcol.Add(_tables[1]);
			AssertEquals("test#2",2, tbcol.Count);
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
				AssertEquals("test#1",_tables[i].TableName,table.TableName);
				j=0;
       			foreach( DataColumn column in table.Columns )
       			{
					AssertEquals("test#2",_tables[i].Columns[j].ColumnName,column.ColumnName);
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
			AssertEquals("test#1",true,tbcol.CanRemove(_tables[0]));
			/* trying to check with a null reference, expecting false */
			AssertEquals("test#2",false,tbcol.CanRemove(tbl));
			/* trying to check with a table that does not exist in collection, expecting false */
			AssertEquals("test#3",false,tbcol.CanRemove(new DataTable("newTable")));
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
			AssertEquals("test#1",count-1,tbcol.Count);
			DataTable tbl = null;
			/* removing a null reference. must generate an Exception */
			try
			{
				tbcol.Remove(tbl);
				Fail("Err:: tbcol.Rmove(null) must fail");
			}
			catch(Exception e)
			{
				AssertEquals ("test#2", typeof (ArgumentNullException), e.GetType());
			}
			/* removing a table that is not there in collection */
			try
			{
				tbcol.Remove(new DataTable("newTable"));
				Fail("Err:: cannot remove a table that is not there in collection");
			}
			catch(Exception e)
			{
				AssertEquals ("test#3", typeof (ArgumentException), e.GetType());
			}
			
		}
		[Test]
		public void Clear()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(_tables[0]);
			tbcol.Clear();
			AssertEquals("Test#1",0,tbcol.Count);
			
			tbcol.AddRange(new DataTable[] {_tables[0],_tables[1]});
			tbcol.Clear();
			AssertEquals("Test#2",0,tbcol.Count);
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
			AssertEquals("test#1",true,tbcol.Contains(_tables[0].TableName));
			/* trying to check with a empty string, expecting false */
			AssertEquals("test#2",false,tbcol.Contains(tblname));
			/* trying to check for a table that donot exist, expecting false */
			AssertEquals("test#3",false,tbcol.Contains("InvalidTableName"));
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
			AssertEquals ("test#01", 4, array.Length);
			AssertEquals ("test#02", "Table1", array[0].TableName);
			AssertEquals ("test#03", "Table2", array[1].TableName);
			AssertEquals ("test#04", "Table3", array[2].TableName);
			AssertEquals ("test#05", "Table4", array[3].TableName);

			/* copying with in a array */
			DataTable [] array1 = new DataTable[6];
			tbcol.CopyTo(array1,2);
			AssertEquals("test#06",null,array1[0]);
			AssertEquals("test#07",null,array1[1]);
			AssertEquals("test#08","Table1",array1[2].TableName);
			AssertEquals("test#09","Table2",array1[3].TableName);
			AssertEquals("test#10","Table3",array1[4].TableName);
			AssertEquals("test#11","Table4",array1[5].TableName);			
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
			
			AssertEquals("test#1",true,tbcol1.Equals(tbcol1));
			AssertEquals("test#2",true,tbcol1.Equals(tbcol3));
			AssertEquals("test#3",true,tbcol3.Equals(tbcol1));
			
			AssertEquals("test#4",false,tbcol1.Equals(tbcol2));
			AssertEquals("test#5",false,tbcol2.Equals(tbcol1));
			
			AssertEquals("test#6",true,Object.Equals(tbcol1,tbcol3));
			AssertEquals("test#7",true,Object.Equals(tbcol1,tbcol1));
			AssertEquals("test#8",false,Object.Equals(tbcol1,tbcol2));
		}
		[Test]
		public void IndexOf()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add(_tables[0]);
			tbcol.Add("table1");
			tbcol.Add("table2");
			
			AssertEquals("test#1",0,tbcol.IndexOf(_tables[0]));
			AssertEquals("test#2",-1,tbcol.IndexOf(_tables[1]));
			AssertEquals("test#3",1,tbcol.IndexOf("table1"));
			AssertEquals("test#4",2,tbcol.IndexOf("table2"));
			
			AssertEquals("test#5",0,tbcol.IndexOf(tbcol[0]));
			AssertEquals("test#6",1,tbcol.IndexOf(tbcol[1]));
			AssertEquals("test#7",-1,tbcol.IndexOf("_noTable_"));
			DataTable tb = new DataTable("new_table");
			AssertEquals("test#8",-1,tbcol.IndexOf(tb));
			
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
				Fail("the index was out of bound: must have failed");
			}
			catch(IndexOutOfRangeException e)
			{
			}
			try
			{
				tbcol.RemoveAt(101);
				Fail("the index was out of bound: must have failed");
			}
			catch(IndexOutOfRangeException e)
			{
			}
			tbcol.RemoveAt (1);
			AssertEquals ("test#5", 1, tbcol.Count);
			tbcol.RemoveAt (0);
			AssertEquals ("test#6", 0, tbcol.Count);
		}

		[Test]
#if TARGET_JVM
		[Ignore ("Does not work with TARGET_JVM")]
#endif
		public void ToStringTest()
		{
			DataTableCollection tbcol = _dataset[0].Tables;
			tbcol.Add("Table1");
			tbcol.Add("Table2");
			tbcol.Add("Table3");
			AssertEquals("test#1","System.Data.DataTableCollection",tbcol.ToString());
		}

		[Test]
		public void TableDataSetNamespaces ()
		{
			DataTable dt = new DataTable ("dt1");
			AssertEquals ("#1-1", String.Empty, dt.Namespace);
			AssertNull ("#1-2", dt.DataSet);

			DataSet ds1 = new DataSet ("ds1");
			ds1.Tables.Add (dt);
			AssertEquals ("#2-1", String.Empty, dt.Namespace);
			AssertEquals ("#2-2", ds1, dt.DataSet);

			ds1.Namespace = "ns1";
			AssertEquals ("#3", "ns1", dt.Namespace);

			// back to null again
			ds1.Tables.Remove (dt);
			AssertEquals ("#4-1", String.Empty, dt.Namespace);
			AssertNull ("#4-2", dt.DataSet);

			// This table is being added to _already namespaced_
			// dataset.
			dt = new DataTable ("dt2");

			ds1.Tables.Add (dt);
			AssertEquals ("#5-1", "ns1", dt.Namespace);
			AssertEquals ("#5-2", ds1, dt.DataSet);

			ds1.Tables.Remove (dt);
			AssertEquals ("#6-1", String.Empty, dt.Namespace);
			AssertNull ("#6-2", dt.DataSet);

			DataSet ds2 = new DataSet ("ds2");
			ds2.Namespace = "ns2";
			ds2.Tables.Add (dt);
			AssertEquals ("#7-1", "ns2", dt.Namespace);
			AssertEquals ("#7-2", ds2, dt.DataSet);
		}
	}
}
