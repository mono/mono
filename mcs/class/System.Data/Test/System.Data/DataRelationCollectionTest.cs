// DataRelationCollection.cs - Nunit Test Cases for for testing the DataRelationCollection
// class
// Author:
//
// 	Punit Kumar Todi ( punit_todi@da-iict.org )
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
	public class DataRelationCollectionTest : Assertion 
	{
		DataSet _dataset;
		DataTable _tblparent,_tblchild;
		DataRelation _relation;
		[SetUp]
		public void GetReady() 
		{
			_dataset = new DataSet();
			_tblparent = new DataTable("Customer");
			_tblchild = new DataTable("Order");
			_dataset.Tables.Add(_tblchild);
			_dataset.Tables.Add(_tblparent);
			_dataset.Tables.Add("Item");
			_dataset.Tables["Customer"].Columns.Add("custid");
			_dataset.Tables["Customer"].Columns.Add("custname");
			_dataset.Tables["Order"].Columns.Add("oid");
			_dataset.Tables["Order"].Columns.Add("custid");
			_dataset.Tables["Order"].Columns.Add("itemid");
			_dataset.Tables["Order"].Columns.Add("desc");
			_dataset.Tables["Item"].Columns.Add("itemid");
			_dataset.Tables["Item"].Columns.Add("desc");
			
		}
	
		[TearDown]
		public void Clean() 
		{
			_dataset.Relations.Clear();
		}
	
		[Test]
		public void Add()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables["Customer"].Columns["custid"];
			DataColumn childCol = _dataset.Tables["Order"].Columns["custid"];
			DataRelation dr = new DataRelation("CustOrder",parentCol,childCol);
			
			drcol.Add(dr);
			AssertEquals("test#1","CustOrder",drcol[0].RelationName);
			drcol.Clear();
			
			drcol.Add(parentCol,childCol);
			AssertEquals("test#2",1,drcol.Count);
			drcol.Clear();
			
			drcol.Add("NewRelation",parentCol,childCol);
			AssertEquals("test#3","NewRelation",drcol[0].RelationName);
			drcol.Clear();
			
			drcol.Add("NewRelation",parentCol,childCol,false);
			AssertEquals("test#4",1,drcol.Count);
			drcol.Clear();
			
			drcol.Add("NewRelation",parentCol,childCol,true);
			AssertEquals("test#5",1,drcol.Count);
			drcol.Clear();
		}
		
		[Test]		
		[ExpectedException(typeof(ArgumentNullException))]
		[Ignore ("It does not pass under MS.NET")]
		public void AddException1()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation drnull = null;
			drcol.Add(drnull);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddException2()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1= new DataRelation("CustOrder"
							,_dataset.Tables["Customer"].Columns["custid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			drcol.Add(dr1);			
			drcol.Add(dr1);
		}
		
		[Test]
		[ExpectedException(typeof(DuplicateNameException))]
		public void AddException3()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1= new DataRelation("DuplicateName"
							,_dataset.Tables["Customer"].Columns["custid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			DataRelation dr2 = new DataRelation("DuplicateName"
							,_dataset.Tables["Item"].Columns["itemid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			
			drcol.Add(dr1);			
			drcol.Add(dr2);
		}
		
		
		[Test]
		public void AddRange()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1= new DataRelation("CustOrder"
							,_dataset.Tables["Customer"].Columns["custid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			DataRelation dr2 = new DataRelation("ItemOrder"
							,_dataset.Tables["Item"].Columns["itemid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			drcol.AddRange(new DataRelation[] {dr1,dr2});
			
			AssertEquals("test#1","CustOrder",drcol[0].RelationName);
			AssertEquals("test#2","ItemOrder",drcol[1].RelationName);
		}
		
		[Test]
		public void CanRemove()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables["Customer"].Columns["custid"];
			DataColumn childCol = _dataset.Tables["Order"].Columns["custid"];
			DataRelation dr = new DataRelation("CustOrder",parentCol,childCol);
			
			drcol.Add(dr);
			AssertEquals("test#1",true,drcol.CanRemove(dr));
			dr = null;
			AssertEquals("test#2",false,drcol.CanRemove(dr));
			DataRelation dr2 = new DataRelation("ItemOrder"
						,_dataset.Tables["Item"].Columns["itemid"]
						,_dataset.Tables["Order"].Columns["custid"]);
			AssertEquals("test#3",false,drcol.CanRemove(dr2));
		}
		
		[Test]
		public void Clear()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables["Customer"].Columns["custid"];
			DataColumn childCol = _dataset.Tables["Order"].Columns["custid"];
			drcol.Add(new DataRelation("CustOrder",parentCol,childCol));
			drcol.Add("ItemOrder",_dataset.Tables["Item"].Columns["itemid"]
								 ,_dataset.Tables["Order"].Columns["itemid"]);
			drcol.Clear();
			AssertEquals("test#1",0,drcol.Count);
		}
		
		[Test]
		public void Contains()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables["Customer"].Columns["custid"];
			DataColumn childCol = _dataset.Tables["Order"].Columns["custid"];
			DataRelation dr = new DataRelation("CustOrder",parentCol,childCol);
			
			drcol.Add(dr);
			AssertEquals("test#1",true,drcol.Contains(dr.RelationName));
			string drnull = "";
			AssertEquals("test#2",false,drcol.Contains(drnull));
			dr = new DataRelation("newRelation",childCol,parentCol);
			AssertEquals("test#3",false,drcol.Contains("NoSuchRelation"));
		}
		
		[Test]
		public void CopyTo()
		{
			DataRelationCollection drcol = _dataset.Relations;
			drcol.Add("CustOrder"
					,_dataset.Tables["Customer"].Columns["custid"]
					,_dataset.Tables["Order"].Columns["custid"]);
			drcol.Add("ItemOrder"
					,_dataset.Tables["Item"].Columns["itemid"]
					,_dataset.Tables["Order"].Columns["custid"]);
			
			DataRelation [] array = new DataRelation[2];
			drcol.CopyTo(array,0);
			AssertEquals("test#1",2,array.Length);
			AssertEquals("test#2", "CustOrder", array[0].RelationName);
			AssertEquals("test#3", "ItemOrder", array[1].RelationName);
			
			DataRelation [] array1 = new DataRelation[4];
			drcol.CopyTo(array1,2);
			AssertEquals("test#4", null, array1[0]);
			AssertEquals("test#5", null, array1[1]);
			AssertEquals("test#6", "CustOrder", array1[2].RelationName);
			AssertEquals("test#7", "ItemOrder", array1[3].RelationName);
		}
		
		[Test]
		public void Equals()
		{
			DataRelationCollection drcol = _dataset.Relations;
			drcol.Add("CustOrder"
					,_dataset.Tables["Customer"].Columns["custid"]
					,_dataset.Tables["Order"].Columns["custid"]);
			drcol.Add("ItemOrder"
					,_dataset.Tables["Item"].Columns["itemid"]
					,_dataset.Tables["Order"].Columns["custid"]);
			DataSet newds = new DataSet();
			DataRelationCollection drcol1 = newds.Relations;
			DataRelationCollection drcol2 = _dataset.Relations;

			AssertEquals("test#1", true, drcol.Equals(drcol));
			AssertEquals("test#2", true, drcol.Equals(drcol2));
			
			AssertEquals("test#3", false, drcol1.Equals(drcol));
			AssertEquals("test#4", false, drcol.Equals(drcol1));
			
			AssertEquals("test#5", true, Object.Equals(drcol,drcol2));
			AssertEquals("test#6", false, Object.Equals(drcol,drcol1));
			
		}
		[Test]
		public void IndexOf()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1= new DataRelation("CustOrder"
							,_dataset.Tables["Customer"].Columns["custid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			DataRelation dr2 = new DataRelation("ItemOrder"
							,_dataset.Tables["Item"].Columns["itemid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			drcol.Add(dr1);
			drcol.Add(dr2);
			
			AssertEquals("test#1",0,drcol.IndexOf(dr1));
			AssertEquals("test#2",1,drcol.IndexOf(dr2));
			
			AssertEquals("test#3",0,drcol.IndexOf("CustOrder"));
			AssertEquals("test#4",1,drcol.IndexOf("ItemOrder"));
			
			AssertEquals("test#5",0,drcol.IndexOf(drcol[0]));
			AssertEquals("test#6",1,drcol.IndexOf(drcol[1]));
			
			AssertEquals("test#7",-1,drcol.IndexOf("_noRelation_"));
			DataRelation newdr = new DataRelation("newdr"
										,_dataset.Tables["Customer"].Columns["custid"]
										,_dataset.Tables["Order"].Columns["custid"]);
			
			AssertEquals("test#8",-1,drcol.IndexOf(newdr));
		}

		[Test]
		public void Remove()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1= new DataRelation("CustOrder"
							,_dataset.Tables["Customer"].Columns["custid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			DataRelation dr2 = new DataRelation("ItemOrder"
							,_dataset.Tables["Item"].Columns["itemid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			drcol.Add(dr1);
			drcol.Add(dr2);
			
			drcol.Remove(dr1);
			AssertEquals("test#1", false, drcol.Contains(dr1.RelationName));
			drcol.Add(dr1);
			
			drcol.Remove("CustOrder");
			AssertEquals("test#2", false, drcol.Contains("CustOrder"));
			drcol.Add(dr1);
			
			DataRelation drnull = null;
			drcol.Remove(drnull);
			
			DataRelation newdr = new DataRelation("newdr"
										,_dataset.Tables["Customer"].Columns["custid"]
										,_dataset.Tables["Order"].Columns["custid"]);
			try
			{
				drcol.Remove(newdr);
				Fail("Err: removed relation which not part of collection");
			}
			catch (Exception e)
			{
				AssertEquals ("test#4", typeof(ArgumentException), e.GetType());
			}
			try
			{
				drcol.Remove("newdr");
				Fail("Err: removed relation which not part of collection");
			}
			catch (ArgumentException e)
			{
			}

			
		}
		
		[Test]
		public void RemoveAt()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1= new DataRelation("CustOrder"
							,_dataset.Tables["Customer"].Columns["custid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			DataRelation dr2 = new DataRelation("ItemOrder"
							,_dataset.Tables["Item"].Columns["itemid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			drcol.Add(dr1);
			drcol.Add(dr2);

			try
			{
				drcol.RemoveAt(-1);
				Fail("the index was out of bound: must have failed");
			}
			catch(IndexOutOfRangeException e)
			{
			}
			try
			{
				drcol.RemoveAt(101);
				Fail("the index was out of bound: must have failed");
			}
			catch(IndexOutOfRangeException e)
			{
			}
			
			drcol.RemoveAt (1);
			AssertEquals ("test#5", false, drcol.Contains(dr2.RelationName));
			drcol.RemoveAt (0);
			AssertEquals ("test#6", false, drcol.Contains(dr1.RelationName));
		}
		
		//[Test]
		public void ToStringTest()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1= new DataRelation("CustOrder"
							,_dataset.Tables["Customer"].Columns["custid"]
							,_dataset.Tables["Order"].Columns["custid"]);
			drcol.Add(dr1);	
			AssertEquals("test#1","System.Data.DataRelationCollection",drcol.ToString());
			Console.WriteLine(drcol.ToString());
		}
	}
}
