//
// MonoTests.System.AllTests, System.dll
//
// Author:
//   Lawrence Pit <loz@cable.a2000nl>
//

using NUnit.Framework;
using System;

namespace MonoTests.System  {
	
	public class AllTests : TestCase {

		public AllTests (string name) : base (name)
		{
		}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (UriTest.Suite);
				suite.AddTest (UriBuilderTest.Suite);
				return suite;
			}
		}
	}
}

