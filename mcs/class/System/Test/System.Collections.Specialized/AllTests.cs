//
// MonoTests.System.Collections.Specialized.AllTests, System.dll
//
// Author:
//   Lawrence Pit <loz@cable.a2000nl>
//

using NUnit.Framework;
using System;

namespace MonoTests.System.Collections.Specialized  {
	
	public class AllTests : TestCase {

		public AllTests (string name) : base (name)
		{
		}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (BitVector32Test.Suite);
				suite.AddTest (HybridDictionaryTest.Suite);				
				suite.AddTest (NameValueCollectionTest.Suite);
				suite.AddTest (StringCollectionTest.Suite);
				return suite;
			}
		}
	}
}

