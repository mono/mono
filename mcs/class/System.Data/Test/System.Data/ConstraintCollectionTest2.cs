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
using MonoTests.System.Data.Test.Utils;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture] public class ConstraintCollectionTest2
	{
		private bool CollectionChangedFlag = false;

		[Test] public void CanRemove_ParentForeign()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			Assert.AreEqual(false, ds.Tables["parent"].Constraints.CanRemove(ds.Tables["parent"].Constraints[0]), "CN1");
		}

		[Test] public void CanRemove_ChildForeign()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			Assert.AreEqual(true, ds.Tables["child"].Constraints.CanRemove(ds.Tables["child"].Constraints[0]), "CN2");
		}

		[Test] public void CanRemove_ParentAndChildForeign()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			//remove the forigen and ask about the unique
			ds.Tables["child"].Constraints.Remove(ds.Tables["child"].Constraints[0]);
			Assert.AreEqual(true, ds.Tables["parent"].Constraints.CanRemove(ds.Tables["parent"].Constraints[0]), "CN3");
		}

		// FIXME. This test isn't complete.
		public void CanRemove_Unique()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			//remove the forigen and ask about the unique
			dt.Constraints.Remove(dt.Constraints[0]);
			Assert.AreEqual(true, dt.Constraints.CanRemove(dt.Constraints[0]), "CN4");
		}

		[Test] public void Clear_Foreign()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			foreach(DataTable dt in ds.Tables)
			{
				dt.Constraints.Clear();
			}
			Assert.AreEqual(0, ds.Tables[0].Constraints.Count, "CN5");
			Assert.AreEqual(0, ds.Tables[0].Constraints.Count, "CN6");

		}

		[Test] public void Clear_Unique()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			int rowsCount = dt.Rows.Count;
			dt.Constraints.Clear();
			DataRow dr = dt.NewRow();
			dr[0] = 1;
			dt.Rows.Add(dr);
			Assert.AreEqual(rowsCount+1, dt.Rows.Count, "CN7"); //Just checking that no expection ocuured
		}

		[Test] public void CollectionChanged()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			CollectionChangedFlag = false;
			dt.Constraints.CollectionChanged += new CollectionChangeEventHandler(Constraints_CollectionChangedHandler);	
			dt = DataProvider.CreateUniqueConstraint(dt);
			Assert.AreEqual(true, CollectionChangedFlag, "CN8"); 
		}

		[Test] public void Contains_ByName()
		{
			DataSet ds =  DataProvider.CreateForigenConstraint();
	 
			//changing the constraints's name

			ds.Tables["child"].Constraints[0].ConstraintName = "name1";
			ds.Tables["parent"].Constraints[0].ConstraintName = "name2";


			Assert.AreEqual(true, ds.Tables["child"].Constraints.Contains("name1"), "CN9");
			Assert.AreEqual(false, ds.Tables["child"].Constraints.Contains("xxx"), "CN10");
			Assert.AreEqual(true, ds.Tables["parent"].Constraints.Contains("name2"), "CN11");
			Assert.AreEqual(false, ds.Tables["parent"].Constraints.Contains("xxx"), "CN12");

		}

		[Test] public void CopyTo()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints.Add("constraint2",dt.Columns["String1"],true);

			object[] ar = new object[2];

			dt.Constraints.CopyTo(ar,0);
			Assert.AreEqual(2, ar.Length, "CN13");
		}

		[Test] public void Count()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			Assert.AreEqual(1, dt.Constraints.Count, "CN14");

			//Add

			dt.Constraints.Add("constraint2",dt.Columns["String1"],false);
			Assert.AreEqual(2, dt.Constraints.Count, "CN15");

			//Remove

			dt.Constraints.Remove("constraint2");
			Assert.AreEqual(1, dt.Constraints.Count, "CN16");
		}

		[Test] public void GetEnumerator()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints.Add("constraint2",dt.Columns["String1"],false);

			int counter=0;
			IEnumerator myEnumerator = dt.Constraints.GetEnumerator();
			while (myEnumerator.MoveNext())
			{
				counter++;

			}
			Assert.AreEqual(2, counter, "CN17");
		}

		[Test] public void IndexOf()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			Assert.AreEqual(0, dt.Constraints.IndexOf(dt.Constraints[0]), "CN18");

			//Add new constraint
			Constraint con = new UniqueConstraint(dt.Columns["String1"],false);

			dt.Constraints.Add(con);
			Assert.AreEqual(1, dt.Constraints.IndexOf(con), "CN19");

			//Remove it and try to look for it 

			dt.Constraints.Remove(con);
			Assert.AreEqual(-1, dt.Constraints.IndexOf(con), "CN20");

		}

		[Test] public void IndexOf_ByName()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints[0].ConstraintName="name1";
			Assert.AreEqual(0, dt.Constraints.IndexOf("name1"), "CN21");

			//Add new constraint
			Constraint con = new UniqueConstraint(dt.Columns["String1"],false);
			con.ConstraintName="name2";

			dt.Constraints.Add(con);
			Assert.AreEqual(1, dt.Constraints.IndexOf("name2"), "CN22");

			//Remove it and try to look for it 

			dt.Constraints.Remove(con);
			Assert.AreEqual(-1, dt.Constraints.IndexOf("name2"), "CN23");

		}

		[Test] public void IsReadOnly()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			Assert.AreEqual(false, dt.Constraints.IsReadOnly, "CN24"); 
		}

		[Test] public void IsSynchronized()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			Assert.AreEqual(false, dt.Constraints.IsSynchronized, "CN25");
	 
			ConstraintCollection col = (ConstraintCollection)dt.Constraints.SyncRoot;

	//		lock(dt.Constraints.SyncRoot)
	//		{
			//	Assert.AreEqual(true, col.IsSynchronized, "CN26");

			//}
		}

		[Test] public void Remove()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints.Remove(dt.Constraints[0]);
			Assert.AreEqual(0, dt.Constraints.Count, "CN27");
		}

		[Test] public void Remove_ByNameSimple()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints[0].ConstraintName = "constraint1";
			dt.Constraints.Remove("constraint1");
			Assert.AreEqual(0, dt.Constraints.Count, "CN28");
		}

		[Test] public void Remove_ByNameWithAdd()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints[0].ConstraintName = "constraint1";
			Constraint con = new UniqueConstraint(dt.Columns["String1"],false);
			dt.Constraints.Add(con);
			dt.Constraints.Remove(con);

			Assert.AreEqual(1, dt.Constraints.Count, "CN29");
			Assert.AreEqual("constraint1", dt.Constraints[0].ConstraintName, "CN30");
		}

		[Test] public void Remove_CollectionChangedEvent()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			CollectionChangedFlag = false;
			dt.Constraints.CollectionChanged += new CollectionChangeEventHandler(Constraints_CollectionChangedHandler);
			dt.Constraints.Remove(dt.Constraints[0]);
			Assert.AreEqual(true, CollectionChangedFlag, "CN31"); //Checking that event has raised
		}

		[Test] public void Remove_ByNameCollectionChangedEvent()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			CollectionChangedFlag = false;
			dt.Constraints.CollectionChanged += new CollectionChangeEventHandler(Constraints_CollectionChangedHandler);
			dt.Constraints.Remove("constraint1");
			Assert.AreEqual(true, CollectionChangedFlag, "CN32"); //Checking that event has raised

		}

		[Test] public void add_CollectionChanged()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			CollectionChangedFlag = false;
			dt.Constraints.CollectionChanged += new CollectionChangeEventHandler(Constraints_CollectionChangedHandler);	
			dt = DataProvider.CreateUniqueConstraint(dt);
			Assert.AreEqual(true, CollectionChangedFlag, "CN33"); 
		}

		private void Constraints_CollectionChangedHandler(object sender, CollectionChangeEventArgs e)
		{
			CollectionChangedFlag = true;
		}
	}
}
