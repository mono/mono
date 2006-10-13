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
//using System.ComponentModel;
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataRelationTest2
	{
		[Test] public void ChildColumns()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// ChildColumns 1
			Assert.AreEqual(1 , dRel.ChildColumns.Length , "DR1");

			// ChildColumns 2
			Assert.AreEqual(dtChild.Columns[0] , dRel.ChildColumns[0] , "DR2");
		}

		[Test] public void ChildKeyConstraint()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// ChildKeyConstraint 1
			Assert.AreEqual(dtChild.Constraints[0] , dRel.ChildKeyConstraint, "DR3");
		}

		[Test] public void ChildTable()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// ChildTable
			Assert.AreEqual(dtChild , dRel.ChildTable , "DR4");
		}

		[Test] public void DataSet()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// DataSet
			Assert.AreEqual(ds , dRel.DataSet , "DR5");
		}

		[Test] public void ParentColumns()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// ParentColumns 1
			Assert.AreEqual(1 , dRel.ParentColumns.Length , "DR6");

			// ParentColumns 2
			Assert.AreEqual(dtParent.Columns[0] , dRel.ParentColumns[0] , "DR7");
		}

		[Test] public void ParentKeyConstraint()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// ChildKeyConstraint 1
			Assert.AreEqual(dtParent.Constraints[0] , dRel.ParentKeyConstraint , "DR8");
		}

		[Test] public void ParentTable()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// ParentTable
			Assert.AreEqual(dtParent , dRel.ParentTable , "DR9");
		}

		[Test] public new void ToString()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation(null,dtParent.Columns[0],dtChild.Columns[0]);

			// ToString 1
			Assert.AreEqual(string.Empty , dRel.ToString() , "DR10");

			ds.Relations.Add(dRel);

			// ToString 2
			Assert.AreEqual("Relation1", dRel.ToString() , "DR11");

			dRel.RelationName = "myRelation";

			// ToString 3
			Assert.AreEqual("myRelation", dRel.ToString() , "DR12");
		}

		[Test] public void ctor_ByNameDataColumns()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// DataRelation - CTor
			Assert.AreEqual(false , dRel == null , "DR13");

			// DataRelation - parent Constraints
			Assert.AreEqual(1, dtParent.Constraints.Count , "DR14");

			// DataRelation - child Constraints
			Assert.AreEqual(1, dtChild.Constraints.Count , "DR15");

			// DataRelation - child relations
			Assert.AreEqual(dRel, dtParent.ChildRelations[0] , "DR16");

			// DataRelation - parent relations
			Assert.AreEqual(dRel , dtChild.ParentRelations[0], "DR17");

			// DataRelation - name
			Assert.AreEqual("MyRelation" , dRel.RelationName , "DR18");

			// DataRelation - parent UniqueConstraint
			Assert.AreEqual(typeof(UniqueConstraint), dtParent.Constraints[0].GetType() , "DR19");

			// DataRelation - Child ForeignKeyConstraint
			Assert.AreEqual(typeof(ForeignKeyConstraint), dtChild.Constraints[0].GetType() , "DR20");

			ds.Relations.Clear();
			// Remove DataRelation - Parent Constraints
			Assert.AreEqual(1, dtParent.Constraints.Count , "DR21");

			// Remove DataRelation - Child Constraints
			Assert.AreEqual(1, dtChild.Constraints.Count , "DR22");

			// Remove DataRelation - child relations
			Assert.AreEqual(0, dtParent.ChildRelations.Count , "DR23");

			// Remove DataRelation - parent relations
			Assert.AreEqual(0, dtChild.ParentRelations.Count , "DR24");

			//add relation which will create invalid constraint
			dtChild.Constraints.Clear();
			dtParent.Constraints.Clear();
			//add duplicated row
			dtParent.Rows.Add(dtParent.Rows[0].ItemArray); 
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);

			// Add relation which will create invalid constraint
			try {
				ds.Relations.Add(dRel);
				Assert.Fail("DR25: Add failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DR26: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test] public void ctor_ByNameDataColumnsCreateConstraints()
		{	
			DataRelation dRel;		
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();

			DataSet ds = new DataSet();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			//parameter createConstraints = true

			bool createConstraints = true;
			for (int i=0; i<=1; i++)
			{
				if (i==0)
					createConstraints = false;
				else 
					createConstraints = true;

				ds.Relations.Clear();
				dtParent.Constraints.Clear();
				dtChild.Constraints.Clear();

				//add duplicated row
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray); 
				dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0],createConstraints);
				// Add relation which will create invalid constraint
				if (createConstraints==true)
				{
					try {
						ds.Relations.Add(dRel);
						Assert.Fail("DR27: Add failed to throw ArgmentException");
					}
					catch (ArgumentException) {}
					catch (AssertionException exc) {throw  exc;}
					catch (Exception exc)
					{
						Assert.Fail("DR28: Add. Wrong exception type. Got:" + exc);
					}
				}
				else
					ds.Relations.Add(dRel);

				dtParent.Rows.Remove(dtParent.Rows[dtParent.Rows.Count-1]);
				ds.Relations.Clear();
				dtParent.Constraints.Clear();
				dtChild.Constraints.Clear();
				dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0],createConstraints);
				ds.Relations.Add(dRel);

				// DataRelation - CTor,createConstraints=
				Assert.AreEqual(false , dRel == null , "DR29:" + createConstraints.ToString());

				// DataRelation - parent Constraints,createConstraints=
				Assert.AreEqual(i, dtParent.Constraints.Count , "DR30:" + createConstraints.ToString());

				// DataRelation - child Constraints,createConstraints=
				Assert.AreEqual(i, dtChild.Constraints.Count , "DR31:" + createConstraints.ToString());

				// DataRelation - child relations,createConstraints=
				Assert.AreEqual(dRel, dtParent.ChildRelations[0] , "DR32:" + createConstraints.ToString());

				// DataRelation - parent relations,createConstraints=
				Assert.AreEqual(dRel , dtChild.ParentRelations[0], "DR33:" + createConstraints.ToString());

				// DataRelation - name
				Assert.AreEqual("MyRelation" , dRel.RelationName , "DR34");
			}
		}

		[Test] public void ctor_ByNameDataColumnsArrays()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;

			//check some exception 
			// DataRelation - CTor ArgumentException, two columns child
			try {
				dRel = new DataRelation("MyRelation",new DataColumn[] {dtParent.Columns[0]},new DataColumn[]  {dtChild.Columns[0],dtChild.Columns[2]});
				Assert.Fail("DR35: ctor failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DR36: ctor. Wrong exception type. Got:" + exc);
			}

			dRel = new DataRelation("MyRelation",new DataColumn[] {dtParent.Columns[0],dtParent.Columns[1]},new DataColumn[]  {dtChild.Columns[0],dtChild.Columns[2]});
			// DataRelation - Add Relation ArgumentException, fail on creating child Constraints
			try {
				ds.Relations.Add(dRel);
				Assert.Fail("DR37: Add failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DR38: Add. Wrong exception type. Got:" + exc);
			}

			// DataRelation ArgumentException - parent Constraints
			Assert.AreEqual(1, dtParent.Constraints.Count , "DR39");

			// DataRelation ArgumentException - child Constraints
			Assert.AreEqual(0, dtChild.Constraints.Count , "DR40");

			// DataRelation ArgumentException - DataSet.Relation count
			Assert.AreEqual(1, ds.Relations.Count , "DR41");

			//begin to check the relation ctor
			dtParent.Constraints.Clear();
			dtChild.Constraints.Clear();
			ds.Relations.Clear();
			dRel = new DataRelation("MyRelation",new DataColumn[] {dtParent.Columns[0]},new DataColumn[]  {dtChild.Columns[0]});
			ds.Relations.Add(dRel);

			// DataSet DataRelation count
			Assert.AreEqual(1, ds.Relations.Count , "DR42");

			// DataRelation - CTor
			Assert.AreEqual(false , dRel == null , "DR43");

			// DataRelation - parent Constraints
			Assert.AreEqual(1, dtParent.Constraints.Count , "DR44");

			// DataRelation - child Constraints
			Assert.AreEqual(1, dtChild.Constraints.Count , "DR45");

			// DataRelation - child relations
			Assert.AreEqual(dRel, dtParent.ChildRelations[0] , "DR46");

			// DataRelation - parent relations
			Assert.AreEqual(dRel , dtChild.ParentRelations[0], "DR47");

			// DataRelation - name
			Assert.AreEqual("MyRelation" , dRel.RelationName , "DR48");
		}

		[Test] public void ctor_ByNameDataColumnsArraysCreateConstraints()
		{
			DataRelation dRel;		
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();

			DataSet ds = new DataSet();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			//parameter createConstraints = true

			bool createConstraints = true;
			for (int i=0; i<=1; i++)
			{
				if (i==0)
					createConstraints = false;
				else 
					createConstraints = true;

				ds.Relations.Clear();
				dtParent.Constraints.Clear();
				dtChild.Constraints.Clear();

				//add duplicated row
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray); 
				dRel = new DataRelation("MyRelation",new DataColumn[] {dtParent.Columns[0]},new DataColumn[]  {dtChild.Columns[0]},createConstraints);
				// Add relation which will create invalid constraint
				if (createConstraints==true)
				{
					try {
						ds.Relations.Add(dRel);
						Assert.Fail("DR49: Add failed to throw ArgmentException");
					}
					catch (ArgumentException) {}
					catch (AssertionException exc) {throw  exc;}
					catch (Exception exc)
					{
						Assert.Fail("DR50: Add. Wrong exception type. Got:" + exc);
					}
				}
				else
					ds.Relations.Add(dRel);

				ds.Relations.Clear();
				dtParent.Constraints.Clear();
				dtChild.Constraints.Clear();
				dtParent.Rows.Remove(dtParent.Rows[dtParent.Rows.Count-1]);

				dRel = new DataRelation("MyRelation",new DataColumn[] {dtParent.Columns[0]},new DataColumn[]  {dtChild.Columns[0]},createConstraints);
				ds.Relations.Add(dRel);

				// DataRelation - CTor,createConstraints=
				Assert.AreEqual(false, dRel == null, "DR51:" + createConstraints.ToString());

				// DataRelation - parent Constraints,createConstraints=
				Assert.AreEqual(i, dtParent.Constraints.Count , "DR52:" + createConstraints.ToString());

				// DataRelation - child Constraints,createConstraints=
				Assert.AreEqual(i, dtChild.Constraints.Count , "DR53:" + createConstraints.ToString());

				// DataRelation - child relations,createConstraints=
				Assert.AreEqual(dRel, dtParent.ChildRelations[0] , "DR54:" + createConstraints.ToString());

				// DataRelation - parent relations,createConstraints=
				Assert.AreEqual(dRel , dtChild.ParentRelations[0], "DR55:" + createConstraints.ToString());

				// DataRelation - name
				Assert.AreEqual("MyRelation" , dRel.RelationName , "DR56");
			}	
		}

		[Test] public void extendedProperties()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation("MyRelation",dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			PropertyCollection pc;
			pc = dRel.ExtendedProperties ;

			// Checking ExtendedProperties default 
			Assert.AreEqual(true, pc != null, "DR57");

			// Checking ExtendedProperties count 
			Assert.AreEqual(0, pc.Count , "DR58");
		}

		[Test] public void nested()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation(null,dtParent.Columns[0],dtChild.Columns[0]);
			ds.Relations.Add(dRel);

			// Nested default 
			Assert.AreEqual(false, dRel.Nested  , "DR59");

			dRel.Nested = true;

			// Nested get/set
			Assert.AreEqual(true, dRel.Nested , "DR60");
		}

		[Test] public void relationName()
		{
			DataSet ds = new DataSet();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			DataRelation dRel;
			dRel = new DataRelation(null,dtParent.Columns[0],dtChild.Columns[0]);

			// RelationName default 1
			Assert.AreEqual(string.Empty , dRel.RelationName , "DR61");

			ds.Relations.Add(dRel);

			// RelationName default 2
			Assert.AreEqual("Relation1", dRel.RelationName , "DR62");

			dRel.RelationName = "myRelation";

			// RelationName get/set
			Assert.AreEqual("myRelation", dRel.RelationName , "DR63");
		}

		[Test]
		public void bug79233 ()
		{
			DataSet ds = new DataSet ();
			DataTable dtChild = DataProvider.CreateChildDataTable ();
			DataTable dtParent = DataProvider.CreateParentDataTable ();
			ds.Tables.Add (dtParent);
			ds.Tables.Add (dtChild);

			dtParent.Rows.Clear ();
			dtChild.Rows.Clear ();

			DataRelation dr = dtParent.ChildRelations.Add (dtParent.Columns [0], dtChild.Columns [0]);
			Assert.AreEqual ("Relation1", dr.RelationName, "#1");
			dr = dtChild.ChildRelations.Add (dtChild.Columns [0], dtParent.Columns [0]);
			Assert.AreEqual ("Relation2", dr.RelationName, "#1");
		}

#if NET_2_0
		[Test]
		public void DataRelationTest()
		{
			DataSet ds = new DataSet();
			
			DataTable t1 = new DataTable("t1");
			t1.Columns.Add("col", typeof(DateTime));

			DataTable t2 = new DataTable("t2");
			t2.Columns.Add("col", typeof(DateTime));
			t2.Columns[0].DateTimeMode = DataSetDateTime.Unspecified;

			ds.Tables.Add(t1);
			ds.Tables.Add(t2);
			ds.Relations.Add("rel", t1.Columns[0], t2.Columns[0], false);

			ds.Relations.Clear();
			t2.Columns[0].DateTimeMode = DataSetDateTime.Local;

			try {
				ds.Relations.Add("rel", t1.Columns[0], t2.Columns[0], false);
				Assert.Fail ("#1");
			} catch (InvalidConstraintException) { }
		}
#endif
	}
}
