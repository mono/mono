// MonoTests.System.Data.AllTests.cs
//
// Author:
//   Rodrigo Moya <rodrigo@ximian.com>
//
// (C) Copyright 2002 Rodrigo Moya
//

using NUnit.Framework;

namespace MonoTests.System.Data
{
	/// <summary>
	///  Combines all unit tests for the System.Data.dll assembly
	///   into one test suite.
	/// </summary>
	public class AllTests : TestCase
	{
		public AllTests (string name) : base (name) { }

		public static ITest Suite {
			get {
				TestSuite suite =  new TestSuite ();
				suite.AddTest (new TestSuite (typeof (DataColumnTest)));
				suite.AddTest (new TestSuite (typeof (UniqueConstraintTest)));
				suite.AddTest (new TestSuite (typeof (ConstraintTest)));
				suite.AddTest (new TestSuite (typeof (ConstraintCollectionTest)));
				suite.AddTest (new TestSuite (typeof (ForeignKeyConstraintTest)));
				suite.AddTest (new TestSuite (typeof (DataTableTest)));
				suite.AddTest (new TestSuite (typeof (DataRowCollectionTest)));
				suite.AddTest (new TestSuite (typeof (DataRowTest)));
				suite.AddTest (new TestSuite (typeof (DataColumnCollectionTest)));
				suite.AddTest (new TestSuite (typeof (DataSetTest)));
				suite.AddTest (new TestSuite (typeof (DataRelationTest)));
				return suite;
			}
		}
	}
}
