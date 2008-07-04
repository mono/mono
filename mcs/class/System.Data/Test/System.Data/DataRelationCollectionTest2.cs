// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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
using System.ComponentModel;
using System.Collections;
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataRelationCollectionTest2
	{
		private int changesCounter = 0;

		private void Relations_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			changesCounter++;
		}

		private DataSet getDataSet()
		{
			DataSet ds = new DataSet();
			DataTable dt1 = DataProvider.CreateParentDataTable();
			DataTable dt2 = DataProvider.CreateChildDataTable();

			ds.Tables.Add(dt1);
			ds.Tables.Add(dt2);
			return ds;
		}

		[SetUp]
		public void SetUp()
		{
			changesCounter = 0;
		}

		[Test] public void AddRange()
		{
			DataSet ds = getDataSet();
			DataRelation[] relArray = new DataRelation[2];

			relArray[0] = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);
			relArray[1] = new DataRelation("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]);

			ds.Relations.AddRange(relArray);

			Assert.AreEqual(2, ds.Relations.Count, "DRC1");
			Assert.AreEqual("rel1", ds.Relations[0].RelationName, "DRC2");
			Assert.AreEqual("rel2", ds.Relations[1].RelationName, "DRC3");

			ds.Relations.AddRange(null);
		}

		[Test] public void Add_ByDataColumns()
		{
			DataSet ds = getDataSet();
			ds.Relations.Add(ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);

			Assert.AreEqual(1, ds.Relations.Count, "DRC4");

			Assert.AreEqual(1, ds.Tables[0].ChildRelations.Count, "DRC5"); //When adding a relation,it's also added on the tables
			Assert.AreEqual(1, ds.Tables[1].ParentRelations.Count, "DRC6");

			Assert.AreEqual(typeof(UniqueConstraint), ds.Tables[0].Constraints[0].GetType(), "DRC7");
			Assert.AreEqual(typeof(ForeignKeyConstraint), ds.Tables[1].Constraints[0].GetType(), "DRC8"); 
		}

		[Test] public void Add_ByNameDataColumns()
		{
			DataSet ds = getDataSet();
			ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);

			Assert.AreEqual(1, ds.Relations.Count, "DRC9");

			Assert.AreEqual(1, ds.Tables[0].ChildRelations.Count, "DRC10"); //When adding a relation,it's also added on the tables
			Assert.AreEqual(1, ds.Tables[1].ParentRelations.Count, "DRC11");

			Assert.AreEqual(typeof(UniqueConstraint), ds.Tables[0].Constraints[0].GetType(), "DRC12");
			Assert.AreEqual(typeof(ForeignKeyConstraint), ds.Tables[1].Constraints[0].GetType(), "DRC13"); 

			Assert.AreEqual("rel1", ds.Relations[0].RelationName, "DRC14");
			Assert.AreEqual("rel1", ds.Tables[0].ChildRelations[0].RelationName, "DRC15");
			Assert.AreEqual("rel1", ds.Tables[1].ParentRelations[0].RelationName, "DRC16");
		}

		[Test] public void Add_ByNameDataColumnsWithConstraint()
		{
			DataSet ds = getDataSet();
			ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"],true);

			Assert.AreEqual(1, ds.Relations.Count, "DRC17");

			Assert.AreEqual(1, ds.Tables[0].ChildRelations.Count, "DRC18"); //When adding a relation,it's also added on the tables
			Assert.AreEqual(1, ds.Tables[1].ParentRelations.Count, "DRC19");

			Assert.AreEqual(typeof(UniqueConstraint), ds.Tables[0].Constraints[0].GetType(), "DRC20");
			Assert.AreEqual(typeof(ForeignKeyConstraint), ds.Tables[1].Constraints[0].GetType(), "DRC21"); 

			Assert.AreEqual("rel1", ds.Relations[0].RelationName, "DRC22");
			Assert.AreEqual("rel1", ds.Tables[0].ChildRelations[0].RelationName, "DRC23");
			Assert.AreEqual("rel1", ds.Tables[1].ParentRelations[0].RelationName, "DRC24");
		}
		[Test] public void Add_ByNameDataColumnsWithOutConstraint()
		{
			DataSet ds = getDataSet();
			ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"],false);

			Assert.AreEqual(1, ds.Relations.Count, "DRC25");

			Assert.AreEqual(1, ds.Tables[0].ChildRelations.Count, "DRC26"); //When adding a relation,it's also added on the tables
			Assert.AreEqual(1, ds.Tables[1].ParentRelations.Count, "DRC27");

			Assert.AreEqual(0, ds.Tables[0].Constraints.Count, "DRC28");
			Assert.AreEqual(0, ds.Tables[1].Constraints.Count, "DRC29"); 

			Assert.AreEqual("rel1", ds.Relations[0].RelationName, "DRC30");
			Assert.AreEqual("rel1", ds.Tables[0].ChildRelations[0].RelationName, "DRC31");
			Assert.AreEqual("rel1", ds.Tables[1].ParentRelations[0].RelationName, "DRC32");
		}

		[Test] public void CanRemove()
		{
			DataSet ds = getDataSet();
			ds.Relations.Add(ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);
			Assert.AreEqual(true, ds.Relations.CanRemove(ds.Relations[0]), "DRC33");
			Assert.AreEqual(true, ds.Tables[0].ChildRelations.CanRemove(ds.Tables[0].ChildRelations[0]), "DRC34");
			Assert.AreEqual(true, ds.Tables[1].ParentRelations.CanRemove(ds.Tables[1].ParentRelations[0]), "DRC35");
			Assert.AreEqual(false, ds.Relations.CanRemove(null), "DRC36");
		}
		[Test] public void CanRemove_DataRelation()
		{
			DataSet ds = getDataSet();
			DataSet ds1 = getDataSet();

			DataRelation rel = new DataRelation("rel1",
				ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);

			Assert.AreEqual(false, ds1.Relations.CanRemove(rel), "DRC37");
		}

		[Test] public void Clear()
		{
			DataSet ds = getDataSet();

			ds.Relations.Add(ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);
			ds.Relations.CollectionChanged+=new CollectionChangeEventHandler(Relations_CollectionChanged);
			ds.Relations.Clear();
			Assert.AreEqual(0, ds.Relations.Count, "DRC38");
			Assert.AreEqual(1, changesCounter, "DRC39");
		}

		/// <summary>
		/// Clear was already checked at the clear sub test
		/// </summary>
		[Test] public void CollectionChanged()
		{
			DataSet ds = getDataSet();

			ds.Relations.CollectionChanged+=new CollectionChangeEventHandler(Relations_CollectionChanged);

			DataRelation rel = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"]
				,ds.Tables[1].Columns["ParentId"]);

			ds.Relations.Add(rel);

			ds.Relations.Remove(rel);

			Assert.AreEqual(2, changesCounter, "DRC40");
		}

		[Test] public void Contains()
		{
			DataSet ds = getDataSet();
			ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);

			Assert.AreEqual(true, ds.Relations.Contains("rel1"), "DRC41");
			Assert.AreEqual(false, ds.Relations.Contains("RelL"), "DRC42");
			Assert.AreEqual(false, ds.Relations.Contains("rel2"), "DRC43");
		}

		[Test] public void CopyTo()
		{
			DataSet ds = getDataSet();

			DataRelation[] dataRelArray = new DataRelation[2];

			ds.Relations.Add(new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]));

			ds.Relations.CopyTo(dataRelArray,1);

			Assert.AreEqual("rel1", dataRelArray[1].RelationName, "DRC44");

			ds.Relations.CopyTo(dataRelArray,0);

			Assert.AreEqual("rel1", dataRelArray[0].RelationName, "DRC45");
		}

		[Test] public void Count()
		{
			DataSet ds = getDataSet();
			Assert.AreEqual(0, ds.Relations.Count, "DRC46");
			ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);
			Assert.AreEqual(1, ds.Relations.Count, "DRC47");
			ds.Relations.Add("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]);
			Assert.AreEqual(2, ds.Relations.Count, "DRC48");
			ds.Relations.Remove("rel2");
			Assert.AreEqual(1, ds.Relations.Count, "DRC49");
			ds.Relations.Remove("rel1");
			Assert.AreEqual(0, ds.Relations.Count, "DRC50");
		}

		[Test] public void GetEnumerator()
		{
			DataSet ds = getDataSet();
			int counter=0;
			ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);
			ds.Relations.Add("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]);

			IEnumerator myEnumerator =  ds.Relations.GetEnumerator();

			while (myEnumerator.MoveNext())
			{
				counter++;
				Assert.AreEqual("rel",  ((DataRelation)myEnumerator.Current).RelationName.Substring(0,3), "DRC51"); 
			}
			Assert.AreEqual(2, counter, "DRC52");
		}

		[Test] public void IndexOf_ByDataRelation()
		{
			DataSet ds = getDataSet();
			DataSet ds1 = getDataSet();

			DataRelation rel1 = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]); 
			DataRelation rel2 = new DataRelation("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]); 
			DataRelation rel3 = new DataRelation("rel3",ds1.Tables[0].Columns["ParentId"],ds1.Tables[1].Columns["ParentId"]); 

			ds.Relations.Add(rel1);
			ds.Relations.Add(rel2);

			Assert.AreEqual(1, ds.Relations.IndexOf(rel2), "DRC53");
			Assert.AreEqual(0, ds.Relations.IndexOf(rel1), "DRC54");
			Assert.AreEqual(-1, ds.Relations.IndexOf((DataRelation)null), "DRC55");
			Assert.AreEqual(-1, ds.Relations.IndexOf(rel3), "DRC56");
		}

		[Test] public void IndexOf_ByDataRelationName()
		{
			DataSet ds = getDataSet();
			DataSet ds1 = getDataSet();

			DataRelation rel1 = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]); 
			DataRelation rel2 = new DataRelation("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]); 
			DataRelation rel3 = new DataRelation("rel3",ds1.Tables[0].Columns["ParentId"],ds1.Tables[1].Columns["ParentId"]); 

			ds.Relations.Add(rel1);
			ds.Relations.Add(rel2);

			Assert.AreEqual(1, ds.Relations.IndexOf("rel2"), "DRC57");
			Assert.AreEqual(0, ds.Relations.IndexOf("rel1"), "DRC58");
			Assert.AreEqual(-1, ds.Relations.IndexOf((string)null), "DRC59");
			Assert.AreEqual(-1, ds.Relations.IndexOf("rel3"), "DRC60");
		}

		[Test] public void Item()
		{
			DataSet ds = getDataSet();
			DataRelation rel1 = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]); 
			DataRelation rel2 = new DataRelation("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]); 

			ds.Relations.Add(rel1);
			ds.Relations.Add(rel2);

			Assert.AreEqual("rel1", ds.Relations["rel1"].RelationName, "DRC61");
			Assert.AreEqual("rel2", ds.Relations["rel2"].RelationName, "DRC62");
			Assert.AreEqual("rel1", ds.Relations[0].RelationName, "DRC63");
			Assert.AreEqual("rel2", ds.Relations[1].RelationName, "DRC64");
		}

		[Test]
		public void Add_DataColumn1()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			int originalRelationsCount = ds.Relations.Count;

			DataRelation rel = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"]
				,ds.Tables[1].Columns["ParentId"]);

			ds.Relations.Add(rel);

			Assert.AreEqual(originalRelationsCount+1,ds.Relations.Count,"DRC70");
			
			Assert.AreEqual(1,ds.Tables[0].ChildRelations.Count,"DRC71"); //When adding a relation,it's also added on the tables
			Assert.AreEqual(1,ds.Tables[1].ParentRelations.Count,"DRC72");

			Assert.AreEqual(typeof(UniqueConstraint),ds.Tables[0].Constraints[0].GetType(),"DRC73");
			Assert.AreEqual(typeof(ForeignKeyConstraint),ds.Tables[1].Constraints[0].GetType(),"DRC74"); 

			try
			{
				ds.Relations.Add(rel);
				Assert.Fail("DR75: Add failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DR76: Add. Wrong exception type. Got:" + exc);
			}
			
			ds.Relations.Add(null);			
		}

		[Test]
		public void Add_DataColumn2()
		{
			DataSet ds = GetDataSet();
			DataTable dt1 = ds.Tables[0];
			DataTable dt2 = ds.Tables[1];

			dt1.ChildRelations.Add(new DataRelation("rel1",dt1.Columns["ParentId"],dt2.Columns["ParentId"]));

			Assert.AreEqual(1,dt1.ChildRelations.Count,"DRC77"); 
			Assert.AreEqual(1,dt2.ParentRelations.Count,"DRC78");
			Assert.AreEqual(1,ds.Relations.Count,"DRC79"); //When adding relationship,it's also added on the dataset
		}

		[Test]
		public void Add_DataColumn3()
		{
			DataSet ds = GetDataSet();

			ds.Tables[1].ParentRelations.Add(new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]));

			Assert.AreEqual(1,ds.Tables[0].ChildRelations.Count,"DRC80"); 
			Assert.AreEqual(1,ds.Tables[1].ParentRelations.Count,"DRC81");
			Assert.AreEqual(1,ds.Relations.Count,"DRC82");
		}

		[Test]
		public void Add_DataColumn4()
		{
			DataSet ds = GetDataSet();
			DataTable dt = new DataTable();
			dt.Columns.Add("ParentId");
			try
			{
				ds.Relations.Add(new DataRelation("rel1",dt.Columns[0],ds.Tables[0].Columns[0]));
				Assert.Fail("DRC82: Add failed to throw InvalidConstraintException");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRC83: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void Add_DataColumn5()
		{
			DataSet ds = GetDataSet();
		}

		private DataSet GetDataSet()
		{
			DataSet ds = new DataSet();
			DataTable dt1 = DataProvider.CreateParentDataTable();
			DataTable dt2 = DataProvider.CreateChildDataTable();

			ds.Tables.Add(dt1);
			ds.Tables.Add(dt2);
			return ds;
		}

		[Test]
		public void Remove_DataColumn()
		{
			DataSet ds = getDataSet();
			DataSet ds1 = getDataSet();
			DataRelation rel1 = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]); 
			DataRelation rel2 = new DataRelation("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]); 

			ds.Relations.Add(rel1);
			ds.Relations.Add(rel2);

			Assert.AreEqual(2,ds.Relations.Count,"DRC84");

			ds.Relations.CollectionChanged+=new CollectionChangeEventHandler(Relations_CollectionChanged);
			//Perform remove

			ds.Relations.Remove(rel1);
			Assert.AreEqual(1,ds.Relations.Count,"DRC85");
			Assert.AreEqual("rel2",ds.Relations[0].RelationName,"DRC86");
			ds.Relations.Remove(rel2);
			Assert.AreEqual(0,ds.Relations.Count,"DRC87");
			Assert.AreEqual(2,changesCounter,"DRC88"); //How many times the event fired
			ds.Relations.Remove((DataRelation)null);

			try
			{
				DataRelation rel3 = new DataRelation("rel3",ds1.Tables[0].Columns["ParentId"],ds1.Tables[1].Columns["ParentId"]); 
				ds.Relations.Remove(rel3);
				Assert.Fail("DRC89: Remove failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRC90: Remove. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void Remove_String()
		{
			DataSet ds = getDataSet();
			DataSet ds1 = getDataSet();
			DataRelation rel1 = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]); 
			DataRelation rel2 = new DataRelation("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]); 

			ds.Relations.Add(rel1);
			ds.Relations.Add(rel2);

			Assert.AreEqual(ds.Relations.Count,2,"DRC91");

			ds.Relations.CollectionChanged+=new CollectionChangeEventHandler(Relations_CollectionChanged);
			//Perform remove

			ds.Relations.Remove("rel1");
			Assert.AreEqual(1,ds.Relations.Count,"DRC92");
			Assert.AreEqual("rel2",ds.Relations[0].RelationName,"DRC93");
			ds.Relations.Remove("rel2");
			Assert.AreEqual(0,ds.Relations.Count,"DRC94");
			Assert.AreEqual(2,changesCounter,"DRC95"); //How many times the event fired

			try
			{
				ds.Relations.Remove((string)null);
				Assert.Fail("DRC96: Remove failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRC97: Remove. Wrong exception type. Got:" + exc);
			}
			

			try
			{
				//DataRelation rel3 = new DataRelation("rel3",ds1.Tables[0].Columns["ParentId"],ds1.Tables[1].Columns["ParentId"]); 
				ds.Relations.Remove("rel3");
				Assert.Fail("DRC98: Remove failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRC99: Remove. Wrong exception type. Got:" + exc);
			}
		}

		private void RemoveAt()
		{
			DataSet ds = getDataSet();
			DataSet ds1 = getDataSet();
			DataRelation rel1 = new DataRelation("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]); 
			DataRelation rel2 = new DataRelation("rel2",ds.Tables[0].Columns["String1"],ds.Tables[1].Columns["String1"]); 

			ds.Relations.Add(rel1);
			ds.Relations.Add(rel2);

			Assert.AreEqual(2,ds.Relations.Count,"DRC100");

			ds.Relations.CollectionChanged+=new CollectionChangeEventHandler(Relations_CollectionChanged);
			//Perform remove

			ds.Relations.RemoveAt(0);
			Assert.AreEqual(1,ds.Relations.Count,"DRC101");
			Assert.AreEqual("rel2",ds.Relations[0].RelationName,"DRC102");
			ds.Relations.RemoveAt(0);
			Assert.AreEqual(0,ds.Relations.Count,"DRC103");
			Assert.AreEqual(2,changesCounter,"DRC104"); //How many times the event fired
			

			try
			{
				//DataRelation rel3 = new DataRelation("rel3",ds1.Tables[0].Columns["ParentId"],ds1.Tables[1].Columns["ParentId"]); 
				ds.Relations.RemoveAt(-1);
				Assert.Fail("DRC105: RemoveAt failed to throw IndexOutOfRangeException");
			}
			catch (IndexOutOfRangeException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRC106: RemoveAt. Wrong exception type. Got:" + exc);
			}
		}
	}
}
