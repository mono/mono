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
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests_System.Data
{
	[TestFixture] public class ForeignKeyConstraintTest2
	{
		[Test] public void Columns()
		{
			//int RowCount;
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// Columns
			Assert.AreEqual(dtChild.Columns[0] , fc.Columns[0]  , "FKC1");

			// Columns count
			Assert.AreEqual(1 , fc.Columns.Length , "FKC2");
		}

		[Test] public void Equals()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
			ds.EnforceConstraints = true;

			ForeignKeyConstraint fc1,fc2;
			fc1 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			fc2 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[1]);
			// different columnn
			Assert.AreEqual(false, fc1.Equals(fc2), "FKC3");

			//Two System.Data.ForeignKeyConstraint are equal if they constrain the same columns.
			// same column
			fc2 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);
			Assert.AreEqual(true, fc1.Equals(fc2), "FKC4");
		}

		[Test] public void RelatedColumns()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// RelatedColumns
			Assert.AreEqual(new DataColumn[] {dtParent.Columns[0]}, fc.RelatedColumns , "FKC5");
		}

		[Test] public void RelatedTable()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// RelatedTable
			Assert.AreEqual(dtParent, fc.RelatedTable , "FKC6");
		}

		[Test] public void Table()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// Table
			Assert.AreEqual(dtChild , fc.Table , "FKC7");
		}

		[Test] public new void ToString()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// ToString - default
			Assert.AreEqual(string.Empty , fc.ToString(), "FKC8");

			fc = new ForeignKeyConstraint("myConstraint",dtParent.Columns[0],dtChild.Columns[0]);
			// Tostring - Constraint name
			Assert.AreEqual("myConstraint", fc.ToString(), "FKC9");
		}

		[Test] public void acceptRejectRule()
		{
			DataSet ds = getNewDataSet();

			ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]);
			fc.AcceptRejectRule= AcceptRejectRule.Cascade;
			ds.Tables[1].Constraints.Add(fc);

			//Update the parent 

			ds.Tables[0].Rows[0]["ParentId"] = 777;
			Assert.AreEqual(true, ds.Tables[1].Select("ParentId=777").Length > 0 , "FKC10");
			ds.Tables[0].RejectChanges();
			Assert.AreEqual(0, ds.Tables[1].Select("ParentId=777").Length , "FKC11");
		}
		private DataSet getNewDataSet()
		{
			DataSet ds1 = new DataSet();
			ds1.Tables.Add(DataProvider.CreateParentDataTable());
			ds1.Tables.Add(DataProvider.CreateChildDataTable());
			//	ds1.Tables.Add(DataProvider.CreateChildDataTable());
			ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};

			return ds1;
		}

		[Test] public void constraintName()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// default 
			Assert.AreEqual(string.Empty , fc.ConstraintName , "FKC12");

			fc.ConstraintName  = "myConstraint";

			// set/get 
			Assert.AreEqual("myConstraint" , fc.ConstraintName , "FKC13");
		}

		[Test] public void ctor_ParentColChildCol()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataSet ds = new DataSet();
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			ForeignKeyConstraint fc = null;

			// Ctor ArgumentException
			try 
			{
				fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0]} ,new DataColumn[] {dtChild.Columns[0],dtChild.Columns[1]});				
				Assert.Fail("FKS14: ctor Indexer Failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("FKS15: ctor. Wrong exception type. Got:" + exc);
			}

			fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0],dtParent.Columns[1]} ,new DataColumn[] {dtChild.Columns[0],dtChild.Columns[2]});				

			// Add constraint to table - ArgumentException
			try 
			{
				dtChild.Constraints.Add(fc);
				Assert.Fail("FKS16: ctor Indexer Failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("FKC17: ctor. Wrong exception type. Got:" + exc);
			}

			// Child Table Constraints Count - two columnns
			Assert.AreEqual(0, dtChild.Constraints.Count , "FKC18");

			// Parent Table Constraints Count - two columnns
			Assert.AreEqual(1, dtParent.Constraints.Count , "FKC19");

			// DataSet relations Count
			Assert.AreEqual(0, ds.Relations.Count , "FKC20");

			dtParent.Constraints.Clear();
			dtChild.Constraints.Clear();

			fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0]} ,new DataColumn[] {dtChild.Columns[0]});
			// Ctor
			Assert.AreEqual(false , fc == null , "FKC21");

			// Child Table Constraints Count
			Assert.AreEqual(0, dtChild.Constraints.Count , "FKC22");

			// Parent Table Constraints Count
			Assert.AreEqual(0, dtParent.Constraints.Count , "FKC");

			// DataSet relations Count
			Assert.AreEqual(0, ds.Relations.Count , "FKC23");

			dtChild.Constraints.Add(fc);

			// Child Table Constraints Count, Add
			Assert.AreEqual(1, dtChild.Constraints.Count , "FKC24");

			// Parent Table Constraints Count, Add
			Assert.AreEqual(1, dtParent.Constraints.Count , "FKC25");

			// DataSet relations Count, Add
			Assert.AreEqual(0, ds.Relations.Count , "FKC26");

			// Parent Table Constraints type
			Assert.AreEqual(typeof(UniqueConstraint), dtParent.Constraints[0].GetType() , "FKC27");

			// Parent Table Constraints type
			Assert.AreEqual(typeof(ForeignKeyConstraint), dtChild.Constraints[0].GetType() , "FKC28");

			// Parent Table Primary key
			Assert.AreEqual(0, dtParent.PrimaryKey.Length , "FKC29");

			dtChild.Constraints.Clear();
			dtParent.Constraints.Clear();
			ds.Relations.Add(new DataRelation("myRelation",dtParent.Columns[0],dtChild.Columns[0]));

			// Relation - Child Table Constraints Count
			Assert.AreEqual(1, dtChild.Constraints.Count , "FKC30");

			// Relation - Parent Table Constraints Count
			Assert.AreEqual(1, dtParent.Constraints.Count , "FKC31");

			// Relation - Parent Table Constraints type
			Assert.AreEqual(typeof(UniqueConstraint), dtParent.Constraints[0].GetType() , "FKC32");

			// Relation - Parent Table Constraints type
			Assert.AreEqual(typeof(ForeignKeyConstraint), dtChild.Constraints[0].GetType() , "FKC33");

			// Relation - Parent Table Primary key
			Assert.AreEqual(0, dtParent.PrimaryKey.Length , "FKC34");
		}

		[Test] public void ctor_NameParentColChildCol()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint("myForeignKey",dtParent.Columns[0],dtChild.Columns[0]);

			// Ctor
			Assert.AreEqual(false , fc == null , "FKC35");

			// Ctor - name
			Assert.AreEqual("myForeignKey" , fc.ConstraintName  , "FKC36");
		}

		[Test] public void ctor_NameParentColsChildCols()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint("myForeignKey",new DataColumn[] {dtParent.Columns[0]} ,new DataColumn[] {dtChild.Columns[0]});

			// Ctor
			Assert.AreEqual(false , fc == null , "FKC37");

			// Ctor - name
			Assert.AreEqual("myForeignKey" , fc.ConstraintName  , "FKC38");
		}

		[Test] public void deleteRule()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
			ds.EnforceConstraints = true;

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			//checking default
			// Default
			Assert.AreEqual(Rule.Cascade , fc.DeleteRule , "FKC39");

			//checking set/get
			foreach (Rule rule in Enum.GetValues(typeof(Rule)))
			{
				// Set/Get - rule
				fc.DeleteRule = rule;
				Assert.AreEqual(rule, fc.DeleteRule , "FKC40");
			}

			dtChild.Constraints.Add(fc);

			//checking delete rule

			// Rule = None, Delete Exception
			fc.DeleteRule = Rule.None;
			//Exception = "Cannot delete this row because constraints are enforced on relation Constraint1, and deleting this row will strand child rows."
			try 
			{
				dtParent.Rows.Find(1).Delete();
				Assert.Fail("FKC41: Find Indexer Failed to throw InvalidConstraintException");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("FKC42: Find. Wrong exception type. Got:" + exc);
			}

			// Rule = None, Delete succeed
			fc.DeleteRule = Rule.None;
			foreach (DataRow dr in dtChild.Select("ParentId = 1"))
				dr.Delete();
			dtParent.Rows.Find(1).Delete();
			Assert.AreEqual(0, dtParent.Select("ParentId=1").Length , "FKC43");

			// Rule = Cascade
			fc.DeleteRule = Rule.Cascade;
			dtParent.Rows.Find(2).Delete();
			Assert.AreEqual(0, dtChild.Select("ParentId=2").Length , "FKC44");

			// Rule = SetNull
			DataSet ds1 = new DataSet();
			ds1.Tables.Add(DataProvider.CreateParentDataTable());
			ds1.Tables.Add(DataProvider.CreateChildDataTable());

			ForeignKeyConstraint fc1 = new ForeignKeyConstraint(ds1.Tables[0].Columns[0],ds1.Tables[1].Columns[1]); 
			fc1.DeleteRule = Rule.SetNull;
			ds1.Tables[1].Constraints.Add(fc1);

			Assert.AreEqual(0, ds1.Tables[1].Select("ChildId is null").Length, "FKC45");

			ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};
			ds1.Tables[0].Rows.Find(3).Delete();

			ds1.Tables[0].AcceptChanges();
			ds1.Tables[1].AcceptChanges();	

			DataRow[] arr =  ds1.Tables[1].Select("ChildId is null");

			/*foreach (DataRow dr in arr)
					{
						Assert.AreEqual(null, dr["ChildId"], "FKC");
					}*/

			Assert.AreEqual(4, arr.Length , "FKC46");

			// Rule = SetDefault
			//fc.DeleteRule = Rule.SetDefault;
			ds1 = new DataSet();
			ds1.Tables.Add(DataProvider.CreateParentDataTable());
			ds1.Tables.Add(DataProvider.CreateChildDataTable());

			fc1 = new ForeignKeyConstraint(ds1.Tables[0].Columns[0],ds1.Tables[1].Columns[1]); 
			fc1.DeleteRule = Rule.SetDefault;
			ds1.Tables[1].Constraints.Add(fc1);
			ds1.Tables[1].Columns[1].DefaultValue="777";

			//Add new row  --> in order to apply the forigen key rules
			DataRow dr2 = ds1.Tables[0].NewRow();
			dr2["ParentId"] = 777;
			ds1.Tables[0].Rows.Add(dr2);

			ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};
			ds1.Tables[0].Rows.Find(3).Delete();
			Assert.AreEqual(4, ds1.Tables[1].Select("ChildId=777").Length  , "FKC47");
		}

		[Test] public void extendedProperties()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateParentDataTable();

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			PropertyCollection pc = fc.ExtendedProperties ;

			// Checking ExtendedProperties default 
			Assert.AreEqual(true, fc != null, "FKC48");

			// Checking ExtendedProperties count 
			Assert.AreEqual(0, pc.Count , "FKC49");
		}

		[Test]
		public void ctor_DclmDclm()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataSet ds = new DataSet();
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			
			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			Assert.IsFalse(fc == null ,"FKC64" );
			
			Assert.AreEqual(0,dtChild.Constraints.Count ,"FKC50");
			
			Assert.AreEqual(0,dtParent.Constraints.Count ,"FKC51");
			
			Assert.AreEqual(0,ds.Relations.Count ,"FKC52");

			dtChild.Constraints.Add(fc);

			Assert.AreEqual(1,dtChild.Constraints.Count ,"FKC53");
			
			Assert.AreEqual(1,dtParent.Constraints.Count ,"FKC54");
			
			Assert.AreEqual(0,ds.Relations.Count ,"FKC55");
			
			Assert.AreEqual(typeof(UniqueConstraint),dtParent.Constraints[0].GetType() ,"FKC56");
			
			Assert.AreEqual(typeof(ForeignKeyConstraint),dtChild.Constraints[0].GetType() ,"FKC57");
			
			Assert.AreEqual(0,dtParent.PrimaryKey.Length ,"FKC58");

			dtChild.Constraints.Clear();
			dtParent.Constraints.Clear();
			ds.Relations.Add(new DataRelation("myRelation",dtParent.Columns[0],dtChild.Columns[0]));

			Assert.AreEqual(1,dtChild.Constraints.Count ,"FKC59");
	
			Assert.AreEqual(1,dtParent.Constraints.Count ,"FKC60");
			
			Assert.AreEqual(typeof(UniqueConstraint),dtParent.Constraints[0].GetType() ,"FKC61");
			
			Assert.AreEqual(typeof(ForeignKeyConstraint),dtChild.Constraints[0].GetType() ,"FKC62");
			
			Assert.AreEqual(0,dtParent.PrimaryKey.Length ,"FKC63");			
		}

		[Test]
		[ExpectedException(typeof(NullReferenceException))]
		public void ctor_DclmDclm1()
		{
			ForeignKeyConstraint fc = new ForeignKeyConstraint((DataColumn)null,(DataColumn)null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ctor_DclmDclm2()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(DataProvider.CreateParentDataTable());
			ds.Tables.Add(DataProvider.CreateChildDataTable());
			ds.Tables["Parent"].Columns["ParentId"].Expression = "2";
			
			ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ctor_DclmDclm3()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(DataProvider.CreateParentDataTable());
			ds.Tables.Add(DataProvider.CreateChildDataTable());
			ds.Tables["Child"].Columns["ParentId"].Expression = "2";
			
			ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]);
		}

		[Test]
		public void UpdateRule1()
		{
			DataSet ds = GetNewDataSet();
			ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]);
			fc.UpdateRule=Rule.Cascade; 
			ds.Tables[1].Constraints.Add(fc);

			//Changing parent row

			ds.Tables[0].Rows.Find(1)["ParentId"] = 8;

			ds.Tables[0].AcceptChanges();
			ds.Tables[1].AcceptChanges();
			//Checking the table

			Assert.IsTrue(ds.Tables[1].Select("ParentId=8").Length > 0, "FKC66");

		}

		[Test]
		[ExpectedException(typeof(ConstraintException))]
		public void UpdateRule2()
		{
			DataSet ds = GetNewDataSet();
			ds.Tables[0].PrimaryKey=null;
			ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]);
			fc.UpdateRule=Rule.None; 
			ds.Tables[1].Constraints.Add(fc);

			//Changing parent row

			ds.Tables[0].Rows[0]["ParentId"] = 5;
			
			/*ds.Tables[0].AcceptChanges();
			ds.Tables[1].AcceptChanges();
			//Checking the table
			Compare(ds.Tables[1].Select("ParentId=8").Length ,0);*/
		}

		[Test]
		public void UpdateRule3()
		{
			DataSet ds = GetNewDataSet();
			ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[1]);
			fc.UpdateRule=Rule.SetDefault; 
			ds.Tables[1].Constraints.Add(fc);

			//Changing parent row

			ds.Tables[1].Columns[1].DefaultValue="777";
			
			//Add new row  --> in order to apply the forigen key rules
			DataRow dr = ds.Tables[0].NewRow();
			dr["ParentId"] = 777;
			ds.Tables[0].Rows.Add(dr);


			ds.Tables[0].Rows.Find(1)["ParentId"] = 8;

			ds.Tables[0].AcceptChanges();
			ds.Tables[1].AcceptChanges();
			//Checking the table

			Assert.IsTrue(ds.Tables[1].Select("ChildId=777").Length > 0, "FKC67");
		}

		[Test]
		public void UpdateRule4()
		{
			DataSet ds = GetNewDataSet();
			ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]);
			fc.UpdateRule=Rule.SetNull; 
			ds.Tables[1].Constraints.Add(fc);

			//Changing parent row

			ds.Tables[0].Rows.Find(1)["ParentId"] = 8;

			ds.Tables[0].AcceptChanges();
			ds.Tables[1].AcceptChanges();
			//Checking the table

			Assert.IsTrue(ds.Tables[1].Select("ParentId is null").Length > 0, "FKC68");

		}

		private DataSet GetNewDataSet()
		{
			DataSet ds1 = new DataSet();
			ds1.Tables.Add(DataProvider.CreateParentDataTable());
			ds1.Tables.Add(DataProvider.CreateChildDataTable());
			ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};

			return ds1;
		}
#if NET_2_0
		[Test]
		public void ForeignConstraint_DateTimeModeTest()
		{
			DataTable t1 = new DataTable("t1");
			t1.Columns.Add("col", typeof(DateTime));

			DataTable t2 = new DataTable("t2");
			t2.Columns.Add("col", typeof(DateTime));
			t2.Columns[0].DateTimeMode = DataSetDateTime.Unspecified;
			
			// DataColumn type shud match, and no exception shud be raised 
			t2.Constraints.Add("fk", t1.Columns[0], t2.Columns[0]);

			t2.Constraints.Clear();
			t2.Columns[0].DateTimeMode = DataSetDateTime.Local;
			try {
				// DataColumn type shud not match, and exception shud be raised 
				t2.Constraints.Add("fk", t1.Columns[0], t2.Columns[0]);
				Assert.Fail("#1");
			} catch (InvalidOperationException e) {}
		}
#endif

		[Test] // bug #79689
		public void ParentChildSameColumn ()
		{
			DataTable dataTable = new DataTable ("Menu");
			DataColumn colID = dataTable.Columns.Add ("ID", typeof (int));
			DataColumn colCulture = dataTable.Columns.Add ("Culture", typeof (string));
			dataTable.Columns.Add ("Name", typeof (string));
			DataColumn colParentID = dataTable.Columns.Add ("ParentID", typeof (int));

			// table PK (ID, Culture)
			dataTable.Constraints.Add (new UniqueConstraint (
				"MenuPK",
				new DataColumn [] { colID, colCulture },
				true));

			// add a FK referencing the same table: (ID, Culture) <- (ParentID, Culture)
			ForeignKeyConstraint fkc = new ForeignKeyConstraint (
				"MenuParentFK",
				new DataColumn [] { colID, colCulture },
				new DataColumn [] { colParentID, colCulture });

			dataTable.Constraints.Add (fkc);
		}
	}
}
