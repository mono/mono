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

namespace MonoTests.System.Data
{
	[TestFixture] public class UniqueConstraintTest2
	{
		[Test] public void Columns()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint(dtParent.Columns[0]);

			// Columns 1
			Assert.AreEqual(1, uc.Columns.Length  , "UC1");

			// Columns 2
			Assert.AreEqual(dtParent.Columns[0], uc.Columns[0], "UC2");
		}

		[Test] public void Equals_O()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);

			UniqueConstraint  uc1,uc2;
			uc1 = new UniqueConstraint(dtParent.Columns[0]);

			uc2 = new UniqueConstraint(dtParent.Columns[1]);
			// different columnn
			Assert.AreEqual(false, uc1.Equals(uc2), "UC3");

			//Two System.Data.ForeignKeyConstraint are equal if they constrain the same columns.
			// same column
			uc2 = new UniqueConstraint(dtParent.Columns[0]);
			Assert.AreEqual(true, uc1.Equals(uc2), "UC4");
		}

		[Test] public void IsPrimaryKey()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint(dtParent.Columns[0],false);
			dtParent.Constraints.Add(uc);

			// primary key 1
			Assert.AreEqual(false, uc.IsPrimaryKey , "UC5");

			dtParent.Constraints.Remove(uc);
			uc = new UniqueConstraint(dtParent.Columns[0],true);
			dtParent.Constraints.Add(uc);

			// primary key 2
			Assert.AreEqual(true, uc.IsPrimaryKey , "UC6");
		}

		[Test] public void Table()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			UniqueConstraint uc = null;
			uc = new UniqueConstraint(dtParent.Columns[0]);

			// Table
			Assert.AreEqual(dtParent , uc.Table , "UC7");
		}

		[Test] public new void ToString()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint(dtParent.Columns[0],false);

			// ToString - default
			Assert.AreEqual(string.Empty , uc.ToString(), "UC8");

			uc = new UniqueConstraint("myConstraint",dtParent.Columns[0],false);
			// Tostring - Constraint name
			Assert.AreEqual("myConstraint", uc.ToString(), "UC9");
		}

		[Test] public void constraintName()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint(dtParent.Columns[0]);

			// default 
			Assert.AreEqual(string.Empty , uc.ConstraintName , "UC10");

			uc.ConstraintName  = "myConstraint";

			// set/get 
			Assert.AreEqual("myConstraint" , uc.ConstraintName , "UC11");
		}

		[Test] public void ctor_DataColumn()
		{
			Exception tmpEx = new Exception();

			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			ds.Tables.Add(dtParent);
			ds.EnforceConstraints = true;

			UniqueConstraint uc = null;

			// DataColumn.Unique - without constraint
			Assert.AreEqual(false, dtParent.Columns[0].Unique , "UC12");

			uc = new UniqueConstraint(dtParent.Columns[0]);

			// Ctor
			Assert.AreEqual(false , uc == null , "UC13");

			// DataColumn.Unique - with constraint
			Assert.AreEqual(false, dtParent.Columns[0].Unique , "UC14");

			// Ctor - add exisiting column
			dtParent.Rows.Add(new object[] {99,"str1","str2"});
			dtParent.Constraints.Add(uc);
			try 
			{
				dtParent.Rows.Add(new object[] {99,"str1","str2"});
				Assert.Fail("DS333: Rows.Add Failed to throw ConstraintException");
			}
			catch (ConstraintException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DS334: Rows.Add. Wrong exception type. Got:" + exc);
			}

			DataTable dtChild = DataProvider.CreateChildDataTable(); 
			uc = new UniqueConstraint(dtChild.Columns[1]);

			//Column[1] is not unique, will throw exception
			// ArgumentException 
			try 
			{
				dtChild.Constraints.Add(uc);        
				Assert.Fail("DS333: Constraints.Add Failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DS334: Constraints.Add. Wrong exception type. Got:" + exc);
			}

			//reset the table
			dtParent = DataProvider.CreateParentDataTable();

			// DataColumn.Unique = true, will add UniqueConstraint
			dtParent.Columns[0].Unique = true;
			Assert.AreEqual(1, dtParent.Constraints.Count , "UC15");

			// Check the created UniqueConstraint
			dtParent.Columns[0].Unique = true;
			Assert.AreEqual(typeof(UniqueConstraint).FullName, dtParent.Constraints[0].GetType().FullName , "UC16");

			// add UniqueConstarint that don't belong to the table
			try 
			{
				dtParent.Constraints.Add(uc);
				Assert.Fail("DS333: Constraints.Add Failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DS334: Constraints.Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test] public void ctor_DataColumnNoPrimary()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint(dtParent.Columns[0],false);
			dtParent.Constraints.Add(uc);

			// Ctor
			Assert.AreEqual(false , uc == null , "UC17");

			// primary key 1
			Assert.AreEqual(0, dtParent.PrimaryKey.Length  , "UC18");

			dtParent.Constraints.Remove(uc);
			uc = new UniqueConstraint(dtParent.Columns[0],true);
			dtParent.Constraints.Add(uc);

			// primary key 2
			Assert.AreEqual(1, dtParent.PrimaryKey.Length  , "UC19");
		}

		[Test] public void ctor_DataColumns()
		{
			Exception tmpEx = new Exception();
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint(new DataColumn[] {dtParent.Columns[0],dtParent.Columns[1]});

			// Ctor - parent
			Assert.AreEqual(false , uc == null , "UC20");

			// Ctor - add exisiting column
			dtParent.Rows.Add(new object[] {99,"str1","str2"});
			dtParent.Constraints.Add(uc);
			try 
			{
				dtParent.Rows.Add(new object[] {99,"str1","str2"});
				Assert.Fail("DS333: Rows.Add Failed to throw ConstraintException");
			}
			catch (ConstraintException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DS334: Rows.Add. Wrong exception type. Got:" + exc);
			}

			DataTable dtChild = DataProvider.CreateChildDataTable(); 
			uc = new UniqueConstraint(new DataColumn[] {dtChild.Columns[0],dtChild.Columns[1]});
			dtChild.Constraints.Add(uc);

			// Ctor - child
			Assert.AreEqual(false , uc == null , "UC21");

			dtChild.Constraints.Clear();
			uc = new UniqueConstraint(new DataColumn[] {dtChild.Columns[1],dtChild.Columns[2]});

			//target columnn are not unnique, will throw an exception
			// ArgumentException - child
			try 
			{
				dtChild.Constraints.Add(uc);        
				Assert.Fail("DS333: Constraints.Add Failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DS334: Constraints.Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test] public void ctor_DataColumnPrimary()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint(dtParent.Columns[0],false);
			dtParent.Constraints.Add(uc);

			// Ctor
			Assert.AreEqual(false , uc == null , "UC22");

			// primary key 1
			Assert.AreEqual(0, dtParent.PrimaryKey.Length  , "UC23");

			dtParent.Constraints.Remove(uc);
			uc = new UniqueConstraint(dtParent.Columns[0],true);
			dtParent.Constraints.Add(uc);

			// primary key 2
			Assert.AreEqual(1, dtParent.PrimaryKey.Length  , "UC24");
		}

		[Test] public void ctor_NameDataColumn()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint("myConstraint",dtParent.Columns[0]);

			// Ctor
			Assert.AreEqual(false , uc == null , "UC25");

			// Ctor name
			Assert.AreEqual("myConstraint", uc.ConstraintName , "UC26");
		}

		[Test] public void ctor_NameDataColumnPrimary()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint("myConstraint",dtParent.Columns[0],false);
			dtParent.Constraints.Add(uc);

			// Ctor
			Assert.AreEqual(false , uc == null , "UC27");

			// primary key 1
			Assert.AreEqual(0, dtParent.PrimaryKey.Length  , "UC28");

			// Ctor name 1
			Assert.AreEqual("myConstraint", uc.ConstraintName , "UC29");

			dtParent.Constraints.Remove(uc);
			uc = new UniqueConstraint("myConstraint",dtParent.Columns[0],true);
			dtParent.Constraints.Add(uc);

			// primary key 2
			Assert.AreEqual(1, dtParent.PrimaryKey.Length  , "UC30");

			// Ctor name 2
			Assert.AreEqual("myConstraint", uc.ConstraintName , "UC31");
		}

		[Test] public void ctor_NameDataColumns()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint("myConstraint",new DataColumn[] {dtParent.Columns[0],dtParent.Columns[1]});

			// Ctor
			Assert.AreEqual(false , uc == null , "UC32");

			// Ctor name
			Assert.AreEqual("myConstraint", uc.ConstraintName , "UC33");
		}

		[Test] public void ctor_NameDataColumnsPrimary()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint("myConstraint",new DataColumn[] {dtParent.Columns[0]},false);
			dtParent.Constraints.Add(uc);

			// Ctor
			Assert.AreEqual(false , uc == null , "UC34");

			// primary key 1
			Assert.AreEqual(0, dtParent.PrimaryKey.Length  , "UC35");

			// Ctor name 1
			Assert.AreEqual("myConstraint", uc.ConstraintName , "UC36");

			dtParent.Constraints.Remove(uc);
			uc = new UniqueConstraint("myConstraint",new DataColumn[] {dtParent.Columns[0]},true);
			dtParent.Constraints.Add(uc);

			// primary key 2
			Assert.AreEqual(1, dtParent.PrimaryKey.Length  , "UC37");

			// Ctor name 2
			Assert.AreEqual("myConstraint", uc.ConstraintName , "UC38");
		}

		[Test] public void extendedProperties()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();

			UniqueConstraint uc = null;
			uc = new UniqueConstraint(dtParent.Columns[0]);
			PropertyCollection pc = uc.ExtendedProperties ;

			// Checking ExtendedProperties default 
			Assert.AreEqual(true, pc != null, "UC39");

			// Checking ExtendedProperties count 
			Assert.AreEqual(0, pc.Count , "UC40");
		}
	}
}
