// DataTableTest.cs - NUnit Test Cases for testing the DataTable 
//
// Franklin Wise (gracenote@earthlink.net)
// 
// (C) Franklin Wise
// 

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{

	public class DataTableTest : TestCase 
	{
	
		public DataTableTest() : base ("MonoTest.System.Data.DataTableTest") {}
		public DataTableTest(string name) : base(name) {}

		protected override void SetUp() {}

		protected override void TearDown() {}

		public static ITest Suite 
		{
			get 
			{ 
				return new TestSuite(typeof(DataTableTest)); 
			}
		}

		public void TestCtor()
		{
			DataTable dt = new DataTable();

			Assertion.AssertEquals("CaseSensitive must be false." ,false,dt.CaseSensitive);
			Assertion.Assert("Col",dt.Columns != null);
			//Assertion.Assert(dt.ChildRelations != null);
			Assertion.Assert("Const", dt.Constraints != null);
			Assertion.Assert("ds", dt.DataSet == null); 
			Assertion.Assert("dv", dt.DefaultView != null);
			Assertion.Assert("de", dt.DisplayExpression == "");
			Assertion.Assert("ep", dt.ExtendedProperties != null);
			Assertion.Assert("he", dt.HasErrors == false);
			Assertion.Assert("lc", dt.Locale != null);
			Assertion.Assert("mc", dt.MinimumCapacity == 50); //LAMESPEC:
			Assertion.Assert("ns", dt.Namespace == "");
			//Assertion.Assert(dt.ParentRelations != null);
			Assertion.Assert("pf", dt.Prefix == "");
			Assertion.Assert("pk", dt.PrimaryKey != null);
			Assertion.Assert("rows", dt.Rows != null);
			Assertion.Assert("Site", dt.Site == null);
			Assertion.Assert("tname", dt.TableName == "");
			
		}

		public void TestToString()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("Col1",typeof(int));
			
			dt.TableName = "Myzable";
			dt.DisplayExpression = "Col1";
			
			
			string cmpr = dt.TableName + " " + dt.DisplayExpression;
			Assertion.AssertEquals(cmpr,dt.ToString());
		}
	}
}
