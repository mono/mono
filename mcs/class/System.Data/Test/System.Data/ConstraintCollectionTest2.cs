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
using MonoTests.System.Data.Utils;
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

		[Test] public void IndexOf_SameColumns ()
		{
			DataSet ds = new DataSet ();
			DataTable table1 = ds.Tables.Add ("table1");
			DataTable table2 = ds.Tables.Add ("table2");
			DataColumn pcol = table1.Columns.Add ("col1");
			DataColumn ccol = table2.Columns.Add ("col1");
	
			ds.Relations.Add ("fk_rel", pcol, ccol); 

			ForeignKeyConstraint fk = new ForeignKeyConstraint ("fk", pcol, ccol);
			Assert.AreEqual (-1, ds.Tables [1].Constraints.IndexOf (fk), "#1");
		}
		
		[Test]
		public void Add_RelationFirst_ConstraintNext()
		{
			DataSet ds = new DataSet ();
			DataTable table1 = ds.Tables.Add ("table1");
			DataTable table2 = ds.Tables.Add ("table2");
			DataColumn pcol = table1.Columns.Add ("col1");
			DataColumn ccol = table2.Columns.Add ("col1");
	
			ds.Relations.Add ("fk_rel", pcol, ccol); 

			try {
				table2.Constraints.Add ("fk_cons", pcol, ccol);
				Assert.Fail ("#1 Cannot add duplicate fk constraint");
			}catch (DataException e) {
			}

			try {
				table1.Constraints.Add ("pk_cons", pcol, false);
				Assert.Fail ("#2 Cannot add duplicate unique constraint");
			}catch (DataException e) {
			}
		}

		[Test]
		public void Add_ConstraintFirst_RelationNext ()
		{
			DataSet ds = new DataSet ();
			DataTable table1 = ds.Tables.Add ("table1");
			DataTable table2 = ds.Tables.Add ("table2");
			DataColumn pcol = table1.Columns.Add ("col1");
			DataColumn ccol = table2.Columns.Add ("col1");
	
			table2.Constraints.Add ("fk_cons", pcol, ccol);

			// Should not throw DataException 
			ds.Relations.Add ("fk_rel", pcol, ccol);

			Assert.AreEqual (1, table2.Constraints.Count, "#1 duplicate constraint shudnt be added");
			Assert.AreEqual (1, table1.Constraints.Count, "#2 duplicate constraint shudnt be added");
			Assert.AreEqual ("fk_cons", table2.Constraints [0].ConstraintName, "#3 shouldnt be overwritten");
			Assert.AreEqual ("Constraint1", table1.Constraints [0].ConstraintName, "#4 shouldnt be overwritten");
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

		[Test] public void Remove_CheckUnique ()
		{
			DataTable table = new DataTable ();
			DataColumn col1 = table.Columns.Add ("col1");
			DataColumn col2 = table.Columns.Add ("col2");

			Assert.IsFalse (col1.Unique, "#1");

			Constraint uc = table.Constraints.Add ("", col1, false);
			Assert.IsTrue (col1.Unique, "#2 col shud be set to unique");

			table.Constraints.Remove (uc);
			Assert.IsFalse (col1.Unique, "#3 col should no longer be unique");

			table.PrimaryKey = new DataColumn[] {col2};

			try {
				table.Constraints.Remove (table.Constraints [0]);
				Assert.Fail ("#4 Cannot Remove PrimaryKey");
			} catch (ArgumentException) {
			}
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

		[Test]
		public void Remove_Constraint ()
		{
			DataTable table1 = new DataTable ("table1");
			DataTable table2 = new DataTable ("table2");

			DataColumn col1 = table1.Columns.Add ("col1", typeof (int));
			DataColumn col2 = table1.Columns.Add ("col2", typeof (int));
			DataColumn col3 = table2.Columns.Add ("col1", typeof (int));

			Constraint c1 = table1.Constraints.Add ("unique1", col1, false);
			Constraint c2 = table1.Constraints.Add ("unique2", col2, false);
			Constraint c3 = table2.Constraints.Add ("fk", col1, col3);

			table1.Constraints.Remove (c1);
			table1.Constraints.Remove (c2);
			table2.Constraints.Remove (c3);

			Assert.AreEqual (0, table1.Constraints.Count, "#1");
			Assert.AreEqual (0, table2.Constraints.Count, "#2");

			DataSet ds = new DataSet ();
			ds.Tables.Add (table1);
			ds.Tables.Add (table2);

			c1 = table1.Constraints.Add ("unique1", col1, false);
			c2 = table1.Constraints.Add ("unique2", col2, false);
			c3 = table2.Constraints.Add ("fk", col1, col3);

			try {
				table1.Constraints.Remove (c1);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			Assert.AreEqual (2, table1.Constraints.Count, "#4");

			table1.Constraints.Remove (c2);
			Assert.AreEqual (1, table1.Constraints.Count, "#5");

			table2.Constraints.Remove (c3);
			Assert.AreEqual (1, table1.Constraints.Count, "#6");
			Assert.AreEqual (0, table2.Constraints.Count, "#7");

			table1.Constraints.Remove (c1);
			Assert.AreEqual (0, table1.Constraints.Count, "#8");
		}

		public delegate void  testExceptionMethodCallback();

		[Test]
		public void Add_Constraint()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			Assert.AreEqual(1,dt.Constraints.Count,"ccac#1"); 
			Assert.AreEqual("Constraint1",dt.Constraints[0].ConstraintName,"ccac#2");			

			DataSet ds = DataProvider.CreateForigenConstraint();
			Assert.AreEqual(1,ds.Tables[1].Constraints.Count,"ccac#3");
			Assert.AreEqual(1,ds.Tables[0].Constraints.Count,"ccac#4");

			ArrayList arr = new ArrayList(1);
			arr.Add(new ConstraintException()); 
			TestException( new testExceptionMethodCallback(DataProvider.TryToBreakUniqueConstraint),arr);

			arr = new ArrayList(1);
			arr.Add(new InvalidConstraintException()); 
			TestException( new testExceptionMethodCallback(DataProvider.TryToBreakForigenConstraint),arr);			
		}

		public void TestException(testExceptionMethodCallback dlg,IList exceptionList)
		{				
			try {
				dlg();
				Assert.Fail("ccac#A", ((Exception)exceptionList[0]).ToString()); 
			}
			catch(Exception ex) {					
				foreach(Exception expectedEx in exceptionList)
					if ( (expectedEx.GetType()) == (ex.GetType()) )
						return;				
				Assert.Fail("ccac#B");
			}		
		}

		[Test]
		public void Add_SDB1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Constraints.Add("UniqueConstraint",dt.Columns["ParentId"],true);
			Assert.AreEqual(1,(double) dt.Constraints.Count,1); 
			Assert.AreEqual("UniqueConstraint",dt.Constraints[0].ConstraintName,"CN34");			
		}
		
		[Test]
		public void Add_SDB2()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Constraints.Add("UniqueConstraint",dt.Columns["ParentId"],false);
			Assert.AreEqual(1,dt.Constraints.Count,"CN34"); 
			Assert.AreEqual("UniqueConstraint",dt.Constraints[0].ConstraintName,"CN35");			
		}
		
		[Test]
		public void Add_SDB3()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Constraints.Add("UniqueConstraint",dt.Columns["ParentId"],true);
			//Break the constraint

			ArrayList arr = new ArrayList(1);
			arr.Add(new ConstraintException()); 
			TestException( new testExceptionMethodCallback(DataProvider.TryToBreakUniqueConstraint),arr);

		}
		
		[Test]
		[ExpectedException(typeof(ConstraintException))]
		public void Add_SDB4()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Constraints.Add("UniqueConstraint",dt.Columns["ParentId"],false);
			//Break the constraint --> but we shouldn't get the excption --> wrong assumpation
			//TODO:check the right thing
			DataProvider.TryToBreakUniqueConstraint();
			Assert.AreEqual(2,dt.Select("ParentId=1").Length,"CN36");
		}

		[Test]
		public void Add_Constraint_Column_Column()
		{
			DataTable parent = DataProvider.CreateParentDataTable();
			DataTable child = DataProvider.CreateChildDataTable();
	 
			child.Constraints.Add("ForigenConstraint",parent.Columns[0],child.Columns[0]);

			Assert.AreEqual(1,parent.Constraints.Count,"ccaccc#1"); 
			Assert.AreEqual(1,child.Constraints.Count,"ccaccc#2"); 
			Assert.AreEqual("ForigenConstraint",child.Constraints[0].ConstraintName,"ccaccc#3");

			parent = DataProvider.CreateParentDataTable();
			child = DataProvider.CreateChildDataTable();
	 
			child.Constraints.Add("ForigenConstraint",parent.Columns[0],child.Columns[0]);

			ArrayList arr = new ArrayList(1);
			arr.Add(new InvalidConstraintException()); 
			TestException( new testExceptionMethodCallback(DataProvider.TryToBreakForigenConstraint),arr);

			Assert.AreEqual(1,parent.Constraints.Count,"ccaccc#4"); 
			Assert.AreEqual(1,child.Constraints.Count,"ccaccc#5"); 
		}

		[Test]
		public void AddRange_C1()
		{
			DataTable dt = new DataTable();
			dt.Constraints.AddRange(null);
			Assert.AreEqual(0,dt.Constraints.Count,"ccarc#1");
		}

		[Test]
		public void AddRange_C2()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(DataProvider.CreateParentDataTable());
			ds.Tables.Add(DataProvider.CreateChildDataTable());
			ds.Tables[1].Constraints.AddRange(GetConstraintArray(ds)); //Cuz foreign key belongs to child table
			Assert.AreEqual(2,ds.Tables[1].Constraints.Count,"ccarc#2");
			Assert.AreEqual(1,ds.Tables[0].Constraints.Count,"ccarc#3");
		}
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddRange_C3()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(DataProvider.CreateParentDataTable());
			ds.Tables.Add(DataProvider.CreateChildDataTable());
			Constraint badConstraint = new UniqueConstraint(ds.Tables[0].Columns[0]);

			ds.Tables[1].Constraints.AddRange(new Constraint[] {badConstraint}); //Cuz foreign key belongs to child table			
		}
		
		[Test]
		public void AddRange_C4()
		{
			ArrayList arr = new ArrayList(1);
			arr.Add(new ArgumentException());
			TestException(new testExceptionMethodCallback(AddRange_C3),arr);
		}

		private Constraint[] GetConstraintArray(DataSet ds)
		{
			DataTable parent = ds.Tables[0]; 
			DataTable child =  ds.Tables[1]; 
			Constraint[] constArray = new Constraint[2];

			//Create unique 
			constArray[0] = new UniqueConstraint("Unique1",child.Columns["ChildDouble"]);
			//Create foreign 
			constArray[1] = new ForeignKeyConstraint(parent.Columns[0],child.Columns[1]);

			return constArray;
		}

		[Test]
		public void Item()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints[0].ConstraintName = "constraint1";
			Assert.AreEqual("constraint1",dt.Constraints[0].ConstraintName,"cci#1");
			Assert.AreEqual("constraint1",dt.Constraints["constraint1"].ConstraintName,"cci#2");

			ArrayList arr = new ArrayList(1);
			arr.Add(new IndexOutOfRangeException()); 
			TestException(new testExceptionMethodCallback(Item2),arr);
		}

		private void Item2()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints[1].ConstraintName = "error";
		}

		private bool collectionChanged=false;

		[Test]
		public void RemoveAt_Integer()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints.RemoveAt(0);
			Assert.AreEqual(0,dt.Constraints.Count,"ccrai#1");

			dt = DataProvider.CreateUniqueConstraint();
			Constraint con = new UniqueConstraint(dt.Columns["String1"],false);
			dt.Constraints[0].ConstraintName = "constraint1";
			con.ConstraintName="constraint2";
			dt.Constraints.Add(con);
			dt.Constraints.RemoveAt(0);
			Assert.AreEqual(1,dt.Constraints.Count,"ccrai#2");
			Assert.AreEqual("constraint2",dt.Constraints[0].ConstraintName,"ccrai#3");

			dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints.CollectionChanged+=new CollectionChangeEventHandler(Constraints_CollectionChanged);
			dt.Constraints.RemoveAt(0);
			Assert.AreEqual(true,collectionChanged,"ccrai#4"); //Checking that event has raised

			ArrayList arr = new ArrayList(1);
			arr.Add(new IndexOutOfRangeException());
			TestException(new testExceptionMethodCallback(RemoveAt_I),arr);
		}

		private void Constraints_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			collectionChanged = true;
		}

		private void RemoveAt_I()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			dt.Constraints.RemoveAt(2);
		}

		[Test]
		public void RemoveTest ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1");
			Constraint c = table.Constraints.Add ("c", table.Columns [0], false);
			try {
				table.Constraints.Remove ("sdfs");
				Assert.Fail ("#1");
			} catch (ArgumentException e) {
				Assert.AreEqual ("Constraint 'sdfs' does not belong to this DataTable.", 
						e.Message, "#2");
			}
			
			table.Constraints.Remove (c);
			Assert.AreEqual (0, table.Constraints.Count, "#3");

			// No exception shud be raised
			table.Constraints.Add (c);
			Assert.AreEqual (1, table.Constraints.Count, "#4");
		}
	}
}
