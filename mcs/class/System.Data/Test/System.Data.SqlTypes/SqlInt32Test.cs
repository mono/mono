// SqlInt32Test.cs - NUnit Test Cases for System.Data.SqlTypes.SqlInt32
//
// Tim Coleman (tim@timcoleman.com)
//
// (C) Tim Coleman
// 

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{

	public class SqlInt32Test : TestCase {
		
		public SqlInt32Test() : base ("System.Data.SqlTypes.SqlInt32") {}
		public SqlInt32Test(string name) : base(name) {}

		protected override void SetUp() {}

		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite(typeof(SqlInt32)); 
			}
		}

		public void TestCreate ()  {
			SqlInt32 foo = new SqlInt32 (0);
			AssertEquals( "Test explicit cast to int", (int)foo, 0);
		}
	}
}
