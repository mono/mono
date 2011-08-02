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

using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataColumnTest2
	{
		private CultureInfo originalCulture;

		[SetUp]
		public void SetUp ()
		{
			originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		[Test] public void AllowDBNull()
		{
			DataTable dt = new DataTable();
			DataColumn dc;
			dc = new DataColumn("ColName",typeof(int));
			dc.DefaultValue = DBNull.Value;
			dt.Columns.Add(dc);
			dc.AutoIncrement=false;

			// Checking default value (True)
			Assert.AreEqual(true ,  dc.AllowDBNull, "DC1");

			// AllowDBNull=true - adding new row with null value
			dt.Rows.Add(dt.NewRow());
			Assert.AreEqual(DBNull.Value , dt.Rows[0][0], "DC2");

			// set AllowDBNull=false 
			try
			{
				dc.AllowDBNull=false; //the exisiting row contains null value
				Assert.Fail("DC3: AllowDbNull Failed to throw DataException");
			}
			catch (DataException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DC4: AllowDbNull. Wrong exception type. Got:" + exc);
			}

			dt.Rows.Clear();
			dc.AllowDBNull=false;
			// AllowDBNull=false - adding new row with null value
			try
			{
				dt.Rows.Add(dt.NewRow());
				Assert.Fail("DC5: RowAdd Failed to throw NoNullAllowedException");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DC6: RowAdd. Wrong exception type. Got:" + exc);
			}

			dc.AutoIncrement=true;
			int iRowCount = dt.Rows.Count;
			// AllowDBNull=false,AutoIncrement=true - adding new row with null value
			dt.Rows.Add(dt.NewRow());
			Assert.AreEqual(dt.Rows.Count, iRowCount+1, "DC7");
		}

		[Test] public void AutoIncrement()
		{
			DataColumn dc;
			dc = new DataColumn("ColName",typeof(string));

			// Checking default value (False)
			Assert.AreEqual(false , dc.AutoIncrement, "DC8");

			//Cheking Set
			dc.AutoIncrement=true;
			// Checking Get
			Assert.AreEqual(true , dc.AutoIncrement, "DC9");
		}

		[Test] public void AutoIncrementSeed()
		{
			DataColumn dc;
			dc = new DataColumn("ColName",typeof(string));

			// Checking default value 0
			Assert.AreEqual((long)0, dc.AutoIncrementSeed , "DC10");

			//Cheking Set
			dc.AutoIncrementSeed = long.MaxValue;
			// Checking Get MaxValue
			Assert.AreEqual(long.MaxValue, dc.AutoIncrementSeed , "DC11");

			//Cheking Set
			dc.AutoIncrementSeed = long.MinValue ;
			// Checking Get MinValue
			Assert.AreEqual(long.MinValue , dc.AutoIncrementSeed, "DC12");
		}

		[Test] public void AutoIncrementStep()
		{
			DataColumn dc;
			dc = new DataColumn("ColName",typeof(string));
			// Checking default value 1
			Assert.AreEqual((long)1,  dc.AutoIncrementStep , "DC13");

			//Cheking Set
			dc.AutoIncrementStep = long.MaxValue;
			// Checking Get MaxValue
			Assert.AreEqual(long.MaxValue, dc.AutoIncrementStep , "DC14");

			//Cheking Set
			dc.AutoIncrementStep = long.MinValue ;
			// Checking Get MinValue
			Assert.AreEqual(long.MinValue ,  dc.AutoIncrementStep , "DC15");
		}

		[Test] public void Caption()
		{
			DataColumn dc;
			string sCaption = "NewCaption";
			dc = new DataColumn("ColName",typeof(string));

			//Checking default value ( ColumnName )
			// Checking default value ( ColumnName )
			Assert.AreEqual(dc.ColumnName , dc.Caption, "DC16");

			//Cheking Set
			dc.Caption = sCaption;
			// Checking Get
			Assert.AreEqual(sCaption , dc.Caption , "DC17");
		}

		[Test] public void ColumnName()
		{
			DataColumn dc;
			string sName = "NewName";

			dc = new DataColumn();
			//Checking default value ("")
			// ColumnName default value
			Assert.AreEqual(string.Empty ,  dc.ColumnName, "DC18");

			//Cheking Set
			dc.ColumnName = sName;
			//Checking Get
			// ColumnName Get/Set
			Assert.AreEqual(sName , dc.ColumnName , "DC19");

			//Special chars (valid chars)
			sName = "~()#\\/=><+-*%&|^'\"[]";
			// ColumnName Special chars
			dc.ColumnName = sName ;	
			Assert.AreEqual(sName , dc.ColumnName , "DC20");
		}

		[Test] public void DataType()
		{
			DataColumn dc;
			dc = new DataColumn();
			string[] sTypeArr = { "System.Boolean", "System.Byte", "System.Char", "System.DateTime",
				"System.Decimal", "System.Double", "System.Int16", "System.Int32",
				"System.Int64", "System.SByte", "System.Single", "System.String", 
				"System.TimeSpan", "System.UInt16", "System.UInt32", "System.UInt64" };

			//Checking default value (string)
			// GetType - Default
			Assert.AreEqual(Type.GetType("System.String") ,  dc.DataType, "DC21");

			foreach (string sType in sTypeArr) 
			{
				//Cheking Set
				dc.DataType = Type.GetType(sType);
				// Checking GetType " + sType);
				Assert.AreEqual(Type.GetType(sType) , dc.DataType , "DC22");
			}
		}

		[Test] public void Equals()
		{
			DataColumn dc1,dc2;
			dc1 = new DataColumn();
			dc2 = new DataColumn();
			// #1
			// Equals 1
			Assert.AreEqual(false , dc1.Equals(dc2) , "DC23");

			dc1 = dc2;
			// #2
			// Equals 2
			Assert.AreEqual(dc2 , dc1 , "DC24");
		}

		[Test] public void ExtendedProperties()
		{
			DataColumn dc;
			PropertyCollection pc;
			dc = new DataColumn();

			pc = dc.ExtendedProperties ;
			// Checking ExtendedProperties default 
			Assert.AreEqual(true, pc != null, "DC25");

			// Checking ExtendedProperties count 
			Assert.AreEqual(0, pc.Count , "DC26");
		}

		[Test] public void TestGetHashCode()
		{
			DataColumn dc1;
			int iHashCode1;
			dc1 = new DataColumn();

			iHashCode1 = dc1.GetHashCode();
			for (int i=0; i<10; i++)
			{	// must return the same value each time
				// GetHashCode #" + i.ToString());
				Assert.AreEqual(dc1.GetHashCode(), iHashCode1 , "DC27");
			}
		}

		[Test] public void TestGetType()
		{
			DataColumn dc;
			Type myType;
			dc = new DataColumn();
			myType = dc.GetType();

			// GetType
			Assert.AreEqual(typeof(DataColumn), myType, "DC28");
		}

		[Test] public void MaxLength()
		{
			DataColumn dc;
			dc = new DataColumn("ColName",typeof(string));

			//Checking default value (-1)
			// MaxLength default
			Assert.AreEqual((int)-1, dc.MaxLength , "DC29");

			//Cheking Set MaxValue
			dc.MaxLength = int.MaxValue ;
			//Checking Get MaxValue
			// MaxLength MaxValue
			Assert.AreEqual(int.MaxValue , dc.MaxLength , "DC30");

			//Cheking Set MinValue
			dc.MaxLength = int.MinValue  ;
			//Checking Get MinValue
			// MaxLength MinValue
			Assert.AreEqual(int.MinValue, dc.MaxLength , "DC31");

			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("col",typeof(string)));
			dt.Columns[0].MaxLength = 5;
			dt.Rows.Add(new object[] {"a"});

			//MaxLength = 5
			try
			{
				// MaxLength = 5
				dt.Rows[0][0] = "123456";
				Assert.Fail("DC32: Indexer failed to throw ArgumentException");
			}
			catch(ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DC33: Indexer. Wrong exception type. Got:" + exc);
			}
		}

		[Test] public void Namespace()
		{
			DataColumn dc;
			string sName = "NewName";

			dc = new DataColumn();
			//Checking default value ("")
				// Namespace default
				Assert.AreEqual(string.Empty , dc.Namespace , "DC34");

			//Cheking Set
			dc.Namespace  = sName;
			//Checking Get
				// Namespace Get/Set
				Assert.AreEqual(sName, dc.Namespace , "DC35");
		}

		[Test] public void Prefix()
		{
			DataColumn dc;
			string sPrefix = "Prefix";
			dc = new DataColumn("ColName",typeof(string));

				// Prefix Checking default value (string.Empty)
				Assert.AreEqual(string.Empty , dc.Prefix , "DC36");

			//Cheking Set
			dc.Prefix = sPrefix;
			//Checking Get
				// Prefix Checking Get
				Assert.AreEqual(sPrefix , dc.Prefix , "DC37");
		}

		[Test] public void ReadOnly()
		{
			DataColumn dc;
			dc = new DataColumn();

			//Checking default value (false)
				// ReadOnly default
				Assert.AreEqual(false , dc.ReadOnly , "DC38");

			//Cheking Set
			dc.ReadOnly=true;
			//Checking Get
				// ReadOnly Get/Set
				Assert.AreEqual(true, dc.ReadOnly , "DC39");
		}

		[Test] public void Table()
		{
			DataColumn dc;
			dc = new DataColumn();

			//Checking First Get
				// Table test1
				Assert.AreEqual(null,  dc.Table, "DC40");

			DataTable dt = new DataTable();
			dt.Columns.Add(dc);

			//Checking Second Get
				// Table test2
				Assert.AreEqual(dt, dc.Table , "DC41");
		}

		[Test] public void TestToString()
		{
			DataColumn dc;
			string sColName,sExp;
			dc = new DataColumn();

			//ToString = ""
			//Console.WriteLine(dc.ToString());

			//ToString = ColumnName 			
			sColName = "Test1";
			dc.ColumnName = sColName;
				// ToString - ColumnName
				Assert.AreEqual(sColName , dc.ToString() , "DC42");

			//TosTring = ColumnName + " + " + Expression
			sExp = "Tax * 1.234";
			dc.Expression = sExp;
				// TosTring=ColumnName + Expression
				Assert.AreEqual(sColName + " + " + sExp , dc.ToString() , "DC43");
		}

		[Test] public void Unique()
		{
			DataColumn dc;
			dc = new DataColumn();
			//Checking default value (false)

				// Unique default
				Assert.AreEqual(false , dc.Unique , "DC44");

			//Cheking Set
			dc.Unique=true;

			//Checking Get
			// Unique Get/Set
			Assert.AreEqual(true,  dc.Unique, "DC45");
		}

		[Test] public void Unique_PrimaryKey()
		{
			DataTable table = new DataTable ("Table1");
			DataColumn col = table.Columns.Add ("col1");
			table.PrimaryKey = new DataColumn [] {col};
		
			Assert.IsTrue (col.Unique, "#1");

			try {
				col.Unique = false;
				Assert.Fail ("#2 cannot remove uniqueness of a primarykey");
			} catch (ArgumentException e) {
			}

			Assert.IsTrue (col.Unique, "#3");
		}

		[Test] public void ctor()
		{
			DataColumn dc;
			dc = new DataColumn();

			// ctor
			Assert.AreEqual(false, dc == null, "DC46");
		}

		[Test] public void ctor_ByColumnName()
		{
			DataColumn dc;
			string sName = "ColName";
			dc = new DataColumn(sName);

			// ctor - object
			Assert.AreEqual(false , dc==null , "DC47");

			// ctor - ColName
			Assert.AreEqual(sName, dc.ColumnName , "DC48");
		}

		[Test] public void ctor_ByColumnNameType()
		{
			Type typTest;
			DataColumn dc = null;
			string[] sTypeArr = { "System.Boolean", "System.Byte", "System.Char", "System.DateTime",
				"System.Decimal", "System.Double", "System.Int16", "System.Int32",
				"System.Int64", "System.SByte", "System.Single", "System.String", 
				"System.TimeSpan", "System.UInt16", "System.UInt32", "System.UInt64" };

			foreach (string sType in sTypeArr) 
			{
				typTest = Type.GetType(sType);
				dc = new DataColumn("ColName",typTest);
				// ctor - object
				Assert.AreEqual(false , dc==null, "DC49");

				// ctor - ColName
				Assert.AreEqual(typTest ,  dc.DataType , "DC50");
			}
		}

		[Test] public void ctor_ByColumnNameTypeExpression()
		{
			DataColumn dc;
			dc = new DataColumn("ColName",typeof(String),"Price * 1.18");

			// ctor - object
			Assert.AreEqual(false , dc==null, "DC51");
		}

		[Test] public void ctor_ByColumnNameTypeExpressionMappingType()
		{
			DataColumn dc;
			//Cheking constructor for each Enum MappingType
			foreach (int i in Enum.GetValues(typeof(MappingType))) 
			{
				dc = null;
				dc = new DataColumn("ColName",typeof(string),"Price * 1.18",(MappingType)i );
				// Ctor #" + i.ToString());
				Assert.AreEqual(false , dc==null , "DC52");
			}
		}

		[Test] public void ordinal()
		{
			DataColumn dc;
			dc = new DataColumn("ColName",typeof(string));

			//DEBUG
			//Console.WriteLine( "***" + dc.Ordinal.ToString()  + "***
			//DEBUG

			//Checking default value 
			// Ordinal default value
			Assert.AreEqual((int)-1 ,  dc.Ordinal, "DC53");

			// needs a DataTable.Columns to test   
			DataColumnCollection dcColl ;
			DataTable tb = new DataTable();
			dcColl = tb.Columns ;
			dcColl.Add();	//0
			dcColl.Add();	//1
			dcColl.Add();	//2
			dcColl.Add(dc);	//3

			//Checking Get
			// Ordinal Get
			Assert.AreEqual((int)3 , dc.Ordinal , "DC54");
		}

		[Test]
		public void Expression()
		{
			DataColumn dc;
			string sExpression = "Tax * 0.59";
			dc = new DataColumn("ColName",typeof(string));

			Assert.AreEqual(string.Empty, dc.Expression, "dce#1");

			dc.Expression = sExpression;

			Assert.AreEqual(sExpression,dc.Expression, "dce#2");				
		}

		[Test]
		public void Expression_Exceptions()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			try
			{
				dt.Columns[0].Unique=true;
				dt.Columns[0].Expression = "sum(" + dt.Columns[0].ColumnName + ")";
				Assert.Fail("dccee#1: Expression failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccee#2: Expression. Wrong exception type. Got:" + exc);
			}	

			try
			{
				DataTable dt1 = DataProvider.CreateParentDataTable();
				dt1.Columns[0].AutoIncrement=true;
				dt1.Columns[0].Expression = "sum(" + dt1.Columns[0].ColumnName + ")";
				Assert.Fail("dccee#3: Expression failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccee#4: Expression. Wrong exception type. Got:" + exc);
			}

			try
			{
				DataTable dt1 = DataProvider.CreateParentDataTable();
				dt1.Constraints.Add(new UniqueConstraint(dt1.Columns[0],false));
				dt1.Columns[0].Expression = "count(" + dt1.Columns[0].ColumnName + ")";
				Assert.Fail("dccee#5: Expression failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccee#6: Expression. Wrong exception type. Got:" + exc);
			}

			try
			{
				DataTable dt1 = DataProvider.CreateParentDataTable();
			
				dt1.Columns[0].Expression = "CONVERT(" + dt1.Columns[1].ColumnName + ",'System.Int32')";
				Assert.Fail("dccee#7: Expression failed to throw FormatException");
			}
			catch (FormatException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccee#8: Expression. Wrong exception type. Got:" + exc);
			}

			try
			{
				DataTable dt1 = DataProvider.CreateParentDataTable();
			
				dt1.Columns[0].Expression = "CONVERT(" + dt1.Columns[0].ColumnName + ",'System.DateTime')";
				Assert.Fail("dccee#9: Expression failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccee#10: Expression. Wrong exception type. Got:" + exc);
			}


			try
			{
				DataTable dt1 = DataProvider.CreateParentDataTable();
			
				dt1.Columns[1].Expression = "CONVERT(" + dt1.Columns[0].ColumnName + ",'System.DateTime')";
				Assert.Fail("dccee#11: Expression failed to throw InvalidCastException");
			}
			catch (InvalidCastException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccee#12: Expression. Wrong exception type. Got:" + exc);
			}

			try
			{
				DataTable dt1 = DataProvider.CreateParentDataTable();
			
				dt1.Columns[1].Expression = "SUBSTRING(" + dt1.Columns[2].ColumnName + ",60000000000,2)";
				Assert.Fail("dccee#13: Expression failed to throw OverflowException");
			}
			catch (OverflowException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccee#14: Expression. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void Expression_Simple()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			//Simple expression --> not aggregate
			DataColumn dc = new DataColumn("expr",Type.GetType("System.Decimal"));
			dt.Columns.Add(dc);
			dt.Columns["expr"].Expression = dt.Columns[0].ColumnName + "*0.52 +" + dt.Columns[0].ColumnName; 

			//Check the values
			//double temp;
			string temp;
			string str;

			foreach(DataRow dr in dt.Rows) {
				str = ( ((int)dr[0])*0.52 + ((int)dr[0])).ToString();
				if (str.Length > 3)
					temp = str.Substring(0,4);
				else
					temp = str;
				//Due to bug in GH 4.56 sometimes looks like : 4.56000000000000005

				//temp = Convert.ToDouble(str);

				if (dr["expr"].ToString().Length >3)
					str = dr["expr"].ToString().Substring(0,4);
				else
					str = dr["expr"].ToString();

				if (str == "7.60")
					str = "7.6";

				Assert.AreEqual(temp,str, "dcse#1");
				//Compare(Convert.ToDouble(dr["expr"]), temp);
			}
		}

		[Test]
		public void Expression_Aggregate()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			//Simple expression -->  aggregate
			DataColumn dc = new DataColumn("expr",Type.GetType("System.Decimal"));
			dt.Columns.Add(dc);
			dt.Columns["expr"].Expression = "sum(" + dt.Columns[0].ColumnName + ") + count(" + dt.Columns[0].ColumnName + ")" ; 
			dt.Columns["expr"].Expression+= " + avg(" + dt.Columns[0].ColumnName + ") + Min(" + dt.Columns[0].ColumnName + ")" ; 


			//Check the values
			double temp;
			string str;

			double sum = Convert.ToDouble(dt.Compute("sum(" + dt.Columns[0].ColumnName + ")",string.Empty));
			double count = Convert.ToDouble(dt.Compute("count(" + dt.Columns[0].ColumnName + ")",string.Empty));
			double avg = Convert.ToDouble(dt.Compute("avg(" + dt.Columns[0].ColumnName + ")",string.Empty));
			double min = Convert.ToDouble(dt.Compute("min(" + dt.Columns[0].ColumnName + ")",string.Empty));

			str = (sum+count+avg+min).ToString();
			foreach(DataRow dr in dt.Rows)
			{
				if (str.Length > 3)
				{
					temp = Convert.ToDouble(str.Substring(0,4));
				}
				else
				{
					temp = Convert.ToDouble(str);
				}
				
				Assert.AreEqual(temp, Convert.ToDouble(dr["expr"]), "dcea#1");
			}
		}

		[Test]
		public void Expression_AggregateRelation()
		{
			DataTable parent = DataProvider.CreateParentDataTable();
			DataTable child  = DataProvider.CreateChildDataTable();
			DataSet ds = new DataSet();
			ds.Tables.Add(parent);
			ds.Tables.Add(child);
			
			ds.Relations.Add("Relation1",parent.Columns[0],child.Columns[0],false);

			//Create the computed columns 

			DataColumn dcComputedParent = new DataColumn("computedParent",Type.GetType("System.Double"));
			parent.Columns.Add(dcComputedParent);
			dcComputedParent.Expression = "sum(child(Relation1)." + child.Columns[1].ColumnName + ")";

			double preCalculatedExpression;

			foreach (DataRow dr in parent.Rows)
			{
				object o = child.Compute("sum(" + child.Columns[1].ColumnName + ")",
					parent.Columns[0].ColumnName + "=" + dr[0]);
				if (o == DBNull.Value)
				{
					Assert.AreEqual(dr["computedParent"],o,"dcear#1");
				}
				else
				{
					preCalculatedExpression = Convert.ToDouble(o);
					Assert.AreEqual(dr["computedParent"],preCalculatedExpression,"dcear#2");
				}
			}

			DataColumn dcComputedChild = new DataColumn("computedChild",Type.GetType("System.Double"));
			child.Columns.Add(dcComputedChild);
			dcComputedChild.Expression = "Parent." + parent.Columns[0].ColumnName;

			int index=0;
			double val;
			foreach (DataRow dr in child.Rows)
			{
				val = Convert.ToDouble(dr.GetParentRow("Relation1")[0]);
				Assert.AreEqual(dr["computedChild"],val,"dcear#3");
				index++;				
			}
		}

		[Test]
		public void Expression_IIF()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataColumn dcComputedParent = new DataColumn("computedCol",Type.GetType("System.Double"));
			dcComputedParent.DefaultValue=25.5;
			dt.Columns.Add(dcComputedParent);
			dcComputedParent.Expression = "IIF(" + dt.Columns[0].ColumnName + ">3" + ",1,2)"; 

			double val;
			foreach (DataRow dr in dt.Rows)
			{
				val = (int)dr[0] >3 ? 1:2;
				Assert.AreEqual(val,dr["computedCol"],"dceiif#1");				
			}
			//Now reset the expression and check that the column got his deafult value

			dcComputedParent.Expression = null;
			foreach (DataRow dr in dt.Rows)
			{
				Assert.AreEqual(25.5,dr["computedCol"],"dceiif#2");
			}
		}

		// bug #78254
		[Test]
		public void Expression_ISNULL ()
		{
			DataSet ds = new DataSet ();

			DataTable ptable = new DataTable ();
			ptable.Columns.Add ("col1", typeof (int));

			DataTable ctable = new DataTable ();
			ctable.Columns.Add ("col1", typeof (int));
			ctable.Columns.Add ("col2", typeof (int));

			ds.Tables.AddRange (new DataTable[] {ptable, ctable});
			ds.Relations.Add ("rel1", ptable.Columns [0], ctable.Columns [0]);

			ptable.Rows.Add (new object[] {1});
			ptable.Rows.Add (new object[] {2});
			for (int i=0; i < 5; ++i)
				ctable.Rows.Add (new object[] {1, i});

			// should not throw exception
			ptable.Columns.Add ("col2", typeof (int), "IsNull (Sum (Child (rel1).col2), -1)");

			Assert.AreEqual (10, ptable.Rows [0][1], "#1");
			Assert.AreEqual (-1, ptable.Rows [1][1], "#2");
		}

#if NET_2_0
		[Test]
		public void DateTimeMode_DataType ()
		{
			DataColumn col = new DataColumn("col", typeof(int));
			Assert.AreEqual (DataSetDateTime.UnspecifiedLocal, col.DateTimeMode, "#1");
			try {
				col.DateTimeMode = DataSetDateTime.Local;
				Assert.Fail ("#2");
			} catch (InvalidOperationException e) {}

			col = new DataColumn ("col", typeof (DateTime));
			col.DateTimeMode = DataSetDateTime.Utc;
			Assert.AreEqual (DataSetDateTime.Utc, col.DateTimeMode, "#3");
			col.DataType = typeof (int);
			Assert.AreEqual (DataSetDateTime.UnspecifiedLocal, col.DateTimeMode, "#4");
		}
	
		[Test]
		public void DateTimeMode_InvalidValues ()
		{
			DataColumn col = new DataColumn("col", typeof(DateTime));
			try {
				col.DateTimeMode = (DataSetDateTime)(-1);
				Assert.Fail("#1");
			} catch (InvalidEnumArgumentException e) {}

			try {
				col.DateTimeMode = (DataSetDateTime)5;
				Assert.Fail("#2");
			} catch (InvalidEnumArgumentException e) {}
		}

		[Test]
		public void DateTimeMode_RowsAdded ()
		{
			DataTable table = new DataTable();
			table.Columns.Add("col", typeof(DateTime));
			table.Rows.Add(new object[] {DateTime.Now});

			Assert.AreEqual(DataSetDateTime.UnspecifiedLocal, table.Columns[0].DateTimeMode, "#1");
			// allowed
			table.Columns[0].DateTimeMode = DataSetDateTime.Unspecified;
			table.Columns[0].DateTimeMode = DataSetDateTime.UnspecifiedLocal;

			try {
				table.Columns[0].DateTimeMode = DataSetDateTime.Local;
				Assert.Fail("#2");
			} catch (InvalidOperationException e) {}

			try {
				table.Columns[0].DateTimeMode = DataSetDateTime.Utc;
				Assert.Fail("#3");
			} catch (InvalidOperationException e) {}
		}

        	[Test]
        	public void SetOrdinalTest()
		{
			DataColumn col = new DataColumn("col", typeof(int));
			try {
				col.SetOrdinal(2);
				Assert.Fail ("#1");
			} catch (ArgumentException e) { }

			DataTable table = new DataTable();
			DataColumn col1 = table.Columns.Add ("col1", typeof (int));
			DataColumn col2 = table.Columns.Add("col2", typeof(int));
			DataColumn col3 = table.Columns.Add("col3", typeof(int));

			Assert.AreEqual("col1", table.Columns[0].ColumnName, "#2");
			Assert.AreEqual("col3", table.Columns[2].ColumnName, "#3");

			table.Columns[0].SetOrdinal (2);
			Assert.AreEqual("col2", table.Columns[0].ColumnName, "#4");
			Assert.AreEqual("col1", table.Columns[2].ColumnName, "#5");

			Assert.AreEqual(0, col2.Ordinal, "#6");
			Assert.AreEqual(1, col3.Ordinal, "#7");
			Assert.AreEqual(2, col1.Ordinal, "#8");

			try {
				table.Columns[0].SetOrdinal (-1);
				Assert.Fail ("#9");
			} catch (ArgumentOutOfRangeException e) { }

			try {
				table.Columns[0].SetOrdinal (4);
				Assert.Fail ("#10");
			} catch (ArgumentOutOfRangeException e) { }
		}
#endif
		[Test]
		public void bug672113_MulpleColConstraint ()
		{
			DataTable FirstTable = new DataTable ("First Table");
			DataColumn col0 = new DataColumn ("empno", typeof (int));
			DataColumn col1 = new DataColumn ("name", typeof (string));
			DataColumn col2 = new DataColumn ("age", typeof (int));
			FirstTable.Columns.Add (col0);
			FirstTable.Columns.Add (col1);
			FirstTable.Columns.Add (col2);
			DataColumn[] primkeys = new DataColumn[2];
			primkeys[0] = FirstTable.Columns[0];
			primkeys[1] = FirstTable.Columns[1];
			FirstTable.Constraints.Add("PRIM1",primkeys,true);

			DataTable SecondTable = new DataTable ("Second Table");
			col0 = new DataColumn ("field1", typeof (int));
			col1 = new DataColumn ("field2", typeof (int));
			col2 = new DataColumn ("field3", typeof (int));
			SecondTable.Columns.Add (col0);
			SecondTable.Columns.Add (col1);
			SecondTable.Columns.Add (col2);

			primkeys[0] = SecondTable.Columns[0];
			primkeys[1] = SecondTable.Columns[1];
			SecondTable.Constraints.Add("PRIM2",primkeys,true);

			DataRow row1 = FirstTable.NewRow ();
			row1["empno"] = 1;
			row1["name"] = "Test";
			row1["age"] = 32;
			FirstTable.Rows.Add (row1);
			FirstTable.AcceptChanges ();
			Assert.AreEqual (32, FirstTable.Rows[0]["age"], "#1");

			row1 = SecondTable.NewRow ();
			row1["field1"] = 10000;
			row1["field2"] = 12000;
			row1["field3"] = 1000;
			SecondTable.Rows.Add (row1);
			SecondTable.AcceptChanges ();
			Assert.AreEqual (12000, SecondTable.Rows[0]["field2"], "#2");
		}
	}
}
