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
			Assertion.Assert(dt.Columns != null);
			//Assertion.Assert(dt.ChildRelations != null);
			Assertion.Assert(dt.Constraints != null);
			Assertion.Assert(dt.DataSet == null); 
			Assertion.Assert(dt.DefaultView != null);
			Assertion.Assert(dt.DisplayExpression == "");
			Assertion.Assert(dt.ExtendedProperties != null);
			Assertion.Assert(dt.HasErrors == false);
			Assertion.Assert(dt.Locale != null);
			Assertion.Assert(dt.MinimumCapacity == 50); //LAMESPEC:
			Assertion.Assert(dt.Namespace == "");
			//Assertion.Assert(dt.ParentRelations != null);
			Assertion.Assert(dt.Prefix == "");
			Assertion.Assert(dt.PrimaryKey != null);
			Assertion.Assert(dt.Rows != null);
			Assertion.Assert(dt.Site == null);
			Assertion.Assert(dt.TableName == "");
			
		}
	}
}
