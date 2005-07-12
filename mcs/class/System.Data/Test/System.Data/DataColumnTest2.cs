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

namespace MonoTests.System.Data
{
	[TestFixture] public class DataColumnTest2
	{
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
	}
}
